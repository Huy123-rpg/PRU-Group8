using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển toàn bộ logic giao diện Scene 04_ChapterSelection.
    /// Tự động liên kết các thẻ chương (chuong1 -> chuong5) và nút Quay lại (quaylai).
    /// Đảm bảo cả 5 chương đều có thể click để phục vụ demo PRU213 theo yêu cầu Phase 2.
    /// </summary>
    public class ChapterSelectionController : MonoBehaviour
    {
        [Header("=== CHAPTER CARDS ===")]
        [SerializeField] private GameObject cardChuong1;
        [SerializeField] private GameObject cardChuong2;
        [SerializeField] private GameObject cardChuong3;
        [SerializeField] private GameObject cardChuong4;
        [SerializeField] private GameObject cardChuong5;

        [Header("=== BACK BUTTON ===")]
        [SerializeField] private GameObject btnBack; // "quaylai"

        [Header("=== CONFIRM BUTTON ===")]
        [SerializeField] private Button btnConfirm; // "Button" (Xác nhận)

        private readonly Dictionary<int, GameObject> _chapterCards = new Dictionary<int, GameObject>();
        private int _selectedChapter = 1;

        private void Awake()
        {
            FindUIReferences();
            SetupChapterButtons();
            SetupBackButton();
            SetupConfirmButton();
            SetupChapterTextLabels();
        }

        private void Start()
        {
            if (PhysicsGameSession.Instance != null && PhysicsGameSession.Instance.SelectedChapterId > 0)
            {
                _selectedChapter = PhysicsGameSession.Instance.SelectedChapterId;
            }
            HighlightCurrentChapter();
            SetupChapterTextLabels();
            CheckFinalProgression();
        }

        private struct ChapterFrameLayout
        {
            public Vector2 offset;
            public Vector2 size;

            public ChapterFrameLayout(Vector2 offset, Vector2 size)
            {
                this.offset = offset;
                this.size = size;
            }
        }

        private struct ChapterInfoMeta
        {
            public string roman;
            public string name;

            public ChapterInfoMeta(string roman, string name)
            {
                this.roman = roman;
                this.name = name;
            }
        }

        private static readonly Dictionary<int, ChapterFrameLayout> _frameOffsets = new Dictionary<int, ChapterFrameLayout>
        {
            { 1, new ChapterFrameLayout(new Vector2(0.7f, -177f), new Vector2(250f, 74f)) },
            { 2, new ChapterFrameLayout(new Vector2(1.9f, -169f), new Vector2(245f, 70f)) },
            { 3, new ChapterFrameLayout(new Vector2(-22.1f, -190f), new Vector2(245f, 74f)) },
            { 4, new ChapterFrameLayout(new Vector2(6.4f, -151f), new Vector2(250f, 74f)) },
            { 5, new ChapterFrameLayout(new Vector2(-31.1f, -153f), new Vector2(255f, 92f)) }
        };

        private static readonly Dictionary<int, ChapterInfoMeta> _chapterMeta = new Dictionary<int, ChapterInfoMeta>
        {
            { 1, new ChapterInfoMeta("I", "Năng lượng cơ học") },
            { 2, new ChapterInfoMeta("II", "Ánh sáng") },
            { 3, new ChapterInfoMeta("III", "Điện") },
            { 4, new ChapterInfoMeta("IV", "Điện từ") },
            { 5, new ChapterInfoMeta("V", "Năng lượng với cuộc sống") }
        };

        /// <summary>
        /// Rà soát và định hình 3 TextMeshPro riêng biệt cho từng ô chương:
        /// 1. Chương I/II/III/IV/V
        /// 2. Tên chương
        /// 3. Tiến độ x/y thiết bị
        /// Căn giữa theo chiều ngang với khung vàng, phân bổ vị trí & rect chuẩn xác, tránh đè chữ.
        /// </summary>
        public void SetupChapterTextLabels()
        {
            foreach (var kvp in _chapterCards)
            {
                int chapterNum = kvp.Key;
                GameObject cardGo = kvp.Value;
                if (cardGo == null) continue;

                // Dọn dẹp Text cũ (ProgressBadge) nếu có
                Transform oldBadge = cardGo.transform.Find("ProgressBadge");
                if (oldBadge != null)
                {
                    if (Application.isPlaying) Destroy(oldBadge.gameObject);
                    else DestroyImmediate(oldBadge.gameObject);
                }

                // Tìm hoặc tạo ChapterInfoPanel tại trung tâm khung vàng của từng đảo
                Transform panelTransform = cardGo.transform.Find("ChapterInfoPanel");
                GameObject panelGo;
                RectTransform panelRt;
                if (panelTransform == null)
                {
                    panelGo = new GameObject("ChapterInfoPanel", typeof(RectTransform));
                    panelGo.transform.SetParent(cardGo.transform, false);
                    panelRt = panelGo.GetComponent<RectTransform>();
                }
                else
                {
                    panelGo = panelTransform.gameObject;
                    panelRt = panelGo.GetComponent<RectTransform>();
                }

                ChapterFrameLayout layout = _frameOffsets.TryGetValue(chapterNum, out var foundLayout)
                    ? foundLayout
                    : new ChapterFrameLayout(Vector2.zero, new Vector2(250f, 74f));

                panelRt.anchorMin = new Vector2(0.5f, 0.5f);
                panelRt.anchorMax = new Vector2(0.5f, 0.5f);
                panelRt.pivot = new Vector2(0.5f, 0.5f);
                panelRt.anchoredPosition = layout.offset;
                panelRt.sizeDelta = layout.size;

                ChapterInfoMeta meta = _chapterMeta.TryGetValue(chapterNum, out var foundMeta)
                    ? foundMeta
                    : new ChapterInfoMeta("I", "Chương");

                int restored = RestorationService.GetRestoredDeviceCount(chapterNum);
                int total = RestorationService.GetTotalDeviceCount(chapterNum);

                // --- 1. TextMeshPro: Tiêu đề "Chương I/II/III/IV/V" ---
                var tmpHeader = GetOrCreateTMPChild(panelGo, "Text_ChapterHeader");
                RectTransform rtHeader = tmpHeader.rectTransform;
                rtHeader.anchorMin = new Vector2(0.5f, 0.5f);
                rtHeader.anchorMax = new Vector2(0.5f, 0.5f);
                rtHeader.pivot = new Vector2(0.5f, 0.5f);
                rtHeader.anchoredPosition = new Vector2(0f, chapterNum == 5 ? 26f : 21f);
                rtHeader.sizeDelta = new Vector2(layout.size.x, 24f);

                tmpHeader.text = $"Chương {meta.roman}";
                tmpHeader.fontSize = 16f;
                tmpHeader.fontStyle = FontStyles.Bold;
                tmpHeader.alignment = TextAlignmentOptions.Center;
                tmpHeader.color = new Color(0.965f, 0.871f, 0.541f, 1f); // Aurelia Gold
                tmpHeader.raycastTarget = false;

                // --- 2. TextMeshPro: Tên chương ---
                var tmpTitle = GetOrCreateTMPChild(panelGo, "Text_ChapterTitle");
                RectTransform rtTitle = tmpTitle.rectTransform;
                rtTitle.anchorMin = new Vector2(0.5f, 0.5f);
                rtTitle.anchorMax = new Vector2(0.5f, 0.5f);
                rtTitle.pivot = new Vector2(0.5f, 0.5f);
                rtTitle.anchoredPosition = new Vector2(0f, chapterNum == 5 ? 2f : 0f);
                rtTitle.sizeDelta = new Vector2(layout.size.x, 22f);

                tmpTitle.text = meta.name;
                tmpTitle.fontSize = 14f;
                tmpTitle.enableAutoSizing = true;
                tmpTitle.fontSizeMin = 11f;
                tmpTitle.fontSizeMax = 14f;
                tmpTitle.fontStyle = FontStyles.Normal;
                tmpTitle.alignment = TextAlignmentOptions.Center;
                tmpTitle.color = Color.white;
                tmpTitle.raycastTarget = false;

                // --- 3. TextMeshPro: Tiến độ "x/y thiết bị" ---
                var tmpProgress = GetOrCreateTMPChild(panelGo, "Text_ChapterProgress");
                RectTransform rtProg = tmpProgress.rectTransform;
                rtProg.anchorMin = new Vector2(0.5f, 0.5f);
                rtProg.anchorMax = new Vector2(0.5f, 0.5f);
                rtProg.pivot = new Vector2(0.5f, 0.5f);
                rtProg.anchoredPosition = new Vector2(0f, chapterNum == 5 ? -24f : -20f);
                rtProg.sizeDelta = new Vector2(layout.size.x, 20f);

                tmpProgress.text = $"{restored}/{total} thiết bị";
                tmpProgress.fontSize = 12.5f;
                tmpProgress.fontStyle = FontStyles.Bold;
                tmpProgress.alignment = TextAlignmentOptions.Center;
                tmpProgress.color = (restored >= total && total > 0)
                    ? new Color(0.3f, 0.93f, 0.55f, 1f)  // Xanh hoàn thành
                    : new Color(0.96f, 0.82f, 0.38f, 1f); // Vàng tiến độ
                tmpProgress.raycastTarget = false;
            }
        }

        private TextMeshProUGUI GetOrCreateTMPChild(GameObject parent, string childName)
        {
            Transform t = parent.transform.Find(childName);
            if (t != null && t.TryGetComponent<TextMeshProUGUI>(out var existing))
            {
                return existing;
            }
            GameObject go = t != null ? t.gameObject : new GameObject(childName, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            var tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
            return tmp;
        }

        private void CheckFinalProgression()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            StoryFlowController.TryShowPostGameplayOrPendingStories(
                completedChapterHint: 0,
                customSave: null,
                onAllFinished: null,
                canvasTransform: canvas != null ? canvas.transform : null);
        }

        private void FindUIReferences()
        {
            if (cardChuong1 == null) cardChuong1 = GameObject.Find("chuong1");
            if (cardChuong2 == null) cardChuong2 = GameObject.Find("chuong2");
            if (cardChuong3 == null) cardChuong3 = GameObject.Find("chuong3");
            if (cardChuong4 == null) cardChuong4 = GameObject.Find("chuong4");
            if (cardChuong5 == null) cardChuong5 = GameObject.Find("chuong5");
            if (btnBack == null) btnBack = GameObject.Find("quaylai");
            if (btnConfirm == null)
            {
                var confirmGo = GameObject.Find("Btn_Confirm") ?? GameObject.Find("Button") ?? GameObject.Find("xacnhan");
                if (confirmGo != null)
                {
                    btnConfirm = confirmGo.GetComponent<Button>() ?? confirmGo.AddComponent<Button>();
                }
            }

            if (cardChuong1 != null) _chapterCards[1] = cardChuong1;
            if (cardChuong2 != null) _chapterCards[2] = cardChuong2;
            if (cardChuong3 != null) _chapterCards[3] = cardChuong3;
            if (cardChuong4 != null) _chapterCards[4] = cardChuong4;
            if (cardChuong5 != null) _chapterCards[5] = cardChuong5;
        }

        private void SetupChapterButtons()
        {
            foreach (var kvp in _chapterCards)
            {
                int chapterNum = kvp.Key;
                GameObject go = kvp.Value;
                if (go == null) continue;

                Button btn = go.GetComponent<Button>();
                if (btn == null)
                {
                    btn = go.AddComponent<Button>();
                    Image img = go.GetComponent<Image>();
                    if (img != null)
                    {
                        img.raycastTarget = true;
                        btn.targetGraphic = img;
                    }
                }

                var fb = go.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);
                UIHoverPulseEffect.Attach(go, hover: 1.07f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnChapterSelected(chapterNum));
            }
        }

        private void SetupBackButton()
        {
            if (btnBack != null)
            {
                Button btn = btnBack.GetComponent<Button>();
                if (btn == null)
                {
                    btn = btnBack.AddComponent<Button>();
                    Image img = btnBack.GetComponent<Image>();
                    if (img != null)
                    {
                        img.raycastTarget = true;
                        btn.targetGraphic = img;
                    }
                }

                var fb = btnBack.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);
                UIHoverPulseEffect.Attach(btnBack, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnBackClicked);
            }
        }

        private void SetupConfirmButton()
        {
            if (btnConfirm != null)
            {
                var img = btnConfirm.GetComponent<Image>();
                if (img != null)
                {
                    img.raycastTarget = true;
                    btnConfirm.targetGraphic = img;
                }

                // Tắt raycastTarget trên các text hoặc sprite con để không chặn click
                foreach (var tmp in btnConfirm.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    tmp.raycastTarget = false;
                }

                var fb = btnConfirm.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);
                UIHoverPulseEffect.Attach(btnConfirm.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);

                btnConfirm.onClick.RemoveAllListeners();
                btnConfirm.onClick.AddListener(OnConfirmClicked);
                Debug.Log("[ChapterSelectionController] Đã thiết lập nút Xác nhận.");
            }
        }

        private void HighlightCurrentChapter()
        {
            int selectedId = _selectedChapter > 0 ? _selectedChapter : (PhysicsGameSession.Instance != null && PhysicsGameSession.Instance.SelectedChapterId > 0 ? PhysicsGameSession.Instance.SelectedChapterId : 1);
            foreach (var kvp in _chapterCards)
            {
                int chapterNum = kvp.Key;
                GameObject go = kvp.Value;
                if (go == null) continue;

                Image img = go.GetComponent<Image>();
                if (img != null)
                {
                    img.color = Color.white; // Luôn giữ màu gốc sáng đẹp theo thiết kế
                }

                var hover = go.GetComponent<UIHoverPulseEffect>();
                if (hover != null)
                {
                    hover.SetSelected(chapterNum == selectedId);
                }
            }
        }

        private void OnChapterSelected(int chapterNumber)
        {
            Debug.Log($"[ChapterSelectionController] Đã chọn Chương {chapterNumber}. Chờ người chơi bấm Xác nhận.");
            _selectedChapter = chapterNumber;
            if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.SelectChapter(chapterNumber);
            }
            NavigationService.Instance.Selection.ChapterId = chapterNumber;
            HighlightCurrentChapter();

            // YÊU CẦU 1: Click đảo chỉ chọn và highlight; KHÔNG tự động chuyển màn.
            // Chỉ khi bấm nút Xác nhận mới chuyển trực tiếp sang Bản đồ thiết bị.
        }

        private void OnConfirmClicked()
        {
            int chapterToOpen = _selectedChapter > 0 
                ? _selectedChapter 
                : (PhysicsGameSession.Instance != null && PhysicsGameSession.Instance.SelectedChapterId > 0 
                    ? PhysicsGameSession.Instance.SelectedChapterId 
                    : 1);

            Debug.Log($"[ChapterSelectionController] Người chơi bấm Xác nhận vào Chương {chapterToOpen}. Chuyển TRỰC TIẾP sang Bản đồ thiết bị.");
            if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.SelectChapter(chapterToOpen);
            }
            NavigationService.Instance.Selection.ChapterId = chapterToOpen;
            NavigationService.Instance.Selection.LessonId = "";
            NavigationService.Instance.Selection.DeviceId = "";
            NavigationService.Instance.Selection.LessonTitle = "";

            bool isCompleted = RestorationService.IsChapterFullyRestored(chapterToOpen);
            RouteId mapRoute = SceneCatalog.GetChapterMapRoute(chapterToOpen, isCompleted);
            NavigationService.Instance.NavigateTo(mapRoute);
        }

        private void OnBackClicked()
        {
            Debug.Log("[ChapterSelectionController] Người chơi bấm Quay lại.");
            if (NavigationService.Instance != null && NavigationService.Instance.PreviousRoute == RouteId.MainMenu)
            {
                NavigationService.Instance.NavigateTo(RouteId.MainMenu);
            }
            else
            {
                NavigationService.Instance.NavigateTo(RouteId.StudySessionSetup);
            }
        }

        private void ShowEndSessionConfirmModal()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Transform existing = canvas.transform.Find("EndSessionConfirmModal");
            if (existing != null) return;

            GameObject modalGo = new GameObject("EndSessionConfirmModal");
            modalGo.transform.SetParent(canvas.transform, false);

            RectTransform rt = modalGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image bg = modalGo.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.08f, 0.16f, 0.88f);

            GameObject cardGo = new GameObject("Card");
            cardGo.transform.SetParent(modalGo.transform, false);
            RectTransform cRt = cardGo.AddComponent<RectTransform>();
            cRt.sizeDelta = new Vector2(520, 260);
            cRt.anchoredPosition = Vector2.zero;

            Image cImg = cardGo.AddComponent<Image>();
            cImg.color = new Color(0.08f, 0.14f, 0.28f, 0.98f);

            Outline outline = cardGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            outline.effectDistance = new Vector2(2, -2);

            GameObject msgGo = new GameObject("Message");
            msgGo.transform.SetParent(cardGo.transform, false);
            RectTransform mRt = msgGo.AddComponent<RectTransform>();
            mRt.anchoredPosition = new Vector2(0, 35);
            mRt.sizeDelta = new Vector2(460, 100);
            var msgTmp = msgGo.AddComponent<TextMeshProUGUI>();
            msgTmp.text = "Bạn đang trong buổi học.\nKết thúc buổi học và quay về Menu chính?";
            msgTmp.fontSize = 20;
            msgTmp.alignment = TextAlignmentOptions.Center;
            msgTmp.color = Color.white;

            // Buttons: Đồng ý & Hủy
            GameObject btnAgreeGo = new GameObject("Btn_Agree");
            btnAgreeGo.transform.SetParent(cardGo.transform, false);
            RectTransform aRt = btnAgreeGo.AddComponent<RectTransform>();
            aRt.anchoredPosition = new Vector2(-110, -60);
            aRt.sizeDelta = new Vector2(160, 44);
            Image aImg = btnAgreeGo.AddComponent<Image>();
            aImg.color = new Color(0.85f, 0.3f, 0.3f, 1f);
            Button aBtn = btnAgreeGo.AddComponent<Button>();
            aBtn.targetGraphic = aImg;
            GameObject aTxtGo = new GameObject("Text");
            aTxtGo.transform.SetParent(btnAgreeGo.transform, false);
            var aTmp = aTxtGo.AddComponent<TextMeshProUGUI>();
            aTmp.text = "KẾT THÚC";
            aTmp.fontSize = 16;
            aTmp.fontStyle = FontStyles.Bold;
            aTmp.alignment = TextAlignmentOptions.Center;
            aTmp.color = Color.white;
            aTmp.raycastTarget = false;
            aBtn.onClick.AddListener(() =>
            {
                if (PhysicsGameSession.Instance != null)
                {
                    PhysicsGameSession.Instance.FinishStudySession();
                }
                NavigationService.Instance.NavigateTo(RouteId.MainMenu);
            });

            GameObject btnCancelGo = new GameObject("Btn_Cancel");
            btnCancelGo.transform.SetParent(cardGo.transform, false);
            RectTransform canRt = btnCancelGo.AddComponent<RectTransform>();
            canRt.anchoredPosition = new Vector2(110, -60);
            canRt.sizeDelta = new Vector2(160, 44);
            Image canImg = btnCancelGo.AddComponent<Image>();
            canImg.color = new Color(0.3f, 0.4f, 0.55f, 1f);
            Button canBtn = btnCancelGo.AddComponent<Button>();
            canBtn.targetGraphic = canImg;
            GameObject canTxtGo = new GameObject("Text");
            canTxtGo.transform.SetParent(btnCancelGo.transform, false);
            var canTmp = canTxtGo.AddComponent<TextMeshProUGUI>();
            canTmp.text = "Ở LẠI";
            canTmp.fontSize = 16;
            canTmp.fontStyle = FontStyles.Bold;
            canTmp.alignment = TextAlignmentOptions.Center;
            canTmp.color = Color.white;
            canTmp.raycastTarget = false;
            canBtn.onClick.AddListener(() => Destroy(modalGo));
        }
    }
}
