// ============================================================
// MainMenuManager.cs
// Flow chọn nội dung trước khi vào trận (#1-#5):
//   MAIN MENU → CHỌN MÔN → CHỌN CHƯƠNG → CHỌN BÀI → CHỌN PHƯƠNG THỨC → START
// ============================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI selectedSubjectText;

    [Header("Panels")]
    public GameObject subjectPanel;
    public GameObject gameModePanel;   // MÀN CHỌN GAME (hiện ngay sau khi chọn môn)
    public GameObject chapterPanel;
    public GameObject lessonPanel;
    public GameObject modePanel;

    [Header("Game Mode")]
    [Tooltip("Tên scene của game VƯỢT CHƯỚNG NGẠI VẬT (game 2 của Bách). Mặc định: ChapterList 1.")]
    public string obstacleGameSceneName = "ChapterList 1";

    [Header("Chapter/Lesson Container (Nơi chứa nút)")]
    public Transform chapterContainer;
    public Transform lessonContainer;

    [Header("Prefabs (Tùy chọn - để trống sẽ tự tạo)")]
    public GameObject buttonPrefab; // Một prefab UI Button cơ bản (có TextMeshProUGUI con)

    // Bài vừa chọn (để Back không mất dữ liệu)
    private string _currentChapterID;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        EnsureSetupUI();   // tự dựng panel + container + prefab nếu scene chưa có
        ShowPanel(subjectPanel);
    }

    public void ShowPanel(GameObject panel)
    {
        if (subjectPanel != null) subjectPanel.SetActive(false);
        if (gameModePanel != null) gameModePanel.SetActive(false);
        if (chapterPanel != null) chapterPanel.SetActive(false);
        if (lessonPanel != null) lessonPanel.SetActive(false);
        if (modePanel != null) modePanel.SetActive(false);

        if (panel != null) panel.SetActive(true);
    }

    // ================================================================
    // TỰ DỰNG UI KHI THIẾU (không cần chạy tool Editor)
    // ================================================================
    private void EnsureSetupUI()
    {
        // Canvas: dùng canvas sẵn trên chính GameObject này, thiếu thì tự thêm
        if (!TryGetComponent(out Canvas canvas))
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameObject.AddComponent<GraphicRaycaster>();
        }

        if (chapterPanel == null)
        {
            chapterPanel = BuildSelectPanel(canvas.transform, "ChapterPanel", "CHỌN CHƯƠNG",
                                            OnBackFromChapterClicked);
            chapterContainer = chapterPanel.transform.Find("Container");
        }
        if (lessonPanel == null)
        {
            lessonPanel = BuildSelectPanel(canvas.transform, "LessonPanel", "CHỌN BÀI",
                                           OnBackToChapterClicked);
            lessonContainer = lessonPanel.transform.Find("Container");
        }
        if (gameModePanel == null)
        {
            gameModePanel = BuildSelectPanel(canvas.transform, "GameModePanel", "CHỌN GAME",
                                             OnBackToSubjectClicked);
            BuildGameModeButtons(gameModePanel.transform);
        }
        if (modePanel == null)
        {
            modePanel = BuildSelectPanel(canvas.transform, "ModePanel", "CHỌN PHƯƠNG THỨC",
                                         OnBackToLessonClicked);
            BuildModeButtons(modePanel.transform);
        }
        if (buttonPrefab == null)
        {
            buttonPrefab = BuildMenuButtonTemplate(canvas.transform);
        }
    }

    /// <summary>1 panel chọn: nền tối + tiêu đề + nút BACK + Grid container.</summary>
    private GameObject BuildSelectPanel(Transform parent, string name, string title, UnityEngine.Events.UnityAction onBack)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.92f);

        CreateText(panel.transform, "Title", title, 42, TextAlignmentOptions.Center,
                   new Vector2(0f, 300f), new Vector2(800f, 80f));

        CreateSimpleButton(panel.transform, "BtnBack", "◀ BACK",
                           new Vector2(-520f, 300f), new Vector2(170f, 56f),
                           new Color(0.55f, 0.35f, 0.25f), onBack);

        GameObject container = new GameObject("Container");
        container.transform.SetParent(panel.transform, false);
        RectTransform crt = container.AddComponent<RectTransform>();
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0.5f);
        crt.anchoredPosition = new Vector2(0f, -60f);
        crt.sizeDelta = new Vector2(800f, 560f);
        GridLayoutGroup grid = container.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(360f, 130f);
        grid.spacing = new Vector2(24f, 24f);
        grid.childAlignment = TextAnchor.UpperCenter;

        panel.SetActive(false);
        return panel;
    }

    /// <summary>2 nút phương thức TRẮC NGHIỆM / TỰ LUẬN trong ModePanel (#5).</summary>
    private void BuildModeButtons(Transform modeParent)
    {
        CreateText(modeParent, "Hint", "Chọn cách bạn muốn ôn tập", 20, TextAlignmentOptions.Center,
                   new Vector2(0f, 40f), new Vector2(700f, 40f));

        CreateSimpleButton(modeParent, "BtnMultipleChoice", "TRẮC NGHIỆM",
                           new Vector2(-180f, -70f), new Vector2(310f, 96f),
                           new Color(0.2f, 0.55f, 0.4f),
                           () => SelectMode("MultipleChoice"));

        CreateSimpleButton(modeParent, "BtnEssay", "TỰ LUẬN",
                           new Vector2(180f, -70f), new Vector2(310f, 96f),
                           new Color(0.65f, 0.45f, 0.2f),
                           () => SelectMode("EssayImage"));
    }

    /// <summary>Prefab nút mẫu (inactive) - mỗi card Chương/Bài clone từ đây.</summary>
    private GameObject BuildMenuButtonTemplate(Transform parent)
    {
        GameObject go = new GameObject("MenuButtonPrefab");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(360f, 130f);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.16f, 0.34f, 0.5f);

        go.AddComponent<Button>();

        GameObject label = new GameObject("Text");
        label.transform.SetParent(go.transform, false);
        RectTransform lrt = label.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = new Vector2(12f, 8f);
        lrt.offsetMax = new Vector2(-12f, -8f);
        TextMeshProUGUI tmp = label.AddComponent<TextMeshProUGUI>();
        tmp.text = "Button";
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.Normal;

        go.SetActive(false);
        return go;
    }

    // ================================================================
    // 1. MÔN (SubjectCard gọi vào đây)
    // ================================================================
    public void SelectSubject(string subjectName)
    {
        Debug.Log("[MainMenuManager] SelectSubject: " + subjectName);

        if (selectedSubjectText != null)
        {
            selectedSubjectText.text = "Môn đã chọn: " + subjectName;
        }

        string lower = subjectName.ToLower();
        if (lower.Contains("physic") || lower.Contains("vật lý") || lower.Contains("vật lí")
            || lower.Contains("phy") || lower.Contains("vat ly") || lower.Contains("vat li"))
        {
            SceneManager.LoadScene("ChapterList");
            return;
        }

        // 2. MÔN HÓA HỌC (Chemistry của HUY): Vào thẳng flow của Hóa (Scene3_LessonType)
        if (lower.Contains("chem") || lower.Contains("hóa") || lower.Contains("hoá")
            || lower.Contains("hoa"))
        {
            GameSessionData.SelectedSubject = "Chemistry";
            if (GameSession.Instance != null && GameSession.Instance.selectedSubject == null)
            {
                SubjectData chem = ScriptableObject.CreateInstance<SubjectData>();
                chem.subjectType = SubjectType.Chemistry;
                chem.subjectName = "Hóa học";
                GameSession.Instance.selectedSubject = chem;
            }

            Debug.Log("[MainMenuManager] 🧪 Đã chọn môn Hóa Học -> Chuyển thẳng sang Scene3_LessonType của Huy");
            if (NavigationManager.Instance != null)
            {
                NavigationManager.Instance.LoadScene(NavigationManager.SCENE_LESSON_TYPE);
            }
            else
            {
                SceneManager.LoadScene("Scene3_LessonType");
            }
            return;
        }

        // 3. MÔN SINH HỌC (Biology của ÁNH): Môn duy nhất có 2 game (Game Sinh Tồn & Vượt Chướng Ngại Vật)
        if (chapterPanel == null || chapterContainer == null || buttonPrefab == null)
        {
            Debug.LogError("[MainMenuManager] LỖI: Không tự dựng được UI chọn chương! Kiểm tra Console.");
            // Fallback vào thẳng game (giữ hành vi cũ để không bị chặn khi test)
            GameSessionData.SelectedSubject = subjectName;
            GameSessionData.SelectedChapterID = "C11";
            GameSessionData.SelectedLessonID = "B39";
            GameSessionData.SelectedQuestionType = QuestionType.MultipleChoice;
            SceneManager.LoadScene("SinhHoc");
            return;
        }

        GameSessionData.SelectedSubject = subjectName;

        // Chỉ riêng môn Sinh mới hiện modal chọn 1 trong 2 game:
        if (gameModePanel != null)
        {
            ShowPanel(gameModePanel);
            return;
        }

        // ===== Flow cũ (giữ nguyên) =====
        GenerateChapterButtons(subjectName);
        ShowPanel(chapterPanel);
    }

    // ================================================================
    // 1.5 MÀN CHỌN GAME - hiện ngay sau khi chọn môn (#mới)
    // ================================================================
    /// <summary>2 nút: GAME SINH TỒN / VƯỢT CHƯỚNG NGẠI VẬT.</summary>
    private void BuildGameModeButtons(Transform parent)
    {
        CreateText(parent, "Hint", "Bạn muốn chơi game nào?", 20, TextAlignmentOptions.Center,
                   new Vector2(0f, 40f), new Vector2(700f, 40f));

        // GAME 1: Sinh tồn miễn dịch - flow cũ nguyên trạng
        CreateSimpleButton(parent, "BtnGameSurvival", "GAME SINH TỒN",
                           new Vector2(-180f, -70f), new Vector2(310f, 96f),
                           new Color(0.2f, 0.55f, 0.4f),
                           OnGameSurvivalSelected);

        // GAME 2: Vượt chướng ngại vật của bạn khác
        CreateSimpleButton(parent, "BtnGameObstacle", "VƯỢT CHƯỚNG NGẠI VẬT",
                           new Vector2(180f, -70f), new Vector2(310f, 96f),
                           new Color(0.65f, 0.45f, 0.2f),
                           OnObstacleGameSelected);
    }

    /// <summary>Game 1 → tiếp tục flow cũ: Chương → Bài → Phương thức → SinhHoc.</summary>
    private void OnGameSurvivalSelected()
    {
        GenerateChapterButtons(GameSessionData.SelectedSubject);
        ShowPanel(chapterPanel);
    }

    /// <summary>Game 2 → load scene vượt chướng ngại vật (game của Bách: ChapterList 1).</summary>
    private void OnObstacleGameSelected()
    {
        string sceneName = string.IsNullOrEmpty(obstacleGameSceneName) || obstacleGameSceneName == "ObstacleGame"
            ? "ChapterList 1" : obstacleGameSceneName;

        Debug.Log("[MainMenu] Đã chọn game VƯỢT CHƯỚNG NGẠI VẬT (Bách) → load scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    // ================================================================
    // 2. CHƯƠNG - sinh từ CurriculumCatalog (#2, #3)
    // ================================================================
    private void GenerateChapterButtons(string subject)
    {
        ClearContainer(chapterContainer);
        List<CurriculumChapter> chapters = CurriculumCatalog.GetChapters(subject);

        foreach (CurriculumChapter ch in chapters)
        {
            string label = $"{CurriculumCatalog.ChapterShortLabel(ch)}\n{ch.cardTitle}";
            string chapterID = ch.chapterID;
            CreateButton(chapterContainer, label, () => SelectChapter(chapterID));
        }
    }

    public void SelectChapter(string chapterID)
    {
        GameSessionData.SelectedChapterID = chapterID;
        _currentChapterID = chapterID;
        GenerateLessonButtons(GameSessionData.SelectedSubject, chapterID);
        ShowPanel(lessonPanel);
    }

    // ================================================================
    // 3. BÀI - chỉ hiện bài thuộc chương đã chọn + Progress (#4, #26)
    // ================================================================
    private void GenerateLessonButtons(string subject, string chapterID)
    {
        ClearContainer(lessonContainer);
        List<CurriculumLesson> lessons = CurriculumCatalog.GetLessons(subject, chapterID);

        foreach (CurriculumLesson lesson in lessons)
        {
            string label = CurriculumCatalog.LessonFullLabel(lesson);
            string progress = BuildProgressLine(subject, chapterID, lesson.lessonID);
            if (!string.IsNullOrEmpty(progress))
                label += $"\n<size=15><color=#9fd8c8>{progress}</color></size>";

            string lessonID = lesson.lessonID;
            CreateButton(lessonContainer, label, () => SelectLesson(lessonID));
        }
    }

    /// <summary>Dòng progress hiển thị trên Lesson Card (#26), rỗng nếu chưa có dữ liệu.</summary>
    private static string BuildProgressLine(string subject, string chapterID, string lessonID)
    {
        KnowledgeProgressManager.LessonStats s =
            KnowledgeProgressManager.GetStats(subject, chapterID, lessonID);

        string username = GameSession.Instance != null && !string.IsNullOrEmpty(GameSession.Instance.username) ? GameSession.Instance.username : "HS001";
        
        int chapIdx = 1;
        System.Text.RegularExpressions.Match mc = System.Text.RegularExpressions.Regex.Match(chapterID, @"\d+");
        if (mc.Success) int.TryParse(mc.Value, out chapIdx);

        int lessonIdx = 1;
        System.Text.RegularExpressions.Match ml = System.Text.RegularExpressions.Regex.Match(lessonID, @"\d+");
        if (ml.Success) int.TryParse(ml.Value, out lessonIdx);

        string baiID = string.Format("B{0}_00{1}", chapIdx, lessonIdx); 
        string monID = "KHTN6";
        if (subject.Contains("Chem") || subject.Contains("Hóa")) monID = "KHTN7";
        if (subject.Contains("Phy") || subject.Contains("Lý")) monID = "KHTN8";

        int attempts = 0;
        int highScore = 0;
        if (ProgressSyncManager.Instance != null)
        {
            var p = ProgressSyncManager.Instance.GetProgress(username, monID, baiID);
            if (p != null)
            {
                attempts = p.soLanLam;
                highScore = p.diemCaoNhat;
            }
        }

        string result = " ";
        if (attempts > 0 || highScore > 0)
        {
            result = $"Số lượt làm: {attempts} | Điểm cao nhất: {highScore}";
        }
        else if (s != null && s.OverallProgress > 0)
        {
            result = $"Progress: {s.OverallProgress:0}% | Trắc nghiệm: {s.McAccuracy:0}%";
        }
        return result;
    }

    public void SelectLesson(string lessonID)
    {
        GameSessionData.SelectedLessonID = lessonID;
        ShowPanel(modePanel);
    }

    // ================================================================
    // 4. PHƯƠNG THỨC - TRẮC NGHIỆM / TỰ LUẬN (#5)
    // ================================================================
    public void SelectMode(string mode)
    {
        GameSessionData.SelectedQuestionType =
            mode == "MultipleChoice" ? QuestionType.MultipleChoice : QuestionType.EssayImage;

        Debug.Log($"[MainMenu] Ready! {GameSessionData.SelectedSubject} - " +
                  $"{GameSessionData.SelectedChapterID} - {GameSessionData.SelectedLessonID} - " +
                  $"{GameSessionData.SelectedQuestionType}");
        SceneManager.LoadScene("SinhHoc");
    }

    // ================================================================
    // BACK BUTTONS
    // ================================================================
    public void OnBackToSubjectClicked() { ShowPanel(subjectPanel); }

    public void OnBackFromChapterClicked()
    {
        string lower = (GameSessionData.SelectedSubject ?? "").ToLower();
        if (lower.Contains("sinh") || lower.Contains("bio"))
        {
            ShowPanel(gameModePanel);
        }
        else
        {
            ShowPanel(subjectPanel);
        }
    }

    public void OnBackToChapterClicked()
    {
        GenerateChapterButtons(GameSessionData.SelectedSubject);
        ShowPanel(chapterPanel);
    }

    public void OnBackToLessonClicked()
    {
        if (!string.IsNullOrEmpty(_currentChapterID))
            GenerateLessonButtons(GameSessionData.SelectedSubject, _currentChapterID);
        ShowPanel(lessonPanel);
    }

    // ================================================================
    // HELPERS
    // ================================================================
    private void ClearContainer(Transform container)
    {
        if (container == null) return;
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateButton(Transform container, string label, UnityEngine.Events.UnityAction action)
    {
        if (container == null) return;

        GameObject btnGo;
        if (buttonPrefab != null)
        {
            btnGo = Instantiate(buttonPrefab, container);
            btnGo.SetActive(true);
        }
        else
        {
            btnGo = new GameObject("MenuButton");
            btnGo.transform.SetParent(container, false);
            btnGo.AddComponent<RectTransform>().sizeDelta = new Vector2(360f, 130f);
            btnGo.AddComponent<Image>().color = new Color(0.16f, 0.34f, 0.5f);
            btnGo.AddComponent<Button>();
            GameObject lbl = new GameObject("Text");
            lbl.transform.SetParent(btnGo.transform, false);
            RectTransform lrt = lbl.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            TextMeshProUGUI tmp = lbl.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
        }

        TextMeshProUGUI tmpOut = btnGo.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmpOut != null)
        {
            tmpOut.text = label;
            tmpOut.textWrappingMode = TextWrappingModes.Normal;
            tmpOut.fontSize = Mathf.Min(tmpOut.fontSize, 20f);
        }

        Button btn = btnGo.GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(action);
    }

    /// <summary>Nút đơn giản dùng cho BACK / TRẮC NGHIỆM / TỰ LUẬN.</summary>
    private Button CreateSimpleButton(Transform parent, string name, string label, Vector2 pos,
                                      Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = color;
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        GameObject labelGo = new GameObject("Text");
        labelGo.transform.SetParent(go.transform, false);
        RectTransform lrt = labelGo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        return btn;
    }

    private TextMeshProUGUI CreateText(Transform parent, string name, string text, int size,
                                       TextAlignmentOptions align, Vector2 pos, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = Color.white;
        return tmp;
    }

    public void OnBackToLoginClicked()
    {
        // Reset trạng thái đăng nhập khi người dùng chủ động đăng xuất
        LoginManager.IsLoggedIn = false;

        // Handle returning to login screen
        LoginManager loginManager = FindObjectOfType<LoginManager>(true);
        if (loginManager != null && loginManager.usePanelSwitching)
        {
            if (loginManager.menuGamePanel != null) loginManager.menuGamePanel.SetActive(false);
            if (loginManager.loginPanel != null) loginManager.loginPanel.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene("SampleScene"); // Or Login scene name
        }
    }
}
