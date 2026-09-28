using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using ScienceQuest.Core;
using ScienceQuest.Quiz;
using ScienceQuest.UI;

namespace ScienceQuest.MiniGames
{
    /// <summary>
    /// PhysicsShootingGallery - Quản lý Mini-game Bắn Vịt và tích hợp trực tiếp màn QuizScene dựng sẵn.
    /// Tính năng:
    /// 1. Thời gian thi đếm ngược 15 phút (900 giây, 15:00).
    /// 2. Bắn trúng 1 con vịt ➔ MỞ TRỰC TIẾP MÀN HÌNH QuizScene DỰNG SẴN CỦA BẠN.
    /// 3. Người chơi chọn đáp án trên giao diện QuizScene ➔ Trả lời đúng +10 điểm, +10 EXP; Trả lời sai 0 điểm.
    /// 4. Bấm "Tiếp Tục Bắn Vịt" ➔ Quay lại trường bắn để bắn con vịt tiếp theo.
    /// 5. Sau khi trả lời xong câu thứ 10 (hoặc hết 15:00) ➔ Hiện Panel_ResultBoard của QuizScene kèm nút "Xem Chi Tiết".
    /// </summary>
    public class PhysicsShootingGallery : MonoBehaviour
    {
        public static PhysicsShootingGallery Instance { get; private set; }

        [Header("=== CẤU HÌNH THỜI GIAN VÀ CÂU HỎI ===")]
        private float gameDuration = 900f; // 15 phút = 900 giây
        private float remainingTime = 900f;
        private int currentScore = 0;
        private int totalQuestions = 10;
        private int currentQuestionIndex = 0;
        private bool isGameActive = false;

        // UI References trên Canvas Physics
        private Canvas mainCanvas;
        private TextMeshProUGUI scoreText;
        private TextMeshProUGUI timerText;
        private TextMeshProUGUI questionCounterText;
        private GameObject timeUpOriginalObj;
        private GameObject shootingHudRoot;

        // Súng & Hồng tâm
        private RectTransform gunTransform;
        private RectTransform crosshairTransform;
        private GraphicRaycaster canvasRaycaster;
        private float fireCooldown = 0.35f;
        private float nextFireTime = 0f;

        // Danh sách mục tiêu (vịt và bia) bơi ngang
        private List<TargetItem> activeTargets = new List<TargetItem>();

        private class TargetItem
        {
            public Transform transform;
            public Vector3 initialLocalPos;
            public Vector3 initialScale;
            public float animOffset;
            public bool isDuck;
            public float moveSpeed;
            public bool isKnockedDown;
            public float knockDownTimer;
            public int pointValue;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitializeOnLoad()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Physics")
            {
                if (FindFirstObjectByType<PhysicsShootingGallery>() == null)
                {
                    GameObject go = new GameObject("PhysicsShootingGallery_Auto");
                    go.AddComponent<PhysicsShootingGallery>();
                    Debug.Log("[PhysicsShootingGallery] 🦆 Khởi tạo hệ thống Bắn Vịt cho scene Physics");
                }
            }
        }

        private void Awake()
        {
            QuizManager.IsDuckShootingMode = true;

            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            QuizManager.IsDuckShootingMode = true;

            mainCanvas = FindFirstObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogWarning("[PhysicsShootingGallery] Không tìm thấy Canvas!");
                return;
            }

            canvasRaycaster = mainCanvas.GetComponent<GraphicRaycaster>();
            if (canvasRaycaster == null)
            {
                canvasRaycaster = mainCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }

            CleanInitialStaticUI();
            SetupHUD();
            SetupGunAndCrosshair();
            ScanTargets();
            StartGame();
        }

        /// <summary>
        /// Tải ngầm QuizScene additively và ẨN HOÀN TOÀN, chỉ bật lên khi người chơi bắn trúng vịt
        /// </summary>
        private IEnumerator LoadQuizSceneAdditive()
        {
            QuizManager.IsDuckShootingMode = true;

            Scene quizScene = SceneManager.GetSceneByName("QuizScene");
            if (!quizScene.isLoaded)
            {
                AsyncOperation op = SceneManager.LoadSceneAsync("QuizScene", LoadSceneMode.Additive);
                yield return op;
            }

            // Ẩn ngay lập tức toàn bộ root GameObjects của QuizScene để KHÔNG hiện lên màn Bắn Vịt ban đầu
            quizScene = SceneManager.GetSceneByName("QuizScene");
            if (quizScene.isLoaded)
            {
                foreach (GameObject rootGo in quizScene.GetRootGameObjects())
                {
                    rootGo.SetActive(false);
                }
            }

            // Tắt Camera trùng từ QuizScene
            foreach (var cam in Camera.allCameras)
            {
                if (cam.gameObject.scene.name == "QuizScene")
                {
                    cam.gameObject.SetActive(false);
                }
            }

            // Tắt EventSystem trùng từ QuizScene (để dùng EventSystem chính của Physics)
            EventSystem[] allEvents = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var es in allEvents)
            {
                if (es.gameObject.scene.name == "QuizScene")
                {
                    es.gameObject.SetActive(false);
                }
            }

