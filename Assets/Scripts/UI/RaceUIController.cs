using ScienceQuest.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý Logic cho Scene 6 — Race & Quiz (Trận Đua & Trả Lời Câu Hỏi)
/// Tinh chỉnh theo cơ chế gameplay mới:
/// 1. Tốc độ nền chú chó người chơi CHẬM HƠN AI (AI ~120s hoàn thành chặng đua).
/// 2. Đợi 2s đầu trận mới hiện Popup câu hỏi đầu tiên.
/// 3. Khi trả lời xong: Tính điểm và hiện ngay câu hỏi tiếp theo.
/// 4. Trả lời ĐÚNG ➔ Nhận Speed Boost trong 10s + Hiệu ứng Tia Tăng Tốc ở đuôi chó.
/// 5. Điểm số giảm dần từ 100 điểm tùy theo thời gian suy nghĩ trả lời nhanh hay chậm.
/// </summary>
public class RaceUIController : MonoBehaviour
{
    [Header("=== 1. Cấu Hình Vận Động Viên & Đường Đua ===")]
    [Tooltip("Transform của 4 chú chó đua: Dog1, Dog2, Dog3, Dog4")]
    public Transform[] dogTransforms;
    public Transform startLineTransform;
    public Transform finishLineTransform;

    [Header("=== 2. Tốc Độ & Cân Bằng Gameplay (Đua ~1 Phút) ===")]
    [Tooltip("Tốc độ AI chạy bình thường (60s / 1 phút hoàn thành chặng đua 1.0)")]
    public float aiBaseSpeed = 0.01667f; // ~1.0 / 60s

    [Tooltip("Tốc độ nền chú chó người chơi (CHẬM HƠN AI một chút)")]
    public float playerBaseSpeed = 0.011f; // Chậm hơn AI

    [Tooltip("Tốc độ cộng thêm khi được Boost nhẹ (trong 10s)")]
    public float speedBoostMultiplier = 0.007f; // Boost nhẹ vừa phải, cân bằng game

    [Tooltip("Thời gian duy trì Speed Boost (giây)")]
    public float boostDuration = 10f;

    [Header("=== 3. Thời Gian Popup Câu Hỏi ===")]
    [Tooltip("Thời gian chờ trước khi hiện câu hỏi đầu tiên (giây)")]
    public float initialPopupDelay = 2f;

    [Tooltip("Thời gian giãn cách giữa 2 câu hỏi (giây)")]
    public float delayBetweenQuestions = 0f;

    [Header("=== 4. Điểm Số Theo Thời Gian Trả Lời ===")]
    public int maxScorePerQuestion = 100;
    public int minScorePerQuestion = 20;
    [Tooltip("Điểm bị trừ mỗi giây suy nghĩ")]
    public int pointsDecayPerSecond = 5;

    [Header("=== 5. Hiệu Ứng Tail Boost FX (Đuôi Chó) ===")]
    public GameObject speedBoostFXPrefab; // Prefab hoặc GameObject (nếu có)
    public Sprite speedBoostSprite;       // Kéo trực tiếp file ảnh fx_speed_boost.png vào đây!
    public Vector3 tailOffset = new Vector3(-0.6f, 0f, 0f); // Vị trí sau đuôi chó

