using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BackgroundScroller - Nền đường cuộn VÔ TẬN theo trục Y (hiệu ứng "đang chạy trên đường").
///
/// Cách hoạt động (KHÔNG cần shader, chạy được mọi máy, không drift, không nháy):
///   - Tạo 1 pool sprite world-space (SpriteRenderer) chồng nhau theo chiều cao, phủ kín màn hình.
///   - Mỗi frame "neo" vị trí các tile theo toạ độ Y hiện tại của Camera (dựa trên camY, không cộng dồn)
///     với hệ số parallax -> nền cuộn xuống đáy màn hình mượt, mép nối liền, và
///     tự DỪNG khi game dừng (timeScale = 0) vì Camera cũng đứng im.
///
/// Sprite nền: gán trong Inspector (bgSprite) HOẶC bỏ trống để script tự lấy từ UI Image tên "bg".
/// Gắn script này lên object nào cũng được trong scene (tự tìm Camera + sprite).
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("Sprite nền")]
    [Tooltip("Sprite nền dùng để cuộn. Để TRỐNG = tự lấy sprite từ UI Image có tên 'bg' trong scene.")]
    public Sprite bgSprite;

    [Tooltip("Thứ tự vẽ: nền nên để âm (đứng sau chướng ngại vật / nhân vật)")]
    public int sortingOrder = -10;

    [Header("Parallax")]
    [Tooltip("0 = nền cuộn đúng tốc độ camera (nền gần). 1 = nền đứng yên (hiện tại). Gợi ý 0.3-0.6 cho nền xa.")]
    [Range(0f, 1f)] public float parallaxFactor = 0.5f;

    [Tooltip("Chiều cao mỗi ô nền (gấp đôi chiều cao màn hình). 1 = mỗi ô cao đúng bằng màn hình.")]
    [Range(0.5f, 3f)] public float tileHeightMultiplier = 1f;

    private Transform cam;
    private SpriteRenderer[] tiles;
    private int tileCount;
    private float tileHeight;
    private float viewW;
    private float viewH;

    private void Awake()
    {
        // 1. Tìm Camera: ưu tiên Camera trên chính object này, nếu không thì Main / bất kỳ
        Camera camComp = GetComponent<Camera>();
        if (camComp != null) cam = transform;
        else
        {
            cam = FindFirstObjectByType<Camera>() != null ? FindFirstObjectByType<Camera>().transform : null;
        }
        if (cam == null)
        {
            Debug.LogError("[BackgroundScroller] Không tìm thấy Camera. Bỏ nền cuộn.");
            enabled = false;
            return;
        }

        // 2. Lấy sprite: đã gán thì dùng, chưa thì tự pull từ UI Image "bg"
        if (bgSprite == null) bgSprite = FindBGSprite();
        if (bgSprite == null)
        {
            Debug.LogWarning("[BackgroundScroller] Chưa có sprite nền (gán 'bgSprite' hoặc thêm UI Image tên 'bg').");
            enabled = false;
            return;
        }

        BuildTiles();
    }

    // Tạo pool tile, mỗi tile cao = viewH * tileHeightMultiplier, đủ phủ + thừa 1-2 cái
    private void BuildTiles()
    {
        viewH = GetViewHeight();
        viewW = GetViewWidth();
        tileHeight = Mathf.Max(viewH * tileHeightMultiplier, 1f);

        int need = Mathf.CeilToInt(viewH / tileHeight) + 2;
        tileCount = Mathf.Max(need, 2);

        tiles = new SpriteRenderer[tileCount];
        for (int i = 0; i < tileCount; i++)
        {
            GameObject go = new GameObject($"bg_tile_{i}");
            go.transform.SetParent(null, false);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = bgSprite;
            sr.sortingOrder = sortingOrder;

            // Căng sprite phủ vừa 1 ô (cao = tileHeight, rộng phủ viewW)
            float nativeH = Mathf.Max(bgSprite.bounds.size.y, 1e-4f);
            float nativeW = Mathf.Max(bgSprite.bounds.size.x, 1e-4f);
            go.transform.localScale = new Vector3(viewW / nativeW, tileHeight / nativeH, 1f);

            tiles[i] = sr;
        }
    }

    // Neo nền theo Camera mỗi frame (drift-free): nền cuộn xuống đáy mượt, mép nối liền
    private void LateUpdate()
    {
        if (tiles == null || tiles.Length == 0 || tileHeight <= 0f) return;

        float camY = cam.position.y;
        float camX = cam.position.x;
        float camZ = cam.position.z;

        // "Nền" trôi chậm hơn camera (parallax): toạ gốc nền = camY * parallaxFactor
        float bgOrigin = camY * parallaxFactor;

        // index ô đầu tiên phải phủ đủ nửa dưới màn hình
        int iStart = Mathf.FloorToInt((camY - viewH * 0.5f - bgOrigin) / tileHeight);

        for (int k = 0; k < tileCount; k++)
        {
            float worldY = bgOrigin + (iStart + k) * tileHeight;
            tiles[k].position = new Vector3(camX, worldY, camZ - 5f); // lùi 5u về sau, đứng sau gameplay
        }
    }

    // ---------- Tiện ích ----------
    private float GetViewHeight()
    {
        Camera c = cam.GetComponent<Camera>();
        if (c == null) return 10f;
        if (c.orthographic) return c.orthographicSize * 2f;
        float dist = Mathf.Max(Mathf.Abs(c.nearClipPlane), 0.1f);
        return 2f * dist * Mathf.Tan(Mathf.Deg2Rad * c.fieldOfView * 0.5f);
    }

    private float GetViewWidth()
    {
        Camera c = cam.GetComponent<Camera>();
        if (c == null) return GetViewHeight();
        return GetViewHeight() * Mathf.Max(c.aspect, 0.1f);
    }

    // Lấy sprite từ UI Image tên "bg"; tắt các nền UI tĩnh cùng tên để nền world cuộn hiện ra
    private Sprite FindBGSprite()
    {
        Image[] imgs = FindObjectsOfType<Image>(true);
        if (imgs == null) return null;

        foreach (Image img in imgs)
        {
            if (img != null && img.name == "bg" && img.sprite != null)
            {
                Sprite found = img.sprite;
                foreach (Image other in FindObjectsOfType<Image>(true))
                {
                    if (other != null && other.name == "bg")
                        other.gameObject.SetActive(false);
                }
                return found;
            }
        }

        // Dự phòng: lấy bất kỳ Image nào có sprite (không tắt UI nào cả)
        foreach (Image img in imgs)
        {
            if (img != null && img.sprite != null)
            {
                Debug.Log($"[BackgroundScroller] Không thấy Image tên 'bg', dùng tạm Image '{img.name}'.");
                return img.sprite;
            }
        }
        return null;
    }
}
