using UnityEngine;

/// <summary>
/// AutoScroll (Legacy) - Đã được thay thế hoàn toàn bằng BackgroundScroller UI trong Canvas.
/// Tự hủy/vô hiệu hóa để tránh Camera bị trôi lệch khỏi thế giới theo spec mục 2.
/// </summary>
public class AutoScroll : MonoBehaviour
{
    [Header("Tốc độ cuộn của Map (Legacy)")]
    public float speed = 0f;

    void Awake()
    {
        // Spec mục 2 & 3: Không dùng Camera world-space cuộn nữa, hủy component này
        enabled = false;
        Destroy(this);
    }

    void Update()
    {
        // Vô hiệu hóa logic đẩy camera
    }
}