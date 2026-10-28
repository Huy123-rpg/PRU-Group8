using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using ScienceQuest.Quiz;

namespace ScienceQuest.PhysicsGame1
{
    using QuestionData = ScienceQuest.Quiz.QuestionData;

    public class PhysicsGame1_DuckShootingController : MonoBehaviour
    {
        [Header("=== HUD REFERENCES ===")]
        private TextMeshProUGUI _scoreText;
        private TextMeshProUGUI _timeText;
        private TextMeshProUGUI _questionCounterText;

        [Header("=== SÚNG & HỒNG TÂM ===")]
        private RectTransform _crosshair;
        private RectTransform _gun;
        private Canvas _canvas;

        [Header("=== VỊT & MỤC TIÊU ===")]
        private List<DuckItem> _ducks = new List<DuckItem>();
        private class DuckItem
        {
            public GameObject gameObject;
            public RectTransform rectTransform;
            public Vector2 basePos;
            public float speed;
            public float bobOffset;
            public bool movingRight;
            public bool isBlue;
            public bool isBomb;
            public bool isHit;
            public float respawnTimer = 0f;
            public Vector3 defaultScale;
        }

        [Header("=== POPUP TRẮC NGHIỆM ===")]
        private GameObject _popupTracNghiem;
        private TextMeshProUGUI _tmpDebaiTN;
        private Button[] _btnAnswers = new Button[4];
        private TextMeshProUGUI[] _tmpAnswers = new TextMeshProUGUI[4];
        private Button _btnNopbaiTN;
        private TextMeshProUGUI _tmpResultFeedback;
        private GameObject _feedbackBanner;
        private Image _feedbackBannerBg;
        private TextMeshProUGUI _tmpResultTitle;
        private TextMeshProUGUI _tmpResultSubtitle;
        private Coroutine _scoreBumpCoroutine;
        private int _selectedChoiceIndex = -1;
        private bool _isCurrentBlueDuck = false;

        [Header("=== POPUP TỰ LUẬN ===")]
        private GameObject _popupTuLuan;
        private TextMeshProUGUI _tmpDebaiTL;
        private TMP_InputField _inputFieldTL;
        private Button _btnUploadImg;
        private Button _btnDeleteImg;
        private Image _previewImg;
        private Button _btnNopbaiTL;
        private bool _hasAttachedImage = false;
        private string _attachedImageBase64 = "";
        private enum EssayState { Editing, Grading, Passed, Failed }
        private EssayState _essayState = EssayState.Editing;
        private string _lastStudentEssayDraft = "";
        private AIGradingResponse _lastGradingResp;

        [Header("=== POPUP PAUSE ===")]
        private GameObject _popupPause;
        private GameObject _gatnutBGM;
        private GameObject _nutxamBGM;
        private GameObject _gatnutSFX;
        private GameObject _nutxamSFX;

        [Header("=== TRẠNG THÁI GAMEPLAY ===")]
        private bool _isPaused = false;
        private bool _isPopupOpen = false;
        private bool _isGameOver = false;
        private int _currentQuestionIndex = 0;

        private void Start()
        {
            Debug.Log("[PhysicsGame1] 🦆 Khởi tạo màn chơi Bắn Vịt (07_DuckShooting)");

            _canvas = FindFirstObjectByType<Canvas>();

            SetupCharacterDisplay();
            SetupHUD();
            SetupGunAndCrosshair();
            SetupDucks();
            SetupMultipleChoicePopup();
            SetupEssayPopup();
            SetupPausePopup();

            UpdateHUD();
        }

        #region ===== 1. NHÂN VẬT HỌC SINH =====
        private void SetupCharacterDisplay()
        {
            string selectedId = PhysicsGame1Session.Instance.SelectedCharacterId;
            string[] charIds = new string[] { "nv1", "nv2", "nv3", "nv4" };

            foreach (var cid in charIds)
            {
                GameObject charObj = GameObject.Find(cid);
                if (charObj != null)
                {
                    charObj.SetActive(cid.Equals(selectedId, StringComparison.OrdinalIgnoreCase));
                    foreach (var g in charObj.GetComponentsInChildren<Graphic>(true))
                    {
                        g.raycastTarget = false;
                    }
                }
            }
        }
        #endregion

        #region ===== 2. HUD & ĐỒNG HỒ =====
        private void SetupHUD()
        {
            // Điểm số: căn giữa chính xác vào ô chữ nhật màu nâu sẫm trên biển gỗ ĐIỂM
            GameObject scoreObj = GameObject.Find("ghidiem") ?? GameObject.Find("score");
            if (scoreObj != null)
            {
                _scoreText = scoreObj.GetComponent<TextMeshProUGUI>() ?? scoreObj.GetComponentInChildren<TextMeshProUGUI>(true);
                if (_scoreText != null)
                {
                    _scoreText.raycastTarget = false;
                    _scoreText.textWrappingMode = TextWrappingModes.NoWrap;
                    _scoreText.alignment = TextAlignmentOptions.Center;
                    _scoreText.fontStyle = FontStyles.Bold;
                    _scoreText.fontSize = 32f;
                    _scoreText.color = new Color(0.25f, 0.15f, 0.05f);

                    RectTransform srt = _scoreText.rectTransform;
                    if (srt != null)
                    {
                        srt.anchorMin = new Vector2(0.5f, 0.5f);
                        srt.anchorMax = new Vector2(0.5f, 0.5f);
                        srt.pivot = new Vector2(0.5f, 0.5f);
                        srt.anchoredPosition = new Vector2(114f, -6f);
                        srt.sizeDelta = new Vector2(100f, 48f);
                    }
                }
            }

            // Tiến độ câu hỏi: căn giữa chính xác vào ô bầu dục sáng màu trên biển gỗ CÂU
            GameObject cauObj = GameObject.Find("ghicauso") ?? GameObject.Find("causo");
            if (cauObj != null)
            {
                _questionCounterText = cauObj.GetComponent<TextMeshProUGUI>() ?? cauObj.GetComponentInChildren<TextMeshProUGUI>(true);
                if (_questionCounterText != null)
                {
                    _questionCounterText.raycastTarget = false;
                    _questionCounterText.textWrappingMode = TextWrappingModes.NoWrap;
                    _questionCounterText.alignment = TextAlignmentOptions.Center;
                    _questionCounterText.fontStyle = FontStyles.Bold;
                    _questionCounterText.fontSize = 26f;
                    _questionCounterText.color = new Color(0.25f, 0.15f, 0.05f);

                    RectTransform crt = _questionCounterText.rectTransform;
                    if (crt != null)
                    {
                        crt.anchorMin = new Vector2(0.5f, 0.5f);
                        crt.anchorMax = new Vector2(0.5f, 0.5f);
                        crt.pivot = new Vector2(0.5f, 0.5f);
                        crt.anchoredPosition = new Vector2(55f, -8f);
                        crt.sizeDelta = new Vector2(90f, 44f);
                    }
                }
            }

            // Thời gian: căn giữa chính xác vào ô bo tròn màu vàng nhạt trên biển THỜI GIAN
            GameObject timeObj = GameObject.Find("ghitime") ?? GameObject.Find("time");
            if (timeObj != null)
            {
                _timeText = timeObj.GetComponent<TextMeshProUGUI>() ?? timeObj.GetComponentInChildren<TextMeshProUGUI>(true);
                if (_timeText != null)
                {
                    _timeText.raycastTarget = false;
                    _timeText.textWrappingMode = TextWrappingModes.NoWrap;
                    _timeText.alignment = TextAlignmentOptions.Center;
                    _timeText.fontStyle = FontStyles.Bold;
                    _timeText.fontSize = 28f;
                    _timeText.color = new Color(0.25f, 0.15f, 0.05f);

                    RectTransform trt = _timeText.rectTransform;
                    if (trt != null)
                    {
                        trt.anchorMin = new Vector2(0.5f, 0.5f);
                        trt.anchorMax = new Vector2(0.5f, 0.5f);
                        trt.pivot = new Vector2(0.5f, 0.5f);
                        trt.anchoredPosition = new Vector2(150f, -7f);
                        trt.sizeDelta = new Vector2(125f, 48f);
                    }
                }
            }

            // Nút Pause
            GameObject pauseObj = GameObject.Find("pause");
            UIHelper.GetOrAddButton(pauseObj, OnPauseClicked);
        }

