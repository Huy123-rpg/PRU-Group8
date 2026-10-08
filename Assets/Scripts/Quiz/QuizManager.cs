using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using ScienceQuest.Core;
using ScienceQuest.MiniGames;
using ScienceQuest.UI;

namespace ScienceQuest.Quiz
{
    /// <summary>
    /// QuizManager - Script chính điều khiển toàn bộ logic trắc nghiệm trong QuizScene.
    /// Tự động tìm và gắn vào các UI elements có sẵn trong scene.
    /// 
    /// Luồng hoạt động:
    /// 1. Load ngân hàng câu hỏi từ JSON (QuizDataBank)
    /// 2. Random 10 câu hỏi
    /// 3. Hiển thị từng câu hỏi lên UI
    /// 4. Đếm ngược 15 phút (QuizTimer)
    /// 5. Người chơi chọn đáp án hoặc nộp bài sớm
    /// 6. Hiển thị bảng kết quả (Panel_ResultBoard)
    /// 7. Cộng Coins/EXP vào GameManager
    /// 
    /// Phân công: M2 - Gameplay Programmer
    /// </summary>
    public class QuizManager : MonoBehaviour
    {
        public static QuizManager Instance { get; private set; }

        [Header("=== CẤU HÌNH QUIZ ===")]
        [SerializeField] private int questionsPerQuiz = 10;
        [SerializeField] private float quizDuration = 900f; // 15 phút = 900 giây
        [SerializeField] private int pointsPerCorrectAnswer = 10;
        [SerializeField] private int rewardCoins = 50;
        [SerializeField] private int rewardEXP = 100;
        [SerializeField] private float delayBetweenQuestions = 1.5f;

        [Header("=== MÀU SẮC FEEDBACK ===")]
        [SerializeField] private Color correctColor = new Color(0.2f, 0.8f, 0.2f, 1f);   // Xanh lá
        [SerializeField] private Color incorrectColor = new Color(0.9f, 0.2f, 0.2f, 1f);  // Đỏ
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 1f);            // Trắng
        [SerializeField] private Color selectedColor = new Color(0.3f, 0.6f, 1f, 1f);     // Xanh dương

        [Header("=== UI REFERENCES (Tự động tìm nếu chưa gán) ===")]
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI questionCounterText;
        [SerializeField] private Button[] answerButtons = new Button[4];
        [SerializeField] private TextMeshProUGUI[] answerTexts = new TextMeshProUGUI[4];
        [SerializeField] private Image[] answerButtonImages = new Image[4];
        [SerializeField] private Button submitButton;
        [SerializeField] private Button nextButton;

        [Header("=== PANEL KẾT QUẢ ===")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultTitleText;
        [SerializeField] private TextMeshProUGUI resultScoreText;
        [SerializeField] private TextMeshProUGUI resultCorrectText;
        [SerializeField] private TextMeshProUGUI resultIncorrectText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button detailButton;   // Nút "Xem chi tiết" (trước là "Về chọn chương")
        [SerializeField] private Button goBackButton;   // Nút "Quay lại" (tạo bằng code)

        // Panel xem chi tiết đáp án (tạo hoàn toàn bằng code)
        private GameObject reviewPanel;
        private int reviewCurrentIndex = 0;

        // Trạng thái quiz
        private QuizDataBank dataBank;
        private QuizTimer quizTimer;
        private List<QuestionData> currentQuestions;
        private int currentQuestionIndex = 0;
        private int totalScore = 0;
        private int correctCount = 0;
        private int incorrectCount = 0;
        private int[] playerAnswers; // Lưu đáp án người chơi đã chọn (-1 = chưa chọn)
        private bool isQuizActive = false;
        private bool isShowingFeedback = false;

        // Tên chương được truyền từ scene trước (qua PlayerPrefs hoặc static)
        public static string SelectedChapter = "";

        // Chế độ Bắn Vịt (Duck Shooting Mode)
        public static bool IsDuckShootingMode = false;
        private Canvas quizCanvas;
        private Button duckContinueButton;
        public bool IsQuizVisible => quizCanvas != null && quizCanvas.gameObject.activeSelf;

        public void SetQuizVisible(bool visible)
        {
            Scene quizScene = SceneManager.GetSceneByName("QuizScene");
            if (quizScene.isLoaded)
            {
                foreach (GameObject rootGo in quizScene.GetRootGameObjects())
                {
                    if (rootGo.name.ToLower().Contains("camera") || rootGo.name.ToLower().Contains("eventsystem"))
                    {
                        rootGo.SetActive(false);
                    }
                    else
                    {
                        rootGo.SetActive(visible);
                    }
                }
            }

            if (quizCanvas == null && quizScene.isLoaded)
            {
                foreach (GameObject rootGo in quizScene.GetRootGameObjects())
                {
                    Canvas c = rootGo.GetComponentInChildren<Canvas>(true);
                    if (c != null)
                    {
                        quizCanvas = c;
                        break;
                    }
                }
            }

            if (quizCanvas != null)
            {
                quizCanvas.sortingOrder = 50; // Luôn hiển thị đè lên màn Bắn Vịt khi kích hoạt
                quizCanvas.gameObject.SetActive(visible);
            }
        }

        public void SyncTimer(float remainingSeconds)
        {
            if (timerText != null)
            {
                int totalSec = Mathf.Max(0, Mathf.CeilToInt(remainingSeconds));
                int min = totalSec / 60;
                int sec = totalSec % 60;
                timerText.text = string.Format("{0:00}:{1:00}", min, sec);
            }
        }

        public void ShowDuckShootingQuestion(int questionIndex)
        {
            // Bật lại Canvas và UI của QuizScene
            SetQuizVisible(true);
            isQuizActive = true;
            HideScoreAndCenterTimer();
            NormalizeAnswerUI();
            if (currentQuestions != null && currentQuestions.Count > 0)
            {
                currentQuestionIndex = Mathf.Clamp(questionIndex, 0, currentQuestions.Count - 1);
                DisplayQuestion(currentQuestionIndex);
            }

            if (resultPanel != null) resultPanel.SetActive(false);
            if (reviewPanel != null) reviewPanel.SetActive(false);

            for (int i = 0; i < 4; i++)
            {
                if (answerButtons[i] != null)
                {
                    answerButtons[i].interactable = true;
                    if (answerButtonImages[i] != null)
                        answerButtonImages[i].color = normalColor;
                }
            }

            if (duckContinueButton != null)
            {
                duckContinueButton.gameObject.SetActive(false);
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitializeOnLoad()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == SceneLoader.SCENE_QUIZ_PHYSICS || scene.name == "QuizScene")
            {
                if (FindFirstObjectByType<QuizManager>() == null)
                {
                    GameObject go = new GameObject("QuizSystem_Auto");
                    go.AddComponent<QuizManager>();
                    Debug.Log("[QuizManager] ⚙️ Tự động tạo QuizManager vì không tìm thấy trong scene!");
                }
            }
        }

        private void Start()
        {
            InitializeQuiz();
            if (IsDuckShootingMode)
            {
                SetQuizVisible(false);
            }
        }

        /// <summary>
        /// Khởi tạo quiz: load câu hỏi, tìm UI, bắt đầu quiz
        /// </summary>
        private void InitializeQuiz()
        {
            // 1. Load ngân hàng câu hỏi
            dataBank = new QuizDataBank();

            if (dataBank.TotalQuestionCount == 0)
            {
                Debug.LogError("[QuizManager] Không tìm thấy câu hỏi nào trong ngân hàng!");
                return;
            }

            // 2. Tìm và gắn UI elements (trỏ chính xác vào QuizScene)
            AutoAssignUIReferences();

            // 3. Lấy chapter từ PlayerPrefs nếu có
            if (string.IsNullOrEmpty(SelectedChapter))
            {
                SelectedChapter = PlayerPrefs.GetString("QuizChapter", "");
            }

            // 4. Nếu là chế độ Bắn Vịt, chuẩn bị câu hỏi và ẨN NGAY LẬP TỨC (không tạo nút điều hướng trên Canvas Physics)
            if (IsDuckShootingMode)
            {
                PrepareDuckShootingMode();
            }
            else
            {
                SetupTimer();
                StartQuiz();
            }
        }

        /// <summary>
        /// Chuẩn bị câu hỏi cho chế độ Bắn Vịt kết hợp Quiz
        /// </summary>
        private void PrepareDuckShootingMode()
        {
            currentQuestionIndex = 0;
            totalScore = 0;
            correctCount = 0;
            incorrectCount = 0;
            isQuizActive = false;

            if (!string.IsNullOrEmpty(SelectedChapter))
            {
                currentQuestions = dataBank.GetRandomQuestionsByChapter(SelectedChapter, questionsPerQuiz);
            }
            else
            {
                currentQuestions = dataBank.GetRandomQuestions(questionsPerQuiz);
            }

            if (currentQuestions == null || currentQuestions.Count == 0)
            {
                currentQuestions = dataBank.GetAllQuestions();
                if (currentQuestions != null && currentQuestions.Count > questionsPerQuiz)
                {
                    currentQuestions = currentQuestions.GetRange(0, questionsPerQuiz);
                }
            }

            playerAnswers = new int[currentQuestions.Count];
            for (int i = 0; i < playerAnswers.Length; i++)
            {
                playerAnswers[i] = -1;
            }

            // Gắn sự kiện cho 4 nút đáp án
            for (int i = 0; i < 4; i++)
            {
                if (answerButtons[i] != null)
                {
                    int index = i;
                    answerButtons[i].onClick.RemoveAllListeners();
                    answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
                }
            }

            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(OnRetryClicked);
            }

            if (detailButton != null)
            {
                detailButton.onClick.RemoveAllListeners();
                detailButton.onClick.AddListener(OnDetailClicked);
            }

            if (resultPanel != null) resultPanel.SetActive(false);
            if (reviewPanel != null) reviewPanel.SetActive(false);

            // Ẩn QuizScene hoàn toàn ban đầu
            SetQuizVisible(false);
            Debug.Log("[QuizManager] 🦆 Đã chuẩn bị xong ngân hàng câu hỏi cho chế độ Bắn Vịt (QuizScene đã được ẩn hoàn toàn)!");
        }

        #region ===== AUTO ASSIGN UI =====

