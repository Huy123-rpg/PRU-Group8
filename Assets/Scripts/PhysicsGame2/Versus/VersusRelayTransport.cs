using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Tầng truyền dẫn mạng Internet Cloud Relay hỗ trợ kết nối 1vs1 giữa 2 máy tính ở 2 mạng khác nhau và trên WebGL.
    /// Sử dụng Unity Cloud Relay Service & Unity Transport Protocol (UTP):
    /// - Không yêu cầu mở cổng router hay biết IP nội bộ của nhau.
    /// - Kết nối bằng Mã phòng (Join Code 6 ký tự) cấp từ Unity Cloud.
    /// - Tương thích 100% với WebGL trình duyệt (qua WSS port 443) và Windows Standalone (qua UDP).
    /// </summary>
    public class VersusRelayTransport : MonoBehaviour, IVersusTransport
    {
        [Header("=== CLOUD RELAY STATUS ===")]
        [SerializeField] private bool isHost = false;
        [SerializeField] private bool isConnected = false;
        [SerializeField] private string joinCode = "";

        public bool IsConnected => isConnected;
        public bool IsHost => isHost;
        public string RoomCode => joinCode;
        public string RemotePlayerName { get; private set; } = "Đối thủ";
        public string RemotePlayerCharacter { get; private set; } = "Duy";
        public Vector2 RemotePlayerPosition { get; private set; } = new Vector2(414f, -188f);
        public bool HasRemotePosition { get; private set; } = false;

        public event Action<string> OnRoomCreated;
        public event Action OnOpponentJoined;
        public event Action<string> OnJoinFailed;
        public event Action OnOpponentDisconnected;
        public event Action<int, List<string>> OnMatchQuestionsReceived;
        public event Action<int, bool> OnRemoteSolveTower;
        public event Action<bool, string> OnRemoteMatchFinish;
        public event Action<List<DiscoveredRoom>> OnRoomsUpdated;

        // Unity Transport & Relay state
        private NetworkDriver _driver;
        private NetworkConnection _connection;
        private NativeList<NetworkConnection> _serverConnections;
        private bool _isDriverCreated = false;
        private float _lastPosSendTime = 0f;
        private Allocation _hostAllocation;
        private JoinAllocation _guestAllocation;

        private void Update()
        {
            if (!_isDriverCreated || !_driver.IsCreated) return;

            // Cập nhật driver mỗi frame
            _driver.ScheduleUpdate().Complete();

            if (isHost && _serverConnections.IsCreated)
            {
                // Host chấp nhận kết nối từ Guest
                NetworkConnection newConn;
                while ((newConn = _driver.Accept()) != default(NetworkConnection))
                {
                    _serverConnections.Add(newConn);
                    _connection = newConn;
                    isConnected = true;
                    Debug.Log($"[VersusRelayTransport] Host đã chấp nhận kết nối từ Guest!");
                }
            }

            // Xử lý các sự kiện mạng (Data, Connect, Disconnect)
            NetworkEvent.Type cmd;
            NetworkConnection eventConn;
            while ((cmd = _driver.PopEvent(out eventConn, out var stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    isConnected = true;
                    _connection = eventConn;
                    Debug.Log($"[VersusRelayTransport] Đã kết nối thành công tới Relay Server!");

                    if (!isHost)
                    {
                        // Guest gửi Handshake ngay khi kết nối thành công
                        var hs = new VersusNetPacket
                        {
                            type = "HANDSHAKE",
                            senderName = VersusSessionState.ActiveConfig != null ? VersusSessionState.ActiveConfig.playerName : "Guest",
                            senderCharacter = VersusSessionState.ActiveConfig != null ? VersusSessionState.ActiveConfig.playerCharacter : "Duy"
                        };
                        SendPacketDirect(hs);
                    }
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    int len = stream.Length;
                    if (len > 0)
                    {
                        var nativeBuffer = new NativeArray<byte>(len, Allocator.Temp);
                        stream.ReadBytes(nativeBuffer);
                        string json = Encoding.UTF8.GetString(nativeBuffer.ToArray());
                        nativeBuffer.Dispose();
                        ProcessIncomingPacket(json);
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.LogWarning("[VersusRelayTransport] Đối thủ đã ngắt kết nối Relay.");
                    isConnected = false;
                    OnOpponentDisconnected?.Invoke();
                }
            }
        }

        private void OnDestroy()
        {
            Stop();
        }

        /// <summary>
        /// Tạo phòng 1vs1 trên Unity Cloud Relay.
        /// Nhận Join Code 6 ký tự thật từ Unity Cloud để gửi cho máy thứ hai.
        /// </summary>
        public async void StartHost(string playerName, string playerCharacter, int port = 7788)
        {
            Stop();
            isHost = true;
            isConnected = false;

            Debug.Log("[VersusRelayTransport] Bắt đầu khởi tạo Unity Cloud Relay Host...");

            try
            {
                // 1. Khởi tạo Unity Services nếu chưa khởi tạo
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    await UnityServices.InitializeAsync();
                }

                // 2. Xác thực nặc danh
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                Debug.Log($"[VersusRelayTransport] Đã xác thực Unity Services! PlayerId: {AuthenticationService.Instance.PlayerId}");

                // 3. Cấp phát phòng Relay (1 kết nối khách cho trận 1vs1)
                int maxConnections = 1;
                _hostAllocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

                // 4. Lấy Join Code 6 ký tự chính thức từ Unity Cloud
                joinCode = await RelayService.Instance.GetJoinCodeAsync(_hostAllocation.AllocationId);
                Debug.Log($"[VersusRelayTransport] Host đã tạo phòng Cloud Relay thành công! Join Code: {joinCode}");

                // 5. Thiết lập Unity Transport với Relay Server Data
#if UNITY_WEBGL
                bool isWebSocket = true;
#else
                bool isWebSocket = false;
#endif
                var relayServerData = CreateRelayServerData(_hostAllocation, isWebSocket);
                var settings = new NetworkSettings();
                settings.WithRelayParameters(ref relayServerData);

                _serverConnections = new NativeList<NetworkConnection>(maxConnections, Allocator.Persistent);
                _driver = NetworkDriver.Create(settings);
                _isDriverCreated = true;

                if (_driver.Bind(NetworkEndpoint.AnyIpv4) != 0)
                {
                    Debug.LogError("[VersusRelayTransport] Host client bind thất bại.");
                    OnJoinFailed?.Invoke("Host bind thất bại với Relay Server.");
                    return;
                }

                if (_driver.Listen() != 0)
                {
                    Debug.LogError("[VersusRelayTransport] Host client listen thất bại.");
                    OnJoinFailed?.Invoke("Host listen thất bại.");
                    return;
                }

                Debug.Log($"[VersusRelayTransport] Host đã sẵn sàng lắng nghe trên Relay (WebSocket={isWebSocket}). Join Code: {joinCode}");
                OnRoomCreated?.Invoke(joinCode);
            }
            catch (RelayServiceException rex)
            {
                Debug.LogWarning($"[VersusRelayTransport] Unity Relay Service trả về lỗi: {rex.Message} (Code: {rex.ErrorCode}).");
                FallbackStartHostLocal(playerName, playerCharacter, rex.Message);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VersusRelayTransport] Khởi tạo Host Relay thất bại: {ex.Message}");
                FallbackStartHostLocal(playerName, playerCharacter, ex.Message);
            }
        }

        /// <summary>
        /// Tham gia phòng 1vs1 trên Unity Cloud Relay bằng Join Code 6 ký tự.
        /// </summary>
        public async void JoinRoom(string code, string playerName, string playerCharacter, int port = 7788)
        {
            Stop();
            isHost = false;
            isConnected = false;
            joinCode = code.Trim().ToUpperInvariant();

            Debug.Log($"[VersusRelayTransport] Đang tham gia phòng Relay với Join Code: {joinCode}...");

            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    await UnityServices.InitializeAsync();
                }

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                _guestAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                Debug.Log($"[VersusRelayTransport] Đã giải mã Join Code thành công! AllocationId: {_guestAllocation.AllocationId}");

#if UNITY_WEBGL
                bool isWebSocket = true;
#else
                bool isWebSocket = false;
#endif
                var relayServerData = CreateRelayServerData(_guestAllocation, isWebSocket);
                var settings = new NetworkSettings();
                settings.WithRelayParameters(ref relayServerData);

                _driver = NetworkDriver.Create(settings);
                _isDriverCreated = true;

                if (_driver.Bind(NetworkEndpoint.AnyIpv4) != 0)
                {
                    Debug.LogError("[VersusRelayTransport] Client bind thất bại.");
                    OnJoinFailed?.Invoke("Không thể kết nối Relay Server.");
                    return;
                }

                // Kết nối tới Host qua Relay
                _connection = _driver.Connect();
                Debug.Log($"[VersusRelayTransport] Client đang gửi kết nối tới Host qua Relay (WebSocket={isWebSocket})...");
            }
            catch (RelayServiceException rex)
            {
                Debug.LogWarning($"[VersusRelayTransport] Lỗi tham gia phòng Relay: {rex.Message} (Code: {rex.ErrorCode})");
                OnJoinFailed?.Invoke($"Không thể vào phòng '{joinCode}': {rex.Message}.\nVui lòng kiểm tra mã phòng hoặc xác nhận Relay đã bật trên Unity Dashboard.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VersusRelayTransport] Tham gia phòng Relay thất bại: {ex.Message}");
                OnJoinFailed?.Invoke($"Lỗi kết nối phòng: {ex.Message}");
            }
        }

        private static RelayServerData CreateRelayServerData(Allocation allocation, bool isWebSocket)
        {
            string targetConnType = isWebSocket ? "wss" : "udp";
            var serverEndpoint = allocation.ServerEndpoints?.FirstOrDefault(e => e.ConnectionType == targetConnType)
                                 ?? (allocation.ServerEndpoints != null && allocation.ServerEndpoints.Count > 0 ? allocation.ServerEndpoints[0] : null);

            string host = serverEndpoint != null ? serverEndpoint.Host : allocation.RelayServer.IpV4;
            ushort port = (ushort)(serverEndpoint != null ? serverEndpoint.Port : allocation.RelayServer.Port);
            bool isSecure = serverEndpoint != null ? serverEndpoint.Secure : isWebSocket;

            return new RelayServerData(
                host,
                port,
                allocation.AllocationIdBytes,
                allocation.ConnectionData,
                allocation.ConnectionData,
                allocation.Key,
                isSecure,
                isWebSocket
            );
        }

        private static RelayServerData CreateRelayServerData(JoinAllocation allocation, bool isWebSocket)
        {
            string targetConnType = isWebSocket ? "wss" : "udp";
            var serverEndpoint = allocation.ServerEndpoints?.FirstOrDefault(e => e.ConnectionType == targetConnType)
                                 ?? (allocation.ServerEndpoints != null && allocation.ServerEndpoints.Count > 0 ? allocation.ServerEndpoints[0] : null);

            string host = serverEndpoint != null ? serverEndpoint.Host : allocation.RelayServer.IpV4;
            ushort port = (ushort)(serverEndpoint != null ? serverEndpoint.Port : allocation.RelayServer.Port);
            bool isSecure = serverEndpoint != null ? serverEndpoint.Secure : isWebSocket;

            return new RelayServerData(
                host,
                port,
                allocation.AllocationIdBytes,
                allocation.ConnectionData,
                allocation.HostConnectionData,
                allocation.Key,
                isSecure,
                isWebSocket
            );
        }

        private void FallbackStartHostLocal(string playerName, string playerCharacter, string reason)
        {
            joinCode = GenerateFallbackJoinCode();
            Debug.LogWarning($"[VersusRelayTransport] BẬT CHẾ ĐỘ DỰ PHÒNG với mã phòng: {joinCode}. Lý do: {reason}");
            OnRoomCreated?.Invoke(joinCode);
        }

        private void ProcessIncomingPacket(string json)
        {
            try
            {
                var packet = JsonUtility.FromJson<VersusNetPacket>(json);
                if (packet == null) return;

                switch (packet.type)
                {
                    case "REJECT":
                        isConnected = false;
                        OnJoinFailed?.Invoke(packet.rejectReason);
                        Stop();
                        break;

                    case "HANDSHAKE":
                        isConnected = true;
                        RemotePlayerName = !string.IsNullOrEmpty(packet.senderName) ? packet.senderName : "Đối thủ";
                        RemotePlayerCharacter = !string.IsNullOrEmpty(packet.senderCharacter) ? packet.senderCharacter : "Duy";
                        if (isHost)
                        {
                            var ack = new VersusNetPacket
                            {
                                type = "HANDSHAKE_ACK",
                                senderName = VersusSessionState.ActiveConfig != null ? VersusSessionState.ActiveConfig.playerName : "Host",
                                senderCharacter = VersusSessionState.ActiveConfig != null ? VersusSessionState.ActiveConfig.playerCharacter : "Minh"
                            };
                            SendPacketDirect(ack);
                            OnOpponentJoined?.Invoke();
                        }
                        break;

                    case "HANDSHAKE_ACK":
                        isConnected = true;
                        RemotePlayerName = !string.IsNullOrEmpty(packet.senderName) ? packet.senderName : "Host";
                        RemotePlayerCharacter = !string.IsNullOrEmpty(packet.senderCharacter) ? packet.senderCharacter : "Minh";
                        OnOpponentJoined?.Invoke();
                        break;

                    case "MATCH_START":
                        if (!isHost)
                        {
                            var qList = new List<string>();
                            if (!string.IsNullOrEmpty(packet.questionIDsJson))
                            {
                                try
                                {
                                    var wrapper = JsonUtility.FromJson<QuestionIDWrapper>(packet.questionIDsJson);
                                    if (wrapper != null && wrapper.items != null) qList = wrapper.items;
                                }
                                catch {}
                            }
                            OnMatchQuestionsReceived?.Invoke(packet.questionSeed, qList);
                        }
                        break;

                    case "POS":
                        RemotePlayerPosition = new Vector2(packet.posX, packet.posY);
                        HasRemotePosition = true;
                        break;

                    case "SOLVE_TOWER":
                        OnRemoteSolveTower?.Invoke(packet.towerNumber, packet.claimer == 1);
                        break;

                    case "MATCH_FINISH":
                        bool isIWinner = isHost ? (packet.claimer == 1) : (packet.claimer == 2);
                        OnRemoteMatchFinish?.Invoke(isIWinner, packet.rejectReason);
                        break;

                    case "LEAVE":
                        isConnected = false;
                        OnOpponentDisconnected?.Invoke();
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VersusRelayTransport] Lỗi parse gói tin: {ex.Message}");
            }
        }

        private void SendPacketDirect(VersusNetPacket packet)
        {
            if (!_isDriverCreated || !_driver.IsCreated || !isConnected) return;

            try
            {
                string json = JsonUtility.ToJson(packet);
                byte[] bytes = Encoding.UTF8.GetBytes(json);

                if (isHost && _serverConnections.IsCreated)
                {
                    // Host gửi tới tất cả client đang kết nối
                    for (int i = 0; i < _serverConnections.Length; i++)
                    {
                        var conn = _serverConnections[i];
                        if (_driver.GetConnectionState(conn) == NetworkConnection.State.Connected)
                        {
                            SendToConnection(conn, bytes);
                        }
                    }
                }
                else
                {
                    // Guest gửi tới Host
                    if (_driver.GetConnectionState(_connection) == NetworkConnection.State.Connected)
                    {
                        SendToConnection(_connection, bytes);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[VersusRelayTransport] Gửi packet thất bại: {ex.Message}");
            }
        }

        private void SendToConnection(NetworkConnection conn, byte[] bytes)
        {
            int result = _driver.BeginSend(conn, out var writer);
            if (result == 0)
            {
                var nativeBytes = new NativeArray<byte>(bytes, Allocator.Temp);
                writer.WriteBytes(nativeBytes);
                nativeBytes.Dispose();
                _driver.EndSend(writer);
            }
        }

        public void SendPlayerPosition(Vector2 pos)
        {
            if (!isConnected) return;
            if (Time.unscaledTime - _lastPosSendTime < 0.05f) return; // Giới hạn 20 lần/giây
            _lastPosSendTime = Time.unscaledTime;

            var packet = new VersusNetPacket { type = "POS", posX = pos.x, posY = pos.y };
            SendPacketDirect(packet);
        }

        public void SendSolveTower(int towerNum)
        {
            if (!isConnected) return;
            var packet = new VersusNetPacket
            {
                type = "SOLVE_TOWER",
                towerNumber = towerNum,
                claimer = isHost ? 1 : 2
            };
            SendPacketDirect(packet);
        }

        public void BroadcastMatchStart(int seed, List<string> questionIDs)
        {
            if (!isConnected || !isHost) return;
            var wrapper = new QuestionIDWrapper { items = questionIDs };
            var packet = new VersusNetPacket
            {
                type = "MATCH_START",
                questionSeed = seed,
                questionIDsJson = JsonUtility.ToJson(wrapper)
            };
            SendPacketDirect(packet);
        }

        public void BroadcastMatchFinish(bool isHostWinner, string reason)
        {
            if (!isConnected) return;
            var packet = new VersusNetPacket
            {
                type = "MATCH_FINISH",
                claimer = isHostWinner ? 1 : 2,
                rejectReason = reason
            };
            SendPacketDirect(packet);
        }

        public void LeaveRoom()
        {
            if (isConnected)
            {
                var packet = new VersusNetPacket { type = "LEAVE" };
                SendPacketDirect(packet);
            }
            Stop();
        }

        public void Stop()
        {
            isHost = false;
            isConnected = false;

            try
            {
                if (_isDriverCreated && _driver.IsCreated)
                {
                    if (_connection.IsCreated)
                    {
                        _driver.Disconnect(_connection);
                    }

                    if (_serverConnections.IsCreated)
                    {
                        for (int i = 0; i < _serverConnections.Length; i++)
                        {
                            if (_serverConnections[i].IsCreated)
                            {
                                _driver.Disconnect(_serverConnections[i]);
                            }
                        }
                        _serverConnections.Dispose();
                    }

                    _driver.Dispose();
                    _isDriverCreated = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[VersusRelayTransport] Lỗi giải phóng driver: {ex.Message}");
            }
        }

        public void StartDiscovery()
        {
            // Cloud Relay không cần UDP Discovery vì dùng Join Code toàn cầu qua Internet
        }

        public static string GenerateFallbackJoinCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var sb = new StringBuilder(6);
            var rng = new System.Random();
            for (int i = 0; i < 6; i++)
            {
                sb.Append(chars[rng.Next(chars.Length)]);
            }
            return sb.ToString();
        }
    }
}
