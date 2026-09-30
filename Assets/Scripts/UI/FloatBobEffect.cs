using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ScienceQuest.UI
{
    /// <summary>
    /// FloatBobEffect - Hiệu ứng nhấp nhô lên xuống (bobbing) cho các UI Image.
    /// Tự động tìm Image_sun và Image_cloud trong scene ChapterList và LessonList.
    ///
    /// Cơ chế: dùng Mathf.Sin() để tạo chuyển động nhấp nhô mượt mà.
    /// Mỗi object có phase offset khác nhau → không nhấp nhô cùng lúc, trông tự nhiên hơn.
    ///
    /// Phân công: M3 - UI/UX Designer / Programmer
    /// </summary>
    public class FloatBobEffect : MonoBehaviour
    {
        // ──────────────────────────────────────────────
        //  CẤU HÌNH (Tuỳ chỉnh qua Inspector)
        // ──────────────────────────────────────────────

        [Header("=== BIÊN ĐỘ NHẤP NHÔI ===")]
        [Tooltip("Di chuyển lên/xuống tối đa bao nhiêu pixel")]
        [SerializeField] private float amplitude = 12f;

        [Header("=== TỐC ĐỘ ===")]
        [Tooltip("Tốc độ nhấp nhô (chu kỳ/giây). Lớn hơn = nhanh hơn)")]
        [SerializeField] private float speedSun   = 0.8f;
        [SerializeField] private float speedCloud = 0.6f;

        [Tooltip("Tốc độ xoay mặt trời (độ/giây). Để 0 = không xoay)")]
        [SerializeField] private float sunRotateSpeed = 0f;

        [Header("=== TÊN OBJECT CẦN TÌM ===")]
        [SerializeField] private string sunName   = "Image_sun";
        [SerializeField] private string cloudName = "Image_cloud";

        // ──────────────────────────────────────────────
        //  INTERNAL STATE
        // ──────────────────────────────────────────────

        private class BobTarget
        {
            public RectTransform rt;
            public Vector2 originalPos;   // Vị trí gốc (anchoredPosition)
            public float speed;
            public float phaseOffset;     // Phase lệch nhau để không cùng nhô lên
            public float rotateSpeed;     // 0 = không xoay
        }

        private readonly List<BobTarget> targets = new List<BobTarget>();
        private float timer = 0f;

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
                if (FindFirstObjectByType<FloatBobEffect>() == null)
                {
                    GameObject go = new GameObject("FloatBobEffect_Auto");
                    go.AddComponent<FloatBobEffect>();
                    Debug.Log($"[FloatBobEffect] ☀️ Tự động khởi tạo hiệu ứng nhấp nhô cho scene: {scene.name}");
                }
            }
        }

        // ──────────────────────────────────────────────
        //  LIFECYCLE
        // ──────────────────────────────────────────────

        private void Start()
        {
            // Tìm tất cả Image trong scene (kể cả inactive)
            Image[] allImages = Resources.FindObjectsOfTypeAll<Image>();

            foreach (Image img in allImages)
            {
                if (img.gameObject.scene != SceneManager.GetActiveScene()) continue;

                string name = img.gameObject.name;
                RectTransform rt = img.GetComponent<RectTransform>();
                if (rt == null) continue;

                if (name == sunName)
                {
                    targets.Add(new BobTarget
                    {
                        rt          = rt,
                        originalPos = rt.anchoredPosition,
                        speed       = speedSun,
                        phaseOffset = 0f,             // Mặt trời bắt đầu từ phase 0
                        rotateSpeed = sunRotateSpeed
                    });
                    Debug.Log($"[FloatBobEffect] ☀️ Đã gắn nhấp nhô vào: {name}");
                }
                else if (name == cloudName)
                {
                    targets.Add(new BobTarget
                    {
                        rt          = rt,
                        originalPos = rt.anchoredPosition,
                        speed       = speedCloud,
                        phaseOffset = Mathf.PI * 0.7f, // Đám mây lệch pha → không cùng nhô với mặt trời
                        rotateSpeed = 0f               // Đám mây không xoay
                    });
                    Debug.Log($"[FloatBobEffect] ☁️ Đã gắn nhấp nhô vào: {name}");
                }
            }

            if (targets.Count == 0)
            {
                Debug.LogWarning($"[FloatBobEffect] ⚠️ Không tìm thấy '{sunName}' hay '{cloudName}' trong scene. " +
                                 "Kiểm tra lại tên GameObject trong Hierarchy.");
            }
        }

        private void Update()
        {
            if (targets.Count == 0) return;

            timer += Time.deltaTime;

            foreach (BobTarget t in targets)
            {
                if (t.rt == null) continue;

                // Nhấp nhô lên xuống bằng sóng sin
                float offsetY = Mathf.Sin(timer * t.speed * Mathf.PI * 2f + t.phaseOffset) * amplitude;
                t.rt.anchoredPosition = new Vector2(t.originalPos.x, t.originalPos.y + offsetY);

                // Xoay mặt trời chậm (nếu có)
                if (t.rotateSpeed != 0f)
                {
                    t.rt.Rotate(0f, 0f, -t.rotateSpeed * Time.deltaTime);
                }
            }
        }

        private void OnDestroy()
        {
            // Restore vị trí gốc khi script bị hủy
            foreach (BobTarget t in targets)
            {
                if (t.rt != null)
                    t.rt.anchoredPosition = t.originalPos;
            }
        }
    }
}
