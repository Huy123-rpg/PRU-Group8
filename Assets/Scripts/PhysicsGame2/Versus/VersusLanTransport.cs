using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

#if !UNITY_WEBGL
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
#endif

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Tầng truyền dẫn mạng cục bộ (LAN / Wi-Fi) sử dụng TCP Socket (Port 7788) và UDP Beacon Discovery (Port 7789).
    /// Hỗ trợ kiểm thử 1vs1 giữa 2 máy tính thật trong cùng mạng Wi-Fi hoặc Hotspot.
    /// Toàn bộ mã nguồn Socket được bọc chỉ thị #if !UNITY_WEBGL để không gây lỗi khi build WebGL.
    /// </summary>
    public class VersusLanTransport : MonoBehaviour, IVersusTransport
    {
        public const int DEFAULT_PORT = 7788;
        public const int UDP_BEACON_PORT = 7789;

        [Header("=== LAN TRANSPORT STATUS ===")]
        [SerializeField] private bool isHost = false;
        [SerializeField] private bool isConnected = false;
        [SerializeField] private string roomCode = "";
        [SerializeField] private string localIp = "127.0.0.1";
        [SerializeField] private string remoteIp = "";

        public bool IsConnected => isConnected;
        public bool IsHost => isHost;
        public string RoomCode => roomCode;
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

#if !UNITY_WEBGL
        private TcpListener _serverListener;
        private TcpClient _clientSocket;
        private NetworkStream _stream;
        private Thread _networkThread;
        private volatile bool _isRunning = false;

        private UdpClient _udpBroadcaster;
        private UdpClient _udpListener;
        private Thread _udpListenThread;
        private volatile bool _isUdpRunning = false;
        private readonly Dictionary<string, DiscoveredRoom> _discoveredRooms = new Dictionary<string, DiscoveredRoom>();

        private readonly Queue<Action> _mainThreadQueue = new Queue<Action>();
        private readonly object _queueLock = new object();
        private float _lastUdpBroadcastTime = 0f;
        private float _lastPosSendTime = 0f;
#endif

        private void Awake()
        {
#if !UNITY_WEBGL
            localIp = ResolveLocalIPAddress();
#endif
        }

        private void Update()
        {
#if !UNITY_WEBGL
            lock (_queueLock)
            {
                while (_mainThreadQueue.Count > 0)
                {
                    _mainThreadQueue.Dequeue()?.Invoke();
                }
            }

            if (isHost && _isRunning && !isConnected)
            {
                if (Time.unscaledTime - _lastUdpBroadcastTime > 1.2f)
                {
                    _lastUdpBroadcastTime = Time.unscaledTime;
                    SendUdpBeacon();
                }
            }

            if (_isUdpRunning && !isHost && !isConnected)
            {
                bool changed = false;
                var now = Time.unscaledTime;
                var keysToRemove = new List<string>();
                lock (_discoveredRooms)
                {
                    foreach (var kvp in _discoveredRooms)
                    {
                        if (now - kvp.Value.lastSeenTime > 4f)
                        {
                            keysToRemove.Add(kvp.Key);
                        }
                    }
                    foreach (var k in keysToRemove)
                    {
                        _discoveredRooms.Remove(k);
                        changed = true;
                    }
                }
                if (changed)
                {
                    TriggerRoomsUpdated();
                }
            }
#endif
        }

        private void OnDestroy()
        {
            Stop();
        }

        public void StartHost(string playerName, string playerCharacter, int port = DEFAULT_PORT)
        {
#if UNITY_WEBGL
            Debug.LogError("[VersusLanTransport] TCP/UDP LAN không được hỗ trợ trên nền tảng WebGL!");
            OnJoinFailed?.Invoke("Nền tảng WebGL không hỗ trợ mạng Socket LAN. Vui lòng sử dụng Internet Relay.");
#else
            Stop();

            isHost = true;
            isConnected = false;
            localIp = ResolveLocalIPAddress();
            roomCode = localIp;
            _isRunning = true;

            try
            {
                _serverListener = new TcpListener(IPAddress.Any, port);
                _serverListener.Start();
                Debug.Log($"[VersusLanTransport] Host TCP Server đã chạy trên IP {localIp}:{port}, Mã phòng: {roomCode}");

                _networkThread = new Thread(ServerListenLoop);
                _networkThread.IsBackground = true;
                _networkThread.Start();

                StartUdpBroadcaster();

                EnqueueMainThread(() =>
                {
                    OnRoomCreated?.Invoke(roomCode);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VersusLanTransport] Lỗi mở Host Server: {ex.Message}");
                EnqueueMainThread(() =>
                {
                    OnJoinFailed?.Invoke($"Không thể mở cổng mạng {port}: {ex.Message}");
                });
            }
#endif
        }

        public void JoinRoom(string inputCode, string playerName, string playerCharacter, int port = DEFAULT_PORT)
        {
#if UNITY_WEBGL
            Debug.LogError("[VersusLanTransport] TCP/UDP LAN không được hỗ trợ trên nền tảng WebGL!");
            OnJoinFailed?.Invoke("Nền tảng WebGL không hỗ trợ mạng Socket LAN. Vui lòng sử dụng Internet Relay.");
#else
            Stop();

            isHost = false;
            isConnected = false;
            _isRunning = true;

            string targetIp = ResolveIpFromCode(inputCode.Trim());
            remoteIp = targetIp;
            roomCode = inputCode.Trim();

            Debug.Log($"[VersusLanTransport] Client đang kết nối tới {targetIp}:{port}...");

            _networkThread = new Thread(() => ClientConnectLoop(targetIp, port, playerName, playerCharacter));
            _networkThread.IsBackground = true;
            _networkThread.Start();
#endif
        }

#if !UNITY_WEBGL
        private void ServerListenLoop()
        {
            try
            {
                while (_isRunning && _serverListener != null)
                {
                    var incomingClient = _serverListener.AcceptTcpClient();

                    // Chặn máy thứ 3 khi phòng đã có đủ 2 người
                    if (isConnected && _clientSocket != null)
                    {
                        Debug.LogWarning("[VersusLanTransport] Có máy thứ 3 kết nối khi phòng đã đủ người! Gửi REJECT.");
                        RejectClient(incomingClient, "Phòng đã đủ 2 người chơi!");
                        continue;
                    }

                    _clientSocket = incomingClient;
                    _stream = incomingClient.GetStream();
                    ReceiveLoop();
                }
            }
            catch (Exception ex)
            {
                if (_isRunning) Debug.Log($"[VersusLanTransport] Server Loop kết thúc: {ex.Message}");
            }
        }

        private void RejectClient(TcpClient client, string reason)
        {
            try
            {
                var packet = new VersusNetPacket { type = "REJECT", rejectReason = reason };
                string json = JsonUtility.ToJson(packet);
                byte[] data = Encoding.UTF8.GetBytes(json);
                byte[] len = BitConverter.GetBytes(data.Length);
                var stream = client.GetStream();
                stream.Write(len, 0, len.Length);
                stream.Write(data, 0, data.Length);
                stream.Flush();
                client.Close();
            }
            catch {}
        }

        private void ClientConnectLoop(string ip, int port, string playerName, string playerCharacter)
        {
            try
            {
                _clientSocket = new TcpClient();
                var result = _clientSocket.BeginConnect(ip, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(6));

                if (!success || !_clientSocket.Connected)
                {
                    _clientSocket.Close();
                    throw new Exception($"Hết thời gian chờ kết nối tới {ip}:{port}. Vui lòng kiểm tra lại mã phòng hoặc IP!");
                }

                _clientSocket.EndConnect(result);
                _stream = _clientSocket.GetStream();

                var hello = new VersusNetPacket
                {
                    type = "HANDSHAKE",
                    senderId = "Guest_" + DateTime.UtcNow.Ticks,
                    senderName = playerName,
                    senderCharacter = playerCharacter
                };
                SendPacketDirect(hello);

                ReceiveLoop();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[VersusLanTransport] Kết nối thất bại: {ex.Message}");
                EnqueueMainThread(() =>
                {
                    isConnected = false;
                    OnJoinFailed?.Invoke($"Không thể kết nối tới phòng: {ex.Message}");
                });
            }
        }

        private void ReceiveLoop()
        {
            byte[] lenBuffer = new byte[4];
            while (_isRunning && _stream != null)
            {
                int readLen = 0;
                try { readLen = _stream.Read(lenBuffer, 0, 4); } catch { break; }
                if (readLen < 4) break;

                int packetSize = BitConverter.ToInt32(lenBuffer, 0);
                if (packetSize <= 0 || packetSize > 131072) break;

                byte[] dataBuffer = new byte[packetSize];
                int totalRead = 0;
                while (totalRead < packetSize)
                {
                    int r = _stream.Read(dataBuffer, totalRead, packetSize - totalRead);
                    if (r <= 0) break;
                    totalRead += r;
                }

                if (totalRead == packetSize)
                {
                    string json = Encoding.UTF8.GetString(dataBuffer);
                    EnqueueMainThread(() => ProcessIncomingPacket(json));
                }
            }

            EnqueueMainThread(() =>
            {
                if (isConnected)
                {
                    isConnected = false;
                    Debug.LogWarning("[VersusLanTransport] Đối thủ đã mất kết nối mạng hoặc thoát phòng.");
                    OnOpponentDisconnected?.Invoke();
                }
            });
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
                                senderName = VersusSessionState.ActiveConfig.playerName,
                                senderCharacter = VersusSessionState.ActiveConfig.playerCharacter
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
                        Debug.Log($"[VersusLanTransport] Nhận MATCH_START! Seed={packet.questionSeed}");
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
                Debug.LogError($"[VersusLanTransport] Parse packet error: {ex.Message}");
            }
        }

        private void SendPacketDirect(VersusNetPacket packet)
        {
            if (!_isRunning || _stream == null) return;
            try
            {
                string json = JsonUtility.ToJson(packet);
                byte[] data = Encoding.UTF8.GetBytes(json);
                byte[] len = BitConverter.GetBytes(data.Length);

                _stream.Write(len, 0, len.Length);
                _stream.Write(data, 0, data.Length);
                _stream.Flush();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[VersusLanTransport] Gửi gói tin thất bại: {ex.Message}");
            }
        }
#endif

        public void SendPlayerPosition(Vector2 pos)
        {
#if !UNITY_WEBGL
            if (!isConnected) return;
            if (Time.unscaledTime - _lastPosSendTime < 0.05f) return;
            _lastPosSendTime = Time.unscaledTime;

            var packet = new VersusNetPacket { type = "POS", posX = pos.x, posY = pos.y };
            SendPacketDirect(packet);
#endif
        }

        public void SendSolveTower(int towerNum)
        {
#if !UNITY_WEBGL
            if (!isConnected) return;
            var packet = new VersusNetPacket
            {
                type = "SOLVE_TOWER",
                towerNumber = towerNum,
                claimer = isHost ? 1 : 2
            };
            SendPacketDirect(packet);
#endif
        }

        public void BroadcastMatchStart(int seed, List<string> questionIDs)
        {
#if !UNITY_WEBGL
            if (!isConnected || !isHost) return;
            var wrapper = new QuestionIDWrapper { items = questionIDs };
            var packet = new VersusNetPacket
            {
                type = "MATCH_START",
                questionSeed = seed,
                questionIDsJson = JsonUtility.ToJson(wrapper)
            };
            SendPacketDirect(packet);
#endif
        }

        public void BroadcastMatchFinish(bool isHostWinner, string reason)
        {
#if !UNITY_WEBGL
            if (!isConnected) return;
            var packet = new VersusNetPacket
            {
                type = "MATCH_FINISH",
                claimer = isHostWinner ? 1 : 2,
                rejectReason = reason
            };
            SendPacketDirect(packet);
#endif
        }

        public void LeaveRoom()
        {
#if !UNITY_WEBGL
            if (isConnected)
            {
                var packet = new VersusNetPacket { type = "LEAVE" };
                SendPacketDirect(packet);
            }
#endif
            Stop();
        }

        public void Stop()
        {
            isHost = false;
            isConnected = false;
#if !UNITY_WEBGL
            _isRunning = false;
            StopUdpDiscovery();
            try
            {
                if (_stream != null) { _stream.Close(); _stream = null; }
                if (_clientSocket != null) { _clientSocket.Close(); _clientSocket = null; }
                if (_serverListener != null) { _serverListener.Stop(); _serverListener = null; }
                if (_networkThread != null && _networkThread.IsAlive) { _networkThread.Abort(); _networkThread = null; }
            }
            catch {}
#endif
        }

        public void StartDiscovery()
        {
#if !UNITY_WEBGL
            StartUdpDiscovery();
#endif
        }

#if !UNITY_WEBGL
        private void StartUdpBroadcaster()
        {
            try
            {
                _udpBroadcaster = new UdpClient();
                _udpBroadcaster.EnableBroadcast = true;
            }
            catch {}
        }

        private void SendUdpBeacon()
        {
            try
            {
                if (_udpBroadcaster == null) return;
                string name = VersusSessionState.ActiveConfig.playerName;
                string ch = VersusSessionState.ActiveConfig.playerCharacter;
                string msg = $"AURELIA_ROOM|{roomCode}|{name}|{ch}|{DEFAULT_PORT}";
                byte[] data = Encoding.UTF8.GetBytes(msg);
                var ep = new IPEndPoint(IPAddress.Broadcast, UDP_BEACON_PORT);
                _udpBroadcaster.Send(data, data.Length, ep);
            }
            catch {}
        }

        private void StartUdpDiscovery()
        {
            StopUdpDiscovery();
            _isUdpRunning = true;
            try
            {
                _udpListener = new UdpClient();
                _udpListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpListener.Client.Bind(new IPEndPoint(IPAddress.Any, UDP_BEACON_PORT));

                _udpListenThread = new Thread(UdpListenLoop);
                _udpListenThread.IsBackground = true;
                _udpListenThread.Start();
            }
            catch {}
        }

        private void UdpListenLoop()
        {
            var ep = new IPEndPoint(IPAddress.Any, 0);
            while (_isUdpRunning && _udpListener != null)
            {
                try
                {
                    byte[] data = _udpListener.Receive(ref ep);
                    string msg = Encoding.UTF8.GetString(data);

                    if (msg.StartsWith("AURELIA_ROOM|"))
                    {
                        string[] parts = msg.Split('|');
                        if (parts.Length >= 5)
                        {
                            string rCode = parts[1];
                            string hName = parts[2];
                            string hChar = parts[3];
                            int port = int.TryParse(parts[4], out int p) ? p : DEFAULT_PORT;
                            string ip = ep.Address.ToString();

                            if (ip == "127.0.0.1" || ip == localIp) continue;

                            lock (_discoveredRooms)
                            {
                                if (!_discoveredRooms.ContainsKey(rCode))
                                {
                                    _discoveredRooms[rCode] = new DiscoveredRoom();
                                }
                                var rm = _discoveredRooms[rCode];
                                rm.roomCode = rCode;
                                rm.hostName = hName;
                                rm.hostCharacter = hChar;
                                rm.ipAddress = ip;
                                rm.port = port;
                                rm.lastSeenTime = Time.unscaledTime;
                            }

                            EnqueueMainThread(TriggerRoomsUpdated);
                        }
                    }
                }
                catch { break; }
            }
        }

        private void TriggerRoomsUpdated()
        {
            var list = new List<DiscoveredRoom>();
            lock (_discoveredRooms)
            {
                list.AddRange(_discoveredRooms.Values);
            }
            OnRoomsUpdated?.Invoke(list);
        }

        private void StopUdpDiscovery()
        {
            _isUdpRunning = false;
            try
            {
                if (_udpBroadcaster != null) { _udpBroadcaster.Close(); _udpBroadcaster = null; }
                if (_udpListener != null) { _udpListener.Close(); _udpListener = null; }
                if (_udpListenThread != null && _udpListenThread.IsAlive) { _udpListenThread.Abort(); _udpListenThread = null; }
            }
            catch {}
        }

        public static string ResolveLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        string s = ip.ToString();
                        if (s.StartsWith("192.168.") || s.StartsWith("10.")) return s;
                        if (s.StartsWith("172.") && !s.StartsWith("172.25.") && !s.StartsWith("172.17.")) return s;
                    }
                }
            }
            catch {}
            return "127.0.0.1";
        }

        public static string ResolveIpFromCode(string code)
        {
            code = code.Trim();
            if (IPAddress.TryParse(code, out _)) return code;

            if (int.TryParse(code, out int octet) && octet > 0 && octet <= 254)
            {
                string myIp = ResolveLocalIPAddress();
                int lastDot = myIp.LastIndexOf('.');
                if (lastDot > 0)
                {
                    return myIp.Substring(0, lastDot + 1) + octet;
                }
            }
            return code;
        }

        private void EnqueueMainThread(Action action)
        {
            lock (_queueLock)
            {
                _mainThreadQueue.Enqueue(action);
            }
        }
#endif
    }
}
