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
    /// Điều khiển toàn bộ logic Scene 11_Quiz:
    /// - Nạp câu hỏi trắc nghiệm từ PhysicsQuestionRepository theo Chapter/Lesson/Teacher.
    /// - Hiển thị 2 feedback UI chuẩn (đã dựng sẵn trong scene):
    ///   + Correct Feedback: "Chính xác!" (khung xanh)
    ///   + Wrong Feedback: "Không đúng. Đáp án đúng là X." (khung đỏ)
    /// - Dừng render inline explanation cũ trong Scene 11 (chỉ xem tại Scene 20).
    /// - Snapshot chính xác 10 câu vào PhysicsGameSession.Instance.CurrentQuizAttempt xuyên scene.
    /// - Đo thời gian làm bài thực tế (Timer).
    /// - Kết thúc câu 10 -> bấm "Tiếp theo" -> chuyển sang Scene 19 Result_TN.
    /// </summary>
    public class QuizGameController : MonoBehaviour
    {
        [Header("=== QUESTION & COUNTER ===")]
        [SerializeField] private TextMeshProUGUI counterText;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private TextMeshProUGUI explanationText;

        [Header("=== ANSWER BUTTONS ===")]
        [SerializeField] private Button btnAnswerA;
        [SerializeField] private Button btnAnswerB;
        [SerializeField] private Button btnAnswerC;
        [SerializeField] private Button btnAnswerD;

        [Header("=== NAVIGATION ===")]
        [SerializeField] private Button btnBack;
        [SerializeField] private Button btnNext;

        [Header("=== FEEDBACK UI (Scene 11 Built-in) ===")]
        [SerializeField] private GameObject correctFeedbackRoot;
        [SerializeField] private TextMeshProUGUI correctFeedbackText;
        [SerializeField] private GameObject wrongFeedbackRoot;
        [SerializeField] private TextMeshProUGUI wrongFeedbackText;

        private List<PhysicsQuestionRaw> _quizQuestions = new List<PhysicsQuestionRaw>();
        private int _currentIndex = 0;
        private int _correctCount = 0;
        private bool _hasAnsweredCurrent = false;
        private bool _isQuizFinalized;
        private float _quizStartTime = 0f;

        private Button[] _answerButtons;
        private TextMeshProUGUI[] _answerTexts;
        private TextMeshProUGUI[] _badgeTexts;
        private Image[] _answerImages;
        private TMP_FontAsset _defaultFont;
        private Sprite _normalFrameSprite;

        private static readonly string[] AnswerPrefixes = { "A", "B", "C", "D" };

        private void Awake()
        {
            EnsureEventSystem();
            FindUIReferences();
            FindFeedbackUIReferences();
            SetupNavigationButtons();
            DisableOldInlineExplanation();
        }

        private void Start()
        {
            StartNewQuizSession();
        }

        private void Update()
        {
            if (PhysicsGameSession.Instance != null && PhysicsGameSession.Instance.StudyTimeLimitReached && btnNext != null && _hasAnsweredCurrent)
            {
                var label = btnNext.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null) label.text = "Kết thúc buổi";
            }
        }

        private void EnsureEventSystem()
        {
            EventSystem es = FindAnyObjectByType<EventSystem>();
            if (es != null)
            {
                if (es.GetComponent<InputSystemUIInputModule>() == null)
                {
                    es.gameObject.AddComponent<InputSystemUIInputModule>();
                }
            }
            else
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }
        }

        private void FindUIReferences()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. Tìm Question Text và Counter Text
            if (counterText == null || questionText == null)
            {
                TextMeshProUGUI[] allTmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in allTmps)
                {
                    string text = t.text != null ? t.text.Trim() : "";
                    if (counterText == null && (text.StartsWith("Câu ") || text.Contains("/10") || text.Contains("/")))
                    {
                        counterText = t;
                    }
                    else if (questionText == null && (text.Contains("?") || (text.Length > 20 && !text.StartsWith("A.") && !text.StartsWith("B.") && !text.StartsWith("C.") && !text.StartsWith("D."))))
                    {
                        questionText = t;
                    }
                }
            }

            if (_defaultFont == null && questionText != null)
            {
                _defaultFont = questionText.font;
            }

            // 2. Tìm 4 Answer Buttons và 2 Navigation Buttons
            if (btnAnswerA == null || btnAnswerB == null || btnAnswerC == null || btnAnswerD == null || btnBack == null || btnNext == null)
            {
                Button[] allButtons = canvas.GetComponentsInChildren<Button>(true);
                List<Button> answerBtnList = new List<Button>();

                foreach (var b in allButtons)
                {
                    Vector2 pos = b.GetComponent<RectTransform>().anchoredPosition;

                    if (pos.x < -200 && pos.y < -400)
                    {
                        if (btnBack == null) btnBack = b;
                    }
                    else if (pos.x > 200 && pos.y < -400)
                    {
                        if (btnNext == null) btnNext = b;
                    }
                    else if (pos.y > 150)
                    {
                        continue;
                    }
                    else if (pos.y >= -300 && pos.y <= 0)
                    {
                        answerBtnList.Add(b);
                    }
                }

                foreach (var b in answerBtnList)
                {
                    Vector2 pos = b.GetComponent<RectTransform>().anchoredPosition;
                    if (pos.x < 0)
                    {
                        if (pos.y > -150f && btnAnswerA == null) btnAnswerA = b;
                        else if (btnAnswerB == null) btnAnswerB = b;
                    }
                    else
                    {
                        if (pos.y > -150f && btnAnswerC == null) btnAnswerC = b;
                        else if (btnAnswerD == null) btnAnswerD = b;
                    }
                }

                if (btnAnswerA == null && answerBtnList.Count >= 1) btnAnswerA = answerBtnList[0];
                if (btnAnswerB == null && answerBtnList.Count >= 2) btnAnswerB = answerBtnList[1];
                if (btnAnswerC == null && answerBtnList.Count >= 3) btnAnswerC = answerBtnList[2];
                if (btnAnswerD == null && answerBtnList.Count >= 4) btnAnswerD = answerBtnList[3];
            }

            _answerButtons = new[] { btnAnswerA, btnAnswerB, btnAnswerC, btnAnswerD };
            _answerTexts = new TextMeshProUGUI[4];
            _badgeTexts = new TextMeshProUGUI[4];
            _answerImages = new Image[4];

            // Tìm Sprite khung bình thường từ các button
            for (int i = 0; i < 4; i++)
            {
                if (_answerButtons[i] != null)
                {
                    var img = _answerButtons[i].GetComponent<Image>();
                    if (img != null && img.sprite != null)
                    {
                        if (img.sprite.name.Contains("sai") || _normalFrameSprite == null)
                        {
                            _normalFrameSprite = img.sprite;
                        }
                    }
                }
            }

            // Chuẩn hóa typography và cấu trúc nút
            for (int i = 0; i < 4; i++)
            {
                if (_answerButtons[i] != null)
                {
                    // Nút Image: raycastTarget = true (Requirement 33)
                    var bImg = _answerButtons[i].GetComponent<Image>();
                    if (bImg != null) bImg.raycastTarget = true;

                    TextMeshProUGUI bodyTmp = null;
                    TextMeshProUGUI badgeTmp = null;

                    var childTmps = _answerButtons[i].GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var t in childTmps)
                    {
                        // Child TMP: raycastTarget = false (Requirement 33)
                        t.raycastTarget = false;

                        if (t.gameObject.name == "BadgeText")
                        {
                            badgeTmp = t;
                        }
                        else
                        {
                            bodyTmp = t;
                        }
                    }

                    if (badgeTmp == null)
                    {
                        GameObject badgeGo = new GameObject("BadgeText");
                        badgeGo.transform.SetParent(_answerButtons[i].transform, false);
                        badgeTmp = badgeGo.AddComponent<TextMeshProUGUI>();
                    }

                    if (_defaultFont != null) badgeTmp.font = _defaultFont;
                    ConfigureBadgeTypography(badgeTmp, AnswerPrefixes[i]);

                    _answerTexts[i] = bodyTmp;
                    _badgeTexts[i] = badgeTmp;
                    _answerImages[i] = bImg;

                    if (_answerImages[i] != null && _normalFrameSprite != null)
                    {
                        _answerImages[i].sprite = _normalFrameSprite;
                        _answerImages[i].color = Color.white;
                    }

                    ConfigureAnswerTypography(_answerTexts[i]);

                    Navigation nav = _answerButtons[i].navigation;
                    nav.mode = Navigation.Mode.None;
                    _answerButtons[i].navigation = nav;

                    var fb = _answerButtons[i].GetComponent<UIButtonFeedback>();
                    if (fb != null) Destroy(fb);
                    UIHoverPulseEffect.Attach(_answerButtons[i].gameObject, hover: 1.05f, minPulse: 1.02f, maxPulse: 1.07f, outline: true);

                    int answerIndex = i;
                    _answerButtons[i].onClick.RemoveAllListeners();
                    _answerButtons[i].onClick.AddListener(() => OnAnswerSelected(answerIndex));
                }
            }
        }

        /// <summary>
        /// Tìm và liên kết chính xác 2 feedback UI (Green / Red) đã dựng sẵn trong Scene 11.
        /// Đồng thời đảm bảo raycastTarget = false để không block các nút bấm (Requirement 32).
        /// </summary>
        private void FindFeedbackUIReferences()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            TextMeshProUGUI[] tmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                string text = t.text != null ? t.text.Trim() : "";

                // Green Feedback: "Chính xác!"
                if (correctFeedbackText == null && (text.Contains("Chính xác") || text.Contains("Ch\xEDnh x\xE1c")))
                {
                    correctFeedbackText = t;
                    t.raycastTarget = false; // Requirement 32
                    t.alignment = TextAlignmentOptions.Center;
                }
                // Red Feedback: "Không đúng. Đáp án đúng là..."
                else if (wrongFeedbackText == null && (text.Contains("Không đúng") || text.Contains("Kh\xF4ng \u0111\xFAng")))
                {
                    wrongFeedbackText = t;
                    t.raycastTarget = false; // Requirement 32
                    t.alignment = TextAlignmentOptions.Center;
                }
            }

            // Tìm Image tương ứng với 2 feedback: sprite "true" (xanh) và "false" (đỏ)
            Image[] images = canvas.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.GetComponent<Button>() != null || img.sprite == null) continue;

                string spriteName = img.sprite.name.ToLower();

                if (correctFeedbackRoot == null && (spriteName == "true" || spriteName.Contains("correct") || spriteName.Contains("dung")))
                {
                    correctFeedbackRoot = img.gameObject;
                    img.raycastTarget = false; // Requirement 32
                }
                else if (wrongFeedbackRoot == null && (spriteName == "false" || spriteName.Contains("wrong") || spriteName.Contains("sai")))
                {
                    wrongFeedbackRoot = img.gameObject;
                    img.raycastTarget = false; // Requirement 32
                }
            }

            // Enforce vị trí và kích thước feedback bar ở trung tâm, cách nút điều hướng ~30px
            if (correctFeedbackRoot != null)
            {
                RectTransform rt = correctFeedbackRoot.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(34f, -345f);
                    rt.sizeDelta = new Vector2(760f, 64f);
                }
            }
            if (wrongFeedbackRoot != null)
            {
                RectTransform rt = wrongFeedbackRoot.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(34f, -345f);
                    rt.sizeDelta = new Vector2(760f, 64f);
                }
            }

            // Đảm bảo cả hai feedback ẩn khi bắt đầu
            SetFeedbackVisibility(false, false);
        }

        private void DisableOldInlineExplanation()
        {
            // Requirement 7: DỪNG RENDER explanation nhỏ dưới câu hỏi trong Scene 11
            if (explanationText != null)
            {
                explanationText.text = "";
                explanationText.gameObject.SetActive(false);
            }
        }

        private void SetupNavigationButtons()
        {
            if (btnBack != null)
            {
                var rt = btnBack.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(-380f, -460f);
                    rt.sizeDelta = new Vector2(440f, 105f);
                }

                var bImg = btnBack.GetComponent<Image>();
                if (bImg != null) bImg.raycastTarget = true;
                foreach (var t in btnBack.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    t.raycastTarget = false;
                }

                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackClicked);

                var fb = btnBack.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);
                UIHoverPulseEffect.Attach(btnBack.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
            }

            if (btnNext != null)
            {
                var rt = btnNext.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(380f, -460f);
                    rt.sizeDelta = new Vector2(440f, 105f);
                }

                var bImg = btnNext.GetComponent<Image>();
                if (bImg != null) bImg.raycastTarget = true;
                foreach (var t in btnNext.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    t.raycastTarget = false;
                }

                btnNext.onClick.RemoveAllListeners();
                btnNext.onClick.AddListener(OnNextClicked);

                var fb = btnNext.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);
                UIHoverPulseEffect.Attach(btnNext.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
            }
        }

        public void StartNewQuizSession()
        {
            NavigationService.Instance.EnsureDevContext();
            int chapterId = NavigationService.Instance.Selection.ChapterId;
            string lessonId = NavigationService.Instance.Selection.LessonId;

            // Dev session fallback: nếu mở thẳng Scene 11_Quiz trong Unity Editor mà chưa qua chọn bài
            if (chapterId == 0 || string.IsNullOrEmpty(lessonId))
            {
                chapterId = 2;
                lessonId = "C2_L6";
                NavigationService.Instance.Selection.ChapterId = chapterId;
                NavigationService.Instance.Selection.LessonId = lessonId;
                PhysicsGameSession.Instance.SelectChapter(chapterId);
                PhysicsGameSession.Instance.SelectLesson(lessonId, "Bài 6: Phản xạ toàn phần");
            }

            Debug.Log($"[QuizGameController] Khởi động phiên Quiz: Chương {chapterId}, Bài '{lessonId}'");

            // Ưu tiên nạp câu hỏi từ NavigationService.Instance.Attempt (đã qua lọc DifficultySelection/Blueprint)
            if (NavigationService.Instance.Attempt != null &&
                NavigationService.Instance.Attempt.Answers != null &&
                NavigationService.Instance.Attempt.Answers.Count > 0)
            {
                _quizQuestions = new List<PhysicsQuestionRaw>();
                foreach (var ans in NavigationService.Instance.Attempt.Answers)
                {
                    _quizQuestions.Add(new PhysicsQuestionRaw
                    {
                        questionID = ans.CauHoiID,
                        questionText = ans.QuestionText,
                        answers = ans.Options,
                        correctAnswerIndex = ans.CorrectOptionIndex,
                        explanation = ans.Explanation,
                        difficulty = NavigationService.Instance.Selection.Difficulty
                    });
                }
            }
            else
            {
                // Fallback nạp từ GameDataRepository
                var mcqItems = GameDataRepository.GetMCQs(lessonId, NavigationService.Instance.Selection.Difficulty, 10);
                if (mcqItems != null && mcqItems.Count > 0)
                {
                    _quizQuestions = new List<PhysicsQuestionRaw>();
                    NavigationService.Instance.Attempt.Answers.Clear();
                    foreach (var m in mcqItems)
                    {
                        _quizQuestions.Add(new PhysicsQuestionRaw
                        {
                            questionID = m.cauHoiID,
                            questionText = m.noiDung,
                            answers = new[] { m.dapAnA, m.dapAnB, m.dapAnC, m.dapAnD },
                            correctAnswerIndex = m.correctIndex,
                            explanation = m.giaiThich,
                            difficulty = m.mucDoGame2
                        });
                        NavigationService.Instance.Attempt.Answers.Add(new QuizAnswerRecord
                        {
                            CauHoiID = m.cauHoiID,
                            QuestionText = m.noiDung,
                            Options = new[] { m.dapAnA, m.dapAnB, m.dapAnC, m.dapAnD },
                            CorrectOptionIndex = m.correctIndex,
                            Explanation = m.giaiThich,
                            KeyKnowledge = m.meoGhiNho,
                            SelectedOptionIndex = -1
                        });
                    }
                }
                else
                {
                    _quizQuestions = PhysicsQuestionRepository.GetQuizQuestions(chapterId, lessonId, "Minh", 10);
                }
            }

            _currentIndex = 0;
            _correctCount = 0;
            _hasAnsweredCurrent = false;
            _isQuizFinalized = false;

            // Khởi tạo QuizAttemptData cho phiên thi hiện tại
            var attempt = new QuizAttemptData
            {
                lessonId = NavigationService.Instance.Selection.LessonId,
                lessonTitle = NavigationService.Instance.Selection.LessonTitle,
                answers = new List<QuizAttemptAnswer>()
            };
            PhysicsGameSession.Instance.CurrentQuizAttempt = attempt;

            // Bắt đầu tính thời gian làm bài (Requirement 13)
            _quizStartTime = Time.time;

            if (_quizQuestions == null || _quizQuestions.Count == 0)
            {
                if (questionText != null) questionText.text = "Chưa có câu hỏi phù hợp cho bài này.";
                if (counterText != null) counterText.text = "Câu 0/0";
                SetButtonsInteractable(false);
                if (btnNext != null) btnNext.interactable = false;
                return;
            }

            DisplayCurrentQuestion();
        }

        private void DisplayCurrentQuestion()
        {
            if (_currentIndex < 0 || _currentIndex >= _quizQuestions.Count) return;

            var q = _quizQuestions[_currentIndex];
            _hasAnsweredCurrent = false;

            // 1. Requirement 3: Ẩn cả 2 Feedback UI khi render câu mới
            SetFeedbackVisibility(false, false);

            // 2. Cập nhật Counter (Câu x/N)
            if (counterText != null)
            {
                counterText.text = $"Câu {_currentIndex + 1}/{_quizQuestions.Count}";
            }

            // 3. Cập nhật Question Text (căn chỉnh vùng hiển thị phía trên 2 nút A/C và bật Auto-Size cho câu hỏi dài)
            if (questionText != null)
            {
                RectTransform qRt = questionText.rectTransform;
                qRt.anchorMin = new Vector2(0.5f, 0.5f);
                qRt.anchorMax = new Vector2(0.5f, 0.5f);
                qRt.pivot = new Vector2(0.5f, 0.5f);
                qRt.anchoredPosition = new Vector2(34f, 76f);
                qRt.sizeDelta = new Vector2(1320f, 124f);

                questionText.enableAutoSizing = true;
                questionText.fontSizeMin = 20f;
                questionText.fontSizeMax = 32f;
                questionText.alignment = TextAlignmentOptions.Center;
                questionText.textWrappingMode = TextWrappingModes.Normal;
                questionText.raycastTarget = false;
                questionText.text = q.questionText;
            }

            // 4. Reset 4 đáp án: Normal visual, interactable = true (Requirement 3)
            for (int i = 0; i < 4; i++)
            {
                if (_answerButtons[i] != null)
                {
                    string rawAnswer = (q.answers != null && i < q.answers.Length) ? q.answers[i] : "";
                    string displayAnswer = FormatAnswerForDisplay(rawAnswer, AnswerPrefixes[i]);

                    if (_badgeTexts != null && _badgeTexts[i] != null)
                    {
                        _badgeTexts[i].text = AnswerPrefixes[i];
                        ConfigureBadgeTypography(_badgeTexts[i], AnswerPrefixes[i]);
                    }

                    if (_answerTexts[i] != null)
                    {
                        _answerTexts[i].text = displayAnswer;
                        ConfigureAnswerTypography(_answerTexts[i]);
                    }

                    if (_answerImages[i] != null)
                    {
                        if (_normalFrameSprite != null)
                        {
                            _answerImages[i].sprite = _normalFrameSprite;
                        }
                        _answerImages[i].color = Color.white;
                    }

                    Navigation nav = _answerButtons[i].navigation;
                    nav.mode = Navigation.Mode.None;
                    _answerButtons[i].navigation = nav;
                }
            }

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            SetButtonsInteractable(true);

            // Requirement 3: Next button interactable = false cho tới khi user đã chọn đáp án
            if (btnNext != null)
            {
                btnNext.interactable = false;
            }

            // Requirement 7: Không hiện explanation
            if (explanationText != null)
            {
                explanationText.text = "";
            }
        }

        private void SetFeedbackVisibility(bool showCorrect, bool showWrong, string correctLetter = "")
        {
            if (correctFeedbackRoot != null) correctFeedbackRoot.SetActive(showCorrect);
            if (correctFeedbackText != null) correctFeedbackText.gameObject.SetActive(showCorrect);

            if (wrongFeedbackRoot != null) wrongFeedbackRoot.SetActive(showWrong);
            if (wrongFeedbackText != null)
            {
                wrongFeedbackText.gameObject.SetActive(showWrong);
                if (showWrong && !string.IsNullOrEmpty(correctLetter))
                {
                    wrongFeedbackText.text = $"Không đúng. Đáp án đúng là {correctLetter}.";
                }
            }
        }

        private string FormatAnswerForDisplay(string rawAnswer, string letterPrefix)
        {
            if (string.IsNullOrEmpty(rawAnswer)) return "";
            string trimmed = rawAnswer.Trim();

            string[] prefixesToStrip = new[]
            {
                letterPrefix + ". ",
                letterPrefix + ".\t",
                letterPrefix + ") ",
                letterPrefix + ": ",
                letterPrefix + " - ",
                letterPrefix + "."
            };

            foreach (var p in prefixesToStrip)
            {
                if (trimmed.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed.Substring(p.Length).TrimStart();
                }
            }

            return trimmed;
        }

        private void ConfigureBadgeTypography(TextMeshProUGUI tmp, string letter)
        {
            if (tmp == null) return;
            RectTransform rt = tmp.rectTransform;
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(80f, 0f);
            rt.sizeDelta = new Vector2(80f, 80f);

            tmp.text = letter;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = false;
            tmp.fontSize = 32f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(1f, 0.90f, 0.45f, 1f);
            tmp.raycastTarget = false;
        }

        private void ConfigureAnswerTypography(TextMeshProUGUI tmp)
        {
            if (tmp == null) return;
            RectTransform rt = tmp.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(47.5f, 0f);
            rt.sizeDelta = new Vector2(-175f, -20f);

            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 18f;
            tmp.fontSizeMax = 26f;
            tmp.fontSize = 24f;
            tmp.lineSpacing = 3f;
            tmp.margin = Vector4.zero;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
        }

        private void SetButtonsInteractable(bool state)
        {
            for (int i = 0; i < 4; i++)
            {
                if (_answerButtons[i] != null)
                {
                    _answerButtons[i].interactable = state;
                }
            }
        }

        private void OnAnswerSelected(int selectedIndex)
        {
            // Requirement 4: Mỗi câu chỉ được trả lời MỘT LẦN, không record lại nếu spam click
            if (_hasAnsweredCurrent || _currentIndex >= _quizQuestions.Count) return;

            _hasAnsweredCurrent = true;
            SetButtonsInteractable(false); // Lock toàn bộ A/B/C/D

            var q = _quizQuestions[_currentIndex];
            bool isCorrect = (selectedIndex == q.correctAnswerIndex);

            // Snapshot câu trả lời vào QuizAttemptData (Requirements 10 & 11)
            var attempt = PhysicsGameSession.Instance.CurrentQuizAttempt;
            if (attempt != null)
            {
                var answerRecord = new QuizAttemptAnswer
                {
                    questionId = q.questionID,
                    questionText = q.questionText,
                    options = new List<string>(q.answers ?? Array.Empty<string>()),
                    selectedAnswerIndex = selectedIndex,
                    correctAnswerIndex = q.correctAnswerIndex,
                    wasCorrect = isCorrect,
                    explanation = q.explanation ?? "",
                    keyKnowledge = PhysicsQuestionRepository.ResolveKeyKnowledge(q, PhysicsGameSession.Instance.SelectedLessonId)
                };
                attempt.answers.Add(answerRecord);
            }

            if (NavigationService.Instance != null && NavigationService.Instance.Attempt != null &&
                NavigationService.Instance.Attempt.Answers != null &&
                _currentIndex < NavigationService.Instance.Attempt.Answers.Count)
            {
                NavigationService.Instance.Attempt.Answers[_currentIndex].SelectedOptionIndex = selectedIndex;
                NavigationService.Instance.Attempt.Answers[_currentIndex].IsCorrect = isCorrect;
            }

            string correctLetter = (q.correctAnswerIndex >= 0 && q.correctAnswerIndex < 4)
                ? AnswerPrefixes[q.correctAnswerIndex]
                : "A";

            if (isCorrect)
            {
                _correctCount++;

                // Requirement 5: B correct green state
                if (_answerImages[selectedIndex] != null)
                {
                    _answerImages[selectedIndex].color = new Color(0.2f, 0.8f, 0.35f, 1f); // Xanh lá
                }

                // Hiện Correct Feedback ("Chính xác!"), Ẩn Wrong Feedback
                SetFeedbackVisibility(true, false);
            }
            else
            {
                // Requirement 6: Selected wrong = RED, correct answer = GREEN
                if (_answerImages[selectedIndex] != null)
                {
                    _answerImages[selectedIndex].color = new Color(0.9f, 0.25f, 0.25f, 1f); // Đỏ
                }

                if (q.correctAnswerIndex >= 0 && q.correctAnswerIndex < 4 && _answerImages[q.correctAnswerIndex] != null)
                {
                    _answerImages[q.correctAnswerIndex].color = new Color(0.2f, 0.8f, 0.35f, 1f); // Xanh lá
                }

                // Hiện Wrong Feedback ("Không đúng. Đáp án đúng là X."), Ẩn Correct Feedback
                SetFeedbackVisibility(false, true, correctLetter);
            }

            // Mở nút Tiếp theo sau khi đã chọn
            if (btnNext != null)
            {
                btnNext.interactable = true;
            }
        }

        private void OnNextClicked()
        {
            if (!_hasAnsweredCurrent || _isQuizFinalized) return;

            if (PhysicsGameSession.Instance != null && PhysicsGameSession.Instance.StudyTimeLimitReached)
            {
                FinalizeAndGoToResult();
                return;
            }

            // Requirement 8: Chưa phải câu cuối -> tiếp tục câu tiếp theo
            if (_currentIndex < _quizQuestions.Count - 1)
            {
                _currentIndex++;
                DisplayCurrentQuestion();
            }
            // Requirement 9: Đã trả lời xong câu 10 -> bấm TIẾP THEO -> chuyển sang Scene 19 Result_TN
            else
            {
                FinalizeAndGoToResult();
            }
        }

        private void FinalizeAndGoToResult()
        {
            if (_isQuizFinalized) return;
            _isQuizFinalized = true;
            float elapsed = Mathf.Max(0f, Time.time - _quizStartTime);
            int totalQ = _quizQuestions.Count;
            bool endByTime = PhysicsGameSession.Instance != null && PhysicsGameSession.Instance.StudyTimeLimitReached;

            var attempt = PhysicsGameSession.Instance.CurrentQuizAttempt;
            int answered = attempt != null && attempt.answers != null ? attempt.answers.Count : 0;
            int unanswered = Mathf.Max(0, totalQ - answered);
            if (attempt != null)
            {
                attempt.elapsedSeconds = elapsed;
                attempt.correctCount = _correctCount;
                attempt.wrongCount = Mathf.Max(0, answered - _correctCount);
                attempt.unansweredCount = unanswered;
                attempt.score = totalQ > 0 ? Mathf.RoundToInt((float)_correctCount / totalQ * 100f) : 0;
            }

            Debug.Log($"[QuizGameController] Hoàn tất Quiz: {_correctCount}/{totalQ} câu đúng, chưa trả lời {unanswered}, thời gian: {elapsed:F1}s.");

            // Requirement 14: Lưu tiến trình an toàn đúng MỘT LẦN
            PhysicsSaveData save = PhysicsSaveManager.Load();
            if (save != null)
            {
                bool isWholeBookReview = PhysicsGameSession.Instance.SelectedLessonId == PhysicsQuestionRepository.WHOLE_BOOK_REVIEW_ID;
                int historyChapterId = isWholeBookReview ? 0 : PhysicsGameSession.Instance.SelectedChapterId;
                string lessonKey = $"{historyChapterId}_{PhysicsGameSession.Instance.SelectedLessonId}";
                if (unanswered == 0) save.RecordQuizCompletion(lessonKey, _correctCount, totalQ);
                save.RecordQuizAttempt(new PhysicsQuizHistoryEntry
                {
                    attemptId = System.Guid.NewGuid().ToString("N"),
                    lessonId = PhysicsGameSession.Instance.SelectedLessonId,
                    lessonTitle = PhysicsGameSession.Instance.SelectedLessonTitle,
                    chapterId = historyChapterId,
                    activityType = "Trắc nghiệm",
                    scorePercent = totalQ > 0 ? Mathf.RoundToInt((float)_correctCount / totalQ * 100f) : 0,
                    correctCount = _correctCount,
                    wrongCount = Mathf.Max(0, answered - _correctCount),
                    unansweredCount = unanswered,
                    totalCount = totalQ,
                    isComplete = unanswered == 0,
                    assessmentKey = string.Join("|", _quizQuestions.ConvertAll(q => q.difficulty ?? "").OrderBy(x => x, StringComparer.OrdinalIgnoreCase)),
                    elapsedSeconds = elapsed,
                    sessionId = save.activeStudySession != null && save.activeStudySession.isActive ? save.activeStudySession.sessionId : "",
                    completedAtUtc = System.DateTime.UtcNow.ToString("o")
                });
                PhysicsSaveManager.Save(save);
                PhysicsGameSession.Instance.ReloadSaveData();
            }

            if (endByTime) PhysicsGameSession.Instance.FinishStudySession(endedByTime: true);

            // Cập nhật tiến độ khôi phục thiết bị theo điểm unrounded float >= 80.0f
            float scoreRaw = totalQ > 0 ? ((float)_correctCount / totalQ * 100f) : 0f;
            string lessonId = NavigationService.Instance.Selection.LessonId;
            string deviceId = NavigationService.Instance.Selection.DeviceId;
            if (string.IsNullOrEmpty(deviceId))
            {
                var dev = GameDataRepository.GetDeviceByLesson(lessonId);
                if (dev != null) deviceId = dev.DeviceId;
            }

            string attemptId = NavigationService.Instance.Attempt.AttemptId;
            if (string.IsNullOrEmpty(attemptId)) attemptId = System.Guid.NewGuid().ToString("N");

            if (!string.IsNullOrEmpty(deviceId))
            {
                RestorationService.RecordAttemptResult(deviceId, lessonId, scoreRaw, attemptId);
            }

            NavigationService.Instance.Attempt.AttemptId = attemptId;
            NavigationService.Instance.Attempt.ScoreRaw = scoreRaw;
            NavigationService.Instance.Attempt.CorrectCount = _correctCount;
            NavigationService.Instance.Attempt.TotalQuestions = totalQ;
            NavigationService.Instance.Attempt.DurationSeconds = elapsed;
            NavigationService.Instance.Attempt.IsFinished = true;
            NavigationService.Instance.Attempt.SubmittedAtUtc = System.DateTime.UtcNow.ToString("o");

            // Chuyển sang RouteId.QuizResult (Scene 19.1. Result_TL)
            NavigationService.Instance.NavigateTo(RouteId.QuizResult);
        }

        private void OnBackClicked()
        {
            Debug.Log("[QuizGameController] Người chơi bấm Quay lại -> Về RouteId.ModeSelection (Scene 17 Type).");
            NavigationService.Instance.NavigateTo(RouteId.ModeSelection);
        }
    }
}
