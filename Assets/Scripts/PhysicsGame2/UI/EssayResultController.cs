using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển logic giao diện Scene 19.1. Result_TL (Màn kết quả Tự luận)
    /// Khắc phục toàn diện các lỗi:
    /// 1. Nút bấm: Căn chỉnh 3 nút hoàn toàn đối xứng (khoảng cách 36px cân đối),
    ///    đưa nhãn chữ và icon thành con của nút, tắt triệt để raycastTarget trên các object trang trí
    ///    để đảm bảo bấm được 100% vào màn khác.
    /// 2. Tiêu đề: Giữ nguyên banner hoàng gia "KẾT QUẢ TỰ LUẬN", không chèn đè text chữ xanh lên tiêu đề.
    /// 3. Điểm số: Tính đúng trung bình 5 câu (1 câu đúng 10đ -> 2.0/10), không rớt dòng.
    /// 4. Trạng thái: Nằm đúng bên dưới chữ ĐIỂM ("CHƯA ĐỦ NĂNG LƯỢNG (CHƯA ĐẠT)" hoặc "ĐỦ NĂNG LƯỢNG KHÔI PHỤC (ĐẠT)").
    /// 5. 3 ô thống kê: Đối xứng, căn giữa cột chữ (ĐÃ LÀM, CHƯA LÀM, THỜI GIAN) kèm icon tương ứng.
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
            RenderAttemptResult();
        }

        private void OnEnable()
        {
            AutoBindUIReferences();
            SetupStatBoxHeaders();
            AlignAndOrganizeButtons();
            FixRaycastBlockers();
            SetupButtonListeners();
            RenderAttemptResult();
        }

        private void Start()
        {
            RenderAttemptResult();
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

        private void AutoBindUIReferences()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. Tiêu đề banner hoàng gia "KẾT QUẢ TỰ LUẬN"
            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
            {
                Vector2 pos = img.rectTransform.anchoredPosition;
                if (pos.y > 380f && Mathf.Abs(pos.x) < 80f)
                {
                    titleBannerImage = img;
                    img.rectTransform.sizeDelta = new Vector2(880f, 168.4f);
                    img.raycastTarget = false;

                    // Xóa triệt để Text con nếu trước đó từng tạo chèn đè lên chữ thư pháp vàng
                    Transform existingTitle = img.transform.Find("Title_Text");
                    if (existingTitle != null)
                    {
                        Destroy(existingTitle.gameObject);
                    }
                    break;
                }
            }

            // 2. Tìm và cấu hình các ô Text
            TextMeshProUGUI[] tmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                Vector2 pos = t.rectTransform.anchoredPosition;
                string text = t.text != null ? t.text.Trim() : "";

                // Dọn dẹp số 8 rác bên phải nếu có
                if (pos.x > 400f && pos.x < 650f && Mathf.Abs(pos.y - -10f) < 30f && text == "8")
                {
                    t.text = "";
                    t.gameObject.SetActive(false);
                    continue;
                }

                // Dọn dẹp text rác trôi nổi ở đáy màn hình
                if (pos.y < -350f && t.GetComponentInParent<Button>() == null)
                {
                    t.text = "";
                    t.gameObject.SetActive(false);
                    continue;
                }

                // Điểm số: nằm giữa vòng nguyệt quế (y trong khoảng [160, 260], x quanh tâm 46)
                if (scoreText == null && pos.y > 160f && pos.y < 260f && pos.x > 0f && pos.x < 160f && t.transform.parent == canvas.transform)
                {
                    scoreText = t;
                    t.rectTransform.anchoredPosition = new Vector2(46f, 226f);
                    t.rectTransform.sizeDelta = new Vector2(380f, 65f);
                    t.textWrappingMode = TextWrappingModes.NoWrap;
                    t.overflowMode = TextOverflowModes.Overflow;
                    t.alignment = TextAlignmentOptions.Center;
                    t.fontSize = 46f;
                    t.fontStyle = FontStyles.Bold;
                    t.color = new Color(1f, 0.95f, 0.70f, 1f);
                    t.raycastTarget = false;
                }
                // Trạng thái đạt/chưa đạt: nằm đúng dưới chữ ĐIỂM (y quanh 0, x quanh 46)
                else if (statusText == null && Mathf.Abs(pos.y) < 50f && Mathf.Abs(pos.x - 46f) < 60f && t.transform.parent == canvas.transform)
                {
                    statusText = t;
                    t.rectTransform.anchoredPosition = new Vector2(46f, 0f);
                    t.rectTransform.sizeDelta = new Vector2(560f, 42f);
                    t.textWrappingMode = TextWrappingModes.NoWrap;
                    t.overflowMode = TextOverflowModes.Overflow;
                    t.alignment = TextAlignmentOptions.Center;
                    t.fontSize = 24f;
                    t.fontStyle = FontStyles.Bold;
                    t.raycastTarget = false;
                }
                // Ô 1: Giá trị số câu Đã làm (x < -200, y trong khoảng [-160, -260])
                else if (box1ValueText == null && pos.x < -200f && pos.y < -160f && pos.y > -260f)
                {
                    box1ValueText = t;
                    t.rectTransform.anchoredPosition = new Vector2(-360f, -218f);
                    t.rectTransform.sizeDelta = new Vector2(220f, 50f);
                    t.textWrappingMode = TextWrappingModes.NoWrap;
                    t.overflowMode = TextOverflowModes.Overflow;
                    t.alignment = TextAlignmentOptions.Center;
                    t.fontSize = 36f;
                    t.fontStyle = FontStyles.Bold;
                    t.color = new Color(0.40f, 0.95f, 0.55f, 1f);
                    t.raycastTarget = false;
                }
                // Ô 2: Giá trị số câu Chưa làm (x trong khoảng [50, 260], y trong khoảng [-160, -260])
                else if (box2ValueText == null && pos.x > 50f && pos.x < 260f && pos.y < -160f && pos.y > -260f)
                {
                    box2ValueText = t;
                    t.rectTransform.anchoredPosition = new Vector2(130f, -218f);
                    t.rectTransform.sizeDelta = new Vector2(220f, 50f);
                    t.textWrappingMode = TextWrappingModes.NoWrap;
                    t.overflowMode = TextOverflowModes.Overflow;
                    t.alignment = TextAlignmentOptions.Center;
                    t.fontSize = 36f;
                    t.fontStyle = FontStyles.Bold;
                    t.raycastTarget = false;
                }
                // Ô 3: Thời gian làm bài (x > 450, y trong khoảng [-160, -260])
                else if (box3ValueText == null && pos.x > 450f && pos.y < -160f && pos.y > -260f)
                {
                    box3ValueText = t;
                    t.rectTransform.anchoredPosition = new Vector2(600f, -218f);
                    t.rectTransform.sizeDelta = new Vector2(220f, 50f);
                    t.textWrappingMode = TextWrappingModes.NoWrap;
                    t.overflowMode = TextOverflowModes.Overflow;
                    t.alignment = TextAlignmentOptions.Center;
                    t.fontSize = 36f;
                    t.fontStyle = FontStyles.Bold;
                    t.color = new Color(1f, 0.95f, 0.70f, 1f);
                    t.raycastTarget = false;
                }
            }

            // 3. Thông báo rubric nếu chưa có
            if (rubricNoticeText == null)
            {
                Transform existingNotice = canvas.transform.Find("RubricNoticeText");
                if (existingNotice != null)
                {
                    rubricNoticeText = existingNotice.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                    GameObject noticeGo = new GameObject("RubricNoticeText");
                    noticeGo.transform.SetParent(canvas.transform, false);
                    rubricNoticeText = noticeGo.AddComponent<TextMeshProUGUI>();
                    rubricNoticeText.fontSize = 18;
                    rubricNoticeText.fontStyle = FontStyles.Bold;
                    rubricNoticeText.alignment = TextAlignmentOptions.Center;
                    rubricNoticeText.raycastTarget = false;

                    RectTransform rt = noticeGo.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(46f, 155f);
                    rt.sizeDelta = new Vector2(700f, 35f);
                }
            }
        }

        private void SetupStatBoxHeaders()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Ẩn các sprite tiêu đề trắc nghiệm cũ ("Câu đúng" = Image (8), "Câu sai" = Image (9))
            // Nhưng bảo toàn các icon Checkmark, Cross, Clock ở bên trái của từng ô
            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
            {
                string n = img.gameObject.name;
                if (n == "Image (8)" || n == "Image (9)")
                {
                    img.gameObject.SetActive(false);
                }
                // Đảm bảo icon ô 1 (Checkmark), icon ô 2 (Cross), icon ô 3 (Clock) active và không cản raycast
                else if (n == "Image" && img.rectTransform.anchoredPosition.x < -400f)
                {
                    img.gameObject.SetActive(true);
                    img.raycastTarget = false;
                    img.rectTransform.anchoredPosition = new Vector2(-540f, -180f);
                    img.rectTransform.sizeDelta = new Vector2(90f, 90f);
                }
                else if (n == "Image (2)" && img.rectTransform.anchoredPosition.x > 0f)
                {
                    img.gameObject.SetActive(true);
                    img.raycastTarget = false;
                    img.rectTransform.anchoredPosition = new Vector2(-50f, -180f);
                    img.rectTransform.sizeDelta = new Vector2(90f, 90f);
                }
                else if (n == "Image (3)")
                {
                    img.gameObject.SetActive(true);
                    img.raycastTarget = false;
                    img.rectTransform.anchoredPosition = new Vector2(420f, -180f);
                    img.rectTransform.sizeDelta = new Vector2(90f, 90f);
                }
            }

            // Tiêu đề Ô 1: "ĐÃ LÀM" (Căn giữa cột chữ ô 1 tại X = -360)
            Transform oldH1 = canvas.transform.Find("Header_DaLam");
            if (oldH1 == null)
            {
                GameObject h1Go = new GameObject("Header_DaLam");
                h1Go.transform.SetParent(canvas.transform, false);
                RectTransform rt = h1Go.AddComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(-360f, -145f);
                rt.sizeDelta = new Vector2(220f, 40f);
                box1HeaderText = h1Go.AddComponent<TextMeshProUGUI>();
                box1HeaderText.text = "ĐÃ LÀM";
                box1HeaderText.fontSize = 22f;
                box1HeaderText.fontStyle = FontStyles.Bold;
                box1HeaderText.alignment = TextAlignmentOptions.Center;
                box1HeaderText.color = new Color(0.96f, 0.82f, 0.38f, 1f);
                box1HeaderText.raycastTarget = false;
            }
            else
            {
                box1HeaderText = oldH1.GetComponent<TextMeshProUGUI>();
                box1HeaderText.rectTransform.anchoredPosition = new Vector2(-360f, -145f);
                box1HeaderText.rectTransform.sizeDelta = new Vector2(220f, 40f);
            }

            // Tiêu đề Ô 2: "CHƯA LÀM" (Căn giữa cột chữ ô 2 tại X = 130)
            Transform oldH2 = canvas.transform.Find("Header_ChuaLam");
            if (oldH2 == null)
            {
                GameObject h2Go = new GameObject("Header_ChuaLam");
                h2Go.transform.SetParent(canvas.transform, false);
                RectTransform rt = h2Go.AddComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(130f, -145f);
                rt.sizeDelta = new Vector2(220f, 40f);
                box2HeaderText = h2Go.AddComponent<TextMeshProUGUI>();
                box2HeaderText.text = "CHƯA LÀM";
                box2HeaderText.fontSize = 22f;
                box2HeaderText.fontStyle = FontStyles.Bold;
                box2HeaderText.alignment = TextAlignmentOptions.Center;
                box2HeaderText.color = new Color(0.96f, 0.82f, 0.38f, 1f);
                box2HeaderText.raycastTarget = false;
            }
            else
            {
                box2HeaderText = oldH2.GetComponent<TextMeshProUGUI>();
                box2HeaderText.rectTransform.anchoredPosition = new Vector2(130f, -145f);
                box2HeaderText.rectTransform.sizeDelta = new Vector2(220f, 40f);
            }
        }

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

            // 2. Căn chỉnh 3 nút theo tỷ lệ hình học cân xứng, khoảng cách đều nhau 36px:
            // Nút giữa: Chiều rộng 570px, đặt tại X = 0 (phạm vi [-285, +285])
            if (btnViewDetail != null)
            {
                RectTransform rt = btnViewDetail.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0f, -472.9f);
                rt.sizeDelta = new Vector2(570f, 134f);
            }

            // Nút trái: Chiều rộng 446px, đặt tại X = -544 (mép trong: -321px, cách nút giữa 36px)
            if (btnReturnToMap != null)
            {
                RectTransform rt = btnReturnToMap.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(-544f, -472.9f);
                rt.sizeDelta = new Vector2(446f, 134f);
            }

            // Nút phải: Chiều rộng 379px, đặt tại X = +510.5 (mép trong: +321px, cách nút giữa 36px)
            if (btnRetry != null)
            {
                RectTransform rt = btnRetry.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(510.5f, -472.9f);
                rt.sizeDelta = new Vector2(379f, 134f);
            }

            // 3. Đưa nhãn chữ và icon thành con của Button tương ứng, căn giữa tuyệt đối
            Image[] images = canvas.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                string n = img.gameObject.name;

                // Nút Trái: "Về khu vực" (Image (13)) + Icon mũi tên (Image (6))
                if (btnReturnToMap != null && (n == "Image (13)" || n == "Image (6)"))
                {
                    img.transform.SetParent(btnReturnToMap.transform, false);
                    img.raycastTarget = false;
                    if (n == "Image (6)")
                    {
                        img.rectTransform.anchoredPosition = new Vector2(-100f, 0f);
                    }
                    else // Image (13)
                    {
                        img.rectTransform.anchoredPosition = new Vector2(35f, 0f);
                    }
                }
                // Nút Giữa: "Xem lời giải chi tiết" (Image (12)) + Icon cuốn sách (Image (5))
                else if (btnViewDetail != null && (n == "Image (12)" || n == "Image (5)"))
                {
                    img.transform.SetParent(btnViewDetail.transform, false);
                    img.raycastTarget = false;
                    if (n == "Image (5)")
                    {
                        img.rectTransform.anchoredPosition = new Vector2(-175f, 0f);
                    }
                    else // Image (12)
                    {
                        img.rectTransform.anchoredPosition = new Vector2(30f, 0f);
                    }
                }
                // Nút Phải: "Làm lại" (Image (11)) + Icon xoay (Image (4))
                else if (btnRetry != null && (n == "Image (11)" || n == "Image (4)"))
                {
                    img.transform.SetParent(btnRetry.transform, false);
                    img.raycastTarget = false;
                    if (n == "Image (4)")
                    {
                        img.rectTransform.anchoredPosition = new Vector2(-90f, 0f);
                    }
                    else // Image (11)
                    {
                        img.rectTransform.anchoredPosition = new Vector2(25f, 0f);
                    }
                }
            }
        }

        private void FixRaycastBlockers()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // BẬT raycast duy nhất trên 3 nút điều hướng. TẤT CẢ các Image và Text khác tắt raycastTarget
            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
            {
                Button b = img.GetComponent<Button>();
                bool isNavButton = (b != null && (b == btnReturnToMap || b == btnViewDetail || b == btnRetry));
                img.raycastTarget = isNavButton;
            }

            foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                tmp.raycastTarget = false;
            }
        }

        private void SetupButtonListeners()
        {
            if (btnReturnToMap != null)
            {
                btnReturnToMap.interactable = true;
                btnReturnToMap.onClick.RemoveAllListeners();
                btnReturnToMap.onClick.AddListener(OnReturnToMapClicked);
                UIHoverPulseEffect.Attach(btnReturnToMap.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
            }

            if (btnViewDetail != null)
            {
                btnViewDetail.interactable = true;
                btnViewDetail.onClick.RemoveAllListeners();
                btnViewDetail.onClick.AddListener(OnViewDetailClicked);
                UIHoverPulseEffect.Attach(btnViewDetail.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
            }

            if (btnRetry != null)
            {
                btnRetry.interactable = true;
                btnRetry.onClick.RemoveAllListeners();
                btnRetry.onClick.AddListener(OnRetryClicked);
                UIHoverPulseEffect.Attach(btnRetry.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
            }
        }

        private void RenderAttemptResult()
        {
            float score100 = NavigationService.Instance.AttemptContext.ScoreRaw;
            float score10 = (NavigationService.Instance.AttemptContext.Score10 > 0f)
                ? NavigationService.Instance.AttemptContext.Score10
                : ((score100 <= 10f && score100 > 0f) ? score100 : (float)Math.Round(score100 / 10f, 2));

            int totalQ = NavigationService.Instance.AttemptContext.TotalQuestions > 0 ? NavigationService.Instance.AttemptContext.TotalQuestions : 5;
            int completedCount = NavigationService.Instance.AttemptContext.CorrectCount;

            // Đếm số câu có bài làm thật từ danh sách EssayAnswers nếu có
            if (NavigationService.Instance.AttemptContext.EssayAnswers != null && NavigationService.Instance.AttemptContext.EssayAnswers.Count > 0)
            {
                totalQ = NavigationService.Instance.AttemptContext.EssayAnswers.Count;
                completedCount = NavigationService.Instance.AttemptContext.EssayAnswers.Count(a => a.ConfirmedText != "(Chưa làm)");
            }

            int unattemptedCount = Mathf.Max(0, totalQ - completedCount);
            float elapsedSeconds = NavigationService.Instance.AttemptContext.DurationSeconds;

            if (elapsedSeconds <= 0f && PhysicsGameSession.Instance?.CurrentQuizAttempt != null)
            {
                elapsedSeconds = PhysicsGameSession.Instance.CurrentQuizAttempt.elapsedSeconds;
            }

            // Điểm số trung bình cả 5 câu (hiển thị X.X/10)
            if (scoreText != null)
            {
                scoreText.text = $"{score10:F1}/10";
            }

            // Điều kiện khôi phục chuẩn: ScoreRaw >= 80/100 (tương đương >= 8.0/10)
            bool passed = (score100 >= RestorationService.PASS_THRESHOLD - 1e-4f);
            bool isDraft = NavigationService.Instance.AttemptContext.EssayAnswers != null &&
                           NavigationService.Instance.AttemptContext.EssayAnswers.Any(a => a.IsDraftRubric);

            if (statusText != null)
            {
                if (passed)
                {
                    statusText.text = isDraft
                        ? "ĐẠT YÊU CẦU (CHẾ ĐỘ THỬ)"
                        : "ĐỦ NĂNG LƯỢNG KHÔI PHỤC (ĐẠT)";
                    statusText.color = new Color(0.25f, 0.95f, 0.45f, 1f);
                }
                else
                {
                    statusText.text = "CHƯA ĐỦ NĂNG LƯỢNG (CHƯA ĐẠT)";
                    statusText.color = new Color(0.95f, 0.45f, 0.35f, 1f);
                }
            }

            // Ô 1: Đã làm
            if (box1ValueText != null)
            {
                box1ValueText.text = $"{completedCount}/{totalQ} câu";
            }

            // Ô 2: Chưa làm
            if (box2ValueText != null)
            {
                box2ValueText.text = $"{unattemptedCount}/{totalQ} câu";
                box2ValueText.color = unattemptedCount > 0 ? new Color(0.95f, 0.55f, 0.45f, 1f) : new Color(0.85f, 0.90f, 1f, 1f);
            }

            // Ô 3: Thời gian
            if (box3ValueText != null)
            {
                int m = (int)(elapsedSeconds / 60);
                int s = (int)(elapsedSeconds % 60);
                box3ValueText.text = $"{m:00}:{s:00}";
            }

            // Thông báo rubric
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
    }
}
