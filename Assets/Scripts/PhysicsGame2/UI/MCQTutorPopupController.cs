using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    public enum MCQTutorMode
    {
        Hint,    // Đang làm bài: Gợi ý tư duy, hướng giải, không tiết lộ đáp án A/B/C/D
        Explain  // Xem lời giải: Giải thích chi tiết lý do đúng/sai, phân tích mẹo nhớ
    }

    [Serializable]
    public class MCQQuestionContext
    {
        public string AttemptId = "";
        public string QuestionId = "";
        public int QuestionNumber = 1;
        public int TotalQuestions = 10;
        public string QuestionText = "";
        public List<string> Options = new List<string>();
        public int SelectedAnswerIndex = -1; // -1 nếu chưa chọn
        public int CorrectAnswerIndex = -1;  // Chỉ dùng khi Explain
        public string Explanation = "";
        public string KeyKnowledge = "";
        public string LessonId = "";
        public string Difficulty = "";
    }

    [Serializable]
    public class MCQTutorChatMessage
    {
        public string role; // "student" | "assistant" | "system"
        public string text;
        public float timestamp;
    }

    [Serializable]
    public class MCQTutorApiResponse
    {
        public bool success;
        public string cauHoiId;
        public string mode;
        public string tutorReply;
        public string[] suggestedPrompts;
        public float durationSeconds;
        public string source;
        public string error;
    }

    /// <summary>
    /// Bộ điều khiển Popup Gia sư AI Trắc nghiệm Hoàng gia Aurelia.
    /// Kiến trúc chuyên nghiệp: Nạp Prefab chuẩn "UI/MCQTutorPopup" từ Resources,
    /// tự động bind sự kiện/dữ liệu lên UI, quản lý hội thoại và kết nối Gemini Backend.
    /// Tính toán layout toán học chính xác cho từng hàng tin nhắn, triệt tiêu hoàn toàn lỗi chồng chữ.
    /// </summary>
    public class MCQTutorPopupController : MonoBehaviour
    {
        private const string BACKEND_URL = "http://127.0.0.1:5050/api/mcq_tutor_chat";
        private const string PREFAB_PATH = "UI/MCQTutorPopup";

        private static MCQTutorPopupController _instance;

        // UI references
        private GameObject _modalRoot;
        private CanvasGroup _canvasGroup;
        private TextMeshProUGUI _headerTitleText;
        private TextMeshProUGUI _headerSubtitleText;
        private TextMeshProUGUI _contextBadgeText;
        private ScrollRect _chatScrollRect;
        private RectTransform _chatContentRect;
        private Transform _quickPromptsContainer;
        private TMP_InputField _inputField;
        private Button _btnSend;
        private Button _btnClose;
        private Button _btnCloseTopX;
        private TMP_FontAsset _defaultFont;

        [Header("=== FANTASY BUBBLE SPRITES ===")]
        [SerializeField] private Sprite aiBubbleSprite;
        [SerializeField] private Sprite studentBubbleSprite;

        private Sprite _aiBubbleFrameSprite;
        private Sprite _aiBookIconSprite;
        private Sprite _studentBubbleFrameSprite;
        private Sprite _studentAvatarSprite;

        // State
        private MCQTutorMode _currentMode = MCQTutorMode.Hint;
        private MCQQuestionContext _currentContext;
        private bool _isWaitingForAi = false;
        private string _activeRequestKey = "";

        // Chat History map: key = AttemptId_QuestionId_Mode
        private readonly Dictionary<string, List<MCQTutorChatMessage>> _chatHistories = new Dictionary<string, List<MCQTutorChatMessage>>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureBubbleSprites();
        }

        private void EnsureBubbleSprites()
        {
#if UNITY_EDITOR
            if (aiBubbleSprite == null || studentBubbleSprite == null)
            {
                var allSprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Environment/Bộ sprite giao diện Aurelia fantasy.png");
                foreach (var asset in allSprites)
                {
                    if (asset is Sprite s)
                    {
                        if (s.name.EndsWith("_13") && aiBubbleSprite == null) aiBubbleSprite = s;
                        else if (s.name.EndsWith("_14") && studentBubbleSprite == null) studentBubbleSprite = s;
                    }
                }
            }
#endif

            if (aiBubbleSprite == null || studentBubbleSprite == null)
            {
                Sprite[] loadedSprites = Resources.FindObjectsOfTypeAll<Sprite>();
                foreach (var s in loadedSprites)
                {
                    if (s == null) continue;
                    if (aiBubbleSprite == null && (s.name.EndsWith("_13") || s.name.Contains("fantasy_13")))
                    {
                        aiBubbleSprite = s;
                    }
                    if (studentBubbleSprite == null && (s.name.EndsWith("_14") || s.name.Contains("fantasy_14")))
                    {
                        studentBubbleSprite = s;
                    }
                }
            }

            // Tách các sprite thành phần chuyên biệt để triệt tiêu hoàn toàn hiện tượng kéo giãn icon
            if (_aiBubbleFrameSprite == null || _aiBookIconSprite == null || _studentBubbleFrameSprite == null || _studentAvatarSprite == null)
            {
                Texture2D atlasTex = (aiBubbleSprite != null) ? aiBubbleSprite.texture : ((studentBubbleSprite != null) ? studentBubbleSprite.texture : null);
                if (atlasTex != null)
                {
                    try
                    {
                        // 1. AI Book Icon: Biểu tượng sách phát sáng (x=21, y=331, w=120, h=159), tỉ lệ 1:1, không stretch
                        if (_aiBookIconSprite == null)
                        {
                            _aiBookIconSprite = Sprite.Create(atlasTex, new Rect(21f, 331f, 120f, 159f), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, Vector4.zero);
                            _aiBookIconSprite.name = "Aurelia_AIBookIcon";
                        }

                        // 2. AI Bubble Frame: Khung xanh hoàng gia 9-slice không chứa icon (x=141, y=331, w=728, h=159)
                        // Border: Left=36, Bottom=36, Right=48, Top=36
                        if (_aiBubbleFrameSprite == null)
                        {
                            _aiBubbleFrameSprite = Sprite.Create(atlasTex, new Rect(141f, 331f, 714f, 159f), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(36f, 36f, 48f, 36f));
                            _aiBubbleFrameSprite.name = "Aurelia_AIBubbleFrame";
                        }

                        // 3. Student Bubble Frame: Khung giấy vàng 9-slice không chứa avatar (x=14, y=195, w=718, h=141)
                        // Border: Left=48, Bottom=32, Right=40, Top=58
                        if (_studentBubbleFrameSprite == null)
                        {
                            _studentBubbleFrameSprite = Sprite.Create(atlasTex, new Rect(14f, 195f, 718f, 141f), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(48f, 32f, 40f, 58f));
                            _studentBubbleFrameSprite.name = "Aurelia_StudentBubbleFrame";
                        }

                        // 4. Student Avatar: Icon avatar tròn học sinh (x=739, y=195, w=110, h=141), tỉ lệ 1:1, không stretch
                        if (_studentAvatarSprite == null)
                        {
                            _studentAvatarSprite = Sprite.Create(atlasTex, new Rect(732f, 195f, 117f, 141f), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, Vector4.zero);
                            _studentAvatarSprite.name = "Aurelia_StudentAvatar";
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[MCQTutor] Không thể tạo sub-sprites từ atlas: {ex.Message}");
                    }
                }
            }
        }

        public static MCQTutorPopupController GetOrCreate()
        {
            if (_instance != null) return _instance;

            var existing = FindAnyObjectByType<MCQTutorPopupController>();
            if (existing != null)
            {
                _instance = existing;
                return _instance;
            }

            GameObject go = new GameObject("Aurelia_MCQTutorPopupController");
            _instance = go.AddComponent<MCQTutorPopupController>();
            return _instance;
        }

        /// <summary>
        /// Tự động chẩn đoán và phục hồi mảng Selectable tĩnh của UGUI khi bị lệch trạng thái hoặc tràn mảng,
        /// triệt tiêu tận gốc lỗi IndexOutOfRangeException tại Selectable.OnEnable() gây đơ toàn bộ nút và input.
        /// </summary>
        public static void EnsureSelectablesHealthy()
        {
            try
            {
                var selectableType = typeof(Selectable);
                var sSelectablesField = selectableType.GetField("s_Selectables", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                var sSelectableCountField = selectableType.GetField("s_SelectableCount", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                var mCurrentIndexField = selectableType.GetField("m_CurrentIndex", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var mEnableCalledField = selectableType.GetField("m_EnableCalled", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                if (sSelectablesField != null && sSelectableCountField != null)
                {
                    Selectable[] currentArr = (Selectable[])sSelectablesField.GetValue(null);
                    int currentCount = (int)sSelectableCountField.GetValue(null);

                    var allActive = UnityEngine.Object.FindObjectsByType<Selectable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

                    bool needsFix = false;
                    if (currentArr == null || currentCount < 0 || currentCount > (currentArr?.Length ?? 0) || currentCount != allActive.Length)
                    {
                        needsFix = true;
                    }
                    else
                    {
                        for (int i = 0; i < currentCount; i++)
                        {
                            if (currentArr[i] == null || !currentArr[i].isActiveAndEnabled)
                            {
                                needsFix = true;
                                break;
                            }
                        }
                    }

                    if (needsFix)
                    {
                        int newCapacity = Mathf.Max(64, (allActive.Length + 8) * 2);
                        Selectable[] newArr = new Selectable[newCapacity];
                        for (int i = 0; i < allActive.Length; i++)
                        {
                            newArr[i] = allActive[i];
                            if (mCurrentIndexField != null)
                            {
                                mCurrentIndexField.SetValue(allActive[i], i);
                            }
                            if (mEnableCalledField != null)
                            {
                                mEnableCalledField.SetValue(allActive[i], true);
                            }
                        }
                        sSelectablesField.SetValue(null, newArr);
                        sSelectableCountField.SetValue(null, allActive.Length);
                        Debug.Log($"[MCQTutor] Đã tự động phục hồi UGUI Selectables: {allActive.Length} đối tượng hợp lệ (dung lượng mảng {newCapacity}).");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MCQTutor] EnsureSelectablesHealthy warning: {ex.Message}");
            }
        }

        private static void EnsureEventSystem(Canvas canvas)
        {
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                var existing = UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
                if (existing != null)
                {
                    UnityEngine.EventSystems.EventSystem.current = existing;
                }
                else
                {
                    GameObject esGo = new GameObject("EventSystem_AutoCreated");
                    var es = esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                    UnityEngine.EventSystems.EventSystem.current = es;
                    Debug.Log("[MCQTutor] Đã tự động tạo EventSystem cho Scene!");
                }
            }

            if (canvas != null && canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
                Debug.Log("[MCQTutor] Đã bổ sung GraphicRaycaster vào Canvas!");
            }
        }

        /// <summary>
        /// Mở popup Gia sư AI cho câu hỏi trắc nghiệm hiện tại.
        /// </summary>
        public void OpenTutor(MCQTutorMode mode, MCQQuestionContext context)
        {
            EnsureSelectablesHealthy();
            _currentMode = mode;
            _currentContext = context;
            EnsureUI();
            if (_modalRoot != null)
            {
                AutoBindUI(_modalRoot);
            }

            if (_modalRoot != null)
            {
                _modalRoot.SetActive(true);
                _modalRoot.transform.SetAsLastSibling();
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 1f;
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                }
                EnsureSelectablesHealthy();
            }

            // 1. Cập nhật Khu vực A: Tiêu đề & Phụ đề
            if (_headerTitleText != null)
            {
                _headerTitleText.text = "GIA SƯ AI AURELIA";
                _headerTitleText.fontSize = 38f;
                _headerTitleText.color = new Color(1f, 0.94f, 0.70f, 1f);
            }
            if (_headerSubtitleText != null)
            {
                _headerSubtitleText.fontSize = 20f;
                _headerSubtitleText.color = new Color(0.78f, 0.90f, 1f, 1f);
                _headerSubtitleText.text = (mode == MCQTutorMode.Hint)
                    ? "Người đồng hành Vật lý 9 • Hướng dẫn tư duy bài thi"
                    : "Người đồng hành Vật lý 9 • Giải thích chi tiết & mẹo ghi nhớ";
            }

            // 2. Cập nhật Khu vực B: Thẻ ngữ cảnh câu hỏi hiện tại (2-4 dòng rộng rãi, font 20pt)
            if (_contextBadgeText != null && context != null)
            {
                string modeTag = (mode == MCQTutorMode.Hint)
                    ? "<color=#FFE57F>[GỢI Ý TƯ DUY]</color>"
                    : "<color=#80D8FF>[GIẢI THÍCH CHI TIẾT]</color>";

                string diffName = "";
                if (!string.IsNullOrEmpty(context.Difficulty))
                {
                    string d = context.Difficulty.Trim().ToUpperInvariant();
                    if (d == "NB" || d == "NHAN_BIET") diffName = "Nhận biết";
                    else if (d == "TH" || d == "THONG_HIEU") diffName = "Thông hiểu";
                    else if (d == "VD" || d == "VAN_DUNG") diffName = "Vận dụng";
                    else if (d == "VDC" || d == "VAN_DUNG_CAO") diffName = "Vận dụng cao";
                    else diffName = context.Difficulty;
                }
                string diffStr = !string.IsNullOrEmpty(diffName)
                    ? $" • <color=#B388FF>[{diffName}]</color>"
                    : "";

                string qNumStr = $"<color=#FFE57F><b>CÂU {context.QuestionNumber}/{context.TotalQuestions}</b></color>{diffStr}";

                string extraContext = "";
                if (mode == MCQTutorMode.Explain)
                {
                    string userChoice = (context.SelectedAnswerIndex >= 0 && context.SelectedAnswerIndex < context.Options.Count)
                        ? context.Options[context.SelectedAnswerIndex] : "(Chưa chọn)";
                    string correctChoice = (context.CorrectAnswerIndex >= 0 && context.CorrectAnswerIndex < context.Options.Count)
                        ? context.Options[context.CorrectAnswerIndex] : "(Chưa rõ)";
                    extraContext = $"\n<size=18><color=#90CAF9>Em chọn: {TruncateText(userChoice, 45)}</color>  |  <color=#A5D6A7>Đáp án đúng: {TruncateText(correctChoice, 45)}</color></size>";
                }

                _contextBadgeText.text = $"{qNumStr} {modeTag}: {context.QuestionText}{extraContext}";
            }

            // 3. Nạp hoặc khởi tạo lịch sử trao đổi của câu hỏi này
            string key = GetHistoryKey(context);
            if (!_chatHistories.ContainsKey(key))
            {
                _chatHistories[key] = new List<MCQTutorChatMessage>();

                string initialGreeting = (mode == MCQTutorMode.Hint)
                    ? $"Chào em! Thầy/Cô là <b>Gia sư AI môn Vật lý 9</b> của Học viện Aurelia. Em đang làm <b>Câu {context.QuestionNumber}</b>. Thầy/Cô sẽ gợi ý hướng suy luận và các công thức liên quan để em tự tìm ra đáp án đúng nhé!"
                    : $"Chào em! Thầy/Cô là <b>Gia sư AI môn Vật lý 9</b>. Em đang xem lại lời giải <b>Câu {context.QuestionNumber}</b>. Thầy/Cô sẽ giúp em phân tích bản chất hiện tượng và chia sẻ mẹo ghi nhớ sâu để không bị nhầm lẫn nhé!";

                _chatHistories[key].Add(new MCQTutorChatMessage
                {
                    role = "assistant",
                    text = initialGreeting,
                    timestamp = Time.time
                });
            }

            // 4. Render lại toàn bộ bong bóng chat (Khu vực C)
            RenderChatHistory();

            // 5. Cập nhật các nút gợi ý nhanh (Khu vực D)
            SetupQuickPrompts();

            // 6. Reset và focus ô nhập liệu
            if (_inputField != null)
            {
                _inputField.text = "";
                _inputField.interactable = true;
                _inputField.ActivateInputField();
                _inputField.Select();
            }
            if (_btnSend != null) _btnSend.interactable = true;
        }

        public void CloseTutor()
        {
            Debug.Log("[MCQ Tutor] Close Clicked");
            if (_modalRoot != null)
            {
                _modalRoot.SetActive(false);
            }
        }

        private string GetHistoryKey(MCQQuestionContext context)
        {
            if (context == null) return "global_mcq";
            string attId = string.IsNullOrEmpty(context.AttemptId) ? "attempt" : context.AttemptId;
            string qId = string.IsNullOrEmpty(context.QuestionId) ? $"Q_{context.QuestionNumber}" : context.QuestionId;
            string modeStr = (_currentMode == MCQTutorMode.Hint) ? "Hint" : "Explain";
            return $"{attId}_{qId}_{modeStr}";
        }

        private void SetupQuickPrompts()
        {
            if (_quickPromptsContainer == null) return;

            foreach (Transform child in _quickPromptsContainer)
            {
                Destroy(child.gameObject);
            }

            List<string> prompts;
            if (_currentMode == MCQTutorMode.Hint)
            {
                prompts = new List<string>
                {
                    "💡 Gợi ý cách làm",
                    "📐 Cần dùng công thức nào?",
                    "🚀 Bắt đầu từ đâu?",
                    "🔍 Phân tích đề bài dễ hơn"
                };
            }
            else
            {
                prompts = new List<string>
                {
                    "❌ Vì sao em chọn sai?",
                    "📖 Giải thích chi tiết",
                    "🧠 Có mẹo ghi nhớ không?",
                    "🌟 Cho ví dụ tương tự"
                };
            }

            foreach (var p in prompts)
            {
                CreatePromptChip(p);
            }
        }

        private void CreatePromptChip(string promptText)
        {
            GameObject chipGo = new GameObject("Chip_" + promptText);
            chipGo.transform.SetParent(_quickPromptsContainer, false);

            RectTransform rt = chipGo.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, 38f);

            Image bg = chipGo.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.12f, 0.24f, 0.95f); // Deep Royal Navy
            bg.raycastTarget = true;

            Outline outline = chipGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 0.85f); // Gold Aurelia
            outline.effectDistance = new Vector2(1.2f, -1.2f);

            Button btn = chipGo.AddComponent<Button>();
            btn.targetGraphic = bg;
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
            cb.pressedColor = new Color(0.80f, 0.80f, 0.80f, 1f);
            btn.colors = cb;

            HorizontalLayoutGroup hlg = chipGo.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 6, 6);
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            ContentSizeFitter csf = chipGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject txtGo = new GameObject("Text");
            txtGo.transform.SetParent(chipGo.transform, false);
            TextMeshProUGUI tmp = txtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) tmp.font = _defaultFont;
            tmp.text = promptText;
            tmp.fontSize = 16f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(1f, 0.95f, 0.82f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            btn.onClick.AddListener(() =>
            {
                Debug.Log($"[MCQ Tutor] Quick Prompt Clicked: '{promptText}'");
                OnQuickPromptClicked(promptText);
            });
        }

        private void OnQuickPromptClicked(string promptText)
        {
            if (_isWaitingForAi) return;
            SendStudentMessage(promptText);
        }

        public void SendStudentMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message) || _isWaitingForAi || _currentContext == null) return;

            string key = GetHistoryKey(_currentContext);
            if (!_chatHistories.ContainsKey(key))
            {
                _chatHistories[key] = new List<MCQTutorChatMessage>();
            }

            // 1. Thêm tin nhắn của học sinh vào lịch sử (role = "user")
            _chatHistories[key].Add(new MCQTutorChatMessage
            {
                role = "user",
                text = message.Trim(),
                timestamp = Time.time
            });

            // 2. Render lại bong bóng
            RenderChatHistory();

            // 3. Clear ô nhập và tạm khóa
            if (_inputField != null)
            {
                _inputField.text = "";
                _inputField.interactable = false;
            }
            if (_btnSend != null) _btnSend.interactable = false;

            // 4. Gửi request bất đồng bộ tới backend Gemini
            StartCoroutine(SendChatToBackendRoutine(key, _currentContext, message.Trim()));
        }

        private IEnumerator SendChatToBackendRoutine(string historyKey, MCQQuestionContext context, string studentMessage)
        {
            _isWaitingForAi = true;
            string requestId = Guid.NewGuid().ToString();
            _activeRequestKey = requestId;

            // Hiển thị bong bóng AI đang suy nghĩ
            GameObject thinkingBubble = CreateBubble(false, "⏳ <i>Gia sư AI Aurelia đang đọc dữ kiện và soạn hướng dẫn...</i>", true);

            // Scroll xuống đáy
            Canvas.ForceUpdateCanvases();
            if (_chatScrollRect != null) _chatScrollRect.verticalNormalizedPosition = 0f;

            // Chuẩn bị dữ liệu gửi đi
            string modeStr = (_currentMode == MCQTutorMode.Hint) ? "HINT" : "EXPLAIN";
            string studentChoiceStr = "";
            if (context.SelectedAnswerIndex >= 0 && context.SelectedAnswerIndex < context.Options.Count)
            {
                studentChoiceStr = context.Options[context.SelectedAnswerIndex];
            }
            string correctChoiceStr = "";
            if (context.CorrectAnswerIndex >= 0 && context.CorrectAnswerIndex < context.Options.Count)
            {
                correctChoiceStr = context.Options[context.CorrectAnswerIndex];
            }

            List<MCQTutorChatMessage> histList = _chatHistories.ContainsKey(historyKey) ? _chatHistories[historyKey] : new List<MCQTutorChatMessage>();
            var historyPayload = histList.TakeLast(6).Select(m => new { role = m.role, text = m.text }).ToList();

            var payloadObj = new
            {
                mode = modeStr,
                cauHoiId = context.QuestionId,
                questionText = context.QuestionText,
                options = context.Options,
                studentChoice = (_currentMode == MCQTutorMode.Explain) ? studentChoiceStr : "",
                correctChoice = (_currentMode == MCQTutorMode.Explain) ? correctChoiceStr : "",
                explanation = context.Explanation,
                keyKnowledge = context.KeyKnowledge,
                studentMessage = studentMessage,
                chatHistory = historyPayload
            };

            string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(payloadObj);

            using (UnityWebRequest www = new UnityWebRequest(BACKEND_URL, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.timeout = 25;

                yield return www.SendWebRequest();

                // Xóa bong bóng thinking
                if (thinkingBubble != null) Destroy(thinkingBubble);

                string aiReply = "";
                List<string> newPrompts = new List<string>();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string respText = www.downloadHandler.text;
                        var apiResp = JsonUtility.FromJson<MCQTutorApiResponse>(respText);
                        if (apiResp != null && !string.IsNullOrEmpty(apiResp.tutorReply))
                        {
                            aiReply = apiResp.tutorReply;
                            if (apiResp.suggestedPrompts != null && apiResp.suggestedPrompts.Length > 0)
                            {
                                newPrompts.AddRange(apiResp.suggestedPrompts);
                            }
                        }
                        else
                        {
                            aiReply = ExtractTutorReplyManual(respText);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[MCQTutor] Lỗi parse JSON response: {ex}");
                        aiReply = ExtractTutorReplyManual(www.downloadHandler.text);
                    }
                }
                else
                {
                    string errMsg = string.IsNullOrEmpty(www.error) ? "Không thể kết nối máy chủ port 5050" : www.error;
                    if (errMsg.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        errMsg = "Yêu cầu phản hồi quá thời gian từ Gemini (Request timeout).";
                    }
                    Debug.LogWarning($"[MCQTutor] Backend không phản hồi ({errMsg}). Tự động kích hoạt phản hồi sư phạm chuẩn.");

                    string fallbackAdvice;
                    if (_currentMode == MCQTutorMode.Hint)
                    {
                        fallbackAdvice = "Thầy/Cô khuyên em hãy đọc kỹ dữ kiện đề bài: xác định đại lượng đã cho và đại lượng cần tìm. Chú ý vận dụng các công thức định luật trong bài học và đối chiếu đơn vị chuẩn trước khi chọn phương án nhé!";
                    }
                    else
                    {
                        fallbackAdvice = $"Phương án đúng là {correctChoiceStr}.\n{context.Explanation}";
                        if (!string.IsNullOrEmpty(context.KeyKnowledge))
                        {
                            fallbackAdvice += $"\n\n💡 <b>Mẹo ghi nhớ:</b> {context.KeyKnowledge}";
                        }
                    }

                    aiReply = $"⚠ <b>Không thể kết nối máy chủ AI (Port 5050):</b> {errMsg}.\n<i>Hệ thống tự động chuyển sang chế độ hướng dẫn ngoại tuyến:</i>\n\n{fallbackAdvice}";
                }

                if (string.IsNullOrEmpty(aiReply))
                {
                    aiReply = "Thầy/Cô đang kết nối lại với hệ thống dữ liệu, em hãy thử hỏi lại nhé!";
                }

                // Lưu vào lịch sử chat của câu hỏi này
                if (_chatHistories.ContainsKey(historyKey))
                {
                    _chatHistories[historyKey].Add(new MCQTutorChatMessage
                    {
                        role = "assistant",
                        text = aiReply,
                        timestamp = Time.time
                    });
                }

                _isWaitingForAi = false;

                // An toàn bất đồng bộ: chỉ render nếu người học VẪN ĐANG ở cùng câu hỏi này
                if (_currentContext != null && GetHistoryKey(_currentContext) == historyKey)
                {
                    RenderChatHistory();
                    if (newPrompts.Count > 0)
                    {
                        UpdateQuickPromptChips(newPrompts);
                    }
                    if (_inputField != null)
                    {
                        _inputField.interactable = true;
                        _inputField.ActivateInputField();
                    }
                    if (_btnSend != null) _btnSend.interactable = true;
                }
            }
        }

        private void UpdateQuickPromptChips(List<string> prompts)
        {
            if (_quickPromptsContainer == null) return;
            foreach (Transform child in _quickPromptsContainer)
            {
                Destroy(child.gameObject);
            }
            foreach (var p in prompts.Take(4))
            {
                CreatePromptChip(p);
            }
        }

        private void RenderChatHistory()
        {
            if (_chatContentRect == null) return;

            // Xóa sạch các bong bóng cũ ngay lập tức
            for (int i = _chatContentRect.childCount - 1; i >= 0; i--)
            {
                var child = _chatContentRect.GetChild(i).gameObject;
                child.transform.SetParent(null);
                DestroyImmediate(child);
            }

            _chatContentRect.anchoredPosition = Vector2.zero;

            if (_currentContext == null) return;
            string key = GetHistoryKey(_currentContext);
            if (!_chatHistories.ContainsKey(key)) return;

            var msgs = _chatHistories[key];
            foreach (var m in msgs)
            {
                bool isStudent = (m.role == "student" || m.role == "user");
                bool isError = m.text != null && m.text.StartsWith("⚠");
                CreateBubble(isStudent, m.text, false, isError);
            }

            Canvas.ForceUpdateCanvases();
            StartCoroutine(ScrollToBottomRoutine());
        }

        private IEnumerator ScrollToBottomRoutine()
        {
            yield return null;
            if (_chatScrollRect != null)
            {
                _chatScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        /// <summary>
        /// Tạo bong bóng chat 2 chiều fantasy chuẩn mực như Gia sư AI Tự luận:
        /// - Gia sư AI: Căn TRÁI (anchorMin=0, anchorMax=0, pivot=0, x=12), nền sprite _13 hoàng gia sliced,
        ///   padLeft=140px vượt qua icon sách, Header "✦ Gia sư AI Aurelia" vàng kim (#FFD700), body trắng (#EAF5FF).
        /// - Học sinh: Căn PHẢI (anchorMin=1, anchorMax=1, pivot=1, x=-12), nền sprite _14 da cừu vàng sliced,
        ///   padRight=160px tránh avatar icon, padTop=54px dưới thẻ tên vàng.
        ///   Thẻ tên "NameTag_Student" ghim cố định (-208px, -31px) ignoreLayout=true, nhãn "Em (Học sinh)" màu mực nâu nghệ thuật fantasy.
        ///   Nội dung text màu nâu sẫm (#1F170D) sắc nét trên nền giấy vàng.
        /// - Tính toán chiều cao tự động theo text, minHeight giữ trọn vẹn icon (140px AI, 135px Học sinh).
        /// </summary>
        private GameObject CreateBubble(bool isStudent, string message, bool isThinking = false, bool isError = false)
        {
            if (_chatContentRect == null) return null;
            EnsureBubbleSprites();

            bool isAi = !isStudent;

            // Kích thước chuẩn tương xứng bề rộng chat 1340px (giống hệ thống chat Tự luận)
            float bubbleWidth = isAi ? 980f : 940f;
            float padLeft = isAi ? 145f : 38f;  // 145px cho AI để hoàn toàn vượt qua biểu tượng quyển sách bên trái của sprite
            float padRight = isAi ? 38f : 150f; // 150px cho Học sinh để hoàn toàn tránh avatar icon bên phải của sprite
            float padTop = isAi ? 22f : 54f;    // 54px cho Học sinh để text bắt đầu HOÀN TOÀN DƯỚI thẻ tên vàng
            float padBottom = isAi ? 24f : 24f;
            float spacing = isAi ? 8f : 0f;
            float textWidth = bubbleWidth - padLeft - padRight;

            // 1. Tạo MessageRow trong Content của ScrollView
            GameObject rowGo = new GameObject(isStudent ? "StudentRow" : "AiRow");
            rowGo.transform.SetParent(_chatContentRect, false);
            RectTransform rowRt = rowGo.AddComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0f, 1f);
            rowRt.anchorMax = new Vector2(1f, 1f);
            rowRt.pivot = new Vector2(0.5f, 1f);

            // 2. Tạo Bubble Container bên trong Row (Định vị độc lập chuẩn xác qua RectTransform Anchors)
            GameObject bubbleGo = new GameObject(isAi ? "AiBubble" : "StudentBubble");
            bubbleGo.transform.SetParent(rowGo.transform, false);
            RectTransform bRt = bubbleGo.AddComponent<RectTransform>();

            if (isAi)
            {
                bRt.anchorMin = new Vector2(0f, 1f);
                bRt.anchorMax = new Vector2(0f, 1f);
                bRt.pivot = new Vector2(0f, 1f);
                bRt.anchoredPosition = new Vector2(12f, 0f);
            }
            else
            {
                bRt.anchorMin = new Vector2(1f, 1f);
                bRt.anchorMax = new Vector2(1f, 1f);
                bRt.pivot = new Vector2(1f, 1f);
                bRt.anchoredPosition = new Vector2(-12f, 0f);
            }

            Image bubbleBg = bubbleGo.AddComponent<Image>();
            bubbleBg.raycastTarget = false;
            bubbleBg.maskable = true; // Bắt buộc maskable để stencil clipping hoàn toàn trong Viewport

            if (isAi)
            {
                if (_aiBubbleFrameSprite != null)
                {
                    bubbleBg.sprite = _aiBubbleFrameSprite;
                    bubbleBg.type = Image.Type.Sliced;
                    bubbleBg.color = Color.white;
                }
                else if (aiBubbleSprite != null)
                {
                    bubbleBg.sprite = aiBubbleSprite;
                    bubbleBg.type = Image.Type.Sliced;
                    bubbleBg.color = Color.white;
                }
                else
                {
                    bubbleBg.color = isError
                        ? new Color(0.24f, 0.08f, 0.10f, 0.96f)
                        : (isThinking ? new Color(0.08f, 0.14f, 0.24f, 0.92f) : new Color(0.06f, 0.13f, 0.25f, 0.96f));
                    var ol = bubbleGo.AddComponent<Outline>();
                    ol.effectColor = isError ? new Color(0.95f, 0.40f, 0.40f, 0.90f) : new Color(0.96f, 0.82f, 0.38f, 0.90f);
                    ol.effectDistance = new Vector2(1.2f, -1.2f);
                }

                // Biểu tượng sách phát sáng độc lập: Giữ nguyên tỉ lệ 1:1, không bao giờ bị kéo giãn theo chiều dọc khi text dài
                if (_aiBookIconSprite != null)
                {
                    GameObject bookGo = new GameObject("Icon_AiBook");
                    bookGo.transform.SetParent(bubbleGo.transform, false);
                    RectTransform bookRt = bookGo.AddComponent<RectTransform>();
                    bookRt.anchorMin = new Vector2(0f, 1f);
                    bookRt.anchorMax = new Vector2(0f, 1f);
                    bookRt.pivot = new Vector2(0.5f, 0.5f);
                    bookRt.anchoredPosition = new Vector2(72f, -70f);
                    bookRt.sizeDelta = new Vector2(100f, 132f);

                    Image bookImg = bookGo.AddComponent<Image>();
                    bookImg.sprite = _aiBookIconSprite;
                    bookImg.preserveAspect = true;
                    bookImg.raycastTarget = false;
                    bookImg.maskable = true;

                    LayoutElement bookLe = bookGo.AddComponent<LayoutElement>();
                    bookLe.ignoreLayout = true;
                }
            }
            else
            {
                if (_studentBubbleFrameSprite != null)
                {
                    bubbleBg.sprite = _studentBubbleFrameSprite;
                    bubbleBg.type = Image.Type.Sliced;
                    bubbleBg.color = Color.white;

                    // Biểu tượng avatar học sinh độc lập: Tỉ lệ 1:1, không bị co giãn
                    if (_studentAvatarSprite != null)
                    {
                        GameObject avaGo = new GameObject("Icon_StudentAvatar");
                        avaGo.transform.SetParent(bubbleGo.transform, false);
                        RectTransform avaRt = avaGo.AddComponent<RectTransform>();
                        avaRt.anchorMin = new Vector2(1f, 1f);
                        avaRt.anchorMax = new Vector2(1f, 1f);
                        avaRt.pivot = new Vector2(0.5f, 0.5f);
                        avaRt.anchoredPosition = new Vector2(-68f, -68f);
                        avaRt.sizeDelta = new Vector2(95f, 120f);

                        Image avaImg = avaGo.AddComponent<Image>();
                        avaImg.sprite = _studentAvatarSprite;
                        avaImg.preserveAspect = true;
                        avaImg.raycastTarget = false;
                        avaImg.maskable = true;

                        LayoutElement avaLe = avaGo.AddComponent<LayoutElement>();
                        avaLe.ignoreLayout = true;
                    }
                }
                else if (studentBubbleSprite != null)
                {
                    bubbleBg.sprite = studentBubbleSprite;
                    bubbleBg.type = Image.Type.Sliced;
                    bubbleBg.color = Color.white;
                }
                else
                {
                    bubbleBg.color = new Color(0.96f, 0.82f, 0.38f, 0.95f);
                    var ol = bubbleGo.AddComponent<Outline>();
                    ol.effectColor = new Color(0.85f, 0.68f, 0.32f, 0.85f);
                    ol.effectDistance = new Vector2(1.2f, -1.2f);
                }
            }

            VerticalLayoutGroup vlg = bubbleGo.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset((int)padLeft, (int)padRight, (int)padTop, (int)padBottom);
            vlg.spacing = spacing;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            float headerHeight = 0f;

            // 3. Tiêu đề người gửi
            if (isAi)
            {
                GameObject hdrGo = new GameObject("Header");
                hdrGo.transform.SetParent(bubbleGo.transform, false);
                var hdrTmp = hdrGo.AddComponent<TextMeshProUGUI>();
                if (_defaultFont != null) hdrTmp.font = _defaultFont;
                hdrTmp.fontSize = 22f;
                hdrTmp.fontStyle = FontStyles.Bold;
                hdrTmp.raycastTarget = false;
                hdrTmp.maskable = true;
                hdrTmp.alignment = TextAlignmentOptions.TopLeft;

                if (isError)
                {
                    hdrTmp.text = "⚠ Gia sư AI Aurelia (Lỗi kết nối)";
                    hdrTmp.color = new Color(1f, 0.50f, 0.50f, 1f);
                }
                else if (isThinking)
                {
                    hdrTmp.text = "⏳ Gia sư AI Aurelia";
                    hdrTmp.color = new Color(0.60f, 0.88f, 1f, 1f);
                }
                else
                {
                    hdrTmp.text = "✦ Gia sư AI Aurelia";
                    hdrTmp.color = new Color(1f, 0.84f, 0f, 1f); // #FFD700
                }

                Vector2 hdrPref = hdrTmp.GetPreferredValues(hdrTmp.text, textWidth, float.PositiveInfinity);
                headerHeight = Mathf.Max(26f, hdrPref.y);
                LayoutElement hdrLe = hdrGo.AddComponent<LayoutElement>();
                hdrLe.preferredHeight = headerHeight;
                hdrLe.minHeight = headerHeight;
            }
            else
            {
                // Thẻ tên học sinh ở góc trên bên phải của sprite fantasy_14 (giống Tự luận)
                GameObject tagGo = new GameObject("NameTag_Student");
                tagGo.transform.SetParent(bubbleGo.transform, false);

                RectTransform tagRt = tagGo.AddComponent<RectTransform>();
                tagRt.anchorMin = new Vector2(1f, 1f);
                tagRt.anchorMax = new Vector2(1f, 1f);
                tagRt.pivot = new Vector2(0.5f, 0.5f);
                tagRt.anchoredPosition = new Vector2(-208f, -31f);
                tagRt.sizeDelta = new Vector2(140f, 36f);

                Image tagImg = tagGo.AddComponent<Image>();
                tagImg.color = Color.clear;
                tagImg.raycastTarget = false;
                tagImg.maskable = true;

                LayoutElement tagLe = tagGo.AddComponent<LayoutElement>();
                tagLe.ignoreLayout = true;

                GameObject tagTextGo = new GameObject("Text_StudentSender");
                tagTextGo.transform.SetParent(tagGo.transform, false);
                RectTransform textRt = tagTextGo.AddComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;

                var tagTmp = tagTextGo.AddComponent<TextMeshProUGUI>();
                if (_defaultFont != null) tagTmp.font = _defaultFont;
                tagTmp.fontSize = 22f;
                tagTmp.fontStyle = FontStyles.Bold;
                tagTmp.color = new Color(0.20f, 0.12f, 0.04f, 1f); // Màu mực nâu nghệ thuật trên nền thẻ vàng
                tagTmp.text = "Em (Học sinh)";
                tagTmp.alignment = TextAlignmentOptions.Center;
                tagTmp.raycastTarget = false;
                tagTmp.maskable = true;
            }

            // 4. Nội dung tin nhắn
            GameObject msgGo = new GameObject("Message");
            msgGo.transform.SetParent(bubbleGo.transform, false);
            TextMeshProUGUI msgTmp = msgGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) msgTmp.font = _defaultFont;
            msgTmp.text = message;
            msgTmp.fontSize = 20f;
            msgTmp.fontStyle = isThinking ? FontStyles.Italic : FontStyles.Normal;
            msgTmp.color = isAi
                ? (isThinking ? new Color(0.70f, 0.88f, 1f, 1f) : new Color(0.92f, 0.96f, 1f, 1f))
                : new Color(0.12f, 0.09f, 0.05f, 1f); // Mực nâu đen sắc nét trên nền giấy vàng
            msgTmp.alignment = TextAlignmentOptions.TopLeft;
            msgTmp.textWrappingMode = TextWrappingModes.Normal;
            msgTmp.overflowMode = TextOverflowModes.Overflow;
            msgTmp.lineSpacing = 4f;
            msgTmp.raycastTarget = false;
            msgTmp.maskable = true;

            Vector2 bodyPref = msgTmp.GetPreferredValues(message, textWidth, float.PositiveInfinity);
            float bodyHeight = Mathf.Max(26f, bodyPref.y + 4f);
            LayoutElement msgLe = msgGo.AddComponent<LayoutElement>();
            msgLe.preferredHeight = bodyHeight;
            msgLe.minHeight = bodyHeight;
            msgLe.preferredWidth = textWidth;

            // 5. Tính toán chính xác tổng chiều cao bong bóng
            float totalBubbleHeight;
            if (isAi)
            {
                totalBubbleHeight = padTop + headerHeight + (headerHeight > 0 ? spacing : 0f) + bodyHeight + padBottom;
                if (totalBubbleHeight < 140f) totalBubbleHeight = 140f;
            }
            else
            {
                totalBubbleHeight = padTop + bodyHeight + padBottom;
                if (totalBubbleHeight < 135f) totalBubbleHeight = 135f;
            }

            bRt.sizeDelta = new Vector2(bubbleWidth, totalBubbleHeight);
            rowRt.sizeDelta = new Vector2(0f, totalBubbleHeight);

            LayoutElement rowLe = rowGo.AddComponent<LayoutElement>();
            rowLe.preferredHeight = totalBubbleHeight;
            rowLe.minHeight = totalBubbleHeight;

            // 6. Force Layout Rebuild ngay lập tức
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rowRt);
            if (_chatContentRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_chatContentRect);
            }

            return rowGo;
        }

        private void EnsureUI()
        {
            if (_modalRoot != null) return;

            Canvas canvas = FindActiveCanvas();
            if (canvas == null) return;

            EnsureEventSystem(canvas);
            EnsureSelectablesHealthy();

            _defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF - Fallback")
                ?? canvas.GetComponentInChildren<TextMeshProUGUI>(true)?.font;

            // 1. Kiểm tra xem trong Canvas đã có instance của Prefab chưa
            Transform existingPopup = canvas.transform.Find("Aurelia_MCQTutorPopup");
            if (existingPopup != null)
            {
                // Kiểm tra xem popup cũ có đầy đủ TMP_InputField không
                var inputInExisting = existingPopup.GetComponentInChildren<TMP_InputField>(true);
                if (inputInExisting == null)
                {
                    Debug.LogWarning("[MCQTutor] Phát hiện popup cũ trong Scene bị thiếu TMP_InputField! Dọn dẹp để nạp Prefab chuẩn từ Resources...");
                    DestroyImmediate(existingPopup.gameObject);
                    existingPopup = null;
                }
                else
                {
                    _modalRoot = existingPopup.gameObject;
                }
            }

            if (_modalRoot == null)
            {
                // 2. Nạp Prefab từ Resources/UI/MCQTutorPopup
                GameObject prefab = Resources.Load<GameObject>(PREFAB_PATH);
                if (prefab != null)
                {
                    _modalRoot = Instantiate(prefab, canvas.transform, false);
                    _modalRoot.name = "Aurelia_MCQTutorPopup";
                }
                else
                {
                    Debug.LogWarning($"[MCQTutor] Không tìm thấy Prefab '{PREFAB_PATH}', khởi tạo cấu trúc chuẩn...");
                    _modalRoot = BuildFallbackPopupUI(canvas);
                    _modalRoot.name = "Aurelia_MCQTutorPopup";
                }
            }

            // 3. Tự động Bind toàn bộ Component UI
            AutoBindUI(_modalRoot);

            _modalRoot.SetActive(false);
        }

        private Canvas FindActiveCanvas()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.gameObject.activeInHierarchy) return canvas;

            Canvas[] allCanvases = FindObjectsByType<Canvas>();
            foreach (var c in allCanvases)
            {
                if (c != null && c.gameObject.activeInHierarchy && c.renderMode != RenderMode.WorldSpace)
                {
                    return c;
                }
            }
            return allCanvases.FirstOrDefault();
        }

        private void AutoBindUI(GameObject root)
        {
            if (root == null) return;

            _canvasGroup = root.GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = root.AddComponent<CanvasGroup>();

            // 1. Khu vực A: Header
            Transform tTitle = FindDeepChild(root.transform, "Text_Title");
            if (tTitle != null)
            {
                _headerTitleText = tTitle.GetComponent<TextMeshProUGUI>();
                if (_headerTitleText != null) _headerTitleText.raycastTarget = false;
            }

            Transform tSub = FindDeepChild(root.transform, "Text_Subtitle");
            if (tSub != null)
            {
                _headerSubtitleText = tSub.GetComponent<TextMeshProUGUI>();
                if (_headerSubtitleText != null) _headerSubtitleText.raycastTarget = false;
            }

            Transform bannerTitle = FindDeepChild(root.transform, "Banner_Title");
            if (bannerTitle != null)
            {
                var img = bannerTitle.GetComponent<Image>();
                if (img != null) img.raycastTarget = false;
            }

            Transform bannerContext = FindDeepChild(root.transform, "Banner_Context");
            if (bannerContext != null)
            {
                var img = bannerContext.GetComponent<Image>();
                if (img != null) img.raycastTarget = false;
            }

            Transform btnX = FindDeepChild(root.transform, "Btn_CloseTopRight");
            if (btnX != null)
            {
                var xImg = btnX.GetComponent<Image>();
                if (xImg != null) xImg.raycastTarget = true;
                _btnCloseTopX = btnX.GetComponent<Button>();
                if (_btnCloseTopX != null)
                {
                    _btnCloseTopX.onClick.RemoveAllListeners();
                    _btnCloseTopX.onClick.AddListener(() =>
                    {
                        Debug.Log("[MCQ Tutor] CloseTopX Clicked");
                        CloseTutor();
                    });
                }
            }

            // 2. Khu vực B: Thẻ câu hỏi
            Transform tCtx = FindDeepChild(root.transform, "Text_ContextInfo");
            if (tCtx != null)
            {
                _contextBadgeText = tCtx.GetComponent<TextMeshProUGUI>();
                if (_contextBadgeText != null)
                {
                    _contextBadgeText.raycastTarget = false;
                    _contextBadgeText.rectTransform.offsetMin = new Vector2(250f, 6f);
                    _contextBadgeText.rectTransform.offsetMax = new Vector2(-40f, -6f);
                }
            }

            // 3. Khu vực C: Vùng chat chính (ScrollRect, Viewport với RectMask2D, Content)
            Transform tScroll = FindDeepChild(root.transform, "Chat_ScrollView");
            if (tScroll != null)
            {
                var scrollImg = tScroll.GetComponent<Image>();
                if (scrollImg != null) scrollImg.raycastTarget = false;
                _chatScrollRect = tScroll.GetComponent<ScrollRect>();
            }

            if (_chatScrollRect != null)
            {
                _chatScrollRect.horizontal = false;
                _chatScrollRect.vertical = true;
                _chatScrollRect.movementType = ScrollRect.MovementType.Clamped;
                _chatScrollRect.scrollSensitivity = 40f;

                // Viewport BẮT BUỘC dùng RectMask2D để clipping 100% TextMeshPro và Image,
                // triệt tiêu tận gốc lỗi tin nhắn tràn qua khung vàng và đè lên tiêu đề khi cuộn!
                Transform vp = FindDeepChild(_chatScrollRect.transform, "Viewport");
                if (vp == null) vp = _chatScrollRect.transform.Find("Viewport");
                if (vp != null)
                {
                    RectTransform vpRt = vp as RectTransform;
                    _chatScrollRect.viewport = vpRt;

                    // Gỡ bỏ Mask stencil nếu có (vì Mask stencil không clip được TextMeshPro gây tràn ra ngoài)
                    var oldMask = vp.GetComponent<Mask>();
                    if (oldMask != null) DestroyImmediate(oldMask);

                    var vpImg = vp.GetComponent<Image>();
                    if (vpImg != null)
                    {
                        vpImg.color = Color.clear;
                        vpImg.raycastTarget = true;
                    }

                    // Bổ sung RectMask2D để clip hoàn hảo 100%
                    var rm = vp.GetComponent<RectMask2D>();
                    if (rm == null) rm = vp.gameObject.AddComponent<RectMask2D>();
                    rm.enabled = true;
                }

                _chatContentRect = _chatScrollRect.content;
                if (_chatContentRect == null && vp != null)
                {
                    Transform tContent = vp.Find("Content");
                    if (tContent != null)
                    {
                        _chatContentRect = tContent as RectTransform;
                        _chatScrollRect.content = _chatContentRect;
                    }
                }
            }

            if (_chatContentRect == null)
            {
                Transform tContent = FindDeepChild(root.transform, "Content");
                if (tContent != null) _chatContentRect = tContent as RectTransform;
            }

            if (_chatContentRect != null)
            {
                var vlg = _chatContentRect.GetComponent<VerticalLayoutGroup>();
                if (vlg == null) vlg = _chatContentRect.gameObject.AddComponent<VerticalLayoutGroup>();
                vlg.padding = new RectOffset(20, 20, 16, 16);
                vlg.spacing = 16f;
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;

                var csf = _chatContentRect.GetComponent<ContentSizeFitter>();
                if (csf == null) csf = _chatContentRect.gameObject.AddComponent<ContentSizeFitter>();
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            // Nạp sprites từ templates nếu có trong root
            Transform tplAi = FindDeepChild(root.transform, "Template_AiBubble");
            if (tplAi != null)
            {
                var img = tplAi.GetComponent<Image>();
                if (img != null && img.sprite != null) aiBubbleSprite = img.sprite;
                tplAi.gameObject.SetActive(false);
            }
            Transform tplStu = FindDeepChild(root.transform, "Template_StudentBubble");
            if (tplStu != null)
            {
                var img = tplStu.GetComponent<Image>();
                if (img != null && img.sprite != null) studentBubbleSprite = img.sprite;
                tplStu.gameObject.SetActive(false);
            }
            EnsureBubbleSprites();

            // 4. Khu vực D: Thanh gợi ý nhanh
            _quickPromptsContainer = FindDeepChild(root.transform, "QuickPromptsBar");

            // 5. Khu vực D: Ô nhập tin nhắn
            Transform tInput = FindDeepChild(root.transform, "Box_InputField");
            if (tInput != null)
            {
                var inImg = tInput.GetComponent<Image>();
                if (inImg != null) inImg.raycastTarget = true;

                // Chuẩn hoá TextArea: lùi x=145px để placeholder và chữ không đè lên icon bút
                Transform taTrans = FindDeepChild(tInput, "TextArea");
                RectTransform taRt = null;
                if (taTrans != null)
                {
                    taRt = taTrans as RectTransform;
                    if (taRt != null)
                    {
                        taRt.anchorMin = Vector2.zero;
                        taRt.anchorMax = Vector2.one;
                        taRt.offsetMin = new Vector2(180f, 12f);
                        taRt.offsetMax = new Vector2(-30f, -12f);
                    }
                    if (taTrans.GetComponent<RectMask2D>() == null)
                    {
                        taTrans.gameObject.AddComponent<RectMask2D>();
                    }
                }

                // Chuẩn hoá Placeholder: raycastTarget=false, căn lề MidlineLeft
                Transform phTrans = FindDeepChild(tInput, "Placeholder");
                TextMeshProUGUI phTmp = null;
                if (phTrans != null)
                {
                    RectTransform phRt = phTrans as RectTransform;
                    if (phRt != null)
                    {
                        phRt.anchorMin = Vector2.zero;
                        phRt.anchorMax = Vector2.one;
                        phRt.offsetMin = Vector2.zero;
                        phRt.offsetMax = Vector2.zero;
                    }
                    phTmp = phTrans.GetComponent<TextMeshProUGUI>();
                    if (phTmp != null)
                    {
                        if (_defaultFont != null && phTmp.font == null) phTmp.font = _defaultFont;
                        phTmp.raycastTarget = false;
                        phTmp.alignment = TextAlignmentOptions.MidlineLeft;
                        phTmp.fontSize = 20f;
                        phTmp.color = new Color(0.55f, 0.70f, 0.90f, 0.85f);
                    }
                }

                // Chuẩn hoá Text: raycastTarget=false, căn lề MidlineLeft
                Transform txtTrans = FindDeepChild(tInput, "Text");
                TextMeshProUGUI txtTmp = null;
                if (txtTrans != null)
                {
                    RectTransform txtRt = txtTrans as RectTransform;
                    if (txtRt != null)
                    {
                        txtRt.anchorMin = Vector2.zero;
                        txtRt.anchorMax = Vector2.one;
                        txtRt.offsetMin = Vector2.zero;
                        txtRt.offsetMax = Vector2.zero;
                    }
                    txtTmp = txtTrans.GetComponent<TextMeshProUGUI>();
                    if (txtTmp != null)
                    {
                        if (_defaultFont != null && txtTmp.font == null) txtTmp.font = _defaultFont;
                        txtTmp.raycastTarget = false;
                        txtTmp.alignment = TextAlignmentOptions.MidlineLeft;
                        txtTmp.fontSize = 20f;
                        txtTmp.color = Color.white;
                    }
                }

                // Tự động kiểm tra hoặc khôi phục TMP_InputField nếu bị thiếu/hỏng
                _inputField = tInput.GetComponent<TMP_InputField>();
                if (_inputField == null)
                {
                    Debug.LogWarning("[MCQTutor] Box_InputField thiếu TMP_InputField! Tự động tạo và cấu hình Component...");
                    _inputField = tInput.gameObject.AddComponent<TMP_InputField>();
                }

                _inputField.targetGraphic = inImg;
                _inputField.textViewport = taRt;
                _inputField.textComponent = txtTmp;
                _inputField.placeholder = phTmp;
                _inputField.lineType = TMP_InputField.LineType.MultiLineSubmit;
                _inputField.characterLimit = 1000;
                _inputField.interactable = true;
                _inputField.enabled = true;
                _inputField.customCaretColor = true;
                _inputField.caretColor = new Color(1f, 0.95f, 0.70f, 1f);
                _inputField.caretWidth = 2;
                _inputField.navigation = new Navigation { mode = Navigation.Mode.None };

                _inputField.onSubmit.RemoveAllListeners();
                _inputField.onSubmit.AddListener(val =>
                {
                    Debug.Log($"[MCQ Tutor] InputField Submitted: '{val}'");
                    if (!string.IsNullOrWhiteSpace(val)) SendStudentMessage(val);
                });

                _inputField.onSelect.RemoveAllListeners();
                _inputField.onSelect.AddListener(val =>
                {
                    Debug.Log("[MCQ Tutor] InputField Focused/Selected! Con trỏ nhập văn bản đã xuất hiện.");
                });

                _inputField.onDeselect.RemoveAllListeners();
                _inputField.onDeselect.AddListener(val =>
                {
                    Debug.Log("[MCQ Tutor] InputField Deselected.");
                });
            }

            // 6. Khu vực D: Nút GỬI & ĐÓNG
            Transform tSend = FindDeepChild(root.transform, "Btn_Send");
            if (tSend != null)
            {
                var sendImg = tSend.GetComponent<Image>();
                if (sendImg != null) sendImg.raycastTarget = true;
                Transform sendLbl = FindDeepChild(tSend, "Text_SendLabel");
                if (sendLbl != null)
                {
                    var sendTmp = sendLbl.GetComponent<TextMeshProUGUI>();
                    if (sendTmp != null)
                    {
                        sendTmp.raycastTarget = false;
                        sendTmp.text = "<b>GỬI</b>";
                        sendTmp.fontSize = 24f;
                        sendTmp.alignment = TextAlignmentOptions.Center;
                    }
                    RectTransform sendLblRt = sendLbl as RectTransform;
                    if (sendLblRt != null)
                    {
                        sendLblRt.anchorMin = new Vector2(0.44f, 0f);
                        sendLblRt.anchorMax = new Vector2(0.96f, 1f);
                        sendLblRt.offsetMin = Vector2.zero;
                        sendLblRt.offsetMax = Vector2.zero;
                    }
                }
                _btnSend = tSend.GetComponent<Button>();
                if (_btnSend != null)
                {
                    _btnSend.onClick.RemoveAllListeners();
                    _btnSend.onClick.AddListener(() =>
                    {
                        string msg = _inputField != null ? _inputField.text : "";
                        Debug.Log($"[MCQ Tutor] Send Clicked: '{msg}'");
                        SendStudentMessage(msg);
                    });
                }
            }

            Transform tClose = FindDeepChild(root.transform, "Btn_CloseBottom");
            if (tClose != null)
            {
                var closeImg = tClose.GetComponent<Image>();
                if (closeImg != null) closeImg.raycastTarget = true;
                Transform closeLbl = FindDeepChild(tClose, "Text_CloseLabel");
                if (closeLbl != null)
                {
                    var closeTmp = closeLbl.GetComponent<TextMeshProUGUI>();
                    if (closeTmp != null)
                    {
                        closeTmp.raycastTarget = false;
                        closeTmp.text = "<b>ĐÓNG</b>";
                        closeTmp.fontSize = 22f;
                        closeTmp.alignment = TextAlignmentOptions.Center;
                    }
                    RectTransform closeLblRt = closeLbl as RectTransform;
                    if (closeLblRt != null)
                    {
                        closeLblRt.anchorMin = new Vector2(0.46f, 0f);
                        closeLblRt.anchorMax = new Vector2(0.96f, 1f);
                        closeLblRt.offsetMin = Vector2.zero;
                        closeLblRt.offsetMax = Vector2.zero;
                    }
                }
                _btnClose = tClose.GetComponent<Button>();
                if (_btnClose != null)
                {
                    _btnClose.onClick.RemoveAllListeners();
                    _btnClose.onClick.AddListener(() =>
                    {
                        Debug.Log("[MCQ Tutor] CloseBottom Clicked");
                        CloseTutor();
                    });
                }
            }
        }

        private GameObject BuildFallbackPopupUI(Canvas canvas)
        {
            GameObject rootGo = new GameObject("Aurelia_MCQTutorPopup");
            rootGo.transform.SetParent(canvas.transform, false);
            RectTransform rootRt = rootGo.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            Image backdrop = rootGo.AddComponent<Image>();
            backdrop.color = new Color(0.01f, 0.03f, 0.08f, 0.82f);
            backdrop.raycastTarget = true;

            CanvasGroup cg = rootGo.AddComponent<CanvasGroup>();
            cg.alpha = 1f;

            // Board (1560 x 940)
            GameObject boardGo = new GameObject("Board_Background");
            boardGo.transform.SetParent(rootGo.transform, false);
            RectTransform boardRt = boardGo.AddComponent<RectTransform>();
            boardRt.anchorMin = new Vector2(0.5f, 0.5f);
            boardRt.anchorMax = new Vector2(0.5f, 0.5f);
            boardRt.pivot = new Vector2(0.5f, 0.5f);
            boardRt.anchoredPosition = Vector2.zero;
            boardRt.sizeDelta = new Vector2(1560f, 940f);
            Image boardImg = boardGo.AddComponent<Image>();
            boardImg.color = new Color(0.04f, 0.08f, 0.18f, 0.98f);
            Outline boardOutline = boardGo.AddComponent<Outline>();
            boardOutline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            boardOutline.effectDistance = new Vector2(2f, -2f);

            // Banner Title (Pos Y = 395)
            GameObject titleGo = new GameObject("Banner_Title");
            titleGo.transform.SetParent(boardGo.transform, false);
            RectTransform titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0f, 395f);
            titleRt.sizeDelta = new Vector2(980f, 160f);

            GameObject tTextGo = new GameObject("Text_Title");
            tTextGo.transform.SetParent(titleGo.transform, false);
            RectTransform tTextRt = tTextGo.AddComponent<RectTransform>();
            tTextRt.anchoredPosition = new Vector2(0f, -14f);
            tTextRt.sizeDelta = new Vector2(-100f, 60f);
            TextMeshProUGUI tTmp = tTextGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) tTmp.font = _defaultFont;
            tTmp.text = "GIA SƯ AI AURELIA";
            tTmp.fontSize = 36f;
            tTmp.fontStyle = FontStyles.Bold;
            tTmp.color = new Color(1f, 0.94f, 0.70f, 1f);
            tTmp.alignment = TextAlignmentOptions.Center;

            GameObject sTextGo = new GameObject("Text_Subtitle");
            sTextGo.transform.SetParent(titleGo.transform, false);
            RectTransform sTextRt = sTextGo.AddComponent<RectTransform>();
            sTextRt.anchoredPosition = new Vector2(0f, 12f);
            sTextRt.sizeDelta = new Vector2(-100f, 30f);
            TextMeshProUGUI sTmp = sTextGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) sTmp.font = _defaultFont;
            sTmp.text = "Người đồng hành Vật lý 9";
            sTmp.fontSize = 19f;
            sTmp.fontStyle = FontStyles.Italic;
            sTmp.color = new Color(0.78f, 0.90f, 1f, 1f);
            sTmp.alignment = TextAlignmentOptions.Center;

            // Close Top Right
            GameObject closeXGo = new GameObject("Btn_CloseTopRight");
            closeXGo.transform.SetParent(boardGo.transform, false);
            RectTransform xRt = closeXGo.AddComponent<RectTransform>();
            xRt.anchoredPosition = new Vector2(705f, 395f);
            xRt.sizeDelta = new Vector2(64f, 64f);
            Image xImg = closeXGo.AddComponent<Image>();
            xImg.color = new Color(0.8f, 0.15f, 0.15f, 1f);
            closeXGo.AddComponent<Button>();

            // Banner Context (Pos Y = 285)
            GameObject ctxGo = new GameObject("Banner_Context");
            ctxGo.transform.SetParent(boardGo.transform, false);
            RectTransform ctxRt = ctxGo.AddComponent<RectTransform>();
            ctxRt.anchoredPosition = new Vector2(0f, 285f);
            ctxRt.sizeDelta = new Vector2(1340f, 95f);
            Image ctxImg = ctxGo.AddComponent<Image>();
            ctxImg.color = new Color(0.06f, 0.12f, 0.24f, 0.95f);

            GameObject ctxTextGo = new GameObject("Text_ContextInfo");
            ctxTextGo.transform.SetParent(ctxGo.transform, false);
            RectTransform ctRt = ctxTextGo.AddComponent<RectTransform>();
            ctRt.anchorMin = Vector2.zero;
            ctRt.anchorMax = Vector2.one;
            ctRt.offsetMin = new Vector2(115f, 8f);
            ctRt.offsetMax = new Vector2(-40f, -8f);
            TextMeshProUGUI cTmp = ctxTextGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) cTmp.font = _defaultFont;
            cTmp.text = "<b>CÂU 1/10</b>";
            cTmp.fontSize = 20f;
            cTmp.color = Color.white;
            cTmp.textWrappingMode = TextWrappingModes.Normal;

            // Chat ScrollView (Pos Y = 30)
            GameObject scrollGo = new GameObject("Chat_ScrollView");
            scrollGo.transform.SetParent(boardGo.transform, false);
            RectTransform sRt = scrollGo.AddComponent<RectTransform>();
            sRt.anchoredPosition = new Vector2(0f, 30f);
            sRt.sizeDelta = new Vector2(1340f, 400f);
            Image scBg = scrollGo.AddComponent<Image>();
            scBg.color = new Color(0.02f, 0.05f, 0.12f, 0.70f);
            ScrollRect sr = scrollGo.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 40f;

            GameObject vpGo = new GameObject("Viewport");
            vpGo.transform.SetParent(scrollGo.transform, false);
            RectTransform vpRt = vpGo.AddComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero;
            vpRt.offsetMax = Vector2.zero;
            Image vpImg = vpGo.AddComponent<Image>();
            vpImg.color = Color.clear;
            vpImg.raycastTarget = true;
            vpGo.AddComponent<RectMask2D>();
            sr.viewport = vpRt;

            GameObject contentGo = new GameObject("Content");
            contentGo.transform.SetParent(vpGo.transform, false);
            RectTransform contentRt = contentGo.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            VerticalLayoutGroup vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 16, 16);
            vlg.spacing = 14f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            ContentSizeFitter csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = contentRt;

            // Quick Prompts (Pos Y = -215)
            GameObject qpGo = new GameObject("QuickPromptsBar");
            qpGo.transform.SetParent(boardGo.transform, false);
            RectTransform qpRt = qpGo.AddComponent<RectTransform>();
            qpRt.anchoredPosition = new Vector2(0f, -215f);
            qpRt.sizeDelta = new Vector2(1340f, 42f);
            HorizontalLayoutGroup qpHlg = qpGo.AddComponent<HorizontalLayoutGroup>();
            qpHlg.spacing = 14f;
            qpHlg.childControlWidth = false;
            qpHlg.childControlHeight = true;

            // Bottom Input Bar (Pos Y = -290)
            GameObject botGo = new GameObject("BottomInputBar");
            botGo.transform.SetParent(boardGo.transform, false);
            RectTransform botRt = botGo.AddComponent<RectTransform>();
            botRt.anchorMin = new Vector2(0.5f, 0.5f);
            botRt.anchorMax = new Vector2(0.5f, 0.5f);
            botRt.pivot = new Vector2(0.5f, 0.5f);
            botRt.anchoredPosition = new Vector2(0f, -290f);
            botRt.sizeDelta = new Vector2(1340f, 85f);

            GameObject inBoxGo = new GameObject("Box_InputField");
            inBoxGo.transform.SetParent(botGo.transform, false);
            RectTransform inBoxRt = inBoxGo.AddComponent<RectTransform>();
            inBoxRt.anchorMin = new Vector2(0f, 0.5f);
            inBoxRt.anchorMax = new Vector2(0f, 0.5f);
            inBoxRt.pivot = new Vector2(0f, 0.5f);
            inBoxRt.anchoredPosition = new Vector2(0f, 0f);
            inBoxRt.sizeDelta = new Vector2(940f, 85f);
            Image inImg = inBoxGo.AddComponent<Image>();
            inImg.color = new Color(0.08f, 0.14f, 0.28f, 0.95f);

            GameObject taGo = new GameObject("TextArea");
            taGo.transform.SetParent(inBoxGo.transform, false);
            RectTransform taRt = taGo.AddComponent<RectTransform>();
            taRt.anchorMin = Vector2.zero;
            taRt.anchorMax = Vector2.one;
            taRt.offsetMin = new Vector2(145f, 12f);
            taRt.offsetMax = new Vector2(-30f, -12f);
            taGo.AddComponent<RectMask2D>();

            GameObject phGo = new GameObject("Placeholder");
            phGo.transform.SetParent(taGo.transform, false);
            RectTransform phRt = phGo.AddComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = Vector2.zero;
            phRt.offsetMax = Vector2.zero;
            TextMeshProUGUI phTmp = phGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) phTmp.font = _defaultFont;
            phTmp.text = "Nhập câu hỏi của em...";
            phTmp.fontSize = 20f;
            phTmp.fontStyle = FontStyles.Italic;
            phTmp.color = new Color(0.55f, 0.70f, 0.90f, 0.85f);

            GameObject txtGo = new GameObject("Text");
            txtGo.transform.SetParent(taGo.transform, false);
            RectTransform txtRt = txtGo.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;
            TextMeshProUGUI txtTmp = txtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) txtTmp.font = _defaultFont;
            txtTmp.fontSize = 20f;
            txtTmp.color = Color.white;

            TMP_InputField inputField = inBoxGo.AddComponent<TMP_InputField>();
            inputField.targetGraphic = inImg;
            inputField.textViewport = taRt;
            inputField.textComponent = txtTmp;
            inputField.placeholder = phTmp;

            // Send Button
            GameObject sendGo = new GameObject("Btn_Send");
            sendGo.transform.SetParent(botGo.transform, false);
            RectTransform sendRt = sendGo.AddComponent<RectTransform>();
            sendRt.anchorMin = new Vector2(0f, 0.5f);
            sendRt.anchorMax = new Vector2(0f, 0.5f);
            sendRt.pivot = new Vector2(0f, 0.5f);
            sendRt.anchoredPosition = new Vector2(960f, 0f);
            sendRt.sizeDelta = new Vector2(180f, 85f);
            Image sendImg = sendGo.AddComponent<Image>();
            sendImg.color = new Color(0.12f, 0.55f, 0.28f, 1f);
            sendGo.AddComponent<Button>();

            // Close Bottom Button
            GameObject closeGo = new GameObject("Btn_CloseBottom");
            closeGo.transform.SetParent(botGo.transform, false);
            RectTransform closeRt = closeGo.AddComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(0f, 0.5f);
            closeRt.anchorMax = new Vector2(0f, 0.5f);
            closeRt.pivot = new Vector2(0f, 0.5f);
            closeRt.anchoredPosition = new Vector2(1160f, 0f);
            closeRt.sizeDelta = new Vector2(180f, 85f);
            Image closeImg = closeGo.AddComponent<Image>();
            closeImg.color = new Color(0.18f, 0.24f, 0.38f, 1f);
            closeGo.AddComponent<Button>();

            return rootGo;
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            if (parent == null) return null;
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                var found = FindDeepChild(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static string TruncateText(string str, int maxLen)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Length <= maxLen ? str : str.Substring(0, maxLen - 3) + "...";
        }

        private static string ExtractTutorReplyManual(string json)
        {
            if (string.IsNullOrEmpty(json)) return "";
            try
            {
                int p = json.IndexOf("\"tutorReply\":");
                if (p != -1)
                {
                    int start = json.IndexOf('"', p + 13);
                    if (start != -1)
                    {
                        int end = json.IndexOf('"', start + 1);
                        if (end != -1)
                        {
                            return json.Substring(start + 1, end - start - 1).Replace("\\n", "\n").Replace("\\\"", "\"");
                        }
                    }
                }
            }
            catch { }
            return json;
        }
    }
}
