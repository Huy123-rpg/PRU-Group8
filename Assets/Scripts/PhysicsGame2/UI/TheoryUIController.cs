using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển logic cho Scene 18. Theory (Xem lý thuyết bài học):
    /// - Hiển thị bài học và chương từ PhysicsGameSession.
    /// - Quản lý phân trang (3–5 trang/bài) cho nội dung lý thuyết và ghi nhớ.
    /// - Nút Quay lại: Ở trang 1 quay về 17. Type; ở các trang sau lùi về trang trước.
    /// - Nút Tiếp theo: Lật sang trang kế tiếp; ở trang cuối đổi thành "VÀO MÔ PHỎNG" dẫn tới Scene Gameplay tương ứng.
    /// - Tự động liên kết các thành phần UI trong scene thật mà không làm thay đổi layout hay asset đã dựng.
    /// </summary>
    public class TheoryUIController : MonoBehaviour
    {
        [Header("=== TEXT REFERENCES (Auto-bind if null) ===")]
        [SerializeField] private TextMeshProUGUI textLessonTitle;
        [SerializeField] private TextMeshProUGUI textChapterTitle;
        [SerializeField] private TextMeshProUGUI textBody;
        [SerializeField] private TextMeshProUGUI textRemember;

        [Header("=== BUTTON REFERENCES (Auto-bind if null) ===")]
        [SerializeField] private Button btnBack;
        [SerializeField] private TextMeshProUGUI textBtnBack;
        [SerializeField] private Button btnNext;
        [SerializeField] private TextMeshProUGUI textBtnNext;

        private TheoryLessonData _currentLessonData;
        private int _currentPageIndex = 0;

        private void Awake()
        {
            var existing = FindObjectsByType<TheoryUIController>();
            if (existing.Length > 1)
            {
                Destroy(this);
                return;
            }

            EnsureEventSystem();
            EnsureSafeDevSession();
            AutoBindUIElements();
            FixRaycastBlockers();
        }

        private void Start()
        {
            LoadLessonTheory();
            SetupButtonEvents();
            RenderPage();
        }

        /// <summary>
        /// Direct Scene Safety: Đồng bộ dữ liệu bài học và chương từ NavigationService/PhysicsGameSession.
        /// </summary>
        private void EnsureSafeDevSession()
        {
            string navLessonId = NavigationService.Instance?.Selection?.LessonId;
            int navChapterId = NavigationService.Instance != null ? NavigationService.Instance.Selection.ChapterId : 0;
            string navTitle = NavigationService.Instance?.Selection?.LessonTitle;

            if (!string.IsNullOrEmpty(navLessonId) && PhysicsGameSession.Instance != null)
            {
                if (navChapterId > 0) PhysicsGameSession.Instance.SelectChapter(navChapterId);
                PhysicsGameSession.Instance.SelectLesson(navLessonId, navTitle);
                return;
            }

            // Standalone dev mode: không ép buộc bài học của chương khác
            if (PhysicsGameSession.Instance != null && string.IsNullOrEmpty(PhysicsGameSession.Instance.SelectedLessonId))
            {
                Debug.Log("[TheoryUIController] Dev standalone mode: Chưa có bài học được chọn.");
            }
        }

        private void EnsureEventSystem()
        {
            EventSystem es = FindAnyObjectByType<EventSystem>();
            if (es != null)
            {
                if (es.GetComponent<InputSystemUIInputModule>() == null)
                {
                    es.gameObject.AddComponent<InputSystemUIInputModule>();
                }
            }
            else
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }
        }

        /// <summary>
        /// Tự động tìm và liên kết chính xác các UI GameObject đã dựng sẵn trong Scene 18.
        /// </summary>
        private void AutoBindUIElements()
        {
            // 1. Tìm các Button
            Button[] buttons = FindObjectsByType<Button>();
            foreach (var b in buttons)
            {
                RectTransform rt = b.GetComponent<RectTransform>();
                if (rt == null) continue;

                Vector2 pos = rt.anchoredPosition;

                // Nút QUAY LẠI ở góc trên bên trái: x < -500, y > 350
                if (pos.x < -500f && pos.y > 350f && btnBack == null)
                {
                    btnBack = b;
                    textBtnBack = b.GetComponentInChildren<TextMeshProUGUI>();
                }
                // Nút TIẾP THEO ở góc trên bên phải: x > 500, y > 350
                else if (pos.x > 500f && pos.y > 350f && btnNext == null)
                {
                    btnNext = b;
                    textBtnNext = b.GetComponentInChildren<TextMeshProUGUI>();
                }
                // Khung Tiêu đề bài học: pos y ~ 258
                else if (pos.y > 200f && pos.y < 300f && textLessonTitle == null)
                {
                    textLessonTitle = b.GetComponentInChildren<TextMeshProUGUI>();
                }
                // Khung Tiêu đề chương: pos y ~ 151
                else if (pos.y > 100f && pos.y < 200f && textChapterTitle == null)
                {
                    textChapterTitle = b.GetComponentInChildren<TextMeshProUGUI>();
                }
            }

            // 2. Tìm các TextMeshProUGUI
            TextMeshProUGUI[] tmps = FindObjectsByType<TextMeshProUGUI>();
            foreach (var t in tmps)
            {
                string raw = t.text;
                // Text lý thuyết bên trái (placeholder chứa "Lý thuyết nè..." hoặc x < -100)
                if (textBody == null && (raw.Contains("Lý thuyết") || (t.GetComponentInParent<Button>() == null && t.rectTransform.anchoredPosition.x < -100f)))
                {
                    textBody = t;
                }
                // Text ghi chú bên phải (placeholder chứa "Ghi chú nè" hoặc x > 100)
                else if (textRemember == null && (raw.Contains("Ghi chú") || (t.GetComponentInParent<Button>() == null && t.rectTransform.anchoredPosition.x > 100f)))
                {
                    textRemember = t;
                }
            }

            // Tối ưu RectTransform & Typography cho text hiển thị đẹp mắt, không đè dải băng tiêu đề, chữ to rõ ràng
            // Bảng trái (Nội dung lý thuyết): Neo mép trên y = -100 (cách dải băng tiêu đề ~50px), cao 370px, chữ to 22-28pt
            ConfigureTextTypography(textBody, new Vector2(-400f, -100f), new Vector2(810f, 370f), 22f, 28f, 26f, 8f);

            // Bảng phải (Ghi nhớ): Neo mép trên y = -235 (nằm hoàn toàn dưới dải băng "GHI NHỚ"), cao 230px, chữ to 20-26pt
            ConfigureTextTypography(textRemember, new Vector2(555f, -235f), new Vector2(560f, 230f), 20f, 26f, 24f, 10f);
        }

        private void ConfigureTextTypography(TextMeshProUGUI tmp, Vector2 topCenterPos, Vector2 size, float minSize, float maxSize, float defaultSize, float lineSpacing = 8f)
        {
            if (tmp == null) return;
            RectTransform rt = tmp.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 1f); // Neo ở mép trên cùng của vùng nội dung để không bao giờ bị lệch khi đổi font/size
            rt.anchoredPosition = topCenterPos;
            rt.sizeDelta = size;

            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = minSize;
            tmp.fontSizeMax = maxSize;
            tmp.fontSize = defaultSize;
            tmp.lineSpacing = lineSpacing;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            // Màu mực xanh đen sẫm (Charcoal Navy Ink) tương phản cao trên nền giấy da vàng kem (Parchment scroll)
            tmp.color = new Color(0.10f, 0.15f, 0.24f, 1f);
        }

        /// <summary>
        /// Đảm bảo các Image hình nền, khung viền không cản trở raycast (click) của các nút.
        /// </summary>
        private void FixRaycastBlockers()
        {
            Image[] allImages = FindObjectsByType<Image>();
            foreach (var img in allImages)
            {
                if (img.GetComponent<Button>() == null)
                {
                    img.raycastTarget = false;
                }
            }

            TextMeshProUGUI[] tmps = FindObjectsByType<TextMeshProUGUI>();
            foreach (var t in tmps)
            {
                if (t.GetComponentInParent<Button>() != null)
                {
                    t.raycastTarget = false;
                }
            }
        }

        private void LoadLessonTheory()
        {
            string lessonId = !string.IsNullOrEmpty(NavigationService.Instance?.Selection?.LessonId)
                ? NavigationService.Instance.Selection.LessonId
                : (PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SelectedLessonId : "");

            if (PhysicsGameSession.Instance != null && !string.IsNullOrEmpty(lessonId))
            {
                string title = NavigationService.Instance?.Selection?.LessonTitle ?? "";
                PhysicsGameSession.Instance.SelectLesson(lessonId, title);
            }

            _currentLessonData = TheoryRepository.GetLesson(lessonId);

            // TUYỆT ĐỐI KHÔNG DÙNG BÀI CỦA CHƯƠNG KHÁC LÀM FALLBACK.
            // Nếu không tìm thấy lý thuyết, để _currentLessonData = null để RenderPage() hiển thị thông báo rõ ràng.
            if (_currentLessonData == null)
            {
                Debug.LogWarning($"[TheoryUIController] Không tìm thấy lý thuyết cho '{lessonId}'. Giữ thông báo chưa có nội dung, không tráo bài khác.");
            }

            _currentPageIndex = 0;
        }

        private void SetupButtonEvents()
        {
            if (btnBack != null)
            {
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackClicked);
                var fb = btnBack.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);
                UIHoverPulseEffect.Attach(btnBack.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
            }

            if (btnNext != null)
            {
                btnNext.onClick.RemoveAllListeners();
                btnNext.onClick.AddListener(OnNextClicked);
                var fb = btnNext.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);
                UIHoverPulseEffect.Attach(btnNext.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
            }
        }

        private void RenderPage()
        {
            // Trường hợp không tìm thấy dữ liệu lý thuyết (Unknown lesson ID hoặc bài không có theory)
            if (_currentLessonData == null || _currentLessonData.pages == null || _currentLessonData.pages.Count == 0)
            {
                if (textLessonTitle != null)
                {
                    string lessonTitle = !string.IsNullOrEmpty(NavigationService.Instance?.Selection?.LessonTitle)
                        ? NavigationService.Instance.Selection.LessonTitle
                        : (PhysicsGameSession.Instance != null && !string.IsNullOrEmpty(PhysicsGameSession.Instance.SelectedLessonTitle)
                            ? PhysicsGameSession.Instance.SelectedLessonTitle
                            : (PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SelectedLessonId : "Chưa chọn bài"));
                    textLessonTitle.text = lessonTitle;
                }

                if (textChapterTitle != null)
                {
                    int ch = NavigationService.Instance != null && NavigationService.Instance.Selection.ChapterId > 0
                        ? NavigationService.Instance.Selection.ChapterId
                        : (PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SelectedChapterId : 0);
                    textChapterTitle.text = ch > 0 ? $"Chương {ch}" : "";
                }

                if (textBody != null)
                {
                    textBody.text = "<b><color=#C53030>THÔNG BÁO</color></b>\n\nChưa có nội dung lý thuyết cho bài học này.\nVui lòng bấm 'Quay lại' để chọn bài học khác.";
                }

                if (textRemember != null)
                {
                    textRemember.text = "";
                }

                // Vô hiệu hóa nút TIẾP THEO
                if (btnNext != null)
                {
                    btnNext.interactable = false;
                    var cg = btnNext.GetComponent<CanvasGroup>();
                    if (cg == null) cg = btnNext.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 0.35f;
                    if (textBtnNext != null) textBtnNext.gameObject.SetActive(false);
                }

                // Nút QUAY LẠI vẫn cho phép quay về 17. Type
                if (btnBack != null)
                {
                    btnBack.interactable = true;
                    var cg = btnBack.GetComponent<CanvasGroup>();
                    if (cg != null) cg.alpha = 1f;
                    if (textBtnBack != null) textBtnBack.gameObject.SetActive(false);
                }
                return;
            }

            // Dữ liệu hợp lệ: kích hoạt lại các nút
            if (btnNext != null)
            {
                btnNext.interactable = true;
                var cg = btnNext.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
            }

            if (btnBack != null)
            {
                btnBack.interactable = true;
                var cg = btnBack.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
            }

            int totalPages = _currentLessonData.pages.Count;
            _currentPageIndex = Mathf.Clamp(_currentPageIndex, 0, totalPages - 1);
            var page = _currentLessonData.pages[_currentPageIndex];

            // 1. Render Headers
            if (textLessonTitle != null)
            {
                textLessonTitle.enableAutoSizing = true;
                textLessonTitle.fontSizeMin = 18f;
                textLessonTitle.fontSizeMax = 28f;
                textLessonTitle.text = _currentLessonData.lessonTitle;
            }
            if (textChapterTitle != null)
            {
                textChapterTitle.text = (totalPages > 1)
                    ? $"{_currentLessonData.chapterTitle}  •  Trang {_currentPageIndex + 1}/{totalPages}"
                    : _currentLessonData.chapterTitle;
            }

            // 2. Render Left Panel (Nội dung lý thuyết)
            if (textBody != null)
            {
                textBody.color = new Color(0.10f, 0.15f, 0.24f, 1f);
                textBody.text = $"<size=125%><color=#0A3678><b>{page.heading}</b></color></size>\n\n{page.body}";
            }

            // 3. Render Right Panel (Ghi nhớ)
            if (textRemember != null)
            {
                textRemember.color = new Color(0.10f, 0.15f, 0.24f, 1f);
                textRemember.text = page.rememberText;
            }

            // 4. Giữ nguyên chữ thư pháp của Sprite nút Quay lại / Tiếp theo, không in đè text
            if (textBtnBack != null)
            {
                textBtnBack.text = "";
                textBtnBack.gameObject.SetActive(false);
            }

            if (textBtnNext != null)
            {
                textBtnNext.text = "";
                textBtnNext.gameObject.SetActive(false);
            }
        }

        private void OnBackClicked()
        {
            if (_currentLessonData == null || _currentPageIndex == 0)
            {
                // Ở trang đầu hoặc trạng thái không có lý thuyết: Quay về ModeSelection (17. Type)
                Debug.Log("[TheoryUIController] Quay lại RouteId.ModeSelection");
                NavigationService.Instance.NavigateTo(RouteId.ModeSelection);
            }
            else
            {
                // Ở trang > 0: Lùi về trang trước, không chuyển scene
                _currentPageIndex--;
                RenderPage();
            }
        }

        private void OnNextClicked()
        {
            if (_currentLessonData == null || _currentLessonData.pages == null || _currentLessonData.pages.Count == 0)
            {
                return; // Disabled trong error state
            }

            int totalPages = _currentLessonData.pages.Count;

            if (_currentPageIndex < totalPages - 1)
            {
                // Sang trang tiếp theo
                _currentPageIndex++;
                RenderPage();
            }
            else
            {
                // Trang cuối: Giữ người học ở lại trang lý thuyết, tuyệt đối KHÔNG tự ý chuyển sang chọn độ khó hay trắc nghiệm.
                // Người học chủ động nhấn nút Quay lại khi đã sẵn sàng.
                Debug.Log("[TheoryUIController] Đã ở trang cuối của phần lý thuyết. Giữ nguyên màn hình.");
            }
        }
    }
}
