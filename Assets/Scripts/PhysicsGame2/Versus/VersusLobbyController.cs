using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển toàn bộ logic giao diện Scene 34. Đối kháng:
    /// - Hiển thị chủ đề/bài học đang chọn
    /// - Mời người chơi / Đấu Online (Tạo phòng 1vs1 hoặc Vào phòng bằng mã/IP)
    /// - Tự động phát hiện phòng trong mạng Wi-Fi/LAN qua UDP Discovery
    /// - Ghép ngẫu nhiên người chơi thật 100%
    /// - Đấu với máy (Bot offline 100%)
    /// - Nút Quay lại về Main Menu
    /// </summary>
    public class VersusLobbyController : MonoBehaviour
    {
        [Header("=== UI ELEMENTS ===")]
        [SerializeField] private Button btnBack;
        [SerializeField] private Button btnInvite;
        [SerializeField] private Button btnRandom;
        [SerializeField] private Button btnBot;
        [SerializeField] private TextMeshProUGUI txtTopic;
        [SerializeField] private TextMeshProUGUI txtDifficulty;

        // Modal Elements
        private GameObject _modalContainer;
        private GameObject _onlinePanel;
        private GameObject _hostWaitPanel;
        private GameObject _matchmakingPanel;

        // Dynamic Text & Inputs
        private TextMeshProUGUI _txtHostRoomCode;
        private TextMeshProUGUI _txtHostStatus;
        private Button _btnHostStartMatch;
        private TMP_InputField _inputJoinCode;
        private TextMeshProUGUI _txtClientStatus;
        private Transform _lanRoomsContainer;
        private readonly List<GameObject> _spawnedRoomRows = new List<GameObject>();

        private Coroutine _matchmakingCoroutine;

        private void Awake()
        {
            EnsureNetworkManager();
            AutoBindUI();
            CreateModals();
        }

        private void Start()
        {
            NavigationService.Instance.EnsureDevContext();
            RenderTopicInfo();
            SetupButtonEvents();
            RegisterNetworkEvents();
        }

        private void OnDestroy()
        {
            UnregisterNetworkEvents();
            if (VersusNetworkManager.Instance != null && !VersusSessionState.ActiveConfig.isOnline)
            {
                VersusNetworkManager.Instance.StopNetwork();
            }
        }

        private void EnsureNetworkManager()
        {
            if (VersusNetworkManager.Instance == null)
            {
                var netGo = new GameObject("VersusNetworkManager");
                netGo.AddComponent<VersusNetworkManager>();
            }
        }

        private void RegisterNetworkEvents()
        {
            if (VersusNetworkManager.Instance == null) return;

            VersusNetworkManager.Instance.OnRoomCreated += HandleRoomCreated;
            VersusNetworkManager.Instance.OnOpponentJoined += HandleOpponentJoined;
            VersusNetworkManager.Instance.OnJoinFailed += HandleJoinFailed;
            VersusNetworkManager.Instance.OnOpponentDisconnected += HandleOpponentDisconnected;
            VersusNetworkManager.Instance.OnRoomsUpdated += HandleLanRoomsUpdated;
        }

        private void UnregisterNetworkEvents()
        {
            if (VersusNetworkManager.Instance == null) return;

            VersusNetworkManager.Instance.OnRoomCreated -= HandleRoomCreated;
            VersusNetworkManager.Instance.OnOpponentJoined -= HandleOpponentJoined;
            VersusNetworkManager.Instance.OnJoinFailed -= HandleJoinFailed;
            VersusNetworkManager.Instance.OnOpponentDisconnected -= HandleOpponentDisconnected;
            VersusNetworkManager.Instance.OnRoomsUpdated -= HandleLanRoomsUpdated;
        }

        private void AutoBindUI()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. Nút Quay lại
            if (btnBack == null)
            {
                foreach (var b in canvas.GetComponentsInChildren<Button>(true))
                {
                    if (b.name.Contains("Quay") || b.name.Contains("Back") || (b.transform.localPosition.x < -600f && b.transform.localPosition.y > 350f))
                    {
                        btnBack = b;
                        break;
                    }
                }
                if (btnBack == null)
                {
                    foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                    {
                        var pos = img.rectTransform.anchoredPosition;
                        if (pos.x < -650f && pos.y > 380f && img.rectTransform.sizeDelta.x < 350f)
                        {
                            btnBack = img.gameObject.GetComponent<Button>() ?? img.gameObject.AddComponent<Button>();
                            btnBack.targetGraphic = img;
                            break;
                        }
                    }
                }
            }

            foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                var pos = tmp.rectTransform.anchoredPosition;
                if (pos.x < -600f && pos.y > 350f && (tmp.text.Contains("Quay") || tmp.text.Contains("Back")))
                {
                    tmp.raycastTarget = false;
                    var tBtn = tmp.gameObject.GetComponent<Button>() ?? tmp.gameObject.AddComponent<Button>();
                    tBtn.onClick.RemoveAllListeners();
                    tBtn.onClick.AddListener(OnBackClicked);
                }
            }

            // 2. Thẻ Mời người chơi / Đấu Online (bên trái, X ~ -370)
            if (btnInvite == null)
            {
                foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                {
                    var pos = img.rectTransform.anchoredPosition;
                    if (pos.x > -500f && pos.x < -200f && pos.y > -200f && pos.y < 50f && img.rectTransform.sizeDelta.x > 300f)
                    {
                        btnInvite = img.gameObject.GetComponent<Button>() ?? img.gameObject.AddComponent<Button>();
                        btnInvite.targetGraphic = img;
                        break;
                    }
                }
            }

            foreach (var g in canvas.GetComponentsInChildren<Graphic>(true))
            {
                if (g.gameObject == (btnInvite != null ? btnInvite.gameObject : null)) continue;
                var pos = g.rectTransform.anchoredPosition;
                if (pos.x > -520f && pos.x < -180f && pos.y > -320f && pos.y < 100f)
                {
                    g.raycastTarget = false;
                    var subBtn = g.gameObject.GetComponent<Button>() ?? g.gameObject.AddComponent<Button>();
                    subBtn.onClick.RemoveAllListeners();
                    subBtn.onClick.AddListener(OnInviteClicked);
                }
            }

            // 3. Thẻ Ghép ngẫu nhiên (bên phải, X ~ 368)
            if (btnRandom == null)
            {
                foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                {
                    var pos = img.rectTransform.anchoredPosition;
                    if (pos.x > 200f && pos.x < 500f && pos.y > -200f && pos.y < 50f && img.rectTransform.sizeDelta.x > 300f)
                    {
                        btnRandom = img.gameObject.GetComponent<Button>() ?? img.gameObject.AddComponent<Button>();
                        btnRandom.targetGraphic = img;
                        break;
                    }
                }
            }

            foreach (var g in canvas.GetComponentsInChildren<Graphic>(true))
            {
                if (g.gameObject == (btnRandom != null ? btnRandom.gameObject : null)) continue;
                var pos = g.rectTransform.anchoredPosition;
                if (pos.x > 180f && pos.x < 520f && pos.y > -320f && pos.y < 100f)
                {
                    g.raycastTarget = false;
                    var subBtn = g.gameObject.GetComponent<Button>() ?? g.gameObject.AddComponent<Button>();
                    subBtn.onClick.RemoveAllListeners();
                    subBtn.onClick.AddListener(OnRandomMatchClicked);
                }
            }

            // 4. Tìm Text Topic và Difficulty
            foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                string txt = tmp.text.ToLower();
                if (txt.Contains("chủ đề") || txt.Contains("chu de"))
                {
                    txtTopic = tmp;
                }
                else if (txt.Contains("mức độ") || txt.Contains("muc do"))
                {
                    txtDifficulty = tmp;
                }
            }

            // 5. Nút Đấu với máy (Bot)
            if (btnBot == null)
            {
                Transform existingBot = canvas.transform.Find("Btn_BotMatch");
                if (existingBot != null)
                {
                    btnBot = existingBot.GetComponent<Button>();
                }
                else
                {
                    foreach (var b in canvas.GetComponentsInChildren<Button>(true))
                    {
                        string txt = b.GetComponentInChildren<TextMeshProUGUI>(true)?.text.ToLower() ?? "";
                        if (txt.Contains("máy") || txt.Contains("offline") || txt.Contains("bot"))
                        {
                            btnBot = b;
                            break;
                        }
                    }
                    if (btnBot == null)
                    {
                        CreateBotButton(canvas);
                    }
                }
            }
        }

        private void CreateBotButton(Canvas canvas)
        {
            GameObject botGo = new GameObject("Btn_BotMatch");
            botGo.transform.SetParent(canvas.transform, false);

            var rt = botGo.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, -370f);
            rt.sizeDelta = new Vector2(340f, 65f);

            var img = botGo.AddComponent<Image>();
            img.color = new Color(0.12f, 0.22f, 0.42f, 0.95f);

            var outline = botGo.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.85f, 0.35f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);

            btnBot = botGo.AddComponent<Button>();
            btnBot.targetGraphic = img;

            var tGo = new GameObject("Text");
            tGo.transform.SetParent(botGo.transform, false);
            var tRt = tGo.AddComponent<RectTransform>();
            tRt.sizeDelta = rt.sizeDelta;
            tRt.anchoredPosition = Vector2.zero;

            var tmp = tGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "⚔️  ĐẤU VỚI MÁY (OFFLINE)";
            tmp.fontSize = 20f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.95f, 0.6f, 1f);
            tmp.raycastTarget = false;
        }

        private void RenderTopicInfo()
        {
            int ch = 1;
            string lessonTitle = "Ánh sáng";
            if (NavigationService.Instance != null && NavigationService.Instance.Selection != null)
            {
                ch = NavigationService.Instance.Selection.ChapterId > 0 ? NavigationService.Instance.Selection.ChapterId : 1;
                if (!string.IsNullOrEmpty(NavigationService.Instance.Selection.LessonTitle))
                {
                    lessonTitle = NavigationService.Instance.Selection.LessonTitle;
                }
                else
                {
                    var chInfo = PhysicsCurriculumData.GetChapter(ch);
                    lessonTitle = chInfo != null ? chInfo.chapterName : "Ánh sáng";
                }
            }

            if (txtTopic != null) txtTopic.text = $"Chủ đề: {lessonTitle}";
            if (txtDifficulty != null) txtDifficulty.text = "Mức độ: Thông hiểu";
        }

        private void SetupButtonEvents()
        {
            if (btnBack != null)
            {
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackClicked);
            }

            if (btnInvite != null)
            {
                btnInvite.onClick.RemoveAllListeners();
                btnInvite.onClick.AddListener(OnInviteClicked);
            }

            if (btnRandom != null)
            {
                btnRandom.onClick.RemoveAllListeners();
                btnRandom.onClick.AddListener(OnRandomMatchClicked);
            }

            if (btnBot != null)
            {
                btnBot.onClick.RemoveAllListeners();
                btnBot.onClick.AddListener(OnBotMatchClicked);
            }
        }

        private void OnBackClicked()
        {
            Debug.Log("[VersusLobbyController] Quay lại Main Menu");
            if (VersusNetworkManager.Instance != null)
            {
                VersusNetworkManager.Instance.StopNetwork();
            }
            NavigationService.Instance.NavigateTo(RouteId.MainMenu);
        }

        private void OnInviteClicked()
        {
            Debug.Log("[VersusLobbyController] Mở màn hình ĐẤU ĐỐI KHÁNG ONLINE");
            ShowOnlineModal();
        }

        private void OnRandomMatchClicked()
        {
            Debug.Log("[VersusLobbyController] Bắt đầu GHÉP NGẪU NHIÊN NGƯỜI CHƠI THẬT");
            StartTrueRandomMatchmaking();
        }

        private void OnBotMatchClicked()
        {
            Debug.Log("[VersusLobbyController] Bắt đầu ĐẤU VỚI MÁY (BOT OFFLINE)");
            var cfg = VersusSessionState.CreateDefaultConfig();
            cfg.mode = VersusGameMode.Bot;
            cfg.opponentName = "AI Aurelia (" + cfg.opponentCharacter + ")";
            VersusSessionState.ActiveConfig = cfg;

            EnterArena();
        }

        private void EnterArena()
        {
            Time.timeScale = 1f;
            NavigationService.Instance.ResetTransitionLock();
            NavigationService.Instance.NavigateTo(RouteId.VersusArena);
        }

        // ==========================================
        // HỆ THỐNG POPUP MODAL MULTIPLAYER THẬT 1VS1
        // ==========================================

        private void CreateModals()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            _modalContainer = new GameObject("VersusModalContainer");
            _modalContainer.transform.SetParent(canvas.transform, false);
            _modalContainer.transform.SetAsLastSibling();

            var rt = _modalContainer.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            var bg = _modalContainer.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.05f, 0.12f, 0.90f);

            CreateOnlinePanel();
            CreateHostWaitPanel();
            CreateMatchmakingPanel();

            _modalContainer.SetActive(false);
        }

        private void CreateOnlinePanel()
        {
            _onlinePanel = new GameObject("OnlinePanel");
            _onlinePanel.transform.SetParent(_modalContainer.transform, false);
            var rt = _onlinePanel.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(850f, 520f);
            rt.anchoredPosition = Vector2.zero;

            var img = _onlinePanel.AddComponent<Image>();
            img.color = new Color(0.06f, 0.12f, 0.24f, 0.98f);

            var outline = _onlinePanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);

            // Title
            var tGo = new GameObject("Title");
            tGo.transform.SetParent(_onlinePanel.transform, false);
            var tRt = tGo.AddComponent<RectTransform>();
            tRt.anchoredPosition = new Vector2(0f, 225f);
            tRt.sizeDelta = new Vector2(750f, 40f);
            var tmp = tGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "⚔️  ĐẤU ĐỐI KHÁNG ONLINE 1VS1";
            tmp.fontSize = 24f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.9f, 0.4f, 1f);

            // Subtitle
            var subGo = new GameObject("Subtitle");
            subGo.transform.SetParent(_onlinePanel.transform, false);
            var sRt = subGo.AddComponent<RectTransform>();
            sRt.anchoredPosition = new Vector2(0f, 190f);
            sRt.sizeDelta = new Vector2(750f, 30f);
            var sTmp = subGo.AddComponent<TextMeshProUGUI>();
            sTmp.text = "Kết nối 2 máy tính thật qua mạng Wi-Fi / Hotspot / LAN";
            sTmp.fontSize = 15f;
            sTmp.alignment = TextAlignmentOptions.Center;
            sTmp.color = new Color(0.85f, 0.9f, 1f, 0.9f);

            // Cột Trái: TẠO PHÒNG (HOST)
            CreateHostSection(_onlinePanel.transform);

            // Cột Phải: VÀO PHÒNG (GUEST)
            CreateGuestSection(_onlinePanel.transform);

            // Nút Đóng
            var closeGo = new GameObject("Btn_Close");
            closeGo.transform.SetParent(_onlinePanel.transform, false);
            var cRt = closeGo.AddComponent<RectTransform>();
            cRt.anchoredPosition = new Vector2(0f, -225f);
            cRt.sizeDelta = new Vector2(160f, 42f);
            var cImg = closeGo.AddComponent<Image>();
            cImg.color = new Color(0.4f, 0.15f, 0.15f, 1f);
            var cOutline = closeGo.AddComponent<Outline>();
            cOutline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            cOutline.effectDistance = new Vector2(1f, -1f);
            var cBtn = closeGo.AddComponent<Button>();
            cBtn.targetGraphic = cImg;
            cBtn.onClick.AddListener(CloseModals);

            var cTxt = new GameObject("Text");
            cTxt.transform.SetParent(closeGo.transform, false);
            var ctRt = cTxt.AddComponent<RectTransform>();
            ctRt.sizeDelta = cRt.sizeDelta;
            var ctTmp = cTxt.AddComponent<TextMeshProUGUI>();
            ctTmp.text = "ĐÓNG";
            ctTmp.fontSize = 16f;
            ctTmp.fontStyle = FontStyles.Bold;
            ctTmp.alignment = TextAlignmentOptions.Center;
            ctTmp.color = Color.white;

            _onlinePanel.SetActive(false);
        }

        private void CreateHostSection(Transform parent)
        {
            var boxGo = new GameObject("HostBox");
            boxGo.transform.SetParent(parent, false);
            var rt = boxGo.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-200f, -10f);
            rt.sizeDelta = new Vector2(370f, 340f);

            var img = boxGo.AddComponent<Image>();
            img.color = new Color(0.10f, 0.18f, 0.34f, 0.9f);
            var outline = boxGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.85f, 0.72f, 0.30f, 0.8f);

            var tGo = new GameObject("Title");
            tGo.transform.SetParent(boxGo.transform, false);
            var tRt = tGo.AddComponent<RectTransform>();
            tRt.anchoredPosition = new Vector2(0f, 135f);
            tRt.sizeDelta = new Vector2(340f, 30f);
            var tmp = tGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "👑  TẠO PHÒNG (MÁY 1)";
            tmp.fontSize = 18f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.92f, 0.5f, 1f);

            var descGo = new GameObject("Desc");
            descGo.transform.SetParent(boxGo.transform, false);
            var dRt = descGo.AddComponent<RectTransform>();
            dRt.anchoredPosition = new Vector2(0f, 65f);
            dRt.sizeDelta = new Vector2(330f, 90f);
            var dTmp = descGo.AddComponent<TextMeshProUGUI>();
            dTmp.text = "Tạo phòng và mở cổng mạng để máy đối thủ kết nối vào.\n\nSau khi tạo, hệ thống sẽ cấp Mã phòng để đối thủ nhập tham gia.";
            dTmp.fontSize = 14f;
            dTmp.alignment = TextAlignmentOptions.Center;
            dTmp.color = Color.white;

            // Nút Tạo phòng
            var btnGo = new GameObject("Btn_CreateRoom");
            btnGo.transform.SetParent(boxGo.transform, false);
            var bRt = btnGo.AddComponent<RectTransform>();
            bRt.anchoredPosition = new Vector2(0f, -60f);
            bRt.sizeDelta = new Vector2(260f, 55f);
            var bImg = btnGo.AddComponent<Image>();
            bImg.color = new Color(0.96f, 0.82f, 0.38f, 1f);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = bImg;
            btn.onClick.AddListener(OnCreateRoomClicked);

            var btTxt = new GameObject("Text");
            btTxt.transform.SetParent(btnGo.transform, false);
            var btRt = btTxt.AddComponent<RectTransform>();
            btRt.sizeDelta = bRt.sizeDelta;
            var btTmp = btTxt.AddComponent<TextMeshProUGUI>();
            btTmp.text = "TẠO PHÒNG NGAY";
            btTmp.fontSize = 18f;
            btTmp.fontStyle = FontStyles.Bold;
            btTmp.alignment = TextAlignmentOptions.Center;
            btTmp.color = new Color(0.06f, 0.12f, 0.24f, 1f);
        }

        private void CreateGuestSection(Transform parent)
        {
            var boxGo = new GameObject("GuestBox");
            boxGo.transform.SetParent(parent, false);
            var rt = boxGo.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(200f, -10f);
            rt.sizeDelta = new Vector2(370f, 340f);

            var img = boxGo.AddComponent<Image>();
            img.color = new Color(0.10f, 0.18f, 0.34f, 0.9f);
            var outline = boxGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.85f, 0.72f, 0.30f, 0.8f);

            var tGo = new GameObject("Title");
            tGo.transform.SetParent(boxGo.transform, false);
            var tRt = tGo.AddComponent<RectTransform>();
            tRt.anchoredPosition = new Vector2(0f, 135f);
            tRt.sizeDelta = new Vector2(340f, 30f);
            var tmp = tGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "⚔️  VÀO PHÒNG (MÁY 2)";
            tmp.fontSize = 18f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.92f, 0.5f, 1f);

            // Container cho các phòng tìm thấy trong LAN
            var lanGo = new GameObject("LanContainer");
            lanGo.transform.SetParent(boxGo.transform, false);
            var lRt = lanGo.AddComponent<RectTransform>();
            lRt.anchoredPosition = new Vector2(0f, 75f);
            lRt.sizeDelta = new Vector2(340f, 70f);
            _lanRoomsContainer = lanGo.transform;

            // Nhập mã phòng thủ công
            _inputJoinCode = CreateInputField(boxGo.transform, new Vector2(0f, 0f), new Vector2(280f, 45f), "Nhập IP hoặc Mã phòng...");

            // Nút Kết nối
            var btnGo = new GameObject("Btn_JoinRoom");
            btnGo.transform.SetParent(boxGo.transform, false);
            var bRt = btnGo.AddComponent<RectTransform>();
            bRt.anchoredPosition = new Vector2(0f, -65f);
            bRt.sizeDelta = new Vector2(220f, 48f);
            var bImg = btnGo.AddComponent<Image>();
            bImg.color = new Color(0.2f, 0.65f, 1f, 1f);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = bImg;
            btn.onClick.AddListener(OnJoinRoomClicked);

            var btTxt = new GameObject("Text");
            btTxt.transform.SetParent(btnGo.transform, false);
            var btRt = btTxt.AddComponent<RectTransform>();
            btRt.sizeDelta = bRt.sizeDelta;
            var btTmp = btTxt.AddComponent<TextMeshProUGUI>();
            btTmp.text = "THAM GIA";
            btTmp.fontSize = 17f;
            btTmp.fontStyle = FontStyles.Bold;
            btTmp.alignment = TextAlignmentOptions.Center;
            btTmp.color = Color.white;

            // Text Status
            var stGo = new GameObject("ClientStatus");
            stGo.transform.SetParent(boxGo.transform, false);
            var sRt = stGo.AddComponent<RectTransform>();
            sRt.anchoredPosition = new Vector2(0f, -125f);
            sRt.sizeDelta = new Vector2(340f, 35f);
            _txtClientStatus = stGo.AddComponent<TextMeshProUGUI>();
            _txtClientStatus.text = "Nhập mã phòng do Máy 1 cung cấp.";
            _txtClientStatus.fontSize = 13f;
            _txtClientStatus.alignment = TextAlignmentOptions.Center;
            _txtClientStatus.color = new Color(0.85f, 0.9f, 1f, 0.8f);
        }

        private void CreateHostWaitPanel()
        {
            _hostWaitPanel = new GameObject("HostWaitPanel");
            _hostWaitPanel.transform.SetParent(_modalContainer.transform, false);
            var rt = _hostWaitPanel.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(650f, 420f);
            rt.anchoredPosition = Vector2.zero;

            var img = _hostWaitPanel.AddComponent<Image>();
            img.color = new Color(0.06f, 0.12f, 0.24f, 0.98f);

            var outline = _hostWaitPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);

            var tGo = new GameObject("Title");
            tGo.transform.SetParent(_hostWaitPanel.transform, false);
            var tRt = tGo.AddComponent<RectTransform>();
            tRt.anchoredPosition = new Vector2(0f, 155f);
            tRt.sizeDelta = new Vector2(600f, 40f);
            var tmp = tGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "👑  PHÒNG ĐỐI KHÁNG ĐÃ KHỞI TẠO";
            tmp.fontSize = 22f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.9f, 0.4f, 1f);

            var lblGo = new GameObject("Label");
            lblGo.transform.SetParent(_hostWaitPanel.transform, false);
            var lRt = lblGo.AddComponent<RectTransform>();
            lRt.anchoredPosition = new Vector2(0f, 105f);
            lRt.sizeDelta = new Vector2(600f, 30f);
            var lTmp = lblGo.AddComponent<TextMeshProUGUI>();
            lTmp.text = "MÃ PHÒNG (BẢO ĐỐI THỦ NHẬP MÃ NÀY ĐỂ VÀO):";
            lTmp.fontSize = 15f;
            lTmp.alignment = TextAlignmentOptions.Center;
            lTmp.color = Color.white;

            // Box Room Code to rõ
            var codeBoxGo = new GameObject("CodeBox");
            codeBoxGo.transform.SetParent(_hostWaitPanel.transform, false);
            var cbRt = codeBoxGo.AddComponent<RectTransform>();
            cbRt.anchoredPosition = new Vector2(0f, 50f);
            cbRt.sizeDelta = new Vector2(380f, 55f);
            var cbImg = codeBoxGo.AddComponent<Image>();
            cbImg.color = new Color(0.02f, 0.05f, 0.12f, 0.95f);
            var cbOut = codeBoxGo.AddComponent<Outline>();
            cbOut.effectColor = new Color(1f, 0.85f, 0.35f, 1f);

            var cTxtGo = new GameObject("Text");
            cTxtGo.transform.SetParent(codeBoxGo.transform, false);
            var ctRt = cTxtGo.AddComponent<RectTransform>();
            ctRt.sizeDelta = cbRt.sizeDelta;
            _txtHostRoomCode = cTxtGo.AddComponent<TextMeshProUGUI>();
            _txtHostRoomCode.text = "192.168.1.12";
            _txtHostRoomCode.fontSize = 26f;
            _txtHostRoomCode.fontStyle = FontStyles.Bold;
            _txtHostRoomCode.alignment = TextAlignmentOptions.Center;
            _txtHostRoomCode.color = new Color(1f, 0.85f, 0.25f, 1f);

            // Text Status
            var stGo = new GameObject("StatusText");
            stGo.transform.SetParent(_hostWaitPanel.transform, false);
            var sRt = stGo.AddComponent<RectTransform>();
            sRt.anchoredPosition = new Vector2(0f, -15f);
            sRt.sizeDelta = new Vector2(580f, 40f);
            _txtHostStatus = stGo.AddComponent<TextMeshProUGUI>();
            _txtHostStatus.text = "Đang chờ đối thủ tham gia... (0/1)";
            _txtHostStatus.fontSize = 16f;
            _txtHostStatus.alignment = TextAlignmentOptions.Center;
            _txtHostStatus.color = new Color(0.8f, 0.9f, 1f, 1f);

            // Nút Bắt đầu trận
            var startGo = new GameObject("Btn_StartMatch");
            startGo.transform.SetParent(_hostWaitPanel.transform, false);
            var startRt = startGo.AddComponent<RectTransform>();
            startRt.anchoredPosition = new Vector2(0f, -85f);
            startRt.sizeDelta = new Vector2(280f, 50f);
            var sImg = startGo.AddComponent<Image>();
            sImg.color = new Color(0.96f, 0.82f, 0.38f, 1f);
            _btnHostStartMatch = startGo.AddComponent<Button>();
            _btnHostStartMatch.targetGraphic = sImg;
            _btnHostStartMatch.onClick.AddListener(OnHostStartMatchClicked);
            _btnHostStartMatch.interactable = false; // Chỉ bật khi đối thủ vào

            var startTxt = new GameObject("Text");
            startTxt.transform.SetParent(startGo.transform, false);
            var sctRt = startTxt.AddComponent<RectTransform>();
            sctRt.sizeDelta = startRt.sizeDelta;
            var stTmp = startTxt.AddComponent<TextMeshProUGUI>();
            stTmp.text = "⚔️  BẮT ĐẦU TRẬN ĐẤU";
            stTmp.fontSize = 17f;
            stTmp.fontStyle = FontStyles.Bold;
            stTmp.alignment = TextAlignmentOptions.Center;
            stTmp.color = new Color(0.06f, 0.12f, 0.24f, 1f);

            // Nút Hủy phòng
            var cancelGo = new GameObject("Btn_CancelHost");
            cancelGo.transform.SetParent(_hostWaitPanel.transform, false);
            var crt = cancelGo.AddComponent<RectTransform>();
            crt.anchoredPosition = new Vector2(0f, -155f);
            crt.sizeDelta = new Vector2(160f, 40f);
            var cImg = cancelGo.AddComponent<Image>();
            cImg.color = new Color(0.4f, 0.15f, 0.15f, 1f);
            var cBtn = cancelGo.AddComponent<Button>();
            cBtn.targetGraphic = cImg;
            cBtn.onClick.AddListener(CancelHostRoom);

            var cTxt = new GameObject("Text");
            cTxt.transform.SetParent(cancelGo.transform, false);
            var cctRt = cTxt.AddComponent<RectTransform>();
            cctRt.sizeDelta = crt.sizeDelta;
            var cctTmp = cTxt.AddComponent<TextMeshProUGUI>();
            cctTmp.text = "HỦY PHÒNG";
            cctTmp.fontSize = 15f;
            cctTmp.fontStyle = FontStyles.Bold;
            cctTmp.alignment = TextAlignmentOptions.Center;
            cctTmp.color = Color.white;

            _hostWaitPanel.SetActive(false);
        }

        private void CreateMatchmakingPanel()
        {
            _matchmakingPanel = new GameObject("MatchmakingPanel");
            _matchmakingPanel.transform.SetParent(_modalContainer.transform, false);
            var rt = _matchmakingPanel.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(520f, 320f);
            rt.anchoredPosition = Vector2.zero;

            var img = _matchmakingPanel.AddComponent<Image>();
            img.color = new Color(0.06f, 0.12f, 0.24f, 0.98f);

            var outline = _matchmakingPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);

            var tGo = new GameObject("Title");
            tGo.transform.SetParent(_matchmakingPanel.transform, false);
            var tRt = tGo.AddComponent<RectTransform>();
            tRt.anchoredPosition = new Vector2(0f, 90f);
            tRt.sizeDelta = new Vector2(460f, 40f);
            var tmp = tGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "ĐANG TÌM ĐỐI THỦ THẬT...";
            tmp.fontSize = 22f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.9f, 0.4f, 1f);

            var infoGo = new GameObject("InfoText");
            infoGo.transform.SetParent(_matchmakingPanel.transform, false);
            var iRt = infoGo.AddComponent<RectTransform>();
            iRt.anchoredPosition = new Vector2(0f, 25f);
            iRt.sizeDelta = new Vector2(460f, 60f);
            var iTmp = infoGo.AddComponent<TextMeshProUGUI>();
            iTmp.name = "InfoTMP";
            iTmp.text = "Đang quét phòng đối kháng trong mạng LAN / Wi-Fi...\nThời gian tìm: 00:01";
            iTmp.fontSize = 16f;
            iTmp.alignment = TextAlignmentOptions.Center;
            iTmp.color = Color.white;

            var cancelGo = new GameObject("Btn_Cancel");
            cancelGo.transform.SetParent(_matchmakingPanel.transform, false);
            var cRt = cancelGo.AddComponent<RectTransform>();
            cRt.anchoredPosition = new Vector2(0f, -80f);
            cRt.sizeDelta = new Vector2(180f, 44f);
            var cImg = cancelGo.AddComponent<Image>();
            cImg.color = new Color(0.45f, 0.15f, 0.15f, 1f);
            var cBtn = cancelGo.AddComponent<Button>();
            cBtn.targetGraphic = cImg;
            cBtn.onClick.AddListener(CancelMatchmaking);

            var ctTxt = new GameObject("Text");
            ctTxt.transform.SetParent(cancelGo.transform, false);
            var ctRt = ctTxt.AddComponent<RectTransform>();
            ctRt.sizeDelta = cRt.sizeDelta;
            var ctTmp = ctTxt.AddComponent<TextMeshProUGUI>();
            ctTmp.text = "HỦY TÌM TRẬN";
            ctTmp.fontSize = 16f;
            ctTmp.fontStyle = FontStyles.Bold;
            ctTmp.alignment = TextAlignmentOptions.Center;
            ctTmp.color = Color.white;

            _matchmakingPanel.SetActive(false);
        }

        private TMP_InputField CreateInputField(Transform parent, Vector2 pos, Vector2 size, string placeholderText)
        {
            GameObject inputGo = new GameObject("InputField");
            inputGo.transform.SetParent(parent, false);
            var rt = inputGo.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var bgImg = inputGo.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.08f, 0.16f, 0.95f);

            var outline = inputGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.8f, 0.7f, 0.3f, 0.8f);
            outline.effectDistance = new Vector2(1f, -1f);

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(inputGo.transform, false);
            var tRt = textGo.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.sizeDelta = new Vector2(-20f, -10f);
            tRt.anchoredPosition = Vector2.zero;

            var textTmp = textGo.AddComponent<TextMeshProUGUI>();
            textTmp.fontSize = 17f;
            textTmp.color = Color.white;
            textTmp.alignment = TextAlignmentOptions.MidlineLeft;

            GameObject phGo = new GameObject("Placeholder");
            phGo.transform.SetParent(inputGo.transform, false);
            var phRt = phGo.AddComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.sizeDelta = new Vector2(-20f, -10f);
            phRt.anchoredPosition = Vector2.zero;

            var phTmp = phGo.AddComponent<TextMeshProUGUI>();
            phTmp.text = placeholderText;
            phTmp.fontSize = 15f;
            phTmp.fontStyle = FontStyles.Italic;
            phTmp.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
            phTmp.alignment = TextAlignmentOptions.MidlineLeft;

            var inputField = inputGo.AddComponent<TMP_InputField>();
            inputField.textViewport = rt;
            inputField.textComponent = textTmp;
            inputField.placeholder = phTmp;
            inputField.fontAsset = textTmp.font;

            return inputField;
        }

        private void ShowOnlineModal()
        {
            _modalContainer.SetActive(true);
            _onlinePanel.SetActive(true);
            _hostWaitPanel.SetActive(false);
            _matchmakingPanel.SetActive(false);

            if (_inputJoinCode != null)
            {
                _inputJoinCode.text = "";
            }
            if (_txtClientStatus != null)
            {
                _txtClientStatus.text = "Nhập mã phòng do Máy 1 cung cấp.";
                _txtClientStatus.color = new Color(0.85f, 0.9f, 1f, 0.8f);
            }

            // Bắt đầu quét phòng trong LAN
            if (VersusNetworkManager.Instance != null)
            {
                VersusNetworkManager.Instance.StartUdpDiscovery();
            }
        }

        private void OnCreateRoomClicked()
        {
            var cfg = VersusSessionState.ActiveConfig;
            string myName = !string.IsNullOrEmpty(cfg.playerName) ? cfg.playerName : "Bạn";
            string myChar = !string.IsNullOrEmpty(cfg.playerCharacter) ? cfg.playerCharacter : "Minh";

            Debug.Log($"[VersusLobbyController] Đang tạo phòng Host: Tên={myName}, Nhân vật={myChar}...");
            VersusNetworkManager.Instance.StartHostRoom(myName, myChar);
        }

        private void HandleRoomCreated(string code)
        {
            _onlinePanel.SetActive(false);
            _hostWaitPanel.SetActive(true);

            if (_txtHostRoomCode != null) _txtHostRoomCode.text = code;
            if (_txtHostStatus != null)
            {
                _txtHostStatus.text = "Đang chờ đối thủ tham gia... (0/1)";
                _txtHostStatus.color = new Color(0.8f, 0.9f, 1f, 1f);
            }
            if (_btnHostStartMatch != null) _btnHostStartMatch.interactable = false;
        }

        private void HandleOpponentJoined()
        {
            Debug.Log("[VersusLobbyController] Đối thủ đã vào phòng!");
            if (_hostWaitPanel.activeSelf)
            {
                string opName = VersusNetworkManager.Instance.RemotePlayerName;
                string opChar = VersusNetworkManager.Instance.RemotePlayerCharacter;

                if (_txtHostStatus != null)
                {
                    _txtHostStatus.text = $"<color=#55FF55><b>{opName} ({opChar})</b> ĐÃ VÀO PHÒNG!</color>\nCả hai đã sẵn sàng vào đấu trường.";
                }
                if (_btnHostStartMatch != null)
                {
                    _btnHostStartMatch.interactable = true;
                }
            }
            else if (_onlinePanel.activeSelf)
            {
                if (_txtClientStatus != null)
                {
                    _txtClientStatus.text = "<color=#55FF55>ĐÃ KẾT NỐI VÀO PHÒNG!</color>\nĐang chờ Host bắt đầu trận...";
                }
            }
            else if (_matchmakingPanel.activeSelf)
            {
                var infoTmp = _matchmakingPanel.transform.Find("InfoText")?.GetComponent<TextMeshProUGUI>();
                if (infoTmp != null)
                {
                    infoTmp.text = "<color=#55FF55>ĐÃ TÌM THẤY ĐỐI THỦ THẬT!</color>\nĐang tải đấu trường...";
                }
            }
        }

        private void OnHostStartMatchClicked()
        {
            if (VersusNetworkManager.Instance == null || !VersusNetworkManager.Instance.IsConnected) return;

            // Host sinh seed ngẫu nhiên cho 10 câu hỏi
            int seed = UnityEngine.Random.Range(1000, 999999);
            var qList = PhysicsQuestionRepository.GetRandomVersusQuestions(10, seed);
            var qIds = new List<string>();
            foreach (var q in qList) qIds.Add(q.questionID);

            var cfg = VersusSessionState.ActiveConfig;
            cfg.mode = VersusGameMode.OnlineHost;
            cfg.questionSeed = seed;
            cfg.matchQuestionIDs = qIds;
            cfg.opponentName = VersusNetworkManager.Instance.RemotePlayerName;
            cfg.opponentCharacter = VersusNetworkManager.Instance.RemotePlayerCharacter;
            VersusSessionState.ActiveConfig = cfg;

            // Broadcast cho Guest
            VersusNetworkManager.Instance.BroadcastMatchStart(seed, qIds);

            Debug.Log($"[VersusLobbyController] Host bắt đầu trận đấu Online! Seed={seed}");
            EnterArena();
        }

        private void OnJoinRoomClicked()
        {
            string code = _inputJoinCode != null ? _inputJoinCode.text.Trim() : "";
            if (string.IsNullOrEmpty(code))
            {
                if (_txtClientStatus != null)
                {
                    _txtClientStatus.text = "<color=red>Vui lòng nhập IP hoặc Mã phòng!</color>";
                }
                return;
            }

            var cfg = VersusSessionState.ActiveConfig;
            string myName = !string.IsNullOrEmpty(cfg.playerName) ? cfg.playerName : "Bạn";
            string myChar = !string.IsNullOrEmpty(cfg.playerCharacter) ? cfg.playerCharacter : "Minh";

            if (_txtClientStatus != null)
            {
                _txtClientStatus.text = $"Đang kết nối tới <b>{code}</b>...";
                _txtClientStatus.color = Color.yellow;
            }

            VersusNetworkManager.Instance.JoinRoomByCode(code, myName, myChar);
        }

        private void HandleJoinFailed(string reason)
        {
            if (_txtClientStatus != null)
            {
                _txtClientStatus.text = $"<color=red>Lỗi kết nối: {reason}</color>";
            }
        }

        private void HandleOpponentDisconnected()
        {
            if (_hostWaitPanel.activeSelf)
            {
                if (_txtHostStatus != null)
                {
                    _txtHostStatus.text = "<color=red>Đối thủ đã ngắt kết nối!</color>\nĐang chờ người chơi khác...";
                }
                if (_btnHostStartMatch != null) _btnHostStartMatch.interactable = false;
            }
        }

        private void HandleLanRoomsUpdated(List<DiscoveredRoom> rooms)
        {
            if (_lanRoomsContainer == null) return;

            foreach (var go in _spawnedRoomRows)
            {
                if (go != null) Destroy(go);
            }
            _spawnedRoomRows.Clear();

            if (rooms == null || rooms.Count == 0) return;

            // Hiển thị tối đa 2 phòng tìm thấy trong Wi-Fi
            for (int i = 0; i < Mathf.Min(rooms.Count, 2); i++)
            {
                var rm = rooms[i];
                var rowGo = new GameObject("LanRoomRow_" + i);
                rowGo.transform.SetParent(_lanRoomsContainer, false);
                var rt = rowGo.AddComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0f, 15f - i * 36f);
                rt.sizeDelta = new Vector2(330f, 32f);

                var img = rowGo.AddComponent<Image>();
                img.color = new Color(0.15f, 0.35f, 0.20f, 0.95f); // Xanh lá cây thông báo phòng sẵn sàng
                var btn = rowGo.AddComponent<Button>();
                btn.targetGraphic = img;

                string targetIp = rm.ipAddress;
                btn.onClick.AddListener(() =>
                {
                    if (_inputJoinCode != null) _inputJoinCode.text = targetIp;
                    OnJoinRoomClicked();
                });

                var txtGo = new GameObject("Text");
                txtGo.transform.SetParent(rowGo.transform, false);
                var tRt = txtGo.AddComponent<RectTransform>();
                tRt.sizeDelta = rt.sizeDelta;
                var tmp = txtGo.AddComponent<TextMeshProUGUI>();
                tmp.text = $"⚡ Vào ngay: {rm.hostName} ({rm.roomCode})";
                tmp.fontSize = 14f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;

                _spawnedRoomRows.Add(rowGo);
            }
        }

        private void CancelHostRoom()
        {
            if (VersusNetworkManager.Instance != null)
            {
                VersusNetworkManager.Instance.StopNetwork();
            }
            ShowOnlineModal();
        }

        // ==========================================
        // GHÉP NGẪU NHIÊN NGƯỜI CHƠI THẬT
        // ==========================================

        private void StartTrueRandomMatchmaking()
        {
            _modalContainer.SetActive(true);
            _onlinePanel.SetActive(false);
            _hostWaitPanel.SetActive(false);
            _matchmakingPanel.SetActive(true);

            if (_matchmakingCoroutine != null) StopCoroutine(_matchmakingCoroutine);
            _matchmakingCoroutine = StartCoroutine(TrueRandomMatchmakingRoutine());
        }

        private IEnumerator TrueRandomMatchmakingRoutine()
        {
            var infoTmp = _matchmakingPanel.transform.Find("InfoText")?.GetComponent<TextMeshProUGUI>();
            var titleTmp = _matchmakingPanel.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (titleTmp != null) titleTmp.text = "GHÉP ĐỐI THỦ THẬT ONLINE";

            if (VersusNetworkManager.Instance != null)
            {
                VersusNetworkManager.Instance.StartUdpDiscovery();
            }

            int timer = 0;
            DiscoveredRoom foundRoom = null;

            // Quét trong 3 giây xem có phòng Host nào đang chờ trong mạng không
            while (timer < 3)
            {
                timer++;
                if (infoTmp != null)
                {
                    infoTmp.text = $"Đang quét người chơi thật trong mạng Wi-Fi/LAN...\nThời gian: 00:{timer:00}";
                }
                yield return new WaitForSeconds(1f);

                // Kiểm tra xem có phòng nào không
                if (VersusNetworkManager.Instance != null)
                {
                    // Nếu đã tìm thấy phòng LAN
                    if (_spawnedRoomRows.Count > 0)
                    {
                        break;
                    }
                }
            }

            // Nếu tìm thấy phòng: Tham gia làm Guest
            if (foundRoom != null || _spawnedRoomRows.Count > 0)
            {
                if (infoTmp != null)
                {
                    infoTmp.text = "<color=#55FF55>ĐÃ TÌM THẤY PHÒNG ĐỐI THỦ!</color>\nĐang tham gia...";
                }
                yield return new WaitForSeconds(1f);
                // Tự động bấm vào phòng đầu tiên
                if (_spawnedRoomRows.Count > 0 && _spawnedRoomRows[0] != null)
                {
                    _spawnedRoomRows[0].GetComponent<Button>()?.onClick.Invoke();
                }
            }
            else
            {
                // Chưa có ai tạo phòng: Tự động trở thành Host chờ người khác vào!
                if (infoTmp != null)
                {
                    infoTmp.text = "<color=yellow>Chưa có ai mở phòng. Đang tạo phòng Host chờ đối thủ...</color>";
                }
                yield return new WaitForSeconds(1f);
                OnCreateRoomClicked();
            }
        }

        private void CancelMatchmaking()
        {
            if (_matchmakingCoroutine != null) StopCoroutine(_matchmakingCoroutine);
            if (VersusNetworkManager.Instance != null)
            {
                VersusNetworkManager.Instance.StopNetwork();
            }
            CloseModals();
        }

        private void CloseModals()
        {
            if (_matchmakingCoroutine != null) StopCoroutine(_matchmakingCoroutine);
            if (VersusNetworkManager.Instance != null)
            {
                VersusNetworkManager.Instance.StopNetwork();
            }
            if (_onlinePanel != null) _onlinePanel.SetActive(false);
            if (_hostWaitPanel != null) _hostWaitPanel.SetActive(false);
            if (_matchmakingPanel != null) _matchmakingPanel.SetActive(false);
            if (_modalContainer != null) _modalContainer.SetActive(false);
        }
    }
}
