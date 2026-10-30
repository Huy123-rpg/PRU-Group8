using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển toàn bộ logic giao diện Scene 12_WrittenAnswer (Học tự luận - Học viện Aurelia):
    /// - Tự động liên kết các thành phần UI mỹ thuật đã dựng sẵn trong Scene:
    ///   + Nút Quay lại fantasy ornate (_1) ở góc trên bên trái (-671, 468).
    ///   + Đề bài (debai) và Bộ đếm (CounterText "Câu i/5").
    ///   + 5 Tab chọn câu hỏi bên sidebar trái (Câu 1 -> Câu 5).
    ///   + Khung "CÂU TRẢ LỜI CỦA BẠN" với TMP_InputField nền trong suốt, chữ trắng kem sắc nét.
    ///   + Khung "ẢNH ĐÃ TẢI LÊN" với ảnh xem trước (RawImage) và trạng thái "Chưa có ảnh tải lên".
    ///   + 5 Nút hành động phía dưới: "Tải ảnh", "Xóa ảnh", "Câu trước", "Câu tiếp", "Nộp bài".
    ///   + Nút nhận dạng chữ OCR phong cách fantasy navy-vàng.
    /// - Quản lý dữ liệu 5 câu tự luận: Lưu vết bài làm (text + ảnh) độc lập cho từng câu khi chuyển qua lại.
    /// - OCR nhận dạng bài viết tay trung thực + Modal kiểm tra và sửa bản chữ.
    /// - Nộp bài -> Chấm điểm AI theo rubric tiêu chí -> Chuyển sang RouteId.EssayResult.
    /// - Tuyệt đối KHÔNG tạo khung chữ nhật trắng/xám đè lên mỹ thuật bản vẽ gốc.
    /// </summary>
    public class WrittenAnswerController : MonoBehaviour
    {
        [Header("=== QUESTION & PROMPT ===")]
        [SerializeField] private TextMeshProUGUI counterText;
        [SerializeField] private TextMeshProUGUI questionPromptText;

        [Header("=== ANSWER INPUT & IMAGE PREVIEW ===")]
        [SerializeField] private TMP_InputField answerInputField;
        [SerializeField] private RawImage uploadedImagePreview;
        [SerializeField] private TextMeshProUGUI noImageNoticeText;
        [SerializeField] private Button btnOcr;

        [Header("=== NAVIGATION & ACTION BUTTONS ===")]
        [SerializeField] private Button btnBack;
        [SerializeField] private Button btnUpload;
        [SerializeField] private Button btnDeleteImg;
        [SerializeField] private Button btnPrev;
        [SerializeField] private Button btnNext;
        [SerializeField] private Button btnSubmit;
        [SerializeField] private TextMeshProUGUI noticeText;

        [Header("=== SIDEBAR QUESTION TABS (1-5) ===")]
        [SerializeField] private List<Button> sidebarQuestionButtons = new List<Button>();
        [SerializeField] private List<TextMeshProUGUI> sidebarQuestionLabels = new List<TextMeshProUGUI>();
        private Sprite _activeTabSprite;
        private Sprite _inactiveTabSprite;

        [Header("=== OCR TRANSCRIPT MODAL ===")]
        [SerializeField] private GameObject ocrModalRoot;
        [SerializeField] private TMP_InputField ocrTranscriptInput;
        [SerializeField] private Button btnOcrConfirm;
        [SerializeField] private Button btnOcrRetry;
        [SerializeField] private Button btnOcrCancel;

        [Header("=== AI TUTOR MODAL ===")]
        [SerializeField] private Button btnAiTutor;
        [SerializeField] private GameObject tutorModalRoot;
        [SerializeField] private TextMeshProUGUI tutorHeaderTitle;
        [SerializeField] private TextMeshProUGUI tutorStepProgressText;
        [SerializeField] private RectTransform tutorStepsContainer;
        [SerializeField] private ScrollRect tutorStepsScrollRect;
        [SerializeField] private Scrollbar tutorStepsScrollbar;
        [SerializeField] private TextMeshProUGUI tutorHintBodyText;
        [SerializeField] private Button btnTutorDismiss;
        [SerializeField] private Button btnTutorShowModelSol;
        [SerializeField] private GameObject tutorModelSolPanel;
        [SerializeField] private TextMeshProUGUI tutorModelSolText;

        // Dữ liệu câu hỏi & bài làm từng câu
        private List<G2EssayItem> _essayQuestions = new List<G2EssayItem>();
        private int _currentIndex = 0;
        private readonly Dictionary<int, string> _userAnswers = new Dictionary<int, string>();
        private readonly Dictionary<int, Texture2D> _userImages = new Dictionary<int, Texture2D>();
        private readonly Dictionary<int, byte[]> _userImageBytes = new Dictionary<int, byte[]>();
        private readonly Dictionary<int, string> _userImageMimes = new Dictionary<int, string>();

        private TMP_FontAsset _defaultFont;
        private string _pendingOcrText = "";
        private GameObject _toastRoot;
        private CanvasGroup _toastCanvasGroup;
        private Image _toastBg;
        private Outline _toastOutline;
        private TextMeshProUGUI _toastText;
        private Coroutine _toastCoroutine;
        private float _sessionStartTime;

        private void Awake()
        {
            EnsureEventSystem();
            DestroyDuplicateRuntimeObjects();
            FindUIReferences();
            EnsureTransparentInputField();
            EnsureImagePreviewAndOcrButton();
            EnsureAiTutorButton();
            EnsureToastNotification();
            BuildOcrTranscriptModal();
            BuildAiTutorModal();
        }

        private void Start()
        {
            _sessionStartTime = Time.time;
            NavigationService.Instance.EnsureDevContext();
            NavigationService.Instance.Selection.Mode = "ESSAY";
            LoadEssayQuestions();
            SetupActionButtons();
            DisplayQuestion(0);
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }
        }

        /// <summary>
        /// Xóa bỏ triệt để các object tạm từng được sinh runtime trước đây (Btn_Back, AnswerInputField cũ nếu có).
        /// </summary>
        private void DestroyDuplicateRuntimeObjects()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Transform dupBack = canvas.transform.Find("Btn_Back");
            if (dupBack != null) Destroy(dupBack.gameObject);

            Transform oldInput = canvas.transform.Find("AnswerInputField");
            if (oldInput != null) Destroy(oldInput.gameObject);

            Transform oldOcr = canvas.transform.Find("Btn_OCR");
            if (oldOcr != null) Destroy(oldOcr.gameObject);
        }

        /// <summary>
        /// Liên kết chính xác toàn bộ GameObject đã dựng sẵn trong Scene 12_WrittenAnswer.
        /// </summary>
        private void FindUIReferences()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            FixBackgroundRaycasts(canvas);

            // 1. Nút QUAY LẠI fantasy ornate (_1) ở góc trên bên trái: pos x ~ -671, y ~ 468
            var allImages = canvas.GetComponentsInChildren<Image>(true);
            foreach (var img in allImages)
            {
                Vector2 pos = img.rectTransform.anchoredPosition;
                if (btnBack == null && pos.x < -600f && pos.y > 400f)
                {
                    btnBack = img.GetComponent<Button>() ?? img.gameObject.AddComponent<Button>();
                    img.raycastTarget = true;
                    btnBack.targetGraphic = img;
                    UIHoverPulseEffect.Attach(btnBack.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
                }
            }

            // 2. Tìm Đề bài và Counter Text
            var allTmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in allTmps)
            {
                string text = t.text != null ? t.text.Trim() : "";

                // Counter: pos x ~ 547, y ~ 182 ("Câu 1/5")
                if (counterText == null && (text.StartsWith("Câu ") || text.Contains("/5") || t.rectTransform.anchoredPosition.x > 400f && t.rectTransform.anchoredPosition.y > 150f))
                {
                    counterText = t;
                    t.raycastTarget = false;
                }
                // Prompt đề bài: pos x ~ 152, y ~ 139 (đề bài dài)
                else if (questionPromptText == null && (text.Length > 25 || (t.rectTransform.anchoredPosition.y > 100f && t.rectTransform.anchoredPosition.y < 200f && t.rectTransform.anchoredPosition.x < 300f)))
                {
                    questionPromptText = t;
                    _defaultFont = t.font;
                    t.raycastTarget = false;
                    t.enableAutoSizing = true;
                    t.fontSizeMin = 18f;
                    t.fontSizeMax = 24f;
                    t.textWrappingMode = TextWrappingModes.Normal;
                    t.alignment = TextAlignmentOptions.TopLeft;
                    // Màu xanh sẫm tương phản cao trên nền giấy da vàng
                    t.color = new Color(0.08f, 0.14f, 0.28f, 1f);
                }
                // Static placeholder text trong khung trả lời ("Nhập câu trả lời của bạn tại đây") -> ẩn đi để nhường cho TMP_InputField
                else if (text.Contains("Nhập câu trả lời"))
                {
                    t.gameObject.SetActive(false);
                }
                // Static text trong khung ảnh ("Chưa có ảnh tải lên")
                else if (text.Contains("Chưa có ảnh"))
                {
                    noImageNoticeText = t;
                    t.raycastTarget = false;
                }
            }

            // 3. Tìm 5 Tab câu hỏi bên sidebar trái: x ~ -671 đến -678, y xếp từ 189 xuống -189
            List<Image> sidebarImgs = new List<Image>();
            foreach (var img in allImages)
            {
                Vector2 pos = img.rectTransform.anchoredPosition;
                if (pos.x < -640f && pos.y < 350f && pos.y > -250f && img.rectTransform.sizeDelta.x > 180f && img.rectTransform.sizeDelta.x < 260f)
                {
                    sidebarImgs.Add(img);
                }
            }
            // Sắp xếp theo thứ tự Y từ trên xuống dưới (Câu 1 đến Câu 5)
            sidebarImgs = sidebarImgs.OrderByDescending(img => img.rectTransform.anchoredPosition.y).ToList();

            // Ghi nhớ sprite active (xanh lá ban đầu ở câu 1) và inactive (xanh lam ở các câu 2..5)
            if (sidebarImgs.Count > 0 && sidebarImgs[0] != null)
            {
                _activeTabSprite = sidebarImgs[0].sprite;
            }
            if (sidebarImgs.Count > 1 && sidebarImgs[1] != null)
            {
                _inactiveTabSprite = sidebarImgs[1].sprite;
            }

            // Tìm 5 nhãn TextMeshProUGUI độc lập ở sidebar trái (x ~ -620, y từ 182 xuống -197)
            List<TextMeshProUGUI> sidebarLabels = new List<TextMeshProUGUI>();
            foreach (var t in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                Vector2 pos = t.rectTransform.anchoredPosition;
                if (pos.x < -580f && pos.x > -650f && pos.y < 250f && pos.y > -250f)
                {
                    sidebarLabels.Add(t);
                }
            }
            sidebarLabels = sidebarLabels.OrderByDescending(t => t.rectTransform.anchoredPosition.y).ToList();

            sidebarQuestionButtons.Clear();
            sidebarQuestionLabels.Clear();
            for (int i = 0; i < sidebarImgs.Count; i++)
            {
                var sImg = sidebarImgs[i];
                var sBtn = sImg.GetComponent<Button>() ?? sImg.gameObject.AddComponent<Button>();
                sImg.raycastTarget = true;
                sBtn.targetGraphic = sImg;
                UIHoverPulseEffect.Attach(sBtn.gameObject, hover: 1.05f, minPulse: 1.02f, maxPulse: 1.06f, outline: true);

                int qIdx = i;
                sBtn.onClick.RemoveAllListeners();
                sBtn.onClick.AddListener(() => OnSidebarQuestionClicked(qIdx));

                TextMeshProUGUI label = (i < sidebarLabels.Count) ? sidebarLabels[i] : sImg.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                {
                    label.raycastTarget = false;
                    label.text = $"Câu {i + 1}";
                }

                sidebarQuestionButtons.Add(sBtn);
                sidebarQuestionLabels.Add(label);
            }

            // 4. Tìm 5 Nút hành động phía dưới: pos y ~ -319
            // Thứ tự từ trái qua phải theo X:
            // Tải ảnh (-176), Xóa ảnh (18), Câu trước (208), Câu tiếp (395), Nộp bài (573)
            List<Image> bottomActionImgs = new List<Image>();
            foreach (var img in allImages)
            {
                Vector2 pos = img.rectTransform.anchoredPosition;
                if (Mathf.Abs(pos.y - -319f) < 25f && img.rectTransform.sizeDelta.y > 60f && img.rectTransform.sizeDelta.y < 130f)
                {
                    bottomActionImgs.Add(img);
                }
            }
            bottomActionImgs = bottomActionImgs.OrderBy(img => img.rectTransform.anchoredPosition.x).ToList();

            for (int i = 0; i < bottomActionImgs.Count; i++)
            {
                var bImg = bottomActionImgs[i];
                var bBtn = bImg.GetComponent<Button>() ?? bImg.gameObject.AddComponent<Button>();
                bImg.raycastTarget = true;
                bBtn.targetGraphic = bImg;
                UIHoverPulseEffect.Attach(bBtn.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.07f, outline: true);

                float posX = bImg.rectTransform.anchoredPosition.x;
                if (posX < -100f) btnUpload = bBtn;
                else if (posX < 100f) btnDeleteImg = bBtn;
                else if (posX < 300f) btnPrev = bBtn;
                else if (posX < 480f) btnNext = bBtn;
                else btnSubmit = bBtn;
            }
        }

        private void FixBackgroundRaycasts(Canvas canvas)
        {
            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
            {
                if (img.GetComponent<Button>() != null) continue;
                string name = img.gameObject.name.ToLower();
                Vector2 size = img.rectTransform.sizeDelta;

                if (size.x > 700f || size.y > 500f || name.Contains("bg") || name.Contains("background") || name.Contains("panel"))
                {
                    img.raycastTarget = false;
                }
            }
        }

        /// <summary>
        /// Tạo TMP_InputField nằm gọn bên trong khung "CÂU TRẢ LỜI CỦA BẠN" (pos: 18.57, -91.37).
        /// Background = Color.clear (trong suốt 100%) để không che mất viền và họa tiết xanh-vàng huyền ảo.
        /// Chữ trắng kem sáng rõ nét, căn đều và tự động xuống dòng mượt mà.
        /// </summary>
        private void EnsureTransparentInputField()
        {
            if (answerInputField != null) return;

            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject inputGo = new GameObject("Aurelia_AnswerInputField");
            inputGo.transform.SetParent(canvas.transform, false);

            RectTransform inputRt = inputGo.AddComponent<RectTransform>();
            inputRt.anchorMin = new Vector2(0.5f, 0.5f);
            inputRt.anchorMax = new Vector2(0.5f, 0.5f);
            inputRt.pivot = new Vector2(0.5f, 0.5f);
            inputRt.anchoredPosition = new Vector2(18.57f, -125f);
            inputRt.sizeDelta = new Vector2(640f, 240f);

            // Background hoàn toàn TRONG SUỐT để tôn lên khung mỹ thuật fantasy
            Image bg = inputGo.AddComponent<Image>();
            bg.color = Color.clear;
            bg.raycastTarget = true;

            // Text Viewport
            GameObject textAreaGo = new GameObject("Text Area");
            textAreaGo.transform.SetParent(inputGo.transform, false);
            RectTransform textAreaRt = textAreaGo.AddComponent<RectTransform>();
            textAreaRt.anchorMin = Vector2.zero;
            textAreaRt.anchorMax = Vector2.one;
            textAreaRt.offsetMin = new Vector2(12, 12);
            textAreaRt.offsetMax = new Vector2(-12, -12);
            textAreaGo.AddComponent<RectMask2D>();

            // Placeholder Text
            GameObject placeholderGo = new GameObject("Placeholder");
            placeholderGo.transform.SetParent(textAreaGo.transform, false);
            RectTransform plRt = placeholderGo.AddComponent<RectTransform>();
            plRt.anchorMin = Vector2.zero;
            plRt.anchorMax = Vector2.one;
            plRt.offsetMin = Vector2.zero;
            plRt.offsetMax = Vector2.zero;

            TextMeshProUGUI plTmp = placeholderGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) plTmp.font = _defaultFont;
            plTmp.text = "Nhập câu trả lời hoặc bấm 'Tải ảnh' để nhận dạng bài viết tay qua OCR...";
            plTmp.fontSize = 19;
            plTmp.fontStyle = FontStyles.Italic;
            plTmp.color = new Color(0.35f, 0.40f, 0.52f, 0.70f); // Mực lam mờ tao nhã trên nền giấy da vàng
            plTmp.textWrappingMode = TextWrappingModes.Normal;
            plTmp.raycastTarget = false;

            // Content Text (Màu mực sẫm tương phản cao trên nền giấy da vàng)
            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(textAreaGo.transform, false);
            RectTransform textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            TextMeshProUGUI textTmp = textGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) textTmp.font = _defaultFont;
            textTmp.fontSize = 20;
            textTmp.color = new Color(0.08f, 0.12f, 0.22f, 1f); // Mực xanh đen sẫm (Charcoal Navy Ink) rõ ràng sắc nét
            textTmp.textWrappingMode = TextWrappingModes.Normal;
            textTmp.raycastTarget = false;

            answerInputField = inputGo.AddComponent<TMP_InputField>();
            answerInputField.targetGraphic = bg;
            answerInputField.textViewport = textAreaRt;
            answerInputField.textComponent = textTmp;
            answerInputField.placeholder = plTmp;
            answerInputField.lineType = TMP_InputField.LineType.MultiLineNewline;
            answerInputField.characterLimit = 5000;
            answerInputField.interactable = true;

            answerInputField.onValueChanged.AddListener((val) =>
            {
                _userAnswers[_currentIndex] = val;
            });
        }

        /// <summary>
        /// Tạo vùng xem trước ảnh đã tải lên bên trong khung "ẢNH ĐÃ TẢI LÊN" (pos: 536.34, -127.98)
        /// kèm theo nút "📷 ĐỌC CHỮ OCR" fantasy sang trọng nằm ngay dưới khung ảnh.
        /// </summary>
        private void EnsureImagePreviewAndOcrButton()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. RawImage Preview nằm trong khung ảnh
            if (uploadedImagePreview == null)
            {
                GameObject previewGo = new GameObject("Aurelia_UploadedImagePreview");
                previewGo.transform.SetParent(canvas.transform, false);

                RectTransform pRt = previewGo.AddComponent<RectTransform>();
                pRt.anchorMin = new Vector2(0.5f, 0.5f);
                pRt.anchorMax = new Vector2(0.5f, 0.5f);
                pRt.pivot = new Vector2(0.5f, 0.5f);
                pRt.anchoredPosition = new Vector2(536.34f, -120f);
                pRt.sizeDelta = new Vector2(260f, 180f);

                uploadedImagePreview = previewGo.AddComponent<RawImage>();
                uploadedImagePreview.color = Color.white;
                uploadedImagePreview.raycastTarget = false;
                previewGo.SetActive(false);
            }

            // 2. Nút OCR fantasy navy-gold đặt ngay dưới khung ảnh (pos: 536.34, -255)
            if (btnOcr == null)
            {
                GameObject ocrGo = new GameObject("Btn_FantasyOcr");
                ocrGo.transform.SetParent(canvas.transform, false);

                RectTransform rt = ocrGo.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(536.34f, -255f);
                rt.sizeDelta = new Vector2(200f, 36f);

                Image ocrImg = ocrGo.AddComponent<Image>();
                ocrImg.color = new Color(0.08f, 0.16f, 0.32f, 0.95f);
                ocrImg.raycastTarget = true;

                Outline outline = ocrGo.AddComponent<Outline>();
                outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f); // Gold ornate border
                outline.effectDistance = new Vector2(1.5f, -1.5f);

                btnOcr = ocrGo.AddComponent<Button>();
                btnOcr.targetGraphic = ocrImg;

                GameObject txtGo = new GameObject("Text");
                txtGo.transform.SetParent(ocrGo.transform, false);
                RectTransform txtRt = txtGo.AddComponent<RectTransform>();
                txtRt.sizeDelta = rt.sizeDelta;
                TextMeshProUGUI tmp = txtGo.AddComponent<TextMeshProUGUI>();
                if (_defaultFont != null) tmp.font = _defaultFont;
                tmp.text = "📷 ĐỌC CHỮ OCR";
                tmp.fontSize = 17;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = new Color(1f, 0.92f, 0.70f, 1f);
                tmp.raycastTarget = false;

                UIHoverPulseEffect.Attach(ocrGo, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.07f, outline: true);
                btnOcr.onClick.AddListener(OnOcrButtonClicked);
            }
        }

        /// <summary>
        /// Tạo nút "✨ GỢI Ý BƯỚC TIẾP (GIA SƯ AI)" phong cách fantasy Aurelia nằm ngay dưới khung bài làm.
        /// Cho phép học sinh hỏi hướng dẫn từng bước mà không bị lộ kết quả bước tiếp theo.
        /// </summary>
        private void EnsureAiTutorButton()
        {
            if (btnAiTutor != null) return;

            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Transform existing = canvas.transform.Find("Btn_FantasyAiTutor");
            if (existing != null)
            {
                btnAiTutor = existing.GetComponent<Button>();
                if (btnAiTutor != null) return;
            }

            GameObject tutorGo = new GameObject("Btn_FantasyAiTutor");
            tutorGo.transform.SetParent(canvas.transform, false);

            RectTransform rt = tutorGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(18.57f, -255f);
            rt.sizeDelta = new Vector2(290f, 36f);

            Image tutorImg = tutorGo.AddComponent<Image>();
            tutorImg.color = new Color(0.08f, 0.20f, 0.42f, 0.95f);
            tutorImg.raycastTarget = true;

            Outline outline = tutorGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f); // Gold ornate border
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            btnAiTutor = tutorGo.AddComponent<Button>();
            btnAiTutor.targetGraphic = tutorImg;

            GameObject txtGo = new GameObject("Text");
            txtGo.transform.SetParent(tutorGo.transform, false);
            RectTransform txtRt = txtGo.AddComponent<RectTransform>();
            txtRt.sizeDelta = rt.sizeDelta;
            TextMeshProUGUI tmp = txtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) tmp.font = _defaultFont;
            tmp.text = "✨ GỢI Ý BƯỚC TIẾP (GIA SƯ AI)";
            tmp.fontSize = 16;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.92f, 0.70f, 1f);
            tmp.raycastTarget = false;

            UIHoverPulseEffect.Attach(tutorGo, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.07f, outline: true);
            btnAiTutor.onClick.AddListener(OnAiTutorClicked);
        }

        public enum ToastType
        {
            Info,
            Success,
            Warning,
            Error
        }

        /// <summary>
        /// Tạo Toast Notification chuẩn phong cách fantasy Aurelia:
        /// - Vị trí an toàn ở trên cùng giữa màn hình (Top Center: 0, -22), tuyệt đối KHÔNG đè lên tiêu đề bài học,
        ///   khung "CÂU TRẢ LỜI CỦA BẠN", ảnh xem trước hay các nút bấm.
        /// - Nền xanh navy sang trọng, viền vàng kim, chữ trắng kem sắc nét.
        /// - Tự động biến mất sau 2.5s (thành công) hoặc 4.5s (lỗi).
        /// - Chỉ hiển thị 1 thông báo duy nhất tại một thời điểm, không chồng chữ.
        /// </summary>
        private void EnsureToastNotification()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Xóa triệt để object NoticeText cũ nếu còn tồn tại trong Hierarchy
            Transform oldNotice = canvas.transform.Find("NoticeText");
            if (oldNotice != null)
            {
                Destroy(oldNotice.gameObject);
            }

            if (_toastRoot != null) return;

            Transform existingToast = canvas.transform.Find("AureliaToastNotification");
            if (existingToast != null)
            {
                _toastRoot = existingToast.gameObject;
                _toastCanvasGroup = _toastRoot.GetComponent<CanvasGroup>();
                _toastBg = _toastRoot.GetComponent<Image>();
                _toastOutline = _toastRoot.GetComponent<Outline>();
                _toastText = _toastRoot.GetComponentInChildren<TextMeshProUGUI>(true);
                noticeText = _toastText;
                _toastRoot.SetActive(false);
                return;
            }

            _toastRoot = new GameObject("AureliaToastNotification");
            _toastRoot.transform.SetParent(canvas.transform, false);

            RectTransform rt = _toastRoot.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -22f);
            rt.sizeDelta = new Vector2(480f, 44f);

            _toastCanvasGroup = _toastRoot.AddComponent<CanvasGroup>();
            _toastCanvasGroup.alpha = 1f;
            _toastCanvasGroup.blocksRaycasts = false;

            _toastBg = _toastRoot.AddComponent<Image>();
            _toastBg.color = new Color(0.06f, 0.12f, 0.24f, 0.96f);

            _toastOutline = _toastRoot.AddComponent<Outline>();
            _toastOutline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            _toastOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var shadow = _toastRoot.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
            shadow.effectDistance = new Vector2(2f, -2f);

            var hlg = _toastRoot.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 8, 8);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var csf = _toastRoot.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            GameObject txtGo = new GameObject("ToastMessage");
            txtGo.transform.SetParent(_toastRoot.transform, false);
            RectTransform txtRt = txtGo.AddComponent<RectTransform>();
            txtRt.sizeDelta = new Vector2(420f, 30f);

            _toastText = txtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) _toastText.font = _defaultFont;
            _toastText.fontSize = 17;
            _toastText.fontStyle = FontStyles.Bold;
            _toastText.alignment = TextAlignmentOptions.Center;
            _toastText.color = new Color(0.98f, 0.96f, 0.90f, 1f);
            _toastText.raycastTarget = false;

            noticeText = _toastText;
            _toastRoot.SetActive(false);
        }

        private void ShowToast(string msg, ToastType type = ToastType.Info, float duration = -1f)
        {
            EnsureToastNotification();
            if (_toastRoot == null || _toastText == null) return;

            if (_toastCoroutine != null)
            {
                StopCoroutine(_toastCoroutine);
                _toastCoroutine = null;
            }

            string plainText = msg;
            if (plainText.Contains("<color="))
            {
                plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"<color=[^>]+>", "");
                plainText = plainText.Replace("</color>", "");
            }

            _toastText.text = plainText;

            switch (type)
            {
                case ToastType.Success:
                    _toastBg.color = new Color(0.04f, 0.16f, 0.18f, 0.96f);
                    _toastOutline.effectColor = new Color(0.35f, 0.88f, 0.55f, 1f);
                    _toastText.color = new Color(0.85f, 1f, 0.90f, 1f);
                    if (duration <= 0f) duration = 2.5f;
                    break;
                case ToastType.Warning:
                    _toastBg.color = new Color(0.18f, 0.14f, 0.05f, 0.96f);
                    _toastOutline.effectColor = new Color(0.96f, 0.75f, 0.25f, 1f);
                    _toastText.color = new Color(1f, 0.95f, 0.80f, 1f);
                    if (duration <= 0f) duration = 3.5f;
                    break;
                case ToastType.Error:
                    _toastBg.color = new Color(0.20f, 0.06f, 0.08f, 0.96f);
                    _toastOutline.effectColor = new Color(0.95f, 0.40f, 0.40f, 1f);
                    _toastText.color = new Color(1f, 0.88f, 0.88f, 1f);
                    if (duration <= 0f) duration = 4.5f;
                    break;
                default:
                    _toastBg.color = new Color(0.06f, 0.12f, 0.24f, 0.96f);
                    _toastOutline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
                    _toastText.color = new Color(0.98f, 0.96f, 0.90f, 1f);
                    if (duration <= 0f) duration = 3.0f;
                    break;
            }

            _toastRoot.transform.SetAsLastSibling();
            _toastRoot.SetActive(true);
            if (_toastCanvasGroup != null) _toastCanvasGroup.alpha = 1f;

            if (duration > 0f)
            {
                _toastCoroutine = StartCoroutine(ToastFadeRoutine(duration));
            }
        }

        private IEnumerator ToastFadeRoutine(float displayTime)
        {
            yield return new WaitForSeconds(displayTime);

            float fadeTime = 0.25f;
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                if (_toastCanvasGroup != null)
                {
                    _toastCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                }
                yield return null;
            }

            if (_toastRoot != null)
            {
                _toastRoot.SetActive(false);
            }
            _toastCoroutine = null;
        }

        private void ShowNotice(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;

            if (msg.Contains("✓") || msg.ToLower().Contains("thành công"))
            {
                ShowToast(msg, ToastType.Success, 2.5f);
            }
            else if (msg.Contains("⚠") || msg.Contains("Cảnh báo") || msg.ToLower().Contains("vui lòng"))
            {
                ShowToast(msg, ToastType.Warning, 3.5f);
            }
            else if (msg.Contains("Lỗi") || msg.Contains("Không thể") || msg.Contains("trống") || msg.Contains("thất bại"))
            {
                ShowToast(msg, ToastType.Error, 4.5f);
            }
            else
            {
                ShowToast(msg, ToastType.Info, 3.0f);
            }
        }

        private void SetupActionButtons()
        {
            if (btnBack != null)
            {
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackClicked);
            }

            if (btnSubmit != null)
            {
                btnSubmit.onClick.RemoveAllListeners();
                btnSubmit.onClick.AddListener(OnSubmitClicked);
            }

            if (btnUpload != null)
            {
                btnUpload.onClick.RemoveAllListeners();
                btnUpload.onClick.AddListener(OnUploadClicked);
            }

            if (btnDeleteImg != null)
            {
                btnDeleteImg.onClick.RemoveAllListeners();
                btnDeleteImg.onClick.AddListener(OnDeleteImgClicked);
            }

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
        }

        /// <summary>
        /// Nạp 5 câu hỏi tự luận thật từ GameDataRepository theo bài học và mức độ đã chọn.
        /// </summary>
        private void LoadEssayQuestions()
        {
            GameDataRepository.EnsureInitialized();

            string baiId = NavigationService.Instance.Selection != null ? NavigationService.Instance.Selection.LessonId : "bai2";
            string mucDo = NavigationService.Instance.Selection != null ? NavigationService.Instance.Selection.Difficulty : "NB";

            if (string.IsNullOrEmpty(baiId)) baiId = "bai2";
            if (string.IsNullOrEmpty(mucDo)) mucDo = "NB";

            // Nạp đúng 5 câu tự luận chuẩn
            _essayQuestions = GameDataRepository.GetEssayQuestions(baiId, mucDo, 5);

            // Fallback an toàn nếu chưa có đủ
            if (_essayQuestions == null || _essayQuestions.Count == 0)
            {
                if (GameDataRepository.AllEssays != null && GameDataRepository.AllEssays.Count > 0)
                {
                    _essayQuestions = GameDataRepository.AllEssays.Take(5).ToList();
                }
            }

            Debug.Log($"[WrittenAnswerController] Đã nạp {_essayQuestions?.Count ?? 0} câu tự luận cho bài '{baiId}' mức '{mucDo}'.");
        }

        /// <summary>
        /// Hiển thị câu hỏi tại vị trí index (0 đến 4) và cập nhật giao diện hoàn chỉnh.
        /// </summary>
        public void DisplayQuestion(int index)
        {
            if (_essayQuestions == null || _essayQuestions.Count == 0)
            {
                if (questionPromptText != null) questionPromptText.text = "Chưa có câu hỏi tự luận cho bài này.";
                if (counterText != null) counterText.text = "Câu 0/0";
                return;
            }

            // 1. Lưu lại nội dung của câu hiện tại trước khi chuyển
            if (answerInputField != null)
            {
                _userAnswers[_currentIndex] = answerInputField.text;
            }

            _currentIndex = Mathf.Clamp(index, 0, _essayQuestions.Count - 1);
            var q = _essayQuestions[_currentIndex];

            // 2. Cập nhật Đề bài
            if (questionPromptText != null && q != null)
            {
                string diffCode = !string.IsNullOrEmpty(q.mucDoGame2) ? q.mucDoGame2 : "NB";
                string content = !string.IsNullOrEmpty(q.noiDung) ? q.noiDung : "Nội dung câu hỏi tự luận";
                questionPromptText.text = $"<b>CÂU {_currentIndex + 1} ({diffCode}):</b> {content}";
            }

            // 3. Cập nhật Bộ đếm "Câu i/5"
            if (counterText != null)
            {
                counterText.text = $"Câu {_currentIndex + 1}/{_essayQuestions.Count}";
            }

            // 4. Highlight Tab bên sidebar trái: di chuyển nút xanh lá theo đúng câu đang chọn
            for (int i = 0; i < sidebarQuestionButtons.Count; i++)
            {
                if (sidebarQuestionButtons[i] != null)
                {
                    bool exists = (i < _essayQuestions.Count);
                    sidebarQuestionButtons[i].gameObject.SetActive(exists);
                    if (!exists) continue;

                    bool isSelected = (i == _currentIndex);
                    var img = sidebarQuestionButtons[i].GetComponent<Image>();
                    if (img != null)
                    {
                        if (_activeTabSprite != null && _inactiveTabSprite != null && _activeTabSprite != _inactiveTabSprite)
                        {
                            img.sprite = isSelected ? _activeTabSprite : _inactiveTabSprite;
                            img.color = Color.white;
                        }
                        else
                        {
                            img.color = isSelected ? new Color(0.18f, 0.82f, 0.44f, 1f) : new Color(0.12f, 0.22f, 0.42f, 0.90f);
                        }
                    }

                    if (i < sidebarQuestionLabels.Count && sidebarQuestionLabels[i] != null)
                    {
                        sidebarQuestionLabels[i].gameObject.SetActive(exists);
                        sidebarQuestionLabels[i].text = $"Câu {i + 1}";
                        sidebarQuestionLabels[i].color = isSelected ? Color.white : new Color(0.85f, 0.90f, 1f, 0.85f);
                    }
                }
            }

            // 5. Khôi phục bài làm đã lưu của câu này (nếu đã gõ trước đó)
            if (answerInputField != null)
            {
                string savedAnswer = _userAnswers.ContainsKey(_currentIndex) ? _userAnswers[_currentIndex] : "";
                answerInputField.text = savedAnswer;
            }

            // 6. Khôi phục ảnh đã tải lên của câu này (nếu có)
            UpdateImagePreview();

            // 7. Cập nhật trạng thái nút Câu trước / Câu tiếp
            if (btnPrev != null) btnPrev.interactable = (_currentIndex > 0);
            if (btnNext != null) btnNext.interactable = (_currentIndex < _essayQuestions.Count - 1);
        }

        private void UpdateImagePreview()
        {
            bool hasImage = _userImages.ContainsKey(_currentIndex) && _userImages[_currentIndex] != null;
            if (uploadedImagePreview != null)
            {
                if (hasImage)
                {
                    Texture2D tex = _userImages[_currentIndex];
                    uploadedImagePreview.texture = tex;

                    // Giữ đúng tỷ lệ ảnh (Aspect Ratio), nằm gọn trong khung không bị méo và không tràn ra ngoài
                    float maxW = 260f;
                    float maxH = 170f;
                    float aspect = (tex != null && tex.width > 0 && tex.height > 0) ? (float)tex.width / tex.height : 1.33f;

                    float targetW = maxW;
                    float targetH = targetW / aspect;
                    if (targetH > maxH)
                    {
                        targetH = maxH;
                        targetW = targetH * aspect;
                    }

                    RectTransform pRt = uploadedImagePreview.rectTransform;
                    pRt.sizeDelta = new Vector2(targetW, targetH);
                    pRt.anchoredPosition = new Vector2(536.34f, -120f);

                    uploadedImagePreview.gameObject.SetActive(true);
                }
                else
                {
                    uploadedImagePreview.texture = null;
                    uploadedImagePreview.gameObject.SetActive(false);
                }
            }

            if (noImageNoticeText != null)
            {
                noImageNoticeText.gameObject.SetActive(!hasImage);
            }

            // Nút OCR: chỉ bật khi đã có ảnh thật hợp lệ
            if (btnOcr != null)
            {
                btnOcr.interactable = hasImage;
                var ocrImg = btnOcr.GetComponent<Image>();
                if (ocrImg != null)
                {
                    ocrImg.color = hasImage ? new Color(0.08f, 0.20f, 0.42f, 0.95f) : new Color(0.12f, 0.16f, 0.22f, 0.40f);
                }
            }
        }

        private void OnSidebarQuestionClicked(int index)
        {
            DisplayQuestion(index);
        }

        private void OnPrevClicked()
        {
            if (_currentIndex > 0)
            {
                DisplayQuestion(_currentIndex - 1);
            }
        }

        private void OnNextClicked()
        {
            if (_currentIndex < _essayQuestions.Count - 1)
            {
                DisplayQuestion(_currentIndex + 1);
            }
        }

        /// <summary>
        /// Nút "TẢI ẢNH": Mở hộp thoại chọn file ảnh thật từ máy tính (PNG, JPG, JPEG).
        /// Nếu người dùng bấm Cancel hoặc đóng hộp thoại: Tuyệt đối không tạo ảnh giả, không đổi trạng thái.
        /// </summary>
        private void OnUploadClicked()
        {
            Debug.Log($"[WrittenAnswerController] Mở hộp thoại chọn ảnh cho Câu {_currentIndex + 1}.");

            string filePath = SelectImageFile();

            // Nếu người dùng hủy hoặc không chọn file
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.Log("[WrittenAnswerController] Người dùng đã hủy chọn ảnh (Cancel). Giữ nguyên trạng thái.");
                return;
            }

            if (!File.Exists(filePath))
            {
                ShowNotice("<color=#ef4444>Tệp ảnh không tồn tại trên hệ thống.</color>");
                return;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                Texture2D loadedTex = new Texture2D(2, 2);
                if (loadedTex.LoadImage(bytes))
                {
                    // Giải phóng ảnh cũ của câu này nếu có
                    if (_userImages.ContainsKey(_currentIndex) && _userImages[_currentIndex] != null)
                    {
                        Destroy(_userImages[_currentIndex]);
                    }

                    _userImages[_currentIndex] = loadedTex;
                    _userImageBytes[_currentIndex] = bytes;
                    string ext = Path.GetExtension(filePath).ToLowerInvariant();
                    _userImageMimes[_currentIndex] = (ext == ".jpg" || ext == ".jpeg") ? "image/jpeg" : "image/png";

                    UpdateImagePreview();
                    ShowToast("✓ Tải ảnh thành công! Bấm 'ĐỌC CHỮ OCR' nếu muốn nhận dạng.", ToastType.Success, 2.5f);
                }
                else
                {
                    Destroy(loadedTex);
                    ShowToast("⚠ Không thể tải ảnh: Tệp không đúng định dạng PNG hoặc JPG.", ToastType.Error, 4.5f);
                }
            }
            catch (Exception ex)
            {
                ShowToast($"⚠ Không thể tải ảnh: {ex.Message}", ToastType.Error, 4.5f);
            }
        }

        private string SelectImageFile()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.OpenFilePanelWithFilters(
                "Chọn ảnh bài làm học sinh (PNG, JPG)",
                "",
                new string[] { "Image files", "png,jpg,jpeg", "All files", "*" });
