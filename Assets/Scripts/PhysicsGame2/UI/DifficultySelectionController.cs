using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    [Serializable]
    public class DifficultyCardUI
    {
        public string levelCode; // "NB", "TH", "VD", "VDC"
        public string levelName; // "Nhận biết", "Thông hiểu", "Vận dụng", "Vận dụng cao"
        public string description; // "Câu hỏi cơ bản", "Hiểu và giải thích", "Áp dụng kiến thức", "Bài tập nâng cao"
        public Button button;
        public Image cardImage;
        public Outline outline;
        public GameObject plaqueObj;
        public TextMeshProUGUI plaqueText;
        public int questionCount;
    }

    /// <summary>
    /// Điều khiển logic cho Scene 33. Chọn độ khó (DifficultySelection).
    /// Đáp ứng tiêu chuẩn Game 2 - Học viện AURELIA:
    /// 1. Đồng bộ Live từ Google Sheets tab NganHangCauHoi / Cauhoituluan, fallback local cache khi offline.
    /// 2. Hiển thị trạng thái "Đang tải ngân hàng câu hỏi..." thay vì báo thiếu câu trước khi tải xong.
    /// 3. Plaque mô tả phong cách Aurelia fantasy dưới 4 thẻ, tuyệt đối không lộ red text / debug text.
    /// 4. Nút lớn "BẮT ĐẦU" ở giữa phía dưới:
    ///    - Chưa chọn độ khó: disable/mờ (alpha ~0.45).
    ///    - Chọn độ khó hợp lệ: sáng rõ, interactable.
    ///    - Click ô độ khó chỉ chọn & highlight, chưa vào bài ngay.
    ///    - Bấm "Bắt đầu" mới chuyển Scene.
    /// 5. In log chẩn đoán chi tiết theo chuẩn:
    ///    ChapterId, LessonId, Mode, Difficulty, DataSource, SyncStatus,
    ///    SheetValidCount, LocalValidCount, FilteredCount, ExcludedReasons, RequiredCount, CanStart.
    /// </summary>
    public class DifficultySelectionController : MonoBehaviour
    {
        [Header("=== SELECTED DIFFICULTY ===")]
        [SerializeField] private string selectedDifficulty = "";

        [Header("=== ACTION BUTTONS ===")]
        [SerializeField] private Button btnStart;
        [SerializeField] private Button btnBack;
        [SerializeField] private CanvasGroup startButtonCanvasGroup;

        [Header("=== AURELIA WARNING MODAL ===")]
        [SerializeField] private GameObject alertModal;
        [SerializeField] private TextMeshProUGUI alertMessageText;

        [Header("=== SYNC STATUS UI ===")]
        [SerializeField] private GameObject syncStatusPlaque;
        [SerializeField] private TextMeshProUGUI syncStatusText;

        private readonly List<DifficultyCardUI> _cards = new List<DifficultyCardUI>();
        private string _lessonId;
        private string _mode;
        private int _chapterId = 1;
        private bool _isSyncing = false;

        private void Awake()
        {
            EnsureEventSystem();
            AutoBindUIElements();
        }

        private void Start()
        {
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.EnsureDevContext();
                _lessonId = NavigationService.Instance.Selection.LessonId;
                _mode = NavigationService.Instance.Selection.Mode;
                _chapterId = NavigationService.Instance.Selection.ChapterId;
            }

            if (string.IsNullOrEmpty(_lessonId)) _lessonId = "bai2";
            if (string.IsNullOrEmpty(_mode)) _mode = "MCQ";
            if (_chapterId <= 0) _chapterId = 1;

            // Đọc cache ban đầu
            RefreshCountsAndPlaques();

            // Trạng thái ban đầu: Chưa chọn mức nào, nút Bắt đầu mờ/disabled
            selectedDifficulty = "";
            foreach (var card in _cards)
            {
                if (card.cardImage != null)
                {
                    card.cardImage.color = Color.white;
                    var hover = card.cardImage.GetComponent<UIHoverPulseEffect>();
                    if (hover != null) hover.SetSelected(false);
                }
            }

            SetStartButtonState(canStart: false);

            // Bắt đầu quá trình đồng bộ Live từ Google Sheets
            StartCoroutine(SyncBankDataRoutine());
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }

        private void AutoBindUIElements()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Xóa bất kỳ NoticeText hoặc nhãn cảnh báo đỏ cũ còn sót lại từ các lần chạy trước
            var oldTransforms = canvas.GetComponentsInChildren<Transform>(true);
            foreach (var t in oldTransforms)
            {
                if (t != null && t.gameObject != null)
                {
                    if (t.name == "NoticeText" || t.name == "CountLabel")
                    {
                        DestroyImmediate(t.gameObject);
                    }
                }
            }

            var images = canvas.GetComponentsInChildren<Image>(true);

            // Tắt raycast background và banner lớn
            foreach (var img in images)
            {
                if (img.rectTransform.sizeDelta.x > 1800f || img.name.Contains("bg") || img.name.Contains("Background"))
                {
                    img.raycastTarget = false;
                }
                else if (img.rectTransform.anchoredPosition.y > 300f && img.rectTransform.sizeDelta.x > 900f)
                {
                    img.raycastTarget = false; // Banner tiêu đề
                }
            }

            // 4 thẻ độ khó xếp ngang theo x: ~ -557, ~ -161, ~ 223, ~ 655
            string[] codes = { "NB", "TH", "VD", "VDC" };
            string[] names = { "Nhận biết", "Thông hiểu", "Vận dụng", "Vận dụng cao" };
            string[] descriptions = { "Câu hỏi cơ bản", "Hiểu và giải thích", "Áp dụng kiến thức", "Bài tập nâng cao" };
            float[] targetXs = { -557f, -161f, 223f, 655f };

            _cards.Clear();
            for (int i = 0; i < 4; i++)
            {
                float targetX = targetXs[i];
                string code = codes[i];
                string name = names[i];
                string desc = descriptions[i];

                // Tìm image thẻ gần vị trí targetX nhất
                Image bestImg = null;
                float bestDist = float.MaxValue;
                foreach (var img in images)
                {
                    if (img.rectTransform.anchoredPosition.y < 100f && img.rectTransform.anchoredPosition.y > -200f)
                    {
                        float d = Mathf.Abs(img.rectTransform.anchoredPosition.x - targetX);
                        if (d < bestDist && img.rectTransform.sizeDelta.x > 150f)
                        {
                            bestDist = d;
                            bestImg = img;
                        }
                    }
                }

                if (bestImg != null)
                {
                    bestImg.raycastTarget = true;
                    var btn = bestImg.GetComponent<Button>();
                    if (btn == null) btn = bestImg.gameObject.AddComponent<Button>();
                    btn.targetGraphic = bestImg;

                    var outline = bestImg.GetComponent<Outline>();
                    if (outline == null) outline = bestImg.gameObject.AddComponent<Outline>();
                    outline.effectDistance = new Vector2(2f, -2f);
                    outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
                    outline.enabled = false;

                    // Xóa plaque cũ nếu có
                    var existingPlaque = bestImg.transform.Find("Plaque");
                    if (existingPlaque != null) DestroyImmediate(existingPlaque.gameObject);

                    // Tạo Plaque nhỏ phong cách Aurelia fantasy dưới thẻ
                    GameObject plaqueGo = new GameObject("Plaque");
                    plaqueGo.transform.SetParent(bestImg.transform, false);
                    var pRt = plaqueGo.AddComponent<RectTransform>();
                    pRt.anchorMin = new Vector2(0.5f, 0f);
                    pRt.anchorMax = new Vector2(0.5f, 0f);
                    pRt.pivot = new Vector2(0.5f, 1f);
                    pRt.anchoredPosition = new Vector2(0f, -8f);
                    pRt.sizeDelta = new Vector2(240f, 38f);

                    var pImg = plaqueGo.AddComponent<Image>();
                    pImg.color = new Color(0.04f, 0.08f, 0.18f, 0.95f); // Deep Aurelia navy
                    pImg.raycastTarget = false; // Cho phép click xuyên vào thẻ

                    var pOutline = plaqueGo.AddComponent<Outline>();
                    pOutline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f); // Aurelia gold
                    pOutline.effectDistance = new Vector2(1.5f, -1.5f);

                    // Chữ mô tả trong plaque
                    GameObject textGo = new GameObject("Text");
                    textGo.transform.SetParent(plaqueGo.transform, false);
                    var tRt = textGo.AddComponent<RectTransform>();
                    tRt.anchorMin = Vector2.zero;
                    tRt.anchorMax = Vector2.one;
                    tRt.offsetMin = Vector2.zero;
                    tRt.offsetMax = Vector2.zero;

                    var tmp = textGo.AddComponent<TextMeshProUGUI>();
                    tmp.text = desc;
                    tmp.fontSize = 17f;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = new Color(0.98f, 0.96f, 0.92f, 1f); // Trắng kem Aurelia
                    tmp.raycastTarget = false;

                    string capturedCode = code;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnCardClicked(capturedCode));

                    UIHoverPulseEffect.Attach(bestImg.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);

                    _cards.Add(new DifficultyCardUI
                    {
                        levelCode = code,
                        levelName = name,
                        description = desc,
                        button = btn,
                        cardImage = bestImg,
                        outline = outline,
                        plaqueObj = plaqueGo,
                        plaqueText = tmp
                    });
                }
            }

            // Nút Bắt đầu lớn ở giữa phía dưới (y ~ -415f)
            Image startImg = images.FirstOrDefault(img => img.rectTransform.anchoredPosition.y < -250f && img.rectTransform.sizeDelta.x > 300f);
            if (startImg != null)
            {
                startImg.rectTransform.anchoredPosition = new Vector2(0f, -415f);
                startImg.raycastTarget = true;
                btnStart = startImg.GetComponent<Button>();
                if (btnStart == null) btnStart = startImg.gameObject.AddComponent<Button>();
                btnStart.targetGraphic = startImg;
                btnStart.onClick.RemoveAllListeners();
                btnStart.onClick.AddListener(OnStartClicked);

                startButtonCanvasGroup = startImg.GetComponent<CanvasGroup>();
                if (startButtonCanvasGroup == null) startButtonCanvasGroup = startImg.gameObject.AddComponent<CanvasGroup>();

                UIHoverPulseEffect.Attach(btnStart.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
            }

            // Nút Quay lại ở top-left
            Image backImg = images.FirstOrDefault(img => img.rectTransform.anchoredPosition.x < -600f && img.rectTransform.anchoredPosition.y > 350f);
            if (backImg != null)
            {
                backImg.raycastTarget = true;
                btnBack = backImg.GetComponent<Button>();
                if (btnBack == null) btnBack = backImg.gameObject.AddComponent<Button>();
                btnBack.targetGraphic = backImg;
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackClicked);
                UIHoverPulseEffect.Attach(btnBack.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
            }

            // Tạo thanh thông báo trạng thái đồng bộ
            CreateSyncStatusPlaque(canvas);

            // Tạo Aurelia Warning Modal
            CreateWarningModal(canvas);
        }

        private void CreateSyncStatusPlaque(Canvas canvas)
        {
            var existing = canvas.transform.Find("SyncStatusPlaque");
            if (existing != null)
            {
                syncStatusPlaque = existing.gameObject;
                syncStatusText = syncStatusPlaque.GetComponentInChildren<TextMeshProUGUI>();
                return;
            }

            syncStatusPlaque = new GameObject("SyncStatusPlaque");
            syncStatusPlaque.transform.SetParent(canvas.transform, false);

            var rt = syncStatusPlaque.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, 240f);
            rt.sizeDelta = new Vector2(560f, 32f);

            var img = syncStatusPlaque.AddComponent<Image>();
            img.color = new Color(0.04f, 0.08f, 0.18f, 0.90f);
            img.raycastTarget = false;

            var outline = syncStatusPlaque.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 0.85f);
            outline.effectDistance = new Vector2(1f, -1f);

            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(syncStatusPlaque.transform, false);
            var tRt = txtGo.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = Vector2.zero;
            tRt.offsetMax = Vector2.zero;

            syncStatusText = txtGo.AddComponent<TextMeshProUGUI>();
            syncStatusText.text = "Đang kiểm tra kết nối Google Sheets...";
            syncStatusText.fontSize = 15f;
            syncStatusText.alignment = TextAlignmentOptions.Center;
            syncStatusText.color = new Color(0.98f, 0.92f, 0.65f, 1f);
            syncStatusText.raycastTarget = false;

            syncStatusPlaque.SetActive(false);
        }

        private IEnumerator SyncBankDataRoutine()
        {
            _isSyncing = true;
            if (syncStatusPlaque != null)
            {
                syncStatusPlaque.SetActive(true);
                if (syncStatusText != null)
                {
                    syncStatusText.text = "✦ Đang tải ngân hàng câu hỏi từ Google Sheets...";
                    syncStatusText.color = new Color(0.98f, 0.92f, 0.65f, 1f);
                }
            }

            bool isEssay = (_mode == "ESSAY");
            bool syncDone = false;
            bool syncSuccess = false;
            int loadedCount = 0;
            string syncError = null;

            if (isEssay)
            {
                yield return GoogleSheetDataManager.Instance.FetchLivePhysicsEssaysRoutine((success, count, err) =>
                {
                    syncSuccess = success;
                    loadedCount = count;
                    syncError = err;
                    syncDone = true;
                });
            }
            else
            {
                yield return GoogleSheetDataManager.Instance.FetchLivePhysicsQuestionsRoutine((success, count, err) =>
                {
                    syncSuccess = success;
                    loadedCount = count;
                    syncError = err;
                    syncDone = true;
                });
            }

            // Đợi tối đa hoặc khi callback xong
            while (!syncDone) yield return null;
            _isSyncing = false;

            // Cập nhật lại số lượng và plaques
            RefreshCountsAndPlaques();

            if (syncStatusPlaque != null && syncStatusText != null)
            {
                if (syncSuccess)
                {
                    syncStatusText.text = $"✓ Đã đồng bộ trực tiếp {loadedCount} câu hỏi từ Google Sheets";
                    syncStatusText.color = new Color(0.45f, 1f, 0.55f, 1f);
                    yield return new WaitForSeconds(2.5f);
                    if (syncStatusPlaque != null) syncStatusPlaque.SetActive(false);
                }
                else
                {
                    syncStatusText.text = $"! Đang dùng ngân hàng offline cache (Không kết nối Sheet: {syncError})";
                    syncStatusText.color = new Color(1f, 0.85f, 0.45f, 1f);
                    GameDataRepository.LastSyncDataSource = "LocalCache";
                    GameDataRepository.LastSyncStatus = "OfflineCacheUsed";
                    GameDataRepository.LastSyncError = syncError;
                    yield return new WaitForSeconds(4.0f);
                    if (syncStatusPlaque != null) syncStatusPlaque.SetActive(false);
                }
            }

            // Nếu người chơi đã click chọn thẻ trước khi tải xong, cập nhật lại trạng thái nút
            if (!string.IsNullOrEmpty(selectedDifficulty))
            {
                var diag = GameDataRepository.GetDetailedDiagnosis(_chapterId, _lessonId, _mode, selectedDifficulty);
                SetStartButtonState(diag.CanStart);
            }
        }

        private void CreateWarningModal(Canvas canvas)
        {
            var existing = canvas.transform.Find("DifficultyAlertModal");
            if (existing != null)
            {
                alertModal = existing.gameObject;
                alertMessageText = alertModal.GetComponentInChildren<TextMeshProUGUI>();
                return;
            }

            alertModal = new GameObject("DifficultyAlertModal");
            alertModal.transform.SetParent(canvas.transform, false);
            alertModal.transform.SetAsLastSibling();

            var rt = alertModal.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Nền tối mờ che chắn click ra sau
            var bgImg = alertModal.AddComponent<Image>();
            bgImg.color = new Color(0.02f, 0.05f, 0.12f, 0.85f);
            bgImg.raycastTarget = true;

            // Click nền ngoài cũng đóng modal
            var bgBtn = alertModal.AddComponent<Button>();
            bgBtn.targetGraphic = bgImg;
            bgBtn.onClick.AddListener(() => alertModal.SetActive(false));

            // Hộp thoại Aurelia Navy Panel
            var panel = new GameObject("Panel");
            panel.transform.SetParent(alertModal.transform, false);
            var pRt = panel.AddComponent<RectTransform>();
            pRt.sizeDelta = new Vector2(580f, 260f);
            pRt.anchoredPosition = Vector2.zero;

            var pImg = panel.AddComponent<Image>();
            pImg.color = new Color(0.04f, 0.09f, 0.20f, 0.98f);
            pImg.raycastTarget = true;

            var outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f); // Aurelia gold
            outline.effectDistance = new Vector2(2f, -2f);

            // Tiêu đề
            var hGo = new GameObject("HeaderTitle");
            hGo.transform.SetParent(panel.transform, false);
            var hRt = hGo.AddComponent<RectTransform>();
            hRt.anchoredPosition = new Vector2(0f, 75f);
            hRt.sizeDelta = new Vector2(500f, 40f);
            var hTmp = hGo.AddComponent<TextMeshProUGUI>();
            hTmp.text = "THÔNG BÁO";
            hTmp.fontSize = 22f;
            hTmp.fontStyle = FontStyles.Bold;
            hTmp.alignment = TextAlignmentOptions.Center;
            hTmp.color = new Color(0.96f, 0.88f, 0.37f, 1f);
            hTmp.raycastTarget = false;

            // Nội dung thông báo
            var mGo = new GameObject("MessageText");
            mGo.transform.SetParent(panel.transform, false);
            var mRt = mGo.AddComponent<RectTransform>();
            mRt.anchoredPosition = new Vector2(0f, 10f);
            mRt.sizeDelta = new Vector2(500f, 90f);
            alertMessageText = mGo.AddComponent<TextMeshProUGUI>();
            alertMessageText.text = "Mức độ này hiện chưa đủ số câu hỏi.\nVui lòng chọn mức khác hoặc quay lại.";
            alertMessageText.fontSize = 18f;
            alertMessageText.alignment = TextAlignmentOptions.Center;
            alertMessageText.color = new Color(0.98f, 0.96f, 1f, 1f); // Trắng kem
            alertMessageText.raycastTarget = false;

            // Nút Xác nhận / Đã hiểu
            var bGo = new GameObject("Btn_Confirm");
            bGo.transform.SetParent(panel.transform, false);
            var bRt = bGo.AddComponent<RectTransform>();
            bRt.anchoredPosition = new Vector2(0f, -75f);
            bRt.sizeDelta = new Vector2(180f, 46f);

            var bImg = bGo.AddComponent<Image>();
            bImg.color = new Color(0.08f, 0.16f, 0.32f, 1f);
            bImg.raycastTarget = true;

            var bOutline = bGo.AddComponent<Outline>();
            bOutline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            bOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var bBtn = bGo.AddComponent<Button>();
            bBtn.targetGraphic = bImg;
            bBtn.onClick.AddListener(() => alertModal.SetActive(false));

            var btGo = new GameObject("Text");
            btGo.transform.SetParent(bGo.transform, false);
            var btRt = btGo.AddComponent<RectTransform>();
            btRt.anchorMin = Vector2.zero;
            btRt.anchorMax = Vector2.one;
            btRt.offsetMin = Vector2.zero;
            btRt.offsetMax = Vector2.zero;

            var btTmp = btGo.AddComponent<TextMeshProUGUI>();
            btTmp.text = "ĐÃ HIỂU";
            btTmp.fontSize = 17f;
            btTmp.fontStyle = FontStyles.Bold;
            btTmp.alignment = TextAlignmentOptions.Center;
            btTmp.color = new Color(0.98f, 0.92f, 0.65f, 1f); // Vàng kem hoàng gia
            btTmp.raycastTarget = false;

            UIHoverPulseEffect.Attach(bGo, hover: 1.06f, minPulse: 1.02f, maxPulse: 1.06f, outline: true);

            alertModal.SetActive(false);
        }

        private void ShowWarningModal(string message)
        {
            if (alertModal != null)
            {
                if (alertMessageText != null)
                {
                    alertMessageText.text = message;
                }
                alertModal.transform.SetAsLastSibling();
                alertModal.SetActive(true);
            }
        }

        private void RefreshCountsAndPlaques()
        {
            foreach (var card in _cards)
            {
                int count = 0;
                if (_mode == "ESSAY")
                {
                    count = GameDataRepository.CountEssayQuestions(_lessonId, card.levelCode);
                }
                else
                {
                    count = GameDataRepository.CountMCQs(_lessonId, card.levelCode);
                }

                card.questionCount = count;
                if (card.plaqueText != null)
                {
                    card.plaqueText.text = card.description;
                }
            }
        }

        private void SetStartButtonState(bool canStart)
        {
            if (btnStart != null)
            {
                btnStart.interactable = canStart;
            }
            if (startButtonCanvasGroup != null)
            {
                startButtonCanvasGroup.alpha = canStart ? 1.0f : 0.45f;
            }
        }

        /// <summary>
        /// Xử lý khi click vào 1 thẻ độ khó.
        /// Chỉ chọn và highlight thẻ, kiểm tra số câu. KHÔNG vào bài ngay.
        /// </summary>
        public void OnCardClicked(string code)
        {
            selectedDifficulty = code;
            if (NavigationService.Instance != null && NavigationService.Instance.Selection != null)
            {
                NavigationService.Instance.Selection.Difficulty = code;
            }

            // Highlight thẻ được chọn, bỏ highlight các thẻ khác
            foreach (var card in _cards)
            {
                bool isThisCardSelected = (card.levelCode == code);
                if (card.cardImage != null)
                {
                    card.cardImage.color = Color.white;
                    var hover = card.cardImage.GetComponent<UIHoverPulseEffect>();
                    if (hover != null)
                    {
                        hover.SetSelected(isThisCardSelected);
                    }
                }
            }

            // Chẩn đoán chi tiết và in log dạng bắt buộc
            var diag = GameDataRepository.GetDetailedDiagnosis(_chapterId, _lessonId, _mode, code);

            if (diag.CanStart)
            {
                SetStartButtonState(canStart: true);
            }
            else
            {
                SetStartButtonState(canStart: false);

                // Nếu đang đồng bộ dở dang:
                if (_isSyncing)
                {
                    ShowWarningModal("Đang đồng bộ ngân hàng câu hỏi từ Google Sheets.\nVui lòng chờ trong giây lát...");
                }
                else
                {
                    // Hiển thị modal cảnh báo Aurelia khi mức chưa đủ câu hỏi
                    ShowWarningModal($"Mức độ này hiện chưa đủ số câu hỏi ({diag.FilteredCount}/{diag.RequiredCount} câu).\nVui lòng chọn mức khác hoặc quay lại.");
                }
            }
        }

        /// <summary>
        /// Giữ phương thức công khai để tương thích với các lệnh gọi chọn độ khó từ test hoặc logic bên ngoài.
        /// </summary>
        public void SelectDifficulty(string code)
        {
            OnCardClicked(code);
        }

        /// <summary>
        /// Xử lý khi click nút lớn "BẮT ĐẦU" ở giữa phía dưới.
        /// Chỉ khi bấm nút này mới tạo AttemptContext và chuyển Scene.
        /// </summary>
        public void OnStartClicked()
        {
            // Kiểm tra đã chọn độ khó chưa
            if (string.IsNullOrEmpty(selectedDifficulty))
            {
                ShowWarningModal("Vui lòng chọn mức độ khó trước khi bắt đầu.");
                return;
            }

            // Kiểm tra session timer buổi học
            if (PhysicsGameSession.Instance != null && PhysicsGameSession.Instance.StudyTimeLimitReached)
            {
                ShowWarningModal("Đã hết thời gian buổi học hôm nay.\nVui lòng vào 'Buổi học hôm nay' để bắt đầu buổi mới.");
                return;
            }

            // Chẩn đoán kiểm tra trước khi chuyển scene
            var diag = GameDataRepository.GetDetailedDiagnosis(_chapterId, _lessonId, _mode, selectedDifficulty);
            if (!diag.CanStart)
            {
                ShowWarningModal($"Mức độ này hiện chưa đủ số câu hỏi ({diag.FilteredCount}/{diag.RequiredCount} câu).\nVui lòng chọn mức khác hoặc quay lại.");
                return;
            }

            Debug.Log($"[DifficultySelectionController] Xác nhận bắt đầu bài {_lessonId} - Mode: {_mode} - Mức: {selectedDifficulty} ({diag.FilteredCount} câu)");

            // Tạo AttemptContext mới
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.Attempt = new AttemptContextData
                {
                    AttemptId = Guid.NewGuid().ToString("N"),
                    Source = AttemptSource.RESTORATION,
                    BankVersion = (GameDataRepository.LastSyncDataSource == "GoogleSheets") ? "live_sheet" : "1.0",
                    ScoreRaw = 0f,
                    CorrectCount = 0,
                    IsFinished = false
                };

                if (_mode == "ESSAY")
                {
                    var essays = GameDataRepository.GetEssayQuestions(_lessonId, selectedDifficulty, 5);
                    NavigationService.Instance.Attempt.TotalQuestions = essays.Count;
                    NavigationService.Instance.Attempt.QuestionCount = essays.Count;
                    NavigationService.Instance.Attempt.QuestionIds = essays.Select(e => e.cauHoiID).ToList();

                    NavigationService.Instance.Attempt.EssayAnswers.Clear();
                    foreach (var e in essays)
                    {
                        NavigationService.Instance.Attempt.EssayAnswers.Add(new EssayAnswerRecord
                        {
                            CauHoiID = e.cauHoiID,
                            QuestionText = e.noiDung,
                            SampleAnswer = e.dapAnMau,
                            Explanation = e.giaiThich,
                            IsGraded = false
                        });
                    }

                    NavigationService.Instance.NavigateTo(RouteId.EssayAnswer);
                }
                else
                {
                    var mcqs = GameDataRepository.GetMCQs(_lessonId, selectedDifficulty, 10);
                    NavigationService.Instance.Attempt.TotalQuestions = mcqs.Count;
                    NavigationService.Instance.Attempt.QuestionCount = mcqs.Count;
                    NavigationService.Instance.Attempt.QuestionIds = mcqs.Select(q => q.cauHoiID).ToList();

                    NavigationService.Instance.Attempt.Answers.Clear();
                    foreach (var q in mcqs)
                    {
                        NavigationService.Instance.Attempt.Answers.Add(new QuizAnswerRecord
                        {
                            CauHoiID = q.cauHoiID,
                            QuestionText = q.noiDung,
                            Options = new[] { q.dapAnA, q.dapAnB, q.dapAnC, q.dapAnD },
                            CorrectOptionIndex = q.correctIndex,
                            Explanation = q.giaiThich,
                            KeyKnowledge = q.meoGhiNho,
                            SelectedOptionIndex = -1
                        });
                    }

                    NavigationService.Instance.NavigateTo(RouteId.PracticeQuiz);
                }
            }
        }

        public void OnBackClicked()
        {
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.NavigateTo(RouteId.ModeSelection);
            }
        }
    }
}
