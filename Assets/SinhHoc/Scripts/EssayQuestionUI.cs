// ============================================================
// EssayQuestionUI.cs
// UI câu TỰ LUẬN (làm bài trên giấy + chụp/tải ảnh gửi AI chấm):
//
//   QUESTION        : KNOWLEDGE CHALLENGE + đề bài + [CHỤP ẢNH][TẢI ẢNH]
//   PREVIEW         : xem ảnh + [CHỤP/TẢI LẠI][GỬI BÀI]
//   LOADING         : "ĐANG PHÂN TÍCH BÀI LÀM..." + Knowledge Core quay (#20)
//   RESULT          : AI EVALUATION - điểm, ✓ làm tốt, △ cần bổ sung,
//                     nhận xét, đáp án gợi ý, [NHẬN NÂNG CẤP] (#24)
//   UNREADABLE      : ảnh mờ/mất góc → [CHỤP/TẢI LẠI] hoặc [BỎ QUA CÂU NÀY],
//                     KHÔNG phạt, KHÔNG mất streak (#17)
//   ERROR           : lỗi kỹ thuật (mất mạng, timeout, backend...) →
//                     [THỬ LẠI] hoặc [BỎ QUA CÂU NÀY], KHÔNG tính là sai (#21)
//
// Combat PAUSE do BiologySurvivalManager quản lý (Time.timeScale = 0),
// mọi animation ở đây chạy bằng Time.unscaledDeltaTime.
// Toàn bộ UI dựng bằng code → không cần setup tay trong Editor.
// ============================================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PRU.Biology;

public class EssayQuestionUI : MonoBehaviour
{
    public static EssayQuestionUI Instance { get; private set; }

    /// <summary>Kết quả tự luận trả về cho BiologySurvivalManager.</summary>
    public class EssayResult
    {
        public float percentage;      // 0..100
        public bool skipped;          // player chủ động bỏ qua hoặc lỗi kỹ thuật → KHÔNG tính sai
        public bool unreadable;       // ảnh không đọc được (đã chụp lại hoặc bỏ qua)
        public AIGradingResponse response; // null nếu skipped/lỗi
    }

    [Header("Trạng thái (đọc để debug)")]
    public bool isShowingQuestion;

    private BiologyQuestion _question;
    private Action<EssayResult> _onGraded;
    private string _base64;
    private Sprite _previewSprite;
    private bool _busy;              // đang upload/chờ AI → chặn bấm nút

    // ---------- Panel gốc ----------
    private GameObject _questionRoot;
    private GameObject _loadingRoot;
    private GameObject _resultRoot;
    private GameObject _errorRoot;

    // ---------- Question ----------
    private TextMeshProUGUI _headerText;
    private TextMeshProUGUI _metaText;
    private TextMeshProUGUI _questionText;
    private RectTransform _pickButtonsRt;
    private RectTransform _previewButtonsRt;
    private RectTransform _previewImageRt;
    private Image _previewImage;
    private Button _btnCapture;
    private Button _btnUpload;
    private Button _btnRetake;
    private Button _btnSubmit;

    // ---------- Loading ----------
    private RectTransform _coreRt;
    private TextMeshProUGUI _loadingText;

    // ---------- Result ----------
    private TextMeshProUGUI _resultScoreText;
    private TextMeshProUGUI _correctPointsText;
    private TextMeshProUGUI _missingPointsText;
    private TextMeshProUGUI _feedbackText;
    private TextMeshProUGUI _suggestedText;
    private Button _btnTakeUpgrade;

    // ---------- Error / Unreadable ----------
    private TextMeshProUGUI _errorTitleText;
    private TextMeshProUGUI _errorMsgText;
    private Button _btnRetry;
    private Button _btnSkip;

