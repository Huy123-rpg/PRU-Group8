using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using ScienceQuest.Core;

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
        private bool hasAnsweredCurrent = false;

        // Tên chương được truyền từ scene trước (qua PlayerPrefs hoặc static)
        public static string SelectedChapter = "";

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

        private void Start()
        {
            InitializeQuiz();
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

            // 2. Tìm và gắn UI elements
            AutoAssignUIReferences();

            // 3. Setup QuizTimer
            SetupTimer();

            // 4. Lấy chapter từ PlayerPrefs nếu có
            if (string.IsNullOrEmpty(SelectedChapter))
            {
                SelectedChapter = PlayerPrefs.GetString("QuizChapter", "");
            }

            // 5. Bắt đầu quiz
            StartQuiz();
        }

        #region ===== AUTO ASSIGN UI =====

        /// <summary>
        /// Tự động tìm và gán các UI references trong scene.
        /// Tìm theo tên GameObject và nội dung text.
        /// Cách này tránh phải sửa file .unity (tránh merge conflict).
        /// </summary>
        private void AutoAssignUIReferences()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[QuizManager] Không tìm thấy Canvas trong scene!");
                return;
            }

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

                // Tổng điểm text
                if (text.Contains("TỔNG ĐIỂM") || text.Contains("T\u1ed4NG \u0110I\u1ec2M"))
                {
                    // Score label đã tìm thấy, tìm text con gần đó để hiển thị điểm
                    resultTitleText = tmp;
                    Debug.Log($"[QuizManager] ✅ Đã tìm thấy Result Title Text");
                }

                // Số điểm (chỉ chứa số)
                if (resultScoreText == null && int.TryParse(text, out _) && text.Length <= 3)
                {
                    resultScoreText = tmp;
                    Debug.Log($"[QuizManager] ✅ Đã tìm thấy Score Text: \"{text}\"");
                }

                // Text "ĐÚNG"
                if (resultCorrectText == null && (text == "ĐÚNG" || text == "\u0110\u00daNG"))
                {
                    resultCorrectText = tmp;
                    Debug.Log($"[QuizManager] ✅ Đã tìm thấy Result Correct Text");
                }

                // Text "SAI"
                if (resultIncorrectText == null && text == "SAI")
                {
                    resultIncorrectText = tmp;
                    Debug.Log($"[QuizManager] ✅ Đã tìm thấy Result Incorrect Text");
                }

                // Text "HOÀN THÀNH!"
                if (text.Contains("HOÀN THÀNH") || text.Contains("HO\u00c0N TH\u00c0NH"))
                {
                    // Đây là title của result panel
                    if (resultTitleText == null)
                        resultTitleText = tmp;
                    Debug.Log($"[QuizManager] ✅ Đã tìm thấy Completion Title");
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
                    // Tìm Button component trên parent hoặc chính nó
                    Button btn = answerTexts[i].GetComponentInParent<Button>();
                    if (btn != null)
                    {
                        answerButtons[i] = btn;
                        answerButtonImages[i] = btn.GetComponent<Image>();
                        Debug.Log($"[QuizManager] ✅ Đã tìm thấy Answer Button {i}");
                    }
                }
            }

            // Nếu không tìm thấy answer buttons qua text, tìm Button không phải Retry/Back
            if (answerButtons[0] == null)
            {
                Debug.LogWarning("[QuizManager] Không tìm thấy Answer Buttons qua text. Đang tìm bằng cách khác...");
                List<Button> candidateButtons = new List<Button>();
                foreach (var btn in allButtons)
                {
                    if (btn != retryButton && btn != backButton && btn != submitButton)
                    {
                        candidateButtons.Add(btn);
                    }
                }

                // Sắp xếp theo vị trí Y (từ trên xuống dưới)
                candidateButtons.Sort((a, b) =>
                    b.transform.position.y.CompareTo(a.transform.position.y));

                for (int i = 0; i < Mathf.Min(4, candidateButtons.Count); i++)
                {
                    answerButtons[i] = candidateButtons[i];
                    answerButtonImages[i] = candidateButtons[i].GetComponent<Image>();
                    TextMeshProUGUI btnText = candidateButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null)
                        answerTexts[i] = btnText;
                }
            }

            // Ẩn Panel_ResultBoard ban đầu
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }

            // Log tóm tắt kết quả tìm kiếm
            LogUIAssignmentSummary();
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
            hasAnsweredCurrent = false;

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

            // Gắn button listeners
            SetupButtonListeners();

            // Hiển thị câu hỏi đầu tiên
            DisplayQuestion(currentQuestionIndex);

            // Bắt đầu đếm ngược 15 phút
            quizTimer.StartTimer(quizDuration);
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
            hasAnsweredCurrent = false;

            // Hiển thị nội dung câu hỏi
            if (questionText != null)
            {
                questionText.text = question.questionText;
            }

            // Hiển thị số câu hỏi (Câu X/Y)
            if (questionCounterText != null)
            {
                questionCounterText.text = $"Câu {index + 1}/{currentQuestions.Count}";
            }

            // Hiển thị 4 đáp án
            for (int i = 0; i < 4; i++)
            {
                if (answerTexts[i] != null && i < question.answers.Length)
                {
                    answerTexts[i].text = question.answers[i];
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

            QuestionData question = currentQuestions[currentQuestionIndex];

            // Lưu đáp án người chơi
            playerAnswers[currentQuestionIndex] = answerIndex;
            hasAnsweredCurrent = true;

            // Kiểm tra đúng/sai
            bool isCorrect = (answerIndex == question.correctAnswerIndex);

            if (isCorrect)
            {
                correctCount++;
                totalScore += pointsPerCorrectAnswer;
                Debug.Log($"[QuizManager] ✅ Đúng! +{pointsPerCorrectAnswer} điểm. Tổng: {totalScore}");
            }
            else
            {
                incorrectCount++;
                Debug.Log($"[QuizManager] ❌ Sai! Đáp án đúng: {question.answers[question.correctAnswerIndex]}");
            }

            // Cập nhật điểm trên UI
            if (scoreText != null)
            {
                scoreText.text = totalScore.ToString();
            }

            // Hiển thị feedback (highlight đúng/sai)
            StartCoroutine(ShowAnswerFeedback(answerIndex, question.correctAnswerIndex));
        }

        /// <summary>
        /// Hiệu ứng feedback đáp án: highlight đúng (xanh), sai (đỏ), sau đó chuyển câu
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
        /// Xử lý khi hết giờ (Timer expired)
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

            // Tính số câu chưa trả lời (bị tính là sai)
            int unanswered = 0;
            for (int i = 0; i < currentQuestions.Count; i++)
            {
                if (playerAnswers[i] < 0)
                {
                    unanswered++;
                    incorrectCount++;
                }
            }

            if (unanswered > 0)
            {
                Debug.Log($"[QuizManager] ⚠️ Có {unanswered} câu chưa trả lời (tính là sai).");
            }

            Debug.Log($"[QuizManager] 🏆 Kết thúc Quiz! Điểm: {totalScore}/{currentQuestions.Count * pointsPerCorrectAnswer}, Đúng: {correctCount}, Sai: {incorrectCount}");

            // Cộng phần thưởng
            GiveRewards();

            // Hiển thị bảng kết quả
            ShowResultPanel();
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
        /// Hiển thị Panel kết quả
        /// </summary>
        private void ShowResultPanel()
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
            }

            // Cập nhật text trên bảng kết quả
            if (resultTitleText != null)
            {
                resultTitleText.text = "HOÀN THÀNH!";
            }

            if (resultScoreText != null)
            {
                resultScoreText.text = totalScore.ToString();
            }

            if (resultCorrectText != null)
            {
                resultCorrectText.text = $"ĐÚNG: {correctCount}";
            }

            if (resultIncorrectText != null)
            {
                resultIncorrectText.text = $"SAI: {incorrectCount}";
            }

            // Cập nhật score text nếu có riêng
            if (scoreText != null)
            {
                scoreText.text = totalScore.ToString();
            }

            // Vô hiệu hóa các nút đáp án phía sau
            for (int i = 0; i < 4; i++)
            {
                if (answerButtons[i] != null)
                    answerButtons[i].interactable = false;
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
        /// Tạo panel xem chi tiết đáp án bằng code
        /// </summary>
        private void CreateReviewPanel()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // === TẠO PANEL CHÍNH ===
            reviewPanel = new GameObject("Panel_ReviewDetail");
            reviewPanel.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = reviewPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Background tối mờ
            Image panelBg = reviewPanel.AddComponent<Image>();
            panelBg.color = new Color(0.1f, 0.1f, 0.15f, 0.97f);

            // === CONTAINER NỘI DUNG (có padding) ===
            GameObject container = new GameObject("Container");
            container.transform.SetParent(reviewPanel.transform, false);
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.08f, 0.05f);
            containerRect.anchorMax = new Vector2(0.92f, 0.95f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            // === TIÊU ĐỀ ===
            GameObject titleObj = CreateTextObject(container.transform, "ReviewTitle",
                "📋 XEM CHI TIẾT ĐÁP ÁN",
                new Vector2(0f, 0.88f), new Vector2(1f, 1f),
                36, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);

            // === SỐ CÂU ===
            GameObject counterObj = CreateTextObject(container.transform, "ReviewCounter",
                "Câu 1/10",
                new Vector2(0f, 0.82f), new Vector2(1f, 0.88f),
                24, FontStyles.Normal, new Color(0.7f, 0.85f, 1f), TextAlignmentOptions.Center);

            // === NỘI DUNG CÂU HỎI ===
            GameObject questionObj = CreateTextObject(container.transform, "ReviewQuestion",
                "Nội dung câu hỏi...",
                new Vector2(0f, 0.62f), new Vector2(1f, 0.82f),
                22, FontStyles.Bold, Color.white, TextAlignmentOptions.TopLeft);

            // === ĐÁP ÁN NGƯỜI CHƠI ===
            GameObject playerAnswerObj = CreateTextObject(container.transform, "ReviewPlayerAnswer",
                "Bạn chọn: ...",
                new Vector2(0f, 0.52f), new Vector2(1f, 0.62f),
                20, FontStyles.Normal, Color.white, TextAlignmentOptions.TopLeft);

            // === ĐÁP ÁN ĐÚNG ===
            GameObject correctAnswerObj = CreateTextObject(container.transform, "ReviewCorrectAnswer",
                "Đáp án đúng: ...",
                new Vector2(0f, 0.42f), new Vector2(1f, 0.52f),
                20, FontStyles.Bold, correctColor, TextAlignmentOptions.TopLeft);

            // === KẾT QUẢ (ĐÚNG/SAI) ===
            GameObject resultObj = CreateTextObject(container.transform, "ReviewResult",
                "✅ ĐÚNG",
                new Vector2(0f, 0.35f), new Vector2(1f, 0.42f),
                26, FontStyles.Bold, correctColor, TextAlignmentOptions.TopLeft);

            // === GIẢI THÍCH ===
            GameObject explanationObj = CreateTextObject(container.transform, "ReviewExplanation",
                "💡 Giải thích: ...",
                new Vector2(0f, 0.12f), new Vector2(1f, 0.35f),
                18, FontStyles.Italic, new Color(1f, 0.95f, 0.7f), TextAlignmentOptions.TopLeft);

            // === CÁC NÚT ĐIỀU HƯỚNG ===
            // Nút "◀ Câu trước"
            CreateButton(container.transform, "Btn_ReviewPrev", "◀ Câu trước",
                new Vector2(0.02f, 0f), new Vector2(0.3f, 0.09f),
                new Color(0.3f, 0.5f, 0.8f), () => NavigateReview(-1));

            // Nút "Quay lại kết quả"
            CreateButton(container.transform, "Btn_ReviewBackToResult", "📊 Kết quả",
                new Vector2(0.32f, 0f), new Vector2(0.68f, 0.09f),
                new Color(0.9f, 0.6f, 0.1f), OnCloseReviewPanel);

            // Nút "Câu sau ▶"
            CreateButton(container.transform, "Btn_ReviewNext", "Câu sau ▶",
                new Vector2(0.7f, 0f), new Vector2(0.98f, 0.09f),
                new Color(0.3f, 0.5f, 0.8f), () => NavigateReview(1));

            // Nút "Quay lại" (về scene trước) - góc trên bên phải
            CreateButton(container.transform, "Btn_GoBack", "🚪 Quay lại",
                new Vector2(0.75f, 0.88f), new Vector2(1f, 0.98f),
                new Color(0.7f, 0.2f, 0.2f), OnGoBackClicked);

            Debug.Log("[QuizManager] ✅ Đã tạo Panel_ReviewDetail bằng code");
        }

        /// <summary>
        /// Hiển thị chi tiết 1 câu hỏi trong review panel
        /// </summary>
        private void DisplayReviewQuestion(int index)
        {
            if (currentQuestions == null || index < 0 || index >= currentQuestions.Count)
                return;

            QuestionData q = currentQuestions[index];
            int playerAns = playerAnswers[index];
            bool isCorrect = (playerAns == q.correctAnswerIndex);

            // Cập nhật text
            SetReviewText("ReviewCounter", $"Câu {index + 1}/{currentQuestions.Count}");
            SetReviewText("ReviewQuestion", q.questionText);

            // Đáp án người chơi
            if (playerAns >= 0 && playerAns < q.answers.Length)
            {
                SetReviewText("ReviewPlayerAnswer", $"📝 Bạn chọn: {q.answers[playerAns]}");
                SetReviewColor("ReviewPlayerAnswer", isCorrect ? correctColor : incorrectColor);
            }
            else
            {
                SetReviewText("ReviewPlayerAnswer", "📝 Bạn chọn: (Không trả lời)");
                SetReviewColor("ReviewPlayerAnswer", new Color(0.6f, 0.6f, 0.6f));
            }

            // Đáp án đúng
            SetReviewText("ReviewCorrectAnswer", $"✔️ Đáp án đúng: {q.answers[q.correctAnswerIndex]}");

            // Kết quả
            if (isCorrect)
            {
                SetReviewText("ReviewResult", "✅ ĐÚNG  (+10 điểm)");
                SetReviewColor("ReviewResult", correctColor);
            }
            else
            {
                SetReviewText("ReviewResult", "❌ SAI  (0 điểm)");
                SetReviewColor("ReviewResult", incorrectColor);
            }

            // Giải thích
            string explanation = !string.IsNullOrEmpty(q.explanation)
                ? $"💡 Giải thích: {q.explanation}"
                : "💡 Giải thích: (Chưa có)";
            SetReviewText("ReviewExplanation", explanation);
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
        /// Tạo một GameObject chứa TextMeshProUGUI
        /// </summary>
        private GameObject CreateTextObject(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax,
            int fontSize, FontStyles fontStyle, Color color, TextAlignmentOptions alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = fontStyle;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Ellipsis;

            return obj;
        }

        /// <summary>
        /// Tạo một Button với text và màu nền
        /// </summary>
        private GameObject CreateButton(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax,
            Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImage;

            // Tạo text bên trong button
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 2);
            textRect.offsetMax = new Vector2(-5, -2);

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = 20;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.enableWordWrapping = false;

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
