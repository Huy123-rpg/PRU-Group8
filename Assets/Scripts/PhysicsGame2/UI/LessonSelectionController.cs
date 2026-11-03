using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển toàn bộ logic giao diện Scene 05_LessonSelection:
    /// - Hiển thị tiêu đề chương động theo SelectedChapterId (Chương I..V).
    /// - Tải danh sách bài học động theo PhysicsCurriculumData:
    ///     + Chương II: 7 bài (Bài 5..10 + TEST tổng hợp Chương II).
    ///     + Chương III: 4 bài (Bài 11..13 + TEST tổng hợp Chương III).
    /// - Hệ thống ScrollRect + Viewport + Content + Scrollbar hoạt động mượt mà, hỗ trợ chuột và cảm ứng.
    /// - Cấu trúc Scrollbar chuẩn: Scrollbar_Track -> Sliding Area -> Scrollbar_Thumb (Handle), ngăn chặn lỗi tràn viền che giao diện.
    /// - Sắp xếp lớp Sibling Index chuẩn mực (Background ở đáy, Banner/Header ở trên, ScrollView và Scrollbar ở giữa, Buttons trên cùng).
    /// - Chọn bài học: highlight viền vàng, cập nhật session. Double-click hoặc bấm Xác nhận để vào Bản đồ chương.
    /// - Nút "Quay lại" trở về Scene 04_ChapterSelection.
    /// - Nút "Xác nhận" đưa người chơi vào Bản đồ chương tương ứng.
    /// </summary>
    public class LessonSelectionController : MonoBehaviour
    {
        [Header("=== HEADER & BUTTONS ===")]
        [SerializeField] private TextMeshProUGUI headerChapterText;
        [SerializeField] private Button btnBackToChapters;
        [SerializeField] private Button btnConfirmLesson;

        [Header("=== SCROLL VIEW & TEMPLATE ===")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentTransform;
        [SerializeField] private Scrollbar verticalScrollbar;
        [SerializeField] private GameObject lessonItemTemplate;

        private CurriculumLessonInfo _currentSelectedLesson;
        private GameObject _selectedCardGo;
        private TMP_FontAsset _defaultFont;
        private float _lastCardClickTime;
        private string _lastClickedLessonId;

        private readonly List<GameObject> _spawnedCards = new List<GameObject>();

        private void Awake()
        {
            EnsureEventSystem();
            EnsureSessionState();
            FindOrBuildUI();
            SetupButtons();
        }

        private void Start()
        {
            UpdateChapterHeader();
            PopulateLessons();
            CheckPendingStoryFlow();
        }

        private void EnsureSessionState()
        {
            if (PhysicsGameSession.Instance == null)
            {
                Debug.LogWarning("[LessonSelectionController] PhysicsGameSession.Instance is null. Sẽ dùng fallback.");
                return;
            }

            // Mặc định là Chương 2 nếu chưa chọn chương nào
            if (PhysicsGameSession.Instance.SelectedChapterId < 1 || PhysicsGameSession.Instance.SelectedChapterId > 5)
            {
                Debug.Log("[LessonSelectionController] Chưa có SelectedChapterId hợp lệ, mặc định chọn Chương II (Ánh sáng).");
                PhysicsGameSession.Instance.SelectChapter(2);
            }
        }

        private void CheckPendingStoryFlow()
        {
            int chapterId = PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SelectedChapterId : 2;
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            StoryFlowController.TryShowPostGameplayOrPendingStories(
                completedChapterHint: chapterId,
                customSave: null,
                onAllFinished: null,
                canvasTransform: canvas != null ? canvas.transform : null);
        }

        private void EnsureEventSystem()
        {
            EventSystem es = FindAnyObjectByType<EventSystem>();
            if (es != null)
            {
                if (es.gameObject.name == "Image")
                {
                    es.gameObject.name = "EventSystem";
                }
            }
            else
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }
        }

        private void FindOrBuildUI()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[LessonSelectionController] Không tìm thấy Canvas trong Scene!");
                return;
            }

            // 0. Đảm bảo Background ở vị trí đầu tiên (lớp sau cùng)
            Transform bgTr = canvas.transform.Find("Background");
            if (bgTr != null)
            {
                bgTr.SetSiblingIndex(0);
                // Đảm bảo không dính HoverPulseEffect
                var bgPulse = bgTr.GetComponent<UIHoverPulseEffect>();
                if (bgPulse != null) Destroy(bgPulse);
            }

            // 1. Tìm Header Text
            if (headerChapterText == null)
            {
                // Ưu tiên tìm đúng tên GameObject
                Transform chTr = canvas.transform.Find("Header_ChapterText");
                if (chTr != null)
                {
                    headerChapterText = chTr.GetComponent<TextMeshProUGUI>();
                }

                if (headerChapterText == null)
                {
                    foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
                    {
                        string t = tmp.text != null ? tmp.text.Trim() : "";
                        if (tmp.gameObject.name.IndexOf("Header", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            t.StartsWith("Chương", StringComparison.OrdinalIgnoreCase) ||
                            t.IndexOf("Ánh sáng", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            t.IndexOf("Điện", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            (tmp.rectTransform.anchoredPosition.y > 250f && tmp.fontSize >= 28f))
                        {
                            headerChapterText = tmp;
                            _defaultFont = tmp.font;
                            break;
                        }
                    }
                }
            }
            if (_defaultFont == null && headerChapterText != null)
            {
                _defaultFont = headerChapterText.font;
            }

            // 2. Tìm Nút Quay lại top-left
            if (btnBackToChapters == null)
            {
                GameObject backGo = GameObject.Find("Btn_Back")
                                 ?? GameObject.Find("btnBack")
                                 ?? GameObject.Find("quaylai")
                                 ?? GameObject.Find("QuayLai")
                                 ?? GameObject.Find("Quay lại")
                                 ?? GameObject.Find("Image (1)");

                if (backGo == null)
                {
                    foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                    {
                        RectTransform rt = img.rectTransform;
                        if (rt.anchoredPosition.x < -400f && rt.anchoredPosition.y > 300f)
                        {
                            backGo = img.gameObject;
                            break;
                        }
                    }
                }

                if (backGo != null)
                {
                    backGo.name = "Btn_Back";
                    btnBackToChapters = backGo.GetComponent<Button>();
                    if (btnBackToChapters == null) btnBackToChapters = backGo.AddComponent<Button>();

                    Image img = backGo.GetComponent<Image>();
                    if (img != null)
                    {
                        img.raycastTarget = true;
                        btnBackToChapters.targetGraphic = img;
                    }
                }
            }

            // 3. Tìm Nút Xác nhận ở dưới
            if (btnConfirmLesson == null)
            {
                GameObject confirmGo = GameObject.Find("Btn_Confirm")
                                    ?? GameObject.Find("btnConfirm")
                                    ?? GameObject.Find("xacnhan")
                                    ?? GameObject.Find("XacNhan")
                                    ?? GameObject.Find("Button");

                if (confirmGo == null)
                {
                    foreach (var btn in canvas.GetComponentsInChildren<Button>(true))
                    {
                        if (btn.gameObject != (btnBackToChapters != null ? btnBackToChapters.gameObject : null) &&
                            btn.transform.localPosition.y < -300f)
                        {
                            confirmGo = btn.gameObject;
                            break;
                        }
                    }
                }

                if (confirmGo != null)
                {
                    confirmGo.name = "Btn_Confirm";
                    btnConfirmLesson = confirmGo.GetComponent<Button>();
                    if (btnConfirmLesson == null) btnConfirmLesson = confirmGo.AddComponent<Button>();

                    Image img = confirmGo.GetComponent<Image>();
                    if (img != null)
                    {
                        img.raycastTarget = true;
                        btnConfirmLesson.targetGraphic = img;
                    }
                }
            }

            // 4. Tìm và Cấu hình Chuẩn Scrollbar (Track -> Sliding Area -> Thumb)
            BuildOrFixScrollbar(canvas);

            // 5. Tìm hoặc cấu hình ScrollView + Viewport + Content
            BuildOrFixScrollView(canvas);

            // 6. Tìm và chuẩn hóa Lesson Item Template
            FindOrCreateLessonTemplate(canvas);

            // 7. Sắp xếp thứ tự Sibling Index để đảm bảo các lớp hiển thị không bao giờ bị đè
            ArrangeCanvasSiblingOrder(canvas);
        }

        private void BuildOrFixScrollbar(Canvas canvas)
        {
            // Tìm Track và Thumb hiện có
            Transform trackTr = canvas.transform.Find("Scrollbar_Track")
                             ?? canvas.transform.Find("Scrollbar Vertical")
                             ?? canvas.transform.Find("Scrollbar");

            Transform thumbTr = canvas.transform.Find("Scrollbar_Thumb")
                             ?? canvas.transform.Find("Handle");

            Image scrollTrackImg = trackTr != null ? trackTr.GetComponent<Image>() : null;
            Image scrollThumbImg = thumbTr != null ? thumbTr.GetComponent<Image>() : null;

            // Fallback tìm kiếm trong Canvas nếu chưa tìm thấy bằng tên
            if (scrollTrackImg == null || scrollThumbImg == null)
            {
                foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                {
                    string n = img.gameObject.name;
                    if (n == "Btn_Back" || n == "Btn_Confirm" ||
                        n == "Header_TopFrame" || n == "Header_Ribbon" || n == "Background" ||
                        n == "LessonItemTemplate") continue;

                    RectTransform rt = img.rectTransform;
                    if (scrollTrackImg == null && rt.anchoredPosition.x > 380f && rt.sizeDelta.y > 400f)
                    {
                        scrollTrackImg = img;
                    }
                    else if (scrollThumbImg == null && rt.anchoredPosition.x > 380f && rt.sizeDelta.y >= 80f && rt.sizeDelta.y <= 350f)
                    {
                        scrollThumbImg = img;
                    }
                }
            }

            // 1. Chuẩn hóa GameObject Scrollbar_Track
            GameObject trackGo;
            if (scrollTrackImg != null)
            {
                trackGo = scrollTrackImg.gameObject;
            }
            else
            {
                trackGo = new GameObject("Scrollbar_Track");
                trackGo.transform.SetParent(canvas.transform, false);
                scrollTrackImg = trackGo.AddComponent<Image>();
                scrollTrackImg.color = new Color(0.12f, 0.16f, 0.28f, 0.75f);
            }

            trackGo.name = "Scrollbar_Track";
            RectTransform trackRt = trackGo.GetComponent<RectTransform>() ?? trackGo.AddComponent<RectTransform>();
            trackRt.SetParent(canvas.transform, false);
            trackRt.anchorMin = new Vector2(0.5f, 0.5f);
            trackRt.anchorMax = new Vector2(0.5f, 0.5f);
            trackRt.pivot = new Vector2(0.5f, 0.5f);
            trackRt.anchoredPosition = new Vector2(446f, -55f);
            trackRt.sizeDelta = new Vector2(24f, 530f); // Khớp chuẩn xác chiều cao Viewport 530f
            scrollTrackImg.raycastTarget = true;

            // 2. Tạo Sliding Area chuẩn bên trong Scrollbar_Track
            Transform slidingAreaTr = trackGo.transform.Find("Sliding Area");
            GameObject slidingAreaGo;
            if (slidingAreaTr != null)
            {
                slidingAreaGo = slidingAreaTr.gameObject;
            }
            else
            {
                slidingAreaGo = new GameObject("Sliding Area");
                slidingAreaGo.transform.SetParent(trackGo.transform, false);
            }
            RectTransform slidingRt = slidingAreaGo.GetComponent<RectTransform>() ?? slidingAreaGo.AddComponent<RectTransform>();
            slidingRt.anchorMin = Vector2.zero;
            slidingRt.anchorMax = Vector2.one;
            slidingRt.offsetMin = new Vector2(0f, 10f);
            slidingRt.offsetMax = new Vector2(0f, -10f);
            slidingRt.pivot = new Vector2(0.5f, 0.5f);

            // 3. Đặt Scrollbar_Thumb làm con của Sliding Area (NGĂN CHẶN TRIỆT ĐỂ LỖI TRÀN MÀN HÌNH)
            GameObject thumbGo;
            if (scrollThumbImg != null)
            {
                thumbGo = scrollThumbImg.gameObject;
            }
            else
            {
                thumbGo = new GameObject("Scrollbar_Thumb");
                scrollThumbImg = thumbGo.AddComponent<Image>();
                scrollThumbImg.color = new Color(0.96f, 0.82f, 0.35f, 1f);
            }

            thumbGo.name = "Scrollbar_Thumb";
            thumbGo.transform.SetParent(slidingRt, false); // Reparent vào Sliding Area!
            RectTransform thumbRt = thumbGo.GetComponent<RectTransform>() ?? thumbGo.AddComponent<RectTransform>();
            thumbRt.anchorMin = Vector2.zero;
            thumbRt.anchorMax = Vector2.one;
            thumbRt.offsetMin = Vector2.zero;
            thumbRt.offsetMax = Vector2.zero;
            thumbRt.pivot = new Vector2(0.5f, 0.5f);
            thumbRt.sizeDelta = Vector2.zero;
            scrollThumbImg.raycastTarget = true;

            // Loại bỏ các component không phù hợp trên scrollbar nếu có
            var badPulse1 = trackGo.GetComponent<UIHoverPulseEffect>(); if (badPulse1 != null) Destroy(badPulse1);
            var badPulse2 = thumbGo.GetComponent<UIHoverPulseEffect>(); if (badPulse2 != null) Destroy(badPulse2);
            var badOutline = thumbGo.GetComponent<Outline>(); if (badOutline != null) Destroy(badOutline);

            // 4. Thiết lập Scrollbar component
            verticalScrollbar = trackGo.GetComponent<Scrollbar>();
            if (verticalScrollbar == null) verticalScrollbar = trackGo.AddComponent<Scrollbar>();

            verticalScrollbar.handleRect = thumbRt;
            verticalScrollbar.targetGraphic = scrollThumbImg;
            verticalScrollbar.direction = Scrollbar.Direction.BottomToTop;
            verticalScrollbar.numberOfSteps = 0;

            Debug.Log("[LessonSelectionController] Đã cấu hình Scrollbar_Track -> Sliding Area -> Scrollbar_Thumb chuẩn chỉnh.");
        }

        private void BuildOrFixScrollView(Canvas canvas)
        {
            if (scrollRect == null)
            {
                scrollRect = canvas.GetComponentInChildren<ScrollRect>(true);
            }

            if (scrollRect == null)
            {
                GameObject scrollRoot = new GameObject("ScrollView_Lessons");
                scrollRoot.transform.SetParent(canvas.transform, false);
                RectTransform scrollRt = scrollRoot.AddComponent<RectTransform>();
                scrollRt.anchorMin = new Vector2(0.5f, 0.5f);
                scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
                scrollRt.pivot = new Vector2(0.5f, 0.5f);
                scrollRt.anchoredPosition = new Vector2(54f, -55f);
                scrollRt.sizeDelta = new Vector2(765f, 530f);

                scrollRect = scrollRoot.AddComponent<ScrollRect>();
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                scrollRect.movementType = ScrollRect.MovementType.Clamped;
                scrollRect.scrollSensitivity = 50f;

                // Viewport
                GameObject viewportGo = new GameObject("Viewport");
                viewportGo.transform.SetParent(scrollRoot.transform, false);
                RectTransform vpRt = viewportGo.AddComponent<RectTransform>();
                vpRt.anchorMin = Vector2.zero;
                vpRt.anchorMax = Vector2.one;
                vpRt.sizeDelta = Vector2.zero;
                vpRt.anchoredPosition = Vector2.zero;
                vpRt.pivot = new Vector2(0.5f, 0.5f);

                viewportGo.AddComponent<RectMask2D>();
                Image vpImg = viewportGo.AddComponent<Image>();
                vpImg.color = new Color(0, 0, 0, 0.001f);
                vpImg.raycastTarget = true;

                // Content
                GameObject contentGo = new GameObject("Content");
                contentGo.transform.SetParent(viewportGo.transform, false);
                contentTransform = contentGo.AddComponent<RectTransform>();
                contentTransform.anchorMin = new Vector2(0f, 1f);
                contentTransform.anchorMax = new Vector2(1f, 1f);
                contentTransform.pivot = new Vector2(0.5f, 1f);
                contentTransform.anchoredPosition = Vector2.zero;
                contentTransform.sizeDelta = new Vector2(0f, 0f);

                VerticalLayoutGroup vlg = contentGo.AddComponent<VerticalLayoutGroup>();
                vlg.spacing = 16f;
                vlg.padding = new RectOffset(5, 5, 12, 20);
                vlg.childAlignment = TextAnchor.UpperCenter;
                vlg.childControlWidth = false;
                vlg.childControlHeight = false;
                vlg.childForceExpandWidth = false;
                vlg.childForceExpandHeight = false;

                ContentSizeFitter csf = contentGo.AddComponent<ContentSizeFitter>();
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                scrollRect.content = contentTransform;
                scrollRect.viewport = vpRt;

                if (verticalScrollbar != null)
                {
                    scrollRect.verticalScrollbar = verticalScrollbar;
                    scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
                }

                Debug.Log("[LessonSelectionController] Đã tự động khởi tạo ScrollView, Viewport và Content.");
            }
            else
            {
                contentTransform = scrollRect.content;
                scrollRect.scrollSensitivity = 50f;
                scrollRect.movementType = ScrollRect.MovementType.Clamped;

                if (verticalScrollbar != null)
                {
                    scrollRect.verticalScrollbar = verticalScrollbar;
                    scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
                }
            }
        }

        private void ArrangeCanvasSiblingOrder(Canvas canvas)
        {
            // Đảm bảo thứ tự hiển thị không che đè lẫn nhau:
            // 0: Background (Thư viện phép thuật)
            // 1: Header_TopFrame (Tiêu đề CHỌN BÀI)
            // 2: Header_Ribbon (Khung tên chương)
            // 3: Header_ChapterText (Text tên chương)
            // 4: ScrollView_Lessons (Khu vực danh sách bài)
            // 5: Scrollbar_Track (Thanh cuộn vàng bên phải)
            // 6: Btn_Back (Nút quay lại)
            // 7: Btn_Confirm (Nút xác nhận)
            int index = 0;

            Transform bg = canvas.transform.Find("Background");
            if (bg != null) bg.SetSiblingIndex(index++);

            Transform topFrame = canvas.transform.Find("Header_TopFrame");
            if (topFrame != null) topFrame.SetSiblingIndex(index++);

            Transform ribbon = canvas.transform.Find("Header_Ribbon");
            if (ribbon != null) ribbon.SetSiblingIndex(index++);

            if (headerChapterText != null) headerChapterText.transform.SetSiblingIndex(index++);

            if (scrollRect != null) scrollRect.transform.SetSiblingIndex(index++);

            if (verticalScrollbar != null) verticalScrollbar.transform.SetSiblingIndex(index++);

            if (btnBackToChapters != null) btnBackToChapters.transform.SetSiblingIndex(index++);

            if (btnConfirmLesson != null) btnConfirmLesson.transform.SetSiblingIndex(index++);
        }

        private void FindOrCreateLessonTemplate(Canvas canvas)
        {
            if (lessonItemTemplate != null) return;

            Transform existingTr = canvas.transform.Find("LessonItemTemplate");
            GameObject baseCard = existingTr != null ? existingTr.gameObject : null;
            GameObject starIcon = null;
            TextMeshProUGUI cardText = null;

            List<GameObject> staticCardsToClean = new List<GameObject>();

            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
            {
                RectTransform rt = img.rectTransform;
                string n = img.gameObject.name;
                if (n == "Btn_Back" || n == "Btn_Confirm" ||
                    n == "Scrollbar_Track" || n == "Scrollbar_Thumb" ||
                    n == "Header_TopFrame" || n == "Header_Ribbon" || n == "Background" ||
                    n == "Viewport" || n == "ScrollView_Lessons")
                    continue;

                // Nhận diện Card khung bài học (~749 x ~160)
                if (rt.sizeDelta.x > 700f && rt.sizeDelta.x < 800f && rt.sizeDelta.y > 120f && rt.sizeDelta.y < 200f)
                {
                    if (baseCard == null)
                    {
                        baseCard = img.gameObject;
                    }
                    else if (img.gameObject != baseCard)
                    {
                        staticCardsToClean.Add(img.gameObject);
                    }
                }
                // Nhận diện ngôi sao trang trí (~121 x ~51)
                else if (rt.sizeDelta.x > 100f && rt.sizeDelta.x < 140f && rt.sizeDelta.y > 35f && rt.sizeDelta.y < 65f)
                {
                    if (starIcon == null) starIcon = img.gameObject;
                    else staticCardsToClean.Add(img.gameObject);
                }
            }

            // Tìm các Text TMP cũ cần làm sạch
            foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (tmp == headerChapterText) continue;
                if (tmp.transform.parent != null && tmp.transform.parent.name == "Btn_Confirm") continue;

                if (cardText == null)
                {
                    cardText = tmp;
                }
                else
                {
                    staticCardsToClean.Add(tmp.gameObject);
                }
            }

            if (baseCard != null)
            {
                baseCard.name = "LessonItemTemplate";
                RectTransform baseRt = baseCard.GetComponent<RectTransform>();
                baseRt.sizeDelta = new Vector2(740f, 145f);

                // Reparent starIcon và cardText thành con của baseCard để di chuyển đồng bộ
                if (starIcon != null)
                {
                    starIcon.name = "StarIcon";
                    starIcon.transform.SetParent(baseCard.transform, false);
                    RectTransform sRt = starIcon.GetComponent<RectTransform>();
                    sRt.anchorMin = new Vector2(0f, 0.5f);
                    sRt.anchorMax = new Vector2(0f, 0.5f);
                    sRt.pivot = new Vector2(0f, 0.5f);
                    sRt.anchoredPosition = new Vector2(25f, 0f);
                    sRt.sizeDelta = new Vector2(110f, 45f);
                }

                if (cardText == null)
                {
                    cardText = baseCard.GetComponentInChildren<TextMeshProUGUI>(true);
                }

                if (cardText != null)
                {
                    cardText.name = "LessonTitle";
                    cardText.transform.SetParent(baseCard.transform, false);
                    RectTransform tRt = cardText.rectTransform;
                    tRt.anchorMin = new Vector2(0.5f, 0.5f);
                    tRt.anchorMax = new Vector2(0.5f, 0.5f);
                    tRt.pivot = new Vector2(0.5f, 0.5f);
                    tRt.anchoredPosition = new Vector2(45f, 0f);
                    tRt.sizeDelta = new Vector2(530f, 85f);

                    FormatLessonText(cardText, "Bài học mẫu", false);
                }

                Button btn = baseCard.GetComponent<Button>();
                if (btn == null) btn = baseCard.AddComponent<Button>();

                // Thêm viền Outline để phục vụ hiệu ứng chọn bài
                Outline outline = baseCard.GetComponent<Outline>();
                if (outline == null) outline = baseCard.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 0.88f, 0.35f, 0.95f);
                outline.effectDistance = new Vector2(3f, -3f);
                outline.enabled = false;

                lessonItemTemplate = baseCard;
                lessonItemTemplate.SetActive(false);

                // Ẩn các Card tĩnh còn lại trong scene để tránh đè giao diện
                foreach (var go in staticCardsToClean)
                {
                    if (go != null) go.SetActive(false);
                }

                Debug.Log("[LessonSelectionController] Đã chuẩn hóa Card Template và làm sạch các Card tĩnh cũ.");
            }
        }

        private void SetupButtons()
        {
            // 1. Nút Quay lại
            if (btnBackToChapters != null)
            {
                btnBackToChapters.onClick.RemoveAllListeners();
                btnBackToChapters.onClick.AddListener(OnBackClicked);

                var fb = btnBackToChapters.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);
                UIHoverPulseEffect.Attach(btnBackToChapters.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);

                Debug.Log("[LessonSelectionController] Đã gắn listener và hiệu ứng cho Nút Quay Lại.");
            }

            // 2. Nút Xác nhận
            if (btnConfirmLesson != null)
            {
                btnConfirmLesson.onClick.RemoveAllListeners();
                btnConfirmLesson.onClick.AddListener(OnConfirmClicked);

                var fb = btnConfirmLesson.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);
                UIHoverPulseEffect.Attach(btnConfirmLesson.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);

                Debug.Log("[LessonSelectionController] Đã gắn listener và hiệu ứng cho Nút Xác Nhận.");
            }
        }

        private void UpdateChapterHeader()
        {
            int chapterId = PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SelectedChapterId : 2;
            if (chapterId < 1 || chapterId > 5) chapterId = 2;

            string headerTitle = PhysicsCurriculumData.GetChapterHeader(chapterId);

            if (headerChapterText != null)
            {
                headerChapterText.text = headerTitle;
                Debug.Log($"[LessonSelectionController] Cập nhật tiêu đề chương: '{headerTitle}' (Chương {chapterId})");
            }
        }

        public void PopulateLessons()
        {
            if (contentTransform == null || lessonItemTemplate == null)
            {
                Debug.LogError("[LessonSelectionController] Chưa có contentTransform hoặc lessonItemTemplate!");
                return;
            }

            // Xóa các card đã sinh trước đó
            foreach (var go in _spawnedCards)
            {
                if (go != null) Destroy(go);
            }
            _spawnedCards.Clear();

            int chapterId = PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SelectedChapterId : 2;
            if (chapterId < 1 || chapterId > 5) chapterId = 2;

            List<CurriculumLessonInfo> lessons = PhysicsCurriculumData.GetLessons(chapterId);
            Debug.Log($"[LessonSelectionController] Đang nạp {lessons.Count} bài học cho Chương {chapterId}...");

            GameObject firstCardGo = null;
            CurriculumLessonInfo firstLesson = null;
            GameObject preselectedCardGo = null;
            string preselectedLessonId = PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SelectedLessonId : null;

            foreach (var lesson in lessons)
            {
                GameObject cardGo = Instantiate(lessonItemTemplate, contentTransform);
                cardGo.name = $"LessonCard_{lesson.lessonId}";
                cardGo.SetActive(true);
                _spawnedCards.Add(cardGo);

                TextMeshProUGUI titleTmp = cardGo.GetComponentInChildren<TextMeshProUGUI>(true);
                if (titleTmp != null)
                {
                    FormatLessonText(titleTmp, lesson.DisplayName, lesson.isTest);
                }

                Button btn = cardGo.GetComponent<Button>();
                if (btn == null) btn = cardGo.AddComponent<Button>();

                var capturedLesson = lesson;
                var capturedCardGo = cardGo;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnLessonCardClicked(capturedLesson, capturedCardGo));

                // Gắn hiệu ứng Hover Pulse (chỉ trên card này)
                var fb = cardGo.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);
                UIHoverPulseEffect.Attach(cardGo, hover: 1.04f, minPulse: 1.02f, maxPulse: 1.06f, outline: false);

                // Gắn ScrollPassThrough để cuộn chuột mượt mà dù đang trỏ vào card/text/star
                var passThrough = cardGo.GetComponent<ScrollPassThrough>();
                if (passThrough == null) passThrough = cardGo.AddComponent<ScrollPassThrough>();
                passThrough.targetScrollRect = scrollRect;

                if (firstCardGo == null)
                {
                    firstCardGo = cardGo;
                    firstLesson = lesson;
                }

                if (!string.IsNullOrEmpty(preselectedLessonId) && lesson.lessonId == preselectedLessonId)
                {
                    preselectedCardGo = cardGo;
                }
            }

            // Tự động chọn bài học đã lưu trước đó, hoặc bài đầu tiên
            if (preselectedCardGo != null)
            {
                var matched = lessons.Find(l => l.lessonId == preselectedLessonId);
                SelectLesson(matched, preselectedCardGo);
            }
            else if (firstCardGo != null && firstLesson != null)
            {
                SelectLesson(firstLesson, firstCardGo);
            }

            // Đưa vị trí Scroll về đỉnh danh sách
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private void FormatLessonText(TextMeshProUGUI tmp, string text, bool isTest)
        {
            if (tmp == null) return;

            if (_defaultFont != null) tmp.font = _defaultFont;
            tmp.text = text;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 18f;
            tmp.fontSizeMax = 28f;
            tmp.lineSpacing = -6f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Ellipsis;

            // Màu chữ: Vàng rực rỡ hoàng gia cho Bài kiểm tra, Trắng sáng cho bài học thông thường
            tmp.color = isTest ? new Color(1f, 0.90f, 0.40f, 1f) : new Color(0.95f, 0.96f, 1f, 1f);
        }

        private void OnLessonCardClicked(CurriculumLessonInfo lesson, GameObject cardGo)
        {
            // Kiểm tra double-click: nếu nhấn lại bài đang chọn trong vòng 0.4s -> Xác nhận vào bản đồ
            if (_currentSelectedLesson != null && _currentSelectedLesson.lessonId == lesson.lessonId &&
                (Time.time - _lastCardClickTime) < 0.4f)
            {
                OnConfirmClicked();
                return;
            }

            _lastCardClickTime = Time.time;
            _lastClickedLessonId = lesson.lessonId;

            SelectLesson(lesson, cardGo);
        }

        private void SelectLesson(CurriculumLessonInfo lesson, GameObject cardGo)
        {
            _currentSelectedLesson = lesson;
            _selectedCardGo = cardGo;

            int chapterId = PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SelectedChapterId : 2;
            if (chapterId < 1 || chapterId > 5) chapterId = 2;

            // Cập nhật State toàn cục
            if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.SelectLesson(lesson.lessonId, lesson.lessonTitle);
            }

            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.Selection.ChapterId = chapterId;
                NavigationService.Instance.Selection.LessonId = lesson.lessonId;
            }

            // Cập nhật hiệu ứng highlight trực quan trên các Card
            foreach (var card in _spawnedCards)
            {
                if (card == null) continue;
                Outline ol = card.GetComponent<Outline>();
                Image img = card.GetComponent<Image>();

                if (card == cardGo)
                {
                    // Card được chọn: bật viền vàng mạ và đổi màu nền sáng hơn
                    if (ol != null) ol.enabled = true;
                    if (img != null) img.color = new Color(1f, 1f, 0.88f, 1f);
                }
                else
                {
                    // Card khác: tắt viền và giữ màu chuẩn
                    if (ol != null) ol.enabled = false;
                    if (img != null) img.color = new Color(0.92f, 0.92f, 0.92f, 1f);
                }
            }

            Debug.Log($"[LessonSelectionController] Đã chọn bài: {lesson.DisplayName} (ID: {lesson.lessonId})");
        }

        private void OnConfirmClicked()
        {
            if (_currentSelectedLesson == null)
            {
                Debug.LogWarning("[LessonSelectionController] Chưa chọn bài học nào để xác nhận!");
                return;
            }

            int chapterId = PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SelectedChapterId : 2;
            if (chapterId < 1 || chapterId > 5) chapterId = 2;

            string deviceId = RestorationService.GetDeviceIdForLesson(_currentSelectedLesson.lessonId);
            bool isCompleted = RestorationService.IsChapterFullyRestored(chapterId);
            RouteId mapRoute = SceneCatalog.GetChapterMapRoute(chapterId, isCompleted);

            Debug.Log($"[LessonSelectionController] Xác nhận bài: '{_currentSelectedLesson.DisplayName}' (DeviceId: {deviceId}) -> Điều hướng đến Bản đồ: {mapRoute}");

            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.Selection.ChapterId = chapterId;
                NavigationService.Instance.Selection.LessonId = _currentSelectedLesson.lessonId;
                NavigationService.Instance.NavigateTo(mapRoute);
            }
        }

        private void OnBackClicked()
        {
            Debug.Log("[LessonSelectionController] Người chơi bấm Quay lại -> Trở về Scene 04_ChapterSelection.");
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.NavigateTo(RouteId.ChapterSelection);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnBackClicked();
            }
        }
    }

    /// <summary>
    /// Chuyển tiếp sự kiện lăn chuột OnScroll lên ScrollRect cha,
    /// bảo đảm thao tác cuộn luôn mượt mà khi con trỏ chuột nằm trên card, chữ hoặc ngôi sao.
    /// </summary>
    public class ScrollPassThrough : MonoBehaviour, IScrollHandler
    {
        public ScrollRect targetScrollRect;

        public void OnScroll(PointerEventData eventData)
        {
            if (targetScrollRect != null)
            {
                targetScrollRect.OnScroll(eventData);
            }
        }
    }
}
