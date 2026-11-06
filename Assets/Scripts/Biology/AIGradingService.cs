using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace ScienceQuest.Biology
{
    /// <summary>
    /// Dịch vụ chấm điểm bài thi tự luận Sinh học bằng AI (Google Gemini API).
    /// Có khả năng phân tích ngữ nghĩa, hiểu từ đồng nghĩa, giải thích ngắn gọn và đưa ra đáp án đúng.
    /// Nếu mất mạng hoặc chưa nhập API Key, hệ thống sẽ tự động chuyển sang chế độ chấm dự phòng (Fallback)
    /// và vẫn cung cấp đầy đủ nhận xét cùng đáp án đúng để người học nắm vững kiến thức.
    /// </summary>
    public static class AIGradingService
    {
        // =========================================================================
        // 👇 DÁN API KEY GOOGLE GEMINI CỦA BẠN VÀO ĐÂY (Lấy miễn phí tại: https://aistudio.google.com/app/apikey)
        // Hoặc nhập tại trường Gemini Api Key trên QuizManager trong Inspector, hoặc file .env
        // =========================================================================
        public static string GeminiApiKey = "geminiApiKey";
        // =========================================================================

        public static string ModelName = "gemini-1.5-flash";

        /// <summary>
        /// Kết quả trả về sau khi AI chấm bài
        /// </summary>
        [Serializable]
        public class GradeResult
        {
            public bool isCorrect;          // Đạt yêu cầu hay không (true/false)
            public int score;               // Điểm số thang 10
            public string feedback;         // Nhận xét ngắn gọn giải thích tại sao đúng hoặc sai
            public string suggestedAnswer;  // Đáp án khoa học mẫu chính xác cho câu hỏi
        }

        /// <summary>
        /// Kiểm tra xem API Key đã được cấu hình hay chưa.
        /// Tìm lần lượt từ: QuizManager Inspector -> PlayerPrefs -> Biến tĩnh trong code -> Biến môi trường Windows -> File .env
        /// </summary>
        public static string GetActiveApiKey()
        {
            // 1. Kiểm tra từ QuizManager nếu có gán trong Inspector
            try
            {
                if (QuizManager.Instance != null && !string.IsNullOrEmpty(QuizManager.Instance.geminiApiKey) && !QuizManager.Instance.geminiApiKey.Contains("DÁN_API_KEY"))
                {
                    return QuizManager.Instance.geminiApiKey.Trim();
                }
            }
            catch {}

            // 2. Kiểm tra PlayerPrefs
            string savedKey = PlayerPrefs.GetString("Gemini_ApiKey", "").Trim();
            if (!string.IsNullOrEmpty(savedKey) && !savedKey.Contains("DÁN_API_KEY")) return savedKey;

            // 3. Kiểm tra biến tĩnh trong code
            if (!string.IsNullOrEmpty(GeminiApiKey) && !GeminiApiKey.Contains("DÁN_API_KEY"))
            {
                return GeminiApiKey.Trim();
            }

            // 4. Kiểm tra biến môi trường hệ thống Windows (User, Machine, Process)
            try
            {
                string envKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.Process);
                if (!string.IsNullOrEmpty(envKey)) return envKey.Trim();

                envKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
                if (!string.IsNullOrEmpty(envKey)) return envKey.Trim();

                envKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.Machine);
                if (!string.IsNullOrEmpty(envKey)) return envKey.Trim();

                envKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
                if (!string.IsNullOrEmpty(envKey)) return envKey.Trim();
            }
            catch {}

            // 5. Kiểm tra file .env trong thư mục dự án
            try
            {
                string[] envPaths = new string[]
                {
                    System.IO.Path.Combine(Application.dataPath, "..", ".env"),
                    System.IO.Path.Combine(Application.dataPath, ".env"),
                    ".env"
                };
                foreach (var p in envPaths)
                {
                    if (System.IO.File.Exists(p))
                    {
                        foreach (var line in System.IO.File.ReadAllLines(p))
                        {
                            var trimmed = line.Trim();
                            if (trimmed.StartsWith("GEMINI_API_KEY=", StringComparison.OrdinalIgnoreCase))
                            {
                                string val = trimmed.Substring("GEMINI_API_KEY=".Length).Trim(' ', '"', '\'');
                                if (!string.IsNullOrEmpty(val)) return val;
                            }
                        }
                    }
                }
            }
            catch {}

            return "";
        }

        /// <summary>
        /// Quá tải chấm điểm dạng text thuần (tương thích ngược).
        /// </summary>
        public static IEnumerator GradeAnswer(string questionText, string studentAnswer, string sampleAnswer, string keywords, Action<GradeResult> onDone)
        {
            return GradeAnswer(questionText, studentAnswer, sampleAnswer, keywords, "", null, onDone);
        }

        /// <summary>
        /// Quá tải có ảnh bài làm.
        /// </summary>
        public static IEnumerator GradeAnswer(string questionText, string studentAnswer, string sampleAnswer, string keywords, byte[] studentAnswerImage, Action<GradeResult> onDone)
        {
            return GradeAnswer(questionText, studentAnswer, sampleAnswer, keywords, "", studentAnswerImage, onDone);
        }

        /// <summary>
        /// Coroutine gửi bài làm của học sinh lên Gemini AI để chấm điểm ngữ nghĩa.
        /// Hỗ trợ: text thông thường (null = không ảnh) hoặc ảnh bài viết (inline_data base64).
        /// </summary>
        public static IEnumerator GradeAnswer(string questionText, string studentAnswer, string sampleAnswer, string keywords, string explanation, byte[] studentAnswerImage, Action<GradeResult> onDone)
        {
            string apiKey = GetActiveApiKey();

            // Nếu chưa có API Key, dùng thuật toán chấm từ khóa cục bộ
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogWarning("[AIGradingService] ⚠️ Chưa cấu hình API Key. Dùng thuật toán chấm dự phòng (Fallback) có kèm đáp án đúng.");
                GradeResult fallback = FallbackGrade(studentAnswer, sampleAnswer, keywords, explanation, studentAnswerImage);
                onDone?.Invoke(fallback);
                yield break;
            }

            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelName}:generateContent?key={apiKey}";

            // Xây dựng Prompt chuẩn sư phạm Sinh học
            string prompt = $@"Bạn là giám khảo chuyên chấm thi bộ môn Sinh học phổ thông.