            // Ẩn Canvas QuizScene ban đầu
            if (QuizManager.Instance != null)
            {
                QuizManager.Instance.SetQuizVisible(false);
            }

            Debug.Log("[PhysicsShootingGallery] ✅ Đã nạp sẵn màn QuizScene dựng sẵn vào nền Bắn Vịt và ẩn hoàn toàn!");
        }

        /// <summary>
        /// Dọn dẹp các UI tĩnh cũ trong scene Physics (số 10, cục trắng giữa màn, con vịt vàng góc trên trái...)
        /// </summary>
        private void CleanInitialStaticUI()
        {
            // 1. Tìm và ẩn TIME UP ban đầu
            Transform timeUpT = mainCanvas.transform.Find("TIME UP");
            if (timeUpT != null)
            {
                timeUpOriginalObj = timeUpT.gameObject;
                timeUpOriginalObj.SetActive(false);
            }
            else
            {
                foreach (Transform t in mainCanvas.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name.ToLower().Contains("time up") || t.name.ToLower().Contains("timeup"))
                    {
                        timeUpOriginalObj = t.gameObject;
                        timeUpOriginalObj.SetActive(false);
                        break;
                    }
                }
            }

            // 2. Tìm và ẩn tất cả các đối tượng rác theo yêu cầu của người chơi
            Image[] allImages = mainCanvas.GetComponentsInChildren<Image>(true);
            foreach (var img in allImages)
            {
                if (img.sprite != null)
                {
                    string sName = img.sprite.name.ToLower();

                    // BỎ SỐ 10 và các số điểm tĩnh: text_0, text_1, text_2, text_plus, text_10, text_0_small, text_1_small...
                    if (sName.Contains("timeup") || sName.Contains("text_") || sName == "text_10")
                    {
                        img.gameObject.SetActive(false);
                    }

                    // BỎ CON VỊT VÀNG GÓC TRÊN CÙNG BÊN TRÁI
                    if (sName.Contains("duck_outline_yellow"))
                    {
                        RectTransform rt = img.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            // Icon vịt vàng nhỏ ở góc trên trái
                            if (rt.sizeDelta.x < 60f || (rt.anchoredPosition.x < -200f && rt.anchoredPosition.y > 150f))
                            {
                                img.gameObject.SetActive(false);
                            }
                        }
                    }
                }
                else
                {
                    // BỎ CỤC TRẮNG TRẮNG Ở GIỮA MÀN HÌNH (Image không có sprite, ở trung tâm màn hình)
                    RectTransform rt = img.GetComponent<RectTransform>();
                    if (rt != null && Mathf.Abs(rt.anchoredPosition.x) < 50f && Mathf.Abs(rt.anchoredPosition.y) < 50f)
                    {
                        img.gameObject.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// Tạo giao diện thời gian 15 phút, điểm số và số câu hỏi (đã bỏ hẳn cấp độ/EXP)
        /// </summary>
        private void SetupHUD()
        {
            // Container chứa toàn bộ HUD màn bắn vịt (sẽ tự động ẩn đi khi QuizScene mở ra)
            shootingHudRoot = new GameObject("Dynamic_ShootingHUDRoot");
            shootingHudRoot.transform.SetParent(mainCanvas.transform, false);

            RectTransform hudRootRt = shootingHudRoot.AddComponent<RectTransform>();
            hudRootRt.anchorMin = Vector2.zero;
            hudRootRt.anchorMax = Vector2.one;
            hudRootRt.sizeDelta = Vector2.zero;
            hudRootRt.anchoredPosition = Vector2.zero;

            // 1. Huy hiệu chữ SCORE (ở góc trên bên trái)
            GameObject scoreBadgeObj = new GameObject("Dynamic_ScoreBadge");
            scoreBadgeObj.transform.SetParent(shootingHudRoot.transform, false);

            RectTransform badgeRt = scoreBadgeObj.AddComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(0f, 1f);
            badgeRt.anchorMax = new Vector2(0f, 1f);
            badgeRt.pivot = new Vector2(0f, 1f);
            badgeRt.anchoredPosition = new Vector2(20f, -12f);
            badgeRt.sizeDelta = new Vector2(115f, 60f);

            Image badgeImg = scoreBadgeObj.AddComponent<Image>();
            Sprite scoreSprite = Resources.Load<Sprite>("Icons/text_score_small");
            if (scoreSprite != null)
            {
                badgeImg.sprite = scoreSprite;
            }
            badgeImg.raycastTarget = false;

            // 2. Chữ số điểm to rõ ràng nằm NGAY BÊN CẠNH chữ SCORE (không đè lên)
            GameObject scoreNumObj = new GameObject("Dynamic_ScoreNumber");
            scoreNumObj.transform.SetParent(shootingHudRoot.transform, false);

            RectTransform numRt = scoreNumObj.AddComponent<RectTransform>();
            numRt.anchorMin = new Vector2(0f, 1f);
            numRt.anchorMax = new Vector2(0f, 1f);
            numRt.pivot = new Vector2(0f, 1f);
            numRt.anchoredPosition = new Vector2(142f, -12f);
            numRt.sizeDelta = new Vector2(140f, 60f);

            scoreText = scoreNumObj.AddComponent<TextMeshProUGUI>();
            scoreText.text = "0";
            scoreText.fontSize = 48f;
            scoreText.fontStyle = FontStyles.Bold;
            scoreText.color = Color.white;
            scoreText.outlineWidth = 0.38f;
            scoreText.outlineColor = new Color32(0, 0, 0, 240);
            scoreText.alignment = TextAlignmentOptions.MidlineLeft;

            // 3. Đồng hồ đếm ngược 15 phút (ở giữa trên cùng)
            GameObject timerObj = new GameObject("Dynamic_TimerText");
            timerObj.transform.SetParent(shootingHudRoot.transform, false);

            RectTransform timerRt = timerObj.AddComponent<RectTransform>();
            timerRt.anchorMin = new Vector2(0.5f, 1f);
            timerRt.anchorMax = new Vector2(0.5f, 1f);
            timerRt.pivot = new Vector2(0.5f, 1f);
            timerRt.anchoredPosition = new Vector2(0f, -20f);
            timerRt.sizeDelta = new Vector2(250f, 55f);

            timerText = timerObj.AddComponent<TextMeshProUGUI>();
            timerText.text = "15:00";
            timerText.fontSize = 36f;
            timerText.fontStyle = FontStyles.Bold;
            timerText.color = Color.white;
            timerText.outlineWidth = 0.4f;
            timerText.outlineColor = new Color32(0, 0, 0, 230);
            timerText.alignment = TextAlignmentOptions.Center;

            // 4. Tiến độ câu hỏi (Câu X/10) ngay dưới cụm SCORE
            GameObject qCountObj = new GameObject("Dynamic_QuestionCounter");
            qCountObj.transform.SetParent(shootingHudRoot.transform, false);

            RectTransform qCountRt = qCountObj.AddComponent<RectTransform>();
            qCountRt.anchorMin = new Vector2(0f, 1f);
            qCountRt.anchorMax = new Vector2(0f, 1f);
            qCountRt.pivot = new Vector2(0f, 1f);
            qCountRt.anchoredPosition = new Vector2(24f, -78f);
            qCountRt.sizeDelta = new Vector2(200f, 35f);

            questionCounterText = qCountObj.AddComponent<TextMeshProUGUI>();
            questionCounterText.text = "Câu: 1/10";
            questionCounterText.fontSize = 20f;
            questionCounterText.fontStyle = FontStyles.Bold;
            questionCounterText.color = new Color(1f, 0.85f, 0.3f);
            questionCounterText.outlineWidth = 0.3f;
            questionCounterText.outlineColor = new Color32(0, 0, 0, 200);
            questionCounterText.alignment = TextAlignmentOptions.MidlineLeft;

            // 5. Banner hướng dẫn thao tác
            GameObject hintObj = new GameObject("Dynamic_HintBanner");
            hintObj.transform.SetParent(shootingHudRoot.transform, false);

            RectTransform hintRt = hintObj.AddComponent<RectTransform>();
            hintRt.anchorMin = new Vector2(0.5f, 0f);
            hintRt.anchorMax = new Vector2(0.5f, 0f);
            hintRt.pivot = new Vector2(0.5f, 0f);
            hintRt.anchoredPosition = new Vector2(0f, 15f);
            hintRt.sizeDelta = new Vector2(660f, 38f);

            Image hintBg = hintObj.AddComponent<Image>();
            hintBg.color = new Color(0f, 0f, 0f, 0.75f);

            GameObject hintTextObj = new GameObject("Text");
            hintTextObj.transform.SetParent(hintObj.transform, false);
            RectTransform textRt = hintTextObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            TextMeshProUGUI hintTmp = hintTextObj.AddComponent<TextMeshProUGUI>();
            hintTmp.text = "Bắn trúng 1 con vịt để mở câu hỏi - Trả lời đúng để ghi 10 điểm!";
            hintTmp.fontSize = 17f;
            hintTmp.fontStyle = FontStyles.Bold;
            hintTmp.color = new Color(1f, 0.95f, 0.6f);
            hintTmp.alignment = TextAlignmentOptions.Center;
        }

        private Vector2 GetMousePosition()
        {
            #if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Mouse.current != null)
            {
                return UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            }
            #endif

            try
            {
                return Input.mousePosition;
            }
            catch
            {
                return Vector2.zero;
            }
        }

        private bool IsFireTriggered()
        {
            #if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
            {
                return true;
            }
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                return true;
            }
            #endif

            try
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                {
                    return true;
                }
            }
            catch {}

            return false;
        }

        private void SetupGunAndCrosshair()
        {
            Image[] allImages = mainCanvas.GetComponentsInChildren<Image>(true);

            foreach (var img in allImages)
            {
                if (img.sprite == null) continue;
                string sName = img.sprite.name.ToLower();

                if (sName.Contains("rifle") && gunTransform == null)
                {
                    gunTransform = img.GetComponent<RectTransform>();
                    SetPivotWithoutMoving(gunTransform, new Vector2(0.5f, 0.05f));
                    img.raycastTarget = false;
                }

                if (sName.Contains("crosshair") && crosshairTransform == null)
                {
                    crosshairTransform = img.GetComponent<RectTransform>();
                    crosshairTransform.pivot = new Vector2(0.5f, 0.5f);
                    crosshairTransform.sizeDelta = new Vector2(55f, 55f);
                    img.raycastTarget = false;
                    crosshairTransform.SetAsLastSibling();
                }
            }

            if (crosshairTransform == null)
            {
                GameObject crossObj = new GameObject("Dynamic_Crosshair");
                crossObj.transform.SetParent(mainCanvas.transform, false);

                crosshairTransform = crossObj.AddComponent<RectTransform>();
                crosshairTransform.sizeDelta = new Vector2(50f, 50f);
                crosshairTransform.pivot = new Vector2(0.5f, 0.5f);

                TextMeshProUGUI tmp = crossObj.AddComponent<TextMeshProUGUI>();
                tmp.text = "+";
                tmp.fontSize = 38f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = Color.red;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
                crosshairTransform.SetAsLastSibling();
            }
        }

        private void SetPivotWithoutMoving(RectTransform rt, Vector2 newPivot)
        {
            Vector2 size = rt.rect.size;
            Vector2 deltaPivot = rt.pivot - newPivot;
            Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x * rt.localScale.x, deltaPivot.y * size.y * rt.localScale.y);
            rt.pivot = newPivot;
            rt.localPosition -= deltaPosition;
        }

        /// <summary>
        /// Ghép cọc vào các con vịt và bảng tâm để chúng cắm chặt vào nhau và di chuyển đồng bộ;
        /// Trả mảng cỏ về đúng vị trí lớp nền cố định; Đảm bảo vịt không bị gắn bảng tiêu.
        /// </summary>
        private void ScanTargets()
        {
            activeTargets.Clear();
            Image[] allImages = mainCanvas.GetComponentsInChildren<Image>(true);

            List<Image> duckImages = new List<Image>();
            List<Image> targetImages = new List<Image>();
            List<Image> woodSticks = new List<Image>();
            List<Image> metalSticks = new List<Image>();

            // 1. Phân loại và xử lý sơ bộ các đối tượng
            foreach (var img in allImages)
            {
                if (img.sprite == null) continue;
                string sName = img.sprite.name.ToLower();

                // Cố định mảng cỏ về nền Canvas, không cho dính vào cọc hay bay lung tung
                if (sName.Contains("grass"))
                {
                    if (img.transform.parent != mainCanvas.transform)
                    {
                        img.transform.SetParent(mainCanvas.transform, true);
                    }
                    img.rectTransform.localScale = Vector3.one;
                    img.rectTransform.localRotation = Quaternion.identity;
                    img.raycastTarget = false;

                    // Image (3) là mảng cỏ trước đó bị kéo lệch sang cọc sắt, đưa về đúng hàng grass2 liền mạch
                    if (img.gameObject.name.Contains("(3)"))
                    {
                        img.rectTransform.pivot = new Vector2(0.9468327f, 1f);
                        img.rectTransform.anchoredPosition = new Vector2(94.68f, 0f);
                        img.rectTransform.sizeDelta = new Vector2(100f, 100f);
                    }
                    continue;
                }

                // Bỏ qua icon vịt nhỏ góc trên cùng bên trái
                if (sName.Contains("duck_outline_yellow"))
                {
                    RectTransform rt = img.GetComponent<RectTransform>();
                    if (rt != null && (rt.sizeDelta.x < 60f || rt.anchoredPosition.y > 150f))
                    {
                        continue;
                    }
                }

                if (sName.Contains("stick_wood"))
                {
                    woodSticks.Add(img);
                }
                else if (sName.Contains("stick_metal"))
                {
                    metalSticks.Add(img);
                }
                else if (sName.Contains("duck"))
                {
                    duckImages.Add(img);
                }
                else if (sName.Contains("target") && !sName.Contains("white"))
                {
                    targetImages.Add(img);
                }
            }

            // Sắp xếp theo trục X để ghép cặp tự nhiên nhất
            duckImages.Sort((a, b) => a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));
            woodSticks.Sort((a, b) => a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));
            targetImages.Sort((a, b) => a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));
            metalSticks.Sort((a, b) => a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));

            // 2. Xử lý các con vịt: cắm cọc gỗ, xóa bỏ mọi bảng tâm hoặc mắt dán nhầm lên vịt
            for (int i = 0; i < duckImages.Count; i++)
            {
                Image duck = duckImages[i];
                duck.raycastTarget = true;
                duck.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                duck.rectTransform.sizeDelta = new Vector2(100f, 100f);

                Button oldBtn = duck.GetComponent<Button>();
                if (oldBtn != null) Destroy(oldBtn);

                // Loại bỏ bất kỳ đối tượng con nào là bảng tâm dán trên vịt
                for (int c = duck.transform.childCount - 1; c >= 0; c--)
                {
                    Transform child = duck.transform.GetChild(c);
                    Image childImg = child.GetComponent<Image>();
                    if (childImg != null && childImg.sprite != null)
                    {
                        string childSprite = childImg.sprite.name.ToLower();
                        if (childSprite.Contains("target"))
                        {
                            Destroy(child.gameObject);
                        }
                    }
                }

                // Cắm cọc gỗ chặt dưới chân con vịt
                if (i < woodSticks.Count)
                {
                    Image stick = woodSticks[i];
                    stick.transform.SetParent(duck.transform, false);
                    stick.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    stick.rectTransform.sizeDelta = new Vector2(25f, 85f);
                    stick.rectTransform.anchoredPosition = new Vector2(0f, -75f);
                    stick.rectTransform.localRotation = Quaternion.identity;
                    stick.rectTransform.localScale = Vector3.one;
                    stick.transform.SetAsFirstSibling(); // Vẽ chìm phía sau con vịt
                    stick.raycastTarget = false;
                }
            }

            // 3. Xử lý 2 bảng tâm: cắm cọc kim loại thẳng dưới tâm, đặt cao độ ngang tầm mặt nước
            for (int i = 0; i < targetImages.Count; i++)
            {
                Image target = targetImages[i];
                target.raycastTarget = true;
                target.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                target.rectTransform.sizeDelta = new Vector2(80f, 80f);

                // Đưa bảng tâm về cao độ hợp lý ngang mặt nước giống vịt (không bị bay lơ lửng trên trời)
                Vector3 tPos = target.transform.localPosition;
                tPos.y = 15f;
                target.transform.localPosition = tPos;

                Button oldBtn = target.GetComponent<Button>();
                if (oldBtn != null) Destroy(oldBtn);

                if (i < metalSticks.Count)
                {
                    Image stick = metalSticks[i];
                    stick.transform.SetParent(target.transform, false);
                    stick.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    stick.rectTransform.sizeDelta = new Vector2(25f, 85f);
                    stick.rectTransform.anchoredPosition = new Vector2(0f, -65f);
                    stick.rectTransform.localRotation = Quaternion.identity;
                    stick.rectTransform.localScale = Vector3.one;
                    stick.transform.SetAsFirstSibling(); // Vẽ chìm phía sau bảng tâm
                    stick.raycastTarget = false;
                }
            }

            // 4. Đăng ký tất cả vịt và bảng tâm vào danh sách di chuyển
            int targetIndex = 0;
            List<Image> mainTargets = new List<Image>();
            mainTargets.AddRange(duckImages);
            mainTargets.AddRange(targetImages);

            foreach (var img in mainTargets)
            {
                bool isDuck = img.sprite.name.ToLower().Contains("duck");
                float speed = 0f;

                if (isDuck)
                {
                    float dir = (targetIndex % 2 == 0) ? 1f : -1f;
                    float baseSpeed = 80f + (targetIndex * 25f) % 75f;
                    speed = dir * baseSpeed;
                }
                else
                {
                    float dir = (targetIndex % 2 == 1) ? 1f : -1f;
                    speed = dir * (85f + (targetIndex * 20f) % 50f);
                }

                TargetItem item = new TargetItem
                {
                    transform = img.transform,
                    initialLocalPos = img.transform.localPosition,
                    initialScale = img.transform.localScale,
                    animOffset = targetIndex * 0.8f,
                    isDuck = isDuck,
                    moveSpeed = speed,
                    pointValue = 10,
                    isKnockedDown = false,
                    knockDownTimer = 0f
                };
                activeTargets.Add(item);
                targetIndex++;
            }

            Debug.Log($"[PhysicsShootingGallery] 🦆 Đã ghép cọc chuẩn xác vào {activeTargets.Count} mục tiêu! Cọc và tiêu bay gắn liền 100%!");
        }

        /// <summary>
        /// Bắt đầu lượt chơi Bắn Vịt 15 phút
        /// </summary>
        public void StartGame()
        {
            currentScore = 0;
            currentQuestionIndex = 0;
            remainingTime = gameDuration; // 900 giây (15 phút)
            isGameActive = true;

            if (scoreText != null) scoreText.text = "0";
            if (questionCounterText != null) questionCounterText.text = "Câu: 1/10";
            if (timerText != null)
            {
                timerText.gameObject.SetActive(true);
                timerText.text = "15:00";
                timerText.color = Color.white;
                timerText.transform.localScale = Vector3.one;
            }

            if (timeUpOriginalObj != null) timeUpOriginalObj.SetActive(false);

            // Bắt đầu tải ngầm QuizScene additively
            StartCoroutine(LoadQuizSceneAdditive());

            Debug.Log("[PhysicsShootingGallery] 🚀 Bắt đầu lượt chơi Bắn Vịt kết hợp QuizScene!");
        }

        private void Update()
        {
            if (!isGameActive) return;

            // Nếu màn QuizScene đang mở ➔ Tạm ngưng ngắm bắn, ẩn triệt để HUD bắn vịt và đồng bộ đồng hồ
            bool isQuizShowing = QuizManager.Instance != null && QuizManager.Instance.IsQuizVisible;

            if (shootingHudRoot != null && shootingHudRoot.activeSelf == isQuizShowing)
            {
                shootingHudRoot.SetActive(!isQuizShowing);
            }
            if (crosshairTransform != null && crosshairTransform.gameObject.activeSelf == isQuizShowing)
            {
                crosshairTransform.gameObject.SetActive(!isQuizShowing);
            }
            if (gunTransform != null && gunTransform.gameObject.activeSelf == isQuizShowing)
            {
                gunTransform.gameObject.SetActive(!isQuizShowing);
            }

            if (isQuizShowing)
            {
                if (QuizManager.Instance != null)
                {
                    QuizManager.Instance.SyncTimer(remainingTime);
                }
            }

            // Đếm ngược 15 phút
            remainingTime -= Time.deltaTime;
            if (remainingTime > 0)
            {
                int totalSec = Mathf.Max(0, Mathf.CeilToInt(remainingTime));
                int min = totalSec / 60;
                int sec = totalSec % 60;

                if (timerText != null)
                {
                    timerText.text = string.Format("{0:00}:{1:00}", min, sec);

                    if (totalSec <= 60)
                    {
                        timerText.color = Color.red;
                        float pulse = 1f + Mathf.PingPong(Time.time * 3f, 0.2f);
                        timerText.transform.localScale = Vector3.one * pulse;
                    }
                    else if (totalSec <= 180)
                    {
                        timerText.color = new Color(1f, 0.6f, 0f);
                        timerText.transform.localScale = Vector3.one;
                    }
                    else
                    {
                        timerText.color = Color.white;
                        timerText.transform.localScale = Vector3.one;
                    }
                }
            }
            else
            {
                remainingTime = 0;
                EndGameDueToTimer();
                return;
            }

            // Nếu QuizScene đang mở, không cho bắn súng và di chuyển hồng tâm
            if (isQuizShowing) return;

            Vector2 mousePos = GetMousePosition();

            // Cập nhật vị trí Hồng tâm
            if (crosshairTransform != null)
            {
                crosshairTransform.position = mousePos;
            }

            // Xoay súng ngắm theo chuột
            if (gunTransform != null)
            {
                Vector3 gunScreen = gunTransform.position;
                Vector2 aimDir = mousePos - (Vector2)gunScreen;

                if (aimDir.y > 10f)
                {
                    float targetAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
                    targetAngle = Mathf.Clamp(targetAngle, 20f, 160f);
                    gunTransform.rotation = Quaternion.Euler(0f, 0f, targetAngle - 90f);
                }
            }

            // Nhận lệnh BẮN
            if (IsFireTriggered())
            {
                if (!IsPointerOverSystemButtons())
                {
                    FireGun();
                }
            }

            // Chuyển động bơi ngang & nhấp nhô của vịt
            float dt = Time.deltaTime;
            float time = Time.time;
            float screenHalfWidth = Screen.width * 0.58f;

            for (int i = 0; i < activeTargets.Count; i++)
            {
                TargetItem item = activeTargets[i];
                if (item.transform == null) continue;

                if (item.isKnockedDown)
                {
                    item.knockDownTimer -= dt;
                    if (item.knockDownTimer <= 0)
                    {
                        item.isKnockedDown = false;
                        StartCoroutine(AnimateTargetRise(item));
                    }
                    continue;
                }

                Vector3 pos = item.transform.localPosition;
                pos.x += item.moveSpeed * dt;

                if (item.moveSpeed > 0 && pos.x > screenHalfWidth)
                {
                    pos.x = -screenHalfWidth;
                }
                else if (item.moveSpeed < 0 && pos.x < -screenHalfWidth)
                {
                    pos.x = screenHalfWidth;
                }

                float waveY = Mathf.Sin(time * 3.5f + item.animOffset) * 6f;
                float waveRot = Mathf.Sin(time * 2.5f + item.animOffset) * 2.5f;

                pos.y = item.initialLocalPos.y + waveY;
                item.transform.localPosition = pos;
                item.transform.localRotation = Quaternion.Euler(0f, 0f, waveRot);
            }
        }

        private bool IsPointerOverSystemButtons()
        {
            if (EventSystem.current == null) return false;

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = GetMousePosition()
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var r in results)
            {
                if (r.gameObject.GetComponent<Button>() != null)
                {
                    if (r.gameObject.name.Contains("Study") || r.gameObject.name.Contains("Btn_"))
                        return true;
                }
            }
            return false;
        }

        private void FireGun()
        {
            if (Time.time < nextFireTime) return;
            nextFireTime = Time.time + fireCooldown;

            Vector2 mousePos = GetMousePosition();

            if (gunTransform != null)
            {
                StartCoroutine(AnimateGunRecoil());
            }

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = mousePos
            };

            List<RaycastResult> results = new List<RaycastResult>();
            if (canvasRaycaster != null)
            {
                canvasRaycaster.Raycast(pointerData, results);
            }

            TargetItem hitTarget = null;
            foreach (var res in results)
            {
                TargetItem found = activeTargets.Find(t => t.transform.gameObject == res.gameObject && !t.isKnockedDown);
                if (found != null)
                {
                    hitTarget = found;
                    break;
                }
            }

            if (hitTarget == null)
            {
                Camera cam = mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCanvas.worldCamera;
                foreach (var item in activeTargets)
                {
                    if (item.transform == null || item.isKnockedDown) continue;
                    RectTransform rt = item.transform.GetComponent<RectTransform>();
                    if (rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos, cam))
                    {
                        hitTarget = item;
                        break;
                    }
                }
            }

            if (hitTarget != null)
            {
                OnTargetHit(hitTarget, mousePos);
            }
            else
            {
                OnShotMissed(mousePos);
            }
        }

        private IEnumerator AnimateGunRecoil()
        {
            if (gunTransform == null) yield break;

            Vector3 origPos = gunTransform.localPosition;
            Vector3 recoilDir = -gunTransform.up;
            gunTransform.localPosition = origPos + recoilDir * 18f;

            yield return new WaitForSeconds(0.08f);

            if (gunTransform != null)
            {
                gunTransform.localPosition = origPos;
            }
        }

        /// <summary>
        /// Xử lý khi bắn trúng vịt: lặn vịt xuống và MỞ MÀN HÌNH QuizScene DỰNG SẴN
        /// </summary>
        private void OnTargetHit(TargetItem item, Vector3 hitPos)
        {
            item.isKnockedDown = true;
            item.knockDownTimer = 3.5f;

            StartCoroutine(AnimateTargetKnockdown(item));

            SpawnFloatingText($"CÂU {currentQuestionIndex + 1}", hitPos, new Color(1f, 0.9f, 0.2f), 34f);

            // MỞ TRỰC TIẾP MÀN HÌNH QuizScene DỰNG SẴN CỦA NGƯỜI CHƠI
            if (QuizManager.Instance != null)
            {
                QuizManager.Instance.ShowDuckShootingQuestion(currentQuestionIndex);
            }
            else
            {
                Debug.LogWarning("[PhysicsShootingGallery] ⚠️ QuizManager.Instance chưa sẵn sàng, đang nạp QuizScene...");
                StartCoroutine(OpenQuizSceneWhenReady(currentQuestionIndex));
            }
        }

        private IEnumerator OpenQuizSceneWhenReady(int qIndex)
        {
            yield return StartCoroutine(LoadQuizSceneAdditive());
            if (QuizManager.Instance != null)
            {
                QuizManager.Instance.ShowDuckShootingQuestion(qIndex);
            }
        }

        /// <summary>
        /// Gọi từ QuizManager khi người chơi hoàn thành 1 câu hỏi và bấm "Tiếp Tục Bắn Vịt"
        /// </summary>
        public void OnReturnFromQuestion(int score)
        {
            currentQuestionIndex++;
            currentScore = score;

            if (scoreText != null)
            {
                scoreText.text = currentScore.ToString();
            }

            if (questionCounterText != null)
            {
                questionCounterText.text = $"Câu: {Mathf.Min(currentQuestionIndex + 1, totalQuestions)}/{totalQuestions}";
            }

            Debug.Log($"[PhysicsShootingGallery] 🎯 Quay lại trường bắn, chuẩn bị cho câu {currentQuestionIndex + 1}/10!");
        }

        private void EndGameDueToTimer()
        {
            isGameActive = false;

            if (timerText != null)
            {
                timerText.gameObject.SetActive(false);
            }

            if (timeUpOriginalObj != null)
            {
                timeUpOriginalObj.SetActive(true);
            }

            if (QuizManager.Instance != null)
            {
                QuizManager.Instance.SetQuizVisible(true);
                // Gọi kết thúc quiz và hiển thị Panel_ResultBoard dựng sẵn của QuizScene
                QuizManager.Instance.SendMessage("EndQuiz", SendMessageOptions.DontRequireReceiver);
            }
        }

        private IEnumerator AnimateTargetKnockdown(TargetItem item)
        {
            if (item.transform == null) yield break;

            Vector3 origScale = item.initialScale;
            float elapsed = 0f;
            float dur = 0.18f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / dur;
                if (item.transform != null)
                {
                    item.transform.localScale = Vector3.Lerp(origScale, new Vector3(origScale.x, 0f, origScale.z), progress);
                }
                yield return null;
            }

            if (item.transform != null)
            {
                item.transform.localScale = Vector3.zero;
            }
        }

        private IEnumerator AnimateTargetRise(TargetItem item)
        {
            if (item.transform == null) yield break;

            Vector3 origScale = item.initialScale;
            float elapsed = 0f;
            float dur = 0.22f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / dur;
                if (item.transform != null)
                {
                    item.transform.localScale = Vector3.Lerp(new Vector3(origScale.x, 0f, origScale.z), origScale, progress);
                }
                yield return null;
            }

            if (item.transform != null)
            {
                item.transform.localScale = origScale;
            }
        }

        private void OnShotMissed(Vector3 missPos)
        {
            SpawnFloatingText("Trượt!", missPos, new Color(0.8f, 0.8f, 0.8f, 0.85f), 22f);
        }

        private void SpawnFloatingText(string text, Vector3 worldPos, Color textColor, float fontSize = 32f)
        {
            GameObject popObj = new GameObject("PopText");
            popObj.transform.SetParent(mainCanvas.transform, false);
            popObj.transform.position = worldPos;

            TextMeshProUGUI tmp = popObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = textColor;
            tmp.outlineWidth = 0.35f;
            tmp.outlineColor = new Color32(0, 0, 0, 230);
            tmp.alignment = TextAlignmentOptions.Center;

            StartCoroutine(AnimateFloatingText(popObj));
        }

        private IEnumerator AnimateFloatingText(GameObject obj)
        {
            if (obj == null) yield break;

            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            RectTransform rt = obj.GetComponent<RectTransform>();
            float duration = 0.65f;
            float elapsed = 0f;
            Vector3 startPos = rt.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                if (rt != null)
                {
                    rt.position = startPos + new Vector3(0f, progress * 45f, 0f);
                }

                if (tmp != null)
                {
                    Color c = tmp.color;
                    c.a = 1f - progress;
                    tmp.color = c;
                }

                yield return null;
            }

            Destroy(obj);
        }
    }
}
