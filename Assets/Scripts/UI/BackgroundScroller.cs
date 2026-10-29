using UnityEngine;

/// <summary>
/// BackgroundScroller - Cuộn nền (2 UI Image chồng nhau) VÔ TẬN theo trục Y.
///
/// Yêu cầu (spec): nền là UI Image trong Canvas, cuộn liên tục, nối liền mép,
/// dừng khi pause/quiz/Game Over, tiếp tục khi resume, cùng tốc độ với gameplay.
///
/// Cách làm: 2 Image "bg" (dùng CÙNG 1 sprite, width phủ ngang canvas) đặt nối tiếp
/// theo trục Y. Mỗi frame cả 2 cùng trôi xuống; ảnh nào đã trôi xuống dưới hết màn hình
/// thì đưa về phía trên (nối tiếp ảnh còn lại) -> vòng lặp vô hạn, không hở mép.
/// Dừng khi RunnerController.Instance.IsRunning = false (quiz / game over / pause).
///
/// Gắn script này lên object "BgScroll" (con Canvas, chứa 2 Image "bg" là con của nó).
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("2 Image nền (dùng CÙNG 1 sprite để nối liền, không đường cắt)")]
    [Tooltip("Image nền thứ 1.")]
    public RectTransform bgImageA;
    [Tooltip("Image nền thứ 2 (nối tiếp A).")]
    public RectTransform bgImageB;

    [Header("Tốc độ")]
    [Tooltip("Tốc độ cuộn (pixels/giây) - nên khớp với worldSpeed của RunnerController.")]
    public float scrollSpeed = 120f;

    private float tileH; // chiều cao mỗi image (để tính điểm nối / wrap)

    void Start()
    {
        tileH = CalcTileHeight();

        // Khởi tạo: Ảnh A chiếm trọn màn hình (y = 0), Ảnh B nối liền ngay trên đỉnh A (y = tileH)
        // Không xuất hiện vệt cắt ngang giữa màn hình ở thời điểm bắt đầu
        if (bgImageA != null)
        {
            SetFullWidth(bgImageA, tileH);
            bgImageA.anchoredPosition = new Vector2(0f, 0f);
        }
        if (bgImageB != null)
        {
            SetFullWidth(bgImageB, tileH);
            bgImageB.anchoredPosition = new Vector2(0f, tileH);
        }
    }

    void Update()
    {
        if (tileH <= 0f) return;

        // Dừng cuộn khi game đang mở quiz / game over / pause
        if (RunnerController.Instance != null && !RunnerController.Instance.IsRunning) return;
        if (Time.timeScale <= 0f) return;

        float dt = Time.deltaTime;

        ScrollOne(bgImageA, dt);
        ScrollOne(bgImageB, dt);
    }

    // Trôi 1 image xuống; khi mép trên của ảnh đã chạm mép dưới màn hình -> đưa lên trên nối tiếp (loop vô hạn)
    private void ScrollOne(RectTransform img, float dt)
    {
        if (img == null) return;

        Vector2 p = img.anchoredPosition;
        p.y -= scrollSpeed * dt;
        img.anchoredPosition = p;

        // Wrap: khi ảnh đã ra khỏi mép dưới màn hình hoàn toàn (p.y <= -tileH)
        if (p.y <= -tileH)
        {
            p.y += tileH * 2f;
            img.anchoredPosition = p;
        }
    }

    // Căng image ngang theo canvas (anchor trái-phải stretch), chiều cao đặt chính xác = tileH
    private static void SetFullWidth(RectTransform rt, float height)
    {
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(0f, height);
        rt.localScale = Vector3.one;
    }

    private float CalcTileHeight()
    {
        // Chiều cao màn hình (canvas) - để mỗi "ô" nền cao vừa đúng màn hình
        Canvas cv = FindFirstObjectByType<Canvas>();
        if (cv != null)
        {
            RectTransform crt = cv.GetComponent<RectTransform>();
            if (crt != null && crt.rect.height > 0f) return crt.rect.height;
        }
        if (bgImageA != null && bgImageA.rect.height > 0f) return bgImageA.rect.height;
        return 1080f; // dự phòng (refResolution mặc định 1920x1080)
    }
}