    [Header("=== 6. Thanh HUD ===")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI boostTimeText;
    public Image[] heartImages;
    public int totalLives = 3;
    public TextMeshProUGUI questionProgressText;

    [Header("=== 7. Popup Câu Hỏi (PopQuiz) ===")]
    public GameObject quizPanel;
    public TextMeshProUGUI questionTitleText;
    public TextMeshProUGUI questionContentText; // Text 'Nội dung câu hỏi'

    [Header("Chế Độ Trắc Nghiệm (Đáp án 1 -> 4)")]
    public GameObject multipleChoiceGroup;
    public Button[] optionButtons; // 4 nút: Đáp án 1, Đáp án 2, Đáp án 3, Đáp án 4
    public TextMeshProUGUI[] optionTexts;

    [Header("Chế Độ Tự Luận (Essay)")]
    public GameObject essayGroup;
    public TMP_InputField essayInputField;
    public Button submitEssayButton;
    public TextMeshProUGUI essayResultText;
    public TextMeshProUGUI essayCountdownText;
    public TextMeshProUGUI essayQuestionText;
    [Tooltip("DogRacer grading backend. Provider keys are never stored in Unity.")]
    public string essayGradingEndpoint = RaceEssayGrader.DefaultEndpoint;
    [Min(0f)] public float essayRaceInterval = 5f;
    public string LastEssaySubmissionJson { get; private set; }
    private string essayRequestId;
    private string lastEssayAnswer;
    private bool essayQuestionOpen;
    private bool essaySubmissionPending;
    private float activeRaceTime;
    private bool IsEssayMode => GameSession.Instance != null && GameSession.Instance.lessonType == LessonType.Essay;

    // State Variables
    private int currentQuestionIndex = 0;
    private int currentScore = 0;
    private int currentLives = 3;
    private int correctCount = 0;
    private int wrongCount = 0;
    private int playerDogIndex = 0; // 0..3 (tương ứng selectedDogId = 1..4)
    private float[] dogProgress; // Tiến trình 0.0 -> 1.0 của 4 con chó
    private float[] aiSpeedVariations; // Tốc độ biến thiên nhẹ của AI
    private List<QuestionData> questionsList;

    private bool isRaceActive = false;
    private float raceStartTime = 0f;
    private float questionShowTime = 0f; // Thời điểm popup câu hỏi hiện lên

    // Boost & Tail Animation State
    private float playerBoostTimer = 0f;
    private Coroutine ambientSFXCoroutine;
    private DogRunAnimator[] dogRunVisuals;
    private float[] dogRunSpeedFactors;

    private void Awake()
    {
        AutoAssignReferences();
    }

    private void Reset()
    {
        AutoAssignReferences();
    }

    /// <summary>
    /// Tự động tìm và gán các GameObject theo đúng chuẩn Hierarchy của bạn
    /// </summary>
    [ContextMenu("Tự Động Điền Reference (Auto Assign)")]
    public void AutoAssignReferences()
    {
        // 1. Tự tìm 4 chú chó (4Dog)
        GameObject fourDogObj = GameObject.Find("4Dog");
        if (fourDogObj != null)
        {
            if (dogTransforms == null || dogTransforms.Length < 4)
            {
                dogTransforms = new Transform[4];
            }
            dogTransforms[0] = fourDogObj.transform.Find("Dog1");
            dogTransforms[1] = fourDogObj.transform.Find("Dog2");
            dogTransforms[2] = fourDogObj.transform.Find("Dog3");
            dogTransforms[3] = fourDogObj.transform.Find("Dog4");
            if (dogRunVisuals == null || dogRunVisuals.Length != 4)
            {
                dogRunVisuals = new DogRunAnimator[4];
                dogRunSpeedFactors = new float[4];
            }
            for (int i = 0; i < 4; i++)
            {
                dogRunVisuals[i] = dogTransforms[i] != null ? dogTransforms[i].GetComponent<DogRunAnimator>() : null;
                dogRunSpeedFactors[i] = 1f;
            }
        }

        // 2. Tìm StartLine và FinishLine
        GameObject startObj = GameObject.Find("StartLine");
        if (startObj != null) startLineTransform = startObj.transform;

        GameObject finishObj = GameObject.Find("FinishLine");
        if (finishObj != null) finishLineTransform = finishObj.transform;

        // 3. Tìm PopQuiz và 'Nội dung câu hỏi', các 'Đáp án 1..4'
        if (quizPanel == null)
        {
            quizPanel = GameObject.Find("PopQuiz");
        }

        if (quizPanel != null)
        {
            Transform qTextTr = quizPanel.transform.Find("Nội dung câu hỏi");
            if (qTextTr != null) questionContentText = qTextTr.GetComponent<TextMeshProUGUI>();

            if (optionButtons == null || optionButtons.Length < 4) optionButtons = new Button[4];
            if (optionTexts == null || optionTexts.Length < 4) optionTexts = new TextMeshProUGUI[4];

            for (int i = 1; i <= 4; i++)
            {
                Transform btnTr = quizPanel.transform.Find($"Đáp án {i}");
                if (btnTr != null)
                {
                    optionButtons[i - 1] = btnTr.GetComponent<Button>();
                    optionTexts[i - 1] = btnTr.GetComponentInChildren<TextMeshProUGUI>();
                }
            }
        }

        // 4. Tìm HUD
        GameObject hudObj = GameObject.Find("HUD");
        if (hudObj != null)
        {
            Transform scoreLabel = hudObj.transform.Find("ScoreGroup /Text (TMP)");
            if (scoreLabel != null) scoreText = scoreLabel.GetComponent<TextMeshProUGUI>();
            Transform boostLabel = hudObj.transform.Find("SpeedBoostedTime/Text (TMP)");
            if (boostLabel != null) boostTimeText = boostLabel.GetComponent<TextMeshProUGUI>();
            Image[] foundHearts = hudObj.GetComponentsInChildren<Image>();
            List<Image> heartsList = new List<Image>();
            foreach (Image img in foundHearts)
            {
                if (img.gameObject.name.ToLower().Contains("heart") || (img.sprite != null && img.sprite.name.ToLower().Contains("heart")))
                {
                    heartsList.Add(img);
                }
            }
            if (heartsList.Count > 0) heartImages = heartsList.ToArray();
        }

        // 5. Tìm Essay Group (EssayGroup) bên trong PopQuiz
        if (quizPanel != null && essayGroup == null)
        {
            Transform eg = quizPanel.transform.Find("EssayGroup");
            if (eg == null)
            {
                // Cũng thử tìm trong QuizBackground_Overlay
                Transform overlay = quizPanel.transform.Find("QuizBackground_Overlay");
                if (overlay != null) eg = overlay.Find("EssayGroup");
            }
            if (eg != null)
            {
                essayGroup = eg.gameObject;
                essayInputField = eg.GetComponentInChildren<TMP_InputField>();
                // Tìm submit button theo tên
                Transform submitTr = eg.Find("SubmitButton");
                if (submitTr == null) submitTr = eg.Find("NopBai");
                if (submitTr != null) submitEssayButton = submitTr.GetComponent<Button>();
                // Tìm result text
                Transform resultTr = eg.Find("ResultText");
                if (resultTr != null) essayResultText = resultTr.GetComponent<TextMeshProUGUI>();
                Transform countdownTr = eg.Find("CountdownText");
                if (countdownTr != null) essayCountdownText = countdownTr.GetComponent<TextMeshProUGUI>();
            }
        }

        Debug.Log("<color=green>[RaceUIController] Đã tự động điền xong tất cả Reference vào Inspector!</color>");
    }

    private void Start()
    {
        InitializeRace();
    }

    private void Update()
    {
        if (DogRaceLanSession.Active)
        {
            UpdateLanRace();
            return;
        }
        if (!isRaceActive)
        {
            UpdateBoostFXState();
            return;
        }

        activeRaceTime += Time.deltaTime;
        UpdateDogPositions();
        UpdateBoostFXState();
        if (boostTimeText != null) boostTimeText.text = playerBoostTimer > 0f ? $"Boost: {Mathf.CeilToInt(playerBoostTimer)}s" : "Boost: --";
    }

    /// <summary>
    /// Khởi tạo ban đầu cho trận đua 2 phút
    /// </summary>
    public void InitializeRace()
    {
        AutoAssignReferences();

        // 1. Xác định chó người chơi chọn
        int selectedId = GameSession.Instance != null ? GameSession.Instance.selectedDogId : 1;
        playerDogIndex = Mathf.Clamp(selectedId - 1, 0, 3);

        // 2. Tải danh sách câu hỏi từ Google Sheet
        questionsList = null;
        if (DogRaceLanSession.Active)
        {
            questionsList = new List<QuestionData>();
            DogRaceLanSession.Instance.ConfigureRace(playerBaseSpeed, speedBoostMultiplier, boostDuration);
        }
        else if (GoogleSheetDataManager.Instance != null)
        {
            StartCoroutine(LoadQuestionsFromGoogleSheetAsync());
        }
        else
        {
            CreateSampleQuestions();
        }

        // 3. Khởi tạo vị trí 4 chú chó ở vạch xuất phát StartLine
        dogProgress = new float[4];
        aiSpeedVariations = new float[4];
        float startX = GetLinePositionX(startLineTransform, -7.0f);

        for (int i = 0; i < 4; i++)
        {
            dogProgress[i] = 0f;
            aiSpeedVariations[i] = Random.Range(0.92f, 1.08f); // Biến thiên tốc độ AI ngẫu nhiên nhẹ

            if (dogTransforms != null && i < dogTransforms.Length && dogTransforms[i] != null)
            {
                Vector3 pos = dogTransforms[i].position;
                pos.x = startX;
                dogTransforms[i].position = pos;
            }
        }

        currentLives = totalLives;
        currentScore = 0;
        correctCount = 0;
        wrongCount = 0;
        currentQuestionIndex = 0;
        playerBoostTimer = 0f;
        if (dogRunVisuals != null)
            foreach (DogRunAnimator visual in dogRunVisuals) if (visual != null) visual.StopAndReset();
        raceStartTime = Time.time;
        activeRaceTime = 0f;
        essayQuestionOpen = false;
        essaySubmissionPending = false;
        essayRequestId = null;
        lastEssayAnswer = null;
        isRaceActive = true;

        if (quizPanel != null) quizPanel.SetActive(false); // Ẩn popup khi mới vào trận
        if (essayGroup != null) essayGroup.SetActive(false);

        UpdateHUD();
        SetupEventListeners();

        // 4. Đợi 2s đầu tiên mới hiển thị popup câu hỏi
        if (!DogRaceLanSession.Active) StartCoroutine(StartQuestionLoopWithInitialDelay());
        if (ambientSFXCoroutine != null) StopCoroutine(ambientSFXCoroutine);
        ambientSFXCoroutine = StartCoroutine(AmbientSoundsLoop());
    }

    private IEnumerator StartQuestionLoopWithInitialDelay()
    {
        yield return new WaitForSeconds(initialPopupDelay);
        while (questionsList == null) yield return null;
        PrepareQuestionsForMode();
        ShowCurrentQuestion();
    }

    private void PrepareQuestionsForMode()
    {
        if (!IsEssayMode || questionsList == null) return;
        var essays = questionsList.FindAll(q => q != null &&
            (q.loaiCauHoi == "Tự luận" || q.loaiCauHoi == "TuLuan" || !string.IsNullOrWhiteSpace(q.dapAnThamChieu)));
        if (essays.Count > 0) { questionsList = essays; return; }

        questionsList = new List<QuestionData>();
        string[] prompts = {
            "Giải thích vì sao cần cân bằng phương trình hóa học trước khi tính toán lượng chất.",
            "Nêu hiện tượng và viết phương trình phản ứng khi cho kẽm vào dung dịch axit clohiđric.",
            "Phân biệt axit và bazơ bằng giấy quỳ tím. Cho một ví dụ mỗi loại."
        };
        string[] references = {
            "Cân bằng để số nguyên tử mỗi nguyên tố được bảo toàn. Hệ số cho biết tỉ lệ mol các chất; nhờ đó tính đúng lượng chất tham gia và sản phẩm.",
            "Kẽm tan dần, có bọt khí hiđro thoát ra. Zn + 2HCl -> ZnCl2 + H2.",
            "Axit làm quỳ tím hóa đỏ (ví dụ HCl); bazơ làm quỳ tím hóa xanh (ví dụ NaOH)."
        };
        string[] rubrics = {
            "4 điểm: bảo toàn số nguyên tử; 3 điểm: tỉ lệ mol theo hệ số; 3 điểm: liên hệ tính lượng chất chính xác.",
            "4 điểm: nêu kẽm tan và khí thoát ra; 6 điểm: viết đúng và cân bằng phương trình Zn + 2HCl -> ZnCl2 + H2.",
            "4 điểm: quỳ tím hóa đỏ với axit; 4 điểm: hóa xanh với bazơ; 2 điểm: ví dụ đúng cho cả hai."
        };
        int sampleIndex = 0;
        foreach (string prompt in prompts)
        {
            QuestionData q = ScriptableObject.CreateInstance<QuestionData>();
            q.questionText = prompt;
            q.loaiCauHoi = "Tự luận";
            q.dapAnThamChieu = references[sampleIndex];
            q.tieuChiCham = rubrics[sampleIndex++];
            q.subject = SubjectType.Chemistry;
            questionsList.Add(q);
        }
    }

    private IEnumerator LoadQuestionsFromGoogleSheetAsync()
    {
        if (GoogleSheetDataManager.Instance == null) yield break;

        // Do not reuse the menu cache: it may contain the other question type.
        GameSession.Instance.loadedQuestions = new List<QuestionData>();
        yield return GoogleSheetDataManager.Instance.FetchQuestionsFromSheet(null, IsEssayMode);

        if (GameSession.Instance != null && GameSession.Instance.loadedQuestions != null && GameSession.Instance.loadedQuestions.Count > 0)
        {
            questionsList = new List<QuestionData>(GameSession.Instance.loadedQuestions);
            Debug.Log($"<color=green>[RaceUIController] Đã nạp thành công {questionsList.Count} câu hỏi từ Google Sheet vào trận đua!</color>");
        }
        else if (GoogleSheetDataManager.Instance != null)
        {
            List<QuestionData> fetched = GoogleSheetDataManager.Instance.GetQuestionsForSubject(SubjectType.Chemistry);

            if (fetched.Count > 0)
            {
                questionsList = fetched;
                Debug.Log($"<color=green>[RaceUIController] Đã lấy {questionsList.Count} câu hỏi theo Môn từ Google Sheet!</color>");
            }
            else
            {
                CreateSampleQuestions();
            }
        }

        PrepareQuestionsForMode();
        if (!IsEssayMode && questionsList != null)
            questionsList.RemoveAll(q => q == null || q.loaiCauHoi == "Tự luận" || !string.IsNullOrWhiteSpace(q.dapAnThamChieu));
        UpdateHUD();
    }

    private void SetupEventListeners()
    {
        if (optionButtons == null || optionButtons.Length < 4)
        {
            optionButtons = new Button[4];
        }

        for (int i = 0; i < 4; i++)
        {
            // Nếu người chơi kéo Text (TMP) mà chưa thêm Button, script tự động lấy hoặc tạo Button component
            if (optionButtons[i] == null && optionTexts != null && i < optionTexts.Length && optionTexts[i] != null)
            {
                optionButtons[i] = optionTexts[i].GetComponent<Button>();
                if (optionButtons[i] == null)
                {
                    optionButtons[i] = optionTexts[i].gameObject.AddComponent<Button>();
                }
            }

            if (optionButtons[i] != null)
            {
                int index = i;
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
            }
        }

        if (submitEssayButton != null)
        {
            submitEssayButton.onClick.RemoveAllListeners();
            submitEssayButton.onClick.AddListener(OnEssaySubmitted);
        }
    }

    [Header("=== Màu Sắc Phản Hồi Đáp Án ===")]
    public Color correctButtonColor = new Color(0.18f, 0.8f, 0.44f); // Màu xanh lá (#2ECC71)
    public Color wrongButtonColor = new Color(0.9f, 0.3f, 0.23f);   // Màu đỏ (#E74C3C)
    public Color defaultButtonColor = Color.white;
    public float feedbackDelay = 0f; // Thời gian hiển thị màu xanh/đỏ trước khi ẩn popup

    private void ShowCurrentQuestion()
    {
        if (!isRaceActive || questionsList == null || questionsList.Count == 0) return;

        if (currentQuestionIndex >= questionsList.Count)
        {
            currentQuestionIndex = 0; // Trộn lại câu hỏi khi đã duyệt hết
        }

        QuestionData q = questionsList[currentQuestionIndex];
        if (quizPanel != null) quizPanel.SetActive(true);

        questionShowTime = Time.time; // Đánh dấu thời điểm hiện câu hỏi để tính điểm decay

        if (questionTitleText != null) questionTitleText.text = $"Câu {currentQuestionIndex + 1}/{questionsList.Count}";
        if (questionContentText != null) questionContentText.text = q.questionText;

        bool isEssay = IsEssayMode;

        if (isEssay)
        {
            essayQuestionOpen = true;
            essaySubmissionPending = false;
        essayRequestId = null;
        lastEssayAnswer = null;
            if (quizPanel != null) quizPanel.SetActive(false);
            if (essayQuestionText != null) essayQuestionText.text = q.questionText;
            if (essayInputField != null) { essayInputField.gameObject.SetActive(true); essayInputField.interactable = true; }
            if (submitEssayButton != null) submitEssayButton.gameObject.SetActive(true);
            SetEssaySubmitLabel("NỘP BÀI");
            if (multipleChoiceGroup != null) multipleChoiceGroup.SetActive(false);
            if (essayGroup != null) essayGroup.SetActive(true);
            if (essayInputField != null) essayInputField.text = "";
            if (essayResultText != null) { essayResultText.text = ""; essayResultText.gameObject.SetActive(false); }
            if (essayCountdownText != null) { essayCountdownText.text = ""; essayCountdownText.gameObject.SetActive(false); }
            if (submitEssayButton != null) submitEssayButton.interactable = true;
            isRaceActive = false; // Đóng băng chú chó khi trả lời tự luận
        }
        else
        {
            if (multipleChoiceGroup != null) multipleChoiceGroup.SetActive(true);
            if (essayGroup != null) essayGroup.SetActive(false);

            if (q.options != null && q.options.Length >= 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (optionTexts != null && i < optionTexts.Length && optionTexts[i] != null)
                    {
                        optionTexts[i].text = $"{(char)('A' + i)}. {q.options[i]}";
                    }

                    // Reset lại màu và mở tương tác cho 4 nút đáp án
                    if (optionButtons != null && i < optionButtons.Length && optionButtons[i] != null)
                    {
                        optionButtons[i].interactable = true;
                        Image btnImage = optionButtons[i].GetComponent<Image>();
                        if (btnImage != null) btnImage.color = defaultButtonColor;
                    }
                }
            }
        }

        UpdateHUD();
    }

