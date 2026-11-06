using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.Biology
{
    /// <summary>
    /// Trung tâm quản lý của GIẢNG VIÊN cho môn Sinh học.
    ///
    /// Toàn bộ giao diện được dựng bằng code nên không phải sửa file .unity
    /// (giảm rủi ro merge conflict với các nhánh khác).
    ///
    /// Chức năng:
    ///   1. Tạo bài học mới theo từng chủ đề Sinh học.
    ///   2. Thêm / xoá câu hỏi trong bài (Trắc nghiệm A/B/C/D hoặc Tự luận chấm theo từ khóa).
    ///   3. Xem bảng kết quả thi của học sinh, xuất ra Excel.
    ///   4. Cấu hình đồng bộ Google Sheet để giảng viên xem điểm online.
    /// </summary>
    public class TeacherManager : MonoBehaviour
    {
        public static TeacherManager Instance { get; private set; }

        // ----- Danh sách chủ đề Sinh học (khớp với BiologyMenuManager) -----
        private static readonly (string id, string title)[] BiologyChapters =
        {
            ("CD1_SH", "Chương I: Di truyền và biến dị"),
            ("CD2_SH", "Chương II: Sinh thái và môi trường"),
            ("CD3_SH", "Chương III: Cơ thể người và sức khỏe"),
            ("CD4_SH", "Chương IV: Trao đổi chất & năng lượng"),
            ("CD5_SH", "Chương V: Tế bào – Đơn vị sự sống")
        };

        // ----- Màu sắc giao diện -----
        private static readonly Color ColorBg = new Color(0.10f, 0.14f, 0.12f, 1f);
        private static readonly Color ColorPanel = new Color(0.17f, 0.22f, 0.19f, 1f);
        private static readonly Color ColorPanelDark = new Color(0.13f, 0.17f, 0.15f, 1f);
        private static readonly Color ColorAccent = new Color(0.16f, 0.62f, 0.35f, 1f);
        private static readonly Color ColorAccentDark = new Color(0.10f, 0.42f, 0.24f, 1f);
        private static readonly Color ColorDanger = new Color(0.70f, 0.24f, 0.20f, 1f);
        private static readonly Color ColorWarning = new Color(0.85f, 0.55f, 0.12f, 1f);
        private static readonly Color ColorInfo = new Color(0.18f, 0.48f, 0.78f, 1f);
        private static readonly Color ColorText = new Color(0.94f, 0.96f, 0.94f, 1f);
        private static readonly Color ColorTextDim = new Color(0.72f, 0.78f, 0.74f, 1f);
        private static readonly Color ColorInputBg = new Color(0.95f, 0.95f, 0.92f, 1f);
        private static readonly Color ColorInputText = new Color(0.12f, 0.14f, 0.13f, 1f);

        // ----- Cỡ chữ (phóng to để dễ đọc) -----
        private const float FontTitle = 32f;      // tiêu đề lớn
        private const float FontSection = 22f;    // tiêu đề section
        private const float FontLabel = 19f;      // nhãn trên ô nhập
        private const float FontBody = 20f;       // chữ trong ô nhập
        private const float FontButton = 21f;     // chữ trên nút
        private const float FontList = 20f;       // tiêu đề danh sách bài học
        private const float FontListSub = 16f;    // phụ đề danh sách bài học
        private const float FontSmall = 17f;      // chữ phụ

        // 3 cấp độ khó giảng viên có thể chọn
        private static readonly string[] DifficultyLevels = { "Dễ", "Trung bình", "Khó" };
        private int selectedDifficulty = 0;

        // ----- Tham chiếu UI -----
        private Canvas canvas;
        private RectTransform rootPanel;
        private RectTransform topBar;
        private RectTransform navBar;
        private RectTransform statusBar;
        private Transform lessonListContent;
        private Transform questionListContent;
        private Transform resultListContent;
        private Transform formRoot;
        private Transform resultRoot;
        private Transform lessonFormRoot;
        private TextMeshProUGUI statusText;
        private TextMeshProUGUI lessonListHeader;
        private TextMeshProUGUI resultSummaryText;

        // ----- Trạng thái -----
        private string selectedChapterId = BiologyChapters[0].id;
        private string selectedLessonId = "";
        private string currentFormMode = "TracNghiem";
        private string correctAnswerChoice = "A";
        private bool showingResults = false;

        // ----- Ô nhập của form câu hỏi -----
        private TMP_InputField inputQuestion;
        private TMP_InputField inputA, inputB, inputC, inputD;
        private TMP_InputField inputTuLuanAnswer, inputKeywords, inputExplain;
        private GameObject groupTracNghiem, groupTuLuan, groupKeywords, groupTuLuanAnswer;
        private Button[] correctButtons = new Button[4];
        private TextMeshProUGUI[] correctLabels = new TextMeshProUGUI[4];
        private Button[] optionBadgeButtons = new Button[4];
        private TextMeshProUGUI[] optionBadgeLabels = new TextMeshProUGUI[4];
        private RectTransform formScrollRt;
        private RectTransform formBoxRt;
        private RectTransform qListBoxRt;
        private TextMeshProUGUI qListHeader;

        // ----- 3 nút chọn độ khó -----
        private Button[] difficultyButtons = new Button[DifficultyLevels.Length];

        // ----- Ô nhập của form tạo bài -----
        private TMP_InputField inputLessonTitle;
        private Button[] chapterButtons = new Button[BiologyChapters.Length];
        private Button[] lessonModeButtons = new Button[2];
        private string newLessonMode = "TracNghiem";

        // ----- Ô nhập của tab kết quả -----
        private TMP_InputField inputSheetUrl;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            EnsureCanvas();
            BuildUI();
            ShowLessonsTab();
            Canvas.ForceUpdateCanvases();
            LogLayoutWarnings();
            Debug.Log("[TeacherManager] 🧑‍🏫 Đã mở Trung tâm giảng viên môn Sinh học.");
        }

        /// <summary>
        /// Kiểm tra và cảnh báo các phần tử UI bị co về 0 pixel (sẽ vô hình, không bấm được).
        /// Giúp phát hiện lỗi layout ngay trong Console mà không cần mở Unity lúc chạy.
        /// </summary>
        private void LogLayoutWarnings()
        {
            if (rootPanel == null) return;

            List<string> broken = new List<string>();
            foreach (RectTransform rt in rootPanel.GetComponentsInChildren<RectTransform>(true))
            {
                Vector2 size = rt.rect.size;
                if (size.x < 1f || size.y < 1f)
                {
                    broken.Add($"{rt.name} ({size.x:0}x{size.y:0})");
                }
            }

            if (broken.Count > 0)
            {
                Debug.LogWarning($"[TeacherManager] ⚠️ {broken.Count} phần tử UI có kích thước 0 (sẽ không hiển thị): " +
                                 string.Join(", ", broken));
            }
            else
            {
                Debug.Log("[TeacherManager] ✅ Kiểm tra layout: mọi phần tử UI đều có kích thước hợp lệ.");
            }
        }

        #region ===== KHỞI TẠO CANVAS & BỐ CỤC =====

        private void EnsureCanvas()
        {
            // Trung tâm giảng viên LUÔN dùng Canvas riêng, không dùng lại Canvas scene trước
            // (login / chọn môn / chapter list) vì chúng sẽ che mất nút điều hướng.
            GameObject existing = GameObject.Find("TeacherCanvas");
            GameObject canvasObj;
            if (existing != null)
            {
                canvasObj = existing;
                canvas = existing.GetComponent<Canvas>();
                if (canvas == null) canvas = canvasObj.AddComponent<Canvas>();
            }
            else
            {
                canvasObj = new GameObject("TeacherCanvas", typeof(RectTransform), typeof(Canvas));
                canvas = canvasObj.GetComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 500;

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            if (canvasObj.GetComponent<GraphicRaycaster>() == null)
            {
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            TeacherNavigation.IsolateOverlayCanvas(canvas);

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem));
            }
        }

        private void BuildUI()
        {
            const float TitleBarH = 52f;
            const float NavBarH = 56f;
            const float HeaderH = TitleBarH + NavBarH;
            const float StatusH = 38f;

            rootPanel = CreateRect("TeacherRoot", canvas.transform);
            Stretch(rootPanel);

            Image bg = rootPanel.gameObject.AddComponent<Image>();
            bg.color = ColorBg;
            bg.raycastTarget = true;

            // Hàng 1: tiêu đề — không chia chỗ với nút nên không bị cắt chữ
            topBar = CreateRect("TopBar", rootPanel);
            topBar.anchorMin = new Vector2(0f, 1f);
            topBar.anchorMax = new Vector2(1f, 1f);
            topBar.pivot = new Vector2(0.5f, 1f);
            topBar.anchoredPosition = Vector2.zero;
            topBar.sizeDelta = new Vector2(0f, TitleBarH);
            Image topBg = topBar.gameObject.AddComponent<Image>();
            topBg.color = ColorAccentDark;
            topBg.raycastTarget = true;

            TextMeshProUGUI title = CreateText(topBar, "Title", "TRUNG TÂM GIẢNG VIÊN • MÔN SINH HỌC", 24f, ColorText, TextAlignmentOptions.Left);
            title.enableAutoSizing = true;
            title.fontSizeMin = 16f;
            title.fontSizeMax = 24f;
            Stretch(title.rectTransform);
            title.rectTransform.offsetMin = new Vector2(24f, 4f);
            title.rectTransform.offsetMax = new Vector2(-24f, -4f);

            // Hàng 2: 4 nút trải đều toàn chiều ngang — không phụ thuộc độ rộng cố định 760px
            navBar = CreateRect("NavBar", rootPanel);
            navBar.anchorMin = new Vector2(0f, 1f);
            navBar.anchorMax = new Vector2(1f, 1f);
            navBar.pivot = new Vector2(0.5f, 1f);
            navBar.anchoredPosition = new Vector2(0f, -TitleBarH);
            navBar.sizeDelta = new Vector2(0f, NavBarH);
            Image navBg = navBar.gameObject.AddComponent<Image>();
            navBg.color = ColorPanel;
            navBg.raycastTarget = true;

            Button btnPlayTest = CreateButton(navBar, "Btn_PlayTest", "CHƠI THỬ", ColorInfo, 160f, () => TeacherNavigation.GoToChapterList());
            Button btnResult = CreateButton(navBar, "Btn_Results", "KẾT QUẢ", ColorAccent, 160f, () => ToggleResultsView());
            Button btnExport = CreateButton(navBar, "Btn_ExportCsv", "XUẤT CÂU HỎI", ColorWarning, 160f, () => ExportQuestionsCsv());
            Button btnExit = CreateButton(navBar, "Btn_Exit", "← CHỌN MÔN", ColorDanger, 160f, () => ExitToSubjectSelect());
            PlaceEqualRow(navBar, new[] { btnPlayTest, btnResult, btnExport, btnExit }, 8f, 8f);

            statusBar = CreateRect("StatusBar", rootPanel);
            statusBar.anchorMin = new Vector2(0f, 0f);
            statusBar.anchorMax = new Vector2(1f, 0f);
            statusBar.pivot = new Vector2(0.5f, 0f);
            statusBar.anchoredPosition = Vector2.zero;
            statusBar.sizeDelta = new Vector2(0f, StatusH);
            Image statusBg = statusBar.gameObject.AddComponent<Image>();
            statusBg.color = ColorPanelDark;
            statusBg.raycastTarget = true;

            statusText = CreateText(statusBar, "StatusText", "Sẵn sàng.", FontBody, ColorTextDim, TextAlignmentOptions.Left);
            Stretch(statusText.rectTransform);
            statusText.rectTransform.offsetMin = new Vector2(24f, 0f);
            statusText.rectTransform.offsetMax = new Vector2(-24f, 0f);

            RectTransform content = CreateRect("Content", rootPanel);
            content.anchorMin = new Vector2(0f, 0f);
            content.anchorMax = new Vector2(1f, 1f);
            content.offsetMin = new Vector2(16f, StatusH + 12f);
            content.offsetMax = new Vector2(-16f, -(HeaderH + 10f));

            // Cột trái: danh sách bài + nút tạo bài
            RectTransform leftCol = CreateRect("LeftColumn", content);
            leftCol.anchorMin = new Vector2(0f, 0f);
            leftCol.anchorMax = new Vector2(0f, 1f);
            leftCol.pivot = new Vector2(0f, 0.5f);
            leftCol.anchoredPosition = Vector2.zero;
            leftCol.sizeDelta = new Vector2(430f, 0f);
            Image leftBg = leftCol.gameObject.AddComponent<Image>();
            leftBg.color = ColorPanel;

            // Cột phải: form thêm câu hỏi / kết quả
            // Sử dụng full-stretch anchor, bắt đầu sau leftCol (430px + 16px khoảng cách = 446px)
            RectTransform rightCol = CreateRect("RightColumn", content);
            rightCol.anchorMin = new Vector2(0f, 0f);
            rightCol.anchorMax = new Vector2(1f, 1f);
            rightCol.offsetMin = new Vector2(446f, 0f);  // bắt đầu sau leftCol
            rightCol.offsetMax = new Vector2(0f, 0f);    // kết thúc tại mép phải
            Image rightBg = rightCol.gameObject.AddComponent<Image>();
            rightBg.color = ColorPanel;

            BuildLeftColumn(leftCol);
            BuildQuestionForm(rightCol);
            BuildResultView(rightCol);
            BuildLessonForm(leftCol);

            if (formRoot != null) formRoot.SetAsLastSibling();
            BringChromeToFront();
        }

        /// <summary>
        /// Thanh tiêu đề / điều hướng / trạng thái luôn nằm trên nội dung và form.
        /// </summary>
        private void BringChromeToFront()
        {
            if (topBar != null) topBar.SetAsLastSibling();
            if (navBar != null) navBar.SetAsLastSibling();
            if (statusBar != null) statusBar.SetAsLastSibling();
        }

        /// <summary>
        /// Chia đều các nút trên một hàng (neo stretch) để không bị tràn / biến mất khi đổi độ phân giải.
        /// </summary>
        private void PlaceEqualRow(RectTransform parent, Button[] buttons, float padX, float padY)
        {
            int n = buttons.Length;
            for (int i = 0; i < n; i++)
            {
                RectTransform rt = buttons[i].GetComponent<RectTransform>();
                rt.SetParent(parent, false);
                float minX = i / (float)n;
                float maxX = (i + 1) / (float)n;
                rt.anchorMin = new Vector2(minX, 0f);
                rt.anchorMax = new Vector2(maxX, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(-(padX * 2f), -(padY * 2f));
            }
        }

        #endregion

        #region ===== CỘT TRÁI: DANH SÁCH BÀI + TẠO BÀI =====

        private void BuildLeftColumn(RectTransform leftCol)
        {
            // Nút mở form tạo bài
            Button btnNew = CreateButton(leftCol, "Btn_NewLesson", "➕ TẠO BÀI MỚI", ColorAccent, 0f, () => ShowLessonForm());
            SetTopStretch(btnNew.GetComponent<RectTransform>(), 14f, 14f, 14f, 52f);

            // Tiêu đề danh sách
            lessonListHeader = CreateText(leftCol, "LessonListTitle", "DANH SÁCH BÀI HỌC", FontSection, ColorText, TextAlignmentOptions.Left);
            SetTopStretch(lessonListHeader.rectTransform, 16f, 16f, 76f, 30f);

            // ScrollView danh sách bài (chiếm phần còn lại của cột trái)
            ScrollRect lessonScroll = CreateScrollView(leftCol, "LessonScroll", out Transform contentOut);
            lessonListContent = contentOut;

            // Neo đáy để cuộn: chiều cao = chiều cao cha - lề trên - lề dưới
            RectTransform lessonScrollRt = lessonScroll.GetComponent<RectTransform>();
            lessonScrollRt.anchorMin = new Vector2(0f, 0f);
            lessonScrollRt.anchorMax = new Vector2(1f, 1f);
            lessonScrollRt.pivot = new Vector2(0.5f, 0.5f);
            lessonScrollRt.offsetMin = new Vector2(14f, 14f);
            lessonScrollRt.offsetMax = new Vector2(-14f, -116f);

            // Form tạo bài: đặt làm overlay trên rootPanel để hiển thị chắc chắn trên cùng
            lessonFormRoot = BuildLessonFormContent(rootPanel);

            // Sau khi BuildLessonFormContent set anchor=stretch+offset12,
            // ta override lại để form nằm đúng trên khu vực cột trái (x: 16..446, y: 54..screen-86)
            RectTransform lFormRt = (RectTransform)lessonFormRoot;
            lFormRt.anchorMin = new Vector2(0f, 0f);
            lFormRt.anchorMax = new Vector2(0f, 1f);
            lFormRt.pivot = new Vector2(0f, 0.5f);
            lFormRt.offsetMin = new Vector2(16f, 50f);
            lFormRt.offsetMax = new Vector2(446f, -118f);

            lessonFormRoot.gameObject.SetActive(false);
        }

        private void BuildLessonForm(RectTransform leftCol)
        {
            // Vùng chứa form được tạo trong BuildLeftColumn
        }

        /// <summary>
        /// Dựng nội dung form tạo bài (ẩn mặc định).
        /// </summary>
        private Transform BuildLessonFormContent(RectTransform parent)
        {
            RectTransform form = CreateRect("LessonForm", parent);
            // Anchor/size sẽ được set bởi caller (BuildLeftColumn) sau khi hàm này trả về
            Image formBg = form.gameObject.AddComponent<Image>();
            formBg.color = ColorPanelDark;

            // Layout dọc tự động: các phần tử xếp theo nhau, không bao giờ tràn
            VerticalLayoutGroup vlg = form.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.spacing = 10f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(8, 8, 8, 8);

            CreateLabel(form, "LblTitle", "TIÊU ĐỀ BÀI HỌC", FontLabel, 30f);
            inputLessonTitle = CreateInput(form, "InputLessonTitle", "Ví dụ: Bài 1: Menđen và Di truyền học", false, 54f);
            AddToLayout(inputLessonTitle.GetComponent<RectTransform>(), 54f);

            CreateLabel(form, "LblChapter", "CHỦ ĐỀ", FontLabel, 30f);

            // Các nút chọn chủ đề
            for (int i = 0; i < BiologyChapters.Length; i++)
            {
                int idx = i;
                string chapId = BiologyChapters[i].id;
                string chapTitle = BiologyChapters[i].title;

                Button chapBtn = CreateButton(form, $"Btn_Chapter_{chapId}", chapTitle, ColorPanel, 0f, () => SelectChapter(chapId));
                AddToLayout(chapBtn.GetComponent<RectTransform>(), 48f);

                TextMeshProUGUI label = chapBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.fontSize = FontSmall;
                    label.enableAutoSizing = true;
                    label.fontSizeMin = 13f;
                    label.fontSizeMax = FontSmall;
                }

                chapterButtons[i] = chapBtn;
            }

            CreateLabel(form, "LblLessonMode", "HÌNH THỨC MẶC ĐỊNH", FontLabel, 30f);

            // Hàng 2 nút chọn hình thức
            RectTransform modeRow = CreateRect("LessonModeRow", form);
            AddToLayout(modeRow, 54f);
            HorizontalLayoutGroup hlg = modeRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = 10f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            string[] modeNames = { "Trắc nghiệm", "Tự luận" };
            string[] modeValues = { "TracNghiem", "TuLuan" };
            for (int i = 0; i < 2; i++)
            {
                int idx = i;
                string modeVal = modeValues[i];
                Button btn = CreateButton(modeRow, $"Btn_LessonMode_{modeVal}", modeNames[i], ColorPanel, 0f, () => SelectLessonMode(modeVal));
                modeRow.SetParent(form, false);
                btn.transform.SetParent(modeRow, false);
                AddToLayout(btn.GetComponent<RectTransform>(), 54f, 0.5f);

                TextMeshProUGUI label = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.fontSize = FontButton;

                lessonModeButtons[i] = btn;
            }

            Button btnCreate = CreateButton(form, "Btn_CreateLesson", "💾 TẠO BÀI", ColorAccent, 0f, () => CreateLesson());
            AddToLayout(btnCreate.GetComponent<RectTransform>(), 60f);

            Button btnCancel = CreateButton(form, "Btn_CancelLesson", "HUỶ", ColorDanger, 0f, () => HideLessonForm());
            AddToLayout(btnCancel.GetComponent<RectTransform>(), 52f);

            RefreshChapterButtons();
            RefreshLessonModeButtons();

            return form;
        }

        private void SelectChapter(string chapterId)
        {
            selectedChapterId = chapterId;
            RefreshChapterButtons();
            SetStatus($"Đã chọn chủ đề: {GetChapterTitle(chapterId)}");
        }

        private void RefreshChapterButtons()
        {
            for (int i = 0; i < chapterButtons.Length; i++)
            {
                if (chapterButtons[i] == null) continue;
                bool active = BiologyChapters[i].id == selectedChapterId;
                Image img = chapterButtons[i].targetGraphic as Image;
                if (img != null) img.color = active ? ColorAccent : ColorPanel;
            }
        }

        private void SelectLessonMode(string mode)
        {
            newLessonMode = mode;
            RefreshLessonModeButtons();
        }

        private void RefreshLessonModeButtons()
        {
            string[] values = { "TracNghiem", "TuLuan" };
            for (int i = 0; i < lessonModeButtons.Length; i++)
            {
                if (lessonModeButtons[i] == null) continue;
                bool active = values[i] == newLessonMode;
                Image img = lessonModeButtons[i].targetGraphic as Image;
                if (img != null) img.color = active ? ColorAccent : ColorPanel;
            }
        }

        private void CreateLesson()
        {
            string title = inputLessonTitle != null ? inputLessonTitle.text.Trim() : "";
            if (string.IsNullOrEmpty(title))
            {
                SetStatus("⚠️ Vui lòng nhập tiêu đề bài học.", new Color(1f, 0.75f, 0.3f));
                return;
            }

            TeacherLesson lesson = TeacherStore.CreateLesson(GetChapterTitle(selectedChapterId), title, newLessonMode);
            if (lesson == null)
            {
                SetStatus("⚠️ Không tạo được bài học.", Color.red);
                return;
            }

            selectedLessonId = lesson.lessonId;
            if (inputLessonTitle != null) inputLessonTitle.text = "";

            SetStatus($"✅ Đã tạo bài \"{lesson.title}\" ({lesson.questions.Count} câu hỏi).");
            HideLessonForm();
            RefreshLessonList();
            ShowQuestionForm();
        }

        private void ShowLessonForm()
        {
            if (lessonFormRoot != null)
            {
                lessonFormRoot.gameObject.SetActive(true);
                lessonFormRoot.SetAsLastSibling();
                BringChromeToFront();
            }
            RefreshChapterButtons();
            RefreshLessonModeButtons();
            SetStatus("Đang tạo bài học mới. Nhập tiêu đề và chọn chủ đề.");
        }

        private void HideLessonForm()
        {
            if (lessonFormRoot != null) lessonFormRoot.gameObject.SetActive(false);
        }

        #endregion

        #region ===== CỘT PHẢI: FORM THÊM CÂU HỎI =====

        private void BuildQuestionForm(RectTransform rightCol)
        {
            // Cột bên trái của rightCol: Form thêm câu hỏi (rộng 62%, chiếm toàn bộ chiều cao)
            formBoxRt = CreateRect("FormBox", rightCol);
            formBoxRt.anchorMin = new Vector2(0f, 0f);
            formBoxRt.anchorMax = new Vector2(0.62f, 1f);
            formBoxRt.offsetMin = new Vector2(8f, 8f);
            formBoxRt.offsetMax = new Vector2(-4f, -8f);

            // Cột bên phải của rightCol: Danh sách câu hỏi của bài đang chọn (rộng 38%, chiếm toàn bộ chiều cao)
            qListBoxRt = CreateRect("QuestionListBox", rightCol);
            qListBoxRt.anchorMin = new Vector2(0.62f, 0f);
            qListBoxRt.anchorMax = new Vector2(1f, 1f);
            qListBoxRt.offsetMin = new Vector2(4f, 8f);
            qListBoxRt.offsetMax = new Vector2(-8f, -8f);
            Image qListBg = qListBoxRt.gameObject.AddComponent<Image>();
            qListBg.color = ColorPanelDark;

            // Header cho danh sách câu hỏi
            qListHeader = CreateText(qListBoxRt, "QListHeader", "CÂU HỎI TRONG BÀI (0)", FontSection, ColorText, TextAlignmentOptions.Left);
            SetTopStretch(qListHeader.rectTransform, 14f, 14f, 12f, 32f);

            // ScrollView danh sách câu hỏi trong bài
            ScrollRect qScroll = CreateScrollView(qListBoxRt, "QuestionScroll", out Transform qContent);
            questionListContent = qContent;
            RectTransform qScrollRt = qScroll.GetComponent<RectTransform>();
            qScrollRt.anchorMin = new Vector2(0f, 0f);
            qScrollRt.anchorMax = new Vector2(1f, 1f);
            qScrollRt.pivot = new Vector2(0.5f, 0.5f);
            qScrollRt.offsetMin = new Vector2(8f, 8f);
            qScrollRt.offsetMax = new Vector2(-8f, -48f);

            // ScrollView bao quanh Form thêm câu hỏi (nằm trong formBoxRt)
            GameObject formScrollObj = new GameObject("QuestionFormScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            formScrollObj.transform.SetParent(formBoxRt, false);
            Image fsBg = formScrollObj.GetComponent<Image>();
            fsBg.color = ColorPanelDark;
            RectTransform fsRt = formScrollObj.GetComponent<RectTransform>();
            Stretch(fsRt);
            formScrollRt = fsRt;

            // Viewport
            GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(Image));
            viewportObj.transform.SetParent(formScrollObj.transform, false);
            Image vpImg = viewportObj.GetComponent<Image>();
            vpImg.color = Color.clear; // Đảm bảo raycastTarget bắt sự kiện cuộn chuột
            RectTransform viewportRt = viewportObj.GetComponent<RectTransform>();
            Stretch(viewportRt);
            viewportObj.AddComponent<RectMask2D>();

            // Form là content của ScrollView
            formRoot = BuildQuestionFormContent(viewportRt);
            RectTransform formRt = formRoot.GetComponent<RectTransform>();
            formRt.anchorMin = new Vector2(0f, 1f);
            formRt.anchorMax = new Vector2(1f, 1f);
            formRt.pivot = new Vector2(0.5f, 1f);
            formRt.offsetMin = Vector2.zero;
            formRt.offsetMax = Vector2.zero;

            // ContentSizeFitter giúp form tự co giãn theo nội dung
            ContentSizeFitter formCsf = formRt.gameObject.AddComponent<ContentSizeFitter>();
            formCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            formCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            ScrollRect formScroll = formScrollObj.GetComponent<ScrollRect>();
            formScroll.content = formRt;
            formScroll.viewport = viewportRt;
            formScroll.horizontal = false;
            formScroll.vertical = true;
            formScroll.movementType = ScrollRect.MovementType.Clamped;
            formScroll.scrollSensitivity = 40f;
        }

        /// <summary>
        /// Tạo một item phương án gồm Badge chữ cái (A, B, C, D) + Ô nhập nội dung phương án.
        /// Bấm vào badge chữ cái cũng sẽ chọn luôn phương án đó làm đáp án đúng.
        /// </summary>
        private TMP_InputField CreateOptionItem(Transform parent, string letter, out Button badgeBtn, out TextMeshProUGUI badgeLabel)
        {
            RectTransform item = CreateRect($"Item_{letter}", parent);

            HorizontalLayoutGroup hlg = item.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = 6f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // Badge chữ cái (A, B, C, D)
            badgeBtn = CreateButton(item, $"Btn_Badge_{letter}", letter, ColorPanelDark, 50f, () => SetCorrectAnswer(letter));
            RectTransform badgeRt = badgeBtn.GetComponent<RectTransform>();
            LayoutElement badgeLe = badgeRt.gameObject.AddComponent<LayoutElement>();
            badgeLe.minWidth = 50f;
            badgeLe.preferredWidth = 50f;
            badgeLe.flexibleWidth = 0f;
            badgeLabel = badgeBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (badgeLabel != null)
            {
                badgeLabel.fontSize = FontButton;
                badgeLabel.fontStyle = FontStyles.Bold;
            }

            // Ô nhập nội dung phương án
            TMP_InputField input = CreateInput(item, $"Input{letter}", $"Phương án {letter}...", false, 42f);
            RectTransform inputRt = input.GetComponent<RectTransform>();
            LayoutElement inputLe = inputRt.gameObject.AddComponent<LayoutElement>();
            inputLe.minWidth = 80f;
            inputLe.flexibleWidth = 1f;

            return input;
        }

        private Transform BuildQuestionFormContent(RectTransform parent)
        {
            RectTransform form = CreateRect("QuestionForm", parent);
            form.anchorMin = Vector2.zero;
            form.anchorMax = Vector2.one;
            form.offsetMin = new Vector2(12f, 12f);
            form.offsetMax = new Vector2(-12f, -12f);

            // Dùng VerticalLayoutGroup xếp dọc gọn gàng
            VerticalLayoutGroup vlg = form.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.spacing = 6f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(4, 4, 4, 12);

            TextMeshProUGUI header = CreateText(form, "Header", "THÊM CÂU HỎI MỚI", FontSection, ColorText, TextAlignmentOptions.Left);
            AddToLayout(header.rectTransform, FontSection + 6f);

            // Chọn hình thức câu hỏi
            Button btnTn = CreateButton(form, "Btn_Mode_TracNghiem", "TRẮC NGHIỆM", ColorPanel, 0f, () => SetQuestionMode("TracNghiem"));
            Button btnTl = CreateButton(form, "Btn_Mode_TuLuan", "TỰ LUẬN", ColorPanel, 0f, () => SetQuestionMode("TuLuan"));
            RectTransform modeRow = CreateRect("ModeRow", form);
            AddToLayout(modeRow, 44f);
            HorizontalLayoutGroup hlg = modeRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = 8f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            modeRow.SetParent(form, false);
            btnTn.transform.SetParent(modeRow, false);
            btnTl.transform.SetParent(modeRow, false);
            AddToLayout(btnTn.GetComponent<RectTransform>(), 44f, 0.5f);
            AddToLayout(btnTl.GetComponent<RectTransform>(), 44f, 0.5f);

            // Nội dung câu hỏi
            CreateLabel(form, "LblQuestion", "NỘI DUNG CÂU HỎI", FontLabel, 26f);
            inputQuestion = CreateInput(form, "InputQuestion", "Nhập nội dung câu hỏi...", true, 68f);
            AddToLayout(inputQuestion.GetComponent<RectTransform>(), 68f);

            // ---- Nhóm trắc nghiệm ----
            groupTracNghiem = CreateRect("GroupTracNghiem", form).gameObject;
            RectTransform tnRt = groupTracNghiem.GetComponent<RectTransform>();
            VerticalLayoutGroup tnLayout = groupTracNghiem.AddComponent<VerticalLayoutGroup>();
            tnLayout.childAlignment = TextAnchor.UpperLeft;
            tnLayout.spacing = 6f;
            tnLayout.childControlWidth = true;
            tnLayout.childControlHeight = true;
            tnLayout.childForceExpandWidth = true;
            tnLayout.childForceExpandHeight = false;
            tnLayout.padding = new RectOffset(0, 0, 2, 2);
            AddToLayout(tnRt, 205f);

            CreateLabel(tnRt, "LblAnswers", "4 PHƯƠNG ÁN (Bấm chữ cái để chọn nhanh đáp án đúng)", FontLabel, 24f);

            // Hàng 1: Phương án A & B
            RectTransform rowAB = CreateRect("Row_AB", tnRt);
            AddToLayout(rowAB, 42f);
            HorizontalLayoutGroup hlgAB = rowAB.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlgAB.childAlignment = TextAnchor.MiddleLeft;
            hlgAB.spacing = 8f;
            hlgAB.childControlWidth = true;
            hlgAB.childControlHeight = true;
            hlgAB.childForceExpandWidth = true;
            hlgAB.childForceExpandHeight = true;

            inputA = CreateOptionItem(rowAB, "A", out optionBadgeButtons[0], out optionBadgeLabels[0]);
            AddToLayout(inputA.transform.parent.GetComponent<RectTransform>(), 42f, 0.5f);
            inputB = CreateOptionItem(rowAB, "B", out optionBadgeButtons[1], out optionBadgeLabels[1]);
            AddToLayout(inputB.transform.parent.GetComponent<RectTransform>(), 42f, 0.5f);

            // Hàng 2: Phương án C & D
            RectTransform rowCD = CreateRect("Row_CD", tnRt);
            AddToLayout(rowCD, 42f);
            HorizontalLayoutGroup hlgCD = rowCD.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlgCD.childAlignment = TextAnchor.MiddleLeft;
            hlgCD.spacing = 8f;
            hlgCD.childControlWidth = true;
            hlgCD.childControlHeight = true;
            hlgCD.childForceExpandWidth = true;
            hlgCD.childForceExpandHeight = true;

            inputC = CreateOptionItem(rowCD, "C", out optionBadgeButtons[2], out optionBadgeLabels[2]);
            AddToLayout(inputC.transform.parent.GetComponent<RectTransform>(), 42f, 0.5f);
            inputD = CreateOptionItem(rowCD, "D", out optionBadgeButtons[3], out optionBadgeLabels[3]);
            AddToLayout(inputD.transform.parent.GetComponent<RectTransform>(), 42f, 0.5f);

            CreateLabel(tnRt, "LblCorrect", "ĐÁP ÁN ĐÚNG", FontLabel, 24f);

            // Hàng 4 nút chọn đáp án đúng A / B / C / D
            RectTransform correctRow = CreateRect("CorrectRow", tnRt);
            AddToLayout(correctRow, 46f);
            HorizontalLayoutGroup chlg = correctRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            chlg.childAlignment = TextAnchor.MiddleLeft;
            chlg.spacing = 8f;
            chlg.childControlWidth = true;
            chlg.childControlHeight = true;
            chlg.childForceExpandWidth = true;
            chlg.childForceExpandHeight = true;

            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                char letter = (char)('A' + i);
                Button btn = CreateButton(correctRow, $"Btn_Correct_{letter}", $"ĐÁP ÁN {letter}", ColorPanel, 0f, () => SetCorrectAnswer(letter.ToString()));
                AddToLayout(btn.GetComponent<RectTransform>(), 46f, 0.25f);

                correctButtons[i] = btn;
                correctLabels[i] = btn.GetComponentInChildren<TextMeshProUGUI>();
            }

            // ---- Nhóm tự luận ----
            groupTuLuan = CreateRect("GroupTuLuan", form).gameObject;
            RectTransform tlRt = groupTuLuan.GetComponent<RectTransform>();
            VerticalLayoutGroup tlLayout = groupTuLuan.AddComponent<VerticalLayoutGroup>();
            tlLayout.childAlignment = TextAnchor.UpperLeft;
            tlLayout.spacing = 6f;
            tlLayout.childControlWidth = true;
            tlLayout.childControlHeight = true;
            tlLayout.childForceExpandWidth = true;
            tlLayout.childForceExpandHeight = false;
            tlLayout.padding = new RectOffset(0, 0, 2, 2);
            AddToLayout(tlRt, 175f);

            CreateLabel(tlRt, "LblTuLuanAnswer", "ĐÁP ÁN MẪU (để đối chiếu)", FontLabel, 24f);
            inputTuLuanAnswer = CreateInput(tlRt, "InputTuLuanAnswer", "Đáp án chuẩn của giáo viên...", true, 64f);
            AddToLayout(inputTuLuanAnswer.GetComponent<RectTransform>(), 64f);

            groupTuLuanAnswer = groupTuLuan;

            CreateLabel(tlRt, "LblKeywords", "TỪ KHÓA CHẤM ĐIỂM (cách nhau bằng dấu ;)", FontLabel, 24f);
            inputKeywords = CreateInput(tlRt, "InputKeywords", "từ khóa 1;từ khóa 2;từ khóa 3", false, 42f);
            AddToLayout(inputKeywords.GetComponent<RectTransform>(), 42f);

            groupKeywords = groupTuLuan;

            // ---- Phần chung (Hiển thị cho cả Trắc nghiệm & Tự luận) ----
            CreateLabel(form, "LblExplain", "GIẢI THÍCH / LỜI GIẢI", FontLabel, 24f);
            inputExplain = CreateInput(form, "InputExplain", "Giải thích ngắn gọn sau khi trả lời...", true, 58f);
            AddToLayout(inputExplain.GetComponent<RectTransform>(), 58f);

            // Độ khó: 3 cấp độ để giảng viên chọn
            CreateLabel(form, "LblDifficulty", "ĐỘ KHÓ", FontLabel, 24f);
            RectTransform diffRow = CreateRect("DiffRow", form);
            AddToLayout(diffRow, 42f);
            HorizontalLayoutGroup dlg = diffRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            dlg.childAlignment = TextAnchor.MiddleLeft;
            dlg.spacing = 8f;
            dlg.childControlWidth = true;
            dlg.childControlHeight = true;
            dlg.childForceExpandWidth = true;
            dlg.childForceExpandHeight = true;

            for (int i = 0; i < DifficultyLevels.Length; i++)
            {
                int idx = i;
                string lvl = DifficultyLevels[i];
                Button dBtn = CreateButton(diffRow, $"Btn_Diff_{lvl}", lvl, ColorPanel, 0f, () => SetDifficulty(idx));
                AddToLayout(dBtn.GetComponent<RectTransform>(), 42f, 1f / 3f);
                difficultyButtons[i] = dBtn;
            }

            // Nút lưu (áp dụng cho người chơi)
            Button btnSave = CreateButton(form, "Btn_SaveQuestion", "💾 LƯU CÂU HỎI VÀO BÀI (ÁP DỤNG CHO NGƯỜI CHƠI)", ColorAccent, 0f, () => SaveQuestion());
            AddToLayout(btnSave.GetComponent<RectTransform>(), 50f);

            // Nút xoá bài đang chọn
            Button btnDeleteLesson = CreateButton(form, "Btn_DeleteLesson", "🗑 XOÁ BÀI HỌC NÀY", ColorDanger, 0f, () => DeleteSelectedLesson());
            AddToLayout(btnDeleteLesson.GetComponent<RectTransform>(), 40f);

            SetQuestionMode("TracNghiem");
            SetCorrectAnswer("A");
            SetDifficulty(0);

            return form;
        }

        /// <summary>
        /// Đưa một phần tử vào layout dọc của form, kèm chiều cao cố định.
        /// </summary>
        private void AddToLayout(RectTransform rt, float height, float flexibleWidth = -1f)
        {
            LayoutElement le = rt.GetComponent<LayoutElement>();
            if (le == null) le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            le.flexibleHeight = 0f;

            if (flexibleWidth > 0f)
            {
                le.preferredWidth = 0f;
                le.flexibleWidth = flexibleWidth;
            }
            else
            {
                le.flexibleWidth = 0f;
            }
        }

        private void SetQuestionMode(string mode)
        {
            currentFormMode = mode;
            if (groupTracNghiem != null) groupTracNghiem.SetActive(mode == "TracNghiem");
            if (groupTuLuan != null) groupTuLuan.SetActive(mode == "TuLuan");

            Button btnTn = formRoot != null ? formRoot.Find("Btn_Mode_TracNghiem")?.GetComponent<Button>() : null;
            Button btnTl = formRoot != null ? formRoot.Find("Btn_Mode_TuLuan")?.GetComponent<Button>() : null;
            if (btnTn != null)
            {
                Image img = btnTn.targetGraphic as Image;
                if (img != null) img.color = mode == "TracNghiem" ? ColorAccent : ColorPanel;
            }
            if (btnTl != null)
            {
                Image img = btnTl.targetGraphic as Image;
                if (img != null) img.color = mode == "TuLuan" ? ColorAccent : ColorPanel;
            }

            Canvas.ForceUpdateCanvases();
        }

        private void SetCorrectAnswer(string letter)
        {
            correctAnswerChoice = letter;
            for (int i = 0; i < 4; i++)
            {
                char ch = (char)('A' + i);
                bool isCorrect = (ch.ToString() == letter);

                // Cập nhật 4 nút ở hàng ĐÁP ÁN ĐÚNG
                if (correctButtons[i] != null)
                {
                    Image img = correctButtons[i].targetGraphic as Image;
                    if (img != null) img.color = isCorrect ? ColorAccent : ColorPanel;
                }
                if (correctLabels[i] != null)
                {
                    correctLabels[i].text = isCorrect ? $"✔ ĐÁP ÁN {ch}" : $"ĐÁP ÁN {ch}";
                    correctLabels[i].color = isCorrect ? Color.white : ColorTextDim;
                }

                // Cập nhật 4 badge ở đầu mỗi dòng phương án
                if (optionBadgeButtons[i] != null)
                {
                    Image bImg = optionBadgeButtons[i].targetGraphic as Image;
                    if (bImg != null) bImg.color = isCorrect ? ColorAccent : ColorPanelDark;
                }
                if (optionBadgeLabels[i] != null)
                {
                    optionBadgeLabels[i].text = isCorrect ? $"{ch} ✔" : $"{ch}";
                    optionBadgeLabels[i].color = isCorrect ? Color.white : ColorText;
                }
            }
        }

        private void SetDifficulty(int idx)
        {
            selectedDifficulty = idx;
            for (int i = 0; i < difficultyButtons.Length; i++)
            {
                if (difficultyButtons[i] == null) continue;
                Image img = difficultyButtons[i].targetGraphic as Image;
                if (img != null) img.color = i == idx ? ColorAccent : ColorPanel;
            }
        }

        private void SaveQuestion()
        {
            if (string.IsNullOrEmpty(selectedLessonId))
            {
                SetStatus("⚠️ Hãy chọn hoặc tạo một bài học trước khi thêm câu hỏi.", new Color(1f, 0.75f, 0.3f));
                return;
            }

            string questionText = inputQuestion != null ? inputQuestion.text.Trim() : "";
            if (string.IsNullOrEmpty(questionText))
            {
                SetStatus("⚠️ Vui lòng nhập nội dung câu hỏi.", new Color(1f, 0.75f, 0.3f));
                return;
            }

            TeacherQuestion q = new TeacherQuestion
            {
                questionText = questionText,
                mode = currentFormMode,
                explain = inputExplain != null ? inputExplain.text.Trim() : "",
                difficulty = DifficultyLevels[selectedDifficulty],
                // Độ khó xác định thời gian và cộng máu mặc định
                timeLimit = selectedDifficulty == 0 ? 20f : (selectedDifficulty == 1 ? 15f : 10f),
                hpBonus = selectedDifficulty == 0 ? 25f : (selectedDifficulty == 1 ? 20f : 10f)
            };

            if (currentFormMode == "TracNghiem")
            {
                q.ansA = inputA != null ? inputA.text.Trim() : "";
                q.ansB = inputB != null ? inputB.text.Trim() : "";
                q.ansC = inputC != null ? inputC.text.Trim() : "";
                q.ansD = inputD != null ? inputD.text.Trim() : "";
                q.correctAnswer = correctAnswerChoice;

                if (string.IsNullOrEmpty(q.ansA) || string.IsNullOrEmpty(q.ansB) ||
                    string.IsNullOrEmpty(q.ansC) || string.IsNullOrEmpty(q.ansD))
                {
                    SetStatus("⚠️ Vui lòng nhập đủ 4 phương án A, B, C, D.", new Color(1f, 0.75f, 0.3f));
                    return;
                }
            }
            else
            {
                q.correctAnswer = inputTuLuanAnswer != null ? inputTuLuanAnswer.text.Trim() : "";
                q.keywordsTuLuan = inputKeywords != null ? inputKeywords.text.Trim() : "";

                if (string.IsNullOrEmpty(q.correctAnswer) && string.IsNullOrEmpty(q.keywordsTuLuan))
                {
                    SetStatus("⚠️ Câu tự luận cần có đáp án mẫu hoặc từ khóa để chấm điểm.", new Color(1f, 0.75f, 0.3f));
                    return;
                }
            }

            TeacherQuestion saved = TeacherStore.AddQuestion(selectedLessonId, q);
            if (saved == null)
            {
                SetStatus("⚠️ Không lưu được câu hỏi.", Color.red);
                return;
            }

            ClearQuestionInputs();
            SetStatus($"✅ Đã lưu câu hỏi \"{saved.questionId}\" vào bài đang chọn.");
            RefreshLessonList();
            RefreshQuestionList();
        }

        private void ClearQuestionInputs()
        {
            if (inputQuestion != null) inputQuestion.text = "";
            if (inputA != null) inputA.text = "";
            if (inputB != null) inputB.text = "";
            if (inputC != null) inputC.text = "";
            if (inputD != null) inputD.text = "";
            if (inputTuLuanAnswer != null) inputTuLuanAnswer.text = "";
            if (inputKeywords != null) inputKeywords.text = "";
            if (inputExplain != null) inputExplain.text = "";
            SetCorrectAnswer("A");
            SetDifficulty(0);
        }

        #endregion

        #region ===== DANH SÁCH BÀI & CÂU HỎI =====

        private void RefreshLessonList()
        {
            if (lessonListContent == null) return;

            foreach (Transform child in lessonListContent)
            {
                Destroy(child.gameObject);
            }

            List<TeacherLesson> lessons = TeacherStore.GetLessons();

            if (lessonListHeader != null)
            {
                lessonListHeader.text = $"DANH SÁCH BÀI HỌC ({lessons.Count})";
            }

            if (lessons.Count == 0)
            {
                TextMeshProUGUI empty = CreateText(lessonListContent, "Empty", "Chưa có bài học nào.\nBấm “➕ TẠO BÀI MỚI” để bắt đầu.",
                    FontBody, ColorTextDim, TextAlignmentOptions.Center);
                RectTransform rt = empty.rectTransform;
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0f, -10f);
                rt.sizeDelta = new Vector2(-20f, 60f);
                return;
            }

            float y = 0f;
            foreach (var lesson in lessons)
            {
                string lessonId = lesson.lessonId;
                bool isSelected = lessonId == selectedLessonId;

                RectTransform row = CreateRect($"Lesson_{lessonId}", lessonListContent);
                row.anchorMin = new Vector2(0f, 1f);
                row.anchorMax = new Vector2(1f, 1f);
                row.pivot = new Vector2(0.5f, 1f);
                row.anchoredPosition = new Vector2(0f, y);
                row.sizeDelta = new Vector2(-16f, 84f);

                Image rowBg = row.gameObject.AddComponent<Image>();
                rowBg.color = isSelected ? ColorAccentDark : ColorPanelDark;

                Button rowBtn = row.gameObject.AddComponent<Button>();
                rowBtn.targetGraphic = rowBg;
                rowBtn.onClick.AddListener(() => SelectLesson(lessonId));

                TextMeshProUGUI title = CreateText(row, "Title", lesson.title, FontList, ColorText, TextAlignmentOptions.Left);
                RectTransform titleRt = title.rectTransform;
                titleRt.anchorMin = new Vector2(0f, 0f);
                titleRt.anchorMax = new Vector2(1f, 1f);
                titleRt.pivot = new Vector2(0.5f, 0.5f);
                titleRt.offsetMin = new Vector2(12f, 40f);
                titleRt.offsetMax = new Vector2(-12f, -6f);

                TextMeshProUGUI sub = CreateText(row, "Sub",
                    $"{GetChapterTitle(lesson.chapterId)}\n{lesson.questions.Count} câu • {FormatMode(lesson.mode)}",
                    FontListSub, ColorTextDim, TextAlignmentOptions.TopLeft);
                RectTransform subRt = sub.rectTransform;
                subRt.anchorMin = new Vector2(0f, 0f);
                subRt.anchorMax = new Vector2(1f, 1f);
                subRt.pivot = new Vector2(0.5f, 0.5f);
                subRt.offsetMin = new Vector2(12f, 4f);
                subRt.offsetMax = new Vector2(-12f, -34f);

                y -= 90f;
            }

            // Cập nhật chiều cao content để scroll hoạt động
            LayoutElement le = lessonListContent.GetComponent<LayoutElement>();
            if (le == null) le = lessonListContent.gameObject.AddComponent<LayoutElement>();
            le.minHeight = Mathf.Max(100f, -y + 10f);
            le.preferredHeight = le.minHeight;
        }

        private void SelectLesson(string lessonId)
        {
            selectedLessonId = lessonId;
            RefreshLessonList();
            ShowQuestionForm();
            RefreshQuestionList();
            SetStatus($"Đang chọn bài: {TeacherStore.FindLesson(lessonId)?.title}");
        }

        private void RefreshQuestionList()
        {
            if (questionListContent == null) return;

            foreach (Transform child in questionListContent)
            {
                Destroy(child.gameObject);
            }

            TeacherLesson lesson = TeacherStore.FindLesson(selectedLessonId);
            if (lesson == null)
            {
                if (qListHeader != null) qListHeader.text = "CÂU HỎI TRONG BÀI (0)";
                return;
            }

            if (qListHeader != null)
            {
                qListHeader.text = $"CÂU HỎI TRONG BÀI ({lesson.questions.Count})";
            }

            if (lesson.questions.Count == 0)
            {
                TextMeshProUGUI empty = CreateText(questionListContent, "EmptyQ", "Chưa có câu hỏi nào trong bài.\nNhập thông tin bên trái và bấm\n“💾 LƯU CÂU HỎI VÀO BÀI”.", FontBody, ColorTextDim, TextAlignmentOptions.Center);
                RectTransform emptyRt = empty.rectTransform;
                emptyRt.anchorMin = new Vector2(0f, 1f);
                emptyRt.anchorMax = new Vector2(1f, 1f);
                emptyRt.pivot = new Vector2(0.5f, 1f);
                emptyRt.anchoredPosition = new Vector2(0f, -20f);
                emptyRt.sizeDelta = new Vector2(-20f, 80f);
                return;
            }

            float y = 0f;
            for (int i = 0; i < lesson.questions.Count; i++)
            {
                TeacherQuestion q = lesson.questions[i];
                string qId = q.questionId;

                RectTransform row = CreateRect($"Q_{qId}", questionListContent);
                row.anchorMin = new Vector2(0f, 1f);
                row.anchorMax = new Vector2(1f, 1f);
                row.pivot = new Vector2(0.5f, 1f);
                row.anchoredPosition = new Vector2(0f, y);
                row.sizeDelta = new Vector2(-16f, 54f);

                Image rowBg = row.gameObject.AddComponent<Image>();
                rowBg.color = ColorPanelDark;

                TextMeshProUGUI txt = CreateText(row, "Text", $"#{i + 1}. {q.questionText}", FontBody, ColorText, TextAlignmentOptions.TopLeft);
                RectTransform txtRt = txt.rectTransform;
                txtRt.anchorMin = new Vector2(0f, 0f);
                txtRt.anchorMax = new Vector2(1f, 1f);
                txtRt.pivot = new Vector2(0.5f, 0.5f);
                txtRt.offsetMin = new Vector2(10f, 22f);
                txtRt.offsetMax = new Vector2(-70f, -6f);

                string answerLabel = q.mode == "TuLuan" ? "Tự luận" : $"Đúng: {q.correctAnswer}";
                TextMeshProUGUI meta = CreateText(row, "Meta", answerLabel, FontSmall, ColorTextDim, TextAlignmentOptions.BottomLeft);
                RectTransform metaRt = meta.rectTransform;
                metaRt.anchorMin = new Vector2(0f, 0f);
                metaRt.anchorMax = new Vector2(1f, 1f);
                metaRt.pivot = new Vector2(0.5f, 0.5f);
                metaRt.offsetMin = new Vector2(10f, 4f);
                metaRt.offsetMax = new Vector2(-70f, -32f);

                Button delBtn = CreateButton(row, "Btn_Del", "🗑", ColorDanger, 0f, () => DeleteQuestion(qId));
                RectTransform delRt = delBtn.GetComponent<RectTransform>();
                delRt.anchorMin = new Vector2(1f, 0.5f);
                delRt.anchorMax = new Vector2(1f, 0.5f);
                delRt.pivot = new Vector2(1f, 0.5f);
                delRt.anchoredPosition = new Vector2(-6f, 0f);
                delRt.sizeDelta = new Vector2(34f, 30f);

                y -= 60f;
            }

            LayoutElement le = questionListContent.GetComponent<LayoutElement>();
            if (le == null) le = questionListContent.gameObject.AddComponent<LayoutElement>();
            le.minHeight = Mathf.Max(100f, -y + 10f);
            le.preferredHeight = le.minHeight;
        }

        private void DeleteQuestion(string questionId)
        {
            if (TeacherStore.DeleteQuestion(selectedLessonId, questionId))
            {
                SetStatus($"🗑 Đã xoá câu hỏi {questionId}.");
                RefreshQuestionList();
                RefreshLessonList();
            }
        }

        private void DeleteSelectedLesson()
        {
            if (string.IsNullOrEmpty(selectedLessonId))
            {
                SetStatus("⚠️ Chưa chọn bài học nào để xoá.", new Color(1f, 0.75f, 0.3f));
                return;
            }

            TeacherLesson lesson = TeacherStore.FindLesson(selectedLessonId);
            if (TeacherStore.DeleteLesson(selectedLessonId))
            {
                selectedLessonId = "";
                SetStatus($"🗑 Đã xoá bài \"{lesson?.title}\".");
                RefreshLessonList();
                RefreshQuestionList();
            }
        }

        #endregion

        #region ===== TAB KẾT QUẢ =====

        private void BuildResultView(RectTransform rightCol)
        {
            resultRoot = BuildResultViewContent(rightCol);
            resultRoot.gameObject.SetActive(false);
        }

        private Transform BuildResultViewContent(RectTransform parent)
        {
            RectTransform view = CreateRect("ResultView", parent);
            view.anchorMin = Vector2.zero;
            view.anchorMax = Vector2.one;
            view.offsetMin = new Vector2(16f, 16f);
            view.offsetMax = new Vector2(-16f, -16f);

            float y = -10f;

            TextMeshProUGUI header = CreateText(view, "Header", "KẾT QUẢ HỌC SINH", FontSection, ColorText, TextAlignmentOptions.Left);
            SetTopStretch(header.rectTransform, 0f, 0f, -y, 34f);
            y -= 42f;

            resultSummaryText = CreateText(view, "Summary", "", FontSmall, ColorTextDim, TextAlignmentOptions.Left);
            SetTopStretch(resultSummaryText.rectTransform, 0f, 0f, -y, 28f);
            y -= 34f;

            // ScrollView bảng kết quả
            ScrollRect scroll = CreateScrollView(view, "ResultScroll", out Transform contentOut);
            resultListContent = contentOut;

            // Neo đáy: cao = chiều cao cha - lề trên - lề dưới
            RectTransform scrollRt = scroll.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0f, 0f);
            scrollRt.anchorMax = new Vector2(1f, 1f);
            scrollRt.pivot = new Vector2(0.5f, 0.5f);
            scrollRt.offsetMin = new Vector2(0f, 150f);
            scrollRt.offsetMax = new Vector2(0f, -y);

            // Ô cấu hình Google Sheet
            RectTransform cfg = CreateRect("SheetConfig", view);
            cfg.anchorMin = new Vector2(0f, 0f);
            cfg.anchorMax = new Vector2(1f, 0f);
            cfg.pivot = new Vector2(0.5f, 0f);
            cfg.offsetMin = new Vector2(0f, 0f);
            cfg.offsetMax = new Vector2(0f, 140f);

            TextMeshProUGUI cfgLabel = CreateText(cfg, "CfgLabel", "LINK GOOGLE SHEET (Apps Script webhook - tuỳ chọn)", FontSmall, ColorTextDim, TextAlignmentOptions.Left);
            SetTopStretch(cfgLabel.rectTransform, 0f, 0f, 0f, 24f);

            inputSheetUrl = CreateInput(cfg, "InputSheetUrl", "https://script.google.com/macros/s/.../exec", false, 38f);
            RectTransform urlRt = inputSheetUrl.GetComponent<RectTransform>();
            urlRt.anchorMin = new Vector2(0f, 1f);
            urlRt.anchorMax = new Vector2(0.72f, 1f);
            urlRt.pivot = new Vector2(0f, 1f);
            urlRt.anchoredPosition = new Vector2(0f, -28f);
            urlRt.sizeDelta = new Vector2(-8f, 38f);

            Button btnSaveUrl = CreateButton(cfg, "Btn_SaveUrl", "💾 LƯU", ColorAccent, 0f, () => SaveSheetUrl());
            RectTransform saveUrlRt = btnSaveUrl.GetComponent<RectTransform>();
            saveUrlRt.anchorMin = new Vector2(0.74f, 1f);
            saveUrlRt.anchorMax = new Vector2(0.87f, 1f);
            saveUrlRt.pivot = new Vector2(0f, 1f);
            saveUrlRt.anchoredPosition = new Vector2(0f, -28f);
            saveUrlRt.sizeDelta = new Vector2(-8f, 38f);

            Button btnOpenExcel = CreateButton(cfg, "Btn_OpenExcel", "📁 XEM FILE EXCEL", ColorWarning, 0f, () => OpenExcelFile());
            RectTransform openRt = btnOpenExcel.GetComponent<RectTransform>();
            openRt.anchorMin = new Vector2(0.89f, 1f);
            openRt.anchorMax = new Vector2(1f, 1f);
            openRt.pivot = new Vector2(0f, 1f);
            openRt.anchoredPosition = new Vector2(0f, -28f);
            openRt.sizeDelta = new Vector2(-8f, 38f);

            Button btnClear = CreateButton(cfg, "Btn_ClearResults", "🗑 XOÁ KẾT QUẢ", ColorDanger, 0f, () => ClearResults());
            RectTransform clearRt = btnClear.GetComponent<RectTransform>();
            clearRt.anchorMin = new Vector2(0f, 0f);
            clearRt.anchorMax = new Vector2(0.45f, 0f);
            clearRt.pivot = new Vector2(0f, 0f);
            clearRt.anchoredPosition = new Vector2(0f, 10f);
            clearRt.sizeDelta = new Vector2(-10f, 40f);

            Button btnRefresh = CreateButton(cfg, "Btn_RefreshResults", "LÀM MỚI", ColorInfo, 0f, () => RefreshResultList());
            RectTransform refreshRt = btnRefresh.GetComponent<RectTransform>();
            refreshRt.anchorMin = new Vector2(0.47f, 0f);
            refreshRt.anchorMax = new Vector2(1f, 0f);
            refreshRt.pivot = new Vector2(0f, 0f);
            refreshRt.anchoredPosition = new Vector2(0f, 10f);
            refreshRt.sizeDelta = new Vector2(-10f, 40f);

            return view;
        }

        private void ToggleResultsView()
        {
            showingResults = !showingResults;
            if (showingResults)
            {
                // Ẩn cả form thêm câu hỏi lẫn danh sách câu hỏi của bài đang chọn
                if (formBoxRt != null) formBoxRt.gameObject.SetActive(false);
                if (qListBoxRt != null) qListBoxRt.gameObject.SetActive(false);
                if (resultRoot != null) resultRoot.gameObject.SetActive(true);
                RefreshResultList();
                SetStatus("Đang xem bảng kết quả học sinh.");
            }
            else
            {
                if (resultRoot != null) resultRoot.gameObject.SetActive(false);
                if (formBoxRt != null) formBoxRt.gameObject.SetActive(true);
                if (qListBoxRt != null) qListBoxRt.gameObject.SetActive(true);
                SetStatus("Đã quay lại màn thêm câu hỏi.");
            }
        }

        /// <summary>
        /// Bật/tắt panel liệt kê câu hỏi của bài đang chọn.
        /// </summary>
        private void SetQuestionListVisible(bool visible)
        {
            if (qListBoxRt != null) qListBoxRt.gameObject.SetActive(visible);
        }

        private void RefreshResultList()
        {
            if (resultListContent == null) return;

            foreach (Transform child in resultListContent)
            {
                Destroy(child.gameObject);
            }

            List<ScoreRecord> history = ScoreRecorder.LoadHistory();

            if (resultSummaryText != null)
            {
                int best = 0;
                string bestUser = "-";
                foreach (var rec in history)
                {
                    if (rec.score > best)
                    {
                        best = rec.score;
                        bestUser = rec.username;
                    }
                }
                resultSummaryText.text = history.Count == 0
                    ? "Chưa có kết quả nào. Học sinh sẽ tự động ghi điểm khi chơi xong."
                    : $"Tổng số lượt thi: {history.Count}   •   Điểm cao nhất: {best} ({bestUser})";
            }

            float y = 0f;
            int limit = Mathf.Min(history.Count, 200);

            for (int i = 0; i < limit; i++)
            {
                ScoreRecord rec = history[i];

                RectTransform row = CreateRect($"Rec_{i}", resultListContent);
                row.anchorMin = new Vector2(0f, 1f);
                row.anchorMax = new Vector2(1f, 1f);
                row.pivot = new Vector2(0.5f, 1f);
                row.anchoredPosition = new Vector2(0f, y);
                row.sizeDelta = new Vector2(-16f, 52f);

                Image rowBg = row.gameObject.AddComponent<Image>();
                rowBg.color = (i % 2 == 0) ? ColorPanelDark : new Color(0.16f, 0.20f, 0.18f, 1f);

                TextMeshProUGUI main = CreateText(row, "Main",
                    $"{rec.timestamp}   •   {rec.username}   •   {rec.lesson}\n" +
                    $"{rec.mode}   •   Đúng {rec.correctCount}/{rec.totalQuestions}   •   Điểm: {rec.score}   •   Kỷ lục: {rec.highScore}   •   Boss: {rec.bossesDefeated}   •   {rec.accuracyPercent:0.0}%",
                    12f, ColorText, TextAlignmentOptions.TopLeft);
                RectTransform mainRt = main.rectTransform;
                mainRt.anchorMin = new Vector2(0f, 0f);
                mainRt.anchorMax = new Vector2(1f, 1f);
                mainRt.pivot = new Vector2(0.5f, 0.5f);
                mainRt.offsetMin = new Vector2(10f, 4f);
                mainRt.offsetMax = new Vector2(-10f, -4f);

                y -= 58f;
            }

            LayoutElement le = resultListContent.GetComponent<LayoutElement>();
            if (le == null) le = resultListContent.gameObject.AddComponent<LayoutElement>();
            le.minHeight = Mathf.Max(100f, -y + 10f);
            le.preferredHeight = le.minHeight;
        }

        private void SaveSheetUrl()
        {
            string url = inputSheetUrl != null ? inputSheetUrl.text.Trim() : "";
            ScoreRecorder.SetSheetUrl(url);
            SetStatus(string.IsNullOrEmpty(url)
                ? "Đã xoá cấu hình Google Sheet."
                : "✅ Đã lưu link Google Sheet. Điểm sẽ được đồng bộ lên sheet này.");
        }

        private void OpenExcelFile()
        {
            try
            {
                List<ScoreRecord> history = ScoreRecorder.LoadHistory();
                ScoreRecorder.WriteExcelFromHistory(history);

                string path = ScoreRecorder.ExcelFilePath;
                if (!System.IO.File.Exists(path))
                {
                    SetStatus("⚠️ Chưa có file kết quả. Hãy cho học sinh chơi ít nhất 1 lượt.", new Color(1f, 0.75f, 0.3f));
                    return;
                }

                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + path + "\"");
                SetStatus($"📁 Đã mở thư mục chứa file: {path}");
            }
            catch (Exception e)
            {
                SetStatus($"⚠️ Không mở được file: {e.Message}", Color.red);
            }
        }

        private void ClearResults()
        {
            ScoreRecorder.ClearHistory();
            RefreshResultList();
            SetStatus("🗑 Đã xoá toàn bộ kết quả.");
        }

        private void ExportQuestionsCsv()
        {
            if (TeacherStore.ExportCsv())
            {
                SetStatus($"⬇ Đã xuất câu hỏi ra CSV: {TeacherStore.CsvFilePath}");
            }
            else
            {
                SetStatus("⚠️ Không xuất được CSV câu hỏi.", Color.red);
            }
        }

        #endregion

        #region ===== ĐIỀU HƯỚNG =====

        private void ShowLessonsTab()
        {
            ShowQuestionForm();
            if (resultRoot != null) resultRoot.gameObject.SetActive(false);
            showingResults = false;
            RefreshLessonList();
            RefreshQuestionList();
        }

        /// <summary>
        /// Hiện form thêm câu hỏi (ẩn tab kết quả nếu đang mở).
        /// </summary>
        private void ShowQuestionForm()
        {
            if (formBoxRt != null) formBoxRt.gameObject.SetActive(true);
            if (qListBoxRt != null) qListBoxRt.gameObject.SetActive(true);
            if (resultRoot != null) resultRoot.gameObject.SetActive(false);
            showingResults = false;
        }

        /// <summary>
        /// Quay lại màn CHỌN MÔN. Không logout, không thoát game.
        /// </summary>
        private void ExitToSubjectSelect()
        {
            TeacherNavigation.GoToSubjectSelect();
        }

        #endregion

        #region ===== HELPER UI =====

        private RectTransform CreateRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, string text, float fontSize, Color color, TextAlignmentOptions align)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = align;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Truncate;

            if (TMP_Settings.defaultFontAsset != null)
            {
                tmp.font = TMP_Settings.defaultFontAsset;
            }

            return tmp;
        }

        private Button CreateButton(Transform parent, string name, string label, Color color, float width, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            Image img = go.GetComponent<Image>();
            img.color = color;

            Button btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(onClick);

            // Kích thước mặc định: nếu không set sau đó thì nút vẫn bấm được & nhìn thấy.
            // (RectTransform mặc định có sizeDelta = 0 nên nút sẽ vô hình nếu bị bỏ sót.)
            RectTransform btnRt = go.GetComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(width > 0f ? width : 160f, 40f);

            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(go.transform, false);
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            Stretch(textRt);
            textRt.offsetMin = new Vector2(6f, 2f);
            textRt.offsetMax = new Vector2(-6f, -2f);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = FontButton;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = ColorText;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            if (TMP_Settings.defaultFontAsset != null)
            {
                tmp.font = TMP_Settings.defaultFontAsset;
            }

            return btn;
        }

        private TMP_InputField CreateInput(Transform parent, string name, string placeholder, bool multiline, float height)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            go.transform.SetParent(parent, false);

            Image img = go.GetComponent<Image>();
            img.color = ColorInputBg;

            // Text Area
            GameObject areaObj = new GameObject("Text Area", typeof(RectTransform));
            areaObj.transform.SetParent(go.transform, false);
            RectTransform areaRt = areaObj.GetComponent<RectTransform>();
            Stretch(areaRt);
            areaRt.offsetMin = new Vector2(10f, 6f);
            areaRt.offsetMax = new Vector2(-10f, -6f);
            areaObj.AddComponent<RectMask2D>();

            // Text
            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(areaObj.transform, false);
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            Stretch(textRt);

            TextMeshProUGUI textTmp = textObj.AddComponent<TextMeshProUGUI>();
            textTmp.text = "";
            textTmp.fontSize = FontBody;
            textTmp.color = ColorInputText;
            textTmp.alignment = multiline ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.Left;
            textTmp.raycastTarget = false;
            textTmp.textWrappingMode = multiline ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            textTmp.overflowMode = multiline ? TextOverflowModes.Truncate : TextOverflowModes.Overflow;
            if (TMP_Settings.defaultFontAsset != null)
            {
                textTmp.font = TMP_Settings.defaultFontAsset;
            }

            // Placeholder
            GameObject phObj = new GameObject("Placeholder", typeof(RectTransform));
            phObj.transform.SetParent(areaObj.transform, false);
            RectTransform phRt = phObj.GetComponent<RectTransform>();
            Stretch(phRt);

            TextMeshProUGUI phTmp = phObj.AddComponent<TextMeshProUGUI>();
            phTmp.text = placeholder;
            phTmp.fontSize = FontBody;
            phTmp.color = new Color(0.45f, 0.45f, 0.45f, 1f);
            phTmp.alignment = multiline ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.Left;
            phTmp.raycastTarget = false;
            phTmp.textWrappingMode = multiline ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            phTmp.overflowMode = TextOverflowModes.Truncate;
            if (TMP_Settings.defaultFontAsset != null)
            {
                phTmp.font = TMP_Settings.defaultFontAsset;
            }

            TMP_InputField input = go.GetComponent<TMP_InputField>();
            input.targetGraphic = img;
            input.textViewport = areaRt;
            input.textComponent = textTmp;
            input.placeholder = phTmp;
            input.lineType = multiline ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;
            input.characterLimit = 0;

            return input;
        }

        /// <summary>
        /// Neo neo theo cạnh trên, giãn ngang theo cha, cao cố định.
        ///
        /// Lưu ý: KHÔNG dùng offsetMin/offsetMax cho các phần tử có neo Y bằng nhau,
        /// vì Unity tính chiều cao = (anchorMax.y - anchorMin.y) * cha + (offsetMax.y - offsetMin.y),
        /// nên nếu đặt offsetMin.y = offsetMax.y = 0 thì chiều cao = 0 và phần tử sẽ biến mất.
        /// Thay vào đó, lề trái/phải được gộp vào sizeDelta.
        /// </summary>
        private void SetTopStretch(RectTransform rt, float left, float right, float topOffset, float height)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(-(left + right), height);
            rt.anchoredPosition = new Vector2((left - right) * 0.5f, -topOffset);
        }

        /// <summary>
        /// Neo giãn đủ 4 cạnh trong cha, với lề (inset) cho từng cạnh.
        /// Dùng cho các panel lớn, ở đây offsetMin/offsetMax là cách đúng.
        /// </summary>
        private void SetStretchInset(RectTransform rt, float left, float bottom, float right, float top)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        private void CreateLabel(Transform parent, string name, string text, float fontSize, float height)
        {
            TextMeshProUGUI label = CreateText(parent, name, text, fontSize, ColorTextDim, TextAlignmentOptions.Left);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            AddToLayout(label.rectTransform, height);
        }

        private void PlaceInput(TMP_InputField input, ref float y, float height)
        {
            RectTransform rt = input.GetComponent<RectTransform>();
            SetTopStretch(rt, 12f, 12f, -y, height);
            y -= (height + 6f);
        }

        private void PlaceMeta(TMP_InputField input, float anchorX, float y, float height)
        {
            RectTransform rt = input.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(anchorX, 1f);
            rt.anchorMax = new Vector2(anchorX + 0.32f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, height);
            rt.anchoredPosition = new Vector2(0f, -y);
        }

        private void PlaceSideBySide(Button a, Button b, float y, float height)
        {
            RectTransform rtA = a.GetComponent<RectTransform>();
            rtA.anchorMin = new Vector2(0f, 1f);
            rtA.anchorMax = new Vector2(0.5f, 1f);
            rtA.pivot = new Vector2(0.5f, 1f);
            rtA.sizeDelta = new Vector2(-14f, height);
            rtA.anchoredPosition = new Vector2(4f, -y);

            RectTransform rtB = b.GetComponent<RectTransform>();
            rtB.anchorMin = new Vector2(0.5f, 1f);
            rtB.anchorMax = new Vector2(1f, 1f);
            rtB.pivot = new Vector2(0.5f, 1f);
            rtB.sizeDelta = new Vector2(-14f, height);
            rtB.anchoredPosition = new Vector2(-4f, -y);
        }

        private ScrollRect CreateScrollView(Transform parent, string name, out Transform content, bool bottom = false)
        {
            GameObject scrollObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollObj.transform.SetParent(parent, false);

            Image bg = scrollObj.GetComponent<Image>();
            bg.color = new Color(0.11f, 0.15f, 0.13f, 1f);

            RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
            if (bottom)
            {
                // Neo đáy: chiều cao lấy từ sizeDelta (đừng dùng offset theo Y)
                scrollRt.anchorMin = new Vector2(0f, 0f);
                scrollRt.anchorMax = new Vector2(1f, 0f);
                scrollRt.pivot = new Vector2(0.5f, 0f);
                scrollRt.anchoredPosition = Vector2.zero;
                scrollRt.sizeDelta = new Vector2(-28f, 400f);
            }
            else
            {
                // Neo đáy + giãn lên, dùng offset để chiều cao = cha - lề trên - lề dưới
                scrollRt.anchorMin = new Vector2(0f, 0f);
                scrollRt.anchorMax = new Vector2(1f, 1f);
                scrollRt.pivot = new Vector2(0.5f, 0.5f);
                scrollRt.offsetMin = new Vector2(14f, 14f);
                scrollRt.offsetMax = new Vector2(-14f, -116f);
            }

            // Viewport
            GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform));
            viewportObj.transform.SetParent(scrollObj.transform, false);
            RectTransform viewportRt = viewportObj.GetComponent<RectTransform>();
            Stretch(viewportRt);
            viewportObj.AddComponent<RectMask2D>();

            // Content
            GameObject contentObj = new GameObject("Content", typeof(RectTransform));
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRt = contentObj.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0f, 0f);

            // Gắt ScrollRect
            ScrollRect scroll = scrollObj.GetComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.viewport = viewportRt;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;

            content = contentRt;
            return scroll;
        }

        private void SetStatus(string message)
        {
            SetStatus(message, ColorTextDim);
        }

        private void SetStatus(string message, Color color)
        {
            if (statusText == null) return;
            statusText.text = message;
            statusText.color = color;
        }

        private static string GetChapterTitle(string chapterId)
        {
            foreach (var ch in BiologyChapters)
            {
                if (ch.id == chapterId) return ch.title;
            }
            return chapterId;
        }

        private static string FormatMode(string mode)
        {
            return mode == "TuLuan" ? "Tự luận" : "Trắc nghiệm";
        }

        #endregion
    }
}
