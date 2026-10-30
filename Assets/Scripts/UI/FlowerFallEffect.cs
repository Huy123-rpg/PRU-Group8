using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace ScienceQuest.UI
{
    /// <summary>
    /// FlowerFallEffect - Hiệu ứng hoa rơi từ trên xuống cho ChapterList và LessonList.
    /// Tự động khởi tạo, KHÔNG cần gắn tay vào scene.
    ///
    /// Thứ tự ưu tiên lấy Sprite hoa:
    ///   1. Các GameObject tên "Image_hoa" đang có trong Hierarchy (ẩn chúng đi sau khi lấy sprite)
    ///   2. Inspector → trường flowerSprites (kéo thả tay)
    ///   3. Resources/Flowers/flower_01..05
    ///
    /// Phân công: M3 - UI/UX Designer / Programmer
    /// </summary>
    public class FlowerFallEffect : MonoBehaviour
    {
        // ──────────────────────────────────────────────
        //  CẤU HÌNH (Tuỳ chỉnh qua Inspector)
        // ──────────────────────────────────────────────

        [Header("=== SPRITE HOA (tuỳ chọn, ưu tiên Image_hoa trong scene) ===")]
        [Tooltip("Chỉ cần gán khi KHÔNG có GameObject tên 'Image_hoa' trong scene.")]
        public Sprite[] flowerSprites;

        [Header("=== SỐ LƯỢNG & TỐC ĐỘ ===")]
        [Tooltip("Số cánh hoa tồn tại đồng thời trên màn hình")]
        [SerializeField] private int maxFlowers = 18;
        [Tooltip("Khoảng thời gian (giây) giữa mỗi lần sinh hoa mới")]
        [SerializeField] private float spawnInterval = 0.35f;
        [Tooltip("Tốc độ rơi (pixel/giây)")]
        [SerializeField] private float fallSpeedMin = 80f;
        [SerializeField] private float fallSpeedMax = 160f;

        [Header("=== KÍCH THƯỚC HOA ===")]
        [SerializeField] private float sizeMin = 30f;
        [SerializeField] private float sizeMax = 65f;

        [Header("=== XOAY ===")]
        [Tooltip("Tốc độ xoay nhẹ (độ/giây)")]
        [SerializeField] private float rotateSpeedMin = 15f;
        [SerializeField] private float rotateSpeedMax = 55f;

        [Header("=== TRONG SUỐT ===")]
        [SerializeField] private float alphaMin = 0.6f;
        [SerializeField] private float alphaMax = 0.95f;

        // ──────────────────────────────────────────────
        //  INTERNAL STATE
        // ──────────────────────────────────────────────

        private Canvas overlayCanvas;
        private RectTransform canvasRect;
        private readonly List<FlowerPetal> activePetals = new List<FlowerPetal>();
        private readonly Queue<GameObject> petalPool = new Queue<GameObject>();
        private Sprite[] resolvedSprites;
        private bool isRunning = false;
        private float spawnTimer = 0f;

        private class FlowerPetal
        {
            public GameObject go;
            public RectTransform rt;
            public Image img;
            public float fallSpeed;
            public float rotateSpeed;
            public float rotateDir;
        }

        // ──────────────────────────────────────────────
        //  AUTO INITIALIZE
        // ──────────────────────────────────────────────

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitOnLoad()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "ChapterList" || scene.name == "LessonList")
            {
                if (FindFirstObjectByType<FlowerFallEffect>() == null)
                {
                    GameObject go = new GameObject("FlowerFallEffect_Auto");
                    go.AddComponent<FlowerFallEffect>();
                    Debug.Log($"[FlowerFallEffect] 🌸 Tự động khởi tạo hiệu ứng hoa rơi cho scene: {scene.name}");
                }
            }
        }

        // ──────────────────────────────────────────────
        //  LIFECYCLE
        // ──────────────────────────────────────────────

        private void Start()
        {
            SetupCanvas();
            ResolveSprites();
            isRunning = true;

            string currentScene = SceneManager.GetActiveScene().name;
            Debug.Log($"[FlowerFallEffect] ✅ Bắt đầu hiệu ứng hoa rơi | Scene: {currentScene} | Sprite count: {(resolvedSprites != null ? resolvedSprites.Length : 0)}");
        }

        private void Update()
        {
            if (!isRunning || overlayCanvas == null) return;

            // Spawn hoa mới nếu chưa đủ số lượng
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f && activePetals.Count < maxFlowers)
            {
                SpawnPetal();
                spawnTimer = spawnInterval;
            }

            // Di chuyển và xoay từng cánh hoa
            float screenH = canvasRect.rect.height;
            float dt = Time.deltaTime;

            for (int i = activePetals.Count - 1; i >= 0; i--)
            {
                FlowerPetal petal = activePetals[i];
                if (petal.go == null) { activePetals.RemoveAt(i); continue; }

                // Rơi xuống
                Vector2 pos = petal.rt.anchoredPosition;
                pos.y -= petal.fallSpeed * dt;
                petal.rt.anchoredPosition = pos;

                // Xoay nhẹ
                petal.rt.Rotate(0f, 0f, petal.rotateSpeed * petal.rotateDir * dt);

                // Thu hồi khi ra khỏi màn hình
                if (pos.y < -(screenH * 0.6f))
                {
                    ReturnToPool(petal);
                    activePetals.RemoveAt(i);
                }
            }
        }

        private void OnDestroy()
        {
            isRunning = false;
        }

        // ──────────────────────────────────────────────
        //  SETUP
        // ──────────────────────────────────────────────

        /// <summary>
        /// Tạo Canvas overlay riêng cho hiệu ứng hoa.
        /// Đặt sortingOrder CAO HƠN canvas chính để hoa hiện trên nền,
        /// nhưng raycastTarget=false nên không chặn click nút.
        /// </summary>
        private void SetupCanvas()
        {
            // Tìm sortingOrder CAO NHẤT của Canvas hiện có → đặt hoa nằm trên nền
            int highestOrder = 0;
            Canvas[] existing = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var c in existing)
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay && c.sortingOrder > highestOrder)
                    highestOrder = c.sortingOrder;
            }

            GameObject canvasObj = new GameObject("Canvas_FlowerEffect");
            overlayCanvas = canvasObj.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Đặt sortingOrder CAO HƠN tất cả canvas để hoa nằm trên nền scene
            overlayCanvas.sortingOrder = highestOrder + 5;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Không thêm GraphicRaycaster → hoa không chặn click
            canvasRect = canvasObj.GetComponent<RectTransform>();

            // Hủy canvas hoa khi chuyển scene
            SceneManager.sceneLoaded += (s, m) =>
            {
                if (canvasObj != null) Destroy(canvasObj);
            };

            Debug.Log($"[FlowerFallEffect] 🎨 Canvas hoa: sortingOrder = {overlayCanvas.sortingOrder}");
        }

        /// <summary>
        /// Lấy Sprite hoa theo thứ tự ưu tiên:
        ///   1. Tìm tất cả GameObject tên "Image_hoa" (kể cả inactive) → lấy sprite, ẩn chúng đi
        ///   2. Inspector → flowerSprites
        ///   3. Resources/Flowers/flower_01..flower_10
        ///
        /// Nếu Image_hoa bị "Missing (Sprite)": vẫn ẩn object và dùng fallback màu hồng
        /// để hiệu ứng vẫn hiển thị được.
        /// </summary>
        private void ResolveSprites()
        {
            // ── ƯU TIÊN 1: Tìm Image_hoa trong Hierarchy (kể cả inactive) ──
            List<Sprite> fromHierarchy = new List<Sprite>();
            int foundCount = 0;

            // FindObjectsByType không tìm inactive → dùng Resources.FindObjectsOfTypeAll
            Image[] allImages = Resources.FindObjectsOfTypeAll<Image>();
            foreach (Image img in allImages)
            {
                // Chỉ lấy object thuộc scene hiện tại
                if (img.gameObject.scene != SceneManager.GetActiveScene()) continue;

                string goName = img.gameObject.name;
                bool isFlowerObj = goName == "Image_hoa"
                                || goName.StartsWith("Image_hoa ")
                                || goName.StartsWith("Image_hoa(");

                if (!isFlowerObj) continue;

                foundCount++;

                if (img.sprite != null)
                {
                    fromHierarchy.Add(img.sprite);
                    Debug.Log($"[FlowerFallEffect] 🌸 Lấy sprite từ '{goName}': {img.sprite.name}");
                }
                else
                {
                    // Sprite bị Missing → cảnh báo nhưng vẫn tiếp tục
                    Debug.LogWarning($"[FlowerFallEffect] ⚠️ '{goName}' bị Missing Sprite. " +
                                     "Hãy chọn file ảnh hoa → Inspector → Texture Type: Sprite → Apply, " +
                                     "rồi kéo vào Image_hoa.Source Image.");
                }

                // Dù có sprite hay không, ẩn object đi (nó chỉ là kho chứa, không display trực tiếp)
                img.gameObject.SetActive(false);
            }

            if (foundCount > 0)
                Debug.Log($"[FlowerFallEffect] 🔍 Tìm thấy {foundCount} Image_hoa, lấy được {fromHierarchy.Count} sprite hợp lệ.");

            if (fromHierarchy.Count > 0)
            {
                resolvedSprites = fromHierarchy.ToArray();
                Debug.Log($"[FlowerFallEffect] ✅ Dùng {resolvedSprites.Length} sprite từ Image_hoa trong Hierarchy.");
                return;
            }

            // ── ƯU TIÊN 2: Inspector flowerSprites ──
            if (flowerSprites != null && flowerSprites.Length > 0)
            {
                List<Sprite> valid = new List<Sprite>();
                foreach (var s in flowerSprites)
                    if (s != null) valid.Add(s);
                if (valid.Count > 0)
                {
                    resolvedSprites = valid.ToArray();
                    Debug.Log($"[FlowerFallEffect] ✅ Dùng {resolvedSprites.Length} sprite từ Inspector.");
                    return;
                }
            }

            // ── ƯU TIÊN 3: Resources/Flowers/ ──
            List<Sprite> fromResources = new List<Sprite>();
            string[] resourceNames = {
                "Flowers/flower_01", "Flowers/flower_02", "Flowers/flower_03",
                "Flowers/flower_04", "Flowers/flower_05",
                "Flowers/hoa_01", "Flowers/hoa_02", "Flowers/hoa_03",
            };
            foreach (var name in resourceNames)
            {
                Sprite sp = Resources.Load<Sprite>(name);
                if (sp != null) fromResources.Add(sp);
            }
            if (fromResources.Count > 0)
            {
                resolvedSprites = fromResources.ToArray();
                Debug.Log($"[FlowerFallEffect] ✅ Dùng {resolvedSprites.Length} sprite từ Resources/Flowers/.");
                return;
            }

            // ── FALLBACK: Không có sprite → dùng null (Image sẽ vẽ hình chữ nhật màu) ──
            Debug.LogWarning("[FlowerFallEffect] ⚠️ Không tìm thấy sprite hoa hợp lệ! " +
                             "Hiệu ứng sẽ dùng hình màu thay thế. " +
                             "Hãy fix 'Missing Sprite' trên các Image_hoa trong Hierarchy.");
            resolvedSprites = null;
        }

        // ──────────────────────────────────────────────
        //  SPAWN & POOL
        // ──────────────────────────────────────────────

        private void SpawnPetal()
        {
            if (overlayCanvas == null) return;

            // Lấy từ pool hoặc tạo mới
            GameObject go = petalPool.Count > 0 ? petalPool.Dequeue() : CreatePetalObject();
            go.SetActive(true);

            RectTransform rt = go.GetComponent<RectTransform>();
            Image img = go.GetComponent<Image>();

            float screenW = canvasRect.rect.width;
            float screenH = canvasRect.rect.height;

            // Vị trí ban đầu: ngẫu nhiên ngang, phía trên màn hình
            float randomX = Random.Range(-screenW * 0.5f, screenW * 0.5f);
            float startY  = screenH * 0.5f + Random.Range(10f, 60f);
            rt.anchoredPosition = new Vector2(randomX, startY);

            // Kích thước
            float size = Random.Range(sizeMin, sizeMax);
            rt.sizeDelta = new Vector2(size, size);

            // Rotation ban đầu
            rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            // Sprite + màu
            if (resolvedSprites != null && resolvedSprites.Length > 0)
            {
                img.sprite = resolvedSprites[Random.Range(0, resolvedSprites.Length)];
                img.preserveAspect = true;
                Color c = Color.white;
                c.a = Random.Range(alphaMin, alphaMax);
                img.color = c;
            }
            else
            {
                // Fallback: không có sprite → vẽ hình màu hồng/đỏ/vàng ngẫu nhiên
                img.sprite = null;
                Color[] fallbackColors = {
                    new Color(1f,  0.4f, 0.6f, 1f),  // Hồng
                    new Color(1f,  0.2f, 0.4f, 1f),  // Đỏ hồng
                    new Color(1f,  0.8f, 0.2f, 1f),  // Vàng
                    new Color(1f,  0.6f, 0.8f, 1f),  // Hồng nhạt
                    new Color(0.9f,0.3f, 0.5f, 1f),  // Hồng đậm
                };
                Color chosen = fallbackColors[Random.Range(0, fallbackColors.Length)];
                chosen.a = Random.Range(alphaMin, alphaMax);
                img.color = chosen;
            }

            activePetals.Add(new FlowerPetal
            {
                go = go,
                rt = rt,
                img = img,
                fallSpeed  = Random.Range(fallSpeedMin, fallSpeedMax),
                rotateSpeed = Random.Range(rotateSpeedMin, rotateSpeedMax),
                rotateDir  = Random.value > 0.5f ? 1f : -1f
            });
        }

        private GameObject CreatePetalObject()
        {
            GameObject go = new GameObject("Petal");
            go.transform.SetParent(overlayCanvas.transform, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);

            Image img = go.AddComponent<Image>();
            img.raycastTarget = false; // Không chặn click
            img.color = Color.white;

            return go;
        }

        private void ReturnToPool(FlowerPetal petal)
        {
            if (petal.go == null) return;
            petal.go.SetActive(false);
            petalPool.Enqueue(petal.go);
        }

        // ──────────────────────────────────────────────
        //  PUBLIC API
        // ──────────────────────────────────────────────

        /// <summary>Dừng sinh hoa mới (hoa đang rơi vẫn tiếp tục)</summary>
        public void StopSpawning()  => isRunning = false;

        /// <summary>Tiếp tục sinh hoa</summary>
        public void ResumeSpawning() => isRunning = true;

        /// <summary>Xoá toàn bộ hoa đang hiển thị</summary>
        public void ClearAll()
        {
            foreach (var p in activePetals)
                if (p.go != null) ReturnToPool(p);
            activePetals.Clear();
        }
    }
}
