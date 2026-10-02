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
/// 3. Khi trả lời xong: Popup ẩn ngay ➔ Đợi 5s mới hiện Popup tiếp theo.
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
    public float delayBetweenQuestions = 3.5f;

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
    private GameObject activeTailFXInstance;

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
            scoreText = hudObj.GetComponentInChildren<TextMeshProUGUI>();
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

        Debug.Log("<color=green>[RaceUIController] Đã tự động điền xong tất cả Reference vào Inspector!</color>");
    }

    private void Start()
    {
        InitializeRace();
    }

    private void Update()
    {
        if (!isRaceActive) return;

        UpdateDogPositions();
        UpdateBoostFXState();
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
        if (GameSession.Instance != null && GameSession.Instance.loadedQuestions != null && GameSession.Instance.loadedQuestions.Count > 0)
        {
            questionsList = GameSession.Instance.loadedQuestions;
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
        raceStartTime = Time.time;
        isRaceActive = true;

        if (quizPanel != null) quizPanel.SetActive(false); // Ẩn popup khi mới vào trận

        UpdateHUD();
        SetupEventListeners();

        // 4. Đợi 2s đầu tiên mới hiển thị popup câu hỏi
        StartCoroutine(StartQuestionLoopWithInitialDelay());
    }

    private IEnumerator StartQuestionLoopWithInitialDelay()
    {
        yield return new WaitForSeconds(initialPopupDelay);
        ShowCurrentQuestion();
    }

    private IEnumerator LoadQuestionsFromGoogleSheetAsync()
    {
        if (GoogleSheetDataManager.Instance == null) yield break;

        bool isDone = false;
        yield return GoogleSheetDataManager.Instance.FetchQuestionsFromSheet((success) =>
        {
            isDone = true;
        });

        if (GameSession.Instance != null && GameSession.Instance.loadedQuestions != null && GameSession.Instance.loadedQuestions.Count > 0)
        {
            questionsList = GameSession.Instance.loadedQuestions;
            Debug.Log($"<color=green>[RaceUIController] Đã nạp thành công {questionsList.Count} câu hỏi từ Google Sheet vào trận đua!</color>");
        }
        else if (GoogleSheetDataManager.Instance != null)
        {
            List<QuestionData> fetched = GoogleSheetDataManager.Instance.GetQuestionsForSubject(SubjectType.Chemistry);
            if (fetched.Count == 0) fetched = GoogleSheetDataManager.Instance.GetQuestionsForSubject(SubjectType.Biology);
            if (fetched.Count == 0) fetched = GoogleSheetDataManager.Instance.GetQuestionsForSubject(SubjectType.Physics);

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
    public float feedbackDelay = 1.2f; // Thời gian hiển thị màu xanh/đỏ trước khi ẩn popup

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

        bool isEssay = GameSession.Instance != null && GameSession.Instance.lessonType == LessonType.Essay;

        if (isEssay)
        {
            if (multipleChoiceGroup != null) multipleChoiceGroup.SetActive(false);
            if (essayGroup != null) essayGroup.SetActive(true);
            if (essayInputField != null) essayInputField.text = "";
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

        StartCoroutine(ProcessAnswerWithFeedback(isCorrect));
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
    /// Xử lý nộp câu hỏi tự luận
    /// </summary>
    public void OnEssaySubmitted()
    {
        if (!isRaceActive || quizPanel == null || !quizPanel.activeSelf) return;

        string userText = essayInputField != null ? essayInputField.text.Trim() : "";
        QuestionData q = questionsList[currentQuestionIndex];

        string correctAnswer = (q.options != null && q.correctOptionIndex >= 0 && q.correctOptionIndex < q.options.Length)
            ? q.options[q.correctOptionIndex].Trim()
            : "";

        bool isCorrect = string.Equals(userText, correctAnswer, System.StringComparison.OrdinalIgnoreCase);
        StartCoroutine(ProcessAnswerWithFeedback(isCorrect));
    }

    private IEnumerator ProcessAnswerWithFeedback(bool isCorrect)
    {
        // 1. Giữ Popup trong 1.2s để người chơi thấy rõ màu Xanh / Đỏ phản hồi
        yield return new WaitForSeconds(feedbackDelay);

        // 2. Ẩn Popup câu hỏi sau khi đã hiển thị phản hồi màu
        if (quizPanel != null) quizPanel.SetActive(false);

        if (isCorrect)
        {
            float timeTaken = Time.time - questionShowTime;
            int pointsEarned = Mathf.Max(minScorePerQuestion, maxScorePerQuestion - Mathf.RoundToInt(timeTaken * pointsDecayPerSecond));

            currentScore += pointsEarned;
            correctCount++;

            playerBoostTimer = boostDuration;

            Debug.Log($"<color=green>ĐÚNG RỒI! Nhận +{pointsEarned} điểm. Kích hoạt Speed Boost trong {boostDuration}s!</color>");
        }
        else
        {
            wrongCount++;
            currentLives--;
            if (currentLives <= 0) currentLives = 0;

            Debug.Log("<color=red>SAI RỒI! Trừ 1 trái tim.</color>");
        }

        UpdateHUD();
        CheckRaceFinish();

        // 3. Đợi 3.5s rồi mới hiển thị Popup câu hỏi tiếp theo
        if (isRaceActive)
        {
            currentQuestionIndex++;
            yield return new WaitForSeconds(delayBetweenQuestions);
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
        Transform playerDogTr = (dogTransforms != null && playerDogIndex < dogTransforms.Length) ? dogTransforms[playerDogIndex] : null;
        if (playerDogTr == null) return;

        if (playerBoostTimer > 0f)
        {
            // Đang Boost -> Tạo hoặc bật FX ở đuôi chó
            if (activeTailFXInstance == null)
            {
                if (speedBoostFXPrefab != null)
                {
                    activeTailFXInstance = Instantiate(speedBoostFXPrefab, playerDogTr);
                    activeTailFXInstance.transform.localPosition = tailOffset;
                }
                else if (speedBoostSprite != null)
                {
                    // Tự động tạo GameObject SpriteRenderer nếu kéo file ảnh fx_speed_boost.png vào
                    activeTailFXInstance = new GameObject("[TailBoostFX]");
                    activeTailFXInstance.transform.SetParent(playerDogTr);
                    activeTailFXInstance.transform.localPosition = tailOffset;
                    activeTailFXInstance.transform.localScale = Vector3.one;

                    SpriteRenderer sr = activeTailFXInstance.AddComponent<SpriteRenderer>();
                    sr.sprite = speedBoostSprite;
                    sr.sortingLayerName = "Gameplay";
                    sr.sortingOrder = 11;
                }
            }
            else
            {
                activeTailFXInstance.SetActive(true);
                activeTailFXInstance.transform.localPosition = tailOffset;
            }
        }
        else
        {
            // Hết Boost -> Tắt FX ở đuôi chó
            if (activeTailFXInstance != null)
            {
                activeTailFXInstance.SetActive(false);
            }
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
        isRaceActive = false;

        // Tắt hiệu ứng boost
        if (activeTailFXInstance != null) activeTailFXInstance.SetActive(false);

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
                completionTime = Time.time - raceStartTime
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
}