        private void UpdateHUD()
        {
            if (_scoreText != null)
            {
                // Chỉ hiển thị số điểm (ví dụ: 10)
                _scoreText.text = $"{PhysicsGame1Session.Instance.CurrentScore}";
            }

            if (_questionCounterText != null)
            {
                // Hiển thị định dạng số câu linh hoạt theo chế độ (Trắc nghiệm: /10, Tự luận: /5)
                int totalQuestions = PhysicsGame1Session.Instance.MaxQuestionsForCurrentSession;
                int currentQNum = Mathf.Clamp(_currentQuestionIndex + 1, 1, totalQuestions);
                _questionCounterText.text = $"{currentQNum}/{totalQuestions}";
            }

            if (_timeText != null)
            {
                float rem = Mathf.Max(0f, PhysicsGame1Session.Instance.RemainingTime);
                int m = (int)(rem / 60);
                int s = (int)(rem % 60);

                if (PhysicsGame1Session.Instance.FreezeTimeRemaining > 0f)
                {
                    _timeText.text = $"❄️ {m:00}:{s:00}";
                    _timeText.color = new Color(0.05f, 0.55f, 0.85f); // Xanh băng tuyết đậm nét, nổi rõ trên nền vàng nhạt
                }
                else
                {
                    _timeText.text = $"{m:00}:{s:00}";
                    _timeText.color = new Color(0.25f, 0.15f, 0.05f); // Màu nâu gỗ sẫm rõ ràng, ăn khớp với toàn bộ HUD
                }
            }
        }
        #endregion

        #region ===== 3. SÚNG & HỒNG TÂM =====
        private void SetupGunAndCrosshair()
        {
            GameObject muctieuObj = GameObject.Find("muctieu");
            if (muctieuObj != null)
            {
                _crosshair = muctieuObj.GetComponent<RectTransform>();
                // Vô hiệu hóa raycastTarget trên hồng tâm để nhấp chuột xuyên qua trúng vịt
                foreach (var g in muctieuObj.GetComponentsInChildren<Graphic>(true))
                {
                    g.raycastTarget = false;
                }
            }

            GameObject sungObj = GameObject.Find("súng") ?? GameObject.Find("sung");
            if (sungObj != null)
            {
                _gun = sungObj.GetComponent<RectTransform>();
                // Vô hiệu hóa raycastTarget trên khẩu súng
                foreach (var g in sungObj.GetComponentsInChildren<Graphic>(true))
                {
                    g.raycastTarget = false;
                }
            }
        }
        #endregion

        #region ===== 4. CÁC MỤC TIÊU VỊT =====
        private void SetupDucks()
        {
            _ducks.Clear();
            string[] duckNames = new string[]
            {
                "vitvang", "vitvang (1)",
                "vitnau", "vitnau (1)",
                "vitxanh", "vitxanh (1)",
                "vitboom", "vitboom (1)"
            };

            float[] speeds = new float[] { 70f, 85f, 65f, 75f, 90f, 95f, 80f, 70f };
            float[] offsets = new float[] { 0f, 1.5f, 0.8f, 2.3f, 1.1f, 3.0f, 0.5f, 2.0f };

            for (int i = 0; i < duckNames.Length; i++)
            {
                GameObject dObj = GameObject.Find(duckNames[i]);
                if (dObj != null)
                {
                    RectTransform rt = dObj.GetComponent<RectTransform>();
                    if (rt == null) rt = dObj.AddComponent<RectTransform>();

                    Image img = dObj.GetComponent<Image>();
                    if (img != null) img.raycastTarget = true;

                    DuckItem item = new DuckItem
                    {
                        gameObject = dObj,
                        rectTransform = rt,
                        basePos = rt.anchoredPosition,
                        speed = speeds[i % speeds.Length],
                        bobOffset = offsets[i % offsets.Length],
                        movingRight = (i % 2 == 0),
                        isBlue = duckNames[i].Contains("xanh"),
                        isBomb = duckNames[i].Contains("boom"),
                        defaultScale = rt.localScale
                    };

                    _ducks.Add(item);

                    // Gán sự kiện click bắn vịt
                    UIHelper.GetOrAddButton(dObj, () => OnDuckClicked(item));
                }
            }
        }

        private void OnDuckClicked(DuckItem duck)
        {
            if (_isPaused || _isPopupOpen || _isGameOver || duck.isHit) return;
            duck.isHit = true;

            Debug.Log($"[PhysicsGame1] 🎯 Bắn trúng: {duck.gameObject.name}");

            // Hiệu ứng bắn trúng vịt
            StartCoroutine(DuckHitAnimation(duck));

            if (duck.isBomb)
            {
                // Vịt bom: trừ 10 điểm, rung giật màn hình, nhấp nháy đỏ và hiệu ứng trừ điểm bay lên
                PhysicsGame1Session.Instance.HitBombDuck();
                UpdateHUD();

                ShowFloatingScore(-10, duck.rectTransform.anchoredPosition);
                TriggerScoreBump(false);
                StartCoroutine(ScreenShakeFeedback());
                StartCoroutine(FlashRedScreen());
                return;
            }

            // Vịt thường hoặc Vịt xanh: mở popup câu hỏi
            _isCurrentBlueDuck = duck.isBlue;
            OpenQuestionPopup();
        }

        private IEnumerator DuckHitAnimation(DuckItem duck)
        {
            Vector3 targetScale = duck.defaultScale;
            if (!duck.movingRight) targetScale.x = -Mathf.Abs(targetScale.x);
            else targetScale.x = Mathf.Abs(targetScale.x);

            duck.rectTransform.localScale = targetScale * 1.35f;
            yield return new WaitForSeconds(0.15f);
            duck.rectTransform.localScale = Vector3.zero;

            yield return new WaitForSeconds(3.5f); // Sau 3.5s vịt bơi lại
            duck.rectTransform.localScale = targetScale;
            duck.isHit = false;
        }

