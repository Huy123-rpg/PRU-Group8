using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// RunnerWiring - GẮN component + cấu hình scene tự động lúc runtime (KHÔNG cần sửa YAML Unity).
///
/// Gắn script này lên object "Canvas" (cùng nơi QuizManager đã gán). Tự động:
///   1. Tìm object "nv", gắn PlayerMovementUI (nếu chưa có) + chỉnh collider thành trigger.
///   2. Tạo object "Runner" (nếu chưa có) + gắn RunnerController + cấu hình.
///   3. Tìm/tạo object "RunnerLayer" ( RectTransform, parent của vật thể spawn ).
///   4. Tạo 2 UI Image "bg" nối tiếp nhau + gắn BackgroundScroller (UI-based).
///   5. Xóa các instance "chuongngaivat"/"hoimau"/"boss" cũ đặt sẵn (để runner spawn mới).
///
/// Chạy TRONG scene SinhScene, lúc Awake (trước QuizManager.Start) để mọi thứ được sẵn sàng.
/// </summary>
public class RunnerWiring : MonoBehaviour
{
    [Header("Tần suất & tốc độ (px/giây)")]
    [Tooltip("Tốc độ di chuyển vật thể + nền (px/giây).")]
    public float worldSpeed = 120f;
    [Tooltip("Khoảng cách (giây) giữa 2 đợt spawn chướng ngại vật.")]
    public float obstacleSpawnInterval = 1.4f;
    [Tooltip("Mỗi N pixel đường chạy spawn 1 boss.")]
    public float bossDistanceInterval = 1400f;
    [Tooltip("Giới hạn X trái/phải (px) - khớp với PlayerMovementUI.")]
    public float runPathMinX = -160f;
    public float runPathMaxX = 160f;

    [Header("Tỷ lệ hồi máu")]
    public int obstaclesPerHeart = 10;
    [Range(0f, 1f)] public float healthPickupChancePerGroup = 0.5f;

    [Header("Danh sách prefab (gán vào trong Inspector)")]
    [Tooltip("Prefab obstacle (1-4 cái, tag Obstacle + BoxCollider2D trigger + EntityMover). Để trống = script tự tạo 1 prefab đơn giản (màu dự phòng).")]
    public GameObject[] obstaclePrefabs;
    [Tooltip("Prefab hồi máu (tag Heart + CircleCollider2D trigger + EntityMover). Để trống = tự tạo.")]
    public GameObject healthPickupPrefab;
    [Tooltip("Prefab boss (6 cái, giữ Animator + EntityMover). Để trống = tự tạo.")]
    public GameObject[] bossPrefabs;

    [Header("Tùy chọn tự dọn dẹp")]
    [Tooltip("Bỏ tick nếu muốn giữ instance chướng ngại vật / hồi máu / boss cũ đặt sẵn trong scene.")]
    public bool removeOldInstances = true;

    void Awake()
    {
        WireEverything();
    }

