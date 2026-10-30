using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ScienceQuest.Core
{
    [Serializable]
    public class EssayGradingRequest
    {
        public string requestId;
        public string subject = "Chemistry";
        public string question;
        public string studentAnswer;
        public string referenceAnswer;
        public string rubric;
        public int maxScore = 10;
    }

    [Serializable]
    public class EssayGradingResult
    {
        public string requestId;
        public float score;
        public float maxScore;
        public bool isCorrect;
        public string feedback;
        public string modelAnswer;
        public string error;
    }

    // Unity sends no provider credentials. Gemini is called only by the local backend.
    public static class RaceEssayGrader
    {
        public const string DefaultEndpoint = "http://127.0.0.1:8787/grade-essay";

        public static EssayGradingRequest CreateRequest(QuestionData question, string answer, string requestId)
        {
            string reference = question.dapAnThamChieu;
            if (string.IsNullOrWhiteSpace(reference) && question.options != null &&
                question.correctOptionIndex >= 0 && question.correctOptionIndex < question.options.Length)
                reference = question.options[question.correctOptionIndex];
            return new EssayGradingRequest
            {
                requestId = requestId,
                question = question.questionText,
                studentAnswer = answer,
                referenceAnswer = reference ?? "",
                rubric = question.tieuChiCham ?? ""
            };
        }

        public static IEnumerator GradeEssayCoroutine(MonoBehaviour runner, QuestionData question,
            string studentAnswer, Action<EssayGradingResult> onDone)
        {
            string json = JsonUtility.ToJson(CreateRequest(question, studentAnswer, Guid.NewGuid().ToString("N")));
            yield return GradeEssayJsonCoroutine(DefaultEndpoint, json, onDone);
        }

        public static IEnumerator GradeEssayJsonCoroutine(string endpoint, string json, Action<EssayGradingResult> onDone)
        {
            Uri address;
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out address) ||
                (address.Scheme != "https" && !(address.Scheme == "http" && address.IsLoopback)))
            {
                onDone?.Invoke(Failure("INVALID_ENDPOINT"));
                yield break;
            }
            using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
                request.timeout = 60;
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    string error = request.result == UnityWebRequest.Result.ConnectionError ? "NETWORK_ERROR" : ReadError(request.downloadHandler.text);
                    onDone?.Invoke(Failure(error));
                    yield break;
                }
                onDone?.Invoke(ParseResult(request.downloadHandler.text, json));
            }
        }

        private static EssayGradingResult ParseResult(string json, string submittedJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json) || json.Length > 16000 ||
                    !json.Contains("\"score\"")) return Failure("AI_RESPONSE_INVALID");
                EssayGradingResult result = JsonUtility.FromJson<EssayGradingResult>(json);
                EssayGradingRequest submitted = JsonUtility.FromJson<EssayGradingRequest>(submittedJson);
                if (result == null || !string.IsNullOrEmpty(result.error) ||
                    string.IsNullOrEmpty(result.requestId) || result.requestId != submitted.requestId ||
                    result.maxScore != 10f || float.IsNaN(result.score) || float.IsInfinity(result.score) ||
                    result.score < 0 || result.score > result.maxScore ||
                    string.IsNullOrWhiteSpace(result.feedback) || string.IsNullOrWhiteSpace(result.modelAnswer))
                    return Failure("AI_RESPONSE_INVALID");
                result.isCorrect = result.score >= result.maxScore * 0.5f;
                return result;
            }
            catch (Exception) { return Failure("AI_RESPONSE_INVALID"); }
        }

        private static string ReadError(string json)
        {
            try
            {
                EssayGradingResult result = JsonUtility.FromJson<EssayGradingResult>(json);
                return !string.IsNullOrEmpty(result?.error) ? result.error : "AI_UNAVAILABLE";
            }
            catch (Exception) { return "AI_UNAVAILABLE"; }
        }

        private static EssayGradingResult Failure(string error)
        {
            return new EssayGradingResult { error = error, feedback = MessageForError(error) };
        }

        public static string MessageForError(string error)
        {
            switch (error)
            {
                case "AI_QUOTA_EXCEEDED": case "LOCAL_RATE_LIMIT":
                    return "Đã hết hạn mức chấm tạm thời. Hãy chờ rồi thử lại.";
                case "AI_KEY_INVALID": return "Khóa Gemini không hợp lệ. Cập nhật khóa rồi thử lại.";
                case "AI_NOT_CONFIGURED": return "Chưa cấu hình khóa Gemini cho máy chủ chấm bài.";
                case "AI_TIMEOUT": return "AI phản hồi quá lâu. Nhấn Thử lại để gửi lại bài.";
                case "NETWORK_ERROR": return "Không kết nối được máy chủ chấm bài. Nhấn Thử lại.";
                case "AI_RESPONSE_INVALID": return "Kết quả AI chưa hợp lệ. Nhấn Thử lại.";
                case "AI_MODEL_UNAVAILABLE": return "Model AI chưa khả dụng. Kiểm tra cấu hình máy chủ.";
                case "INVALID_ENDPOINT": return "Địa chỉ máy chủ chấm bài chưa hợp lệ.";
                default: return "Chưa chấm được bài. Bài làm được giữ lại để bạn thử lại.";
            }
        }
    }
}