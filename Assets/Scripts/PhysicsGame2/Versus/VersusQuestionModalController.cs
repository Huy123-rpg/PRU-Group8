using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển Popup câu hỏi tại các trụ trong Đấu trường Đối kháng 1vs1.
    /// KẾT NỐI TRỰC TIẾP VỚI CÁC TEXTMESHPRO VÀ BUTTON DO NGƯỜI DÙNG TỰ THIẾT KẾ TRONG UNITY EDITOR.
    /// Tuân thủ nghiêm ngặt:
    /// - KHÔNG tạo TMP runtime mới đè lên thiết kế.
    /// - Căn chỉnh chữ câu hỏi và 4 đáp án vừa vặn hoàn hảo trong khung fantasy theo đúng Ảnh 2.
    /// - Đề bài nằm gọn trong vùng câu hỏi (y ~ 175), font size 24, auto-sizing không tràn khung.
    /// - 4 đáp án nằm đúng trong 4 thanh capsule, lệch sang phải để tránh icon oval tròn bên trái.
    /// - Nút Xác nhận và Đóng hoạt động 100%.
    /// - Trả lời SAI: Hiện "Sai rồi! Hãy thử lại.", không cộng điểm, không chiếm trụ, cho chọn lại ngay.
    /// - Trả lời ĐÚNG: Hiện "CHÍNH XÁC! EM ĐÃ CHINH PHỤC TRỤ NÀY.", +100 điểm, chiếm trụ màu xanh.
    /// - Ẩn/Hiện đồng bộ toàn bộ 15 GameObjects popup trên Canvas.
    /// </summary>
    public class VersusQuestionModalController : MonoBehaviour
    {
        [Header("=== USER DESIGNED TEXTMESHPRO REFERENCES ===")]
        [SerializeField] private TextMeshProUGUI txtTowerTitle;
        [SerializeField] private TextMeshProUGUI txtQuestion;
        [SerializeField] private TextMeshProUGUI txtOptionA;
        [SerializeField] private TextMeshProUGUI txtOptionB;
        [SerializeField] private TextMeshProUGUI txtOptionC;
        [SerializeField] private TextMeshProUGUI txtOptionD;

        [Header("=== USER DESIGNED BUTTON REFERENCES ===")]
        [SerializeField] private Button btnOptionA;
        [SerializeField] private Button btnOptionB;
        [SerializeField] private Button btnOptionC;
        [SerializeField] private Button btnOptionD;
        [SerializeField] private Button btnConfirm;
        [SerializeField] private Button btnClose;

        [Header("=== USER DESIGNED VISUAL FRAMES & HIGHLIGHTS ===")]
        [SerializeField] private Image imgOptionA;
        [SerializeField] private Image imgOptionB;
        [SerializeField] private Image imgOptionC;
        [SerializeField] private Image imgOptionD;
        [SerializeField] private Outline outlineOptionA;
        [SerializeField] private Outline outlineOptionB;
        [SerializeField] private Outline outlineOptionC;
        [SerializeField] private Outline outlineOptionD;

        [Header("=== FEEDBACK & TOGGLE ELEMENTS ===")]
        [SerializeField] private TextMeshProUGUI txtFeedback;
        [SerializeField] private TextMeshProUGUI txtClose;
        [SerializeField] private GameObject[] popupElements;

        private int _currentTowerNumber = 1;
        private PhysicsQuestionRaw _currentQuestion;
        private int _selectedOptionIndex = -1;
        private bool _isSubmitting = false;
        private bool _isPopupActive = false;

        public bool IsOpen => _isPopupActive;
        public int CurrentTowerNumber => _currentTowerNumber;
        public int SelectedOptionIndex => _selectedOptionIndex;
        public PhysicsQuestionRaw CurrentQuestion => _currentQuestion;

        public TextMeshProUGUI TxtTowerTitle => txtTowerTitle;
        public TextMeshProUGUI TxtQuestion => txtQuestion;
        public TextMeshProUGUI TxtOptionA => txtOptionA;
        public TextMeshProUGUI TxtOptionB => txtOptionB;
        public TextMeshProUGUI TxtOptionC => txtOptionC;
        public TextMeshProUGUI TxtOptionD => txtOptionD;

        public Button BtnOptionA => btnOptionA;
        public Button BtnOptionB => btnOptionB;
        public Button BtnOptionC => btnOptionC;
        public Button BtnOptionD => btnOptionD;
        public Button BtnConfirm => btnConfirm;
        public Button BtnClose => btnClose;

        public Image ImgOptionA => imgOptionA;
        public Image ImgOptionB => imgOptionB;
        public Image ImgOptionC => imgOptionC;
        public Image ImgOptionD => imgOptionD;

        private void Awake()
        {
            AutoBindUI();
            SetPopupVisible(false);
        }

        private void Start()
        {
            if (!_isPopupActive)
            {
                SetPopupVisible(false);
            }
        }

        public void AutoBindUI()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            var allTMPs = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTMPs)
            {
                string n = tmp.gameObject.name;
                string t = tmp.text != null ? tmp.text.Trim() : "";

                if (txtTowerTitle == null && (n == "Txt_TowerTitle" || t.StartsWith("TRỤ SỐ") || t.StartsWith("TRU SO") || (tmp.fontSize > 40f && tmp.rectTransform.anchoredPosition.y > 220f)))
                {
                    txtTowerTitle = tmp;
                    txtTowerTitle.raycastTarget = false;
                }
                else if (txtQuestion == null && (n == "Txt_Question" || t.Contains("tia sáng") || (tmp.rectTransform.sizeDelta.x > 700f && tmp.rectTransform.anchoredPosition.y > 120f && tmp.rectTransform.anchoredPosition.y < 220f)))
                {
                    txtQuestion = tmp;
                    txtQuestion.raycastTarget = false;
                }
                else if (txtOptionA == null && (n == "Txt_OptionA" || (tmp.rectTransform.anchoredPosition.y > 50f && tmp.rectTransform.anchoredPosition.y < 120f && tmp.rectTransform.sizeDelta.x > 350f)))
                {
                    txtOptionA = tmp;
                    txtOptionA.raycastTarget = false;
                }
                else if (txtOptionB == null && (n == "Txt_OptionB" || (tmp.rectTransform.anchoredPosition.y > -40f && tmp.rectTransform.anchoredPosition.y <= 50f && tmp.rectTransform.sizeDelta.x > 350f)))
                {
                    txtOptionB = tmp;
                    txtOptionB.raycastTarget = false;
                }
                else if (txtOptionC == null && (n == "Txt_OptionC" || (tmp.rectTransform.anchoredPosition.y > -140f && tmp.rectTransform.anchoredPosition.y <= -40f && tmp.rectTransform.sizeDelta.x > 350f)))
                {
                    txtOptionC = tmp;
                    txtOptionC.raycastTarget = false;
                }
                else if (txtOptionD == null && (n == "Txt_OptionD" || (tmp.rectTransform.anchoredPosition.y > -220f && tmp.rectTransform.anchoredPosition.y <= -140f && tmp.rectTransform.sizeDelta.x > 350f)))
                {
                    txtOptionD = tmp;
                    txtOptionD.raycastTarget = false;
                }
                else if (txtClose == null && (n == "Txt_Close" || t == "Đóng" || t == "ĐÓNG"))
                {
                    txtClose = tmp;
                    txtClose.raycastTarget = false;
                }
            }

            var allImages = canvas.GetComponentsInChildren<Image>(true);
            foreach (var img in allImages)
            {
                string n = img.gameObject.name;
                var pos = img.rectTransform.anchoredPosition;
                var size = img.rectTransform.sizeDelta;

                if (size.x > 600f && size.y > 60f)
                {
                    if (pos.y > 50f && pos.y < 130f && (n == "Btn_OptionA" || imgOptionA == null))
                    {
                        imgOptionA = img;
                        btnOptionA = SetupOptionButton(img.gameObject, 0);
                    }
                    else if (pos.y > -40f && pos.y <= 50f && (n == "Btn_OptionB" || imgOptionB == null))
                    {
                        imgOptionB = img;
                        btnOptionB = SetupOptionButton(img.gameObject, 1);
                    }
                    else if (pos.y > -140f && pos.y <= -40f && (n == "Btn_OptionC" || imgOptionC == null))
                    {
                        imgOptionC = img;
                        btnOptionC = SetupOptionButton(img.gameObject, 2);
                    }
                    else if (pos.y > -220f && pos.y <= -140f && (n == "Btn_OptionD" || imgOptionD == null))
                    {
                        imgOptionD = img;
                        btnOptionD = SetupOptionButton(img.gameObject, 3);
                    }
                }
                else if (btnConfirm == null && (n == "Btn_Confirm" || (pos.y < -250f && pos.x > 50f && size.x < 350f)))
                {
                    btnConfirm = img.gameObject.GetComponent<Button>() ?? img.gameObject.AddComponent<Button>();
                    btnConfirm.targetGraphic = img;
                    img.raycastTarget = true;
                }
                else if (btnClose == null && (n == "Btn_Close" || (pos.y < -250f && pos.x < -50f && size.x < 350f)))
                {
                    btnClose = img.gameObject.GetComponent<Button>() ?? img.gameObject.AddComponent<Button>();
                    btnClose.targetGraphic = img;
                    img.raycastTarget = true;
                }
            }

            // Gán listener nút
            if (btnOptionA != null) { btnOptionA.onClick.RemoveAllListeners(); btnOptionA.onClick.AddListener(() => SelectOption(0)); }
            if (btnOptionB != null) { btnOptionB.onClick.RemoveAllListeners(); btnOptionB.onClick.AddListener(() => SelectOption(1)); }
            if (btnOptionC != null) { btnOptionC.onClick.RemoveAllListeners(); btnOptionC.onClick.AddListener(() => SelectOption(2)); }
            if (btnOptionD != null) { btnOptionD.onClick.RemoveAllListeners(); btnOptionD.onClick.AddListener(() => SelectOption(3)); }

            if (btnConfirm != null) { btnConfirm.onClick.RemoveAllListeners(); btnConfirm.onClick.AddListener(OnConfirmClicked); }
            if (btnClose != null) { btnClose.onClick.RemoveAllListeners(); btnClose.onClick.AddListener(Close); }

            // Outlines
            if (imgOptionA != null && outlineOptionA == null) outlineOptionA = imgOptionA.GetComponent<Outline>() ?? imgOptionA.gameObject.AddComponent<Outline>();
            if (imgOptionB != null && outlineOptionB == null) outlineOptionB = imgOptionB.GetComponent<Outline>() ?? imgOptionB.gameObject.AddComponent<Outline>();
            if (imgOptionC != null && outlineOptionC == null) outlineOptionC = imgOptionC.GetComponent<Outline>() ?? imgOptionC.gameObject.AddComponent<Outline>();
            if (imgOptionD != null && outlineOptionD == null) outlineOptionD = imgOptionD.GetComponent<Outline>() ?? imgOptionD.gameObject.AddComponent<Outline>();

            ConfigOutline(outlineOptionA);
            ConfigOutline(outlineOptionB);
            ConfigOutline(outlineOptionC);
            ConfigOutline(outlineOptionD);

            // Đảm bảo không text nào chặn click
            if (txtTowerTitle != null) txtTowerTitle.raycastTarget = false;
            if (txtQuestion != null) txtQuestion.raycastTarget = false;
            if (txtOptionA != null) txtOptionA.raycastTarget = false;
            if (txtOptionB != null) txtOptionB.raycastTarget = false;
            if (txtOptionC != null) txtOptionC.raycastTarget = false;
            if (txtOptionD != null) txtOptionD.raycastTarget = false;
            if (txtClose != null) txtClose.raycastTarget = false;

            // Căn chỉnh Layout tinh tế chuẩn theo Ảnh 2:
            ApplyCleanLayout();

            // Tự động thu thập 15 phần tử popup
            if (popupElements == null || popupElements.Length == 0)
            {
                var list = new List<GameObject>();
                string[] targetNames = new string[]
                {
                    "PopupFrame", "BannerHeader", "Txt_TowerTitle", "Txt_Question",
                    "Btn_OptionA", "Txt_OptionA", "Btn_OptionB", "Txt_OptionB",
                    "Btn_OptionC", "Txt_OptionC", "Btn_OptionD", "Txt_OptionD",
                    "Btn_Close", "Txt_Close", "Btn_Confirm"
                };
                foreach (var tName in targetNames)
                {
                    var t = canvas.transform.Find(tName);
                    if (t != null) list.Add(t.gameObject);
                }
                if (list.Count > 0)
                {
                    popupElements = list.ToArray();
                }
            }

            // Ép thứ tự hiển thị phân lớp chuẩn ngay từ khi bind
            EnforcePopupHierarchyOrder();
        }

        private Transform FindElementTransform(string name)
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return null;
            return canvas.transform.Find(name);
        }

        /// <summary>
        /// Ép thứ tự Sibling Index tuyệt đối của 15 phần tử popup trên Canvas:
        /// Thứ tự phân lớp chuẩn từ dưới lên trên:
        /// 1. PopupFrame (nền to nhất 1460x820 vẽ ĐẦU TIÊN trong popup)
        /// 2. BannerHeader (khiên header)
        /// 3. Txt_TowerTitle (chữ TRỤ SỐ X - VẼ SAU BannerHeader để luôn nổi trên mặt khiên)
        /// 4. Txt_Question (nội dung đề bài)
        /// 5. Btn_OptionA
        /// 6. Txt_OptionA (chữ đáp án A - VẼ SAU capsule A)
        /// 7. Btn_OptionB
        /// 8. Txt_OptionB (chữ đáp án B - VẼ SAU capsule B)
        /// 9. Btn_OptionC
        /// 10. Txt_OptionC (chữ đáp án C - VẼ SAU capsule C)
        /// 11. Btn_OptionD
        /// 12. Txt_OptionD (chữ đáp án D - VẼ SAU capsule D)
        /// 13. Btn_Close (nút đóng)
        /// 14. Txt_Close (chữ Đóng - VẼ SAU nút đóng)
        /// 15. Btn_Confirm (nút xác nhận)
        /// </summary>
        public void EnforcePopupHierarchyOrder()
        {
            Transform canvasTransform = transform.parent;
            if (canvasTransform == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
                if (canvas != null) canvasTransform = canvas.transform;
            }
            if (canvasTransform == null) return;

            Transform[] layerOrder = new Transform[]
            {
                transform, // PopupFrame
                FindElementTransform("BannerHeader"),
                txtTowerTitle != null ? txtTowerTitle.transform : FindElementTransform("Txt_TowerTitle"),
                txtQuestion != null ? txtQuestion.transform : FindElementTransform("Txt_Question"),
                btnOptionA != null ? btnOptionA.transform : FindElementTransform("Btn_OptionA"),
                txtOptionA != null ? txtOptionA.transform : FindElementTransform("Txt_OptionA"),
                btnOptionB != null ? btnOptionB.transform : FindElementTransform("Btn_OptionB"),
                txtOptionB != null ? txtOptionB.transform : FindElementTransform("Txt_OptionB"),
                btnOptionC != null ? btnOptionC.transform : FindElementTransform("Btn_OptionC"),
                txtOptionC != null ? txtOptionC.transform : FindElementTransform("Txt_OptionC"),
                btnOptionD != null ? btnOptionD.transform : FindElementTransform("Btn_OptionD"),
                txtOptionD != null ? txtOptionD.transform : FindElementTransform("Txt_OptionD"),
                btnClose != null ? btnClose.transform : FindElementTransform("Btn_Close"),
                txtClose != null ? txtClose.transform : FindElementTransform("Txt_Close"),
                btnConfirm != null ? btnConfirm.transform : FindElementTransform("Btn_Confirm")
            };

            foreach (var t in layerOrder)
            {
                if (t != null && t.parent == canvasTransform)
                {
                    t.SetAsLastSibling();
                }
            }
        }

        /// <summary>
        /// Căn chỉnh kích thước và vị trí các TextMeshPro chuẩn xác theo thiết kế Ảnh 2.
        /// </summary>
        private void ApplyCleanLayout()
        {
            // 1. Tiêu đề TRỤ SỐ X (nằm chính giữa Banner vàng)
            if (txtTowerTitle != null)
            {
                txtTowerTitle.rectTransform.anchoredPosition = new Vector2(-4.14f, 292f);
                txtTowerTitle.rectTransform.sizeDelta = new Vector2(400f, 65f);
                txtTowerTitle.fontSize = 44f;
                txtTowerTitle.enableAutoSizing = true;
                txtTowerTitle.fontSizeMin = 28f;
                txtTowerTitle.fontSizeMax = 46f;
                txtTowerTitle.alignment = TextAlignmentOptions.Center;
                txtTowerTitle.color = new Color(1f, 0.91f, 0.51f, 1f); // Vàng hoàng gia sáng
                txtTowerTitle.fontStyle = FontStyles.Bold;
            }

            // 2. Nội dung câu hỏi (nằm gọn trong vùng câu hỏi giữa banner và option đầu tiên)
            if (txtQuestion != null)
            {
                txtQuestion.rectTransform.anchoredPosition = new Vector2(0f, 175f);
                txtQuestion.rectTransform.sizeDelta = new Vector2(880f, 85f);
                txtQuestion.fontSize = 24f;
                txtQuestion.enableAutoSizing = true;
                txtQuestion.fontSizeMin = 18f;
                txtQuestion.fontSizeMax = 26f;
                txtQuestion.textWrappingMode = TextWrappingModes.Normal;
                txtQuestion.alignment = TextAlignmentOptions.Center;
                txtQuestion.color = Color.white;
            }

            // 3. Bốn phương án: đặt lệch sang phải x=75 để nhường chỗ cho oval ring bên trái
            float[] optY = new float[] { 92f, 0f, -89f, -176f };
            TextMeshProUGUI[] tmps = new TextMeshProUGUI[] { txtOptionA, txtOptionB, txtOptionC, txtOptionD };

            for (int i = 0; i < 4; i++)
            {
                if (tmps[i] != null)
                {
                    tmps[i].rectTransform.anchoredPosition = new Vector2(75f, optY[i]);
                    tmps[i].rectTransform.sizeDelta = new Vector2(660f, 68f);
                    tmps[i].fontSize = 24f;
                    tmps[i].enableAutoSizing = true;
                    tmps[i].fontSizeMin = 16f;
                    tmps[i].fontSizeMax = 24f;
                    tmps[i].textWrappingMode = TextWrappingModes.Normal;
                    tmps[i].alignment = TextAlignmentOptions.Left;
                    tmps[i].color = Color.white;
                }
            }
        }

        private Button SetupOptionButton(GameObject go, int index)
        {
            var btn = go.GetComponent<Button>() ?? go.AddComponent<Button>();
            var img = go.GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = true;
                btn.targetGraphic = img;
            }
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelectOption(index));
            return btn;
        }

        private void ConfigOutline(Outline ol)
        {
            if (ol == null) return;
            ol.enabled = false;
            ol.effectColor = new Color(1f, 0.92f, 0.35f, 1f);
            ol.effectDistance = new Vector2(4f, -4f);
        }

        public void Open(int towerNumber, PhysicsQuestionRaw question)
        {
            _currentTowerNumber = towerNumber;
            _currentQuestion = question;
            _selectedOptionIndex = -1;
            _isSubmitting = false;

            AutoBindUI();

            // 1. Tiêu đề TRỤ SỐ X
            if (txtTowerTitle != null)
            {
                txtTowerTitle.text = $"TRỤ SỐ {towerNumber}";
            }

            // 2. Nội dung câu hỏi
            if (txtQuestion != null)
            {
                txtQuestion.text = (question != null && !string.IsNullOrEmpty(question.questionText))
                    ? question.questionText
                    : $"Câu hỏi đối kháng tại Trụ {towerNumber}";
            }

            // 3. Nội dung 4 phương án A, B, C, D
            string ansA = (question != null && question.answers != null && question.answers.Length > 0) ? question.answers[0] : "Phương án A";
            string ansB = (question != null && question.answers != null && question.answers.Length > 1) ? question.answers[1] : "Phương án B";
            string ansC = (question != null && question.answers != null && question.answers.Length > 2) ? question.answers[2] : "Phương án C";
            string ansD = (question != null && question.answers != null && question.answers.Length > 3) ? question.answers[3] : "Phương án D";

            if (txtOptionA != null) txtOptionA.text = EnsureAnswerPrefix(ansA, 'A');
            if (txtOptionB != null) txtOptionB.text = EnsureAnswerPrefix(ansB, 'B');
            if (txtOptionC != null) txtOptionC.text = EnsureAnswerPrefix(ansC, 'C');
            if (txtOptionD != null) txtOptionD.text = EnsureAnswerPrefix(ansD, 'D');

            // 4. Reset toàn bộ visual state và màu sắc về sáng rõ đồng nhất
            ResetVisualState();

            // 5. Ép thứ tự hiển thị phân lớp chuẩn xác (Sibling Index)
            EnforcePopupHierarchyOrder();

            SetPopupVisible(true);
        }

        /// <summary>
        /// Khôi phục toàn bộ trạng thái thị giác: màu sắc, độ sáng, alpha và tương tác.
        /// Ngăn chặn triệt để hiện tượng tối chữ, mất màu, lệch màu hoặc dính selected state của UGUI.
        /// </summary>
        public void ResetVisualState()
        {
            // Xóa selection của EventSystem để không nút nào bị kẹt ở trạng thái Highlighted/Selected/Pressed
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }

            // 1. Tiêu đề TRỤ SỐ X: Màu vàng hoàng gia sáng (#FFE882), alpha = 1
            if (txtTowerTitle != null)
            {
                txtTowerTitle.color = new Color(1f, 0.91f, 0.51f, 1f);
                txtTowerTitle.alpha = 1f;
            }

            // 2. Nội dung câu hỏi: Màu trắng rõ nét, alpha = 1
            if (txtQuestion != null)
            {
                txtQuestion.color = Color.white;
                txtQuestion.alpha = 1f;
            }

            // 3. Khung Popup & Header: Trắng nguyên bản không bị tint màu tối
            var frameImg = GetComponent<Image>();
            if (frameImg != null)
            {
                frameImg.color = Color.white;
            }
            var headerTrans = FindElementTransform("BannerHeader");
            if (headerTrans != null)
            {
                var hImg = headerTrans.GetComponent<Image>();
                if (hImg != null) hImg.color = Color.white;
            }

            // 4. Bốn phương án: Trắng đồng nhất, font trắng rõ, interactable = true, tắt outline
            Image[] optImgs = new Image[] { imgOptionA, imgOptionB, imgOptionC, imgOptionD };
            TextMeshProUGUI[] optTxts = new TextMeshProUGUI[] { txtOptionA, txtOptionB, txtOptionC, txtOptionD };
            Button[] optBtns = new Button[] { btnOptionA, btnOptionB, btnOptionC, btnOptionD };
            Outline[] optOutlines = new Outline[] { outlineOptionA, outlineOptionB, outlineOptionC, outlineOptionD };

            for (int i = 0; i < 4; i++)
            {
                if (optImgs[i] != null) optImgs[i].color = Color.white;
                if (optTxts[i] != null)
                {
                    optTxts[i].color = Color.white;
                    optTxts[i].alpha = 1f;
                }
                if (optBtns[i] != null) optBtns[i].interactable = true;
                if (optOutlines[i] != null) optOutlines[i].enabled = false;
            }

            // 5. Nút Đóng & Nút Xác nhận: Sáng rõ, interactable = true
            if (btnClose != null)
            {
                btnClose.interactable = true;
                var bImg = btnClose.GetComponent<Image>();
                if (bImg != null) bImg.color = Color.white;
            }
            if (txtClose != null)
            {
                txtClose.color = Color.white;
                txtClose.alpha = 1f;
            }
            if (btnConfirm != null)
            {
                btnConfirm.interactable = true;
                var cImg = btnConfirm.GetComponent<Image>();
                if (cImg != null) cImg.color = Color.white;
            }

            if (txtFeedback != null) txtFeedback.text = "";
        }

        private string EnsureAnswerPrefix(string answer, char expectedPrefix)
        {
            if (string.IsNullOrEmpty(answer)) return "";
            string trimmed = answer.Trim();

            if (trimmed.Contains("Xanh:") && trimmed.Contains("Vàng:"))
            {
                return $"{expectedPrefix}. Phương án {expectedPrefix}";
            }

            string[] patterns = new string[]
            {
                $"{expectedPrefix}.", $"{expectedPrefix})", $"{expectedPrefix}:", $"{expectedPrefix} -",
                $"{char.ToLower(expectedPrefix)}.", $"{char.ToLower(expectedPrefix)})"
            };

            foreach (var p in patterns)
            {
                if (trimmed.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed;
                }
            }

            return $"{expectedPrefix}. {trimmed}";
        }

        public void SelectOption(int index)
        {
            if (_isSubmitting) return;
            if (index < 0 || index >= 4) return;

            _selectedOptionIndex = index;
            char[] prefixes = new char[] { 'A', 'B', 'C', 'D' };
            Debug.Log($"[VersusPopup] OptionClicked: TowerId={_currentTowerNumber}, Option={index} ({prefixes[index]})");

            UpdateOptionVisual(0, imgOptionA, outlineOptionA, index == 0);
            UpdateOptionVisual(1, imgOptionB, outlineOptionB, index == 1);
            UpdateOptionVisual(2, imgOptionC, outlineOptionC, index == 2);
            UpdateOptionVisual(3, imgOptionD, outlineOptionD, index == 3);

            if (txtFeedback != null) txtFeedback.text = "";
        }

        private void UpdateOptionVisual(int optIdx, Image img, Outline ol, bool isSelected)
        {
            if (ol != null) ol.enabled = isSelected;
            if (img != null)
            {
                img.color = isSelected ? new Color(0.78f, 0.9f, 1f, 1f) : Color.white;
            }
        }

        private void ResetOptionHighlights()
        {
            UpdateOptionVisual(0, imgOptionA, outlineOptionA, false);
            UpdateOptionVisual(1, imgOptionB, outlineOptionB, false);
            UpdateOptionVisual(2, imgOptionC, outlineOptionC, false);
            UpdateOptionVisual(3, imgOptionD, outlineOptionD, false);
        }

        public void OnConfirmClicked()
        {
            if (_isSubmitting) return;

            Debug.Log($"[VersusPopup] ConfirmClicked: TowerId={_currentTowerNumber}, Selected={_selectedOptionIndex}");

            if (_selectedOptionIndex < 0)
            {
                Debug.LogWarning("[VersusPopup] Em hãy chọn một đáp án.");
                if (txtFeedback != null)
                {
                    txtFeedback.text = "<color=yellow>Em hãy chọn một đáp án.</color>";
                }
                StartCoroutine(FlashOptionsRoutine());
                return;
            }

            _isSubmitting = true;

            bool isCorrect = (_currentQuestion != null && _selectedOptionIndex == _currentQuestion.correctAnswerIndex);

            if (isCorrect)
            {
                Debug.Log($"[VersusPopup] AnswerProcessed: Correct=True, TowerId={_currentTowerNumber}");
                if (txtFeedback != null)
                {
                    txtFeedback.text = "<color=#38b6ff><b>CHÍNH XÁC! EM ĐÃ CHINH PHỤC TRỤ NÀY.</b></color>";
                }

                Image selectedImg = GetOptionImage(_selectedOptionIndex);
                if (selectedImg != null) selectedImg.color = new Color(0.4f, 1f, 0.6f, 1f);

                StartCoroutine(CloseAfterSuccessRoutine());
            }
            else
            {
                Debug.Log($"[VersusPopup] AnswerProcessed: Correct=False, TowerId={_currentTowerNumber}");
                if (txtFeedback != null)
                {
                    txtFeedback.text = "<color=#ff5555><b>Sai rồi! Hãy thử lại.</b></color>";
                }

                Image selectedImg = GetOptionImage(_selectedOptionIndex);
                if (selectedImg != null) selectedImg.color = new Color(1f, 0.5f, 0.5f, 1f);

                StartCoroutine(RetryAfterFailureRoutine());
            }
        }

        private IEnumerator FlashOptionsRoutine()
        {
            var outlines = new Outline[] { outlineOptionA, outlineOptionB, outlineOptionC, outlineOptionD };
            for (int step = 0; step < 2; step++)
            {
                foreach (var ol in outlines)
                {
                    if (ol != null)
                    {
                        ol.enabled = true;
                        ol.effectColor = new Color(1f, 0.45f, 0.2f, 1f);
                    }
                }
                yield return new WaitForSecondsRealtime(0.12f);
                foreach (var ol in outlines)
                {
                    if (ol != null)
                    {
                        ol.enabled = false;
                        ol.effectColor = new Color(1f, 0.92f, 0.35f, 1f);
                    }
                }
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        private Image GetOptionImage(int index)
        {
            switch (index)
            {
                case 0: return imgOptionA;
                case 1: return imgOptionB;
                case 2: return imgOptionC;
                case 3: return imgOptionD;
                default: return null;
            }
        }

        private IEnumerator CloseAfterSuccessRoutine()
        {
            yield return new WaitForSecondsRealtime(0.4f);
            if (VersusArenaController.Instance != null)
            {
                VersusArenaController.Instance.SolveTower(_currentTowerNumber, isPlayer: true);
            }
            Close();
        }

        private IEnumerator RetryAfterFailureRoutine()
        {
            yield return new WaitForSecondsRealtime(0.6f);
            ResetVisualState();
            _selectedOptionIndex = -1;
            _isSubmitting = false;
        }

        public void OnOpponentClaimedCurrentTower()
        {
            // Trong luật 10/10 độc lập: Đối thủ giải đúng trụ này không ảnh hưởng đến người chơi
            Debug.Log($"[VersusPopup] Đối thủ đã giải đúng Trụ {_currentTowerNumber}. Người chơi vẫn tiếp tục giải trụ này bình thường.");
        }

        public void Close()
        {
            Debug.Log($"[VersusPopup] CloseClicked: TowerId={_currentTowerNumber}");
            _isSubmitting = false;
            _selectedOptionIndex = -1;
            ResetVisualState();
            SetPopupVisible(false);

            if (VersusArenaController.Instance != null)
            {
                VersusArenaController.Instance.OnModalClosed();
            }

            Debug.Log("[VersusPopup] PopupClosed");
        }

        public void SetPopupVisible(bool visible)
        {
            _isPopupActive = visible;

            if (popupElements == null || popupElements.Length == 0)
            {
                Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    var list = new List<GameObject>();
                    string[] targetNames = new string[]
                    {
                        "PopupFrame", "BannerHeader", "Txt_TowerTitle", "Txt_Question",
                        "Btn_OptionA", "Txt_OptionA", "Btn_OptionB", "Txt_OptionB",
                        "Btn_OptionC", "Txt_OptionC", "Btn_OptionD", "Txt_OptionD",
                        "Btn_Confirm", "Btn_Close", "Txt_Close"
                    };
                    foreach (var tName in targetNames)
                    {
                        var t = canvas.transform.Find(tName);
                        if (t != null) list.Add(t.gameObject);
                    }
                    if (list.Count > 0)
                    {
                        popupElements = list.ToArray();
                    }
                }
            }

            if (popupElements != null && popupElements.Length > 0)
            {
                foreach (var elem in popupElements)
                {
                    if (elem != null)
                    {
                        elem.SetActive(visible);
                    }
                }
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }

        public void SetReferences(
            TextMeshProUGUI title, TextMeshProUGUI question,
            TextMeshProUGUI optA, TextMeshProUGUI optB, TextMeshProUGUI optC, TextMeshProUGUI optD,
            Button btnA, Button btnB, Button btnC, Button btnD,
            Button confirm, Button close,
            Image imgA = null, Image imgB = null, Image imgC = null, Image imgD = null)
        {
            txtTowerTitle = title;
            txtQuestion = question;
            txtOptionA = optA;
            txtOptionB = optB;
            txtOptionC = optC;
            txtOptionD = optD;

            btnOptionA = btnA;
            btnOptionB = btnB;
            btnOptionC = btnC;
            btnOptionD = btnD;
            btnConfirm = confirm;
            btnClose = close;

            imgOptionA = imgA;
            imgOptionB = imgB;
            imgOptionC = imgC;
            imgOptionD = imgD;

            if (txtTowerTitle != null) txtTowerTitle.raycastTarget = false;
            if (txtQuestion != null) txtQuestion.raycastTarget = false;
            if (txtOptionA != null) txtOptionA.raycastTarget = false;
            if (txtOptionB != null) txtOptionB.raycastTarget = false;
            if (txtOptionC != null) txtOptionC.raycastTarget = false;
            if (txtOptionD != null) txtOptionD.raycastTarget = false;

            if (btnOptionA != null) { btnOptionA.onClick.RemoveAllListeners(); btnOptionA.onClick.AddListener(() => SelectOption(0)); }
            if (btnOptionB != null) { btnOptionB.onClick.RemoveAllListeners(); btnOptionB.onClick.AddListener(() => SelectOption(1)); }
            if (btnOptionC != null) { btnOptionC.onClick.RemoveAllListeners(); btnOptionC.onClick.AddListener(() => SelectOption(2)); }
            if (btnOptionD != null) { btnOptionD.onClick.RemoveAllListeners(); btnOptionD.onClick.AddListener(() => SelectOption(3)); }

            if (btnConfirm != null) { btnConfirm.onClick.RemoveAllListeners(); btnConfirm.onClick.AddListener(OnConfirmClicked); }
            if (btnClose != null) { btnClose.onClick.RemoveAllListeners(); btnClose.onClick.AddListener(Close); }
        }

        public void SetPopupElements(GameObject[] elements)
        {
            popupElements = elements;
        }
    }
}
