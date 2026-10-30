using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển logic giao diện Scene 20.1 Detail Result TL (Xem chi tiết bài Tự luận)
    /// Đáp ứng tiêu chí nghiệm thu:
    /// - Hiển thị đầy đủ cả 5 câu (Câu 1 đến Câu 5) thông qua thanh chuyển câu.
    /// - Câu đã làm: hiển thị điểm Gemini, từng bước đúng/sai/thiếu và lời giải mẫu.
    /// - Câu chưa làm: trạng thái CHƯA LÀM, điểm 0 và hiển thị lời giải mẫu khi người chơi chủ động xem.
    /// - Hai tab chính: "BÀI CỦA BẠN" và "LỜI GIẢI THAM KHẢO".
    /// - Nút "← VỀ KẾT QUẢ" chuyển về Scene 19.1. Result_TL (RouteId.EssayResult).
    /// </summary>
    public class EssayReviewController : MonoBehaviour
    {
        [Header("=== QUESTION SELECTOR (1-5) ===")]
        [SerializeField] private List<Button> questionButtons = new List<Button>();
        [SerializeField] private List<TextMeshProUGUI> questionButtonLabels = new List<TextMeshProUGUI>();

        [Header("=== TABS ===")]
        [SerializeField] private Button btnTabStudentWork;
        [SerializeField] private Button btnTabModelSolution;
        [SerializeField] private GameObject panelStudentWork;
        [SerializeField] private GameObject panelModelSolution;

        [Header("=== CONTENT CONTAINERS ===")]
        [SerializeField] private RectTransform studentStepsContainer;
        [SerializeField] private TextMeshProUGUI modelSolutionText;
        [SerializeField] private TextMeshProUGUI questionTitleText;

        [Header("=== NAVIGATION ===")]
        [SerializeField] private Button btnBackToResult;

        private List<EssayAnswerRecord> _essayAnswers = new List<EssayAnswerRecord>();
        private int _currentQuestionIndex = 0;
        private bool _isViewingStudentWork = true;

        private void Awake()
        {
            EnsureEventSystem();
            BuildUIStructureIfMissing();
            SetupButtonListeners();
        }

        private void Start()
        {
            NavigationService.Instance.EnsureDevContext();
            LoadGradingData();
            RenderContent();
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

        private void BuildUIStructureIfMissing()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Tắt raycastTarget trên các object trang trí
            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
            {
                if (img.GetComponent<Button>() != null) continue;
                string n = img.gameObject.name.ToLower();
                if (n.Contains("bg") || n.Contains("panel") || n.Contains("frame") || n == "image")
                {
                    img.raycastTarget = false;
                }
            }

            // 1. Tạo Tiêu đề câu hỏi (QuestionTitleText)
            Transform existingTitle = canvas.transform.Find("ReviewQuestionTitle");
            if (existingTitle == null)
            {
                GameObject titleGo = new GameObject("ReviewQuestionTitle");
                titleGo.transform.SetParent(canvas.transform, false);
                RectTransform ttRt = titleGo.AddComponent<RectTransform>();
                ttRt.anchorMin = new Vector2(0.5f, 0.5f);
                ttRt.anchorMax = new Vector2(0.5f, 0.5f);
                ttRt.pivot = new Vector2(0.5f, 0.5f);
                ttRt.anchoredPosition = new Vector2(100f, 255f);
                ttRt.sizeDelta = new Vector2(780f, 40f);

                questionTitleText = titleGo.AddComponent<TextMeshProUGUI>();
                questionTitleText.fontSize = 20;
                questionTitleText.fontStyle = FontStyles.Bold;
                questionTitleText.color = new Color(0.96f, 0.82f, 0.38f, 1f);
                questionTitleText.alignment = TextAlignmentOptions.Center;
                questionTitleText.raycastTarget = false;
            }
            else
            {
                questionTitleText = existingTitle.GetComponent<TextMeshProUGUI>();
            }

            // 2. Tạo thanh chuyển câu hỏi (Câu 1..5)
            Transform existingQBar = canvas.transform.Find("QuestionSelectorBar");
            if (existingQBar == null)
            {
                GameObject qbGo = new GameObject("QuestionSelectorBar");
                qbGo.transform.SetParent(canvas.transform, false);
                RectTransform qbRt = qbGo.AddComponent<RectTransform>();
                qbRt.anchorMin = new Vector2(0.5f, 0.5f);
                qbRt.anchorMax = new Vector2(0.5f, 0.5f);
                qbRt.pivot = new Vector2(0.5f, 0.5f);
                qbRt.anchoredPosition = new Vector2(100f, 210f);
                qbRt.sizeDelta = new Vector2(780f, 40f);

                questionButtons.Clear();
                questionButtonLabels.Clear();

                float btnWidth = 140f;
                float spacing = 12f;
                float startX = -((5 * btnWidth + 4 * spacing) / 2f) + btnWidth / 2f;

                for (int i = 0; i < 5; i++)
                {
                    int qIdx = i;
                    GameObject bGo = new GameObject($"Btn_Q{qIdx + 1}");
                    bGo.transform.SetParent(qbGo.transform, false);
                    RectTransform bRt = bGo.AddComponent<RectTransform>();
                    bRt.anchoredPosition = new Vector2(startX + qIdx * (btnWidth + spacing), 0f);
                    bRt.sizeDelta = new Vector2(btnWidth, 38f);

                    Image bImg = bGo.AddComponent<Image>();
                    bImg.color = (qIdx == 0) ? new Color(0.15f, 0.45f, 0.80f, 1f) : new Color(0.08f, 0.16f, 0.28f, 0.90f);

                    Outline bOl = bGo.AddComponent<Outline>();
                    bOl.effectColor = new Color(0.96f, 0.82f, 0.38f, 0.8f);
                    bOl.effectDistance = new Vector2(1f, -1f);

                    Button btn = bGo.AddComponent<Button>();
                    btn.targetGraphic = bImg;
                    btn.onClick.AddListener(() => OnQuestionTabClicked(qIdx));
                    questionButtons.Add(btn);

                    GameObject txtGo = new GameObject("Text");
                    txtGo.transform.SetParent(bGo.transform, false);
                    var tmp = txtGo.AddComponent<TextMeshProUGUI>();
                    tmp.text = $"CÂU {qIdx + 1}";
                    tmp.fontSize = 16;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = Color.white;
                    tmp.raycastTarget = false;
                    questionButtonLabels.Add(tmp);

                    UIHoverPulseEffect.Attach(bGo, hover: 1.05f, minPulse: 1.02f, maxPulse: 1.06f, outline: true);
                }
            }

            // 3. Tạo khung Tab (BÀI CỦA BẠN vs LỜI GIẢI THAM KHẢO)
            Transform existingTabs = canvas.transform.Find("ReviewTabsContainer");
            if (existingTabs == null)
            {
                GameObject tabsGo = new GameObject("ReviewTabsContainer");
                tabsGo.transform.SetParent(canvas.transform, false);

                RectTransform tabsRt = tabsGo.AddComponent<RectTransform>();
                tabsRt.anchorMin = new Vector2(0.5f, 0.5f);
                tabsRt.anchorMax = new Vector2(0.5f, 0.5f);
                tabsRt.pivot = new Vector2(0.5f, 0.5f);
                tabsRt.anchoredPosition = new Vector2(100f, 160f);
                tabsRt.sizeDelta = new Vector2(780f, 42f);

                // Tab 1: Bài của bạn
                GameObject t1Go = new GameObject("Tab_StudentWork");
                t1Go.transform.SetParent(tabsGo.transform, false);
                RectTransform t1Rt = t1Go.AddComponent<RectTransform>();
                t1Rt.anchoredPosition = new Vector2(-180f, 0f);
                t1Rt.sizeDelta = new Vector2(240f, 38f);
                Image t1Img = t1Go.AddComponent<Image>();
                t1Img.color = new Color(0.12f, 0.35f, 0.65f, 1f);
                btnTabStudentWork = t1Go.AddComponent<Button>();
                btnTabStudentWork.targetGraphic = t1Img;
                GameObject t1TxtGo = new GameObject("Text");
                t1TxtGo.transform.SetParent(t1Go.transform, false);
                var t1Tmp = t1TxtGo.AddComponent<TextMeshProUGUI>();
                t1Tmp.text = "BÀI CỦA BẠN";
                t1Tmp.fontSize = 16;
                t1Tmp.fontStyle = FontStyles.Bold;
                t1Tmp.alignment = TextAlignmentOptions.Center;
                t1Tmp.color = Color.white;
                t1Tmp.raycastTarget = false;

                // Tab 2: Lời giải tham khảo
                GameObject t2Go = new GameObject("Tab_ModelSolution");
                t2Go.transform.SetParent(tabsGo.transform, false);
                RectTransform t2Rt = t2Go.AddComponent<RectTransform>();
                t2Rt.anchoredPosition = new Vector2(180f, 0f);
                t2Rt.sizeDelta = new Vector2(240f, 38f);
                Image t2Img = t2Go.AddComponent<Image>();
                t2Img.color = new Color(0.08f, 0.15f, 0.28f, 0.85f);
                btnTabModelSolution = t2Go.AddComponent<Button>();
                btnTabModelSolution.targetGraphic = t2Img;
                GameObject t2TxtGo = new GameObject("Text");
                t2TxtGo.transform.SetParent(t2Go.transform, false);
                var t2Tmp = t2TxtGo.AddComponent<TextMeshProUGUI>();
                t2Tmp.text = "LỜI GIẢI THAM KHẢO";
                t2Tmp.fontSize = 16;
                t2Tmp.fontStyle = FontStyles.Bold;
                t2Tmp.alignment = TextAlignmentOptions.Center;
                t2Tmp.color = new Color(0.85f, 0.85f, 0.85f, 1f);
                t2Tmp.raycastTarget = false;
            }

            // 4. Tạo Panel Bài của bạn
            Transform existingWork = canvas.transform.Find("Panel_StudentWork");
            if (existingWork == null)
            {
                panelStudentWork = new GameObject("Panel_StudentWork");
                panelStudentWork.transform.SetParent(canvas.transform, false);
                RectTransform pwRt = panelStudentWork.AddComponent<RectTransform>();
                pwRt.anchorMin = new Vector2(0.5f, 0.5f);
                pwRt.anchorMax = new Vector2(0.5f, 0.5f);
                pwRt.pivot = new Vector2(0.5f, 0.5f);
                pwRt.anchoredPosition = new Vector2(100f, -45f);
                pwRt.sizeDelta = new Vector2(780f, 350f);

                var scrollGo = new GameObject("Scroll");
                scrollGo.transform.SetParent(panelStudentWork.transform, false);
                RectTransform sRt = scrollGo.AddComponent<RectTransform>();
                sRt.anchorMin = Vector2.zero;
                sRt.anchorMax = Vector2.one;
                sRt.sizeDelta = Vector2.zero;
                ScrollRect sr = scrollGo.AddComponent<ScrollRect>();
                sr.horizontal = false;
                sr.vertical = true;
                sr.scrollSensitivity = 25f;
                scrollGo.AddComponent<RectMask2D>();

                var contentGo = new GameObject("Content");
                contentGo.transform.SetParent(scrollGo.transform, false);
                studentStepsContainer = contentGo.AddComponent<RectTransform>();
                studentStepsContainer.anchorMin = new Vector2(0, 1);
                studentStepsContainer.anchorMax = new Vector2(1, 1);
                studentStepsContainer.pivot = new Vector2(0.5f, 1);
                studentStepsContainer.sizeDelta = new Vector2(0, 350);

                var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
                vlg.spacing = 10f;
                vlg.padding = new RectOffset(8, 8, 8, 8);
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;

                var csf = contentGo.AddComponent<ContentSizeFitter>();
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                sr.content = studentStepsContainer;
            }
            else
            {
                panelStudentWork = existingWork.gameObject;
                studentStepsContainer = panelStudentWork.GetComponentInChildren<RectTransform>();
            }

            // 5. Tạo Panel Lời giải tham khảo
            Transform existingSol = canvas.transform.Find("Panel_ModelSolution");
            if (existingSol == null)
            {
                panelModelSolution = new GameObject("Panel_ModelSolution");
                panelModelSolution.transform.SetParent(canvas.transform, false);
                RectTransform psRt = panelModelSolution.AddComponent<RectTransform>();
                psRt.anchorMin = new Vector2(0.5f, 0.5f);
                psRt.anchorMax = new Vector2(0.5f, 0.5f);
                psRt.pivot = new Vector2(0.5f, 0.5f);
                psRt.anchoredPosition = new Vector2(100f, -45f);
                psRt.sizeDelta = new Vector2(780f, 350f);

                Image solBg = panelModelSolution.AddComponent<Image>();
                solBg.color = new Color(0.06f, 0.12f, 0.24f, 0.95f);

                Outline solOl = panelModelSolution.AddComponent<Outline>();
                solOl.effectColor = new Color(0.96f, 0.82f, 0.38f, 0.8f);
                solOl.effectDistance = new Vector2(1.5f, -1.5f);

                var scrollGo = new GameObject("Scroll");
                scrollGo.transform.SetParent(panelModelSolution.transform, false);
                RectTransform sRt = scrollGo.AddComponent<RectTransform>();
                sRt.anchorMin = Vector2.zero;
                sRt.anchorMax = Vector2.one;
                sRt.offsetMin = new Vector2(16, 16);
                sRt.offsetMax = new Vector2(-16, -16);
                ScrollRect sr = scrollGo.AddComponent<ScrollRect>();
                sr.horizontal = false;
                sr.vertical = true;
                scrollGo.AddComponent<RectMask2D>();

                GameObject solTxtGo = new GameObject("ModelSolutionText");
                solTxtGo.transform.SetParent(scrollGo.transform, false);
                RectTransform stRt = solTxtGo.AddComponent<RectTransform>();
                stRt.anchorMin = new Vector2(0, 1);
                stRt.anchorMax = new Vector2(1, 1);
                stRt.pivot = new Vector2(0.5f, 1);
                stRt.sizeDelta = new Vector2(0, 300);

                modelSolutionText = solTxtGo.AddComponent<TextMeshProUGUI>();
                modelSolutionText.fontSize = 17;
                modelSolutionText.color = Color.white;
                modelSolutionText.alignment = TextAlignmentOptions.TopLeft;
                modelSolutionText.textWrappingMode = TextWrappingModes.Normal;

                var csf = solTxtGo.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                sr.content = stRt;

                panelModelSolution.SetActive(false);
            }
            else
            {
                panelModelSolution = existingSol.gameObject;
                modelSolutionText = panelModelSolution.GetComponentInChildren<TextMeshProUGUI>();
            }

            // 6. Nút Quay lại màn kết quả ở góc dưới
            Transform existingBack = canvas.transform.Find("Btn_BackToResult");
            if (existingBack == null)
            {
                GameObject bGo = new GameObject("Btn_BackToResult");
                bGo.transform.SetParent(canvas.transform, false);
                RectTransform bRt = bGo.AddComponent<RectTransform>();
                bRt.anchorMin = new Vector2(0.5f, 0.5f);
                bRt.anchorMax = new Vector2(0.5f, 0.5f);
                bRt.pivot = new Vector2(0.5f, 0.5f);
                bRt.anchoredPosition = new Vector2(-380f, -280f);
                bRt.sizeDelta = new Vector2(220f, 48f);

                Image bImg = bGo.AddComponent<Image>();
                bImg.color = new Color(0.12f, 0.35f, 0.65f, 1f);
                btnBackToResult = bGo.AddComponent<Button>();
                btnBackToResult.targetGraphic = bImg;

                GameObject bTxtGo = new GameObject("Text");
                bTxtGo.transform.SetParent(bGo.transform, false);
                var bTmp = bTxtGo.AddComponent<TextMeshProUGUI>();
                bTmp.text = "← VỀ KẾT QUẢ";
                bTmp.fontSize = 17;
                bTmp.fontStyle = FontStyles.Bold;
                bTmp.alignment = TextAlignmentOptions.Center;
                bTmp.color = Color.white;
                bTmp.raycastTarget = false;
                UIHoverPulseEffect.Attach(bGo, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.07f, outline: true);
            }
            else
            {
                btnBackToResult = existingBack.GetComponent<Button>();
            }
        }

        private void SetupButtonListeners()
        {
            if (btnTabStudentWork != null)
            {
                btnTabStudentWork.onClick.RemoveAllListeners();
                btnTabStudentWork.onClick.AddListener(() => SwitchTab(true));
            }

            if (btnTabModelSolution != null)
            {
                btnTabModelSolution.onClick.RemoveAllListeners();
                btnTabModelSolution.onClick.AddListener(() => SwitchTab(false));
            }

            if (btnBackToResult != null)
            {
                btnBackToResult.onClick.RemoveAllListeners();
                btnBackToResult.onClick.AddListener(OnBackToResultClicked);
            }
        }

        private void LoadGradingData()
        {
            _essayAnswers = NavigationService.Instance.AttemptContext.EssayAnswers;

            if (_essayAnswers == null || _essayAnswers.Count == 0)
            {
                _essayAnswers = new List<EssayAnswerRecord>();
                for (int i = 0; i < 5; i++)
                {
                    _essayAnswers.Add(new EssayAnswerRecord
                    {
                        CauHoiID = $"TL_C1_L2_{i + 1}",
                        QuestionText = $"Câu hỏi tự luận {i + 1}",
                        SampleAnswer = "Lời giải tham khảo chi tiết của câu hỏi.",
                        Explanation = "Chưa có dữ liệu bài làm.",
                        ConfirmedText = "(Chưa làm)",
                        Score = 0f,
                        IsGraded = true
                    });
                }
            }
        }

        private void OnQuestionTabClicked(int index)
        {
            _currentQuestionIndex = Mathf.Clamp(index, 0, _essayAnswers.Count - 1);
            RenderContent();
        }

        private void RenderContent()
        {
            if (_essayAnswers == null || _essayAnswers.Count == 0) return;
            if (_currentQuestionIndex >= _essayAnswers.Count) _currentQuestionIndex = 0;

            var rec = _essayAnswers[_currentQuestionIndex];

            // Cập nhật trạng thái màu các nút chọn câu
            for (int i = 0; i < questionButtons.Count; i++)
            {
                if (i >= _essayAnswers.Count) continue;
                bool isSelected = (i == _currentQuestionIndex);
                bool hasAnswer = (_essayAnswers[i].ConfirmedText != "(Chưa làm)");

                Image bImg = questionButtons[i].GetComponent<Image>();
                if (bImg != null)
                {
                    if (isSelected)
                    {
                        bImg.color = new Color(0.20f, 0.55f, 0.95f, 1f); // Xanh sáng đang chọn
                    }
                    else if (hasAnswer)
                    {
                        bImg.color = new Color(0.10f, 0.28f, 0.18f, 0.95f); // Xanh lá đã làm
                    }
                    else
                    {
                        bImg.color = new Color(0.12f, 0.18f, 0.30f, 0.90f); // Xanh tối chưa làm
                    }
                }

                if (i < questionButtonLabels.Count && questionButtonLabels[i] != null)
                {
                    questionButtonLabels[i].text = $"CÂU {i + 1}\n<size=12>({_essayAnswers[i].Score:F1}đ)</size>";
                }
            }

            // Tiêu đề câu hỏi
            if (questionTitleText != null)
            {
                string stateLabel = (rec.ConfirmedText == "(Chưa làm)") ? "<color=#f87171>[CHƯA LÀM — 0.0 đ]</color>" : $"<color=#4ade80>[ĐIỂM: {rec.Score:F1}/10 đ]</color>";
                questionTitleText.text = $"<b>CÂU {_currentQuestionIndex + 1}/5:</b> {rec.QuestionText}  {stateLabel}";
            }

            // Render Lời giải tham khảo
            if (modelSolutionText != null)
            {
                modelSolutionText.text = $"<b>LỜI GIẢI THAM KHẢO — CÂU {_currentQuestionIndex + 1}:</b>\n\n{rec.SampleAnswer}\n\n" +
                                         $"<b>ĐIỂM ĐẠT ĐƯỢC:</b> {rec.Score:F1}/10 đ\n" +
                                         $"<b>TRẠNG THÁI:</b> {(rec.IsDraftRubric ? "Chấm thử — rubric DRAFT" : "Rubric chính thức")}\n\n" +
                                         $"<b>NHẬN XÉT:</b>\n{rec.Explanation}";
            }

            // Render Bài của bạn
            if (studentStepsContainer != null)
            {
                foreach (Transform child in studentStepsContainer)
                {
                    Destroy(child.gameObject);
                }

                if (rec.ConfirmedText == "(Chưa làm)")
                {
                    CreateUnattemptedCard(studentStepsContainer, _currentQuestionIndex + 1);
                }
                else if (rec.Steps != null && rec.Steps.Count > 0)
                {
                    for (int s = 0; s < rec.Steps.Count; s++)
                    {
                        CreateStepItem(studentStepsContainer, rec.Steps[s], s + 1);
                    }
                }
                else
                {
                    CreateSimpleAnswerCard(studentStepsContainer, rec);
                }

                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(studentStepsContainer);
            }
        }

        private void CreateUnattemptedCard(Transform parent, int qNum)
        {
            GameObject item = new GameObject($"Unattempted_Q{qNum}");
            item.transform.SetParent(parent, false);

            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(760f, 100f);

            Image img = item.AddComponent<Image>();
            img.color = new Color(0.18f, 0.22f, 0.32f, 0.95f);

            Outline outline = item.AddComponent<Outline>();
            outline.effectColor = new Color(0.90f, 0.40f, 0.40f, 0.8f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            GameObject txtGo = new GameObject("Text");
            txtGo.transform.SetParent(item.transform, false);
            RectTransform tRt = txtGo.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = new Vector2(16, 12);
            tRt.offsetMax = new Vector2(-16, -12);

            TextMeshProUGUI tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            tmp.text = $"<b><color=#f87171>[⏸ CHƯA LÀM] CÂU {qNum} (0.0/10 điểm)</color></b>\n" +
                       $"<i>Học sinh chưa hoàn thành câu hỏi này trong bài làm.</i>\n" +
                       $"<color=#fef08a>💡 Gợi ý: Hãy bấm tab \"LỜI GIẢI THAM KHẢO\" ở trên để xem chi tiết các bước giải mẫu nhé!</color>";
        }

        private void CreateSimpleAnswerCard(Transform parent, EssayAnswerRecord rec)
        {
            GameObject item = new GameObject("AnswerCard");
            item.transform.SetParent(parent, false);

            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(760f, 120f);

            Image img = item.AddComponent<Image>();
            img.color = new Color(0.10f, 0.18f, 0.30f, 0.95f);

            Outline outline = item.AddComponent<Outline>();
            outline.effectColor = new Color(0.40f, 0.70f, 1.0f, 0.8f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            GameObject txtGo = new GameObject("Text");
            txtGo.transform.SetParent(item.transform, false);
            RectTransform tRt = txtGo.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = new Vector2(16, 12);
            tRt.offsetMax = new Vector2(-16, -12);

            TextMeshProUGUI tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            tmp.text = $"<b><color=#4ade80>Bài làm đã nộp ({rec.Score:F1}/10 đ):</color></b>\n" +
                       $"<i>\"{rec.ConfirmedText}\"</i>\n\n" +
                       $"<b>Nhận xét:</b> {rec.Explanation}";
        }

        private void CreateStepItem(Transform parent, GradingStepFeedback step, int stepNum)
        {
            GameObject item = new GameObject($"Step_{step.StepId}");
            item.transform.SetParent(parent, false);

            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(760f, 90f);

            Image img = item.AddComponent<Image>();
            Outline outline = item.AddComponent<Outline>();
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            string badge = "";
            Color badgeColor;
            Color bgColor;
            Color outlineColor;

            switch (step.Status)
            {
                case "correct":
                    badge = "✓ ĐÚNG";
                    bgColor = new Color(0.08f, 0.28f, 0.16f, 0.95f);
                    badgeColor = new Color(0.40f, 0.95f, 0.55f, 1f);
                    outlineColor = new Color(0.30f, 0.80f, 0.40f, 0.8f);
                    break;
                case "partial":
                    badge = "! CHƯA ĐỦ";
                    bgColor = new Color(0.38f, 0.28f, 0.08f, 0.95f);
                    badgeColor = new Color(1f, 0.85f, 0.35f, 1f);
                    outlineColor = new Color(0.90f, 0.75f, 0.20f, 0.8f);
                    break;
                case "incorrect":
                    badge = "✕ CẦN SỬA";
                    bgColor = new Color(0.38f, 0.10f, 0.10f, 0.95f);
                    badgeColor = new Color(1f, 0.45f, 0.45f, 1f);
                    outlineColor = new Color(0.90f, 0.25f, 0.25f, 0.8f);
                    break;
                default:
                    badge = "⏸ CHƯA LÀM";
                    bgColor = new Color(0.14f, 0.18f, 0.28f, 0.95f);
                    badgeColor = new Color(0.70f, 0.82f, 0.98f, 1f);
                    outlineColor = new Color(0.35f, 0.50f, 0.70f, 0.8f);
                    break;
            }

            img.color = bgColor;
            outline.effectColor = outlineColor;

            GameObject txtGo = new GameObject("Text");
            txtGo.transform.SetParent(item.transform, false);
            RectTransform tRt = txtGo.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = new Vector2(16, 10);
            tRt.offsetMax = new Vector2(-16, -10);

            TextMeshProUGUI tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 15;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;

            string excerptText = !string.IsNullOrEmpty(step.StudentText) ? $"<b>Trích bài:</b> <i>\"{step.StudentText}\"</i>\n" : "";
            string corrText = !string.IsNullOrEmpty(step.SuggestedCorrection) ? $"\n<color=#fef08a><b>💡 Hướng dẫn sửa:</b> {step.SuggestedCorrection}</color>" : "";

            tmp.text = $"<b><color=#{ColorUtility.ToHtmlStringRGB(badgeColor)}>[{badge}] BƯỚC {stepNum} ({step.score:F1}/{step.maxPoints:F1} đ):</color></b>\n" +
                       excerptText +
                       $"<b>Nhận xét:</b> {step.Explanation}" +
                       corrText;

            var csf = item.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void SwitchTab(bool viewStudentWork)
        {
            _isViewingStudentWork = viewStudentWork;
            if (panelStudentWork != null) panelStudentWork.SetActive(viewStudentWork);
            if (panelModelSolution != null) panelModelSolution.SetActive(!viewStudentWork);

            if (btnTabStudentWork != null)
            {
                btnTabStudentWork.GetComponent<Image>().color = viewStudentWork
                    ? new Color(0.12f, 0.35f, 0.65f, 1f)
                    : new Color(0.08f, 0.15f, 0.28f, 0.85f);
            }

            if (btnTabModelSolution != null)
            {
                btnTabModelSolution.GetComponent<Image>().color = !viewStudentWork
                    ? new Color(0.12f, 0.35f, 0.65f, 1f)
                    : new Color(0.08f, 0.15f, 0.28f, 0.85f);
            }
        }

        private void OnBackToResultClicked()
        {
            Debug.Log("[EssayReviewController] Quay lại màn kết quả tự luận: RouteId.EssayResult (Scene 19.1. Result_TL).");
            NavigationService.Instance.NavigateTo(RouteId.EssayResult);
        }
    }
}
