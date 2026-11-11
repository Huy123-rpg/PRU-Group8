using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    public enum VersusTransportType
    {
        LanTcpUdp,       // Kết nối trực tiếp LAN / Wi-Fi (Windows Standalone)
        InternetRelay    // Kết nối qua Relay Server / WebSockets (Internet & WebGL)
    }

    [Serializable]
    public class VersusNetPacket
    {
        public string type;            // HANDSHAKE, MATCH_START, POS, SOLVE_TOWER, MATCH_FINISH, LEAVE, REJECT
        public string matchId;
        public string senderId;
        public string senderName;
        public string senderCharacter;
        public float posX;
        public float posY;
        public int towerNumber;
        public int claimer;            // 1 = Host, 2 = Guest
        public int p1Score;
        public int p2Score;
        public float remainingTime;
        public int questionSeed;
        public string questionIDsJson; // JSON array string chứa 10 question IDs
        public string rejectReason;
    }

    [Serializable]
    public class DiscoveredRoom
    {
        public string roomCode;
        public string hostName;
        public string hostCharacter;
        public string ipAddress;
        public int port;
        public float lastSeenTime;
    }

    [Serializable]
    public class QuestionIDWrapper
    {
        public List<string> items = new List<string>();
    }

    /// <summary>
    /// Giao diện trừu tượng hóa tầng truyền dẫn mạng (Transport Layer) cho chế độ Đối Kháng 1vs1.
    /// Giúp tách biệt hoàn toàn Logic trận đấu (Luật 10 trụ, điểm số, vòng sáng) khỏi công nghệ kết nối (TCP LAN vs Cloud Relay).
    /// </summary>
    public interface IVersusTransport
    {
        bool IsConnected { get; }
        bool IsHost { get; }
        string RoomCode { get; }
        string RemotePlayerName { get; }
        string RemotePlayerCharacter { get; }
        Vector2 RemotePlayerPosition { get; }
        bool HasRemotePosition { get; }

        event Action<string> OnRoomCreated;
        event Action OnOpponentJoined;
        event Action<string> OnJoinFailed;
        event Action OnOpponentDisconnected;
        event Action<int, List<string>> OnMatchQuestionsReceived; // seed, questionIDs
        event Action<int, bool> OnRemoteSolveTower;              // towerNum, isHostClaimer
        event Action<bool, string> OnRemoteMatchFinish;          // isPlayerWin, reason
        event Action<List<DiscoveredRoom>> OnRoomsUpdated;       // LAN discovered rooms (nếu có)

        void StartHost(string playerName, string playerCharacter, int port = 7788);
        void JoinRoom(string roomCode, string playerName, string playerCharacter, int port = 7788);
        void SendPlayerPosition(Vector2 pos);
        void SendSolveTower(int towerNum);
        void BroadcastMatchStart(int seed, List<string> questionIDs);
        void BroadcastMatchFinish(bool isHostWinner, string reason);
        void LeaveRoom();
        void Stop();
        void StartDiscovery();
    }
}
