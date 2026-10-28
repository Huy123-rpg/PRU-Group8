// ============================================================
// EssayQuestionUI.cs
// UI câu TỰ LUẬN (cho phép nhập trực tiếp hoặc tải ảnh chữ viết tay):
//
// 1. Cho phép nhập văn bản trực tiếp (Type) vào ô làm bài nhiều dòng.
// 2. Cho phép tải ảnh bài làm / chữ viết tay từ máy tính (hộp thoại file Windows thật).
// 3. AI OCR tự động nhận diện chữ viết tay, chép lại từng dòng lên màn hình.
// 4. Cho phép người chơi trực tiếp kiểm tra và sửa lại các dòng AI nhận sai trước khi nộp.
// 5. AI chấm chi tiết: điểm tổng, chấm từng dòng / từng bước, chỉ rõ dòng nào sai,
//    lỗi sai cụ thể, gợi ý hướng làm đúng, nhận xét và lời giải chuẩn mực.
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

    public class EssayResult
    {
        public float percentage;
        public bool skipped;
        public bool unreadable;
        public AIGradingResponse response;
    }

    [Header("Trạng thái (đọc để debug)")]
    public bool isShowingQuestion;

    private BiologyQuestion _question;
    private Action<EssayResult> _onGraded;
    private string _base64;
    private Sprite _previewSprite;
    private bool _busy;

    // ---------- Panel gốc ----------
    private GameObject _questionRoot;
    private GameObject _loadingRoot;
    private GameObject _resultRoot;
    private GameObject _errorRoot;

    // ---------- Question ----------
    private TextMeshProUGUI _headerText;
    private TextMeshProUGUI _metaText;
    private TextMeshProUGUI _questionText;
    private TMP_InputField _essayInputField;
    private TextMeshProUGUI _ocrStatusText;
    private RectTransform _previewImageRt;
    private Image _previewImage;
    private Button _btnCapture;
    private Button _btnUpload;
    private Button _btnSubmit;
    private Button _btnClearImage;

    // ---------- Loading ----------
    private RectTransform _coreRt;
    private TextMeshProUGUI _loadingText;
    private TextMeshProUGUI _loadingSubText;

    // ---------- Result ----------
    private TextMeshProUGUI _resultScoreText;
    private Transform _resultScrollContent;
    private Button _btnTakeUpgrade;

    // ---------- Error / Unreadable ----------
    private TextMeshProUGUI _errorTitleText;
    private TextMeshProUGUI _errorMsgText;
    private Button _btnRetry;
    private Button _btnSkip;

    private RectTransform _frameRt;
    private AIGradingResponse _pendingResponse;

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
        _headerText.text = "BÀI TẬP TỰ LUẬN (AI ĐÁNH GIÁ)";
        _metaText.text = $"{q.subject.ToUpperInvariant()}  •  {lessonLabel}  •  {diffLabel}";
        _questionText.text = q.questionText;

        if (_essayInputField != null) _essayInputField.text = "";
        if (_ocrStatusText != null)
        {
            _ocrStatusText.text = "💡 <i>Bạn có thể gõ câu trả lời vào khung bên dưới, hoặc bấm [TẢI ẢNH TỪ MÁY] để AI chép chữ viết tay.</i>";
            _ocrStatusText.color = new Color(0.85f, 0.95f, 1f);
        }

        if (_previewImageRt != null) _previewImageRt.gameObject.SetActive(false);
        if (_btnClearImage != null) _btnClearImage.gameObject.SetActive(false);

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
    // CHỌN ẢNH & NHẬN DIỆN CHỮ VIẾT TAY
    // ================================================================
    public void OnCaptureClicked() => PickImage(true);
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
                    ShowError("ẢNH QUÁ LỚN", "Dung lượng ảnh vượt quá giới hạn. Hãy chọn ảnh nhỏ hơn.", allowSkip: true);
                return;
            }

            _base64 = base64;
            _previewSprite = sprite;
            if (_previewImage != null)
            {
                _previewImage.sprite = sprite;
                _previewImageRt.gameObject.SetActive(true);
            }
            if (_btnClearImage != null) _btnClearImage.gameObject.SetActive(true);

            // Bắt đầu nhận diện chữ viết tay qua AI OCR
            StartCoroutine(PerformOCR(base64));
        });
    }

    private IEnumerator PerformOCR(string base64)
    {
        _busy = true;
        if (_ocrStatusText != null)
        {
            _ocrStatusText.text = "<color=#FACC15>🔍 AI đang nhận diện chữ viết tay từ bài làm của bạn...</color>";
        }

        yield return null;

        AIGradingClient client = AIGradingClient.Instance;
        if (client == null)
        {
            GameObject go = new GameObject("AIGradingClient");
            client = go.AddComponent<AIGradingClient>();
        }

        client.TranscribeHandwriting(_question != null ? _question.questionID : "Q_DEFAULT", base64,
            (transcribedText) =>
            {
                _busy = false;
                if (_essayInputField != null)
                {
                    _essayInputField.text = transcribedText;
                }
                if (_ocrStatusText != null)
                {
                    _ocrStatusText.text = "<color=#4ADE80>✅ AI đã nhận diện chữ viết tay! Bạn có thể xem và chỉnh sửa từng dòng bên dưới trước khi nộp.</color>";
                }
            },
            (error) =>
            {
                _busy = false;
                if (_ocrStatusText != null)
                {
                    _ocrStatusText.text = $"<color=#F87171>⚠️ Nhận diện ảnh chưa rõ ({error}). Bạn hãy gõ trực tiếp câu trả lời vào khung bên dưới.</color>";
                }
            });
    }

    public void OnClearImageClicked()
    {
        _base64 = null;
        _previewSprite = null;
        if (_previewImageRt != null) _previewImageRt.gameObject.SetActive(false);
        if (_btnClearImage != null) _btnClearImage.gameObject.SetActive(false);
        if (_ocrStatusText != null)
        {
            _ocrStatusText.text = "💡 <i>Đã xóa ảnh đính kèm. Bạn có thể tiếp tục gõ bài hoặc chọn ảnh khác.</i>";
            _ocrStatusText.color = new Color(0.85f, 0.95f, 1f);
        }
    }

    // ================================================================
    // GỬI BÀI TỰ LUẬN ĐỂ AI CHẤM
    // ================================================================
    public void OnSubmitClicked()
    {
        if (_busy) return;

        string studentText = (_essayInputField != null) ? _essayInputField.text.Trim() : "";
        if (string.IsNullOrEmpty(studentText) && string.IsNullOrEmpty(_base64))
        {
            if (_ocrStatusText != null)
            {
                _ocrStatusText.text = "<color=#F87171>⚠️ Vui lòng nhập câu trả lời hoặc tải ảnh bài làm trước khi nộp!</color>";
            }
            return;
        }

        _busy = true;
        _questionRoot.SetActive(false);
        _loadingRoot.SetActive(true);

        if (_loadingText != null) _loadingText.text = "AI ĐANG CHẤM BÀI THEO TỪNG BƯỚC...";
        if (_loadingSubText != null) _loadingSubText.text = "Phân tích cú pháp, đánh giá từng dòng và lập bảng điểm chi tiết";

        AIGradingClient client = AIGradingClient.Instance;
        if (client == null)
        {
            GameObject go = new GameObject("AIGradingClient");
            client = go.AddComponent<AIGradingClient>();
        }

        client.SubmitEssay(_question.questionID, studentText, _base64, HandleGradingSuccess, HandleGradingError);
    }

    // ================================================================
    // XỬ LÝ KẾT QUẢ AI CHẤM
    // ================================================================
    private void HandleGradingSuccess(AIGradingResponse resp)
    {
        _busy = false;
        _loadingRoot.SetActive(false);

        if (resp == null || !resp.readable)
        {
            string msg = (resp != null && !string.IsNullOrEmpty(resp.feedback))
                ? resp.feedback
                : "Không thể đọc rõ bài làm. Hãy chụp lại ảnh rõ nét hơn hoặc gõ trực tiếp câu trả lời.";
            ShowError("KHÔNG ĐỌC ĐƯỢC BÀI LÀM", msg, allowSkip: true, isUnreadable: true);
            return;
        }

        // Lưu tiến độ học tập
        KnowledgeProgressManager.RecordEssay(
            GameSessionData.SelectedSubject,
            GameSessionData.SelectedChapterID,
            GameSessionData.SelectedLessonID,
            resp.score, resp.maxScore);

        // Hiển thị kết quả chi tiết
        DisplayDetailedResult(resp);
    }

    private void DisplayDetailedResult(AIGradingResponse resp)
    {
        _pendingResponse = resp;

        _resultScoreText.text = $"ĐIỂM TỔNG KẾT: <size=38><b>{resp.score:0.#} / {resp.maxScore:0.#}</b></size>   ({resp.percentage:0}%)";
        _resultScoreText.color = GradeColor(resp.percentage);

        // Xóa nội dung cũ trong scrollview
        if (_resultScrollContent != null)
        {
            for (int i = _resultScrollContent.childCount - 1; i >= 0; i--)
            {
                Destroy(_resultScrollContent.GetChild(i).gameObject);
            }

            // 1. Nhận xét tổng quan của AI
            AddResultSectionHeader(_resultScrollContent, "💬 NHẬN XÉT CỦA AI:", new Color(0.4f, 0.9f, 1f));
            AddResultTextItem(_resultScrollContent, resp.feedback, new Color(0.95f, 0.95f, 0.95f));

            // 2. Bảng chấm chi tiết theo từng dòng / bước
            if (resp.stepBreakdown != null && resp.stepBreakdown.Count > 0)
            {
                AddResultSectionHeader(_resultScrollContent, "📝 ĐÁNH GIÁ CHI TIẾT THEO TỪNG DÒNG / TỪNG BƯỚC:", new Color(1f, 0.85f, 0.3f));

                foreach (var step in resp.stepBreakdown)
                {
                    string statusColor = step.isCorrect ? "#4ADE80" : "#F87171";
                    string statusIcon = step.isCorrect ? "✓" : "✗";
                    string stepText = $"<b><color={statusColor}>[Dòng {step.lineNumber}] {statusIcon} {step.statusText} (+{step.stepScore:0.#}/{step.maxStepScore:0.#}đ)</color></b>\n" +
                                      $"<i>\"{step.lineText}\"</i>\n" +
                                      $"<color=#E0E7FF>→ Ghi chú: {step.errorDetail}</color>";
                    AddResultCardItem(_resultScrollContent, stepText, step.isCorrect ? new Color(0.12f, 0.28f, 0.22f, 0.9f) : new Color(0.32f, 0.15f, 0.15f, 0.9f));
                }
            }

            // 3. Các lỗi sai cụ thể cần lưu ý
            if (resp.errorsIdentified != null && resp.errorsIdentified.Count > 0)
            {
                AddResultSectionHeader(_resultScrollContent, "⚠️ CÁC LỖI SAI CẦN KHẮC PHỤC:", new Color(1f, 0.5f, 0.4f));
                foreach (var err in resp.errorsIdentified)
                {
                    AddResultTextItem(_resultScrollContent, $"• {err}", new Color(1f, 0.75f, 0.7f));
                }
            }

            // 4. Gợi ý & hướng dẫn làm bài
            if (!string.IsNullOrEmpty(resp.suggestions))
            {
                AddResultSectionHeader(_resultScrollContent, "💡 GỢI Ý & HƯỚNG DẪN TỪ AI:", new Color(0.5f, 1f, 0.7f));
                AddResultTextItem(_resultScrollContent, resp.suggestions, new Color(0.9f, 1f, 0.9f));
            }

            // 5. Đáp án & lời giải chi tiết chuẩn mực
            if (!string.IsNullOrEmpty(resp.suggestedAnswer))
            {
                AddResultSectionHeader(_resultScrollContent, "📘 ĐÁP ÁN & LỜI GIẢI CHUẨN MỰC:", new Color(0.7f, 0.85f, 1f));
                AddResultTextItem(_resultScrollContent, resp.suggestedAnswer, new Color(0.9f, 0.95f, 1f));
            }

            // Ép layout tự tính toán chiều cao
            LayoutRebuilder.ForceRebuildLayoutImmediate(_resultScrollContent as RectTransform);
        }

        _resultRoot.SetActive(true);
    }

    private void AddResultSectionHeader(Transform parent, string title, Color color)
    {
        GameObject go = new GameObject("SectionHeader");
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = $"<b>{title}</b>";
        tmp.fontSize = 20;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        ContentSizeFitter csf = go.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void AddResultTextItem(Transform parent, string text, Color color)
    {
        GameObject go = new GameObject("TextItem");
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 17;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        ContentSizeFitter csf = go.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void AddResultCardItem(Transform parent, string text, Color bgColor)
    {
        GameObject card = new GameObject("CardItem");
        card.transform.SetParent(parent, false);
        Image bg = card.AddComponent<Image>();
        bg.color = bgColor;

        VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(14, 14, 10, 10);
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = card.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject textGo = new GameObject("CardText");
        textGo.transform.SetParent(card.transform, false);
        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 16;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.Normal;
    }

    private static Color GradeColor(float pct)
    {
        if (pct >= 90f) return new Color(0.35f, 1f, 0.55f);
        if (pct >= 70f) return new Color(0.45f, 0.85f, 1f);
        if (pct >= 50f) return new Color(1f, 0.85f, 0.4f);
        return new Color(1f, 0.45f, 0.4f);
    }

    private void HandleGradingError(string err)
    {
        _busy = false;
        _loadingRoot.SetActive(false);
        ShowError("CÓ LỖI KỸ THUẬT", $"Không thể gửi bài ({err}). Vui lòng kiểm tra lại kết nối mạng hoặc thử lại.", allowSkip: true);
    }

    private void ShowError(string title, string message, bool allowSkip, bool isUnreadable = false)
    {
        _errorTitleText.text = title;
        _errorTitleText.color = isUnreadable ? new Color(1f, 0.8f, 0.3f) : new Color(1f, 0.45f, 0.4f);
        _errorMsgText.text = message;
        _btnSkip.gameObject.SetActive(allowSkip);
        _errorRoot.SetActive(true);
    }

    public void OnRetryClicked()
    {
        _busy = false;
        _errorRoot.SetActive(false);
        _questionRoot.SetActive(true);
    }

    public void OnSkipClicked()
    {
        EndQuestion(new EssayResult
        {
            percentage = 0f,
            skipped = true,
            unreadable = false,
            response = null
        });
    }

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
    // DỰNG UI (HOÀN TOÀN TỰ ĐỘNG, CHUẨN THIẾT KẾ HIỆN ĐẠI)
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
            canvas.sortingOrder = 30;
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
        _questionRoot.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

        GameObject frame = new GameObject("Frame");
        frame.transform.SetParent(_questionRoot.transform, false);
        RectTransform frameRt = frame.AddComponent<RectTransform>();
        frameRt.anchorMin = frameRt.anchorMax = frameRt.pivot = new Vector2(0.5f, 0.5f);
        frameRt.sizeDelta = new Vector2(980f, 660f);
        _frameRt = frameRt;
        Image frameImg = frame.AddComponent<Image>();
        frameImg.sprite = BiologySpriteFactory.GetWhiteSprite();
        frameImg.color = new Color(0.08f, 0.16f, 0.22f, 0.98f);

        _headerText = CreateText(frame.transform, "Header", "BÀI TẬP TỰ LUẬN (AI ĐÁNH GIÁ)", 26,
                                 TextAlignmentOptions.Center, new Vector2(0f, 300f), new Vector2(920f, 38f));
        _headerText.color = new Color(0.4f, 0.9f, 1f);

        _metaText = CreateText(frame.transform, "Meta", "", 18,
                               TextAlignmentOptions.Center, new Vector2(0f, 270f), new Vector2(920f, 26f));
        _metaText.color = new Color(0.75f, 0.9f, 0.95f);

        // Khung đề bài
        GameObject questionBox = new GameObject("QuestionBox");
        questionBox.transform.SetParent(frame.transform, false);
        RectTransform qBoxRt = questionBox.AddComponent<RectTransform>();
        qBoxRt.anchorMin = qBoxRt.anchorMax = qBoxRt.pivot = new Vector2(0.5f, 0.5f);
        qBoxRt.anchoredPosition = new Vector2(0f, 205f);
        qBoxRt.sizeDelta = new Vector2(920f, 95f);
        Image qBoxImg = questionBox.AddComponent<Image>();
        qBoxImg.sprite = BiologySpriteFactory.GetWhiteSprite();
        qBoxImg.color = new Color(0.12f, 0.24f, 0.32f, 0.9f);

        _questionText = CreateText(questionBox.transform, "QuestionText", "", 19,
                                   TextAlignmentOptions.TopLeft, Vector2.zero, new Vector2(890f, 80f));
        _questionText.color = Color.white;

        // Trạng thái OCR / Hướng dẫn
        _ocrStatusText = CreateText(frame.transform, "OCRStatus", "💡 Bạn có thể gõ bài làm hoặc bấm [TẢI ẢNH] để AI nhận diện chữ viết tay.", 16,
                                    TextAlignmentOptions.MidlineLeft, new Vector2(0f, 140f), new Vector2(920f, 30f));

        // Khung nhập câu trả lời (Multi-line InputField)
        _essayInputField = CreateInputField(frame.transform, "EssayInputField",
                                            new Vector2(-110f, -20f), new Vector2(700f, 260f),
                                            "Gõ câu trả lời của bạn tại đây, hoặc bấm [TẢI ẢNH TỪ MÁY] để AI chép chữ viết tay...");

        // Khung preview thumbnail ảnh đính kèm (nằm bên phải ô gõ)
        GameObject pvGo = new GameObject("ImageThumbnail");
        pvGo.transform.SetParent(frame.transform, false);
        _previewImageRt = pvGo.AddComponent<RectTransform>();
        _previewImageRt.anchorMin = _previewImageRt.anchorMax = _previewImageRt.pivot = new Vector2(0.5f, 0.5f);
        _previewImageRt.anchoredPosition = new Vector2(355f, 10f);
        _previewImageRt.sizeDelta = new Vector2(200f, 200f);
        _previewImage = pvGo.AddComponent<Image>();
        _previewImage.color = Color.white;
        _previewImage.preserveAspect = true;
        pvGo.SetActive(false);

        _btnClearImage = CreateButton(frame.transform, "BtnClearImage", "XÓA ẢNH",
                                      new Vector2(355f, -115f), new Vector2(180f, 38f),
                                      new Color(0.6f, 0.25f, 0.25f), OnClearImageClicked);
        _btnClearImage.gameObject.SetActive(false);

        // Hàng nút chức năng bên dưới
        GameObject btnRow = new GameObject("ButtonRow");
        btnRow.transform.SetParent(frame.transform, false);
        RectTransform bRowRt = btnRow.AddComponent<RectTransform>();
        bRowRt.anchorMin = bRowRt.anchorMax = bRowRt.pivot = new Vector2(0.5f, 0.5f);
        bRowRt.anchoredPosition = new Vector2(0f, -265f);
        bRowRt.sizeDelta = new Vector2(920f, 65f);

        _btnCapture = CreateButton(btnRow.transform, "BtnCapture", "📷 CHỤP ẢNH",
                                   new Vector2(-310f, 0f), new Vector2(260f, 58f),
                                   new Color(0.18f, 0.55f, 0.42f), OnCaptureClicked);

        _btnUpload = CreateButton(btnRow.transform, "BtnUpload", "📁 TẢI ẢNH TỪ MÁY",
                                  new Vector2(0f, 0f), new Vector2(280f, 58f),
                                  new Color(0.18f, 0.45f, 0.72f), OnUploadClicked);

        _btnSubmit = CreateButton(btnRow.transform, "BtnSubmit", "🚀 GỬI BÀI ĐỂ AI CHẤM",
                                  new Vector2(310f, 0f), new Vector2(280f, 58f),
                                  new Color(0.22f, 0.68f, 0.35f), OnSubmitClicked);

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
        _loadingRoot.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

        GameObject core = new GameObject("KnowledgeCore");
        core.transform.SetParent(_loadingRoot.transform, false);
        RectTransform coreRt = core.AddComponent<RectTransform>();
        coreRt.anchorMin = coreRt.anchorMax = coreRt.pivot = new Vector2(0.5f, 0.5f);
        coreRt.anchoredPosition = new Vector2(0f, 70f);
        coreRt.sizeDelta = new Vector2(100f, 100f);
        Image coreImg = core.AddComponent<Image>();
        coreImg.sprite = BiologySpriteFactory.GetXpBubbleSprite();
        coreImg.color = new Color(0.3f, 0.9f, 1f);
        _coreRt = coreRt;

        _loadingText = CreateText(_loadingRoot.transform, "LoadingText", "AI ĐANG PHÂN TÍCH VÀ CHẤM BÀI...", 28,
                                  TextAlignmentOptions.Center, new Vector2(0f, -50f), new Vector2(900f, 50f));
        _loadingText.color = new Color(0.4f, 0.95f, 1f);

        _loadingSubText = CreateText(_loadingRoot.transform, "SubText", "Đang chấm theo từng dòng và lập bảng điểm chi tiết", 18,
                                     TextAlignmentOptions.Center, new Vector2(0f, -95f), new Vector2(900f, 35f));
        _loadingSubText.color = new Color(0.8f, 0.9f, 0.95f);

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
        _resultRoot.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

        GameObject frame = new GameObject("Frame");
        frame.transform.SetParent(_resultRoot.transform, false);
        RectTransform frameRt = frame.AddComponent<RectTransform>();
        frameRt.anchorMin = frameRt.anchorMax = frameRt.pivot = new Vector2(0.5f, 0.5f);
        frameRt.sizeDelta = new Vector2(960f, 660f);
        Image frameImg = frame.AddComponent<Image>();
        frameImg.sprite = BiologySpriteFactory.GetWhiteSprite();
        frameImg.color = new Color(0.08f, 0.15f, 0.22f, 0.98f);

        CreateText(frame.transform, "Title", "KẾT QUẢ ĐÁNH GIÁ BÀI TỰ LUẬN (AI)", 28, TextAlignmentOptions.Center,
                   new Vector2(0f, 295f), new Vector2(900f, 40f))
            .color = new Color(0.4f, 0.9f, 1f);

        _resultScoreText = CreateText(frame.transform, "Score", "", 24, TextAlignmentOptions.Center,
                                      new Vector2(0f, 255f), new Vector2(900f, 38f));

        // Khung Scroll View chứa bảng chấm chi tiết
        GameObject scrollGo = new GameObject("ResultScrollView");
        scrollGo.transform.SetParent(frame.transform, false);
        RectTransform srt = scrollGo.AddComponent<RectTransform>();
        srt.anchorMin = srt.anchorMax = srt.pivot = new Vector2(0.5f, 0.5f);
        srt.anchoredPosition = new Vector2(0f, 15f);
        srt.sizeDelta = new Vector2(900f, 420f);

        ScrollRect scrollRect = scrollGo.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollGo.transform, false);
        RectTransform vprt = viewport.AddComponent<RectTransform>();
        vprt.anchorMin = Vector2.zero;
        vprt.anchorMax = Vector2.one;
        vprt.sizeDelta = Vector2.zero;
        viewport.AddComponent<RectMask2D>();

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform crt = content.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0f, 1f);
        crt.anchorMax = new Vector2(1f, 1f);
        crt.pivot = new Vector2(0.5f, 1f);
        crt.anchoredPosition = Vector2.zero;
        crt.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 14;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = vprt;
        scrollRect.content = crt;
        _resultScrollContent = content.transform;

        _btnTakeUpgrade = CreateButton(frame.transform, "BtnTakeUpgrade", "TIẾP TỤC HÀNH TRÌNH",
                                       new Vector2(0f, -270f), new Vector2(380f, 58f),
                                       new Color(0.24f, 0.65f, 0.38f), OnGetUpgradeClicked);

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
        frameImg.color = new Color(0.20f, 0.12f, 0.14f);

        _errorTitleText = CreateText(frame.transform, "Title", "", 30, TextAlignmentOptions.Center,
                                     new Vector2(0f, 140f), new Vector2(580f, 50f));

        _errorMsgText = CreateText(frame.transform, "Message", "", 20, TextAlignmentOptions.Center,
                                   new Vector2(0f, 45f), new Vector2(580f, 140f));
        _errorMsgText.color = Color.white;

        _btnRetry = CreateButton(frame.transform, "BtnRetry", "THỬ LẠI",
                                 new Vector2(-160f, -120f), new Vector2(280f, 58f), new Color(0.25f, 0.55f, 0.7f),
                                 OnRetryClicked);
        _btnSkip = CreateButton(frame.transform, "BtnSkip", "BỎ QUA CÂU NÀY",
                                new Vector2(160f, -120f), new Vector2(280f, 58f), new Color(0.5f, 0.5f, 0.55f),
                                OnSkipClicked);

        _errorRoot.SetActive(false);
    }

    private void FitFrame()
    {
        RectTransform canvasRt = transform as RectTransform;
        if (canvasRt == null || _frameRt == null) return;
        float h = canvasRt.rect.height;
        float w = canvasRt.rect.width;
        if (h <= 0f || w <= 0f) return;
        float k = Mathf.Min(1f, h / 720f, w / 1020f);
        _frameRt.localScale = new Vector3(k, k, 1f);
    }

    private TMP_InputField CreateInputField(Transform parent, string name, Vector2 pos, Vector2 size, string placeholderText)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image bg = go.AddComponent<Image>();
        bg.sprite = BiologySpriteFactory.GetWhiteSprite();
        bg.color = new Color(0.05f, 0.10f, 0.15f, 0.95f);

        GameObject textArea = new GameObject("TextArea");
        textArea.transform.SetParent(go.transform, false);
        RectTransform taRt = textArea.AddComponent<RectTransform>();
        taRt.anchorMin = Vector2.zero;
        taRt.anchorMax = Vector2.one;
        taRt.offsetMin = new Vector2(12f, 12f);
        taRt.offsetMax = new Vector2(-12f, -12f);
        textArea.AddComponent<RectMask2D>();

        GameObject ph = new GameObject("Placeholder");
        ph.transform.SetParent(textArea.transform, false);
        RectTransform phRt = ph.AddComponent<RectTransform>();
        phRt.anchorMin = Vector2.zero;
        phRt.anchorMax = Vector2.one;
        phRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmpPh = ph.AddComponent<TextMeshProUGUI>();
        tmpPh.text = placeholderText;
        tmpPh.fontSize = 17;
        tmpPh.color = new Color(0.6f, 0.7f, 0.8f, 0.6f);
        tmpPh.fontStyle = FontStyles.Italic;
        tmpPh.textWrappingMode = TextWrappingModes.Normal;

        GameObject text = new GameObject("Text");
        text.transform.SetParent(textArea.transform, false);
        RectTransform textRt = text.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmpText = text.AddComponent<TextMeshProUGUI>();
        tmpText.fontSize = 18;
        tmpText.color = Color.white;
        tmpText.textWrappingMode = TextWrappingModes.Normal;

        TMP_InputField input = go.AddComponent<TMP_InputField>();
        input.textViewport = taRt;
        input.textComponent = tmpText;
        input.placeholder = tmpPh;
        input.lineType = TMP_InputField.LineType.MultiLineNewline;
        input.caretColor = Color.white;
        input.selectionColor = new Color(0.3f, 0.6f, 0.9f, 0.5f);

        return input;
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
        btn.onClick.AddListener(() => onClick?.Invoke());

        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(go.transform, false);
        RectTransform lrt = labelGo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        return btn;
    }
}