    private RectTransform _frameRt;
    private Image _coreImage;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        // Hiệu ứng loading chạy bằng unscaled time (combat đang pause)
        if (_loadingRoot != null && _loadingRoot.activeSelf)
        {
            if (_coreRt != null) _coreRt.Rotate(0f, 0f, -160f * Time.unscaledDeltaTime);
            if (_loadingText != null)
            {
                float a = 0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * 5f);
                _loadingText.alpha = a;
            }
        }
    }

    // ================================================================
    // FLOW CHÍNH
    // ================================================================
    public void ShowQuestion(BiologyQuestion q, Action<EssayResult> onGraded)
    {
        _question = q;
        _onGraded = onGraded;
        _base64 = null;
        _previewSprite = null;
        _busy = false;
        isShowingQuestion = true;

        string lessonLabel = string.IsNullOrEmpty(q.lessonName) ? q.lessonID : q.lessonName;
        string diffLabel = q.difficulty.ToString().ToUpperInvariant();
        _headerText.text = "KNOWLEDGE CHALLENGE";
        _metaText.text = $"{q.subject.ToUpperInvariant()}  •  {lessonLabel}  •  {diffLabel}";
        _questionText.text = q.questionText;

        ShowPickMode();
        _questionRoot.SetActive(true);
        _loadingRoot.SetActive(false);
        _resultRoot.SetActive(false);
        _errorRoot.SetActive(false);
    }

    private void EndQuestion(EssayResult result)
    {
        isShowingQuestion = false;
        _questionRoot.SetActive(false);
        _loadingRoot.SetActive(false);
        _resultRoot.SetActive(false);
        _errorRoot.SetActive(false);
        _onGraded?.Invoke(result);
    }

    // ================================================================
    // CHỌN ẢNH / PREVIEW
    // ================================================================
    private void ShowPickMode()
    {
        _pickButtonsRt.gameObject.SetActive(true);
        _previewImageRt.gameObject.SetActive(false);
        _previewButtonsRt.gameObject.SetActive(false);
    }

    private void ShowPreviewMode()
    {
        _pickButtonsRt.gameObject.SetActive(false);
        _previewImageRt.gameObject.SetActive(true);
        _previewButtonsRt.gameObject.SetActive(true);
    }

    /// <summary>Gắn nút [CHỤP ẢNH].</summary>
    public void OnCaptureClicked() => PickImage(true);

    /// <summary>Gắn nút [TẢI ẢNH].</summary>
    public void OnUploadClicked() => PickImage(false);

    private void PickImage(bool capture)
    {
        if (_busy) return;
        ImagePickerManager picker = ImagePickerManager.EnsureInstance();
        picker.PickImage(capture, (success, base64, sprite) =>
        {
            if (!success || string.IsNullOrEmpty(base64))
            {
                if (base64 == "TOO_LARGE")
                    ShowError("ẢNH QUÁ LỚN", "Dung lượng ảnh vượt quá giới hạn. Hãy chụp lại bằng ảnh nhỏ hơn hoặc chọn ảnh khác.",
                              allowSkip: true);
                // Hủy chọn → giữ nguyên màn hiện tại, không làm gì thêm
                return;
            }
            _base64 = base64;
            _previewSprite = sprite;
            _previewImage.sprite = sprite;
            _previewImage.preserveAspect = true;
            ShowPreviewMode();
        });
    }

    /// <summary>Gắn nút [CHỤP/TẢI LẠI] (ở preview) - quay về chọn ảnh.</summary>
    public void OnRetakeClicked()
    {
        if (_busy) return;
        _base64 = null;
        _previewSprite = null;
        ShowPickMode();
    }

    /// <summary>Gắn nút [GỬI BÀI].</summary>
    public void OnSubmitClicked()
    {
        if (_busy || string.IsNullOrEmpty(_base64)) return;
        _busy = true;
        _questionRoot.SetActive(false);
        _loadingRoot.SetActive(true);

        AIGradingClient client = AIGradingClient.Instance;
        if (client == null)
        {
            GameObject go = new GameObject("AIGradingClient");
            client = go.AddComponent<AIGradingClient>();
        }
        client.SubmitEssay(_question.questionID, _base64, HandleGradingSuccess, HandleGradingError);
    }

    // ================================================================
    // KẾT QUẢ AI
    // ================================================================
    private void HandleGradingSuccess(AIGradingResponse resp)
    {
        _busy = false;
        _loadingRoot.SetActive(false);

        // ----- ẢNH KHÔNG ĐỌC ĐƯỢC (#17): không trừ điểm, không phạt -----
        if (resp == null || !resp.readable)
        {
            string msg = resp != null && !string.IsNullOrEmpty(resp.feedback)
                ? resp.feedback
                : "Không thể đọc rõ bài làm. Hãy chụp lại ảnh.";
            ShowError("KHÔNG ĐỌC ĐƯỢC BÀI LÀM", msg, allowSkip: true, isUnreadable: true);
            return;
        }

        // ----- LƯU PROGRESS (#26) -----
        KnowledgeProgressManager.RecordEssay(
            GameSessionData.SelectedSubject,
            GameSessionData.SelectedChapterID,
            GameSessionData.SelectedLessonID,
            resp.score, resp.maxScore);

        // ----- HIỂN THỊ KẾT QUẢ (#24) -----
        _resultScoreText.text = $"ĐIỂM: {resp.score:0.#} / {resp.maxScore:0.#}   ({resp.percentage:0}%)";
        _resultScoreText.color = GradeColor(resp.percentage);

        _correctPointsText.text = FormatPoints("LÀM TỐT:", "✓", resp.correctPoints);
        _missingPointsText.text = FormatPoints("CẦN BỔ SUNG:", "△", resp.missingPoints);
        _feedbackText.text = string.IsNullOrEmpty(resp.feedback) ? "" : $"NHẬN XÉT:\n\"{resp.feedback}\"";
        _suggestedText.text = string.IsNullOrEmpty(resp.suggestedAnswer) ? "" : $"ĐÁP ÁN GỢI Ý:\n{resp.suggestedAnswer}";

        _resultRoot.SetActive(true);
        _pendingResponse = resp;
    }

    private AIGradingResponse _pendingResponse;

    private static Color GradeColor(float pct)
    {
        if (pct >= 90f) return new Color(0.55f, 1f, 0.55f);   // PERFECT
        if (pct >= 70f) return new Color(0.55f, 0.9f, 1f);    // GOOD
        if (pct >= 50f) return new Color(1f, 0.9f, 0.5f);     // PASS
        return new Color(1f, 0.6f, 0.4f);                     // NEEDS IMPROVEMENT
    }

    private static string FormatPoints(string header, string icon, List<string> points)
    {
        if (points == null || points.Count == 0) return "";
        System.Text.StringBuilder sb = new System.Text.StringBuilder(header);
        foreach (string p in points) sb.Append($"\n{icon} {p}");
        return sb.ToString();
    }

    private void HandleGradingError(string err)
    {
        _busy = false;
        _loadingRoot.SetActive(false);
        // Lỗi kỹ thuật (#21) → KHÔNG tính là trả lời sai
        ShowError("CÓ LỖI KỸ THUẬT", $"Không gửi được bài làm ({err}). Bạn có thể thử lại hoặc bỏ qua câu này.",
                  allowSkip: true);
    }

    // ================================================================
    // ERROR / UNREADABLE PANEL
    // ================================================================
    private void ShowError(string title, string message, bool allowSkip, bool isUnreadable = false)
    {
        _errorTitleText.text = title;
        _errorTitleText.color = isUnreadable ? new Color(1f, 0.8f, 0.3f) : new Color(1f, 0.45f, 0.4f);
        _errorMsgText.text = message;
        _btnSkip.gameObject.SetActive(allowSkip);
        _errorRoot.SetActive(true);
        _unreadablePending = isUnreadable;
    }

    private bool _unreadablePending;

    /// <summary>Gắn nút [THỬ LẠI] / [CHỤP/TẢI ẢNH LẠI].</summary>
    public void OnRetryClicked()
    {
        _busy = false;
        _errorRoot.SetActive(false);
        _base64 = null;
        _previewSprite = null;
        ShowPickMode();
        _questionRoot.SetActive(true);
    }

    /// <summary>Gắn nút [BỎ QUA CÂU NÀY] - KHÔNG tính sai, KHÔNG mất streak (#17, #21).</summary>
    public void OnSkipClicked()
    {
        EssayResult r = new EssayResult
        {
            percentage = 0f,
            skipped = true,
            unreadable = _unreadablePending,
            response = null
        };
        _unreadablePending = false;
        EndQuestion(r);
    }

    // ================================================================
    // NÚT NHẬN NÂNG CẤP (#24)
    // ================================================================
    public void OnGetUpgradeClicked()
    {
        AIGradingResponse resp = _pendingResponse;
        _pendingResponse = null;
        float pct = resp != null ? resp.percentage : 0f;
        EndQuestion(new EssayResult
        {
            percentage = pct,
            skipped = false,
            unreadable = false,
            response = resp
        });
    }

    // ================================================================
    // DỰNG UI (tự động, không cần setup Inspector)
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
            canvas.sortingOrder = 25; // trên QuizUI một tầng để không bị che
        }

        BuildQuestionPanel();
        BuildLoadingPanel();
        BuildResultPanel();
        BuildErrorPanel();

        FitFrame();
    }

    private void BuildQuestionPanel()
    {
        _questionRoot = new GameObject("EssayQuestionPanel");
        _questionRoot.transform.SetParent(transform, false);
        RectTransform rootRt = _questionRoot.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.sizeDelta = Vector2.zero;
        _questionRoot.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

        GameObject frame = new GameObject("Frame");
        frame.transform.SetParent(_questionRoot.transform, false);
        RectTransform frameRt = frame.AddComponent<RectTransform>();
        frameRt.anchorMin = frameRt.anchorMax = frameRt.pivot = new Vector2(0.5f, 0.5f);
        frameRt.sizeDelta = new Vector2(820f, 640f);
        _frameRt = frameRt;
        Image frameImg = frame.AddComponent<Image>();
        frameImg.sprite = BiologySpriteFactory.GetWhiteSprite();
        frameImg.color = new Color(0.10f, 0.28f, 0.34f);

        _headerText = CreateText(frame.transform, "Header", "KNOWLEDGE CHALLENGE", 30,
                                 TextAlignmentOptions.Center, new Vector2(0f, 275f), new Vector2(760f, 50f));
        _headerText.color = new Color(0.5f, 1f, 0.85f);

        _metaText = CreateText(frame.transform, "Meta", "", 20,
                               TextAlignmentOptions.Center, new Vector2(0f, 235f), new Vector2(760f, 34f));
        _metaText.color = new Color(0.8f, 0.95f, 0.9f);

        _questionText = CreateText(frame.transform, "QuestionText", "", 25,
                                   TextAlignmentOptions.Center, new Vector2(0f, 150f), new Vector2(760f, 140f));
        _questionText.color = Color.white;

        // ----- Hai nút chọn ảnh -----
        GameObject pickGroup = new GameObject("PickButtons");
        pickGroup.transform.SetParent(frame.transform, false);
        RectTransform pickRt = pickGroup.AddComponent<RectTransform>();
        pickRt.anchorMin = pickRt.anchorMax = pickRt.pivot = new Vector2(0.5f, 0.5f);
        pickRt.anchoredPosition = new Vector2(0f, -60f);
        pickRt.sizeDelta = new Vector2(760f, 80f);
        _pickButtonsRt = pickRt;

        _btnCapture = CreateButton(pickGroup.transform, "BtnCapture", "CHỤP ẢNH",
                                   new Vector2(-195f, 0f), new Vector2(360f, 72f), new Color(0.2f, 0.6f, 0.45f),
                                   OnCaptureClicked);
        _btnUpload = CreateButton(pickGroup.transform, "BtnUpload", "TẢI ẢNH",
                                  new Vector2(195f, 0f), new Vector2(360f, 72f), new Color(0.2f, 0.45f, 0.65f),
                                  OnUploadClicked);

        // ----- Ảnh preview -----
        GameObject previewGo = new GameObject("ImagePreview");
        previewGo.transform.SetParent(frame.transform, false);
        RectTransform pvRt = previewGo.AddComponent<RectTransform>();
        pvRt.anchorMin = pvRt.anchorMax = pvRt.pivot = new Vector2(0.5f, 0.5f);
        pvRt.anchoredPosition = new Vector2(0f, -40f);
        pvRt.sizeDelta = new Vector2(520f, 300f);
        _previewImageRt = pvRt;
        _previewImage = previewGo.AddComponent<Image>();
        _previewImage.color = new Color(0.9f, 0.9f, 0.9f);
        _previewImage.preserveAspect = true;

        // ----- Hai nút preview -----
        GameObject pvGroup = new GameObject("PreviewButtons");
        pvGroup.transform.SetParent(frame.transform, false);
        RectTransform pvBtnRt = pvGroup.AddComponent<RectTransform>();
        pvBtnRt.anchorMin = pvBtnRt.anchorMax = pvBtnRt.pivot = new Vector2(0.5f, 0.5f);
        pvBtnRt.anchoredPosition = new Vector2(0f, -240f);
        pvBtnRt.sizeDelta = new Vector2(760f, 80f);
        _previewButtonsRt = pvBtnRt;

        _btnRetake = CreateButton(pvGroup.transform, "BtnRetake", "CHỤP/TẢI LẠI",
                                  new Vector2(-195f, 0f), new Vector2(360f, 66f), new Color(0.75f, 0.45f, 0.3f),
                                  OnRetakeClicked);
        _btnSubmit = CreateButton(pvGroup.transform, "BtnSubmit", "GỬI BÀI",
                                  new Vector2(195f, 0f), new Vector2(360f, 66f), new Color(0.3f, 0.65f, 0.4f),
                                  OnSubmitClicked);

        _questionRoot.SetActive(false);
    }

    private void BuildLoadingPanel()
    {
        _loadingRoot = new GameObject("EssayLoadingPanel");
        _loadingRoot.transform.SetParent(transform, false);
        RectTransform rootRt = _loadingRoot.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.sizeDelta = Vector2.zero;
        _loadingRoot.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

        // Knowledge Core: hình thoi quay quanh tâm
        GameObject core = new GameObject("KnowledgeCore");
        core.transform.SetParent(_loadingRoot.transform, false);
        RectTransform coreRt = core.AddComponent<RectTransform>();
        coreRt.anchorMin = coreRt.anchorMax = coreRt.pivot = new Vector2(0.5f, 0.5f);
        coreRt.anchoredPosition = new Vector2(0f, 60f);
        coreRt.sizeDelta = new Vector2(90f, 90f);
        Image coreImg = core.AddComponent<Image>();
        coreImg.sprite = BiologySpriteFactory.GetXpBubbleSprite();
        coreImg.color = new Color(0.4f, 1f, 0.9f);
        _coreRt = coreRt;
        _coreImage = coreImg;

        _loadingText = CreateText(_loadingRoot.transform, "LoadingText", "ĐANG PHÂN TÍCH BÀI LÀM...", 34,
                                  TextAlignmentOptions.Center, new Vector2(0f, -80f), new Vector2(900f, 70f));
        _loadingText.color = new Color(0.6f, 1f, 0.9f);

        CreateText(_loadingRoot.transform, "SubText", "AI đang chấm bài theo rubric của câu hỏi", 20,
                   TextAlignmentOptions.Center, new Vector2(0f, -130f), new Vector2(900f, 40f))
            .color = new Color(0.8f, 0.9f, 0.9f);

        _loadingRoot.SetActive(false);
    }

    private void BuildResultPanel()
    {
        _resultRoot = new GameObject("EssayResultPanel");
        _resultRoot.transform.SetParent(transform, false);
        RectTransform rootRt = _resultRoot.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.sizeDelta = Vector2.zero;
        _resultRoot.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

        GameObject frame = new GameObject("Frame");
        frame.transform.SetParent(_resultRoot.transform, false);
        RectTransform frameRt = frame.AddComponent<RectTransform>();
        frameRt.anchorMin = frameRt.anchorMax = frameRt.pivot = new Vector2(0.5f, 0.5f);
        frameRt.sizeDelta = new Vector2(720f, 640f);
        Image frameImg = frame.AddComponent<Image>();
        frameImg.sprite = BiologySpriteFactory.GetWhiteSprite();
        frameImg.color = new Color(0.10f, 0.22f, 0.30f);

        CreateText(frame.transform, "Title", "AI EVALUATION", 34, TextAlignmentOptions.Center,
                   new Vector2(0f, 275f), new Vector2(660f, 50f))
            .color = new Color(0.5f, 1f, 0.85f);

        _resultScoreText = CreateText(frame.transform, "Score", "", 30, TextAlignmentOptions.Center,
                                      new Vector2(0f, 225f), new Vector2(660f, 46f));

        _correctPointsText = CreateText(frame.transform, "CorrectPoints", "", 20, TextAlignmentOptions.Left,
                                        new Vector2(0f, 140f), new Vector2(660f, 120f));
        _correctPointsText.color = new Color(0.6f, 1f, 0.6f);

        _missingPointsText = CreateText(frame.transform, "MissingPoints", "", 20, TextAlignmentOptions.Left,
                                        new Vector2(0f, 55f), new Vector2(660f, 90f));
        _missingPointsText.color = new Color(1f, 0.85f, 0.55f);

        _feedbackText = CreateText(frame.transform, "Feedback", "", 20, TextAlignmentOptions.Left,
                                   new Vector2(0f, -25f), new Vector2(660f, 80f));
        _feedbackText.color = Color.white;

        _suggestedText = CreateText(frame.transform, "Suggested", "", 20, TextAlignmentOptions.Left,
                                    new Vector2(0f, -105f), new Vector2(660f, 80f));
        _suggestedText.color = new Color(0.75f, 0.9f, 1f);

        _btnTakeUpgrade = CreateButton(frame.transform, "BtnTakeUpgrade", "NHẬN NÂNG CẤP",
                                       new Vector2(0f, -230f), new Vector2(340f, 66f), new Color(0.55f, 0.35f, 0.9f),
                                       OnGetUpgradeClicked);

        _resultRoot.SetActive(false);
    }

    private void BuildErrorPanel()
    {
        _errorRoot = new GameObject("EssayErrorPanel");
        _errorRoot.transform.SetParent(transform, false);
        RectTransform rootRt = _errorRoot.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.sizeDelta = Vector2.zero;
        _errorRoot.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);

        GameObject frame = new GameObject("Frame");
        frame.transform.SetParent(_errorRoot.transform, false);
        RectTransform frameRt = frame.AddComponent<RectTransform>();
        frameRt.anchorMin = frameRt.anchorMax = frameRt.pivot = new Vector2(0.5f, 0.5f);
        frameRt.sizeDelta = new Vector2(640f, 420f);
        Image frameImg = frame.AddComponent<Image>();
        frameImg.sprite = BiologySpriteFactory.GetWhiteSprite();
        frameImg.color = new Color(0.22f, 0.14f, 0.14f);

        _errorTitleText = CreateText(frame.transform, "Title", "", 32, TextAlignmentOptions.Center,
                                     new Vector2(0f, 140f), new Vector2(580f, 50f));

        _errorMsgText = CreateText(frame.transform, "Message", "", 22, TextAlignmentOptions.Center,
                                   new Vector2(0f, 55f), new Vector2(580f, 130f));
        _errorMsgText.color = Color.white;

        _btnRetry = CreateButton(frame.transform, "BtnRetry", "CHỤP/TẢI LẠI",
                                 new Vector2(-160f, -110f), new Vector2(300f, 64f), new Color(0.3f, 0.6f, 0.75f),
                                 OnRetryClicked);
        _btnSkip = CreateButton(frame.transform, "BtnSkip", "BỎ QUA CÂU NÀY",
                                new Vector2(160f, -110f), new Vector2(300f, 64f), new Color(0.5f, 0.5f, 0.55f),
                                OnSkipClicked);

        _errorRoot.SetActive(false);
    }

    // ================================================================
    // TIỆN ÍCH
    // ================================================================
    private void OnRectTransformDimensionsChange() => FitFrame();

    private void FitFrame()
    {
        RectTransform canvasRt = transform as RectTransform;
        if (canvasRt == null || _frameRt == null) return;
        float h = canvasRt.rect.height;
        float w = canvasRt.rect.width;
        if (h <= 0f || w <= 0f) return;
        float k = Mathf.Min(1f, h / 680f, w / 860f);
        _frameRt.localScale = new Vector3(k, k, 1f);
    }

    private TextMeshProUGUI CreateText(Transform parent, string name, string text, int size,
                                       TextAlignmentOptions align, Vector2 pos, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
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
                                Color color, Action onClick)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.sprite = BiologySpriteFactory.GetWhiteSprite();
        img.color = color;

        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(() => onClick());

        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(go.transform, false);
        RectTransform lrt = labelGo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        return btn;
    }
}
