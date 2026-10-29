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

    [Header("Hiệu ứng khói chạy theo chân (khoi)")]
    [Tooltip("RectTransform khói - tự động tìm object 'khoi' nếu để trống")]
    public RectTransform smokeEffect;
    private float smokeOffsetY = -74.95f;
    private bool smokeOffsetInitialized = false;

    private RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        FindSmokeEffect();
    }

    public void FindSmokeEffect()
    {
        if (smokeEffect == null)
        {
            if (transform.parent != null)
            {
                Transform k = transform.parent.Find("khoi");
                if (k != null) smokeEffect = k as RectTransform;
            }
            if (smokeEffect == null)
            {
                GameObject kgo = GameObject.Find("khoi");
                if (kgo != null) smokeEffect = kgo.transform as RectTransform;
            }
        }
        if (smokeEffect != null && !smokeOffsetInitialized)
        {
            if (rt == null) rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                smokeOffsetY = smokeEffect.anchoredPosition.y - rt.anchoredPosition.y;
                smokeOffsetInitialized = true;
            }
        }
    }

    void Update()
    {
        // Không di chuyển khi đang mở quiz / pause / game over
        if (Time.timeScale <= 0f) return;
        if (RunnerController.Instance != null && !RunnerController.Instance.IsRunning) return;
        if (QuizManager.Instance != null && QuizManager.Instance.MauHienTai <= 0f) return;

        if (rt == null) rt = GetComponent<RectTransform>();
        if (rt == null) return;

        float input = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            input = -1f; // sang trái
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            input = 1f;  // sang phải
        }
        else
        {
            input = Input.GetAxisRaw("Horizontal");
        }

        if (Mathf.Abs(input) < 0.01f) return;

        Vector2 p = rt.anchoredPosition;
        p.x += Mathf.Sign(input) * moveSpeed * Time.deltaTime;
        p.x = Mathf.Clamp(p.x, minX, maxX); // giữ nhân vật trong giới hạn đường chạy
        rt.anchoredPosition = p;
    }

    void LateUpdate()
    {
        // Khói chạy flow bám sát 100% theo nhân vật nv
        if (smokeEffect == null) FindSmokeEffect();
        if (smokeEffect != null && rt != null)
        {
            Vector2 sp = smokeEffect.anchoredPosition;
            sp.x = rt.anchoredPosition.x; // bám chặt trục X theo nhân vật
            sp.y = rt.anchoredPosition.y + smokeOffsetY; // giữ chuẩn vị trí dưới chân
            smokeEffect.anchoredPosition = sp;

            // Đồng bộ trạng thái hiển thị của khói theo nhân vật
            if (smokeEffect.gameObject.activeSelf != gameObject.activeInHierarchy)
            {
                smokeEffect.gameObject.SetActive(gameObject.activeInHierarchy);
            }
        }
    }
}
