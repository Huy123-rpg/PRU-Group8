using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển toàn bộ logic giao diện Scene 02_CharacterSelection:
    /// - 4 nhân vật: Minh, Lan, Duy, Hà.
    /// - Click nhân vật: đổi lựa chọn, viền sáng/dấu chọn, KHÔNG tự chuyển màn.
    /// - Nút Xác nhận: lưu lựa chọn -> vào StudySessionSetup nếu returnAction == StartStudy, ngược lại về MainMenu.
    /// - Nút Quay lại: hủy thay đổi chưa xác nhận -> về MainMenu.
    /// - Tuyệt đối không điều hướng tới TeacherSelection (ngoài phạm vi 34 ảnh).
    /// </summary>
    public class CharacterSelectionController : MonoBehaviour
    {
        [Header("=== CHARACTER CARDS ===")]
        [SerializeField] private GameObject cardMinh;
        [SerializeField] private GameObject cardLan;
        [SerializeField] private GameObject cardDuy;
        [SerializeField] private GameObject cardHa;

        [Header("=== ACTION BUTTONS ===")]
        [SerializeField] private Button btnConfirm;
        [SerializeField] private Button btnBack;

        private readonly Dictionary<string, GameObject> _characterCards = new Dictionary<string, GameObject>();
        private string _pendingCharacterId = "Minh";

        private void Awake()
        {
            FindUIReferences();
            SetupCharacterButtons();
            SetupActionButtons();
        }

        private void Start()
        {
            // Lấy nhân vật đã lưu trước đó nếu có trong danh sách 4 nhân vật, mặc định chọn Minh nếu chưa có
            var save = PhysicsSaveManager.Load();
            if (save != null && !string.IsNullOrEmpty(save.selectedCharacterId))
            {
                foreach (var k in _characterCards.Keys)
                {
                    if (string.Equals(k, save.selectedCharacterId, System.StringComparison.OrdinalIgnoreCase))
                    {
                        _pendingCharacterId = k;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(_pendingCharacterId) || !_characterCards.ContainsKey(_pendingCharacterId))
            {
                _pendingCharacterId = "Minh";
            }

            HighlightSelectedCharacter();
        }

        private void FindUIReferences()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Dọn dẹp bất kỳ overlay tối hoặc loading veil nào sót lại từ NavigationService hoặc DontDestroyOnLoad
            var strayOverlay = GameObject.Find("NavigationLoadingOverlay");
            if (strayOverlay != null) strayOverlay.SetActive(false);

            // Tắt raycastTarget trên các object nền và decorative elements, giữ nguyên màu sắc gốc
            var bgHall = GameObject.Find("bg_hall")?.GetComponent<Image>();
            if (bgHall != null)
            {
                bgHall.raycastTarget = false;
                bgHall.color = Color.white;
            }

            var panel = GameObject.Find("Panel");
            if (panel != null)
            {
                var pImg = panel.GetComponent<Image>();
                if (pImg != null) pImg.raycastTarget = false;
                panel.SetActive(false); // Ẩn hoàn toàn Panel che phủ
            }

            var title = GameObject.Find("title")?.GetComponent<Image>();
            if (title != null) title.raycastTarget = false;
            var kai9 = GameObject.Find("kai (9)")?.GetComponent<Image>();
            if (kai9 != null) kai9.raycastTarget = false;

            // Tắt raycastTarget trên các nhãn tên bên dưới nhân vật để không chặn raycast của thẻ nhân vật
            var kai5 = GameObject.Find("kai (5)")?.GetComponent<Image>();
            if (kai5 != null) kai5.raycastTarget = false;
            var kai6 = GameObject.Find("kai (6)")?.GetComponent<Image>();
            if (kai6 != null) kai6.raycastTarget = false;
            var kai7 = GameObject.Find("kai (7)")?.GetComponent<Image>();
            if (kai7 != null) kai7.raycastTarget = false;
            var kai8 = GameObject.Find("kai (8)")?.GetComponent<Image>();
            if (kai8 != null) kai8.raycastTarget = false;

            // Tìm 4 nhân vật theo tên scene hoặc tọa độ X
            if (cardMinh == null) cardMinh = GameObject.Find("kai (2)");
            if (cardLan == null) cardLan = GameObject.Find("kai");
            if (cardDuy == null) cardDuy = GameObject.Find("kai (1)");
            if (cardHa == null) cardHa = GameObject.Find("kai (3)");

            // Fallback: Quét tất cả RectTransform nếu tên bị đổi
            if (cardMinh == null || cardLan == null || cardDuy == null || cardHa == null)
            {
                RectTransform[] allRts = canvas.GetComponentsInChildren<RectTransform>(true);
                foreach (var rt in allRts)
                {
                    if (rt.name.StartsWith("kai") && rt.sizeDelta.y > 400f)
                    {
                        float x = rt.anchoredPosition.x;
                        if (x < -400f && cardMinh == null) cardMinh = rt.gameObject;
                        else if (x >= -400f && x < 0f && cardLan == null) cardLan = rt.gameObject;
                        else if (x >= 0f && x < 400f && cardDuy == null) cardDuy = rt.gameObject;
                        else if (x >= 400f && cardHa == null) cardHa = rt.gameObject;
                    }
                }
            }

            if (cardMinh != null) _characterCards["Minh"] = cardMinh;
            if (cardLan != null) _characterCards["Lan"] = cardLan;
            if (cardDuy != null) _characterCards["Duy"] = cardDuy;
            if (cardHa != null) _characterCards["Ha"] = cardHa;

            // Tìm nút Xác nhận (kai (4) ở Y ~ -505)
            if (btnConfirm == null)
            {
                var confirmGo = GameObject.Find("kai (4)") ?? GameObject.Find("Btn_Confirm");
                if (confirmGo != null) btnConfirm = confirmGo.GetComponent<Button>() ?? confirmGo.AddComponent<Button>();
            }

            // Tìm nút Quay lại (quaylai)
            if (btnBack == null)
            {
                var backGo = GameObject.Find("quaylai") ?? GameObject.Find("Btn_Back");
                if (backGo != null) btnBack = backGo.GetComponent<Button>() ?? backGo.AddComponent<Button>();
            }

            // Đảm bảo nút Xác nhận có giao diện rõ ràng, không bị chồng chữ
            if (btnConfirm != null)
            {
                var cImg = btnConfirm.GetComponent<Image>();
                if (cImg != null)
                {
                    cImg.raycastTarget = true;
                    cImg.color = Color.white;
                }
                btnConfirm.targetGraphic = cImg;

                // Kiểm tra sprite của nút Xác nhận:
                // Sprite gốc chibi_8 trong "Bộ giao diện chọn nhân vật fantasy chibi" đã vẽ sẵn chữ "XÁC NHẬN"
                // với kiểu chữ serif viền vàng hoàng gia rất sắc nét và chuẩn thiết kế.
                // Nếu sprite đã có sẵn chữ đẹp thì giữ nguyên sprite và loại bỏ/ẩn mọi component Text/TextMeshPro
                // tạo thêm để không bị chồng chữ (chữ đè lên nhau).
                // Nếu dùng sprite trống chưa có chữ thì mới giữ duy nhất 1 TextMeshPro căn chính giữa.
                bool spriteHasBakedText = false;
                if (cImg != null && cImg.sprite != null)
                {
                    string sName = cImg.sprite.name.ToLower();
                    if (sName.Contains("chibi_8") || sName.Contains("xac") || sName.Contains("confirm") || sName.Contains("bộ giao diện chọn nhân vật"))
                    {
                        spriteHasBakedText = true;
                    }
                    else
                    {
                        // Mặc định sprite nút xác nhận kai (4) trong scene 02_CharacterSelection đã có chữ
                        spriteHasBakedText = true;
                    }
                }

                // Dọn dẹp/quản lý các lớp Text con để đảm bảo không bị trùng lặp hoặc đè chữ
                var tmps = btnConfirm.GetComponentsInChildren<TextMeshProUGUI>(true);
                var legacyTexts = btnConfirm.GetComponentsInChildren<UnityEngine.UI.Text>(true);

                if (spriteHasBakedText)
                {
                    // Sprite đã có sẵn chữ chuẩn đẹp -> Gỡ bỏ/ẩn tất cả layer Text thừa
                    foreach (var tmp in tmps)
                    {
                        if (tmp != null)
                        {
                            tmp.gameObject.SetActive(false);
                            if (Application.isPlaying) Destroy(tmp.gameObject);
                            else DestroyImmediate(tmp.gameObject);
                        }
                    }
                    foreach (var txt in legacyTexts)
                    {
                        if (txt != null)
                        {
                            txt.gameObject.SetActive(false);
                            if (Application.isPlaying) Destroy(txt.gameObject);
                            else DestroyImmediate(txt.gameObject);
                        }
                    }
                }
                else
                {
                    // Sprite trống -> Sử dụng đúng 1 TextMeshPro duy nhất căn chính giữa
                    TextMeshProUGUI primaryTmp = (tmps.Length > 0) ? tmps[0] : null;

                    for (int i = 1; i < tmps.Length; i++)
                    {
                        if (tmps[i] != null)
                        {
                            tmps[i].gameObject.SetActive(false);
                            if (Application.isPlaying) Destroy(tmps[i].gameObject);
                            else DestroyImmediate(tmps[i].gameObject);
                        }
                    }
                    foreach (var txt in legacyTexts)
                    {
                        if (txt != null)
                        {
                            txt.gameObject.SetActive(false);
                            if (Application.isPlaying) Destroy(txt.gameObject);
                            else DestroyImmediate(txt.gameObject);
                        }
                    }

                    if (primaryTmp == null)
                    {
                        GameObject txtGo = new GameObject("Text_Confirm", typeof(RectTransform));
                        txtGo.transform.SetParent(btnConfirm.transform, false);
                        primaryTmp = txtGo.AddComponent<TextMeshProUGUI>();
                    }

                    primaryTmp.gameObject.SetActive(true);
                    primaryTmp.text = "XÁC NHẬN";
                    primaryTmp.fontSize = 24;
                    primaryTmp.fontStyle = FontStyles.Bold;
                    primaryTmp.alignment = TextAlignmentOptions.Center;
                    primaryTmp.color = new Color(0.1f, 0.15f, 0.3f, 1f);
                    primaryTmp.raycastTarget = false;
                    RectTransform txtRt = primaryTmp.rectTransform;
                    txtRt.anchorMin = Vector2.zero;
                    txtRt.anchorMax = Vector2.one;
                    txtRt.offsetMin = Vector2.zero;
                    txtRt.offsetMax = Vector2.zero;
                }
            }
        }

        private void SetupCharacterButtons()
        {
            foreach (var kvp in _characterCards)
            {
                string charId = kvp.Key;
                GameObject go = kvp.Value;
                if (go == null) continue;

                Button btn = go.GetComponent<Button>();
                if (btn == null)
                {
                    btn = go.AddComponent<Button>();
                }
                Image img = go.GetComponent<Image>();
                if (img != null)
                {
                    img.raycastTarget = true;
                    img.color = Color.white; // Luôn giữ màu gốc bản thiết kế
                    btn.targetGraphic = img;
                }

                // Gỡ UIButtonFeedback cũ tránh xung đột hiệu ứng
                var fb = go.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);

                // Gắn hiệu ứng UIHoverPulseEffect
                UIHoverPulseEffect hover = UIHoverPulseEffect.Attach(go, hover: 1.08f, minPulse: 1.05f, maxPulse: 1.10f, outline: true);

                foreach (var t in go.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    t.raycastTarget = false;
                }

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnCharacterClicked(charId));
            }
        }

        private void SetupActionButtons()
        {
            if (btnConfirm != null)
            {
                // Gỡ UIButtonFeedback cũ tránh xung đột hiệu ứng
                var fb = btnConfirm.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);

                UIHoverPulseEffect.Attach(btnConfirm.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);

                btnConfirm.onClick.RemoveAllListeners();
                btnConfirm.onClick.AddListener(OnConfirmClicked);
            }

            if (btnBack != null)
            {
                var bImg = btnBack.GetComponent<Image>();
                if (bImg != null)
                {
                    bImg.raycastTarget = true;
                    bImg.color = Color.white;
                }
                btnBack.targetGraphic = bImg;

                var fb = btnBack.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);

                UIHoverPulseEffect.Attach(btnBack.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);

                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackClicked);
            }
        }

        private void OnCharacterClicked(string charId)
        {
            Debug.Log($"[CharacterSelectionController] Đã chọn nhân vật: {charId} (chưa xác nhận).");
            _pendingCharacterId = charId;
            HighlightSelectedCharacter();
        }

        private void HighlightSelectedCharacter()
        {
            foreach (var kvp in _characterCards)
            {
                string charId = kvp.Key;
                GameObject go = kvp.Value;
                if (go == null) continue;

                bool isSelected = (charId == _pendingCharacterId);
                Image img = go.GetComponent<Image>();
                if (img != null)
                {
                    // GIỮ NGUYÊN ĐỘ SÁNG THIẾT KẾ CHO TẤT CẢ NHÂN VẬT (Color.white),
                    // KHÔNG LÀM TỐI/XÁM CÁC NHÂN VẬT CHƯA CHỌN (0.7f, 0.7f, 0.7f)
                    img.color = Color.white;
                }

                // Cập nhật trạng thái qua UIHoverPulseEffect:
                // Nhân vật được chọn có viền sáng hoàng gia và scale nổi bật
                // Nhân vật khác trở về trạng thái bình thường (100% scale, không viền),
                // khi di chuột vào thì phóng to và nhấp nhô nhẹ nhàng
                var hover = go.GetComponent<UIHoverPulseEffect>();
                if (hover == null) hover = UIHoverPulseEffect.Attach(go, hover: 1.08f, minPulse: 1.05f, maxPulse: 1.10f, outline: true);
                hover.SetSelected(isSelected);
            }
        }

        private void OnConfirmClicked()
        {
            Debug.Log($"[CharacterSelectionController] Xác nhận chọn nhân vật: {_pendingCharacterId}");

            // 1. Lưu vào SaveData và GameSession
            PhysicsSaveData save = PhysicsSaveManager.Load() ?? new PhysicsSaveData();
            save.selectedCharacterId = _pendingCharacterId;
            PhysicsSaveManager.Save(save);

            if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.SelectCharacter(_pendingCharacterId);
            }

            if (NavigationService.Instance != null && NavigationService.Instance.GameContext != null)
            {
                NavigationService.Instance.GameContext.CharacterId = _pendingCharacterId;
            }

            // 2. Chuyển sang màn 22. Buổi học hôm nay (StudySessionSetup)
            Debug.Log("[CharacterSelectionController] Đã lưu nhân vật -> Chuyển sang màn 22. Buổi học hôm nay.");
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.NavigateTo(RouteId.StudySessionSetup);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(PhysicsSceneNames.TodayStudy);
            }
        }

        private void OnBackClicked()
        {
            Debug.Log("[CharacterSelectionController] Quay lại MainMenu (hủy thay đổi chưa xác nhận).");
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.NavigateTo(RouteId.MainMenu);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(PhysicsSceneNames.MainMenu);
            }
        }
    }
}
