using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RunnerController - Điều khiển vòng lặp "chạy vô tận" của màn Sinh (UI-based).
///
/// Thiết kế: nhân vật `nv` đứng yên dưới màn hình; các vật thể (chướng ngại vật,
/// vật phẩm hồi máu, boss) được SPAWN ở mép TRÊN đường chạy rồi DI CHUYỂN XUỐNG
/// (tọa độ RectTransform.anchoredPosition) để tạo cảm giác đang chạy về phía trước.
/// Nền (bg) cũng cuộn xuống cùng tốc độ (gọi UIBackgroundScroller qua sự kiện riêng).
///
/// CHẠY DỪNG khi: quiz mở (RunnerController.Instance.SetPaused(true) do QuizManager gọi),
///               game over, hoặc pause panel.
/// CHẠY TIẾP khi: đóng quiz (boss bị hạ), resume.
///
/// Gắn script này lên 1 object con của Canvas (tên gợi ý: "Runner").
/// </summary>
public class RunnerController : MonoBehaviour
{
    public static RunnerController Instance;

    [Header("Prefab vật thể (gán trong Inspector)")]
    [Tooltip("Danh sách prefab chướng ngại vật (cùng cấu trúc BoxCollider2D solid + tag Obstacle + EntityMover).")]
    public GameObject[] obstaclePrefabs;
    [Tooltip("Prefab vật phẩm hồi máu (CircleCollider2D solid + tag Heart + EntityMover).")]
    public GameObject healthPickupPrefab;
    [Tooltip("Danh sách prefab boss (giữ Animator + BossTrigger).")]
    public GameObject[] bossPrefabs;

    [Header("Vùng spawn")]
    [Tooltip("Object 'Spawns' (RectTransform rỗng, stretch full) - parent của mọi vật thể spawn.")]
    public Transform spawnRoot;
    [Tooltip("X trái nhất của đường chạy (pixels so với giữa).")]
    public float runPathMinX = -160f;
    [Tooltip("X phải nhất của đường chạy (pixels so với giữa).")]
    public float runPathMaxX = 160f;
    [Tooltip("Y (anchored) mà vật thể spawn - ở ngoài mép trên màn hình.")]
    public float spawnTopY = 620f;
    [Tooltip("Y (anchored) mà vật thể bị hủy - dưới đáy màn hình.")]
    public float despawnBottomY = -620f;

    [Header("Tốc độ & nhịp spawn")]
    [Tooltip("Tốc độ di chuyển của tất cả vật thể (pixels/giây). Nền cũng dùng số này.")]
    public float worldSpeed = 120f;
    [Tooltip("Khoảng cách (giây) giữa 2 đợt spawn chướng ngại vật.")]
    public float obstacleSpawnInterval = 1.4f;
    [Tooltip("Mỗi N pixel đường chạy sẽ spawn 1 boss (khoảng 10 giây chạy).")]
    public float bossDistanceInterval = 1200f;

    [Header("Tỷ lệ vật phẩm hồi máu (đúng 7 chướng ngại vật / 1 hồi máu)")]
    [Tooltip("Mỗi N chướng ngại vật sẽ sinh 1 lượt hồi máu (yêu cầu: đúng 7:1).")]
    public int obstaclesPerHeart = 7;

    [Header("Khoảng né (lane)")]
    [Tooltip("Số 'hàng dọc' (lanes) chia đều đường chạy. Luôn để trống >=1 lane để né được.")]
    public int laneCount = 3;
    [Tooltip("Tối đa chướng ngại vật mỗi đợt (phải < laneCount để luôn còn khoảng trống).")]
    public int maxObstaclesPerWave = 2;

    // ============ Trạng thái runtime ============
    private readonly List<GameObject> activeEntities = new List<GameObject>();
    public IReadOnlyList<GameObject> ActiveEntities => activeEntities;
    private readonly HashSet<int> lanesBlockedThisWave = new HashSet<int>();
    private float timeSinceObstacleWave = 0f;
    private int obstaclesSpawned = 0;
    private float distanceSinceBoss = 0f;
    private int currentBossSegment = 0;
    private bool bossSpawnedThisSegment = false;

