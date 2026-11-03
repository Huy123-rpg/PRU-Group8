using System;
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
    /// Điều khiển logic hiển thị kết quả trắc nghiệm thật và điều hướng cho Scene 19. Result_TN / 19.1. Result_TL:
    /// - Đọc chính xác dữ liệu từ NavigationService.Instance.Attempt và PhysicsGameSession.Instance.CurrentQuizAttempt.
    /// - Tuyệt đối KHÔNG dùng số ảo hay số mẫu (80, 2, 03:45).
    /// - Không in đè nhãn chữ "Câu đúng:", "Thời gian làm bài:" lên nền sprite mỹ thuật đã có sẵn chữ thư pháp.
    /// - Điền đúng số nguyên/thời gian vào các ô số chuyên biệt:
    ///   + Điểm số: ô số trên khung điểm.
    ///   + Câu đúng: ô số trên khung CÂU ĐÚNG.
    ///   + Câu sai: ô số trên khung CÂU SAI.
    ///   + Thời gian: ô số trên khung THỜI GIAN (phút : giây).
    ///   + Trạng thái: "Đạt yêu cầu khôi phục" (>= 80đ) hoặc "Chưa đạt yêu cầu khôi phục" (< 80đ).
    /// - 3 Nút chức năng phía dưới:
    ///   + Về khu vực: quay về bản đồ chương, giữ nguyên vị trí nhân vật đã lưu, cập nhật trạng thái khôi phục máy nếu >= 80.
    ///   + Xem lời giải chi tiết: chuyển sang Scene 20. Detail Result TN (RouteId.QuizReview).
    ///   + Làm lại: tạo lượt làm mới của cùng bài học và độ khó, không reset session timer.
    /// </summary>
    public class QuizResultController : MonoBehaviour
    {
        [Header("=== TEXT REFERENCES ===")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI correctCountText;
        [SerializeField] private TextMeshProUGUI wrongCountText;
        [SerializeField] private TextMeshProUGUI singleTimeText;
        [SerializeField] private TextMeshProUGUI minutesText;
        [SerializeField] private TextMeshProUGUI secondsText;
        [SerializeField] private TextMeshProUGUI restorationStatusText;

        [Header("=== BUTTON REFERENCES ===")]
        [SerializeField] private Button btnBackToLesson;
        [SerializeField] private Button btnDetailReview;
        [SerializeField] private Button btnRetry;

        private void Awake()
        {
            EnsureEventSystem();
            AutoBindUIReferences();
            FixRaycastBlockers();
            SetupButtonListeners();
            RenderAttemptResult();
        }

        private void OnEnable()
        {
            AutoBindUIReferences();
            SetupButtonListeners();
            RenderAttemptResult();
        }

        private void Start()
        {
            RenderAttemptResult();
            EvaluateChapterCompletionStory();
        }

        private void EvaluateChapterCompletionStory()
        {
            var session = PhysicsGameSession.Instance;
            if (session == null || session.CurrentQuizAttempt == null || session.CurrentQuizAttempt.unansweredCount > 0) return;

            var requiredTest = PhysicsCurriculumData.GetLessons(session.SelectedChapterId)
                .Find(lesson => lesson != null && lesson.lessonId == session.SelectedLessonId && lesson.isTest && lesson.isRequiredForChapterCompletion);
            if (requiredTest == null) return;

            var save = session.SaveData;
            if (save == null) return;
            string scoreId = $"{session.SelectedChapterId}_{requiredTest.lessonId}";
            var score = save.lessonScores != null ? save.lessonScores.Find(entry => entry != null && entry.lessonId == scoreId) : null;
            if (score == null || !score.isCompleted) return;

            PhysicsFinalProgression.EvaluateFinalProgression(save);
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            StoryFlowController.TryShowPostGameplayOrPendingStories(
                completedChapterHint: session.SelectedChapterId,
                customSave: null,
                onAllFinished: null,
                canvasTransform: canvas != null ? canvas.transform : null);
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

        private void AutoBindUIReferences()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? GetComponent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. Tìm các TextMeshProUGUI theo vị trí chính xác đã audit
            TextMeshProUGUI[] tmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                if (t == null) continue;
                string text = t.text != null ? t.text.Trim() : "";
                Vector2 pos = t.rectTransform.anchoredPosition;

                // Ẩn số "8" thừa bên phải bảng điểm (GO=389078780 tại pos=(531.43, -10.0))
                if (pos.x > 450f && pos.x < 600f && Mathf.Abs(pos.y - -10f) < 20f && text == "8")
                {
                    t.text = "";
                    t.gameObject.SetActive(false);
                    continue;
                }

                // Điểm số: Ô số nằm trên khung điểm (y trong khoảng [180, 270], x trong khoảng [40, 150])
                if (scoreText == null && pos.y > 180f && pos.y < 270f && pos.x > 30f && pos.x < 150f)
                {
                    scoreText = t;
                    ConfigureNumberSlot(scoreText, 54f, new Color(1f, 0.95f, 0.70f, 1f));
                }
                // Số câu đúng: Khung bên trái (y trong khoảng [-250, -170], x trong khoảng [-380, -260])
                else if (correctCountText == null && pos.y < -170f && pos.y > -250f && pos.x < -240f && pos.x > -400f)
                {
                    correctCountText = t;
                    ConfigureNumberSlot(correctCountText, 44f, new Color(0.40f, 1f, 0.50f, 1f));
                }
                // Số câu sai: Khung ở giữa (y trong khoảng [-250, -170], x trong khoảng [130, 230])
                else if (wrongCountText == null && pos.y < -170f && pos.y > -250f && pos.x > 120f && pos.x < 240f)
                {
                    wrongCountText = t;
                    ConfigureNumberSlot(wrongCountText, 44f, new Color(1f, 0.45f, 0.45f, 1f));
                }
                // Khung thời gian:
                // Trường hợp 1: Có ô phút riêng (pos x ~ 620, y ~ -192) và ô giây riêng (pos x ~ 677, y ~ -192)
                else if (minutesText == null && pos.y < -160f && pos.y > -240f && pos.x > 590f && pos.x < 650f)
                {
                    minutesText = t;
                    ConfigureNumberSlot(minutesText, 42f, new Color(1f, 0.92f, 0.65f, 1f));
                }
                else if (secondsText == null && pos.y < -160f && pos.y > -240f && pos.x >= 650f && pos.x < 720f)
                {
                    secondsText = t;
                    ConfigureNumberSlot(secondsText, 42f, new Color(1f, 0.92f, 0.65f, 1f));
                }
                // Trường hợp 2: Single time text hiển thị "MM:SS" (pos x ~ 550 đến 600, y ~ -218)
                else if (singleTimeText == null && pos.y < -170f && pos.y > -250f && pos.x > 520f && pos.x < 620f)
                {
                    singleTimeText = t;
                    ConfigureNumberSlot(singleTimeText, 40f, new Color(1f, 0.92f, 0.65f, 1f));
                }
                // Dòng trạng thái khôi phục: ở giữa khung bảng điểm (pos y ~ 0)
                else if (restorationStatusText == null && Mathf.Abs(pos.y) < 30f && Mathf.Abs(pos.x - 45f) < 50f)
                {
                    restorationStatusText = t;
                    restorationStatusText.alignment = TextAlignmentOptions.Center;
                    restorationStatusText.fontSize = 22f;
                    restorationStatusText.fontStyle = FontStyles.Bold;
                    restorationStatusText.raycastTarget = false;
                }
            }

            // 2. Tìm 3 Buttons theo tọa độ X đặc trưng đã audit trong Scene 19 & 19.1
            Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                Vector2 pos = b.GetComponent<RectTransform>().anchoredPosition;

                // Nút Bên trái: X < -300 -> Về khu vực / Quay lại bài học
                if (pos.x < -300f && pos.y < -350f && btnBackToLesson == null)
                {
                    btnBackToLesson = b;
                }
                // Nút Ở giữa: X từ -200 đến 250 -> Xem lời giải chi tiết
                else if (pos.x >= -200f && pos.x <= 250f && pos.y < -350f && btnDetailReview == null)
                {
                    btnDetailReview = b;
                }
                // Nút Bên phải: X > 250 -> Làm lại
                else if (pos.x > 250f && pos.y < -350f && btnRetry == null)
                {
                    btnRetry = b;
                }
            }

            Debug.Log($"[QuizResultController] AutoBind hoàn tất: Score={scoreText?.name}, Correct={correctCountText?.name}, Wrong={wrongCountText?.name}, Status={restorationStatusText?.name}, BackBtn={btnBackToLesson?.name}, DetailBtn={btnDetailReview?.name}, RetryBtn={btnRetry?.name}");
        }

        private void ConfigureNumberSlot(TextMeshProUGUI tmp, float fontSize, Color color)
        {
            if (tmp == null) return;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = color;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
        }

        private void FixRaycastBlockers()
        {
            Button[] buttons = FindObjectsByType<Button>();
            foreach (var b in buttons)
            {
                var img = b.GetComponent<Image>();
                if (img != null) img.raycastTarget = true;

                var childTmps = b.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in childTmps)
                {
                    t.raycastTarget = false;
                }
            }
        }

        private void SetupButtonListeners()
        {
            if (btnBackToLesson != null)
            {
                btnBackToLesson.onClick.RemoveAllListeners();
                btnBackToLesson.onClick.AddListener(OnBackToLessonClicked);
                var fb = btnBackToLesson.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);
                UIHoverPulseEffect.Attach(btnBackToLesson.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
            }

            if (btnDetailReview != null)
            {
                btnDetailReview.onClick.RemoveAllListeners();
                btnDetailReview.onClick.AddListener(OnDetailReviewClicked);
                var fb = btnDetailReview.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);
                UIHoverPulseEffect.Attach(btnDetailReview.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
            }

            if (btnRetry != null)
            {
                btnRetry.onClick.RemoveAllListeners();
                btnRetry.onClick.AddListener(OnRetryClicked);
                var fb = btnRetry.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);
                UIHoverPulseEffect.Attach(btnRetry.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
            }
        }

        private void RenderAttemptResult()
        {
            int totalQ = 10;
            int correct = 0;
            int wrong = 0;
            float scoreRaw = 0f;
            float elapsedSeconds = 0f;
            bool hasData = false;

            // 1. Ưu tiên đọc từ NavigationService.Instance.Attempt
            if (NavigationService.Instance != null && NavigationService.Instance.Attempt != null &&
                NavigationService.Instance.Attempt.TotalQuestions > 0)
            {
                var att = NavigationService.Instance.Attempt;
                totalQ = att.TotalQuestions;
                correct = att.CorrectCount;
                wrong = Mathf.Max(0, totalQ - correct);
                scoreRaw = att.ScoreRaw;
                elapsedSeconds = att.DurationSeconds;
                hasData = true;
            }
            // 2. Fallback đọc từ PhysicsGameSession.Instance.CurrentQuizAttempt
            else if (PhysicsGameSession.Instance != null && PhysicsGameSession.Instance.CurrentQuizAttempt != null)
            {
                var att = PhysicsGameSession.Instance.CurrentQuizAttempt;
                correct = att.correctCount;
                wrong = att.wrongCount;
                totalQ = correct + wrong + Mathf.Max(0, att.unansweredCount);
                if (totalQ == 0) totalQ = 10;
                scoreRaw = totalQ > 0 ? ((float)correct / totalQ * 100f) : 0f;
                elapsedSeconds = att.elapsedSeconds;
                hasData = true;
            }

            if (!hasData)
            {
                Debug.LogWarning("[QuizResultController] Chưa có dữ liệu bài làm thực tế. Hiển thị 0 mặc định an toàn.");
                if (scoreText != null) scoreText.text = "0";
                if (correctCountText != null) correctCountText.text = "0";
                if (wrongCountText != null) wrongCountText.text = "0";
                if (minutesText != null) minutesText.text = "00";
                if (secondsText != null) secondsText.text = "00";
                if (singleTimeText != null) singleTimeText.text = "00:00";
                if (restorationStatusText != null)
                {
                    restorationStatusText.text = "Chưa có lượt thi";
                    restorationStatusText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
                }
                if (btnDetailReview != null) btnDetailReview.interactable = false;
                return;
            }

            // Điền SỐ NGUYÊN hoặc SỐ THỰC CHÍNH XÁC (Không kèm nhãn chữ thừa đè sprite)
            // 1. Điểm số (0 - 100)
            if (scoreText != null)
            {
                scoreText.text = Mathf.Approximately(scoreRaw, Mathf.Round(scoreRaw))
                    ? Mathf.RoundToInt(scoreRaw).ToString()
                    : $"{scoreRaw:F1}";
            }

            // 2. Số câu đúng
            if (correctCountText != null)
            {
                correctCountText.text = correct.ToString();
            }

            // 3. Số câu sai
            if (wrongCountText != null)
            {
                wrongCountText.text = wrong.ToString();
            }

            // 4. Thời gian làm bài (phút và giây)
            int totalSec = Mathf.Max(0, Mathf.RoundToInt(elapsedSeconds));
            int mins = totalSec / 60;
            int secs = totalSec % 60;

            if (minutesText != null && secondsText != null)
            {
                minutesText.text = mins.ToString("D2");
                secondsText.text = secs.ToString("D2");
            }
            else if (singleTimeText != null)
            {
                singleTimeText.text = $"{mins:D2}:{secs:D2}";
            }

            // 5. Trạng thái khôi phục thiết bị (>= 80% điểm đạt yêu cầu)
            bool isRestored = scoreRaw >= 80f;
            if (restorationStatusText != null)
            {
                restorationStatusText.text = isRestored ? "Đạt yêu cầu khôi phục" : "Chưa đạt yêu cầu khôi phục";
                restorationStatusText.color = isRestored
                    ? new Color(0.20f, 0.88f, 0.35f, 1f)
                    : new Color(0.95f, 0.40f, 0.35f, 1f);
            }

            // Cập nhật khôi phục thiết bị trên bản đồ nếu đạt >= 80
            if (isRestored)
            {
                string deviceId = NavigationService.Instance?.Selection?.DeviceId;
                string lessonId = NavigationService.Instance?.Selection?.LessonId;
                if (string.IsNullOrEmpty(deviceId))
                {
                    var dev = GameDataRepository.GetDeviceByLesson(lessonId);
                    if (dev != null) deviceId = dev.DeviceId;
                }
                if (!string.IsNullOrEmpty(deviceId))
                {
                    RestorationService.RecordAttemptResult(deviceId, lessonId, scoreRaw, NavigationService.Instance?.Attempt?.AttemptId);
                    Debug.Log($"[QuizResultController] Điểm {scoreRaw} >= 80 -> Thiết bị '{deviceId}' đã được khôi phục thành công!");
                }
            }

            // Nút xem lời giải chi tiết chỉ khả dụng khi có danh sách câu trả lời
            if (btnDetailReview != null)
            {
                bool canReview = (NavigationService.Instance != null && NavigationService.Instance.Attempt != null &&
                                  NavigationService.Instance.Attempt.Answers != null && NavigationService.Instance.Attempt.Answers.Count > 0) ||
                                 (PhysicsGameSession.Instance != null && PhysicsGameSession.Instance.CurrentQuizAttempt != null &&
                                  PhysicsGameSession.Instance.CurrentQuizAttempt.answers != null && PhysicsGameSession.Instance.CurrentQuizAttempt.answers.Count > 0);
                btnDetailReview.interactable = canReview;
            }
        }

        private void OnBackToLessonClicked()
        {
            int chapterId = NavigationService.Instance.Selection.ChapterId;
            if (chapterId < 1 || chapterId > 5)
            {
                chapterId = NavigationService.Instance.ReturnContext.ChapterId;
            }
            if ((chapterId < 1 || chapterId > 5) && PhysicsGameSession.Instance != null && PhysicsGameSession.Instance.SelectedChapterId > 0)
            {
                chapterId = PhysicsGameSession.Instance.SelectedChapterId;
            }
            if (chapterId < 1 || chapterId > 5) chapterId = 1;

            bool isCompleted = RestorationService.IsChapterFullyRestored(chapterId);
            RouteId mapRoute = SceneCatalog.GetChapterMapRoute(chapterId, isCompleted);
            Debug.Log($"[QuizResultController] Quay về khu vực bản đồ Chương {chapterId} (Route: {mapRoute}). Vị trí nhân vật bảo toàn.");
            NavigationService.Instance.NavigateTo(mapRoute);
        }

        private void OnDetailReviewClicked()
        {
            Debug.Log("[QuizResultController] Xem lời giải chi tiết -> RouteId.QuizReview (20. Detail Result TN).");
            NavigationService.Instance.NavigateTo(RouteId.QuizReview);
        }

        private void OnRetryClicked()
        {
            Debug.Log("[QuizResultController] Làm lại bài trắc nghiệm với cùng bài học và độ khó (không reset timer buổi học).");
            if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.ClearQuizAttempt();
            }

            // Tạo lượt làm bài mới
            string lessonId = NavigationService.Instance.Selection.LessonId;
            int chapterIndex = NavigationService.Instance.Selection.ChapterId;
            string difficulty = NavigationService.Instance.Selection.Difficulty;
            if (string.IsNullOrEmpty(difficulty)) difficulty = "NB";

            List<G2MCQItem> mcqs;
            if (!string.IsNullOrEmpty(lessonId) && (lessonId.StartsWith("test_") || lessonId.Contains("_test")))
            {
                mcqs = GameDataRepository.GetChapterTestMCQs(chapterIndex);
            }
            else
            {
                mcqs = GameDataRepository.GetMCQs(lessonId, difficulty, 10);
            }

            NavigationService.Instance.Attempt = new AttemptContextData
            {
                AttemptId = Guid.NewGuid().ToString("N"),
                Source = AttemptSource.RESTORATION,
                BankVersion = "1.0",
                TotalQuestions = mcqs.Count,
                QuestionCount = mcqs.Count,
                QuestionIds = mcqs.Select(q => q.cauHoiID).ToList(),
                ScoreRaw = 0f,
                CorrectCount = 0,
                IsFinished = false
            };

            NavigationService.Instance.Attempt.Answers.Clear();
            foreach (var q in mcqs)
            {
                NavigationService.Instance.Attempt.Answers.Add(new QuizAnswerRecord
                {
                    CauHoiID = q.cauHoiID,
                    QuestionText = q.noiDung,
                    Options = new[] { q.dapAnA, q.dapAnB, q.dapAnC, q.dapAnD },
                    CorrectOptionIndex = q.correctIndex,
                    Explanation = q.giaiThich,
                    KeyKnowledge = q.meoGhiNho,
                    SelectedOptionIndex = -1
                });
            }

            NavigationService.Instance.NavigateTo(RouteId.PracticeQuiz);
        }
    }
}
