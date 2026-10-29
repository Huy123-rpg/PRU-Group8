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
    /// Có khả năng phân tích ngữ nghĩa, hiểu từ đồng nghĩa và đưa ra nhận xét chi tiết.
    /// Nếu mất mạng hoặc chưa nhập API Key, hệ thống sẽ tự động chuyển sang chế độ chấm bằng từ khóa (Fallback).
    /// </summary>
    public static class AIGradingService
    {
        // =========================================================================
        // 👇 DÁN API KEY GOOGLE GEMINI CỦA BẠN VÀO ĐÂY (Lấy miễn phí tại: https://aistudio.google.com/app/apikey)
        // =========================================================================
        public static string GeminiApiKey = "DÁN_API_KEY_CỦA_BẠN_VÀO_ĐÂY";
        // =========================================================================

        private const string ModelName = "gemini-1.5-flash";

        /// <summary>
        /// Kết quả trả về sau khi AI chấm bài
        /// </summary>
        [Serializable]
        public class GradeResult
        {
            public bool isCorrect;      // Đạt yêu cầu hay không
            public int score;           // Điểm số thang 10
            public string feedback;     // Nhận xét chi tiết từ AI
        }

        /// <summary>
        /// Kiểm tra xem API Key đã được cấu hình hay chưa.
        /// Có thể lưu qua PlayerPrefs để cấu hình linh hoạt từ giao diện.
        /// </summary>
        public static string GetActiveApiKey()
        {
            string savedKey = PlayerPrefs.GetString("Gemini_ApiKey", "").Trim();
            if (!string.IsNullOrEmpty(savedKey)) return savedKey;

            if (!string.IsNullOrEmpty(GeminiApiKey) && !GeminiApiKey.Contains("DÁN_API_KEY"))
            {
                return GeminiApiKey.Trim();
            }

            return "";
        }

        /// <summary>
        /// Quá tải chấm điểm dạng text thuần (tương thích ngược).
        /// </summary>
        public static IEnumerator GradeAnswer(string questionText, string studentAnswer, string sampleAnswer, string keywords, Action<GradeResult> onDone)
        {
            return GradeAnswer(questionText, studentAnswer, sampleAnswer, keywords, null, onDone);
        }

        /// <summary>
        /// Coroutine gửi bài làm của học sinh lên Gemini AI để chấm điểm ngữ nghĩa.
        /// Hỗ trợ: text thông thường (null = không ảnh) hoặc ảnh bài viết (inline_data base64).
        /// </summary>
        public static IEnumerator GradeAnswer(string questionText, string studentAnswer, string sampleAnswer, string keywords, byte[] studentAnswerImage, Action<GradeResult> onDone)
        {
            string apiKey = GetActiveApiKey();

            // Nếu chưa có API Key, dùng thuật toán chấm từ khóa cục bộ
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogWarning("[AIGradingService] ⚠️ Chưa cấu hình API Key. Dùng thuật toán chấm từ khóa cục bộ.");
                GradeResult fallback = FallbackGrade(studentAnswer, sampleAnswer, keywords);
                onDone?.Invoke(fallback);
                yield break;
            }

            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelName}:generateContent?key={apiKey}";

            // Xây dựng Prompt chuẩn sư phạm Sinh học
            string prompt = $@"Bạn là giám khảo chuyên chấm thi bộ môn Sinh học phổ thông.
Nhiệm vụ của bạn là đánh giá câu trả lời tự luận của học sinh một cách công tâm và thông minh.

[THÔNG TIN BÀI THI]
- Câu hỏi: {questionText}
- Đáp án mẫu chuẩn: {sampleAnswer}
- Từ khóa/ý chính bắt buộc có: {keywords}
- Bài làm của học sinh: {studentAnswer}
{GetImageNote(studentAnswerImage)}

[QUY TẮC CHẤM ĐIỂM]
1. Hãy đánh giá theo BẢN CHẤT KHOA HỌC: Nếu học sinh diễn đạt bằng từ đồng nghĩa, câu chữ khác nhưng đúng ý nghĩa Sinh học thì VẪN TÍNH LÀ ĐÚNG.
2. Nếu câu trả lời đúng từ 60% ý cốt lõi trở lên: Cho isCorrect = true, score từ 6 đến 10.
3. Nếu câu trả lời sai lệch bản chất, quá sơ sài hoặc lạc đề: Cho isCorrect = false, score từ 0 đến 5.
4. Đưa ra nhận xét ngắn gọn (feedback) khoảng 1-2 câu, khích lệ và chỉ rõ điểm đúng hoặc điểm còn thiếu.

