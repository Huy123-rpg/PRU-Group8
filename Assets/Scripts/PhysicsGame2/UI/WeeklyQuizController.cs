using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển logic giao diện Scene 11.1_Quiz 1 (Thử thách tuần)
    /// theo đặc tả Mục 12.2:
    /// - 10 câu hỏi trắc nghiệm cố định trong tuần (blueprint 3 NB + 3 TH + 3 VD + 1 VDC).
    /// - Đồng hồ đếm ngược 15 phút (900 giây), không pause khi mất focus/Esc.
    /// - Chọn đáp án -> lưu lựa chọn, cho phép đổi trước khi nộp; TUYỆT ĐỐI KHÔNG hiện đúng/sai.
    /// - Đi lại tự do giữa các câu 1..10 (Câu trước / Câu tiếp).
    /// - Hiển thị tiến độ: "Đã trả lời x/10".
    /// - Nộp bài thủ công chỉ khi đủ 10/10. Hết 15 phút server/game tự động chốt các đáp án đã nhận.
    /// - Sau khi nộp -> chấm điểm -> chuyển sang Scene 32. Result_Tự luận (RouteId.WeeklyResult).
    /// </summary>
    public class WeeklyQuizController : MonoBehaviour
    {
        [Header("=== TEXT REFERENCES ===")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private TextMeshProUGUI counterText;

        [Header("=== ANSWER BUTTONS ===")]
        [SerializeField] private Button[] answerButtons = new Button[4];
        [SerializeField] private TextMeshProUGUI[] answerTexts = new TextMeshProUGUI[4];
        [SerializeField] private Image[] answerImages = new Image[4];

        [Header("=== NAVIGATION ===")]
        [SerializeField] private Button btnPrev;
        [SerializeField] private Button btnNext;
        [SerializeField] private Button btnSubmit;
        [SerializeField] private Button btnBack;
        [SerializeField] private TextMeshProUGUI noticeText;

        private List<PhysicsQuestionRaw> _weeklyQuestions = new List<PhysicsQuestionRaw>();
        private readonly int[] _selectedAnswers = new int[10]; // -1 nếu chưa chọn
        private int _currentIndex = 0;
        private float _timeRemaining = 900f; // 15 phút
        private bool _isSubmitted = false;
        private Coroutine _noticeCoroutine;

        private void Awake()
        {
            EnsureEventSystem();
            AutoBindUIReferences();
            SetupButtonListeners();
        }

        private void Start()
        {
            InitializeWeeklyChallenge();
        }

        private void Update()
        {
            if (_isSubmitted) return;

            // Đồng hồ đếm ngược 15 phút
            _timeRemaining -= Time.unscaledDeltaTime;
            if (_timeRemaining <= 0f)
            {
                _timeRemaining = 0f;
                UpdateTimerDisplay();
                AutoSubmitOnTimeOut();
                return;
            }

            UpdateTimerDisplay();
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

            // Tìm Answer Buttons
            Button[] allButtons = canvas.GetComponentsInChildren<Button>(true);
            int ansIdx = 0;
            foreach (var b in allButtons)
            {
                var rt = b.GetComponent<RectTransform>();
                // 4 nút đáp án thường có width > 300, nằm ở nửa dưới màn hình
                if (rt.sizeDelta.x > 350f && rt.anchoredPosition.y < 0f && ansIdx < 4)
                {
                    answerButtons[ansIdx] = b;
                    answerImages[ansIdx] = b.GetComponent<Image>();
                    answerTexts[ansIdx] = b.GetComponentInChildren<TextMeshProUGUI>(true);
                    ansIdx++;
                }
            }

            // Tìm Question text & Counter text
            TextMeshProUGUI[] tmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                string text = t.text != null ? t.text.Trim() : "";
                if (text.Contains("?") || text.Length > 25)
                {
                    if (questionText == null) questionText = t;
                }
                else if (text.StartsWith("Câu") || text.Contains("/10"))
                {
                    if (counterText == null) counterText = t;
                }
            }

            // Tạo Timer Text nếu chưa có
            if (timerText == null)
            {
                GameObject timerGo = new GameObject("WeeklyTimerText");
                timerGo.transform.SetParent(canvas.transform, false);
                RectTransform tRt = timerGo.AddComponent<RectTransform>();
                tRt.anchorMin = new Vector2(0.5f, 1f);
                tRt.anchorMax = new Vector2(0.5f, 1f);
                tRt.pivot = new Vector2(0.5f, 1f);
                tRt.anchoredPosition = new Vector2(0, -30);
                tRt.sizeDelta = new Vector2(300, 45);

                timerText = timerGo.AddComponent<TextMeshProUGUI>();
                timerText.fontSize = 24;
                timerText.fontStyle = FontStyles.Bold;
                timerText.alignment = TextAlignmentOptions.Center;
                timerText.color = new Color(0.96f, 0.82f, 0.38f, 1f);
                timerText.raycastTarget = false;
            }

            // Tạo Progress Text (Đã làm x/10)
            if (progressText == null)
            {
                GameObject progGo = new GameObject("WeeklyProgressText");
                progGo.transform.SetParent(canvas.transform, false);
                RectTransform pRt = progGo.AddComponent<RectTransform>();
                pRt.anchorMin = new Vector2(0.5f, 1f);
                pRt.anchorMax = new Vector2(0.5f, 1f);
                pRt.pivot = new Vector2(0.5f, 1f);
                pRt.anchoredPosition = new Vector2(280, -30);
                pRt.sizeDelta = new Vector2(240, 45);

                progressText = progGo.AddComponent<TextMeshProUGUI>();
                progressText.fontSize = 20;
                progressText.alignment = TextAlignmentOptions.Right;
                progressText.color = Color.white;
                progressText.raycastTarget = false;
            }

            // Tạo Notice Text
            if (noticeText == null)
            {
                GameObject noticeGo = new GameObject("NoticeText");
                noticeGo.transform.SetParent(canvas.transform, false);
                RectTransform nRt = noticeGo.AddComponent<RectTransform>();
                nRt.anchoredPosition = new Vector2(0, -220);
                nRt.sizeDelta = new Vector2(800, 40);

                noticeText = noticeGo.AddComponent<TextMeshProUGUI>();
                noticeText.fontSize = 20;
                noticeText.alignment = TextAlignmentOptions.Center;
                noticeText.color = new Color(0.95f, 0.35f, 0.3f, 1f);
                noticeText.gameObject.SetActive(false);
            }

            // Tạo / Tìm các nút Prev, Next, Submit, Back
            EnsureNavigationControls(canvas);
        }

        private void EnsureNavigationControls(Canvas canvas)
        {
            // Nút Quay lại
            if (btnBack == null)
            {
                var bGo = GameObject.Find("quaylai") ?? GameObject.Find("Btn_Back");
                if (bGo != null) btnBack = bGo.GetComponent<Button>();
            }

            // Nút Tiếp theo / Câu trước
            if (btnNext == null)
            {
                var nGo = GameObject.Find("tieptheo") ?? GameObject.Find("Btn_Next");
                if (nGo != null) btnNext = nGo.GetComponent<Button>();
            }

            if (btnPrev == null)
            {
                var pGo = GameObject.Find("Btn_Prev");
                if (pGo != null) btnPrev = pGo.GetComponent<Button>();
                else
                {
                    GameObject go = new GameObject("Btn_Prev");
                    go.transform.SetParent(canvas.transform, false);
                    RectTransform rt = go.AddComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(-420, -320);
                    rt.sizeDelta = new Vector2(160, 48);

                    Image img = go.AddComponent<Image>();
                    img.color = new Color(0.12f, 0.25f, 0.45f, 1f);
                    btnPrev = go.AddComponent<Button>();
                    btnPrev.targetGraphic = img;

                    GameObject tGo = new GameObject("Text");
                    tGo.transform.SetParent(go.transform, false);
                    var tmp = tGo.AddComponent<TextMeshProUGUI>();
                    tmp.text = "← CÂU TRƯỚC";
                    tmp.fontSize = 17;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = Color.white;
                    tmp.raycastTarget = false;
                }
            }

            // Nút Nộp bài
            if (btnSubmit == null)
            {
                GameObject sGo = new GameObject("Btn_SubmitWeekly");
                sGo.transform.SetParent(canvas.transform, false);
                RectTransform sRt = sGo.AddComponent<RectTransform>();
                sRt.anchoredPosition = new Vector2(420, -320);
                sRt.sizeDelta = new Vector2(160, 48);

                Image sImg = sGo.AddComponent<Image>();
                sImg.color = new Color(0.15f, 0.65f, 0.35f, 1f);
                btnSubmit = sGo.AddComponent<Button>();
                btnSubmit.targetGraphic = sImg;

                GameObject sTxtGo = new GameObject("Text");
                sTxtGo.transform.SetParent(sGo.transform, false);
                var sTmp = sTxtGo.AddComponent<TextMeshProUGUI>();
                sTmp.text = "NỘP BÀI";
                sTmp.fontSize = 17;
                sTmp.fontStyle = FontStyles.Bold;
                sTmp.alignment = TextAlignmentOptions.Center;
                sTmp.color = Color.white;
                sTmp.raycastTarget = false;
            }
        }

        private void SetupButtonListeners()
        {
            for (int i = 0; i < 4; i++)
            {
                if (answerButtons[i] != null)
                {
                    int idx = i;
                    answerButtons[i].onClick.RemoveAllListeners();
                    answerButtons[i].onClick.AddListener(() => OnOptionClicked(idx));
                    UIHoverPulseEffect.Attach(answerButtons[i].gameObject, hover: 1.05f, minPulse: 1.02f, maxPulse: 1.07f, outline: true);
                }
            }

            if (btnPrev != null)
            {
                UIHoverPulseEffect.Attach(btnPrev.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
                btnPrev.onClick.RemoveAllListeners();
                btnPrev.onClick.AddListener(OnPrevClicked);
            }

            if (btnNext != null)
            {
                UIHoverPulseEffect.Attach(btnNext.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
                btnNext.onClick.RemoveAllListeners();
                btnNext.onClick.AddListener(OnNextClicked);
            }

            if (btnSubmit != null)
            {
                UIHoverPulseEffect.Attach(btnSubmit.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
                btnSubmit.onClick.RemoveAllListeners();
                btnSubmit.onClick.AddListener(OnSubmitClicked);
            }

            if (btnBack != null)
            {
                UIHoverPulseEffect.Attach(btnBack.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackClicked);
            }
        }

        private static PhysicsQuestionRaw ConvertToRaw(G2MCQItem item)
        {
            if (item == null) return null;
            return new PhysicsQuestionRaw
            {
                questionID = item.cauHoiID,
                questionText = item.noiDung,
                answers = new string[] { item.dapAnA, item.dapAnB, item.dapAnC, item.dapAnD },
                correctAnswerIndex = item.correctIndex,
                chapter = item.chuongId,
                difficulty = item.mucDoGame2,
                explanation = item.giaiThich,
                keyKnowledge = item.meoGhiNho
            };
        }

        private void InitializeWeeklyChallenge()
        {
            for (int i = 0; i < 10; i++) _selectedAnswers[i] = -1;

            _weeklyQuestions.Clear();
            // Tạo đề thi tuần 10 câu (3 NB + 3 TH + 3 VD + 1 VDC)
            var g2Questions = GameDataRepository.CreateChapterTestQuestions(1);
            if (g2Questions != null)
            {
                foreach (var q in g2Questions)
                {
                    _weeklyQuestions.Add(ConvertToRaw(q));
                }
            }

            if (_weeklyQuestions.Count < 10)
            {
                var extra = GameDataRepository.GetMCQQuestions("bai2", "NB", 10);
                if (extra != null)
                {
                    foreach (var q in extra)
                    {
                        if (_weeklyQuestions.Count >= 10) break;
                        _weeklyQuestions.Add(ConvertToRaw(q));
                    }
                }
            }

            _currentIndex = 0;
            _timeRemaining = 900f; // 15 phút
            _isSubmitted = false;

            DisplayCurrentQuestion();
            UpdateProgressDisplay();
        }

        private void DisplayCurrentQuestion()
        {
            if (_weeklyQuestions == null || _currentIndex < 0 || _currentIndex >= _weeklyQuestions.Count) return;

            var q = _weeklyQuestions[_currentIndex];

            if (counterText != null)
            {
                counterText.text = $"Câu {_currentIndex + 1}/{_weeklyQuestions.Count}";
            }

            if (questionText != null)
            {
                questionText.text = q.questionText;
            }

            int selected = _selectedAnswers[_currentIndex];

            // Render 4 đáp án (chỉ đổi màu viền/nền khi đang chọn, KHÔNG hiện đúng/sai)
            for (int i = 0; i < 4; i++)
            {
                if (answerButtons[i] != null)
                {
                    string ansText = (q.answers != null && i < q.answers.Length) ? q.answers[i] : "";
                    if (answerTexts[i] != null)
                    {
                        answerTexts[i].text = ansText;
                    }

                    if (answerImages[i] != null)
                    {
                        answerImages[i].color = (i == selected)
                            ? new Color(0.2f, 0.45f, 0.8f, 1f) // Xanh dương chọn
                            : Color.white;
                    }
                }
            }

            // Cập nhật trạng thái nút Prev / Next
            if (btnPrev != null) btnPrev.interactable = (_currentIndex > 0);
            if (btnNext != null) btnNext.interactable = (_currentIndex < _weeklyQuestions.Count - 1);
        }

        private void OnOptionClicked(int optionIndex)
        {
            if (_isSubmitted) return;

            _selectedAnswers[_currentIndex] = optionIndex;
            DisplayCurrentQuestion();
            UpdateProgressDisplay();
        }

        private void OnPrevClicked()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                DisplayCurrentQuestion();
            }
        }

        private void OnNextClicked()
        {
            if (_currentIndex < _weeklyQuestions.Count - 1)
            {
                _currentIndex++;
                DisplayCurrentQuestion();
            }
        }

        private void UpdateProgressDisplay()
        {
            int answeredCount = 0;
            for (int i = 0; i < 10; i++)
            {
                if (_selectedAnswers[i] >= 0) answeredCount++;
            }

            if (progressText != null)
            {
                progressText.text = $"Đã làm: <b>{answeredCount}/10</b>";
            }
        }

        private void UpdateTimerDisplay()
        {
            if (timerText == null) return;
            int totalSec = Mathf.Max(0, Mathf.FloorToInt(_timeRemaining));
            int minutes = totalSec / 60;
            int seconds = totalSec % 60;
            timerText.text = $"⏱ {minutes:D2}:{seconds:D2}";
        }

        private void OnSubmitClicked()
        {
            if (_isSubmitted) return;

            // Kiểm tra đủ 10 câu
            int unansweredCount = 0;
            int firstUnanswered = -1;
            for (int i = 0; i < _weeklyQuestions.Count; i++)
            {
                if (_selectedAnswers[i] < 0)
                {
                    unansweredCount++;
                    if (firstUnanswered < 0) firstUnanswered = i;
                }
            }

            if (unansweredCount > 0)
            {
                ShowNotice($"Bạn còn {unansweredCount} câu chưa làm. Hãy hoàn thành đủ 10 câu trước khi nộp!");
                if (firstUnanswered >= 0)
                {
                    _currentIndex = firstUnanswered;
                    DisplayCurrentQuestion();
                }
                return;
            }

            FinalizeAndSubmitWeekly(isTimeout: false);
        }

        private void AutoSubmitOnTimeOut()
        {
            if (_isSubmitted) return;
            Debug.Log("[WeeklyQuizController] Hết thời gian 15 phút -> Tự động nộp bài phần đã làm.");
            FinalizeAndSubmitWeekly(isTimeout: true);
        }

        private void FinalizeAndSubmitWeekly(bool isTimeout)
        {
            if (_isSubmitted) return;
            _isSubmitted = true;

            int correctCount = 0;
            int totalQ = _weeklyQuestions.Count;

            for (int i = 0; i < totalQ; i++)
            {
                int sel = _selectedAnswers[i];
                if (sel >= 0 && sel == _weeklyQuestions[i].correctAnswerIndex)
                {
                    correctCount++;
                }
            }

            float score = totalQ > 0 ? (100f * correctCount / totalQ) : 0f;
            float elapsed = 900f - _timeRemaining;

            Debug.Log($"[WeeklyQuizController] Kết quả Thử thách tuần: {correctCount}/{totalQ} đúng, Điểm {score:F1}, Thời gian {elapsed:F1}s.");

            // Lưu kết quả vào AttemptContext
            NavigationService.Instance.AttemptContext.AttemptId = Guid.NewGuid().ToString("N");
            NavigationService.Instance.AttemptContext.Score = score;
            NavigationService.Instance.AttemptContext.ElapsedSeconds = elapsed;
            NavigationService.Instance.AttemptContext.CorrectCount = correctCount;
            NavigationService.Instance.AttemptContext.WrongCount = totalQ - correctCount;

            // Ghi nhận lượt tham gia thử thách tuần trong SaveData
            PhysicsSaveData save = PhysicsSaveManager.Load();
            if (save != null)
            {
                string weekId = $"2026-W{Mathf.CeilToInt(System.DateTime.UtcNow.DayOfYear / 7f):D2}";
                if (save.joinedChallengeWeekIds == null) save.joinedChallengeWeekIds = new List<string>();
                if (!save.joinedChallengeWeekIds.Contains(weekId))
                {
                    save.joinedChallengeWeekIds.Add(weekId);
                }
                PhysicsSaveManager.Save(save);
            }

            // Chuyển sang màn Scene 32. Result_Tự luận (RouteId.WeeklyResult)
            NavigationService.Instance.NavigateTo(RouteId.WeeklyResult);
        }

        private void OnBackClicked()
        {
            Debug.Log("[WeeklyQuizController] Bấm Quay lại.");
            ShowExitConfirmModal();
        }

        private void ShowExitConfirmModal()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject modalGo = new GameObject("WeeklyExitConfirmModal");
            modalGo.transform.SetParent(canvas.transform, false);

            RectTransform rt = modalGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image bg = modalGo.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.08f, 0.16f, 0.90f);

            GameObject cardGo = new GameObject("Card");
            cardGo.transform.SetParent(modalGo.transform, false);
            RectTransform cRt = cardGo.AddComponent<RectTransform>();
            cRt.sizeDelta = new Vector2(520, 240);
            cRt.anchoredPosition = Vector2.zero;

            Image cImg = cardGo.AddComponent<Image>();
            cImg.color = new Color(0.08f, 0.14f, 0.28f, 0.98f);

            Outline outline = cardGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            outline.effectDistance = new Vector2(2, -2);

            GameObject msgGo = new GameObject("Message");
            msgGo.transform.SetParent(cardGo.transform, false);
            RectTransform mRt = msgGo.AddComponent<RectTransform>();
            mRt.anchoredPosition = new Vector2(0, 30);
            mRt.sizeDelta = new Vector2(460, 80);
            var msgTmp = msgGo.AddComponent<TextMeshProUGUI>();
            msgTmp.text = "Rời khỏi sẽ kết thúc lượt tính hạng của tuần này.\nBạn có chắc chắn muốn rời không?";
            msgTmp.fontSize = 19;
            msgTmp.alignment = TextAlignmentOptions.Center;
            msgTmp.color = Color.white;

            GameObject btnLeaveGo = new GameObject("Btn_Leave");
            btnLeaveGo.transform.SetParent(cardGo.transform, false);
            RectTransform lRt = btnLeaveGo.AddComponent<RectTransform>();
            lRt.anchoredPosition = new Vector2(-100, -50);
            lRt.sizeDelta = new Vector2(150, 44);
            Image lImg = btnLeaveGo.AddComponent<Image>();
            lImg.color = new Color(0.85f, 0.25f, 0.25f, 1f);
            Button lBtn = btnLeaveGo.AddComponent<Button>();
            lBtn.targetGraphic = lImg;
            GameObject lTxtGo = new GameObject("Text");
            lTxtGo.transform.SetParent(btnLeaveGo.transform, false);
            var lTmp = lTxtGo.AddComponent<TextMeshProUGUI>();
            lTmp.text = "RỜI ĐI";
            lTmp.fontSize = 16;
            lTmp.fontStyle = FontStyles.Bold;
            lTmp.alignment = TextAlignmentOptions.Center;
            lTmp.color = Color.white;
            lTmp.raycastTarget = false;
            lBtn.onClick.AddListener(() =>
            {
                Destroy(modalGo);
                NavigationService.Instance.NavigateTo(RouteId.WeeklyLeaderboard);
            });

            GameObject btnCancelGo = new GameObject("Btn_Cancel");
            btnCancelGo.transform.SetParent(cardGo.transform, false);
            RectTransform canRt = btnCancelGo.AddComponent<RectTransform>();
            canRt.anchoredPosition = new Vector2(100, -50);
            canRt.sizeDelta = new Vector2(150, 44);
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

        private void ShowNotice(string msg)
        {
            if (noticeText == null) return;
            noticeText.text = msg;
            noticeText.gameObject.SetActive(true);

            if (_noticeCoroutine != null) StopCoroutine(_noticeCoroutine);
            _noticeCoroutine = StartCoroutine(HideNoticeRoutine());
        }

        private IEnumerator HideNoticeRoutine()
        {
            yield return new WaitForSeconds(3f);
            if (noticeText != null) noticeText.gameObject.SetActive(false);
        }
    }
}