    private bool isRunning = true;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Bắt đầu là đang chạy (game chưa mở quiz)
        isRunning = true;
    }

    private void Update()
    {
        if (!isRunning) return;
        if (spawnRoot == null) spawnRoot = transform; // dự phòng

        float dt = Time.deltaTime; // dừng khi Time.timeScale=0 (quiz mở)

        // 1. Chạy bộ đếm quãng đường cho boss (float chính xác, không ép kiểu int làm mất giá trị ở FPS cao)
        distanceSinceBoss += worldSpeed * dt;
        if (distanceSinceBoss >= bossDistanceInterval && !bossSpawnedThisSegment)
        {
            distanceSinceBoss = 0f;
            currentBossSegment++;
            SpawnBoss(); // set bossSpawnedThisSegment = true bên trong
        }

        // 2. Spawn đợt chướng ngại vật theo nhịp
        timeSinceObstacleWave += dt;
        if (timeSinceObstacleWave >= obstacleSpawnInterval)
        {
            timeSinceObstacleWave = 0f;
            SpawnObstacleWave();
        }
    }

    // ============ SPAWN ============

    /// <summary>
    /// Sinh 1 đợt chướng ngại vật (1 cái), luôn để lại >=1 lane trống (không chặn kín đường).
    /// Đồng thời kiểm tra tỷ lệ sinh hồi máu (đúng 7 chướng ngại vật / 1 bình máu).
    /// </summary>
    private void SpawnObstacleWave()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        int laneW = (int)((runPathMaxX - runPathMinX) / Mathf.Max(laneCount, 1));

        // Chọn random 1 lane để đặt chướng ngại vật (chừa trống 2 lane còn lại)
        int[] freeLanes = new int[laneCount];
        for (int i = 0; i < laneCount; i++) freeLanes[i] = i;
        Shuffle(freeLanes);

        int chosenLane = freeLanes[0];
        float x = runPathMinX + laneW * (chosenLane + 0.5f); // giữa lane
        x += Random.Range(-laneW * 0.2f, laneW * 0.2f); // lệch nhẹ trong lane để ngẫu nhiên tự nhiên

        if (obstaclePrefabs.Length > 0)
        {
            int pi = Random.Range(0, obstaclePrefabs.Length);
            var obs = Instantiate(obstaclePrefabs[pi], spawnRoot, false);
            obs.SetActive(true); // BẬT ACTIVE để clone hiển thị và chạy script
            SetAnchoredPosition(obs, x, spawnTopY);
            EntityMover mover = obs.GetComponent<EntityMover>();
            if (mover != null)
            {
                mover.kind = EntityMover.Kind.Obstacle;
                mover.speed = worldSpeed;
            }
            activeEntities.Add(obs);
        }

        obstaclesSpawned++;

        // Tỷ lệ hồi máu: đúng 7 chướng ngại vật thì sinh 1 bình hồi máu (tỷ lệ 7:1)
        if (obstaclesPerHeart > 0 && obstaclesSpawned % obstaclesPerHeart == 0)
        {
            if (healthPickupPrefab != null)
            {
                // Chọn 1 lane TRỐNG (không cùng lane với chướng ngại vật vừa spawn)
                int heartLane = -1;
                foreach (int l in freeLanes) { if (l != chosenLane) { heartLane = l; break; } }
                if (heartLane < 0) heartLane = (chosenLane + 1) % laneCount;

                float hx = runPathMinX + laneW * (heartLane + 0.5f);
                var heart = Instantiate(healthPickupPrefab, spawnRoot, false);
                heart.SetActive(true); // BẬT ACTIVE
                SetAnchoredPosition(heart, hx, spawnTopY);
                EntityMover hm = heart.GetComponent<EntityMover>();
                if (hm != null)
                {
                    hm.kind = EntityMover.Kind.HealthPickup;
                    hm.speed = worldSpeed;
                }
                activeEntities.Add(heart);
                Debug.Log($"[RunnerController] 💖 Sinh bình hồi máu (chướng ngại vật #{obstaclesSpawned}, tỷ lệ {obstaclesPerHeart}:1)");
            }
        }
    }

    private void SpawnBoss()
    {
        bossSpawnedThisSegment = true; // đánh dấu (chống spawn trùng segment)
        if (bossPrefabs == null || bossPrefabs.Length == 0) return;

        int bi = Random.Range(0, bossPrefabs.Length);
        GameObject boss = Instantiate(bossPrefabs[bi], spawnRoot, false);
        boss.SetActive(true); // BẬT ACTIVE
        SetAnchoredPosition(boss, 0f, spawnTopY); // spawn giữa, đầu trên
        EntityMover bm = boss.GetComponent<EntityMover>();
        if (bm != null)
        {
            bm.kind = EntityMover.Kind.Boss;
            bm.speed = worldSpeed;
            bm.triggerY = -100f; // tới gần giữa màn hình thì kích hoạt quiz
        }
        activeEntities.Add(boss);
        Debug.Log($"[RunnerController] 👑 ĐÃ XUẤT HIỆN BOSS #{currentBossSegment}!");
    }

    // ============ DI CHUYỂN VẬT THỂ ============

    /// <summary>
    /// Di chuyển tất cả vật thể con của spawnRoot xuống theo worldSpeed, hủy khi đi ra ngoài đáy.
    /// (Mọi vật thể spawn đều phải có EntityMover; phương pháp này dùng để đồng bộ, tránh vật thể đứng yên)
    /// </summary>
    public void MoveActiveEntities()
    {
        if (!isRunning) return;
        float dt = Time.deltaTime;

        for (int i = activeEntities.Count - 1; i >= 0; i--)
        {
            GameObject e = activeEntities[i];
            if (e == null) { activeEntities.RemoveAt(i); continue; }

            RectTransform rt = e.transform as RectTransform;
            if (rt == null) continue;

            Vector2 p = rt.anchoredPosition;
            p.y -= worldSpeed * dt;
            rt.anchoredPosition = p;

            if (p.y < despawnBottomY)
            {
                activeEntities.RemoveAt(i);
                Destroy(e); // dọn dẹp vật thể đã đi qua (hạn chế tăng bộ nhớ)
            }
        }
    }

    // ============ PAUSE / RESUME / RESET ============

    public void SetRunning(bool running)
    {
        isRunning = running;
    }

    public bool IsRunning => isRunning;

    /// <summary>Gọi sau khi hạ gục Boss để kích hoạt đếm đường chạy cho Boss tiếp theo (vòng lặp vô tận).</summary>
    public void NotifyBossDefeated()
    {
        bossSpawnedThisSegment = false;
        distanceSinceBoss = 0f;
        isRunning = true;
    }

    /// <summary>Gọi khi chơi lại (reset toàn bộ runner).</summary>
    public void ResetForNewRun()
    {
        // Hủy mọi vật thể đang tồn tại
        foreach (GameObject e in activeEntities) if (e != null) Destroy(e);
        activeEntities.Clear();
        obstaclesSpawned = 0;
        distanceSinceBoss = 0f;
        currentBossSegment = 0;
        bossSpawnedThisSegment = false;
        timeSinceObstacleWave = 0f;
        isRunning = true;
    }

    // ============ Tiện ích ============

    private static void SetAnchoredPosition(GameObject go, float x, float y)
    {
        RectTransform rt = go.transform as RectTransform;
        if (rt != null)
        {
            // neo về giữa Canvas (anchor 0.5,0.5) để X/Y = pixel so với giữa màn hình
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
        }
    }

    private static void Shuffle(int[] a)
    {
        for (int i = a.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (a[i], a[j]) = (a[j], a[i]);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
