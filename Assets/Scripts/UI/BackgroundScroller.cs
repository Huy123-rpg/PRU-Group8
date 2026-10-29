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

        if (bgImageA != null)
        {
            SetFullWidth(bgImageA);
            bgImageA.anchoredPosition = new Vector2(0f, tileH * 0.5f); // A ở nửa trên
        }
        if (bgImageB != null)
        {
            SetFullWidth(bgImageB);
            bgImageB.anchoredPosition = new Vector2(0f, -tileH * 0.5f); // B nối tiếp dưới A
        }
    }

    void Update()
    {
        if (tileH <= 0f) return;

        // Dừng cuộn khi game đang mở quiz / game over / pause
        if (RunnerController.Instance != null && !RunnerController.Instance.IsRunning) return;

        float dt = Time.deltaTime; // dừng tự động khi timeScale=0

        ScrollOne(bgImageA, dt);
        ScrollOne(bgImageB, dt);
    }

    // Trôi 1 image xuống; khi đã đi hết dưới màn hình -> đưa lên trên nối tiếp (loop)
    private void ScrollOne(RectTransform img, float dt)
    {
        if (img == null) return;

        Vector2 p = img.anchoredPosition;
        p.y -= scrollSpeed * dt;
        img.anchoredPosition = p;

        // wrap: khi đầu dưới của image (p.y - tileH/2) đã lên trên đỉnh canvas (tileH/2)
        if (p.y < -tileH)
        {
            p.y += tileH * 2f;
            img.anchoredPosition = p;
        }
    }

    // Căng image ngang theo canvas (anchor trái-phải stretch), chiều cao giữ = tileH
    private static void SetFullWidth(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
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