#else
            return OpenFilePanelWindows("Chọn ảnh bài làm học sinh", "Image Files (*.png;*.jpg;*.jpeg)\0*.png;*.jpg;*.jpeg\0All Files (*.*)\0*.*\0\0");
#endif
        }

#if !UNITY_EDITOR
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public struct OpenFileName
        {
            public int structSize;
            public IntPtr dlgOwner;
            public IntPtr instance;
            public string filter;
            public string customFilter;
            public int maxCustFilter;
            public int filterIndex;
            public string file;
            public int maxFile;
            public string fileTitle;
            public int maxFileTitle;
            public string initialDir;
            public string title;
            public int flags;
            public short fileOffset;
            public short fileExtension;
            public string defExt;
            public IntPtr custData;
            public IntPtr hook;
            public string templateName;
            public IntPtr reservedPtr;
            public int reservedInt;
            public int flagsEx;
        }

        [System.Runtime.InteropServices.DllImport("comdlg32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern bool GetOpenFileName([System.Runtime.InteropServices.In, System.Runtime.InteropServices.Out] ref OpenFileName ofn);

        private static string OpenFilePanelWindows(string title, string filter)
        {
            var ofn = new OpenFileName();
            ofn.structSize = System.Runtime.InteropServices.Marshal.SizeOf(ofn);
            ofn.filter = filter;
            ofn.file = new string(new char[260]);
            ofn.maxFile = ofn.file.Length;
            ofn.title = title;
            ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

            if (GetOpenFileName(ref ofn))
            {
                return ofn.file;
            }
            return null;
        }
#endif

        /// <summary>
        /// Nút "XÓA ẢNH": Xóa ảnh đã tải của câu hiện tại.
        /// </summary>
        private void OnDeleteImgClicked()
        {
            if (_userImages.ContainsKey(_currentIndex) && _userImages[_currentIndex] != null)
            {
                Destroy(_userImages[_currentIndex]);
                _userImages[_currentIndex] = null;
                _userImageBytes[_currentIndex] = null;
                _userImageMimes[_currentIndex] = null;
                UpdateImagePreview();
                ShowToast("✓ Đã xóa ảnh bài làm", ToastType.Success, 2.5f);
            }
            else
            {
                ShowToast("⚠ Chưa có ảnh nào được tải lên ở câu này", ToastType.Warning, 3.0f);
            }
        }

        /// <summary>
        /// Nút "📷 ĐỌC CHỮ OCR": Trích xuất văn bản viết tay từ ảnh bài làm qua Google Gemini AI.
        /// Chạy trực tiếp qua endpoint POST /api/ocr_physics trên Backend server.
        /// Cho phép học sinh xem trước và sửa bản chữ trong modal trước khi nộp.
        /// </summary>
        private void OnOcrButtonClicked()
        {
            Texture2D tex = _userImages.ContainsKey(_currentIndex) ? _userImages[_currentIndex] : null;
            byte[] rawBytes = _userImageBytes.ContainsKey(_currentIndex) ? _userImageBytes[_currentIndex] : null;
            string mimeType = _userImageMimes.ContainsKey(_currentIndex) ? _userImageMimes[_currentIndex] : "image/png";

            if (tex == null && (rawBytes == null || rawBytes.Length == 0))
            {
                ShowToast("⚠ Vui lòng tải ảnh bài làm lên trước khi đọc OCR", ToastType.Warning, 3.5f);
                return;
            }

            Debug.Log($"[WrittenAnswerController] Yêu cầu Gemini AI OCR cho Câu {_currentIndex + 1}...");
            ShowToast("⏳ Đang gửi ảnh tới Google Gemini AI để nhận dạng...", ToastType.Info, 12f);

            if (btnOcr != null) btnOcr.interactable = false;

            EssayGradingService.PerformOCRWithGeminiAsync(tex, rawBytes, mimeType, (ocrResult) =>
            {
                if (btnOcr != null) btnOcr.interactable = true;

                if (ocrResult != null && ocrResult.isSuccess && !string.IsNullOrWhiteSpace(ocrResult.transcript))
                {
                    ShowToast("✓ Nhận dạng thành công từ Google Gemini AI!", ToastType.Success, 2.5f);
                    ShowOcrTranscriptModal(ocrResult.transcript);
                }
                else
                {
                    string err = (ocrResult != null && !string.IsNullOrEmpty(ocrResult.errorMessage))
                        ? ocrResult.errorMessage
                        : "Không nhận dạng được chữ trong ảnh. Vui lòng thử ảnh rõ nét hơn.";
                    ShowToast($"⚠ Lỗi OCR: {err}", ToastType.Error, 4.5f);
                }
            });
        }

        private void BuildOcrTranscriptModal()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Xóa triệt để modal cũ để luôn cập nhật phiên bản mới có Scrollbar và RectMask2D
            Transform existing = canvas.transform.Find("OcrTranscriptModal");
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }

            ocrModalRoot = new GameObject("OcrTranscriptModal");
            ocrModalRoot.transform.SetParent(canvas.transform, false);

            RectTransform rootRt = ocrModalRoot.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.sizeDelta = Vector2.zero;

            Image overlay = ocrModalRoot.AddComponent<Image>();
            overlay.color = new Color(0.04f, 0.08f, 0.16f, 0.90f);
            overlay.raycastTarget = true;

            GameObject card = new GameObject("ModalCard");
            card.transform.SetParent(ocrModalRoot.transform, false);
            RectTransform cardRt = card.AddComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(840, 560);
            cardRt.anchoredPosition = Vector2.zero;

            Image cardImg = card.AddComponent<Image>();
            cardImg.color = new Color(0.08f, 0.14f, 0.28f, 0.98f);

            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            cardOutline.effectDistance = new Vector2(2, -2);

            // 1. Title
            GameObject titleGo = new GameObject("Title");
            titleGo.transform.SetParent(card.transform, false);
            RectTransform titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0, 235);
            titleRt.sizeDelta = new Vector2(780, 42);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) titleTmp.font = _defaultFont;
            titleTmp.text = "KIỂM TRA VÀ SỬA BẢN CHỮ NHẬN DẠNG";
            titleTmp.fontSize = 22;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.96f, 0.82f, 0.38f, 1f);

            // 2. Subtitle
            GameObject subGo = new GameObject("Subtitle");
            subGo.transform.SetParent(card.transform, false);
            RectTransform subRt = subGo.AddComponent<RectTransform>();
            subRt.anchoredPosition = new Vector2(0, 195);
            subRt.sizeDelta = new Vector2(780, 30);
            var subTmp = subGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) subTmp.font = _defaultFont;
            subTmp.text = "Bạn có thể dùng con lăn chuột để cuộn và chỉnh sửa trực tiếp nội dung dưới đây:";
            subTmp.fontSize = 16;
            subTmp.alignment = TextAlignmentOptions.Center;
            subTmp.color = new Color(0.90f, 0.94f, 1f, 0.90f);

            // 3. Scrollable Input Field Container (Vùng văn bản chiếm diện tích chính)
            GameObject inputGo = new GameObject("OcrInput");
            inputGo.transform.SetParent(card.transform, false);
            RectTransform inRt = inputGo.AddComponent<RectTransform>();
            inRt.anchoredPosition = new Vector2(0, 8);
            inRt.sizeDelta = new Vector2(780, 290);

            Image inBg = inputGo.AddComponent<Image>();
            inBg.color = new Color(0.97f, 0.98f, 1f, 1f);
            inBg.raycastTarget = true;

            Outline inOutline = inputGo.AddComponent<Outline>();
            inOutline.effectColor = new Color(0.20f, 0.35f, 0.60f, 0.45f);
            inOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // 3a. Text Area (Viewport có RectMask2D để giới hạn chữ trong ô trắng)
            GameObject textAreaGo = new GameObject("Text Area");
            textAreaGo.transform.SetParent(inputGo.transform, false);
            RectTransform textAreaRt = textAreaGo.AddComponent<RectTransform>();
            textAreaRt.anchorMin = Vector2.zero;
            textAreaRt.anchorMax = Vector2.one;
            textAreaRt.offsetMin = new Vector2(16f, 12f);
            textAreaRt.offsetMax = new Vector2(-36f, -12f);

            textAreaGo.AddComponent<RectMask2D>();

            // 3b. Text Component
            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(textAreaGo.transform, false);
            RectTransform textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.pivot = new Vector2(0f, 1f);
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            TextMeshProUGUI inTmp = textGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) inTmp.font = _defaultFont;
            inTmp.fontSize = 19;
            inTmp.color = new Color(0.06f, 0.10f, 0.18f, 1f);
            inTmp.textWrappingMode = TextWrappingModes.Normal;
            inTmp.alignment = TextAlignmentOptions.TopLeft;
            inTmp.raycastTarget = false;

            // 3c. Vertical Scrollbar (Thanh cuộn dọc trang nhã, dễ nhìn)
            GameObject scrollbarGo = new GameObject("Scrollbar");
            scrollbarGo.transform.SetParent(inputGo.transform, false);
            RectTransform sbRt = scrollbarGo.AddComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(1f, 0f);
            sbRt.anchorMax = new Vector2(1f, 1f);
            sbRt.pivot = new Vector2(1f, 0.5f);
            sbRt.anchoredPosition = new Vector2(-8f, 0f);
            sbRt.sizeDelta = new Vector2(18f, -20f);

            Image sbBg = scrollbarGo.AddComponent<Image>();
            sbBg.color = new Color(0.86f, 0.90f, 0.95f, 0.85f);
            sbBg.raycastTarget = true;

            GameObject slidingArea = new GameObject("Sliding Area");
            slidingArea.transform.SetParent(scrollbarGo.transform, false);
            RectTransform saRt = slidingArea.AddComponent<RectTransform>();
            saRt.anchorMin = Vector2.zero;
            saRt.anchorMax = Vector2.one;
            saRt.sizeDelta = new Vector2(-18f, -18f);

            GameObject handleGo = new GameObject("Handle");
            handleGo.transform.SetParent(slidingArea.transform, false);
            RectTransform hRt = handleGo.AddComponent<RectTransform>();
            hRt.anchorMin = Vector2.zero;
            hRt.anchorMax = Vector2.one;
            hRt.sizeDelta = new Vector2(18f, 18f);

            Image handleImg = handleGo.AddComponent<Image>();
            handleImg.color = new Color(0.18f, 0.36f, 0.62f, 1f);
            handleImg.raycastTarget = true;

            Scrollbar sb = scrollbarGo.AddComponent<Scrollbar>();
            sb.targetGraphic = handleImg;
            sb.handleRect = hRt;
            sb.direction = Scrollbar.Direction.TopToBottom;
            sb.size = 0.25f;
            sb.value = 0f;

            // 3d. Cấu hình TMP_InputField hoàn chỉnh (dùng AureliaOcrInputField để tránh bug OnScroll của TMP)
            AureliaOcrInputField customInput = inputGo.AddComponent<AureliaOcrInputField>();
            customInput.targetGraphic = inBg;
            customInput.textViewport = textAreaRt;
            customInput.textComponent = inTmp;
            customInput.pointSize = 19;
            customInput.caretColor = new Color(0.06f, 0.10f, 0.18f, 1f);
            customInput.selectionColor = new Color(0.25f, 0.55f, 0.95f, 0.35f);
            customInput.lineType = TMP_InputField.LineType.MultiLineNewline;
            ocrTranscriptInput = customInput;

            // 3e. Quản lý cuộn chuyên dụng: scrollbar drag, mouse wheel, caret sync, layout refresh
            OcrTranscriptScrollManager scrollMgr = inputGo.AddComponent<OcrTranscriptScrollManager>();
            scrollMgr.Setup(ocrTranscriptInput, inTmp, textAreaRt, sb);
            customInput.OnUserScroll = scrollMgr.HandleScrollDelta;

            var fwdTextArea = textAreaGo.AddComponent<OcrScrollForwarder>();
            fwdTextArea.OnScrollEvent = scrollMgr.HandleScrollDelta;

            var fwdSb = scrollbarGo.AddComponent<OcrScrollForwarder>();
            fwdSb.OnScrollEvent = scrollMgr.HandleScrollDelta;

            var fwdHandle = handleGo.AddComponent<OcrScrollForwarder>();
            fwdHandle.OnScrollEvent = scrollMgr.HandleScrollDelta;

            // 4. Fixed Action Buttons (Hàng nút cố định phía dưới, không bị cuộn che)
            // 4a. Nút 1: Confirm (DÙNG BẢN CHỮ NÀY)
            GameObject btnConfirmGo = new GameObject("Btn_ConfirmOcr");
            btnConfirmGo.transform.SetParent(card.transform, false);
            RectTransform cRt = btnConfirmGo.AddComponent<RectTransform>();
            cRt.anchoredPosition = new Vector2(-240, -220);
            cRt.sizeDelta = new Vector2(230, 48);
            Image cImg = btnConfirmGo.AddComponent<Image>();
            cImg.color = new Color(0.12f, 0.58f, 0.32f, 1f);
            btnOcrConfirm = btnConfirmGo.AddComponent<Button>();
            btnOcrConfirm.targetGraphic = cImg;
            GameObject cTxtGo = new GameObject("Text");
            cTxtGo.transform.SetParent(btnConfirmGo.transform, false);
            var cTmp = cTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) cTmp.font = _defaultFont;
            cTmp.text = "DÙNG BẢN CHỮ NÀY";
            cTmp.fontSize = 17;
            cTmp.fontStyle = FontStyles.Bold;
            cTmp.alignment = TextAlignmentOptions.Center;
            cTmp.color = Color.white;
            cTmp.raycastTarget = false;
            btnOcrConfirm.onClick.AddListener(OnConfirmOcrClicked);

            // 4b. Nút 2: Retry (ĐỌC LẠI)
            GameObject btnRetryGo = new GameObject("Btn_RetryOcr");
            btnRetryGo.transform.SetParent(card.transform, false);
            RectTransform rRt = btnRetryGo.AddComponent<RectTransform>();
            rRt.anchoredPosition = new Vector2(20, -220);
            rRt.sizeDelta = new Vector2(170, 48);
            Image rImg = btnRetryGo.AddComponent<Image>();
            rImg.color = new Color(0.18f, 0.38f, 0.68f, 1f);
            btnOcrRetry = btnRetryGo.AddComponent<Button>();
            btnOcrRetry.targetGraphic = rImg;
            GameObject rTxtGo = new GameObject("Text");
            rTxtGo.transform.SetParent(btnRetryGo.transform, false);
            var rTmp = rTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) rTmp.font = _defaultFont;
            rTmp.text = "ĐỌC LẠI";
            rTmp.fontSize = 17;
            rTmp.fontStyle = FontStyles.Bold;
            rTmp.alignment = TextAlignmentOptions.Center;
            rTmp.color = Color.white;
            rTmp.raycastTarget = false;
            btnOcrRetry.onClick.AddListener(() =>
            {
                if (ocrModalRoot != null) ocrModalRoot.SetActive(false);
                OnOcrButtonClicked();
            });

            // 4c. Nút 3: Cancel (HỦY)
            GameObject btnCancelGo = new GameObject("Btn_CancelOcr");
            btnCancelGo.transform.SetParent(card.transform, false);
            RectTransform clRt = btnCancelGo.AddComponent<RectTransform>();
            clRt.anchoredPosition = new Vector2(240, -220);
            clRt.sizeDelta = new Vector2(150, 48);
            Image clImg = btnCancelGo.AddComponent<Image>();
            clImg.color = new Color(0.40f, 0.45f, 0.55f, 1f);
            btnOcrCancel = btnCancelGo.AddComponent<Button>();
            btnOcrCancel.targetGraphic = clImg;
            GameObject clTxtGo = new GameObject("Text");
            clTxtGo.transform.SetParent(btnCancelGo.transform, false);
            var clTmp = clTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) clTmp.font = _defaultFont;
            clTmp.text = "HỦY";
            clTmp.fontSize = 17;
            clTmp.fontStyle = FontStyles.Bold;
            clTmp.alignment = TextAlignmentOptions.Center;
            clTmp.color = Color.white;
            clTmp.raycastTarget = false;
            btnOcrCancel.onClick.AddListener(() => ocrModalRoot.SetActive(false));

            ocrModalRoot.SetActive(false);
        }

        #region === AI TUTOR MODAL (GIA SƯ AI GỢI Ý TỪNG BƯỚC) ===

        /// <summary>
        /// Xây dựng popup Gia sư AI Aurelia hướng dẫn học sinh làm bài theo từng bước.
        /// Đáp ứng tiêu chí sư phạm của Giảng viên: "Đến bước nào dừng bước đó, không giải hộ bước sau".
        /// </summary>
        private void BuildAiTutorModal()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Xóa triệt để modal cũ để luôn cập nhật phiên bản mới có ScrollRect, Viewport, Scrollbar và Card động
            Transform existing = canvas.transform.Find("AiTutorModal");
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }

            tutorModalRoot = new GameObject("AiTutorModal");
            tutorModalRoot.transform.SetParent(canvas.transform, false);

            RectTransform rootRt = tutorModalRoot.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.sizeDelta = Vector2.zero;

            Image overlay = tutorModalRoot.AddComponent<Image>();
            overlay.color = new Color(0.04f, 0.08f, 0.16f, 0.92f);
            overlay.raycastTarget = true;

            // Card phong cách Aurelia Navy - Gold Ornate (840 x 560)
            GameObject card = new GameObject("TutorCard");
            card.transform.SetParent(tutorModalRoot.transform, false);
            RectTransform cardRt = card.AddComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(840, 560);
            cardRt.anchoredPosition = Vector2.zero;

            Image cardImg = card.AddComponent<Image>();
            cardImg.color = new Color(0.07f, 0.13f, 0.25f, 0.98f);

            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            cardOutline.effectDistance = new Vector2(2, -2);

            // 1. Header Title (Cố định ở trên: Y = 240, height = 36)
            GameObject titleGo = new GameObject("HeaderTitle");
            titleGo.transform.SetParent(card.transform, false);
            RectTransform titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0, 240);
            titleRt.sizeDelta = new Vector2(800, 36);
            tutorHeaderTitle = titleGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) tutorHeaderTitle.font = _defaultFont;
            tutorHeaderTitle.text = "✨ GIA SƯ AI AURELIA — HƯỚNG DẪN TỪNG BƯỚC";
            tutorHeaderTitle.fontSize = 22;
            tutorHeaderTitle.fontStyle = FontStyles.Bold;
            tutorHeaderTitle.alignment = TextAlignmentOptions.Center;
            tutorHeaderTitle.color = new Color(0.96f, 0.82f, 0.38f, 1f);
            tutorHeaderTitle.raycastTarget = false;

            // 2. Tiến độ / Tóm tắt đánh giá nháp (Cố định dưới Header: Y = 206, height = 26)
            GameObject summaryGo = new GameObject("StepProgressText");
            summaryGo.transform.SetParent(card.transform, false);
            RectTransform sumRt = summaryGo.AddComponent<RectTransform>();
            sumRt.anchoredPosition = new Vector2(0, 206);
            sumRt.sizeDelta = new Vector2(800, 26);
            tutorStepProgressText = summaryGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) tutorStepProgressText.font = _defaultFont;
            tutorStepProgressText.fontSize = 15;
            tutorStepProgressText.alignment = TextAlignmentOptions.Center;
            tutorStepProgressText.color = new Color(0.85f, 0.92f, 1f, 1f);
            tutorStepProgressText.raycastTarget = false;

            // 3. Vùng cuộn danh sách các bước đánh giá (StepsScrollView: 800 x 216, centered at Y = 72)
            GameObject scrollGo = new GameObject("StepsScrollView");
            scrollGo.transform.SetParent(card.transform, false);
            RectTransform sRt = scrollGo.AddComponent<RectTransform>();
            sRt.anchoredPosition = new Vector2(0, 72);
            sRt.sizeDelta = new Vector2(800, 216);

            ScrollRect sr = scrollGo.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 28f;
            tutorStepsScrollRect = sr;

            // 3a. Viewport có RectMask2D và nền tối
            GameObject vpGo = new GameObject("Viewport");
            vpGo.transform.SetParent(scrollGo.transform, false);
            RectTransform vpRt = vpGo.AddComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.pivot = new Vector2(0, 1);
            vpRt.offsetMin = Vector2.zero;
            vpRt.offsetMax = new Vector2(-18f, 0f); // 18px bên phải cho VerticalScrollbar

            Image vpImg = vpGo.AddComponent<Image>();
            vpImg.color = new Color(0.04f, 0.08f, 0.16f, 0.55f);
            vpImg.raycastTarget = true;
            vpGo.AddComponent<RectMask2D>();
            sr.viewport = vpRt;

            // 3b. Content (tutorStepsContainer) tự động co giãn theo VerticalLayoutGroup + ContentSizeFitter
            GameObject contentGo = new GameObject("Content");
            contentGo.transform.SetParent(vpGo.transform, false);
            tutorStepsContainer = contentGo.AddComponent<RectTransform>();
            tutorStepsContainer.anchorMin = new Vector2(0, 1);
            tutorStepsContainer.anchorMax = new Vector2(1, 1);
            tutorStepsContainer.pivot = new Vector2(0.5f, 1);
            tutorStepsContainer.sizeDelta = Vector2.zero;

            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10f;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = tutorStepsContainer;

            // 3c. Thanh cuộn dọc VerticalScrollbar hoạt động thực tế
            GameObject scrollbarGo = new GameObject("VerticalScrollbar");
            scrollbarGo.transform.SetParent(scrollGo.transform, false);
            RectTransform sbRt = scrollbarGo.AddComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(1f, 0f);
            sbRt.anchorMax = new Vector2(1f, 1f);
            sbRt.pivot = new Vector2(1f, 0.5f);
            sbRt.anchoredPosition = Vector2.zero;
            sbRt.sizeDelta = new Vector2(14f, 0f);

            Image sbBg = scrollbarGo.AddComponent<Image>();
            sbBg.color = new Color(0.06f, 0.10f, 0.20f, 0.85f);
            sbBg.raycastTarget = true;

            GameObject slidingArea = new GameObject("Sliding Area");
            slidingArea.transform.SetParent(scrollbarGo.transform, false);
            RectTransform saRt = slidingArea.AddComponent<RectTransform>();
            saRt.anchorMin = Vector2.zero;
            saRt.anchorMax = Vector2.one;
            saRt.sizeDelta = new Vector2(-14f, -14f);

            GameObject handleGo = new GameObject("Handle");
            handleGo.transform.SetParent(slidingArea.transform, false);
            RectTransform hRt = handleGo.AddComponent<RectTransform>();
            hRt.anchorMin = Vector2.zero;
            hRt.anchorMax = Vector2.one;
            hRt.sizeDelta = new Vector2(14f, 14f);

            Image handleImg = handleGo.AddComponent<Image>();
            handleImg.color = new Color(0.20f, 0.45f, 0.80f, 1f); // Xanh Sapphire
            handleImg.raycastTarget = true;

            Scrollbar sb = scrollbarGo.AddComponent<Scrollbar>();
            sb.targetGraphic = handleImg;
            sb.handleRect = hRt;
            sb.direction = Scrollbar.Direction.BottomToTop;
            sb.size = 0.35f;
            sb.value = 1f;
            tutorStepsScrollbar = sb;

            sr.verticalScrollbar = sb;
            sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

            // 4. Khung Gợi ý tiếp theo (CỐ ĐỊNH PHÍA DƯỚI: Y = -96, height = 96)
            GameObject hintBoxGo = new GameObject("HintBox");
            hintBoxGo.transform.SetParent(card.transform, false);
            RectTransform hbRt = hintBoxGo.AddComponent<RectTransform>();
            hbRt.anchoredPosition = new Vector2(0, -96);
            hbRt.sizeDelta = new Vector2(800, 96);
            Image hbImg = hintBoxGo.AddComponent<Image>();
            hbImg.color = new Color(0.10f, 0.20f, 0.35f, 0.95f);
            Outline hbOutline = hintBoxGo.AddComponent<Outline>();
            hbOutline.effectColor = new Color(0.40f, 0.70f, 1.0f, 0.8f);
            hbOutline.effectDistance = new Vector2(1.5f, -1.5f);

            GameObject hintTxtGo = new GameObject("HintText");
            hintTxtGo.transform.SetParent(hintBoxGo.transform, false);
            RectTransform htRt = hintTxtGo.AddComponent<RectTransform>();
            htRt.anchorMin = Vector2.zero;
            htRt.anchorMax = Vector2.one;
            htRt.offsetMin = new Vector2(16, 8);
            htRt.offsetMax = new Vector2(-16, -8);
            tutorHintBodyText = hintTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) tutorHintBodyText.font = _defaultFont;
            tutorHintBodyText.fontSize = 15;
            tutorHintBodyText.alignment = TextAlignmentOptions.TopLeft;
            tutorHintBodyText.textWrappingMode = TextWrappingModes.Normal;
            tutorHintBodyText.color = Color.white;
            tutorHintBodyText.raycastTarget = false;

            // 5. Nút "ĐÃ HIỂU GỢI Ý — TIẾP TỤC LÀM" (CỐ ĐỊNH DƯỚI CÙNG: Y = -220, height = 46)
            GameObject btnContinueGo = new GameObject("Btn_TutorContinue");
            btnContinueGo.transform.SetParent(card.transform, false);
            RectTransform bcRt = btnContinueGo.AddComponent<RectTransform>();
            bcRt.anchoredPosition = new Vector2(0, -220);
            bcRt.sizeDelta = new Vector2(340, 46);
            Image bcImg = btnContinueGo.AddComponent<Image>();
            bcImg.color = new Color(0.15f, 0.65f, 0.35f, 1f);
            btnTutorDismiss = btnContinueGo.AddComponent<Button>();
            btnTutorDismiss.targetGraphic = bcImg;

            GameObject bcTxtGo = new GameObject("Text");
            bcTxtGo.transform.SetParent(btnContinueGo.transform, false);
            var bcTmp = bcTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) bcTmp.font = _defaultFont;
            bcTmp.text = "✓ ĐÃ HIỂU GỢI Ý — TIẾP TỤC LÀM";
            bcTmp.fontSize = 16;
            bcTmp.fontStyle = FontStyles.Bold;
            bcTmp.alignment = TextAlignmentOptions.Center;
            bcTmp.color = Color.white;
            bcTmp.raycastTarget = false;
            btnTutorDismiss.onClick.AddListener(() => tutorModalRoot.SetActive(false));
            UIHoverPulseEffect.Attach(btnContinueGo, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.07f, outline: true);

            // 6. Panel Lời giải mẫu (Chỉ bật khi cần)
            GameObject solPanelGo = new GameObject("TutorModelSolPanel");
            solPanelGo.transform.SetParent(card.transform, false);
            RectTransform spRt = solPanelGo.AddComponent<RectTransform>();
            spRt.anchorMin = Vector2.zero;
            spRt.anchorMax = Vector2.one;
            spRt.offsetMin = new Vector2(20, 20);
            spRt.offsetMax = new Vector2(-20, -20);
            Image spBg = solPanelGo.AddComponent<Image>();
            spBg.color = new Color(0.05f, 0.10f, 0.20f, 0.98f);
            Outline spOutline = solPanelGo.AddComponent<Outline>();
            spOutline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            spOutline.effectDistance = new Vector2(2, -2);
            tutorModelSolPanel = solPanelGo;

            GameObject solTxtGo = new GameObject("Text");
            solTxtGo.transform.SetParent(solPanelGo.transform, false);
            RectTransform stRt = solTxtGo.AddComponent<RectTransform>();
            stRt.anchorMin = Vector2.zero;
            stRt.anchorMax = Vector2.one;
            stRt.offsetMin = new Vector2(20, 70);
            stRt.offsetMax = new Vector2(-20, -20);
            tutorModelSolText = solTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) tutorModelSolText.font = _defaultFont;
            tutorModelSolText.fontSize = 16;
            tutorModelSolText.alignment = TextAlignmentOptions.TopLeft;
            tutorModelSolText.textWrappingMode = TextWrappingModes.Normal;
            tutorModelSolText.color = Color.white;

            GameObject closeSolGo = new GameObject("Btn_CloseSol");
            closeSolGo.transform.SetParent(solPanelGo.transform, false);
            RectTransform csRt = closeSolGo.AddComponent<RectTransform>();
            csRt.anchoredPosition = new Vector2(0, -210);
            csRt.sizeDelta = new Vector2(200, 42);
            Image csImg = closeSolGo.AddComponent<Image>();
            csImg.color = new Color(0.40f, 0.45f, 0.55f, 1f);
            Button csBtn = closeSolGo.AddComponent<Button>();
            csBtn.targetGraphic = csImg;
            GameObject csTxtGo = new GameObject("Text");
            csTxtGo.transform.SetParent(closeSolGo.transform, false);
            var csTmp = csTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) csTmp.font = _defaultFont;
            csTmp.text = "ĐÓNG LỜI GIẢI";
            csTmp.fontSize = 16;
            csTmp.fontStyle = FontStyles.Bold;
            csTmp.alignment = TextAlignmentOptions.Center;
            csTmp.color = Color.white;
            csBtn.onClick.AddListener(() => tutorModelSolPanel.SetActive(false));

            tutorModelSolPanel.SetActive(false);
            tutorModalRoot.SetActive(false);
        }

        private void OnAiTutorClicked()
        {
            Debug.Log($"[WrittenAnswerController] Yêu cầu gợi ý Gia sư AI cho Câu {_currentIndex + 1}.");

            if (answerInputField != null)
            {
                _userAnswers[_currentIndex] = answerInputField.text;
            }

            string currentAnswer = _userAnswers.ContainsKey(_currentIndex) ? _userAnswers[_currentIndex].Trim() : "";
            if (string.IsNullOrEmpty(currentAnswer))
            {
                ShowNotice("<color=#f59e0b>Hãy viết những bước bạn đã làm được để Gia sư AI hướng dẫn bước tiếp theo nhé!</color>");
                return;
            }

            if (btnAiTutor != null) btnAiTutor.interactable = false;
            ShowNotice("<color=#38bdf8>✨ Gia sư AI đang phân tích các bước bài làm...</color>");

            var currentQ = (_essayQuestions != null && _currentIndex < _essayQuestions.Count) ? _essayQuestions[_currentIndex] : null;
            string cauHoiId = (currentQ != null) ? currentQ.cauHoiID : "TL_C1_L2_Duy";

            EssayGradingService.GradeEssayAsync(cauHoiId, currentAnswer, (result) =>
            {
                if (btnAiTutor != null) btnAiTutor.interactable = true;
                if (noticeText != null) noticeText.gameObject.SetActive(false);

                if (result == null || !result.success || result.mode == "error")
                {
                    ShowNotice("<color=#ef4444>⚠ Không thể kết nối Gemini API: " + (result != null ? result.generalFeedback : "Lỗi kết nối") + "</color>");
                    return;
                }

                ShowTutorModal(result);
            });
        }

        private void ShowTutorModal(EssayGradingResult result)
        {
            if (tutorModalRoot == null) BuildAiTutorModal();
            if (tutorModalRoot == null) return;

            if (tutorModelSolPanel != null) tutorModelSolPanel.SetActive(false);

            int correctCount = 0;
            int totalSteps = result.steps != null ? result.steps.Count : 0;
            if (result.steps != null)
            {
                foreach (var s in result.steps)
                {
                    if (s.Status == "correct") correctCount++;
                }
            }

            if (tutorStepProgressText != null)
            {
                tutorStepProgressText.text = $"Tiến độ: <b>{correctCount}/{totalSteps} bước hoàn thành</b> | <color=#94a3b8>(Đánh giá nháp gợi ý — Chưa nộp bài chính thức)</color>";
            }

            if (tutorStepsContainer != null)
            {
                // Ngắt parent các card cũ trước khi Destroy để dọn dẹp tức thì cho LayoutRebuilder
                List<GameObject> toDestroy = new List<GameObject>();
                foreach (Transform child in tutorStepsContainer)
                {
                    toDestroy.Add(child.gameObject);
                }
                for (int i = 0; i < toDestroy.Count; i++)
                {
                    toDestroy[i].transform.SetParent(null);
                    Destroy(toDestroy[i]);
                }

                if (result.steps != null)
                {
                    for (int i = 0; i < result.steps.Count; i++)
                    {
                        var s = result.steps[i];
                        CreateTutorStepCard(tutorStepsContainer, s, i + 1);
                    }
                }

                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(tutorStepsContainer);
            }

            if (tutorHintBodyText != null)
            {
                string hint = !string.IsNullOrEmpty(result.nextStepHint)
                    ? result.nextStepHint
                    : "Hãy tiếp tục viết các bước giải kế tiếp dựa trên các định luật vật lý đã học.";
                tutorHintBodyText.text = $"<b>💡 GỢI Ý BƯỚC TIẾP THEO:</b>\n{hint}";
            }

            if (btnTutorShowModelSol != null)
            {
                btnTutorShowModelSol.onClick.RemoveAllListeners();
                btnTutorShowModelSol.onClick.AddListener(() =>
                {
                    if (tutorModelSolPanel != null && tutorModelSolText != null)
                    {
                        string sol = !string.IsNullOrEmpty(result.modelSolution) ? result.modelSolution : "Chưa có lời giải mẫu cho câu này.";
                        tutorModelSolText.text = $"<b>LỜI GIẢI THAM KHẢO CHI TIẾT:</b>\n\n{sol}\n\n<i>Lưu ý: Xem lời giải tham khảo sẽ giúp bạn nắm vững phương pháp giải, hãy tự làm lại để khôi phục trạm nhé!</i>";
                        tutorModelSolPanel.SetActive(true);
                    }
                });
            }

            tutorModalRoot.transform.SetAsLastSibling();
            tutorModalRoot.SetActive(true);

            StartCoroutine(DelayedResetTutorScroll());
        }

        private IEnumerator DelayedResetTutorScroll()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            if (tutorStepsContainer != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(tutorStepsContainer);
            }
            if (tutorStepsScrollRect != null)
            {
                tutorStepsScrollRect.verticalNormalizedPosition = 1f;
            }
            if (tutorStepsScrollbar != null)
            {
                tutorStepsScrollbar.value = 1f;
            }
        }

        private void CreateTutorStepCard(Transform parent, GradingStepFeedback step, int stepNum)
        {
            GameObject item = new GameObject($"TutorStep_{step.StepId}");
            item.transform.SetParent(parent, false);

            RectTransform rt = item.AddComponent<RectTransform>();

            Image bg = item.AddComponent<Image>();
            bg.raycastTarget = false;

            Outline ol = item.AddComponent<Outline>();
            ol.effectDistance = new Vector2(1.5f, -1.5f);

            string badge = "";
            Color badgeColor;
            Color bgColor;
            Color outlineColor;

            switch (step.Status)
            {
                case "correct":
                    badge = "✓ ĐÚNG";
                    bgColor = new Color(0.08f, 0.28f, 0.16f, 0.95f);
                    badgeColor = new Color(0.40f, 0.95f, 0.55f, 1f);
                    outlineColor = new Color(0.30f, 0.80f, 0.40f, 0.8f);
                    break;
                case "partial":
                    badge = "! CHƯA ĐỦ";
                    bgColor = new Color(0.38f, 0.28f, 0.08f, 0.95f);
                    badgeColor = new Color(1f, 0.85f, 0.35f, 1f);
                    outlineColor = new Color(0.90f, 0.75f, 0.20f, 0.8f);
                    break;
                case "incorrect":
                    badge = "✕ CẦN SỬA";
                    bgColor = new Color(0.38f, 0.10f, 0.10f, 0.95f);
                    badgeColor = new Color(1f, 0.45f, 0.45f, 1f);
                    outlineColor = new Color(0.90f, 0.25f, 0.25f, 0.8f);
                    break;
                default: // unassessable
                    badge = "⏸ CHƯA LÀM";
                    bgColor = new Color(0.14f, 0.18f, 0.28f, 0.95f);
                    badgeColor = new Color(0.70f, 0.82f, 0.98f, 1f);
                    outlineColor = new Color(0.35f, 0.50f, 0.70f, 0.8f);
                    break;
            }

            bg.color = bgColor;
            ol.effectColor = outlineColor;

            // Cấu hình Layout tự động xếp dọc theo nội dung cho từng Card
            VerticalLayoutGroup cardVlg = item.AddComponent<VerticalLayoutGroup>();
            cardVlg.padding = new RectOffset(12, 12, 8, 8);
            cardVlg.spacing = 4f;
            cardVlg.childControlWidth = true;
            cardVlg.childControlHeight = true;
            cardVlg.childForceExpandWidth = true;
            cardVlg.childForceExpandHeight = false;

            ContentSizeFitter cardCsf = item.AddComponent<ContentSizeFitter>();
            cardCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            cardCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            LayoutElement cardLe = item.AddComponent<LayoutElement>();
            cardLe.minHeight = 48f;

            // 1. Tiêu đề Bước (Badge + Tên bước + Điểm nháp)
            GameObject headerGo = new GameObject("Header");
            headerGo.transform.SetParent(item.transform, false);
            headerGo.AddComponent<RectTransform>();
            TextMeshProUGUI headerTmp = headerGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) headerTmp.font = _defaultFont;
            headerTmp.fontSize = 15;
            headerTmp.fontStyle = FontStyles.Bold;
            headerTmp.color = badgeColor;
            headerTmp.alignment = TextAlignmentOptions.TopLeft;
            headerTmp.textWrappingMode = TextWrappingModes.Normal;
            headerTmp.raycastTarget = false;
            headerTmp.text = $"[{badge}] BƯỚC {stepNum} ({step.score:F1}/{step.maxPoints:F1} điểm)";

            // 2. Nội dung chi tiết (Trích bài làm + Nhận xét + Hướng dẫn sửa)
            GameObject bodyGo = new GameObject("Body");
            bodyGo.transform.SetParent(item.transform, false);
            bodyGo.AddComponent<RectTransform>();
            TextMeshProUGUI bodyTmp = bodyGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) bodyTmp.font = _defaultFont;
            bodyTmp.fontSize = 14;
            bodyTmp.color = Color.white;
            bodyTmp.alignment = TextAlignmentOptions.TopLeft;
            bodyTmp.textWrappingMode = TextWrappingModes.Normal;
            bodyTmp.raycastTarget = false;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(step.StudentText))
            {
                sb.Append($"<b>Trích bài làm:</b> <i>\"{step.StudentText}\"</i>\n");
            }
            if (!string.IsNullOrEmpty(step.Explanation))
            {
                sb.Append($"<b>Nhận xét:</b> {step.Explanation}");
            }
            if (!string.IsNullOrEmpty(step.SuggestedCorrection))
            {
                if (sb.Length > 0) sb.Append("\n");
                sb.Append($"<color=#fef08a><b>💡 Hướng dẫn sửa:</b> {step.SuggestedCorrection}</color>");
            }
            if (sb.Length == 0)
            {
                sb.Append("<i>(Chưa phát hiện nội dung của bước này trong bài làm)</i>");
            }

            bodyTmp.text = sb.ToString();

            headerTmp.ForceMeshUpdate();
            bodyTmp.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

        #endregion

        private void ShowOcrTranscriptModal(string text)
        {
            if (ocrModalRoot != null)
            {
                ocrModalRoot.transform.SetAsLastSibling();
                ocrModalRoot.SetActive(true);

                if (ocrTranscriptInput != null)
                {
                    string cleaned = CleanOcrTextForStudent(text);
                    ocrTranscriptInput.text = cleaned;
                    ocrTranscriptInput.caretPosition = 0;
                }

                var scrollMgr = ocrModalRoot.GetComponentInChildren<OcrTranscriptScrollManager>();
                if (scrollMgr != null)
                {
                    scrollMgr.RefreshLayout(true);
                    StartCoroutine(DelayedRefreshOcrModal(scrollMgr));
                }
            }
        }

        private IEnumerator DelayedRefreshOcrModal(OcrTranscriptScrollManager scrollMgr)
        {
            yield return null;
            if (scrollMgr != null)
            {
                scrollMgr.RefreshLayout(true);
            }
        }

        private static string CleanOcrTextForStudent(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            string cleaned = raw;
            cleaned = cleaned.Replace("\\Omega", "Ω").Replace("\\omega", "ω");
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\$(.*?)\$", "$1");
            cleaned = cleaned.Replace("$", "");
            return cleaned.Trim();
        }

        private void OnConfirmOcrClicked()
        {
            if (ocrTranscriptInput == null) return;
            string confirmedText = ocrTranscriptInput.text != null ? ocrTranscriptInput.text.Trim() : "";

            if (string.IsNullOrWhiteSpace(confirmedText))
            {
                ShowToast("⚠ Bản chữ đang trống! Vui lòng nhập hoặc kiểm tra nội dung bài làm.", ToastType.Warning, 3.5f);
                return;
            }

            if (ocrModalRoot != null) ocrModalRoot.SetActive(false);

            if (answerInputField != null)
            {
                string existing = answerInputField.text != null ? answerInputField.text.Trim() : "";
                if (string.IsNullOrEmpty(existing))
                {
                    answerInputField.text = confirmedText;
                    _userAnswers[_currentIndex] = confirmedText;
                    ShowToast($"✓ Đã đưa bản chữ nhận dạng vào bài làm Câu {_currentIndex + 1}!", ToastType.Success, 2.5f);
                }
                else
                {
                    ShowMergeChoiceModal(existing, confirmedText);
                }
            }
        }

        private void ShowMergeChoiceModal(string existingText, string ocrText)
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject modalGo = new GameObject("MergeChoiceModal");
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
            cRt.sizeDelta = new Vector2(560, 260);
            cRt.anchoredPosition = Vector2.zero;

            Image cImg = cardGo.AddComponent<Image>();
            cImg.color = new Color(0.08f, 0.14f, 0.28f, 0.98f);

            Outline outline = cardGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            outline.effectDistance = new Vector2(2, -2);

            GameObject msgGo = new GameObject("Message");
            msgGo.transform.SetParent(cardGo.transform, false);
            RectTransform mRt = msgGo.AddComponent<RectTransform>();
            mRt.anchoredPosition = new Vector2(0, 45);
            mRt.sizeDelta = new Vector2(500, 80);
            var msgTmp = msgGo.AddComponent<TextMeshProUGUI>();
            msgTmp.text = "Bạn đã có nội dung gõ tay trước đó.\nBạn muốn áp dụng bản chữ OCR này như thế nào?";
            msgTmp.fontSize = 19;
            msgTmp.alignment = TextAlignmentOptions.Center;
            msgTmp.color = Color.white;

            // Nút 1: Ghép vào cuối
            GameObject btnAppendGo = new GameObject("Btn_Append");
            btnAppendGo.transform.SetParent(cardGo.transform, false);
            RectTransform apRt = btnAppendGo.AddComponent<RectTransform>();
            apRt.anchoredPosition = new Vector2(-170, -50);
            apRt.sizeDelta = new Vector2(150, 44);
            Image apImg = btnAppendGo.AddComponent<Image>();
            apImg.color = new Color(0.2f, 0.6f, 0.3f, 1f);
            Button apBtn = btnAppendGo.AddComponent<Button>();
            apBtn.targetGraphic = apImg;
            GameObject apTxtGo = new GameObject("Text");
            apTxtGo.transform.SetParent(btnAppendGo.transform, false);
            var apTmp = apTxtGo.AddComponent<TextMeshProUGUI>();
            apTmp.text = "GHÉP CUỐI";
            apTmp.fontSize = 16;
            apTmp.fontStyle = FontStyles.Bold;
            apTmp.alignment = TextAlignmentOptions.Center;
            apTmp.color = Color.white;
            apTmp.raycastTarget = false;
            apBtn.onClick.AddListener(() =>
            {
                answerInputField.text = existingText + "\n\n" + ocrText;
                _userAnswers[_currentIndex] = answerInputField.text;
                Destroy(modalGo);
                ShowToast("✓ Đã ghép nội dung OCR vào cuối bài làm!", ToastType.Success, 2.5f);
            });

            // Nút 2: Thay toàn bộ
            GameObject btnReplaceGo = new GameObject("Btn_Replace");
            btnReplaceGo.transform.SetParent(cardGo.transform, false);
            RectTransform rpRt = btnReplaceGo.AddComponent<RectTransform>();
            rpRt.anchoredPosition = new Vector2(0, -50);
            rpRt.sizeDelta = new Vector2(150, 44);
            Image rpImg = btnReplaceGo.AddComponent<Image>();
            rpImg.color = new Color(0.8f, 0.35f, 0.2f, 1f);
            Button rpBtn = btnReplaceGo.AddComponent<Button>();
            rpBtn.targetGraphic = rpImg;
            GameObject rpTxtGo = new GameObject("Text");
            rpTxtGo.transform.SetParent(btnReplaceGo.transform, false);
            var rpTmp = rpTxtGo.AddComponent<TextMeshProUGUI>();
            rpTmp.text = "THAY HẾT";
            rpTmp.fontSize = 16;
            rpTmp.fontStyle = FontStyles.Bold;
            rpTmp.alignment = TextAlignmentOptions.Center;
            rpTmp.color = Color.white;
            rpTmp.raycastTarget = false;
            rpBtn.onClick.AddListener(() =>
            {
                answerInputField.text = ocrText;
                _userAnswers[_currentIndex] = answerInputField.text;
                Destroy(modalGo);
                ShowToast("✓ Đã thay thế toàn bộ bằng nội dung OCR!", ToastType.Success, 2.5f);
            });

            // Nút 3: Hủy
            GameObject btnCancelGo = new GameObject("Btn_Cancel");
            btnCancelGo.transform.SetParent(cardGo.transform, false);
            RectTransform canRt = btnCancelGo.AddComponent<RectTransform>();
            canRt.anchoredPosition = new Vector2(170, -50);
            canRt.sizeDelta = new Vector2(130, 44);
            Image canImg = btnCancelGo.AddComponent<Image>();
            canImg.color = new Color(0.4f, 0.45f, 0.55f, 1f);
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

        /// <summary>
        /// Nộp bài tự luận chung cho cả 2 cách làm (gõ trực tiếp bằng bàn phím hoặc tải ảnh qua OCR):
        /// 1. Đồng bộ câu trả lời cuối cùng từ ô answerInputField.
        /// 2. Kiểm tra nếu học sinh tải ảnh nhưng chưa bấm OCR: hướng dẫn OCR hoặc gõ chữ.
        /// 3. Nếu còn câu chưa làm, hiển thị modal thông báo số câu trống cho học sinh chọn nộp tiếp hay làm thêm.
        /// 4. Gửi nội dung bài làm tới Gemini AI để chấm theo rubric từng bước.
        /// </summary>
        private void OnSubmitClicked()
        {
            if (answerInputField != null)
            {
                _userAnswers[_currentIndex] = answerInputField.text;
            }

            int totalQ = _essayQuestions != null && _essayQuestions.Count > 0 ? _essayQuestions.Count : 5;
            int completedCount = 0;
            for (int i = 0; i < totalQ; i++)
            {
                if (_userAnswers.ContainsKey(i) && !string.IsNullOrWhiteSpace(_userAnswers[i]) && _userAnswers[i].Trim().Length >= 10)
                {
                    completedCount++;
                }
            }

            // Kiểm tra nếu chưa có câu nào đạt yêu cầu nội dung
            if (completedCount == 0)
            {
                bool hasAnyImage = _userImages.Values.Any(tex => tex != null);
                if (hasAnyImage)
                {
                    ShowToast("⚠ Bạn đã tải ảnh bài làm nhưng chưa bấm ĐỌC CHỮ OCR! Hãy bấm ĐỌC CHỮ OCR hoặc gõ câu trả lời trước khi nộp.", ToastType.Warning, 4.5f);
                    return;
                }

                // Kiểm tra xem có câu nào có chữ nhưng quá ngắn không
                bool hasShortAnswer = _userAnswers.Values.Any(a => !string.IsNullOrWhiteSpace(a));
                if (hasShortAnswer)
                {
                    ShowToast("⚠ Câu trả lời quá ngắn! Hãy trình bày chi tiết các bước giải trước khi nộp.", ToastType.Warning, 3.5f);
                    return;
                }

                ShowToast("⚠ Vui lòng gõ câu trả lời hoặc tải ảnh và bấm ĐỌC CHỮ OCR trước khi nộp bài.", ToastType.Warning, 3.5f);
                return;
            }

            // Nếu còn câu chưa làm: Hiển thị popup xác nhận theo Nghiệp vụ Mục 9
            if (completedCount < totalQ)
            {
                ShowIncompleteSubmitConfirmModal(completedCount, totalQ, ExecuteGradingAndSubmit);
            }
            else
            {
                ExecuteGradingAndSubmit();
            }
        }

        private void ShowIncompleteSubmitConfirmModal(int completedCount, int totalQ, Action onConfirm)
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                onConfirm?.Invoke();
                return;
            }

            GameObject modalGo = new GameObject("IncompleteSubmitModal");
            modalGo.transform.SetParent(canvas.transform, false);

            RectTransform rt = modalGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image bg = modalGo.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.08f, 0.16f, 0.90f);

            GameObject cardGo = new GameObject("Card");
            cardGo.transform.SetParent(modalGo.transform, false);
            RectTransform cRt = cardGo.AddComponent<RectTransform>();
            cRt.sizeDelta = new Vector2(580, 270);
            cRt.anchoredPosition = Vector2.zero;

            Image cImg = cardGo.AddComponent<Image>();
            cImg.color = new Color(0.07f, 0.13f, 0.25f, 0.98f);

            Outline outline = cardGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            outline.effectDistance = new Vector2(2, -2);

            // Title
            GameObject titleGo = new GameObject("Title");
            titleGo.transform.SetParent(cardGo.transform, false);
            RectTransform tRt = titleGo.AddComponent<RectTransform>();
            tRt.anchoredPosition = new Vector2(0, 85);
            tRt.sizeDelta = new Vector2(520, 36);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) titleTmp.font = _defaultFont;
            titleTmp.text = "✨ XÁC NHẬN NỘP BÀI TỰ LUẬN";
            titleTmp.fontSize = 20;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.96f, 0.82f, 0.38f, 1f);

            // Message
            GameObject msgGo = new GameObject("Message");
            msgGo.transform.SetParent(cardGo.transform, false);
            RectTransform mRt = msgGo.AddComponent<RectTransform>();
            mRt.anchoredPosition = new Vector2(0, 25);
            mRt.sizeDelta = new Vector2(520, 75);
            var msgTmp = msgGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) msgTmp.font = _defaultFont;
            int unDone = totalQ - completedCount;
            msgTmp.text = $"Bạn đã làm <b><color=#4ade80>{completedCount}/{totalQ} câu</color></b>.\nCòn <b><color=#f87171>{unDone} câu</color></b> chưa làm (sẽ tính 0 điểm).\n\nBạn có muốn nộp ngay phần bài đã hoàn thành không?";
            msgTmp.fontSize = 17;
            msgTmp.alignment = TextAlignmentOptions.Center;
            msgTmp.color = Color.white;

            // Nút 1: Làm tiếp
            GameObject btnStayGo = new GameObject("Btn_Stay");
            btnStayGo.transform.SetParent(cardGo.transform, false);
            RectTransform sRt = btnStayGo.AddComponent<RectTransform>();
            sRt.anchoredPosition = new Vector2(-120, -60);
            sRt.sizeDelta = new Vector2(170, 44);
            Image sImg = btnStayGo.AddComponent<Image>();
            sImg.color = new Color(0.25f, 0.35f, 0.50f, 1f);
            Button sBtn = btnStayGo.AddComponent<Button>();
            sBtn.targetGraphic = sImg;
            GameObject sTxtGo = new GameObject("Text");
            sTxtGo.transform.SetParent(btnStayGo.transform, false);
            var sTmp = sTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) sTmp.font = _defaultFont;
            sTmp.text = "LÀM TIẾP";
            sTmp.fontSize = 16;
            sTmp.fontStyle = FontStyles.Bold;
            sTmp.alignment = TextAlignmentOptions.Center;
            sTmp.color = Color.white;
            sTmp.raycastTarget = false;
            sBtn.onClick.AddListener(() => Destroy(modalGo));
            UIHoverPulseEffect.Attach(btnStayGo, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.07f, outline: true);

            // Nút 2: Xác nhận nộp
            GameObject btnConfirmGo = new GameObject("Btn_Confirm");
            btnConfirmGo.transform.SetParent(cardGo.transform, false);
            RectTransform cfRt = btnConfirmGo.AddComponent<RectTransform>();
            cfRt.anchoredPosition = new Vector2(120, -60);
            cfRt.sizeDelta = new Vector2(170, 44);
            Image cfImg = btnConfirmGo.AddComponent<Image>();
            cfImg.color = new Color(0.15f, 0.65f, 0.35f, 1f);
            Button cfBtn = btnConfirmGo.AddComponent<Button>();
            cfBtn.targetGraphic = cfImg;
            GameObject cfTxtGo = new GameObject("Text");
            cfTxtGo.transform.SetParent(btnConfirmGo.transform, false);
            var cfTmp = cfTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) cfTmp.font = _defaultFont;
            cfTmp.text = "XÁC NHẬN NỘP";
            cfTmp.fontSize = 16;
            cfTmp.fontStyle = FontStyles.Bold;
            cfTmp.alignment = TextAlignmentOptions.Center;
            cfTmp.color = Color.white;
            cfTmp.raycastTarget = false;
            cfBtn.onClick.AddListener(() =>
            {
                Destroy(modalGo);
                onConfirm?.Invoke();
            });
            UIHoverPulseEffect.Attach(btnConfirmGo, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.07f, outline: true);
        }

        private void ExecuteGradingAndSubmit()
        {
            if (btnSubmit != null) btnSubmit.interactable = false;
            ShowToast("⏳ Đang gửi bài làm tới Gemini AI để chấm từng bước theo rubric...", ToastType.Info, 4.5f);

            int totalQ = _essayQuestions != null && _essayQuestions.Count > 0 ? _essayQuestions.Count : 5;
            float elapsedSeconds = Mathf.Max(1f, Time.time - _sessionStartTime);

            StartCoroutine(GradeAllQuestionsAndSubmitRoutine(totalQ, elapsedSeconds));
        }

        private IEnumerator GradeAllQuestionsAndSubmitRoutine(int totalQ, float elapsedSeconds)
        {
            List<EssayAnswerRecord> recordsList = new List<EssayAnswerRecord>();
            float totalScoreAccumulated = 0f;
            int attemptedCount = 0;
            EssayGradingResult primaryGradingResult = null;

            for (int i = 0; i < totalQ; i++)
            {
                var q = (i < _essayQuestions.Count) ? _essayQuestions[i] : null;
                string cauHoiId = (q != null && !string.IsNullOrEmpty(q.cauHoiID)) ? q.cauHoiID : $"TL_C1_L2_{i + 1}";
                string answerText = (_userAnswers.ContainsKey(i) && !string.IsNullOrWhiteSpace(_userAnswers[i])) ? _userAnswers[i].Trim() : "";
                bool isAttempted = (answerText.Length >= 10);

                if (isAttempted)
                {
                    attemptedCount++;
                    bool requestDone = false;
                    EssayGradingResult gradRes = null;

                    EssayGradingService.GradeEssayAsync(cauHoiId, answerText, (res) =>
                    {
                        gradRes = res;
                        requestDone = true;
                    });

                    // Chờ request hoàn thành
                    while (!requestDone)
                    {
                        yield return null;
                    }

                    if (gradRes == null || !gradRes.success || gradRes.mode == "error")
                    {
                        if (btnSubmit != null) btnSubmit.interactable = true;
                        ShowToast("⚠ Không thể kết nối Gemini API: " + (gradRes != null ? gradRes.generalFeedback : "Lỗi kết nối backend"), ToastType.Error, 5f);
                        yield break; // Dừng lại, KHÔNG chấm tiếp, KHÔNG cấp khôi phục khi lỗi
                    }

                    if (primaryGradingResult == null) primaryGradingResult = gradRes;

                    float qScore10 = (gradRes.totalScore <= 10f) ? (float)Math.Round(gradRes.totalScore, 2) : (float)Math.Round(gradRes.totalScore / 10f, 2);
                    totalScoreAccumulated += qScore10;

                    var rec = new EssayAnswerRecord
                    {
                        CauHoiID = cauHoiId,
                        QuestionText = (q != null && !string.IsNullOrEmpty(q.noiDung)) ? q.noiDung : $"Câu {i + 1}",
                        SampleAnswer = !string.IsNullOrEmpty(gradRes.modelSolution) ? gradRes.modelSolution : ((q != null && !string.IsNullOrEmpty(q.dapAnMau)) ? q.dapAnMau : "Chưa có lời giải mẫu"),
                        Explanation = gradRes.generalFeedback,
                        ConfirmedText = answerText,
                        Score = qScore10,
                        IsGraded = true,
                        IsDraftRubric = !gradRes.rubricApproved,
                        Steps = gradRes.steps != null ? new List<GradingStepFeedback>(gradRes.steps) : new List<GradingStepFeedback>()
                    };
                    recordsList.Add(rec);
                }
                else
                {
                    // Câu chưa làm -> 0 điểm, không gọi Gemini để tiết kiệm thời gian
                    var rec = new EssayAnswerRecord
                    {
                        CauHoiID = cauHoiId,
                        QuestionText = (q != null && !string.IsNullOrEmpty(q.noiDung)) ? q.noiDung : $"Câu {i + 1}",
                        SampleAnswer = (q != null && !string.IsNullOrEmpty(q.dapAnMau)) ? q.dapAnMau : "Lời giải tham khảo chi tiết đang cập nhật.",
                        Explanation = "Học sinh chưa làm câu hỏi này.",
                        ConfirmedText = "(Chưa làm)",
                        Score = 0f,
                        IsGraded = true,
                        IsDraftRubric = false,
                        Steps = new List<GradingStepFeedback>()
                    };
                    recordsList.Add(rec);
                }
            }

            if (btnSubmit != null) btnSubmit.interactable = true;

            // Tính điểm trung bình trên cả 5 câu (Mỗi câu trọng số bằng nhau: tổng / 5)
            float finalScoreScale10 = (float)Math.Round(totalScoreAccumulated / totalQ, 2);
            float finalScoreScale100 = (float)Math.Round(finalScoreScale10 * 10f, 1);

            string lessonId = NavigationService.Instance.Selection.LessonId;
            string deviceId = NavigationService.Instance.Selection.DeviceId;
            string attemptId = Guid.NewGuid().ToString("N");

            // Lưu kết quả vào AttemptContext
            NavigationService.Instance.AttemptContext.AttemptId = attemptId;
            NavigationService.Instance.AttemptContext.Score10 = finalScoreScale10; // 0..10
            NavigationService.Instance.AttemptContext.ScoreRaw = finalScoreScale100; // 0..100
            NavigationService.Instance.AttemptContext.Score = finalScoreScale10; // 0..10
            NavigationService.Instance.AttemptContext.TotalQuestions = totalQ;
            NavigationService.Instance.AttemptContext.CorrectCount = attemptedCount;
            NavigationService.Instance.AttemptContext.DurationSeconds = elapsedSeconds;
            NavigationService.Instance.AttemptContext.EssayAnswers = recordsList;
            NavigationService.Instance.AttemptContext.GradingResult = primaryGradingResult ?? new EssayGradingResult
            {
                success = true,
                totalScore = finalScoreScale10,
                rubricApproved = true,
                generalFeedback = $"Hoàn thành {attemptedCount}/{totalQ} câu tự luận."
            };

            // Lưu dữ liệu vào PhysicsGameSession
            if (PhysicsGameSession.Instance != null)
            {
                var att = new QuizAttemptData
                {
                    lessonId = lessonId,
                    lessonTitle = NavigationService.Instance.Selection.LessonTitle,
                    score = Mathf.RoundToInt(finalScoreScale100),
                    correctCount = attemptedCount,
                    wrongCount = totalQ - attemptedCount,
                    unansweredCount = totalQ - attemptedCount,
                    elapsedSeconds = elapsedSeconds
                };
                PhysicsGameSession.Instance.CurrentQuizAttempt = att;
            }

            // Đạt ngưỡng: >= 8.0/10 tương đương >= 80/100
            bool isPassed = (finalScoreScale10 >= 8.0f - 1e-4f) || (finalScoreScale100 >= RestorationService.PASS_THRESHOLD - 1e-4f);
            if (isPassed)
            {
                bool allRubricsApproved = recordsList.All(r => !r.IsDraftRubric);
                if (allRubricsApproved)
                {
                    RestorationService.RecordAttemptResult(deviceId, lessonId, finalScoreScale100, attemptId);
                    Debug.Log($"[WrittenAnswerController] ĐẠT KHÔI PHỤC THIẾT BỊ {deviceId}! Điểm toàn bài: {finalScoreScale10:F1}/10 ({finalScoreScale100:F0}/100)");
                }
                else
                {
                    Debug.Log($"[WrittenAnswerController] Điểm >= 8.0/10 nhưng có câu Rubric DRAFT -> Chấm thử, chưa cấp khôi phục.");
                }
            }
            else
            {
                Debug.Log($"[WrittenAnswerController] CHƯA ĐẠT! Điểm toàn bài: {finalScoreScale10:F1}/10 ({finalScoreScale100:F0}/100) < 8.0/10 -> Không khôi phục thiết bị.");
            }

            // Chuyển sang màn kết quả Tự luận: Scene 19.1. Result_TL (RouteId.EssayResult)
            NavigationService.Instance.NavigateTo(RouteId.EssayResult);
        }

        private void OnBackClicked()
        {
            Debug.Log("[WrittenAnswerController] Bấm Quay lại.");
            bool hasContent = _userAnswers.Values.Any(a => !string.IsNullOrWhiteSpace(a));

            if (hasContent)
            {
                ShowDiscardConfirmModal();
            }
            else
            {
                NavigationService.Instance.NavigateTo(RouteId.ModeSelection);
            }
        }

        private void ShowDiscardConfirmModal()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject modalGo = new GameObject("DiscardConfirmModal");
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
            cRt.sizeDelta = new Vector2(520, 240);
            cRt.anchoredPosition = Vector2.zero;

            Image cImg = cardGo.AddComponent<Image>();
            cImg.color = new Color(0.08f, 0.14f, 0.28f, 0.98f);

            Outline outline = cardGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            outline.effectDistance = new Vector2(2, -2);

            GameObject msgGo = new GameObject("Message");
            msgGo.transform.SetParent(cardGo.transform, false);
            RectTransform mRt = msgGo.AddComponent<RectTransform>();
            mRt.anchoredPosition = new Vector2(0, 35);
            mRt.sizeDelta = new Vector2(460, 80);
            var msgTmp = msgGo.AddComponent<TextMeshProUGUI>();
            msgTmp.text = "Bạn có bài làm chưa nộp.\nNếu quay lại bây giờ, bài làm sẽ bị hủy.";
            msgTmp.fontSize = 19;
            msgTmp.alignment = TextAlignmentOptions.Center;
            msgTmp.color = Color.white;

            // Nút Rời đi
            GameObject btnLeaveGo = new GameObject("Btn_Leave");
            btnLeaveGo.transform.SetParent(cardGo.transform, false);
            RectTransform lRt = btnLeaveGo.AddComponent<RectTransform>();
            lRt.anchoredPosition = new Vector2(-110, -55);
            lRt.sizeDelta = new Vector2(180, 44);
            Image lImg = btnLeaveGo.AddComponent<Image>();
            lImg.color = new Color(0.8f, 0.35f, 0.2f, 1f);
            Button lBtn = btnLeaveGo.AddComponent<Button>();
            lBtn.targetGraphic = lImg;
            GameObject lTxtGo = new GameObject("Text");
            lTxtGo.transform.SetParent(btnLeaveGo.transform, false);
            var lTmp = lTxtGo.AddComponent<TextMeshProUGUI>();
            lTmp.text = "RỜI ĐI";
            lTmp.fontSize = 16;
            lTmp.fontStyle = FontStyles.Bold;
            lTmp.alignment = TextAlignmentOptions.Center;
            lTmp.color = Color.white;
            lTmp.raycastTarget = false;
            lBtn.onClick.AddListener(() =>
            {
                Destroy(modalGo);
                NavigationService.Instance.NavigateTo(RouteId.ModeSelection);
            });

            // Nút Ở lại làm tiếp
            GameObject btnStayGo = new GameObject("Btn_Stay");
            btnStayGo.transform.SetParent(cardGo.transform, false);
            RectTransform sRt = btnStayGo.AddComponent<RectTransform>();
            sRt.anchoredPosition = new Vector2(110, -55);
            sRt.sizeDelta = new Vector2(180, 44);
            Image sImg = btnStayGo.AddComponent<Image>();
            sImg.color = new Color(0.2f, 0.6f, 0.3f, 1f);
            Button sBtn = btnStayGo.AddComponent<Button>();
            sBtn.targetGraphic = sImg;
            GameObject sTxtGo = new GameObject("Text");
            sTxtGo.transform.SetParent(btnStayGo.transform, false);
            var sTmp = sTxtGo.AddComponent<TextMeshProUGUI>();
            sTmp.text = "Ở LẠI LÀM TIẾP";
            sTmp.fontSize = 16;
            sTmp.fontStyle = FontStyles.Bold;
            sTmp.alignment = TextAlignmentOptions.Center;
            sTmp.color = Color.white;
            sTmp.raycastTarget = false;
            sBtn.onClick.AddListener(() => Destroy(modalGo));
        }
    }

    /// <summary>
    /// TMP_InputField tùy biến cho màn hình OCR Aurelia.
    /// Khắc phục hoàn toàn lỗi OnScroll mặc định của TMP_InputField (vốn bị lỗi clamping âm
    /// trong GetScrollPositionRelativeToViewport khiến con lăn chuột bị reset về 0).
    /// </summary>
    public class AureliaOcrInputField : TMP_InputField
    {
        public System.Action<PointerEventData> OnUserScroll;

        public override void OnScroll(PointerEventData eventData)
        {
            if (OnUserScroll != null)
            {
                OnUserScroll(eventData);
            }
        }
    }

    /// <summary>
    /// Bộ chuyển tiếp sự kiện cuộn chuột (Scroll Forwarder) cho Text Area, Card hoặc Scrollbar.
    /// </summary>
    public class OcrScrollForwarder : MonoBehaviour, IScrollHandler
    {
        public System.Action<PointerEventData> OnScrollEvent;

        public void OnScroll(PointerEventData eventData)
        {
            if (OnScrollEvent != null)
            {
                OnScrollEvent(eventData);
            }
        }
    }

    /// <summary>
    /// Quản lý thanh cuộn dọc (Scrollbar), con lăn chuột (Mouse Wheel), kéo thanh cuộn (Drag),
    /// và phím mũi tên / gõ phím cho modal OCR bài làm viết tay Aurelia.
    /// </summary>
    public class OcrTranscriptScrollManager : MonoBehaviour
    {
        public TMP_InputField inputField;
        public TextMeshProUGUI textComponent;
        public RectTransform viewportRt;
        public Scrollbar scrollbar;

        private float _maxScrollY = 0f;
        private float _lastAnchoredY = 0f;
        private string _lastText = "";
        private bool _isUpdatingInternally = false;

        public void Setup(TMP_InputField input, TextMeshProUGUI text, RectTransform viewport, Scrollbar sb)
        {
            inputField = input;
            textComponent = text;
            viewportRt = viewport;
            scrollbar = sb;

            if (scrollbar != null)
            {
                scrollbar.onValueChanged.RemoveAllListeners();
                scrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);
            }
        }

        public void HandleScrollDelta(PointerEventData eventData)
        {
            if (eventData == null || _maxScrollY <= 0.01f || textComponent == null) return;

            float delta = -eventData.scrollDelta.y * 38f;
            float currentY = textComponent.rectTransform.anchoredPosition.y;
            float newY = Mathf.Clamp(currentY + delta, 0f, _maxScrollY);

            ApplyScrollY(newY);
        }

        private void OnScrollbarValueChanged(float val)
        {
            if (_isUpdatingInternally || _maxScrollY <= 0.01f || textComponent == null) return;

            float targetY = Mathf.Clamp(val * _maxScrollY, 0f, _maxScrollY);
            _isUpdatingInternally = true;
            textComponent.rectTransform.anchoredPosition = new Vector2(textComponent.rectTransform.anchoredPosition.x, targetY);
            _lastAnchoredY = targetY;
            _isUpdatingInternally = false;
        }

        public void ApplyScrollY(float targetY)
        {
            if (textComponent == null) return;
            targetY = Mathf.Clamp(targetY, 0f, _maxScrollY);

            _isUpdatingInternally = true;
            textComponent.rectTransform.anchoredPosition = new Vector2(textComponent.rectTransform.anchoredPosition.x, targetY);
            _lastAnchoredY = targetY;

            if (scrollbar != null)
            {
                float scrollVal = _maxScrollY > 0.01f ? (targetY / _maxScrollY) : 0f;
                scrollbar.SetValueWithoutNotify(Mathf.Clamp01(scrollVal));
            }
            _isUpdatingInternally = false;
        }

        public void RefreshLayout(bool resetToTop = false)
        {
            if (textComponent == null || viewportRt == null) return;

            Canvas.ForceUpdateCanvases();
            textComponent.ForceMeshUpdate(true, true);

            float contentHeight = textComponent.preferredHeight;
            float viewHeight = viewportRt.rect.height;

            if (contentHeight > viewHeight + 2f)
            {
                _maxScrollY = contentHeight - viewHeight + 45f; // 45px đệm dưới giúp đọc trọn vẹn dòng cuối cùng
                if (scrollbar != null)
                {
                    scrollbar.gameObject.SetActive(true);
                    scrollbar.interactable = true;
                    float handleRatio = Mathf.Clamp(viewHeight / (contentHeight + 45f), 0.12f, 0.85f);
                    scrollbar.size = handleRatio;
                }
            }
            else
            {
                _maxScrollY = 0f;
                if (scrollbar != null)
                {
                    scrollbar.size = 1f;
                    scrollbar.interactable = false;
                }
            }

            if (resetToTop || _maxScrollY <= 0.01f)
            {
                ApplyScrollY(0f);
            }
            else
            {
                float curY = textComponent.rectTransform.anchoredPosition.y;
                ApplyScrollY(curY);
            }

            _lastText = textComponent.text ?? "";
        }

        private void LateUpdate()
        {
            if (textComponent == null || viewportRt == null) return;

            // Kiểm tra nội dung chữ có thay đổi do học sinh gõ hoặc xóa không
            string curText = textComponent.text ?? "";
            if (curText != _lastText)
            {
                _lastText = curText;
                RefreshLayout(false);
            }

            // Đồng bộ thanh cuộn nếu vị trí văn bản bị dịch chuyển do phím mũi tên hoặc con trỏ TMP
            float curAnchoredY = textComponent.rectTransform.anchoredPosition.y;
            if (!_isUpdatingInternally && Mathf.Abs(curAnchoredY - _lastAnchoredY) > 0.5f)
            {
                _lastAnchoredY = curAnchoredY;
                if (scrollbar != null && _maxScrollY > 0.01f)
                {
                    float scrollVal = Mathf.Clamp01(curAnchoredY / _maxScrollY);
                    scrollbar.SetValueWithoutNotify(scrollVal);
                }
            }
        }
    }
}
