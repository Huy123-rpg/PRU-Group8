using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ScienceQuest.UI
{
    /// <summary>
    /// DuckWalkEffect - Đàn vịt đi sang phải.
    /// Riêng ở LessonList:
    ///   - Bắt đầu ngay sát mép trái màn hình (nhú ra ngay, không phải chờ lâu).
    ///   - Tốc độ nhanh hơn (80 px/s).
    ///   - Khi CON VỊT ĐẦU TIÊN chạm mép ground (Image_gr) thì RESET ngay lập tức.
    /// Ở ChapterList:
    ///   - Giữ nguyên cơ chế đi hết màn hình rồi mới reset.
    /// </summary>
    public class DuckWalkEffect : MonoBehaviour
    {
        [Header("=== TỐC ĐỘ ===")]
        [SerializeField] private float walkSpeed = 55f;

        [Header("=== NHẤP NHÔI KHI ĐI ===")]
        [SerializeField] private float bobAmplitude = 6f;
        [SerializeField] private float bobFrequency = 2.8f;

        [Header("=== TÊN OBJECT ===")]
        [SerializeField] private string duckName = "Image_vit";

        [Header("=== CẤU HÌNH CHO SCENE ===")]
        public bool isLessonList = false;

        private class DuckData
        {
            public RectTransform rt;
            public Image img;
            public Vector2 startPos;
            public bool isTopLevel;
            public float phaseOffset;
        }

        private List<DuckData> ducks = new List<DuckData>();
        private DuckData leadingDuck = null; // Con vịt dẫn đầu (ở xa nhất bên phải)
        private RectTransform groundRt = null; // Ground của LessonList (Image_gr)
        private float groupOffsetX = 0f;
        private float timer = 0f;
        private Canvas refCanvas;
        private RectTransform canvasRt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitOnLoad()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "ChapterList" || scene.name == "LessonList")
            {
                if (FindFirstObjectByType<DuckWalkEffect>() == null)
                {
                    GameObject go = new GameObject("DuckWalkEffect_Auto");
                    DuckWalkEffect effect = go.AddComponent<DuckWalkEffect>();

                    if (scene.name == "LessonList")
                    {
                        effect.isLessonList = true;
                        effect.walkSpeed = 80f; // Đi tốc độ vừa đẹp, không bị chậm
                    }
                    else
                    {
                        effect.isLessonList = false;
                        effect.walkSpeed = 55f;
                    }
                }
            }
        }

        private void Start()
        {
            refCanvas = FindFirstObjectByType<Canvas>();
            if (refCanvas != null) canvasRt = refCanvas.GetComponent<RectTransform>();

            FindDucks();

            if (ducks.Count == 0)
            {
                Destroy(gameObject);
                return;
            }

            // Tìm con vịt dẫn đầu (X lớn nhất ban đầu)
            float maxStartX = float.MinValue;
            foreach (var d in ducks)
            {
                if (d.startPos.x > maxStartX)
                {
                    maxStartX = d.startPos.x;
                    leadingDuck = d;
                }
            }

            // RIÊNG Ở LESSON LIST: Căn chỉnh vị trí xuất hiện sát góc trái & tìm Ground
            if (isLessonList && leadingDuck != null)
            {
                // Tìm ground Image_gr
                GameObject grObj = GameObject.Find("Image_gr");
                if (grObj != null)
                {
                    groundRt = grObj.GetComponent<RectTransform>();
                }
                else if (leadingDuck.rt.parent != null)
                {
                    groundRt = leadingDuck.rt.parent as RectTransform;
                }

                // Chuyển toạ độ mép trái màn hình sang toạ độ local của parent con vịt
                RectTransform parentRt = leadingDuck.rt.parent as RectTransform;
                Camera cam = (refCanvas != null && refCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? refCanvas.worldCamera : null;

                if (parentRt != null)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        parentRt,
                        new Vector2(0f, Screen.height * 0.5f),
                        cam,
                        out Vector2 screenLeftInParent
                    );

                    // Mép phải của con vịt dẫn đầu trong toạ độ local
                    float leadingDuckRightLocal = leadingDuck.startPos.x + leadingDuck.rt.rect.width * (1f - leadingDuck.rt.pivot.x);

                    // Đặt mép phải con vịt đầu tiên sát mép trái màn hình (chỉ lùi 5px)
                    // -> Vừa bấm Play là mỏ vịt đã lập tức xuất hiện, không phải chờ lâu!
                    float offset = (screenLeftInParent.x - 5f) - leadingDuckRightLocal;

                    foreach (var d in ducks)
                    {
                        if (d.isTopLevel)
                        {
                            d.startPos.x += offset;
                            d.rt.anchoredPosition = d.startPos;
                        }
                    }
                }
            }
        }

        private void Update()
        {
            if (ducks.Count == 0 || canvasRt == null) return;

            timer += Time.deltaTime;
            groupOffsetX += walkSpeed * Time.deltaTime;

            // 1. Di chuyển và nhấp nhô
            foreach (var d in ducks)
            {
                if (d.rt == null) continue;

                Vector2 pos = d.rt.anchoredPosition;

                if (d.isTopLevel)
                {
                    pos.x = d.startPos.x + groupOffsetX;
                }

                float bob = Mathf.Sin(timer * bobFrequency * Mathf.PI * 2f + d.phaseOffset) * bobAmplitude;
                pos.y = d.startPos.y + bob;

                d.rt.anchoredPosition = pos;
            }

            // 2. Kiểm tra Reset
            bool shouldReset = false;

            if (isLessonList)
            {
                // Ở LESSON LIST: Con vịt ĐẦU TIÊN chạm mép phải ground là RESET ngay!
                if (leadingDuck != null && groundRt != null)
                {
                    Vector3[] duckCorners = new Vector3[4];
                    leadingDuck.rt.GetWorldCorners(duckCorners);
                    float duckRightWorldX = duckCorners[2].x; // Mép phải con vịt dẫn đầu

                    Vector3[] groundCorners = new Vector3[4];
                    groundRt.GetWorldCorners(groundCorners);
                    float groundRightWorldX = groundCorners[2].x; // Mép phải của ground

                    if (duckRightWorldX >= groundRightWorldX - 10f)
                    {
                        shouldReset = true;
                    }
                }
            }
            else
            {
                // Ở CHAPTER LIST: Chờ toàn bộ đàn vịt đi qua hết mép phải màn hình
                Vector3[] canvasCorners = new Vector3[4];
                canvasRt.GetWorldCorners(canvasCorners);
                float canvasRightWorld = canvasCorners[2].x;

                bool allOut = true;
                foreach (var d in ducks)
                {
                    if (d.rt == null) continue;
                    Vector3[] duckCorners = new Vector3[4];
                    d.rt.GetWorldCorners(duckCorners);
                    float duckLeftWorld = duckCorners[0].x;

                    if (duckLeftWorld <= canvasRightWorld + 10f)
                    {
                        allOut = false;
                        break;
                    }
                }
                shouldReset = allOut;
            }

            // 3. Thực hiện Reset vòng lặp
            if (shouldReset)
            {
                groupOffsetX = 0f;
                timer = 0f;
                foreach (var d in ducks)
                {
                    if (d.rt != null)
                    {
                        d.rt.anchoredPosition = d.startPos;
                        if (d.img != null) d.img.enabled = true;
                    }
                }
                Debug.Log("[DuckWalkEffect] 🔁 Con vịt đầu tiên chạm mép ground -> Reset vòng lặp.");
            }
        }

        private void FindDucks()
        {
            Image[] allImages = Resources.FindObjectsOfTypeAll<Image>();
            int phaseIndex = 0;

            foreach (Image img in allImages)
            {
                if (img.gameObject.scene != SceneManager.GetActiveScene()) continue;
                if (!img.gameObject.name.StartsWith(duckName)) continue;

                RectTransform rt = img.GetComponent<RectTransform>();
                if (rt == null) continue;

                rt.gameObject.SetActive(true);

                bool isTopLevel = true;
                Transform p = rt.parent;
                while (p != null)
                {
                    if (p.name.StartsWith(duckName))
                    {
                        isTopLevel = false;
                        break;
                    }
                    p = p.parent;
                }

                ducks.Add(new DuckData
                {
                    rt = rt,
                    img = img,
                    startPos = rt.anchoredPosition,
                    isTopLevel = isTopLevel,
                    phaseOffset = phaseIndex * (Mathf.PI / 2f)
                });

                phaseIndex++;
            }
        }

        private void OnDestroy()
        {
            foreach (var d in ducks)
            {
                if (d.rt != null) d.rt.anchoredPosition = d.startPos;
                if (d.img != null) d.img.enabled = true;
            }
        }
    }
}
