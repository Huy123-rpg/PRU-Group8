using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.UI;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển giao diện màn Thành tích Game 2 (Học viện Aurelia):
    /// - Sử dụng 100% thiết kế Fantasy gốc do người dùng tự dựng trong Unity Editor.
    /// - Tuyệt đối không sinh thêm các thanh danh sách xám/vàng hay nút thô runtime.
    /// - Quản lý 12 thành tích chuẩn (A01 đến A12) chia làm 2 trang (Trang 1: A01-A06; Trang 2: A07-A12).
    /// - Mỗi tiêu đề nằm đúng 1 DÒNG, mỗi mô tả nằm đúng 1 DÒNG (NoWrap, AutoSizing, căn lề trái, rộng 560px).
    /// - Nút "Trang sau" và "Trang trước" bắt click đa tầng (cả Image khung và Text TMP) đảm bảo luôn bấm được.
    /// - Mỗi thành tích có trạng thái độc lập (UNLOCKED: huy hiệu sáng + dấu tích vàng; LOCKED: icon khóa + huy hiệu bạc).
    /// - Tính tiến độ thật "Đã mở khóa: X/12" từ PhysicsSaveData.
    /// </summary>
    public class AchievementUIController : MonoBehaviour
    {
        [Header("=== TEXT REFERENCES ===")]
        [SerializeField] private TextMeshProUGUI progressText;      // "Đã mở khóa: X/12"
        [SerializeField] private TextMeshProUGUI pageIndicatorText; // "1/2" hoặc "2/2"

        [Header("=== BUTTON REFERENCES ===")]
        [SerializeField] private Button btnPrevPage;
        [SerializeField] private Button btnNextPage;
        [SerializeField] private Button btnBack;

        [Header("=== ROW REFERENCES (6 ROWS) ===")]
        [SerializeField] private Image[] rowBadgeImages = new Image[6];
        [SerializeField] private Image[] rowStatusImages = new Image[6];
        [SerializeField] private TextMeshProUGUI[] rowTitleTexts = new TextMeshProUGUI[6];
        [SerializeField] private TextMeshProUGUI[] rowDescTexts = new TextMeshProUGUI[6];

        // Thành phần nút phụ trên Text TMP để đảm bảo click vào chữ hay vào khung đều hoạt động 100%
        private Button _tmpNextBtn;
        private Button _tmpPrevBtn;
        private TextMeshProUGUI _tmpNextText;
        private TextMeshProUGUI _tmpPrevText;

        // Tọa độ chuẩn của 6 hàng chữ (Left X = -260, Width = 560, Pivot = Left Midline)
        private static readonly float[] RowTitleCenterY = new float[6] { 101.5f, 6.5f, -74.5f, -165.0f, -251.2f, -341.0f };
        private static readonly float[] RowDescCenterY = new float[6] { 68.0f, -26.0f, -113.0f, -204.4f, -294.2f, -379.0f };
        private const float TextLeftX = -260f;
        private const float TextAvailableWidth = 560f;
        private const float TitleBoxHeight = 32f;
        private const float DescBoxHeight = 28f;

        [Serializable]
        public class AchievementDefinition
        {
            public string id;
            public string name;
            public string description;
        }

        public static readonly List<AchievementDefinition> AllAchievements = new List<AchievementDefinition>
        {
            // Trang 1: A01 - A06
            new AchievementDefinition { id = "A01", name = "Tia sáng đầu tiên", description = "Khôi phục ít nhất 1 thiết bị của học viện" },
            new AchievementDefinition { id = "A02", name = "Kỹ sư Cơ học", description = "Khôi phục toàn bộ 4/4 thiết bị Chương I" },
            new AchievementDefinition { id = "A03", name = "Người thắp sáng", description = "Khôi phục toàn bộ 7/7 thiết bị Chương II" },
            new AchievementDefinition { id = "A04", name = "Bậc thầy Điện học", description = "Khôi phục toàn bộ 4/4 thiết bị Chương III" },
            new AchievementDefinition { id = "A05", name = "Nhà phát minh Cảm ứng", description = "Khôi phục toàn bộ 3/3 thiết bị Chương IV" },
            new AchievementDefinition { id = "A06", name = "Vệ binh Năng lượng xanh", description = "Khôi phục toàn bộ 3/3 thiết bị Chương V" },

            // Trang 2: A07 - A12
            new AchievementDefinition { id = "A07", name = "Người hồi sinh Aurelia", description = "Khôi phục toàn bộ 21/21 thiết bị của cả 5 chương" },
            new AchievementDefinition { id = "A08", name = "Điểm tuyệt đối", description = "Một lượt học hợp lệ đạt điểm 100/100" },
            new AchievementDefinition { id = "A09", name = "Bứt phá bản thân", description = "Hai lượt so sánh hợp lệ cùng nhóm tăng ít nhất 20 điểm" },
            new AchievementDefinition { id = "A10", name = "Không bỏ cuộc", description = "Cùng nhóm từng dưới 80, lần sau đạt từ 80 trở lên" },
            new AchievementDefinition { id = "A11", name = "Kho tàng kiến thức", description = "Đạt 100 câu hỏi khác nhau (MCQ đúng, essay >= 80)" },
            new AchievementDefinition { id = "A12", name = "Nhịp học đều đặn", description = "Có lượt hoàn tất hợp lệ ở 3 ngày khác nhau trong tuần" }
        };

        private int _currentPage = 1; // 1 hoặc 2
        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

        // Texture paths
        private const string TexPath09_00_35 = "Assets/Art/Environment/Ảnh ChatGPT 09_00_35 26 thg 10, 2026.png";
        private const string TexPath09_00_29 = "Assets/Art/Environment/Ảnh ChatGPT 09_00_29 26 thg 10, 2026.png";
        private const string TexPath03_47_38 = "Assets/Art/Environment/Ảnh ChatGPT 03_47_38 26 thg 10, 2026.png";
        private const string TexPathBadgesMystic = "Assets/Art/Environment/Bộ huy hiệu thành tựu bạc xanh huyền ảo.png";

        // Sprite Names
        private const string SpriteCheckmark = "Ảnh ChatGPT 09_00_35 26 thg 10, 2026_8";
        private const string SpriteLock = "Ảnh ChatGPT 09_00_29 26 thg 10, 2026_14";

        private static readonly string[] Page1UnlockedSprites = new string[6]
        {
            "Ảnh ChatGPT 09_00_29 26 thg 10, 2026_8",
            "Ảnh ChatGPT 09_00_35 26 thg 10, 2026_10",
            "Ảnh ChatGPT 09_00_35 26 thg 10, 2026_11",
            "Ảnh ChatGPT 09_00_35 26 thg 10, 2026_12",
            "Ảnh ChatGPT 09_00_35 26 thg 10, 2026_13",
            "Ảnh ChatGPT 09_00_35 26 thg 10, 2026_14"
        };

        private static readonly string[] Page1LockedSprites = new string[6]
        {
            "Ảnh ChatGPT 09_00_35 26 thg 10, 2026_2",
            "Ảnh ChatGPT 09_00_35 26 thg 10, 2026_3",
            "Ảnh ChatGPT 09_00_35 26 thg 10, 2026_4",
            "Ảnh ChatGPT 09_00_35 26 thg 10, 2026_5",
            "Ảnh ChatGPT 09_00_35 26 thg 10, 2026_6",
            "Ảnh ChatGPT 09_00_35 26 thg 10, 2026_7"
        };

        private static readonly string[] Page2UnlockedSprites = new string[6]
        {
            "Ảnh ChatGPT 03_47_38 26 thg 10, 2026_0",
            "Ảnh ChatGPT 03_47_38 26 thg 10, 2026_1",
            "Ảnh ChatGPT 03_47_38 26 thg 10, 2026_2",
            "Ảnh ChatGPT 03_47_38 26 thg 10, 2026_4",
            "Ảnh ChatGPT 03_47_38 26 thg 10, 2026_5",
            "Ảnh ChatGPT 03_47_38 26 thg 10, 2026_6"
        };

        private static readonly string[] Page2LockedSprites = new string[6]
        {
            "Bộ huy hiệu thành tựu bạc xanh huyền ảo_4",
            "Bộ huy hiệu thành tựu bạc xanh huyền ảo_5",
            "Bộ huy hiệu thành tựu bạc xanh huyền ảo_33",
            "Bộ huy hiệu thành tựu bạc xanh huyền ảo_23",
            "Bộ huy hiệu thành tựu bạc xanh huyền ảo_34",
            "Bộ huy hiệu thành tựu bạc xanh huyền ảo_35"
        };

        private void Awake()
        {
            EnsureEventSystem();
            CleanupSynthesizedContainers();

            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName.Contains("15.4") || sceneName.Contains("15_Achievements"))
            {
                _currentPage = 2;
            }
            else
            {
                _currentPage = 1;
            }

            FindSceneReferences();
            StandardizeRowLayouts();
            SetupActionButtons();
        }

        private void Start()
        {
            RefreshUI();
        }

        /// <summary>
        /// Loại bỏ triệt để các container UI runtime cũ nếu còn tồn đọng
        /// </summary>
        private void CleanupSynthesizedContainers()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Transform oldList = canvas.transform.Find("AchievementListContainer");
            if (oldList != null)
            {
                Debug.Log("[AchievementUIController] Đã loại bỏ AchievementListContainer thừa.");
                DestroyImmediate(oldList.gameObject);
            }

            Transform oldControls = canvas.transform.Find("AchievementPageControls");
            if (oldControls != null)
            {
                Debug.Log("[AchievementUIController] Đã loại bỏ AchievementPageControls thừa.");
                DestroyImmediate(oldControls.gameObject);
            }

            // Xóa bất kỳ row runtime nào nếu bị gắn trực tiếp vào canvas
            for (int i = 0; i < 12; i++)
            {
                Transform r = canvas.transform.Find($"Row_{i}");
                if (r != null) DestroyImmediate(r.gameObject);
            }
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }
        }

        /// <summary>
        /// Tự động tìm và bind chính xác các component UI Fantasy có sẵn trong Scene
        /// </summary>
        private void FindSceneReferences()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[AchievementUIController] Không tìm thấy Canvas trong Scene!");
                return;
            }

            Image[] allImages = canvas.GetComponentsInChildren<Image>(true);
            TextMeshProUGUI[] allTmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);

            // 1. Phân loại và cấu hình Image
            List<Image> foundLeftBadges = new List<Image>();
            List<Image> foundRightStatus = new List<Image>();
            Image imgNextFrame = null;
            Image imgPrevFrame = null;
            Image imgBackFrame = null;

            foreach (var img in allImages)
            {
                if (img == null || img.transform == canvas.transform) continue;
                Vector2 pos = img.rectTransform.anchoredPosition;
                Vector2 size = img.rectTransform.sizeDelta;

                // Nút Quay lại (x > 500, y < -450)
                if (pos.x > 500f && pos.y < -450f)
                {
                    imgBackFrame = img;
                }
                // Nút Trang sau (x: 150..250, y: -445..-390)
                else if (pos.x >= 150f && pos.x <= 250f && pos.y >= -445f && pos.y <= -390f)
                {
                    imgNextFrame = img;
                }
                // Nút Trang trước (x: -240..-150, y: -445..-390)
                else if (pos.x >= -240f && pos.x <= -150f && pos.y >= -445f && pos.y <= -390f)
                {
                    imgPrevFrame = img;
                }
                // Left badge (x < -250, -405 <= y <= 120, size >= 70)
                else if (pos.x < -250f && pos.y >= -405f && pos.y <= 120f && size.x >= 70f && size.y >= 70f)
                {
                    foundLeftBadges.Add(img);
                    img.raycastTarget = false;
                }
                // Right status (dấu tích / khóa, x > 320, -380 <= y <= 120)
                else if (pos.x > 320f && pos.y >= -380f && pos.y <= 120f)
                {
                    foundRightStatus.Add(img);
                    img.raycastTarget = false;
                }
                else
                {
                    // Toàn bộ các background, viền trang trí khác tuyệt đối không chặn raycast
                    img.raycastTarget = false;
                }
            }

            // Gắn Button và đảm bảo raycastTarget cho các nút chuyển trang và quay lại
            if (imgBackFrame != null)
            {
                imgBackFrame.raycastTarget = true;
                btnBack = imgBackFrame.GetComponent<Button>() ?? imgBackFrame.gameObject.AddComponent<Button>();
                btnBack.targetGraphic = imgBackFrame;
            }

            if (imgNextFrame != null)
            {
                imgNextFrame.raycastTarget = true;
                btnNextPage = imgNextFrame.GetComponent<Button>() ?? imgNextFrame.gameObject.AddComponent<Button>();
                btnNextPage.targetGraphic = imgNextFrame;
            }

            if (imgPrevFrame != null)
            {
                imgPrevFrame.raycastTarget = true;
                btnPrevPage = imgPrevFrame.GetComponent<Button>() ?? imgPrevFrame.gameObject.AddComponent<Button>();
                btnPrevPage.targetGraphic = imgPrevFrame;
            }

            // Sắp xếp badges và status từ trên xuống dưới (Y giảm dần)
            foundLeftBadges.Sort((a, b) => b.rectTransform.anchoredPosition.y.CompareTo(a.rectTransform.anchoredPosition.y));
            foundRightStatus.Sort((a, b) => b.rectTransform.anchoredPosition.y.CompareTo(a.rectTransform.anchoredPosition.y));

            for (int i = 0; i < 6 && i < foundLeftBadges.Count; i++) rowBadgeImages[i] = foundLeftBadges[i];
            for (int i = 0; i < 6 && i < foundRightStatus.Count; i++) rowStatusImages[i] = foundRightStatus[i];

            // 2. Phân loại TextMeshProUGUI
            (float ymin, float ymax)[] titleYRanges = new (float, float)[6]
            {
                (75f, 120f),
                (-10f, 35f),
                (-90f, -55f),
                (-180f, -145f),
                (-270f, -230f),
                (-360f, -320f)
            };

            (float ymin, float ymax)[] descYRanges = new (float, float)[6]
            {
                (50f, 80f),
                (-45f, -15f),
                (-130f, -95f),
                (-225f, -185f),
                (-315f, -275f),
                (-400f, -360f)
            };

            foreach (var tmp in allTmps)
            {
                if (tmp == null) continue;
                Vector2 pos = tmp.rectTransform.anchoredPosition;

                // Page Indicator: "1/2" hoặc "2/2" (pos.x ~ 47, pos.y ~ -420)
                if (pos.x >= 10f && pos.x <= 90f && pos.y >= -440f && pos.y <= -400f)
                {
                    pageIndicatorText = tmp;
                    tmp.raycastTarget = false;
                    continue;
                }

                // Tiến độ: "Đã mở khóa: .../12" (pos.y ~ -497)
                if (pos.x >= -50f && pos.x <= 150f && pos.y >= -520f && pos.y <= -475f)
                {
                    progressText = tmp;
                    tmp.raycastTarget = false;
                    continue;
                }

                // Nút "Trang trước" text (pos.x: -220..-120, pos.y: -440..-400)
                if (pos.x >= -220f && pos.x <= -120f && pos.y >= -440f && pos.y <= -400f)
                {
                    _tmpPrevText = tmp;
                    _tmpPrevText.raycastTarget = true;
                    _tmpPrevBtn = tmp.GetComponent<Button>() ?? tmp.gameObject.AddComponent<Button>();
                    _tmpPrevBtn.targetGraphic = tmp;
                    if (btnPrevPage == null) btnPrevPage = _tmpPrevBtn;
                    continue;
                }

                // Nút "Trang sau" text (pos.x: 150..260, pos.y: -440..-400)
                if (pos.x >= 150f && pos.x <= 260f && pos.y >= -440f && pos.y <= -400f)
                {
                    _tmpNextText = tmp;
                    _tmpNextText.raycastTarget = true;
                    _tmpNextBtn = tmp.GetComponent<Button>() ?? tmp.gameObject.AddComponent<Button>();
                    _tmpNextBtn.targetGraphic = tmp;
                    if (btnNextPage == null) btnNextPage = _tmpNextBtn;
                    continue;
                }

                // Phân loại Title
                bool matchedTitle = false;
                for (int r = 0; r < 6; r++)
                {
                    if (pos.x >= -150f && pos.x <= 15f && pos.y >= titleYRanges[r].ymin && pos.y <= titleYRanges[r].ymax)
                    {
                        rowTitleTexts[r] = tmp;
                        tmp.raycastTarget = false;
                        matchedTitle = true;
                        break;
                    }
                }
                if (matchedTitle) continue;

                // Phân loại Description
                for (int r = 0; r < 6; r++)
                {
                    if (pos.x >= -90f && pos.x <= 90f && pos.y >= descYRanges[r].ymin && pos.y <= descYRanges[r].ymax)
                    {
                        rowDescTexts[r] = tmp;
                        tmp.raycastTarget = false;
                        break;
                    }
                }
            }

            Debug.Log($"[AchievementUIController] Bind UI: Badges={foundLeftBadges.Count}, Status={foundRightStatus.Count}, " +
                      $"PageInd={(pageIndicatorText != null)}, Prog={(progressText != null)}, " +
                      $"Prev={(btnPrevPage != null)}, Next={(btnNextPage != null)}, Back={(btnBack != null)}.");
        }

        /// <summary>
        /// Chuẩn hóa tuyệt đối hình học và cài đặt TextMeshProUGUI của 6 hàng:
        /// - Mỗi tên đúng 1 DÒNG, mỗi mô tả đúng 1 DÒNG riêng biệt.
        /// - Pivot (0, 0.5) căn trái từ X = -260, width = 560px đủ rộng giữa huy hiệu và icon khóa.
        /// - NoWrap, AutoSizing vừa vặn, không bao giờ ngắt dòng hay đè lên hàng khác.
        /// </summary>
        public void StandardizeRowLayouts()
        {
            for (int i = 0; i < 6; i++)
            {
                if (rowTitleTexts[i] != null)
                {
                    var t = rowTitleTexts[i];
                    var rt = t.rectTransform;
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0f, 0.5f);
                    rt.anchoredPosition = new Vector2(TextLeftX, RowTitleCenterY[i]);
                    rt.sizeDelta = new Vector2(TextAvailableWidth, TitleBoxHeight);

                    t.textWrappingMode = TextWrappingModes.NoWrap;
                    t.overflowMode = TextOverflowModes.Ellipsis;
                    t.alignment = TextAlignmentOptions.MidlineLeft;
                    t.margin = Vector4.zero;
                    t.fontStyle = FontStyles.Bold;
                    t.enableAutoSizing = true;
                    t.fontSizeMin = 18f;
                    t.fontSizeMax = 23f;
                    t.raycastTarget = false;
                }

                if (rowDescTexts[i] != null)
                {
                    var d = rowDescTexts[i];
                    var rt = d.rectTransform;
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0f, 0.5f);
                    rt.anchoredPosition = new Vector2(TextLeftX, RowDescCenterY[i]);
                    rt.sizeDelta = new Vector2(TextAvailableWidth, DescBoxHeight);

                    d.textWrappingMode = TextWrappingModes.NoWrap;
                    d.overflowMode = TextOverflowModes.Ellipsis;
                    d.alignment = TextAlignmentOptions.MidlineLeft;
                    d.margin = Vector4.zero;
                    d.fontStyle = FontStyles.Normal;
                    d.enableAutoSizing = true;
                    d.fontSizeMin = 14f;
                    d.fontSizeMax = 17f;
                    d.raycastTarget = false;
                }
            }
        }

        private void SetupActionButtons()
        {
            Navigation navNone = new Navigation { mode = Navigation.Mode.None };

            if (btnPrevPage != null)
            {
                btnPrevPage.navigation = navNone;
                btnPrevPage.onClick.RemoveAllListeners();
                btnPrevPage.onClick.AddListener(OnPrevPageClicked);
                UIHoverPulseEffect.Attach(btnPrevPage.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
            }
            if (_tmpPrevBtn != null)
            {
                _tmpPrevBtn.navigation = navNone;
                _tmpPrevBtn.onClick.RemoveAllListeners();
                _tmpPrevBtn.onClick.AddListener(OnPrevPageClicked);
            }

            if (btnNextPage != null)
            {
                btnNextPage.navigation = navNone;
                btnNextPage.onClick.RemoveAllListeners();
                btnNextPage.onClick.AddListener(OnNextPageClicked);
                UIHoverPulseEffect.Attach(btnNextPage.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
            }
            if (_tmpNextBtn != null)
            {
                _tmpNextBtn.navigation = navNone;
                _tmpNextBtn.onClick.RemoveAllListeners();
                _tmpNextBtn.onClick.AddListener(OnNextPageClicked);
            }

            if (btnBack != null)
            {
                btnBack.navigation = navNone;
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackClicked);
                UIHoverPulseEffect.Attach(btnBack.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
            }
        }

        public void OnPrevPageClicked()
        {
            if (_currentPage > 1)
            {
                _currentPage = 1;
                RefreshUI();
            }
        }

        public void OnNextPageClicked()
        {
            if (_currentPage < 2)
            {
                _currentPage = 2;
                RefreshUI();
            }
        }

        public void OnBackClicked()
        {
            Debug.Log("[AchievementUIController] Quay lại MainMenu.");
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.NavigateTo(RouteId.MainMenu);
            }
            else
            {
                SceneManager.LoadScene(PhysicsSceneNames.MainMenu);
            }
        }

        /// <summary>
        /// Làm mới và hiển thị toàn bộ 6 dòng của trang hiện tại với trạng thái thật
        /// </summary>
        public void RefreshUI()
        {
            StandardizeRowLayouts();

            var save = PhysicsSaveManager.Load() ?? new PhysicsSaveData();
            EvaluateSaveAchievements(save);

            HashSet<string> unlocked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (save.unlockedAchievementIds != null)
            {
                foreach (var id in save.unlockedAchievementIds)
                {
                    unlocked.Add(id);
                }
            }

            // Tính tổng mở khóa thực tế X/12
            int totalUnlocked = 0;
            foreach (var ach in AllAchievements)
            {
                if (IsAchievementUnlocked(unlocked, ach.id)) totalUnlocked++;
            }

            // Cập nhật nhãn tiến độ
            if (progressText != null)
            {
                progressText.text = $"Đã mở khóa: <b>{totalUnlocked}/12</b>";
            }

            // Cập nhật số trang
            if (pageIndicatorText != null)
            {
                pageIndicatorText.text = $"{_currentPage}/2";
            }

            // Trạng thái nút chuyển trang (cả Button khung và Button text)
            bool canPrev = (_currentPage > 1);
            if (btnPrevPage != null)
            {
                btnPrevPage.interactable = canPrev;
                var graphic = btnPrevPage.targetGraphic ?? btnPrevPage.GetComponent<Graphic>();
                if (graphic != null) graphic.color = canPrev ? Color.white : new Color(1f, 1f, 1f, 0.4f);
            }
            if (_tmpPrevBtn != null)
            {
                _tmpPrevBtn.interactable = canPrev;
                if (_tmpPrevText != null) _tmpPrevText.color = canPrev ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.4f);
            }

            bool canNext = (_currentPage < 2);
            if (btnNextPage != null)
            {
                btnNextPage.interactable = canNext;
                var graphic = btnNextPage.targetGraphic ?? btnNextPage.GetComponent<Graphic>();
                if (graphic != null) graphic.color = canNext ? Color.white : new Color(1f, 1f, 1f, 0.4f);
            }
            if (_tmpNextBtn != null)
            {
                _tmpNextBtn.interactable = canNext;
                if (_tmpNextText != null) _tmpNextText.color = canNext ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.4f);
            }

            // Render 6 dòng Fantasy của trang hiện tại
            int startIndex = (_currentPage - 1) * 6;
            Sprite checkmarkSprite = GetSprite(SpriteCheckmark, TexPath09_00_35);
            Sprite lockSprite = GetSprite(SpriteLock, TexPath09_00_29);

            for (int i = 0; i < 6; i++)
            {
                int achIndex = startIndex + i;
                if (achIndex >= AllAchievements.Count) break;

                var def = AllAchievements[achIndex];
                bool isAchUnlocked = IsAchievementUnlocked(unlocked, def.id);

                // 1. Tiêu đề (Chuẩn hóa sạch, không chứa ký tự xuống dòng)
                if (rowTitleTexts[i] != null)
                {
                    rowTitleTexts[i].text = def.name.Trim().Replace("\r", "").Replace("\n", "");
                    rowTitleTexts[i].color = isAchUnlocked
                        ? new Color(0.988f, 0.890f, 0.541f, 1f)  // Vàng sáng
                        : new Color(0.580f, 0.640f, 0.720f, 1f); // Xám bạc (khóa)
                }

                // 2. Mô tả (Chuẩn hóa sạch, không chứa ký tự xuống dòng)
                if (rowDescTexts[i] != null)
                {
                    rowDescTexts[i].text = def.description.Trim().Replace("\r", "").Replace("\n", "");
                    rowDescTexts[i].color = isAchUnlocked
                        ? new Color(0.950f, 0.950f, 0.980f, 1f)  // Trắng sáng
                        : new Color(0.400f, 0.460f, 0.540f, 1f); // Xám tối
                }

                // 3. Huy hiệu bên trái
                if (rowBadgeImages[i] != null)
                {
                    Sprite badgeSp = GetBadgeSprite(_currentPage, i, isAchUnlocked);
                    if (badgeSp != null)
                    {
                        rowBadgeImages[i].sprite = badgeSp;
                        rowBadgeImages[i].color = Color.white;
                    }
                }

                // 4. Trạng thái bên phải: Dấu tích vàng (ĐÃ ĐẠT) hoặc Khóa (CHƯA ĐẠT)
                if (rowStatusImages[i] != null)
                {
                    Sprite statusSp = isAchUnlocked ? checkmarkSprite : lockSprite;
                    if (statusSp != null)
                    {
                        rowStatusImages[i].sprite = statusSp;
                        rowStatusImages[i].color = Color.white;
                    }
                }
            }
        }

        private bool IsAchievementUnlocked(HashSet<string> unlockedSet, string achId)
        {
            if (unlockedSet.Contains(achId)) return true;

            // Ánh xạ tương đương từ các ID cũ trong hệ thống
            switch (achId)
            {
                case "A02": return unlockedSet.Contains(AchievementManager.ACH_COMPLETE_CH1);
                case "A07": return unlockedSet.Contains(AchievementManager.ACH_COMPLETE_ALL_CHAPTERS);
                case "A11": return unlockedSet.Contains(AchievementManager.ACH_QUIZ_50) || unlockedSet.Contains(AchievementManager.ACH_CORRECT_100);
                default: return false;
            }
        }

        /// <summary>
        /// Đánh giá các điều kiện thực tế từ SaveData để tự động mở khóa thành tích tương ứng
        /// </summary>
        private void EvaluateSaveAchievements(PhysicsSaveData save)
        {
            if (save == null) return;
            if (save.unlockedAchievementIds == null) save.unlockedAchievementIds = new List<string>();

            bool changed = false;

            // A01: Khôi phục >= 1 thiết bị
            int totalRestored = (save.restoredDevices != null) ? save.restoredDevices.Count : 0;
            if (totalRestored >= 1 && !save.unlockedAchievementIds.Contains("A01"))
            {
                save.unlockedAchievementIds.Add("A01");
                changed = true;
            }

            // A02: Khôi phục 4/4 thiết bị Chương I
            int ch1Restored = GetChapterRestoredCount(save, 1);
            if (ch1Restored >= 4 && !save.unlockedAchievementIds.Contains("A02"))
            {
                save.unlockedAchievementIds.Add("A02");
                changed = true;
            }

            // A03: Khôi phục 7/7 thiết bị Chương II
            int ch2Restored = GetChapterRestoredCount(save, 2);
            if (ch2Restored >= 7 && !save.unlockedAchievementIds.Contains("A03"))
            {
                save.unlockedAchievementIds.Add("A03");
                changed = true;
            }

            // A04: Khôi phục 4/4 thiết bị Chương III
            int ch3Restored = GetChapterRestoredCount(save, 3);
            if (ch3Restored >= 4 && !save.unlockedAchievementIds.Contains("A04"))
            {
                save.unlockedAchievementIds.Add("A04");
                changed = true;
            }

            // A05: Khôi phục 3/3 thiết bị Chương IV
            int ch4Restored = GetChapterRestoredCount(save, 4);
            if (ch4Restored >= 3 && !save.unlockedAchievementIds.Contains("A05"))
            {
                save.unlockedAchievementIds.Add("A05");
                changed = true;
            }

            // A06: Khôi phục 3/3 thiết bị Chương V
            int ch5Restored = GetChapterRestoredCount(save, 5);
            if (ch5Restored >= 3 && !save.unlockedAchievementIds.Contains("A06"))
            {
                save.unlockedAchievementIds.Add("A06");
                changed = true;
            }

            // A07: Khôi phục toàn bộ 21 thiết bị
            if (totalRestored >= 21 && !save.unlockedAchievementIds.Contains("A07"))
            {
                save.unlockedAchievementIds.Add("A07");
                changed = true;
            }

            // A08: Điểm tuyệt đối 100/100
            bool has100 = false;
            if (save.quizHistory != null)
            {
                foreach (var q in save.quizHistory)
                {
                    if (q.scorePercent >= 100) { has100 = true; break; }
                }
            }
            if (has100 && !save.unlockedAchievementIds.Contains("A08"))
            {
                save.unlockedAchievementIds.Add("A08");
                changed = true;
            }

            // A11: Kho tàng kiến thức (>= 100 câu)
            if ((save.totalQuestionsAnswered >= 100 || save.totalCorrectAnswers >= 100) && !save.unlockedAchievementIds.Contains("A11"))
            {
                save.unlockedAchievementIds.Add("A11");
                changed = true;
            }

            if (changed)
            {
                PhysicsSaveManager.Save(save);
            }
        }

        private int GetChapterRestoredCount(PhysicsSaveData save, int chapterId)
        {
            return RestorationService.GetRestoredCountForChapter(chapterId);
        }

        private Sprite GetBadgeSprite(int page, int rowIndex, bool isUnlocked)
        {
            if (rowIndex < 0 || rowIndex >= 6) return null;

            if (page == 1)
            {
                string spName = isUnlocked ? Page1UnlockedSprites[rowIndex] : Page1LockedSprites[rowIndex];
                string texPath = isUnlocked && rowIndex == 0 ? TexPath09_00_29 : TexPath09_00_35;
                return GetSprite(spName, texPath);
            }
            else
            {
                string spName = isUnlocked ? Page2UnlockedSprites[rowIndex] : Page2LockedSprites[rowIndex];
                string texPath = isUnlocked ? TexPath03_47_38 : TexPathBadgesMystic;
                return GetSprite(spName, texPath);
            }
        }

        private static Sprite GetSprite(string spriteName, string textureAssetPath)
        {
            if (string.IsNullOrEmpty(spriteName)) return null;

            if (SpriteCache.TryGetValue(spriteName, out var cached) && cached != null)
            {
                return cached;
            }

            // 1. Tìm trong danh sách Sprite đã được Unity tải vào bộ nhớ
            Sprite[] allLoaded = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (var s in allLoaded)
            {
                if (s != null && !string.IsNullOrEmpty(s.name))
                {
                    if (!SpriteCache.ContainsKey(s.name))
                    {
                        SpriteCache[s.name] = s;
                    }
                }
            }

            if (SpriteCache.TryGetValue(spriteName, out var found) && found != null)
            {
                return found;
            }

#if UNITY_EDITOR
            // 2. Dự phòng trong Editor: Tải trực tiếp từ asset texture bằng AssetDatabase
            if (!string.IsNullOrEmpty(textureAssetPath))
            {
                var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(textureAssetPath);
                foreach (var a in assets)
                {
                    if (a is Sprite sp && sp != null)
                    {
                        if (!SpriteCache.ContainsKey(sp.name))
                        {
                            SpriteCache[sp.name] = sp;
                        }
                    }
                }
            }

            if (SpriteCache.TryGetValue(spriteName, out var editorFound) && editorFound != null)
            {
                return editorFound;
            }
#endif

            return null;
        }

        // Public accessors for testing
        public int CurrentPage => _currentPage;
        public Button BtnPrevPage => btnPrevPage;
        public Button BtnNextPage => btnNextPage;
        public Button BtnBack => btnBack;
        public TextMeshProUGUI ProgressText => progressText;
        public TextMeshProUGUI PageIndicatorText => pageIndicatorText;
        public Image[] RowBadgeImages => rowBadgeImages;
        public Image[] RowStatusImages => rowStatusImages;
        public TextMeshProUGUI[] RowTitleTexts => rowTitleTexts;
        public TextMeshProUGUI[] RowDescTexts => rowDescTexts;
        public Button TmpNextBtn => _tmpNextBtn;
        public Button TmpPrevBtn => _tmpPrevBtn;
    }
}
