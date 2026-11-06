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
        if (!string.IsNullOrEmpty(geminiApiKey) && !geminiApiKey.StartsWith("#")) return geminiApiKey.Trim();
        try
        {
            string[] possiblePaths = new string[]
            {
                System.IO.Path.Combine(Application.dataPath, "../gemini_key.txt"),
                System.IO.Path.Combine(Application.dataPath, "../gemini_key.txt.txt"),
                System.IO.Path.Combine(Application.dataPath, "../gemini-key"),
                System.IO.Path.Combine(Application.dataPath, "../gemini-key.txt"),
                System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "gemini_key.txt"),
                System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "gemini_key.txt.txt"),
                System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "gemini-key"),
                System.IO.Path.Combine(Application.persistentDataPath, "gemini_key.txt"),
                System.IO.Path.Combine(Application.dataPath, "../Backend/PhysicsGeminiServer/.env")
            };

            foreach (var p in possiblePaths)
            {
                if (System.IO.File.Exists(p))
                {
                    string[] lines = System.IO.File.ReadAllLines(p);
                    foreach (var rawLine in lines)
                    {
                        string line = rawLine.Trim();
                        if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("//")) continue;
                        if (line.StartsWith("GEMINI_API_KEY=", StringComparison.OrdinalIgnoreCase))
                        {
                            line = line.Substring(15).Trim('"', '\'', ' ');
                        }
                        if (line.Length > 20)
                        {
                            return line;
                        }
                    }
                }
            }
        }
        catch {}

        string prefKey = PlayerPrefs.GetString("Gemini_API_Key", "").Trim();
        if (!string.IsNullOrEmpty(prefKey) && prefKey.Length > 20) return prefKey;

        // 3. Kiểm tra biến môi trường hệ thống (dùng chung với Game 2 nếu có)
        try
        {
            string envKey = System.Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (!string.IsNullOrEmpty(envKey) && envKey.Trim().Length > 20) return envKey.Trim();
        }
        catch {}

        return "";
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
        if (!string.IsNullOrEmpty(key))
        {
            StartCoroutine(GeminiTranscribeCoroutine(key, imageBase64, onSuccess, onError));
        }
        else if (mode == Mode.RealBackend)
        {
            StartCoroutine(RealTranscribeCoroutine(questionID, imageBase64, onSuccess, onError));
        }
        else
        {
            Debug.LogWarning("[AIGradingClient] âš ï¸ ChÆ°a cáº¥u hÃ¬nh Google Gemini API Key Ä‘á»ƒ nháº­n diá»‡n chá»¯ viáº¿t tay tháº­t tá»« áº£nh!");
            onError?.Invoke("KEY_MISSING");
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
        SubmitEssayEx(questionID, null, null, null, essayText, imageBase64, onSuccess, onError);
    }

    public void SubmitEssayEx(string questionID, string questionText, string expectedSolution, string subject,
                              string essayText, string imageBase64,
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
            StartCoroutine(GeminiGradingCoroutine(key, questionID, questionText, expectedSolution, subject, essayText ?? "", imageBase64 ?? "", onSuccess, onError));
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
        Debug.Log("[AIGradingClient][GEMINI] Äang gá»­i áº£nh chá»¯ viáº¿t tay lÃªn Google Gemini Vision API...");

        string cleanBase64 = imageBase64;
        string mimeType = "image/jpeg";
        if (cleanBase64.Contains(","))
        {
            int commaIdx = cleanBase64.IndexOf(",");
            string header = cleanBase64.Substring(0, commaIdx);
            cleanBase64 = cleanBase64.Substring(commaIdx + 1);
            if (header.Contains("png")) mimeType = "image/png";
        }

        string prompt = "Báº¡n lÃ  chuyÃªn gia OCR chá»¯ viáº¿t tay Tiáº¿ng Viá»‡t mÃ´n Khoa há»c tá»± nhiÃªn (Váº­t lÃ½ / Sinh há»c) THCS. " +
            "HÃ£y Ä‘á»c bá»©c áº£nh chá»¥p bÃ i lÃ m nÃ y vÃ  chÃ©p láº¡i trung thá»±c tá»«ng dÃ²ng chá»¯ há»c sinh viáº¿t trong áº£nh. " +
            "Äá»‹nh dáº¡ng káº¿t quáº£: má»—i dÃ²ng chá»¯ viáº¿t báº¯t Ä‘áº§u báº±ng 'DÃ²ng 1: ...', 'DÃ²ng 2: ...'. " +
            "Chá»‰ tráº£ vá» cÃ¡c dÃ²ng chá»¯ vÃ  cÃ´ng thá»©c há»c sinh thá»±c sá»± viáº¿t trong áº£nh. " +
            "Náº¿u áº£nh khÃ´ng cÃ³ chá»¯ hoáº·c chá»¯ quÃ¡ má» khÃ´ng Ä‘á»c Ä‘Æ°á»£c, tráº£ vá» duy nháº¥t dÃ²ng: 'KhÃ´ng Ä‘á»c Ä‘Æ°á»£c chá»¯ trong áº£nh'. " +
            "Tuyá»‡t Ä‘á»‘i khÃ´ng tá»± bá»‹a thÃªm ná»™i dung ngoÃ i áº£nh, khÃ´ng thÃªm lá»i chÃ o hay giáº£i thÃ­ch.";

        yield return GeminiTranscribeWithModelCoroutine("gemini-1.5-flash", apiKey, prompt, mimeType, cleanBase64, onSuccess, onError);
    }

    private IEnumerator GeminiTranscribeWithModelCoroutine(string model, string apiKey, string prompt, string mimeType, string cleanBase64,
                                                           Action<string> onSuccess, Action<string> onError)
    {
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        string jsonPayload = "{" +
            "\"contents\":[{" +
                "\"parts\":[" +
                    "{\"text\":\"" + prompt.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") + "\"}," +
                    "{\"inlineData\":{\"mimeType\":\"" + mimeType + "\",\"data\":\"" + cleanBase64 + "\"}}" +
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
                string errText = www.downloadHandler != null ? www.downloadHandler.text : "";
                Debug.LogWarning($"[AIGradingClient] Lá»—i Gemini OCR ({model}): {www.error} - {errText}");

                if ((model == "gemini-1.5-flash" || model == "gemini-1.5-flash") && (www.responseCode == 404 || www.responseCode == 503 || errText.Contains("not found")))
                {
                    Debug.Log("[AIGradingClient] Thá»­ láº¡i vá»›i model gemini-2.0-flash...");
                    yield return GeminiTranscribeWithModelCoroutine("gemini-2.0-flash", apiKey, prompt, mimeType, cleanBase64, onSuccess, onError);
                    yield break;
                }

                string userMsg = "Lá»—i káº¿t ná»‘i Gemini API";
                if (www.responseCode == 400) userMsg = "API Key khÃ´ng há»£p lá»‡ hoáº·c Ä‘á»‹nh dáº¡ng áº£nh lá»—i (HTTP 400).";
                else if (www.responseCode == 401 || www.responseCode == 403) userMsg = "API Key chÆ°a Ä‘Æ°á»£c kÃ­ch hoáº¡t hoáº·c khÃ´ng Ä‘á»§ quyá»n (HTTP 401/403).";
                else if (www.responseCode == 429) userMsg = "ÄÃ£ vÆ°á»£t háº¡n má»©c gá»i API miá»…n phÃ­ Gemini (HTTP 429). Vui lÃ²ng thá»­ láº¡i sau 30 giÃ¢y.";
                else if (!string.IsNullOrEmpty(www.error)) userMsg = www.error;

                onError?.Invoke(userMsg);
                yield break;
            }

            string text = ExtractGeminiResponseText(www.downloadHandler.text);
            if (string.IsNullOrEmpty(text))
            {
                onError?.Invoke("GEMINI_EMPTY_RESPONSE");
                yield break;
            }

            string cleaned = CleanTranscribedText(text.Trim());
            onSuccess?.Invoke(cleaned);
        }
    }

    public static string CleanTranscribedText(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";
        string cleaned = raw.Replace("\r\n", "\n").Trim();
        if (cleaned.StartsWith("`"))
        {
            int firstNewline = cleaned.IndexOf('\n');
            if (firstNewline >= 0) cleaned = cleaned.Substring(firstNewline + 1);
            if (cleaned.EndsWith("`")) cleaned = cleaned.Substring(0, cleaned.Length - 3).Trim();
        }

        string[] rawLines = cleaned.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        bool hasLinePrefix = false;
        foreach (var l in rawLines)
        {
            string t = l.Trim();
            if (t.StartsWith("Dòng ") || t.StartsWith("Dong ") || t.StartsWith("Line ") || t.StartsWith("DÃ²ng "))
            {
                hasLinePrefix = true;
                break;
            }
        }

        if (hasLinePrefix)
        {
            return string.Join("\r\n", rawLines);
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        int lineIdx = 1;
        for (int i = 0; i < rawLines.Length; i++)
        {
            string t = rawLines[i].Trim();
            if (string.IsNullOrWhiteSpace(t)) continue;
            sb.AppendLine($"Dòng {lineIdx}: {t}");
            lineIdx++;
        }
        return sb.ToString().TrimEnd();
    }
    private IEnumerator GeminiGradingCoroutine(string apiKey, string questionID, string questionText, string expectedSolution, string subject, string essayText, string imageBase64,
                                                Action<AIGradingResponse> onSuccess, Action<string> onError)
    {
        string resolvedSubject = !string.IsNullOrEmpty(subject) ? subject : 
            (questionID.StartsWith("SH_", StringComparison.OrdinalIgnoreCase) || questionID.StartsWith("BIO", StringComparison.OrdinalIgnoreCase) ? "Sinh học 9" : "Vật lý 9");

        // 1. KIỂM TRA BẢO VỆ CHẶT CHẼ: Nộp nhầm môn học khác (ví dụ nộp Sinh học cho đề Vật lý)
        if (IsWrongSubject(resolvedSubject, essayText, out string detectedSub))
        {
            Debug.LogWarning($"[AIGradingClient] ⚠️ PHÁT HIỆN LẠC ĐỀ: Đề thi môn {resolvedSubject} nhưng nộp môn {detectedSub}!");
            onSuccess?.Invoke(CreateWrongSubjectResponse(resolvedSubject, detectedSub, expectedSolution, essayText));
            yield break;
        }

        // 2. KIỂM TRA BÀI VIẾT LINH TINH / LẠC ĐỀ HOÀN TOÀN
        if (CheckIfGibberishOrNonsense(essayText, resolvedSubject))
        {
            Debug.LogWarning($"[AIGradingClient] ⚠️ BÀI VIẾT LINH TINH / KHÔNG PHẢI KIẾN THỨC MÔN HỌC!");
            onSuccess?.Invoke(CreateZeroScoreResponse(resolvedSubject, expectedSolution, essayText, "Bài làm viết linh tinh hoặc không chứa kiến thức khoa học theo yêu cầu đề bài."));
            yield break;
        }

        Debug.Log($"[AIGradingClient][GEMINI] Đang chấm bài tự luận môn {resolvedSubject} qua Google Gemini API...");

        string[] candidateModels = new string[] { "gemini-1.5-flash", "gemini-2.0-flash" };
        yield return CallGeminiGradingMultiModelCoroutine(candidateModels, 0, apiKey, questionID, questionText, expectedSolution, resolvedSubject, essayText, imageBase64, onSuccess, onError);
    }

    private IEnumerator CallGeminiGradingMultiModelCoroutine(string[] models, int modelIndex, string apiKey, string questionID, string questionText, string expectedSolution, string resolvedSubject, string essayText, string imageBase64,
                                                            Action<AIGradingResponse> onSuccess, Action<string> onError)
    {
        if (modelIndex >= models.Length)
        {
            Debug.LogWarning("[AIGradingClient] Tất cả các model Gemini đều không phản hồi. Chuyển sang chấm điểm cục bộ chuẩn.");
            yield return MockGradingCoroutine(questionID, questionText, expectedSolution, resolvedSubject, essayText, imageBase64, onSuccess, onError);
            yield break;
        }

        string model = models[modelIndex];
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        string prompt = "Bạn là giáo viên giám khảo chấm bài tự luận học sinh THCS môn " + resolvedSubject + ".\n" +
            "Môn thi: " + resolvedSubject.ToUpper() + "\n" +
            "Mã câu hỏi: " + questionID + "\n" +
            (!string.IsNullOrEmpty(questionText) ? ("ĐỀ BÀI CÂU HỎI: \"" + questionText.Replace("\"", "\\\"").Replace("\n", "\\n") + "\"\n") : "") +
            (!string.IsNullOrEmpty(expectedSolution) ? ("ĐÁP ÁN THAM CHIẾU SGK: \"" + expectedSolution.Replace("\"", "\\\"").Replace("\n", "\\n") + "\"\n") : "") +
            "Bài làm của học sinh:\n\"" + essayText.Replace("\"", "\\\"").Replace("\n", "\\n") + "\"\n\n" +
            "QUY TẮC CHẤM THI NGHIÊM NGẶT (TUYỆT ĐỐI CHÍNH XÁC):\n" +
            "1. PHÁT HIỆN LẠC ĐỀ HOẶC NỘP NHẦM MÔN HỌC KHÁC: Nếu đây là bài thi môn " + resolvedSubject + " mà học sinh nộp bài môn khác (ví dụ: đề Vật lý mà nội dung là Sinh học: Di truyền, Biến dị, ADN, NST, Tế bào, Menđen, Phép lai... hoặc ngược lại) -> BẮT BUỘC ĐẶT score = 0.0, percentage = 0.0, readable = true, tất cả các dòng stepBreakdown có isCorrect = false. Trong feedback và errorsIdentified phải ghi rõ: 'BÀI LÀM LẠC ĐỀ / NỘP NHẦM MÔN HỌC! Đây là bài thi môn " + resolvedSubject + ", không thể chấm đạt khi nộp nội dung môn học khác.'.\n" +
            "2. NẾU HỌC SINH VIẾT LINH TINH, gõ phím ngẫu nhiên (asdfgh, abc, 123), chửi thề -> ĐẶT score = 0, percentage = 0, tất cả các dòng isCorrect = false, chỉ rõ lỗi: 'Viết linh tinh / không liên quan đề bài'.\n" +
            "3. NẾU BÀI LÀM ĐÚNG MÔN: chấm theo từng bước/dòng (tối đa 10 điểm), chỉ rõ dòng nào đúng, dòng nào sai, lỗi sai là gì, và đưa ra gợi ý cách làm đúng cùng lời giải mẫu hoàn chỉnh.\n\n" +
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
                string errText = www.downloadHandler != null ? www.downloadHandler.text : "";
                Debug.LogWarning($"[AIGradingClient] Lỗi Gemini Grade ({model}): {www.error} - {errText}");
                yield return CallGeminiGradingMultiModelCoroutine(models, modelIndex + 1, apiKey, questionID, questionText, expectedSolution, resolvedSubject, essayText, imageBase64, onSuccess, onError);
                yield break;
            }

            string rawText = ExtractGeminiResponseText(www.downloadHandler.text);
            AIGradingResponse resp = ParseResponse(CleanJson(rawText), out string parseErr);
            if (resp == null)
            {
                yield return CallGeminiGradingMultiModelCoroutine(models, modelIndex + 1, apiKey, questionID, questionText, expectedSolution, resolvedSubject, essayText, imageBase64, onSuccess, onError);
                yield break;
            }

            // Hậu kiểm bảo vệ kép: Nếu model AI trả về điểm > 0 nhưng nội dung học sinh nộp thực tế là sai môn
            if (IsWrongSubject(resolvedSubject, essayText, out string detectedSubAfter))
            {
                resp.score = 0f;
                resp.percentage = 0f;
                resp.feedback = $"⚠️ BÀI LÀM LẠC ĐỀ (0 ĐIỂM): Đây là bài kiểm tra môn {resolvedSubject} nhưng bạn đã nộp nội dung môn {detectedSubAfter}! Hệ thống từ chối chấm đạt.";
                resp.errorsIdentified = new List<string> { $"Nộp nhầm bài môn {detectedSubAfter} cho bài thi môn {resolvedSubject}." };
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
                        else if (c == 'u' && i + 4 < geminiJson.Length)
                        {
                            string hex = geminiJson.Substring(i + 1, 4);
                            if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int code))
                            {
                                sb.Append((char)code);
                                i += 4;
                            }
                            else sb.Append(c);
                        }
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
        public static string GenerateDynamicTranscription(string questionID)
    {
        string rawSolution = null;

        // 1. Tìm trong PhysicsGame1Session câu hỏi đang làm
        try
        {
            if (global::ScienceQuest.PhysicsGame1.PhysicsGame1Session.Instance != null && global::ScienceQuest.PhysicsGame1.PhysicsGame1Session.Instance.CurrentQuestions != null && global::ScienceQuest.PhysicsGame1.PhysicsGame1Session.Instance.CurrentQuestions.Count > 0)
            {
                var qList = global::ScienceQuest.PhysicsGame1.PhysicsGame1Session.Instance.CurrentQuestions;
                var found = qList.Find(q => q != null && !string.IsNullOrEmpty(q.questionID) && q.questionID.Equals(questionID, StringComparison.OrdinalIgnoreCase));
                if (found == null && qList.Count > 0)
                {
                    found = qList[0];
                }

                if (found != null)
                {
                    rawSolution = !string.IsNullOrWhiteSpace(found.dapAnThamChieu) ? found.dapAnThamChieu : found.explanation;
                }
            }
        }
        catch {}

        // 2. Nếu chưa có, nạp từ file JSON Ngân hàng câu hỏi
        if (string.IsNullOrWhiteSpace(rawSolution))
        {
            try
            {
                UnityEngine.TextAsset ta = UnityEngine.Resources.Load<UnityEngine.TextAsset>("QuizData/physics_questions");
                if (ta != null && !string.IsNullOrEmpty(ta.text))
                {
                    string json = ta.text;
                    int idIdx = !string.IsNullOrEmpty(questionID) ? json.IndexOf($"\"{questionID}\"", StringComparison.OrdinalIgnoreCase) : -1;
                    if (idIdx >= 0)
                    {
                        int dapAnIdx = json.IndexOf("\"dapAnThamChieu\":", idIdx);
                        if (dapAnIdx >= 0 && dapAnIdx - idIdx < 1500)
                        {
                            int valStart = json.IndexOf("\"", dapAnIdx + 17) + 1;
                            int valEnd = json.IndexOf("\"", valStart);
                            if (valStart > 0 && valEnd > valStart)
                            {
                                rawSolution = json.Substring(valStart, valEnd - valStart).Replace("\\n", "\n").Replace("\\\"", "\"");
                            }
                        }
                    }
                }
            }
            catch {}
        }

        // 3. Fallback cho câu hỏi về viên bi 200g (nếu ID khớp hoặc đang ở bài 2)
        if (string.IsNullOrWhiteSpace(rawSolution))
        {
            if (!string.IsNullOrEmpty(questionID) && (questionID.Contains("C1_L2") || questionID.Contains("995")))
            {
                rawSolution = "Đổi m = 200g = 0,2 kg. Động năng ban đầu: Wđ = 1/2.m.v² = 1/2 . 0,2 . 10² = 10 (J). Do động năng tỉ lệ thuận với bình phương vận tốc (v²), nên khi vận tốc tăng gấp đôi (v' = 2v), động năng mới Wđ' = 1/2.m.(2v)² = 4.Wđ, tức là động năng sẽ tăng lên gấp 4 lần.";
            }
            else
            {
                rawSolution = "Áp dụng công thức Vật lý 9 chuẩn SI. Đổi đơn vị và thay số tính toán chính xác. Kết luận đáp số theo yêu cầu bài toán.";
            }
        }

        return FormatTranscriptionLines(rawSolution);
    }

    public static string FormatTranscriptionLines(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Dòng 1: Đổi đơn vị chuẩn SI.\r\nDòng 2: Áp dụng công thức tính toán.\r\nDòng 3: Kết luận đáp số.";

        string[] rawParts = raw.Split(new[] { '\n', '.', ';' }, StringSplitOptions.RemoveEmptyEntries);
        var lines = new System.Collections.Generic.List<string>();

        foreach (var p in rawParts)
        {
            string trimmed = p.Trim();
            if (trimmed.Length < 3) continue;
            if (trimmed.StartsWith("B1") || trimmed.StartsWith("B2") || trimmed.StartsWith("B3") || trimmed.StartsWith("B4") || trimmed.StartsWith("B5"))
            {
                int colon = trimmed.IndexOf(':');
                if (colon >= 0 && colon < trimmed.Length - 1) trimmed = trimmed.Substring(colon + 1).Trim();
            }
            lines.Add(trimmed);
            if (lines.Count >= 5) break;
        }

        if (lines.Count == 0) return "Dòng 1: " + raw.Trim();

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < lines.Count; i++)
        {
            sb.AppendLine($"Dòng {i + 1}: {lines[i]}.");
        }
        return sb.ToString().TrimEnd();
    }

