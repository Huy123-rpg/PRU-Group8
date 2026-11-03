using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    [Serializable]
    public class MapStationInfo
    {
        public int stationIndex; // 1..7
        public string deviceId;
        public string baiId;
        public string lessonTitle;
        public RectTransform stationRect;
        public RectTransform deviceGraphicRect;
        public TextMeshProUGUI numberText;
        public Image stationImage;
        public Outline outlineEffect;
        public bool isRestored;

        // Biển tên bài học gắn trực tiếp trên bản đồ
        public GameObject plaqueGo;
        public RectTransform plaqueRect;
        public Image plaqueBg;
        public Outline plaqueOutline;
        public TextMeshProUGUI plaqueTitleText;
        public Image plaqueGemImg;
        public TextMeshProUGUI plaqueGemText;
        public UIHoverPulseEffect plaquePulse;
    }

    /// <summary>
    /// Điều khiển toàn bộ logic Bản đồ 5 Chương (Scene 06, 07, 08, 09, 10 và các scene after).
    /// Tuân thủ nghiêm ngặt đặc tả mục 7:
    /// - Di chuyển WASD theo deltaTime, chuẩn hóa vector chéo, khóa khi focus InputField hoặc mở modal Pause.
    /// - Hiển thị đúng nhân vật (Minh, Lan, Duy, Hà) đã chọn ở 02_CharacterSelection.
    /// - Đến gần thiết bị hiện "Nhấn E để học: [Tên bài]" + viền trạm sáng nổi bật.
    /// - Nhấn E chọn trạm gần nhất trong bán kính -> lưu vị trí -> mở 17. Type (ModeSelection).
    /// - Nhấn E ngoài phạm vi -> hiện gợi ý "Hãy đến gần một thiết bị để học".
    /// - Sáng từng trạm 1/4, 2/4... theo dữ liệu thật (partial restoration); trạm đã khôi phục vẫn cho phép ôn tập.
    /// - Nút "Chọn chương" quay về 04_ChapterSelection không bị chặn raycast.
    /// - Nút Pause / phím Esc mở popup: Tiếp tục, Cài đặt, Kết thúc buổi.
    /// </summary>
    public class ChapterMapController : MonoBehaviour
    {
        [Header("=== MAP CONFIG ===")]
        [SerializeField] private int chapterIndex = 1;
        [SerializeField] private float moveSpeed = 380f;
        [SerializeField] private float interactionRadius = 130f;

        public float InteractionRadius { get => interactionRadius; set => interactionRadius = value; }
        public MapStationInfo NearestStation => _nearestStation;
        public IReadOnlyList<MapStationInfo> Stations => _stations;
        public GameObject PromptPopup => _promptPopup;
        public TextMeshProUGUI PromptText => _promptText;
        public RectTransform PlayerRect { get => playerRect; set => playerRect = value; }

        [Header("=== PLAYER REFERENCES ===")]
        [SerializeField] private RectTransform playerRect;
        [SerializeField] private Image playerImage;

        [Header("=== UI LABELS & BUTTONS ===")]
        [SerializeField] private TextMeshProUGUI txtProgress; // "Đã sáng: 0/4"
        [SerializeField] private TextMeshProUGUI txtTimer;    // "30 phút"
        [SerializeField] private TextMeshProUGUI txtAreaTitle;// "XƯỞNG ĐIỆN", etc.
        [SerializeField] private Button btnBack;              // "Chọn chương"
        [SerializeField] private Button btnPause;             // Nút Pause

        private readonly List<MapStationInfo> _stations = new List<MapStationInfo>();
        private GameObject _promptPopup;
        private TextMeshProUGUI _promptText;
        private GameObject _pauseModal;
        private MapStationInfo _nearestStation = null;
        private bool _isPaused = false;
        private bool _isTransitioning = false;
        private Vector2 _minBounds = new Vector2(-850f, -400f);
        private Vector2 _maxBounds = new Vector2(850f, 320f);

        private void Awake()
        {
            EnsureEventSystem();
            DetermineChapterIndex();
            FindUIReferences();
            SetupStations();
            SetupBackButton();
            SetupPauseButton();
            CreatePromptUI();
            CreatePauseModal();
        }

        private void Start()
        {
            NavigationService.Instance.EnsureDevContext();
            NavigationService.Instance.Selection.ChapterId = chapterIndex;

            ApplyPlayerPositionAndSprite();
            RefreshRestorationVisuals();
            UpdateSessionTimerDisplay();
        }

        private void DetermineChapterIndex()
        {
            string sceneName = gameObject.scene.name ?? "";

            // 1. Kiểm tra các scene after đặc biệt bị đổi tên/đảo ngược
            if (sceneName.Contains("30.") || sceneName.Contains("Chuong IV after") || sceneName.Contains("Chương IV after"))
            {
                chapterIndex = 5; // Scene 30 nội dung là Vườn Năng Lượng (Chương V)
            }
            else if (sceneName.Contains("31.") || sceneName.Contains("Chuong V after") || sceneName.Contains("Chương V after"))
            {
                chapterIndex = 4; // Scene 31 nội dung là Tháp Cảm Ứng (Chương IV)
            }
            // 2. Kiểm tra theo số La Mã: PHẢI kiểm tra III, IV, II trước I để tránh bị match chuỗi con "Chương I" trong "Chương III"!
            else if (sceneName.Contains("III") || sceneName.Contains("08_") || sceneName.Contains("29."))
            {
                chapterIndex = 3;
            }
            else if (sceneName.Contains("IV") || sceneName.Contains("09_"))
            {
                chapterIndex = 4;
            }
            else if (sceneName.Contains("II") || sceneName.Contains("07_") || sceneName.Contains("28."))
            {
                chapterIndex = 2;
            }
            else if (sceneName.Contains("Chương V") || sceneName.Contains("Chuong V") || sceneName.Contains(" 5") || sceneName.Contains("10_"))
            {
                chapterIndex = 5;
            }
            else if (sceneName.Contains("Chương I") || sceneName.Contains("Chuong I") || sceneName.Contains(" 1") || sceneName.Contains("06_") || sceneName.Contains("27."))
            {
                chapterIndex = 1;
            }
            else
            {
                // Fallback theo Selection hoặc Session đã chọn trước đó
                if (NavigationService.Instance != null && NavigationService.Instance.Selection.ChapterId >= 1 && NavigationService.Instance.Selection.ChapterId <= 5)
                {
                    chapterIndex = NavigationService.Instance.Selection.ChapterId;
                }
                else if (PhysicsGameSession.Instance != null && PhysicsGameSession.Instance.SelectedChapterId >= 1 && PhysicsGameSession.Instance.SelectedChapterId <= 5)
                {
                    chapterIndex = PhysicsGameSession.Instance.SelectedChapterId;
                }
                else if (chapterIndex < 1 || chapterIndex > 5)
                {
                    chapterIndex = 1;
                }
            }

            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.Selection.ChapterId = chapterIndex;
            }
            if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.SelectChapter(chapterIndex);
            }
            Debug.Log($"[ChapterMapController] Xác định chapterIndex = {chapterIndex} cho scene '{sceneName}'.");
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }

        private void FindUIReferences()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. Tìm nhân vật người chơi (Image có tên Player/PlayerCharacter hoặc kích thước xấp xỉ 110x140)
            if (playerRect == null)
            {
                var pGo = GameObject.Find("Player") ?? GameObject.Find("PlayerCharacter");
                if (pGo != null)
                {
                    playerRect = pGo.GetComponent<RectTransform>();
                    playerImage = pGo.GetComponent<Image>();
                }
            }

            if (playerRect == null)
            {
                foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                {
                    var rt = img.rectTransform;
                    if (rt.sizeDelta.x >= 90f && rt.sizeDelta.x <= 160f && rt.sizeDelta.y >= 120f && rt.sizeDelta.y <= 200f)
                    {
                        if (!img.name.Contains("station") && !img.name.Contains("Background") && !img.name.Contains("bg"))
                        {
                            playerRect = rt;
                            playerImage = img;
                            break;
                        }
                    }
                }
            }

            if (playerRect == null)
            {
                GameObject pGo = new GameObject("PlayerCharacter");
                pGo.transform.SetParent(canvas.transform, false);
                playerRect = pGo.AddComponent<RectTransform>();
                playerRect.sizeDelta = new Vector2(110f, 140f);
                playerRect.anchoredPosition = new Vector2(-201f, -132f);
                playerImage = pGo.AddComponent<Image>();
            }

            if (playerImage != null)
            {
                playerImage.raycastTarget = false;
            }

            // 2. Tìm nhãn tiến độ, timer, title
            foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                string txt = tmp.text.Trim();
                if (txt.Contains("Đã sáng:") || txt.Contains("Da sang:"))
                {
                    txtProgress = tmp;
                    tmp.raycastTarget = false;
                }
                else if (txt.Contains("phút") || txt.Contains("phut") || txt.Contains(":"))
                {
                    txtTimer = tmp;
                    tmp.raycastTarget = false;
                }
                else if (txt.Contains("SÂN CƠ HỌC") || txt.Contains("THƯ VIỆN") || txt.Contains("XƯỞNG ĐIỆN") || txt.Contains("THÁP CẢM ỨNG") || txt.Contains("VƯỜN NĂNG LƯỢNG"))
                {
                    txtAreaTitle = tmp;
                    tmp.raycastTarget = false;
                }
            }

            // Tìm nút Chọn chương: Khung Image ở góc dưới bên trái (x < -450, y < -350, size >= 200)
            if (btnBack == null)
            {
                foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                {
                    var rt = img.rectTransform;
                    if (rt.anchoredPosition.x < -450f && rt.anchoredPosition.y < -350f && rt.sizeDelta.x >= 200f)
                    {
                        btnBack = img.GetComponent<Button>() ?? img.gameObject.AddComponent<Button>();
                        img.raycastTarget = true;
                        btnBack.targetGraphic = img;
                        break;
                    }
                }
            }

            // 3. Tìm nút Pause ở góc trên bên phải (x > 700, y > 350)
            if (btnPause == null)
            {
                foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                {
                    var rt = img.rectTransform;
                    if (rt.anchoredPosition.x > 700f && rt.anchoredPosition.y > 350f)
                    {
                        if (img.gameObject.name.Contains("Image (5)") || img.gameObject.name.ToLower().Contains("pause") || img.gameObject.name.ToLower().Contains("menu"))
                        {
                            btnPause = img.GetComponent<Button>() ?? img.gameObject.AddComponent<Button>();
                            img.raycastTarget = true;
                            btnPause.targetGraphic = img;
                            break;
                        }
                    }
                }
            }

            // 4. Đảm bảo background không chặn raycast
            var bg = GameObject.Find("bg") ?? GameObject.Find("Background");
            if (bg != null)
            {
                var bgImg = bg.GetComponent<Image>();
                if (bgImg != null) bgImg.raycastTarget = false;
            }
        }

        private static Sprite _cachedPlaqueSprite;

        public static Vector2 GetVisualCenter(RectTransform rt)
        {
            if (rt == null) return Vector2.zero;
            Vector2 pivotOffset = new Vector2(0.5f, 0.5f) - rt.pivot;
            return rt.anchoredPosition + new Vector2(pivotOffset.x * rt.sizeDelta.x, pivotOffset.y * rt.sizeDelta.y);
        }

        /// <summary>
        /// Tính khoảng cách Euclid thực tế từ tọa độ nhân vật đến mép ngoài khung tương tác của thiết bị.
        /// Giúp nhân vật tiếp cận thiết bị tự nhiên ở mọi phía mà không bị bắt buộc đâm sát sạt máy mới nhận diện.
        /// </summary>
        public static float GetDistanceToDeviceBounds(Vector2 point, RectTransform rt)
        {
            if (rt == null) return float.MaxValue;
            Vector2 center = GetVisualCenter(rt);
            float w = (rt.rect.width > 0f) ? rt.rect.width : rt.sizeDelta.x;
            float h = (rt.rect.height > 0f) ? rt.rect.height : rt.sizeDelta.y;
            Vector2 halfSize = new Vector2(w * 0.5f, h * 0.5f);

            float dx = Mathf.Max(0f, Mathf.Abs(point.x - center.x) - halfSize.x);
            float dy = Mathf.Max(0f, Mathf.Abs(point.y - center.y) - halfSize.y);
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        public static Sprite GetLessonCardSprite()
        {
            if (_cachedPlaqueSprite != null) return _cachedPlaqueSprite;

            // 1. Tải từ Resources/UI/Card_LessonPlaque
            _cachedPlaqueSprite = Resources.Load<Sprite>("UI/Card_LessonPlaque");
            if (_cachedPlaqueSprite != null) return _cachedPlaqueSprite;

#if UNITY_EDITOR
            // 2. Fallback Editor AssetDatabase từ sprite sheet màn Chọn bài
            string sheetPath = "Assets/Art/Environment/Bộ giao diện chọn bài học học viện phép thuật.png";
            var allAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(sheetPath);
            if (allAssets != null)
            {
                foreach (var obj in allAssets)
                {
                    if (obj is Sprite s && s.name == "Bộ giao diện chọn bài học học viện phép thuật_3")
                    {
                        _cachedPlaqueSprite = s;
                        return s;
                    }
                }
            }
#endif
            return null;
        }

        public static void CleanLegacyStationMarkers(Canvas canvas)
        {
            if (canvas == null) return;

            // 1. Ẩn hoàn toàn tất cả TextMeshPro số trạm (1, 2, 3, 4, 5, 6, 7)
            var allTmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTmps)
            {
                if (tmp == null || tmp.gameObject.name.StartsWith("Plaque_")) continue;
                string txt = tmp.text.Trim();
                if (int.TryParse(txt, out int num) && num >= 1 && num <= 10)
                {
                    tmp.gameObject.SetActive(false);
                }
            }

            // 2. Ẩn hoàn toàn tất cả trạm tròn / nút tròn (sprite kai (4) hoặc size ~87.14 x 70)
            var allImgs = canvas.GetComponentsInChildren<Image>(true);
            foreach (var img in allImgs)
            {
                if (img == null || img.gameObject.name.StartsWith("Plaque_")) continue;
                var rt = img.rectTransform;
                if (rt == null) continue;

                // Loại trừ background, nút quay lại, nút pause, player, hình máy lớn
                if (img.gameObject.name.Contains("bg") || img.gameObject.name.Contains("Background")) continue;
                if (rt.anchoredPosition.x < -450f && rt.anchoredPosition.y < -350f && rt.sizeDelta.x >= 200f) continue;
                if (rt.anchoredPosition.x > 700f && rt.anchoredPosition.y > 350f) continue;
                if (rt.sizeDelta.x >= 150f && rt.sizeDelta.y >= 150f) continue;

                bool isCircleMarker = false;
                if (img.sprite != null && (img.sprite.name.Contains("kai") || img.sprite.name.Contains("Circle") || img.sprite.name.Contains("Station")))
                {
                    isCircleMarker = true;
                }
                else if (rt.sizeDelta.x >= 75f && rt.sizeDelta.x <= 98f && rt.sizeDelta.y >= 60f && rt.sizeDelta.y <= 85f)
                {
                    isCircleMarker = true;
                }

                if (isCircleMarker)
                {
                    img.gameObject.SetActive(false);
                }
            }
        }

        private void SetupStations()
        {
            _stations.Clear();
            var devices = GameDataRepository.GetDevicesForChapter(chapterIndex);
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Dọn dẹp các plaque cũ nếu có (tránh duplicate khi reload hoặc đổi chương)
            var oldPlaques = canvas.GetComponentsInChildren<Transform>(true)
                                   .Where(t => t != null && t.gameObject.name.StartsWith("Plaque_Station_"))
                                   .ToList();
            foreach (var op in oldPlaques)
            {
                if (op != null) Destroy(op.gameObject);
            }

            // Tìm tất cả nhãn số trạm cũ trước khi ẩn để dùng làm tọa độ ghép nối (nếu cần cho các chương khác)
            var numberTexts = canvas.GetComponentsInChildren<TextMeshProUGUI>(true)
                                    .Where(t => int.TryParse(t.text.Trim(), out _))
                                    .OrderBy(t => int.Parse(t.text.Trim()))
                                    .ToList();

            // Tìm các visual device images (máy móc kích thước lớn >= 150x150)
            var deviceImages = canvas.GetComponentsInChildren<Image>(true)
                                     .Where(img => img.rectTransform.sizeDelta.x >= 150f && img.rectTransform.sizeDelta.y >= 150f
                                                && !img.name.Contains("bg") && !img.name.Contains("Background"))
                                     .ToList();

            // Bỏ hoàn toàn các nút tròn/trạm tròn ghi số 1, 2, 3, 4 theo Yêu cầu 2
            CleanLegacyStationMarkers(canvas);

            for (int i = 0; i < devices.Count; i++)
            {
                var dev = devices[i];
                int stationNum = i + 1;

                RectTransform graphicRect = null;
                Image devImg = null;
                Vector2 plaquePos = Vector2.zero;
                Vector2 plaqueSize = new Vector2(370f, 86f);

                if (chapterIndex == 1)
                {
                    // Chương I: Sân Cơ Học (4 thiết bị)
                    plaqueSize = new Vector2(370f, 86f);
                    switch (i)
                    {
                        case 0: // Bài 2: Top-Left máy Image tại (-348, 319)
                            devImg = deviceImages.FirstOrDefault(img => GetVisualCenter(img.rectTransform).x < -150f && GetVisualCenter(img.rectTransform).y > 100f);
                            plaquePos = new Vector2(-348f, 135f);
                            break;
                        case 1: // Bài 3: Bottom-Left máy Image (7) tại (-385, -98)
                            devImg = deviceImages.FirstOrDefault(img => GetVisualCenter(img.rectTransform).x < -150f && GetVisualCenter(img.rectTransform).y < 50f);
                            plaquePos = new Vector2(-385f, -270f);
                            break;
                        case 2: // Bài 4: Bottom-Right máy Image (8) tại (385, -51)
                            devImg = deviceImages.FirstOrDefault(img => GetVisualCenter(img.rectTransform).x > 150f && GetVisualCenter(img.rectTransform).y < 50f);
                            plaquePos = new Vector2(385f, -225f);
                            break;
                        case 3: // Kiểm tra Chương I: Top-Right máy Image (6) tại (343, 286)
                            devImg = deviceImages.FirstOrDefault(img => GetVisualCenter(img.rectTransform).x > 150f && GetVisualCenter(img.rectTransform).y > 100f);
                            plaquePos = new Vector2(349f, 115f);
                            break;
                    }
                    if (devImg != null) graphicRect = devImg.rectTransform;
                }
                else if (chapterIndex == 2)
                {
                    // Chương II: Khu Quang Học (7 thiết bị)
                    switch (i)
                    {
                        case 0: // Bài 5. Khúc xạ ánh sáng: Top-Left máy Image (8) tại (-385, 388)
                            devImg = deviceImages.FirstOrDefault(img => GetVisualCenter(img.rectTransform).x < -200f && GetVisualCenter(img.rectTransform).y > 250f);
                            plaquePos = new Vector2(-385f, 185f);
                            plaqueSize = new Vector2(330f, 78f);
                            break;
                        case 1: // Bài 6. Phản xạ toàn phần: Top-Center máy Image (16) tại (3, 378)
                            devImg = deviceImages.FirstOrDefault(img => Mathf.Abs(GetVisualCenter(img.rectTransform).x) < 150f && GetVisualCenter(img.rectTransform).y > 250f);
                            plaquePos = new Vector2(3f, 205f);
                            plaqueSize = new Vector2(330f, 78f);
                            break;
                        case 2: // Bài 7. Lăng kính: Top-Right máy Image (17) tại (400, 354)
                            devImg = deviceImages.FirstOrDefault(img => GetVisualCenter(img.rectTransform).x > 250f && GetVisualCenter(img.rectTransform).y > 250f);
                            plaquePos = new Vector2(400f, 185f);
                            plaqueSize = new Vector2(330f, 78f);
                            break;
                        case 3: // Bài 8. Thấu kính: Left-Upper mirror Image (21) tại (-531, 138)
                            devImg = deviceImages.FirstOrDefault(img => GetVisualCenter(img.rectTransform).x < -400f && GetVisualCenter(img.rectTransform).y > 0f && GetVisualCenter(img.rectTransform).y < 250f);
                            plaquePos = new Vector2(-530f, -50f);
                            plaqueSize = new Vector2(320f, 76f);
                            break;
                        case 4: // Bài 9. Thực hành đo tiêu cự: Right-Middle optical bench Image (18) tại (536, 83)
                            devImg = deviceImages.FirstOrDefault(img => GetVisualCenter(img.rectTransform).x > 400f && GetVisualCenter(img.rectTransform).y > -150f && GetVisualCenter(img.rectTransform).y < 200f);
                            plaquePos = new Vector2(535f, -90f);
                            plaqueSize = new Vector2(340f, 78f);
                            break;
                        case 5: // Bài 10. Kính lúp. Bài tập thấu kính: Left-Lower mirror Image (20) tại (-288, -139)
                            devImg = deviceImages.FirstOrDefault(img => GetVisualCenter(img.rectTransform).x > -400f && GetVisualCenter(img.rectTransform).x < -100f && GetVisualCenter(img.rectTransform).y < 0f);
                            plaquePos = new Vector2(-290f, -295f);
                            plaqueSize = new Vector2(320f, 76f);
                            break;
                        case 6: // Kiểm tra Chương II: Bottom-Right crystal lantern Image (19) tại (301, -153)
                            devImg = deviceImages.FirstOrDefault(img => GetVisualCenter(img.rectTransform).x > 150f && GetVisualCenter(img.rectTransform).x < 400f && GetVisualCenter(img.rectTransform).y < 0f);
                            plaquePos = new Vector2(300f, -300f);
                            plaqueSize = new Vector2(320f, 76f);
                            break;
                    }
                    if (devImg != null) graphicRect = devImg.rectTransform;
                }
                else if (chapterIndex == 3)
                {
                    // CHƯƠNG III - GIỮ NGUYÊN 100% TUYỆT ĐỐI KHÔNG SỬA ĐỔI
                    switch (i)
                    {
                        case 0: // Bài 11: Top-Left máy Image (6) tại (-352, 253)
                            devImg = deviceImages.FirstOrDefault(img => img.rectTransform.anchoredPosition.x < 0 && img.rectTransform.anchoredPosition.y > 0);
                            plaquePos = new Vector2(-352f, 78f);
                            plaqueSize = new Vector2(370f, 86f);
                            break;
                        case 1: // Bài 12: Top-Right máy Image (7) tại (349, 261)
                            devImg = deviceImages.FirstOrDefault(img => img.rectTransform.anchoredPosition.x > 0 && img.rectTransform.anchoredPosition.y > 0);
                            plaquePos = new Vector2(349f, 86f);
                            plaqueSize = new Vector2(370f, 86f);
                            break;
                        case 2: // Bài 13: Bottom-Left máy Image (16) tại (-377, -125)
                            devImg = deviceImages.FirstOrDefault(img => img.rectTransform.anchoredPosition.x < 0 && img.rectTransform.anchoredPosition.y < 0);
                            plaquePos = new Vector2(-377f, -286f);
                            plaqueSize = new Vector2(370f, 86f);
                            break;
                        case 3: // Kiểm tra Chương III: Bottom-Right máy Image (8) tại (370, -92)
                            devImg = deviceImages.FirstOrDefault(img => img.rectTransform.anchoredPosition.x > 0 && img.rectTransform.anchoredPosition.y < 0);
                            plaquePos = new Vector2(370f, -267f);
                            plaqueSize = new Vector2(370f, 86f);
                            break;
                    }
                    if (devImg != null) graphicRect = devImg.rectTransform;
                }
                else if (chapterIndex == 4)
                {
                    // Chương IV: Tháp Cảm Ứng (3 thiết bị)
                    switch (i)
                    {
                        case 0: // Bài 14: Top máy Image (6) tại (0, 318)
                            devImg = deviceImages.FirstOrDefault(img => GetVisualCenter(img.rectTransform).y > 150f);
                            plaquePos = new Vector2(0f, 135f);
                            plaqueSize = new Vector2(390f, 88f);
                            break;
                        case 1: // Bài 15: Bottom-Right máy Image (7) tại (405, -23)
                            devImg = deviceImages.FirstOrDefault(img => GetVisualCenter(img.rectTransform).x > 150f);
                            plaquePos = new Vector2(405f, -205f);
                            plaqueSize = new Vector2(380f, 86f);
                            break;
                        case 2: // Kiểm tra Chương IV: Bottom-Left máy Image (16) tại (-394, -57)
                            devImg = deviceImages.FirstOrDefault(img => GetVisualCenter(img.rectTransform).x < -150f);
                            plaquePos = new Vector2(-395f, -235f);
                            plaqueSize = new Vector2(370f, 86f);
                            break;
                    }
                    if (devImg != null) graphicRect = devImg.rectTransform;
                }
                else if (chapterIndex == 5)
                {
                    // Chương V: Vườn Năng Lượng (3 thiết bị)
                    switch (i)
                    {
                        case 0: // Bài 16: Top máy Image (6) tại (5, 314)
                            devImg = deviceImages.FirstOrDefault(img => GetVisualCenter(img.rectTransform).y > 150f);
                            plaquePos = new Vector2(5f, 145f);
                            plaqueSize = new Vector2(390f, 88f);
                            break;
                        case 1: // Bài 17: Bottom-Right máy Image (7) tại (396, -37)
                            devImg = deviceImages.FirstOrDefault(img => GetVisualCenter(img.rectTransform).x > 150f);
                            plaquePos = new Vector2(396f, -215f);
                            plaqueSize = new Vector2(380f, 86f);
                            break;
                        case 2: // Kiểm tra Chương V: Bottom-Left máy Image (16) tại (-366, -42)
                            devImg = deviceImages.FirstOrDefault(img => GetVisualCenter(img.rectTransform).x < -150f);
                            plaquePos = new Vector2(-366f, -245f);
                            plaqueSize = new Vector2(370f, 86f);
                            break;
                    }
                    if (devImg != null) graphicRect = devImg.rectTransform;
                }
                else
                {
                    // Fallback tổng quát cho các màn chơi thử nghiệm
                    if (i < deviceImages.Count) devImg = deviceImages[i];
                    if (devImg != null)
                    {
                        graphicRect = devImg.rectTransform;
                        Vector2 vc = GetVisualCenter(graphicRect);
                        float posX = Mathf.Clamp(vc.x, -680f, 680f);
                        float posY = vc.y;
                        float halfH = graphicRect.sizeDelta.y * 0.5f;
                        float targetY = (posY - halfH - 46f >= -360f) ? (posY - halfH - 46f) : (posY + halfH + 46f);
                        plaquePos = new Vector2(posX, Mathf.Clamp(targetY, -350f, 440f));
                        plaqueSize = new Vector2(370f, 86f);
                    }
                }

                RectTransform sRect = graphicRect;

                // Gắn Button listener và viền sáng Outline cho máy móc thiết bị
                Outline stationOutline = null;
                if (graphicRect != null)
                {
                    var gImg = graphicRect.GetComponent<Image>();
                    if (gImg != null) gImg.raycastTarget = true;

                    var gBtn = graphicRect.GetComponent<Button>() ?? graphicRect.gameObject.AddComponent<Button>();
                    int capturedIndex = stationNum;
                    string capturedLessonTitle = dev.TenBai;
                    gBtn.onClick.RemoveAllListeners();
                    gBtn.onClick.AddListener(() => OnStationClicked(capturedIndex));

                    UIHoverPulseEffect.Attach(graphicRect.gameObject, hover: 1.05f, minPulse: 1.02f, maxPulse: 1.06f, outline: true);
                    AttachStationHover(graphicRect.gameObject, capturedLessonTitle);

                    stationOutline = graphicRect.GetComponent<Outline>() ?? graphicRect.gameObject.AddComponent<Outline>();
                    stationOutline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f); // Aurelia Gold
                    stationOutline.effectDistance = new Vector2(3f, -3f);
                    stationOutline.enabled = false;
                }

                var stInfo = new MapStationInfo
                {
                    stationIndex = stationNum,
                    deviceId = dev.DeviceId,
                    baiId = dev.BaiID,
                    lessonTitle = dev.TenBai,
                    stationRect = sRect,
                    deviceGraphicRect = graphicRect,
                    numberText = null,
                    stationImage = devImg,
                    outlineEffect = stationOutline,
                    isRestored = RestorationService.IsDeviceRestored(dev.DeviceId)
                };

                // Tạo biển tên bài học phong cách màn Chọn bài (Navy + Gold Ornate)
                CreateStationNamePlaque(canvas.transform, stInfo, dev, plaquePos, plaqueSize);
                _stations.Add(stInfo);
            }

            // Đảm bảo player, interaction prompt popup và pause modal luôn nằm trên các biển tên
            if (playerRect != null) playerRect.SetAsLastSibling();
            if (_promptPopup != null) _promptPopup.transform.SetAsLastSibling();
            if (_pauseModal != null) _pauseModal.transform.SetAsLastSibling();
        }

        private string FormatPlaqueTitle(string rawTitle, bool isRestored = false)
        {
            if (string.IsNullOrEmpty(rawTitle)) return "";

            string formatted = rawTitle;

            // Rút gọn đề test tổng hợp thành "Kiểm tra Chương ..." theo quy chuẩn mẫu
            if (rawTitle.StartsWith("Đề TEST", StringComparison.OrdinalIgnoreCase) || rawTitle.Contains("TEST", StringComparison.OrdinalIgnoreCase))
            {
                if (rawTitle.Contains("Chương I.") || rawTitle.Contains("Chương I ") || rawTitle.EndsWith("Chương I") || rawTitle.Contains("Chương 1")) formatted = "Kiểm tra Chương I";
                else if (rawTitle.Contains("Chương II.") || rawTitle.Contains("Chương II ") || rawTitle.EndsWith("Chương II") || rawTitle.Contains("Chương 2")) formatted = "Kiểm tra Chương II";
                else if (rawTitle.Contains("Chương III.") || rawTitle.Contains("Chương III ") || rawTitle.EndsWith("Chương III") || rawTitle.Contains("Chương 3")) formatted = "Kiểm tra Chương III";
                else if (rawTitle.Contains("Chương IV.") || rawTitle.Contains("Chương IV ")) formatted = "Kiểm tra Chương IV";
                else if (rawTitle.Contains("Chương V.") || rawTitle.Contains("Chương V ")) formatted = "Kiểm tra Chương V";
                else formatted = "Kiểm tra Chương";
            }
            else if (rawTitle.Contains("Bài 14"))
            {
                formatted = "Bài 14. Cảm ứng điện từ\nNguyên tắc tạo ra dòng điện xoay chiều";
            }
            else if (rawTitle.Contains("Bài 15"))
            {
                formatted = "Bài 15. Tác dụng của\ndòng điện xoay chiều";
            }
            else if (rawTitle.Contains("Bài 16"))
            {
                formatted = "Bài 16. Vòng năng lượng trên Trái Đất\nNăng lượng hoá thạch";
            }
            else if (rawTitle.Contains("Bài 17"))
            {
                formatted = "Bài 17. Một số dạng\nnăng lượng tái tạo";
            }
            else if (rawTitle.Contains("Bài 9.") && rawTitle.Contains("tiêu cự"))
            {
                formatted = "Bài 9. Thực hành đo tiêu cự\ncủa thấu kính hội tụ";
            }
            else if (rawTitle.Contains("Bài 10.") && rawTitle.Contains("Kính lúp"))
            {
                formatted = "Bài 10. Kính lúp\nBài tập thấu kính";
            }

            if (isRestored)
            {
                formatted = "<color=#40FFB0><b>✓</b></color> " + formatted;
            }

            return formatted;
        }

        private void CreateStationNamePlaque(Transform parent, MapStationInfo st, G2DeviceItem dev, Vector2 basePos, Vector2 customSize)
        {
            var plaqueGo = new GameObject($"Plaque_Station_{st.stationIndex}");
            plaqueGo.transform.SetParent(parent, false);

            var rt = plaqueGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            // Giới hạn tọa độ trong khoảng nhìn thấy tốt, không che UI viền hay HUD
            float posX = Mathf.Clamp(basePos.x, -720f, 720f);
            float posY = Mathf.Clamp(basePos.y, -370f, 470f);
            rt.anchoredPosition = new Vector2(posX, posY);
            rt.sizeDelta = customSize;

            // 1. Nền Navy fantasy & Viền hoa văn vàng lấy từ màn Chọn bài (Card_LessonPlaque)
            var bgImg = plaqueGo.AddComponent<Image>();
            Sprite cardSprite = GetLessonCardSprite();
            if (cardSprite != null)
            {
                bgImg.sprite = cardSprite;
                bgImg.type = Image.Type.Sliced;
                bgImg.color = Color.white;
            }
            else
            {
                bgImg.color = new Color(0.06f, 0.12f, 0.28f, 0.95f);
            }
            bgImg.raycastTarget = true;

            // 2. Viền phát sáng fantasy Aurelia
            var outline = plaqueGo.AddComponent<Outline>();
            outline.effectColor = st.isRestored
                ? new Color(0.35f, 0.95f, 0.65f, 0.85f) // Xanh ngọc lục bảo khôi phục
                : new Color(0.96f, 0.82f, 0.38f, 0.65f); // Vàng kim Aurelia
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            // 3. Cho phép click trực tiếp vào biển tên để vào học bài đó ngay lập tức
            var btn = plaqueGo.AddComponent<Button>();
            btn.targetGraphic = bgImg;
            int capturedIndex = st.stationIndex;
            string capturedTitle = dev.TenBai;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnStationClicked(capturedIndex));

            // Hiệu ứng hover pulse mượt mà (tối đa 1.04x theo yêu cầu)
            var pulse = UIHoverPulseEffect.Attach(plaqueGo, hover: 1.04f, minPulse: 1.01f, maxPulse: 1.04f, outline: true);
            AttachStationHover(plaqueGo, capturedTitle);

            // 4. Tên bài học dạng TextMeshPro chữ lớn rõ ràng (20–22pt), in đậm màu trắng kem, tối đa 2 dòng
            var titleGo = new GameObject("LessonTitle");
            titleGo.transform.SetParent(plaqueGo.transform, false);
            var tRt = titleGo.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.pivot = new Vector2(0.5f, 0.5f);
            tRt.offsetMin = new Vector2(18f, 8f);
            tRt.offsetMax = new Vector2(-18f, -8f);

            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = FormatPlaqueTitle(dev.TenBai, st.isRestored);
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.fontSize = 21f;
            titleTmp.enableAutoSizing = true;
            titleTmp.fontSizeMin = 14f;
            titleTmp.fontSizeMax = 21f;
            titleTmp.lineSpacing = -8f;
            titleTmp.characterSpacing = 0.2f;
            titleTmp.textWrappingMode = TextWrappingModes.Normal;
            titleTmp.overflowMode = TextOverflowModes.Ellipsis;
            titleTmp.maxVisibleLines = 2;
            titleTmp.raycastTarget = false;

            // Màu chữ: Trắng kem sang trọng cho bài học, Vàng ánh kim cho Đề kiểm tra
            bool isTest = dev.LoaiBai == "CHAPTER_TEST" || dev.BaiID.StartsWith("test_", StringComparison.OrdinalIgnoreCase);
            titleTmp.color = isTest ? new Color(1f, 0.94f, 0.68f, 1f) : new Color(1f, 0.98f, 0.92f, 1f);

            // Thêm Shadow sắc nét cho chữ để nổi bật tuyệt đối trên Game View Scale 0.57x
            var shadow = titleGo.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);

            // Lưu tham chiếu vào MapStationInfo
            st.plaqueGo = plaqueGo;
            st.plaqueRect = rt;
            st.plaqueBg = bgImg;
            st.plaqueOutline = outline;
            st.plaqueTitleText = titleTmp;
            st.plaquePulse = pulse;
        }

        private void SetupBackButton()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. Nếu btnBack chưa được gán, tìm Image nền ở góc dưới bên trái (x < -450, y < -350, width >= 200)
            if (btnBack == null)
            {
                foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                {
                    var rt = img.rectTransform;
                    if (rt.anchoredPosition.x < -450f && rt.anchoredPosition.y < -350f && rt.sizeDelta.x >= 200f)
                    {
                        btnBack = img.GetComponent<Button>() ?? img.gameObject.AddComponent<Button>();
                        img.raycastTarget = true;
                        btnBack.targetGraphic = img;
                        break;
                    }
                }
            }

            if (btnBack != null)
            {
                var bImg = btnBack.GetComponent<Image>();
                if (bImg != null)
                {
                    bImg.raycastTarget = true;
                    btnBack.targetGraphic = bImg;
                }
                UIHoverPulseEffect.Attach(btnBack.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackClicked);
                Debug.Log($"[ChapterMapController] Đã gắn sự kiện cho nút Chọn chương (btnBack: {btnBack.name}).");
            }
            else
            {
                Debug.LogWarning("[ChapterMapController] Không tìm thấy Image nền btnBack cho nút Chọn chương!");
            }

            // 2. Tìm Text "Chọn chương" và icon mũi tên lân cận:
            // Đảm bảo cả Text và Icon đều có thể nhận click hoặc chuyển sự kiện click về OnBackClicked
            foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                string txt = tmp.text.Trim();
                if (txt.Contains("Chọn chương") || txt.Contains("Chon chuong") ||
                    (tmp.rectTransform.anchoredPosition.x < -450f && tmp.rectTransform.anchoredPosition.y < -350f))
                {
                    tmp.raycastTarget = true;
                    var tBtn = tmp.GetComponent<Button>() ?? tmp.gameObject.AddComponent<Button>();
                    tBtn.onClick.RemoveAllListeners();
                    tBtn.onClick.AddListener(OnBackClicked);
                    Debug.Log("[ChapterMapController] Đã gắn sự kiện click cho Text 'Chọn chương'.");
                }
            }

            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
            {
                var rt = img.rectTransform;
                if (rt.anchoredPosition.x < -450f && rt.anchoredPosition.y < -350f)
                {
                    if (btnBack == null || img.gameObject != btnBack.gameObject)
                    {
                        img.raycastTarget = true;
                        var iBtn = img.GetComponent<Button>() ?? img.gameObject.AddComponent<Button>();
                        iBtn.onClick.RemoveAllListeners();
                        iBtn.onClick.AddListener(OnBackClicked);
                        Debug.Log($"[ChapterMapController] Đã gắn sự kiện click cho icon/graphic phụ '{img.gameObject.name}'.");
                    }
                }
            }

            // 3. Đưa toàn bộ cụm nút Chọn chương lên trên cùng Canvas để không bị bất kỳ layer/vignette nào che khuất raycast
            if (btnBack != null)
            {
                btnBack.transform.SetAsLastSibling();
                foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                {
                    var rt = img.rectTransform;
                    if (img.gameObject != btnBack.gameObject && rt.anchoredPosition.x < -450f && rt.anchoredPosition.y < -350f)
                    {
                        img.transform.SetAsLastSibling();
                    }
                }
                foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    if (tmp.rectTransform.anchoredPosition.x < -450f && tmp.rectTransform.anchoredPosition.y < -350f)
                    {
                        tmp.transform.SetAsLastSibling();
                    }
                }
            }
        }

        private void SetupPauseButton()
        {
            if (btnPause != null)
            {
                UIHoverPulseEffect.Attach(btnPause.gameObject, hover: 1.1f, minPulse: 1.05f, maxPulse: 1.12f, outline: true);
                btnPause.onClick.RemoveAllListeners();
                btnPause.onClick.AddListener(TogglePause);
                Debug.Log("[ChapterMapController] Đã gắn sự kiện cho nút Pause.");
            }
        }

        private void ApplyPlayerPositionAndSprite()
        {
            if (playerRect == null) return;

            // 1. Phục hồi vị trí người chơi cạnh trạm nếu quay về từ bài thi
            if (NavigationService.Instance.ReturnContext.PlayerPosition != Vector2.zero &&
                NavigationService.Instance.ReturnContext.ChapterId == chapterIndex)
            {
                playerRect.anchoredPosition = NavigationService.Instance.ReturnContext.PlayerPosition;
                Debug.Log($"[ChapterMapController] Khôi phục vị trí người chơi cạnh trạm: {playerRect.anchoredPosition}");
            }
            else if (!string.IsNullOrEmpty(NavigationService.Instance.Selection.LessonId))
            {
                // Nếu người chơi chọn một bài cụ thể từ LessonSelection, định vị nhân vật đứng gần trạm đó
                var targetStation = _stations.FirstOrDefault(s => s.baiId.Equals(NavigationService.Instance.Selection.LessonId, StringComparison.OrdinalIgnoreCase));
                if (targetStation != null && targetStation.stationRect != null)
                {
                    playerRect.anchoredPosition = targetStation.stationRect.anchoredPosition + new Vector2(0f, -80f);
                    Debug.Log($"[ChapterMapController] Định vị nhân vật cạnh trạm {targetStation.stationIndex} ({targetStation.lessonTitle}) theo bài đã chọn: {playerRect.anchoredPosition}");
                }
            }

            // 2. Đổi sprite người chơi theo đúng SelectedCharacterId (Minh, Lan, Duy, Hà)
            string charId = GetSelectedCharacterId();
            Debug.Log($"[ChapterMapController] Áp dụng sprite cho nhân vật đã chọn: {charId}");

            Sprite charSprite = LoadCharacterSprite(charId);
            if (charSprite != null && playerImage != null)
            {
                playerImage.sprite = charSprite;
                playerImage.preserveAspect = true;
                playerImage.color = Color.white;
            }
        }

        private string GetSelectedCharacterId()
        {
            if (PhysicsGameSession.Instance != null && !string.IsNullOrEmpty(PhysicsGameSession.Instance.SelectedCharacterId))
            {
                return PhysicsGameSession.Instance.SelectedCharacterId;
            }
            if (NavigationService.Instance != null && !string.IsNullOrEmpty(NavigationService.Instance.GameContext.CharacterId))
            {
                return NavigationService.Instance.GameContext.CharacterId;
            }
            var save = PhysicsSaveManager.Load();
            if (save != null && !string.IsNullOrEmpty(save.selectedCharacterId))
            {
                return save.selectedCharacterId;
            }
            return "Minh";
        }

        private Sprite LoadCharacterSprite(string charId)
        {
            if (string.IsNullOrEmpty(charId)) charId = "Minh";
            if (charId.Equals("Hà", StringComparison.OrdinalIgnoreCase)) charId = "Ha";

            // 1. Thử nạp từ Resources/Characters/
            Sprite sp = Resources.Load<Sprite>($"Characters/{charId}");
            if (sp != null) return sp;

            sp = Resources.Load<Sprite>($"Characters/{charId.ToLowerInvariant()}");
            if (sp != null) return sp;

#if UNITY_EDITOR
            // 2. Fallback Editor AssetDatabase từ sprite sheet gốc
            string sheetPath = "Assets/Art/Environment/Bộ tài sản chọn nhân vật chibi fantasy.png";
            var allAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(sheetPath);
            if (allAssets != null)
            {
                string targetSubName = charId switch
                {
                    "Lan" => "Bộ tài sản chọn nhân vật chibi fantasy_3",
                    "Duy" => "Bộ tài sản chọn nhân vật chibi fantasy_4",
                    "Ha" => "Bộ tài sản chọn nhân vật chibi fantasy_5",
                    _ => "Bộ tài sản chọn nhân vật chibi fantasy_0" // Minh
                };
                foreach (var obj in allAssets)
                {
                    if (obj is Sprite s && s.name == targetSubName)
                    {
                        return s;
                    }
                }
            }
#endif
            return null;
        }

        private void Update()
        {
            HandlePauseInput();
            if (_isPaused) return;

            HandlePlayerMovement();
            CheckProximityToStations();
            HandleInteractionKey();
            UpdateSessionTimer();
        }

        private void HandlePlayerMovement()
        {
            if (playerRect == null) return;

            // Không di chuyển khi InputField đang focus
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                var inputField = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
                if (inputField != null && inputField.isFocused) return;
            }

            float horizontal = 0f;
            float vertical = 0f;

            // 1. Kiểm tra Unity 6 Input System
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) vertical += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) vertical -= 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) horizontal -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal += 1f;
            }
