// ============================================================
// BiologyQuizUI.cs
// Toàn bộ UI của game SINH TỒN MIỄN DỊCH:
//   • Bảng câu hỏi (có TIMER, hiện đáp án đúng + GIẢI THÍCH khi sai)
//   • Bảng chọn nâng cấp (Common/Rare/Epic/Legendary - màu riêng từng loại)
//   • Banner Knowledge Clash: PERFECT COUNTER / ULTIMATE INCOMING
//   • Hiệu ứng AWAKENING MODE
//   • Màn kết quả chi tiết (điểm, kill, câu đúng/sai, streak...)
// Toàn bộ UI dựng bằng code → không cần setup tay trong Editor.
// ============================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace PRU.Biology
{
    public class BiologyQuizUI : MonoBehaviour
    {
        public static BiologyQuizUI Instance { get; private set; }

        [Header("Trạng thái (đọc để debug)")]
        public bool isShowingQuestion;

        // ---------- Panel câu hỏi ----------
        private GameObject _questionRoot;
        private RectTransform _frameRt;
        private TextMeshProUGUI _questionText;
        private Image _timerFill;
        private TextMeshProUGUI _timerText;
        private TextMeshProUGUI _feedbackText;
        private Button[] _answerButtons = new Button[4];
        private TextMeshProUGUI[] _answerTexts = new TextMeshProUGUI[4];
        private Image[] _answerBgs = new Image[4];

        // ---------- Panel nâng cấp ----------
        private GameObject _upgradeRoot;
        private RectTransform _upgradeFrameRt;
        private TextMeshProUGUI _upgradeTitle;
        private Button[] _upgradeButtons = new Button[3];
        private TextMeshProUGUI[] _upgradeTexts = new TextMeshProUGUI[3];
        private Image[] _upgradeBgs = new Image[3];

        // ---------- Panel kết thúc ----------
        private GameObject _endRoot;
        private RectTransform _endFrameRt;
        private TextMeshProUGUI _endTitle;
        private TextMeshProUGUI _endStats;

        // ---------- Banner + Awakening ----------
        private TextMeshProUGUI _bannerText;
        private GameObject _awakeningRoot;
        private TextMeshProUGUI _awakeningText;

        // ---------- Nội bộ ----------
        private System.Action<bool> _onAnswered;
        private bool _answered;
        private int _correctIndex;
        private Coroutine _timerCo;
        private System.Action<int> _onUpgradeChosen;

        private Color _normalColor = new Color(0.35f, 0.55f, 0.85f);
        private Color _correctColor = new Color(0.35f, 0.85f, 0.45f);
        private Color _wrongColor = new Color(0.9f, 0.35f, 0.35f);

        private static readonly Color[] RarityColors =
        {
            new Color(0.55f, 0.60f, 0.65f), // Common - xám
            new Color(0.30f, 0.55f, 0.95f), // Rare - xanh dương
            new Color(0.62f, 0.35f, 0.92f), // Epic - tím
            new Color(1f, 0.55f, 0.15f),    // Legendary - cam vàng
        };
        private static readonly string[] RarityTags =
        {
            "",                            // Common
            "★ HIẾM ★\n",                  // Rare
            "★★ SỬ THUYẾT ★★\n",           // Epic
            "✦✦✦ THẦN THOẠI ✦✦✦\n",        // Legendary
        };

        private void Awake()
        {
            Instance = this;
            BuildUI();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ================================================================
        // DỰNG UI
        // ================================================================
        private void BuildUI()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280f, 720f);
                scaler.matchWidthOrHeight = 0.5f;
                gameObject.AddComponent<GraphicRaycaster>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 20;
            }

            BuildQuestionPanel();
            BuildEndPanel();
            BuildUpgradePanel();
            BuildBannerAndAwakening();

            FitFrame();
        }

        private void BuildQuestionPanel()
        {
            _questionRoot = new GameObject("QuestionPanel");
            _questionRoot.transform.SetParent(transform, false);
            RectTransform rootRt = _questionRoot.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.sizeDelta = Vector2.zero;
            _questionRoot.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            GameObject frame = new GameObject("Frame");
            frame.transform.SetParent(_questionRoot.transform, false);
            RectTransform frameRt = frame.AddComponent<RectTransform>();
            frameRt.anchorMin = new Vector2(0.5f, 0.5f);
            frameRt.anchorMax = new Vector2(0.5f, 0.5f);
            frameRt.pivot = new Vector2(0.5f, 0.5f);
            frameRt.sizeDelta = new Vector2(780f, 600f);
            Image frameImg = frame.AddComponent<Image>();
            frameImg.sprite = BiologySpriteFactory.GetWhiteSprite();
            frameImg.color = new Color(0.13f, 0.3f, 0.24f);
            _frameRt = frameRt;

            CreateText(frame.transform, "Header", "CÂU HỎI SINH HỌC", 26, TextAlignmentOptions.Center,
                       new Vector2(0f, 240f), new Vector2(700f, 44f));

            // Đồng hồ đếm ngược
            _timerText = CreateText(frame.transform, "TimerText", "", 22, TextAlignmentOptions.Center,
                                    new Vector2(330f, 240f), new Vector2(120f, 36f));
            _timerText.color = new Color(1f, 0.85f, 0.4f);

            // Thanh thời gian
            GameObject timerBg = new GameObject("TimerBg");
            timerBg.transform.SetParent(frame.transform, false);
            RectTransform tbRt = timerBg.AddComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0.5f, 0.5f);
            tbRt.anchorMax = new Vector2(0.5f, 0.5f);
            tbRt.anchoredPosition = new Vector2(0f, 208f);
            tbRt.sizeDelta = new Vector2(700f, 10f);
            timerBg.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.4f);

            GameObject timerFillGo = new GameObject("TimerFill");
            timerFillGo.transform.SetParent(timerBg.transform, false);
            RectTransform tfRt = timerFillGo.AddComponent<RectTransform>();
            tfRt.anchorMin = Vector2.zero;
            tfRt.anchorMax = Vector2.one;
            tfRt.sizeDelta = Vector2.zero;
            _timerFill = timerFillGo.AddComponent<Image>();
            _timerFill.sprite = BiologySpriteFactory.GetWhiteSprite();
            _timerFill.color = new Color(1f, 0.85f, 0.4f);
            _timerFill.type = Image.Type.Filled;
            _timerFill.fillMethod = Image.FillMethod.Horizontal;

            // Câu hỏi
            _questionText = CreateText(frame.transform, "QuestionText", "", 27, TextAlignmentOptions.Center,
                                       new Vector2(0f, 135f), new Vector2(700f, 110f));
            _questionText.color = Color.white;

            // 4 nút đáp án
            string[] labels = { "A", "B", "C", "D" };
            for (int i = 0; i < 4; i++)
            {
                GameObject btn = new GameObject($"Answer_{labels[i]}");
                btn.transform.SetParent(frame.transform, false);
                RectTransform rt = btn.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 30f - i * 72f);
                rt.sizeDelta = new Vector2(700f, 62f);

                Image bg = btn.AddComponent<Image>();
                bg.sprite = BiologySpriteFactory.GetWhiteSprite();
                bg.color = _normalColor;

                Button b = btn.AddComponent<Button>();
                int idx = i;
                b.onClick.AddListener(() => OnAnswerButtonClicked(idx));

                GameObject label = new GameObject("Label");
                label.transform.SetParent(btn.transform, false);
                RectTransform lrt = label.AddComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = new Vector2(20f, 6f);
                lrt.offsetMax = new Vector2(-20f, -6f);
                TextMeshProUGUI tmp = label.AddComponent<TextMeshProUGUI>();
                tmp.fontSize = 22;
                tmp.alignment = TextAlignmentOptions.Left;
                tmp.color = Color.white;
                tmp.textWrappingMode = TextWrappingModes.Normal;

                _answerButtons[i] = b;
                _answerTexts[i] = tmp;
                _answerBgs[i] = bg;
            }

            // Feedback (đúng/sai + giải thích)
            _feedbackText = CreateText(frame.transform, "FeedbackText", "", 21, TextAlignmentOptions.Center,
                                       new Vector2(0f, -240f), new Vector2(700f, 90f));
            _feedbackText.color = Color.yellow;
            _feedbackText.gameObject.SetActive(false);

            _questionRoot.SetActive(false);
        }

        private void BuildEndPanel()
        {
            _endRoot = new GameObject("EndPanel");
            _endRoot.transform.SetParent(transform, false);
            RectTransform endRt = _endRoot.AddComponent<RectTransform>();
            endRt.anchorMin = Vector2.zero;
            endRt.anchorMax = Vector2.one;
            endRt.sizeDelta = Vector2.zero;
            _endRoot.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

            GameObject endFrame = new GameObject("EndFrame");
            endFrame.transform.SetParent(_endRoot.transform, false);
            RectTransform endFrameRt = endFrame.AddComponent<RectTransform>();
            endFrameRt.anchorMin = new Vector2(0.5f, 0.5f);
            endFrameRt.anchorMax = new Vector2(0.5f, 0.5f);
            endFrameRt.sizeDelta = new Vector2(660f, 580f);
            _endFrameRt = endFrameRt;
            Image endFrameImg = endFrame.AddComponent<Image>();
            endFrameImg.sprite = BiologySpriteFactory.GetWhiteSprite();
            endFrameImg.color = new Color(0.13f, 0.3f, 0.24f);

            _endTitle = CreateText(endFrame.transform, "Title", "", 44, TextAlignmentOptions.Center,
                                   new Vector2(0f, 200f), new Vector2(580f, 80f));

            _endStats = CreateText(endFrame.transform, "Stats", "", 24, TextAlignmentOptions.Center,
                                   new Vector2(0f, 60f), new Vector2(580f, 260f));
            _endStats.color = new Color(0.9f, 0.95f, 0.9f);

            CreateButton(endFrame.transform, "RestartButton", "CHƠI LẠI",
                         new Vector2(0f, -150f), new Vector2(280f, 58f),
                         new Color(0.3f, 0.65f, 0.4f), OnRestartClicked);

            CreateButton(endFrame.transform, "MenuButton", "VỀ MENU",
                         new Vector2(0f, -235f), new Vector2(280f, 58f),
                         new Color(0.75f, 0.45f, 0.3f), OnMenuClicked);

            _endRoot.SetActive(false);
        }

        private void BuildUpgradePanel()
        {
            _upgradeRoot = new GameObject("UpgradePanel");
            _upgradeRoot.transform.SetParent(transform, false);
            RectTransform upRt = _upgradeRoot.AddComponent<RectTransform>();
            upRt.anchorMin = Vector2.zero;
            upRt.anchorMax = Vector2.one;
            upRt.sizeDelta = Vector2.zero;
            _upgradeRoot.AddComponent<Image>().color = new Color(0f, 0.05f, 0.15f, 0.8f);

            GameObject frame = new GameObject("UpgradeFrame");
            frame.transform.SetParent(_upgradeRoot.transform, false);
            RectTransform frameRt = frame.AddComponent<RectTransform>();
            frameRt.anchorMin = new Vector2(0.5f, 0.5f);
            frameRt.anchorMax = new Vector2(0.5f, 0.5f);
            frameRt.sizeDelta = new Vector2(680f, 540f);
            _upgradeFrameRt = frameRt;
            Image frameImg = frame.AddComponent<Image>();
            frameImg.sprite = BiologySpriteFactory.GetWhiteSprite();
            frameImg.color = new Color(0.10f, 0.16f, 0.28f);

            _upgradeTitle = CreateText(frame.transform, "UpgradeTitle", "CHỌN NÂNG CẤP", 34,
                                       TextAlignmentOptions.Center, new Vector2(0f, 200f), new Vector2(620f, 70f));
            _upgradeTitle.color = new Color(0.4f, 0.95f, 1f);

            for (int i = 0; i < 3; i++)
            {
                GameObject btn = new GameObject($"Upgrade_{i}");
                btn.transform.SetParent(frame.transform, false);
                RectTransform rt = btn.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 80f - i * 140f);
                rt.sizeDelta = new Vector2(600f, 115f);

                Image bg = btn.AddComponent<Image>();
                bg.sprite = BiologySpriteFactory.GetWhiteSprite();
                bg.color = RarityColors[0];

                Button b = btn.AddComponent<Button>();
                int idx = i;
                b.onClick.AddListener(() => OnUpgradeClicked(idx));

                GameObject label = new GameObject("Label");
                label.transform.SetParent(btn.transform, false);
                RectTransform lrt = label.AddComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = new Vector2(16f, 8f);
                lrt.offsetMax = new Vector2(-16f, -8f);
                TextMeshProUGUI tmp = label.AddComponent<TextMeshProUGUI>();
                tmp.fontSize = 24;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                tmp.textWrappingMode = TextWrappingModes.Normal;

                _upgradeButtons[i] = b;
                _upgradeTexts[i] = tmp;
                _upgradeBgs[i] = bg;
            }

            _upgradeRoot.SetActive(false);
        }

        private void BuildBannerAndAwakening()
        {
            // Banner giữa màn (Perfect Counter / Ultimate Incoming)
            _bannerText = CreateText(transform, "BannerText", "", 52, TextAlignmentOptions.Center,
                                     new Vector2(0f, 140f), new Vector2(1100f, 130f));
            _bannerText.color = Color.yellow;
            _bannerText.outlineWidth = 0.4f;
            _bannerText.gameObject.SetActive(false);

            // Awakening overlay
            _awakeningRoot = new GameObject("AwakeningPanel");
            _awakeningRoot.transform.SetParent(transform, false);
            RectTransform awRt = _awakeningRoot.AddComponent<RectTransform>();
            awRt.anchorMin = Vector2.zero;
            awRt.anchorMax = Vector2.one;
            awRt.sizeDelta = Vector2.zero;
            _awakeningRoot.AddComponent<Image>().color = new Color(1f, 0.75f, 0.1f, 0.28f);

            _awakeningText = CreateText(_awakeningRoot.transform, "AwakeningText",
                                        "AWAKENING!", 90, TextAlignmentOptions.Center,
                                        new Vector2(0f, 0f), new Vector2(1100f, 160f));
            _awakeningText.color = new Color(1f, 0.95f, 0.5f);
            _awakeningText.outlineWidth = 0.5f;

            _awakeningRoot.SetActive(false);
        }

        // ================================================================
        // HIỂN THỊ CÂU HỎI (có timer)
        // ================================================================
        public void ShowQuestion(BiologyQuestion q, System.Action<bool> onAnswered)
        {
            _onAnswered = onAnswered;
            _answered = false;
            _correctIndex = q.correctIndex;
            _currentQuestion = q;
            isShowingQuestion = true;

            _questionText.text = q.question;
            string[] answers = { q.answerA, q.answerB, q.answerC, q.answerD };
            string[] labels = { "A", "B", "C", "D" };

            for (int i = 0; i < 4; i++)
            {
                _answerTexts[i].text = $"{labels[i]}. {answers[i]}";
                _answerBgs[i].color = _normalColor;
                _answerButtons[i].interactable = true;
            }

            _feedbackText.gameObject.SetActive(false);
            _questionRoot.SetActive(true);

            // Bắt đầu đếm ngược (chạy bằng thời gian THỰC vì game đang pause)
            float limit = BiologyGameConfig.QUESTION_TIMER_SECONDS;
            if (_timerCo != null) StopCoroutine(_timerCo);
            _timerCo = limit > 0f ? StartCoroutine(TimerTick(limit)) : null;
            if (limit <= 0f)
            {
                _timerText.text = "";
                _timerFill.fillAmount = 1f;
            }
        }

        private IEnumerator TimerTick(float limit)
        {
            float t = limit;
            while (t > 0f && !_answered)
            {
                t -= Time.unscaledDeltaTime;
                float frac = Mathf.Clamp01(t / limit);
                _timerFill.fillAmount = frac;
                _timerText.text = $"{t:0}s";
                _timerText.color = t <= 5f ? new Color(1f, 0.4f, 0.35f) : new Color(1f, 0.85f, 0.4f);
                _timerFill.color = _timerText.color;
                yield return null;
            }

            // Hết giờ mà chưa trả lời → coi như SAI (để hiện đáp án + giải thích)
            if (!_answered && _questionRoot.activeSelf)
            {
                ResolveAnswer(-1);
            }
        }

        private void OnAnswerButtonClicked(int idx)
        {
            if (_answered) return;
            ResolveAnswer(idx);
        }

        private void ResolveAnswer(int idx)
        {
            if (_answered) return;
            _answered = true;
            isShowingQuestion = false;
            if (_timerCo != null) { StopCoroutine(_timerCo); _timerCo = null; }

            bool correct = idx == _correctIndex;

            // Tô màu: đáp án đúng luôn xanh; nếu chọn sai thì tô đỏ ô đã chọn (hết giờ thì không tô đỏ ô nào)
            _answerBgs[_correctIndex].color = _correctColor;
            if (!correct && idx >= 0) _answerBgs[idx].color = _wrongColor;

            for (int i = 0; i < 4; i++) _answerButtons[i].interactable = false;

            if (correct)
            {
                _feedbackText.text = "CHÍNH XÁC!";
                _feedbackText.color = _correctColor;
            }
            else
            {
                string header = idx < 0 ? "HẾT GIỜ!" : "SAI RỒI!";
                string[] labels = { "A", "B", "C", "D" };
                _pendingFeedback = $"{header}  Đáp án đúng: {labels[_correctIndex]}";

                BiologyQuestion q = _currentQuestion;
                if (q != null && !string.IsNullOrEmpty(q.explanation))
                    _feedbackText.text = $"{_pendingFeedback}\n<size=18><color=#ffe28a>Giải thích: {q.explanation}</color></size>";
                else
                    _feedbackText.text = _pendingFeedback;

                _feedbackText.color = _wrongColor;
            }

            _feedbackText.gameObject.SetActive(true);

            float delay = correct
                ? BiologyGameConfig.ANSWER_FEEDBACK_CORRECT_TIME
                : BiologyGameConfig.ANSWER_FEEDBACK_WRONG_TIME;
            StartCoroutine(CloseAfterDelay(delay, correct));
        }

        private string _pendingFeedback;

        private IEnumerator CloseAfterDelay(float delay, bool correct)
        {
            yield return new WaitForSecondsRealtime(delay);
            _questionRoot.SetActive(false);
            _onAnswered?.Invoke(correct);
        }

        private BiologyQuestion _currentQuestion;

        // ================================================================
        // BẢNG CHỌN NÂNG CẤP (theo rarity)
        // ================================================================
        public void ShowUpgradeChoice(List<BiologySurvivalManager.BioUpgrade> options, System.Action<int> onChosen)
        {
            _onUpgradeChosen = onChosen;
            _upgradeTitle.text = "CHỌN NÂNG CẤP";

            int count = Mathf.Min(options.Count, 3);
            float[] y3 = { 80f, -60f, -200f };
            float[] y2 = { 40f, -110f };

            for (int i = 0; i < 3; i++)
            {
                bool active = i < count;
                _upgradeButtons[i].gameObject.SetActive(active);
                if (!active) continue;

                var up = options[i];
                _upgradeTexts[i].text = RarityTags[(int)up.Rarity] + BiologySurvivalManager.UpgradeName(up);
                _upgradeBgs[i].color = RarityColors[(int)up.Rarity];
                _upgradeButtons[i].interactable = true;

                RectTransform rt = _upgradeButtons[i].transform as RectTransform;
                rt.anchoredPosition = new Vector2(0f, count == 2 ? y2[i] : y3[i]);
            }

            _upgradeRoot.SetActive(true);
        }

        private void OnUpgradeClicked(int idx)
        {
            _upgradeRoot.SetActive(false);
            _onUpgradeChosen?.Invoke(idx);
        }

        // ================================================================
        // BANNER + AWAKENING
        // ================================================================
        public void ShowPerfectCounter(float stunSeconds)
        {
            ShowBanner($"PERFECT COUNTER!\nBoss bị choáng {stunSeconds:0}s - TẤN CÔNG!", new Color(1f, 0.9f, 0.3f), 1.2f);
        }

        public void ShowUltimateIncoming()
        {
            ShowBanner("BOSS TUNG ULTIMATE!\nNÉ VIÊN ĐẠN!", new Color(1f, 0.35f, 0.3f), 1.2f);
        }

        private void ShowBanner(string msg, Color color, float duration)
        {
            _bannerText.text = msg;
            _bannerText.color = color;
            _bannerText.gameObject.SetActive(true);
            StartCoroutine(HideBannerAfter(duration));
        }

        private IEnumerator HideBannerAfter(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            if (_bannerText != null) _bannerText.gameObject.SetActive(false);
        }

        public void ShowAwakening(float duration)
        {
            StartCoroutine(AwakeningFx(duration));
        }

        private IEnumerator AwakeningFx(float duration)
        {
            _awakeningRoot.SetActive(true);
            CanvasGroup cg = _awakeningRoot.GetComponent<CanvasGroup>();
            if (cg == null) cg = _awakeningRoot.AddComponent<CanvasGroup>();

            // Nhấp nháy mạnh 2 giây đầu
            float t = 0f;
            while (t < 2f)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = 0.5f + 0.5f * Mathf.Sin(t * 12f);
                yield return null;
            }
            cg.alpha = 0f;
            _awakeningRoot.SetActive(false);
        }

        // ================================================================
        // MÀN KẾT QUẢ CHI TIẾT
        // ================================================================
        public void ShowResults(bool victory)
        {
            BiologySurvivalManager mgr = BiologySurvivalManager.Instance;
            BiologyQuestionBank bank = BiologyQuestionBank.Instance;

            int correct = bank != null ? bank.TotalCorrect() : 0;
            int wrong = bank != null ? bank.TotalWrong() : 0;
            int total = correct + wrong;
            float acc = total > 0 ? (float)correct / total * 100f : 0f;

            string title = victory ? "CHIẾN THẮNG!" : "GAME OVER";
            Color titleColor = victory ? new Color(0.35f, 0.85f, 0.45f) : new Color(0.9f, 0.3f, 0.35f);

            string stats =
                $"Điểm: {(mgr != null ? mgr.Score : 0)}\n" +
                $"Mầm bệnh tiêu diệt: {(mgr != null ? mgr.Kills : 0)}   |   Cấp độ đạt: Lv.{(mgr != null ? mgr.Level : 1)}\n" +
                $"Thời gian sống sót: {FormatTime(mgr != null ? mgr.Elapsed : 0f)}\n" +
                $"─────────────────────\n" +
                $"Câu hỏi: {correct}/{total} đúng  ({acc:0}%)\n" +
                $"Chuỗi đúng dài nhất: {(mgr != null ? mgr.BestStreak : 0)}\n" +
                (victory ? "\nBạn là nhà miễn dịch học tương lai!" : "\nĐừng bỏ cuộc - thử lại nhé!");

            _endTitle.text = title;
            _endTitle.color = titleColor;
            _endStats.text = stats;
            _questionRoot.SetActive(false);
            if (_upgradeRoot != null) _upgradeRoot.SetActive(false);
            _endRoot.SetActive(true);
            Time.timeScale = 0f;
        }

        private static string FormatTime(float seconds)
        {
            int m = Mathf.FloorToInt(seconds / 60f);
            int s = Mathf.FloorToInt(seconds % 60f);
            return $"{m:00}:{s:00}";
        }

        public void HideAll()
        {
            Time.timeScale = 1f;
            _questionRoot.SetActive(false);
            _endRoot.SetActive(false);
            if (_upgradeRoot != null) _upgradeRoot.SetActive(false);
            if (_awakeningRoot != null) _awakeningRoot.SetActive(false);
            if (_bannerText != null) _bannerText.gameObject.SetActive(false);
        }

        // ================================================================
        // TIỆN ÍCH
        // ================================================================
        private void OnRestartClicked()
        {
            Time.timeScale = 1f;
            _endRoot.SetActive(false);
            BiologySurvivalManager.Instance?.RestartGame();
        }

        private void OnMenuClicked()
        {
            Time.timeScale = 1f;
            BiologySurvivalManager.Instance?.BackToMenu();
        }

        private void OnRectTransformDimensionsChange()
        {
            FitFrame();
        }

        private void FitFrame()
        {
            RectTransform canvasRt = transform as RectTransform;
            if (canvasRt == null) return;
            float h = canvasRt.rect.height;
            float w = canvasRt.rect.width;
            if (h <= 0f || w <= 0f) return;

            float kQ = Mathf.Min(1f, h / 660f, w / 820f);
            if (_frameRt != null) _frameRt.localScale = new Vector3(kQ, kQ, 1f);

            float kE = Mathf.Min(1f, h / 640f, w / 700f);
            if (_endFrameRt != null) _endFrameRt.localScale = new Vector3(kE, kE, 1f);

            float kU = Mathf.Min(1f, h / 580f, w / 720f);
            if (_upgradeFrameRt != null) _upgradeFrameRt.localScale = new Vector3(kU, kU, 1f);
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, string text, int size,
                                           TextAlignmentOptions align, Vector2 pos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size,
                                    Color color, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.sprite = BiologySpriteFactory.GetWhiteSprite();
            img.color = color;

            Button btn = go.AddComponent<Button>();
            btn.onClick.AddListener(onClick);

            GameObject labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            RectTransform lrt = labelGo.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 26;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return btn;
        }
    }
}
