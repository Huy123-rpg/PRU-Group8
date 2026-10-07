using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý logic UI cho Scene 7 - Results (Kết Quả)
/// Đọc dữ liệu từ GameSession và hiển thị với hiệu ứng animation.
/// </summary>
public class ResultsUIController : MonoBehaviour
{
    [Header("=== THÔNG TIN NGƯỜI CHƠI ===")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerMetaText; // Môn học + Chương

    [Header("=== KẾT QUẢ & XẾP HẠNG ===")]
    public TextMeshProUGUI rankValueText;  // Vd: Hạng 1 / 4
    public TextMeshProUGUI rankMedalText;  // Vd: 🥇

    [Header("=== THỐNG KÊ CHI TIẾT ===")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI correctText;
    public TextMeshProUGUI wrongText;
    public TextMeshProUGUI timeText;

    [Header("=== NÚT BẤM ===")]
    public Button replayButton;
    public Button leaderboardButton;
    public Button menuButton;

    [Header("=== CẤU HÌNH ANIMATION ===")]
    public float scoreAnimationDuration = 1.5f;
    public GameObject trophyIcon;

    private int targetScore = 0;

    private void Start()
    {
        SetupEventListeners();
        LoadResultData();
        PlayTrophyAnimation();
    }

    /// <summary>
    /// Tìm hoặc gắn sự kiện cho các nút
    /// </summary>
    private void SetupEventListeners()
    {
        if (replayButton != null)
        {
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(OnReplayClicked);
        }

        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.RemoveAllListeners();
            leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(OnMenuClicked);
        }
    }

    /// <summary>
    /// Lấy dữ liệu từ GameSession để đưa lên UI
    /// </summary>
    private void LoadResultData()
    {
        if (GameSession.Instance == null)
        {
            Debug.LogWarning("[ResultsUIController] Không tìm thấy GameSession. Hiển thị dữ liệu mẫu.");
            DisplayMockData();
            return;
        }

        // 1. Thông tin người chơi
        if (playerNameText != null)
        {
            playerNameText.text = $"Học viên: {GameSession.Instance.username}";
        }

        if (playerMetaText != null)
        {
            string subjectName = GameSession.Instance.selectedSubject != null ? GameSession.Instance.selectedSubject.subjectName : "Chưa chọn môn";
            string chapterName = GameSession.Instance.selectedChapter != null ? GameSession.Instance.selectedChapter.chapterTitle : "";
            string lessonName = !string.IsNullOrEmpty(GameSession.Instance.selectedLessonTitle) ? $" • {GameSession.Instance.selectedLessonTitle}" : "";
            playerMetaText.text = $"{subjectName} • {chapterName}{lessonName}";
        }

        // 2. Dữ liệu trận đua vừa qua
        RaceResult result = GameSession.Instance.lastRaceResult;
        if (result != null)
        {
            // Rank
            if (rankValueText != null) rankValueText.text = $"Hạng {result.rank} / 4";
            if (rankMedalText != null) rankMedalText.text = GetMedalEmoji(result.rank);

            // Thống kê (Câu đúng, sai, thời gian)
            if (correctText != null) correctText.text = result.correctAnswers.ToString();
            if (wrongText != null) wrongText.text = result.wrongAnswers.ToString();
            
            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(result.completionTime / 60f);
                int seconds = Mathf.FloorToInt(result.completionTime % 60f);
                timeText.text = $"{minutes}:{seconds:00}";
            }

            // Điểm số (Animation)
            targetScore = result.totalScore;
            StartCoroutine(AnimateScoreCounter());
        }
        else
        {
            DisplayMockData();
        }
    }

    private void DisplayMockData()
    {
        if (playerNameText != null) playerNameText.text = "Học viên: Demo";
        if (playerMetaText != null) playerMetaText.text = "Sinh Học • Chương 1";
        if (rankValueText != null) rankValueText.text = "Hạng 1 / 4";
        if (rankMedalText != null) rankMedalText.text = "🥇";
        
        if (correctText != null) correctText.text = "8";
        if (wrongText != null) wrongText.text = "2";
        if (timeText != null) timeText.text = "1:45";

        targetScore = 850;
        StartCoroutine(AnimateScoreCounter());
    }

    private string GetMedalEmoji(int rank)
    {
        switch (rank)
        {
            case 1: return "🥇";
            case 2: return "🥈";
            case 3: return "🥉";
            default: return "🏅";
        }
    }

    /// <summary>
    /// Hiệu ứng đếm số điểm tăng dần
    /// </summary>
    private IEnumerator AnimateScoreCounter()
    {
        if (scoreText == null) yield break;

        float timer = 0f;
        while (timer < scoreAnimationDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / scoreAnimationDuration;
            
            // Easing function (easeOutCubic) để đếm số mượt hơn
            float easeOut = 1f - Mathf.Pow(1f - progress, 3f);
            int currentScore = Mathf.RoundToInt(targetScore * easeOut);
            
            scoreText.text = currentScore.ToString();
            yield return null;
        }

        scoreText.text = targetScore.ToString();
    }

    /// <summary>
    /// Hiệu ứng scale lên xuống cho cúp
    /// </summary>
    private void PlayTrophyAnimation()
    {
        if (trophyIcon != null)
        {
            // Nếu project có LeanTween thì dùng
            // Reset scale
            // trophyIcon.transform.localScale = Vector3.zero;
            // LeanTween.scale(trophyIcon, Vector3.one, 0.8f).setEaseOutBounce();

            // Nếu không dùng LeanTween thì dùng Coroutine đơn giản
            StartCoroutine(TrophyBounceRoutine());
        }
    }

    private IEnumerator TrophyBounceRoutine()
    {
        float duration = 0.8f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Easing overshoot nhỏ
            float scale = 1 + 0.5f * Mathf.Sin(t * Mathf.PI) * (1 - t);
            trophyIcon.transform.localScale = Vector3.one * scale * (1 - Mathf.Pow(1 - t, 3));
            yield return null;
        }
        trophyIcon.transform.localScale = endScale;
    }

    // ============================================
    // CÁC HÀM XỬ LÝ NÚT BẤM (ĐIỀU HƯỚNG)
    // ============================================

    public void OnReplayClicked()
    {
        if (NavigationManager.Instance != null)
        {
            // Chơi lại thì về màn chọn Chó hoặc vào thẳng màn Đua.
            // Ở đây cho về màn Dog Select để chọn lại chó nếu muốn
            NavigationManager.Instance.LoadScene(NavigationManager.SCENE_DOG_SELECT);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Scene5_DogSelect");
        }
    }

    public void OnLeaderboardClicked()
    {
        Debug.Log("Mở Bảng Xếp Hạng - Tính năng sẽ làm ở các phần sau.");
        // Gợi ý: Có thể bật một Panel popup Leaderboard lên đây
    }

    public void OnMenuClicked()
    {
        if (NavigationManager.Instance != null)
        {
            NavigationManager.Instance.LoadScene(NavigationManager.SCENE_MAIN_MENU);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Scene2_MainMenu");
        }
    }
}
