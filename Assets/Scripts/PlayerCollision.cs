using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PlayerCollision - Xử lý va chạm của nhân vật `nv` (UI-based runner).
///
/// Quy tắc theo spec:
///   - Chạm CHƯỚNG NGẠI VẬT (tag Obstacle): trừ ĐÚNG 20 HP (tuyệt đối, không %),
///     hủy chướng ngại vật ngay (không trừ máu lặp), bắt đầu 1s bất tử + chớp đỏ.
///   - Chạm VẬT PHẨM HỒI MÁU (tag Heart / hoimau): cộng ĐÚNG 30 HP (cập nhật thanh máu),
///     hủy vật phẩm (chỉ nhặt 1 lần).
///   - Nếu HP về 0 -> QuizManager.ChangeHP tự kích hoạt Game Over.
///
/// Gắn script này lên object `nv` (đã có BoxCollider2D trigger).
/// </summary>
public class PlayerCollision : MonoBehaviour
{
    [Header("Thời gian bất tử tạm thời (giây)")]
    public float thoiGianHoiMau = 1f;

    [Header("Sát thương / hồi máu (TUYỆT ĐỐI, theo spec)")]
    [Tooltip("Số HP bị trừ khi chạm chướng ngại vật (spec: 20).")]
    public int damagePerObstacleHit = 20;
    [Tooltip("Số HP được cộng khi nhặt vật phẩm hồi máu (spec: 30).")]
    public int healPerPickup = 30;

    private float thoiGianCho = 0f;
    private Image imageRenderer;

    private void Start()
    {
        // nv là UI Image (không phải SpriteRenderer) -> chớp đỏ qua màu Image
        imageRenderer = GetComponent<Image>();
    }

    private void Update()
    {
        if (thoiGianCho > 0) thoiGianCho -= Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        XuLyVaCham(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        XuLyVaCham(collision.gameObject);
    }

    // Gộp chung 2 loại va chạm (solid + trigger)
    private void XuLyVaCham(GameObject vatChamVao)
    {
        if (vatChamVao.CompareTag("Obstacle"))
        {
            // 1. LUÔN XÓA CHƯỚNG NGẠI VẬT (tránh trừ máu lặp / va chạm nhiều lần)
            Destroy(vatChamVao);

            // 2. Nếu KHÔNG trong thời gian bất tử -> trừ 20 HP tuyệt đối + SFX + bất tử
            if (thoiGianCho <= 0)
            {
                if (QuizManager.Instance != null)
                {
                    QuizManager.Instance.ChangeHP(-damagePerObstacleHit); // trừ đúng 20 HP
                }

                if (AudioManager.Instance != null) AudioManager.Instance.PlayObstacleHit(); // T7: SFX va chạm

                thoiGianCho = thoiGianHoiMau; // khởi động 1s bất tử (không bị trừ máu liên tục)
                if (imageRenderer != null) StartCoroutine(ChopDoHieuUng());
            }
        }
        else if (vatChamVao.CompareTag("Heart"))
        {
            // Nhặt vật phẩm hồi máu: hủy vật (chỉ nhặt 1 lần) + cộng 30 HP (không vượt max)
            Destroy(vatChamVao);
            if (QuizManager.Instance != null)
            {
                QuizManager.Instance.ChangeHP(+healPerPickup);
            }
            if (AudioManager.Instance != null) AudioManager.Instance.PlayGeneric(); // SFX nhặt đồ
        }
    }

    // Hiệu ứng chớp đỏ (UI Image thay vì SpriteRenderer)
    private IEnumerator ChopDoHieuUng()
    {
        if (imageRenderer == null) yield break;
        Color orig = imageRenderer.color;
        imageRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        if (imageRenderer != null) imageRenderer.color = orig;
    }
}
