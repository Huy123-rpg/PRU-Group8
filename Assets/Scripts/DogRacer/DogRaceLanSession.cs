using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Desktop LAN 1v1. All socket polling and game rules run on the Unity thread.</summary>
public sealed class DogRaceLanSession : MonoBehaviour
{
    public const string ModeScene = "Scene5_PlayMode";
    public static DogRaceLanSession Instance { get; private set; }
    public static bool Active => Instance != null && Instance.IsPvp;
    public bool IsPvp { get; private set; }
    public bool IsHost { get; private set; }
    public bool Connected { get; private set; }
    public bool Racing { get; private set; }
    public bool Finished { get; private set; }
    public int LocalPlayer => IsHost ? 0 : 1;
    public int LocalDog => dogs[LocalPlayer];
    public int RemoteDog => dogs[1 - LocalPlayer];
    public string Status { get; private set; } = "";
    public float Elapsed { get; private set; }
    public float[] Progress { get; } = new float[2];
    public int[] Scores { get; } = new int[2];
    public int[] Correct { get; } = new int[2];
    public int[] Wrong { get; } = new int[2];
    public float[] Boost { get; } = new float[2];
    public int QuestionSequence { get; private set; } = -1;
    public QuestionData CurrentQuestion { get; private set; }
    public string LastResult { get; private set; } = "";
    public bool LocalReady => ready[LocalPlayer];

    [SerializeField] private int discoveryPort = 47778;
    // Fallbacks match Scene6_Race; the host must receive the scene configuration before starting.
    [SerializeField] private float baseSpeed = 0.0055f;
    [SerializeField] private float boostSpeed = 0.015f;
    [SerializeField] private float boostSeconds = 10f;
    private const string Protocol = "DOG_RACE_LAN_1";
    private readonly int[] dogs = new int[2];
    private readonly bool[] ready = new bool[2];
    private readonly bool[] loaded = new bool[2];
    private readonly int[] cursor = new int[2];
    private readonly List<QuestionData>[] decks = { new List<QuestionData>(), new List<QuestionData>() };
    private readonly float[] answerAt = new float[2];
    private readonly string[] feedback = { "", "" };
    private readonly List<byte> incoming = new List<byte>();
    private readonly List<byte> outgoing = new List<byte>();
    private UdpClient discovery;
    private TcpListener listener;
    private TcpClient peer;
    private IAsyncResult connecting;
    private string roomId, context;
    private float advertiseAt, heartbeatAt, lastReceive, connectedAt, snapshotAt, startAt = -1;
    private bool preparing, localLoaded, answerPending;
    private bool raceConfigured;
    private bool oldRunInBackground;
    private string failure = "";

