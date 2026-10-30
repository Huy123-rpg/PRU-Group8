using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ScienceQuest.UI
{
    /// <summary>
    /// CharacterAnimationEffect - Tự động kích hoạt hiệu ứng cử động (Sprite Animation & Idle Bobbing)
    /// cho nhân vật cha2 (và các nhân vật khác) trên cả màn CharacterSelection lẫn Physics.
    /// 
    /// Cơ chế:
    /// 1. Tự động tìm GameObject cha2 (hoặc cha1, cha3, cha4) khi load scene.
    /// 2. Gắn component UICharacterAnimator:
    ///    - Frame Animation: Chạy tuần tự các frame sprite (cheer0, cheer1, behindBack 2)
    ///    - Idle Breathing: Nhấp nhô thở nhẹ nhàng bằng sine-wave (tự nhiên như game 2D)
    /// 3. Hỗ trợ cả Animator Controller nếu có.
    /// </summary>
    public class CharacterAnimationEffect : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitOnLoad()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "CharacterSelection" || scene.name == "Physics")
            {
                if (FindFirstObjectByType<CharacterAnimationEffect>() == null)
                {
                    GameObject go = new GameObject("CharacterAnimationEffect_Auto");
                    go.AddComponent<CharacterAnimationEffect>();
                    Debug.Log($"[CharacterAnimationEffect] 🎭 Khởi tạo quản lý cử động nhân vật cho scene: {scene.name}");
                }
            }
        }

        private void Start()
        {
            SetupCharacterAnimations();
        }

        /// <summary>
        /// Quét tất cả GameObject nhân vật trong scene và gắn component cử động.
        /// </summary>
        public void SetupCharacterAnimations()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            bool isPhysicsScene = sceneName == "Physics";

            // Tìm tất cả GameObject trong scene (kể cả đang bị ẩn/inactive)
            Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();

            foreach (Transform t in allTransforms)
            {
                if (t.gameObject.scene != SceneManager.GetActiveScene()) continue;

                string objName = t.gameObject.name.ToLower();

                // Kiểm tra xem có phải là nhân vật cha1, cha2, cha3, cha4 không
                if (objName.StartsWith("cha") && objName.Length >= 4 && char.IsDigit(objName[3]))
                {
                    Image img = t.GetComponent<Image>();
                    if (img == null) continue;

                    // Gắn UICharacterAnimator nếu chưa có
                    UICharacterAnimator animator = t.GetComponent<UICharacterAnimator>();
                    if (animator == null)
                    {
                        animator = t.gameObject.AddComponent<UICharacterAnimator>();
                    }

                    // Cấu hình riêng cho cha2 (đã có bộ sprite animation hoàn chỉnh)
                    if (objName == "cha2")
                    {
                        ConfigureCha2(animator, isPhysicsScene);
                    }
                    else
                    {
                        // Các nhân vật cha1, cha3, cha4: kích hoạt hiệu ứng thở nhẹ (breathing) trong khi chờ thêm frame
                        animator.enableBreathing = true;
                        animator.breathingAmplitude = 3.5f;
                    }
                }
            }
        }

        /// <summary>
        /// Nạp các frame chuyển động cho cha2 dựa theo ngữ cảnh màn hình.
        /// </summary>
        private void ConfigureCha2(UICharacterAnimator animator, bool isPhysicsScene)
        {
            List<Sprite> frames = new List<Sprite>();

            // Lấy toàn bộ sprite đã load trong bộ nhớ
            Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();

#if UNITY_EDITOR
            // Trong Unity Editor: Nạp trực tiếp từ thư mục Art/Characters để đảm bảo 100% đầy đủ frame
            if (isPhysicsScene)
            {
                // Màn Physics (nhìn từ sau lưng bắn vịt): dùng các frame nhìn từ sau
                AddSpriteIfFound(frames, "Assets/Art/Characters/character_maleAdventurer_back.png");
                AddSpriteIfFound(frames, "Assets/Art/Characters/character_maleAdventurer_behindBack 1.png");
                AddSpriteIfFound(frames, "Assets/Art/Characters/character_maleAdventurer_behindBack 2.png");
            }
            else
            {
                // Màn CharacterSelection (nhìn chính diện): dùng các frame vẫy tay/cổ vũ (như trong cha2.anim)
                AddSpriteIfFound(frames, "Assets/Art/Characters/character_maleAdventurer_cheer0.png");
                AddSpriteIfFound(frames, "Assets/Art/Characters/character_maleAdventurer_cheer1.png");
                AddSpriteIfFound(frames, "Assets/Art/Characters/character_maleAdventurer_behindBack 2.png");
            }
#endif

            // Fallback nếu chạy ở môi trường không có AssetDatabase: tìm theo tên sprite trong bộ nhớ
            if (frames.Count == 0)
            {
                string[] targetNames = isPhysicsScene
                    ? new[] { "character_maleAdventurer_back", "character_maleAdventurer_behindBack 1", "character_maleAdventurer_behindBack 2" }
                    : new[] { "character_maleAdventurer_cheer0", "character_maleAdventurer_cheer1", "character_maleAdventurer_behindBack 2" };

                foreach (string tName in targetNames)
                {
                    foreach (Sprite sp in allSprites)
                    {
                        if (sp.name.Equals(tName, System.StringComparison.OrdinalIgnoreCase))
                        {
                            if (!frames.Contains(sp)) frames.Add(sp);
                            break;
                        }
                    }
                }
            }

            // Gán frames vào animator
            animator.animationFrames = frames.ToArray();
            animator.frameRate = 6f; // 6 FPS tạo cử động rõ ràng, mượt mà và dễ nhìn
            animator.enableBreathing = true;
            animator.breathingAmplitude = 4f;

#if UNITY_EDITOR
            // Đồng thời gán RuntimeAnimatorController nếu scene có dùng Animator
            RuntimeAnimatorController controller = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Characters/cha2.controller");
            if (controller != null)
            {
                Animator unityAnim = animator.GetComponent<Animator>();
                if (unityAnim == null) unityAnim = animator.gameObject.AddComponent<Animator>();
                unityAnim.runtimeAnimatorController = controller;
            }
#endif

            Debug.Log($"[CharacterAnimationEffect] ✅ Đã kích hoạt cử động cho cha2 ({frames.Count} frames, scene={SceneManager.GetActiveScene().name})");
        }