Nhiệm vụ của bạn là đánh giá câu trả lời tự luận của học sinh một cách công tâm, nghiêm túc và chính xác.

[THÔNG TIN BÀI THI]
- Câu hỏi: {questionText}
- Đáp án mẫu / Kiến thức chuẩn: {sampleAnswer}
- Giải thích chi tiết từ đề bài: {explanation}
- Từ khóa/ý chính bắt buộc có: {keywords}
- Bài làm của học sinh (dạng chữ): {studentAnswer}
{GetImageNote(studentAnswerImage)}

[QUY TẮC ĐÁNH GIÁ NGHIÊM NGẶT]
1. NẾU CÓ ẢNH ĐÍNH KÈM:
   - Hãy quan sát và đọc kỹ nội dung bài làm trong ảnh.
   - NGUYÊN TẮC BẮT BUỘC: Nếu ảnh gửi lên là ảnh linh tinh, ảnh rác, ảnh chụp màn hình máy tính, ảnh đồ họa UI, ảnh phong cảnh hoặc nội dung KHÔNG LIÊN QUAN gì đến câu hỏi Sinh học này: BẮT BUỘC đánh giá isCorrect = false. Tuyệt đối KHÔNG ĐƯỢC chấm đúng cho ảnh không liên quan!
2. TIÊU CHUẨN ĐÚNG / CHƯA ĐÚNG:
   - Chỉ cho isCorrect = true khi bài làm (qua chữ hoặc qua ảnh) trả lời đúng trọng tâm và đúng bản chất kiến thức Sinh học của câu hỏi (đạt từ 50-60% ý cốt lõi trở lên).
   - Nếu câu trả lời sai lệch bản chất, trả lời bừa, lạc đề, 'tao k biết', 'không biết', chép linh tinh hoặc quá sơ sài: Cho isCorrect = false.
3. PHẦN GIẢI THÍCH (feedback):
   - Ngắn gọn 1-2 câu, nêu rõ vì sao câu trả lời đúng hoặc còn thiếu ý nào.
   - TUYỆT ĐỐI KHÔNG ghi điểm số (ví dụ: không ghi 3/10, 9/10, không ghi điểm) và KHÔNG dùng chữ 'Đạt / Chưa đạt'. Chỉ tập trung giải thích kiến thức khoa học.
4. PHẦN ĐÁP ÁN ĐÚNG (suggestedAnswer):
   - Tóm tắt 1-2 câu đáp án khoa học mẫu chính xác, chuẩn xác theo bài học để học sinh học hỏi.