[YÊU CẦU ĐỊNH DẠNG TRẢ VỀ]
BẮT BUỘC chỉ trả về DUY NHẤT một chuỗi JSON hợp lệ (không kèm bất kỳ chữ nào khác):
{{
  ""isCorrect"": true,
  ""score"": 10,
  ""feedback"": ""Nhận xét ngắn gọn ở đây""
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
                req.timeout = 15; // Chờ tối đa 15 giây (ảnh nặng hơn text)

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    string resText = req.downloadHandler.text;
                    GradeResult result = ParseGeminiResponse(resText);

                    if (result != null)
                    {
                        Debug.Log($"[AIGradingService] 🤖 AI chấm: {(result.isCorrect ? "ĐÚNG" : "SAI")} ({result.score}/10) - {result.feedback}");
                        onDone?.Invoke(result);
                        yield break;
                    }
                }
                else
                {
                    Debug.LogWarning($"[AIGradingService] ⚠️ Lỗi kết nối AI ({req.error}): {req.downloadHandler?.text}");
                }

                // Nếu gọi AI thất bại (mất mạng/hết quota/lỗi mạng), dùng fallback
                GradeResult fallbackResult = FallbackGrade(studentAnswer, sampleAnswer, keywords);
                onDone?.Invoke(fallbackResult);
            }
        }

        /// <summary>
        /// T4: Dấu hiệu để prompt biết học sinh có kèm ảnh bài viết không.
        /// </summary>
        private static string GetImageNote(byte[] image)
        {
            if (image == null || image.Length == 0) return "";
            return "- Học sinh ĐÃ KÈM THEO ẢNH BÀI VIẾT (ảnh được gửi cùng request này, xem ở phần ảnh phía dưới). Hãy CHẤM THEO NỘI DUNG ẢNH nếu ảnh có chữ rõ, ưu tiên kết hợp cả text và ảnh.";
        }

        /// <summary>
        /// T4: Xây JSON parts = [text] hoặc [text + inline_data(base64 ảnh)] tùy có ảnh hay không.
        /// </summary>
        private static string BuildPartsJson(string prompt, byte[] image)
        {
            string textPart = $@"{{ ""text"": ""{EscapeJson(prompt)}"" }}";

            if (image == null || image.Length == 0)
            {
                return $@"[ {textPart} ]";
            }

            string base64 = System.Convert.ToBase64String(image);
            string mime = GuessImageMimeType(image); // nhận biết png/jpeg từ byte đầu để không gán sai format
            // Gemini inline_data: { mimeType, data(base64) }
            string imagePart = $@"{{
                ""inline_data"": {{
                    ""mimeType"": ""{mime}"",
                    ""data"": ""{base64}""
                }}
            }}";
            return $@"[ {textPart}, {imagePart} ]";
        }

        /// <summary>
        /// T4: Nhận biết loại ảnh từ byte (magic number) để gán đúng mimeType cho Gemini.
        /// Mặc định trả về "image/png" nếu không nhận dạng được.
        /// </summary>
        private static string GuessImageMimeType(byte[] img)
        {
            if (img == null || img.Length < 3) return "image/png";
            // PNG: 89 50 4E 47
            if (img[0] == 0x89 && img[1] == 0x50 && img[2] == 0x4E) return "image/png";
            // JPEG: FF D8 FF
            if (img[0] == 0xFF && img[1] == 0xD8 && img[2] == 0xFF) return "image/jpeg";
            // GIF: 47 49 46
            if (img[0] == 0x47 && img[1] == 0x49 && img[2] == 0x46) return "image/gif";
            // WebP: 52 49 46 46 ... 57 45 42 50
            if (img.Length >= 12 && img[0] == 0x52 && img[1] == 0x49 && img[2] == 0x46 && img[3] == 0x46
                && img[8] == 0x57 && img[9] == 0x45 && img[10] == 0x42 && img[11] == 0x50) return "image/webp";
            return "image/png"; // dự phòng an toàn
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
                string innerJson = "";
                if (match.Success)
                {
                    innerJson = UnescapeJson(match.Groups[1].Value);
                }
                else
                {
                    innerJson = rawJson;
                }

                // Tìm JSON dạng {"isCorrect": ..., "score": ..., "feedback": ...}
                Match jsonMatch = Regex.Match(innerJson, @"\{[\s\S]*?""isCorrect""[\s\S]*?\}");
                if (jsonMatch.Success)
                {
                    string cleanJson = jsonMatch.Value;
                    GradeResult res = JsonUtility.FromJson<GradeResult>(cleanJson);
                    if (res != null) return res;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AIGradingService] Lỗi parse JSON từ Gemini: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Chế độ chấm từ khóa dự phòng khi mất mạng hoặc chưa nhập API Key
        /// </summary>
        public static GradeResult FallbackGrade(string studentAnswer, string sampleAnswer, string keywords)
        {
            string normUser = studentAnswer.ToLower().Trim();
            string normCorrect = sampleAnswer.ToLower().Trim();

            bool correct = false;
            if (!string.IsNullOrEmpty(normCorrect) && (normUser.Contains(normCorrect) || normCorrect.Contains(normUser)))
            {
                correct = true;
            }

            int matchKw = 0;
            int totalKw = 0;
            if (!string.IsNullOrEmpty(keywords))
            {
                string[] kwList = keywords.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                totalKw = kwList.Length;
                foreach (var kw in kwList)
                {
                    if (normUser.Contains(kw.Trim().ToLower())) matchKw++;
                }

                if (matchKw > 0) correct = true;
            }

            return new GradeResult
            {
                isCorrect = correct,
                score = correct ? (totalKw > 0 ? Mathf.RoundToInt((float)matchKw / totalKw * 10f) : 10) : 0,
                feedback = correct ? "Đúng theo từ khóa chuẩn." : "Chưa đủ ý hoặc chưa đúng từ khóa yêu cầu."
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
