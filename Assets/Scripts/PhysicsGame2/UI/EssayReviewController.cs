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
    /// Điều khiển logic giao diện Scene 20.1 Detail Result TL (Xem chi tiết lời giải tự luận - Game 2 Aurelia).
    /// Phương pháp: INTEGRATE TRỰC TIẾP VÀO CÁC GAMEOBJECT CÓ SẴN TRONG SCENE.
    /// Giữ nguyên 100% bố cục và art gốc do người dùng thiết kế:
    /// - Không tạo thêm text chồng lên tiêu đề các khung (ĐỀ BÀI, NHẬN XÉT, GỢI Ý CẢI THIỆN, CÁC BƯỚC GIẢI).
    /// - Loại bỏ hoàn toàn các tab runtime, text thừa hay icon lệch ở Câu 1 sidebar.
    /// - 4 vùng nội dung dài (Đề bài, Nhận xét, Gợi ý cải thiện, Các bước giải) đều có ScrollRect + Viewport + RectMask2D nội bộ.
    /// - Header của mỗi khung cố định (nằm trên Sprite), chỉ vùng nội dung cuộn được bằng con lăn chuột / kéo chuột.
    /// </summary>
    public class EssayReviewController : MonoBehaviour
    {
        [Header("=== SIDEBAR CÂU HỎI (CÂU 1 - 5) ===")]
        [SerializeField] private List<Button> sidebarQuestionButtons = new List<Button>();
        [SerializeField] private List<TextMeshProUGUI> sidebarQuestionLabels = new List<TextMeshProUGUI>();
        [SerializeField] private List<Image> sidebarQuestionStatusIcons = new List<Image>();

        [Header("=== 5 KHUNG NỘI DUNG CHÍNH ===")]
        // 1. Khung Đề bài
        [SerializeField] private Image boxQuestionPromptImg;
        [SerializeField] private TextMeshProUGUI textQuestionPrompt;
        [SerializeField] private ScrollRect scrollQuestionPrompt;

        // 2. Khung Điểm số
        [SerializeField] private Image boxScoreImg;
        [SerializeField] private TextMeshProUGUI textScore;

        // 3. Khung Nhận xét
        [SerializeField] private Image boxAiFeedbackImg;
        [SerializeField] private TextMeshProUGUI textAiFeedback;
        [SerializeField] private ScrollRect scrollAiFeedback;

        // 4. Khung Gợi ý cải thiện (Khung bên trái)
        [SerializeField] private Image boxSuggestionsImg;
        [SerializeField] private TextMeshProUGUI textSuggestions;
        [SerializeField] private ScrollRect scrollSuggestions;

        // 5. Khung Các bước giải / Đáp án mẫu (Khung bên phải)
        [SerializeField] private Image boxSolutionImg;
        [SerializeField] private TextMeshProUGUI textSolution;
        [SerializeField] private ScrollRect scrollSolution;

        [Header("=== ĐIỀU HƯỚNG DƯỚI CÙNG ===")]
        [SerializeField] private Button btnBack;
        [SerializeField] private Button btnPrev;
        [SerializeField] private Button btnNext;

        [Header("=== ASSETS STATUS ICONS ===")]
        private Sprite _iconCorrectSprite;   // Atlas sprite 46 (Dấu tích xanh ✓)
        private Sprite _iconIncorrectSprite; // Atlas sprite 47 (Dấu X đỏ ✗)
        private TMP_FontAsset _defaultFont;
        private Canvas _canvas;

        private List<EssayAnswerRecord> _essayAnswers = new List<EssayAnswerRecord>();
        private int _currentQuestionIndex = 0;

        private void Awake()
        {
            EnsureEventSystem();
            _canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (_canvas != null)
            {
                BindSceneReferences(_canvas);
            }
            NavigationService.Instance.EnsureDevContext();
        }

        private void Start()
        {
            LoadAttemptData();
            SelectQuestion(0);
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
                OnBackToResultClicked();
            }
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
        /// Liên kết trực tiếp vào các GameObject UI có sẵn trong Scene 20.1 Detail Result TL.
        /// Bảo toàn 100% layout, sprite, vị trí và kích thước do người dùng thiết kế.
        /// </summary>
        private void BindSceneReferences(Canvas canvas)
        {
            if (canvas == null) return;

            // 1. Dọn dẹp overlay cũ nếu có
            Transform oldRoot = canvas.transform.Find("Aurelia_EssayReviewRoot");
            if (oldRoot != null) DestroyImmediate(oldRoot.gameObject);

            // Gỡ bỏ QuizReviewController trên Canvas nếu có để tránh xung đột
            var oldQrs = canvas.GetComponents<QuizReviewController>();
            foreach (var qr in oldQrs) DestroyImmediate(qr);

            // Đảm bảo toàn bộ đối tượng gốc của Scene đều active, ngoại trừ icon/element thừa ở sidebar
            foreach (Transform child in canvas.transform)
            {
                if (child.name != "Aurelia_EssayReviewRoot")
                {
                    RectTransform crt = child as RectTransform;
                    if (crt != null && child.GetComponent<Image>() != null && child.GetComponent<Button>() == null)
                    {
                        Vector2 p = crt.anchoredPosition;
                        if (p.x > -650f && p.x < -540f && p.y > -220f && p.y < 250f)
                        {
                            child.gameObject.SetActive(false);
                            continue;
                        }
                    }
                    child.gameObject.SetActive(true);
                }
            }

            var allImages = canvas.GetComponentsInChildren<Image>(true).ToList();
            var allTexts = canvas.GetComponentsInChildren<TextMeshProUGUI>(true).ToList();

            // 2. Dọn dẹp triệt để bất kỳ text / tab tự sinh thừa nào từ lần chạy trước đè lên khung
            foreach (var img in allImages)
            {
                Transform oldHdr = img.transform.Find("HeaderLabel");
                if (oldHdr != null) DestroyImmediate(oldHdr.gameObject);

                Transform oldTabs = img.transform.Find("SolutionModeTabs");
                if (oldTabs != null) DestroyImmediate(oldTabs.gameObject);
            }

            if (allTexts.Count > 0 && allTexts[0].font != null)
            {
                _defaultFont = allTexts[0].font;
            }

            // Đảm bảo các Image nền lớn không chặn tương tác UI
            foreach (var img in allImages)
            {
                if (img.rectTransform.sizeDelta.x > 700f && img.rectTransform.sizeDelta.y > 600f)
                {
                    img.raycastTarget = false;
                }
                if (img.rectTransform.anchoredPosition == Vector2.zero && img.rectTransform.sizeDelta == Vector2.zero)
                {
                    img.raycastTarget = false;
                }
            }

            // 3. Tìm 3 Nút điều hướng dưới cùng
            foreach (var img in allImages)
            {
                Vector2 pos = img.rectTransform.anchoredPosition;
                if (pos.y < -420f)
                {
                    if (pos.x < -400f && btnBack == null)
                    {
                        img.gameObject.name = "Btn_BackToResult";
                        btnBack = img.GetComponent<Button>() ?? img.gameObject.AddComponent<Button>();
                        btnBack.targetGraphic = img;
                        btnBack.onClick.RemoveAllListeners();
                        btnBack.onClick.AddListener(OnBackToResultClicked);
                        UIHoverPulseEffect.Attach(img.gameObject, hover: 1.05f, outline: true);
                    }
                    else if (pos.x > -150f && pos.x < 150f && btnPrev == null)
                    {
                        img.gameObject.name = "Btn_PrevQuestion";
                        btnPrev = img.GetComponent<Button>() ?? img.gameObject.AddComponent<Button>();
                        btnPrev.targetGraphic = img;
                        btnPrev.onClick.RemoveAllListeners();
                        btnPrev.onClick.AddListener(OnPrevQuestionClicked);
                        UIHoverPulseEffect.Attach(img.gameObject, hover: 1.05f, outline: true);
                    }
                    else if (pos.x > 400f && btnNext == null)
                    {
                        img.gameObject.name = "Btn_NextQuestion";
                        btnNext = img.GetComponent<Button>() ?? img.gameObject.AddComponent<Button>();
                        btnNext.targetGraphic = img;
                        btnNext.onClick.RemoveAllListeners();
                        btnNext.onClick.AddListener(OnNextQuestionClicked);
                        UIHoverPulseEffect.Attach(img.gameObject, hover: 1.05f, outline: true);
                    }
                }
            }

            // 4. Sidebar Câu hỏi (5 Tabs, 5 Labels, 5 Status Icons)
            List<Image> sidebarTabs = new List<Image>();
            foreach (var img in allImages)
            {
                Vector2 pos = img.rectTransform.anchoredPosition;
                Vector2 size = img.rectTransform.sizeDelta;
                if (pos.x < -680f && pos.y > -220f && pos.y < 220f && size.x > 280f && size.y > 90f && size.y < 200f)
                {
                    sidebarTabs.Add(img);
                }
            }
            sidebarTabs = sidebarTabs.OrderByDescending(t => t.rectTransform.anchoredPosition.y).Take(5).ToList();

            List<TextMeshProUGUI> sidebarLabels = new List<TextMeshProUGUI>();
            foreach (var t in allTexts)
            {
                Vector2 pos = t.rectTransform.anchoredPosition;
                if (pos.x < -650f && pos.y > -220f && pos.y < 220f)
                {
                    sidebarLabels.Add(t);
                }
            }
            sidebarLabels = sidebarLabels.OrderByDescending(t => t.rectTransform.anchoredPosition.y).Take(5).ToList();

            List<Image> statusIcons = new List<Image>();
            foreach (var img in allImages)
            {
                bool isStatusSprite = (img.sprite != null && (img.sprite.name.EndsWith("_46") || img.sprite.name.EndsWith("_47") || img.sprite.name.Contains("46") || img.sprite.name.Contains("47")));
                Vector2 pos = img.rectTransform.anchoredPosition;
                Vector2 size = img.rectTransform.sizeDelta;
                bool isCanvasPos = (pos.x > -650f && pos.x < -540f && pos.y > -220f && pos.y < 250f && size.x < 80f && size.y < 80f);
                if ((isStatusSprite || isCanvasPos) && size.x < 80f && size.y < 80f)
                {
                    if (!statusIcons.Contains(img))
                    {
                        statusIcons.Add(img);
                    }
                }
            }
            statusIcons = statusIcons.OrderByDescending(t => t.transform.position.y).Take(5).ToList();

            // Phân biệt chính xác Sprite Đúng (Check xanh 46) và Sai (X đỏ 47)
            foreach (var img in allImages)
            {
                if (img.sprite != null)
                {
                    string sName = img.sprite.name;
                    if (sName.EndsWith("_46") || sName.Contains("46"))
                    {
                        _iconCorrectSprite = img.sprite;
                    }
                    else if (sName.EndsWith("_47") || sName.Contains("47"))
                    {
                        _iconIncorrectSprite = img.sprite;
                    }
                }
            }
            if (_iconCorrectSprite == null && statusIcons.Count > 1) _iconCorrectSprite = statusIcons[1].sprite;
            if (_iconIncorrectSprite == null && statusIcons.Count > 0) _iconIncorrectSprite = statusIcons[0].sprite;

            sidebarQuestionButtons.Clear();
            sidebarQuestionLabels.Clear();
            sidebarQuestionStatusIcons.Clear();

            for (int i = 0; i < sidebarTabs.Count; i++)
            {
                var sImg = sidebarTabs[i];
                sImg.gameObject.name = $"Sidebar_QuestionTab_{i + 1}";
                var sBtn = sImg.GetComponent<Button>() ?? sImg.gameObject.AddComponent<Button>();
                sBtn.targetGraphic = sImg;
                sImg.raycastTarget = true;
                int qIdx = i;
                sBtn.onClick.RemoveAllListeners();
                sBtn.onClick.AddListener(() => SelectQuestion(qIdx));
                UIHoverPulseEffect.Attach(sImg.gameObject, hover: 1.03f, outline: false);

                TextMeshProUGUI sLbl = (i < sidebarLabels.Count) ? sidebarLabels[i] : sImg.GetComponentInChildren<TextMeshProUGUI>(true);
                if (sLbl != null) sLbl.raycastTarget = false;

                // Đảm bảo nút Câu 1-5 không chứa bất kỳ Image con thừa nào (giữ nguyên cấu trúc sạch như các nút Câu 2-5)
                for (int c = sBtn.transform.childCount - 1; c >= 0; c--)
                {
                    Transform child = sBtn.transform.GetChild(c);
                    if (child.GetComponent<TextMeshProUGUI>() == null)
                    {
                        child.gameObject.SetActive(false);
                        if (canvas != null) child.SetParent(canvas.transform, false);
                    }
                }

                Image sIco = (i < statusIcons.Count) ? statusIcons[i] : null;
                if (sIco != null)
                {
                    sIco.raycastTarget = false;
                    // Tuyệt đối không reparent vào nút Câu 1 để tránh sinh hình lạ/mũi tên đè mép phải
                    if (sIco.transform.parent == sBtn.transform)
                    {
                        sIco.transform.SetParent(canvas.transform, false);
                    }
                    sIco.gameObject.SetActive(false);
                }

                sidebarQuestionButtons.Add(sBtn);
                sidebarQuestionLabels.Add(sLbl);
                sidebarQuestionStatusIcons.Add(sIco);
            }

            // 5. KHUNG 1: ĐỀ BÀI (Box_QuestionPrompt - Image 882630266)
            foreach (var img in allImages)
            {
                Vector2 pos = img.rectTransform.anchoredPosition;
                Vector2 size = img.rectTransform.sizeDelta;
                if (pos.y > 140f && pos.y < 200f && size.x > 1000f)
                {
                    boxQuestionPromptImg = img;
                    img.gameObject.name = "Box_QuestionPrompt";
                    img.raycastTarget = false;
                    break;
                }
            }
            foreach (var t in allTexts)
            {
                Vector2 pos = t.rectTransform.anchoredPosition;
                Vector2 size = t.rectTransform.sizeDelta;
                if (pos.y > 130f && pos.y < 190f && size.x > 800f)
                {
                    textQuestionPrompt = t;
                    t.gameObject.name = "Text_QuestionPrompt";
                    break;
                }
            }
            if (boxQuestionPromptImg != null && textQuestionPrompt != null)
            {
                // Safe viewport: chừa 48px phía trên (hoàn toàn không đè lên badge ✦ ĐỀ BÀI ✦), 14px viền dưới, 28px hai bên
                scrollQuestionPrompt = SetupSafeScrollArea(
                    boxQuestionPromptImg.transform,
                    textQuestionPrompt,
                    new Vector2(54f, 14f),
                    new Vector2(-54f, -48f),
                    fontSize: 21f
                );
            }

            // 6. KHUNG 2: ĐIỂM AI CHẤM (Badge_Score - Image 158900586)
            foreach (var img in allImages)
            {
                Vector2 pos = img.rectTransform.anchoredPosition;
                if (pos.x < -180f && pos.x > -350f && pos.y > -10f && pos.y < 60f)
                {
                    boxScoreImg = img;
                    img.gameObject.name = "Badge_Score";
                    img.raycastTarget = false;
                    break;
                }
            }
            foreach (var t in allTexts)
            {
                Vector2 pos = t.rectTransform.anchoredPosition;
                if (pos.x < -150f && pos.x > -300f && pos.y > -40f && pos.y < 20f)
                {
                    textScore = t;
                    t.gameObject.name = "Text_Score";
                    t.raycastTarget = false;
                    t.alignment = TextAlignmentOptions.Center;
                    t.fontSize = 34f;
                    t.color = new Color(0.96f, 0.82f, 0.38f, 1f); // Vàng kim Aurelia
                    t.rectTransform.sizeDelta = new Vector2(220f, 55f);
                    t.rectTransform.anchoredPosition = new Vector2(-263f, -10f); // Căn giữa tuyệt đối trong khung điểm
                    break;
                }
            }

            // 7. KHUNG 3: NHẬN XÉT (Box_AiFeedback - Image 1863997599)
            foreach (var img in allImages)
            {
                Vector2 pos = img.rectTransform.anchoredPosition;
                Vector2 size = img.rectTransform.sizeDelta;
                if (pos.x > 200f && pos.x < 450f && pos.y > -10f && pos.y < 60f && size.x > 600f)
                {
                    boxAiFeedbackImg = img;
                    img.gameObject.name = "Box_AiFeedback";
                    img.raycastTarget = false;
                    break;
                }
            }
            foreach (var t in allTexts)
            {
                Vector2 pos = t.rectTransform.anchoredPosition;
                Vector2 size = t.rectTransform.sizeDelta;
                if (pos.x > 200f && pos.x < 450f && pos.y > -30f && pos.y < 30f && size.x > 600f)
                {
                    textAiFeedback = t;
                    t.gameObject.name = "Text_AiFeedback";
                    break;
                }
            }
            if (boxAiFeedbackImg != null && textAiFeedback != null)
            {
                // Safe viewport: chừa 48px phía trên (hoàn toàn không đè lên badge ✦ NHẬN XÉT ✦), 14px viền dưới, 26px hai bên
                scrollAiFeedback = SetupSafeScrollArea(
                    boxAiFeedbackImg.transform,
                    textAiFeedback,
                    new Vector2(26f, 14f),
                    new Vector2(-26f, -48f),
                    fontSize: 20f
                );
            }

            // 8. KHUNG 4: GỢI Ý CẢI THIỆN (Box_Suggestions - Image 744005601 - Khung bên trái)
            foreach (var img in allImages)
            {
                Vector2 pos = img.rectTransform.anchoredPosition;
                Vector2 size = img.rectTransform.sizeDelta;
                if (pos.x < 0f && pos.x > -250f && pos.y < -150f && pos.y > -300f && size.x > 450f)
                {
                    boxSuggestionsImg = img;
                    img.gameObject.name = "Box_Suggestions";
                    img.raycastTarget = false;
                    break;
                }
            }
            foreach (var t in allTexts)
            {
                Vector2 pos = t.rectTransform.anchoredPosition;
                Vector2 size = t.rectTransform.sizeDelta;
                if (pos.x < 0f && pos.x > -250f && pos.y < -150f && pos.y > -300f && size.x > 400f)
                {
                    textSuggestions = t;
                    t.gameObject.name = "Text_Suggestions";
                    break;
                }
            }
            if (boxSuggestionsImg != null && textSuggestions != null)
            {
                // Viewport an toàn: chừa 90px phía trên (hoàn toàn bên dưới thanh tiêu đề fantasy ✦ GỢI Ý CẢI THIỆN ✦), 20px viền dưới, 24px hai bên
                scrollSuggestions = SetupSafeScrollArea(
                    boxSuggestionsImg.transform,
                    textSuggestions,
                    new Vector2(24f, 20f),
                    new Vector2(-24f, -90f),
                    fontSize: 20f,
                    createScrollbar: true
                );
            }

            // 9. KHUNG 5: CÁC BƯỚC GIẢI & ĐÁP ÁN MẪU (Box_Solution - Image 96224552 - Khung bên phải)
            foreach (var img in allImages)
            {
                Vector2 pos = img.rectTransform.anchoredPosition;
                Vector2 size = img.rectTransform.sizeDelta;
                if (pos.x > 300f && pos.x < 550f && pos.y < -150f && pos.y > -300f && size.x > 500f)
                {
                    boxSolutionImg = img;
                    img.gameObject.name = "Box_Solution";
                    img.raycastTarget = false;
                    break;
                }
            }
            foreach (var t in allTexts)
            {
                Vector2 pos = t.rectTransform.anchoredPosition;
                Vector2 size = t.rectTransform.sizeDelta;
                if (pos.x > 300f && pos.x < 550f && pos.y < -150f && pos.y > -300f && size.x > 450f)
                {
                    textSolution = t;
                    t.gameObject.name = "Text_Solution";
                    break;
                }
            }
            if (boxSolutionImg != null && textSolution != null)
            {
                // Viewport an toàn: chừa 90px phía trên (hoàn toàn bên dưới thanh tiêu đề fantasy ✦ CÁC BƯỚC GIẢI ✦), 20px viền dưới, 28px hai bên
                scrollSolution = SetupSafeScrollArea(
                    boxSolutionImg.transform,
                    textSolution,
                    new Vector2(28f, 20f),
                    new Vector2(-28f, -90f),
                    fontSize: 20f,
                    createScrollbar: true
                );
            }

            Debug.Log("[EssayReviewController] BindSceneReferences hoàn tất kết nối an toàn 100%.");
        }

        /// <summary>
        /// Tạo vùng cuộn nội bộ với Viewport riêng chỉ bao phần thân khung (loại trừ hoàn toàn phần header/trang trí).
        /// RectMask2D gắn trên Viewport cắt gọt 100% nội dung chữ tràn lên tiêu đề hoặc ra ngoài viền khung.
        /// Thêm Image trong suốt raycastTarget = true để con lăn chuột và vuốt kéo hoạt động mượt mà.
        /// </summary>
        private ScrollRect SetupSafeScrollArea(Transform parentBox, TextMeshProUGUI existingText, Vector2 offsetMin, Vector2 offsetMax, float fontSize = 20f, bool createScrollbar = false)
        {
            if (parentBox == null || existingText == null) return null;

            // 1. Dọn dẹp an toàn các container cũ nếu có (bảo toàn existingText trước khi destroy)
            string[] oldNames = { "ScrollArea", "ScrollContainer", "ScrollView", "Viewport", "ScrollbarVertical" };
            foreach (var oName in oldNames)
            {
                Transform oldTr = parentBox.Find(oName);
                if (oldTr != null)
                {
                    if (existingText.transform.IsChildOf(oldTr))
                    {
                        existingText.transform.SetParent(parentBox, false);
                    }
                    DestroyImmediate(oldTr.gameObject);
                }
            }

            // 2. Gắn ScrollRect lên parentBox (Image nền của khung)
            ScrollRect sr = parentBox.GetComponent<ScrollRect>() ?? parentBox.gameObject.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.scrollSensitivity = 45f;
            sr.movementType = ScrollRect.MovementType.Clamped;

            var boxImg = parentBox.GetComponent<Image>();
            if (boxImg != null)
            {
                boxImg.raycastTarget = true;
            }

            // 3. Tạo Viewport riêng CHỈ BAO PHẦN THÂN KHUNG (nằm hoàn toàn bên dưới thanh tiêu đề)
            GameObject viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(parentBox, false);
            RectTransform vpRt = viewportGo.AddComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = offsetMin;
            vpRt.offsetMax = offsetMax;

            var vpImg = viewportGo.AddComponent<Image>();
            vpImg.color = Color.clear;
            vpImg.raycastTarget = true;

            // RectMask2D cắt gọt tuyệt đối: text vượt ra ngoài thân khung (lên header hay xuống viền) bị clip 100%
            var mask = viewportGo.AddComponent<RectMask2D>();
            mask.softness = new Vector2Int(0, 4);

            sr.viewport = vpRt;

            // 4. Tạo Content nằm bên trong Viewport (bắt đầu thấp hơn thanh tiêu đề)
            GameObject contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            RectTransform cRt = contentGo.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 1f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(0f, 100f);

            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(6, 6, 8, 8);
            vlg.spacing = 6f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = cRt;

            // 5. Đưa TextMeshPro có sẵn vào trong Content
            existingText.transform.SetParent(contentGo.transform, false);
            RectTransform tRt = existingText.rectTransform;
            tRt.anchorMin = new Vector2(0f, 1f);
            tRt.anchorMax = new Vector2(1f, 1f);
            tRt.pivot = new Vector2(0.5f, 1f);
            tRt.anchoredPosition = Vector2.zero;
            tRt.sizeDelta = new Vector2(0f, 50f);

            existingText.fontSize = fontSize;
            existingText.textWrappingMode = TextWrappingModes.Normal;
            existingText.overflowMode = TextOverflowModes.Overflow;
            existingText.alignment = TextAlignmentOptions.TopLeft;
            existingText.maskable = true;
            existingText.raycastTarget = false;

            // 6. Thanh cuộn kéo mảnh Auto-Hide tiện dụng nếu nội dung dài
            if (createScrollbar)
            {
                var sb = CreateSubtleScrollbar(parentBox, offsetMin, offsetMax);
                sr.verticalScrollbar = sb;
                sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            }

            return sr;
        }

        /// <summary>
        /// Tạo thanh cuộn thanh mảnh màu vàng kim Aurelia bám theo thân khung.
        /// Tự động ẩn khi nội dung ngắn, tự động hiện khi nội dung dài để kéo vuốt.
        /// </summary>
        private Scrollbar CreateSubtleScrollbar(Transform parentBox, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject sbGo = new GameObject("ScrollbarVertical");
            sbGo.transform.SetParent(parentBox, false);
            RectTransform sbRt = sbGo.AddComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(1f, 0f);
            sbRt.anchorMax = new Vector2(1f, 1f);
            sbRt.pivot = new Vector2(1f, 0.5f);
            sbRt.offsetMin = new Vector2(-16f, offsetMin.y + 4f);
            sbRt.offsetMax = new Vector2(-8f, offsetMax.y - 4f);

            var trackImg = sbGo.AddComponent<Image>();
            trackImg.color = new Color(0f, 0f, 0f, 0.20f);
            trackImg.raycastTarget = true;

            GameObject slideArea = new GameObject("SlidingArea");
            slideArea.transform.SetParent(sbGo.transform, false);
            RectTransform saRt = slideArea.AddComponent<RectTransform>();
            saRt.anchorMin = Vector2.zero;
            saRt.anchorMax = Vector2.one;
            saRt.offsetMin = Vector2.zero;
            saRt.offsetMax = Vector2.zero;

            GameObject handleGo = new GameObject("Handle");
            handleGo.transform.SetParent(slideArea.transform, false);
            RectTransform hRt = handleGo.AddComponent<RectTransform>();
            hRt.anchorMin = Vector2.zero;
            hRt.anchorMax = Vector2.one;
            hRt.offsetMin = Vector2.zero;
            hRt.offsetMax = Vector2.zero;

            var handleImg = handleGo.AddComponent<Image>();
            handleImg.color = new Color(0.96f, 0.82f, 0.38f, 0.65f); // Vàng kim Aurelia
            handleImg.raycastTarget = true;

            var sb = sbGo.AddComponent<Scrollbar>();
            sb.direction = Scrollbar.Direction.BottomToTop;
            sb.targetGraphic = handleImg;
            sb.handleRect = hRt;

            return sb;
        }

        /// <summary>
        /// Nạp dữ liệu thật của bài làm từ NavigationService.
        /// </summary>
        private void LoadAttemptData()
        {
            _essayAnswers = NavigationService.Instance.AttemptContext.EssayAnswers;

            // Direct Play Fallback: Nếu mở trực tiếp Scene 20.1 để test mà chưa qua lượt thi
            if (_essayAnswers == null || _essayAnswers.Count == 0)
            {
                GameDataRepository.EnsureInitialized();
                string lessonId = NavigationService.Instance.Selection.LessonId;
                if (string.IsNullOrEmpty(lessonId)) lessonId = "bai2";
                string diff = NavigationService.Instance.Selection.Difficulty;
                if (string.IsNullOrEmpty(diff)) diff = "NB";

                var questions = GameDataRepository.GetEssayQuestions(lessonId, diff, 5);
                _essayAnswers = new List<EssayAnswerRecord>();

                for (int i = 0; i < 5; i++)
                {
                    var q = (questions != null && i < questions.Count) ? questions[i] : null;
                    string cauHoiId = (q != null && !string.IsNullOrEmpty(q.cauHoiID)) ? q.cauHoiID : $"TL_C1_L2_{i + 1}";
                    string prompt = (q != null && !string.IsNullOrEmpty(q.noiDung)) ? q.noiDung : $"Câu hỏi tự luận bài {lessonId} câu số {i + 1}";
                    string sampleAns = (q != null && !string.IsNullOrEmpty(q.dapAnMau)) ? q.dapAnMau : "1. Xác định môi trường truyền sáng.\n2. So sánh góc tới với góc giới hạn.\n3. Kết luận và nêu ứng dụng.";

                    _essayAnswers.Add(new EssayAnswerRecord
                    {
                        CauHoiID = cauHoiId,
                        QuestionText = prompt,
                        ConfirmedText = "(Chưa làm)",
                        SampleAnswer = sampleAns,
                        Explanation = "Học sinh chưa làm câu hỏi này.",
                        Score = 0f,
                        IsGraded = true,
                        IsDraftRubric = false,
                        Steps = new List<GradingStepFeedback>()
                    });
                }
            }

            if (_currentQuestionIndex >= _essayAnswers.Count)
            {
                _currentQuestionIndex = 0;
            }
        }

        /// <summary>
        /// Cập nhật nội dung động cho câu hỏi đang chọn (Câu 1 - 5).
        /// </summary>
        public void SelectQuestion(int index)
        {
            if (_essayAnswers == null || _essayAnswers.Count == 0) return;
            _currentQuestionIndex = Mathf.Clamp(index, 0, _essayAnswers.Count - 1);
            var record = _essayAnswers[_currentQuestionIndex];

            // 1. Cập nhật Sidebar: Highlight tab đang chọn & Icon trạng thái bài làm THẬT
            for (int i = 0; i < sidebarQuestionButtons.Count; i++)
            {
                bool isSelected = (i == _currentQuestionIndex);
                var btn = sidebarQuestionButtons[i];
                var img = btn.GetComponent<Image>();
                if (img != null)
                {
                    img.color = isSelected ? new Color(1f, 1f, 1f, 1f) : new Color(0.70f, 0.75f, 0.85f, 0.80f);
                }

                if (i < sidebarQuestionStatusIcons.Count && sidebarQuestionStatusIcons[i] != null)
                {
                    var ico = sidebarQuestionStatusIcons[i];
                    // Đảm bảo không reparent vào nút Câu 1 và ẩn hoàn toàn để bảo toàn họa tiết ngôi sao vàng nguyên bản
                    if (ico.transform.parent == btn.transform)
                    {
                        ico.transform.SetParent(_canvas != null ? _canvas.transform : btn.transform.parent, false);
                    }
                    ico.gameObject.SetActive(false);
                }
            }

            // 2. KHUNG 1: ĐỀ BÀI (Chỉ hiển thị nội dung câu hỏi, không đè lên badge ✦ ĐỀ BÀI ✦)
            if (textQuestionPrompt != null)
            {
                textQuestionPrompt.text = record.QuestionText;
            }

            // 3. KHUNG 2: ĐIỂM AI CHẤM (Căn giữa, không chữ nhỏ hay nhãn phụ chồng lên)
            if (textScore != null)
            {
                bool isAttempted = (!string.IsNullOrEmpty(record.ConfirmedText) && record.ConfirmedText != "(Chưa làm)");
                if (isAttempted)
                {
                    textScore.text = $"<b>{record.Score:0.0} / 10</b>";
                }
                else
                {
                    textScore.text = "<b>0 / 10</b>";
                }
            }

            // 4. KHUNG 3: NHẬN XÉT (Nội dung nằm trọn trong khung, cuộn nếu dài, không đè tiêu đề khung dưới)
            if (textAiFeedback != null)
            {
                bool isAttempted = (!string.IsNullOrEmpty(record.ConfirmedText) && record.ConfirmedText != "(Chưa làm)");
                if (isAttempted)
                {
                    string fb = !string.IsNullOrEmpty(record.Explanation) ? record.Explanation : "Bài làm đã được Gemini AI ghi nhận và chấm điểm theo rubric.";
                    textAiFeedback.text = fb;
                }
                else
                {
                    textAiFeedback.text = "Học sinh chưa làm câu hỏi này nên hệ thống ghi nhận 0 điểm.";
                }
            }

            // 5. KHUNG 4: GỢI Ý CẢI THIỆN (Chỉ hiển thị gợi ý cải thiện từ AI, không có chữ BÀI CỦA BẠN đè lên)
            if (textSuggestions != null)
            {
                bool isAttempted = (!string.IsNullOrEmpty(record.ConfirmedText) && record.ConfirmedText != "(Chưa làm)");
                if (isAttempted)
                {
                    string sugText = "";
                    var failedSteps = (record.Steps != null) ? record.Steps.Where(s => s.Status != "correct").ToList() : null;

                    if (failedSteps != null && failedSteps.Count > 0)
                    {
                        for (int s = 0; s < failedSteps.Count; s++)
                        {
                            var step = failedSteps[s];
                            sugText += $"• <b>Bước {step.StepId}:</b> {step.Explanation}\n";
                            sugText += "  <i>Khuyên:</i> Xem lại công thức và lý thuyết liên quan để hoàn thiện bước này.\n\n";
                        }
                    }
                    else
                    {
                        sugText += "<color=#4ade80><b>★ KẾT QUẢ XUẤT SẮC!</b></color>\n\n";
                        sugText += "Bạn đã thực hiện chính xác và đầy đủ tất cả các bước theo yêu cầu rubric.\n";
                        sugText += "Hãy tiếp tục phát huy ở các bài học tiếp theo!";
                    }

                    textSuggestions.text = sugText;
                }
                else
                {
                    textSuggestions.text = "<i>(Chưa có gợi ý vì bạn chưa làm câu hỏi này)</i>\n\nBạn hãy xem phần <b>CÁC BƯỚC GIẢI</b> bên cạnh để nắm các bước giải đúng của bài này.";
                }
            }

            // 6. KHUNG 5: CÁC BƯỚC GIẢI & ĐÁP ÁN MẪU (Không có chữ hay tab lạ đè lên ✦ CÁC BƯỚC GIẢI ✦)
            if (textSolution != null)
            {
                string solText = "";
                // 6.1. Các bước giải chi tiết theo rubric
                if (record.Steps != null && record.Steps.Count > 0)
                {
                    for (int s = 0; s < record.Steps.Count; s++)
                    {
                        var step = record.Steps[s];
                        solText += $"• <b>Bước {s + 1} ({step.StepId}):</b>\n  {step.Explanation}\n\n";
                    }
                }

                // 6.2. Lời giải / Đáp án mẫu chuẩn
                if (!string.IsNullOrEmpty(record.SampleAnswer))
                {
                    solText += $"<color=#fef08a><b>ĐÁP ÁN THAM KHẢO CHUẨN:</b></color>\n{record.SampleAnswer}\n\n";
                }
                else
                {
                    solText += "<color=#fef08a><b>ĐÁP ÁN THAM KHẢO CHUẨN:</b></color>\n1. Xác định môi trường truyền sáng.\n2. So sánh góc tới với góc giới hạn.\n3. Kết luận và nêu ứng dụng.\n\n";
                }

                // 6.3. Bài làm của học sinh (nếu đã nộp bài)
                bool isAttempted = (!string.IsNullOrEmpty(record.ConfirmedText) && record.ConfirmedText != "(Chưa làm)");
                if (isAttempted)
                {
                    solText += $"<color=#86efac><b>BÀI LÀM ĐÃ NỘP CỦA BẠN:</b></color>\n{record.ConfirmedText}";
                }

                textSolution.text = solText;
            }

            // 7. Reset vị trí cuộn lên đầu trang
            Canvas.ForceUpdateCanvases();
            ResetScroll(scrollQuestionPrompt);
            ResetScroll(scrollAiFeedback);
            ResetScroll(scrollSuggestions);
            ResetScroll(scrollSolution);

            // 8. Cập nhật trạng thái nút Prev/Next
            if (btnPrev != null)
            {
                btnPrev.interactable = (_currentQuestionIndex > 0);
                if (btnPrev.image != null)
                {
                    btnPrev.image.color = (_currentQuestionIndex > 0) ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.45f);
                }
            }
            if (btnNext != null)
            {
                btnNext.interactable = (_currentQuestionIndex < _essayAnswers.Count - 1);
                if (btnNext.image != null)
                {
                    btnNext.image.color = (_currentQuestionIndex < _essayAnswers.Count - 1) ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.45f);
                }
            }
        }

        private void ResetScroll(ScrollRect sr)
        {
            if (sr != null)
            {
                sr.StopMovement();
                if (sr.content != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(sr.content);
                }
                sr.verticalNormalizedPosition = 1f;
            }
        }

        private void OnPrevQuestionClicked()
        {
            if (_currentQuestionIndex > 0)
            {
                SelectQuestion(_currentQuestionIndex - 1);
            }
        }

        private void OnNextQuestionClicked()
        {
            if (_currentQuestionIndex < _essayAnswers.Count - 1)
            {
                SelectQuestion(_currentQuestionIndex + 1);
            }
        }

        private void OnBackToResultClicked()
        {
            NavigationService.Instance.NavigateTo(RouteId.EssayResult);
        }
    }
}