[YÊU CẦU ĐỊNH DẠNG TRẢ VỀ]
BẮT BUỘC chỉ trả về DUY NHẤT một chuỗi JSON hợp lệ (không kèm markdown ```json hay văn bản thừa nào):
{{
  ""isCorrect"": false,
  ""score"": 0,
  ""feedback"": ""Nội dung giải thích ngắn gọn 1-2 câu ở đây"",
  ""suggestedAnswer"": ""Đáp án mẫu khoa học chuẩn xác ở đây""
}}";

            string partsJson = BuildPartsJson(prompt, studentAnswerImage);

            string requestJson = $@"{{
                ""contents"": [
                    {{
                        ""parts"": {partsJson}
                    }}
                ],
                ""generationConfig"": {{
                    ""temperature"": 0.2,
                    ""responseMimeType"": ""application/json""
                }}
            }}";

            using (UnityWebRequest req = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(requestJson);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 15; // Chờ tối đa 15 giây

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    string resText = req.downloadHandler.text;
                    GradeResult result = ParseGeminiResponse(resText);

                    if (result != null)
                    {
                        // Đảm bảo luôn có đáp án gợi ý nếu model bỏ trống
                        if (string.IsNullOrEmpty(result.suggestedAnswer))
                        {
                            result.suggestedAnswer = !string.IsNullOrEmpty(sampleAnswer) ? sampleAnswer : explanation;
                        }

                        Debug.Log($"[AIGradingService] 🤖 AI chấm: {(result.isCorrect ? "ĐÚNG" : "SAI")} - Giải thích: {result.feedback} - Đáp án: {result.suggestedAnswer}");
                        onDone?.Invoke(result);
                        yield break;
                    }
                }
                else
                {
                    Debug.LogWarning($"[AIGradingService] ⚠️ Lỗi kết nối AI ({req.error}): {req.downloadHandler?.text}");
                }

                // Nếu gọi AI thất bại (mất mạng/lỗi mạng), dùng fallback kèm đầy đủ đáp án đúng
                GradeResult fallbackResult = FallbackGrade(studentAnswer, sampleAnswer, keywords, explanation, studentAnswerImage);
                onDone?.Invoke(fallbackResult);
            }
        }

        /// <summary>
        /// Tạo lời giải thích sư phạm cho câu hỏi trắc nghiệm bằng Gemini AI (hoặc fallback nếu offline).
        /// </summary>
        public static IEnumerator ExplainMultipleChoice(
            string questionText,
            string optionA, string optionB, string optionC, string optionD,
            string userChoice,
            string correctChoice,
            string defaultExplain,
            bool isTimeout,
            Action<string> onDone)
        {
            string apiKey = GetActiveApiKey();

            // Nếu không có API Key hoặc đang là placeholder, dùng giải thích dự phòng (fallback) ngay để tránh chờ timeout
            if (string.IsNullOrEmpty(apiKey) || apiKey.Equals("geminiApiKey", StringComparison.OrdinalIgnoreCase))
            {
                string fallback = BuildFallbackMultipleChoiceExplanation(userChoice, correctChoice, defaultExplain, isTimeout);
                onDone?.Invoke(fallback);
                yield break;
            }

            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelName}:generateContent?key={apiKey}";

            string studentStatus = isTimeout 
                ? "Học sinh không kịp trả lời (Hết giờ)" 
                : $"Học sinh đã chọn: {userChoice}";

            string prompt = $@"Bạn là giáo viên bộ môn Sinh học. Hãy giải thích ngắn gọn câu hỏi trắc nghiệm sau cho học sinh:

[CÂU HỎI]
{questionText}

[CÁC ĐÁP ÁN LỰA CHỌN]
A. {optionA}
B. {optionB}
C. {optionC}
D. {optionD}

[KẾT QUẢ]
- Đáp án đúng: {correctChoice}
- Trạng thái học sinh: {studentStatus}
- Giải thích gốc từ tài liệu: {defaultExplain}

[YÊU CẦU GIẢI THÍCH]
- Ngắn gọn 2-3 câu, nêu rõ bản chất kiến thức Sinh học vì sao đáp án {correctChoice} là chính xác.
- Nếu học sinh chọn sai hoặc hết giờ, chỉ ra điểm lưu ý ngắn gọn để tránh nhầm lẫn.
- Giọng văn sư phạm, ân cần, súc tích, dễ nhớ.
- Chỉ trả về duy nhất nội dung giải thích, không kèm tiêu đề, không chào hỏi, không markdown thừa.";

            string requestJson = $@"{{
                ""contents"": [
                    {{
                        ""parts"": [
                            {{ ""text"": ""{EscapeJson(prompt)}"" }}
                        ]
                    }}
                ],
                ""generationConfig"": {{
                    ""temperature"": 0.3,
                    ""maxOutputTokens"": 300
                }}
            }}";

            using (UnityWebRequest req = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(requestJson);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 10;

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string resText = req.downloadHandler.text;
                        string extracted = ExtractGeminiText(resText);
                        if (!string.IsNullOrEmpty(extracted))
                        {
                            onDone?.Invoke(extracted.Trim());
                            yield break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[AIGradingService] Lỗi parse kết quả giải thích trắc nghiệm: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[AIGradingService] ⚠️ Lỗi kết nối AI trắc nghiệm ({req.error}): {req.downloadHandler?.text}");
                }

                // Fallback nếu request thất bại hoặc timeout
                string fallback = BuildFallbackMultipleChoiceExplanation(userChoice, correctChoice, defaultExplain, isTimeout);
                onDone?.Invoke(fallback);
            }
        }

        private static string ExtractGeminiText(string json)
        {
            if (string.IsNullOrEmpty(json)) return "";
            var match = Regex.Match(json, @"\""text\""\s*:\s*\""([\s\S]*?)\""\s*\}", RegexOptions.Multiline);
            if (match.Success)
            {
                return UnescapeJson(match.Groups[1].Value);
            }
            return "";
        }

        private static string BuildFallbackMultipleChoiceExplanation(string userChoice, string correctChoice, string defaultExplain, bool isTimeout)
        {
            if (!string.IsNullOrEmpty(defaultExplain) && !defaultExplain.Contains("(Không có giải thích chi tiết)"))
            {
                return defaultExplain.Trim();
            }

            if (isTimeout)
            {
                return $"Đã hết thời gian làm bài! Đáp án chính xác là {correctChoice}. Hãy ghi nhớ kiến thức trọng tâm này nhé!";
            }

            bool isCorrect = userChoice.Trim().Equals(correctChoice.Trim(), StringComparison.OrdinalIgnoreCase);
            if (isCorrect)
            {
                return $"Chính xác! Đáp án đúng là {correctChoice}. Bạn đã nắm vững kiến thức phần này.";
            }
            else
            {
                return $"Đáp án đúng là {correctChoice}. Hãy xem lại bài học để củng cố kiến thức nhé!";
            }
        }

        /// <summary>
        /// Dấu hiệu để prompt biết học sinh có kèm ảnh bài viết không.
        /// </summary>
        private static string GetImageNote(byte[] image)
        {
            if (image == null || image.Length == 0) return "";
            return "- Học sinh ĐÃ KÈM THEO ẢNH BÀI VIẾT (ảnh được gửi cùng request này, xem ở phần ảnh phía dưới). Hãy CHẤM THEO NỘI DUNG ẢNH nếu ảnh có chữ rõ, ưu tiên kết hợp cả text và ảnh.";
        }

        /// <summary>
        /// Xây JSON parts = [text] hoặc [text + inline_data(base64 ảnh)] tùy có ảnh hay không.
        /// </summary>
        private static string BuildPartsJson(string prompt, byte[] image)
        {
            string textPart = $@"{{ ""text"": ""{EscapeJson(prompt)}"" }}";

            if (image == null || image.Length == 0)
            {
                return $@"[ {textPart} ]";
            }

            string base64 = System.Convert.ToBase64String(image);
            string mime = GuessImageMimeType(image);
            string imagePart = $@"{{
                ""inline_data"": {{
                    ""mimeType"": ""{mime}"",
                    ""data"": ""{base64}""
                }}
            }}";
            return $@"[ {textPart}, {imagePart} ]";
        }

        /// <summary>
        /// Nhận biết loại ảnh từ byte (magic number) để gán đúng mimeType cho Gemini.
        /// </summary>
        private static string GuessImageMimeType(byte[] img)
        {
            if (img == null || img.Length < 3) return "image/png";
            if (img[0] == 0x89 && img[1] == 0x50 && img[2] == 0x4E) return "image/png";
            if (img[0] == 0xFF && img[1] == 0xD8 && img[2] == 0xFF) return "image/jpeg";
            if (img[0] == 0x47 && img[1] == 0x49 && img[2] == 0x46) return "image/gif";
            if (img.Length >= 12 && img[0] == 0x52 && img[1] == 0x49 && img[2] == 0x46 && img[3] == 0x46
                && img[8] == 0x57 && img[9] == 0x45 && img[10] == 0x42 && img[11] == 0x50) return "image/webp";
            return "image/png";
        }

        /// <summary>
        /// Bóc tách kết quả JSON từ phản hồi của Google Gemini
        /// </summary>
        private static GradeResult ParseGeminiResponse(string rawJson)
        {
            try
            {
                // Tìm chuỗi text bên trong candidates[0].content.parts[0].text
                Match match = Regex.Match(rawJson, @"\""text\""\s*:\s*\""([\s\S]*?)\""\s*\}", RegexOptions.Multiline);
                string innerJson = match.Success ? UnescapeJson(match.Groups[1].Value) : rawJson;

                // Tìm khối { ... } chứa kết quả
                int firstBrace = innerJson.IndexOf('{');
                int lastBrace = innerJson.LastIndexOf('}');
                if (firstBrace >= 0 && lastBrace > firstBrace)
                {
                    string cleanJson = innerJson.Substring(firstBrace, lastBrace - firstBrace + 1);
                    GradeResult res = JsonUtility.FromJson<GradeResult>(cleanJson);
                    if (res != null)
                    {
                        // Nếu suggestedAnswer chưa có trong object (do định dạng phụ)
                        if (string.IsNullOrEmpty(res.suggestedAnswer))
                        {
                            Match saMatch = Regex.Match(cleanJson, @"\""suggestedAnswer\""\s*:\s*\""([\s\S]*?)\""");
                            if (saMatch.Success) res.suggestedAnswer = UnescapeJson(saMatch.Groups[1].Value);
                        }
                        return res;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AIGradingService] Lỗi parse JSON từ Gemini: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Chế độ chấm dự phòng khi mất mạng hoặc chưa cấu hình API Key.
        /// Luôn cung cấp cả giải thích lẫn đáp án đúng khoa học để người học tham khảo.
        /// </summary>
        public static GradeResult FallbackGrade(string studentAnswer, string sampleAnswer, string keywords, string explanation = "", byte[] studentAnswerImage = null)
        {
            string normUser = (studentAnswer ?? "").ToLower().Trim();
            string normCorrect = (sampleAnswer ?? "").ToLower().Trim();

            // Xác định đáp án chuẩn mẫu
            string fallbackAnswer = !string.IsNullOrEmpty(sampleAnswer) ? sampleAnswer : explanation;
            if (string.IsNullOrEmpty(fallbackAnswer))
            {
                fallbackAnswer = "Tham khảo kiến thức chuẩn trong sách giáo khoa và bài giảng của thầy cô.";
            }

            // Trường hợp học sinh trả lời không biết, nói bừa hoặc bỏ trống
            if (string.IsNullOrEmpty(normUser) ||
                normUser.Contains("k biet") || normUser.Contains("ko biet") ||
                normUser.Contains("khong biet") || normUser.Contains("chua biet") ||
                normUser.Contains("tao k biet") || normUser.Contains("chịu") || normUser == "...")
            {
                return new GradeResult
                {
                    isCorrect = false,
                    score = 0,
                    feedback = "Chưa đúng! Câu trả lời chưa nêu được kiến thức trọng tâm của bài học.",
                    suggestedAnswer = fallbackAnswer
                };
            }

            // Trường hợp chỉ tải ảnh mà không nhập chữ khi chưa kết nối AI online
            if (studentAnswerImage != null && studentAnswerImage.Length > 0 && string.IsNullOrEmpty(normUser))
            {
                return new GradeResult
                {
                    isCorrect = false,
                    score = 0,
                    feedback = "Hệ thống chưa nhận diện được nội dung kiến thức hợp lệ trong ảnh này khi ở chế độ offline. Vui lòng nhập văn bản vào ô trả lời.",
                    suggestedAnswer = fallbackAnswer
                };
            }

            bool correct = false;
            if (!string.IsNullOrEmpty(normCorrect) && !string.IsNullOrEmpty(normUser) &&
                (normUser.Contains(normCorrect) || normCorrect.Contains(normUser)))
            {
                correct = true;
            }

            if (!string.IsNullOrEmpty(keywords) && !string.IsNullOrEmpty(normUser))
            {
                string[] kwList = keywords.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                int matchKw = 0;
                foreach (var kw in kwList)
                {
                    if (normUser.Contains(kw.Trim().ToLower())) matchKw++;
                }

                if (matchKw > 0 && normUser.Length >= 4) correct = true;
            }

            return new GradeResult
            {
                isCorrect = correct,
                score = correct ? 10 : 0,
                feedback = correct
                    ? "Chính xác! Câu trả lời đã nêu đúng kiến thức trọng tâm của câu hỏi."
                    : "Chưa đúng! Câu trả lời chưa đủ ý hoặc chưa sát với kiến thức bài học.",
                suggestedAnswer = fallbackAnswer
            };
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\r", "")
                    .Replace("\n", "\\n");
        }

        private static string UnescapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\n", "\n")
                    .Replace("\\\"", "\"")
                    .Replace("\\\\", "\\");
        }
    }
}
