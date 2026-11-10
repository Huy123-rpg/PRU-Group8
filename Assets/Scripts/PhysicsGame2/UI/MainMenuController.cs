using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển toàn bộ logic giao diện Scene 01_MainMenu theo đặc tả 01_Luong_34_Man_Aurelia.md.
    /// Quản lý 7 nút chính: Bắt đầu, Chọn nhân vật, Thành tích, Bảng xếp hạng, Tiến bộ, Cài đặt, Quay lại.
    /// Đảm bảo tính toán raycast chính xác, loại bỏ các blocker che khuất, loại bỏ thanh phím tắt phụ thừa.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("=== 7 NÚT CHÍNH CỦA AURELIA ACADEMY MAIN MENU ===")]
        [SerializeField] private Button btnStart;              // 1. Bắt đầu (Y ~ 200)
        [SerializeField] private Button btnAchievements;       // 2. Thành tích (Y ~ 75)
        [SerializeField] private Button btnSettings;           // 3. Cài đặt (Y ~ -50)
        [SerializeField] private Button btnLeaderboard;        // 4. Bảng xếp hạng (Y ~ -175)
        [SerializeField] private Button btnProgress;           // 5. Tiến bộ học tập (Y ~ -300)
        [SerializeField] private Button btnVersus;             // 6. Đối kháng 1vs1 (Image (6) @ Y ~ -330)
        [SerializeField] private Button btnExit;               // 7. Quay lại (Y ~ -425)

        [Header("=== LEGACY / COMPATIBILITY ===")]
        [SerializeField] private Button btnContinue;

        [Header("=== FEEDBACK NOTIFICATION ===")]
        [SerializeField] private TextMeshProUGUI noticeText;

        private Coroutine _noticeCoroutine;
        private bool _isInitialized = false;

        private void Awake()
        {
            InitializeMainMenu();
        }

        private void Start()
        {
            if (!_isInitialized)
            {
                InitializeMainMenu();
            }
            else
            {
                // Đảm bảo trạng thái raycast và event vẫn an toàn sau khi hierarchy ổn định
                Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    EnsureRaycastSafety(canvas);
                    EnsureEventSystemSafety();
                }
            }
        }

        /// <summary>
        /// Khởi tạo toàn diện toàn bộ giao diện Main Menu: dọn dẹp UI thừa, giải phóng blocker raycast,
        /// gán button, đè nhãn 'CHỌN NHÂN VẬT' và kết nối sự kiện điều hướng.
        /// </summary>
        public void InitializeMainMenu()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[MainMenuController] Không tìm thấy Canvas trong scene 01_MainMenu!");
                return;
            }

            // 1. Xóa ngay lập tức mọi UI nhỏ/thừa được tạo runtime không có trong thiết kế gốc
            CleanupUnwantedAuxiliaryUI(canvas);

            // 2. Bảo đảm EventSystem và InputModule hoạt động chuẩn xác, nhận click chuột/phím
            EnsureEventSystemSafety();

            // 3. Khóa raycastTarget trên tất cả decorative panels, banners, background strips
            EnsureRaycastSafety(canvas);

            // 4. Nhận diện và gán đúng 6 nút chính theo thiết kế ban đầu
            BindSixMainMenuButtons(canvas);

            // 5. Kết nối các sự kiện OnClick chuẩn
            BindButtonEvents();

            _isInitialized = true;
            Debug.Log("[MainMenuController] Khởi tạo MainMenuController (6 nút) thành công 100%.");
        }

        /// <summary>
        /// Xóa bỏ hoàn toàn thanh phím tắt phụ (AuxiliaryPillBar), nút bí mật (Btn_AcademySecretEmblem)
        /// và bất kỳ nút Chọn nhân vật hoặc Cover_Pill nào để đảm bảo đúng thiết kế ban đầu 6 nút.
        /// </summary>
        private void CleanupUnwantedAuxiliaryUI(Canvas canvas)
        {
            // Xóa AuxiliaryPillBar nếu đã từng được tạo
            Transform auxBar = canvas.transform.Find("AuxiliaryPillBar");
            if (auxBar != null)
            {
                DestroyImmediate(auxBar.gameObject);
                Debug.Log("[MainMenuController] Đã xóa thanh nút nhỏ AuxiliaryPillBar.");
            }

            // Xóa ô vuông bí mật Btn_AcademySecretEmblem nếu đã từng được tạo
            Transform emblem = canvas.transform.Find("Btn_AcademySecretEmblem");
            if (emblem != null)
            {
                DestroyImmediate(emblem.gameObject);
                Debug.Log("[MainMenuController] Đã xóa ô vuông bí mật Btn_AcademySecretEmblem.");
            }

            // Xóa nút Chọn nhân vật riêng khỏi Main Menu theo đúng thiết kế ban đầu
            Transform charSelect = canvas.transform.Find("Btn_CharacterSelection");
            if (charSelect != null)
            {
                DestroyImmediate(charSelect.gameObject);
                Debug.Log("[MainMenuController] Đã loại bỏ nút Btn_CharacterSelection thừa trên Main Menu.");
            }

            // Xóa bất kỳ Cover_Pill nào còn sót lại
            Transform coverPill = canvas.transform.Find("Cover_Pill");
            if (coverPill != null)
            {
                DestroyImmediate(coverPill.gameObject);
            }
        }

        /// <summary>
        /// Bảo đảm EventSystem có InputModule hợp lệ. Nếu InputSystemUIInputModule bị thiếu ActionsAsset,
        /// tự động gọi AssignDefaultActions() để chuột và click trong Game View hoạt động bình thường.
        /// </summary>
        private void EnsureEventSystemSafety()
        {
            EventSystem es = Object.FindAnyObjectByType<EventSystem>();
            if (es == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                es = esGo.AddComponent<EventSystem>();
                Debug.Log("[MainMenuController] Đã tạo EventSystem mới cho Scene.");
            }

            if (!es.enabled)
            {
                es.enabled = true;
            }

            InputSystemUIInputModule inputSystemModule = es.GetComponent<InputSystemUIInputModule>();
            if (inputSystemModule != null)
            {
                if (inputSystemModule.actionsAsset == null)
                {
                    inputSystemModule.AssignDefaultActions();
                    Debug.Log("[MainMenuController] Đã tự động gán DefaultActions cho InputSystemUIInputModule.");
                }
                inputSystemModule.enabled = true;
            }
            else
            {
                StandaloneInputModule standalone = es.GetComponent<StandaloneInputModule>();
                if (standalone == null)
                {
                    standalone = es.gameObject.AddComponent<StandaloneInputModule>();
                    Debug.Log("[MainMenuController] Đã bổ sung StandaloneInputModule cho EventSystem.");
                }
                standalone.enabled = true;
            }
        }

        /// <summary>
        /// Tắt raycastTarget trên tất cả graphic nền, khung viền, dải trang trí để click truyền thẳng vào 7 nút.
        /// </summary>
        private void EnsureRaycastSafety(Canvas canvas)
        {
            Image[] allImages = canvas.GetComponentsInChildren<Image>(true);
            foreach (var img in allImages)
            {
                string gName = img.gameObject.name;
                float w = img.rectTransform.sizeDelta.x;
                float h = img.rectTransform.sizeDelta.y;

                // 0. Ngoại lệ cho nút Đối kháng do người dùng tự thiết kế: Image (6) và Text_Pvp
                if (gName == "Image (6)" || gName == "Btn_Versus")
                {
                    img.raycastTarget = true;
                    continue;
                }
                if (gName == "Text_Pvp")
                {
                    // Tắt raycastTarget trên text để click truyền thẳng xuống nút cha Image (6)
                    img.raycastTarget = false;
                    continue;
                }

                // 1. Background toàn màn hình
                if (gName == "Background" || gName == "Canvas")
                {
                    img.raycastTarget = false;
                    continue;
                }

                // 2. Banner tiêu đề (Y ~ 409)
                if (img.rectTransform.anchoredPosition.y > 350f)
                {
                    img.raycastTarget = false;
                    continue;
                }

                // 3. Khung lớn trung tâm Image (1) (W ~ 575, H ~ 485)
                if (w > 350f && h > 100f)
                {
                    img.raycastTarget = false;

                    // Nếu đối tượng khung này vô tình bị gắn Button từ trước -> gỡ bỏ
                    Button unwantedBtn = img.GetComponent<Button>();
                    if (unwantedBtn != null)
                    {
                        Destroy(unwantedBtn);
                    }
                    continue;
                }

                // 4. Gỡ bỏ script feedback làm nuốt sự kiện pointer nếu có
                UIButtonFeedback fb = img.GetComponent<UIButtonFeedback>();
                if (fb != null)
                {
                    Destroy(fb);
                }
            }

            // Tắt raycastTarget trên tất cả TextMeshPro trang trí
            foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                tmp.raycastTarget = false;
            }
        }

        /// <summary>
        /// Gán 7 nút chính Aurelia Academy theo đúng thiết kế và sprite trực quan:
        /// 1. Bắt đầu (Btn_Start, Y ~ 231) -> CharacterSelection
        /// 2. Cài đặt (Btn_Settings, Y ~ 116, sprite fantasy_4 "Cài đặt") -> 14_Settings
        /// 3. Tiến độ (Btn_Progress, Y ~ 0, sprite Atlas_173 "Tiến độ") -> LearningProgress
        /// 4. Xếp hạng (Btn_Leaderboard, Y ~ -109, sprite Atlas_90 "Xếp hạng") -> WeeklyRank
        /// 5. Thành tích (Btn_Achievements, Y ~ -226, sprite Atlas_149 "Thành tích") -> Achievements
        /// 6. Đối kháng (Image (6) / Btn_Versus, Y ~ -330) -> Versus
        /// 7. Quay lại (Btn_Exit, Y ~ -443, sprite fantasy_5 "Quay lại") -> HostMenu / ExitModal
        /// </summary>
        private void BindSixMainMenuButtons(Canvas canvas)
        {
            // Nhận diện tự động theo Sprite thực tế để bảo đảm hiển thị chữ gì trên màn hình thì click trúng chức năng đó 100%
            Button[] allButtons = canvas.GetComponentsInChildren<Button>(true);
            foreach (var b in allButtons)
            {
                Image img = b.GetComponent<Image>();
                if (img != null && img.sprite != null)
                {
                    string sName = img.sprite.name;
                    // fantasy_4 = "Cài đặt" (fileID: 1172909369)
                    if (sName.Contains("fantasy_4") || sName.Contains("Cài đặt") || sName.Contains("Cai dat"))
                    {
                        btnSettings = b;
                    }
                    // fantasy_2 = "Bắt đầu" (fileID: 1473378593)
                    else if (sName.Contains("fantasy_2") || sName.Contains("Bắt đầu") || sName.Contains("Bat dau"))
                    {
                        btnStart = b;
                    }
                    // fantasy_5 = "Quay lại" (fileID: -1453150625)
                    else if (sName.Contains("fantasy_5") || sName.Contains("Quay lại") || sName.Contains("Quay lai"))
                    {
                        btnExit = b;
                    }
                    // Atlas nút menu xanh vàng_90 = "Xếp hạng" (fileID: 5250840933021119155)
                    else if (sName.Contains("_90") || sName.Contains("Xếp hạng") || sName.Contains("Xep hang"))
                    {
                        btnLeaderboard = b;
                    }
                    // Atlas nút menu xanh vàng_173 = "Tiến độ" (fileID: 6311273609557242485)
                    else if (sName.Contains("_173") || sName.Contains("Tiến độ") || sName.Contains("Tien do"))
                    {
                        btnProgress = b;
                    }
                    // Atlas nút menu xanh vàng_149 = "Thành tích" (fileID: 5718479360214963104) HOẶC fantasy_3
                    else if (sName.Contains("_149") || sName.Contains("fantasy_3") || sName.Contains("Thành tích") || sName.Contains("Thanh tich"))
                    {
                        btnAchievements = b;
                    }
                }

                if (b.gameObject.name == "Image (6)" || b.gameObject.name == "Btn_Versus" || b.transform.Find("Text_Pvp") != null)
                {
                    btnVersus = b;
                }
            }

            // Fallback theo tên GameObject nếu chưa tìm thấy bằng sprite
            btnStart = btnStart ?? FindButtonByName(canvas, "Btn_Start");
            btnSettings = btnSettings ?? FindButtonByName(canvas, "Btn_Settings");
            btnProgress = btnProgress ?? FindButtonByName(canvas, "Btn_Progress");
            btnLeaderboard = btnLeaderboard ?? FindButtonByName(canvas, "Btn_Leaderboard");
            btnAchievements = btnAchievements ?? FindButtonByName(canvas, "Btn_Achievements");
            btnVersus = btnVersus ?? FindButtonByName(canvas, "Image (6)") ?? FindButtonByName(canvas, "Btn_Versus");
            btnExit = btnExit ?? FindButtonByName(canvas, "Btn_Exit");

            ConfigureButtonSafety(btnStart, "Btn_Start");
            ConfigureButtonSafety(btnSettings, "Btn_Settings");
            ConfigureButtonSafety(btnProgress, "Btn_Progress");
            ConfigureButtonSafety(btnLeaderboard, "Btn_Leaderboard");
            ConfigureButtonSafety(btnAchievements, "Btn_Achievements");
            ConfigureButtonSafety(btnVersus, "Image (6)");
            ConfigureButtonSafety(btnExit, "Btn_Exit");
        }

        private Button FindButtonByName(Canvas canvas, string buttonName)
        {
            Transform t = canvas.transform.Find(buttonName);
            if (t != null)
            {
                Button btn = t.GetComponent<Button>();
                if (btn == null) btn = t.gameObject.AddComponent<Button>();
                return btn;
            }
            return null;
        }

        private void ConfigureButtonSafety(Button btn, string standardName)
        {
            if (btn == null)
            {
                Debug.LogWarning($"[MainMenuController] Thiếu nút: {standardName}!");
                return;
            }

            btn.gameObject.name = standardName;
            btn.interactable = true;

            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = true;
                btn.targetGraphic = img;
            }

            btn.transition = Selectable.Transition.ColorTint;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.05f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
            btn.colors = colors;

            UIButtonFeedback fb = btn.GetComponent<UIButtonFeedback>();
            if (fb != null) Destroy(fb);

            UIHoverPulseEffect.Attach(btn.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
        }

        /// <summary>
        /// Gắn các hàm callback tương ứng cho 6 nút chính.
        /// </summary>
        private void BindButtonEvents()
        {
            try
            {
                if (btnStart != null)
                {
                    btnStart.onClick.RemoveAllListeners();
                    btnStart.onClick.AddListener(OnStartClicked);
                    Debug.Log("[MainMenuController] Đã gán onClick cho Btn_Start.");
                }
                else Debug.LogWarning("[MainMenuController] Thiếu tham chiếu Btn_Start!");

                if (btnAchievements != null)
                {
                    btnAchievements.onClick.RemoveAllListeners();
                    btnAchievements.onClick.AddListener(OnAchievementsClicked);
                    Debug.Log("[MainMenuController] Đã gán onClick cho Btn_Achievements.");
                }
                else Debug.LogWarning("[MainMenuController] Thiếu tham chiếu Btn_Achievements!");

                if (btnSettings != null)
                {
                    btnSettings.onClick.RemoveAllListeners();
                    btnSettings.onClick.AddListener(OnSettingsClicked);
                    Debug.Log("[MainMenuController] Đã gán onClick cho Btn_Settings.");
                }
                else Debug.LogWarning("[MainMenuController] Thiếu tham chiếu Btn_Settings!");

                if (btnLeaderboard != null)
                {
                    btnLeaderboard.onClick.RemoveAllListeners();
                    btnLeaderboard.onClick.AddListener(OnLeaderboardClicked);
                    Debug.Log("[MainMenuController] Đã gán onClick cho Btn_Leaderboard.");
                }
                else Debug.LogWarning("[MainMenuController] Thiếu tham chiếu Btn_Leaderboard!");

                if (btnProgress != null)
                {
                    btnProgress.onClick.RemoveAllListeners();
                    btnProgress.onClick.AddListener(OnProgressClicked);
                    Debug.Log("[MainMenuController] Đã gán onClick cho Btn_Progress.");
                }
                else Debug.LogWarning("[MainMenuController] Thiếu tham chiếu Btn_Progress!");

                if (btnVersus != null)
                {
                    btnVersus.onClick.RemoveAllListeners();
                    btnVersus.onClick.AddListener(OnVersusClicked);
                    Debug.Log("[MainMenuController] Đã gán onClick cho Btn_Versus (Image (6)).");
                }
                else Debug.LogWarning("[MainMenuController] Thiếu tham chiếu Btn_Versus!");

                if (btnExit != null)
                {
                    btnExit.onClick.RemoveAllListeners();
                    btnExit.onClick.AddListener(OnExitClicked);
                    Debug.Log("[MainMenuController] Đã gán onClick cho Btn_Exit.");
                }
                else Debug.LogWarning("[MainMenuController] Thiếu tham chiếu Btn_Exit!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MainMenuController] Ngoại lệ BindButtonEvents: {ex.Message}");
            }
        }

        // =====================================================================
        // CÁC HÀM ĐIỀU HƯỚNG CHUẨN THEO 01_Luong_34_Man_Aurelia.md (Bảng 6.1)
        // Kèm log 6 bước giám sát theo yêu cầu
        // =====================================================================

        /// <summary>
        /// Nút 1: Bắt đầu.
        /// Luôn chuyển sang Scene 02_CharacterSelection (Chọn nhân vật),
        /// kể cả khi người chơi đã từng lưu nhân vật trước đó.
        /// </summary>
        public void OnStartClicked()
        {
            Debug.Log("[MainMenu Flow] [1. CLICK RECEIVED] Nút 'Bắt đầu' được bấm.");
            Debug.Log("[MainMenu Flow] [2. CALLBACK EXECUTED] Bắt đầu xử lý điều hướng cho 'Bắt đầu'.");

            // Reset toàn bộ cờ đối kháng để đảm bảo luồng học tập chính không bao giờ bị nhảy nhầm sang Đối kháng
            if (NavigationService.Instance != null)
            {
                if (NavigationService.Instance.Selection != null)
                {
                    NavigationService.Instance.Selection.Mode = "STUDY";
                }
                if (NavigationService.Instance.GameContext != null)
                {
                    NavigationService.Instance.GameContext.ReturnAction = "StartStudy";
                }
            }
            VersusSessionState.ClearSession();

            PhysicsSaveData save = PhysicsGameSession.Instance != null
                ? PhysicsGameSession.Instance.SaveData
                : PhysicsSaveManager.Load();

            string charId = (save != null && !string.IsNullOrEmpty(save.selectedCharacterId))
                ? save.selectedCharacterId
                : "(chưa chọn)";
            Debug.Log($"[MainMenu Flow] [3. CONDITION CHECKED] CharacterId='{charId}'. Luôn mở Chọn nhân vật trước khi vào buổi học.");

            Debug.Log($"[MainMenu Flow] [4. ROUTE IDENTIFIED] RouteId={RouteId.CharacterSelection}, TargetScene='{PhysicsSceneNames.CharacterSelection}'.");
            NavigateToRoute(RouteId.CharacterSelection, PhysicsSceneNames.CharacterSelection);
        }

        /// <summary>
        /// Nút 2: Thành tích.
        /// Chuyển sang màn Thành tích (15.1_Achievements (màn 2)).
        /// </summary>
        public void OnAchievementsClicked()
        {
            Debug.Log("[MainMenu Flow] [1. CLICK RECEIVED] Nút 'Thành tích' được bấm.");
            Debug.Log("[MainMenu Flow] [2. CALLBACK EXECUTED] Bắt đầu xử lý điều hướng cho 'Thành tích'.");
            Debug.Log("[MainMenu Flow] [3. CONDITION CHECKED] Mở màn Thành tích (15.1_Achievements (màn 2)).");
            Debug.Log($"[MainMenu Flow] [4. ROUTE IDENTIFIED] RouteId={RouteId.Achievements}, TargetScene='{PhysicsSceneNames.Achievements}'.");
            NavigateToRoute(RouteId.Achievements, PhysicsSceneNames.Achievements);
        }

        /// <summary>
        /// Nút 3: Cài đặt.
        /// Chuyển sang màn Cài đặt (14_Settings).
        /// </summary>
        public void OnSettingsClicked()
        {
            Debug.Log("[MainMenu Flow] [1. CLICK RECEIVED] Nút 'Cài đặt' được bấm.");
            Debug.Log("[MainMenu Flow] [2. CALLBACK EXECUTED] Bắt đầu xử lý điều hướng cho 'Cài đặt'.");
            Debug.Log("[MainMenu Flow] [3. CONDITION CHECKED] Mở màn Cài đặt (14_Settings).");
            Debug.Log($"[MainMenu Flow] [4. ROUTE IDENTIFIED] RouteId={RouteId.Settings}, TargetScene='{PhysicsSceneNames.Settings}'.");
            if (SettingsManager.Instance != null) SettingsManager.Instance.PlaySFX();
            NavigateToRoute(RouteId.Settings, PhysicsSceneNames.Settings);
        }

        /// <summary>
        /// Nút 4: Bảng xếp hạng.
        /// Chuyển sang màn Bảng xếp hạng tuần (23. Bảng xếp hạng).
        /// </summary>
        public void OnLeaderboardClicked()
        {
            Debug.Log("[MainMenu Flow] [1. CLICK RECEIVED] Nút 'Bảng xếp hạng' được bấm.");
            Debug.Log("[MainMenu Flow] [2. CALLBACK EXECUTED] Bắt đầu xử lý điều hướng cho 'Bảng xếp hạng'.");
            Debug.Log("[MainMenu Flow] [3. CONDITION CHECKED] Mở màn Bảng xếp hạng tuần (23. Bảng xếp hạng).");
            Debug.Log($"[MainMenu Flow] [4. ROUTE IDENTIFIED] RouteId={RouteId.WeeklyLeaderboard}, TargetScene='{PhysicsSceneNames.WeeklyRank}'.");
            NavigateToRoute(RouteId.WeeklyLeaderboard, PhysicsSceneNames.WeeklyRank);
        }

        /// <summary>
        /// Nút 3: Tiến độ học tập.
        /// Chuyển sang màn Tổng quan tiến độ học tập (24.1. Tổng quan tiến độ).
        /// </summary>
        public void OnProgressClicked()
        {
            Debug.Log("[MainMenu Flow] [1. CLICK RECEIVED] Nút 'Tiến độ' được bấm.");
            Debug.Log("[MainMenu Flow] [2. CALLBACK EXECUTED] Bắt đầu xử lý điều hướng cho 'Tiến độ'.");
            Debug.Log("[MainMenu Flow] [3. CONDITION CHECKED] Mở màn Tổng quan tiến độ học tập.");
            Debug.Log($"[MainMenu Flow] [4. ROUTE IDENTIFIED] RouteId={RouteId.ProgressOverview}, TargetScene='{PhysicsSceneNames.ProgressOverview}'.");
            NavigateToRoute(RouteId.ProgressOverview, PhysicsSceneNames.ProgressOverview);
        }

        /// <summary>
        /// Nút 6: Đối kháng 1vs1.
        /// - Nếu chưa chọn nhân vật: chuyển sang Chọn nhân vật (02_CharacterSelection).
        /// - Nếu đã có nhân vật: chuyển thẳng sang Sảnh đối kháng (34. Đối kháng).
        /// </summary>
        public void OnVersusClicked()
        {
            Debug.Log("[MainMenu Flow] [1. CLICK RECEIVED] Nút 'Đối kháng' (Image (6)) được bấm.");
            Debug.Log("[MainMenu Flow] [2. CALLBACK EXECUTED] Bắt đầu xử lý điều hướng cho 'Đối kháng'.");

            if (NavigationService.Instance != null && NavigationService.Instance.Selection != null)
            {
                NavigationService.Instance.Selection.Mode = "VERSUS";
            }
            if (NavigationService.Instance != null && NavigationService.Instance.GameContext != null)
            {
                NavigationService.Instance.GameContext.ReturnAction = "Versus";
            }

            PhysicsSaveData save = PhysicsGameSession.Instance != null
                ? PhysicsGameSession.Instance.SaveData
                : PhysicsSaveManager.Load();

            bool hasCharacter = (save != null && !string.IsNullOrEmpty(save.selectedCharacterId))
                || (PhysicsGameSession.Instance != null && !string.IsNullOrEmpty(PhysicsGameSession.Instance.SelectedCharacterId));

            if (!hasCharacter)
            {
                Debug.Log("[MainMenu Flow] [3. CONDITION CHECKED] Chưa chọn nhân vật. Mở màn Chọn nhân vật trước khi vào Đối kháng.");
                Debug.Log($"[MainMenu Flow] [4. ROUTE IDENTIFIED] RouteId={RouteId.CharacterSelection}, TargetScene='{PhysicsSceneNames.CharacterSelection}'.");
                NavigateToRoute(RouteId.CharacterSelection, PhysicsSceneNames.CharacterSelection);
            }
            else
            {
                Debug.Log("[MainMenu Flow] [3. CONDITION CHECKED] Đã có nhân vật. Chuyển thẳng sang sảnh Đối kháng (34. Đối kháng).");
                Debug.Log($"[MainMenu Flow] [4. ROUTE IDENTIFIED] RouteId={RouteId.VersusLobby}, TargetScene='{PhysicsSceneNames.VersusLobby}'.");
                NavigateToRoute(RouteId.VersusLobby, PhysicsSceneNames.VersusLobby);
            }
        }

        /// <summary>
        /// Nút 7: Quay lại.
        /// - Nếu project có Host Menu (Scene2_MainMenu): quay về Host Menu.
        /// - Ngược lại: hiển thị modal xác nhận đóng game theo đặc tả mục 6.1.
        /// </summary>
        public void OnExitClicked()
        {
            Debug.Log("[MainMenu Flow] [1. CLICK RECEIVED] Nút 'Quay lại' được bấm.");
            Debug.Log("[MainMenu Flow] [2. CALLBACK EXECUTED] Bắt đầu xử lý điều hướng cho 'Quay lại'.");

            if (Application.CanStreamedLevelBeLoaded("Scene2_MainMenu"))
            {
                Debug.Log("[MainMenu Flow] [3. CONDITION CHECKED] Scene2_MainMenu có sẵn trong Build Settings.");
                Debug.Log($"[MainMenu Flow] [4. ROUTE IDENTIFIED] RouteId={RouteId.HostMenu}, TargetScene='Scene2_MainMenu'.");
                NavigateToRoute(RouteId.HostMenu, "Scene2_MainMenu");
            }
            else
            {
                Debug.Log("[MainMenu Flow] [3. CONDITION CHECKED] Scene2_MainMenu không có trong build -> Hiển thị Modal xác nhận thoát.");
                ShowExitConfirmationModal();
            }
        }

        private void NavigateToRoute(RouteId route, string fallbackSceneName)
        {
            string targetScene = !string.IsNullOrEmpty(fallbackSceneName) ? fallbackSceneName : SceneCatalog.GetSceneName(route);
            Debug.Log($"[MainMenu Flow] [5. LOADING SCENE] Bắt đầu tải scene '{targetScene}' (Route: {route})...");

            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.ResetTransitionLock();
                NavigationService.Instance.NavigateTo(route);
            }
            else if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.LoadScene(targetScene);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
            }
        }

        private void ShowExitConfirmationModal()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Transform existing = canvas.transform.Find("ExitConfirmationModal");
            if (existing != null) return;

            GameObject modalGo = new GameObject("ExitConfirmationModal");
            modalGo.transform.SetParent(canvas.transform, false);

            RectTransform rt = modalGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image bg = modalGo.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.08f, 0.16f, 0.88f);

            GameObject cardGo = new GameObject("Card");
            cardGo.transform.SetParent(modalGo.transform, false);
            RectTransform cRt = cardGo.AddComponent<RectTransform>();
            cRt.sizeDelta = new Vector2(500, 260);
            cRt.anchoredPosition = Vector2.zero;

            Image cImg = cardGo.AddComponent<Image>();
            cImg.color = new Color(0.08f, 0.14f, 0.28f, 0.98f);

            Outline outline = cardGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            outline.effectDistance = new Vector2(2, -2);

            // Message
            GameObject msgGo = new GameObject("Message");
            msgGo.transform.SetParent(cardGo.transform, false);
            RectTransform mRt = msgGo.AddComponent<RectTransform>();
            mRt.anchoredPosition = new Vector2(0, 40);
            mRt.sizeDelta = new Vector2(440, 100);
            var msgTmp = msgGo.AddComponent<TextMeshProUGUI>();
            msgTmp.text = "Bạn đang ở Menu chính.\nBạn có muốn đóng trò chơi không?";
            msgTmp.fontSize = 20;
            msgTmp.alignment = TextAlignmentOptions.Center;
            msgTmp.color = Color.white;
            msgTmp.raycastTarget = false;

            // Buttons: Thoát & Hủy
            GameObject btnQuitGo = new GameObject("Btn_QuitConfirm");
            btnQuitGo.transform.SetParent(cardGo.transform, false);
            RectTransform qRt = btnQuitGo.AddComponent<RectTransform>();
            qRt.anchoredPosition = new Vector2(-110, -60);
            qRt.sizeDelta = new Vector2(160, 44);
            Image qImg = btnQuitGo.AddComponent<Image>();
            qImg.color = new Color(0.85f, 0.25f, 0.25f, 1f);
            Button qBtn = btnQuitGo.AddComponent<Button>();
            qBtn.targetGraphic = qImg;
            GameObject qTxtGo = new GameObject("Text");
            qTxtGo.transform.SetParent(btnQuitGo.transform, false);
            var qTmp = qTxtGo.AddComponent<TextMeshProUGUI>();
            qTmp.text = "ĐÓNG GAME";
            qTmp.fontSize = 16;
            qTmp.fontStyle = FontStyles.Bold;
            qTmp.alignment = TextAlignmentOptions.Center;
            qTmp.color = Color.white;
            qTmp.raycastTarget = false;
            qBtn.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            GameObject btnCancelGo = new GameObject("Btn_CancelConfirm");
            btnCancelGo.transform.SetParent(cardGo.transform, false);
            RectTransform canRt = btnCancelGo.AddComponent<RectTransform>();
            canRt.anchoredPosition = new Vector2(110, -60);
            canRt.sizeDelta = new Vector2(160, 44);
            Image canImg = btnCancelGo.AddComponent<Image>();
            canImg.color = new Color(0.3f, 0.4f, 0.55f, 1f);
            Button canBtn = btnCancelGo.AddComponent<Button>();
            canBtn.targetGraphic = canImg;
            GameObject canTxtGo = new GameObject("Text");
            canTxtGo.transform.SetParent(btnCancelGo.transform, false);
            var canTmp = canTxtGo.AddComponent<TextMeshProUGUI>();
            canTmp.text = "HỦY";
            canTmp.fontSize = 16;
            canTmp.fontStyle = FontStyles.Bold;
            canTmp.alignment = TextAlignmentOptions.Center;
            canTmp.color = Color.white;
            canTmp.raycastTarget = false;
            canBtn.onClick.AddListener(() => Destroy(modalGo));
        }

        public void ShowNotice(string message)
        {
            Debug.Log($"[MainMenuController] Thông báo: {message}");

            if (noticeText == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject textGo = new GameObject("Notice_Text");
                    textGo.transform.SetParent(canvas.transform, false);
                    noticeText = textGo.AddComponent<TextMeshProUGUI>();
                    noticeText.fontSize = 26;
                    noticeText.alignment = TextAlignmentOptions.Center;
                    noticeText.color = new Color(1f, 0.85f, 0.4f, 1f);
                    noticeText.raycastTarget = false;
                    RectTransform rt = textGo.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(0, -260);
                    rt.sizeDelta = new Vector2(800, 60);
                }
            }

            if (noticeText != null)
            {
                noticeText.raycastTarget = false;
                noticeText.text = message;
                noticeText.gameObject.SetActive(true);

                if (_noticeCoroutine != null) StopCoroutine(_noticeCoroutine);
                _noticeCoroutine = StartCoroutine(HideNoticeRoutine());
            }
        }

        private IEnumerator HideNoticeRoutine()
        {
            yield return new WaitForSeconds(2.5f);
            if (noticeText != null)
            {
                noticeText.gameObject.SetActive(false);
            }
        }

        // Getters công khai để kiểm thử unit test / integration test (7 nút theo thiết kế ban đầu)
        public Button BtnStart => btnStart;
        public Button BtnAchievements => btnAchievements;
        public Button BtnSettings => btnSettings;
        public Button BtnLeaderboard => btnLeaderboard;
        public Button BtnProgress => btnProgress;
        public Button BtnVersus => btnVersus;
        public Button BtnExit => btnExit;
    }
}