#if UNITY_EDITOR
        private void AddSpriteIfFound(List<Sprite> list, string path)
        {
            Sprite sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sp != null && !list.Contains(sp))
            {
                list.Add(sp);
            }
        }
#endif
    }

    /// <summary>
    /// Component xử lý chuyển động sprite và hiệu ứng thở (idle breathing) trên UI Image.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UICharacterAnimator : MonoBehaviour
    {
        [Header("=== FRAME ANIMATION ===")]
        public Sprite[] animationFrames = new Sprite[0];
        [Tooltip("Số frame trên giây (FPS)")]
        public float frameRate = 6f;

        [Header("=== BREATHING / BOBBING ===")]
        public bool enableBreathing = true;
        [Tooltip("Biên độ nhấp nhô lên xuống (pixels)")]
        public float breathingAmplitude = 4f;
        [Tooltip("Tốc độ thở")]
        public float breathingSpeed = 3.5f;

        private Image targetImage;
        private RectTransform rectTransform;
        private Vector2 baseAnchoredPosition;
        private Vector3 baseScale;

        private int currentFrameIndex = 0;
        private float frameTimer = 0f;
        private float breathingTimer = 0f;
        private bool isInitialized = false;

        private void Awake()
        {
            targetImage = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                baseAnchoredPosition = rectTransform.anchoredPosition;
                baseScale = rectTransform.localScale;
            }
            isInitialized = true;
        }

        private void OnEnable()
        {
            if (!isInitialized && rectTransform != null)
            {
                baseAnchoredPosition = rectTransform.anchoredPosition;
                baseScale = rectTransform.localScale;
            }
            currentFrameIndex = 0;
            frameTimer = 0f;
            breathingTimer = 0f;
        }

        private void OnDisable()
        {
            // Trả lại vị trí và scale ban đầu khi bị ẩn
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = baseAnchoredPosition;
                rectTransform.localScale = baseScale;
            }
        }

        private void Update()
        {
            // 1. Cập nhật Frame Animation
            if (animationFrames != null && animationFrames.Length > 1 && targetImage != null)
            {
                frameTimer += Time.deltaTime;
                float interval = 1f / Mathf.Max(frameRate, 1f);

                if (frameTimer >= interval)
                {
                    frameTimer -= interval;
                    currentFrameIndex = (currentFrameIndex + 1) % animationFrames.Length;
                    if (animationFrames[currentFrameIndex] != null)
                    {
                        targetImage.sprite = animationFrames[currentFrameIndex];
                    }
                }
            }

            // 2. Cập nhật hiệu ứng thở / nhấp nhô sống động (Idle Breathing)
            if (enableBreathing && rectTransform != null)
            {
                breathingTimer += Time.deltaTime * breathingSpeed;
                float sinValue = Mathf.Sin(breathingTimer);

                // Nhấp nhô toạ độ Y nhẹ nhàng
                float offsetY = sinValue * breathingAmplitude;
                rectTransform.anchoredPosition = baseAnchoredPosition + new Vector2(0f, offsetY);

                // Hiệu ứng co giãn nhẹ (Squash & Stretch)
                float scaleMod = 1f + (sinValue * 0.02f);
                rectTransform.localScale = new Vector3(baseScale.x * (2f - scaleMod), baseScale.y * scaleMod, baseScale.z);
            }
        }
    }
}