    [Serializable] private class Wire
    {
        public string type, protocol = Protocol, room, context, text, prompt;
        public int port, dog, sequence, option, winner;
        public int[] dogs, scores, correct, wrong;
        public bool[] ready;
        public float[] progress, boost;
        public float elapsed, countdown;
        public string[] options;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic() { Instance = null; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null) new GameObject("DogRace LAN").AddComponent<DogRaceLanSession>();
    }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += SceneLoaded;
    }
    private void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= SceneLoaded;
        Leave();
        Instance = null;
    }
    private void OnApplicationQuit() { Leave(); }
    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsPvp) return;
        if (scene.name == ModeScene) { Leave(); return; }
        if (scene.name == NavigationManager.SCENE_RACE && !localLoaded)
        {
            localLoaded = true;
            if (IsHost) loaded[0] = true;
            else Send(new Wire { type = "loaded" });
        }
        else if (scene.name != NavigationManager.SCENE_DOG_SELECT &&
                 scene.name != NavigationManager.SCENE_RESULTS && scene.name != ModeScene &&
                 scene.name != NavigationManager.SCENE_RACE) Leave();
    }
    public void BeginSearch()
    {
        Leave();
        if (GameSession.Instance.lessonType == LessonType.Essay) return;
        IsPvp = true;
        oldRunInBackground = Application.runInBackground;
        Application.runInBackground = true;
        roomId = Guid.NewGuid().ToString("N");
        context = "Chemistry:" + (GameSession.Instance.selectedChapter != null ? GameSession.Instance.selectedChapter.chapterNumber : 0)
            + ":" + GameSession.Instance.selectedLessonNumber;
        try
        {
            listener = new TcpListener(IPAddress.Any, 0);
            listener.Start(1);
            discovery = new UdpClient();
            discovery.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            discovery.Client.Bind(new IPEndPoint(IPAddress.Any, discoveryPort));
            discovery.EnableBroadcast = true;
            discovery.Client.Blocking = false;
            Status = "Đang tự tìm đối thủ cùng chương/bài trong mạng LAN…";
            failure = "";
        }
        catch (Exception e) { Fail("Không mở được kết nối LAN: " + e.Message); }
    }
    public void Leave()
    {
        StopAllCoroutines();
        if (IsPvp) Application.runInBackground = oldRunInBackground;
        IsPvp = Connected = Racing = Finished = preparing = localLoaded = answerPending = false;
        raceConfigured = false;
        try { peer?.Close(); discovery?.Close(); listener?.Stop(); } catch (SocketException) { }
        peer = null; discovery = null; listener = null; connecting = null;
        incoming.Clear(); outgoing.Clear();
        Array.Clear(dogs, 0, 2); Array.Clear(ready, 0, 2); Array.Clear(loaded, 0, 2);
        Array.Clear(Progress, 0, 2); Array.Clear(Scores, 0, 2);
        Array.Clear(Correct, 0, 2); Array.Clear(Wrong, 0, 2); Array.Clear(Boost, 0, 2);
        Array.Clear(cursor, 0, 2);
        decks[0].Clear(); decks[1].Clear();
        startAt = -1; Elapsed = 0; QuestionSequence = -1; CurrentQuestion = null;
        LastResult = "";
    }
    private void Fail(string reason)
    {
        Leave();
        failure = reason;
        Debug.LogWarning("[DogRace LAN] " + reason);
        SceneManager.LoadScene(ModeScene);
    }
    private void Update()
    {
        if (!IsPvp) return;
        try
        {
            if (peer == null) Discover();
            if (connecting != null && connecting.IsCompleted)
            {
                peer.EndConnect(connecting); connecting = null;
                SetupPeer(false);
                Send(new Wire { type = "hello", room = roomId, context = context });
            }
            if (connecting != null && Time.realtimeSinceStartup - connectedAt > 6f)
            { Fail("Không kết nối được phòng. Hãy thử tìm lại và kiểm tra Firewall."); return; }
            if (peer == null || connecting != null) return;
            Pump();
            if (peer == null) return;
            if (!Finished && Time.realtimeSinceStartup - lastReceive > 12f) { Fail("Mất kết nối với đối thủ. Trận đã hủy."); return; }
            if (!Connected && Time.realtimeSinceStartup - connectedAt > 6f) { Fail("Không xác nhận được phòng LAN."); return; }
            if (Time.realtimeSinceStartup >= heartbeatAt)
            {
                heartbeatAt = Time.realtimeSinceStartup + 1;
                Send(new Wire { type = "ping" });
            }
            if (IsHost && Connected)
            {
                if (ready[0] && ready[1] && !preparing) StartCoroutine(PrepareRace());
                if (preparing && raceConfigured && loaded[0] && loaded[1] && startAt < 0)
                {
                    startAt = Time.realtimeSinceStartup + 3f;
                    answerAt[0] = answerAt[1] = startAt;
                }
                if (startAt > 0 && !Finished)
                {
                    Racing = Time.realtimeSinceStartup >= startAt;
                    if (Racing) Advance(Time.unscaledDeltaTime);
                }
                if (Time.realtimeSinceStartup >= snapshotAt)
                {
                    snapshotAt = Time.realtimeSinceStartup + .1f;
                    Publish();
                }
            }
            Flush();
        }
        catch (Exception e) when (e is SocketException || e is System.IO.IOException || e is ObjectDisposedException || e is ArgumentException)
        {
            if (Finished) { peer?.Close(); peer = null; Connected = false; }
            else Fail("Kết nối LAN bị gián đoạn: " + e.Message);
        }
    }
    private void Discover()
    {
        if (listener == null || discovery == null) return;
        if (listener.Pending())
        {
            peer = listener.AcceptTcpClient(); SetupPeer(true); return;
        }
        if (Time.realtimeSinceStartup >= advertiseAt)
        {
            advertiseAt = Time.realtimeSinceStartup + .6f;
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Wire {
                type = "offer", room = roomId, context = context,
                port = ((IPEndPoint)listener.LocalEndpoint).Port }));
            discovery.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, discoveryPort));
        }
        for (int n = 0; n < 16 && discovery.Available > 0; n++)
        {
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = discovery.Receive(ref remote);
            if (data.Length > 4096) continue;
            Wire offer;
            try { offer = JsonUtility.FromJson<Wire>(Encoding.UTF8.GetString(data)); } catch (ArgumentException) { continue; }
            if (offer == null || offer.protocol != Protocol || offer.type != "offer" || offer.context != context ||
                string.IsNullOrEmpty(offer.room) || string.CompareOrdinal(roomId, offer.room) <= 0 || offer.port < 1 || offer.port > 65535) continue;
            // A deterministic ordering prevents two waiting players joining each other simultaneously.
            peer = new TcpClient(); connectedAt = Time.realtimeSinceStartup;
            connecting = peer.BeginConnect(remote.Address, offer.port, null, null);
            Status = "Đang kết nối đối thủ…";
            break;
        }
    }
    private void SetupPeer(bool host)
    {
        IsHost = host; peer.NoDelay = true; peer.Client.Blocking = false;
        connectedAt = lastReceive = Time.realtimeSinceStartup;
        listener?.Stop(); listener = null;
        discovery?.Close(); discovery = null;
    }
    private void Pump()
    {
        Socket socket = peer.Client;
        if (socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0) throw new System.IO.IOException("Đối thủ đã đóng kết nối");
        byte[] buffer = new byte[8192];
        for (int n = 0; n < 16 && socket.Available > 0; n++)
        {
            int count = socket.Receive(buffer, 0, Math.Min(buffer.Length, socket.Available), SocketFlags.None);
            for (int i = 0; i < count; i++) incoming.Add(buffer[i]);
            if (incoming.Count > 131072) throw new System.IO.IOException("Gói tin quá lớn");
        }
        for (int n = 0; n < 64; n++)
        {
            int end = incoming.IndexOf(10);
            if (end < 0) break;
            string json = Encoding.UTF8.GetString(incoming.GetRange(0, end).ToArray());
            incoming.RemoveRange(0, end + 1);
            Wire message = JsonUtility.FromJson<Wire>(json);
            if (message == null || message.protocol != Protocol) throw new System.IO.IOException("Sai phiên bản giao thức");
            lastReceive = Time.realtimeSinceStartup;
            Receive(message);
            if (peer == null) break;
        }
    }
    private void Send(Wire message)
    {
        if (peer == null) return;
        byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(message) + "\n");
        if (outgoing.Count + bytes.Length > 262144) throw new System.IO.IOException("Đường truyền quá chậm");
        outgoing.AddRange(bytes);
    }
    private void Flush()
    {
        if (outgoing.Count == 0 || peer == null || !peer.Client.Poll(0, SelectMode.SelectWrite)) return;
        byte[] bytes = outgoing.ToArray();
        int count = peer.Client.Send(bytes, SocketFlags.None);
        outgoing.RemoveRange(0, count);
    }
    private void Receive(Wire m)
    {
        if (m.type == "ping") return;
        if (IsHost)
        {
            if (m.type == "hello" && !Connected)
            {
                if (m.context != context) { Fail("Đối thủ chọn chương/bài khác."); return; }
                Connected = true; Status = "Đã ghép đối thủ — chọn chó rồi bấm Sẵn sàng";
                Send(new Wire { type = "welcome" }); Publish();
            }
            else if (Connected && m.type == "dog") ChooseDog(1, m.dog);
            else if (Connected && m.type == "ready") SetReady(1);
            else if (Connected && m.type == "loaded") loaded[1] = true;
            else if (Connected && m.type == "answer") ValidateAnswer(1, m.sequence, m.option);
        }
        else
        {
            if (m.type == "welcome") { Connected = true; Status = "Đã ghép đối thủ — chọn chó rồi bấm Sẵn sàng"; }
            else if (Connected && m.type == "state") ApplyState(m);
            else if (Connected && m.type == "load") SceneManager.LoadScene(NavigationManager.SCENE_RACE);
            else if (Connected && m.type == "finish") Complete(m.winner);
            else if (m.type == "cancel") Fail(m.text);
        }
    }
    public void SelectDog(int dog)
    {
        if (!Connected || preparing) return;
        if (IsHost) ChooseDog(0, dog); else Send(new Wire { type = "dog", dog = dog });
    }
    private void ChooseDog(int player, int dog)
    {
        if (preparing || dog < 1 || dog > 4 || dogs[1 - player] == dog || ready[player]) return;
        dogs[player] = dog; Publish();
    }
    public void Ready()
    {
        if (!Connected || LocalDog == 0) return;
        if (IsHost) SetReady(0); else Send(new Wire { type = "ready" });
    }
    private void SetReady(int player)
    {
        if (dogs[player] == 0 || preparing) return;
        ready[player] = true; Publish();
    }
    private IEnumerator PrepareRace()
    {
        preparing = true; Status = "Đang tải ngân hàng câu hỏi trắc nghiệm…";
        bool success = false;
        yield return GoogleSheetDataManager.Instance.FetchQuestionsFromSheet(ok => success = ok, false);
        if (!IsPvp) yield break;
        var bank = GoogleSheetDataManager.Instance.GetQuestionsForSubject(SubjectType.Chemistry);
        bank.RemoveAll(q => q == null || q.loaiCauHoi == "Tự luận" || !string.IsNullOrWhiteSpace(q.dapAnThamChieu)
            || q.options == null || q.options.Length != 4 || Array.Exists(q.options, string.IsNullOrWhiteSpace));
        if (!success || bank.Count < 2)
        {
            Send(new Wire { type = "cancel", text = "Cần ít nhất 2 câu trắc nghiệm hợp lệ từ Sheet. Hãy kiểm tra mạng và quyền chia sẻ." });
            Flush(); Fail("Không đủ câu hỏi từ Sheet để bắt đầu PvP."); yield break;
        }
        for (int i = bank.Count - 1; i > 0; i--)
        { int j = UnityEngine.Random.Range(0, i + 1); var q = bank[i]; bank[i] = bank[j]; bank[j] = q; }
        for (int i = 0; i < bank.Count; i++) decks[i % 2].Add(bank[i]);
        Send(new Wire { type = "load" }); Flush();
        GameSession.Instance.selectedDogId = LocalDog;
        SceneManager.LoadScene(NavigationManager.SCENE_RACE);
    }
    public void ConfigureRace(float speed, float extra, float seconds)
    {
        if (!IsHost) return;
        baseSpeed = Mathf.Max(.001f, speed); boostSpeed = Mathf.Max(0, extra); boostSeconds = Mathf.Max(0, seconds);
        raceConfigured = true;
    }
    public void Answer(int option)
    {
        if (!Racing || Finished || answerPending || CurrentQuestion == null) return;
        answerPending = true;
        if (IsHost) ValidateAnswer(0, QuestionSequence, option);
        else Send(new Wire { type = "answer", sequence = QuestionSequence, option = option });
    }
    private void ValidateAnswer(int player, int sequence, int option)
    {
        if (!Racing || Finished || option < 0 || option > 3 || sequence != cursor[player] ||
            Time.realtimeSinceStartup - answerAt[player] < .35f) { if (player == 0) answerPending = false; return; }
        var question = decks[player][cursor[player] % decks[player].Count];
        bool correct = question.correctOptionIndex == option;
        if (correct) { Correct[player]++; Scores[player] += 100; Boost[player] = boostSeconds; }
        else Wrong[player]++;
        feedback[player] = correct ? "Đúng! Tăng tốc" : "Sai — tiếp tục cố gắng";
        cursor[player]++; answerAt[player] = Time.realtimeSinceStartup;
        Publish();
    }
    private void Advance(float delta)
    {
        delta = Mathf.Min(delta, .1f);
        Elapsed += delta;
        float[] crossing = { float.PositiveInfinity, float.PositiveInfinity };
        for (int i = 0; i < 2; i++)
        {
            float speed = baseSpeed + (Boost[i] > 0 ? boostSpeed : 0);
            if (Progress[i] + speed * delta >= 1) crossing[i] = (1 - Progress[i]) / speed;
            Progress[i] = Mathf.Min(1, Progress[i] + speed * delta);
            Boost[i] = Mathf.Max(0, Boost[i] - delta);
        }
        if (!float.IsPositiveInfinity(crossing[0]) || !float.IsPositiveInfinity(crossing[1]))
        {
            int winner = Mathf.Abs(crossing[0] - crossing[1]) < .0001f ? -1 : (crossing[0] < crossing[1] ? 0 : 1);
            Publish(); Send(new Wire { type = "finish", winner = winner }); Flush(); Complete(winner);
        }
    }
    private Wire Snapshot(int player)
    {
        QuestionData q = decks[player].Count > 0 ? decks[player][cursor[player] % decks[player].Count] : null;
        return new Wire { type = "state", dogs = dogs, ready = ready, progress = Progress,
            scores = Scores, correct = Correct, wrong = Wrong, boost = Boost, elapsed = Elapsed,
            countdown = startAt < 0 ? -1 : Mathf.Max(0, startAt - Time.realtimeSinceStartup),
            sequence = cursor[player], prompt = q != null ? q.questionText : "",
            options = q != null ? q.options : null, text = feedback[player] };
    }
    private void Publish() { if (!Connected || !IsHost) return; ApplyState(Snapshot(0)); Send(Snapshot(1)); }
    private void ApplyState(Wire m)
    {
        if (m.dogs == null || m.dogs.Length != 2 || m.progress == null || m.progress.Length != 2 ||
            m.scores == null || m.correct == null || m.wrong == null || m.boost == null || m.ready == null ||
            m.scores.Length != 2 || m.correct.Length != 2 || m.wrong.Length != 2 || m.boost.Length != 2 || m.ready.Length != 2)
            throw new System.IO.IOException("Trạng thái trận không hợp lệ");
        Array.Copy(m.dogs, dogs, 2); Array.Copy(m.ready, ready, 2);
        if (!IsHost)
        {
            Array.Copy(m.progress, Progress, 2); Array.Copy(m.scores, Scores, 2);
            Array.Copy(m.correct, Correct, 2); Array.Copy(m.wrong, Wrong, 2); Array.Copy(m.boost, Boost, 2);
            Elapsed = m.elapsed; Racing = m.countdown == 0 && localLoaded && !Finished;
        }
        Status = m.countdown > 0 ? "Bắt đầu sau " + Mathf.CeilToInt(m.countdown) + "…" :
            (Racing ? "Đang đua — " + m.text : "Bạn: " + (ready[LocalPlayer] ? "sẵn sàng" : "chọn chó")
             + " · Đối thủ: " + (ready[1 - LocalPlayer] ? "sẵn sàng" : "chọn chó"));
        if (!string.IsNullOrEmpty(m.prompt) && (CurrentQuestion == null || QuestionSequence != m.sequence))
        {
            if (CurrentQuestion != null) Destroy(CurrentQuestion);
            CurrentQuestion = ScriptableObject.CreateInstance<QuestionData>();
            CurrentQuestion.questionText = m.prompt; CurrentQuestion.options = m.options;
            CurrentQuestion.subject = SubjectType.Chemistry;
            QuestionSequence = m.sequence; answerPending = false;
        }
        GameSession.Instance.selectedDogId = LocalDog;
        answerPending = false;
    }
    public string GetDiagnosticState() { return JsonUtility.ToJson(Snapshot(LocalPlayer)); }
    private void Complete(int winner)
    {
        if (Finished) return;
        Finished = true; Racing = false;
        int p = LocalPlayer;
        LastResult = winner < 0 ? "HÒA" : (winner == p ? "BẠN CHIẾN THẮNG" : "ĐỐI THỦ CHIẾN THẮNG");
        GameSession.Instance.lastRaceResult = new RaceResult { rank = winner < 0 || winner == p ? 1 : 2,
            totalScore = Scores[p], correctAnswers = Correct[p], wrongAnswers = Wrong[p], completionTime = Elapsed };
        SceneManager.LoadScene(NavigationManager.SCENE_RESULTS);
    }
    private void OnGUI()
    {
        string scene = SceneManager.GetActiveScene().name;
        bool mode = scene == ModeScene;
        if (!mode && !IsPvp) return;
        float scale = Mathf.Min(Screen.width / 1000f, Screen.height / 650f);
        Matrix4x4 previous = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
        float width = Screen.width / scale;
        var label = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        var button = new GUIStyle(GUI.skin.button) { fontSize = 22 };
        if (mode)
        {
            GUI.Box(new Rect(width / 2 - 350, 100, 700, 430), "");
            GUI.Label(new Rect(width / 2 - 330, 120, 660, 65), "DOG RACER · CHỌN CHẾ ĐỘ", label);
            if (GUI.Button(new Rect(width / 2 - 280, 210, 560, 65), "CHƠI VỚI MÁY", button))
            { failure = ""; Leave(); NavigationManager.Instance.LoadScene(NavigationManager.SCENE_DOG_SELECT); }
            bool wasEnabled = GUI.enabled;
            GUI.enabled = GameSession.Instance.lessonType != LessonType.Essay;
            if (GUI.Button(new Rect(width / 2 - 280, 295, 560, 65), "PvP LAN · 1 ĐẤU 1 · TRẮC NGHIỆM", button))
            { BeginSearch(); if (IsPvp) NavigationManager.Instance.LoadScene(NavigationManager.SCENE_DOG_SELECT); }
            GUI.enabled = wasEnabled;
            GUI.Label(new Rect(width / 2 - 320, 365, 640, 90), string.IsNullOrEmpty(failure)
                ? "Hai máy cùng mạng LAN, cùng chương/bài.\nTự tìm phòng · Mỗi người một chó · Câu hỏi riêng" : failure, label);
            if (GUI.Button(new Rect(width / 2 - 100, 465, 200, 45), "Quay lại", button))
            { Leave(); NavigationManager.Instance.LoadScene(NavigationManager.SCENE_CHAPTER_SELECT); }
        }
        else if (IsPvp)
        {
            GUI.Box(new Rect(0, 0, width, 75), "");
            GUI.Label(new Rect(10, 0, width - 160, 70), Finished ? LastResult : Status, label);
            if (GUI.Button(new Rect(width - 140, 15, 125, 45), "Rời phòng", button))
            { Leave(); SceneManager.LoadScene(ModeScene); }
        }
        GUI.matrix = previous;
    }
}