    void WireEverything()
    {
        Transform canvasT = transform;

        // 1. nv: gắn PlayerMovementUI + collider trigger
        GameObject nv = FindChildByName(canvasT, "nv");
        if (nv != null)
        {
            PlayerMovementUI pm = nv.GetComponent<PlayerMovementUI>();
            if (pm == null) pm = nv.AddComponent<PlayerMovementUI>();
            pm.minX = runPathMinX;
            pm.maxX = runPathMaxX;
            pm.moveSpeed = 260f;

            BoxCollider2D c = nv.GetComponent<BoxCollider2D>();
            if (c != null) c.isTrigger = true;
        }

        // 2. Tạo "RunnerLayer" (RectTransform con Canvas) làm parent chung cho vật thể spawn
        GameObject runnerLayer = FindChildByName(canvasT, "RunnerLayer")?.gameObject;
        if (runnerLayer == null)
        {
            runnerLayer = new GameObject("RunnerLayer");
            RectTransform rt = runnerLayer.AddComponent<RectTransform>();
            rt.SetParent(canvasT, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        // 3. Tạo "Runner" + RunnerController
        GameObject runner = FindChildByName(canvasT, "Runner")?.gameObject;
        if (runner == null)
        {
            runner = new GameObject("Runner");
            runner.transform.SetParent(canvasT, false);
        }
        RunnerController rc = runner.GetComponent<RunnerController>();
        if (rc == null) rc = runner.AddComponent<RunnerController>();
        rc.spawnRoot = runnerLayer.transform;
        rc.worldSpeed = worldSpeed;
        rc.obstacleSpawnInterval = obstacleSpawnInterval;
        rc.bossDistanceInterval = bossDistanceInterval;
        rc.runPathMinX = runPathMinX;
        rc.runPathMaxX = runPathMaxX;
        rc.obstaclesPerHeart = obstaclesPerHeart;
        rc.healthPickupChancePerGroup = healthPickupChancePerGroup;

        // 4. Prefab: dùng đã gán nếu có, không thì lấy con object "tắt" trong scene (do ta sẽ tạo trước đó)
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            GameObject o = EnsureInactiveTemplate(canvasT, "ObstacleTemplate", CreateObstacleTemplate);
            rc.obstaclePrefabs = new[] { o };
        }
        else rc.obstaclePrefabs = obstaclePrefabs;

        if (healthPickupPrefab == null)
        {
            healthPickupPrefab = EnsureInactiveTemplate(canvasT, "HealthPickupTemplate", CreateHealthPickupTemplate);
        }
        rc.healthPickupPrefab = healthPickupPrefab;

        if (bossPrefabs == null || bossPrefabs.Length == 0)
        {
            GameObject b = EnsureInactiveTemplate(canvasT, "BossTemplate", CreateBossTemplate);
            rc.bossPrefabs = new[] { b };
        }
        else rc.bossPrefabs = bossPrefabs;

        // 5. Nền (2 image + BackgroundScroller UI)
        WireBackground(canvasT);

        // 6. Xóa instance chướng ngại / hồi máu / boss CŨ đặt sẵn (runner sẽ spawn mới)
        if (removeOldInstances) RemoveOldInstances(canvasT);
    }

    // Lấy (hoặc tạo mới) một object "mẫu" TẮT trong scene, dùng làm "prefab" tạm thời
    // (Instantiate sẽ tạo copy active mới, mẫu gốc vẫn nằm im trong scene)
    GameObject EnsureInactiveTemplate(Transform canvasT, string name, System.Func<Transform, GameObject> factory)
    {
        GameObject existing = FindChildByName(canvasT, name)?.gameObject;
        if (existing != null) return existing;
        GameObject go = factory(canvasT);
        go.name = name;
        go.SetActive(false);
        return go;
    }

    // ============ NỀN (2 UI Image + BackgroundScroller) ============
    void WireBackground(Transform canvasT)
    {
        // Lấy sprite "bg" hiện có (object "bg" đầu tiên con Canvas)
        Sprite bgSprite = null;
        GameObject existingBg = FindChildByName(canvasT, "bg")?.gameObject;
        if (existingBg != null)
        {
            Image img = existingBg.GetComponent<Image>();
            if (img != null) bgSprite = img.sprite;
        }

        // Tạo 1 object "BgScroll" (RectTransform, full canvas) làm parent của 2 image
        GameObject bgScroll = FindChildByName(canvasT, "BgScroll")?.gameObject;
        if (bgScroll == null)
        {
            bgScroll = new GameObject("BgScroll");
            RectTransform rt = bgScroll.AddComponent<RectTransform>();
            rt.SetParent(canvasT, false);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            bgScroll.transform.SetAsFirstSibling(); // đứng sau UI khác
        }

        // Tạo 2 image (dùng sprite cùng 1 file để nối liền, không đường cắt)
        GameObject imgA = FindChildByName(bgScroll.transform, "bgA")?.gameObject;
        if (imgA == null) imgA = CreateBgImage(bgScroll.transform, "bgA", bgSprite);
        GameObject imgB = FindChildByName(bgScroll.transform, "bgB")?.gameObject;
        if (imgB == null) imgB = CreateBgImage(bgScroll.transform, "bgB", bgSprite);

        BackgroundScroller scroller = bgScroll.GetComponent<BackgroundScroller>();
        if (scroller == null) scroller = bgScroll.AddComponent<BackgroundScroller>();
        scroller.bgImageA = imgA.transform as RectTransform;
        scroller.bgImageB = imgB.transform as RectTransform;
        scroller.scrollSpeed = worldSpeed; // cùng tốc độ với runner

        // Xóa image "bg" cũ (đơn lẻ, con Canvas) nếu có - nền mới sẽ là 2 ảnh loop
        if (existingBg != null && existingBg.transform.parent == canvasT)
        {
            Destroy(existingBg);
        }
    }

    GameObject CreateBgImage(Transform parent, string name, Sprite sprite)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = Vector2.zero; // stretch ngang canvas

        Image img = go.AddComponent<Image>();
        if (sprite != null) img.sprite = sprite;
        else img.color = new Color(0.55f, 0.45f, 0.3f, 1f); // dự phòng (nâu đất)
        img.preserveAspect = false;
        img.raycastTarget = false;
        return go;
    }

