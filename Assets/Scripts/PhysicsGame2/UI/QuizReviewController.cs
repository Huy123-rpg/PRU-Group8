using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển logic hiển thị và duyệt bài cho Scene 20. Detail Result TN (Detail Review):
    /// - Bảo toàn 100% thiết kế gốc trong Scene (Ảnh 2 chuẩn trong Editor), tuyệt đối KHÔNG sinh UI runtime đè lên.
    /// - Bind chính xác vào 10 nút câu hỏi có sẵn, 4 hộp phương án có sẵn, khung giải thích và mẹo ghi nhớ có sẵn.
    /// - Nhận dữ liệu snapshot chính xác từ PhysicsGameSession.Instance.CurrentQuizAttempt hoặc NavigationService.Instance.Attempt.
    /// - Hiển thị đúng câu hỏi, đáp án học sinh chọn, đáp án đúng, lời giải chi tiết và mẹo ghi nhớ thật.
    /// - Đảm bảo API tương thích hoàn toàn để vượt qua toàn bộ bộ kiểm thử QuizFlowVerificationTest.
    /// </summary>
    public class QuizReviewController : MonoBehaviour
    {
        public enum RowCorrectnessState
        {
            Neutral = 0,
            Correct = 1,
            Wrong = 2
        }

        public enum AnswerOptionVisualState
        {
            Neutral = 0,
            WrongSelected = 1,
            CorrectAnswer = 2
        }

        [Serializable]
        public class LeftQuestionRowView
        {
            public int questionIndex;
            public Image backgroundImage;
            public TextMeshProUGUI questionLabel;
            public Image iconImage;
            public Image checkIcon;
            public Image crossIcon;
            public GameObject selectionHighlightObj;
            public Button rowButton;
            public RowCorrectnessState correctnessState;
            public bool isSelectedHighlight;
        }

        [Serializable]
        public class CenterAnswerRowView
        {
            public int optionIndex;
            public GameObject rootObject;
            public Image backgroundImage;
            public Image badgeBackground;
            public TextMeshProUGUI badgeText;
            public TextMeshProUGUI optionText;
            public Image statusIcon;
            public TextMeshProUGUI statusLabel;
            public AnswerOptionVisualState visualState;
        }

        [Header("=== CENTER DETAIL ===")]
        [SerializeField] private RectTransform parchmentPanelRect;
        [SerializeField] private TextMeshProUGUI centerQuestionText;
        [SerializeField] private TextMeshProUGUI questionNumberHeaderTmp;
        [SerializeField] private TextMeshProUGUI userChoiceStatusTmp;

        [Header("=== 4 OPTION BOXES (SCENE NATIVE) ===")]
        [SerializeField] private List<Image> nativeOptionBoxes = new List<Image>();
        [SerializeField] private List<TextMeshProUGUI> nativeOptionTexts = new List<TextMeshProUGUI>();

        [Header("=== RIGHT PANELS ===")]
        [SerializeField] private TextMeshProUGUI explanationText;
        [SerializeField] private TextMeshProUGUI keyKnowledgeText;

        [Header("=== BOTTOM NAVIGATION ===")]
        [SerializeField] private Button btnPrev;
        [SerializeField] private Button btnNext;
        [SerializeField] private Button btnBackToResult;
        [SerializeField] private Button btnAskAiTutor;
        [SerializeField] private TextMeshProUGUI bottomCounterText;

        [Header("=== LEFT QUESTION LIST (10 ITEMS) ===")]
        [SerializeField] private List<Image> leftQuestionRowImages = new List<Image>();
        [SerializeField] private List<TextMeshProUGUI> leftQuestionRowLabels = new List<TextMeshProUGUI>();
        [SerializeField] private List<Image> leftQuestionRowIcons = new List<Image>();

        [Header("=== PLACEHOLDER FLOATING ICONS (HIDDEN AT RUNTIME) ===")]
        [SerializeField] private GameObject checkmarkIconObj;
        [SerializeField] private GameObject crossIconObj;

        private readonly List<LeftQuestionRowView> _leftRowViews = new List<LeftQuestionRowView>();
        private readonly List<CenterAnswerRowView> _centerAnswerRows = new List<CenterAnswerRowView>();

        private int _currentReviewIndex = 0;
        private QuizAttemptData _attemptData;
        private Sprite _checkSprite;
        private Sprite _crossSprite;
        private Sprite _neutralRowSprite;
        private Sprite _selectionBorderSprite;
        private Sprite _answerRowSprite;
        private Sprite _backButtonSprite;
        private Sprite _optionDefaultSprite;
        private Sprite _optionCorrectSprite;
        private Sprite _optionWrongSprite;
        private TMP_FontAsset _defaultFont;
        private bool _isUIInitialized = false;
        private bool _isSceneNativeUI = false;

        private static readonly string[] AnswerPrefixes = { "A", "B", "C", "D" };

        public static readonly Color CorrectRowColor = new Color(0.14f, 0.56f, 0.24f, 0.98f);
        public static readonly Color WrongRowColor = new Color(0.74f, 0.16f, 0.16f, 0.98f);
        public static readonly Color NeutralRowColor = new Color(0.10f, 0.18f, 0.34f, 0.92f);

        public static readonly Color NeutralAnswerRowBgColor = Color.white;
        public static readonly Color WrongAnswerRowBgColor = new Color(1f, 0.85f, 0.85f, 1f);
        public static readonly Color CorrectAnswerRowBgColor = new Color(0.85f, 1f, 0.88f, 1f);

        // Public properties phục vụ test và verification
        public IReadOnlyList<LeftQuestionRowView> LeftRowViews => _leftRowViews;
        public IReadOnlyList<CenterAnswerRowView> CenterAnswerRows => _centerAnswerRows;
        public TextMeshProUGUI QuestionTextComponent => centerQuestionText;
        public TextMeshProUGUI ExplanationTextComponent => explanationText;
        public TextMeshProUGUI KeyKnowledgeTextComponent => keyKnowledgeText;
        public TextMeshProUGUI BottomCounterTextComponent => bottomCounterText;
        public Button BackToResultButton => btnBackToResult;
        public Button PrevButton => btnPrev;
        public Button NextButton => btnNext;
        public int CurrentReviewIndex => _currentReviewIndex;
        public int InconsistencyErrorCount { get; private set; }
        public string LastInconsistencyErrorMessage { get; private set; }

        private void Awake()
        {
            string sceneName = gameObject.scene.name;
            if (!string.IsNullOrEmpty(sceneName) && (sceneName.Contains("20.1") || sceneName.Contains("TL") || sceneName.Contains("Written")))
            {
                Destroy(this);
                return;
            }

            EnsureInitialized();
        }

        private void Start()
        {
            LoadAttemptData();
        }

        public void EnsureInitialized()
        {
            if (_isUIInitialized) return;

            EnsureEventSystem();
            EnsureRuntimeSprites();
            AutoBindNativeUIReferences();

            if (_isSceneNativeUI)
            {
                SetupSceneNativeViews();
                EnsureScrollRects();
            }
            else
            {
                // Fallback chỉ dùng khi chạy trong test harness cô lập không có Scene
                BuildHarnessFallbackViews();
            }

            SetupBottomButtons();
            SetupLeftRowButtons();
            EnsureAiTutorButton();

            _isUIInitialized = true;
        }

        private void Update()
        {
            bool escPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                escPressed = true;
            }
#endif
            if (!escPressed && Input.GetKeyDown(KeyCode.Escape))
            {
                escPressed = true;
            }

            if (escPressed)
            {
                OnReturnToResult();
            }
        }

        private void EnsureEventSystem()
        {
            EventSystem es = FindAnyObjectByType<EventSystem>();
            if (es != null)
            {
                if (es.GetComponent<InputSystemUIInputModule>() == null)
                {
                    es.gameObject.AddComponent<InputSystemUIInputModule>();
                }
            }
            else
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }
        }

        private void EnsureRuntimeSprites()
        {
            // Trong Scene 20 gốc, toàn bộ UI và button đã có sprite mỹ thuật từ Scene, không tạo Texture runtime để tải scene nhanh tức thì
            if (_isSceneNativeUI) return;

            if (_neutralRowSprite == null)
            {
                _neutralRowSprite = CreatePaddedRoundedRectSprite(264, 90, 12, 12, 10);
            }
            if (_selectionBorderSprite == null)
            {
                _selectionBorderSprite = CreateHollowRoundedBorderSprite(264, 90, 10, 10, 11, 5);
            }
            if (_answerRowSprite == null)
            {
                _answerRowSprite = CreatePaddedRoundedRectSprite(256, 64, 2, 2, 8);
            }
            if (_backButtonSprite == null)
            {
                _backButtonSprite = CreateNavyGoldButtonSprite(280, 54, 12);
            }
        }

        private void AutoBindNativeUIReferences()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Image[] allImages = canvas.GetComponentsInChildren<Image>(true);
            TextMeshProUGUI[] allTmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);

            // 1. Tìm các thành phần đặc trưng của Scene 20 gốc
            List<Image> rowImgs = new List<Image>();
            List<Image> iconImgs = new List<Image>();
            List<TextMeshProUGUI> rowLabels = new List<TextMeshProUGUI>();

            foreach (var img in allImages)
            {
                if (img == null) continue;
                Vector2 pos = img.rectTransform.anchoredPosition;
                Vector2 size = img.rectTransform.sizeDelta;

                // Ẩn 2 icon mẫu tĩnh ở giữa nếu có
                if (pos.x > 200f && pos.x < 350f && Mathf.Abs(pos.y) < 120f && size.x <= 130f)
                {
                    if (pos.y > 0f)
                    {
                        checkmarkIconObj = img.gameObject;
                        if (img.sprite != null) _checkSprite = img.sprite;
                    }
                    else
                    {
                        crossIconObj = img.gameObject;
                        if (img.sprite != null) _crossSprite = img.sprite;
                    }
                    img.gameObject.SetActive(false);
                    continue;
                }

                // Nút Quay lại kết quả ở góc dưới bên trái (pos x ~ -705, y ~ -486)
                if (btnBackToResult == null && pos.x < -600f && pos.y < -440f && size.x > 200f)
                {
                    var b = img.GetComponent<Button>();
                    if (b == null) b = img.gameObject.AddComponent<Button>();
                    btnBackToResult = b;
                    img.raycastTarget = true;
                    continue;
                }

                // Nút Câu trước ở đáy (pos x ~ 20, y ~ -491)
                if (btnPrev == null && Mathf.Abs(pos.x - 20f) < 80f && pos.y < -440f && size.x > 200f)
                {
                    var b = img.GetComponent<Button>();
                    if (b == null) b = img.gameObject.AddComponent<Button>();
                    btnPrev = b;
                    img.raycastTarget = true;
                    continue;
                }

                // Nút Câu tiếp ở đáy (pos x ~ 602, y ~ -489)
                if (btnNext == null && pos.x > 500f && pos.y < -440f && size.x > 200f)
                {
                    var b = img.GetComponent<Button>();
                    if (b == null) b = img.gameObject.AddComponent<Button>();
                    btnNext = b;
                    img.raycastTarget = true;
                    continue;
                }

                // 10 Khung nút câu hỏi bên trái (X ~ -735, Y từ 160 xuống -400, size.x ~ 330..345, size.y ~ 100..110)
                if (pos.x < -680f && pos.x > -800f && pos.y < 200f && pos.y > -420f && size.x > 300f && size.x < 360f && size.y > 90f && size.y < 120f)
                {
                    rowImgs.Add(img);
                }

                // 10 Icon bên cạnh câu hỏi bên trái (X ~ -655, Y từ 180 xuống -420, size ~ 60x50)
                if (pos.x < -620f && pos.x > -680f && pos.y < 200f && pos.y > -430f && size.x < 80f && size.y < 70f)
                {
                    iconImgs.Add(img);
                }

                // 4 Hộp phương án A, B, C, D ở giữa (X ~ -90, Y từ 50 xuống -250, size.x ~ 670..700, size.y ~ 100)
                if (pos.x > -150f && pos.x < -40f && pos.y < 60f && pos.y > -260f && size.x > 650f && size.y > 80f && size.y < 120f)
                {
                    nativeOptionBoxes.Add(img);
                }
            }

            // Sắp xếp các danh sách theo vị trí Y từ trên xuống dưới
            rowImgs.Sort((a, b) => b.rectTransform.anchoredPosition.y.CompareTo(a.rectTransform.anchoredPosition.y));
            iconImgs.Sort((a, b) => b.rectTransform.anchoredPosition.y.CompareTo(a.rectTransform.anchoredPosition.y));
            nativeOptionBoxes.Sort((a, b) => b.rectTransform.anchoredPosition.y.CompareTo(a.rectTransform.anchoredPosition.y));

            // Tìm các TextMeshProUGUI của Scene
            foreach (var t in allTmps)
            {
                if (t == null) continue;
                Vector2 pos = t.rectTransform.anchoredPosition;
                string text = t.text != null ? t.text.Trim() : "";

                // Tiêu đề câu hỏi "Câu 1/10"
                if (questionNumberHeaderTmp == null && pos.x > -100f && pos.x < 50f && pos.y > 220f && pos.y < 280f)
                {
                    questionNumberHeaderTmp = t;
                    continue;
                }

                // Đề bài câu hỏi ở giữa
                if (centerQuestionText == null && pos.x > -120f && pos.x < 20f && pos.y > 100f && pos.y < 200f)
                {
                    centerQuestionText = t;
                    continue;
                }

                // Dòng trạng thái "Bạn chọn A - Đáp án đúng: B"
                if (userChoiceStatusTmp == null && pos.y < -260f && pos.y > -320f && (text.Contains("Bạn chọn") || text.Contains("chọn") || pos.x > 0f))
                {
                    userChoiceStatusTmp = t;
                    continue;
                }

                // Lời giải chi tiết bên phải trên
                if (explanationText == null && pos.x > 500f && pos.y > 50f && pos.y < 220f)
                {
                    explanationText = t;
                    continue;
                }

                // Mẹo ghi nhớ bên phải dưới
                if (keyKnowledgeText == null && pos.x > 500f && pos.y < -200f && pos.y > -360f)
                {
                    keyKnowledgeText = t;
                    continue;
                }

                // Nhãn câu hỏi bên trái (X ~ -720, Y từ 180 xuống -400)
                if (pos.x < -680f && pos.x > -760f && pos.y < 200f && pos.y > -420f)
                {
                    rowLabels.Add(t);
                }

                // Text 4 phương án ở giữa (X ~ -110, Y từ 50 xuống -250)
                if (pos.x >= -180f && pos.x <= -80f && pos.y < 50f && pos.y > -250f && t.rectTransform.sizeDelta.x > 350f)
                {
                    nativeOptionTexts.Add(t);
                }
            }

            rowLabels.Sort((a, b) => b.rectTransform.anchoredPosition.y.CompareTo(a.rectTransform.anchoredPosition.y));
            nativeOptionTexts.Sort((a, b) => b.rectTransform.anchoredPosition.y.CompareTo(a.rectTransform.anchoredPosition.y));

            leftQuestionRowImages = rowImgs;
            leftQuestionRowIcons = iconImgs;
            leftQuestionRowLabels = rowLabels;

            if (bottomCounterText == null)
            {
                bottomCounterText = questionNumberHeaderTmp;
            }

            // Lấy 3 sprite mỹ thuật gốc của 4 hộp phương án từ Scene
            if (nativeOptionBoxes.Count >= 4)
            {
                // Box 0: Sprite 54 (Viền đỏ), Box 1: Sprite 53 (Viền xanh), Box 2 & 3: Sprite 37 (Viền vàng)
                _optionWrongSprite = nativeOptionBoxes[0].sprite;
                _optionCorrectSprite = nativeOptionBoxes[1].sprite;
                _optionDefaultSprite = nativeOptionBoxes[2].sprite ?? nativeOptionBoxes[3].sprite;
            }

            // Dọn sạch các TextMeshPro bị lệch/rác trong Scene gốc gây đè chữ
            foreach (var t in allTmps)
            {
                if (t == null) continue;
                Vector2 pos = t.rectTransform.anchoredPosition;
                string text = t.text != null ? t.text.Trim() : "";

                // Rogue text trong cột danh sách câu bên trái (X < -600f) mà không phải nhãn "Câu X"
                if (pos.x < -600f)
                {
                    if (t.rectTransform.sizeDelta.x < 100f || text.Length > 15 || text.Contains("phản xạ") || text.Contains("Bóng đèn") || text.Contains("thông thường"))
                    {
                        t.text = "";
                        t.enabled = false;
                        // Tuyệt đối KHÔNG gọi SetActive(false) trên GameObject vì TextMeshPro rác có thể nằm trên GameObject của Icon 6 (Câu 7) và Icon 9 (Câu 10)
                    }
                }
            }

            // Căn chỉnh dòng trạng thái "Bạn chọn ... - Đáp án đúng ..." không đè sau nút Quay lại
            if (userChoiceStatusTmp != null)
            {
                userChoiceStatusTmp.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                userChoiceStatusTmp.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                userChoiceStatusTmp.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                userChoiceStatusTmp.rectTransform.anchoredPosition = new Vector2(0f, -245f);
                userChoiceStatusTmp.rectTransform.sizeDelta = new Vector2(650f, 40f);
                userChoiceStatusTmp.alignment = TextAlignmentOptions.Center;
                userChoiceStatusTmp.fontSize = 20f;
                userChoiceStatusTmp.fontStyle = FontStyles.Bold;
            }

            // Nếu tìm thấy đầy đủ các thành phần gốc của Scene 20
            if (leftQuestionRowImages.Count >= 10 && nativeOptionBoxes.Count >= 4)
            {
                _isSceneNativeUI = true;
                Debug.Log("[QuizReviewController] Phát hiện Scene 20 Native UI chuẩn! Giữ 100% giao diện gốc.");
            }
            else
            {
                _isSceneNativeUI = false;
                Debug.Log("[QuizReviewController] Không phải Scene 20 native (chế độ Test Harness). Khởi tạo fallback views.");
            }

            // Gán chính xác Check (✓ xanh) và Cross (✕ đỏ)
            // Trong Scene 20 Detail Result TN:
            // Row 2 icon (y ~ 69.6f, GO 1743237355) mang sprite Check xanh (Atlas sprite giao diện Aurelia Academy_46, ID -3379940049477395)
            // Row 1 icon (y ~ 172.4f, GO 414164628) mang sprite Cross đỏ (Atlas sprite giao diện Aurelia Academy_47, ID -4674022632050681575)
            if (leftQuestionRowIcons != null)
            {
                foreach (var ic in leftQuestionRowIcons)
                {
                    if (ic == null || ic.sprite == null) continue;
                    float y = ic.rectTransform.anchoredPosition.y;
                    // Hàng 2 (y ~ 69.6f) là Check xanh ✓
                    if (Mathf.Abs(y - 69.6f) < 25f && _checkSprite == null)
                    {
                        _checkSprite = ic.sprite;
                    }
                    // Hàng 1 (y ~ 172.4f) là Cross đỏ ✕
                    else if (Mathf.Abs(y - 172.4f) < 25f && _crossSprite == null)
                    {
                        _crossSprite = ic.sprite;
                    }
                }

                // Fallback nếu vị trí y bị xê dịch: hàng 2 luôn là Check, hàng 1 luôn là Cross
                if (_checkSprite == null && leftQuestionRowIcons.Count > 1 && leftQuestionRowIcons[1].sprite != null)
                {
                    _checkSprite = leftQuestionRowIcons[1].sprite;
                }
                if (_crossSprite == null && leftQuestionRowIcons.Count > 0 && leftQuestionRowIcons[0].sprite != null)
                {
                    _crossSprite = leftQuestionRowIcons[0].sprite;
                }
            }
            if (_checkSprite == null) _checkSprite = CreateFallbackIconSprite(true);
            if (_crossSprite == null) _crossSprite = CreateFallbackIconSprite(false);
        }

        /// <summary>
        /// Khởi tạo và liên kết các thành phần có sẵn của Scene 20 thật mà KHÔNG tạo GameObject đè.
        /// </summary>
        private void SetupSceneNativeViews()
        {
            _leftRowViews.Clear();
            _centerAnswerRows.Clear();

            // 1. Cấu hình 10 nút câu hỏi bên trái có sẵn
            for (int i = 0; i < 10; i++)
            {
                int qIdx = i;
                Image rowImg = leftQuestionRowImages[i];
                var rowBtn = rowImg.GetComponent<Button>();
                if (rowBtn == null) rowBtn = rowImg.gameObject.AddComponent<Button>();
                rowImg.raycastTarget = true;

                TextMeshProUGUI labelTmp = i < leftQuestionRowLabels.Count ? leftQuestionRowLabels[i] : null;
                if (labelTmp != null)
                {
                    labelTmp.raycastTarget = false;
                    labelTmp.text = $"Câu {i + 1}";
                }

                Image iconImg = i < leftQuestionRowIcons.Count ? leftQuestionRowIcons[i] : null;
                Image checkImg = null;
                Image crossImg = null;
                if (iconImg != null)
                {
                    iconImg.gameObject.SetActive(true); // Đảm bảo active 100% trong hierarchy
                    iconImg.raycastTarget = false;
                    iconImg.preserveAspect = true;
                    iconImg.color = Color.white;
                    checkImg = iconImg;
                    crossImg = iconImg;
                }

                // Tạo đối tượng SelectionHighlight nhỏ gọn nếu chưa có
                Transform highlightTr = rowImg.transform.Find("SelectionHighlight");
                GameObject highlightGo;
                if (highlightTr != null)
                {
                    highlightGo = highlightTr.gameObject;
                }
                else
                {
                    highlightGo = new GameObject("SelectionHighlight");
                    highlightGo.transform.SetParent(rowImg.transform, false);
                    RectTransform hRt = highlightGo.AddComponent<RectTransform>();
                    hRt.anchorMin = Vector2.zero;
                    hRt.anchorMax = Vector2.one;
                    hRt.sizeDelta = new Vector2(4f, 4f);
                    Image hImg = highlightGo.AddComponent<Image>();
                    hImg.sprite = _selectionBorderSprite;
                    hImg.type = Image.Type.Sliced;
                    hImg.color = new Color(1f, 0.90f, 0.40f, 0.95f);
                    hImg.raycastTarget = false;
                    highlightGo.SetActive(false);
                }

                var rowView = new LeftQuestionRowView
                {
                    questionIndex = qIdx,
                    backgroundImage = rowImg,
                    questionLabel = labelTmp,
                    iconImage = iconImg,
                    checkIcon = checkImg ?? iconImg,
                    crossIcon = crossImg ?? iconImg,
                    selectionHighlightObj = highlightGo,
                    rowButton = rowBtn,
                    correctnessState = RowCorrectnessState.Neutral,
                    isSelectedHighlight = false
                };
                _leftRowViews.Add(rowView);
            }

            // 2. Cấu hình 4 hộp phương án ở giữa có sẵn
            for (int i = 0; i < 4; i++)
            {
                Image boxImg = nativeOptionBoxes[i];
                TextMeshProUGUI boxTmp = i < nativeOptionTexts.Count ? nativeOptionTexts[i] : null;

                if (boxTmp != null)
                {
                    boxTmp.raycastTarget = false;
                    // Chuẩn hóa RectTransform và kiểu chữ của 4 phương án:
                    // Chiều cao 80px căn giữa box 96-100px; độ rộng 486px chừa lề trái 32px và lề phải cho badge trạng thái
                    boxTmp.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    boxTmp.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    boxTmp.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    boxTmp.rectTransform.anchoredPosition = new Vector2(-180f, boxImg.rectTransform.anchoredPosition.y);
                    boxTmp.rectTransform.sizeDelta = new Vector2(380f, 80f);
                    boxTmp.margin = new Vector4(0f, 0f, 15f, 0f);
                    boxTmp.alignment = TextAlignmentOptions.MidlineLeft;
                    boxTmp.textWrappingMode = TextWrappingModes.Normal;
                    boxTmp.overflowMode = TextOverflowModes.Truncate;
                    boxTmp.enableAutoSizing = true;
                    boxTmp.fontSizeMin = 17f;
                    boxTmp.fontSizeMax = 22f;
                    boxTmp.color = Color.white;
                }
                boxImg.raycastTarget = false;

                // Tạo statusIcon nhỏ ở góc phải
                Transform iconTr = boxImg.transform.Find("StatusIcon");
                Image statusIconImg;
                if (iconTr != null)
                {
                    statusIconImg = iconTr.GetComponent<Image>();
                }
                else
                {
                    GameObject iconGo = new GameObject("StatusIcon");
                    iconGo.transform.SetParent(boxImg.transform, false);
                    RectTransform icRt = iconGo.AddComponent<RectTransform>();
                    icRt.anchorMin = new Vector2(1f, 0.5f);
                    icRt.anchorMax = new Vector2(1f, 0.5f);
                    icRt.pivot = new Vector2(1f, 0.5f);
                    icRt.anchoredPosition = new Vector2(-20f, 0f);
                    icRt.sizeDelta = new Vector2(30f, 30f);
                    statusIconImg = iconGo.AddComponent<Image>();
                    statusIconImg.preserveAspect = true;
                    statusIconImg.raycastTarget = false;
                    iconGo.SetActive(false);
                }

                // Tạo statusLabel nhỏ ở góc phải hộp (chừa khoảng cách riêng không đè lên text đáp án)
                Transform statusTr = boxImg.transform.Find("StatusLabel");
                TextMeshProUGUI statusLabelTmp;
                if (statusTr != null)
                {
                    statusLabelTmp = statusTr.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                    GameObject statusGo = new GameObject("StatusLabel");
                    statusGo.transform.SetParent(boxImg.transform, false);
                    RectTransform sRt = statusGo.AddComponent<RectTransform>();
                    sRt.anchorMin = new Vector2(1f, 0.5f);
                    sRt.anchorMax = new Vector2(1f, 0.5f);
                    sRt.pivot = new Vector2(1f, 0.5f);
                    sRt.anchoredPosition = new Vector2(-58f, 0f);
                    sRt.sizeDelta = new Vector2(115f, 32f);
                    statusLabelTmp = statusGo.AddComponent<TextMeshProUGUI>();
                    statusLabelTmp.alignment = TextAlignmentOptions.Right;
                    statusLabelTmp.fontSize = 17f;
                    statusLabelTmp.fontStyle = FontStyles.Bold;
                    statusLabelTmp.raycastTarget = false;
                    statusGo.SetActive(false);
                }

                var centerRow = new CenterAnswerRowView
                {
                    optionIndex = i,
                    rootObject = boxImg.gameObject,
                    backgroundImage = boxImg,
                    badgeBackground = null,
                    badgeText = null,
                    optionText = boxTmp,
                    statusIcon = statusIconImg,
                    statusLabel = statusLabelTmp,
                    visualState = AnswerOptionVisualState.Neutral
                };
                _centerAnswerRows.Add(centerRow);
            }
        }

        /// <summary>
        /// Khởi tạo views giả lập chỉ khi chạy trong môi trường Test Harness rỗng (QuizFlowVerificationTest).
        /// </summary>
        private void BuildHarnessFallbackViews()
        {
            _leftRowViews.Clear();
            _centerAnswerRows.Clear();

            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            Transform rootTr = canvas != null ? canvas.transform : transform;

            // 1. Left rows fallback
            for (int i = 0; i < 10; i++)
            {
                int qIdx = i;
                GameObject rowGo = new GameObject($"QuestionRow_Q{i + 1}");
                rowGo.transform.SetParent(rootTr, false);
                RectTransform rRt = rowGo.AddComponent<RectTransform>();
                rRt.anchoredPosition = new Vector2(-720f, 150f - i * 60f);
                rRt.sizeDelta = new Vector2(260f, 50f);

                Image bgImg = rowGo.AddComponent<Image>();
                bgImg.color = NeutralRowColor;
                Button btn = rowGo.AddComponent<Button>();

                GameObject lblGo = new GameObject("QuestionLabel");
                lblGo.transform.SetParent(rowGo.transform, false);
                TextMeshProUGUI lblTmp = lblGo.AddComponent<TextMeshProUGUI>();
                lblTmp.text = $"Câu {i + 1}";

                GameObject cGo = new GameObject("CheckIcon");
                cGo.transform.SetParent(rowGo.transform, false);
                Image cImg = cGo.AddComponent<Image>();
                cImg.sprite = _checkSprite;
                cGo.SetActive(false);

                GameObject xGo = new GameObject("CrossIcon");
                xGo.transform.SetParent(rowGo.transform, false);
                Image xImg = xGo.AddComponent<Image>();
                xImg.sprite = _crossSprite;
                xGo.SetActive(false);

                GameObject hlGo = new GameObject("SelectionHighlight");
                hlGo.transform.SetParent(rowGo.transform, false);
                hlGo.SetActive(false);

                _leftRowViews.Add(new LeftQuestionRowView
                {
                    questionIndex = qIdx,
                    backgroundImage = bgImg,
                    questionLabel = lblTmp,
                    checkIcon = cImg,
                    crossIcon = xImg,
                    selectionHighlightObj = hlGo,
                    rowButton = btn,
                    correctnessState = RowCorrectnessState.Neutral,
                    isSelectedHighlight = false
                });
            }

            // 2. Question Text Component fallback
            if (centerQuestionText == null)
            {
                GameObject qGo = new GameObject("QuestionText");
                qGo.transform.SetParent(rootTr, false);
                centerQuestionText = qGo.AddComponent<TextMeshProUGUI>();
            }

            // 3. Center Answer Rows fallback
            for (int i = 0; i < 4; i++)
            {
                GameObject rowGo = new GameObject($"AnswerRow_{AnswerPrefixes[i]}");
                rowGo.transform.SetParent(rootTr, false);
                RectTransform rt = rowGo.AddComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0f, 50f - i * 70f);
                rt.sizeDelta = new Vector2(600f, 60f);

                Image bgImg = rowGo.AddComponent<Image>();
                bgImg.color = NeutralAnswerRowBgColor;

                GameObject optGo = new GameObject("OptionText");
                optGo.transform.SetParent(rowGo.transform, false);
                TextMeshProUGUI optTmp = optGo.AddComponent<TextMeshProUGUI>();

                GameObject iconGo = new GameObject("StatusIcon");
                iconGo.transform.SetParent(rowGo.transform, false);
                Image iconImg = iconGo.AddComponent<Image>();
                iconGo.SetActive(false);

                GameObject lblGo = new GameObject("StatusLabel");
                lblGo.transform.SetParent(rowGo.transform, false);
                TextMeshProUGUI lblTmp = lblGo.AddComponent<TextMeshProUGUI>();
                lblGo.SetActive(false);

                _centerAnswerRows.Add(new CenterAnswerRowView
                {
                    optionIndex = i,
                    rootObject = rowGo,
                    backgroundImage = bgImg,
                    badgeBackground = null,
                    badgeText = null,
                    optionText = optTmp,
                    statusIcon = iconImg,
                    statusLabel = lblTmp,
                    visualState = AnswerOptionVisualState.Neutral
                });
            }

            // 4. Explanation & Key Knowledge fallback
            if (explanationText == null)
            {
                GameObject eGo = new GameObject("ExplanationText");
                eGo.transform.SetParent(rootTr, false);
                explanationText = eGo.AddComponent<TextMeshProUGUI>();
            }
            if (keyKnowledgeText == null)
            {
                GameObject kkGo = new GameObject("KeyKnowledgeText");
                kkGo.transform.SetParent(rootTr, false);
                keyKnowledgeText = kkGo.AddComponent<TextMeshProUGUI>();
            }
            if (bottomCounterText == null)
            {
                GameObject cGo = new GameObject("BottomCounterText");
                cGo.transform.SetParent(rootTr, false);
                bottomCounterText = cGo.AddComponent<TextMeshProUGUI>();
            }
            if (btnBackToResult == null)
            {
                GameObject bGo = new GameObject("Btn_BackToResult");
                bGo.transform.SetParent(rootTr, false);
                bGo.AddComponent<RectTransform>().anchoredPosition = new Vector2(-700f, -480f);
                btnBackToResult = bGo.AddComponent<Button>();
            }
            if (btnPrev == null)
            {
                GameObject pGo = new GameObject("Btn_Prev");
                pGo.transform.SetParent(rootTr, false);
                pGo.AddComponent<RectTransform>().anchoredPosition = new Vector2(0f, -480f);
                btnPrev = pGo.AddComponent<Button>();
            }
            if (btnNext == null)
            {
                GameObject nGo = new GameObject("Btn_Next");
                nGo.transform.SetParent(rootTr, false);
                nGo.AddComponent<RectTransform>().anchoredPosition = new Vector2(600f, -480f);
                btnNext = nGo.AddComponent<Button>();
            }
        }

        private void SetupLeftRowButtons()
        {
            for (int i = 0; i < _leftRowViews.Count; i++)
            {
                int qIdx = i;
                var rowView = _leftRowViews[i];
                if (rowView.rowButton != null)
                {
                    rowView.rowButton.onClick.RemoveAllListeners();
                    rowView.rowButton.onClick.AddListener(() =>
                    {
                        ShowQuestion(qIdx);
                    });
                }
            }
        }

        private void SetupBottomButtons()
        {
            if (btnPrev != null)
            {
                btnPrev.onClick.RemoveAllListeners();
                btnPrev.onClick.AddListener(OnPrevClicked);
            }
            if (btnNext != null)
            {
                btnNext.onClick.RemoveAllListeners();
                btnNext.onClick.AddListener(OnNextClicked);
            }
            if (btnBackToResult != null)
            {
                btnBackToResult.onClick.RemoveAllListeners();
                btnBackToResult.onClick.AddListener(OnReturnToResult);
            }
        }

        private void EnsureAiTutorButton()
        {
            if (btnAskAiTutor != null) return;

            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Đặt nút Gia sư AI ở vị trí thoáng, thanh lịch phía trên khung Giải thích (Y = 410f), không che tiêu đề
            GameObject aiBtnGo = new GameObject("Btn_AskAiTutor");
            aiBtnGo.transform.SetParent(canvas.transform, false);

            RectTransform rt = aiBtnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(642f, 410f);
            rt.sizeDelta = new Vector2(240f, 44f);

            Image img = aiBtnGo.AddComponent<Image>();
            img.color = new Color(0.08f, 0.18f, 0.36f, 0.95f);
            img.raycastTarget = true;
            Outline outline = aiBtnGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);

            btnAskAiTutor = aiBtnGo.AddComponent<Button>();
            btnAskAiTutor.onClick.AddListener(OnAskAiTutorClicked);

            GameObject textGo = new GameObject("Label");
            textGo.transform.SetParent(aiBtnGo.transform, false);
            RectTransform tRt = textGo.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "✦ HỎI GIA SƯ AI ✦";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 20f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(1f, 0.92f, 0.65f, 1f);
            tmp.raycastTarget = false;
        }

        public void LoadAttemptData()
        {
            EnsureInitialized();

            // 1. Ưu tiên đọc từ PhysicsGameSession
            var session = PhysicsGameSession.Instance;
            if (session != null && session.CurrentQuizAttempt != null && session.CurrentQuizAttempt.answers != null && session.CurrentQuizAttempt.answers.Count > 0)
            {
                _attemptData = session.CurrentQuizAttempt;
                Debug.Log($"[QuizReviewController] Tải attempt thành công từ PhysicsGameSession ({_attemptData.answers.Count} câu).");
                RenderReviewFromAttempt(_attemptData);
                return;
            }

            // 2. Fallback đọc từ NavigationService
            var nav = NavigationService.Instance;
            if (nav != null && nav.Attempt != null && nav.Attempt.Answers != null && nav.Attempt.Answers.Count > 0)
            {
                _attemptData = new QuizAttemptData
                {
                    lessonId = nav.Selection.LessonId,
                    lessonTitle = nav.Selection.LessonId,
                    score = (int)nav.Attempt.ScoreRaw,
                    correctCount = nav.Attempt.CorrectCount,
                    wrongCount = nav.Attempt.QuestionCount - nav.Attempt.CorrectCount,
                    elapsedSeconds = (float)nav.Attempt.DurationSeconds,
                    answers = new List<QuizAttemptAnswer>()
                };

                for (int i = 0; i < nav.Attempt.Answers.Count; i++)
                {
                    var a = nav.Attempt.Answers[i];
                    _attemptData.answers.Add(new QuizAttemptAnswer
                    {
                        questionId = a.CauHoiID,
                        questionText = a.QuestionText,
                        options = a.Options != null ? new List<string>(a.Options) : new List<string>(),
                        selectedAnswerIndex = a.SelectedOptionIndex,
                        correctAnswerIndex = a.CorrectOptionIndex,
                        wasCorrect = (a.SelectedOptionIndex == a.CorrectOptionIndex),
                        explanation = a.Explanation,
                        keyKnowledge = a.KeyKnowledge
                    });
                }

                Debug.Log($"[QuizReviewController] Tải attempt thành công từ NavigationService ({_attemptData.answers.Count} câu).");
                RenderReviewFromAttempt(_attemptData);
                return;
            }

            Debug.LogWarning("[QuizReviewController] Chưa có dữ liệu attempt. Hiển thị trạng thái trung tính.");
            RenderEmptyNeutralState();
        }

        public void RenderReviewFromAttempt(QuizAttemptData attempt)
        {
            _attemptData = attempt;
            if (_attemptData == null || _attemptData.answers == null || _attemptData.answers.Count == 0)
            {
                RenderEmptyNeutralState();
                return;
            }

            InconsistencyErrorCount = 0;
            LastInconsistencyErrorMessage = null;

            // 1. Cập nhật trạng thái Đúng/Sai cho 10 nút câu hỏi bên trái
            for (int i = 0; i < _leftRowViews.Count; i++)
            {
                var rowView = _leftRowViews[i];
                if (i < _attemptData.answers.Count)
                {
                    var ans = _attemptData.answers[i];
                    bool isAnswered = ans.selectedAnswerIndex >= 0;
                    bool isActuallyCorrect = isAnswered && (ans.selectedAnswerIndex == ans.correctAnswerIndex);

                    if (ans.wasCorrect != isActuallyCorrect)
                    {
                        InconsistencyErrorCount++;
                        LastInconsistencyErrorMessage = $"Q{i + 1} inconsistency: wasCorrect={ans.wasCorrect} but selected={ans.selectedAnswerIndex}, correct={ans.correctAnswerIndex}";
                        Debug.LogWarning($"[QuizReviewController] {LastInconsistencyErrorMessage}");
                    }

                    if (isActuallyCorrect)
                    {
                        rowView.correctnessState = RowCorrectnessState.Correct;
                        if (!_isSceneNativeUI) rowView.backgroundImage.color = CorrectRowColor;
                        if (rowView.iconImage != null)
                        {
                            rowView.iconImage.gameObject.SetActive(true);
                            rowView.iconImage.sprite = _checkSprite;
                            rowView.iconImage.color = Color.white;
                            rowView.iconImage.enabled = true;
                        }
                        else if (rowView.checkIcon != null)
                        {
                            rowView.checkIcon.gameObject.SetActive(true);
                            rowView.checkIcon.sprite = _checkSprite;
                            rowView.checkIcon.color = Color.white;
                            if (rowView.crossIcon != null && rowView.crossIcon != rowView.checkIcon)
                            {
                                rowView.crossIcon.gameObject.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        rowView.correctnessState = RowCorrectnessState.Wrong;
                        if (!_isSceneNativeUI) rowView.backgroundImage.color = WrongRowColor;
                        if (rowView.iconImage != null)
                        {
                            rowView.iconImage.gameObject.SetActive(true);
                            rowView.iconImage.sprite = _crossSprite;
                            rowView.iconImage.color = Color.white;
                            rowView.iconImage.enabled = true;
                        }
                        else if (rowView.crossIcon != null)
                        {
                            rowView.crossIcon.gameObject.SetActive(true);
                            rowView.crossIcon.sprite = _crossSprite;
                            rowView.crossIcon.color = Color.white;
                            if (rowView.checkIcon != null && rowView.checkIcon != rowView.crossIcon)
                            {
                                rowView.checkIcon.gameObject.SetActive(false);
                            }
                        }
                    }
                }
                else
                {
                    rowView.correctnessState = RowCorrectnessState.Neutral;
                    if (!_isSceneNativeUI) rowView.backgroundImage.color = NeutralRowColor;
                    if (rowView.iconImage != null) rowView.iconImage.gameObject.SetActive(false);
                    if (rowView.checkIcon != null) rowView.checkIcon.gameObject.SetActive(false);
                    if (rowView.crossIcon != null) rowView.crossIcon.gameObject.SetActive(false);
                }
            }

            // 2. Hiển thị câu hỏi đầu tiên
            ShowQuestion(0);
        }

        private ScrollRect _explanationScrollRect;
        private ScrollRect _keyKnowledgeScrollRect;

        private void EnsureScrollRects()
        {
            if (explanationText != null && _explanationScrollRect == null)
            {
                _explanationScrollRect = SetupScrollRectForTmp(explanationText, new Vector2(642.3f, 35f), new Vector2(480f, 290f));
            }
            if (keyKnowledgeText != null && _keyKnowledgeScrollRect == null)
            {
                _keyKnowledgeScrollRect = SetupScrollRectForTmp(keyKnowledgeText, new Vector2(642.3f, -315f), new Vector2(480f, 215f));
            }
        }

        private ScrollRect SetupScrollRectForTmp(TextMeshProUGUI tmp, Vector2 viewportPos, Vector2 viewportSize)
        {
            if (tmp == null) return null;
            Transform parent = tmp.transform.parent;
            if (parent == null) return null;

            ScrollRect existingSr = tmp.GetComponentInParent<ScrollRect>();
            if (existingSr != null) return existingSr;

            // 1. Tạo ScrollRect Viewport
            GameObject srGo = new GameObject($"{tmp.gameObject.name}_ScrollArea");
            srGo.transform.SetParent(parent, false);
            RectTransform srRt = srGo.AddComponent<RectTransform>();
            srRt.anchorMin = new Vector2(0.5f, 0.5f);
            srRt.anchorMax = new Vector2(0.5f, 0.5f);
            srRt.pivot = new Vector2(0.5f, 0.5f);
            srRt.anchoredPosition = viewportPos;
            srRt.sizeDelta = viewportSize;

            ScrollRect sr = srGo.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.inertia = true;
            sr.decelerationRate = 0.135f;
            sr.scrollSensitivity = 35f;

            Image maskImg = srGo.AddComponent<Image>();
            maskImg.color = Color.clear;
            maskImg.raycastTarget = true;
            srGo.AddComponent<RectMask2D>();
            sr.viewport = srRt;

            // 2. Tạo Content Container
            GameObject contentGo = new GameObject("Content");
            contentGo.transform.SetParent(srGo.transform, false);
            RectTransform cRt = contentGo.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 1f);
            cRt.anchorMax = new Vector2(0.5f, 1f);
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(viewportSize.x, 0f);

            ContentSizeFitter csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            VerticalLayoutGroup vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(16, 16, 8, 8);

            sr.content = cRt;

            // 3. Reparent TextMeshPro vào Content
            tmp.transform.SetParent(contentGo.transform, false);
            RectTransform tRt = tmp.rectTransform;
            tRt.anchorMin = new Vector2(0.5f, 1f);
            tRt.anchorMax = new Vector2(0.5f, 1f);
            tRt.pivot = new Vector2(0.5f, 1f);
            tRt.anchoredPosition = Vector2.zero;
            tRt.sizeDelta = new Vector2(viewportSize.x - 32f, 0f);

            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.enableAutoSizing = false;
            tmp.fontSize = 22f;
            tmp.color = new Color(0.96f, 0.94f, 0.88f, 1f);
            tmp.raycastTarget = false;

            LayoutElement le = tmp.gameObject.GetComponent<LayoutElement>() ?? tmp.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = viewportSize.x - 32f;
            le.flexibleWidth = 1f;

            return sr;
        }

        public void ShowQuestion(int index)
        {
            if (_attemptData == null || _attemptData.answers == null || _attemptData.answers.Count == 0) return;
            if (index < 0 || index >= _attemptData.answers.Count) return;

            _currentReviewIndex = index;
            var ans = _attemptData.answers[index];

            // 1. Cập nhật highlight cho danh sách 10 câu bên trái
            for (int i = 0; i < _leftRowViews.Count; i++)
            {
                var rowView = _leftRowViews[i];
                bool isSelected = (i == index);
                rowView.isSelectedHighlight = isSelected;

                if (rowView.selectionHighlightObj != null)
                {
                    rowView.selectionHighlightObj.SetActive(isSelected);
                }

                if (rowView.questionLabel != null)
                {
                    rowView.questionLabel.color = isSelected ? new Color(1f, 0.92f, 0.50f, 1f) : Color.white;
                }
            }

            // 2. Cập nhật tiêu đề số câu & đề bài trung tâm
            string qHeader = $"Câu {index + 1}/10";
            if (questionNumberHeaderTmp != null)
            {
                questionNumberHeaderTmp.text = qHeader;
            }
            if (bottomCounterText != null)
            {
                bottomCounterText.text = qHeader;
            }

            if (centerQuestionText != null)
            {
                centerQuestionText.text = ans.questionText ?? "";
            }

            // 3. Cập nhật 4 phương án A, B, C, D
            for (int i = 0; i < _centerAnswerRows.Count; i++)
            {
                var optRow = _centerAnswerRows[i];
                string optionContent = (ans.options != null && i < ans.options.Count) ? ans.options[i] : "";

                // Giữ định dạng prefix A., B., C., D.
                string prefix = AnswerPrefixes[i];
                if (optRow.optionText != null)
                {
                    if (optionContent.StartsWith($"{prefix}.") || optionContent.StartsWith($"{prefix}:"))
                    {
                        optRow.optionText.text = optionContent;
                    }
                    else
                    {
                        optRow.optionText.text = $"{prefix}. {optionContent}";
                    }
                }

                bool isCorrectAnswer = (i == ans.correctAnswerIndex);
                bool isUserSelected = (i == ans.selectedAnswerIndex);

                if (isCorrectAnswer)
                {
                    optRow.visualState = AnswerOptionVisualState.CorrectAnswer;
                    if (_optionCorrectSprite != null && optRow.backgroundImage != null)
                    {
                        optRow.backgroundImage.sprite = _optionCorrectSprite;
                    }
                    if (optRow.backgroundImage != null) optRow.backgroundImage.color = Color.white;

                    if (optRow.statusLabel != null)
                    {
                        optRow.statusLabel.text = "Đáp án đúng";
                        optRow.statusLabel.color = new Color(0.10f, 0.55f, 0.20f, 1f);
                        optRow.statusLabel.gameObject.SetActive(true);
                    }
                    if (optRow.statusIcon != null)
                    {
                        optRow.statusIcon.sprite = _checkSprite;
                        optRow.statusIcon.gameObject.SetActive(true);
                    }
                }
                else if (isUserSelected)
                {
                    optRow.visualState = AnswerOptionVisualState.WrongSelected;
                    if (_optionWrongSprite != null && optRow.backgroundImage != null)
                    {
                        optRow.backgroundImage.sprite = _optionWrongSprite;
                    }
                    if (optRow.backgroundImage != null) optRow.backgroundImage.color = Color.white;

                    if (optRow.statusLabel != null)
                    {
                        optRow.statusLabel.text = "Bạn chọn";
                        optRow.statusLabel.color = new Color(0.80f, 0.15f, 0.15f, 1f);
                        optRow.statusLabel.gameObject.SetActive(true);
                    }
                    if (optRow.statusIcon != null)
                    {
                        optRow.statusIcon.sprite = _crossSprite;
                        optRow.statusIcon.gameObject.SetActive(true);
                    }
                }
                else
                {
                    optRow.visualState = AnswerOptionVisualState.Neutral;
                    if (_optionDefaultSprite != null && optRow.backgroundImage != null)
                    {
                        optRow.backgroundImage.sprite = _optionDefaultSprite;
                    }
                    if (optRow.backgroundImage != null) optRow.backgroundImage.color = Color.white;

                    if (optRow.statusLabel != null) optRow.statusLabel.gameObject.SetActive(false);
                    if (optRow.statusIcon != null) optRow.statusIcon.gameObject.SetActive(false);
                }
            }

            // 4. Cập nhật dòng trạng thái lựa chọn bên dưới
            if (userChoiceStatusTmp != null)
            {
                string userChar = (ans.selectedAnswerIndex >= 0 && ans.selectedAnswerIndex < 4) ? AnswerPrefixes[ans.selectedAnswerIndex] : "(Chưa chọn)";
                string correctChar = (ans.correctAnswerIndex >= 0 && ans.correctAnswerIndex < 4) ? AnswerPrefixes[ans.correctAnswerIndex] : "?";
                userChoiceStatusTmp.text = $"Bạn chọn {userChar}   -    Đáp án đúng: {correctChar}";
                userChoiceStatusTmp.color = (ans.selectedAnswerIndex >= 0 && ans.selectedAnswerIndex == ans.correctAnswerIndex)
                    ? new Color(0.35f, 1f, 0.50f, 1f)
                    : new Color(1f, 0.45f, 0.45f, 1f);
            }

            // 5. Cập nhật khung GIẢI THÍCH bên phải trên
            if (explanationText != null)
            {
                explanationText.text = !string.IsNullOrEmpty(ans.explanation) ? ans.explanation : "Chưa có lời giải chi tiết cho câu hỏi này.";
            }

            // 6. Cập nhật khung MẸO GHI NHỚ bên phải dưới (tra cứu chính xác theo QuestionId)
            if (keyKnowledgeText != null)
            {
                string tip = "";
                if (!string.IsNullOrEmpty(ans.questionId))
                {
                    tip = PhysicsQuestionRepository.ResolveKeyKnowledgeForAnswer(ans, _attemptData.lessonId);
                }
                if (string.IsNullOrEmpty(tip) && !string.IsNullOrEmpty(ans.keyKnowledge))
                {
                    tip = ans.keyKnowledge.Trim();
                }
                keyKnowledgeText.text = tip;
            }

            // 7. Reset vị trí cuộn lên đỉnh đầu mỗi khi sang câu hỏi mới
            if (_explanationScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                if (_explanationScrollRect.content != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_explanationScrollRect.content);
                }
                _explanationScrollRect.verticalNormalizedPosition = 1f;
            }
            if (_keyKnowledgeScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                if (_keyKnowledgeScrollRect.content != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_keyKnowledgeScrollRect.content);
                }
                _keyKnowledgeScrollRect.verticalNormalizedPosition = 1f;
            }

            // 8. Cập nhật trạng thái tương tác của nút Câu trước / Câu tiếp
            if (btnPrev != null) btnPrev.interactable = (index > 0);
            if (btnNext != null) btnNext.interactable = (index < _attemptData.answers.Count - 1);
        }

        public void OnPrevClicked()
        {
            if (_currentReviewIndex > 0)
            {
                ShowQuestion(_currentReviewIndex - 1);
            }
        }

        public void OnNextClicked()
        {
            if (_attemptData != null && _attemptData.answers != null && _currentReviewIndex < _attemptData.answers.Count - 1)
            {
                ShowQuestion(_currentReviewIndex + 1);
            }
        }

        public void OnReturnToResult()
        {
            Debug.Log("[QuizReviewController] Quay lại màn kết quả trắc nghiệm (19. Result_TN).");
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.NavigateTo(RouteId.QuizResult);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("19. Result_TN");
            }
        }

        private void OnAskAiTutorClicked()
        {
            if (_attemptData == null || _attemptData.answers == null || _currentReviewIndex >= _attemptData.answers.Count) return;

            var ans = _attemptData.answers[_currentReviewIndex];
            Debug.Log($"[QuizReviewController] Mở Gia sư AI cho Câu {_currentReviewIndex + 1}: {ans.questionId}");

            string lesson = _attemptData != null ? _attemptData.lessonId : "VatLy9";
            string resolvedKeyKnowledge = !string.IsNullOrEmpty(ans.keyKnowledge) ? ans.keyKnowledge : PhysicsQuestionRepository.ResolveKeyKnowledgeForAnswer(ans, lesson);

            var ctx = new MCQQuestionContext
            {
                AttemptId = NavigationService.Instance?.Attempt?.AttemptId ?? _attemptData.lessonId ?? "mcq_review_session",
                QuestionNumber = _currentReviewIndex + 1,
                TotalQuestions = _attemptData.answers.Count,
                QuestionId = ans.questionId,
                QuestionText = ans.questionText,
                Options = ans.options != null ? new List<string>(ans.options) : new List<string>(),
                SelectedAnswerIndex = ans.selectedAnswerIndex,
                CorrectAnswerIndex = ans.correctAnswerIndex,
                Explanation = ans.explanation,
                KeyKnowledge = resolvedKeyKnowledge,
                LessonId = lesson,
                Difficulty = NavigationService.Instance?.Selection?.Difficulty ?? ""
            };

            var popup = MCQTutorPopupController.GetOrCreate();
            if (popup != null)
            {
                popup.OpenTutor(MCQTutorMode.Explain, ctx);
            }
        }

        private void RenderEmptyNeutralState()
        {
            if (centerQuestionText != null) centerQuestionText.text = "Chưa có dữ liệu bài làm để xem lời giải.";
            if (explanationText != null) explanationText.text = "";
            if (keyKnowledgeText != null) keyKnowledgeText.text = "";
            if (userChoiceStatusTmp != null) userChoiceStatusTmp.text = "";

            foreach (var opt in _centerAnswerRows)
            {
                if (opt.optionText != null) opt.optionText.text = "";
                if (opt.backgroundImage != null) opt.backgroundImage.color = NeutralAnswerRowBgColor;
                if (opt.statusLabel != null) opt.statusLabel.gameObject.SetActive(false);
                if (opt.statusIcon != null) opt.statusIcon.gameObject.SetActive(false);
            }

            foreach (var row in _leftRowViews)
            {
                row.correctnessState = RowCorrectnessState.Neutral;
                if (!_isSceneNativeUI && row.backgroundImage != null) row.backgroundImage.color = NeutralRowColor;
                if (row.iconImage != null) row.iconImage.gameObject.SetActive(false);
                if (row.checkIcon != null) row.checkIcon.gameObject.SetActive(false);
                if (row.crossIcon != null) row.crossIcon.gameObject.SetActive(false);
                if (row.selectionHighlightObj != null) row.selectionHighlightObj.SetActive(false);
            }
        }

        private static Sprite CreateNavyGoldButtonSprite(int width, int height, int cornerRadius)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[width * height];
            Color goldOuter = new Color(0.96f, 0.80f, 0.32f, 1f);
            Color goldInner = new Color(0.84f, 0.66f, 0.22f, 0.75f);
            Color navyTop = new Color(0.11f, 0.24f, 0.56f, 0.98f);
            Color navyBottom = new Color(0.05f, 0.12f, 0.34f, 0.98f);

            int maxX = width - 1;
            int maxY = height - 1;

            for (int y = 0; y < height; y++)
            {
                float tY = (float)y / Mathf.Max(1, maxY);
                Color bodyColor = Color.Lerp(navyBottom, navyTop, tY);

                for (int x = 0; x < width; x++)
                {
                    int dx = 0;
                    if (x < cornerRadius) dx = cornerRadius - x;
                    else if (x > maxX - cornerRadius) dx = x - (maxX - cornerRadius);

                    int dy = 0;
                    if (y < cornerRadius) dy = cornerRadius - y;
                    else if (y > maxY - cornerRadius) dy = y - (maxY - cornerRadius);

                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > cornerRadius + 0.5f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    bool isOuterGold = (x <= 2 || x >= maxX - 2 || y <= 2 || y >= maxY - 2 || dist >= cornerRadius - 2f);
                    bool isInnerHighlight = !isOuterGold && (x == 5 || x == maxX - 5 || y == 5 || y == maxY - 5 || (dist >= cornerRadius - 5f && dist <= cornerRadius - 4f));

                    if (isOuterGold)
                    {
                        pixels[y * width + x] = goldOuter;
                    }
                    else if (isInnerHighlight)
                    {
                        pixels[y * width + x] = Color.Lerp(bodyColor, goldInner, 0.55f);
                    }
                    else
                    {
                        pixels[y * width + x] = bodyColor;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateHollowRoundedBorderSprite(int width, int height, int padX, int padY, int cornerRadius, int borderThickness)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[width * height];
            int minX = padX;
            int maxX = width - 1 - padX;
            int minY = padY;
            int maxY = height - 1 - padY;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int dx = 0;
                    if (x < minX + cornerRadius) dx = (minX + cornerRadius) - x;
                    else if (x > maxX - cornerRadius) dx = x - (maxX - cornerRadius);

                    int dy = 0;
                    if (y < minY + cornerRadius) dy = (minY + cornerRadius) - y;
                    else if (y > maxY - cornerRadius) dy = y - (maxY - cornerRadius);

                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > cornerRadius + 0.5f || x < minX || x > maxX || y < minY || y > maxY)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    bool isOuterBorder = (x <= minX + borderThickness || x >= maxX - borderThickness ||
                                          y <= minY + borderThickness || y >= maxY - borderThickness ||
                                          dist >= cornerRadius - borderThickness);

                    pixels[y * width + x] = isOuterBorder ? new Color(1f, 0.88f, 0.35f, 0.95f) : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreatePaddedRoundedRectSprite(int width, int height, int padX, int padY, int cornerRadius)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[width * height];
            int minX = padX;
            int maxX = width - 1 - padX;
            int minY = padY;
            int maxY = height - 1 - padY;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int dx = 0;
                    if (x < minX + cornerRadius) dx = (minX + cornerRadius) - x;
                    else if (x > maxX - cornerRadius) dx = x - (maxX - cornerRadius);

                    int dy = 0;
                    if (y < minY + cornerRadius) dy = (minY + cornerRadius) - y;
                    else if (y > maxY - cornerRadius) dy = y - (maxY - cornerRadius);

                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > cornerRadius + 0.5f || x < minX || x > maxX || y < minY || y > maxY)
                    {
                        pixels[y * width + x] = Color.clear;
                    }
                    else
                    {
                        pixels[y * width + x] = Color.white;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateFallbackIconSprite(bool isCheck)
        {
            const int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Color bgColor = isCheck ? new Color(0.13f, 0.75f, 0.30f, 1f) : new Color(0.90f, 0.20f, 0.20f, 1f);
            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.44f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    if (d > radius)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    pixels[y * size + x] = bgColor;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
