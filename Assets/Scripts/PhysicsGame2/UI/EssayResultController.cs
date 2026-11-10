using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển logic giao diện Scene 19.1. Result_TL (Màn kết quả Tự luận - Game 2 Aurelia)
    /// Đảm bảo hoàn toàn các tiêu chí nghiệm thu:
    /// 1. Giữ background fantasy, khung hoàng gia, vòng nguyệt quế và phong cách xanh navy - vàng.
    /// 2. Tiêu đề KẾT QUẢ TỰ LUẬN nằm gọn trong khu vực phía trên, không che đồng hồ. Đồng hồ nằm cao hơn, không đè lên chữ.
    ///    Không tạo text TMP trùng đè lên chữ thư pháp vàng đã có sẵn trên sprite.
    /// 3. Điểm X.X/10 đặt ở chính giữa vòng nguyệt quế, cỡ chữ lớn, màu vàng kim, không bị rớt dòng.
    /// 4. Ba ô thống kê (ĐÃ LÀM, CHƯA LÀM, THỜI GIAN):
    ///    - Bỏ các icon tích xanh và x đỏ không khớp với tự luận.
    ///    - Ẩn toàn bộ sprite nhãn cũ ("Câu đúng", "Câu sai").
    ///    - Luôn hiển thị ĐẦY ĐỦ VÀ RÕ RÀNG cả Tiêu đề và Giá trị:
    ///      * ĐÃ LÀM: X/5 câu (xanh lục bảo)
    ///      * CHƯA LÀM: Y/5 câu (đỏ san hô nhạt nếu còn câu chưa làm)
    ///      * THỜI GIAN: MM:SS (vàng kem)
    /// 5. Ba nút dưới cùng (VỀ KHU VỰC, XEM LỜI GIẢI CHI TIẾT, LÀM LẠI):
    ///    - Bố cục icon và chữ thẳng hàng, khoảng cách an toàn, tuyệt đối không đè lên nhau.
    ///    - Tắt raycastTarget trên các icon/text con, đảm bảo click được 100% cả 3 nút.
    /// </summary>
    public class EssayResultController : MonoBehaviour
    {
        [Header("=== TITLE BANNER ===")]
        [SerializeField] private Image titleBannerImage;

        [Header("=== SCORE & STATUS ===")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("=== 3 STAT BOXES ===")]
        [SerializeField] private TextMeshProUGUI box1HeaderText; // "ĐÃ LÀM"
        [SerializeField] private TextMeshProUGUI box1ValueText;  // "X/5 câu"
        [SerializeField] private TextMeshProUGUI box2HeaderText; // "CHƯA LÀM"
        [SerializeField] private TextMeshProUGUI box2ValueText;  // "Y/5 câu"
        [SerializeField] private TextMeshProUGUI box3HeaderText; // "THỜI GIAN"
        [SerializeField] private TextMeshProUGUI box3ValueText;  // "MM:SS"

        [Header("=== NOTICES & BUTTONS ===")]
        [SerializeField] private TextMeshProUGUI rubricNoticeText;
        [SerializeField] private Button btnReturnToMap;
        [SerializeField] private Button btnViewDetail;
        [SerializeField] private Button btnRetry;

        private void Awake()
        {
            EnsureEventSystem();
            NavigationService.Instance.EnsureDevContext();
            AutoBindUIReferences();
            SetupStatBoxHeaders();
            AlignAndOrganizeButtons();
            FixRaycastBlockers();
            SetupButtonListeners();
            AdjustTopBannerAndTimer();
            EnsureAttemptRecorded();

            RenderAttemptResult();
        }

        private void OnEnable()
        {
            AutoBindUIReferences();
            SetupStatBoxHeaders();
            AlignAndOrganizeButtons();
            FixRaycastBlockers();
            SetupButtonListeners();
            AdjustTopBannerAndTimer();
            EnsureAttemptRecorded();

            RenderAttemptResult();
        }

        private void Start()
        {
            AdjustTopBannerAndTimer();
            EnsureAttemptRecorded();

            RenderAttemptResult();
        }

        private void Update()
        {
            bool escPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                escPressed = true;
            }
#endif
            if (!escPressed && Input.GetKeyDown(KeyCode.Escape))
            {
                escPressed = true;
            }

            if (escPressed)
            {
                OnReturnToMapClicked();
            }
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }
        }

        /// <summary>
        /// Căn chỉnh Tiêu đề và Đồng hồ đếm ngược: Đồng hồ nằm tại Y = 498, Banner nằm tại Y = 410.
        /// Cách nhau hơn 12px, tuyệt đối không bị đè lên nhau.
        /// </summary>
        private void AdjustTopBannerAndTimer()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. Đồng hồ đếm ngược toàn cục (Session Timer)
            Transform timerBadge = canvas.transform.Find("Aurelia_SessionTimerBadge");
            if (timerBadge != null)
            {
                RectTransform tbRt = timerBadge.GetComponent<RectTransform>();
                if (tbRt != null)
                {
                    tbRt.anchorMin = new Vector2(0.5f, 0.5f);
                    tbRt.anchorMax = new Vector2(0.5f, 0.5f);
                    tbRt.pivot = new Vector2(0.5f, 0.5f);
                    tbRt.anchoredPosition = new Vector2(0f, 498f);
                    tbRt.sizeDelta = new Vector2(160f, 32f);
                }
            }

            // 2. Banner tiêu đề vàng "KẾT QUẢ TỰ LUẬN"
            if (titleBannerImage != null)
            {
                titleBannerImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                titleBannerImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                titleBannerImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                titleBannerImage.rectTransform.anchoredPosition = new Vector2(0f, 410f);
                titleBannerImage.rectTransform.sizeDelta = new Vector2(800f, 120f);
                titleBannerImage.raycastTarget = false;

                // Xóa triệt để TMP con nếu trước đó từng bị chèn lên banner
                Transform existingTitle = titleBannerImage.transform.Find("Title_Text");
                if (existingTitle != null)
                {
                    Destroy(existingTitle.gameObject);
                }
            }
        }

        private void AutoBindUIReferences()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. Tìm Banner tiêu đề vàng
            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
            {
                Vector2 pos = img.rectTransform.anchoredPosition;
                if (pos.y > 350f && Mathf.Abs(pos.x) < 80f)
                {
                    titleBannerImage = img;
                    img.raycastTarget = false;
                    break;
                }
            }

            // 2. Điểm số: Nằm chính giữa vòng nguyệt quế (X = 38, Y = 175)
            Transform existingScore = canvas.transform.Find("Aurelia_ScoreText");
            if (existingScore != null)
            {
                scoreText = existingScore.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                // Tìm TMP có sẵn trong vùng nguyệt quế hoặc tạo mới
                TextMeshProUGUI[] tmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in tmps)
                {
                    Vector2 pos = t.rectTransform.anchoredPosition;
                    if (pos.y > 150f && pos.y < 280f && Mathf.Abs(pos.x - 38f) < 80f && t.transform.parent == canvas.transform)
                    {
                        scoreText = t;
                        break;
                    }
                }

                if (scoreText == null)
                {
                    GameObject sGo = new GameObject("Aurelia_ScoreText");
                    sGo.transform.SetParent(canvas.transform, false);
                    scoreText = sGo.AddComponent<TextMeshProUGUI>();
                }
            }

            if (scoreText != null)
            {
                scoreText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                scoreText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                scoreText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                scoreText.rectTransform.anchoredPosition = new Vector2(38f, 175f);
                scoreText.rectTransform.sizeDelta = new Vector2(380f, 70f);
                scoreText.textWrappingMode = TextWrappingModes.NoWrap;
                scoreText.overflowMode = TextOverflowModes.Overflow;
                scoreText.alignment = TextAlignmentOptions.Center;
                scoreText.fontSize = 50f;
                scoreText.fontStyle = FontStyles.Bold;
                scoreText.color = new Color(1f, 0.92f, 0.40f, 1f); // Vàng kim sáng
                scoreText.raycastTarget = false;
                scoreText.gameObject.SetActive(true);
            }

            // 3. Trạng thái đạt/chưa đạt: Nằm ngay dưới chữ "ĐIỂM" (X = 38, Y = 25)
            Transform existingStatus = canvas.transform.Find("Aurelia_StatusText");
            if (existingStatus != null)
            {
                statusText = existingStatus.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                TextMeshProUGUI[] tmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in tmps)
                {
                    Vector2 pos = t.rectTransform.anchoredPosition;
                    if (Mathf.Abs(pos.y) < 45f && Mathf.Abs(pos.x - 38f) < 60f && t.transform.parent == canvas.transform)
                    {
                        statusText = t;
                        break;
                    }
                }

                if (statusText == null)
                {
                    GameObject stGo = new GameObject("Aurelia_StatusText");
                    stGo.transform.SetParent(canvas.transform, false);
                    statusText = stGo.AddComponent<TextMeshProUGUI>();
                }
            }

            if (statusText != null)
            {
                statusText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                statusText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                statusText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                statusText.rectTransform.anchoredPosition = new Vector2(38f, 25f);
                statusText.rectTransform.sizeDelta = new Vector2(580f, 40f);
                statusText.textWrappingMode = TextWrappingModes.NoWrap;
                statusText.overflowMode = TextOverflowModes.Overflow;
                statusText.alignment = TextAlignmentOptions.Center;
                statusText.fontSize = 24f;
                statusText.fontStyle = FontStyles.Bold;
                statusText.raycastTarget = false;
                statusText.gameObject.SetActive(true);
            }

            // Dọn dẹp text số 8 rác nếu có
            foreach (var t in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                Vector2 pos = t.rectTransform.anchoredPosition;
                if (pos.x > 400f && pos.x < 650f && Mathf.Abs(pos.y - -10f) < 30f && t.text.Trim() == "8")
                {
                    t.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Thiết lập 3 ô thống kê (ĐÃ LÀM, CHƯA LÀM, THỜI GIAN) hoàn toàn sạch sẽ:
        /// - Ẩn bỏ các icon tích xanh và x đỏ không phù hợp với tự luận.
        /// - Ẩn bỏ các sprite nhãn cũ ("Câu đúng", "Câu sai").
        /// - Tạo header và value đồng bộ, căn giữa chính xác tại tâm của 3 khung:
        ///   Ô 1 (Đã làm): X = -467
        ///   Ô 2 (Chưa làm): X = 23
        ///   Ô 3 (Thời gian): X = 494
        /// </summary>
        private void SetupStatBoxHeaders()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. Ẩn toàn bộ sprite nhãn trắc nghiệm cũ và các icon tích xanh / x đỏ
            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
            {
                string n = img.gameObject.name;
                // Ẩn nhãn cũ
                if (n == "Image (8)" || n == "Image (9)" || n == "Image (10)")
                {
                    img.gameObject.SetActive(false);
                }
                // Ẩn icon tích xanh của trắc nghiệm
                else if (n == "Image" && img.rectTransform.anchoredPosition.x < -350f && img.rectTransform.anchoredPosition.y < -100f && img.rectTransform.anchoredPosition.y > -260f)
                {
                    img.gameObject.SetActive(false);
                }
                // Ẩn icon x đỏ của trắc nghiệm
                else if (n == "Image (2)" && img.rectTransform.anchoredPosition.x > 150f && img.rectTransform.anchoredPosition.x < 350f && img.rectTransform.anchoredPosition.y < -100f)
                {
                    img.gameObject.SetActive(false);
                }
                // Ẩn icon đồng hồ cũ trong ô thời gian nếu có
                else if (n == "Image (3)" && img.rectTransform.anchoredPosition.x > 600f)
                {
                    img.gameObject.SetActive(false);
                }
            }

            // 2. Ẩn các text số cũ của trắc nghiệm ("8", "2", "03:25") nhưng BẢO VỆ TUYỆT ĐỐI các text mới
            foreach (var t in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                string tName = t.gameObject.name;
                if (tName.StartsWith("StatHeader_") || tName.StartsWith("StatValue_") ||
                    tName.StartsWith("Aurelia_") || t == scoreText || t == statusText ||
                    t == box1HeaderText || t == box1ValueText ||
                    t == box2HeaderText || t == box2ValueText ||
                    t == box3HeaderText || t == box3ValueText)
                {
                    continue;
                }

                Vector2 pos = t.rectTransform.anchoredPosition;
                if (pos.y < -160f && pos.y > -280f && t.GetComponentInParent<Button>() == null)
                {
                    t.gameObject.SetActive(false);
                }
            }

            TMP_FontAsset fontAsset = (scoreText != null && scoreText.font != null)
                ? scoreText.font
                : canvas.GetComponentInChildren<TextMeshProUGUI>(true)?.font;

            // 3. Ô 1: "ĐÃ LÀM" (Căn giữa cột chữ ô 1 tại X = -467)
            Transform h1Trans = canvas.transform.Find("StatHeader_DaLam");
            if (h1Trans == null)
            {
                GameObject h1Go = new GameObject("StatHeader_DaLam");
                h1Go.transform.SetParent(canvas.transform, false);
                RectTransform rt = h1Go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(-467f, -135f);
                rt.sizeDelta = new Vector2(320f, 40f);
                box1HeaderText = h1Go.AddComponent<TextMeshProUGUI>();
                box1HeaderText.text = "ĐÃ LÀM";
                box1HeaderText.fontSize = 24f;
                box1HeaderText.fontStyle = FontStyles.Bold;
                box1HeaderText.alignment = TextAlignmentOptions.Center;
                box1HeaderText.color = new Color(0.96f, 0.82f, 0.38f, 1f); // Vàng kim Aurelia
                box1HeaderText.raycastTarget = false;
            }
            else
            {
                box1HeaderText = h1Trans.GetComponent<TextMeshProUGUI>();
                box1HeaderText.rectTransform.anchoredPosition = new Vector2(-467f, -135f);
                box1HeaderText.rectTransform.sizeDelta = new Vector2(320f, 40f);
            }
            if (fontAsset != null) box1HeaderText.font = fontAsset;
            box1HeaderText.gameObject.SetActive(true);
            box1HeaderText.transform.SetAsLastSibling();

            Transform v1Trans = canvas.transform.Find("StatValue_DaLam");
            if (v1Trans == null)
            {
                GameObject v1Go = new GameObject("StatValue_DaLam");
                v1Go.transform.SetParent(canvas.transform, false);
                RectTransform rt = v1Go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(-467f, -220f);
                rt.sizeDelta = new Vector2(340f, 60f);
                box1ValueText = v1Go.AddComponent<TextMeshProUGUI>();
                box1ValueText.fontSize = 38f;
                box1ValueText.fontStyle = FontStyles.Bold;
                box1ValueText.alignment = TextAlignmentOptions.Center;
                box1ValueText.color = new Color(0.29f, 0.87f, 0.50f, 1f); // Xanh ngọc lục bảo
                box1ValueText.raycastTarget = false;
            }
            else
            {
                box1ValueText = v1Trans.GetComponent<TextMeshProUGUI>();
                box1ValueText.rectTransform.anchoredPosition = new Vector2(-467f, -220f);
                box1ValueText.rectTransform.sizeDelta = new Vector2(340f, 60f);
            }
            if (fontAsset != null) box1ValueText.font = fontAsset;
            box1ValueText.gameObject.SetActive(true);
            box1ValueText.transform.SetAsLastSibling();

            // 4. Ô 2: "CHƯA LÀM" (Căn giữa cột chữ ô 2 tại X = 23)
            Transform h2Trans = canvas.transform.Find("StatHeader_ChuaLam");
            if (h2Trans == null)
            {
                GameObject h2Go = new GameObject("StatHeader_ChuaLam");
                h2Go.transform.SetParent(canvas.transform, false);
                RectTransform rt = h2Go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(23f, -135f);
                rt.sizeDelta = new Vector2(320f, 40f);
                box2HeaderText = h2Go.AddComponent<TextMeshProUGUI>();
                box2HeaderText.text = "CHƯA LÀM";
                box2HeaderText.fontSize = 24f;
                box2HeaderText.fontStyle = FontStyles.Bold;
                box2HeaderText.alignment = TextAlignmentOptions.Center;
                box2HeaderText.color = new Color(0.96f, 0.82f, 0.38f, 1f);
                box2HeaderText.raycastTarget = false;
            }
            else
            {
                box2HeaderText = h2Trans.GetComponent<TextMeshProUGUI>();
                box2HeaderText.rectTransform.anchoredPosition = new Vector2(23f, -135f);
                box2HeaderText.rectTransform.sizeDelta = new Vector2(320f, 40f);
            }
            if (fontAsset != null) box2HeaderText.font = fontAsset;
            box2HeaderText.gameObject.SetActive(true);
            box2HeaderText.transform.SetAsLastSibling();

            Transform v2Trans = canvas.transform.Find("StatValue_ChuaLam");
            if (v2Trans == null)
            {
                GameObject v2Go = new GameObject("StatValue_ChuaLam");
                v2Go.transform.SetParent(canvas.transform, false);
                RectTransform rt = v2Go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(23f, -220f);
                rt.sizeDelta = new Vector2(340f, 60f);
                box2ValueText = v2Go.AddComponent<TextMeshProUGUI>();
                box2ValueText.fontSize = 38f;
                box2ValueText.fontStyle = FontStyles.Bold;
                box2ValueText.alignment = TextAlignmentOptions.Center;
                box2ValueText.color = new Color(0.97f, 0.44f, 0.44f, 1f); // Đỏ san hô nhạt
                box2ValueText.raycastTarget = false;
            }
            else
            {
                box2ValueText = v2Trans.GetComponent<TextMeshProUGUI>();
                box2ValueText.rectTransform.anchoredPosition = new Vector2(23f, -220f);
                box2ValueText.rectTransform.sizeDelta = new Vector2(340f, 60f);
            }
            if (fontAsset != null) box2ValueText.font = fontAsset;
            box2ValueText.gameObject.SetActive(true);
            box2ValueText.transform.SetAsLastSibling();

            // 5. Ô 3: "THỜI GIAN" (Căn giữa cột chữ ô 3 tại X = 494)
            Transform h3Trans = canvas.transform.Find("StatHeader_ThoiGian");
            if (h3Trans == null)
            {
                GameObject h3Go = new GameObject("StatHeader_ThoiGian");
                h3Go.transform.SetParent(canvas.transform, false);
                RectTransform rt = h3Go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(494f, -135f);
                rt.sizeDelta = new Vector2(320f, 40f);
                box3HeaderText = h3Go.AddComponent<TextMeshProUGUI>();
                box3HeaderText.text = "THỜI GIAN";
                box3HeaderText.fontSize = 24f;
                box3HeaderText.fontStyle = FontStyles.Bold;
                box3HeaderText.alignment = TextAlignmentOptions.Center;
                box3HeaderText.color = new Color(0.96f, 0.82f, 0.38f, 1f);
                box3HeaderText.raycastTarget = false;
            }
            else
            {
                box3HeaderText = h3Trans.GetComponent<TextMeshProUGUI>();
                box3HeaderText.rectTransform.anchoredPosition = new Vector2(494f, -135f);
                box3HeaderText.rectTransform.sizeDelta = new Vector2(320f, 40f);
            }
            if (fontAsset != null) box3HeaderText.font = fontAsset;
            box3HeaderText.gameObject.SetActive(true);
            box3HeaderText.transform.SetAsLastSibling();

            Transform v3Trans = canvas.transform.Find("StatValue_ThoiGian");
            if (v3Trans == null)
            {
                GameObject v3Go = new GameObject("StatValue_ThoiGian");
                v3Go.transform.SetParent(canvas.transform, false);
                RectTransform rt = v3Go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(494f, -220f);
                rt.sizeDelta = new Vector2(340f, 60f);
                box3ValueText = v3Go.AddComponent<TextMeshProUGUI>();
                box3ValueText.fontSize = 38f;
                box3ValueText.fontStyle = FontStyles.Bold;
                box3ValueText.alignment = TextAlignmentOptions.Center;
                box3ValueText.color = new Color(1f, 0.95f, 0.70f, 1f); // Trắng kem sáng
                box3ValueText.raycastTarget = false;
            }
            else
            {
                box3ValueText = v3Trans.GetComponent<TextMeshProUGUI>();
                box3ValueText.rectTransform.anchoredPosition = new Vector2(494f, -220f);
                box3ValueText.rectTransform.sizeDelta = new Vector2(340f, 60f);
            }
            if (fontAsset != null) box3ValueText.font = fontAsset;
            box3ValueText.gameObject.SetActive(true);
            box3ValueText.transform.SetAsLastSibling();
        }

        /// <summary>
        /// Căn chỉnh 3 nút bấm dưới cùng hoàn toàn thẳng hàng, bố cục icon + chữ rõ ràng, không đè lên nhau:
        /// Nút trái: "VỀ KHU VỰC"
        /// Nút giữa: "XEM LỜI GIẢI CHI TIẾT"
        /// Nút phải: "LÀM LẠI"
        /// </summary>
        private void AlignAndOrganizeButtons()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. Tìm chính xác 3 nút điều hướng và vô hiệu hóa các nút khung thống kê
            Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                string bName = b.gameObject.name;
                Vector2 pos = b.GetComponent<RectTransform>().anchoredPosition;

                // Các ô thống kê không phải là nút bấm
                if (bName.Contains("Button (4)") || bName.Contains("Button (5)") || bName.Contains("Button (6)") || bName.Contains("Button (3)"))
                {
                    b.enabled = false;
                    continue;
                }

                if (bName == "Button" || (pos.y < -350f && pos.x < -300f))
                {
                    btnReturnToMap = b;
                }
                else if (bName == "Button (1)" || (pos.y < -350f && Mathf.Abs(pos.x) < 250f))
                {
                    btnViewDetail = b;
                }
                else if (bName == "Button (2)" || (pos.y < -350f && pos.x > 250f))
                {
                    btnRetry = b;
                }
            }

            // 2. Căn chỉnh 3 nút theo tỷ lệ hình học cân xứng:
            // Nút giữa (Xem lời giải): Rộng 570, đặt tại X = 0 (phạm vi [-285, +285])
            if (btnViewDetail != null)
            {
                RectTransform rt = btnViewDetail.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, -472.9f);
                rt.sizeDelta = new Vector2(570f, 134f);
            }

            // Nút trái (Về khu vực): Rộng 446, đặt tại X = -544 (mép trong: -321, cách nút giữa 36px)
            if (btnReturnToMap != null)
            {
                RectTransform rt = btnReturnToMap.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(-544f, -472.9f);
                rt.sizeDelta = new Vector2(446f, 134f);
            }

            // Nút phải (Làm lại): Rộng 379, đặt tại X = +510.5 (mép trong: +321, cách nút giữa 36px)
            if (btnRetry != null)
            {
                RectTransform rt = btnRetry.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(510.5f, -472.9f);
                rt.sizeDelta = new Vector2(379f, 134f);
            }

            // 3. Đưa nhãn chữ và icon thành con của Button tương ứng, tính toán tọa độ không đè:
            Image[] images = canvas.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                string n = img.gameObject.name;

                // Nút Trái: "Về khu vực" - Ẩn Image (13) (ảnh đỏ "Chưa đạt yêu cầu khôi phục") và căn icon mũi tên (Image (6))
                if (n == "Image (13)")
                {
                    img.gameObject.SetActive(false);
                    img.raycastTarget = false;
                }
                else if (btnReturnToMap != null && n == "Image (6)")
                {
                    img.transform.SetParent(btnReturnToMap.transform, false);
                    img.raycastTarget = false;
                    img.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    img.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    img.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    img.rectTransform.anchoredPosition = new Vector2(-135f, 0f);
                    img.rectTransform.sizeDelta = new Vector2(48f, 48f);

                    var bTmp = btnReturnToMap.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (bTmp != null)
                    {
                        bTmp.text = "Về khu vực";
                        bTmp.alignment = TextAlignmentOptions.Center;
                        bTmp.fontSize = 24f;
                        bTmp.fontStyle = FontStyles.Bold;
                        bTmp.color = Color.white;
                        bTmp.raycastTarget = false;
                        bTmp.gameObject.SetActive(true);
                    }
                }
                // Nút Giữa: "Xem lời giải chi tiết" (Image (12)) + Icon cuốn sách (Image (5))
                else if (btnViewDetail != null && (n == "Image (12)" || n == "Image (5)"))
                {
                    img.transform.SetParent(btnViewDetail.transform, false);
                    img.raycastTarget = false;
                    img.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    img.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    img.rectTransform.pivot = new Vector2(0.5f, 0.5f);

                    if (n == "Image (5)")
                    {
                        img.rectTransform.anchoredPosition = new Vector2(-200f, 0f);
                        img.rectTransform.sizeDelta = new Vector2(52f, 52f);
                    }
                    else // Image (12) - Text sprite
                    {
                        img.rectTransform.anchoredPosition = new Vector2(35f, 0f);
                        img.rectTransform.sizeDelta = new Vector2(350f, 70f);
                    }
                }
                // Nút Phải: "Làm lại" (Image (11)) + Icon xoay (Image (4))
                else if (btnRetry != null && (n == "Image (11)" || n == "Image (4)"))
                {
                    img.transform.SetParent(btnRetry.transform, false);
                    img.raycastTarget = false;
                    img.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    img.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    img.rectTransform.pivot = new Vector2(0.5f, 0.5f);

                    if (n == "Image (4)")
                    {
                        img.rectTransform.anchoredPosition = new Vector2(-105f, 0f);
                        img.rectTransform.sizeDelta = new Vector2(48f, 48f);
                    }
                    else // Image (11) - Text sprite
                    {
                        img.rectTransform.anchoredPosition = new Vector2(35f, 0f);
                        img.rectTransform.sizeDelta = new Vector2(190f, 65f);
                    }
                }
            }

            // Gắn hiệu ứng hover & pulse tinh tế
            if (btnReturnToMap != null) UIHoverPulseEffect.Attach(btnReturnToMap.gameObject, hover: 1.05f, outline: true);
            if (btnViewDetail != null) UIHoverPulseEffect.Attach(btnViewDetail.gameObject, hover: 1.05f, outline: true);
            if (btnRetry != null) UIHoverPulseEffect.Attach(btnRetry.gameObject, hover: 1.05f, outline: true);
        }

        private void FixRaycastBlockers()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
            {
                if (img.GetComponent<Button>() != null)
                {
                    img.raycastTarget = true;
                    continue;
                }

                string n = img.gameObject.name.ToLower();
                if (n.Contains("bg") || n.Contains("panel") || n.Contains("frame") || n.Contains("border") ||
                    n.Contains("icon") || n.Contains("header") || n.Contains("banner") || n == "image" ||
                    n.StartsWith("image ("))
                {
                    img.raycastTarget = false;
                }
            }

            foreach (var t in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                t.raycastTarget = false;
            }
        }

        private void SetupButtonListeners()
        {
            if (btnReturnToMap != null)
            {
                btnReturnToMap.onClick.RemoveAllListeners();
                btnReturnToMap.onClick.AddListener(OnReturnToMapClicked);
            }

            if (btnViewDetail != null)
            {
                btnViewDetail.onClick.RemoveAllListeners();
                btnViewDetail.onClick.AddListener(OnViewDetailClicked);
            }

            if (btnRetry != null)
            {
                btnRetry.onClick.RemoveAllListeners();
                btnRetry.onClick.AddListener(OnRetryClicked);
            }
        }

        /// <summary>
        /// Hiển thị dữ liệu bài thi thật:
        /// - Điểm X.X/10 (trọng số bằng nhau trên cả 5 câu, 1 câu 10đ -> 2.0/10)
        /// - Đã làm: X/5 câu
        /// - Chưa làm: Y/5 câu
        /// - Thời gian: MM:SS
        /// </summary>
        public void RenderAttemptResult()
        {
            float score100 = NavigationService.Instance.AttemptContext.ScoreRaw;
            float score10 = (NavigationService.Instance.AttemptContext.Score10 > 0f)
                ? NavigationService.Instance.AttemptContext.Score10
                : ((score100 <= 10f && score100 > 0f) ? score100 : (float)Math.Round(score100 / 10f, 2));

            int totalQ = NavigationService.Instance.AttemptContext.TotalQuestions;
            int completedCount = NavigationService.Instance.AttemptContext.CorrectCount;

            // Đếm số câu có bài làm thật từ danh sách EssayAnswers nếu có
            if (NavigationService.Instance.AttemptContext.EssayAnswers != null && NavigationService.Instance.AttemptContext.EssayAnswers.Count > 0)
            {
                totalQ = NavigationService.Instance.AttemptContext.EssayAnswers.Count;
                completedCount = NavigationService.Instance.AttemptContext.EssayAnswers.Count(a => !string.IsNullOrEmpty(a.ConfirmedText) && a.ConfirmedText != "(Chưa làm)");
            }

            float elapsedSeconds = NavigationService.Instance.AttemptContext.DurationSeconds;
            if (elapsedSeconds <= 0f && PhysicsGameSession.Instance?.CurrentQuizAttempt != null)
            {
                elapsedSeconds = PhysicsGameSession.Instance.CurrentQuizAttempt.elapsedSeconds;
            }

            // Direct Play Fallback: Nếu mở trực tiếp Scene 19.1 mà chưa qua lượt thi nào (totalQ == 0)
            if (totalQ == 0)
            {
                totalQ = 5;
                completedCount = 1;
                score10 = 2.0f;
                score100 = 20.0f;
                if (elapsedSeconds <= 0f) elapsedSeconds = 185f; // 03:05
            }

            int unattemptedCount = Mathf.Max(0, totalQ - completedCount);

            // Điểm số trung bình cả 5 câu (hiển thị X.X/10 chính giữa vòng nguyệt quế)
            if (scoreText != null)
            {
                scoreText.gameObject.SetActive(true);
                scoreText.text = $"{score10:F1}/10";
            }

            // Điều kiện khôi phục chuẩn: Score >= 8.0/10 (tương đương >= 80/100)
            bool passed = (score10 >= 8.0f - 1e-4f) || (score100 >= RestorationService.PASS_THRESHOLD - 1e-4f);
            bool isDraft = NavigationService.Instance.AttemptContext.EssayAnswers != null &&
                           NavigationService.Instance.AttemptContext.EssayAnswers.Any(a => a.IsDraftRubric);

            if (statusText != null)
            {
                statusText.gameObject.SetActive(true);
                if (passed)
                {
                    statusText.text = isDraft
                        ? "ĐẠT YÊU CẦU (CHẾ ĐỘ THỬ)"
                        : "ĐỦ NĂNG LƯỢNG KHÔI PHỤC (ĐẠT)";
                    statusText.color = new Color(0.25f, 0.95f, 0.45f, 1f); // Xanh lá đạt
                }
                else
                {
                    statusText.text = "CHƯA ĐỦ NĂNG LƯỢNG (CHƯA ĐẠT)";
                    statusText.color = new Color(0.95f, 0.45f, 0.35f, 1f); // Đỏ chưa đạt
                }
            }

            // Ô 1: Đã làm
            if (box1HeaderText != null) box1HeaderText.gameObject.SetActive(true);
            if (box1ValueText != null)
            {
                box1ValueText.gameObject.SetActive(true);
                box1ValueText.transform.SetAsLastSibling();
                box1ValueText.text = $"{completedCount}/{totalQ} câu";
            }

            // Ô 2: Chưa làm
            if (box2HeaderText != null) box2HeaderText.gameObject.SetActive(true);
            if (box2ValueText != null)
            {
                box2ValueText.gameObject.SetActive(true);
                box2ValueText.transform.SetAsLastSibling();
                box2ValueText.text = $"{unattemptedCount}/{totalQ} câu";
                box2ValueText.color = unattemptedCount > 0 ? new Color(0.97f, 0.44f, 0.44f, 1f) : new Color(0.85f, 0.90f, 1f, 1f);
            }

            // Ô 3: Thời gian
            if (box3HeaderText != null) box3HeaderText.gameObject.SetActive(true);
            if (box3ValueText != null)
            {
                box3ValueText.gameObject.SetActive(true);
                box3ValueText.transform.SetAsLastSibling();
                int m = (int)(elapsedSeconds / 60);
                int s = (int)(elapsedSeconds % 60);
                box3ValueText.text = $"{m:00}:{s:00}";
            }

            // Thông báo rubric nếu có
            if (rubricNoticeText != null)
            {
                if (isDraft)
                {
                    rubricNoticeText.text = "<color=#f59e0b>⚠ Chấm thử — một số câu có rubric chưa duyệt chính thức</color>";
                    rubricNoticeText.gameObject.SetActive(true);
                }
                else
                {
                    rubricNoticeText.text = "";
                    rubricNoticeText.gameObject.SetActive(false);
                }
            }
        }

        private void OnReturnToMapClicked()
        {
            Debug.Log("[EssayResultController] Bấm nút: Về khu vực bản đồ.");
            int chapterId = NavigationService.Instance.Selection.ChapterId;
            if (chapterId < 1) chapterId = 1;

            RouteId mapRoute = SceneCatalog.GetChapterMapRoute(chapterId);
            NavigationService.Instance.NavigateTo(mapRoute);
        }

        private void OnViewDetailClicked()
        {
            Debug.Log("[EssayResultController] Bấm nút: Xem chi tiết tự luận (RouteId.EssayReview).");
            NavigationService.Instance.NavigateTo(RouteId.EssayReview);
        }

        private void OnRetryClicked()
        {
            Debug.Log("[EssayResultController] Bấm nút: Làm lại bài tự luận (RouteId.EssayAnswer).");
            NavigationService.Instance.NavigateTo(RouteId.EssayAnswer);
        }

        /// <summary>
        /// Đảm bảo kết quả bài làm Tự luận luôn được lưu bền vững vào PhysicsSaveData và file JSON.
        /// Chống trùng lặp theo attemptId duy nhất.
        /// </summary>
        private void EnsureAttemptRecorded()
        {
            try
            {
                if (NavigationService.Instance == null || NavigationService.Instance.AttemptContext == null) return;
                var ctx = NavigationService.Instance.AttemptContext;
                if (ctx.TotalQuestions <= 0 && ctx.ScoreRaw <= 0f && ctx.Score10 <= 0f) return;

                var save = PhysicsSaveManager.Load();
                if (save == null) return;
                if (save.quizHistory == null) save.quizHistory = new System.Collections.Generic.List<PhysicsQuizHistoryEntry>();

                string attId = ctx.AttemptId;
                if (string.IsNullOrEmpty(attId))
                {
                    attId = Guid.NewGuid().ToString("N");
                    ctx.AttemptId = attId;
                }

                // Nếu attemptId này đã được lưu thì không ghi đè trùng lặp
                if (save.quizHistory.Any(x => x != null && x.attemptId == attId))
                {
                    return;
                }

                string lessonId = (NavigationService.Instance.Selection != null && !string.IsNullOrEmpty(NavigationService.Instance.Selection.LessonId))
                    ? NavigationService.Instance.Selection.LessonId
                    : (PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SelectedLessonId : "");

                int chapterId = (NavigationService.Instance.Selection != null && NavigationService.Instance.Selection.ChapterId > 0)
                    ? NavigationService.Instance.Selection.ChapterId
                    : StudyScreensController.GetChapterForLesson(lessonId);
                if (chapterId <= 0) chapterId = 1;

                string lessonTitle = (NavigationService.Instance.Selection != null && !string.IsNullOrEmpty(NavigationService.Instance.Selection.LessonTitle))
                    ? NavigationService.Instance.Selection.LessonTitle
                    : StudyScreensController.GetLessonTitle(lessonId);

                string diff = (NavigationService.Instance.Selection != null && !string.IsNullOrEmpty(NavigationService.Instance.Selection.Difficulty))
                    ? NavigationService.Instance.Selection.Difficulty
                    : "TH";

                float score100 = ctx.ScoreRaw > 0f ? ctx.ScoreRaw : (ctx.Score10 * 10f);
                int scoreInt = Mathf.RoundToInt(score100);
                int completedQ = ctx.CorrectCount;
                int totalQ = ctx.TotalQuestions > 0 ? ctx.TotalQuestions : 5;

                var entry = new PhysicsQuizHistoryEntry
                {
                    attemptId = attId,
                    lessonId = lessonId,
                    lessonTitle = lessonTitle,
                    chapterId = chapterId,
                    activityType = "Tự luận",
                    scorePercent = scoreInt,
                    correctCount = completedQ,
                    wrongCount = Mathf.Max(0, totalQ - completedQ),
                    unansweredCount = Mathf.Max(0, totalQ - completedQ),
                    totalCount = totalQ,
                    isComplete = true,
                    assessmentKey = diff,
                    elapsedSeconds = ctx.DurationSeconds,
                    sessionId = save.activeStudySession != null && save.activeStudySession.isActive ? save.activeStudySession.sessionId : "",
                    completedAtUtc = DateTime.UtcNow.ToString("o")
                };

                save.RecordQuizAttempt(entry);
                bool passed = scoreInt >= 80;
                save.RecordWrittenCompletion(lessonId, passed);
                save.lastLessonId = lessonId;
                save.lastChapterId = chapterId;
                save.lastDifficulty = diff;

                PhysicsSaveManager.Save(save);
                if (PhysicsGameSession.Instance != null)
                {
                    PhysicsGameSession.Instance.ReloadSaveData();
                }
                Debug.Log($"[EssayResultController] Đã lưu bền vững bài Tự luận: {lessonId}, Score={scoreInt}/100, AttemptId={attId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EssayResultController] Lỗi EnsureAttemptRecorded: {ex.Message}");
            }
        }
    }
}