        private IEnumerator ScreenShakeFeedback()
        {
            if (_canvas != null)
            {
                Vector3 origin = _canvas.transform.position;
                for (int i = 0; i < 7; i++)
                {
                    _canvas.transform.position = origin + new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-8f, 8f), 0);
                    yield return new WaitForSeconds(0.035f);
                }
                _canvas.transform.position = origin;
            }
        }
        #endregion

        #region ===== 5. POPUP TRẮC NGHIỆM =====
        private void SetupMultipleChoicePopup()
        {
            _popupTracNghiem = GameObject.Find("panelcauhoitracno");
            if (_popupTracNghiem == null) return;

            // Đề bài: căn giữa với lề thoáng, font chữ đậm rõ nét
            GameObject debaiObj = _popupTracNghiem.transform.Find("debai")?.gameObject
                               ?? _popupTracNghiem.transform.Find("ghidebai")?.gameObject;
            if (debaiObj != null)
            {
                _tmpDebaiTN = debaiObj.GetComponentInChildren<TextMeshProUGUI>(true);
                UIHelper.FitTextInContainer(_tmpDebaiTN, 70f, 70f, 60f, 30f, 15f, 24f, TextAlignmentOptions.Center, new Color(0.18f, 0.12f, 0.05f), true);
            }

            // 4 Đáp án: lề trái 148px thoát hoàn toàn huy hiệu tròn A/B/C/D, lề phải 36px tránh bo cong mép, lề trên 30px né gờ gỗ, lề dưới 22px
            string[] ansBtnNames = new string[] { "dapana", "dapanb", "dapanc", "dapand" };
            for (int i = 0; i < 4; i++)
            {
                int choiceIdx = i;
                Transform ansT = _popupTracNghiem.transform.Find(ansBtnNames[i]);
                if (ansT != null)
                {
                    _btnAnswers[i] = UIHelper.GetOrAddButton(ansT.gameObject, () => SelectMultipleChoiceOption(choiceIdx));
                    _tmpAnswers[i] = ansT.GetComponentInChildren<TextMeshProUGUI>(true);
                    UIHelper.FitTextInContainer(_tmpAnswers[i], 148f, 36f, 30f, 22f, 10f, 20f, TextAlignmentOptions.MidlineLeft, new Color(0.14f, 0.09f, 0.04f), true);
                }
            }

            // Nút Nộp bài
            Transform nopbaiT = _popupTracNghiem.transform.Find("nopbai");
            if (nopbaiT != null)
            {
                _btnNopbaiTN = UIHelper.GetOrAddButton(nopbaiT.gameObject, SubmitMultipleChoice);
            }

            // Tạo Banner phản hồi Đúng / Sai chuyên nghiệp, nổi bật trên Popup
            GameObject bannerObj = new GameObject("ResultFeedbackBanner");
            bannerObj.transform.SetParent(_popupTracNghiem.transform, false);
            RectTransform brt = bannerObj.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 0.5f);
            brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.anchoredPosition = new Vector2(25.25f, -424f);
            brt.sizeDelta = new Vector2(520f, 95f);

            _feedbackBannerBg = bannerObj.AddComponent<Image>();
            _feedbackBannerBg.color = new Color(0.08f, 0.42f, 0.18f, 0.95f);
            _feedbackBannerBg.raycastTarget = false;

            // Dòng tiêu đề kết quả
            GameObject titleObj = new GameObject("ResultTitle");
            titleObj.transform.SetParent(bannerObj.transform, false);
            RectTransform trt = titleObj.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0f, 0.45f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.offsetMin = new Vector2(10f, 0f);
            trt.offsetMax = new Vector2(-10f, -4f);
            _tmpResultTitle = titleObj.AddComponent<TextMeshProUGUI>();
            _tmpResultTitle.fontSize = 28f;
            _tmpResultTitle.fontStyle = FontStyles.Bold;
            _tmpResultTitle.alignment = TextAlignmentOptions.Center;
            _tmpResultTitle.raycastTarget = false;

            // Dòng phụ chú điểm số / đáp án đúng
            GameObject subObj = new GameObject("ResultSubtitle");
            subObj.transform.SetParent(bannerObj.transform, false);
            RectTransform srt = subObj.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0f, 0f);
            srt.anchorMax = new Vector2(1f, 0.45f);
            srt.offsetMin = new Vector2(10f, 4f);
            srt.offsetMax = new Vector2(-10f, 0f);
            _tmpResultSubtitle = subObj.AddComponent<TextMeshProUGUI>();
            _tmpResultSubtitle.fontSize = 20f;
            _tmpResultSubtitle.fontStyle = FontStyles.Bold;
            _tmpResultSubtitle.alignment = TextAlignmentOptions.Center;
            _tmpResultSubtitle.raycastTarget = false;

            _feedbackBanner = bannerObj;
            _feedbackBanner.SetActive(false);

            _popupTracNghiem.SetActive(false);
        }

        private void SelectMultipleChoiceOption(int idx)
        {
            _selectedChoiceIndex = idx;
            for (int i = 0; i < 4; i++)
            {
                if (_btnAnswers[i] != null)
                {
                    Image img = _btnAnswers[i].GetComponent<Image>();
                    RectTransform art = _btnAnswers[i].GetComponent<RectTransform>();
                    if (img != null)
                    {
                        if (i == idx)
                        {
                            // Nút được chọn: viền vàng ấm áp, hiệu ứng nảy nhẹ
                            img.color = new Color(1f, 0.94f, 0.68f, 1f);
                            StartCoroutine(AnimateButtonPunch(art));
                        }
                        else
                        {
                            img.color = Color.white;
                            if (art != null) art.localScale = Vector3.one;
                        }
                    }
                }
            }
        }

        private void SubmitMultipleChoice()
        {
            if (_selectedChoiceIndex < 0)
            {
                Debug.Log("[PhysicsGame1] ⚠️ Vui lòng chọn một đáp án trước khi nộp!");
                return;
            }

            if (_currentQuestionIndex >= PhysicsGame1Session.Instance.CurrentQuestions.Count) return;
            QuestionData q = PhysicsGame1Session.Instance.CurrentQuestions[_currentQuestionIndex];

            bool isCorrect = (_selectedChoiceIndex == q.correctAnswerIndex);

            // Khóa tương tác
            for (int i = 0; i < 4; i++)
            {
                if (_btnAnswers[i] != null) _btnAnswers[i].interactable = false;
            }
            if (_btnNopbaiTN != null)
            {
                _btnNopbaiTN.interactable = false;
                _btnNopbaiTN.gameObject.SetActive(false);
            }

            // Ghi nhận vào Session
            PhysicsGame1Session.Instance.RecordMultipleChoiceAnswer(_currentQuestionIndex, _selectedChoiceIndex, _isCurrentBlueDuck);

            if (isCorrect)
            {
                // 1. Đáp án đúng: nút nảy sóng xanh lá rực rỡ
                if (_btnAnswers[q.correctAnswerIndex] != null)
                {
                    Image img = _btnAnswers[q.correctAnswerIndex].GetComponent<Image>();
                    if (img != null) img.color = new Color(0.25f, 0.88f, 0.42f);
                    StartCoroutine(AnimateButtonBounce(_btnAnswers[q.correctAnswerIndex].GetComponent<RectTransform>()));
                }

                // 2. Hiện Banner chúc mừng
                if (_feedbackBanner != null)
                {
                    _feedbackBanner.SetActive(true);
                    if (_feedbackBannerBg != null) _feedbackBannerBg.color = new Color(0.08f, 0.45f, 0.18f, 0.95f);
                    if (_tmpResultTitle != null)
                    {
                        _tmpResultTitle.text = "CHÍNH XÁC!";
                        _tmpResultTitle.color = new Color(1f, 0.88f, 0.2f); // Màu vàng hoàng kim
                    }
                    if (_tmpResultSubtitle != null)
                    {
                        _tmpResultSubtitle.text = _isCurrentBlueDuck ? "+10 ĐIỂM  •  ĐÓNG BĂNG ĐỒNG HỒ 10 GIÂY! ❄️" : "+10 ĐIỂM!";
                        _tmpResultSubtitle.color = new Color(0.65f, 1f, 0.35f);
                    }
                    StartCoroutine(AnimateBannerEntrance(_feedbackBanner.GetComponent<RectTransform>()));
                }

                // 3. Hiệu ứng cộng điểm bay lên và nảy điểm trên HUD
                ShowFloatingScore(10, new Vector2(0f, -50f), _isCurrentBlueDuck ? "ĐÓNG BĂNG 10S" : null);
                TriggerScoreBump(true);
            }
            else
            {
                // 1. Nút chọn sai: đổi màu đỏ và rung lắc cảnh báo
                if (_selectedChoiceIndex >= 0 && _selectedChoiceIndex < 4 && _btnAnswers[_selectedChoiceIndex] != null)
                {
                    Image img = _btnAnswers[_selectedChoiceIndex].GetComponent<Image>();
                    if (img != null) img.color = new Color(0.95f, 0.30f, 0.30f);
                    StartCoroutine(AnimateButtonShake(_btnAnswers[_selectedChoiceIndex].GetComponent<RectTransform>()));
                }

                // 2. Nút đáp án đúng: nổi bật xanh ngọc để học sinh nhận diện
                if (_btnAnswers[q.correctAnswerIndex] != null)
                {
                    Image img = _btnAnswers[q.correctAnswerIndex].GetComponent<Image>();
                    if (img != null) img.color = new Color(0.35f, 0.85f, 0.45f);
                }

                // 3. Hiện Banner thông báo sai và chỉ ra đáp án đúng
                if (_feedbackBanner != null)
                {
                    _feedbackBanner.SetActive(true);
                    if (_feedbackBannerBg != null) _feedbackBannerBg.color = new Color(0.50f, 0.08f, 0.08f, 0.95f);
                    if (_tmpResultTitle != null)
                    {
                        _tmpResultTitle.text = "CHƯA CHÍNH XÁC!";
                        _tmpResultTitle.color = Color.white;
                    }
                    if (_tmpResultSubtitle != null)
                    {
                        char correctChar = (char)('A' + q.correctAnswerIndex);
                        _tmpResultSubtitle.text = $"Đáp án đúng là: {correctChar}";
                        _tmpResultSubtitle.color = new Color(1f, 0.95f, 0.75f);
                    }
                    StartCoroutine(AnimateBannerEntrance(_feedbackBanner.GetComponent<RectTransform>()));
                }

                TriggerScoreBump(false);
            }

            _currentQuestionIndex++;
            UpdateHUD();
            StartCoroutine(ClosePopupAndResume(1.8f));
        }
        #endregion

        #region ===== 6. POPUP TỰ LUẬN =====
        private void SetupEssayPopup()
        {
            _popupTuLuan = GameObject.Find("panelcauhoituluan");
            if (_popupTuLuan == null) return;

            // Đề bài tự luận: căn giữa thoáng mát, tránh biển gỗ ĐỀ BÀI ở trên
            GameObject debaiObj = _popupTuLuan.transform.Find("debai")?.gameObject
                               ?? _popupTuLuan.transform.Find("ghidebai")?.gameObject;
            if (debaiObj != null)
            {
                _tmpDebaiTL = debaiObj.GetComponentInChildren<TextMeshProUGUI>(true);
                UIHelper.FitTextInContainer(_tmpDebaiTL, 70f, 70f, 55f, 35f, 16f, 26f, TextAlignmentOptions.Center, new Color(0.15f, 0.10f, 0.05f), true);
            }

            // Ô nhập đáp án tự luận: Khung sen vàng rộng lớn, lề trái 145px (thoát hoa sen), lề trên 110px (dưới viền gỗ)
            Transform nhapT = _popupTuLuan.transform.Find("nhapdapan");
            if (nhapT != null)
            {
                _inputFieldTL = nhapT.GetComponent<TMP_InputField>();
                if (_inputFieldTL == null)
                {
                    _inputFieldTL = nhapT.gameObject.AddComponent<TMP_InputField>();
                }
                TextMeshProUGUI txt = nhapT.GetComponentInChildren<TextMeshProUGUI>(true);
                if (txt != null)
                {
                    _inputFieldTL.textComponent = txt;
                    UIHelper.FitTextInContainer(txt, 145f, 135f, 110f, 95f, 15f, 22f, TextAlignmentOptions.TopLeft, new Color(0.12f, 0.08f, 0.04f));
                    txt.lineSpacing = 4f;
                }
                _inputFieldTL.lineType = TMP_InputField.LineType.MultiLineNewline;
            }

            // Nút tải ảnh lên: căn chỉnh lề vừa khít trong khung nút
            Transform upT = _popupTuLuan.transform.Find("taianhlen");
            if (upT != null)
            {
                _btnUploadImg = UIHelper.GetOrAddButton(upT.gameObject, OnUploadImageClicked);
                TextMeshProUGUI upTxt = upT.GetComponentInChildren<TextMeshProUGUI>(true);
                if (upTxt != null) UIHelper.FitTextInContainer(upTxt, 20f, 20f, 12f, 12f, 14f, 20f, TextAlignmentOptions.Center, new Color(0.18f, 0.12f, 0.05f), true);
            }

            // Nút xóa ảnh: căn chỉnh chữ nằm gọn trong khung
            Transform delT = _popupTuLuan.transform.Find("xoaanh");
            if (delT != null)
            {
                _btnDeleteImg = UIHelper.GetOrAddButton(delT.gameObject, OnDeleteImageClicked);
                TextMeshProUGUI delTxt = delT.GetComponentInChildren<TextMeshProUGUI>(true);
                if (delTxt != null) UIHelper.FitTextInContainer(delTxt, 15f, 15f, 10f, 10f, 13f, 18f, TextAlignmentOptions.Center, new Color(0.18f, 0.12f, 0.05f), true);
            }

            // Nút nộp bài: căn chỉnh chữ nổi bật, vừa vặn nút gỗ
            Transform nopbaiT = _popupTuLuan.transform.Find("nopbai");
            if (nopbaiT != null)
            {
                _btnNopbaiTL = UIHelper.GetOrAddButton(nopbaiT.gameObject, SubmitEssay);
                TextMeshProUGUI nopTxt = nopbaiT.GetComponentInChildren<TextMeshProUGUI>(true);
                if (nopTxt != null) UIHelper.FitTextInContainer(nopTxt, 25f, 25f, 15f, 15f, 16f, 24f, TextAlignmentOptions.Center, Color.white, true);
            }

            _popupTuLuan.SetActive(false);
        }

        private void OnUploadImageClicked()
        {
            if (_btnUploadImg != null)
            {
                UIHelper.SetTMPText(_btnUploadImg.gameObject, "⏳ ĐANG CHỌN...");
            }

            PRU.Biology.ImagePickerManager.EnsureInstance().PickImage(false, (success, base64, sprite) =>
            {
                if (!success || string.IsNullOrEmpty(base64))
                {
                    Debug.Log("[PhysicsGame1] 📷 Hủy chọn ảnh hoặc file ảnh không hợp lệ");
                    if (_btnUploadImg != null)
                    {
                        UIHelper.SetTMPText(_btnUploadImg.gameObject, _hasAttachedImage ? "✅ ĐÃ CÓ ẢNH" : "TẢI ẢNH LÊN");
                    }
                    return;
                }

                _hasAttachedImage = true;
                _attachedImageBase64 = base64;
                Debug.Log($"[PhysicsGame1] 📷 Đã chọn ảnh thành công (base64 length: {base64.Length}). Gửi sang AI OCR nhận diện chữ viết tay...");

                if (_btnUploadImg != null)
                {
                    UIHelper.SetTMPText(_btnUploadImg.gameObject, "⏳ AI ĐỌC CHỮ...");
                }

                string qId = (_currentQuestionIndex < PhysicsGame1Session.Instance.CurrentQuestions.Count)
                    ? PhysicsGame1Session.Instance.CurrentQuestions[_currentQuestionIndex].questionID
                    : $"Q_{_currentQuestionIndex + 1}";

                AIGradingClient.EnsureInstance().TranscribeHandwriting(qId, base64,
                    (transcribedText) =>
                    {
                        Debug.Log($"[PhysicsGame1] ✅ AI OCR chữ viết tay thành công:\n{transcribedText}");
                        if (_inputFieldTL != null)
                        {
                            _inputFieldTL.text = transcribedText;
                        }
                        if (_btnUploadImg != null)
                        {
                            UIHelper.SetTMPText(_btnUploadImg.gameObject, "✅ ĐÃ CHÉP CHỮ");
                        }
                    },
                    (err) =>
                    {
                        Debug.LogWarning($"[PhysicsGame1] ⚠️ Lỗi OCR: {err}");
                        if (_btnUploadImg != null)
                        {
                            UIHelper.SetTMPText(_btnUploadImg.gameObject, "✅ ĐÃ CÓ ẢNH");
                        }
                    });
            });
        }

        private void OnDeleteImageClicked()
        {
            if (_essayState == EssayState.Failed)
            {
                // Khi Chưa Đạt và nhấn "BỎ QUA ⏩", bỏ qua câu này với 0 điểm và đi tiếp
                ProceedToNextAfterEssay(false);
                return;
            }

            _hasAttachedImage = false;
            _attachedImageBase64 = "";
            if (_inputFieldTL != null) _inputFieldTL.text = "";
            Debug.Log("[PhysicsGame1] 🗑️ Đã xóa ảnh đính kèm và nội dung câu trả lời");
            if (_btnUploadImg != null)
            {
                UIHelper.SetTMPText(_btnUploadImg.gameObject, "TẢI ẢNH LÊN");
            }
        }

        private void SubmitEssay()
        {
            if (_essayState == EssayState.Passed)
            {
                // Học sinh đã ĐẠT và nhấn "ĐI TIẾP ➔"
                ProceedToNextAfterEssay(true);
                return;
            }

            if (_essayState == EssayState.Failed)
            {
                // Học sinh CHƯA ĐẠT và nhấn "SỬA BÀI ↺"
                ResumeEssayEditing();
                return;
            }

            string answer = _inputFieldTL != null ? _inputFieldTL.text : "";
            if (string.IsNullOrWhiteSpace(answer) && !_hasAttachedImage)
            {
                Debug.Log("[PhysicsGame1] ⚠️ Vui lòng nhập lời giải hoặc tải ảnh bài làm!");
                return;
            }

            _lastStudentEssayDraft = answer;
            _essayState = EssayState.Grading;

            if (_inputFieldTL != null) _inputFieldTL.readOnly = true;
            if (_btnUploadImg != null) _btnUploadImg.interactable = false;
            if (_btnDeleteImg != null) _btnDeleteImg.interactable = false;

            if (_btnNopbaiTL != null)
            {
                _btnNopbaiTL.interactable = false;
                UIHelper.SetTMPText(_btnNopbaiTL.gameObject, "⏳ AI ĐANG CHẤM BÀI...");
            }

            string qId = (_currentQuestionIndex < PhysicsGame1Session.Instance.CurrentQuestions.Count)
                ? PhysicsGame1Session.Instance.CurrentQuestions[_currentQuestionIndex].questionID
                : $"Q_{_currentQuestionIndex + 1}";

            AIGradingClient.EnsureInstance().SubmitEssay(qId, answer, _attachedImageBase64,
                (AIGradingResponse resp) =>
                {
                    HandleEssayGradingResult(resp, answer);
                },
                (string error) =>
                {
                    Debug.LogWarning($"[PhysicsGame1] ⚠️ Lỗi kết nối AI: {error}. Sử dụng đánh giá cục bộ.");
                    HandleEssayGradingFallback(answer);
                });
        }

        private void ResumeEssayEditing()
        {
            _essayState = EssayState.Editing;
            if (_inputFieldTL != null)
            {
                _inputFieldTL.readOnly = false;
                _inputFieldTL.text = _lastStudentEssayDraft;
            }

            if (_currentQuestionIndex < PhysicsGame1Session.Instance.CurrentQuestions.Count)
            {
                var q = PhysicsGame1Session.Instance.CurrentQuestions[_currentQuestionIndex];
                if (_tmpDebaiTL != null) _tmpDebaiTL.text = $"CÂU {_currentQuestionIndex + 1}: {q.questionText}";
            }

            if (_btnNopbaiTL != null)
            {
                _btnNopbaiTL.interactable = true;
                UIHelper.SetTMPText(_btnNopbaiTL.gameObject, "NỘP BÀI");
            }

            if (_btnUploadImg != null)
            {
                _btnUploadImg.interactable = true;
                UIHelper.SetTMPText(_btnUploadImg.gameObject, _hasAttachedImage ? "✅ ĐÃ CÓ ẢNH" : "TẢI ẢNH LÊN");
            }

            if (_btnDeleteImg != null)
            {
                _btnDeleteImg.interactable = true;
                UIHelper.SetTMPText(_btnDeleteImg.gameObject, "XÓA ẢNH");
            }
        }

        private void ProceedToNextAfterEssay(bool passed)
        {
            float score = _lastGradingResp != null ? _lastGradingResp.score : 0f;
            int earnedScore = passed ? Mathf.Clamp(Mathf.RoundToInt(score), 5, 10) : 0;

            PhysicsGame1Session.Instance.RecordEssayResult(_currentQuestionIndex, _lastStudentEssayDraft, passed, earnedScore, _lastGradingResp);

            if (_feedbackBanner != null)
            {
                _feedbackBanner.SetActive(true);
                if (passed)
                {
                    if (_feedbackBannerBg != null) _feedbackBannerBg.color = new Color(0.08f, 0.45f, 0.18f, 0.95f);
                    if (_tmpResultTitle != null)
                    {
                        _tmpResultTitle.text = $"AI ĐÃ CHẤM: {earnedScore}/10 ĐIỂM";
                        _tmpResultTitle.color = new Color(1f, 0.88f, 0.2f);
                    }
                    if (_tmpResultSubtitle != null)
                    {
                        string sub = (_lastGradingResp != null && !string.IsNullOrEmpty(_lastGradingResp.feedback)) ? _lastGradingResp.feedback : "Lập luận chính xác, đạt yêu cầu bài học!";
                        if (_isCurrentBlueDuck) sub += "  •  ❄️ ĐÓNG BĂNG 10S!";
                        _tmpResultSubtitle.text = sub;
                        _tmpResultSubtitle.color = new Color(0.65f, 1f, 0.35f);
                    }

                    ShowFloatingScore(earnedScore, Vector2.zero, _isCurrentBlueDuck ? "ĐÓNG BĂNG 10S" : null);
                    TriggerScoreBump(true);
                }
                else
                {
                    if (_feedbackBannerBg != null) _feedbackBannerBg.color = new Color(0.55f, 0.08f, 0.08f, 0.95f);
                    if (_tmpResultTitle != null)
                    {
                        _tmpResultTitle.text = "CHƯA ĐẠT (0 ĐIỂM)";
                        _tmpResultTitle.color = Color.white;
                    }
                    if (_tmpResultSubtitle != null)
                    {
                        string reason = (_lastGradingResp != null && _lastGradingResp.errorsIdentified != null && _lastGradingResp.errorsIdentified.Count > 0)
                            ? _lastGradingResp.errorsIdentified[0]
                            : (_lastGradingResp != null && !string.IsNullOrEmpty(_lastGradingResp.feedback) ? _lastGradingResp.feedback : "Bài làm chưa đạt yêu cầu!");
                        _tmpResultSubtitle.text = reason;
                        _tmpResultSubtitle.color = new Color(1f, 0.85f, 0.75f);
                    }

                    TriggerScoreBump(false);
                }
                StartCoroutine(AnimateBannerEntrance(_feedbackBanner.GetComponent<RectTransform>()));
            }

            _currentQuestionIndex++;
            UpdateHUD();

            _essayState = EssayState.Editing;
            _hasAttachedImage = false;
            _attachedImageBase64 = "";
            if (_popupTuLuan != null) _popupTuLuan.SetActive(false);
            _isPopupOpen = false;
            Time.timeScale = 1f;

            if (_currentQuestionIndex >= PhysicsGame1Session.Instance.CurrentQuestions.Count)
            {
                EndGameSession();
            }
        }

        private void HandleEssayGradingResult(AIGradingResponse resp, string answer)
        {
            _lastGradingResp = resp;
            float score = resp != null ? resp.score : 0f;
            float maxScore = (resp != null && resp.maxScore > 0f) ? resp.maxScore : 10f;
            float percentage = (resp != null && resp.percentage > 0f) ? resp.percentage : (score / maxScore * 100f);
            bool isAccepted = (score >= 5f && percentage >= 50f && (resp == null || resp.readable));

            _essayState = isAccepted ? EssayState.Passed : EssayState.Failed;

            // Xây dựng kết quả phản hồi chi tiết chuẩn xác theo mô hình nhánh anh-dev
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"📊 KẾT QUẢ ĐÁNH GIÁ: {score:0.#}/{maxScore:0.#} ĐIỂM ({percentage:0}%)");
            sb.AppendLine(isAccepted ? "🎉 ĐẠT — Bài làm tốt và đáp ứng tiêu chí!" : "⚠️ CHƯA ĐẠT — Bạn có thể xem nhận xét bên dưới và sửa lại bài.");
            sb.AppendLine();

            if (resp != null && resp.correctPoints != null && resp.correctPoints.Count > 0)
            {
                sb.AppendLine("✅ Các điểm làm tốt:");
                foreach (var pt in resp.correctPoints)
                {
                    if (!string.IsNullOrWhiteSpace(pt)) sb.AppendLine("• " + pt);
                }
                sb.AppendLine();
            }

            if (resp != null && resp.missingPoints != null && resp.missingPoints.Count > 0)
            {
                sb.AppendLine("❌ Cần sửa / bổ sung:");
                foreach (var pt in resp.missingPoints)
                {
                    if (!string.IsNullOrWhiteSpace(pt)) sb.AppendLine("• " + pt);
                }
                sb.AppendLine();
            }

            if (resp != null && !string.IsNullOrWhiteSpace(resp.feedback))
            {
                sb.AppendLine("📝 Nhận xét của AI:");
                sb.AppendLine(resp.feedback);
                sb.AppendLine();
            }

            if (resp != null && !string.IsNullOrWhiteSpace(resp.suggestedAnswer))
            {
                sb.AppendLine("💡 Gợi ý giải thích / Lời giải mẫu:");
                sb.AppendLine(resp.suggestedAnswer);
            }

            // Hiển thị trực tiếp vào ô nội dung như MazeEssayController
            if (_inputFieldTL != null)
            {
                _inputFieldTL.readOnly = true;
                _inputFieldTL.text = sb.ToString();
            }

            if (_tmpDebaiTL != null)
            {
                _tmpDebaiTL.text = isAccepted ? "🎉 BẠN ĐÃ VƯỢT QUA CÂU TỰ LUẬN NÀY!" : "⚠️ HÃY ĐỌC GÓP Ý VÀ BẤM 'SỬA BÀI' ĐỂ LÀM LẠI";
            }

            if (_btnNopbaiTL != null)
            {
                _btnNopbaiTL.interactable = true;
                UIHelper.SetTMPText(_btnNopbaiTL.gameObject, isAccepted ? "ĐI TIẾP ➔" : "SỬA BÀI ↺");
            }

            // Nếu chưa đạt, cho phép nút thứ 2 làm nút "BỎ QUA ⏩"
            if (!isAccepted && _btnDeleteImg != null)
            {
                _btnDeleteImg.interactable = true;
                UIHelper.SetTMPText(_btnDeleteImg.gameObject, "BỎ QUA ⏩");
            }
        }

        private void HandleEssayGradingFallback(string answer)
        {
            // Kiểm tra nội dung viết linh tinh cục bộ
            bool isNonsense = string.IsNullOrWhiteSpace(answer) || answer.Trim().Length < 12;
            if (!isNonsense)
            {
                string lower = answer.ToLower();
                string[] badWords = new string[] { "linh tinh", "asdf", "qwer", "12345", "test" };
                foreach (var b in badWords)
                {
                    if (lower.Contains(b)) { isNonsense = true; break; }
                }
            }

            AIGradingResponse fallbackResp = new AIGradingResponse
            {
                score = isNonsense ? 0f : 8f,
                maxScore = 10f,
                percentage = isNonsense ? 0f : 80f,
                correctPoints = isNonsense ? new List<string>() : new List<string> { "Đã nêu đúng khái niệm và công thức cơ bản." },
                missingPoints = isNonsense ? new List<string> { "Chưa trả lời đúng câu hỏi đề bài." } : new List<string> { "Cần diễn giải chi tiết hơn đơn vị đo lường." },
                feedback = isNonsense ? "Bài làm viết linh tinh hoặc không liên quan đến kiến thức đề bài." : "Bài giải có ý đúng theo kiến thức cơ bản.",
                suggestedAnswer = "Cần áp dụng chính xác công thức và nêu rõ đơn vị trong hệ SI."
            };

            HandleEssayGradingResult(fallbackResp, answer);
        }
        #endregion

        #region ===== 7. MỞ VÀ ĐÓNG POPUP CÂU HỎI =====
        private void OpenQuestionPopup()
        {
            if (_currentQuestionIndex >= PhysicsGame1Session.Instance.CurrentQuestions.Count)
            {
                EndGameSession();
                return;
            }

            _isPopupOpen = true;
            QuestionData q = PhysicsGame1Session.Instance.CurrentQuestions[_currentQuestionIndex];

            string mode = PhysicsGame1Session.Instance.SelectedGameMode;
            if (mode == "TuLuan" && _popupTuLuan != null)
            {
                _essayState = EssayState.Editing;
                _lastStudentEssayDraft = "";
                _hasAttachedImage = false;
                _attachedImageBase64 = "";

                // Mở popup Tự luận
                _popupTuLuan.SetActive(true);
                if (_feedbackBanner != null) _feedbackBanner.SetActive(false);
                if (_tmpDebaiTL != null) _tmpDebaiTL.text = $"CÂU {_currentQuestionIndex + 1}: {q.questionText}";
                if (_inputFieldTL != null)
                {
                    _inputFieldTL.readOnly = false;
                    _inputFieldTL.text = "";
                }
                if (_btnNopbaiTL != null)
                {
                    _btnNopbaiTL.interactable = true;
                    UIHelper.SetTMPText(_btnNopbaiTL.gameObject, "NỘP BÀI");
                }
                if (_btnUploadImg != null)
                {
                    _btnUploadImg.interactable = true;
                    UIHelper.SetTMPText(_btnUploadImg.gameObject, "TẢI ẢNH LÊN");
                }
                if (_btnDeleteImg != null)
                {
                    _btnDeleteImg.interactable = true;
                    UIHelper.SetTMPText(_btnDeleteImg.gameObject, "XÓA ẢNH");
                }
            }
            else if (_popupTracNghiem != null)
            {
                // Mở popup Trắc nghiệm
                _popupTracNghiem.SetActive(true);
                if (_tmpDebaiTN != null) _tmpDebaiTN.text = $"CÂU {_currentQuestionIndex + 1}: {q.questionText}";

                _selectedChoiceIndex = -1;
                if (_feedbackBanner != null) _feedbackBanner.SetActive(false);

                for (int i = 0; i < 4; i++)
                {
                    if (_btnAnswers[i] != null)
                    {
                        _btnAnswers[i].interactable = true;
                        _btnAnswers[i].transform.localScale = Vector3.one;
                        Image img = _btnAnswers[i].GetComponent<Image>();
                        if (img != null) img.color = Color.white;
                    }

                    if (_tmpAnswers[i] != null)
                    {
                        string ansStr = (i < q.answers.Length) ? q.answers[i] : "";
                        _tmpAnswers[i].text = UIHelper.CleanAnswerText(ansStr);
                    }
                }

                if (_btnNopbaiTN != null)
                {
                    _btnNopbaiTN.gameObject.SetActive(true);
                    _btnNopbaiTN.interactable = true;
                }
            }
        }

        private IEnumerator ClosePopupAndResume(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (_popupTracNghiem != null) _popupTracNghiem.SetActive(false);
            if (_popupTuLuan != null) _popupTuLuan.SetActive(false);
            _isPopupOpen = false;

            // Kiểm tra kết thúc nếu đã xong câu 10
            if (PhysicsGame1Session.Instance.AnsweredCount >= PhysicsGame1Session.MaxQuestionsPerSession)
            {
                EndGameSession();
            }
        }
        #endregion

        #region ===== 8. POPUP PAUSE =====
        private void SetupPausePopup()
        {
            _popupPause = GameObject.Find("panelpause");
            if (_popupPause == null) return;

            Transform tieptucT = _popupPause.transform.Find("tieptuc");
            if (tieptucT != null) UIHelper.GetOrAddButton(tieptucT.gameObject, OnResumeClicked);

            Transform choilaiT = _popupPause.transform.Find("choilai");
            if (choilaiT != null) UIHelper.GetOrAddButton(choilaiT.gameObject, OnRestartClicked);

            Transform vemenuT = _popupPause.transform.Find("vemenu");
            if (vemenuT != null) UIHelper.GetOrAddButton(vemenuT.gameObject, OnBackToMenuClicked);

            // Công tắc Nhạc nền & Âm thanh
            _gatnutBGM = _popupPause.transform.Find("gatnut")?.gameObject;
            _nutxamBGM = _popupPause.transform.Find("nutxam")?.gameObject;
            _gatnutSFX = _popupPause.transform.Find("gatnut (1)")?.gameObject;
            _nutxamSFX = _popupPause.transform.Find("nutxam (1)")?.gameObject;

            UnityEngine.Events.UnityAction toggleBGM = ToggleBGM;
            if (_gatnutBGM != null) UIHelper.GetOrAddButton(_gatnutBGM, toggleBGM);
            if (_nutxamBGM != null) UIHelper.GetOrAddButton(_nutxamBGM, toggleBGM);
            Transform nhacT = _popupPause.transform.Find("nhacnen");
            if (nhacT != null) UIHelper.GetOrAddButton(nhacT.gameObject, toggleBGM);

            UnityEngine.Events.UnityAction toggleSFX = ToggleSFX;
            if (_gatnutSFX != null) UIHelper.GetOrAddButton(_gatnutSFX, toggleSFX);
            if (_nutxamSFX != null) UIHelper.GetOrAddButton(_nutxamSFX, toggleSFX);
            Transform amthanhT = _popupPause.transform.Find("amthanh");
            if (amthanhT != null) UIHelper.GetOrAddButton(amthanhT.gameObject, toggleSFX);

            UpdatePauseAudioVisuals();
            _popupPause.SetActive(false);
        }

        private void OnPauseClicked()
        {
            if (_isGameOver) return;
            _isPaused = true;
            if (_popupPause != null) _popupPause.SetActive(true);
            UpdatePauseAudioVisuals();
        }

        private void OnResumeClicked()
        {
            _isPaused = false;
            if (_popupPause != null) _popupPause.SetActive(false);
        }

        private void OnRestartClicked()
        {
            PhysicsGame1Session.Instance.StartNewGameSession();
            SceneManager.LoadScene("07_DuckShooting");
        }

        private void OnBackToMenuClicked()
        {
            SceneManager.LoadScene("01_MainMenu");
        }

        private void ToggleBGM()
        {
            PhysicsGame1Session.Instance.BGMEnabled = !PhysicsGame1Session.Instance.BGMEnabled;
            PlayerPrefs.SetInt("PhysicsGame1_BGM", PhysicsGame1Session.Instance.BGMEnabled ? 1 : 0);
            PlayerPrefs.Save();
            UpdatePauseAudioVisuals();
        }

        private void ToggleSFX()
        {
            PhysicsGame1Session.Instance.SFXEnabled = !PhysicsGame1Session.Instance.SFXEnabled;
            PlayerPrefs.SetInt("PhysicsGame1_SFX", PhysicsGame1Session.Instance.SFXEnabled ? 1 : 0);
            PlayerPrefs.Save();
            UpdatePauseAudioVisuals();
        }

        private void UpdatePauseAudioVisuals()
        {
            bool bgm = PhysicsGame1Session.Instance.BGMEnabled;
            if (_gatnutBGM != null) _gatnutBGM.SetActive(bgm);
            if (_nutxamBGM != null) _nutxamBGM.SetActive(!bgm);

            bool sfx = PhysicsGame1Session.Instance.SFXEnabled;
            if (_gatnutSFX != null) _gatnutSFX.SetActive(sfx);
            if (_nutxamSFX != null) _nutxamSFX.SetActive(!sfx);
        }
        #endregion

        #region ===== 9. UPDATE VÒNG LẶP GAMEPLAY =====
        private void Update()
        {
            if (_isGameOver || _isPaused) return;

            // Cập nhật súng và hồng tâm theo chuột
            UpdateAimAndGun();

            // Cập nhật vịt bơi trong hồ & xử lý bắn vịt (khi không mở popup câu hỏi)
            if (!_isPopupOpen)
            {
                UpdateDuckMovement();
                HandleShootingInput();
            }

            // Đếm ngược thời gian
            UpdateTimers();
        }

        private void HandleShootingInput()
        {
            if (!Input.GetMouseButtonDown(0)) return;
            if (_isGameOver || _isPaused || _isPopupOpen) return;

            Vector2 mouseScreenPos = Input.mousePosition;
            Camera cam = (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? _canvas.worldCamera : null;

            // Bỏ qua nếu bấm trúng nút Pause
            GameObject pauseObj = GameObject.Find("pause");
            if (pauseObj != null)
            {
                RectTransform pauseRt = pauseObj.GetComponent<RectTransform>();
                if (pauseRt != null && RectTransformUtility.RectangleContainsScreenPoint(pauseRt, mouseScreenPos, cam))
                {
                    return;
                }
            }

            // Tìm vịt bị trúng đạn gần tâm ngắm nhất
            DuckItem hitDuck = null;
            float closestDistSq = float.MaxValue;

            for (int i = 0; i < _ducks.Count; i++)
            {
                var duck = _ducks[i];
                if (duck == null || duck.isHit || duck.gameObject == null || !duck.gameObject.activeInHierarchy)
                    continue;

                RectTransform rt = duck.rectTransform;
                if (rt == null) continue;

                // 1. Kiểm tra RectTransform chứa tọa độ chuột
                bool isInside = RectTransformUtility.RectangleContainsScreenPoint(rt, mouseScreenPos, cam);

                // 2. Hitbox mở rộng (1.25x) hỗ trợ bắn mượt mà mục tiêu di chuyển
                Vector2 screenCenter = RectTransformUtility.WorldToScreenPoint(cam, rt.position);
                if (!isInside)
                {
                    float halfW = Mathf.Abs(rt.rect.width * rt.lossyScale.x) * 0.5f * 1.25f;
                    float halfH = Mathf.Abs(rt.rect.height * rt.lossyScale.y) * 0.5f * 1.25f;
                    if (Mathf.Abs(mouseScreenPos.x - screenCenter.x) <= halfW &&
                        Mathf.Abs(mouseScreenPos.y - screenCenter.y) <= halfH)
                    {
                        isInside = true;
                    }
                }

                if (isInside)
                {
                    float distSq = (mouseScreenPos - screenCenter).sqrMagnitude;
                    if (distSq < closestDistSq)
                    {
                        closestDistSq = distSq;
                        hitDuck = duck;
                    }
                }
            }

            if (hitDuck != null)
            {
                OnDuckClicked(hitDuck);
            }
        }

        private void UpdateAimAndGun()
        {
            Vector3 mousePos = Input.mousePosition;

            // Hồng tâm theo chuột
            if (_crosshair != null && _canvas != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvas.transform as RectTransform, mousePos, _canvas.worldCamera, out Vector2 localPoint);
                _crosshair.anchoredPosition = localPoint;
            }

            // Súng xoay theo chuột
            if (_gun != null)
            {
                Vector3 dir = mousePos - _gun.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                _gun.rotation = Quaternion.Euler(0, 0, angle - 90f);
            }
        }

        private void UpdateDuckMovement()
        {
            float dt = Time.deltaTime;
            float minX = -450f;
            float maxX = 450f;

            foreach (var d in _ducks)
            {
                if (d.isHit) continue;

                Vector2 pos = d.rectTransform.anchoredPosition;

                // Di chuyển ngang
                if (d.movingRight)
                {
                    pos.x += d.speed * dt;
                    if (pos.x >= maxX)
                    {
                        pos.x = maxX;
                        d.movingRight = false;
                        Vector3 s = d.rectTransform.localScale;
                        s.x = -Mathf.Abs(s.x); // Quay mặt sang trái
                        d.rectTransform.localScale = s;
                    }
                }
                else
                {
                    pos.x -= d.speed * dt;
                    if (pos.x <= minX)
                    {
                        pos.x = minX;
                        d.movingRight = true;
                        Vector3 s = d.rectTransform.localScale;
                        s.x = Mathf.Abs(s.x); // Quay mặt sang phải
                        d.rectTransform.localScale = s;
                    }
                }

                // Chuyển động bồng bềnh
                pos.y = d.basePos.y + Mathf.Sin(Time.time * 2.5f + d.bobOffset) * 12f;
                d.rectTransform.anchoredPosition = pos;
            }
        }

        private void UpdateTimers()
        {
            if (PhysicsGame1Session.Instance.FreezeTimeRemaining > 0f)
            {
                // Đang đóng băng đồng hồ
                PhysicsGame1Session.Instance.FreezeTimeRemaining -= Time.deltaTime;
                if (PhysicsGame1Session.Instance.FreezeTimeRemaining <= 0f)
                {
                    PhysicsGame1Session.Instance.FreezeTimeRemaining = 0f;
                }
            }
            else
            {
                // Đồng hồ đếm ngược bình thường
                PhysicsGame1Session.Instance.RemainingTime -= Time.deltaTime;
                if (PhysicsGame1Session.Instance.RemainingTime <= 0f)
                {
                    PhysicsGame1Session.Instance.RemainingTime = 0f;
                    EndGameSession();
                }
            }

            UpdateHUD();
        }

        private void EndGameSession()
        {
            if (_isGameOver) return;
            _isGameOver = true;
            Debug.Log($"[PhysicsGame1] 🏆 Hoàn thành lượt chơi! Tổng điểm: {PhysicsGame1Session.Instance.CurrentScore}");

            PhysicsGame1Session.Instance.SaveScoreRecord();
            SceneManager.LoadScene("08_ResultScene");
        }
        #endregion

        #region ===== 10. HIỆU ỨNG HÌNH ẢNH, ĐIỂM SỐ & NẢY NÚT =====
        private void ShowFloatingScore(int delta, Vector2 anchoredPos, string extraNote = null)
        {
            if (_canvas == null) return;

            GameObject floatObj = new GameObject("FloatingScorePopup");
            floatObj.transform.SetParent(_canvas.transform, false);

            RectTransform rt = floatObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(360f, 75f);

            CanvasGroup cg = floatObj.AddComponent<CanvasGroup>();

            Image bgImg = floatObj.AddComponent<Image>();
            bgImg.color = (delta >= 0)
                ? new Color(0.04f, 0.32f, 0.14f, 0.88f)
                : new Color(0.42f, 0.06f, 0.06f, 0.88f);
            bgImg.raycastTarget = false;

            GameObject textChild = new GameObject("Text");
            textChild.transform.SetParent(floatObj.transform, false);
            RectTransform trt = textChild.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textChild.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.fontSize = 30f;
            tmp.raycastTarget = false;

            if (delta > 0)
            {
                tmp.text = string.IsNullOrEmpty(extraNote) ? $"+{delta} ĐIỂM!" : $"+{delta} ĐIỂM!\n<size=18>{extraNote}</size>";
                tmp.color = new Color(0.35f, 1f, 0.45f);
            }
            else if (delta < 0)
            {
                tmp.text = $"{delta} ĐIỂM!";
                tmp.color = new Color(1f, 0.35f, 0.35f);
            }
            else
            {
                tmp.text = "0 ĐIỂM";
                tmp.color = new Color(0.9f, 0.9f, 0.9f);
            }

            StartCoroutine(AnimateFloatingScore(rt, cg, delta < 0));
        }

        private IEnumerator AnimateFloatingScore(RectTransform rt, CanvasGroup cg, bool isNegative)
        {
            float duration = 1.3f;
            float elapsed = 0f;
            Vector2 startPos = rt.anchoredPosition;
            Vector2 targetPos = startPos + new Vector2(0f, 85f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (t < 0.2f)
                {
                    float scaleT = t / 0.2f;
                    rt.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.25f, scaleT);
                }
                else if (t < 0.35f)
                {
                    float scaleT = (t - 0.2f) / 0.15f;
                    rt.localScale = Vector3.one * Mathf.Lerp(1.25f, 1.0f, scaleT);
                }
                else
                {
                    rt.localScale = Vector3.one;
                }

                float xOffset = isNegative ? Mathf.Sin(t * 35f) * 6f * (1f - t) : 0f;
                float easeOutY = 1f - Mathf.Pow(1f - t, 2.5f);
                rt.anchoredPosition = new Vector2(startPos.x + xOffset, Mathf.Lerp(startPos.y, targetPos.y, easeOutY));

                if (t > 0.65f)
                {
                    cg.alpha = 1f - ((t - 0.65f) / 0.35f);
                }

                yield return null;
            }

            if (rt != null && rt.gameObject != null)
            {
                Destroy(rt.gameObject);
            }
        }

        private void TriggerScoreBump(bool isPositive)
        {
            if (_scoreBumpCoroutine != null) StopCoroutine(_scoreBumpCoroutine);
            _scoreBumpCoroutine = StartCoroutine(AnimateScoreBump(isPositive));
        }

        private IEnumerator AnimateScoreBump(bool isPositive)
        {
            if (_scoreText == null) yield break;
            RectTransform srt = _scoreText.rectTransform;
            Vector3 origScale = Vector3.one;
            Color origColor = new Color(0.25f, 0.15f, 0.05f);
            Color flashColor = isPositive ? new Color(0.15f, 0.75f, 0.25f) : new Color(0.9f, 0.15f, 0.15f);

            float duration = 0.45f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.35f;
                srt.localScale = origScale * scale;
                _scoreText.color = Color.Lerp(flashColor, origColor, t);

                yield return null;
            }

            srt.localScale = origScale;
            _scoreText.color = origColor;
        }

        private IEnumerator FlashRedScreen()
        {
            if (_canvas == null) yield break;

            GameObject flashObj = new GameObject("ScreenFlashRed");
            flashObj.transform.SetParent(_canvas.transform, false);
            RectTransform frt = flashObj.AddComponent<RectTransform>();
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero;
            frt.offsetMax = Vector2.zero;

            Image fImg = flashObj.AddComponent<Image>();
            fImg.color = new Color(1f, 0.15f, 0.15f, 0.35f);
            fImg.raycastTarget = false;

            float elapsed = 0f;
            float dur = 0.3f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float a = Mathf.Lerp(0.35f, 0f, elapsed / dur);
                fImg.color = new Color(1f, 0.15f, 0.15f, a);
                yield return null;
            }

            Destroy(flashObj);
        }

        private IEnumerator AnimateButtonPunch(RectTransform rt)
        {
            if (rt == null) yield break;
            float dur = 0.18f;
            float elapsed = 0f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float s = 1f;
                if (t < 0.4f) s = Mathf.Lerp(1f, 0.94f, t / 0.4f);
                else s = Mathf.Lerp(0.94f, 1.04f, (t - 0.4f) / 0.6f);

                rt.localScale = Vector3.one * s;
                yield return null;
            }
            rt.localScale = Vector3.one * 1.03f;
        }

        private IEnumerator AnimateButtonBounce(RectTransform rt)
        {
            if (rt == null) yield break;
            float dur = 0.35f;
            float elapsed = 0f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float s = 1f + Mathf.Sin(t * Mathf.PI * 2f) * 0.08f;
                rt.localScale = Vector3.one * s;
                yield return null;
            }
            rt.localScale = Vector3.one;
        }

        private IEnumerator AnimateButtonShake(RectTransform rt)
        {
            if (rt == null) yield break;
            Vector2 origPos = rt.anchoredPosition;
            float dur = 0.35f;
            float elapsed = 0f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float xOffset = Mathf.Sin(elapsed * 45f) * 10f * (1f - t);
                rt.anchoredPosition = new Vector2(origPos.x + xOffset, origPos.y);
                yield return null;
            }
            rt.anchoredPosition = origPos;
        }

        private IEnumerator AnimateBannerEntrance(RectTransform rt)
        {
            if (rt == null) yield break;
            float dur = 0.25f;
            float elapsed = 0f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float scale = (t < 0.7f)
                    ? Mathf.Lerp(0.2f, 1.15f, t / 0.7f)
                    : Mathf.Lerp(1.15f, 1.0f, (t - 0.7f) / 0.3f);
                rt.localScale = Vector3.one * scale;
                yield return null;
            }
            rt.localScale = Vector3.one;
        }
        #endregion
    }
}
