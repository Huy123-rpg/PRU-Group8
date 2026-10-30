using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển logic hiển thị và duyệt bài cho Scene 20. Detail Result (Detail Review):
    /// - Nhận dữ liệu snapshot chính xác từ PhysicsGameSession.Instance.CurrentQuizAttempt.
    /// - Tuyệt đối READ-ONLY: Không random lại, không tính điểm lại, không ghi đè save stats.
    /// - Xóa toàn bộ trạng thái placeholder tĩnh (baked sprites / floating icons) ngay tại Awake.
    /// - Danh sách 10 câu bên trái: Render hoàn toàn từ dữ liệu attempt (so sánh selectedAnswerIndex == correctAnswerIndex,
    ///   kiểm tra nhất quán với wasCorrect). Tách biệt hoàn toàn trạng thái Đúng/Sai (màu nền + icon ✓/✕) với trạng thái
    ///   đang chọn câu hỏi (viền sáng + scale).
    /// - Trung tâm: Cấu trúc UI tường minh bên trong khung giấy da (QuestionText + AnswerRow_A..D), mỗi hàng gồm
    ///   OptionText, StatusIcon (✓/✕), StatusLabel ("Đáp án đúng" / "Bạn chọn").
    /// - Phải trên: Render explanation từ attempt.answers[index].explanation.
    /// - Phải dưới: Giữ an toàn cho Kiến thức cần nhớ (để trống nếu keyKnowledge rỗng).
    /// - Direct Play Fallback: Hiển thị trạng thái trung tính an toàn và thông báo "Chưa có dữ liệu bài làm để xem lời giải."
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

        [Header("=== RIGHT PANELS ===")]
        [SerializeField] private TextMeshProUGUI explanationText;
        [SerializeField] private TextMeshProUGUI keyKnowledgeText;

        [Header("=== BOTTOM NAVIGATION ===")]
        [SerializeField] private Button btnPrev;
        [SerializeField] private Button btnNext;
        [SerializeField] private Button btnBackToResult;
        [SerializeField] private TextMeshProUGUI bottomCounterText;

        [Header("=== LEFT QUESTION LIST (10 ITEMS) ===")]
        [SerializeField] private List<Image> leftQuestionRowImages = new List<Image>();

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
        private TMP_FontAsset _defaultFont;
        private bool _isUIInitialized = false;

        private static readonly string[] AnswerPrefixes = { "A", "B", "C", "D" };

        // Màu sắc chuẩn cho Left Question List (Correctness vs Neutral)
        public static readonly Color CorrectRowColor = new Color(0.14f, 0.56f, 0.24f, 0.98f);
        public static readonly Color WrongRowColor = new Color(0.74f, 0.16f, 0.16f, 0.98f);
        public static readonly Color NeutralRowColor = new Color(0.10f, 0.18f, 0.34f, 0.92f);

        // Màu sắc chuẩn cho Center Answer Rows (Neutral / WrongSelected / CorrectAnswer)
        public static readonly Color NeutralAnswerRowBgColor = new Color(0.93f, 0.87f, 0.74f, 0.55f);
        public static readonly Color WrongAnswerRowBgColor = new Color(0.98f, 0.80f, 0.80f, 0.95f);
        public static readonly Color CorrectAnswerRowBgColor = new Color(0.78f, 0.95f, 0.82f, 0.95f);

        // Read-only properties phục vụ kiểm thử logic runtime
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
            AutoBindUIReferences();
            ResetAndHideStaticPlaceholders();
            BuildLeftRowRuntimeViews();
            BuildCenterAnswerRuntimeViews();
            EnsureBackToResultButton();
            FixRaycastBlockers();
            SetupBottomButtons();
            SetupLeftRowButtons();

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
                    if (x < minX || x > maxX || y < minY || y > maxY)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    int dx = 0;
                    if (x < minX + cornerRadius) dx = (minX + cornerRadius) - x;
                    else if (x > maxX - cornerRadius) dx = x - (maxX - cornerRadius);

                    int dy = 0;
                    if (y < minY + cornerRadius) dy = (minY + cornerRadius) - y;
                    else if (y > maxY - cornerRadius) dy = y - (maxY - cornerRadius);

                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > cornerRadius + 0.5f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    bool isNearOuterEdge =
                        (x < minX + borderThickness) ||
                        (x > maxX - borderThickness) ||
                        (y < minY + borderThickness) ||
                        (y > maxY - borderThickness) ||
                        (dist >= cornerRadius - borderThickness + 0.5f);

                    pixels[y * width + x] = isNearOuterEdge ? Color.white : Color.clear;
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
                    if (x < minX || x > maxX || y < minY || y > maxY)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    int dx = 0;
                    if (x < minX + cornerRadius) dx = (minX + cornerRadius) - x;
                    else if (x > maxX - cornerRadius) dx = x - (maxX - cornerRadius);

                    int dy = 0;
                    if (y < minY + cornerRadius) dy = (minY + cornerRadius) - y;
                    else if (y > maxY - cornerRadius) dy = y - (maxY - cornerRadius);

                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > cornerRadius + 0.5f)
                    {
                        pixels[y * width + x] = Color.clear;
                    }
                    else
                    {
                        bool isBorder = (x <= minX + 2 || x >= maxX - 2 || y <= minY + 2 || y >= maxY - 2 || dist >= cornerRadius - 1.5f);
                        pixels[y * width + x] = isBorder
                            ? new Color(1f, 0.95f, 0.78f, 0.95f)
                            : Color.white;
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

                    bool isSymbol = false;
                    float nx = (x - center.x) / radius;
                    float ny = (y - center.y) / radius;

                    if (isCheck)
                    {
                        // Dấu ✓
                        float distSeg1 = DistanceToSegment(new Vector2(nx, ny), new Vector2(-0.45f, -0.05f), new Vector2(-0.10f, -0.40f));
                        float distSeg2 = DistanceToSegment(new Vector2(nx, ny), new Vector2(-0.10f, -0.40f), new Vector2(0.48f, 0.38f));
                        isSymbol = (distSeg1 < 0.14f || distSeg2 < 0.14f);
                    }
                    else
                    {
                        // Dấu ✕
                        float distDiag1 = DistanceToSegment(new Vector2(nx, ny), new Vector2(-0.38f, -0.38f), new Vector2(0.38f, 0.38f));
                        float distDiag2 = DistanceToSegment(new Vector2(nx, ny), new Vector2(-0.38f, 0.38f), new Vector2(0.38f, -0.38f));
                        isSymbol = (distDiag1 < 0.14f || distDiag2 < 0.14f);
                    }

                    pixels[y * size + x] = isSymbol ? Color.white : bgColor;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(0.0001f, ab.sqrMagnitude));
            return Vector2.Distance(p, a + ab * t);
        }

        private void AutoBindUIReferences()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindAnyObjectByType<Canvas>();
            }
            if (canvas == null) return;

            // 1. Tìm TextMeshProUGUI
            TextMeshProUGUI[] tmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                if (t == null || t.GetComponentInParent<Button>() != null) continue;

                string text = t.text != null ? t.text.Trim() : "";
                Vector2 pos = t.rectTransform.anchoredPosition;

                if (centerQuestionText == null && (t.gameObject.name == "QuestionText" || text.Contains("A.") || text.Contains("B.") || (pos.x > -150f && pos.x < 150f && pos.y > 100f)))
                {
                    centerQuestionText = t;
                }
                else if (explanationText == null && pos.x > 350f && pos.y > -50f && pos.y < 200f)
                {
                    explanationText = t;
                }
                else if (keyKnowledgeText == null && pos.x > 350f && pos.y < -150f)
                {
                    keyKnowledgeText = t;
                }
                else if (bottomCounterText == null && (text.StartsWith("Câu ") || text.Contains("/10") || text.Contains("/")) && pos.y < -350f)
                {
                    bottomCounterText = t;
                }
            }

            if (_defaultFont == null)
            {
                if (centerQuestionText != null && centerQuestionText.font != null)
                    _defaultFont = centerQuestionText.font;
                else if (explanationText != null && explanationText.font != null)
                    _defaultFont = explanationText.font;
                else if (bottomCounterText != null && bottomCounterText.font != null)
                    _defaultFont = bottomCounterText.font;
            }

            // 2. Tìm Buttons điều hướng đáy
            Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                if (b == null) continue;
                if (b.gameObject.name == "Btn_BackToResult")
                {
                    if (btnBackToResult == null) btnBackToResult = b;
                    continue;
                }

                Vector2 pos = b.GetComponent<RectTransform>().anchoredPosition;

                if (pos.x < 0f && pos.x > -500f && pos.y < -350f && btnPrev == null)
                {
                    btnPrev = b;
                }
                else if (pos.x > 0f && pos.y < -350f && btnNext == null)
                {
                    btnNext = b;
                }
            }

            // 3. Tìm Parchment Panel, Floating Placeholder Icons, và 10 Left Question Rows
            Image[] images = canvas.GetComponentsInChildren<Image>(true);
            List<Image> rowCandidates = new List<Image>();

            foreach (var img in images)
            {
                if (img == null) continue;
                if (img == btnPrev?.GetComponent<Image>() ||
                    img == btnNext?.GetComponent<Image>() ||
                    img == btnBackToResult?.GetComponent<Image>()) continue;
                if (img.transform.parent != canvas.transform) continue;

                Vector2 pos = img.rectTransform.anchoredPosition;
                Vector2 size = img.rectTransform.sizeDelta;

                // Khung giấy da trung tâm (pos.x ~ 206.6, pos.y ~ -136.0, size ~ 1418x926)
                if (parchmentPanelRect == null && size.x > 1200f && size.y > 700f && pos.x > 0f)
                {
                    parchmentPanelRect = img.rectTransform;
                    continue;
                }

                // Floating placeholder checkmark icon (pos ~ 271.22, 40.78, size ~ 100x100)
                if (checkmarkIconObj == null && pos.x > 200f && pos.x < 350f && pos.y > 0f && pos.y < 100f && size.x <= 130f)
                {
                    checkmarkIconObj = img.gameObject;
                    if (img.sprite != null) _checkSprite = img.sprite;
                    continue;
                }

                // Floating placeholder cross icon (pos ~ 281.00, -50.00, size ~ 100x100)
                if (crossIconObj == null && pos.x > 200f && pos.x < 350f && pos.y < 0f && pos.y > -100f && size.x <= 130f)
                {
                    crossIconObj = img.gameObject;
                    if (img.sprite != null) _crossSprite = img.sprite;
                    continue;
                }

                // 10 Left Question Rows:
                // Loại trừ hoàn toàn header "Danh sách câu" (pos.y = 452.26, size.x = 384.65)
                if (pos.x > -850f && pos.x < -580f && pos.y < 380f && size.x < 320f && size.y < 110f)
                {
                    rowCandidates.Add(img);
                }
            }

            if (checkmarkIconObj != null && _checkSprite == null)
            {
                var cImg = checkmarkIconObj.GetComponent<Image>();
                if (cImg != null) _checkSprite = cImg.sprite;
            }

            if (crossIconObj != null && _crossSprite == null)
            {
                var xImg = crossIconObj.GetComponent<Image>();
                if (xImg != null) _crossSprite = xImg.sprite;
            }

            if (_checkSprite == null) _checkSprite = CreateFallbackIconSprite(true);
            if (_crossSprite == null) _crossSprite = CreateFallbackIconSprite(false);

            // Nếu leftQuestionRowImages chưa đủ hoặc chứa nhầm header "Danh sách câu", chuẩn hóa lại đúng 10 hàng Q1..Q10
            bool needsRebindRows = (leftQuestionRowImages == null || leftQuestionRowImages.Count != 10);
            if (!needsRebindRows)
            {
                for (int i = 0; i < leftQuestionRowImages.Count; i++)
                {
                    var r = leftQuestionRowImages[i];
                    if (r == null || r.rectTransform.sizeDelta.x > 320f || r.rectTransform.anchoredPosition.y > 380f)
                    {
                        needsRebindRows = true;
                        break;
                    }
                }
            }

            if (needsRebindRows && rowCandidates.Count > 0)
            {
                // Chuẩn hóa tâm thực tế theo Y trước khi sort để Q1..Q10 luôn đúng thứ tự từ trên xuống dưới
                rowCandidates.Sort((a, b) => GetVisualCenterY(b.rectTransform).CompareTo(GetVisualCenterY(a.rectTransform)));
                leftQuestionRowImages = rowCandidates;
            }
        }

        private static float GetVisualCenterY(RectTransform rt)
        {
            if (rt == null) return 0f;
            return rt.anchoredPosition.y + (0.5f - rt.pivot.y) * rt.sizeDelta.y;
        }

        private static void NormalizeRectTransformPivotToCenter(RectTransform rt)
        {
            if (rt == null) return;
            Vector2 targetPivot = new Vector2(0.5f, 0.5f);
            if (Vector2.Distance(rt.pivot, targetPivot) > 0.001f)
            {
                Vector2 deltaPivot = targetPivot - rt.pivot;
                rt.anchoredPosition += new Vector2(deltaPivot.x * rt.sizeDelta.x, deltaPivot.y * rt.sizeDelta.y);
                rt.pivot = targetPivot;
            }
        }

        /// <summary>
        /// Requirement 16: Ngay tại Awake/Start trước khi render attempt, ẩn/reset toàn bộ các đối tượng placeholder tĩnh.
        /// </summary>
        private void ResetAndHideStaticPlaceholders()
        {
            // 1. Ẩn ngay lập tức 2 icon placeholder nổi ở trung tâm (A -> green ✓, B -> red ✕)
            if (checkmarkIconObj != null) checkmarkIconObj.SetActive(false);
            if (crossIconObj != null) crossIconObj.SetActive(false);

            // 2. Xóa các text placeholder tĩnh ("... etc", "Câu 1/10: Hiện tượng...")
            if (centerQuestionText != null)
            {
                centerQuestionText.text = "";
                centerQuestionText.raycastTarget = false;
            }

            if (explanationText != null)
            {
                explanationText.text = "";
                explanationText.raycastTarget = false;
                RectTransform expRt = explanationText.rectTransform;
                NormalizeRectTransformPivotToCenter(expRt);
                // Khung GIẢI THÍCH có tâm (573.15, 43.97), cao 410.36 (top = +249.15, bottom = -161.21).
                // Thanh tiêu đề nâu "GIẢI THÍCH" nằm từ +249 xuống đến khoảng +115.
                // Đặt vùng text từ Y_top = +92 xuống Y_bottom = -136 (tâm Y = -22, cao = 228) để không bao giờ đè lên tiêu đề.
                expRt.anchoredPosition = new Vector2(573f, -22f);
                expRt.sizeDelta = new Vector2(386f, 228f);
                explanationText.margin = Vector4.zero;
                explanationText.color = new Color(0.12f, 0.14f, 0.36f, 1f);
            }

            if (keyKnowledgeText != null)
            {
                keyKnowledgeText.text = "";
                keyKnowledgeText.raycastTarget = false;
                RectTransform kkRt = keyKnowledgeText.rectTransform;
                NormalizeRectTransformPivotToCenter(kkRt);
                // Khung KIẾN THỨC CẦN NHỚ có tâm (568.26, -277.23), cao 232.04 (top = -161.21, bottom = -393.25).
                // Thanh tiêu đề nâu nằm từ -161 xuống khoảng -240. Đặt vùng text từ Y_top = -250 xuống Y_bottom = -374 (tâm Y = -312, cao = 124).
                kkRt.anchoredPosition = new Vector2(568f, -312f);
                kkRt.sizeDelta = new Vector2(386f, 124f);
                keyKnowledgeText.margin = Vector4.zero;
                keyKnowledgeText.color = new Color(0.12f, 0.14f, 0.36f, 1f);
            }

            if (bottomCounterText != null)
            {
                bottomCounterText.raycastTarget = false;
                RectTransform counterRt = bottomCounterText.rectTransform;
                NormalizeRectTransformPivotToCenter(counterRt);
                if (counterRt.sizeDelta.x < 260f)
                {
                    counterRt.sizeDelta = new Vector2(280f, counterRt.sizeDelta.y);
                }
                bottomCounterText.alignment = TextAlignmentOptions.Center;
            }
        }

        private void BuildLeftRowRuntimeViews()
        {
            _leftRowViews.Clear();

            for (int i = 0; i < leftQuestionRowImages.Count; i++)
            {
                Image rowImg = leftQuestionRowImages[i];
                if (rowImg == null) continue;

                rowImg.gameObject.name = $"QuestionRow_Q{i + 1}";
                NormalizeRectTransformPivotToCenter(rowImg.rectTransform);
                // Đồng bộ trục X = -720 để cả 10 hàng thẳng cột tuyệt đối
                rowImg.rectTransform.anchoredPosition = new Vector2(-720f, rowImg.rectTransform.anchoredPosition.y);

                // Loại bỏ sprite placeholder có baked màu xanh/đỏ và baked ✓/✕
                rowImg.sprite = _neutralRowSprite;
                rowImg.type = Image.Type.Simple;
                rowImg.color = NeutralRowColor;
                rowImg.raycastTarget = true;

                // Xóa IndicatorIcon cũ nếu có
                Transform legacyIcon = rowImg.transform.Find("IndicatorIcon");
                if (legacyIcon != null)
                {
                    DestroyImmediate(legacyIcon.gameObject);
                }

                // 1. SelectionHighlight: Khung viền rỗng (hollow border) bao quanh mép ngoài.
                // Phần ruột trong suốt 100% (Color.clear) để tuyệt đối KHÔNG phủ màu vàng đè lên nền Đỏ/Xanh của câu hỏi.
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
                    hRt.pivot = new Vector2(0.5f, 0.5f);
                    hRt.anchoredPosition = Vector2.zero;
                    hRt.sizeDelta = new Vector2(6f, 6f);

                    Image hImg = highlightGo.AddComponent<Image>();
                    hImg.sprite = _selectionBorderSprite;
                    hImg.color = new Color(1f, 0.92f, 0.28f, 1f);
                    hImg.raycastTarget = false;
                }
                highlightGo.SetActive(false);

                // 2. RowLabel ("Câu 1" .. "Câu 10")
                Transform labelTr = rowImg.transform.Find("RowLabel");
                TextMeshProUGUI labelTmp;
                if (labelTr != null)
                {
                    labelTmp = labelTr.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                    GameObject labelGo = new GameObject("RowLabel");
                    labelGo.transform.SetParent(rowImg.transform, false);
                    RectTransform lRt = labelGo.AddComponent<RectTransform>();
                    lRt.anchorMin = new Vector2(0f, 0f);
                    lRt.anchorMax = new Vector2(1f, 1f);
                    lRt.pivot = new Vector2(0.5f, 0.5f);
                    lRt.anchoredPosition = new Vector2(-18f, 0f);
                    lRt.sizeDelta = new Vector2(-84f, -24f);

                    labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
                }

                if (_defaultFont != null) labelTmp.font = _defaultFont;
                labelTmp.text = $"Câu {i + 1}";
                labelTmp.fontSize = 26f;
                labelTmp.fontStyle = FontStyles.Bold;
                labelTmp.color = Color.white;
                labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
                labelTmp.raycastTarget = false;

                // 3. CheckIcon (✓)
                Image checkImg = EnsureRowStatusChildIcon(rowImg.transform, "CheckIcon", _checkSprite);
                checkImg.gameObject.SetActive(false);

                // 4. CrossIcon (✕)
                Image crossImg = EnsureRowStatusChildIcon(rowImg.transform, "CrossIcon", _crossSprite);
                crossImg.gameObject.SetActive(false);

                // 5. Button (Transition.None để không làm biến đổi màu đúng/sai của rowImg)
                Button btn = rowImg.GetComponent<Button>();
                if (btn == null)
                {
                    btn = rowImg.gameObject.AddComponent<Button>();
                }
                btn.transition = Selectable.Transition.None;
                Navigation nav = btn.navigation;
                nav.mode = Navigation.Mode.None;
                btn.navigation = nav;

                // Gỡ UIButtonFeedback trên row trái để không ghi đè localScale của highlight
                UIButtonFeedback feedback = rowImg.GetComponent<UIButtonFeedback>();
                if (feedback != null)
                {
                    DestroyImmediate(feedback);
                }

                _leftRowViews.Add(new LeftQuestionRowView
                {
                    questionIndex = i,
                    backgroundImage = rowImg,
                    questionLabel = labelTmp,
                    checkIcon = checkImg,
                    crossIcon = crossImg,
                    selectionHighlightObj = highlightGo,
                    rowButton = btn,
                    correctnessState = RowCorrectnessState.Neutral,
                    isSelectedHighlight = false
                });
            }
        }

        private static Image EnsureRowStatusChildIcon(Transform rowParent, string childName, Sprite iconSprite)
        {
            Transform existing = rowParent.Find(childName);
            Image iconImg;
            if (existing != null)
            {
                iconImg = existing.GetComponent<Image>();
                if (iconImg == null) iconImg = existing.gameObject.AddComponent<Image>();
            }
            else
            {
                GameObject iconGo = new GameObject(childName);
                iconGo.transform.SetParent(rowParent, false);
                RectTransform rt = iconGo.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(1f, 0.5f);
                rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(-42f, 0f);
                rt.sizeDelta = new Vector2(40f, 40f);

                iconImg = iconGo.AddComponent<Image>();
            }

            iconImg.sprite = iconSprite;
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;
            iconImg.raycastTarget = false;
            return iconImg;
        }

        /// <summary>
        /// Requirement 8 & 9: Tạo cấu trúc UI trung tâm bên trong khung giấy da hiện có:
        /// - QuestionText
        /// - AnswerRow_A, AnswerRow_B, AnswerRow_C, AnswerRow_D
        /// Mỗi AnswerRow chứa: OptionText, StatusIcon, StatusLabel (raycastTarget = false).
        /// </summary>
        private void BuildCenterAnswerRuntimeViews()
        {
            _centerAnswerRows.Clear();

            Transform container = parchmentPanelRect != null ? parchmentPanelRect : transform;
            Vector2 parchmentOffset = parchmentPanelRect != null ? parchmentPanelRect.anchoredPosition : Vector2.zero;

            if (parchmentPanelRect != null)
            {
                var parchmentImg = parchmentPanelRect.GetComponent<Image>();
                if (parchmentImg != null) parchmentImg.raycastTarget = false;
            }

            // 1. Chuẩn hóa QuestionText bên trong khung giấy da
            if (centerQuestionText == null)
            {
                GameObject qGo = new GameObject("QuestionText");
                qGo.transform.SetParent(container, false);
                qGo.AddComponent<RectTransform>();
                centerQuestionText = qGo.AddComponent<TextMeshProUGUI>();
            }
            else if (parchmentPanelRect != null && centerQuestionText.transform.parent != parchmentPanelRect)
            {
                centerQuestionText.transform.SetParent(parchmentPanelRect, true);
            }

            centerQuestionText.gameObject.name = "QuestionText";
            if (_defaultFont != null) centerQuestionText.font = _defaultFont;

            RectTransform qRt = centerQuestionText.rectTransform;
            qRt.anchorMin = new Vector2(0.5f, 0.5f);
            qRt.anchorMax = new Vector2(0.5f, 0.5f);
            qRt.pivot = new Vector2(0.5f, 0.5f);
            // Vị trí trong Canvas: (-31.685, 165) -> quy đổi theo parchmentPanelRect
            qRt.anchoredPosition = new Vector2(-31.685f - parchmentOffset.x, 165f - parchmentOffset.y);
            qRt.sizeDelta = new Vector2(706f, 130f);

            centerQuestionText.enableAutoSizing = true;
            centerQuestionText.fontSizeMin = 18f;
            centerQuestionText.fontSizeMax = 28f;
            centerQuestionText.fontStyle = FontStyles.Bold;
            centerQuestionText.color = new Color(0.10f, 0.12f, 0.38f, 1f);
            centerQuestionText.alignment = TextAlignmentOptions.TopLeft;
            centerQuestionText.textWrappingMode = TextWrappingModes.Normal;
            centerQuestionText.overflowMode = TextOverflowModes.Overflow;
            centerQuestionText.raycastTarget = false;

            // 2. Tạo 4 hàng đáp án AnswerRow_A .. AnswerRow_D theo đúng vị trí thị giác hiện có
            // Trong Canvas space: Y = +40.78 (A), -50.0 (B), -140.78 (C), -231.56 (D), X = -31.685
            float[] canvasRowYs = { 40.78f, -50.00f, -140.78f, -231.56f };

            for (int i = 0; i < 4; i++)
            {
                string rowName = $"AnswerRow_{AnswerPrefixes[i]}";
                Transform existingRow = container.Find(rowName);
                GameObject rowGo;
                if (existingRow != null)
                {
                    rowGo = existingRow.gameObject;
                }
                else
                {
                    rowGo = new GameObject(rowName);
                    rowGo.transform.SetParent(container, false);
                }

                RectTransform rowRt = rowGo.GetComponent<RectTransform>();
                if (rowRt == null) rowRt = rowGo.AddComponent<RectTransform>();
                rowRt.anchorMin = new Vector2(0.5f, 0.5f);
                rowRt.anchorMax = new Vector2(0.5f, 0.5f);
                rowRt.pivot = new Vector2(0.5f, 0.5f);
                rowRt.anchoredPosition = new Vector2(-31.685f - parchmentOffset.x, canvasRowYs[i] - parchmentOffset.y);
                rowRt.sizeDelta = new Vector2(706f, 76f);

                Image rowBg = rowGo.GetComponent<Image>();
                if (rowBg == null) rowBg = rowGo.AddComponent<Image>();
                rowBg.sprite = _answerRowSprite;
                rowBg.type = Image.Type.Simple;
                rowBg.color = NeutralAnswerRowBgColor;
                rowBg.raycastTarget = false;

                // Badge nền (A/B/C/D)
                Image badgeBg = EnsureChildImage(rowGo.transform, "BadgeBg", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(48f, 48f));
                badgeBg.sprite = _answerRowSprite;
                badgeBg.color = new Color(0.12f, 0.18f, 0.28f, 1f);
                badgeBg.raycastTarget = false;

                TextMeshProUGUI badgeTmp = EnsureChildTmp(badgeBg.transform, "BadgeText", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                if (_defaultFont != null) badgeTmp.font = _defaultFont;
                badgeTmp.text = AnswerPrefixes[i];
                badgeTmp.fontSize = 24f;
                badgeTmp.fontStyle = FontStyles.Bold;
                badgeTmp.color = new Color(1f, 0.90f, 0.45f, 1f);
                badgeTmp.alignment = TextAlignmentOptions.Center;
                badgeTmp.raycastTarget = false;

                // TMP Option Label/Text
                TextMeshProUGUI optionTmp = EnsureChildTmp(rowGo.transform, "OptionText", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(-42f, 0f), new Vector2(-248f, -12f));
                if (_defaultFont != null) optionTmp.font = _defaultFont;
                optionTmp.enableAutoSizing = true;
                optionTmp.fontSizeMin = 15f;
                optionTmp.fontSizeMax = 23f;
                optionTmp.fontSize = 21f;
                optionTmp.color = new Color(0.12f, 0.16f, 0.24f, 1f);
                optionTmp.alignment = TextAlignmentOptions.MidlineLeft;
                optionTmp.textWrappingMode = TextWrappingModes.Normal;
                optionTmp.raycastTarget = false;

                // Status Icon (✓ hoặc ✕)
                Image statusIcon = EnsureChildImage(rowGo.transform, "StatusIcon", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-148f, 0f), new Vector2(36f, 36f));
                statusIcon.preserveAspect = true;
                statusIcon.raycastTarget = false;
                statusIcon.gameObject.SetActive(false);

                // Status Label ("Đáp án đúng" hoặc "Bạn chọn")
                TextMeshProUGUI statusLabel = EnsureChildTmp(rowGo.transform, "StatusLabel", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-64f, 0f), new Vector2(120f, -12f));
                if (_defaultFont != null) statusLabel.font = _defaultFont;
                statusLabel.enableAutoSizing = true;
                statusLabel.fontSizeMin = 14f;
                statusLabel.fontSizeMax = 20f;
                statusLabel.fontSize = 18f;
                statusLabel.fontStyle = FontStyles.Bold;
                statusLabel.alignment = TextAlignmentOptions.MidlineLeft;
                statusLabel.textWrappingMode = TextWrappingModes.Normal;
                statusLabel.raycastTarget = false;
                statusLabel.text = "";
                statusLabel.gameObject.SetActive(false);

                _centerAnswerRows.Add(new CenterAnswerRowView
                {
                    optionIndex = i,
                    rootObject = rowGo,
                    backgroundImage = rowBg,
                    badgeBackground = badgeBg,
                    badgeText = badgeTmp,
                    optionText = optionTmp,
                    statusIcon = statusIcon,
                    statusLabel = statusLabel,
                    visualState = AnswerOptionVisualState.Neutral
                });
            }
        }

        private static Image EnsureChildImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            Transform existing = parent.Find(name);
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = new GameObject(name);
                go.transform.SetParent(parent, false);
            }

            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Image img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.raycastTarget = false;
            return img;
        }

        private static TextMeshProUGUI EnsureChildTmp(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            Transform existing = parent.Find(name);
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = new GameObject(name);
                go.transform.SetParent(parent, false);
            }

            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            return tmp;
        }

        /// <summary>
        /// Đảm bảo nút hiển thị "QUAY LẠI KẾT QUẢ" nằm ở góc dưới bên trái vùng giấy da (parchment),
        /// không đè lên "Câu trước" (btnPrev) hay hàng đáp án D, dùng chuẩn style Navy/Gold của Scene 20.
        /// Raycast: Button Image = true, Button child TMP = false.
        /// </summary>
        private void EnsureBackToResultButton()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            Transform parentTransform = canvas != null ? canvas.transform : transform;

            if (btnBackToResult == null)
            {
                Transform existingTr = parentTransform.Find("Btn_BackToResult");
                if (existingTr != null)
                {
                    btnBackToResult = existingTr.GetComponent<Button>();
                    if (btnBackToResult == null)
                    {
                        btnBackToResult = existingTr.gameObject.AddComponent<Button>();
                    }
                }
            }

            GameObject btnGo;
            if (btnBackToResult != null)
            {
                btnGo = btnBackToResult.gameObject;
                if (btnGo.transform.parent != parentTransform)
                {
                    btnGo.transform.SetParent(parentTransform, false);
                }
            }
            else
            {
                btnGo = new GameObject("Btn_BackToResult");
                btnGo.transform.SetParent(parentTransform, false);
                btnBackToResult = btnGo.AddComponent<Button>();
            }

            btnGo.name = "Btn_BackToResult";
            btnGo.SetActive(true);

            RectTransform rt = btnGo.GetComponent<RectTransform>();
            if (rt == null) rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            // Nằm gọn ở góc dưới bên trái của vùng giấy da (parchment):
            // Dưới AnswerRow_D (bottom = -269.56) và trên "Câu trước" (top = -386.10), hoàn toàn không chồng lấp
            rt.anchoredPosition = new Vector2(-249f, -332f);
            rt.sizeDelta = new Vector2(280f, 54f);
            rt.localScale = Vector3.one;

            Image btnImg = btnGo.GetComponent<Image>();
            if (btnImg == null) btnImg = btnGo.AddComponent<Image>();
            btnImg.sprite = _backButtonSprite;
            btnImg.type = Image.Type.Simple;
            btnImg.color = Color.white;
            btnImg.raycastTarget = true;

            btnBackToResult.targetGraphic = btnImg;
            btnBackToResult.interactable = true;

            var colors = btnBackToResult.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.88f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.selectedColor = Color.white;
            btnBackToResult.colors = colors;

            // Gỡ UIButtonFeedback nếu có để tránh xung đột IPointerDownHandler
            var feedback = btnGo.GetComponent<UIButtonFeedback>();
            if (feedback != null)
            {
                DestroyImmediate(feedback);
            }

            // Text con "QUAY LẠI KẾT QUẢ" (raycastTarget = false)
            Transform txtTr = btnGo.transform.Find("Text (TMP)") ?? btnGo.transform.Find("Text");
            GameObject txtGo;
            if (txtTr != null)
            {
                txtGo = txtTr.gameObject;
            }
            else
            {
                txtGo = new GameObject("Text (TMP)");
                txtGo.transform.SetParent(btnGo.transform, false);
            }

            RectTransform txtRt = txtGo.GetComponent<RectTransform>();
            if (txtRt == null) txtRt = txtGo.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.pivot = new Vector2(0.5f, 0.5f);
            txtRt.anchoredPosition = Vector2.zero;
            txtRt.sizeDelta = new Vector2(-20f, -8f);

            TextMeshProUGUI tmp = txtGo.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = txtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) tmp.font = _defaultFont;
            tmp.text = "QUAY LẠI KẾT QUẢ";
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 15f;
            tmp.fontSizeMax = 21f;
            tmp.fontSize = 20f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(1f, 0.94f, 0.68f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;
        }

        private void FixRaycastBlockers()
        {
            Button[] buttons = FindObjectsByType<Button>();
            foreach (var b in buttons)
            {
                if (b == null) continue;
                var img = b.GetComponent<Image>();
                if (img != null) img.raycastTarget = true;

                var childTmps = b.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in childTmps)
                {
                    if (t != null) t.raycastTarget = false;
                }
            }
        }

        private void SetupBottomButtons()
        {
            if (btnPrev != null)
            {
                ClearDefaultButtonLabel(btnPrev);
                btnPrev.onClick.RemoveAllListeners();
                btnPrev.onClick.AddListener(OnPrevClicked);
                if (btnPrev.GetComponent<UIButtonFeedback>() == null)
                {
                    btnPrev.gameObject.AddComponent<UIButtonFeedback>();
                }
            }

            if (btnNext != null)
            {
                ClearDefaultButtonLabel(btnNext);
                btnNext.onClick.RemoveAllListeners();
                btnNext.onClick.AddListener(OnNextClicked);
                if (btnNext.GetComponent<UIButtonFeedback>() == null)
                {
                    btnNext.gameObject.AddComponent<UIButtonFeedback>();
                }
            }

            if (btnBackToResult != null)
            {
                btnBackToResult.onClick.RemoveAllListeners();
                btnBackToResult.onClick.AddListener(OnReturnToResult);
            }
        }

        private static void ClearDefaultButtonLabel(Button btn)
        {
            if (btn == null) return;
            var tmps = btn.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                if (t != null && string.Equals(t.text != null ? t.text.Trim() : "", "Button", StringComparison.OrdinalIgnoreCase))
                {
                    t.text = "";
                    t.raycastTarget = false;
                }
            }
        }

        private void SetupLeftRowButtons()
        {
            for (int i = 0; i < _leftRowViews.Count; i++)
            {
                var rowView = _leftRowViews[i];
                if (rowView == null || rowView.rowButton == null) continue;

                int questionIndex = i;
                rowView.rowButton.onClick.RemoveAllListeners();
                rowView.rowButton.onClick.AddListener(() => ShowQuestion(questionIndex));
            }
        }

        public void LoadAttemptData()
        {
            EnsureInitialized();
            RenderReviewFromAttempt(PhysicsGameSession.Instance.CurrentQuizAttempt);
        }

        /// <summary>
        /// Hàm nạp và render dữ liệu QuizAttemptData (phục vụ cả runtime Play Mode lẫn Automated Tests).
        /// </summary>
        public void RenderReviewFromAttempt(QuizAttemptData attempt)
        {
            EnsureInitialized();

            _attemptData = attempt;
            InconsistencyErrorCount = 0;
            LastInconsistencyErrorMessage = null;

            // Requirement 16: Reset toàn bộ trạng thái trước khi render
            ResetAllVisualStatesToNeutral();

            // Requirement 17: Safe Direct Play Fallback (không có attempt -> neutral list, thông báo rõ ràng)
            if (_attemptData == null || _attemptData.answers == null || _attemptData.answers.Count == 0)
            {
                Debug.LogWarning("[QuizReviewController] Chưa có dữ liệu bài làm (Play trực tiếp Scene 20). Hiển thị fallback trung tính an toàn.");

                if (centerQuestionText != null)
                {
                    centerQuestionText.text = "Chưa có dữ liệu bài làm để xem lời giải.";
                }

                for (int i = 0; i < _centerAnswerRows.Count; i++)
                {
                    if (_centerAnswerRows[i].rootObject != null)
                    {
                        _centerAnswerRows[i].rootObject.SetActive(false);
                    }
                }

                if (explanationText != null) explanationText.text = "";
                if (keyKnowledgeText != null) keyKnowledgeText.text = "";
                if (btnPrev != null) btnPrev.interactable = false;
                if (btnNext != null) btnNext.interactable = false;
                if (bottomCounterText != null) bottomCounterText.text = "Câu 0/0";
                return;
            }

            _currentReviewIndex = 0;
            RenderQuestionList();
            ShowQuestion(0);
        }

        private void ResetAllVisualStatesToNeutral()
        {
            if (checkmarkIconObj != null) checkmarkIconObj.SetActive(false);
            if (crossIconObj != null) crossIconObj.SetActive(false);

            for (int i = 0; i < _leftRowViews.Count; i++)
            {
                var row = _leftRowViews[i];
                if (row == null) continue;

                row.correctnessState = RowCorrectnessState.Neutral;
                row.isSelectedHighlight = false;

                if (row.backgroundImage != null)
                {
                    row.backgroundImage.gameObject.SetActive(true);
                    row.backgroundImage.sprite = _neutralRowSprite;
                    row.backgroundImage.color = NeutralRowColor;
                    row.backgroundImage.transform.localScale = Vector3.one;
                }

                if (row.checkIcon != null) row.checkIcon.gameObject.SetActive(false);
                if (row.crossIcon != null) row.crossIcon.gameObject.SetActive(false);
                if (row.selectionHighlightObj != null) row.selectionHighlightObj.SetActive(false);
                if (row.rowButton != null) row.rowButton.interactable = false;
            }

            for (int i = 0; i < _centerAnswerRows.Count; i++)
            {
                ApplyCenterAnswerRowState(_centerAnswerRows[i], AnswerPrefixes[i], "", AnswerOptionVisualState.Neutral);
            }
        }

        /// <summary>
        /// Requirement 10 & 11:
        /// Kiểm tra selectedAnswerIndex (0..3) và correctAnswerIndex (0..3),
        /// tính computedCorrect = (selectedAnswerIndex == correctAnswerIndex).
        /// Nếu answer.wasCorrect != computedCorrect -> log ERROR "[QuizReview] Attempt inconsistency..."
        /// Luôn ưu tiên kết quả so sánh chỉ mục (computedCorrect) làm nguồn sự thật để render.
        /// </summary>
        public bool EvaluateAnswerCorrectness(QuizAttemptAnswer answer, int questionIndex, bool logInconsistency = true)
        {
            if (answer == null)
            {
                if (logInconsistency)
                {
                    RecordInconsistencyError($"[QuizReview] Attempt inconsistency at Q{questionIndex + 1}: answer record is null.");
                }
                return false;
            }

            if (!IsAnswerGraded(answer)) return false;

            bool validSelected = (answer.selectedAnswerIndex >= 0 && answer.selectedAnswerIndex <= 3);
            bool validCorrect = (answer.correctAnswerIndex >= 0 && answer.correctAnswerIndex <= 3);

            if (!validSelected || !validCorrect)
            {
                if (logInconsistency)
                {
                    RecordInconsistencyError(
                        $"[QuizReview] Attempt inconsistency at Q{questionIndex + 1}: out-of-range answer indices (selectedAnswerIndex={answer.selectedAnswerIndex}, correctAnswerIndex={answer.correctAnswerIndex}).");
                }
            }

            bool computedCorrect = validSelected && validCorrect && (answer.selectedAnswerIndex == answer.correctAnswerIndex);

            if (answer.wasCorrect != computedCorrect)
            {
                if (logInconsistency)
                {
                    RecordInconsistencyError(
                        $"[QuizReview] Attempt inconsistency at Q{questionIndex + 1}: wasCorrect={answer.wasCorrect} != computedCorrect={computedCorrect} (selectedAnswerIndex={answer.selectedAnswerIndex}, correctAnswerIndex={answer.correctAnswerIndex}). Rendering from index comparison.");
                }
            }

            return computedCorrect;
        }

        private void RecordInconsistencyError(string message)
        {
            InconsistencyErrorCount++;
            LastInconsistencyErrorMessage = message;
            Debug.LogError(message);
        }

        /// <summary>
        /// Requirement 3, 4, 5, 11, 16:
        /// Render danh sách 10 câu bên trái từ dữ liệu attempt thật:
        /// - isCorrect == true -> CORRECT visual (Green) + show ✓ + hide ✕
        /// - isCorrect == false -> WRONG visual (Red) + show ✕ + hide ✓
        /// </summary>
        public void RenderQuestionList()
        {
            EnsureInitialized();
            if (_attemptData == null || _attemptData.answers == null) return;

            int count = Mathf.Min(_leftRowViews.Count, _attemptData.answers.Count);

            for (int i = 0; i < _leftRowViews.Count; i++)
            {
                var row = _leftRowViews[i];
                if (row == null || row.backgroundImage == null) continue;

                if (i < count)
                {
                    row.backgroundImage.gameObject.SetActive(true);
                    row.backgroundImage.sprite = _neutralRowSprite;
                    if (row.rowButton != null) row.rowButton.interactable = true;

                    var ans = _attemptData.answers[i];
                    if (!IsAnswerGraded(ans))
                    {
                        row.correctnessState = RowCorrectnessState.Neutral;
                        row.backgroundImage.color = NeutralRowColor;
                        if (row.checkIcon != null) row.checkIcon.gameObject.SetActive(false);
                        if (row.crossIcon != null) row.crossIcon.gameObject.SetActive(false);
                        continue;
                    }

                    bool isCorrect = EvaluateAnswerCorrectness(ans, i, logInconsistency: false);

                    if (isCorrect)
                    {
                        row.correctnessState = RowCorrectnessState.Correct;
                        row.backgroundImage.color = CorrectRowColor;
                        if (row.checkIcon != null) row.checkIcon.gameObject.SetActive(true);
                        if (row.crossIcon != null) row.crossIcon.gameObject.SetActive(false);
                    }
                    else
                    {
                        row.correctnessState = RowCorrectnessState.Wrong;
                        row.backgroundImage.color = WrongRowColor;
                        if (row.checkIcon != null) row.checkIcon.gameObject.SetActive(false);
                        if (row.crossIcon != null) row.crossIcon.gameObject.SetActive(true);
                    }
                }
                else
                {
                    row.correctnessState = RowCorrectnessState.Neutral;
                    row.backgroundImage.gameObject.SetActive(false);
                    if (row.checkIcon != null) row.checkIcon.gameObject.SetActive(false);
                    if (row.crossIcon != null) row.crossIcon.gameObject.SetActive(false);
                    if (row.rowButton != null) row.rowButton.interactable = false;
                }
            }
        }

        private static bool IsAnswerGraded(QuizAttemptAnswer answer)
        {
            if (answer == null) return false;
            // Older saved multiple-choice attempts predate isGraded and deserialize it as false.
            // Their question text does not carry the written-question marker.
            bool legacyMultipleChoice = string.IsNullOrEmpty(answer.questionText)
                || !answer.questionText.StartsWith("[TỰ LUẬN", StringComparison.OrdinalIgnoreCase);
            return answer.isGraded || legacyMultipleChoice;
        }

        /// <summary>
        /// Hiển thị chi tiết câu hỏi theo chỉ mục xem lại (_currentReviewIndex).
        /// </summary>
        public void ShowQuestion(int index)
        {
            EnsureInitialized();
            if (_attemptData == null || _attemptData.answers == null || _attemptData.answers.Count == 0) return;

            int totalQ = _attemptData.answers.Count;
            _currentReviewIndex = Mathf.Clamp(index, 0, totalQ - 1);

            var answer = _attemptData.answers[_currentReviewIndex];

            // Requirement 10: Kiểm tra nhất quán dữ liệu khi ShowQuestion(index) chạy
            EvaluateAnswerCorrectness(answer, _currentReviewIndex, logInconsistency: true);

            // 1. Cập nhật Counter đáy
            if (bottomCounterText != null)
            {
                bottomCounterText.text = $"Câu {_currentReviewIndex + 1}/{totalQ}";
            }

            // 2. Cập nhật tương tác nút Câu trước / Câu tiếp theo ở biên
            if (btnPrev != null)
            {
                btnPrev.interactable = (_currentReviewIndex > 0);
            }
            if (btnNext != null)
            {
                btnNext.interactable = (_currentReviewIndex < totalQ - 1);
            }

            // 3. Requirement 14 & 15: Highlight câu đang chọn độc lập hoàn toàn với màu Đúng/Sai
            HighlightSelectedRow(_currentReviewIndex);

            // 4. Requirement 6, 7, 8, 9: Render câu hỏi & 4 hàng đáp án trung tâm
            RenderCenterQuestionDetail(answer, _currentReviewIndex + 1, totalQ);

            // 5. Requirement 12: Render GIẢI THÍCH (Right Top)
            if (explanationText != null)
            {
                explanationText.enableAutoSizing = true;
                explanationText.fontSizeMin = 14f;
                explanationText.fontSizeMax = 22f;
                explanationText.alignment = TextAlignmentOptions.TopLeft;
                explanationText.textWrappingMode = TextWrappingModes.Normal;

                if (!string.IsNullOrEmpty(answer.explanation))
                {
                    explanationText.text = answer.explanation;
                }
                else
                {
                    explanationText.text = "(Chưa có lời giải chi tiết cho câu hỏi này)";
                }
            }

            // 6. Requirement 13: KIẾN THỨC CẦN NHỚ (Right Bottom - riêng cho từng câu hỏi)
            if (keyKnowledgeText != null)
            {
                keyKnowledgeText.enableAutoSizing = true;
                keyKnowledgeText.fontSizeMin = 13f;
                keyKnowledgeText.fontSizeMax = 19f;
                keyKnowledgeText.alignment = TextAlignmentOptions.TopLeft;
                keyKnowledgeText.textWrappingMode = TextWrappingModes.Normal;

                string lessonId = _attemptData != null ? _attemptData.lessonId : "";
                string kk = PhysicsQuestionRepository.ResolveKeyKnowledgeForAnswer(answer, lessonId);
                keyKnowledgeText.text = !string.IsNullOrEmpty(kk) ? kk : "";
            }
        }

        /// <summary>
        /// Requirement 14 & 15: Highlight câu hỏi hiện tại bằng viền sáng + scale.
        /// Tuyệt đối KHÔNG ghi đè màu xanh/đỏ hoặc icon ✓/✕ của hàng bên trái.
        /// </summary>
        private void HighlightSelectedRow(int selectedIndex)
        {
            for (int i = 0; i < _leftRowViews.Count; i++)
            {
                var row = _leftRowViews[i];
                if (row == null || row.backgroundImage == null) continue;

                bool isCurrent = (i == selectedIndex);
                row.isSelectedHighlight = isCurrent;

                if (row.selectionHighlightObj != null)
                {
                    row.selectionHighlightObj.SetActive(isCurrent);
                }

                row.backgroundImage.transform.localScale = isCurrent
                    ? new Vector3(1.06f, 1.06f, 1f)
                    : Vector3.one;
            }
        }

        /// <summary>
        /// Requirement 6, 7, 8, 9:
        /// Render QuestionText và 4 hàng AnswerRow_A..D từ selectedAnswerIndex và correctAnswerIndex:
        /// - i == correctAnswerIndex -> GREEN + ✓ + "Đáp án đúng"
        /// - i == selectedAnswerIndex (khi chọn sai) -> RED + ✕ + "Bạn chọn"
        /// - Các đáp án khác -> Neutral (không icon, không nhãn trạng thái)
        /// </summary>
        private void RenderCenterQuestionDetail(QuizAttemptAnswer ans, int questionNum, int totalQ)
        {
            if (ans == null) return;

            bool isWritten = (PhysicsGameSession.Instance != null && PhysicsGameSession.Instance.SelectedMode == GameMode.Written)
                || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("TL");

            if (isWritten)
            {
                RenderWrittenQuestionDetail(ans, questionNum, totalQ);
                return;
            }

            if (centerQuestionText != null)
            {
                centerQuestionText.text = $"<b>Câu {questionNum}/{totalQ}:</b> {ans.questionText}";
            }

            for (int i = 0; i < 4 && i < _centerAnswerRows.Count; i++)
            {
                var row = _centerAnswerRows[i];
                if (row == null) continue;

                string rawOption = (ans.options != null && i < ans.options.Count) ? ans.options[i] : "";
                string displayOption = FormatOptionForDisplay(rawOption, AnswerPrefixes[i]);

                AnswerOptionVisualState state;
                if (i == ans.correctAnswerIndex)
                {
                    state = AnswerOptionVisualState.CorrectAnswer;
                }
                else if (i == ans.selectedAnswerIndex)
                {
                    state = AnswerOptionVisualState.WrongSelected;
                }
                else
                {
                    state = AnswerOptionVisualState.Neutral;
                }

                ApplyCenterAnswerRowState(row, AnswerPrefixes[i], displayOption, state);
            }
        }

        private void RenderWrittenQuestionDetail(QuizAttemptAnswer ans, int questionNum, int totalQ)
        {
            // Ẩn 4 hàng trắc nghiệm A-B-C-D nếu có
            for (int i = 0; i < _centerAnswerRows.Count; i++)
            {
                if (_centerAnswerRows[i]?.rootObject != null)
                {
                    _centerAnswerRows[i].rootObject.SetActive(false);
                }
            }

            // 1. Hiển thị đề bài câu hỏi
            if (centerQuestionText != null)
            {
                centerQuestionText.text = $"<b>Câu {questionNum}/{totalQ}:</b> {ans.questionText}";
                centerQuestionText.enableAutoSizing = true;
                centerQuestionText.fontSizeMin = 18f;
                centerQuestionText.fontSizeMax = 24f;
                centerQuestionText.alignment = TextAlignmentOptions.TopLeft;
            }

            // 2. Tìm 2 khung text bài làm học sinh và đáp án mẫu trong Scene 21
            // Text (TMP) và Text (TMP) (1)
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                TextMeshProUGUI[] tmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
                TextMeshProUGUI userAnsTmp = null;
                TextMeshProUGUI sampleAnsTmp = null;

                foreach (var t in tmps)
                {
                    if (t == null) continue;
                    if (t == centerQuestionText || t == explanationText || t == keyKnowledgeText || t == bottomCounterText) continue;
                    if (t.GetComponentInParent<Button>() != null) continue;

                    string name = t.gameObject.name;
                    Vector2 pos = t.rectTransform.anchoredPosition;

                    if (name == "Text (TMP)" && Math.Abs(pos.y) < 50f)
                    {
                        userAnsTmp = t;
                    }
                    else if (name == "Text (TMP) (1)" || (name.Contains("Text") && pos.y < -150f && pos.x < 300f))
                    {
                        sampleAnsTmp = t;
                    }
                }

                string userText = (ans.options != null && ans.options.Count > 0) ? ans.options[0] : "";
                string sampleText = (ans.options != null && ans.options.Count > 1) ? ans.options[1] : "";

                if (userAnsTmp != null)
                {
                    userAnsTmp.text = $"<b>Bài làm của bạn:</b>\n{(string.IsNullOrEmpty(userText) ? "<i>(Chưa nhập câu trả lời)</i>" : userText)}";
                    userAnsTmp.enableAutoSizing = true;
                    userAnsTmp.fontSizeMin = 16f;
                    userAnsTmp.fontSizeMax = 22f;
                    userAnsTmp.alignment = TextAlignmentOptions.TopLeft;
                    userAnsTmp.color = new Color(0.08f, 0.12f, 0.3f, 1f);
                    userAnsTmp.rectTransform.sizeDelta = new Vector2(780f, 180f);
                    userAnsTmp.rectTransform.anchoredPosition = new Vector2(20f, -22f);
                }

                if (sampleAnsTmp != null)
                {
                    sampleAnsTmp.text = $"<b>Đáp án mẫu tham khảo:</b>\n{sampleText}";
                    sampleAnsTmp.enableAutoSizing = true;
                    sampleAnsTmp.fontSizeMin = 16f;
                    sampleAnsTmp.fontSizeMax = 22f;
                    sampleAnsTmp.alignment = TextAlignmentOptions.TopLeft;
                    sampleAnsTmp.color = new Color(0.12f, 0.28f, 0.15f, 1f);
                    sampleAnsTmp.rectTransform.sizeDelta = new Vector2(780f, 190f);
                    sampleAnsTmp.rectTransform.anchoredPosition = new Vector2(20f, -260f);
                }
            }
        }

        private void ApplyCenterAnswerRowState(CenterAnswerRowView row, string prefix, string displayOption, AnswerOptionVisualState state)
        {
            if (row == null) return;

            row.visualState = state;
            if (row.rootObject != null) row.rootObject.SetActive(true);
            if (row.badgeText != null) row.badgeText.text = prefix;
            if (row.optionText != null)
            {
                row.optionText.text = string.IsNullOrEmpty(displayOption) ? $"{prefix}." : $"{prefix}. {displayOption}";
            }

            switch (state)
            {
                case AnswerOptionVisualState.CorrectAnswer:
                    if (row.backgroundImage != null) row.backgroundImage.color = CorrectAnswerRowBgColor;
                    if (row.badgeBackground != null) row.badgeBackground.color = new Color(0.10f, 0.48f, 0.20f, 1f);
                    if (row.optionText != null)
                    {
                        row.optionText.color = new Color(0.06f, 0.34f, 0.14f, 1f);
                        row.optionText.fontStyle = FontStyles.Bold;
                    }
                    if (row.statusIcon != null)
                    {
                        row.statusIcon.sprite = _checkSprite;
                        row.statusIcon.color = Color.white;
                        row.statusIcon.gameObject.SetActive(true);
                    }
                    if (row.statusLabel != null)
                    {
                        row.statusLabel.text = "Đáp án đúng";
                        row.statusLabel.color = new Color(0.08f, 0.50f, 0.22f, 1f);
                        row.statusLabel.gameObject.SetActive(true);
                    }
                    break;

                case AnswerOptionVisualState.WrongSelected:
                    if (row.backgroundImage != null) row.backgroundImage.color = WrongAnswerRowBgColor;
                    if (row.badgeBackground != null) row.badgeBackground.color = new Color(0.68f, 0.14f, 0.14f, 1f);
                    if (row.optionText != null)
                    {
                        row.optionText.color = new Color(0.56f, 0.10f, 0.10f, 1f);
                        row.optionText.fontStyle = FontStyles.Bold;
                    }
                    if (row.statusIcon != null)
                    {
                        row.statusIcon.sprite = _crossSprite;
                        row.statusIcon.color = Color.white;
                        row.statusIcon.gameObject.SetActive(true);
                    }
                    if (row.statusLabel != null)
                    {
                        row.statusLabel.text = "Bạn chọn";
                        row.statusLabel.color = new Color(0.78f, 0.12f, 0.12f, 1f);
                        row.statusLabel.gameObject.SetActive(true);
                    }
                    break;

                default: // Neutral
                    if (row.backgroundImage != null) row.backgroundImage.color = NeutralAnswerRowBgColor;
                    if (row.badgeBackground != null) row.badgeBackground.color = new Color(0.12f, 0.18f, 0.28f, 1f);
                    if (row.optionText != null)
                    {
                        row.optionText.color = new Color(0.12f, 0.16f, 0.24f, 1f);
                        row.optionText.fontStyle = FontStyles.Normal;
                    }
                    if (row.statusIcon != null)
                    {
                        row.statusIcon.gameObject.SetActive(false);
                    }
                    if (row.statusLabel != null)
                    {
                        row.statusLabel.text = "";
                        row.statusLabel.gameObject.SetActive(false);
                    }
                    break;
            }
        }

        private string FormatOptionForDisplay(string rawAnswer, string letterPrefix)
        {
            if (string.IsNullOrEmpty(rawAnswer)) return "";
            string trimmed = rawAnswer.Trim();

            string[] prefixesToStrip = new[]
            {
                letterPrefix + ". ",
                letterPrefix + ".\t",
                letterPrefix + ") ",
                letterPrefix + ": ",
                letterPrefix + " - ",
                letterPrefix + "."
            };

            foreach (var p in prefixesToStrip)
            {
                if (trimmed.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed.Substring(p.Length).TrimStart();
                }
            }

            return trimmed;
        }

        public void OnClickQuestionRow(int index)
        {
            ShowQuestion(index);
        }

        private void OnPrevClicked()
        {
            if (_currentReviewIndex > 0)
            {
                ShowQuestion(_currentReviewIndex - 1);
            }
        }

        private void OnNextClicked()
        {
            if (_attemptData != null && _attemptData.answers != null && _currentReviewIndex < _attemptData.answers.Count - 1)
            {
                ShowQuestion(_currentReviewIndex + 1);
            }
        }

        public void OnReturnToResult()
        {
            if (PhysicsGameSession.Instance != null && PhysicsGameSession.Instance.SelectedMode == GameMode.Written)
            {
                Debug.Log("[QuizReviewController] Quay lại màn hình Tự luận 12_WrittenAnswer.");
                PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.WrittenAnswer);
                return;
            }

            Debug.Log("[QuizReviewController] Quay lại màn hình kết quả 19. Result_TN (giữ nguyên CurrentQuizAttempt).");
            if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.QuizResult);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(PhysicsSceneNames.QuizResult);
            }
        }
    }
}
