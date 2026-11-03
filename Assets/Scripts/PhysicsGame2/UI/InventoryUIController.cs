using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển toàn bộ logic hiển thị và tương tác của Scene 13_Inventory.
    /// Tự động liên kết 12 ô vật phẩm với dữ liệu thật trong PhysicsSaveData.unlockedItemIds.
    /// Hiển thị Popup chi tiết vật phẩm và xử lý nút Quay lại an toàn.
    /// </summary>
    public class InventoryUIController : MonoBehaviour
    {
        [Header("=== UI REFERENCES ===")]
        [SerializeField] private Button btnBack;

        private TMP_FontAsset _defaultFont;

        // Modal Popup Chi tiết vật phẩm
        private GameObject _popupRoot;
        private TextMeshProUGUI _popupTitleText;
        private TextMeshProUGUI _popupDescText;
        private TextMeshProUGUI _popupChapterText;
        private TextMeshProUGUI _popupSourceText;

        // Ánh xạ từ Tên nhãn Text UI sang Item ID chuẩn
        private static readonly Dictionary<string, string> LabelToItemId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Hộp điện", "electrical_box" },
            { "Công tắc", "switch" },
            { "Gương", "mirror" },
            { "Thấu kính", "lens" },
            { "Lăng kính", "prism" },
            { "Nam châm", "magnet" },
            { "Turbune", "turbine" },
            { "Turbine", "turbine" },
            { "Tấm pin mặt trời", "solar_panel" },
            { "Pin", "battery" },
            { "Dây dẫn", "wire" },
            { "Bánh răng", "gear" },
            { "Tinh thể năng lượng", "energy_crystal" }
        };

        private void Awake()
        {
            EnsureEventSystem();
            BuildPopupModalIfMissing();
            SetupBackButton();
        }

        private void Start()
        {
            PhysicsFinalProgression.EvaluateFinalProgression();
            RefreshInventoryDisplay();
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }

        private void SetupBackButton()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Tìm nút Back bên góc trái dưới (pos.x < 0 && pos.y < -300)
            Button[] allButtons = canvas.GetComponentsInChildren<Button>(true);
            foreach (var b in allButtons)
            {
                Vector2 pos = b.GetComponent<RectTransform>().anchoredPosition;
                if (pos.x < 0 && pos.y < -300)
                {
                    btnBack = b;
                    break;
                }
            }

            if (btnBack != null)
            {
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackClicked);

                if (btnBack.GetComponent<UIButtonFeedback>() == null)
                {
                    btnBack.gameObject.AddComponent<UIButtonFeedback>();
                }
            }
        }

        private void OnBackClicked()
        {
            string prev = PhysicsGameSession.Instance.PreviousSceneName;
            if (string.IsNullOrEmpty(prev) || prev == PhysicsSceneNames.Inventory)
            {
                prev = PhysicsSceneNames.MainMenu;
            }
            Debug.Log($"[InventoryUIController] Quay lại cảnh trước: {prev}");
            PhysicsGameSession.Instance.LoadScene(prev);
        }

        /// <summary>
        /// Rà soát 12 Button vật phẩm trên Canvas và cập nhật trạng thái Locked/Unlocked theo SaveData.
        /// </summary>
        public void RefreshInventoryDisplay()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            var save = PhysicsSaveManager.Load();
            HashSet<string> unlockedSet = new HashSet<string>(save != null && save.unlockedItemIds != null ? save.unlockedItemIds : new List<string>());

            Button[] allButtons = canvas.GetComponentsInChildren<Button>(true);

            foreach (var btn in allButtons)
            {
                // Bỏ qua các nút điều hướng phía dưới
                Vector2 pos = btn.GetComponent<RectTransform>().anchoredPosition;
                if (pos.y < -300) continue;

                // Tìm TextMeshProUGUI con
                TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp == null) continue;

                if (_defaultFont == null) _defaultFont = tmp.font;

                string rawText = tmp.text != null ? tmp.text.Trim().Replace("\"", "") : "";
                string itemId = null;

                foreach (var kvp in LabelToItemId)
                {
                    if (rawText.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        itemId = kvp.Value;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(itemId)) continue;

                bool isUnlocked = unlockedSet.Contains(itemId);
                ApplySlotVisual(btn, tmp, isUnlocked, itemId);
            }
        }

        private void ApplySlotVisual(Button btn, TextMeshProUGUI label, bool isUnlocked, string itemId)
        {
            Image btnImg = btn.GetComponent<Image>();

            if (isUnlocked)
            {
                // Trạng thái ĐÃ MỞ KHÓA: Sáng rõ, màu đầy đủ, chữ vàng/trắng
                if (btnImg != null)
                {
                    btnImg.color = Color.white;
                }
                if (label != null)
                {
                    label.color = new Color(1f, 0.9f, 0.5f, 1f); // Aurelia Gold
                }

                // Gắn listener xem chi tiết
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ShowItemDetailPopup(itemId));
            }
            else
            {
                // Trạng thái KHÓA: Màu tối mờ (alpha 0.45), chữ xám
                if (btnImg != null)
                {
                    btnImg.color = new Color(0.45f, 0.45f, 0.55f, 0.45f);
                }
                if (label != null)
                {
                    label.color = new Color(0.6f, 0.65f, 0.75f, 0.6f);
                }

                // Gắn listener thông báo chưa mở
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ShowLockedItemPopup(itemId));
            }

            if (btn.GetComponent<UIButtonFeedback>() == null)
            {
                btn.gameObject.AddComponent<UIButtonFeedback>();
            }
        }

        private void ShowItemDetailPopup(string itemId)
        {
            var item = PhysicsItemData.Get(itemId);
            if (item == null || _popupRoot == null) return;

            if (_popupTitleText != null) _popupTitleText.text = item.displayName;
            if (_popupDescText != null) _popupDescText.text = $"\"{item.description}\"";
            if (_popupChapterText != null) _popupChapterText.text = $"<b>Chương:</b> {item.chapterAssociation}";
            if (_popupSourceText != null) _popupSourceText.text = $"<b>Nguồn gốc:</b> {item.sourceHint}";

            _popupRoot.SetActive(true);
        }

        private void ShowLockedItemPopup(string itemId)
        {
            var item = PhysicsItemData.Get(itemId);
            if (_popupRoot == null) return;

            string name = item != null ? item.displayName : "Vật phẩm";
            if (_popupTitleText != null) _popupTitleText.text = $"{name} (Chưa mở khóa)";
            if (_popupDescText != null) _popupDescText.text = "Vật phẩm này chưa được mở khóa.";
            if (_popupChapterText != null) _popupChapterText.text = item != null ? $"<b>Liên quan:</b> {item.chapterAssociation}" : "";
            if (_popupSourceText != null) _popupSourceText.text = item != null ? $"<b>Gợi ý:</b> {item.sourceHint}" : "";

            _popupRoot.SetActive(true);
        }

        private void BuildPopupModalIfMissing()
        {
            if (_popupRoot != null) return;

            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Transform existing = canvas.transform.Find("ItemDetailPopup");
            if (existing != null)
            {
                _popupRoot = existing.gameObject;
                _popupTitleText = existing.Find("Card/Title")?.GetComponent<TextMeshProUGUI>();
                _popupDescText = existing.Find("Card/Desc")?.GetComponent<TextMeshProUGUI>();
                _popupChapterText = existing.Find("Card/Chapter")?.GetComponent<TextMeshProUGUI>();
                _popupSourceText = existing.Find("Card/Source")?.GetComponent<TextMeshProUGUI>();
                _popupRoot.SetActive(false);
                return;
            }

            // 1. Root Scrim
            _popupRoot = new GameObject("ItemDetailPopup");
            _popupRoot.transform.SetParent(canvas.transform, false);
            RectTransform rootRt = _popupRoot.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.sizeDelta = Vector2.zero;

            Image scrim = _popupRoot.AddComponent<Image>();
            scrim.color = new Color(0.02f, 0.05f, 0.12f, 0.88f);
            scrim.raycastTarget = true;

            // 2. Card
            GameObject card = new GameObject("Card");
            card.transform.SetParent(_popupRoot.transform, false);
            RectTransform cardRt = card.AddComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(660, 420);
            cardRt.anchoredPosition = Vector2.zero;

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.07f, 0.12f, 0.24f, 0.98f);

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.85f, 0.35f, 0.95f);
            outline.effectDistance = new Vector2(3, -3);

            // Title
            GameObject titleGo = new GameObject("Title");
            titleGo.transform.SetParent(card.transform, false);
            RectTransform titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0, 140);
            titleRt.sizeDelta = new Vector2(580, 50);

            _popupTitleText = titleGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) _popupTitleText.font = _defaultFont;
            _popupTitleText.fontSize = 32;
            _popupTitleText.fontStyle = FontStyles.Bold;
            _popupTitleText.alignment = TextAlignmentOptions.Center;
            _popupTitleText.color = new Color(1f, 0.85f, 0.35f, 1f);

            // Description
            GameObject descGo = new GameObject("Desc");
            descGo.transform.SetParent(card.transform, false);
            RectTransform descRt = descGo.AddComponent<RectTransform>();
            descRt.anchoredPosition = new Vector2(0, 60);
            descRt.sizeDelta = new Vector2(580, 80);

            _popupDescText = descGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) _popupDescText.font = _defaultFont;
            _popupDescText.fontSize = 22;
            _popupDescText.alignment = TextAlignmentOptions.Center;
            _popupDescText.color = Color.white;
            _popupDescText.textWrappingMode = TextWrappingModes.Normal;

            // Chapter
            GameObject chGo = new GameObject("Chapter");
            chGo.transform.SetParent(card.transform, false);
            RectTransform chRt = chGo.AddComponent<RectTransform>();
            chRt.anchoredPosition = new Vector2(0, -20);
            chRt.sizeDelta = new Vector2(580, 40);

            _popupChapterText = chGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) _popupChapterText.font = _defaultFont;
            _popupChapterText.fontSize = 20;
            _popupChapterText.alignment = TextAlignmentOptions.Center;
            _popupChapterText.color = new Color(0.75f, 0.85f, 1f, 1f);

            // Source
            GameObject srcGo = new GameObject("Source");
            srcGo.transform.SetParent(card.transform, false);
            RectTransform srcRt = srcGo.AddComponent<RectTransform>();
            srcRt.anchoredPosition = new Vector2(0, -65);
            srcRt.sizeDelta = new Vector2(580, 40);

            _popupSourceText = srcGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) _popupSourceText.font = _defaultFont;
            _popupSourceText.fontSize = 19;
            _popupSourceText.alignment = TextAlignmentOptions.Center;
            _popupSourceText.color = new Color(0.85f, 0.85f, 0.85f, 0.85f);

            // Close Button
            GameObject btnCloseGo = new GameObject("Btn_Close");
            btnCloseGo.transform.SetParent(card.transform, false);
            RectTransform closeRt = btnCloseGo.AddComponent<RectTransform>();
            closeRt.anchoredPosition = new Vector2(0, -145);
            closeRt.sizeDelta = new Vector2(160, 48);

            Image closeImg = btnCloseGo.AddComponent<Image>();
            closeImg.color = new Color(0.18f, 0.3f, 0.52f, 1f);

            Button closeBtn = btnCloseGo.AddComponent<Button>();
            closeBtn.targetGraphic = closeImg;
            closeBtn.onClick.AddListener(() => _popupRoot.SetActive(false));
            btnCloseGo.AddComponent<UIButtonFeedback>();

            GameObject closeTxtGo = new GameObject("Text");
            closeTxtGo.transform.SetParent(btnCloseGo.transform, false);
            RectTransform closeTxtRt = closeTxtGo.AddComponent<RectTransform>();
            closeTxtRt.anchorMin = Vector2.zero;
            closeTxtRt.anchorMax = Vector2.one;
            closeTxtRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI closeTmp = closeTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) closeTmp.font = _defaultFont;
            closeTmp.text = "Đóng";
            closeTmp.fontSize = 22;
            closeTmp.fontStyle = FontStyles.Bold;
            closeTmp.alignment = TextAlignmentOptions.Center;
            closeTmp.color = Color.white;

            _popupRoot.SetActive(false);
        }
    }
}