    /// <summary>
    /// Xử lý chọn đáp án trắc nghiệm (Đổi màu Xanh/Đỏ & Đợi trả lời xong)
    /// </summary>
    public void OnOptionSelected(int optionIndex)
    {
        if (DogRaceLanSession.Active)
        {
            DogRaceLanSession.Instance.Answer(optionIndex);
            return;
        }
        if (!isRaceActive || quizPanel == null || !quizPanel.activeSelf) return;

        QuestionData q = questionsList[currentQuestionIndex];
        bool isCorrect = (optionIndex == q.correctOptionIndex);

        // Khóa tất cả các nút để tránh người chơi bấm liên tiếp
        if (optionButtons != null)
        {
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] != null) optionButtons[i].interactable = false;
            }
        }

        // Đổi màu Nút: Đúng ➔ Xanh lá, Sai ➔ Đỏ (và hiện đáp án đúng màu xanh)
        if (isCorrect)
        {
            SetButtonColor(optionIndex, correctButtonColor);
        }
        else
        {
            SetButtonColor(optionIndex, wrongButtonColor);
            // Hiện luôn đáp án đúng màu xanh để người chơi học bài
            if (q.correctOptionIndex >= 0 && q.correctOptionIndex < 4)
            {
                SetButtonColor(q.correctOptionIndex, correctButtonColor);
            }
        }

        ProcessAnswer(isCorrect);
    }

    private void SetButtonColor(int index, Color color)
    {
        if (optionButtons != null && index >= 0 && index < optionButtons.Length && optionButtons[index] != null)
        {
            Image btnImage = optionButtons[index].GetComponent<Image>();
            if (btnImage != null) btnImage.color = color;
        }
    }

    /// <summary>
    /// Xử lý nộp câu hỏi tự luận — gửi AI chấm, hiển thị kết quả, đợi 5s rồi tiếp tục đua
    /// </summary>
    public void OnEssaySubmitted()
    {
        if (!IsEssayMode || !essayQuestionOpen || essaySubmissionPending) return;
        string answer = essayInputField != null ? essayInputField.text.Trim() : "";
        if (string.IsNullOrWhiteSpace(answer))
        {
            if (essayCountdownText != null)
            {
                essayCountdownText.gameObject.SetActive(true);
                essayCountdownText.text = "Hãy nhập câu trả lời trước khi nộp bài.";
            }
            return;
        }
        QuestionData submittedQuestion = questionsList[currentQuestionIndex];
        if (string.IsNullOrEmpty(essayRequestId) || lastEssayAnswer != answer)
            essayRequestId = System.Guid.NewGuid().ToString("N");
        lastEssayAnswer = answer;
        LastEssaySubmissionJson = JsonUtility.ToJson(RaceEssayGrader.CreateRequest(submittedQuestion, answer, essayRequestId));
        essaySubmissionPending = true;
        if (submitEssayButton != null) submitEssayButton.interactable = false;
        if (essayInputField != null) { essayInputField.interactable = false; essayInputField.gameObject.SetActive(false); }
        StartCoroutine(EssayGradingCoroutine(questionsList[currentQuestionIndex], answer));
    }

    private void SetEssaySubmitLabel(string label)
    {
        if (submitEssayButton == null) return;
        TextMeshProUGUI text = submitEssayButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null) text.text = label;
    }

    private IEnumerator EssayGradingCoroutine(QuestionData question, string answer)
    {
        if (essayCountdownText != null) essayCountdownText.gameObject.SetActive(false);
        if (submitEssayButton != null) submitEssayButton.gameObject.SetActive(false);
        if (essayResultText != null)
        {
            essayResultText.gameObject.SetActive(true);
            essayResultText.text = "AI đang chấm bài...\nĐường đua đang tạm dừng.";
        }

        EssayGradingResult grade = null;
        yield return RaceEssayGrader.GradeEssayJsonCoroutine(essayGradingEndpoint, LastEssaySubmissionJson, result => grade = result);
        if (grade == null || !string.IsNullOrEmpty(grade.error))
        {
            essaySubmissionPending = false;
            if (essayResultText != null) essayResultText.gameObject.SetActive(false);
            if (essayCountdownText != null)
            {
                essayCountdownText.gameObject.SetActive(true);
                essayCountdownText.text = grade != null ? grade.feedback : RaceEssayGrader.MessageForError("AI_UNAVAILABLE");
            }
            if (essayInputField != null) { essayInputField.gameObject.SetActive(true); essayInputField.interactable = true; }
            if (submitEssayButton != null) { submitEssayButton.gameObject.SetActive(true); submitEssayButton.interactable = true; }
            SetEssaySubmitLabel("THỬ LẠI");
            yield break;
        }

        essayQuestionOpen = false;
        currentScore += Mathf.RoundToInt(grade.score / grade.maxScore * maxScorePerQuestion);
        if (grade.isCorrect)
        {
            correctCount++;
            playerBoostTimer = boostDuration;
            if (AudioManager.Instance != null) { AudioManager.Instance.PlayCorrectSFX(); AudioManager.Instance.PlayDogBarkSFX(); }
        }
        else
        {
            wrongCount++;
            currentLives = Mathf.Max(0, currentLives - 1);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayWrongSFX();
        }
        UpdateHUD();
        if (essayResultText != null)
            essayResultText.text = $"KẾT QUẢ AI • {grade.score:0.#}/{grade.maxScore:0.#}\n\n{grade.feedback}\n\nGợi ý: {grade.modelAnswer}";
        if (essayCountdownText != null) essayCountdownText.gameObject.SetActive(true);

        if (currentLives <= 0)
        {
            if (essayCountdownText != null) essayCountdownText.text = "Hết tim • Đang chuyển đến kết quả...";
            yield return new WaitForSecondsRealtime(2f);
            if (essayGroup != null) essayGroup.SetActive(false);
            EndRace(-1);
            yield break;
        }

        isRaceActive = true;
        float remaining = essayRaceInterval;
        while (remaining > 0f && isRaceActive)
        {
            if (essayCountdownText != null)
                essayCountdownText.text = $"Đang đua • Câu tiếp theo sau {Mathf.CeilToInt(remaining)}s";
            remaining -= Time.deltaTime;
            yield return null;
        }
        if (!isRaceActive) yield break;
        currentQuestionIndex++;
        ShowCurrentQuestion();
    }
    private void ProcessAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            float timeTaken = Time.time - questionShowTime;
            int pointsEarned = Mathf.Max(minScorePerQuestion, maxScorePerQuestion - Mathf.RoundToInt(timeTaken * pointsDecayPerSecond));

            currentScore += pointsEarned;
            correctCount++;

            playerBoostTimer = boostDuration;
            if (AudioManager.Instance != null) { AudioManager.Instance.PlayCorrectSFX(); AudioManager.Instance.PlayDogBarkSFX(); }

            Debug.Log($"<color=green>ĐÚNG RỒI! Nhận +{pointsEarned} điểm. Kích hoạt Speed Boost trong {boostDuration}s!</color>");
        }
        else
        {
            wrongCount++;
            currentLives--;
            if (currentLives <= 0) currentLives = 0;

            Debug.Log("<color=red>SAI RỒI! Trừ 1 trái tim.</color>");
            if (AudioManager.Instance != null) { AudioManager.Instance.PlayWrongSFX(); }
        }

        UpdateHUD();

        // Kiem tra dieu kien het tim (Game Over)
        if (currentLives <= 0)
        {
            Debug.Log("<color=red>HET TIM! Man choi ket thuc.</color>");
            if (quizPanel != null) quizPanel.SetActive(false);
            EndRace(-1);
            return;
        }
        CheckRaceFinish();

        // Hiện ngay câu tiếp theo sau khi ghi nhận kết quả.
        if (isRaceActive)
        {
            currentQuestionIndex++;

            if (isRaceActive)
            {
                ShowCurrentQuestion();
            }
        }
    }

    /// <summary>
    /// Cập nhật di chuyển cho 4 con chó trên đường đua
    /// </summary>
    private void UpdateDogPositions()
    {
        if (dogProgress == null) return;

        for (int i = 0; i < 4; i++)
        {
            float speed = 0f;

            if (i == playerDogIndex)
            {
                // Tốc độ chú chó của người chơi
                speed = playerBaseSpeed;

                // Nếu đang có Speed Boost (trả lời đúng) -> Cộng thêm speedBoostMultiplier
                if (playerBoostTimer > 0f)
                {
                    speed += speedBoostMultiplier;
                    playerBoostTimer -= Time.deltaTime;
                    if (playerBoostTimer < 0f) playerBoostTimer = 0f;
                }
            }
            else
            {
                // Tốc độ 3 chú chó NPC không đồng đều, nhấp nhô nhịp nhàng ngẫu nhiên qua lại
                float sineFluctuation = Mathf.Sin(Time.time * (0.8f + i * 0.5f) + i * 1.8f) * 0.18f;
                float dynamicMultiplier = aiSpeedVariations[i] + sineFluctuation;
                speed = aiBaseSpeed * Mathf.Max(0.6f, dynamicMultiplier);
            }

            if (dogRunSpeedFactors != null && i < dogRunSpeedFactors.Length)
                dogRunSpeedFactors[i] = speed / Mathf.Max(0.0001f, i == playerDogIndex ? playerBaseSpeed : aiBaseSpeed);
            dogProgress[i] += speed * Time.deltaTime;
            ApplyDogPosition(i);
        }

        CheckRaceFinish();
    }

    private float GetLinePositionX(Transform lineTr, float defaultFallbackX)
    {
        if (lineTr == null) return defaultFallbackX;

        // 1. Nếu lineTr có SpriteRenderer trực tiếp
        SpriteRenderer sr = lineTr.GetComponent<SpriteRenderer>();
        if (sr != null) return lineTr.position.x;

        // 2. Nếu lineTr chứa con SpriteRenderer (như StartStripe / StartFlag / FinishFlag)
        SpriteRenderer childSR = lineTr.GetComponentInChildren<SpriteRenderer>();
        if (childSR != null && Mathf.Abs(childSR.transform.position.x) > 0.1f)
        {
            return childSR.transform.position.x;
        }

        // 3. Nếu vị trí transform của lineTr khác 0
        if (Mathf.Abs(lineTr.position.x) > 0.1f)
        {
            return lineTr.position.x;
        }

        return defaultFallbackX;
    }

    private void ApplyDogPosition(int i)
    {
        if (dogTransforms == null || i >= dogTransforms.Length || dogTransforms[i] == null) return;

        float startX = GetLinePositionX(startLineTransform, -7.0f);
        float finishX = GetLinePositionX(finishLineTransform, 7.0f);

        float newX = Mathf.Lerp(startX, finishX, dogProgress[i]);
        Vector3 pos = dogTransforms[i].position;
        pos.x = newX;
        dogTransforms[i].position = pos;
    }

    /// <summary>
    /// Hiệu ứng Animation Boosted Speed (Hiệu ứng vệt sáng ở đuôi chó)
    /// </summary>
    private void UpdateBoostFXState()
    {
        if (dogRunVisuals == null) return;
        for (int i = 0; i < dogRunVisuals.Length; i++)
        {
            if (dogRunVisuals[i] == null) continue;
            float speed = dogRunSpeedFactors != null && i < dogRunSpeedFactors.Length ? dogRunSpeedFactors[i] : 1f;
            dogRunVisuals[i].TickRace(isRaceActive, i == playerDogIndex && playerBoostTimer > 0f,
                speed, isRaceActive ? Time.deltaTime : 0f);
        }
    }
    private void CheckRaceFinish()
    {
        if (dogProgress == null) return;

        for (int i = 0; i < dogProgress.Length; i++)
        {
            if (dogProgress[i] >= 1.0f)
            {
                EndRace(i);
                break;
            }
        }
    }

    private void EndRace(int winnerIndex)
    {
        if (DogRaceLanSession.Active) return;
        isRaceActive = false;
        if (ambientSFXCoroutine != null)
        {
            StopCoroutine(ambientSFXCoroutine);
            ambientSFXCoroutine = null;
        }

        // Tắt hiệu ứng boost
        playerBoostTimer = 0f;
        if (dogRunVisuals != null)
            foreach (DogRunAnimator visual in dogRunVisuals) if (visual != null) visual.StopAndReset();

        Debug.Log($"<color=yellow>Trận Đua Kết Thúc! Chú chó chiến thắng: Dog #{winnerIndex + 1}</color>");

        int playerRank = 1;
        for (int i = 0; i < dogProgress.Length; i++)
        {
            if (i != playerDogIndex && dogProgress[i] > dogProgress[playerDogIndex])
            {
                playerRank++;
            }
        }

        if (GameSession.Instance != null)
        {
            RaceResult result = new RaceResult
            {
                rank = playerRank,
                totalScore = currentScore,
                correctAnswers = correctCount,
                wrongAnswers = wrongCount,
                completionTime = activeRaceTime
            };
            GameSession.Instance.lastRaceResult = result;

            // Tự động đẩy kết quả lên Google Sheet
            if (GoogleSheetDataManager.Instance != null)
            {
                StartCoroutine(GoogleSheetDataManager.Instance.SubmitResultToSheet(result));
            }
        }

        if (NavigationManager.Instance != null)
        {
            NavigationManager.Instance.LoadScene(NavigationManager.SCENE_RESULTS);
        }
    }

    private void UpdateHUD()
    {
        if (scoreText != null) scoreText.text = $"Điểm: {currentScore}";
        if (questionProgressText != null && questionsList != null)
        {
            questionProgressText.text = $"{currentQuestionIndex + 1}/{questionsList.Count}";
        }

        if (heartImages != null)
        {
            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] != null)
                {
                    heartImages[i].enabled = (i < currentLives);
                }
            }
        }
    }

    private void CreateSampleQuestions()
    {
        questionsList = new List<QuestionData>();

        QuestionData q1 = ScriptableObject.CreateInstance<QuestionData>();
        q1.questionText = "Công thức hóa học của Nước là gì?";
        q1.options = new string[] { "H2O", "CO2", "NaCl", "O2" };
        q1.correctOptionIndex = 0;
        questionsList.Add(q1);

        QuestionData q2 = ScriptableObject.CreateInstance<QuestionData>();
        q2.questionText = "Chất nào sau đây là Axit?";
        q2.options = new string[] { "NaOH", "HCl", "NaCl", "KOH" };
        q2.correctOptionIndex = 1;
        questionsList.Add(q2);

        QuestionData q3 = ScriptableObject.CreateInstance<QuestionData>();
        q3.questionText = "Khí nào duy trì sự sống và sự cháy?";
        q3.options = new string[] { "Nitơ", "Cacbonic", "Oxi", "Hỗn hợp" };
        q3.correctOptionIndex = 2;
        questionsList.Add(q3);
    }

    private IEnumerator AmbientSoundsLoop()
    {
        while (isRaceActive)
        {
            // Kho?ng c?ch ng?u nhi?n t? 6 ??n 12 gi?y ph?t 1 hi?u ?ng
            float waitTime = Random.Range(6f, 12f);
            yield return new WaitForSeconds(waitTime);

            if (!isRaceActive) break;

            if (AudioManager.Instance != null)
            {
                // T? l? 50/50 gi?a ti?ng ch? s?a thi tho?ng v? ti?ng ??m ??ng c? v?
                if (Random.value < 0.5f)
                {
                    AudioManager.Instance.PlayDogBarkSFX();
                }
                else
                {
                    AudioManager.Instance.PlayCheerSFX();
                }
            }
        }
    }

    private int lanQuestionSequence = -1;
    private void UpdateLanRace()
    {
        var lan = DogRaceLanSession.Instance;
        int p = lan.LocalPlayer;
        playerDogIndex = Mathf.Max(0, lan.LocalDog - 1);
        isRaceActive = lan.Racing && !lan.Finished;
        activeRaceTime = lan.Elapsed;
        currentScore = lan.Scores[p]; correctCount = lan.Correct[p]; wrongCount = lan.Wrong[p];
        playerBoostTimer = lan.Boost[p];
        for (int i = 0; i < 4; i++)
        {
            bool used = i + 1 == lan.LocalDog || i + 1 == lan.RemoteDog;
            if (dogTransforms != null && i < dogTransforms.Length && dogTransforms[i] != null)
                dogTransforms[i].gameObject.SetActive(used);
            if (!used || dogProgress == null) continue;
            int owner = i + 1 == lan.LocalDog ? p : 1 - p;
            dogProgress[i] = lan.Progress[owner];
            ApplyDogPosition(i);
            if (dogRunSpeedFactors != null && i < dogRunSpeedFactors.Length)
                dogRunSpeedFactors[i] = lan.Boost[owner] > 0 ? 1.5f : 1;
        }
        if (isRaceActive && lan.CurrentQuestion != null && lanQuestionSequence != lan.QuestionSequence)
        {
            lanQuestionSequence = lan.QuestionSequence;
            questionsList = new List<QuestionData> { lan.CurrentQuestion };
            currentQuestionIndex = 0;
            ShowCurrentQuestion();
            if (questionTitleText != null) questionTitleText.text = $"PvP · Câu {lan.QuestionSequence + 1}";
        }
        if (!isRaceActive && quizPanel != null) quizPanel.SetActive(false);
        UpdateBoostFXState();
        UpdateHUD();
    }
}
