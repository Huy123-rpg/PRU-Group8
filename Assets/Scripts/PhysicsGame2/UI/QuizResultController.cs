using System;
using System.Globalization;
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
        [SerializeField] private Button btnHome;

        private void Awake()
        {
            EnsureEventSystem();
            AutoBindUIReferences();
            FixRaycastBlockers();
            SetupButtonListeners();
            RenderAttemptResult();
            EnsureAttemptRecorded();
        }

        private void OnEnable()
        {
            AutoBindUIReferences();
            SetupButtonListeners();
            RenderAttemptResult();
            EnsureAttemptRecorded();
        }

        private void Start()
        {
            RenderAttemptResult();
            EnsureAttemptRecorded();
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

            // 1. Title Banner: Đảm bảo hiển thị đúng banner KẾT QUẢ TRẮC NGHIỆM và ẩn ảnh chữ thừa đè nút Về khu vực
            Image[] allImages = canvas.GetComponentsInChildren<Image>(true);
            foreach (var img in allImages)
            {
                if (img == null) continue;
                Vector2 pos = img.rectTransform.anchoredPosition;

                // Ẩn Image (13) hoặc ảnh chữ thư pháp cũ nằm đè sau nút Về khu vực
                if (img.gameObject.name == "Image (13)" || (pos.x < -400f && pos.y < -400f && img.GetComponentInParent<Button>() == null))
                {
                    img.gameObject.SetActive(false);
                    continue;
                }

                if (pos.y > 380f && Mathf.Abs(pos.x) < 100f)
                {
                    if (img.sprite == null || img.sprite.name.Contains("tự luận") || img.sprite.name.Contains("tu luan"))
                    {
                        Sprite tnSprite = Resources.FindObjectsOfTypeAll<Sprite>()
                            .FirstOrDefault(s => s != null && (s.name == "kết quả trắc nghiệm" || s.name.Contains("kết quả trắc nghiệm")));
                        if (tnSprite != null)
                        {
                            img.sprite = tnSprite;
                        }
                    }
                    img.raycastTarget = false;
                }
            }

            // 2. Tìm 3 nút điều hướng đáy trước để thiết lập label chuẩn và tránh gán nhầm trạng thái khôi phục vào nút
            bool isRestoration = NavigationService.Instance != null &&
                                 NavigationService.Instance.Attempt != null &&
                                 NavigationService.Instance.Attempt.Source == AttemptSource.RESTORATION;

            Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                if (b == null) continue;
                Vector2 pos = b.GetComponent<RectTransform>().anchoredPosition;
                string btnName = b.gameObject.name.ToLowerInvariant();
                var bTmp = b.GetComponentInChildren<TextMeshProUGUI>(true);
                string textVal = bTmp != null && bTmp.text != null ? bTmp.text.ToLowerInvariant() : "";

                if (btnHome == null && (btnName.Contains("home") || btnName.Contains("mainmenu") || btnName.Contains("trangchu") || textVal.Contains("trang chủ") || textVal.Contains("trang chu")))
                {
                    btnHome = b;
                }

                // Nút Bên trái: X < -300 -> Về khu vực (nếu RESTORATION) hoặc Về trang chủ (nếu PROGRESS_PRACTICE / PRACTICE / mặc định)
                if (pos.x < -300f && pos.y < -350f && btnBackToLesson == null)
                {
                    btnBackToLesson = b;
                    if (bTmp != null)
                    {
                        bTmp.text = isRestoration ? "Về khu vực" : "Về trang chủ";
                        bTmp.alignment = TextAlignmentOptions.Center;
                        bTmp.fontSize = 24f;
                        bTmp.fontStyle = FontStyles.Bold;
                        bTmp.color = Color.white;
                        bTmp.raycastTarget = false;
                    }
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

                // Ẩn toàn bộ child TextMeshPro bên trong 3 khung card mỹ thuật Câu đạt / Cần cải thiện / Thời gian
                // để tuyệt đối không gây chồng chữ với số liệu runtime
                if (pos.y < -150f && pos.y > -230f && (pos.x < -400f || Mathf.Abs(pos.x - 23f) < 50f || pos.x > 400f))
                {
                    var childTmps = b.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var ct in childTmps)
                    {
                        ct.text = "";
                        ct.gameObject.SetActive(false);
                    }
                }
            }

            // 3. Tìm các TextMeshProUGUI chính xác
            TextMeshProUGUI[] tmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            List<TextMeshProUGUI> timeCandidates = new List<TextMeshProUGUI>();

            foreach (var t in tmps)
            {
                if (t == null) continue;
                // BỎ QUA toàn bộ TMP thuộc Button để không bao giờ ghi đè lên nhãn nút
                if (t.GetComponentInParent<Button>() != null) continue;

                string text = t.text != null ? t.text.Trim() : "";
                Vector2 pos = t.rectTransform.anchoredPosition;

                // Ẩn số "8" thừa bên phải bảng điểm
                if (pos.x > 450f && pos.x < 600f && Mathf.Abs(pos.y - -10f) < 30f && text == "8")
                {
                    t.text = "";
                    t.gameObject.SetActive(false);
                    continue;
                }

                // Điểm số: Ô số nằm chính giữa vòng nguyệt quế
                if (scoreText == null && pos.y > 150f && pos.y < 270f && pos.x > 20f && pos.x < 150f)
                {
                    scoreText = t;
                    scoreText.rectTransform.anchoredPosition = new Vector2(49.97f, 179.23f);
                    scoreText.rectTransform.sizeDelta = new Vector2(160f, 80f);
                    ConfigureNumberSlot(scoreText, 64f, new Color(1f, 0.95f, 0.70f, 1f));
                }
                // Số câu đúng: Khung vàng "Câu đạt" bên trái -> Căn chính xác giữa ô vuông vàng (Card 1: slot center X = -365, Y = -192)
                else if (correctCountText == null && pos.y < -160f && pos.y > -250f && pos.x < -250f)
                {
                    correctCountText = t;
                    ConfigureNumberSlot(correctCountText, 38f, new Color(0.35f, 1f, 0.50f, 1f));
                    correctCountText.rectTransform.anchoredPosition = new Vector2(-365f, -192f);
                    correctCountText.rectTransform.sizeDelta = new Vector2(80f, 60f);
                }
                // Số câu sai: Khung vàng "Cần cải thiện" ở giữa -> Căn chính xác giữa ô vuông vàng (Card 2: slot center X = 120, Y = -192)
                else if (wrongCountText == null && pos.y < -160f && pos.y > -250f && pos.x > 0f && pos.x < 250f)
                {
                    wrongCountText = t;
                    ConfigureNumberSlot(wrongCountText, 38f, new Color(1f, 0.45f, 0.45f, 1f));
                    wrongCountText.rectTransform.anchoredPosition = new Vector2(120f, -192f);
                    wrongCountText.rectTransform.sizeDelta = new Vector2(80f, 60f);
                }
                // Khung thời gian bên phải (X > 450, Y trong khoảng [-250, -160])
                else if (pos.y < -160f && pos.y > -250f && pos.x > 450f && pos.x < 740f)
                {
                    timeCandidates.Add(t);
                }
                // Dòng trạng thái khôi phục: ở giữa khung bảng điểm (Y ~ 0, X ~ 45), KHÔNG thuộc Button
                else if (restorationStatusText == null && Mathf.Abs(pos.y) < 50f && Mathf.Abs(pos.x - 45f) < 60f)
                {
                    restorationStatusText = t;
                    restorationStatusText.rectTransform.anchoredPosition = new Vector2(46f, 0f);
                    restorationStatusText.rectTransform.sizeDelta = new Vector2(450f, 44f);
                    restorationStatusText.alignment = TextAlignmentOptions.Center;
                    restorationStatusText.fontSize = 24f;
                    restorationStatusText.fontStyle = FontStyles.Bold;
                    restorationStatusText.raycastTarget = false;
                }
            }

            // Xử lý khung thời gian duy nhất để không bao giờ bị ô vuông thừa hay lệch số
            if (timeCandidates.Count > 0)
            {
                // Căn chính xác giữa ô vuông vàng của khung Thời gian (Card 3: slot center X=595, Y=-169)
                singleTimeText = timeCandidates[0];
                ConfigureNumberSlot(singleTimeText, 24f, new Color(1f, 0.95f, 0.70f, 1f));
                singleTimeText.rectTransform.anchoredPosition = new Vector2(595f, -169f);
                singleTimeText.rectTransform.sizeDelta = new Vector2(84f, 44f);

                // Ẩn tất cả các candidate thừa khác để triệt tiêu hoàn toàn hiện tượng chồng chữ
                for (int i = 1; i < timeCandidates.Count; i++)
                {
                    timeCandidates[i].text = "";
                    timeCandidates[i].gameObject.SetActive(false);
                }
                minutesText = null;
                secondsText = null;
            }

            Debug.Log($"[QuizResultController] AutoBind hoàn tất: Score={scoreText?.name}, Correct={correctCountText?.name}, Wrong={wrongCountText?.name}, Status={restorationStatusText?.name}, Time={singleTimeText?.name}");
        }

        private void ConfigureNumberSlot(TextMeshProUGUI tmp, float fontSize, Color color)
        {
            if (tmp == null) return;
            tmp.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            tmp.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            tmp.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
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
            Canvas canvas = GetComponentInParent<Canvas>() ?? GetComponent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                // Tắt raycastTarget trên tất cả Image trang trí trôi nổi đè lên các nút
                Image[] allImages = canvas.GetComponentsInChildren<Image>(true);
                foreach (var img in allImages)
                {
                    if (img == null) continue;
                    if (img.gameObject.name == "Image (13)" || img.gameObject.name == "Image (12)" || img.gameObject.name == "Image (5)" || img.gameObject.name == "Image (11)" || img.gameObject.name == "Image (4)")
                    {
                        img.raycastTarget = false;
                        if (img.gameObject.name == "Image (13)")
                        {
                            img.gameObject.SetActive(false);
                        }
                    }
                    else if (img.GetComponent<Button>() == null && img.GetComponentInParent<Button>() == null)
                    {
                        img.raycastTarget = false;
                    }
                }
            }

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
            bool isRestoration = NavigationService.Instance != null &&
                                 NavigationService.Instance.Attempt != null &&
                                 NavigationService.Instance.Attempt.Source == AttemptSource.RESTORATION;

            if (btnBackToLesson != null)
            {
                btnBackToLesson.onClick.RemoveAllListeners();
                if (isRestoration)
                {
                    btnBackToLesson.onClick.AddListener(OnBackToLessonClicked);
                }
                else
                {
                    btnBackToLesson.onClick.AddListener(OnBackToMainMenuClicked);
                }
                var fb = btnBackToLesson.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);
                UIHoverPulseEffect.Attach(btnBackToLesson.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
            }

            if (btnHome != null && btnHome != btnBackToLesson)
            {
                btnHome.onClick.RemoveAllListeners();
                btnHome.onClick.AddListener(OnBackToMainMenuClicked);
                var fb = btnHome.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);
                UIHoverPulseEffect.Attach(btnHome.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
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

        public void OnBackToMainMenuClicked()
        {
            Debug.Log("[QuizResultController] Người chơi bấm 'Về trang chủ' -> Chuyển về Main Menu Aurelia (RouteId.MainMenu / 01_MainMenu).");
            EnsureAttemptRecorded();
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.NavigateTo(RouteId.MainMenu);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(PhysicsSceneNames.MainMenu);
            }
        }

        private void EnsureAttemptRecorded()
        {
            if (NavigationService.Instance == null || NavigationService.Instance.Attempt == null) return;
            var att = NavigationService.Instance.Attempt;
            if (!att.IsFinished && att.TotalQuestions == 0) return;

            var save = PhysicsSaveManager.Load();
            if (save == null) return;
            if (save.quizHistory == null) save.quizHistory = new List<PhysicsQuizHistoryEntry>();

            string attId = att.AttemptId;
            if (string.IsNullOrEmpty(attId))
            {
                attId = Guid.NewGuid().ToString("N");
                att.AttemptId = attId;
            }

            // Nếu attemptId này đã được lưu vào quizHistory thì không lưu trùng lặp
            if (save.quizHistory.Any(x => x != null && x.attemptId == attId))
            {
                return;
            }

            int score = Mathf.RoundToInt(att.ScoreRaw);
            string lessonId = NavigationService.Instance.Selection != null && !string.IsNullOrEmpty(NavigationService.Instance.Selection.LessonId)
                ? NavigationService.Instance.Selection.LessonId
                : (PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SelectedLessonId : "");

            int chapterId = NavigationService.Instance.Selection != null && NavigationService.Instance.Selection.ChapterId > 0
                ? NavigationService.Instance.Selection.ChapterId
                : (PhysicsGameSession.Instance != null && PhysicsGameSession.Instance.SelectedChapterId > 0 ? PhysicsGameSession.Instance.SelectedChapterId : StudyScreensController.GetChapterForLesson(lessonId));
            if (chapterId <= 0) chapterId = StudyScreensController.GetChapterForLesson(lessonId);

            string lessonTitle = NavigationService.Instance.Selection != null && !string.IsNullOrEmpty(NavigationService.Instance.Selection.LessonTitle)
                ? NavigationService.Instance.Selection.LessonTitle
                : (PhysicsGameSession.Instance != null && !string.IsNullOrEmpty(PhysicsGameSession.Instance.SelectedLessonTitle) ? PhysicsGameSession.Instance.SelectedLessonTitle : StudyScreensController.GetLessonTitle(lessonId));

            string diff = NavigationService.Instance.Selection != null && !string.IsNullOrEmpty(NavigationService.Instance.Selection.Difficulty)
                ? NavigationService.Instance.Selection.Difficulty
                : "TH";

            save.lastLessonId = lessonId;
            save.lastChapterId = chapterId;

            save.RecordQuizAttempt(new PhysicsQuizHistoryEntry
            {
                attemptId = attId,
                lessonId = lessonId,
                lessonTitle = lessonTitle,
                chapterId = chapterId,
                activityType = "Trắc nghiệm",
                scorePercent = score,
                correctCount = att.CorrectCount,
                wrongCount = Mathf.Max(0, att.TotalQuestions - att.CorrectCount),
                unansweredCount = 0,
                totalCount = att.TotalQuestions,
                isComplete = true,
                assessmentKey = diff,
                elapsedSeconds = att.DurationSeconds,
                sessionId = save.activeStudySession != null && save.activeStudySession.isActive ? save.activeStudySession.sessionId : "",
                completedAtUtc = string.IsNullOrEmpty(att.SubmittedAtUtc) ? DateTime.UtcNow.ToString("o") : att.SubmittedAtUtc
            });

            PhysicsSaveManager.Save(save);
            if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.ReloadSaveData();
            }
            Debug.Log($"[QuizResultController] EnsureAttemptRecorded thành công: attemptId={attId}, lesson={lessonId}, ch={chapterId}, score={score}%");
        }

        private void OnBackToLessonClicked()
        {
            EnsureAttemptRecorded();
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
            EnsureAttemptRecorded();
            Debug.Log("[QuizResultController] Xem lời giải chi tiết -> RouteId.QuizReview (20. Detail Result TN).");
            NavigationService.Instance.NavigateTo(RouteId.QuizReview);
        }

        private void OnRetryClicked()
        {
            EnsureAttemptRecorded();
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
