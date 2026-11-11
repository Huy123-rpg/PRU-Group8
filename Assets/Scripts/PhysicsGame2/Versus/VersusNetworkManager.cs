using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Bộ điều phối Networking đối kháng thời gian thực (Versus Network Dispatcher / Facade).
    /// Tách rời hoàn toàn Logic Đấu trường (Arena/Lobby) khỏi tầng truyền dẫn cụ thể (Transport Layer).
    /// - Trên Windows Standalone: Sử dụng VersusLanTransport (TCP/UDP LAN Socket) hoặc Internet Relay.
    /// - Trên WebGL: Tự động chuyển sang Internet Relay (WebSockets/WSS), hoàn toàn không dùng raw TCP socket.
    /// </summary>
    public class VersusNetworkManager : MonoBehaviour
    {
        public static VersusNetworkManager Instance { get; private set; }

        [Header("=== TRANSPORT CONFIG ===")]
        [SerializeField] private VersusTransportType transportType = VersusTransportType.LanTcpUdp;

        private IVersusTransport _activeTransport;

        public bool IsConnected => _activeTransport != null && _activeTransport.IsConnected;
        public bool IsHost => _activeTransport != null && _activeTransport.IsHost;
        public string CurrentRoomCode => _activeTransport != null ? _activeTransport.RoomCode : "";
        public string RemotePlayerName => _activeTransport != null ? _activeTransport.RemotePlayerName : "Đối thủ";
        public string RemotePlayerCharacter => _activeTransport != null ? _activeTransport.RemotePlayerCharacter : "Duy";
        public Vector2 RemotePlayerPosition => _activeTransport != null ? _activeTransport.RemotePlayerPosition : Vector2.zero;
        public bool HasRemotePosition => _activeTransport != null && _activeTransport.HasRemotePosition;
        public string LocalIP
        {
            get
            {
#if !UNITY_WEBGL
                return VersusLanTransport.ResolveLocalIPAddress();
#else
                return "WebGL-Client";
#endif
            }
        }

        public static string ResolveLocalIPAddress()
        {
#if !UNITY_WEBGL
            return VersusLanTransport.ResolveLocalIPAddress();
#else
            return "127.0.0.1";
#endif
        }

        public static string GenerateRoomCode(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return "000";
            int lastDot = ip.LastIndexOf('.');
            return (lastDot >= 0 && lastDot < ip.Length - 1) ? ip.Substring(lastDot + 1) : ip;
        }

        public static string ResolveIpFromCode(string code)
        {
#if !UNITY_WEBGL
            return VersusLanTransport.ResolveIpFromCode(code);
#else
            return code;
#endif
        }

        // Events
        public event Action<string> OnRoomCreated;
        public event Action OnOpponentJoined;
        public event Action<string> OnJoinFailed;
        public event Action OnOpponentDisconnected;
        public event Action<List<string>> OnMatchQuestionsReceived;
        public event Action<int, bool> OnRemoteSolveTower;
        public event Action<bool, string> OnRemoteMatchFinish;
        public event Action<List<DiscoveredRoom>> OnRoomsUpdated;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeTransport();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void InitializeTransport()
        {
#if UNITY_WEBGL
            // Trên WebGL: Bắt buộc dùng Internet Relay, cấm tuyệt đối Socket LAN
            transportType = VersusTransportType.InternetRelay;
#endif

            if (_activeTransport != null)
            {
                UnbindTransportEvents(_activeTransport);
            }

            if (transportType == VersusTransportType.LanTcpUdp)
            {
                var lan = GetComponent<VersusLanTransport>() ?? gameObject.AddComponent<VersusLanTransport>();
                _activeTransport = lan;
            }
            else
            {
                var relay = GetComponent<VersusRelayTransport>() ?? gameObject.AddComponent<VersusRelayTransport>();
                _activeTransport = relay;
            }

            BindTransportEvents(_activeTransport);
        }

        public void SwitchTransport(VersusTransportType newType)
        {
            if (transportType == newType && _activeTransport != null) return;
            transportType = newType;
            InitializeTransport();
        }

        public void SetTransport(IVersusTransport customTransport)
        {
            if (_activeTransport != null)
            {
                _activeTransport.Stop();
                UnbindTransportEvents(_activeTransport);
            }
            _activeTransport = customTransport;
            if (_activeTransport != null)
            {
                BindTransportEvents(_activeTransport);
            }
        }

        private void BindTransportEvents(IVersusTransport transport)
        {
            if (transport == null) return;
            transport.OnRoomCreated += HandleRoomCreated;
            transport.OnOpponentJoined += HandleOpponentJoined;
            transport.OnJoinFailed += HandleJoinFailed;
            transport.OnOpponentDisconnected += HandleOpponentDisconnected;
            transport.OnMatchQuestionsReceived += HandleMatchQuestionsReceived;
            transport.OnRemoteSolveTower += HandleRemoteSolveTower;
            transport.OnRemoteMatchFinish += HandleRemoteMatchFinish;
            transport.OnRoomsUpdated += HandleRoomsUpdated;
        }

        private void UnbindTransportEvents(IVersusTransport transport)
        {
            if (transport == null) return;
            transport.OnRoomCreated -= HandleRoomCreated;
            transport.OnOpponentJoined -= HandleOpponentJoined;
            transport.OnJoinFailed -= HandleJoinFailed;
            transport.OnOpponentDisconnected -= HandleOpponentDisconnected;
            transport.OnMatchQuestionsReceived -= HandleMatchQuestionsReceived;
            transport.OnRemoteSolveTower -= HandleRemoteSolveTower;
            transport.OnRemoteMatchFinish -= HandleRemoteMatchFinish;
            transport.OnRoomsUpdated -= HandleRoomsUpdated;
        }

        private void HandleRoomCreated(string code) => OnRoomCreated?.Invoke(code);
        private void HandleOpponentJoined() => OnOpponentJoined?.Invoke();
        private void HandleJoinFailed(string reason) => OnJoinFailed?.Invoke(reason);
        private void HandleOpponentDisconnected() => OnOpponentDisconnected?.Invoke();
        private void HandleMatchQuestionsReceived(int seed, List<string> qIds)
        {
            VersusSessionState.ActiveConfig.questionSeed = seed;
            VersusSessionState.ActiveConfig.matchQuestionIDs = qIds;
            OnMatchQuestionsReceived?.Invoke(qIds);
        }
        private void HandleRemoteSolveTower(int tNum, bool isH) => OnRemoteSolveTower?.Invoke(tNum, isH);
        private void HandleRemoteMatchFinish(bool isWin, string reason) => OnRemoteMatchFinish?.Invoke(isWin, reason);
        private void HandleRoomsUpdated(List<DiscoveredRoom> rooms) => OnRoomsUpdated?.Invoke(rooms);

        // ==========================================
        // PUBLIC APIS
        // ==========================================

        public void StartHostRoom(string hostPlayerName, string hostCharacter, int port = 7788)
        {
            if (_activeTransport == null) InitializeTransport();
            _activeTransport?.StartHost(hostPlayerName, hostCharacter, port);
        }

        public void JoinRoomByCode(string inputCode, string playerName, string playerCharacter, int port = 7788)
        {
            if (_activeTransport == null) InitializeTransport();
            _activeTransport?.JoinRoom(inputCode, playerName, playerCharacter, port);
        }

        public void SendPlayerPosition(Vector2 pos)
        {
            _activeTransport?.SendPlayerPosition(pos);
        }

        public void SendSolveTower(int towerNum)
        {
            _activeTransport?.SendSolveTower(towerNum);
        }

        public void BroadcastMatchStart(int seed, List<string> questionIDs)
        {
            _activeTransport?.BroadcastMatchStart(seed, questionIDs);
        }

        public void BroadcastMatchFinish(bool isHostWinner, string reason)
        {
            _activeTransport?.BroadcastMatchFinish(isHostWinner, reason);
        }

        public void LeaveRoom()
        {
            _activeTransport?.LeaveRoom();
        }

        public void StopNetwork()
        {
            _activeTransport?.Stop();
        }

        public void StartUdpDiscovery()
        {
            _activeTransport?.StartDiscovery();
        }
    }
}
