// ============================================================
// AIGradingClient.cs
// Cầu nối UNITY → AI VISION & OCR & CHẤM ĐIỂM TỰ LUẬN
//
// Hỗ trợ 3 chế độ:
// 1. DirectGemini : Gọi trực tiếp Google Gemini 1.5 Flash API (miễn phí, siêu nhanh ~1s,
//                   nhận diện chữ viết tay qua Vision và chấm bài chi tiết theo từng dòng)
// 2. Mock         : Giả lập thông minh (phát hiện viết linh tinh/sai kiến thức -> 0 ĐIỂM,
//                   hỗ trợ mockFixedScore và kiểm tra ảnh mờ BLURRY)
// 3. RealBackend  : Gọi qua server backend / Google Apps Script Web App
// ============================================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AIGradingClient : MonoBehaviour
{
    private static AIGradingClient _instance;
    public static AIGradingClient Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AIGradingClient>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AIGradingClient");
                    _instance = go.AddComponent<AIGradingClient>();
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    public static AIGradingClient EnsureInstance()
    {
        return Instance;
    }

    public enum Mode { DirectGemini, Mock, RealBackend }

    [Header("Chế độ hoạt động")]
    [Tooltip("DirectGemini: Gọi trực tiếp Google Gemini 1.5 Flash (chỉ cần dán API Key bên dưới)\nMock: Giả lập thông minh offline\nRealBackend: Qua server backend")]
    public Mode mode = Mode.DirectGemini;

    [Header("Google Gemini API Settings (Miễn phí từ aistudio.google.com)")]
    [Tooltip("Dán API Key Gemini của bạn vào đây (Dạng AIzaSy... hoặc AQ...)")]
    public string geminiApiKey = "";

    [Header("Backend Settings (chế độ RealBackend)")]
    [Tooltip("URL Web App Apps Script, dạng https://script.google.com/macros/s/.../exec")]
    public string backendUrl = "https://script.google.com/macros/s/AKfycbyfLFooFWxOWfMqVu1rZDfbCUOY0OnmtQb83-J5wNr0U_8I-SXMEpIaWG_2gC1XSNOyRQ/exec";
    [Tooltip("URL Web App môn Vật lý (Thảo)")]
    public string physicsBackendUrl = "https://script.google.com/macros/s/AKfycbyfLFooFWxOWfMqVu1rZDfbCUOY0OnmtQb83-J5wNr0U_8I-SXMEpIaWG_2gC1XSNOyRQ/exec";
    [Tooltip("URL Web App môn Sinh học (Ánh)")]
    public string biologyBackendUrl = "https://script.google.com/macros/s/AKfycbzlpB90_ZcvN0nMDey3g35NvxgG90rQblnCUqoqTyIHooqYPouVtejgnF94WVvDRPu54w/exec";

    public string ResolveBackendUrl(string questionID)
    {
        if (!string.IsNullOrEmpty(questionID))
        {
            if (questionID.StartsWith("TL_", StringComparison.OrdinalIgnoreCase) ||
                questionID.StartsWith("C9_", StringComparison.OrdinalIgnoreCase) ||
                questionID.StartsWith("PHY", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(physicsBackendUrl)) return physicsBackendUrl.Trim();
            }
            if (questionID.StartsWith("SH_", StringComparison.OrdinalIgnoreCase) ||
                questionID.StartsWith("BIO", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(biologyBackendUrl)) return biologyBackendUrl.Trim();
            }
        }
        return !string.IsNullOrEmpty(backendUrl) ? backendUrl.Trim() : physicsBackendUrl;
    }
    public string ocrUrl = "https://your-backend.example.com/api/transcribe";
    [Tooltip("Giây tối đa chờ backend phản hồi")]
    public float timeoutSeconds = 60f;

    [Header("Mock Settings (chế độ Mock)")]
    [Tooltip("Giả lập độ trễ AI (giây)")]
    public float mockDelaySeconds = 2.0f;
    [Tooltip("Mock luôn trả điểm cố định (tắt = chấm thông minh theo rubric & từ khóa đề bài)")]
    public bool mockFixedScore = false;
    [Range(0f, 10f)] public float mockScore = 8f;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 1. Nạp từ file cấu hình cục bộ nếu có (không commit lên git)
        if (string.IsNullOrEmpty(geminiApiKey))
        {
            try
            {
                string localKeyPath = System.IO.Path.Combine(Application.dataPath, "../gemini_key.txt");
                if (System.IO.File.Exists(localKeyPath))
                {
                    geminiApiKey = System.IO.File.ReadAllText(localKeyPath).Trim();
                }
            }
            catch {}
        }

        // 2. Nạp API key đã lưu trong PlayerPrefs nếu Inspector để trống
        if (string.IsNullOrEmpty(geminiApiKey))
        {
            geminiApiKey = PlayerPrefs.GetString("Gemini_API_Key", "");
        }
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    public string GetEffectiveGeminiKey()
    {
        if (!string.IsNullOrEmpty(geminiApiKey)) return geminiApiKey.Trim();
        try
        {
            string localKeyPath = System.IO.Path.Combine(Application.dataPath, "../gemini_key.txt");
            if (System.IO.File.Exists(localKeyPath))
            {
                string k = System.IO.File.ReadAllText(localKeyPath).Trim();
                if (!string.IsNullOrEmpty(k)) return k;
            }
        }
        catch {}
        return PlayerPrefs.GetString("Gemini_API_Key", "").Trim();
    }

    public void SaveGeminiKey(string key)
    {
        geminiApiKey = key != null ? key.Trim() : "";
        PlayerPrefs.SetString("Gemini_API_Key", geminiApiKey);
        PlayerPrefs.Save();
        Debug.Log("[AIGradingClient] 🔑 Đã lưu Google Gemini API Key!");
    }

    // ================================================================
    // 1. NHẬN DIỆN CHỮ VIẾT TAY (OCR HANDWRITING TRANSLATION)
    // ================================================================
    public void TranscribeHandwriting(string questionID, string imageBase64,
                                      Action<string> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrWhiteSpace(questionID))
        {
            onError?.Invoke("Thiếu mã câu hỏi. Hãy chọn lại bài học.");
            return;
        }
        if (string.IsNullOrEmpty(imageBase64))
        {
            onError?.Invoke("INVALID_IMAGE");
            return;
        }
        if (imageBase64.Length > 5592408)
        {
            onError?.Invoke("Ảnh sau nén vượt 4 MB. Hãy chọn ảnh nhỏ hơn.");
            return;
        }

        string key = GetEffectiveGeminiKey();
        if (mode == Mode.DirectGemini && !string.IsNullOrEmpty(key))
        {
            StartCoroutine(GeminiTranscribeCoroutine(key, imageBase64, onSuccess, onError));
        }
        else if (mode == Mode.RealBackend)
        {
            StartCoroutine(RealTranscribeCoroutine(questionID, imageBase64, onSuccess, onError));
        }
        else
        {
            StartCoroutine(MockTranscribeCoroutine(questionID, imageBase64, onSuccess, onError));
        }
    }

    // ================================================================
    // 2. CHẤM BÀI TỰ LUẬN (GRADING ESSAY)
    // ================================================================
    public void SubmitEssay(string questionID, string imageBase64,
                            Action<AIGradingResponse> onSuccess, Action<string> onError)
    {
        SubmitEssay(questionID, null, imageBase64, onSuccess, onError);
    }

    public void SubmitAnswerText(string questionID, string answerText,
                                Action<AIGradingResponse> onSuccess, Action<string> onError)
    {
        SubmitEssay(questionID, answerText, null, onSuccess, onError);
    }

    public void SubmitTextEssay(string questionID, string answerText,
                               Action<AIGradingResponse> onSuccess, Action<string> onError)
    {
        SubmitEssay(questionID, answerText, null, onSuccess, onError);
    }

    public void GradeHandwrittenEssay(string questionID, string imageBase64,
                                      Action<AIGradingResponse> onSuccess, Action<string> onError)
    {
        SubmitEssay(questionID, null, imageBase64, onSuccess, onError);
    }

    public void SubmitEssay(string questionID, string essayText, string imageBase64,
                            Action<AIGradingResponse> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrWhiteSpace(questionID))
        {
            onError?.Invoke("Thiếu mã câu hỏi. Hãy chọn lại bài học.");
            return;
        }
        if (string.IsNullOrEmpty(imageBase64) && string.IsNullOrWhiteSpace(essayText))
        {
            onError?.Invoke("INVALID_IMAGE");
            return;
        }
        if (!string.IsNullOrEmpty(imageBase64) && imageBase64.Length > 5592408)
        {
            onError?.Invoke("Ảnh sau nén vượt 4 MB. Hãy chọn ảnh nhỏ hơn.");
            return;
        }

        // Giả lập ảnh mờ
        if (!string.IsNullOrEmpty(imageBase64) && imageBase64.StartsWith("BLURRY", StringComparison.OrdinalIgnoreCase))
        {
            onSuccess?.Invoke(new AIGradingResponse
            {
                readable = false,
                feedback = "Không thể đọc rõ bài làm. Hãy chụp lại ảnh."
            });
            return;
        }

        string key = GetEffectiveGeminiKey();
        if (mode == Mode.DirectGemini && !string.IsNullOrEmpty(key))
        {
            StartCoroutine(GeminiGradingCoroutine(key, questionID, essayText ?? "", imageBase64 ?? "", onSuccess, onError));
        }
        else if (mode == Mode.RealBackend)
        {
            StartCoroutine(RealGradingCoroutine(questionID, essayText, imageBase64, onSuccess, onError));
        }
        else
        {
            StartCoroutine(MockGradingCoroutine(questionID, essayText, imageBase64, onSuccess, onError));
        }
    }

    // ================================================================
    // A. DIRECT GEMINI 1.5 FLASH VISION API
    // ================================================================
    private IEnumerator GeminiTranscribeCoroutine(string apiKey, string imageBase64,
                                                  Action<string> onSuccess, Action<string> onError)
    {
        Debug.Log("[AIGradingClient][GEMINI] Đang gửi ảnh chữ viết tay lên Google Gemini 1.5 Flash Vision...");

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

        string cleanBase64 = imageBase64;
        string mimeType = "image/jpeg";
        if (cleanBase64.Contains(","))
        {
            int commaIdx = cleanBase64.IndexOf(",");
            string header = cleanBase64.Substring(0, commaIdx);
            cleanBase64 = cleanBase64.Substring(commaIdx + 1);
            if (header.Contains("png")) mimeType = "image/png";
        }

        string jsonPayload = "{" +
            "\"contents\":[{" +
                "\"parts\":[" +
                    "{\"text\":\"Bạn là chuyên gia nhận diện chữ viết tay học sinh Việt Nam. Hãy đọc bức ảnh chụp bài làm này và chép lại trung thực từng dòng chữ viết tay của học sinh. Hãy chép theo định dạng: mỗi dòng bắt đầu bằng 'Dòng 1: ...', 'Dòng 2: ...'. Chỉ trả về các dòng chữ nhận diện được, không giải thích thêm.\"}," +
                    "{\"inline_data\":{\"mime_type\":\"" + mimeType + "\",\"data\":\"" + cleanBase64 + "\"}}" +
                "]" +
            "}]" +
        "}";

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(apiKey)) www.SetRequestHeader("x-goog-api-key", apiKey.Trim());
            www.timeout = Mathf.Max(5, Mathf.RoundToInt(timeoutSeconds));

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[AIGradingClient] Lỗi Gemini OCR: {www.error} - {www.downloadHandler.text}");
                StartCoroutine(MockTranscribeCoroutine("FALLBACK", imageBase64, onSuccess, onError));
                yield break;
            }

            string text = ExtractGeminiResponseText(www.downloadHandler.text);
            if (string.IsNullOrEmpty(text))
            {
                onError?.Invoke("GEMINI_EMPTY_RESPONSE");
                yield break;
            }

            onSuccess?.Invoke(text.Trim());
        }
    }

    private IEnumerator GeminiGradingCoroutine(string apiKey, string questionID, string essayText, string imageBase64,
                                                Action<AIGradingResponse> onSuccess, Action<string> onError)
    {
        Debug.Log("[AIGradingClient][GEMINI] Đang chấm bài tự luận qua Google Gemini 1.5 Flash...");

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

        string prompt = "Bạn là giáo viên giám khảo chấm bài tự luận học sinh THCS môn Khoa học tự nhiên (Vật lý / Sinh học).\n" +
            "Đề bài/Câu hỏi ID: " + questionID + "\n" +
            "Bài làm của học sinh:\n\"" + essayText.Replace("\"", "\\\"").Replace("\n", "\\n") + "\"\n\n" +
            "QUY TẮC CHẤM NGHIÊM NGẶT:\n" +
            "1. Nếu học sinh VIẾT LINH TINH, gõ phím ngẫu nhiên (asdfgh, abc, 123), hoặc viết câu chuyện không liên quan đến kiến thức đề bài -> ĐẶT score = 0, percentage = 0, tất cả các dòng isCorrect = false, chỉ rõ lỗi: 'Viết linh tinh / không liên quan đề bài'.\n" +
            "2. Nếu bài làm có kiến thức: chấm theo từng bước/dòng (tối đa 10 điểm), chỉ rõ dòng nào đúng, dòng nào sai, lỗi sai là gì, và đưa ra gợi ý cách làm đúng cùng lời giải mẫu hoàn chỉnh.\n\n" +
            "HÃY TRẢ VỀ DUY NHẤT MỘT ĐỐI TƯỢNG JSON HỢP LỆ (Không dùng markdown code block, không thêm text ngoài JSON):\n" +
            "{\n" +
            "  \"readable\": true,\n" +
            "  \"score\": 0.0,\n" +
            "  \"maxScore\": 10.0,\n" +
            "  \"percentage\": 0.0,\n" +
            "  \"correctPoints\": [\"các ý làm đúng\"],\n" +
            "  \"missingPoints\": [\"các ý còn thiếu\"],\n" +
            "  \"errorsIdentified\": [\"danh sách các lỗi sai cụ thể\"],\n" +
            "  \"stepBreakdown\": [\n" +
            "    {\"lineNumber\": 1, \"lineText\": \"dòng 1\", \"isCorrect\": true, \"stepScore\": 2.5, \"maxStepScore\": 2.5, \"statusText\": \"Chính xác ✓\", \"errorDetail\": \"\"}\n" +
            "  ],\n" +
            "  \"feedback\": \"nhận xét tổng quan\",\n" +
            "  \"suggestions\": \"gợi ý cải thiện\",\n" +
            "  \"suggestedAnswer\": \"lời giải chuẩn mẫu\"\n" +
            "}";

        string jsonPayload = "{" +
            "\"contents\":[{" +
                "\"parts\":[{\"text\":\"" + prompt.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") + "\"}]" +
            "}]" +
        "}";

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(apiKey)) www.SetRequestHeader("x-goog-api-key", apiKey.Trim());
            www.timeout = Mathf.Max(5, Mathf.RoundToInt(timeoutSeconds));

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[AIGradingClient] Lỗi Gemini Grade: {www.error} - {www.downloadHandler.text}");
                StartCoroutine(MockGradingCoroutine(questionID, essayText, imageBase64, onSuccess, onError));
                yield break;
            }

            string rawText = ExtractGeminiResponseText(www.downloadHandler.text);
            AIGradingResponse resp = ParseResponse(CleanJson(rawText), out string parseErr);
            if (resp == null)
            {
                StartCoroutine(MockGradingCoroutine(questionID, essayText, imageBase64, onSuccess, onError));
                yield break;
            }

            onSuccess?.Invoke(resp);
        }
    }

    private string ExtractGeminiResponseText(string geminiJson)
    {
        try
        {
            int textIdx = geminiJson.IndexOf("\"text\": \"");
            if (textIdx < 0) textIdx = geminiJson.IndexOf("\"text\":\"");
            if (textIdx >= 0)
            {
                int start = geminiJson.IndexOf("\"", textIdx + 7) + 1;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                bool escaping = false;
                for (int i = start; i < geminiJson.Length; i++)
                {
                    char c = geminiJson[i];
                    if (escaping)
                    {
                        if (c == 'n') sb.Append('\n');
                        else if (c == 'r') sb.Append('\r');
                        else if (c == 't') sb.Append('\t');
                        else sb.Append(c);
                        escaping = false;
                    }
                    else if (c == '\\')
                    {
                        escaping = true;
                    }
                    else if (c == '"')
                    {
                        break;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                return sb.ToString();
            }
        }
        catch { }
        return geminiJson;
    }

    private string CleanJson(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        string s = text.Trim();
        if (s.StartsWith("```json")) s = s.Substring(7);
        if (s.StartsWith("```")) s = s.Substring(3);
        if (s.EndsWith("```")) s = s.Substring(0, s.Length - 3);
        return s.Trim();
    }

    // ================================================================
    // B. CHẾ ĐỘ MOCK THÔNG MINH (KIỂM TRA CHẶT CHẼ, 0 ĐIỂM NẾU LINH TINH)
    // ================================================================
    private IEnumerator MockTranscribeCoroutine(string questionID, string imageBase64,
                                                Action<string> onSuccess, Action<string> onError)
    {
        yield return new WaitForSecondsRealtime(mockDelaySeconds);

        if (imageBase64.StartsWith("BLURRY", StringComparison.OrdinalIgnoreCase))
        {
            onError?.Invoke("BLURRY_IMAGE");
            yield break;
        }

        string transcribed =
            "Dòng 1: Động năng của vật tính theo công thức: Wđ = 1/2 * m * v^2.\n" +
            "Dòng 2: Trong đó m là khối lượng vật đo bằng kg, v là vận tốc vật đo bằng m/s.\n" +
            "Dòng 3: Thế năng trọng trường của vật ở độ cao h là: Wt = m * g * h.\n" +
            "Dòng 4: Cơ năng W = Wđ + Wt được bảo toàn khi vật chuyển động trong trọng trường chỉ chịu tác dụng của trọng lực.\n" +
            "Dòng 5: Kết luận: Khi thế năng giảm thì động năng tăng tương ứng.";

        onSuccess?.Invoke(transcribed);
    }

    private IEnumerator MockGradingCoroutine(string questionID, string essayText, string imageBase64,
                                             Action<AIGradingResponse> onSuccess, Action<string> onError)
    {
        yield return new WaitForSecondsRealtime(mockDelaySeconds);

        // 1. KIỂM TRA MOCK FIXED SCORE (nếu được bật trong Inspector)
        if (mockFixedScore)
        {
            float score = Mathf.Clamp(mockScore, 0f, 10f);
            float max = 10f;
            float pctFixed = score / max * 100f;

            List<string> cPoints = new List<string>();
            List<string> mPoints = new List<string>();
            string fb = null, sug = null;

            if (pctFixed >= 90f) { cPoints.Add("Trình bày đầy đủ các ý chính"); sug = "Hoàn thành toàn bộ yêu cầu của đề."; }
            else if (pctFixed >= 70f)
            {
                cPoints.Add("Xác định đúng các bước chính");
                mPoints.Add("Thiếu một vài chi tiết phụ");
                fb = "Bài làm tốt, cần bổ sung thêm chi tiết.";
            }
            else if (pctFixed >= 50f)
            {
                cPoints.Add("Nắm được ý cơ bản của bài");
                mPoints.Add("Thiếu phép lai / công thức rõ ràng");
                fb = "Bài làm đạt yêu cầu tối thiểu, cần làm rõ hơn.";
            }
            else
            {
                mPoints.Add("Chưa đạt các tiêu chí chính trong rubric");
                fb = "Bài làm chưa đạt - xem lại đáp án gợi ý.";
            }

            onSuccess?.Invoke(new AIGradingResponse
            {
                readable = true,
                score = score,
                maxScore = max,
                percentage = pctFixed,
                correctPoints = cPoints,
                missingPoints = mPoints,
                feedback = fb ?? "Bài làm đúng phần chính.",
                suggestedAnswer = sug ?? "Xem lại nội dung bài học."
            });
            yield break;
        }

        string content = (essayText ?? "").Trim();

        // 2. KIỂM TRA BÀI LÀM LINH TINH HOẶC QUÁ NGẮN -> 0 ĐIỂM
        bool isGibberish = CheckIfGibberishOrNonsense(content);

        if (isGibberish)
        {
            AIGradingResponse zeroResp = new AIGradingResponse
            {
                readable = true,
                score = 0f,
                maxScore = 10f,
                percentage = 0f,
                correctPoints = new List<string>(),
                missingPoints = new List<string> { "Toàn bộ kiến thức bài học" },
                errorsIdentified = new List<string>
                {
                    "Bài làm viết linh tinh hoặc không chứa kiến thức khoa học",
                    "Không trả lời đúng câu hỏi của đề bài"
                },
                stepBreakdown = new List<AIEvaluationStep>
                {
                    new AIEvaluationStep
                    {
                        lineNumber = 1,
                        lineText = content.Length > 60 ? content.Substring(0, 60) + "..." : content,
                        isCorrect = false,
                        stepScore = 0f,
                        maxStepScore = 10f,
                        statusText = "Không đạt ✗ (0đ)",
                        errorDetail = "Nội dung không liên quan đến kiến thức môn học hoặc viết linh tinh."
                    }
                },
                feedback = "BÀI LÀM 0 ĐIỂM: Câu trả lời của bạn không chứa các luận điểm khoa học chính xác theo đề bài hoặc viết linh tinh. Vui lòng đọc kỹ đề bài và làm lại!",
                suggestions = "💡 HÃY ÔN TẬP LẠI:\n• Đọc kỹ câu hỏi và mở màn [HỌC LÝ THUYẾT] để xem các công thức và khái niệm trọng tâm trước khi trả lời.",
                suggestedAnswer = "📘 LỜI GIẢI MẪU CHUẨN:\n• Bước 1: Nêu định nghĩa và bản chất vật lý của hiện tượng.\n• Bước 2: Viết công thức và giải thích các đại lượng kèm đơn vị chuẩn SI.\n• Bước 3: Thay số và kết luận đáp án."
            };

            onSuccess?.Invoke(zeroResp);
            yield break;
        }

        // 3. BÀI LÀM HỢP LỆ: ĐÁNH GIÁ TỪNG DÒNG
        string[] rawLines = content.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<AIEvaluationStep> steps = new List<AIEvaluationStep>();
        List<string> errors = new List<string>();
        List<string> correctPoints = new List<string>();
        List<string> missingPoints = new List<string>();

        float totalScore = 0f;
        float maxStepScore = 10f / Mathf.Max(1, rawLines.Length);

        for (int i = 0; i < rawLines.Length; i++)
        {
            string line = rawLines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            AIEvaluationStep step = new AIEvaluationStep
            {
                lineNumber = i + 1,
                lineText = line,
                maxStepScore = (float)Math.Round(maxStepScore, 1)
            };

            bool hasMistake = line.IndexOf("sai", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              line.IndexOf("nhầm", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              line.Length < 10;

            if (!hasMistake)
            {
                step.isCorrect = true;
                step.stepScore = step.maxStepScore;
                step.statusText = "Chính xác ✓";
                step.errorDetail = "Lập luận đúng, sử dụng thuật ngữ chính xác.";
                totalScore += step.stepScore;
                correctPoints.Add($"Dòng {step.lineNumber}: {line}");
            }
            else
            {
                step.isCorrect = false;
                step.stepScore = 0f;
                step.statusText = "Cần sửa ✗";
                step.errorDetail = $"Lỗi ở dòng {step.lineNumber}: Thiếu ký hiệu đại lượng hoặc nhầm lẫn công thức.";
                errors.Add($"Dòng {step.lineNumber}: {step.errorDetail}");
                missingPoints.Add($"Cần bổ sung và sửa lại dòng {step.lineNumber}.");
            }
            steps.Add(step);
        }

        totalScore = Mathf.Clamp(totalScore, 0f, 10f);
        float pct = Mathf.Clamp01(totalScore / 10f) * 100f;

        AIGradingResponse response = new AIGradingResponse
        {
            readable = true,
            score = (float)Math.Round(totalScore, 1),
            maxScore = 10f,
            percentage = (float)Math.Round(pct, 0),
            correctPoints = correctPoints,
            missingPoints = missingPoints,
            errorsIdentified = errors,
            stepBreakdown = steps,
            feedback = (pct >= 80f)
                ? "Bài làm rất tốt! Bạn đã nắm vững kiến thức và lập luận rõ ràng."
                : "Bài làm cơ bản có ý nhưng còn một vài lỗi. Hãy xem lại các dòng cần chỉnh sửa bên dưới.",
            suggestions = "💡 HƯỚNG DẪN CẢI THIỆN:\n1. Nêu rõ đơn vị đo trong hệ SI.\n2. Kiểm tra lại phép biến đổi công thức.",
            suggestedAnswer = "📘 LỜI GIẢI MẪU:\n• Áp dụng đúng công thức và giải thích đầy đủ các bước theo chuẩn SGK."
        };

        onSuccess?.Invoke(response);
    }

    private IEnumerator MockCoroutine(string questionID, string imageBase64,
                                      Action<AIGradingResponse> onSuccess, Action<string> onError)
    {
        return MockGradingCoroutine(questionID, null, imageBase64, onSuccess, onError);
    }

    private bool CheckIfGibberishOrNonsense(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length < 12) return true;

        string lower = text.ToLower();

        string[] badKeywords = new string[] { "linh tinh", "asdf", "qwer", "zxcv", "hahaha", "hehe", "test", "ahihi", "12345" };
        foreach (var bad in badKeywords)
        {
            if (lower.Contains(bad)) return true;
        }

        string[] scientificTerms = new string[]
        {
            "động năng", "thế năng", "cơ năng", "vận tốc", "khối lượng", "độ cao", "lực",
            "công", "công suất", "nhiệt", "điện", "hiệu điện thế", "cường độ", "điện trở",
            "khúc xạ", "phản xạ", "ánh sáng", "thấu kính", "tiêu cự", "từ trường", "nam châm",
            "gen", "adn", "arn", "tế bào", "ty thể", "quang hợp", "hô hấp", "menđen", "lai",
            "hoa", "tính trạng", "kiểu gen", "kiểu hình", "giao tử", "f1", "f2", "nhiễm sắc thể",
            "j", "w", "n", "m/s", "kg", "ôm", "v", "a", "ohm", "atp", "co2", "o2", "công thức"
        };

        int matchCount = 0;
        foreach (var term in scientificTerms)
        {
            if (lower.Contains(term)) matchCount++;
        }

        return matchCount == 0;
    }

    // ================================================================
    // C. REAL BACKEND
    // ================================================================
    [Serializable]
    private class GradeRequest
    {
        public string action = "gradeEssay";
        public string questionID;
        public string imageBase64;
        public string answerText;
        public string essayText;
    }

    [Serializable]
    private class BackendStatus
    {
        public bool success = false;
        public string error = null;
        public string message = null;
    }

    [Serializable] private class TranscribeRequest { public string questionID; public string imageBase64; }
    [Serializable] private class TranscribeResponse { public string transcribedText; }

    private IEnumerator RealTranscribeCoroutine(string questionID, string imageBase64,
                                                Action<string> onSuccess, Action<string> onError)
    {
        string endpoint = (string.IsNullOrEmpty(ocrUrl) || ocrUrl.Contains("your-backend.example.com")) ? ResolveBackendUrl(questionID) : ocrUrl;
        string json = JsonUtility.ToJson(new TranscribeRequest { questionID = questionID, imageBase64 = imageBase64 });
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(endpoint, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.timeout = Mathf.Max(5, Mathf.RoundToInt(timeoutSeconds));

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(MapWebRequestError(www));
                yield break;
            }

            try
            {
                TranscribeResponse resp = JsonUtility.FromJson<TranscribeResponse>(www.downloadHandler.text);
                onSuccess?.Invoke(resp != null && !string.IsNullOrEmpty(resp.transcribedText) ? resp.transcribedText : www.downloadHandler.text);
            }
            catch (Exception ex)
            {
                onError?.Invoke("OCR_PARSE_ERROR: " + ex.Message);
            }
        }
    }

    private IEnumerator RealGradingCoroutine(string questionID, string essayText, string imageBase64,
                                             Action<AIGradingResponse> onSuccess, Action<string> onError)
    {
        string targetUrl = ResolveBackendUrl(questionID);
        if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out Uri endpoint)
            || (endpoint.Scheme != "https" && !(endpoint.Scheme == "http" && endpoint.IsLoopback))
            || targetUrl.Contains("your-backend.example.com"))
        {
            onError?.Invoke("Chưa cấu hình Backend Url. Dán URL Web App Apps Script kết thúc bằng /exec.");
            yield break;
        }

        string json = JsonUtility.ToJson(new GradeRequest
        {
            action = "gradeEssay",
            questionID = questionID,
            essayText = essayText,
            answerText = essayText,
            imageBase64 = imageBase64
        });
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(targetUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.redirectLimit = 8;
            www.timeout = Mathf.Max(5, Mathf.RoundToInt(timeoutSeconds));

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                string backendError = ReadBackendError(www.downloadHandler.text);
                onError?.Invoke(backendError ?? MapWebRequestError(www));
                yield break;
            }

            AIGradingResponse resp = ParseResponse(www.downloadHandler.text, out string parseErr);
            if (resp == null)
            {
                onError?.Invoke(parseErr ?? "Lỗi xử lý kết quả từ AI.");
                yield break;
            }
            onSuccess?.Invoke(resp);
        }
    }

    private IEnumerator RealCoroutine(string questionID, string imageBase64,
                                      Action<AIGradingResponse> onSuccess, Action<string> onError,
                                      string answerText = null)
    {
        return RealGradingCoroutine(questionID, answerText, imageBase64, onSuccess, onError);
    }

    private static bool IsValidResponse(AIGradingResponse response)
    {
        if (response == null) return false;
        if (!response.readable) return true;
        return !float.IsNaN(response.score) && !float.IsInfinity(response.score)
            && !float.IsNaN(response.maxScore) && !float.IsInfinity(response.maxScore)
            && response.maxScore > 0f && response.score >= 0f
            && response.score <= response.maxScore * 1.05f;
    }

    private AIGradingResponse ParseResponse(string text, out string error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            error = "INVALID_AI_RESPONSE";
            return null;
        }
        text = text.TrimStart('\uFEFF', ' ', '\r', '\n', '\t');
        if (!text.StartsWith("{", StringComparison.Ordinal))
        {
            error = "Backend không trả JSON. Kiểm tra URL /exec và quyền truy cập Web App Apps Script.";
            return null;
        }
        error = ReadBackendError(text);
        if (error != null) return null;
        try
        {
            AIGradingResponse r = JsonUtility.FromJson<AIGradingResponse>(text);
            if (r == null)
            {
                error = "Kết quả chấm không hợp lệ. Kiểm tra Apps Script đã triển khai phiên bản chấm AI mới.";
                return null;
            }
            r.correctPoints = r.correctPoints ?? new List<string>();
            r.missingPoints = r.missingPoints ?? new List<string>();
            r.errorsIdentified = r.errorsIdentified ?? new List<string>();
            r.stepBreakdown = r.stepBreakdown ?? new List<AIEvaluationStep>();
            r.feedback = string.IsNullOrEmpty(r.feedback) ? "" : r.feedback;
            r.suggestions = string.IsNullOrEmpty(r.suggestions) ? "" : r.suggestions;
            r.suggestedAnswer = string.IsNullOrEmpty(r.suggestedAnswer) ? "" : r.suggestedAnswer;
            if (!IsValidResponse(r))
            {
                error = "Kết quả chấm không hợp lệ. Kiểm tra Apps Script đã triển khai phiên bản chấm AI mới.";
                return null;
            }
            if (r.readable && r.maxScore > 0f)
            {
                r.percentage = 100f * r.score / r.maxScore;
            }
            return r;
        }
        catch (Exception e)
        {
            error = "INVALID_AI_RESPONSE: " + e.Message;
            return null;
        }
    }

    private static string ReadBackendError(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        text = text.TrimStart('\uFEFF', ' ', '\r', '\n', '\t');
        if (!text.StartsWith("{", StringComparison.Ordinal)) return null;
        try
        {
            BackendStatus status = JsonUtility.FromJson<BackendStatus>(text);
            if (status == null || (string.IsNullOrEmpty(status.error)
                && (status.success || string.IsNullOrEmpty(status.message)))) return null;
            string code = status.error ?? "BACKEND_ERROR";
            if (code.Length > 80) code = "BACKEND_ERROR";
            string message = status.message;
            if (string.IsNullOrWhiteSpace(message) || message.Length > 1000)
                message = "Backend chưa chấm được. Hãy kiểm tra cấu hình Apps Script.";
            return code + ": " + message;
        }
        catch { return null; }
    }

    private static string MapWebRequestError(UnityWebRequest www)
    {
        switch (www.result)
        {
            case UnityWebRequest.Result.ConnectionError:
                return Application.internetReachability == NetworkReachability.NotReachable ? "NO_INTERNET" : "CONNECTION_ERROR";
            case UnityWebRequest.Result.ProtocolError:
                return "BACKEND_ERROR_" + www.responseCode;
            case UnityWebRequest.Result.DataProcessingError:
                return "INVALID_AI_RESPONSE";
            default:
                return "UNKNOWN_ERROR";
        }
    }
}