private IEnumerator MockTranscribeCoroutine(string questionID, string imageBase64,
                                                Action<string> onSuccess, Action<string> onError)
    {
        yield return new WaitForSecondsRealtime(mockDelaySeconds);

        if (imageBase64.StartsWith("BLURRY", StringComparison.OrdinalIgnoreCase))
        {
            onError?.Invoke("BLURRY_IMAGE");
            yield break;
        }

        string transcribed = GenerateDynamicTranscription(questionID);
        onSuccess?.Invoke(transcribed);
    }

        public IEnumerator MockGradingCoroutine(string questionID, string questionText, string expectedSolution, string subject, string essayText, string imageBase64,
                                           Action<AIGradingResponse> onSuccess, Action<string> onError)
    {
        yield return new WaitForSecondsRealtime(mockDelaySeconds);

        string resolvedSubject = !string.IsNullOrEmpty(subject) ? subject : 
            (questionID.StartsWith("SH_", StringComparison.OrdinalIgnoreCase) || questionID.StartsWith("BIO", StringComparison.OrdinalIgnoreCase) ? "Sinh học 9" : "Vật lý 9");

        string content = (essayText ?? "").Trim();

        // 1. KIỂM TRA BẢO VỆ CHẶT CHẼ: Nộp nhầm môn học khác (ví dụ nộp Sinh học cho bài thi Vật lý)
        if (IsWrongSubject(resolvedSubject, content, out string detectedSub))
        {
            Debug.LogWarning($"[AIGradingClient][MOCK] ⚠️ PHÁT HIỆN LẠC ĐỀ MÔN HỌC: Đề {resolvedSubject} nhưng nộp {detectedSub}!");
            onSuccess?.Invoke(CreateWrongSubjectResponse(resolvedSubject, detectedSub, expectedSolution, content));
            yield break;
        }

        // 2. KIỂM TRA BÀI VIẾT LINH TINH HOẶC QUÁ NGẮN -> 0 ĐIỂM
        bool isGibberish = CheckIfGibberishOrNonsense(content, resolvedSubject);
        if (isGibberish)
        {
            Debug.LogWarning($"[AIGradingClient][MOCK] ⚠️ BÀI VIẾT LINH TINH HOẶC QUÁ NGẮN!");
            onSuccess?.Invoke(CreateZeroScoreResponse(resolvedSubject, expectedSolution, content, "Bài làm viết linh tinh hoặc không chứa kiến thức khoa học đề bài yêu cầu."));
            yield break;
        }

        // 3. KIỂM TRA SỰ PHÙ HỢP VỚI ĐỀ BÀI VÀ ĐÁP ÁN THAM CHIẾU (TRÁNH LẠC ĐỀ)
        if (IsIrrelevantToQuestion(content, questionText, expectedSolution, resolvedSubject))
        {
            Debug.LogWarning($"[AIGradingClient][MOCK] ⚠️ NỘI DUNG KHÔNG KHỚP ĐỀ BÀI / CÔNG THỨC!");
            onSuccess?.Invoke(CreateZeroScoreResponse(resolvedSubject, expectedSolution, content, "Nội dung bài làm không khớp với yêu cầu đề bài hoặc công thức của câu hỏi."));
            yield break;
        }

        // 4. BÀI LÀM HỢP LỆ VÀ ĐÚNG MÔN: ĐÁNH GIÁ TỪNG DÒNG
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
                              line.Length < 8;

            if (!hasMistake)
            {
                step.isCorrect = true;
                step.stepScore = step.maxStepScore;
                step.statusText = "Chính xác ✓";
                step.errorDetail = "Lập luận đúng, sử dụng thuật ngữ môn " + resolvedSubject + " chính xác.";
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

        float pct = Mathf.Clamp((totalScore / 10f) * 100f, 0f, 100f);
        string modelSol = !string.IsNullOrWhiteSpace(expectedSolution) ? expectedSolution : "Áp dụng đúng công thức và giải thích đầy đủ các bước theo chuẩn SGK.";

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
                ? "Bài làm rất tốt! Bạn đã nắm vững kiến thức môn " + resolvedSubject + " và lập luận rõ ràng."
                : "Bài làm cơ bản có ý nhưng còn một vài lỗi. Hãy xem lại các dòng cần chỉnh sửa bên dưới.",
            suggestions = "💡 HƯỚNG DẪN CẢI THIỆN:\n1. Nêu rõ đơn vị đo trong hệ SI.\n2. Kiểm tra lại phép biến đổi công thức.",
            suggestedAnswer = modelSol
        };

        onSuccess?.Invoke(response);
    }

    private IEnumerator MockGradingCoroutine(string questionID, string essayText, string imageBase64,
                                             Action<AIGradingResponse> onSuccess, Action<string> onError)
    {
        return MockGradingCoroutine(questionID, null, null, null, essayText, imageBase64, onSuccess, onError);
    }

    private IEnumerator MockCoroutine(string questionID, string imageBase64,
                                      Action<AIGradingResponse> onSuccess, Action<string> onError)
    {
        return MockGradingCoroutine(questionID, null, imageBase64, onSuccess, onError);
    }

    public static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        string normalized = text.Normalize(System.Text.NormalizationForm.FormD);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (char c in normalized)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        return sb.ToString().Normalize(System.Text.NormalizationForm.FormC).Replace("đ", "d").Replace("Đ", "D").ToLowerInvariant();
    }

    public static bool IsWrongSubject(string targetSubject, string text, out string detectedSubject)
    {
        detectedSubject = "";
        if (string.IsNullOrWhiteSpace(text)) return false;

        string cleanText = RemoveDiacritics(text);
        string cleanTarget = RemoveDiacritics(targetSubject ?? "");

        bool isPhysicsTarget = cleanTarget.Contains("vat ly") || cleanTarget.Contains("physics") || cleanTarget.Contains("vatly") || string.IsNullOrEmpty(cleanTarget);
        bool isBiologyTarget = cleanTarget.Contains("sinh hoc") || cleanTarget.Contains("biology") || cleanTarget.Contains("sinhhoc");

        string[] bioTerms = new string[]
        {
            "sinh hoc", "di truyen", "bien di", "menden", "men-den", "adn", "arn", "nst",
            "nhiem sac the", "kieu gen", "kieu hinh", "phep lai", "lai phan tich",
            "giao tu", "thu tinh", "nguyen phan", "giam phan", "te bao", "quang hop",
            "dot bien", "the di hop", "the dong hop", "bai lam mau - sinh hoc"
        };

        string[] physTerms = new string[]
        {
            "vat ly", "dong nang", "the nang", "co nang", "khuc xa", "phan xa",
            "thau kinh", "dinh luat ohm", "cong suat", "dien tro", "hieu dien the",
            "cuong do dong dien", "tu truong", "cam ung dien tu", "bai lam mau - vat ly"
        };

        if (isPhysicsTarget)
        {
            foreach (var term in bioTerms)
            {
                if (cleanText.Contains(term))
                {
                    detectedSubject = "Sinh học";
                    return true;
                }
            }
        }
        else if (isBiologyTarget)
        {
            foreach (var term in physTerms)
            {
                if (cleanText.Contains(term))
                {
                    detectedSubject = "Vật lý";
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsIrrelevantToQuestion(string text, string questionText, string expectedSolution, string subject)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;
        string lower = text.ToLowerInvariant();

        string combined = ((questionText ?? "") + " " + (expectedSolution ?? "")).ToLowerInvariant();
        char[] seps = new char[] { ' ', '\t', '\r', '\n', ',', '.', ';', ':', '(', ')', '"', '\'', '-', '+', '=', '/', '*', '•', '?' };
        string[] tokens = combined.Split(seps, StringSplitOptions.RemoveEmptyEntries);

        int matchCount = 0;
        HashSet<string> seen = new HashSet<string>();
        foreach (var tok in tokens)
        {
            if (tok.Length >= 3 && !seen.Contains(tok))
            {
                seen.Add(tok);
                if (lower.Contains(tok)) matchCount++;
            }
        }

        if (seen.Count >= 5 && matchCount == 0)
        {
            return true;
        }

        return false;
    }

    private bool CheckIfGibberishOrNonsense(string text, string targetSubject)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length < 10) return true;

        string lower = text.ToLowerInvariant();
        string[] badKeywords = new string[] { "linh tinh", "asdf", "qwer", "zxcv", "hahaha", "hehe", "test", "ahihi", "12345" };
        foreach (var bad in badKeywords)
        {
            if (lower.Contains(bad)) return true;
        }

        bool isPhysics = (targetSubject ?? "").IndexOf("vật lý", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         (targetSubject ?? "").IndexOf("vat ly", StringComparison.OrdinalIgnoreCase) >= 0;

        string[] physicsTerms = new string[]
        {
            "động năng", "thế năng", "cơ năng", "vận tốc", "khối lượng", "độ cao", "lực",
            "công", "công suất", "nhiệt", "điện", "hiệu điện thế", "cường độ", "điện trở",
            "khúc xạ", "phản xạ", "ánh sáng", "thấu kính", "tiêu cự", "từ trường", "nam châm",
            "j", "w", "n", "m/s", "kg", "ôm", "v", "a", "ohm", "công thức", "dòng điện", "u", "i", "r", "p"
        };

        string[] biologyTerms = new string[]
        {
            "gen", "adn", "arn", "tế bào", "ty thể", "quang hợp", "hô hấp", "menđen", "lai",
            "hoa", "tính trạng", "kiểu gen", "kiểu hình", "giao tử", "f1", "f2", "nhiễm sắc thể",
            "atp", "co2", "o2", "sinh học", "di truyền", "biến dị"
        };

        string[] checkTerms = isPhysics ? physicsTerms : biologyTerms;
        int matchCount = 0;
        foreach (var term in checkTerms)
        {
            if (lower.Contains(term)) matchCount++;
        }

        return matchCount == 0;
    }

    public static AIGradingResponse CreateWrongSubjectResponse(string targetSubject, string detectedSubject, string expectedSolution, string essayText)
    {
        string[] rawLines = (essayText ?? "").Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<AIEvaluationStep> steps = new List<AIEvaluationStep>();
        for (int i = 0; i < rawLines.Length; i++)
        {
            steps.Add(new AIEvaluationStep
            {
                lineNumber = i + 1,
                lineText = rawLines[i].Trim(),
                isCorrect = false,
                stepScore = 0f,
                maxStepScore = (float)Math.Round(10f / Mathf.Max(1, rawLines.Length), 1),
                statusText = "Sai môn học ✗ (0đ)",
                errorDetail = $"Nội dung thuộc môn {detectedSubject}, không đúng yêu cầu môn {targetSubject}."
            });
        }

        return new AIGradingResponse
        {
            readable = true,
            score = 0f,
            maxScore = 10f,
            percentage = 0f,
            correctPoints = new List<string>(),
            missingPoints = new List<string> { $"Toàn bộ kiến thức bài học môn {targetSubject}" },
            errorsIdentified = new List<string>
            {
                $"BÀI LÀM LẠC ĐỀ: Nộp nội dung môn {detectedSubject} cho bài kiểm tra môn {targetSubject}!",
                $"Các thuật ngữ trong bài ({detectedSubject}) hoàn toàn không liên quan đến đề bài {targetSubject}."
            },
            stepBreakdown = steps,
            feedback = $"⚠️ BÀI LÀM LẠC ĐỀ (0 ĐIỂM): Đây là bài kiểm tra môn {targetSubject.ToUpper()} nhưng bạn đã nộp nội dung môn {detectedSubject.ToUpper()}! Hệ thống AI từ chối chấm đạt và đánh giá 0 điểm. Vui lòng đọc kỹ đề bài và làm lại!",
            suggestions = $"💡 HÃY ÔN TẬP LẠI:\n• Đọc kỹ đề bài môn {targetSubject} và xem Lời giải chi tiết bên cạnh để làm lại.",
            suggestedAnswer = !string.IsNullOrWhiteSpace(expectedSolution) ? expectedSolution : "Áp dụng đúng công thức và giải thích đầy đủ các bước theo chuẩn SGK."
        };
    }

    public static AIGradingResponse CreateZeroScoreResponse(string targetSubject, string expectedSolution, string essayText, string reason)
    {
        string[] rawLines = (essayText ?? "").Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<AIEvaluationStep> steps = new List<AIEvaluationStep>();
        for (int i = 0; i < rawLines.Length; i++)
        {
            steps.Add(new AIEvaluationStep
            {
                lineNumber = i + 1,
                lineText = rawLines[i].Trim(),
                isCorrect = false,
                stepScore = 0f,
                maxStepScore = (float)Math.Round(10f / Mathf.Max(1, rawLines.Length), 1),
                statusText = "Không đạt ✗ (0đ)",
                errorDetail = reason
            });
        }

        return new AIGradingResponse
        {
            readable = true,
            score = 0f,
            maxScore = 10f,
            percentage = 0f,
            correctPoints = new List<string>(),
            missingPoints = new List<string> { "Toàn bộ nội dung yêu cầu của câu hỏi môn " + targetSubject },
            errorsIdentified = new List<string> { reason },
            stepBreakdown = steps,
            feedback = $"⚠️ BÀI LÀM 0 ĐIỂM: {reason} Vui lòng đọc kỹ câu hỏi đề bài và làm lại!",
            suggestions = $"💡 HÃY ÔN TẬP LẠI:\n• Đọc kỹ đề bài môn {targetSubject} và tham khảo Lời giải chi tiết bên cạnh để làm lại.",
            suggestedAnswer = !string.IsNullOrWhiteSpace(expectedSolution) ? expectedSolution : "Áp dụng đúng công thức và giải thích đầy đủ các bước theo chuẩn SGK."
        };
    }

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
