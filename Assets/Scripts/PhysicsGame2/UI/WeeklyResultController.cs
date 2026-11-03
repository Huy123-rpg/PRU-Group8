using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển logic giao diện Scene 32. Result_Tự luận (tái sử dụng cho Kết quả Thử thách tuần)
    /// theo đặc tả Mục 12.3:
    /// - Tiêu đề: "KẾT QUẢ THỬ THÁCH TUẦN".
    /// - Hiển thị điểm số, số đúng, số sai, thời gian làm bài, và trạng thái tính hạng tuần.
    /// - 3 nút:
    ///   1. VỀ BẢNG XẾP HẠNG -> RouteId.WeeklyLeaderboard (Scene 23. Bảng xếp hạng).
    ///   2. XEM LỜI GIẢI CHI TIẾT -> RouteId.QuizReview (Scene 20. Detail Result TN).
    ///   3. LUYỆN TẬP LẠI (KHÔNG TÍNH HẠNG) -> RouteId.WeeklyQuiz (Scene 11.1_Quiz 1).
    /// </summary>
    public class WeeklyResultController : MonoBehaviour
    {
        [Header("=== TEXT REFERENCES ===")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI correctCountText;
        [SerializeField] private TextMeshProUGUI wrongCountText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI rankStatusText;

        [Header("=== BUTTON REFERENCES ===")]
        [SerializeField] private Button btnLeaderboard;
        [SerializeField] private Button btnViewDetail;
        [SerializeField] private Button btnRetryPractice;

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
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }
        }

        private void FixRaycastBlockers()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
            {
                if (img.GetComponent<Button>() != null) continue;
                string n = img.gameObject.name.ToLower();
                if (n.Contains("bg") || n.Contains("frame") || n.Contains("panel") || n == "image")
                {
                    img.raycastTarget = false;
                }
            }

            foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (tmp.GetComponent<Button>() == null)
                {
                    tmp.raycastTarget = false;
                }
            }
        }

        private void AutoBindUIReferences()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Tìm và sửa tiêu đề thành KẾT QUẢ THỬ THÁCH TUẦN
            TextMeshProUGUI[] tmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                string text = t.text != null ? t.text.Trim() : "";

                if (text.Contains("TRẮC NGHIỆM") || text.Contains("TỰ LUẬN") || text.Contains("KẾT QUẢ"))
                {
                    titleText = t;
                    titleText.text = "KẾT QUẢ THỬ THÁCH TUẦN";
                }
                else if (text.Contains("điểm") || text == "8" || text == "80")
                {
                    scoreText = t;
                }
                else if (text.StartsWith("Câu đúng") || text.Contains("đúng"))
                {
                    correctCountText = t;
                }
                else if (text.StartsWith("Câu sai") || text.Contains("sai"))
                {
                    wrongCountText = t;
                }
                else if (text.StartsWith("Thời gian") || text.Contains("phút"))
                {
                    timeText = t;
                }
            }

            // Tìm 3 nút
            Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                string bText = "";
                var tmp = b.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp != null && tmp.text != null) bText = tmp.text.ToLowerInvariant();

                if (bText.Contains("khu vực") || bText.Contains("bài học") || bText.Contains("bảng xếp hạng") || bText.Contains("xếp hạng"))
                {
                    btnLeaderboard = b;
                    if (tmp != null) tmp.text = "VỀ BẢNG XẾP HẠNG";
                }
                else if (bText.Contains("chi tiết") || bText.Contains("lời giải"))
                {
                    btnViewDetail = b;
                }
                else if (bText.Contains("làm lại") || bText.Contains("luyện tập"))
                {
                    btnRetryPractice = b;
                    if (tmp != null) tmp.text = "LUYỆN TẬP LẠI (KHÔNG TÍNH HẠNG)";
                }
            }

            // Tạo rankStatusText nếu chưa có
            if (rankStatusText == null)
            {
                GameObject rankGo = new GameObject("RankStatusText");
                rankGo.transform.SetParent(canvas.transform, false);
                rankStatusText = rankGo.AddComponent<TextMeshProUGUI>();
                rankStatusText.fontSize = 20;
                rankStatusText.fontStyle = FontStyles.Bold;
                rankStatusText.alignment = TextAlignmentOptions.Center;
                rankStatusText.color = new Color(0.96f, 0.82f, 0.38f, 1f);
                rankStatusText.raycastTarget = false;

                RectTransform rt = rankGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0, 160);
                rt.sizeDelta = new Vector2(700, 40);
            }
        }

        private void SetupButtonListeners()
        {
            if (btnLeaderboard != null)
            {
                UIHoverPulseEffect.Attach(btnLeaderboard.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
                btnLeaderboard.onClick.RemoveAllListeners();
                btnLeaderboard.onClick.AddListener(OnLeaderboardClicked);
            }

            if (btnViewDetail != null)
            {
                UIHoverPulseEffect.Attach(btnViewDetail.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
                btnViewDetail.onClick.RemoveAllListeners();
                btnViewDetail.onClick.AddListener(OnViewDetailClicked);
            }

            if (btnRetryPractice != null)
            {
                UIHoverPulseEffect.Attach(btnRetryPractice.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
                btnRetryPractice.onClick.RemoveAllListeners();
                btnRetryPractice.onClick.AddListener(OnRetryPracticeClicked);
            }
        }

        private void RenderAttemptResult()
        {
            float score = NavigationService.Instance.AttemptContext.Score;
            float elapsed = NavigationService.Instance.AttemptContext.ElapsedSeconds;
            int correct = NavigationService.Instance.AttemptContext.CorrectCount;
            int wrong = NavigationService.Instance.AttemptContext.WrongCount;

            if (score <= 0f && correct == 0)
            {
                // Fallback direct play
                score = 90.0f;
                correct = 9;
                wrong = 1;
                elapsed = 360f;
            }

            if (titleText != null) titleText.text = "KẾT QUẢ THỬ THÁCH TUẦN";
            if (scoreText != null) scoreText.text = $"{score:F0} điểm";
            if (correctCountText != null) correctCountText.text = $"Câu đúng: {correct}";
            if (wrongCountText != null) wrongCountText.text = $"Câu sai: {wrong}";

            if (timeText != null)
            {
                int totalSec = Mathf.Max(0, Mathf.RoundToInt(elapsed));
                int minutes = totalSec / 60;
                int seconds = totalSec % 60;
                timeText.text = $"Thời gian làm bài: {minutes} phút {seconds:D2} giây";
            }

            if (rankStatusText != null)
            {
                rankStatusText.text = "✦ ĐÃ GHI NHẬN KẾT QUẢ VÀO BẢNG XẾP HẠNG TUẦN ✦";
            }
        }

        private void OnLeaderboardClicked()
        {
            Debug.Log("[WeeklyResultController] Quay về Bảng xếp hạng tuần: RouteId.WeeklyLeaderboard (Scene 23).");
            NavigationService.Instance.NavigateTo(RouteId.WeeklyLeaderboard);
        }

        private void OnViewDetailClicked()
        {
            Debug.Log("[WeeklyResultController] Xem lời giải chi tiết: RouteId.QuizReview (Scene 20).");
            NavigationService.Instance.NavigateTo(RouteId.QuizReview);
        }

        private void OnRetryPracticeClicked()
        {
            Debug.Log("[WeeklyResultController] Luyện tập lại không tính hạng -> RouteId.WeeklyQuiz.");
            NavigationService.Instance.AttemptContext.Source = AttemptSource.WeeklyPractice;
            NavigationService.Instance.NavigateTo(RouteId.WeeklyQuiz);
        }
    }
}
