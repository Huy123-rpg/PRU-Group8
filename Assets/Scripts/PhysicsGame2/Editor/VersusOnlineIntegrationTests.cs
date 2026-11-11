using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ScienceQuest.PhysicsGame2.Tests
{
    [TestFixture]
    public class VersusOnlineIntegrationTests
    {
        private GameObject _hostGo;
        private GameObject _guestGo;
        private VersusNetworkManager _hostNet;
        private VersusNetworkManager _guestNet;

        [TearDown]
        public void Teardown()
        {
            if (_hostNet != null) _hostNet.StopNetwork();
            if (_guestNet != null) _guestNet.StopNetwork();
            if (_hostGo != null) UnityEngine.Object.DestroyImmediate(_hostGo);
            if (_guestGo != null) UnityEngine.Object.DestroyImmediate(_guestGo);
        }

        [Test]
        public void Test01_ResolveLocalIPAddress_ReturnsValidIPv4()
        {
            string ip = VersusNetworkManager.ResolveLocalIPAddress();
            Assert.IsFalse(string.IsNullOrEmpty(ip), "IP không được rỗng");
            Assert.IsTrue(ip.Contains("."), "IP phải có định dạng IPv4");
            Debug.Log($"[Test01] Local IPv4 resolved: {ip}");
        }

        [Test]
        public void Test02_RoomCodeGeneration_MatchesIPOrSubnet()
        {
            string ip = "192.168.1.55";
            string code = VersusNetworkManager.GenerateRoomCode(ip);
            string resolvedIp = VersusNetworkManager.ResolveIpFromCode(code);
            Assert.AreEqual("192.168.1.55", resolvedIp, "Giải mã RoomCode phải ra đúng IP");
        }

        [Test]
        public void Test03_QuestionSeedConsistency_GeneratesIdenticalQuestionSets()
        {
            int seed = 556677;
            var setA = PhysicsQuestionRepository.GetRandomVersusQuestions(10, seed);
            var setB = PhysicsQuestionRepository.GetRandomVersusQuestions(10, seed);

            Assert.AreEqual(10, setA.Count, "Set A phải có đủ 10 câu");
            Assert.AreEqual(10, setB.Count, "Set B phải có đủ 10 câu");

            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(setA[i].questionID, setB[i].questionID, $"Câu hỏi tại vị trí {i} phải giống nhau tuyệt đối");
            }
            Debug.Log($"[Test03] 10/10 câu hỏi hoàn toàn trùng khớp giữa 2 máy nhờ Seed {seed}!");
        }

        [UnityTest]
        public IEnumerator Test04_HostAndClientConnection_CompletesHandshake()
        {
            int testPort = 17788; // Port kiểm thử
            _hostGo = new GameObject("Host_NetManager");
            _hostNet = _hostGo.AddComponent<VersusNetworkManager>();

            bool roomCreated = false;
            _hostNet.OnRoomCreated += (code) => { roomCreated = true; };
            _hostNet.StartHostRoom("PlayerHost", "Minh", testPort);

            yield return new WaitForSeconds(0.2f);
            Assert.IsTrue(roomCreated, "Host phải tạo phòng thành công");
            Assert.IsTrue(_hostNet.IsHost, "NetworkManager phải nhận diện đúng vai trò Host");

            // Client kết nối tới Host
            _guestGo = new GameObject("Guest_NetManager");
            _guestNet = _guestGo.AddComponent<VersusNetworkManager>();

            bool guestConnected = false;
            _guestNet.OnOpponentJoined += () => { guestConnected = true; };
            _guestNet.JoinRoomByCode("127.0.0.1", "PlayerGuest", "Lan", testPort);

            // Chờ handshake hoàn thành (tối đa 2 giây)
            float timeout = 2.5f;
            while (timeout > 0f && (!guestConnected || !_hostNet.IsConnected))
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }

            Assert.IsTrue(_hostNet.IsConnected, "Host phải báo IsConnected = true sau handshake");
            Assert.IsTrue(_guestNet.IsConnected, "Guest phải báo IsConnected = true sau handshake");
            Assert.AreEqual("PlayerGuest", _hostNet.RemotePlayerName, "Host phải nhận đúng tên của Guest");
            Assert.AreEqual("Lan", _hostNet.RemotePlayerCharacter, "Host phải nhận đúng nhân vật của Guest");

            Debug.Log("[Test04] Kết nối 1vs1 và Handshake thành công giữa 2 máy tính!");
        }

        [UnityTest]
        public IEnumerator Test05_ThirdPlayerRejected_WhenRoomIsFull()
        {
            int testPort = 17789;
            _hostGo = new GameObject("Host_NetManager");
            _hostNet = _hostGo.AddComponent<VersusNetworkManager>();
            _hostNet.StartHostRoom("HostUser", "Minh", testPort);

            yield return new WaitForSeconds(0.2f);

            // Máy 2 kết nối
            _guestGo = new GameObject("Guest1_NetManager");
            _guestNet = _guestGo.AddComponent<VersusNetworkManager>();
            _guestNet.JoinRoomByCode("127.0.0.1", "Guest1", "Lan", testPort);

            yield return new WaitForSeconds(0.5f);
            Assert.IsTrue(_hostNet.IsConnected, "Guest 1 phải vào được phòng");

            // Máy 3 cố tình kết nối khi phòng đã đủ 2 người
            var rogueGo = new GameObject("Guest2_Rogue");
            var rogueNet = rogueGo.AddComponent<VersusNetworkManager>();

            string rejectReason = null;
            rogueNet.OnJoinFailed += (reason) => { rejectReason = reason; };
            rogueNet.JoinRoomByCode("127.0.0.1", "Guest2_Rogue", "Duy", testPort);

            yield return new WaitForSeconds(0.5f);
            Assert.IsFalse(rogueNet.IsConnected, "Máy thứ 3 KHÔNG ĐƯỢC PHÉP vào phòng đã đầy!");
            Assert.IsNotNull(rejectReason, "Máy thứ 3 phải nhận được thông báo từ chối");
            StringAssert.Contains("đủ 2 người", rejectReason, "Lý do từ chối phải ghi rõ phòng đã đủ 2 người");

            rogueNet.StopNetwork();
            UnityEngine.Object.DestroyImmediate(rogueGo);
            Debug.Log("[Test05] Đã kiểm định từ chối máy thứ 3 thành công khi phòng đã đủ 2 người!");
        }

        [UnityTest]
        public IEnumerator Test06_SolveTowerPacket_DeliversToOpponent()
        {
            int testPort = 17790;
            _hostGo = new GameObject("Host_NetManager");
            _hostNet = _hostGo.AddComponent<VersusNetworkManager>();
            _hostNet.StartHostRoom("HostUser", "Minh", testPort);

            yield return new WaitForSeconds(0.2f);

            _guestGo = new GameObject("Guest_NetManager");
            _guestNet = _guestGo.AddComponent<VersusNetworkManager>();
            _guestNet.JoinRoomByCode("127.0.0.1", "GuestUser", "Lan", testPort);

            yield return new WaitForSeconds(0.5f);

            int remoteTowerSolved = -1;
            bool remoteIsHost = false;
            _guestNet.OnRemoteSolveTower += (tNum, isH) =>
            {
                remoteTowerSolved = tNum;
                remoteIsHost = isH;
            };

            // Host gửi giải đúng trụ số 4
            _hostNet.SendSolveTower(4);

            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual(4, remoteTowerSolved, "Guest phải nhận đúng trụ số 4 được giải");
            Assert.IsTrue(remoteIsHost, "Trụ số 4 phải do Host giải");

            Debug.Log("[Test06] Gói tin giải đúng trụ đã được đồng bộ chuẩn xác sang máy đối thủ!");
        }
    }
}
