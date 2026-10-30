using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển logic giao diện Scene 15.1_Achievements (màn 2) theo đặc tả Mục 13.2:
    /// - 12 thành tích chuẩn (A01 đến A12).
    /// - 2 trang, 6 dòng/trang (Trang 1: A01-A06; Trang 2: A07-A12).
    /// - Ở trang 1 nút "Trang trước" bị vô hiệu; ở trang 2 nút "Trang sau" bị vô hiệu.
    /// - Không chuyển scene khi chuyển trang (chỉ đổi dữ liệu render 6 dòng).
    /// - Hiển thị tiến độ thật: "Đã mở khóa: X/12".
    /// - Nút Quay lại -> MainMenu.
    /// </summary>
    public class AchievementUIController : MonoBehaviour
    {
        [Header("=== TEXT REFERENCES ===")]
        [SerializeField] private TextMeshProUGUI progressText; // "Đã mở khóa: X/12"
        [SerializeField] private TextMeshProUGUI pageIndicatorText; // "Trang 1/2"

        [Header("=== BUTTON REFERENCES ===")]
        [SerializeField] private Button btnPrevPage;
        [SerializeField] private Button btnNextPage;
        [SerializeField] private Button btnBack;

        [Serializable]
        public class AchievementDefinition
        {
            public string id;
            public string name;
            public string description;
        }

        private static readonly List<AchievementDefinition> AllAchievements = new List<AchievementDefinition>
        {
            // Trang 1: A01 - A06
            new AchievementDefinition { id = "A01", name = "Tia sáng đầu tiên", description = "Khôi phục ít nhất 1 thiết bị của học viện" },
            new AchievementDefinition { id = "A02", name = "Kỹ sư Cơ học", description = "Khôi phục toàn bộ 4/4 thiết bị Chương I" },
            new AchievementDefinition { id = "A03", name = "Người thắp sáng", description = "Khôi phục toàn bộ 7/7 thiết bị Chương II" },
            new AchievementDefinition { id = "A04", name = "Bậc thầy Điện học", description = "Khôi phục toàn bộ 4/4 thiết bị Chương III" },
            new AchievementDefinition { id = "A05", name = "Nhà phát minh Cảm ứng", description = "Khôi phục toàn bộ 3/3 thiết bị Chương IV" },
            new AchievementDefinition { id = "A06", name = "Vệ binh Năng lượng xanh", description = "Khôi phục toàn bộ 3/3 thiết bị Chương V" },

            // Trang 2: A07 - A12
            new AchievementDefinition { id = "A07", name = "Người hồi sinh Aurelia", description = "Khôi phục toàn bộ 21/21 thiết bị của cả 5 chương" },
            new AchievementDefinition { id = "A08", name = "Điểm tuyệt đối", description = "Một lượt học hợp lệ đạt điểm 100/100" },
            new AchievementDefinition { id = "A09", name = "Bứt phá bản thân", description = "Hai lượt so sánh hợp lệ cùng nhóm tăng ít nhất 20 điểm" },
            new AchievementDefinition { id = "A10", name = "Không bỏ cuộc", description = "Cùng nhóm từng dưới 80, lần sau đạt từ 80 trở lên" },
            new AchievementDefinition { id = "A11", name = "Kho tàng kiến thức", description = "Đạt 100 câu hỏi khác nhau (MCQ đúng, essay >= 80)" },
            new AchievementDefinition { id = "A12", name = "Nhịp học đều đặn", description = "Có lượt hoàn tất hợp lệ ở 3 ngày khác nhau trong tuần" }
        };

        private int _currentPage = 1; // 1 hoặc 2
        private readonly List<GameObject> _rowObjects = new List<GameObject>();

        private void Awake()
        {
            EnsureEventSystem();
            FindUIReferences();
            SetupActionButtons();
        }

        private void Start()
        {
            RefreshUI();
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }
        }

        private void FindUIReferences()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Fix background raycasts
            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
            {
                if (img.GetComponent<Button>() != null) continue;
                string n = img.gameObject.name.ToLower();
                if (n.Contains("bg") || n.Contains("frame") || n.Contains("panel") || n == "image")
                {
                    img.raycastTarget = false;
                }
            }

            // Tìm progressText
            TextMeshProUGUI[] tmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                string text = t.text ?? "";
                if (text.Contains("Đã mở khóa") || text.Contains("/7") || text.Contains("/12"))
                {
                    progressText = t;
                    break;
                }
            }

            // Tìm nút Back
            Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                var rt = b.GetComponent<RectTransform>();
                if (rt.anchoredPosition.x > 500f && rt.anchoredPosition.y < -380f)
                {
                    btnBack = b;
                }
                else if (b.gameObject.name.ToLower().Contains("back") || b.gameObject.name.ToLower().Contains("quaylai"))
                {
                    btnBack = b;
                }
            }

            // Tạo các nút chuyển trang Prev/Next và PageIndicator nếu chưa có
            EnsurePageControls(canvas);

            // Tìm hoặc dựng 6 container dòng
            EnsureRowContainers(canvas);
        }

        private void EnsurePageControls(Canvas canvas)
        {
            Transform existingControls = canvas.transform.Find("AchievementPageControls");
            if (existingControls != null) return;

            GameObject controlsGo = new GameObject("AchievementPageControls");
            controlsGo.transform.SetParent(canvas.transform, false);

            RectTransform cRt = controlsGo.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0f);
            cRt.anchorMax = new Vector2(0.5f, 0f);
            cRt.pivot = new Vector2(0.5f, 0f);
            cRt.anchoredPosition = new Vector2(0f, 40f);
            cRt.sizeDelta = new Vector2(450f, 50f);

            // Nút Trang trước
            GameObject prevGo = new GameObject("Btn_PrevPage");
            prevGo.transform.SetParent(controlsGo.transform, false);
            RectTransform pRt = prevGo.AddComponent<RectTransform>();
            pRt.anchoredPosition = new Vector2(-150f, 0f);
            pRt.sizeDelta = new Vector2(140f, 42f);
            Image pImg = prevGo.AddComponent<Image>();
            pImg.color = new Color(0.12f, 0.25f, 0.45f, 1f);
            btnPrevPage = prevGo.AddComponent<Button>();
            btnPrevPage.targetGraphic = pImg;
            GameObject pTxtGo = new GameObject("Text");
            pTxtGo.transform.SetParent(prevGo.transform, false);
            var pTmp = pTxtGo.AddComponent<TextMeshProUGUI>();
            pTmp.text = "◀ TRƯỚC";
            pTmp.fontSize = 17;
            pTmp.fontStyle = FontStyles.Bold;
            pTmp.alignment = TextAlignmentOptions.Center;
            pTmp.color = Color.white;
            pTmp.raycastTarget = false;

            // Page Indicator
            GameObject indGo = new GameObject("PageIndicator");
            indGo.transform.SetParent(controlsGo.transform, false);
            RectTransform iRt = indGo.AddComponent<RectTransform>();
            iRt.anchoredPosition = Vector2.zero;
            iRt.sizeDelta = new Vector2(120f, 42f);
            pageIndicatorText = indGo.AddComponent<TextMeshProUGUI>();
            pageIndicatorText.fontSize = 20;
            pageIndicatorText.fontStyle = FontStyles.Bold;
            pageIndicatorText.alignment = TextAlignmentOptions.Center;
            pageIndicatorText.color = new Color(0.96f, 0.82f, 0.38f, 1f);
            pageIndicatorText.raycastTarget = false;

            // Nút Trang sau
            GameObject nextGo = new GameObject("Btn_NextPage");
            nextGo.transform.SetParent(controlsGo.transform, false);
            RectTransform nRt = nextGo.AddComponent<RectTransform>();
            nRt.anchoredPosition = new Vector2(150f, 0f);
            nRt.sizeDelta = new Vector2(140f, 42f);
            Image nImg = nextGo.AddComponent<Image>();
            nImg.color = new Color(0.12f, 0.25f, 0.45f, 1f);
            btnNextPage = nextGo.AddComponent<Button>();
            btnNextPage.targetGraphic = nImg;
            GameObject nTxtGo = new GameObject("Text");
            nTxtGo.transform.SetParent(nextGo.transform, false);
            var nTmp = nTxtGo.AddComponent<TextMeshProUGUI>();
            nTmp.text = "SAU ▶";
            nTmp.fontSize = 17;
            nTmp.fontStyle = FontStyles.Bold;
            nTmp.alignment = TextAlignmentOptions.Center;
            nTmp.color = Color.white;
            nTmp.raycastTarget = false;
        }

        private void EnsureRowContainers(Canvas canvas)
        {
            _rowObjects.Clear();
            Transform existingList = canvas.transform.Find("AchievementListContainer");
            GameObject containerGo;

            if (existingList == null)
            {
                containerGo = new GameObject("AchievementListContainer");
                containerGo.transform.SetParent(canvas.transform, false);
                RectTransform rt = containerGo.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 30f);
                rt.sizeDelta = new Vector2(900f, 540f);

                var vlg = containerGo.AddComponent<VerticalLayoutGroup>();
                vlg.spacing = 10f;
                vlg.childControlWidth = true;
                vlg.childControlHeight = false;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;
            }
            else
            {
                containerGo = existingList.gameObject;
            }

            // Tạo 6 dòng UI tái sử dụng cho cả 2 trang
            for (int i = 0; i < 6; i++)
            {
                string rowName = $"Row_{i}";
                Transform existingRow = containerGo.transform.Find(rowName);
                GameObject rowGo;

                if (existingRow == null)
                {
                    rowGo = new GameObject(rowName);
                    rowGo.transform.SetParent(containerGo.transform, false);

                    RectTransform rRt = rowGo.AddComponent<RectTransform>();
                    rRt.sizeDelta = new Vector2(900f, 75f);

                    Image img = rowGo.AddComponent<Image>();
                    img.color = new Color(0.08f, 0.14f, 0.28f, 0.90f);

                    Outline outline = rowGo.AddComponent<Outline>();
                    outline.effectDistance = new Vector2(1.5f, -1.5f);
                    outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 0.6f);

                    // Title & Description
                    GameObject txtGo = new GameObject("Text");
                    txtGo.transform.SetParent(rowGo.transform, false);
                    RectTransform tRt = txtGo.AddComponent<RectTransform>();
                    tRt.anchorMin = Vector2.zero;
                    tRt.anchorMax = Vector2.one;
                    tRt.offsetMin = new Vector2(30f, 8f);
                    tRt.offsetMax = new Vector2(-120f, -8f);

                    var tmp = txtGo.AddComponent<TextMeshProUGUI>();
                    tmp.fontSize = 18;
                    tmp.alignment = TextAlignmentOptions.TopLeft;
                    tmp.textWrappingMode = TextWrappingModes.Normal;
                    tmp.raycastTarget = false;

                    // Status Badge (ĐÃ ĐẠT / KHÓA)
                    GameObject badgeGo = new GameObject("StatusBadge");
                    badgeGo.transform.SetParent(rowGo.transform, false);
                    RectTransform bRt = badgeGo.AddComponent<RectTransform>();
                    bRt.anchorMin = new Vector2(1f, 0.5f);
                    bRt.anchorMax = new Vector2(1f, 0.5f);
                    bRt.pivot = new Vector2(1f, 0.5f);
                    bRt.anchoredPosition = new Vector2(-20f, 0f);
                    bRt.sizeDelta = new Vector2(100f, 40f);

                    var bTmp = badgeGo.AddComponent<TextMeshProUGUI>();
                    bTmp.fontSize = 16;
                    bTmp.fontStyle = FontStyles.Bold;
                    bTmp.alignment = TextAlignmentOptions.Center;
                    bTmp.raycastTarget = false;
                }
                else
                {
                    rowGo = existingRow.gameObject;
                }

                _rowObjects.Add(rowGo);
            }
        }

        private void SetupActionButtons()
        {
            if (btnPrevPage != null)
            {
                UIHoverPulseEffect.Attach(btnPrevPage.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
                btnPrevPage.onClick.RemoveAllListeners();
                btnPrevPage.onClick.AddListener(OnPrevPageClicked);
            }

            if (btnNextPage != null)
            {
                UIHoverPulseEffect.Attach(btnNextPage.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
                btnNextPage.onClick.RemoveAllListeners();
                btnNextPage.onClick.AddListener(OnNextPageClicked);
            }

            if (btnBack != null)
            {
                UIHoverPulseEffect.Attach(btnBack.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackClicked);
            }
        }

        private void OnPrevPageClicked()
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                RefreshUI();
            }
        }

        private void OnNextClicked()
        {
            if (_currentPage < 2)
            {
                _currentPage++;
                RefreshUI();
            }
        }

        private void OnNextPageClicked()
        {
            OnNextClicked();
        }

        private void OnBackClicked()
        {
            Debug.Log("[AchievementUIController] Quay lại MainMenu.");
            NavigationService.Instance.NavigateTo(RouteId.MainMenu);
        }

        private void RefreshUI()
        {
            var save = PhysicsSaveManager.Load();
            HashSet<string> unlocked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (save != null && save.unlockedAchievementIds != null)
            {
                foreach (var id in save.unlockedAchievementIds) unlocked.Add(id);
            }

            // Tính tổng mở khóa thực tế X/12
            int totalUnlocked = 0;
            foreach (var ach in AllAchievements)
            {
                if (unlocked.Contains(ach.id)) totalUnlocked++;
            }

            if (progressText != null)
            {
                progressText.text = $"Đã mở khóa: <b>{totalUnlocked}/12</b>";
            }

            if (pageIndicatorText != null)
            {
                pageIndicatorText.text = $"Trang {_currentPage}/2";
            }

            // Trạng thái nút phân trang
            if (btnPrevPage != null)
            {
                btnPrevPage.interactable = (_currentPage > 1);
                var img = btnPrevPage.GetComponent<Image>();
                if (img != null) img.color = (_currentPage > 1) ? new Color(0.12f, 0.25f, 0.45f, 1f) : new Color(0.2f, 0.25f, 0.35f, 0.5f);
            }

            if (btnNextPage != null)
            {
                btnNextPage.interactable = (_currentPage < 2);
                var img = btnNextPage.GetComponent<Image>();
                if (img != null) img.color = (_currentPage < 2) ? new Color(0.12f, 0.25f, 0.45f, 1f) : new Color(0.2f, 0.25f, 0.35f, 0.5f);
            }

            // Render 6 dòng theo trang hiện tại
            int startIndex = (_currentPage - 1) * 6;
            for (int i = 0; i < 6; i++)
            {
                int achIndex = startIndex + i;
                if (achIndex >= AllAchievements.Count) break;

                var def = AllAchievements[achIndex];
                bool isUnlocked = unlocked.Contains(def.id);
                GameObject rowGo = _rowObjects[i];

                var tmp = rowGo.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
                var badgeTmp = rowGo.transform.Find("StatusBadge")?.GetComponent<TextMeshProUGUI>();
                var img = rowGo.GetComponent<Image>();

                if (tmp != null)
                {
                    string colorTag = isUnlocked ? "#F5D161" : "#8A94A6";
                    string descColor = isUnlocked ? "#FFFFFF" : "#6B7280";
                    tmp.text = $"<color={colorTag}><b>[{def.id}] {def.name}</b></color>\n<size=85%><color={descColor}>{def.description}</color></size>";
                }

                if (badgeTmp != null)
                {
                    badgeTmp.text = isUnlocked ? "<color=#38D368>✓ ĐẠT</color>" : "<color=#6B7280>🔒 KHÓA</color>";
                }

                if (img != null)
                {
                    img.color = isUnlocked
                        ? new Color(0.08f, 0.16f, 0.32f, 0.95f)
                        : new Color(0.06f, 0.10f, 0.20f, 0.75f);
                }
            }
        }
    }
}
