using UnityEngine;

/// <summary>
/// EntityMover - Di chuyển 1 vật thể UI (RectTransform) xuống theo trục Y (tạo cảm giác "đang chạy").
/// Gắn lên prefab chướng ngại vật / hồi máu / boss. RunnerController sẽ spawn, gán speed.
///
/// Vật thể tự di chuyển bằng RectTransform.anchoredPosition (không dùng Rigidbody/Camera world).
/// Dừng khi RunnerController.IsRunning = false (quiz mở / game over / pause).
/// </summary>
public class EntityMover : MonoBehaviour
{
    public enum Kind { Obstacle, HealthPickup, Boss }

    public Kind kind = Kind.Obstacle;
    [Tooltip("Tốc độ di chuyển xuống (pixels/giây) - RunnerController tự gán = worldSpeed.")]
    public float speed = 120f;
    [Tooltip("Chỉ dùng cho Boss: Y (anchored) mà boss kích hoạt Quiz khi trôi tới.")]
    public float triggerY = -100f;

    private bool hasTriggered = false;

    void Update()
    {
        if (Time.timeScale <= 0f) return;
        if (RunnerController.Instance != null && !RunnerController.Instance.IsRunning) return;

        RectTransform rt = transform as RectTransform;
        if (rt == null) return;

        Vector2 p = rt.anchoredPosition;
        p.y -= speed * Time.deltaTime;
        rt.anchoredPosition = p;

        // Boss: khi trôi tới ngưỡng triggerY -> mở Quiz (1 lần duy nhất)
        if (kind == Kind.Boss && !hasTriggered && p.y <= triggerY)
        {
            hasTriggered = true;
            if (QuizManager.Instance != null)
            {
                QuizManager.Instance.ShowQuiz(gameObject);
            }
        }

        // Tự dọn dẹp khi trôi ra khỏi đáy màn hình (chống tràn bộ nhớ theo spec)
        if (p.y < -700f && kind != Kind.Boss)
        {
            Destroy(gameObject);
        }
    }
}
