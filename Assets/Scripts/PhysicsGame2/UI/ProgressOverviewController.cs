using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Controller điều khiển màn TỔNG QUAN TIẾN ĐỘ HỌC TẬP (Phương án A).
    /// Hiển thị dữ liệu học tập thật từ PhysicsSaveData.quizHistory:
    /// - Thống kê tổng quan: Tổng bài đã luyện, tổng lượt hoàn thành, điểm TB, điểm cao nhất, ngày học gần nhất, tỷ lệ hoàn thành.
    /// - Bộ lọc: Chương (1-5), Mức độ (NB, TH, VD, VDC), Hình thức (Trắc nghiệm).
    /// - Danh sách bài học theo chương trình Vật lý 9 với số lượt, điểm gần nhất, điểm cao nhất, trạng thái ĐÃ HỌC / CHƯA HỌC.
    /// - Nút "Xem chi tiết" -> Chuyển sang Scene 24 (Tiến bộ học tập) kèm đúng bộ lọc.
    /// - Nút "Quay lại" -> Chuyển về Main Menu Aurelia.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProgressOverviewController : MonoBehaviour
    {
        [Header("Thống kê tổng quan (Cột trái)")]
        [SerializeField] private TextMeshProUGUI txtTotalLessons;
        [SerializeField] private TextMeshProUGUI txtTotalAttempts;
        [SerializeField] private TextMeshProUGUI txtAvgScore;
        [SerializeField] private TextMeshProUGUI txtMaxScore;
        [SerializeField] private TextMeshProUGUI txtLastDate;
        [SerializeField] private TextMeshProUGUI txtCompletionRate;

        [Header("Bộ lọc (Cột phải)")]
        [SerializeField] private TextMeshProUGUI txtChapterTitle;
        [SerializeField] private Button btnPrevChapter;
        [SerializeField] private Button btnNextChapter;
        [SerializeField] private TextMeshProUGUI txtDifficultyTitle;
        [SerializeField] private Button btnPrevDifficulty;
        [SerializeField] private Button btnNextDifficulty;
        [SerializeField] private TextMeshProUGUI txtModeTitle;

        [Header("Danh sách bài học")]
        [SerializeField] private Transform lessonListContainer;
        [SerializeField] private List<LessonOverviewRowUI> lessonRows = new List<LessonOverviewRowUI>();

        [Header("Điều hướng")]
        [SerializeField] private Button btnBack;
        [SerializeField] private Button btnViewDetail;

        private static readonly string[] Difficulties = { "NB", "TH", "VD", "VDC" };
        private static readonly string[] DifficultyNames = { "Nhận biết", "Thông hiểu", "Vận dụng", "Vận dụng cao" };
        private static readonly string[] Modes = { "", "MCQ", "ESSAY" };
        private static readonly string[] ModeNames = { "Tất cả hình thức", "Trắc nghiệm", "Tự luận" };
        private int _selectedModeIndex = 0; // 0 = Tất cả hình thức
        private const int TotalCurriculumLessons = 17;

        private int _selectedChapter = 1;
        private int _selectedDifficultyIndex = 0; // 0 = NB
        private string _selectedLessonId = "C1_L2";
        private string _selectedLessonTitle = "Động năng. Thế năng";

        public int SelectedChapter => _selectedChapter;
        public string SelectedDifficulty => Difficulties[_selectedDifficultyIndex];
        public string SelectedLessonId => _selectedLessonId;
        public string SelectedLessonTitle => _selectedLessonTitle;
        public string SelectedMode => Modes[_selectedModeIndex];

        private void Awake()
        {
            EnsureFantasyUI();
            AutoWireUIReferences();
            BindButtonEvents();
        }

        private void Start()
        {
            InitFiltersFromSaveOrSession();
            RefreshOverview();
        }

        public void AutoWireUIReferences()
        {
            var allTmp = GetComponentsInChildren<TextMeshProUGUI>(true);
            var allButtons = GetComponentsInChildren<Button>(true);

            if (txtTotalLessons == null) txtTotalLessons = FindTmpByName(allTmp, "Txt_TotalLessons");
            if (txtTotalAttempts == null) txtTotalAttempts = FindTmpByName(allTmp, "Txt_TotalAttempts");
            if (txtAvgScore == null) txtAvgScore = FindTmpByName(allTmp, "Txt_AvgScore");
            if (txtMaxScore == null) txtMaxScore = FindTmpByName(allTmp, "Txt_MaxScore");
            if (txtLastDate == null) txtLastDate = FindTmpByName(allTmp, "Txt_LastDate");
            if (txtCompletionRate == null) txtCompletionRate = FindTmpByName(allTmp, "Txt_CompletionRate");

            if (txtChapterTitle == null) txtChapterTitle = FindTmpByName(allTmp, "Txt_ChapterTitle");
            if (txtDifficultyTitle == null) txtDifficultyTitle = FindTmpByName(allTmp, "Txt_DifficultyTitle");
            if (txtModeTitle == null) txtModeTitle = FindTmpByName(allTmp, "Txt_ModeTitle");

            if (btnPrevChapter == null) btnPrevChapter = FindButtonByName(allButtons, "Btn_PrevChapter");
            if (btnNextChapter == null) btnNextChapter = FindButtonByName(allButtons, "Btn_NextChapter");
            if (btnPrevDifficulty == null) btnPrevDifficulty = FindButtonByName(allButtons, "Btn_PrevDifficulty");
            if (btnNextDifficulty == null) btnNextDifficulty = FindButtonByName(allButtons, "Btn_NextDifficulty");

            if (btnBack == null) btnBack = FindButtonByName(allButtons, "Btn_Back");
            if (btnViewDetail == null) btnViewDetail = FindButtonByName(allButtons, "Btn_ViewDetail");

            if (lessonRows.Count == 0)
            {
                var rows = GetComponentsInChildren<LessonOverviewRowUI>(true);
                if (rows != null && rows.Length > 0)
                {
                    lessonRows.AddRange(rows);
                }
            }
        }

        private TextMeshProUGUI FindTmpByName(TextMeshProUGUI[] list, string name)
        {
            return list.FirstOrDefault(t => t != null && t.gameObject.name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private Button FindButtonByName(Button[] list, string name)
        {
            return list.FirstOrDefault(b => b != null && b.gameObject.name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private void BindButtonEvents()
        {
            if (btnPrevChapter != null)
            {
                btnPrevChapter.onClick.RemoveAllListeners();
                btnPrevChapter.onClick.AddListener(OnPrevChapterClicked);
            }
            if (btnNextChapter != null)
            {
                btnNextChapter.onClick.RemoveAllListeners();
                btnNextChapter.onClick.AddListener(OnNextChapterClicked);
            }
            if (btnPrevDifficulty != null)
            {
                btnPrevDifficulty.onClick.RemoveAllListeners();
                btnPrevDifficulty.onClick.AddListener(OnPrevDifficultyClicked);
            }
            if (btnNextDifficulty != null)
            {
                btnNextDifficulty.onClick.RemoveAllListeners();
                btnNextDifficulty.onClick.AddListener(OnNextDifficultyClicked);
            }
            if (btnBack != null)
            {
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackClicked);
            }
            if (btnViewDetail != null)
            {
                btnViewDetail.onClick.RemoveAllListeners();
                btnViewDetail.onClick.AddListener(OnViewDetailClicked);
            }
        }

        private void EnsureFantasyUI()
        {
            var canvas = GetComponentInParent<Canvas>() ?? GetComponent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Nếu đã có panel LeftPanel_Stats thì UI đã được dựng sẵn
            var existingLeftPanel = canvas.transform.Find("LeftPanel_Stats");
            if (existingLeftPanel != null)
            {
                return;
            }

            // Thu thập các sprite và font sẵn có trên Scene
            Sprite cardFrameSprite = null;
            Sprite mainBoardSprite = null;
            Sprite actionBtnSprite = null;
            Sprite backBtnSprite = null;
            Sprite gemSprite = null;
            Sprite titleBannerSprite = null;
            TMP_FontAsset font = null;

            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
            {
                if (img == null || img.sprite == null) continue;
                string sName = img.sprite.name;
                if (sName.Contains("_14") && cardFrameSprite == null) cardFrameSprite = img.sprite;
                else if (sName.Contains("tiến độ xanh vàng_0") && mainBoardSprite == null) mainBoardSprite = img.sprite;
                else if (sName.Contains("_16") && actionBtnSprite == null) actionBtnSprite = img.sprite;
                else if ((sName.Contains("bảng xếp hạng") || sName.Contains("1")) && backBtnSprite == null) backBtnSprite = img.sprite;
                else if ((sName.Contains("_22") || sName.Contains("_21")) && gemSprite == null) gemSprite = img.sprite;
                else if (sName.Contains("tiến độ xanh vàng 1_0") && titleBannerSprite == null) titleBannerSprite = img.sprite;
            }

            // Fallback lấy sprite bất kỳ từ các Image có sẵn
            var allImgs = canvas.GetComponentsInChildren<Image>(true);
            if (cardFrameSprite == null && allImgs.Length > 0) cardFrameSprite = allImgs[0].sprite;
            if (mainBoardSprite == null) mainBoardSprite = cardFrameSprite;
            if (actionBtnSprite == null) actionBtnSprite = cardFrameSprite;
            if (backBtnSprite == null) backBtnSprite = cardFrameSprite;
            if (gemSprite == null) gemSprite = cardFrameSprite;
            if (titleBannerSprite == null) titleBannerSprite = cardFrameSprite;

            var existingTmp = canvas.GetComponentInChildren<TextMeshProUGUI>(true);
            if (existingTmp != null) font = existingTmp.font;

            // Ẩn các đối tượng cũ của Scene 24 (khung so sánh, progress bar...)
            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                var child = canvas.transform.GetChild(i);
                string cName = child.name;
                if (cName.Contains("Image (14)") || cName.Contains("Image (10)") ||
                    cName.Contains("Button") || cName.Contains("Image (18)") ||
                    cName.Contains("Image (8)") || cName.Contains("Image (20)") ||
                    cName.Contains("Text (TMP)"))
                {
                    child.gameObject.SetActive(false);
                }
            }

            // TopBar & Title
            var titleBannerGo = new GameObject("HeaderBanner");
            titleBannerGo.transform.SetParent(canvas.transform, false);
            var titleBannerRt = titleBannerGo.AddComponent<RectTransform>();
            titleBannerRt.anchoredPosition = new Vector2(0, 440f);
            titleBannerRt.sizeDelta = new Vector2(850f, 130f);
            var titleBannerImg = titleBannerGo.AddComponent<Image>();
            titleBannerImg.sprite = titleBannerSprite;
            titleBannerImg.type = Image.Type.Simple;
            titleBannerImg.raycastTarget = false;

            var titleTmp = CreateTmp(titleBannerGo, "Txt_Title", "TỔNG QUAN TIẾN ĐỘ HỌC TẬP", 32f, new Color(1f, 0.92f, 0.65f), font, TextAlignmentOptions.Center, true);
            titleTmp.rectTransform.anchoredPosition = new Vector2(0, 5f);
            titleTmp.rectTransform.sizeDelta = new Vector2(750f, 50f);

            // Nút Quay lại
            var backBtnGo = new GameObject("Btn_Back");
            backBtnGo.transform.SetParent(canvas.transform, false);
            var backBtnRt = backBtnGo.AddComponent<RectTransform>();
            backBtnRt.anchoredPosition = new Vector2(-780f, 440f);
            backBtnRt.sizeDelta = new Vector2(190f, 56f);
            var backBtnImg = backBtnGo.AddComponent<Image>();
            backBtnImg.sprite = backBtnSprite;
            backBtnImg.type = Image.Type.Sliced;
            backBtnImg.raycastTarget = true;
            btnBack = backBtnGo.AddComponent<Button>();

            var backText = CreateTmp(backBtnGo, "Txt_Back", "< Quay lại", 22f, Color.white, font, TextAlignmentOptions.Center, true);
            StretchFull(backText.rectTransform);

            // Cột Trái - THỐNG KÊ TỔNG QUAN
            var leftPanelGo = new GameObject("LeftPanel_Stats");
            leftPanelGo.transform.SetParent(canvas.transform, false);
            var leftPanelRt = leftPanelGo.AddComponent<RectTransform>();
            leftPanelRt.anchoredPosition = new Vector2(-540f, -50f);
            leftPanelRt.sizeDelta = new Vector2(560f, 740f);
            var leftPanelImg = leftPanelGo.AddComponent<Image>();
            leftPanelImg.sprite = mainBoardSprite;
            leftPanelImg.type = Image.Type.Sliced;
            leftPanelImg.color = new Color(0.9f, 0.95f, 1f, 0.95f);

            var statsHeaderTmp = CreateTmp(leftPanelGo, "Txt_StatsHeader", "THỐNG KÊ TỔNG QUAN", 24f, new Color(1f, 0.84f, 0.2f), font, TextAlignmentOptions.Center, true);
            statsHeaderTmp.rectTransform.anchoredPosition = new Vector2(0, 310f);
            statsHeaderTmp.rectTransform.sizeDelta = new Vector2(480f, 40f);

            string[] cardLabels = {
                "Tổng số bài đã luyện",
                "Tổng lượt hoàn thành",
                "Điểm trung bình",
                "Điểm cao nhất",
                "Lượt học gần nhất",
                "Tỷ lệ hoàn thành"
            };
            string[] cardValNames = {
                "Txt_TotalLessons",
                "Txt_TotalAttempts",
                "Txt_AvgScore",
                "Txt_MaxScore",
                "Txt_LastDate",
                "Txt_CompletionRate"
            };
            string[] defaultValues = { "0 bài", "0 lượt", "—", "—", "Chưa có", "0%" };
            Color[] valueColors = {
                new Color(1f, 0.92f, 0.65f),
                new Color(1f, 0.92f, 0.65f),
                new Color(1f, 0.85f, 0.35f),
                new Color(1f, 0.85f, 0.35f),
                new Color(0.6f, 0.9f, 1f),
                new Color(0.45f, 0.95f, 0.5f)
            };

            float startY = 225f;
            float stepY = 88f;
            for (int i = 0; i < 6; i++)
            {
                var cardGo = new GameObject($"Card_{i+1}");
                cardGo.transform.SetParent(leftPanelGo.transform, false);
                var cardRt = cardGo.AddComponent<RectTransform>();
                cardRt.anchoredPosition = new Vector2(0, startY - i * stepY);
                cardRt.sizeDelta = new Vector2(490f, 74f);
                var cardImg = cardGo.AddComponent<Image>();
                cardImg.sprite = cardFrameSprite;
                cardImg.type = Image.Type.Sliced;
                cardImg.color = new Color(0.95f, 0.95f, 1f, 0.95f);

                var lblTmp = CreateTmp(cardGo, "Label", cardLabels[i], 18f, new Color(0.88f, 0.85f, 0.78f), font, TextAlignmentOptions.Left);
                lblTmp.rectTransform.anchoredPosition = new Vector2(-65f, 0);
                lblTmp.rectTransform.sizeDelta = new Vector2(300f, 36f);

                var valTmp = CreateTmp(cardGo, cardValNames[i], defaultValues[i], i == 4 ? 17f : 22f, valueColors[i], font, TextAlignmentOptions.Right, true);
                valTmp.rectTransform.anchoredPosition = new Vector2(155f, 0);
                valTmp.rectTransform.sizeDelta = new Vector2(150f, 36f);

                if (i == 0) txtTotalLessons = valTmp;
                else if (i == 1) txtTotalAttempts = valTmp;
                else if (i == 2) txtAvgScore = valTmp;
                else if (i == 3) txtMaxScore = valTmp;
                else if (i == 4) txtLastDate = valTmp;
                else if (i == 5) txtCompletionRate = valTmp;
            }

            // Cột Phải - DANH SÁCH BÀI HỌC & BỘ LỌC
            var rightPanelGo = new GameObject("RightPanel_Lessons");
            rightPanelGo.transform.SetParent(canvas.transform, false);
            var rightPanelRt = rightPanelGo.AddComponent<RectTransform>();
            rightPanelRt.anchoredPosition = new Vector2(300f, -50f);
            rightPanelRt.sizeDelta = new Vector2(1080f, 740f);
            var rightPanelImg = rightPanelGo.AddComponent<Image>();
            rightPanelImg.sprite = mainBoardSprite;
            rightPanelImg.type = Image.Type.Sliced;
            rightPanelImg.color = new Color(0.9f, 0.95f, 1f, 0.95f);

            // Filter Bar
            var filterBarGo = new GameObject("FilterBar");
            filterBarGo.transform.SetParent(rightPanelGo.transform, false);
            var filterBarRt = filterBarGo.AddComponent<RectTransform>();
            filterBarRt.anchoredPosition = new Vector2(0, 310f);
            filterBarRt.sizeDelta = new Vector2(1020f, 54f);

            btnPrevChapter = CreateButton(filterBarGo, "Btn_PrevChapter", new Vector2(-440f, 0), new Vector2(42f, 42f), backBtnSprite, "◀", font, 18f);
            var frameCh = CreateFrameWithText(filterBarGo, "Frame_Chapter", "Txt_ChapterTitle", new Vector2(-265f, 0), new Vector2(300f, 46f), cardFrameSprite, "Chương I: Năng lượng cơ học", font, 17f, new Color(1f, 0.92f, 0.65f));
            txtChapterTitle = frameCh.GetComponentInChildren<TextMeshProUGUI>();
            btnNextChapter = CreateButton(filterBarGo, "Btn_NextChapter", new Vector2(-90f, 0), new Vector2(42f, 42f), backBtnSprite, "▶", font, 18f);

            btnPrevDifficulty = CreateButton(filterBarGo, "Btn_PrevDifficulty", new Vector2(-15f, 0), new Vector2(42f, 42f), backBtnSprite, "◀", font, 18f);
            var frameDiff = CreateFrameWithText(filterBarGo, "Frame_Difficulty", "Txt_DifficultyTitle", new Vector2(120f, 0), new Vector2(215f, 46f), cardFrameSprite, "Nhận biết", font, 17f, new Color(1f, 0.92f, 0.65f));
            txtDifficultyTitle = frameDiff.GetComponentInChildren<TextMeshProUGUI>();
            btnNextDifficulty = CreateButton(filterBarGo, "Btn_NextDifficulty", new Vector2(250f, 0), new Vector2(42f, 42f), backBtnSprite, "▶", font, 18f);

            var frameMode = CreateFrameWithText(filterBarGo, "Frame_Mode", "Txt_ModeTitle", new Vector2(385f, 0), new Vector2(210f, 46f), cardFrameSprite, "Tất cả hình thức", font, 16f, new Color(0.6f, 0.9f, 1f));
            txtModeTitle = frameMode.GetComponentInChildren<TextMeshProUGUI>();
            var modeImg = frameMode.GetComponent<Image>();
            if (modeImg != null) modeImg.raycastTarget = true;
            var modeBtn = frameMode.AddComponent<Button>();
            modeBtn.onClick.AddListener(OnNextModeClicked);

            // Lesson List Container
            var listContainerGo = new GameObject("LessonContainer");
            listContainerGo.transform.SetParent(rightPanelGo.transform, false);
            var listContainerRt = listContainerGo.AddComponent<RectTransform>();
            listContainerRt.anchoredPosition = new Vector2(0, 20f);
            listContainerRt.sizeDelta = new Vector2(1020f, 480f);
            lessonListContainer = listContainerRt;

            lessonRows.Clear();
            float rowStartY = 195f;
            float rowStepY = 65f;
            for (int r = 0; r < 7; r++)
            {
                var rowGo = new GameObject($"LessonItem_{r}");
                rowGo.transform.SetParent(listContainerGo.transform, false);
                var rowRt = rowGo.AddComponent<RectTransform>();
                rowRt.anchoredPosition = new Vector2(0, rowStartY - r * rowStepY);
                rowRt.sizeDelta = new Vector2(1000f, 56f);

                var rowImg = rowGo.AddComponent<Image>();
                rowImg.sprite = cardFrameSprite;
                rowImg.type = Image.Type.Sliced;
                rowImg.raycastTarget = true;
                rowGo.AddComponent<Button>();

                var rowUI = rowGo.AddComponent<LessonOverviewRowUI>();

                var hlGo = new GameObject("Img_HighlightBorder");
                hlGo.transform.SetParent(rowGo.transform, false);
                var hlRt = hlGo.AddComponent<RectTransform>();
                StretchFull(hlRt);
                var hlImg = hlGo.AddComponent<Image>();
                hlImg.sprite = cardFrameSprite;
                hlImg.type = Image.Type.Sliced;
                hlImg.color = new Color(1f, 0.84f, 0f, 0.95f);
                hlImg.raycastTarget = false;
                hlGo.SetActive(false);

                var gemGo = new GameObject("Img_Gem");
                gemGo.transform.SetParent(rowGo.transform, false);
                var gemRt = gemGo.AddComponent<RectTransform>();
                gemRt.anchoredPosition = new Vector2(-465f, 0);
                gemRt.sizeDelta = new Vector2(28f, 28f);
                var gemImg = gemGo.AddComponent<Image>();
                gemImg.sprite = gemSprite;
                gemImg.raycastTarget = false;

                var nameTmp = CreateTmp(rowGo, "Txt_LessonName", $"Bài {r+2}. Tên bài học", 18f, Color.white, font, TextAlignmentOptions.Left, true);
                nameTmp.rectTransform.anchoredPosition = new Vector2(-190f, 0);
                nameTmp.rectTransform.sizeDelta = new Vector2(490f, 36f);

                var attTmp = CreateTmp(rowGo, "Txt_AttemptsCount", "0 lượt", 16f, new Color(0.6f, 0.9f, 1f), font, TextAlignmentOptions.Center);
                attTmp.rectTransform.anchoredPosition = new Vector2(120f, 0);
                attTmp.rectTransform.sizeDelta = new Vector2(90f, 36f);

                var latestTmp = CreateTmp(rowGo, "Txt_LatestScore", "Gần nhất: —", 16f, new Color(1f, 0.92f, 0.65f), font, TextAlignmentOptions.Center);
                latestTmp.rectTransform.anchoredPosition = new Vector2(235f, 0);
                latestTmp.rectTransform.sizeDelta = new Vector2(130f, 36f);

                var bestTmp = CreateTmp(rowGo, "Txt_BestScore", "Cao nhất: —", 16f, new Color(1f, 0.85f, 0.35f), font, TextAlignmentOptions.Center, true);
                bestTmp.rectTransform.anchoredPosition = new Vector2(360f, 0);
                bestTmp.rectTransform.sizeDelta = new Vector2(130f, 36f);

                var badgeBgGo = new GameObject("Img_StatusBadgeBg");
                badgeBgGo.transform.SetParent(rowGo.transform, false);
                var badgeBgRt = badgeBgGo.AddComponent<RectTransform>();
                badgeBgRt.anchoredPosition = new Vector2(440f, 0);
                badgeBgRt.sizeDelta = new Vector2(86f, 32f);
                var badgeBgImg = badgeBgGo.AddComponent<Image>();
                badgeBgImg.sprite = cardFrameSprite;
                badgeBgImg.type = Image.Type.Sliced;
                badgeBgImg.color = new Color(0.18f, 0.22f, 0.28f, 0.7f);
                badgeBgImg.raycastTarget = false;

                var badgeTmp = CreateTmp(badgeBgGo, "Txt_StatusBadge", "CHƯA HỌC", 14f, new Color(0.7f, 0.7f, 0.75f), font, TextAlignmentOptions.Center, true);
                StretchFull(badgeTmp.rectTransform);

                rowUI.AutoWireRowReferences();
                lessonRows.Add(rowUI);
            }

            // Action Button (Xem chi tiết)
            var actionBtnGo = new GameObject("Btn_ViewDetail");
            actionBtnGo.transform.SetParent(rightPanelGo.transform, false);
            var actionBtnRt = actionBtnGo.AddComponent<RectTransform>();
            actionBtnRt.anchoredPosition = new Vector2(0, -280f);
            actionBtnRt.sizeDelta = new Vector2(380f, 72f);
            var actionBtnImg = actionBtnGo.AddComponent<Image>();
            actionBtnImg.sprite = actionBtnSprite;
            actionBtnImg.type = Image.Type.Sliced;
            actionBtnImg.raycastTarget = true;
            btnViewDetail = actionBtnGo.AddComponent<Button>();

            var actionTmp = CreateTmp(actionBtnGo, "Txt_Action", "Xem chi tiết  ➔", 24f, new Color(1f, 0.95f, 0.8f), font, TextAlignmentOptions.Center, true);
            StretchFull(actionTmp.rectTransform);
        }

        private static TextMeshProUGUI CreateTmp(GameObject parent, string name, string text, float size, Color color, TMP_FontAsset font, TextAlignmentOptions align, bool bold = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt = go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            if (font != null) tmp.font = font;
            tmp.alignment = align;
            if (bold) tmp.fontStyle = FontStyles.Bold;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CreateButton(GameObject parent, string name, Vector2 pos, Vector2 size, Sprite sprite, string text, TMP_FontAsset font, float textSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.raycastTarget = true;
            var btn = go.AddComponent<Button>();

            var tmp = CreateTmp(go, "Text", text, textSize, Color.white, font, TextAlignmentOptions.Center, true);
            StretchFull(tmp.rectTransform);
            return btn;
        }

        private static GameObject CreateFrameWithText(GameObject parent, string frameName, string textName, Vector2 pos, Vector2 size, Sprite sprite, string text, TMP_FontAsset font, float textSize, Color textColor)
        {
            var frameGo = new GameObject(frameName);
            frameGo.transform.SetParent(parent.transform, false);
            var rt = frameGo.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = frameGo.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.raycastTarget = false;

            var tmp = CreateTmp(frameGo, textName, text, textSize, textColor, font, TextAlignmentOptions.Center, true);
            StretchFull(tmp.rectTransform);
            return frameGo;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void InitFiltersFromSaveOrSession()
        {
            if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.ReloadSaveData();
            }

            var save = PhysicsSaveManager.Load();

            // 1. Ưu tiên lựa chọn từ NavigationService.Instance.Selection nếu có
            if (NavigationService.Instance != null && NavigationService.Instance.Selection != null &&
                NavigationService.Instance.Selection.ChapterId > 0 &&
                !string.IsNullOrEmpty(NavigationService.Instance.Selection.LessonId))
            {
                _selectedChapter = Mathf.Clamp(NavigationService.Instance.Selection.ChapterId, 1, 5);
                _selectedLessonId = NavigationService.Instance.Selection.LessonId;
                _selectedLessonTitle = NavigationService.Instance.Selection.LessonTitle;
                string diff = NavigationService.Instance.Selection.Difficulty;
                int diffIdx = Array.IndexOf(Difficulties, diff);
                _selectedDifficultyIndex = diffIdx >= 0 ? diffIdx : 0;
                return;
            }

            // 2. Ưu tiên lựa chọn đã lưu trong SaveData
            if (save != null && save.progressFilterChapterId >= 1 && save.progressFilterChapterId <= 5)
            {
                _selectedChapter = save.progressFilterChapterId;
                if (!string.IsNullOrEmpty(save.progressFilterLessonId))
                {
                    _selectedLessonId = save.progressFilterLessonId;
                }
                int diffIdx = Array.IndexOf(Difficulties, save.progressFilterDifficulty);
                _selectedDifficultyIndex = diffIdx >= 0 ? diffIdx : 0;
                return;
            }

            // 3. Ưu tiên lượt thi gần nhất trong quizHistory
            if (save != null && save.quizHistory != null)
            {
                var latest = save.quizHistory.LastOrDefault(x => x != null && (x.isComplete || (x.totalCount > 0 && x.correctCount + x.wrongCount >= x.totalCount)));
                if (latest != null && !string.IsNullOrEmpty(latest.lessonId))
                {
                    _selectedLessonId = latest.lessonId;
                    _selectedChapter = latest.chapterId > 0 ? latest.chapterId : GetChapterForLesson(latest.lessonId);
                    int diffIdx = Array.IndexOf(Difficulties, latest.assessmentKey);
                    _selectedDifficultyIndex = diffIdx >= 0 ? diffIdx : 0;
                    return;
                }
            }

            // 4. Mặc định: Chương 1, Bài C1_L2, Nhận biết
            _selectedChapter = 1;
            _selectedLessonId = "C1_L2";
            _selectedLessonTitle = "Động năng. Thế năng";
            _selectedDifficultyIndex = 0;
        }

        public void RefreshOverview()
        {
            var save = PhysicsSaveManager.Load();
            var allHistory = (save?.quizHistory ?? new List<PhysicsQuizHistoryEntry>())
                .Where(x => x != null && (x.isComplete || (x.totalCount > 0 && x.correctCount + x.wrongCount >= x.totalCount)))
                .ToList();

            var validMcq = allHistory.Where(x => IsMcq(x)).ToList();
            var validEssay = allHistory.Where(x => IsEssay(x)).ToList();
            var validHistory = allHistory; // Tất cả lượt học hợp lệ (cả Trắc nghiệm và Tự luận)

            // A. THỐNG KÊ TỔNG QUAN (chuẩn 100% từ lịch sử thật)
            int totalMcq = validMcq.Count;
            int totalEssay = validEssay.Count;
            int totalAttempts = validHistory.Count;

            var distinctLessons = validHistory.Select(x => NormalizeLessonKey(x.lessonId)).Distinct().ToList();
            int totalLessonsTrained = distinctLessons.Count;

            float avgScore = totalAttempts > 0 ? (float)validHistory.Average(x => x.scorePercent) : 0f;
            int maxScore = totalAttempts > 0 ? validHistory.Max(x => x.scorePercent) : 0;

            string lastDateStr = "Chưa có";
            if (totalAttempts > 0)
            {
                var latest = validHistory.OrderBy(x => ParseCompletedTime(x.completedAtUtc)).LastOrDefault();
                if (latest != null && !string.IsNullOrEmpty(latest.completedAtUtc))
                {
                    if (DateTime.TryParse(latest.completedAtUtc, null, DateTimeStyles.RoundtripKind, out DateTime dt))
                    {
                        lastDateStr = dt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
                    }
                    else
                    {
                        lastDateStr = latest.completedAtUtc;
                    }
                }
                else
                {
                    lastDateStr = "Đã lưu";
                }
            }

            float completionPct = (float)totalLessonsTrained / TotalCurriculumLessons * 100f;

            if (txtTotalLessons != null)
                txtTotalLessons.text = $"{totalLessonsTrained} bài";
            if (txtTotalAttempts != null)
            {
                if (totalAttempts > 0)
                    txtTotalAttempts.text = $"{totalAttempts} lượt\n<size=12><color=#93c5fd>TN: {totalMcq} • TL: {totalEssay}</color></size>";
                else
                    txtTotalAttempts.text = "0 lượt";
            }
            if (txtAvgScore != null)
                txtAvgScore.text = totalAttempts > 0 ? $"{avgScore:F1}%" : "—";
            if (txtMaxScore != null)
                txtMaxScore.text = totalAttempts > 0 ? $"{maxScore}/100" : "—";
            if (txtLastDate != null)
                txtLastDate.text = lastDateStr;
            if (txtCompletionRate != null)
                txtCompletionRate.text = totalAttempts > 0 ? $"{completionPct:F1}%" : "0%";

            // B. TIÊU ĐỀ BỘ LỌC
            var chapterInfo = PhysicsCurriculumData.GetChapter(_selectedChapter);
            if (txtChapterTitle != null)
            {
                txtChapterTitle.text = $"Chương {chapterInfo.romanNumber}: {chapterInfo.chapterName}";
            }
            if (txtDifficultyTitle != null)
            {
                txtDifficultyTitle.text = DifficultyNames[_selectedDifficultyIndex];
            }
            if (txtModeTitle != null)
            {
                txtModeTitle.text = ModeNames[_selectedModeIndex];
            }

            // C. DANH SÁCH BÀI HỌC THEO CHƯƠNG ĐÃ CHỌN
            var lessons = PhysicsCurriculumData.GetLessons(_selectedChapter);
            string currentDiff = Difficulties[_selectedDifficultyIndex];
            string currentMode = Modes[_selectedModeIndex];

            // Nếu bài đang chọn không thuộc chương hiện tại, chọn bài đầu tiên của chương
            if (!lessons.Any(l => StudyScreensController.AreLessonsMatching(l.lessonId, _selectedLessonId)))
            {
                if (lessons.Count > 0)
                {
                    _selectedLessonId = lessons[0].lessonId;
                    _selectedLessonTitle = lessons[0].DisplayName;
                }
            }

            for (int i = 0; i < lessonRows.Count; i++)
            {
                var row = lessonRows[i];
                if (row == null) continue;

                if (i < lessons.Count)
                {
                    var lesson = lessons[i];
                    row.gameObject.SetActive(true);

                    // Lọc lịch sử theo bài học, mức độ và hình thức
                    var lessonHistory = validHistory.Where(x =>
                    {
                        if (!StudyScreensController.AreLessonsMatching(x.lessonId, lesson.lessonId)) return false;
                        if (!string.IsNullOrEmpty(currentDiff) && !StudyScreensController.IsDifficultyMatching(x.assessmentKey, currentDiff)) return false;
                        if (currentMode == "MCQ" && !IsMcq(x)) return false;
                        if (currentMode == "ESSAY" && !IsEssay(x)) return false;
                        return true;
                    }).ToList();

                    int attemptsCount = lessonHistory.Count;
                    var latestAttempt = lessonHistory.OrderBy(x => ParseCompletedTime(x.completedAtUtc)).LastOrDefault();
                    var bestAttempt = lessonHistory.OrderByDescending(x => x.scorePercent).FirstOrDefault();

                    bool isSelected = StudyScreensController.AreLessonsMatching(lesson.lessonId, _selectedLessonId);

                    row.SetupRow(
                        lesson: lesson,
                        attemptsCount: attemptsCount,
                        latestScore: latestAttempt != null ? latestAttempt.scorePercent : -1,
                        bestScore: bestAttempt != null ? bestAttempt.scorePercent : -1,
                        isSelected: isSelected,
                        onRowClicked: () => OnLessonSelected(lesson)
                    );
                }
                else
                {
                    row.gameObject.SetActive(false);
                }
            }
        }

        private static bool IsMcq(PhysicsQuizHistoryEntry e)
        {
            if (e == null) return false;
            string act = (e.activityType ?? "").ToLowerInvariant();
            return !act.Contains("tự luận") && !act.Contains("written") && !act.Contains("essay");
        }

        private static bool IsEssay(PhysicsQuizHistoryEntry e)
        {
            if (e == null) return false;
            string act = (e.activityType ?? "").ToLowerInvariant();
            return act.Contains("tự luận") || act.Contains("written") || act.Contains("essay");
        }

        private static DateTime ParseCompletedTime(string timeStr)
        {
            if (string.IsNullOrEmpty(timeStr)) return DateTime.MinValue;
            if (DateTime.TryParse(timeStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dt))
                return dt;
            if (DateTime.TryParse(timeStr, out dt))
                return dt;
            return DateTime.MinValue;
        }

        private void OnLessonSelected(CurriculumLessonInfo lesson)
        {
            if (lesson == null) return;
            _selectedLessonId = lesson.lessonId;
            _selectedLessonTitle = lesson.DisplayName;
            RefreshOverview();
        }

        public void OnPrevChapterClicked()
        {
            _selectedChapter--;
            if (_selectedChapter < 1) _selectedChapter = 5;
            RefreshOverview();
        }

        public void OnNextChapterClicked()
        {
            _selectedChapter++;
            if (_selectedChapter > 5) _selectedChapter = 1;
            RefreshOverview();
        }

        public void OnPrevDifficultyClicked()
        {
            _selectedDifficultyIndex--;
            if (_selectedDifficultyIndex < 0) _selectedDifficultyIndex = Difficulties.Length - 1;
            RefreshOverview();
        }

        public void OnNextDifficultyClicked()
        {
            _selectedDifficultyIndex++;
            if (_selectedDifficultyIndex >= Difficulties.Length) _selectedDifficultyIndex = 0;
            RefreshOverview();
        }

        public void OnNextModeClicked()
        {
            _selectedModeIndex++;
            if (_selectedModeIndex >= Modes.Length) _selectedModeIndex = 0;
            RefreshOverview();
        }

        public void OnPrevModeClicked()
        {
            _selectedModeIndex--;
            if (_selectedModeIndex < 0) _selectedModeIndex = Modes.Length - 1;
            RefreshOverview();
        }

        public void OnViewDetailClicked()
        {
            // 1. Lưu lại lựa chọn hiện tại vào SaveData
            var save = PhysicsSaveManager.Load();
            save.progressFilterChapterId = _selectedChapter;
            save.progressFilterLessonId = _selectedLessonId;
            save.progressFilterDifficulty = Difficulties[_selectedDifficultyIndex];
            PhysicsSaveManager.Save(save);

            // 2. Gán dữ liệu sang NavigationService.Instance.Selection để truyền trực tiếp vào Scene 24
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.Selection.ChapterId = _selectedChapter;
                NavigationService.Instance.Selection.LessonId = _selectedLessonId;
                NavigationService.Instance.Selection.LessonTitle = _selectedLessonTitle;
                NavigationService.Instance.Selection.Difficulty = Difficulties[_selectedDifficultyIndex];
                NavigationService.Instance.Selection.Mode = Modes[_selectedModeIndex];
                NavigationService.Instance.ReturnContext.OriginRoute = RouteId.ProgressOverview;
            }

            if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.SelectChapter(_selectedChapter);
                PhysicsGameSession.Instance.SelectLesson(_selectedLessonId, _selectedLessonTitle);
            }

            Debug.Log($"[ProgressOverviewController] Bấm 'Xem chi tiết' -> Mở Scene 24 (Tiến bộ học tập). Chapter={_selectedChapter}, Lesson={_selectedLessonId}, Diff={Difficulties[_selectedDifficultyIndex]}, Mode={Modes[_selectedModeIndex]}");

            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.NavigateTo(RouteId.LearningProgress);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(PhysicsSceneNames.LearningProgress);
            }
        }

        public void OnBackClicked()
        {
            Debug.Log("[ProgressOverviewController] Bấm 'Quay lại' -> Về Main Menu Aurelia (RouteId.MainMenu).");
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.NavigateTo(RouteId.MainMenu);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(PhysicsSceneNames.MainMenu);
            }
        }

        private static string NormalizeLessonKey(string lessonId)
        {
            if (string.IsNullOrEmpty(lessonId)) return "";
            string s = lessonId.Trim().ToLowerInvariant();
            if (s.StartsWith("c") && s.Contains("_l"))
            {
                var parts = s.Split('_');
                if (parts.Length == 2 && parts[1].StartsWith("l"))
                {
                    return "bai" + parts[1].Substring(1);
                }
            }
            if (s.StartsWith("bai")) return s;
            if (s.StartsWith("bài")) return "bai" + s.Substring(3).Trim();
            return s;
        }

        private static int GetChapterForLesson(string lessonId)
        {
            if (string.IsNullOrEmpty(lessonId)) return 1;
            string s = lessonId.Trim().ToLowerInvariant();
            if (s.StartsWith("c") && s.Length >= 2 && char.IsDigit(s[1]))
            {
                return s[1] - '0';
            }
            int num = 0;
            var digits = new string(s.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out num))
            {
                if (num >= 2 && num <= 4) return 1;
                if (num >= 5 && num <= 10) return 2;
                if (num >= 11 && num <= 13) return 3;
                if (num >= 14 && num <= 15) return 4;
                if (num >= 16 && num <= 17) return 5;
            }
            return 1;
        }
    }

    /// <summary>
    /// Component quản lý hiển thị cho 1 hàng bài học trong bảng danh sách bài của màn Tổng quan.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LessonOverviewRowUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI txtLessonName;
        [SerializeField] private TextMeshProUGUI txtAttemptsCount;
        [SerializeField] private TextMeshProUGUI txtLatestScore;
        [SerializeField] private TextMeshProUGUI txtBestScore;
        [SerializeField] private TextMeshProUGUI txtStatusBadge;
        [SerializeField] private Image imgStatusBadgeBg;
        [SerializeField] private Image imgHighlightBorder;
        [SerializeField] private Button btnRow;

        private Action _onRowClicked;

        private void Awake()
        {
            AutoWireRowReferences();
            if (btnRow != null)
            {
                btnRow.onClick.RemoveAllListeners();
                btnRow.onClick.AddListener(() => _onRowClicked?.Invoke());
            }
        }

        public void AutoWireRowReferences()
        {
            var tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
            var imgs = GetComponentsInChildren<Image>(true);

            if (txtLessonName == null) txtLessonName = tmps.FirstOrDefault(t => t.name.Contains("Name") || t.name.Contains("Title"));
            if (txtAttemptsCount == null) txtAttemptsCount = tmps.FirstOrDefault(t => t.name.Contains("Attempts") || t.name.Contains("Luot"));
            if (txtLatestScore == null) txtLatestScore = tmps.FirstOrDefault(t => t.name.Contains("Latest") || t.name.Contains("LanNay"));
            if (txtBestScore == null) txtBestScore = tmps.FirstOrDefault(t => t.name.Contains("Best") || t.name.Contains("CaoNhat"));
            if (txtStatusBadge == null) txtStatusBadge = tmps.FirstOrDefault(t => t.name.Contains("Status") || t.name.Contains("TrangThai"));

            if (imgHighlightBorder == null) imgHighlightBorder = imgs.FirstOrDefault(i => i.name.Contains("Highlight") || i.name.Contains("Border"));
            if (imgStatusBadgeBg == null) imgStatusBadgeBg = imgs.FirstOrDefault(i => i.name.Contains("BadgeBg") || i.name.Contains("TagBg"));

            if (btnRow == null) btnRow = GetComponent<Button>() ?? GetComponentInChildren<Button>(true);
        }

        public void SetupRow(CurriculumLessonInfo lesson, int attemptsCount, int latestScore, int bestScore, bool isSelected, Action onRowClicked)
        {
            _onRowClicked = onRowClicked;

            if (txtLessonName != null)
            {
                txtLessonName.text = lesson != null ? lesson.DisplayName : "—";
            }

            if (txtAttemptsCount != null)
            {
                txtAttemptsCount.text = attemptsCount > 0 ? $"{attemptsCount} lượt" : "0 lượt";
            }

            if (txtLatestScore != null)
            {
                txtLatestScore.text = latestScore >= 0 ? $"Gần nhất: {latestScore}/100" : "Gần nhất: —";
            }

            if (txtBestScore != null)
            {
                txtBestScore.text = bestScore >= 0 ? $"Cao nhất: {bestScore}/100" : "Cao nhất: —";
            }

            if (txtStatusBadge != null)
            {
                if (attemptsCount > 0)
                {
                    txtStatusBadge.text = "ĐÃ HỌC";
                    txtStatusBadge.color = new Color(0.35f, 0.95f, 0.45f);
                    if (imgStatusBadgeBg != null)
                    {
                        imgStatusBadgeBg.color = new Color(0.08f, 0.35f, 0.15f, 0.85f);
                    }
                }
                else
                {
                    txtStatusBadge.text = "CHƯA HỌC";
                    txtStatusBadge.color = new Color(0.7f, 0.7f, 0.75f);
                    if (imgStatusBadgeBg != null)
                    {
                        imgStatusBadgeBg.color = new Color(0.18f, 0.22f, 0.28f, 0.7f);
                    }
                }
            }

            if (imgHighlightBorder != null)
            {
                imgHighlightBorder.gameObject.SetActive(isSelected);
                imgHighlightBorder.color = new Color(1f, 0.84f, 0f, 0.95f);
            }
        }
    }
}