    // ============ TẠO TEMPLATE (object tắt, dùng làm "prefab tạm") ============

    GameObject CreateObstacleTemplate(Transform canvasT)
    {
        GameObject go = new GameObject();
        go.tag = "Obstacle";
        go.transform.SetParent(canvasT, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80f, 100f);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.4f, 0.2f, 0.1f, 1f); // dự phòng
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(80f, 100f);
        EntityMover m = go.AddComponent<EntityMover>();
        m.kind = EntityMover.Kind.Obstacle;
        return go;
    }

    GameObject CreateHealthPickupTemplate(Transform canvasT)
    {
        GameObject go = new GameObject();
        go.tag = "Heart";
        go.transform.SetParent(canvasT, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50f, 50f);
        Image img = go.AddComponent<Image>();
        img.color = new Color(1f, 0.3f, 0.3f, 1f); // đỏ tim
        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 25f;
        EntityMover m = go.AddComponent<EntityMover>();
        m.kind = EntityMover.Kind.HealthPickup;
        return go;
    }

    GameObject CreateBossTemplate(Transform canvasT)
    {
        GameObject go = new GameObject();
        go.transform.SetParent(canvasT, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(180f, 180f);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.3f, 0.7f, 0.3f, 1f); // xanh lá (boss)
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(180f, 180f);
        EntityMover m = go.AddComponent<EntityMover>();
        m.kind = EntityMover.Kind.Boss;
        m.triggerY = -100f;
        return go;
    }

    // ============ XÓA INSTANCE CŨ ============
    void RemoveOldInstances(Transform canvasT)
    {
        string[] namesToRemove =
        {
            "chuongngaivat", "chuongngaivat (1)", "chuongngaivat (2)", "chuongngaivat (3)",
            "chuongngaivat (4)", "chuongngaivat (5)", "chuongngaivat (6)",
            "hoimau",
            "boss", "boss (2)", "boss (4)", "boss (6)", "boss (8)", "boss (10)"
        };
        foreach (string n in namesToRemove)
        {
            Transform found = FindChildByName(canvasT, n);
            if (found != null) Destroy(found.gameObject);
        }
    }

    // ============ HELPERS ============
    Transform FindChildByName(Transform root, string name)
    {
        // Duyệt con + tổ con (depth-first), KHÔNG dùng FindObjectsByType (nặng + tìm cả scene)
        if (root == null) return null;
        foreach (Transform c in root)
        {
            if (c.name == name) return c;
        }
        foreach (Transform c in root)
        {
            Transform r = FindChildByName(c, name);
            if (r != null) return r;
        }
        return null;
    }
}
