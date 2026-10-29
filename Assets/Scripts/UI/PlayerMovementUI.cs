using UnityEngine;

/// <summary>
/// PlayerMovementUI - Điều khiển nhân vật `nv` di chuyển TRÁI/PHẢI trong Canvas (UI).
///
/// Vì nv là UI object (RectTransform con Canvas), di chuyển bằng RectTransform.anchoredPosition.x
/// (KHÔNG dùng world-space position). Phím: A / ← (trái), D / → (phải), giữ phím để đi liên tục.
///
/// Dừng di chuyển khi game pause / mở quiz / Game Over (RunnerController.Instance.IsRunning = false)
/// hoặc khi HP về 0. Resume thì chạy lại bình thường.
///
/// Gắn script này lên object `nv` trong SinhScene.
/// </summary>
public class PlayerMovementUI : MonoBehaviour
{
    [Header("Tốc độ di chuyển ngang")]
    [Tooltip("Tốc độ di chuyển ngang (pixels/giây) - đủ nhanh để né vật thể đang tới.")]
    public float moveSpeed = 260f;

    [Header("Giới hạn đường chạy")]
    [Tooltip("X trái nhất (pixels so với giữa) - phải KHỚP với runPathMinX của RunnerController.")]
    public float minX = -160f;
    [Tooltip("X phải nhất (pixels so với giữa) - phải KHỚP với runPathMaxX của RunnerController.")]
    public float maxX = 160f;

    private RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Không di chuyển khi đang mở quiz / pause / game over
        if (RunnerController.Instance != null && !RunnerController.Instance.IsRunning) return;

        float input = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            input = -1f; // sang trái
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            input = 1f;  // sang phải
        }

        if (input == 0f) return;

        Vector2 p = rt.anchoredPosition;
        p.x += input * moveSpeed * Time.deltaTime;
        p.x = Mathf.Clamp(p.x, minX, maxX); // giữ nhân vật trong đường chạy
        rt.anchoredPosition = p;
    }
}