#endif

            // 2. Fallback sang Legacy Input nếu New Input System không bấm
            if (horizontal == 0f && vertical == 0f)
            {
                try
                {
                    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical += 1f;
                    if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
                    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
                    if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
                }
                catch { }
            }

            if (horizontal != 0f || vertical != 0f)
            {
                // Chuẩn hóa vector di chuyển để đi chéo không nhanh hơn đi thẳng
                Vector2 dir = new Vector2(horizontal, vertical).normalized;
                Vector2 newPos = playerRect.anchoredPosition + dir * (moveSpeed * Time.unscaledDeltaTime);

                // Giới hạn trong sàn đi được của bản đồ
                newPos.x = Mathf.Clamp(newPos.x, _minBounds.x, _maxBounds.x);
                newPos.y = Mathf.Clamp(newPos.y, _minBounds.y, _maxBounds.y);

                playerRect.anchoredPosition = newPos;

                // Quay mặt nhân vật theo hướng di chuyển ngang
                if (horizontal != 0f && playerImage != null)
                {
                    Vector3 scale = playerImage.rectTransform.localScale;
                    scale.x = horizontal < 0f ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
                    playerImage.rectTransform.localScale = scale;
                }
            }
        }

        public void CheckProximityToStations()
        {
            if (playerRect == null) return;

            MapStationInfo closest = null;
            float closestDist = float.MaxValue;

            foreach (var st in _stations)
            {
                // Chỉ tính khoảng cách theo vị trí thực tế của nhân vật và vùng tương tác của thiết bị (deviceGraphicRect / stationRect).
                // TUYỆT ĐỐI KHÔNG lấy vị trí biển tên làm vị trí máy!
                RectTransform targetRect = st.deviceGraphicRect ?? st.stationRect;
                if (targetRect == null) continue;

                float dist = GetDistanceToDeviceBounds(playerRect.anchoredPosition, targetRect);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = st;
                }
            }

            if (closest != null && closestDist <= interactionRadius)
            {
                if (_nearestStation != closest)
                {
                    // Tắt highlight trạm cũ
                    if (_nearestStation != null)
                    {
                        if (_nearestStation.outlineEffect != null) _nearestStation.outlineEffect.enabled = false;
                        if (_nearestStation.plaqueOutline != null)
                        {
                            _nearestStation.plaqueOutline.effectColor = _nearestStation.isRestored
                                ? new Color(0.35f, 0.95f, 0.65f, 0.85f)
                                : new Color(0.96f, 0.82f, 0.38f, 0.65f);
                            _nearestStation.plaqueOutline.effectDistance = new Vector2(1.5f, -1.5f);
                        }
                        if (_nearestStation.plaqueRect != null) _nearestStation.plaqueRect.localScale = Vector3.one;
                    }

                    _nearestStation = closest;
                    if (_nearestStation.outlineEffect != null)
                    {
                        _nearestStation.outlineEffect.enabled = true;
                    }
                    if (_nearestStation.plaqueOutline != null)
                    {
                        _nearestStation.plaqueOutline.effectColor = new Color(1f, 0.95f, 0.45f, 1f); // Sáng vàng hoàng gia
                        _nearestStation.plaqueOutline.effectDistance = new Vector2(2.5f, -2.5f);
                    }
                    if (_nearestStation.plaqueRect != null)
                    {
                        _nearestStation.plaqueRect.localScale = new Vector3(1.04f, 1.04f, 1f);
                    }

                    ShowInteractionPrompt(true, "Nhấn [ E ] để học");
                }
            }
            else
            {
                if (_nearestStation != null)
                {
                    if (_nearestStation.outlineEffect != null)
                    {
                        _nearestStation.outlineEffect.enabled = false;
                    }
                    if (_nearestStation.plaqueOutline != null)
                    {
                        _nearestStation.plaqueOutline.effectColor = _nearestStation.isRestored
                            ? new Color(0.35f, 0.95f, 0.65f, 0.85f)
                            : new Color(0.96f, 0.82f, 0.38f, 0.65f);
                        _nearestStation.plaqueOutline.effectDistance = new Vector2(1.5f, -1.5f);
                    }
                    if (_nearestStation.plaqueRect != null)
                    {
                        _nearestStation.plaqueRect.localScale = Vector3.one;
                    }

                    _nearestStation = null;
                    ShowInteractionPrompt(false);
                }
            }
        }

        public void HandleInteractionKey()
        {
            bool ePressed = false;
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame) ePressed = true;
#endif
            if (!ePressed)
            {
                try
                {
                    if (Input.GetKeyDown(KeyCode.E)) ePressed = true;
                }
                catch { }
            }

            if (ePressed)
            {
                // Kiểm tra focus TMP_InputField
                if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
                {
                    var inputField = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
                    if (inputField != null && inputField.isFocused) return;
                }

                if (_nearestStation != null)
                {
                    EnterStationModeSelection(_nearestStation);
                }
                else
                {
                    if (_hintCoroutine != null) StopCoroutine(_hintCoroutine);
                    _hintCoroutine = StartCoroutine(ShowBriefHintRoutine("Hãy đến gần một thiết bị để học"));
                }
            }
        }

        private void HandlePauseInput()
        {
            bool escPressed = false;
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame) escPressed = true;
#endif
            if (!escPressed)
            {
                try
                {
                    if (Input.GetKeyDown(KeyCode.Escape)) escPressed = true;
                }
                catch { }
            }

            if (escPressed)
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            _isPaused = !_isPaused;
            if (_pauseModal != null) _pauseModal.SetActive(_isPaused);
            Debug.Log($"[ChapterMapController] Trạng thái Pause: {_isPaused}");
        }

        public void EnterStationModeSelection(MapStationInfo station)
        {
            if (station == null) return;
            Debug.Log($"[ChapterMapController] Vào học: {station.deviceId} - {station.lessonTitle}");

            // Lưu ReturnContext và Selection
            NavigationService.Instance.Selection.ChapterId = chapterIndex;
            NavigationService.Instance.Selection.DeviceId = station.deviceId;
            NavigationService.Instance.Selection.LessonId = station.baiId;
            NavigationService.Instance.Selection.LessonTitle = station.lessonTitle;

            if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.SelectChapter(chapterIndex);
                PhysicsGameSession.Instance.SelectLesson(station.baiId, station.lessonTitle);
            }

            // Điểm đứng cạnh thiết bị: Lưu chính xác vị trí nhân vật hiện tại khi nhấn E
            Vector2 playerPos = (playerRect != null) ? playerRect.anchoredPosition : Vector2.zero;
            if (playerPos == Vector2.zero && (station.deviceGraphicRect != null || station.stationRect != null))
            {
                playerPos = GetVisualCenter(station.deviceGraphicRect ?? station.stationRect) + new Vector2(0f, -80f);
            }

            NavigationService.Instance.ReturnContext.PlayerPosition = playerPos;
            NavigationService.Instance.ReturnContext.ChapterId = chapterIndex;
            NavigationService.Instance.ReturnContext.DeviceId = station.deviceId;
            NavigationService.Instance.ReturnContext.SelectedLessonId = station.baiId;

            // Chuyển sang màn 17. Type (ModeSelection)
            NavigationService.Instance.NavigateTo(RouteId.ModeSelection);
        }

        private Coroutine _stationPulseCoroutine = null;
        private Coroutine _hintCoroutine = null;

        public void OnStationClicked(int stationIndex)
        {
            var st = _stations.FirstOrDefault(s => s.stationIndex == stationIndex);
            if (st == null) return;

            // TUYỆT ĐỐI LOẠI BỎ MỞ BÀI HỌC BẰNG CLICK CHUỘT (CẢ TỪ XA VÀ TẠI CHỖ).
            // Người chơi bắt buộc phải điều khiển nhân vật đến vùng thiết bị bằng WASD và nhấn phím E.
            // Click chuột từ xa hoặc tại chỗ KHÔNG chuyển Scene, chỉ báo hiệu mục tiêu.

            // 1. Phát hiệu ứng nhấp nháy / phát sáng nhẹ vào máy và biển tên để báo hiệu mục tiêu
            TriggerStationPulseHint(st);

            // 2. Kiểm tra vị trí người chơi:
            if (_nearestStation == st)
            {
                // Đã đứng gần máy này: nhắc người chơi nhấn phím E
                ShowInteractionPrompt(true, "Nhấn [ E ] để học");
            }
            else
            {
                // Đứng xa máy này: hiển thị thông báo ngắn hướng dẫn di chuyển
                if (_hintCoroutine != null) StopCoroutine(_hintCoroutine);
                _hintCoroutine = StartCoroutine(ShowBriefHintRoutine("Hãy di chuyển đến thiết bị và nhấn E"));
            }
        }

        private void TriggerStationPulseHint(MapStationInfo st)
        {
            if (st == null) return;
            if (_stationPulseCoroutine != null) StopCoroutine(_stationPulseCoroutine);
            _stationPulseCoroutine = StartCoroutine(StationPulseHintRoutine(st));
        }

        private IEnumerator StationPulseHintRoutine(MapStationInfo targetStation)
        {
            if (targetStation == null) yield break;

            // Bật outline sáng cho máy móc để chỉ rõ vị trí mục tiêu
            if (targetStation.outlineEffect != null)
            {
                targetStation.outlineEffect.enabled = true;
                targetStation.outlineEffect.effectColor = new Color(1f, 0.95f, 0.45f, 1f); // Vàng kim rực rỡ
                targetStation.outlineEffect.effectDistance = new Vector2(4f, -4f);
            }

            // Nhấp nháy nhẹ biển tên
            if (targetStation.plaqueRect != null)
            {
                targetStation.plaqueRect.localScale = new Vector3(1.06f, 1.06f, 1f);
            }

            yield return new WaitForSeconds(1.2f);

            // Phục hồi lại trạng thái nếu người chơi chưa tới gần máy này
            if (_nearestStation != targetStation)
            {
                if (targetStation.outlineEffect != null)
                {
                    targetStation.outlineEffect.enabled = false;
                    targetStation.outlineEffect.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
                    targetStation.outlineEffect.effectDistance = new Vector2(3f, -3f);
                }
                if (targetStation.plaqueRect != null)
                {
                    targetStation.plaqueRect.localScale = Vector3.one;
                }
            }
            else
            {
                // Nếu người chơi đã đứng gần máy này, giữ visual của _nearestStation
                if (targetStation.plaqueRect != null)
                {
                    targetStation.plaqueRect.localScale = new Vector3(1.04f, 1.04f, 1f);
                }
            }
        }

        public void RefreshRestorationVisuals()
        {
            int restoredCount = RestorationService.GetRestoredCountForChapter(chapterIndex);
            int totalCount = RestorationService.GetTotalDeviceCountForChapter(chapterIndex);

            if (txtProgress != null)
            {
                txtProgress.text = $"Đã sáng: {restoredCount}/{totalCount}";
            }

            // Cập nhật trạng thái sáng cho từng trạm độc lập
            foreach (var st in _stations)
            {
                bool isRestored = RestorationService.IsDeviceRestored(st.deviceId);
                st.isRestored = isRestored;

                // Cập nhật trạng thái hiển thị trên biển tên bài học
                if (st.plaqueGo != null)
                {
                    if (st.plaqueTitleText != null)
                    {
                        st.plaqueTitleText.text = FormatPlaqueTitle(st.lessonTitle, isRestored);
                    }
                    if (st.plaqueOutline != null)
                    {
                        st.plaqueOutline.effectColor = isRestored
                            ? new Color(0.35f, 0.95f, 0.65f, 0.85f)
                            : new Color(0.96f, 0.82f, 0.38f, 0.65f);
                    }
                }
            }
        }

        private void UpdateSessionTimer()
        {
            var session = PhysicsGameSession.Instance;
            if (session == null || session.SaveData == null || session.SaveData.activeStudySession == null) return;

            var active = session.SaveData.activeStudySession;
            if (!active.isActive) return;

            float remaining = Mathf.Max(0f, active.plannedMinutes * 60f - active.activeSeconds);
            int mins = Mathf.FloorToInt(remaining / 60f);
            int secs = Mathf.FloorToInt(remaining % 60f);

            if (txtTimer != null)
            {
                txtTimer.text = $"{mins:00}:{secs:00}";
            }
        }

        private void UpdateSessionTimerDisplay()
        {
            UpdateSessionTimer();
        }

        private void OnBackClicked()
        {
            Time.timeScale = 1f;
            _isTransitioning = false;

            Debug.Log("[ChapterMapController] Bấm nút Chọn chương -> Chuyển về 04_ChapterSelection.");
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.ResetTransitionLock();
                NavigationService.Instance.NavigateTo(RouteId.ChapterSelection);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(PhysicsSceneNames.ChapterSelection);
            }
        }

        private void OnDisable()
        {
            _isTransitioning = false;
        }

        private void AttachStationHover(GameObject targetGo, string lessonTitle)
        {
            if (targetGo == null) return;
            var trigger = targetGo.GetComponent<StationHoverTrigger>() ?? targetGo.AddComponent<StationHoverTrigger>();
            trigger.onHoverEnter = () =>
            {
                // Chỉ hiển thị "Nhấn [ E ] để học" nếu người chơi đã thực sự di chuyển đến gần thiết bị
                if (_nearestStation != null)
                {
                    ShowInteractionPrompt(true, "Nhấn [ E ] để học");
                }
            };
            trigger.onHoverExit = () =>
            {
                if (_nearestStation == null)
                {
                    ShowInteractionPrompt(false);
                }
                else
                {
                    ShowInteractionPrompt(true, "Nhấn [ E ] để học");
                }
            };
        }

        private void CreatePromptUI()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Thanh thông báo ngắn gọn "Nhấn [ E ] để học" nằm gọn gàng giữa 2 máy dưới, không chạm biển tên hay thanh WASD
            _promptPopup = new GameObject("InteractionPromptPanel");
            _promptPopup.transform.SetParent(canvas.transform, false);
            var rt = _promptPopup.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, -345f);
            rt.sizeDelta = new Vector2(340f, 46f);

            // Nền xanh navy đậm viền vàng fantasy từ Card_LessonPlaque
            var img = _promptPopup.AddComponent<Image>();
            Sprite cardSprite = GetLessonCardSprite();
            if (cardSprite != null)
            {
                img.sprite = cardSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.06f, 0.12f, 0.28f, 0.95f);
            }
            img.raycastTarget = false;

            var outline = _promptPopup.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 0.75f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var tGo = new GameObject("Text");
            tGo.transform.SetParent(_promptPopup.transform, false);
            var tRt = tGo.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = new Vector2(12f, 0f);
            tRt.offsetMax = new Vector2(-12f, 0f);

            _promptText = tGo.AddComponent<TextMeshProUGUI>();
            _promptText.text = "Nhấn [ E ] để học";
            _promptText.fontSize = 18f;
            _promptText.enableAutoSizing = true;
            _promptText.fontSizeMin = 14f;
            _promptText.fontSizeMax = 18f;
            _promptText.fontStyle = FontStyles.Bold;
            _promptText.alignment = TextAlignmentOptions.Center;
            _promptText.color = new Color(1f, 0.98f, 0.92f, 1f); // Trắng kem
            _promptText.raycastTarget = false;

            var shadow = tGo.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);

            _promptPopup.SetActive(false);
        }

        private void ShowInteractionPrompt(bool show, string text = "")
        {
            if (_promptPopup != null)
            {
                _promptPopup.SetActive(show);
                if (show && _promptText != null) _promptText.text = text;
            }
        }

        private IEnumerator ShowBriefHintRoutine(string msg)
        {
            ShowInteractionPrompt(true, msg);
            yield return new WaitForSeconds(2.5f);
            if (_nearestStation == null)
            {
                ShowInteractionPrompt(false);
            }
            else
            {
                ShowInteractionPrompt(true, "Nhấn [ E ] để học");
            }
            _hintCoroutine = null;
        }

        private void CreatePauseModal()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            _pauseModal = new GameObject("MapPauseModal");
            _pauseModal.transform.SetParent(canvas.transform, false);
            _pauseModal.transform.SetAsLastSibling();

            var rt = _pauseModal.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            var bg = _pauseModal.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.08f, 0.18f, 0.85f);

            var panel = new GameObject("Panel");
            panel.transform.SetParent(_pauseModal.transform, false);
            var pRt = panel.AddComponent<RectTransform>();
            pRt.sizeDelta = new Vector2(460f, 380f);
            pRt.anchoredPosition = Vector2.zero;

            var pImg = panel.AddComponent<Image>();
            pImg.color = new Color(0.08f, 0.14f, 0.28f, 0.98f);

            var outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);

            // Title
            CreateModalText(panel.transform, "TẠM DỪNG BUỔI HỌC", 24f, new Vector2(0f, 130f), new Vector2(400f, 50f));

            // 3 Buttons: Tiếp tục, Cài đặt, Kết thúc buổi
            CreateModalButton(panel.transform, "TIẾP TỤC", new Vector2(0f, 40f), () => TogglePause());
            CreateModalButton(panel.transform, "CÀI ĐẶT", new Vector2(0f, -30f), () =>
            {
                TogglePause();
                NavigationService.Instance.NavigateTo(RouteId.Settings);
            });
            CreateModalButton(panel.transform, "KẾT THÚC BUỔI HỌC", new Vector2(0f, -100f), () =>
            {
                TogglePause();
                if (PhysicsGameSession.Instance != null) PhysicsGameSession.Instance.FinishStudySession();
                NavigationService.Instance.NavigateTo(RouteId.MainMenu);
            });

            _pauseModal.SetActive(false);
        }

        private void CreateModalText(Transform parent, string text, float size, Vector2 pos, Vector2 delta)
        {
            var go = new GameObject("Title");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = delta;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.96f, 0.82f, 0.38f, 1f);
            tmp.raycastTarget = false;
        }

        private void CreateModalButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
        {
            var btnGo = new GameObject("Btn_" + label);
            btnGo.transform.SetParent(parent, false);
            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(280f, 46f);

            var img = btnGo.AddComponent<Image>();
            img.color = new Color(0.12f, 0.22f, 0.44f, 1f);

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(action);

            var tGo = new GameObject("Text");
            tGo.transform.SetParent(btnGo.transform, false);
            var tRt = tGo.AddComponent<RectTransform>();
            tRt.sizeDelta = rt.sizeDelta;

            var tmp = tGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 17f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
        }
    }

    /// <summary>
    /// Component bắt sự kiện rê chuột (hover) để hiển thị tooltip tên bài học cho trạm thiết bị trên bản đồ.
    /// </summary>
    public class StationHoverTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Action onHoverEnter;
        public Action onHoverExit;

        public void OnPointerEnter(PointerEventData eventData) => onHoverEnter?.Invoke();
        public void OnPointerExit(PointerEventData eventData) => onHoverExit?.Invoke();
    }
}