        /// <summary>
        /// Tự động tìm và gán các UI references trong scene.
        /// Tìm theo tên GameObject và nội dung text.
        /// Cách này tránh phải sửa file .unity (tránh merge conflict).
        /// </summary>
        private void AutoAssignUIReferences()
        {
            Canvas canvas = null;

            // Nếu đang trong chế độ Bắn Vịt, bắt buộc phải lấy Canvas thuộc QuizScene
            if (IsDuckShootingMode)
            {
                Scene quizScene = SceneManager.GetSceneByName("QuizScene");
                if (quizScene.isLoaded)
                {
                    foreach (GameObject rootGo in quizScene.GetRootGameObjects())
                    {
                        Canvas c = rootGo.GetComponentInChildren<Canvas>(true);
                        if (c != null)
                        {
                            canvas = c;
                            quizCanvas = c;
                            break;
                        }
                    }
                }
            }

            if (canvas == null)
            {
                Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var c in canvases)
                {
                    if (c.gameObject.scene.name == "QuizScene")
                    {
                        canvas = c;
                        quizCanvas = c;
                        break;
                    }
                }
            }

            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }

            if (canvas == null)
            {
                Debug.LogError("[QuizManager] Không tìm thấy Canvas trong scene!");
                return;
            }

            quizCanvas = canvas;
            Transform root = canvas.transform;

            // Tìm Panel_ResultBoard
            if (resultPanel == null)
            {
                Transform resultTransform = FindDeepChild(root, "Panel_ResultBoard");
                if (resultTransform != null)
                {
                    resultPanel = resultTransform.gameObject;
                    Debug.Log("[QuizManager] ✅ Đã tìm thấy Panel_ResultBoard");
                }
            }

            // Tìm Btn_Retry
            if (retryButton == null)
            {
                Transform retryTransform = FindDeepChild(root, "Btn_Retry");
                if (retryTransform != null)
                {
                    retryButton = retryTransform.GetComponent<Button>();
                    Debug.Log("[QuizManager] ✅ Đã tìm thấy Btn_Retry");
                }
            }

            // Tìm tất cả TextMeshProUGUI trong scene để gán theo nội dung
            TextMeshProUGUI[] allTexts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            Button[] allButtons = root.GetComponentsInChildren<Button>(true);

            // Tìm các text UI dựa trên nội dung hiện có trong scene
            foreach (var tmp in allTexts)
            {
                string text = tmp.text.Trim();

                // Timer text (hiển thị "25s" hoặc format thời gian)
                if (timerText == null && (text.Contains("s") && text.Length <= 5 && char.IsDigit(text[0])))
                {
                    timerText = tmp;
                    Debug.Log($"[QuizManager] ✅ Đã tìm thấy Timer Text: \"{text}\"");
                }

                // Câu hỏi (text dài nhất thường là câu hỏi)
                if (questionText == null && text.Contains("?") && text.Length > 20)
                {
                    questionText = tmp;
                    Debug.Log($"[QuizManager] ✅ Đã tìm thấy Question Text");
                }
                // Hoặc câu hỏi kết thúc bằng dấu ":"
                if (questionText == null && text.Contains(":") && text.Length > 20)
                {
                    questionText = tmp;
                    Debug.Log($"[QuizManager] ✅ Đã tìm thấy Question Text (dạng :)");
                }

                // Tổng điểm text (ô hiển thị điểm số trên bảng kết quả)
                if (text.Contains("TỔNG ĐIỂM") || text.Contains("T\u1ed4NG \u0110I\u1ec2M") || tmp.gameObject.name == "Text (TMP) (1)")
                {
                    resultScoreText = tmp;
                    Debug.Log($"[QuizManager] ✅ Đã tìm thấy Result Score Text (TỔNG ĐIỂM)");
                }

                // Text "HOÀN THÀNH!" (tiêu đề chính trên cùng của bảng kết quả)
                if (text.Contains("HOÀN THÀNH") || text.Contains("HO\u00c0N TH\u00c0NH"))
                {
                    resultTitleText = tmp;
                    resultTitleText.margin = Vector4.zero;
                    Debug.Log($"[QuizManager] ✅ Đã tìm thấy Completion Title (HOÀN THÀNH)");
                }

                // Text "ĐÚNG"
                if (resultCorrectText == null && (text.Contains("ĐÚNG") || text.Contains("\u0110\u00daNG")))
                {
                    resultCorrectText = tmp;
                    Debug.Log($"[QuizManager] ✅ Đã tìm thấy Result Correct Text");
                }

                // Text "SAI"
                if (resultIncorrectText == null && text.Contains("SAI"))
                {
                    resultIncorrectText = tmp;
                    Debug.Log($"[QuizManager] ✅ Đã tìm thấy Result Incorrect Text");
                }

                // Đáp án A, B, C, D
                if (text.StartsWith("A.") && answerTexts[0] == null)
                {
                    answerTexts[0] = tmp;
                    Debug.Log($"[QuizManager] ✅ Đã tìm thấy Answer A Text");
                }
                else if (text.StartsWith("B.") && answerTexts[1] == null)
                {
                    answerTexts[1] = tmp;
                    Debug.Log($"[QuizManager] ✅ Đã tìm thấy Answer B Text");
                }
                else if (text.StartsWith("C.") && answerTexts[2] == null)
                {
                    answerTexts[2] = tmp;
                    Debug.Log($"[QuizManager] ✅ Đã tìm thấy Answer C Text");
                }
                else if (text.StartsWith("D.") && answerTexts[3] == null)
                {
                    answerTexts[3] = tmp;
                    Debug.Log($"[QuizManager] ✅ Đã tìm thấy Answer D Text");
                }

                // Text "Làm Lại"
                if (text.Contains("Làm Lại") || text.Contains("L\u00e0m L\u1ea1i"))
                {
                    // Tìm button cha
                    Button parentBtn = tmp.GetComponentInParent<Button>();
                    if (parentBtn != null && retryButton == null)
                    {
                        retryButton = parentBtn;
                        Debug.Log($"[QuizManager] ✅ Đã tìm thấy Retry Button qua text");
                    }
                }

                // Text "Về chọn chương" → sẽ đổi thành "Xem chi tiết"
                if (text.Contains("chọn chương") || text.Contains("ch\u1ecdn ch\u01b0\u01a1ng"))
                {
                    Button parentBtn = tmp.GetComponentInParent<Button>();
                    if (parentBtn != null && detailButton == null)
                    {
                        detailButton = parentBtn;
                        // Đổi text nút thành "Xem chi tiết"
                        tmp.text = "Xem chi tiết";
                        Debug.Log($"[QuizManager] ✅ Đã tìm thấy nút 'Về chọn chương' → đổi thành 'Xem chi tiết'");
                    }
                }
            }

            // Tìm Answer Buttons (Button component cha của answer texts)
            for (int i = 0; i < 4; i++)
            {
                if (answerTexts[i] != null && answerButtons[i] == null)
                {
                    // Thử tìm cha có Image (hình tròn)
                    Transform parent = answerTexts[i].transform.parent;
                    Image parentImage = parent != null ? parent.GetComponent<Image>() : null;
                    GameObject targetObj = parentImage != null ? parent.gameObject : answerTexts[i].gameObject;

                    Button btn = targetObj.GetComponent<Button>();
                    if (btn == null)
                    {
                        // Chưa có Button -> tự động thêm
                        btn = targetObj.AddComponent<Button>();
                        Debug.Log($"[QuizManager] ➕ Đã tự động thêm Button vào đáp án {i}");
                    }
                    
                    answerButtons[i] = btn;
                    answerButtonImages[i] = targetObj.GetComponent<Image>();
                    Debug.Log($"[QuizManager] ✅ Đã tìm thấy Answer Button {i}");
                }
            }

            // Đồng bộ hình ảnh (Sprite) của 4 đáp án giống hệt nhau (sử dụng sprite của đáp án A)
            if (answerButtonImages[0] != null && answerButtonImages[0].sprite != null)
            {
                Sprite standardSprite = answerButtonImages[0].sprite;
                for (int i = 1; i < 4; i++)
                {
                    if (answerButtonImages[i] != null)
                    {
                        answerButtonImages[i].sprite = standardSprite;
                    }
                }
            }

            // Ẩn bảng Điểm và đưa Thời Gian ra giữa
            HideScoreAndCenterTimer();

             // Fix Result Board Hierarchy (Gom tự động các UI rải rác vào Panel)
            FixResultBoardHierarchy();

            // Chuẩn hóa UI đáp án (sửa lỗi text to nhỏ không đều)
            NormalizeAnswerUI();

            // Ẩn Panel_ResultBoard ban đầu
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }

            // Log tóm tắt kết quả tìm kiếm
            LogUIAssignmentSummary();
        }

        /// <summary>
        /// Ẩn SCORE + số 40 và dời TIME ra giữa
        /// </summary>
        private void HideScoreAndCenterTimer()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            Canvas targetCanvas = quizCanvas != null ? quizCanvas : canvas;
            if (targetCanvas == null) return;

            // === 1. Ẩn SCORE triệt để bằng cách quét hết TMP text (ngoại trừ các text của bảng kết quả) ===
            TextMeshProUGUI[] allTMPs = targetCanvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTMPs)
            {
                if (resultPanel != null && tmp.transform.IsChildOf(resultPanel.transform)) continue;
                string text = tmp.text.Trim();

                if (text == "40" || tmp == scoreText ||
                    (tmp.transform.parent != null && tmp.transform.parent.name.ToLower().Contains("score")))
                {
                    HideWithParents(tmp.transform);
                    Debug.Log($"[QuizManager] 🔇 Ẩn Score Text: \"{text}\"");
                }
            }

            // === 2. Ẩn ảnh/text SCORE trong canvas (ngoại trừ bảng kết quả) ===
            Transform[] allTransforms = targetCanvas.GetComponentsInChildren<Transform>(true);
            foreach (var t in allTransforms)
            {
                if (resultPanel != null && t.IsChildOf(resultPanel.transform)) continue;
                string nameLower = t.name.ToLower();
                if (nameLower.Contains("score"))
                {
                    t.gameObject.SetActive(false);
                    Debug.Log($"[QuizManager] 🔇 Ẩn object: {t.name}");
                }
            }

            // === 3. Tìm và ẩn ảnh "SCORE" nếu nó là SpriteRenderer hoặc Image có sprite tên "Score" (ngoại trừ bảng kết quả) ===
            Image[] allImages = targetCanvas.GetComponentsInChildren<Image>(true);
            foreach (var img in allImages)
            {
                if (resultPanel != null && img.transform.IsChildOf(resultPanel.transform)) continue;
                if (img.sprite != null && img.sprite.name.ToLower().Contains("score"))
                {
                    img.gameObject.SetActive(false);
                    Debug.Log($"[QuizManager] 🔇 Ẩn Score Image: {img.sprite.name}");
                }
            }

            // === 4. Dời toàn bộ cụm TIME ra chính giữa tuyệt đối (ngang và dọc) ===
            if (timerText != null)
            {
                Transform timeImageTransform = null;
                if (timerText.transform.parent != null && timerText.transform.parent != targetCanvas.transform)
                {
                    if (timerText.transform.parent.GetComponent<Image>() != null)
                    {
                        timeImageTransform = timerText.transform.parent;
                    }
                }
                if (timeImageTransform == null)
                {
                    foreach (var img in allImages)
                    {
                        if (img.sprite != null && (img.sprite.name.ToLower().Contains("time") || img.name.ToLower().Contains("time")))
                        {
                            timeImageTransform = img.transform;
                            break;
                        }
                    }
                }

                Transform existingRoot = targetCanvas.transform.Find("Dynamic_QuizTimerCenterRoot");
                GameObject timerRoot = existingRoot != null ? existingRoot.gameObject : new GameObject("Dynamic_QuizTimerCenterRoot");
                timerRoot.transform.SetParent(targetCanvas.transform, false);

                RectTransform rootRt = timerRoot.GetComponent<RectTransform>();
                if (rootRt == null) rootRt = timerRoot.AddComponent<RectTransform>();
                rootRt.anchorMin = new Vector2(0.5f, 1f);
                rootRt.anchorMax = new Vector2(0.5f, 1f);
                rootRt.pivot = new Vector2(0.5f, 1f);
                rootRt.anchoredPosition = new Vector2(0f, -12f);
                rootRt.sizeDelta = new Vector2(350f, 70f);

                HorizontalLayoutGroup hlg = timerRoot.GetComponent<HorizontalLayoutGroup>();
                if (hlg == null) hlg = timerRoot.AddComponent<HorizontalLayoutGroup>();
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.spacing = 15f;
                hlg.childControlWidth = false;
                hlg.childControlHeight = false;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;

                ContentSizeFitter csf = timerRoot.GetComponent<ContentSizeFitter>();
                if (csf == null) csf = timerRoot.AddComponent<ContentSizeFitter>();
                csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                if (timeImageTransform != null)
                {
                    timeImageTransform.SetParent(timerRoot.transform, false);
                    RectTransform imgRt = timeImageTransform.GetComponent<RectTransform>();
                    if (imgRt != null)
                    {
                        imgRt.sizeDelta = new Vector2(160f, 65f);
                    }
                    LayoutElement leImg = timeImageTransform.GetComponent<LayoutElement>();
                    if (leImg == null) leImg = timeImageTransform.gameObject.AddComponent<LayoutElement>();
                    leImg.preferredWidth = 160f;
                    leImg.preferredHeight = 65f;
                }

                timerText.transform.SetParent(timerRoot.transform, false);
                RectTransform textRt = timerText.GetComponent<RectTransform>();
                if (textRt != null)
                {
                    textRt.sizeDelta = new Vector2(175f, 65f);
                }
                LayoutElement leText = timerText.GetComponent<LayoutElement>();
                if (leText == null) leText = timerText.gameObject.AddComponent<LayoutElement>();
                leText.preferredWidth = 175f;
                leText.preferredHeight = 65f;

                timerText.alignment = TextAlignmentOptions.MidlineLeft;
                timerText.fontSize = 44f;
                timerText.fontStyle = FontStyles.Bold;
                timerText.color = Color.white;
                timerText.textWrappingMode = TextWrappingModes.NoWrap;
                timerText.overflowMode = TextOverflowModes.Overflow;

                LayoutRebuilder.ForceRebuildLayoutImmediate(rootRt);
                Debug.Log("[QuizManager] ⏱️ Đã gom TIME và Text Timer vào giữa màn hình tuyệt đối!");
            }
        }

        /// <summary>
        /// Ẩn object và cả parent chứa nó (thường là Image nền)
        /// </summary>
        private void HideWithParents(Transform t)
        {
            t.gameObject.SetActive(false);
            if (t.parent != null)
            {
                // Nếu parent là Image hoặc chỉ chứa 1-2 con (layout nhóm nhỏ), ẩn luôn parent
                if (t.parent.GetComponent<Image>() != null && t.parent.childCount <= 3)
                {
                    t.parent.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Chuẩn hóa UI cho 4 đáp án: dịch sang phải và in đậm
        /// </summary>
        private void NormalizeAnswerUI()
        {
            Canvas targetCanvas = quizCanvas != null ? quizCanvas : FindFirstObjectByType<Canvas>();

            // Đảm bảo answerButtonImages[3] tìm đúng Circle D nếu nó bị tách
            if (answerButtonImages[3] == null && targetCanvas != null)
            {
                foreach (var img in targetCanvas.GetComponentsInChildren<Image>(true))
                {
                    RectTransform rt = img.GetComponent<RectTransform>();
                    if (rt != null && Mathf.Abs(rt.anchoredPosition.x - 171f) < 20f && Mathf.Abs(rt.anchoredPosition.y - (-430f)) < 30f)
                    {
                        answerButtonImages[3] = img;
                        Button btn = img.GetComponent<Button>();
                        if (btn == null) btn = img.gameObject.AddComponent<Button>();
                        answerButtons[3] = btn;
                        break;
                    }
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (answerTexts[i] != null)
                {
                    var tmp = answerTexts[i];

                    // Nếu có hình tròn tâm bắn tương ứng, gắn text làm con của hình tròn
                    if (answerButtonImages[i] != null)
                    {
                        tmp.transform.SetParent(answerButtonImages[i].transform, false);

                        RectTransform rt = tmp.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            rt.anchorMin = new Vector2(0.5f, 0.5f);
                            rt.anchorMax = new Vector2(0.5f, 0.5f);
                            rt.pivot = new Vector2(0f, 0.5f); // Căn lề trái từ tâm hình tròn
                            rt.anchoredPosition = new Vector2(75f, 0f); // Dịch hẳn sang phải cách tâm 75px (cách mép tròn 25px)
                            rt.sizeDelta = new Vector2(650f, 80f);
                        }
                    }

                    // Bật auto-sizing để text tự co giãn vừa khung
                    tmp.enableAutoSizing = true;
                    tmp.fontSizeMin = 22f;
                    tmp.fontSizeMax = 28f;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.fontWeight = FontWeight.Bold;

                    // Fix text overflow
                    tmp.textWrappingMode = TextWrappingModes.Normal;
                    tmp.overflowMode = TextOverflowModes.Ellipsis;

                    // Căn trái, giữa dọc
                    tmp.alignment = TextAlignmentOptions.MidlineLeft;
                    tmp.margin = Vector4.zero;

                    // Chữ TRẮNG ĐẬM, viền đen sắc nét nổi bật trên nền gỗ nâu
                    tmp.color = Color.white;
                    tmp.outlineWidth = 0.25f;
                    tmp.outlineColor = new Color32(20, 20, 20, 255);

                    Debug.Log($"[QuizManager] 🎨 Đã chuẩn hóa UI đáp án {(char)('A' + i)}: Dịch sang phải và in đậm");
                }

                // Chuẩn hóa màu nút (để 4 hình tròn đều giống nhau ban đầu)
                if (answerButtonImages[i] != null)
                {
                    answerButtonImages[i].color = normalColor;
                }
            }

            // Chuẩn hóa question text
            if (questionText != null)
            {
                questionText.enableAutoSizing = true;
                questionText.fontSizeMin = 16f;
                questionText.fontSizeMax = 32f;
                questionText.textWrappingMode = TextWrappingModes.Normal;
                questionText.overflowMode = TextOverflowModes.Ellipsis;
                questionText.alignment = TextAlignmentOptions.Center;
                questionText.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            }

            // Chuẩn hóa Timer text để trên 1 dòng
            if (timerText != null)
            {
                timerText.textWrappingMode = TextWrappingModes.NoWrap;
                timerText.overflowMode = TextOverflowModes.Overflow;
            }
        }

        /// <summary>
        /// Sửa lỗi Hierarchy cho Result Board bằng cách tự động gom nhóm các object rải rác.
        /// </summary>
        private void FixResultBoardHierarchy()
        {
            if (resultPanel == null || resultPanel.transform.parent == null) return;
            Transform canvas = resultPanel.transform.parent;
            int panelIndex = resultPanel.transform.GetSiblingIndex();

            // Lấy tất cả các object nằm dưới Panel_ResultBoard trong cùng Canvas
            List<Transform> floatingElements = new List<Transform>();
            for (int i = panelIndex + 1; i < canvas.childCount; i++)
            {
                floatingElements.Add(canvas.GetChild(i));
            }

            // Chuyển toàn bộ tụi nó vào làm con của Panel_ResultBoard
            foreach (var element in floatingElements)
            {
                element.SetParent(resultPanel.transform, true);
            }

            if (floatingElements.Count > 0)
            {
                Debug.Log($"[QuizManager] 🛠️ Đã gom tự động {floatingElements.Count} object rải rác vào Panel_ResultBoard!");
            }
        }

        /// <summary>
        /// Tìm kiếm đệ quy trong hierarchy theo tên GameObject
        /// </summary>
        private Transform FindDeepChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                    return child;
                Transform found = FindDeepChild(child, childName);
                if (found != null)
                    return found;
            }
            return null;
        }

        /// <summary>
        /// Log tóm tắt các UI đã tìm được
        /// </summary>
        private void LogUIAssignmentSummary()
        {
            Debug.Log("=== [QuizManager] TÓM TẮT UI ASSIGNMENT ===");
            Debug.Log($"  Question Text: {(questionText != null ? "✅" : "❌")}");
            Debug.Log($"  Timer Text: {(timerText != null ? "✅" : "❌")}");
            Debug.Log($"  Answer Button A: {(answerButtons[0] != null ? "✅" : "❌")}");
            Debug.Log($"  Answer Button B: {(answerButtons[1] != null ? "✅" : "❌")}");
            Debug.Log($"  Answer Button C: {(answerButtons[2] != null ? "✅" : "❌")}");
            Debug.Log($"  Answer Button D: {(answerButtons[3] != null ? "✅" : "❌")}");
            Debug.Log($"  Result Panel: {(resultPanel != null ? "✅" : "❌")}");
            Debug.Log($"  Retry Button: {(retryButton != null ? "✅" : "❌")}");
            Debug.Log($"  Detail Button: {(detailButton != null ? "✅" : "❌")}");
            Debug.Log($"  Score Text: {(resultScoreText != null ? "✅" : "❌")}");
            Debug.Log("============================================");
        }

        #endregion

        #region ===== SETUP =====

        /// <summary>
        /// Thiết lập QuizTimer component
        /// </summary>
        private void SetupTimer()
        {
            quizTimer = gameObject.GetComponent<QuizTimer>();
            if (quizTimer == null)
            {
                quizTimer = gameObject.AddComponent<QuizTimer>();
            }

            if (timerText != null)
            {
                quizTimer.SetTimerText(timerText);
            }

            quizTimer.OnTimeUp += OnTimerExpired;
        }

        /// <summary>
        /// Gắn sự kiện OnClick cho các buttons
        /// </summary>
        private void SetupButtonListeners()
        {
            // Gắn listener cho 4 nút đáp án
            for (int i = 0; i < 4; i++)
            {
                if (answerButtons[i] != null)
                {
                    int index = i; // Capture local variable cho closure
                    answerButtons[i].onClick.RemoveAllListeners();
                    answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
                }
            }

            // Nút Làm Lại
            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(OnRetryClicked);
            }

            // Nút Xem chi tiết (trước là "Về chọn chương")
            if (detailButton != null)
            {
                detailButton.onClick.RemoveAllListeners();
                detailButton.onClick.AddListener(OnDetailClicked);
            }

            // Tạo nút Nộp Bài nếu chưa có
            if (submitButton != null)
            {
                submitButton.onClick.RemoveAllListeners();
                submitButton.onClick.AddListener(OnSubmitClicked);
            }
        }

        #endregion

        #region ===== QUIZ FLOW =====

        /// <summary>
        /// Bắt đầu một lượt quiz mới
        /// </summary>
        public void StartQuiz()
        {
            Debug.Log("[QuizManager] 🎮 Bắt đầu Quiz mới!");

            // Reset trạng thái
            currentQuestionIndex = 0;
            totalScore = 0;
            correctCount = 0;
            incorrectCount = 0;
            isQuizActive = true;

            // Lấy câu hỏi
            if (!string.IsNullOrEmpty(SelectedChapter))
            {
                currentQuestions = dataBank.GetRandomQuestionsByChapter(SelectedChapter, questionsPerQuiz);
                Debug.Log($"[QuizManager] Đã lấy {currentQuestions.Count} câu hỏi từ chương: {SelectedChapter}");
            }
            else
            {
                currentQuestions = dataBank.GetRandomQuestions(questionsPerQuiz);
                Debug.Log($"[QuizManager] Đã lấy {currentQuestions.Count} câu hỏi ngẫu nhiên");
            }

            if (currentQuestions == null || currentQuestions.Count == 0)
            {
                Debug.LogError("[QuizManager] Không lấy được câu hỏi!");
                return;
            }

            // Khởi tạo mảng lưu đáp án
            playerAnswers = new int[currentQuestions.Count];
            for (int i = 0; i < playerAnswers.Length; i++)
            {
                playerAnswers[i] = -1; // Chưa chọn
            }

            // Ẩn bảng kết quả và review panel
            if (resultPanel != null)
                resultPanel.SetActive(false);
            if (reviewPanel != null)
                reviewPanel.SetActive(false);
            if (resultExtraButtonsObj != null)
                resultExtraButtonsObj.SetActive(false);

            // Gắn button listeners
            SetupButtonListeners();

            // Hiển thị câu hỏi đầu tiên
            DisplayQuestion(currentQuestionIndex);

            // Bắt đầu đếm ngược 15 phút
            quizTimer.StartTimer(quizDuration);

            Canvas curCanvas = quizCanvas != null ? quizCanvas : (resultPanel != null ? resultPanel.GetComponentInParent<Canvas>() : FindFirstObjectByType<Canvas>());
            if (curCanvas != null)
            {
                Transform timerRoot = curCanvas.transform.Find("Dynamic_QuizTimerCenterRoot");
                if (timerRoot != null)
                    timerRoot.gameObject.SetActive(true);
            }

            // ẨN ĐIỂM khi đang thi (chỉ hiện sau khi nộp bài)
            if (scoreText != null)
            {
                scoreText.text = $"Câu 1/{currentQuestions.Count}";
            }

            // Tạo thanh điều hướng (Prev, Submit, Next)
            CreateNavigationButtons();

            // Tạo text hiển thị số câu hỏi (Câu X/Y) ở góc trên trái
            CreateQuestionCounter();
        }

        // Text hiển thị "Câu X/Y"
        private TextMeshProUGUI questionCounterLabel;
        private GameObject questionCounterObj;
        private GameObject navPanelObj;
        private GameObject resultExtraButtonsObj;

        /// <summary>
        /// Tạo text hiển thị "Câu 1/15" ở góc trên trái
        /// </summary>
        private void CreateQuestionCounter()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            if (questionCounterObj != null)
            {
                Destroy(questionCounterObj);
            }

            questionCounterObj = new GameObject("QuestionCounter");
            questionCounterObj.transform.SetParent(canvas.transform, false);

            RectTransform rt = questionCounterObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(20f, -15f);
            rt.sizeDelta = new Vector2(200f, 40f);

            questionCounterLabel = questionCounterObj.AddComponent<TextMeshProUGUI>();
            questionCounterLabel.text = $"Câu 1/{currentQuestions.Count}";
            questionCounterLabel.fontSize = 24f;
            questionCounterLabel.fontStyle = FontStyles.Bold;
            questionCounterLabel.color = Color.white;
            questionCounterLabel.outlineWidth = 0.4f;
            questionCounterLabel.outlineColor = new Color32(0, 0, 0, 220);
            questionCounterLabel.alignment = TextAlignmentOptions.MidlineLeft;
            questionCounterLabel.textWrappingMode = TextWrappingModes.NoWrap;

            Debug.Log("[QuizManager] ✅ Đã tạo Question Counter");
        }

        // Tạo nút Next/Prev
        private Button prevButton;
        private Button nextQuestionButton; // Dùng biến khác tên với nextButton có sẵn để khỏi trùng lặp

        /// <summary>
        /// Tạo thanh điều hướng: Câu Trước, Nộp Bài, Câu Tiếp
        /// </summary>
        private void CreateNavigationButtons()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            // Xóa nút Nộp Bài cũ nếu có
            if (submitButton != null && submitButton.name == "Btn_Submit")
            {
                Destroy(submitButton.gameObject);
            }

            if (navPanelObj != null)
            {
                Destroy(navPanelObj);
            }

            // Tạo Panel chứa 3 nút ở dưới cùng
            navPanelObj = new GameObject("NavPanel");
            navPanelObj.transform.SetParent(canvas.transform, false);
            RectTransform navRt = navPanelObj.AddComponent<RectTransform>();
            navRt.anchorMin = new Vector2(0.5f, 0f);
            navRt.anchorMax = new Vector2(0.5f, 0f);
            navRt.pivot = new Vector2(0.5f, 0f);
            navRt.anchoredPosition = new Vector2(0f, 20f);
            navRt.sizeDelta = new Vector2(600f, 60f); // Chiều rộng 600

            // Tạo nút Prev
            prevButton = CreateNavBtn(navPanelObj, "Btn_Prev", "⏪ Câu Trước", new Vector2(-200f, 0f), new Color(0.8f, 0.4f, 0f, 1f));
            prevButton.onClick.AddListener(() => {
                if (currentQuestionIndex > 0)
                {
                    currentQuestionIndex--;
                    DisplayQuestion(currentQuestionIndex);
                }
            });

            // Tạo nút Submit ở giữa
            submitButton = CreateNavBtn(navPanelObj, "Btn_Submit", "📤 NỘP BÀI", new Vector2(0f, 0f), new Color(0.2f, 0.7f, 0.3f, 1f));
            submitButton.onClick.AddListener(OnSubmitClicked);

            // Tạo nút Next
            nextQuestionButton = CreateNavBtn(navPanelObj, "Btn_Next", "Câu Tiếp ⏩", new Vector2(200f, 0f), new Color(0.1f, 0.5f, 0.8f, 1f));
            nextQuestionButton.onClick.AddListener(() => {
                if (currentQuestionIndex < currentQuestions.Count - 1)
                {
                    currentQuestionIndex++;
                    DisplayQuestion(currentQuestionIndex);
                }
            });

            Debug.Log("[QuizManager] ✅ Đã tạo thanh điều hướng (Prev, Submit, Next)");
        }

        private Button CreateNavBtn(GameObject parent, string name, string text, Vector2 pos, Color color)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent.transform, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(180f, 50f);

            Image img = btnObj.AddComponent<Image>();
            img.color = color;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 20f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return btn;
        }

        /// <summary>
        /// Hiển thị câu hỏi theo index lên UI
        /// </summary>
        private void DisplayQuestion(int index)
        {
            if (index < 0 || index >= currentQuestions.Count)
            {
                Debug.LogWarning($"[QuizManager] Index câu hỏi không hợp lệ: {index}");
                return;
            }

            QuestionData question = currentQuestions[index];

            // Cập nhật trạng thái nút Prev/Next
            if (prevButton != null) prevButton.interactable = (index > 0);
            if (nextQuestionButton != null) nextQuestionButton.interactable = (index < currentQuestions.Count - 1);

            // Cập nhật question counter "Câu X/Y"
            if (questionCounterLabel != null)
            {
                questionCounterLabel.text = $"Câu {index + 1}/{currentQuestions.Count}";
            }

            // Hiển thị nội dung câu hỏi
            if (questionText != null)
            {
                questionText.text = question.questionText;
            }

            // Hiển thị 4 đáp án (in đậm rõ nét)
            for (int i = 0; i < 4; i++)
            {
                if (answerTexts[i] != null && i < question.answers.Length)
                {
                    string ansText = question.answers[i];
                    if (!ansText.StartsWith("<b>"))
                    {
                        ansText = $"<b>{ansText}</b>";
                    }
                    answerTexts[i].text = ansText;
                    answerTexts[i].fontStyle = FontStyles.Bold;
                    answerTexts[i].fontWeight = FontWeight.Bold;
                }

                // Reset màu về mặc định
                if (answerButtonImages[i] != null)
                {
                    answerButtonImages[i].color = normalColor;
                }

                // Bật lại tương tác
                if (answerButtons[i] != null)
                {
                    answerButtons[i].interactable = true;
                }
            }

            // Nếu đã chọn đáp án trước đó (khi quay lại câu), highlight lại
            if (playerAnswers[index] >= 0 && playerAnswers[index] < 4)
            {
                if (answerButtonImages[playerAnswers[index]] != null)
                {
                    answerButtonImages[playerAnswers[index]].color = selectedColor;
                }
            }

            Debug.Log($"[QuizManager] 📋 Hiển thị câu {index + 1}/{currentQuestions.Count}: {question.questionID}");
        }

        /// <summary>
        /// Xử lý khi người chơi chọn 1 đáp án
        /// </summary>
        private void OnAnswerSelected(int answerIndex)
        {
            if (!isQuizActive || isShowingFeedback)
                return;

            // Lưu đáp án người chơi
            playerAnswers[currentQuestionIndex] = answerIndex;

            if (IsDuckShootingMode)
            {
                // Trong chế độ Bắn Vịt: hiện phản hồi đúng/sai ngay lập tức trên UI gốc của QuizScene!
                bool isCorrect = (answerIndex == currentQuestions[currentQuestionIndex].correctAnswerIndex);

                // Khóa 4 nút đáp án
                for (int i = 0; i < 4; i++)
                {
                    if (answerButtons[i] != null)
                        answerButtons[i].interactable = false;
                }

                if (isCorrect)
                {
                    if (answerButtonImages[answerIndex] != null)
                        answerButtonImages[answerIndex].color = correctColor; // Xanh lá

                    totalScore += pointsPerCorrectAnswer; // +10 điểm
                    correctCount++;

                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.AddEXP(pointsPerCorrectAnswer); // +10 EXP
                    }
                    Debug.Log($"[QuizManager] 🎯 Bắn Vịt: Trả lời ĐÚNG câu {currentQuestionIndex + 1}! (+10 điểm, +10 EXP)");
                }
                else
                {
                    if (answerButtonImages[answerIndex] != null)
                        answerButtonImages[answerIndex].color = incorrectColor; // Đỏ

                    int correctIdx = currentQuestions[currentQuestionIndex].correctAnswerIndex;
                    if (correctIdx >= 0 && correctIdx < 4 && answerButtonImages[correctIdx] != null)
                        answerButtonImages[correctIdx].color = correctColor; // Xanh lá đáp án đúng

                    incorrectCount++;
                    Debug.Log($"[QuizManager] ❌ Bắn Vịt: Trả lời SAI câu {currentQuestionIndex + 1}! (0 điểm)");
                }

                if (scoreText != null)
                {
                    scoreText.text = totalScore.ToString();
                }

                ShowDuckContinueButton();
                return;
            }

            // Reset tất cả nút về màu mặc định
            for (int i = 0; i < 4; i++)
            {
                if (answerButtonImages[i] != null)
                {
                    answerButtonImages[i].color = normalColor;
                }
            }

            // Highlight đáp án đã chọn bằng màu xanh
            if (answerButtonImages[answerIndex] != null)
            {
                answerButtonImages[answerIndex].color = selectedColor;
            }

            Debug.Log($"[QuizManager] 🔘 Chọn đáp án {(char)('A' + answerIndex)} cho câu {currentQuestionIndex + 1}");
        }

        private void ShowDuckContinueButton()
        {
            if (quizCanvas == null)
            {
                quizCanvas = FindFirstObjectByType<Canvas>();
            }

            if (duckContinueButton == null && quizCanvas != null)
            {
                GameObject btnObj = new GameObject("Btn_DuckContinue");
                btnObj.transform.SetParent(quizCanvas.transform, false);

                RectTransform rt = btnObj.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.43f);
                rt.anchorMax = new Vector2(0.5f, 0.43f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 0f);
                rt.sizeDelta = new Vector2(320f, 60f);

                Image img = btnObj.AddComponent<Image>();
                Sprite buttonSprite = (retryButton != null && retryButton.GetComponent<Image>() != null)
                    ? retryButton.GetComponent<Image>().sprite
                    : null;
                if (buttonSprite != null)
                {
                    img.sprite = buttonSprite;
                    img.type = Image.Type.Sliced;
                }
                img.color = new Color(0.16f, 0.68f, 0.32f, 1f);

                Outline outline = btnObj.AddComponent<Outline>();
                outline.effectColor = new Color(0.08f, 0.40f, 0.18f, 1f);
                outline.effectDistance = new Vector2(2f, -2f);

                duckContinueButton = btnObj.AddComponent<Button>();
                duckContinueButton.targetGraphic = img;

                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(btnObj.transform, false);
                RectTransform textRt = textObj.AddComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;

                TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = "TIẾP TỤC BẮN VỊT";
                tmp.fontSize = 20f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.Center;

                duckContinueButton.onClick.AddListener(OnDuckContinueClicked);
            }

            if (duckContinueButton != null)
            {
                RectTransform rt = duckContinueButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0.5f, 0.43f);
                    rt.anchorMax = new Vector2(0.5f, 0.43f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(0f, 0f);
                    rt.sizeDelta = new Vector2(320f, 60f);
                }

                duckContinueButton.gameObject.SetActive(true);
                duckContinueButton.transform.SetAsLastSibling();
                TextMeshProUGUI btnText = duckContinueButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = (currentQuestionIndex >= currentQuestions.Count - 1)
                        ? "XEM KẾT QUẢ"
                        : "TIẾP TỤC BẮN VỊT";
                }
            }
        }

        private void OnDuckContinueClicked()
        {
            if (duckContinueButton != null)
                duckContinueButton.gameObject.SetActive(false);

            if (currentQuestionIndex >= currentQuestions.Count - 1)
            {
                // Đã hoàn thành cả 10 câu -> Hiện Panel_ResultBoard dựng sẵn của QuizScene!
                EndQuiz();
            }
            else
            {
                // Ẩn QuizScene để quay lại màn Bắn Vịt săn câu tiếp theo
                SetQuizVisible(false);
                if (PhysicsShootingGallery.Instance != null)
                {
                    PhysicsShootingGallery.Instance.OnReturnFromQuestion(totalScore);
                }
            }
        }

        /// <summary>
        /// Hiệu ứng feedback đáp án: highlight đúng (xanh), sai (đỏ), sau đó chuyển câu
        /// (Chỉ dùng trong review, không dùng khi đang thi)
        /// </summary>
        private IEnumerator ShowAnswerFeedback(int selectedIndex, int correctIndex)
        {
            isShowingFeedback = true;

            // Vô hiệu hóa tất cả buttons
            for (int i = 0; i < 4; i++)
            {
                if (answerButtons[i] != null)
                    answerButtons[i].interactable = false;
            }

            // Highlight đáp án đúng = xanh
            if (answerButtonImages[correctIndex] != null)
            {
                answerButtonImages[correctIndex].color = correctColor;
            }

            // Nếu chọn sai, highlight đáp án đã chọn = đỏ
            if (selectedIndex != correctIndex && answerButtonImages[selectedIndex] != null)
            {
                answerButtonImages[selectedIndex].color = incorrectColor;
            }

            // Đợi một chút để người chơi thấy feedback
            yield return new WaitForSeconds(delayBetweenQuestions);

            isShowingFeedback = false;

            // Chuyển sang câu tiếp theo
            currentQuestionIndex++;

            if (currentQuestionIndex < currentQuestions.Count)
            {
                DisplayQuestion(currentQuestionIndex);
            }
            else
            {
                // Hết câu hỏi → kết thúc quiz
                EndQuiz();
            }
        }

        /// <summary>
        /// Xử lý khi hết giờ (Timer expired) → TỰ ĐỘNG NỘP BÀI
        /// </summary>
        private void OnTimerExpired()
        {
            Debug.Log("[QuizManager] ⏰ Hết giờ! Tự động nộp bài.");
            EndQuiz();
        }

        /// <summary>
        /// Xử lý khi người chơi ấn nút "Nộp bài"
        /// </summary>
        private void OnSubmitClicked()
        {
            if (!isQuizActive) return;

            Debug.Log("[QuizManager] 📝 Người chơi nộp bài sớm.");
            EndQuiz();
        }

        /// <summary>
        /// Kết thúc quiz và hiển thị kết quả
        /// </summary>
        /// <summary>
        /// Kết thúc quiz và hiển thị kết quả
        /// </summary>
        private void EndQuiz()
        {
            if (!isQuizActive) return;

            isQuizActive = false;
            StopAllCoroutines(); // Dừng feedback nếu đang chạy

            // Dừng timer
            if (quizTimer != null)
            {
                quizTimer.StopTimer();
            }

            // ===== TÍNH ĐIỂM TẠI ĐÂY (sau khi nộp bài) =====
            correctCount = 0;
            incorrectCount = 0;
            totalScore = 0;

            for (int i = 0; i < currentQuestions.Count; i++)
            {
                if (playerAnswers[i] >= 0 && playerAnswers[i] == currentQuestions[i].correctAnswerIndex)
                {
                    correctCount++;
                    totalScore += pointsPerCorrectAnswer;
                }
                else
                {
                    incorrectCount++;
                }
            }

            int unanswered = 0;
            for (int i = 0; i < currentQuestions.Count; i++)
            {
                if (playerAnswers[i] < 0) unanswered++;
            }
            if (unanswered > 0)
            {
                Debug.Log($"[QuizManager] ⚠️ Có {unanswered} câu chưa trả lời (tính là sai).");
            }

            Debug.Log($"[QuizManager] 🏆 Kết thúc Quiz! Điểm: {totalScore}/{currentQuestions.Count * pointsPerCorrectAnswer}, Đúng: {correctCount}, Sai: {incorrectCount}");

            // Ẩn nút Nộp Bài
            if (submitButton != null)
            {
                submitButton.gameObject.SetActive(false);
            }

            // Cộng phần thưởng
            GiveRewards();

            // Hiển thị bảng kết quả
            ShowResultPanel();

            if (GoogleSheetDataManager.Instance != null)
            {
                string userID = GameSession.Instance != null && !string.IsNullOrEmpty(GameSession.Instance.username) ? GameSession.Instance.username : "HS001";
                string monID = "KHTN8"; 
                string baiID = PlayerPrefs.GetString("QuizLesson", "B1_001");
                if (baiID.Contains("Bài 1") || baiID.Contains("Bai 1")) baiID = "B1_001";
                else if (baiID.Contains("Bài 2") || baiID.Contains("Bai 2")) baiID = "B1_002";
                else if (baiID.Contains("Bài 3") || baiID.Contains("Bai 3")) baiID = "B1_003";
                else baiID = "B1_001";
                float accuracy = currentQuestions.Count > 0 ? (float)correctCount / currentQuestions.Count : 0f;
                StartCoroutine(GoogleSheetDataManager.Instance.SubmitProgressToSheet(userID, monID, baiID, totalScore, accuracy));
            }
        }

        /// <summary>
        /// Cộng Coins và EXP vào GameManager
        /// </summary>
        private void GiveRewards()
        {
            if (GameManager.Instance != null)
            {
                // Luôn cộng 50 xu khi hoàn thành quiz
                GameManager.Instance.AddCoins(rewardCoins);
                Debug.Log($"[QuizManager] 💰 +{rewardCoins} Coins");

                // EXP bonus tỷ lệ với điểm
                int earnedEXP = (int)(rewardEXP * ((float)correctCount / currentQuestions.Count));
                if (earnedEXP > 0)
                {
                    GameManager.Instance.AddEXP(earnedEXP);
                    Debug.Log($"[QuizManager] ⭐ +{earnedEXP} EXP");
                }
            }
            else
            {
                Debug.LogWarning("[QuizManager] GameManager.Instance không tồn tại. Không thể cộng phần thưởng.");
            }
        }

        /// <summary>
        /// Tải Sprite từ thư mục Assets khi runtime
        /// </summary>
        private Sprite LoadSpriteFromDisk(string relativePath)
        {
            try
            {
                string fullPath = System.IO.Path.Combine(Application.dataPath, relativePath);
                if (System.IO.File.Exists(fullPath))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (tex.LoadImage(bytes))
                    {
                        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[QuizManager] LoadSpriteFromDisk error: {e.Message}");
            }
            return null;
        }

        /// <summary>
        /// Hiển thị Panel kết quả
        /// </summary>
        private void ShowResultPanel()
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
            }

            if (quizTimer != null)
            {
                quizTimer.StopTimer();
            }

            // Ẩn cụm timer trên đầu khi đang ở màn hình kết quả
            Canvas canvas = quizCanvas != null ? quizCanvas : (resultPanel != null ? resultPanel.GetComponentInParent<Canvas>() : FindFirstObjectByType<Canvas>());
            if (canvas != null)
            {
                Transform timerRoot = canvas.transform.Find("Dynamic_QuizTimerCenterRoot");
                if (timerRoot != null)
                    timerRoot.gameObject.SetActive(false);
            }

            int totalQ = currentQuestions != null && currentQuestions.Count > 0 ? currentQuestions.Count : 10;
            int maxScore = totalQ * pointsPerCorrectAnswer;

            // Đảm bảo các component text được gán chính xác trên Panel_ResultBoard
            if (resultPanel != null)
            {
                TextMeshProUGUI[] panelTexts = resultPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in panelTexts)
                {
                    string txt = t.text.Trim();
                    RectTransform rt = t.GetComponent<RectTransform>();
                    float posY = rt != null ? rt.anchoredPosition.y : 0f;

                    if (txt.Contains("HOÀN THÀNH") || txt.Contains("HO\u00c0N TH\u00c0NH") || posY > 250f)
                    {
                        if (resultTitleText == null || resultTitleText == resultScoreText)
                            resultTitleText = t;
                    }
                    else if (txt.Contains("TỔNG ĐIỂM") || txt.Contains("T\u1ed4NG \u0110I\u1ec2M") || (posY > 50f && posY < 200f))
                    {
                        resultScoreText = t;
                    }
                    else if (txt.Contains("ĐÚNG") || txt.Contains("\u0110\u00daNG"))
                    {
                        resultCorrectText = t;
                    }
                    else if (txt.Contains("SAI"))
                    {
                        resultIncorrectText = t;
                    }
                }
            }

            // Cập nhật text trên bảng kết quả ngay lập tức
            if (resultTitleText != null)
            {
                resultTitleText.margin = Vector4.zero;
                resultTitleText.alignment = TextAlignmentOptions.Center;
                resultTitleText.text = "<b>HOÀN THÀNH!</b>";
            }

            if (resultScoreText != null)
            {
                resultScoreText.text = $"<b><color=#4E342E><size=95%>TỔNG ĐIỂM: </size></color><size=140%><color=#D84315><b>{totalScore}</b></color></size><color=#5D4037><size=95%> / {maxScore} ĐIỂM</size></color></b>";
            }

            if (resultCorrectText != null)
            {
                resultCorrectText.text = $"<b><color=#166534>ĐÚNG: {correctCount} / {totalQ}</color></b>";
            }

            if (resultIncorrectText != null)
            {
                resultIncorrectText.text = $"<b><color=#991B1B>SAI: {incorrectCount} / {totalQ}</color></b>";
            }

            // Vô hiệu hóa các nút đáp án phía sau
            for (int i = 0; i < 4; i++)
            {
                if (answerButtons[i] != null)
                    answerButtons[i].interactable = false;
            }

            // Ẩn thanh điều hướng và question counter
            if (navPanelObj != null)
                navPanelObj.SetActive(false);
            if (questionCounterObj != null)
                questionCounterObj.SetActive(false);

            // Thiết lập và tạo nút "XEM CHI TIẾT" đồng bộ 3 cột phía dưới
            SetupResultBoardButtons();

            // Tinh chỉnh bố cục và mỹ thuật cho bảng kết quả (đẹp mắt, hiện đại, nổi bật)
            StyleResultBoard(totalQ, maxScore);
        }

        /// <summary>
        /// Tạo nút "XEM CHI TIẾT" và bố trí 3 nút bấm thành 3 cột đồng bộ dưới đáy bảng
        /// </summary>
        private void SetupResultBoardButtons()
        {
            if (resultPanel == null) return;

            // Lấy sprite 9-slice chuẩn từ retryButton
            Sprite sharedButtonSprite = null;
            if (retryButton != null && retryButton.GetComponent<Image>() != null)
            {
                sharedButtonSprite = retryButton.GetComponent<Image>().sprite;
            }

            // 1. Nút "LÀM LẠI" (Cột 1 - Bên trái)
            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(OnRetryClicked);
                RectTransform retryRt = retryButton.GetComponent<RectTransform>();
                if (retryRt != null)
                {
                    retryRt.anchorMin = new Vector2(0.5f, 0.5f);
                    retryRt.anchorMax = new Vector2(0.5f, 0.5f);
                    retryRt.pivot = new Vector2(0.5f, 0.5f);
                    retryRt.anchoredPosition = new Vector2(-330f, -325f);
                    retryRt.sizeDelta = new Vector2(290f, 68f);
                }
                Image retryImg = retryButton.GetComponent<Image>();
                if (retryImg != null)
                {
                    retryImg.type = Image.Type.Sliced;
                    retryImg.color = new Color(0.96f, 0.62f, 0.12f, 1f); // Màu cam vàng ấm áp (#F59E0B)
                }
                TextMeshProUGUI retryTxt = retryButton.GetComponentInChildren<TextMeshProUGUI>(true);
                if (retryTxt != null)
                {
                    retryTxt.text = "<b>LÀM LẠI</b>";
                    retryTxt.fontSize = 24f;
                    retryTxt.fontStyle = FontStyles.Bold;
                    retryTxt.color = Color.white;
                    retryTxt.alignment = TextAlignmentOptions.Center;
                }
            }

            // 2. Nút "XEM CHI TIẾT" (Cột 2 - Ở giữa)
            if (resultExtraButtonsObj == null)
            {
                resultExtraButtonsObj = new GameObject("ExtraResultButtons", typeof(RectTransform));
                resultExtraButtonsObj.transform.SetParent(resultPanel.transform, false);
            }
            resultExtraButtonsObj.SetActive(true);

            RectTransform revRootRt = resultExtraButtonsObj.GetComponent<RectTransform>();
            revRootRt.anchorMin = new Vector2(0.5f, 0.5f);
            revRootRt.anchorMax = new Vector2(0.5f, 0.5f);
            revRootRt.pivot = new Vector2(0.5f, 0.5f);
            revRootRt.anchoredPosition = new Vector2(0f, -325f);
            revRootRt.sizeDelta = new Vector2(290f, 68f);

            Transform revBtnTrans = resultExtraButtonsObj.transform.Find("Btn_ShowReview");
            GameObject revBtnObj = revBtnTrans != null ? revBtnTrans.gameObject : null;
            if (revBtnObj == null)
            {
                Button createdBtn = CreateNavBtn(resultExtraButtonsObj, "Btn_ShowReview", "<b>XEM CHI TIẾT</b>", Vector2.zero, new Color(0.15f, 0.45f, 0.92f, 1f));
                revBtnObj = createdBtn.gameObject;
            }

            RectTransform btnRt = revBtnObj.GetComponent<RectTransform>();
            if (btnRt != null)
            {
                btnRt.anchoredPosition = Vector2.zero;
                btnRt.sizeDelta = new Vector2(290f, 68f);
            }

            Image revImg = revBtnObj.GetComponent<Image>();
            if (revImg != null)
            {
                if (sharedButtonSprite != null)
                    revImg.sprite = sharedButtonSprite;
                revImg.type = Image.Type.Sliced;
                revImg.color = new Color(0.15f, 0.45f, 0.92f, 1f); // Xanh dương tươi sáng (#2563EB)
            }

            TextMeshProUGUI revTmp = revBtnObj.GetComponentInChildren<TextMeshProUGUI>(true);
            if (revTmp != null)
            {
                revTmp.text = "<b>XEM CHI TIẾT</b>";
                revTmp.fontSize = 24f;
                revTmp.fontStyle = FontStyles.Bold;
                revTmp.color = Color.white;
                revTmp.alignment = TextAlignmentOptions.Center;
            }

            Button reviewBtn = revBtnObj.GetComponent<Button>();
            if (reviewBtn != null)
            {
                reviewBtn.onClick.RemoveAllListeners();
                reviewBtn.onClick.AddListener(() => {
                    Debug.Log("[QuizManager] 📖 Người chơi bấm nút XEM CHI TIẾT!");
                    ShowReviewPanel();
                });
            }

            // 3. Nút "VỀ CHỌN CHƯƠNG" (Cột 3 - Bên phải)
            Button[] allResultButtons = resultPanel.GetComponentsInChildren<Button>(true);
            foreach (var b in allResultButtons)
            {
                if (b != retryButton && (resultExtraButtonsObj == null || !b.transform.IsChildOf(resultExtraButtonsObj.transform)))
                {
                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(() => {
                        Debug.Log("[QuizManager] 📚 Quay về màn chọn chương (ChapterList)");
                        PhysicsMenuManager.SelectedChapterName = "";
                        SelectedChapter = "";
                        if (SceneLoader.Instance != null)
                            SceneLoader.Instance.LoadScene("ChapterList");
                        else
                            SceneManager.LoadScene("ChapterList");
                    });

                    RectTransform backRt = b.GetComponent<RectTransform>();
                    if (backRt != null)
                    {
                        backRt.anchorMin = new Vector2(0.5f, 0.5f);
                        backRt.anchorMax = new Vector2(0.5f, 0.5f);
                        backRt.pivot = new Vector2(0.5f, 0.5f);
                        backRt.anchoredPosition = new Vector2(330f, -325f);
                        backRt.sizeDelta = new Vector2(290f, 68f);
                    }
                    Image backImg = b.GetComponent<Image>();
                    if (backImg != null)
                    {
                        if (sharedButtonSprite != null)
                            backImg.sprite = sharedButtonSprite;
                        backImg.type = Image.Type.Sliced;
                        backImg.color = new Color(0.10f, 0.65f, 0.35f, 1f); // Xanh lục bảo (#10B981)
                    }
                    TextMeshProUGUI backTxt = b.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (backTxt != null)
                    {
                        backTxt.text = "<b>VỀ CHỌN CHƯƠNG</b>";
                        backTxt.fontSize = 24f;
                        backTxt.fontStyle = FontStyles.Bold;
                        backTxt.color = Color.white;
                        backTxt.alignment = TextAlignmentOptions.Center;
                    }
                }
            }

            resultExtraButtonsObj.transform.SetAsLastSibling();
        }

        /// <summary>
        /// Tạo hoặc cập nhật huy hiệu Đúng / Sai có icon và nền pill đẹp mắt
        /// </summary>
        private void CreateOrUpdateResultBadges(Transform parentCard, int correctCount, int incorrectCount, int totalQ)
        {
            try
            {
                Sprite checkSprite = LoadSpriteFromDisk("Art/Icons/iconCheck_grey.png");
                Sprite crossSprite = LoadSpriteFromDisk("Art/Icons/iconCross_grey.png");
                Sprite pillSprite = retryButton != null && retryButton.GetComponent<Image>() != null ? retryButton.GetComponent<Image>().sprite : null;

                // --- 1. Badge ĐÚNG ---
                Transform existingCorrect = parentCard.Find("Badge_ResultCorrect");
                GameObject correctObj = existingCorrect != null ? existingCorrect.gameObject : new GameObject("Badge_ResultCorrect", typeof(RectTransform), typeof(Image));
                correctObj.transform.SetParent(parentCard, false);
                RectTransform cRt = correctObj.GetComponent<RectTransform>();
                cRt.anchorMin = new Vector2(0.5f, 0.5f);
                cRt.anchorMax = new Vector2(0.5f, 0.5f);
                cRt.pivot = new Vector2(0.5f, 0.5f);
                cRt.anchoredPosition = new Vector2(-200f, -50f);
                cRt.sizeDelta = new Vector2(330f, 75f);

                Image cBg = correctObj.GetComponent<Image>();
                cBg.color = new Color(0.88f, 0.96f, 0.90f, 1f); // Nền xanh bạc hà nhạt
                if (pillSprite != null)
                {
                    cBg.sprite = pillSprite;
                    cBg.type = Image.Type.Sliced;
                }
                Outline cOutline = correctObj.GetComponent<Outline>() ?? correctObj.AddComponent<Outline>();
                cOutline.effectColor = new Color(0.20f, 0.65f, 0.32f, 0.9f);
                cOutline.effectDistance = new Vector2(2f, -2f);

                // Icon Check
                Transform cIconTrans = correctObj.transform.Find("Icon_Check");
                GameObject cIconObj = cIconTrans != null ? cIconTrans.gameObject : new GameObject("Icon_Check", typeof(RectTransform), typeof(Image));
                cIconObj.transform.SetParent(correctObj.transform, false);
                RectTransform ciRt = cIconObj.GetComponent<RectTransform>();
                ciRt.anchorMin = new Vector2(0f, 0.5f);
                ciRt.anchorMax = new Vector2(0f, 0.5f);
                ciRt.pivot = new Vector2(0.5f, 0.5f);
                ciRt.anchoredPosition = new Vector2(36f, 0f);
                ciRt.sizeDelta = new Vector2(40f, 40f);
                Image ciImg = cIconObj.GetComponent<Image>();
                if (checkSprite != null)
                {
                    ciImg.sprite = checkSprite;
                    ciImg.color = new Color(0.12f, 0.68f, 0.28f, 1f);
                }

                if (resultCorrectText != null)
                {
                    resultCorrectText.transform.SetParent(correctObj.transform, false);
                    RectTransform rt = resultCorrectText.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = new Vector2(70f, 0f);
                    rt.offsetMax = new Vector2(-10f, 0f);
                    resultCorrectText.alignment = TextAlignmentOptions.MidlineLeft;
                    resultCorrectText.fontSize = 26f;
                    resultCorrectText.fontStyle = FontStyles.Bold;
                    resultCorrectText.text = $"<b><color=#166534>ĐÚNG: {correctCount} / {totalQ}</color></b>";
                }

                // --- 2. Badge SAI ---
                Transform existingIncorrect = parentCard.Find("Badge_ResultIncorrect");
                GameObject incorrectObj = existingIncorrect != null ? existingIncorrect.gameObject : new GameObject("Badge_ResultIncorrect", typeof(RectTransform), typeof(Image));
                incorrectObj.transform.SetParent(parentCard, false);
                RectTransform iRt = incorrectObj.GetComponent<RectTransform>();
                iRt.anchorMin = new Vector2(0.5f, 0.5f);
                iRt.anchorMax = new Vector2(0.5f, 0.5f);
                iRt.pivot = new Vector2(0.5f, 0.5f);
                iRt.anchoredPosition = new Vector2(200f, -50f);
                iRt.sizeDelta = new Vector2(330f, 75f);

                Image iBg = incorrectObj.GetComponent<Image>();
                iBg.color = new Color(0.99f, 0.89f, 0.89f, 1f); // Nền hồng nhạt
                if (pillSprite != null)
                {
                    iBg.sprite = pillSprite;
                    iBg.type = Image.Type.Sliced;
                }
                Outline iOutline = incorrectObj.GetComponent<Outline>() ?? incorrectObj.AddComponent<Outline>();
                iOutline.effectColor = new Color(0.85f, 0.28f, 0.28f, 0.9f);
                iOutline.effectDistance = new Vector2(2f, -2f);

                // Icon Cross
                Transform iIconTrans = incorrectObj.transform.Find("Icon_Cross");
                GameObject iIconObj = iIconTrans != null ? iIconTrans.gameObject : new GameObject("Icon_Cross", typeof(RectTransform), typeof(Image));
                iIconObj.transform.SetParent(incorrectObj.transform, false);
                RectTransform iiRt = iIconObj.GetComponent<RectTransform>();
                iiRt.anchorMin = new Vector2(0f, 0.5f);
                iiRt.anchorMax = new Vector2(0f, 0.5f);
                iiRt.pivot = new Vector2(0.5f, 0.5f);
                iiRt.anchoredPosition = new Vector2(36f, 0f);
                iiRt.sizeDelta = new Vector2(40f, 40f);
                Image iiImg = iIconObj.GetComponent<Image>();
                if (crossSprite != null)
                {
                    iiImg.sprite = crossSprite;
                    iiImg.color = new Color(0.85f, 0.20f, 0.20f, 1f);
                }

                if (resultIncorrectText != null)
                {
                    resultIncorrectText.transform.SetParent(incorrectObj.transform, false);
                    RectTransform rt = resultIncorrectText.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = new Vector2(70f, 0f);
                    rt.offsetMax = new Vector2(-10f, 0f);
                    resultIncorrectText.alignment = TextAlignmentOptions.MidlineLeft;
                    resultIncorrectText.fontSize = 26f;
                    resultIncorrectText.fontStyle = FontStyles.Bold;
                    resultIncorrectText.text = $"<b><color=#991B1B>SAI: {incorrectCount} / {totalQ}</color></b>";
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[QuizManager] CreateOrUpdateResultBadges error: {ex.Message}\n{ex.StackTrace}");
                if (resultCorrectText != null)
                {
                    resultCorrectText.transform.SetParent(parentCard, false);
                    RectTransform rt = resultCorrectText.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(-200f, -50f);
                    rt.sizeDelta = new Vector2(300f, 70f);
                    resultCorrectText.alignment = TextAlignmentOptions.Center;
                    resultCorrectText.fontSize = 26f;
                    resultCorrectText.fontStyle = FontStyles.Bold;
                    resultCorrectText.text = $"<b><color=#166534>ĐÚNG: {correctCount} / {totalQ}</color></b>";
                }
                if (resultIncorrectText != null)
                {
                    resultIncorrectText.transform.SetParent(parentCard, false);
                    RectTransform rt = resultIncorrectText.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(200f, -50f);
                    rt.sizeDelta = new Vector2(300f, 70f);
                    resultIncorrectText.alignment = TextAlignmentOptions.Center;
                    resultIncorrectText.fontSize = 26f;
                    resultIncorrectText.fontStyle = FontStyles.Bold;
                    resultIncorrectText.text = $"<b><color=#991B1B>SAI: {incorrectCount} / {totalQ}</color></b>";
                }
            }
        }

        /// <summary>
        /// Tinh chỉnh giao diện bảng kết quả: 9-slice bo tròn trắng ngà/vàng kem, viền nâu, bóng đổ, gradient
        /// </summary>
        private void StyleResultBoard(int totalQ, int maxScore)
        {
            if (resultPanel == null) return;

            // 1. Panel Overlay (lớp phủ nền mờ nhẹ giúp popup nổi bật)
            Image panelImg = resultPanel.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.color = new Color(0f, 0f, 0f, 0.40f);
            }

            // 2. Tìm thẻ Card chính giữa (Ban đầu có size ~1462x496)
            Image cardImg = null;
            Image[] allImages = resultPanel.GetComponentsInChildren<Image>(true);
            foreach (var img in allImages)
            {
                if (img.gameObject == resultPanel) continue;
                if (img.GetComponent<Button>() != null || img.transform.parent.GetComponent<Button>() != null) continue;

                RectTransform rt = img.GetComponent<RectTransform>();
                if (rt != null && rt.sizeDelta.x > 700f && rt.sizeDelta.y > 300f && rt.sizeDelta.y < 850f)
                {
                    cardImg = img;
                    break;
                }
            }

            if (cardImg != null)
            {
                RectTransform cardRt = cardImg.GetComponent<RectTransform>();
                cardRt.anchorMin = new Vector2(0.5f, 0.5f);
                cardRt.anchorMax = new Vector2(0.5f, 0.5f);
                cardRt.pivot = new Vector2(0.5f, 0.5f);
                cardRt.anchoredPosition = new Vector2(0f, 15f);
                cardRt.sizeDelta = new Vector2(980f, 440f);

                // Màu trắng ngà / vàng kem sáng sang trọng
                cardImg.color = new Color(0.99f, 0.97f, 0.91f, 1f); // #FFFDF0
                cardImg.type = Image.Type.Sliced;
                cardImg.pixelsPerUnitMultiplier = 1.0f;

                // Thêm viền nâu gỗ đậm (Outline)
                Outline cardOutline = cardImg.GetComponent<Outline>() ?? cardImg.gameObject.AddComponent<Outline>();
                cardOutline.effectColor = new Color(0.40f, 0.22f, 0.08f, 1f); // Nâu gỗ đậm
                cardOutline.effectDistance = new Vector2(3.5f, -3.5f);

                // Thêm lớp bóng đổ (Drop Shadow) màu đen mờ phía sau
                Transform existingShadow = resultPanel.transform.Find("ResultCard_DropShadow");
                GameObject shadowObj = existingShadow != null ? existingShadow.gameObject : new GameObject("ResultCard_DropShadow", typeof(RectTransform), typeof(Image));
                shadowObj.transform.SetParent(resultPanel.transform, false);
                shadowObj.transform.SetSiblingIndex(cardImg.transform.GetSiblingIndex());

                RectTransform sRt = shadowObj.GetComponent<RectTransform>();
                sRt.anchorMin = new Vector2(0.5f, 0.5f);
                sRt.anchorMax = new Vector2(0.5f, 0.5f);
                sRt.pivot = new Vector2(0.5f, 0.5f);
                sRt.anchoredPosition = new Vector2(0f, 7f); // Lệch xuống 8px
                sRt.sizeDelta = new Vector2(1004f, 460f);   // Đổ bóng lan tỏa

                Image sImg = shadowObj.GetComponent<Image>();
                sImg.sprite = cardImg.sprite;
                sImg.type = Image.Type.Sliced;
                sImg.color = new Color(0.10f, 0.06f, 0.03f, 0.50f);

                // Tạo các badge Đúng/Sai có icon bên trong thẻ Card
                CreateOrUpdateResultBadges(cardImg.transform, correctCount, incorrectCount, totalQ);
            }

            // 3. Tiêu đề "HOÀN THÀNH!" với Color Gradient vàng -> cam và Outline sắc nét
            if (resultTitleText != null)
            {
                resultTitleText.transform.SetParent(resultPanel.transform, false);
                RectTransform titleRt = resultTitleText.GetComponent<RectTransform>();
                if (titleRt != null)
                {
                    titleRt.anchorMin = new Vector2(0.5f, 0.5f);
                    titleRt.anchorMax = new Vector2(0.5f, 0.5f);
                    titleRt.pivot = new Vector2(0.5f, 0.5f);
                    titleRt.anchoredPosition = new Vector2(0f, 335f); // Chính giữa trục ngang X = 0
                    titleRt.sizeDelta = new Vector2(800f, 90f);
                }
                resultTitleText.margin = Vector4.zero; // Xóa triệt để margin lệch phải từ scene gốc
                resultTitleText.alignment = TextAlignmentOptions.Center;
                resultTitleText.fontSize = 54f;
                resultTitleText.fontStyle = FontStyles.Bold;
                resultTitleText.text = "<b>HOÀN THÀNH!</b>";

                // Gradient từ vàng sáng chuyển dần sang cam rực rỡ
                resultTitleText.enableVertexGradient = true;
                Color topYellow = new Color(1f, 0.95f, 0.35f, 1f);    // #FFF359
                Color bottomOrange = new Color(1f, 0.42f, 0.05f, 1f);  // #FF6B0D
                resultTitleText.colorGradient = new VertexGradient(topYellow, topYellow, bottomOrange, bottomOrange);

                // Viền chữ nâu đậm tạo khối nổi bật
                Outline titleOutline = resultTitleText.GetComponent<Outline>() ?? resultTitleText.gameObject.AddComponent<Outline>();
                titleOutline.effectColor = new Color(0.35f, 0.12f, 0.02f, 1f);
                titleOutline.effectDistance = new Vector2(3f, -3f);
            }

            // 4. "TỔNG ĐIỂM: XX / YY ĐIỂM" tương phản cao trên nền giấy sáng
            if (resultScoreText != null)
            {
                if (cardImg != null)
                    resultScoreText.transform.SetParent(cardImg.transform, false);

                RectTransform scoreRt = resultScoreText.GetComponent<RectTransform>();
                if (scoreRt != null)
                {
                    scoreRt.anchorMin = new Vector2(0.5f, 0.5f);
                    scoreRt.anchorMax = new Vector2(0.5f, 0.5f);
                    scoreRt.pivot = new Vector2(0.5f, 0.5f);
                    scoreRt.anchoredPosition = new Vector2(0f, 65f);
                    scoreRt.sizeDelta = new Vector2(850f, 80f);
                }
                resultScoreText.alignment = TextAlignmentOptions.Center;
                resultScoreText.fontSize = 38f;
                resultScoreText.fontStyle = FontStyles.Bold;

                // Tương phản cao: Chữ nâu đậm, con số cam cháy/đỏ sẫm nổi bật
                resultScoreText.text = $"<b><color=#4E342E><size=95%>TỔNG ĐIỂM: </size></color><size=140%><color=#D84315><b>{totalScore}</b></color></size><color=#5D4037><size=95%> / {maxScore} ĐIỂM</size></color></b>";
            }
        }

        #endregion

        #region ===== BUTTON HANDLERS =====

        /// <summary>
        /// Xử lý nút "Làm Lại" - Reset và bắt đầu quiz mới
        /// </summary>
        private void OnRetryClicked()
        {
            Debug.Log("[QuizManager] 🔄 Làm lại Quiz!");

            // Ẩn review panel nếu đang mở
            if (reviewPanel != null)
                reviewPanel.SetActive(false);

            if (IsDuckShootingMode)
            {
                SetQuizVisible(false);
                if (PhysicsShootingGallery.Instance != null)
                {
                    PhysicsShootingGallery.Instance.StartGame();
                }
                return;
            }

            StartQuiz();
        }

        /// <summary>
        /// Xử lý nút "Xem chi tiết" - Mở panel review chi tiết đáp án
        /// </summary>
        private void OnDetailClicked()
        {
            Debug.Log("[QuizManager] 📖 Mở xem chi tiết đáp án.");
            ShowReviewPanel();
        }

        /// <summary>
        /// Xử lý nút "Quay lại" - Quay về scene trước
        /// </summary>
        private void OnGoBackClicked()
        {
            Debug.Log("[QuizManager] ↩️ Quay lại scene trước.");

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadPhysics();
            }
            else
            {
                SceneManager.LoadScene("Physics");
            }
        }

        #endregion

        #region ===== REVIEW PANEL (XEM CHI TIẾT) =====

        /// <summary>
        /// Tạo và hiển thị panel xem chi tiết đáp án.
        /// Panel được tạo hoàn toàn bằng code (không sửa file .unity).
        /// Hiển thị từng câu một với nút Trước/Sau để điều hướng.
        /// </summary>
        private void ShowReviewPanel()
        {
            // Ẩn result panel
            if (resultPanel != null)
                resultPanel.SetActive(false);

            // Tạo review panel nếu chưa có
            if (reviewPanel == null)
            {
                CreateReviewPanel();
            }

            reviewCurrentIndex = 0;
            reviewPanel.SetActive(true);
            DisplayReviewQuestion(reviewCurrentIndex);
        }

        /// <summary>
        /// <summary>
        /// Tạo panel xem chi tiết đáp án bằng code - UI đẹp, bo góc 9-slice, phong cách Circus đồng bộ
        /// </summary>
        private void CreateReviewPanel()
        {
            Canvas canvas = quizCanvas != null ? quizCanvas : (resultPanel != null ? resultPanel.GetComponentInParent<Canvas>() : FindFirstObjectByType<Canvas>());
            if (canvas == null) return;

            Sprite sharedButtonSprite = (retryButton != null && retryButton.GetComponent<Image>() != null)
                ? retryButton.GetComponent<Image>().sprite
                : null;
            Sprite checkSprite = LoadSpriteFromDisk("Art/Icons/iconCheck_grey.png");
            Sprite crossSprite = LoadSpriteFromDisk("Art/Icons/iconCross_grey.png");

            // === 1. PANEL CHÍNH - LỚP PHỦ NỀN TỐI MỜ GIÚP CARD NỔI BẬT ===
            reviewPanel = new GameObject("Panel_ReviewDetail", typeof(RectTransform), typeof(Image));
            reviewPanel.transform.SetParent(canvas.transform, false);
            reviewPanel.transform.SetAsLastSibling();

            RectTransform panelRect = reviewPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelBg = reviewPanel.GetComponent<Image>();
            panelBg.color = new Color(0.06f, 0.08f, 0.12f, 0.88f);

            // === 2. LỚP BÓNG ĐỔ (DROP SHADOW) CHO THẺ CARD ===
            GameObject shadowObj = new GameObject("ReviewCard_Shadow", typeof(RectTransform), typeof(Image));
            shadowObj.transform.SetParent(reviewPanel.transform, false);
            RectTransform shadowRt = shadowObj.GetComponent<RectTransform>();
            shadowRt.anchorMin = new Vector2(0.5f, 0.5f);
            shadowRt.anchorMax = new Vector2(0.5f, 0.5f);
            shadowRt.pivot = new Vector2(0.5f, 0.5f);
            shadowRt.anchoredPosition = new Vector2(0f, -8f);
            shadowRt.sizeDelta = new Vector2(1184f, 664f);

            Image shadowImg = shadowObj.GetComponent<Image>();
            if (sharedButtonSprite != null)
            {
                shadowImg.sprite = sharedButtonSprite;
                shadowImg.type = Image.Type.Sliced;
            }
            shadowImg.color = new Color(0f, 0f, 0f, 0.45f);

            // === 3. THẺ CHÍNH (CARD) - TRẮNG NGÀ BO GÓC 9-SLICE ===
            GameObject card = new GameObject("ReviewCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(reviewPanel.transform, false);
            RectTransform cardRt = card.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.anchoredPosition = Vector2.zero;
            cardRt.sizeDelta = new Vector2(1160f, 640f);

            Image cardImg = card.GetComponent<Image>();
            if (sharedButtonSprite != null)
            {
                cardImg.sprite = sharedButtonSprite;
                cardImg.type = Image.Type.Sliced;
            }
            cardImg.color = new Color(0.99f, 0.98f, 0.95f, 1f); // Nền trắng ngà dịu mắt

            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.38f, 0.22f, 0.10f, 1f); // Viền nâu đậm sang trọng
            cardOutline.effectDistance = new Vector2(3f, -3f);

            // === 4. THANH HEADER - XANH DA TRỜI CIRCUS RỰC RỠ (#248CEB) ===
            GameObject header = new GameObject("Header", typeof(RectTransform), typeof(Image));
            header.transform.SetParent(card.transform, false);
            RectTransform headerRt = header.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0.5f, 1f);
            headerRt.anchorMax = new Vector2(0.5f, 1f);
            headerRt.pivot = new Vector2(0.5f, 1f);
            headerRt.anchoredPosition = Vector2.zero;
            headerRt.sizeDelta = new Vector2(1160f, 68f);

            Image headerImg = header.GetComponent<Image>();
            if (sharedButtonSprite != null)
            {
                headerImg.sprite = sharedButtonSprite;
                headerImg.type = Image.Type.Sliced;
            }
            headerImg.color = new Color(0.14f, 0.55f, 0.92f, 1f); // Xanh da trời Circus

            // Tiêu đề Header
            CreateTextObject(header.transform, "ReviewTitle",
                "<b>XEM CHI TIẾT ĐÁP ÁN</b>",
                new Vector2(0f, 0f), new Vector2(0.55f, 1f),
                25, FontStyles.Bold, Color.white, TextAlignmentOptions.MidlineLeft,
                new Vector2(35f, 0f), new Vector2(-10f, 0f));

            // Số thứ tự câu (Câu 1/10)
            CreateTextObject(header.transform, "ReviewCounter",
                "<b>Câu 1/10</b>",
                new Vector2(0.55f, 0f), new Vector2(0.83f, 1f),
                20, FontStyles.Bold, new Color(0.90f, 0.96f, 1f, 1f), TextAlignmentOptions.MidlineRight,
                new Vector2(0f, 0f), new Vector2(-15f, 0f));

            // Nút "Đóng" màu cam ấm góc trên phải Header (dùng text thuần ĐÓNG tránh lỗi ô vuông font chữ)
            CreateButton(header.transform, "Btn_ReviewClose", "<b>ĐÓNG</b>",
                new Vector2(0.85f, 0.16f), new Vector2(0.98f, 0.84f),
                new Color(0.96f, 0.62f, 0.15f, 1f), OnCloseReviewPanel, sharedButtonSprite, Color.white, 18);

            // === 5. KHUNG CÂU HỎI & STATUS STAMP ===
            GameObject qSection = new GameObject("QuestionSection", typeof(RectTransform), typeof(Image));
            qSection.transform.SetParent(card.transform, false);
            RectTransform qRt = qSection.GetComponent<RectTransform>();
            qRt.anchorMin = new Vector2(0.5f, 1f);
            qRt.anchorMax = new Vector2(0.5f, 1f);
            qRt.pivot = new Vector2(0.5f, 1f);
            qRt.anchoredPosition = new Vector2(0f, -80f);
            qRt.sizeDelta = new Vector2(1110f, 76f);

            Image qBg = qSection.GetComponent<Image>();
            if (sharedButtonSprite != null)
            {
                qBg.sprite = sharedButtonSprite;
                qBg.type = Image.Type.Sliced;
            }
            qBg.color = Color.white;

            Outline qOutline = qSection.AddComponent<Outline>();
            qOutline.effectColor = new Color(0.80f, 0.84f, 0.88f, 1f);
            qOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Text câu hỏi (căn giữa theo chiều dọc, chừa khoảng trống cho Status Tag bên phải)
            GameObject qTextObj = new GameObject("ReviewQuestion", typeof(RectTransform), typeof(TextMeshProUGUI));
            qTextObj.transform.SetParent(qSection.transform, false);
            RectTransform qtRt = qTextObj.GetComponent<RectTransform>();
            qtRt.anchorMin = Vector2.zero;
            qtRt.anchorMax = Vector2.one;
            qtRt.offsetMin = new Vector2(24f, 6f);
            qtRt.offsetMax = new Vector2(-205f, -6f);

            TextMeshProUGUI qTmp = qTextObj.GetComponent<TextMeshProUGUI>();
            qTmp.fontSize = 18f;
            qTmp.fontStyle = FontStyles.Bold;
            qTmp.color = new Color(0.12f, 0.16f, 0.24f, 1f);
            qTmp.alignment = TextAlignmentOptions.MidlineLeft;
            qTmp.textWrappingMode = TextWrappingModes.Normal;
            qTmp.overflowMode = TextOverflowModes.Ellipsis;

            // Status Stamp / Tag (Góc trên phải khung câu hỏi)
            GameObject statusTag = new GameObject("ReviewStatusTag", typeof(RectTransform), typeof(Image));
            statusTag.transform.SetParent(qSection.transform, false);
            RectTransform stRt = statusTag.GetComponent<RectTransform>();
            stRt.anchorMin = new Vector2(1f, 0.5f);
            stRt.anchorMax = new Vector2(1f, 0.5f);
            stRt.pivot = new Vector2(1f, 0.5f);
            stRt.anchoredPosition = new Vector2(-15f, 0f);
            stRt.sizeDelta = new Vector2(180f, 48f);

            Image stBg = statusTag.GetComponent<Image>();
            if (sharedButtonSprite != null)
            {
                stBg.sprite = sharedButtonSprite;
                stBg.type = Image.Type.Sliced;
            }
            stBg.color = new Color(0.88f, 0.98f, 0.91f, 1f);

            Outline stOutline = statusTag.AddComponent<Outline>();
            stOutline.effectColor = new Color(0.13f, 0.77f, 0.37f, 1f);
            stOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Icon trong Status Tag
            GameObject stIconObj = new GameObject("StatusIcon", typeof(RectTransform), typeof(Image));
            stIconObj.transform.SetParent(statusTag.transform, false);
            RectTransform stiRt = stIconObj.GetComponent<RectTransform>();
            stiRt.anchorMin = new Vector2(0f, 0.5f);
            stiRt.anchorMax = new Vector2(0f, 0.5f);
            stiRt.pivot = new Vector2(0.5f, 0.5f);
            stiRt.anchoredPosition = new Vector2(24f, 0f);
            stiRt.sizeDelta = new Vector2(24f, 24f);
            Image stiImg = stIconObj.GetComponent<Image>();
            if (checkSprite != null) stiImg.sprite = checkSprite;

            // Text trong Status Tag
            GameObject stTextObj = new GameObject("StatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
            stTextObj.transform.SetParent(statusTag.transform, false);
            RectTransform sttRt = stTextObj.GetComponent<RectTransform>();
            sttRt.anchorMin = Vector2.zero;
            sttRt.anchorMax = Vector2.one;
            sttRt.offsetMin = new Vector2(44f, 0f);
            sttRt.offsetMax = new Vector2(-8f, 0f);
            TextMeshProUGUI stTmp = stTextObj.GetComponent<TextMeshProUGUI>();
            stTmp.fontSize = 16.5f;
            stTmp.fontStyle = FontStyles.Bold;
            stTmp.alignment = TextAlignmentOptions.MidlineLeft;
            stTmp.text = "<b>ĐÚNG (+10đ)</b>";
            stTmp.color = new Color(0.08f, 0.50f, 0.24f, 1f);

            // === 6. LƯỚI 4 ĐÁP ÁN (2x2 GRID) - CHIỀU CAO 58PX ĐỦ RỘNG CHO 2 DÒNG CHỮ ===
            Vector2[] optionPositions = new Vector2[]
            {
                new Vector2(-282f, -166f), // Đáp án A
                new Vector2(282f, -166f),  // Đáp án B
                new Vector2(-282f, -230f), // Đáp án C
                new Vector2(282f, -230f)   // Đáp án D
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject optCard = new GameObject($"ReviewOption_{i}", typeof(RectTransform), typeof(Image));
                optCard.transform.SetParent(card.transform, false);
                RectTransform ocRt = optCard.GetComponent<RectTransform>();
                ocRt.anchorMin = new Vector2(0.5f, 1f);
                ocRt.anchorMax = new Vector2(0.5f, 1f);
                ocRt.pivot = new Vector2(0.5f, 1f);
                ocRt.anchoredPosition = optionPositions[i];
                ocRt.sizeDelta = new Vector2(544f, 58f);

                Image ocBg = optCard.GetComponent<Image>();
                if (sharedButtonSprite != null)
                {
                    ocBg.sprite = sharedButtonSprite;
                    ocBg.type = Image.Type.Sliced;
                }
                ocBg.color = new Color(0.97f, 0.98f, 0.99f, 1f);

                Outline ocOutline = optCard.AddComponent<Outline>();
                ocOutline.effectColor = new Color(0.85f, 0.88f, 0.92f, 1f);
                ocOutline.effectDistance = new Vector2(1.5f, -1.5f);

                // Text nội dung phương án
                GameObject optTextObj = new GameObject("OptionText", typeof(RectTransform), typeof(TextMeshProUGUI));
                optTextObj.transform.SetParent(optCard.transform, false);
                RectTransform otRt = optTextObj.GetComponent<RectTransform>();
                otRt.anchorMin = Vector2.zero;
                otRt.anchorMax = Vector2.one;
                otRt.offsetMin = new Vector2(16f, 0f);
                otRt.offsetMax = new Vector2(-95f, 0f); // Tối ưu chiều rộng để không bị cắt chữ

                TextMeshProUGUI otTmp = optTextObj.GetComponent<TextMeshProUGUI>();
                otTmp.fontSize = 15f;
                otTmp.fontStyle = FontStyles.Bold;
                otTmp.color = new Color(0.20f, 0.25f, 0.33f, 1f);
                otTmp.alignment = TextAlignmentOptions.MidlineLeft;
                otTmp.textWrappingMode = TextWrappingModes.Normal;
                otTmp.overflowMode = TextOverflowModes.Ellipsis;

                // Tag nhãn "(Bạn chọn)"
                GameObject optTagObj = new GameObject("OptionTag", typeof(RectTransform), typeof(TextMeshProUGUI));
                optTagObj.transform.SetParent(optCard.transform, false);
                RectTransform oTagRt = optTagObj.GetComponent<RectTransform>();
                oTagRt.anchorMin = new Vector2(1f, 0.5f);
                oTagRt.anchorMax = new Vector2(1f, 0.5f);
                oTagRt.pivot = new Vector2(1f, 0.5f);
                oTagRt.anchoredPosition = new Vector2(-36f, 0f);
                oTagRt.sizeDelta = new Vector2(88f, 30f);

                TextMeshProUGUI oTagTmp = optTagObj.GetComponent<TextMeshProUGUI>();
                oTagTmp.fontSize = 13.5f;
                oTagTmp.fontStyle = FontStyles.Bold;
                oTagTmp.alignment = TextAlignmentOptions.MidlineRight;
                oTagTmp.text = "<b>(Bạn chọn)</b>";
                optTagObj.SetActive(false);

                // Icon Check / Cross bên phải phương án
                GameObject optIconObj = new GameObject("OptionIcon", typeof(RectTransform), typeof(Image));
                optIconObj.transform.SetParent(optCard.transform, false);
                RectTransform oiRt = optIconObj.GetComponent<RectTransform>();
                oiRt.anchorMin = new Vector2(1f, 0.5f);
                oiRt.anchorMax = new Vector2(1f, 0.5f);
                oiRt.pivot = new Vector2(0.5f, 0.5f);
                oiRt.anchoredPosition = new Vector2(-16f, 0f);
                oiRt.sizeDelta = new Vector2(22f, 22f);
                optIconObj.SetActive(false);
            }

            // === 7. KHUNG GIẢI THÍCH CHI TIẾT (MÀU TRẮNG NGÀ / VÀNG KEM DỊU MẮT) ===
            GameObject explSection = new GameObject("ExplainSection", typeof(RectTransform), typeof(Image));
            explSection.transform.SetParent(card.transform, false);
            RectTransform explRt = explSection.GetComponent<RectTransform>();
            explRt.anchorMin = new Vector2(0.5f, 1f);
            explRt.anchorMax = new Vector2(0.5f, 1f);
            explRt.pivot = new Vector2(0.5f, 1f);
            explRt.anchoredPosition = new Vector2(0f, -298f);
            explRt.sizeDelta = new Vector2(1110f, 225f);

            Image explBg = explSection.GetComponent<Image>();
            if (sharedButtonSprite != null)
            {
                explBg.sprite = sharedButtonSprite;
                explBg.type = Image.Type.Sliced;
            }
            explBg.color = new Color(1f, 0.992f, 0.949f, 1f); // #FFFDF2 dịu mắt

            Outline explOutline = explSection.AddComponent<Outline>();
            explOutline.effectColor = new Color(0.92f, 0.82f, 0.68f, 1f); // Viền nâu vàng đất nhạt
            explOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Tiêu đề nhỏ "LỜI GIẢI CHI TIẾT:"
            GameObject explTitleObj = new GameObject("ExplainTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            explTitleObj.transform.SetParent(explSection.transform, false);
            RectTransform etRt = explTitleObj.GetComponent<RectTransform>();
            etRt.anchorMin = new Vector2(0f, 1f);
            etRt.anchorMax = new Vector2(1f, 1f);
            etRt.pivot = new Vector2(0f, 1f);
            etRt.anchoredPosition = new Vector2(24f, -14f);
            etRt.sizeDelta = new Vector2(600f, 28f);

            TextMeshProUGUI etTmp = explTitleObj.GetComponent<TextMeshProUGUI>();
            etTmp.fontSize = 17f;
            etTmp.fontStyle = FontStyles.Bold;
            etTmp.text = "<b><color=#B45309>LỜI GIẢI CHI TIẾT:</color></b>";
            etTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Nội dung giải thích
            GameObject explTextObj = new GameObject("ReviewExplanation", typeof(RectTransform), typeof(TextMeshProUGUI));
            explTextObj.transform.SetParent(explSection.transform, false);
            RectTransform exRt = explTextObj.GetComponent<RectTransform>();
            exRt.anchorMin = Vector2.zero;
            exRt.anchorMax = Vector2.one;
            exRt.offsetMin = new Vector2(24f, 12f);
            exRt.offsetMax = new Vector2(-24f, -42f);

            TextMeshProUGUI exTmp = explTextObj.GetComponent<TextMeshProUGUI>();
            exTmp.fontSize = 16f;
            exTmp.fontStyle = FontStyles.Normal;
            exTmp.color = new Color(0.27f, 0.10f, 0.01f, 1f); // Nâu ấm tương phản cao, dễ đọc
            exTmp.alignment = TextAlignmentOptions.TopLeft;
            exTmp.textWrappingMode = TextWrappingModes.Normal;
            exTmp.overflowMode = TextOverflowModes.Ellipsis;

            // === 8. THANH ĐIỀU HƯỚNG DƯỚI CÙNG (NAVIGATION BAR) ===
            GameObject navSection = new GameObject("NavigationSection", typeof(RectTransform));
            navSection.transform.SetParent(card.transform, false);
            RectTransform navRt = navSection.GetComponent<RectTransform>();
            navRt.anchorMin = new Vector2(0.5f, 0f);
            navRt.anchorMax = new Vector2(0.5f, 0f);
            navRt.pivot = new Vector2(0.5f, 0f);
            navRt.anchoredPosition = new Vector2(0f, 28f);
            navRt.sizeDelta = new Vector2(1110f, 62f);

            // Nút "< Câu trước" (Dạng Outline trắng - viền xanh)
            GameObject prevBtnObj = CreateButton(navSection.transform, "Btn_ReviewPrev", "<b>< Câu trước</b>",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Color.white, () => NavigateReview(-1), sharedButtonSprite, new Color(0.11f, 0.31f, 0.85f, 1f), 19);
            RectTransform pRt = prevBtnObj.GetComponent<RectTransform>();
            pRt.anchoredPosition = new Vector2(-360f, 0f);
            pRt.sizeDelta = new Vector2(220f, 52f);
            Outline prevOutline = prevBtnObj.AddComponent<Outline>();
            prevOutline.effectColor = new Color(0.23f, 0.51f, 0.96f, 1f);
            prevOutline.effectDistance = new Vector2(2f, -2f);

            // Nút "VỀ BẢNG ĐIỂM" (Nút chính nổi bật to nhất ở giữa - Màu cam Circus #F59E0B)
            GameObject centerBtnObj = CreateButton(navSection.transform, "Btn_ReviewBackToResult", "<b>VỀ BẢNG ĐIỂM</b>",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Color(0.96f, 0.62f, 0.12f, 1f), OnCloseReviewPanel, sharedButtonSprite, Color.white, 22);
            RectTransform cRt = centerBtnObj.GetComponent<RectTransform>();
            cRt.anchoredPosition = new Vector2(0f, 0f);
            cRt.sizeDelta = new Vector2(290f, 62f);
            Outline centerOutline = centerBtnObj.AddComponent<Outline>();
            centerOutline.effectColor = new Color(0.47f, 0.21f, 0.06f, 1f);
            centerOutline.effectDistance = new Vector2(2.5f, -2.5f);

            // Nút "Câu tiếp >" (Dạng Outline trắng - viền xanh)
            GameObject nextBtnObj = CreateButton(navSection.transform, "Btn_ReviewNext", "<b>Câu tiếp ></b>",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Color.white, () => NavigateReview(1), sharedButtonSprite, new Color(0.11f, 0.31f, 0.85f, 1f), 19);
            RectTransform nRt = nextBtnObj.GetComponent<RectTransform>();
            nRt.anchoredPosition = new Vector2(360f, 0f);
            nRt.sizeDelta = new Vector2(220f, 52f);
            Outline nextOutline = nextBtnObj.AddComponent<Outline>();
            nextOutline.effectColor = new Color(0.23f, 0.51f, 0.96f, 1f);
            nextOutline.effectDistance = new Vector2(2f, -2f);

            Debug.Log("[QuizManager] ✅ Đã tạo Panel_ReviewDetail với bố cục và màu sắc mới chuẩn Circus 9-Slice");
        }

        /// <summary>
        /// Hiển thị chi tiết 1 câu hỏi trong review panel với bố cục 2x2 grid và thẻ trạng thái
        /// </summary>
        private void DisplayReviewQuestion(int index)
        {
            if (currentQuestions == null || index < 0 || index >= currentQuestions.Count)
                return;

            QuestionData q = currentQuestions[index];
            int playerAns = playerAnswers[index];
            bool isCorrect = (playerAns == q.correctAnswerIndex);

            Sprite checkSprite = LoadSpriteFromDisk("Art/Icons/iconCheck_grey.png");
            Sprite crossSprite = LoadSpriteFromDisk("Art/Icons/iconCross_grey.png");

            // 1. Cập nhật số thứ tự câu & nội dung câu hỏi
            SetReviewText("ReviewCounter", $"<b>Câu {index + 1}/{currentQuestions.Count}</b>");
            SetReviewText("ReviewQuestion", q.questionText);

            // 2. Cập nhật Status Stamp (Góc trên phải)
            Transform statusTrans = FindDeepChild(reviewPanel.transform, "ReviewStatusTag");
            if (statusTrans != null)
            {
                Image stBg = statusTrans.GetComponent<Image>();
                Outline stOutline = statusTrans.GetComponent<Outline>();
                Transform iconTrans = statusTrans.Find("StatusIcon");
                Transform textTrans = statusTrans.Find("StatusText");

                if (isCorrect)
                {
                    if (stBg != null) stBg.color = new Color(0.88f, 0.98f, 0.91f, 1f); // Nền xanh lá pastel
                    if (stOutline != null) stOutline.effectColor = new Color(0.13f, 0.77f, 0.37f, 1f); // Viền xanh lá
                    if (iconTrans != null)
                    {
                        Image img = iconTrans.GetComponent<Image>();
                        if (img != null)
                        {
                            if (checkSprite != null) img.sprite = checkSprite;
                            img.color = new Color(0.08f, 0.50f, 0.24f, 1f);
                        }
                    }
                    if (textTrans != null)
                    {
                        TextMeshProUGUI tmp = textTrans.GetComponent<TextMeshProUGUI>();
                        if (tmp != null)
                        {
                            tmp.text = "<b>ĐÚNG (+10đ)</b>";
                            tmp.color = new Color(0.08f, 0.50f, 0.24f, 1f);
                        }
                    }
                }
                else
                {
                    if (stBg != null) stBg.color = new Color(1f, 0.89f, 0.89f, 1f); // Nền đỏ pastel
                    if (stOutline != null) stOutline.effectColor = new Color(0.94f, 0.27f, 0.27f, 1f); // Viền đỏ
                    if (iconTrans != null)
                    {
                        Image img = iconTrans.GetComponent<Image>();
                        if (img != null)
                        {
                            if (crossSprite != null) img.sprite = crossSprite;
                            img.color = new Color(0.73f, 0.11f, 0.11f, 1f);
                        }
                    }
                    if (textTrans != null)
                    {
                        TextMeshProUGUI tmp = textTrans.GetComponent<TextMeshProUGUI>();
                        if (tmp != null)
                        {
                            tmp.text = "<b>SAI (0đ)</b>";
                            tmp.color = new Color(0.73f, 0.11f, 0.11f, 1f);
                        }
                    }
                }
            }

            // 3. Cập nhật 4 phương án (Lưới 2x2)
            for (int i = 0; i < 4; i++)
            {
                Transform optTrans = FindDeepChild(reviewPanel.transform, $"ReviewOption_{i}");
                if (optTrans == null) continue;

                string ansText = (q.answers != null && i < q.answers.Length) ? q.answers[i] : "";

                Image optBg = optTrans.GetComponent<Image>();
                Outline optOutline = optTrans.GetComponent<Outline>();
                Transform textTrans = optTrans.Find("OptionText");
                Transform tagTrans = optTrans.Find("OptionTag");
                Transform iconTrans = optTrans.Find("OptionIcon");

                TextMeshProUGUI txtTmp = textTrans != null ? textTrans.GetComponent<TextMeshProUGUI>() : null;
                TextMeshProUGUI tagTmp = tagTrans != null ? tagTrans.GetComponent<TextMeshProUGUI>() : null;
                Image iconImg = iconTrans != null ? iconTrans.GetComponent<Image>() : null;

                if (txtTmp != null) txtTmp.text = ansText;

                if (i == q.correctAnswerIndex)
                {
                    // Đáp án đúng: Viền xanh lá, nền xanh nhạt, có tích xanh
                    if (optBg != null) optBg.color = new Color(0.92f, 0.99f, 0.96f, 1f);
                    if (optOutline != null)
                    {
                        optOutline.effectColor = new Color(0.06f, 0.73f, 0.51f, 1f);
                        optOutline.effectDistance = new Vector2(2f, -2f);
                    }
                    if (txtTmp != null) txtTmp.color = new Color(0.02f, 0.37f, 0.27f, 1f);

                    if (iconTrans != null && iconImg != null)
                    {
                        iconTrans.gameObject.SetActive(true);
                        if (checkSprite != null) iconImg.sprite = checkSprite;
                        iconImg.color = new Color(0.06f, 0.73f, 0.51f, 1f);
                    }

                    if (tagTrans != null && tagTmp != null)
                    {
                        if (playerAns == i)
                        {
                            tagTrans.gameObject.SetActive(true);
                            tagTmp.text = "<b>(Bạn chọn)</b>";
                            tagTmp.color = new Color(0.02f, 0.59f, 0.41f, 1f);
                        }
                        else
                        {
                            tagTrans.gameObject.SetActive(false);
                        }
                    }
                }
                else if (i == playerAns && playerAns != q.correctAnswerIndex)
                {
                    // Đáp án người chơi chọn sai: Viền đỏ, nền đỏ nhạt, dấu X và nhãn (Bạn chọn)
                    if (optBg != null) optBg.color = new Color(1f, 0.95f, 0.95f, 1f);
                    if (optOutline != null)
                    {
                        optOutline.effectColor = new Color(0.94f, 0.27f, 0.27f, 1f);
                        optOutline.effectDistance = new Vector2(2f, -2f);
                    }
                    if (txtTmp != null) txtTmp.color = new Color(0.60f, 0.11f, 0.11f, 1f);

                    if (iconTrans != null && iconImg != null)
                    {
                        iconTrans.gameObject.SetActive(true);
                        if (crossSprite != null) iconImg.sprite = crossSprite;
                        iconImg.color = new Color(0.94f, 0.27f, 0.27f, 1f);
                    }

                    if (tagTrans != null && tagTmp != null)
                    {
                        tagTrans.gameObject.SetActive(true);
                        tagTmp.text = "<b>(Bạn chọn)</b>";
                        tagTmp.color = new Color(0.86f, 0.15f, 0.15f, 1f);
                    }
                }
                else
                {
                    // Các phương án trung tính còn lại
                    if (optBg != null) optBg.color = new Color(0.97f, 0.98f, 0.99f, 1f);
                    if (optOutline != null)
                    {
                        optOutline.effectColor = new Color(0.85f, 0.88f, 0.92f, 1f);
                        optOutline.effectDistance = new Vector2(1.5f, -1.5f);
                    }
                    if (txtTmp != null) txtTmp.color = new Color(0.20f, 0.25f, 0.33f, 1f);

                    if (iconTrans != null) iconTrans.gameObject.SetActive(false);
                    if (tagTrans != null) tagTrans.gameObject.SetActive(false);
                }
            }

            // 4. Cập nhật lời giải chi tiết
            string explanation = !string.IsNullOrEmpty(q.explanation)
                ? q.explanation
                : "(Không có lời giải chi tiết cho câu hỏi này.)";
            SetReviewText("ReviewExplanation", explanation);

            // 5. Cập nhật trạng thái kích hoạt của nút Trước / Sau
            UpdateNavigationButtonState("Btn_ReviewPrev", index > 0);
            UpdateNavigationButtonState("Btn_ReviewNext", index < currentQuestions.Count - 1);
        }

        /// <summary>
        /// Cập nhật hiển thị và độ tương tác cho các nút điều hướng (đảm bảo độ tương phản cao, không bị mờ tịt)
        /// </summary>
        private void UpdateNavigationButtonState(string btnName, bool isEnabled)
        {
            Transform btnTrans = FindDeepChild(reviewPanel.transform, btnName);
            if (btnTrans == null) return;

            Button btn = btnTrans.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = isEnabled;
                btn.transition = Selectable.Transition.None; // Tắt ColorTint mặc định của Unity để tránh bị mờ tịt
            }

            Image img = btnTrans.GetComponent<Image>();
            Outline outline = btnTrans.GetComponent<Outline>();
            TextMeshProUGUI txt = btnTrans.GetComponentInChildren<TextMeshProUGUI>(true);

            if (isEnabled)
            {
                if (img != null) img.color = Color.white;
                if (outline != null)
                {
                    outline.effectColor = new Color(0.23f, 0.51f, 0.96f, 1f); // Viền xanh dương
                    outline.enabled = true;
                }
                if (txt != null) txt.color = new Color(0.11f, 0.31f, 0.85f, 1f); // Chữ xanh đậm sắc nét
            }
            else
            {
                // Khi bị disable: nền xám nhẹ, viền xám rõ ràng, chữ xám đậm tương phản cao dễ đọc
                if (img != null) img.color = new Color(0.88f, 0.90f, 0.93f, 1f);
                if (outline != null)
                {
                    outline.effectColor = new Color(0.72f, 0.76f, 0.82f, 1f);
                    outline.enabled = true;
                }
                if (txt != null) txt.color = new Color(0.42f, 0.46f, 0.54f, 1f);
            }
        }

        /// <summary>
        /// Điều hướng qua lại giữa các câu hỏi trong review
        /// </summary>
        private void NavigateReview(int direction)
        {
            reviewCurrentIndex += direction;
            reviewCurrentIndex = Mathf.Clamp(reviewCurrentIndex, 0, currentQuestions.Count - 1);
            DisplayReviewQuestion(reviewCurrentIndex);
        }

        /// <summary>
        /// Đóng review panel, quay lại bảng kết quả
        /// </summary>
        private void OnCloseReviewPanel()
        {
            if (reviewPanel != null)
                reviewPanel.SetActive(false);
            if (resultPanel != null)
                resultPanel.SetActive(true);
        }

        #endregion

        #region ===== UI HELPER (TẠO UI BẰNG CODE) =====

        /// <summary>
        /// Tạo một GameObject chứa TextMeshProUGUI với hỗ trợ offset padding tùy chọn
        /// </summary>
        private GameObject CreateTextObject(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax,
            int fontSize, FontStyles fontStyle, Color color, TextAlignmentOptions alignment,
            Vector2? offsetMin = null, Vector2? offsetMax = null)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin ?? Vector2.zero;
            rect.offsetMax = offsetMax ?? Vector2.zero;

            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = fontStyle;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Ellipsis;

            return obj;
        }

        /// <summary>
        /// Tạo một Button với text, sprite 9-slice và màu nền
        /// </summary>
        private GameObject CreateButton(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax,
            Color bgColor, UnityEngine.Events.UnityAction onClick,
            Sprite buttonSprite = null, Color? textColor = null, int fontSize = 20)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);

            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image btnImage = btnObj.GetComponent<Image>();
            if (buttonSprite != null)
            {
                btnImage.sprite = buttonSprite;
                btnImage.type = Image.Type.Sliced;
            }
            btnImage.color = bgColor;

            Button btn = btnObj.GetComponent<Button>();
            btn.targetGraphic = btnImage;
            btn.transition = Selectable.Transition.None; // Tắt ColorTint để kiểm soát màu sắc chính xác

            // Tạo text bên trong button
            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 2);
            textRect.offsetMax = new Vector2(-5, -2);

            TextMeshProUGUI btnText = textObj.GetComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = fontSize;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = textColor ?? Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.textWrappingMode = TextWrappingModes.NoWrap;

            btn.onClick.AddListener(onClick);

            return btnObj;
        }

        /// <summary>
        /// Cập nhật text của một element trong review panel theo tên
        /// </summary>
        private void SetReviewText(string childName, string text)
        {
            if (reviewPanel == null) return;
            Transform child = FindDeepChild(reviewPanel.transform, childName);
            if (child != null)
            {
                TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.text = text;
            }
        }

        /// <summary>
        /// Cập nhật màu text của một element trong review panel theo tên
        /// </summary>
        private void SetReviewColor(string childName, Color color)
        {
            if (reviewPanel == null) return;
            Transform child = FindDeepChild(reviewPanel.transform, childName);
            if (child != null)
            {
                TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.color = color;
            }
        }

        #endregion

        #region ===== CLEANUP =====

        private void OnDestroy()
        {
            if (quizTimer != null)
            {
                quizTimer.OnTimeUp -= OnTimerExpired;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion
    }
}
