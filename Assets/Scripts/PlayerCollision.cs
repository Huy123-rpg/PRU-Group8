using System.Collections;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [Header("Thời gian bất tử tạm thời (giây)")]
    public float thoiGianHoiMau = 1f;
    private float thoiGianCho = 0f;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

    // Gộp chung hàm xử lý để code gọn gàng, chạy mượt hơn
    private void XuLyVaCham(GameObject vatChamVao)
    {
        // Nhờ dòng Debug này, hễ cái nào đi qua KHÔNG CÓ PHẢN ỨNG, bạn mở Console xem tên nó là gì rồi vào sửa đúng con đó!
        Debug.Log("Vừa quẹt qua: " + vatChamVao.name + " | Tag: [" + vatChamVao.tag + "]");

        if (vatChamVao.CompareTag("Obstacle"))
        {
            // 1. LUÔN LUÔN XÓA CHƯỚNG NGẠI VẬT (Bất kể đang bất tử hay không)
            float satThuong = 10f;
            ObstacleDamage damageScript = vatChamVao.GetComponent<ObstacleDamage>();
            if (damageScript != null) satThuong = damageScript.phanTramTruMau;

            Destroy(vatChamVao); // Tiêu diệt bẫy ngay lập tức

            // 2. NẾU KHÔNG TRONG THỜI GIAN BẤT TỬ -> TRỪ MÁU VÀ CHỚP ĐỎ
            if (thoiGianCho <= 0)
            {
                Debug.Log("Bị trừ " + satThuong + "% máu!");
                if (QuizManager.Instance != null) QuizManager.Instance.TruMau(satThuong);

                thoiGianCho = thoiGianHoiMau; // Khởi động 1 giây bất tử
                if (spriteRenderer != null) StartCoroutine(ChopDoHieuUng());
            }
            else
            {
                Debug.Log("Đang bất tử! Phá bẫy nhưng KHÔNG mất máu.");
            }
        }
    }

    private IEnumerator ChopDoHieuUng()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
    }
}