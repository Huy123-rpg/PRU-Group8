// ============================================================
// AIGradingClient.cs
// Cầu nối UNITY → BACKEND API → AI VISION → BACKEND → UNITY (#22).
//
// • KHÔNG BAO GIỜ chứa AI API Key. Key chỉ nằm ở Backend / env var.
// • Unity gửi action + questionID và imageBase64 HOẶC answerText.
//   → Backend tự tra Question/ModelAnswer/Rubric/MaxScore từ Google Sheet
//   → gọi AI Vision chấm theo Rubric
//   → trả structured JSON (schema khớp AIGradingResponse, #16)
// • Có 2 chế độ (bật/tắt trong Inspector):
//     MOCK   : chưa có backend → giả lập kết quả để test đủ 3 flows
//     REAL   : POST thật qua UnityWebRequest, có timeout + error mapping (#21)
// • Không bao giờ làm Unity crash - mọi lỗi đều trả về qua callback onError.
// ============================================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AIGradingClient : MonoBehaviour
{
    public static AIGradingClient Instance { get; private set; }

    public enum Mode { Mock, RealBackend }

    [Header("Chế độ (MOCK khi chưa có backend)")]
    public Mode mode = Mode.Mock;

    [Header("Backend Settings (chế độ RealBackend)")]
    [Tooltip("URL Web App Apps Script, dạng https://script.google.com/macros/s/.../exec")]
    public string backendUrl = "https://your-backend.example.com/api/grade";
    [Tooltip("Giây tối đa chờ backend phản hồi")]
    public float timeoutSeconds = 180f;

    [Header("Mock Settings (chế độ Mock)")]
    [Tooltip("Giả lập độ trễ AI (giây)")]
    public float mockDelaySeconds = 2.5f;
    [Tooltip("Mock luôn trả điểm 8/10 (tắt = random 5..10)")]
    public bool mockFixedScore = true;
    [Range(0f, 10f)] public float mockScore = 8f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        // Scene-local: do not keep MazeManager and its UI references across scenes.
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Gửi bài làm tự luận. KHÔNG gửi ModelAnswer/Rubric từ client (#23) -
    /// backend tra cứu bằng questionID.
    /// </summary>
    public void SubmitEssay(string questionID, string imageBase64,
                            Action<AIGradingResponse> onSuccess, Action<string> onError)
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
        StartCoroutine(mode == Mode.Mock
            ? MockCoroutine(questionID, imageBase64, onSuccess, onError)
            : RealCoroutine(questionID, imageBase64, onSuccess, onError));
    }

    // ================================================================
    // CHẾ ĐỘ REAL BACKEND
    // ================================================================
    [Serializable]
    private class GradeRequest
    {
        public string action = "gradeEssay";
        public string questionID;
        public string imageBase64;
        public string answerText;
    }

    [Serializable]
    private class BackendStatus
    {
        public bool success = false;
        public string error = null;
        public string message = null;
    }

    private IEnumerator RealCoroutine(string questionID, string imageBase64,
                                      Action<AIGradingResponse> onSuccess, Action<string> onError,
                                      string answerText = null)
    {
        if (!Uri.TryCreate(backendUrl, UriKind.Absolute, out Uri endpoint)
            || (endpoint.Scheme != "https" && !(endpoint.Scheme == "http" && endpoint.IsLoopback))
            || backendUrl.Contains("your-backend.example.com"))
        {
            onError?.Invoke("Chưa cấu hình Backend Url. Dán URL Web App Apps Script kết thúc bằng /exec.");
            yield break;
        }
        string json = JsonUtility.ToJson(new GradeRequest { questionID = questionID, imageBase64 = imageBase64, answerText = answerText });
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(backendUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.redirectLimit = 8; // ContentService redirects to script.googleusercontent.com.
            www.timeout = Mathf.Max(5, Mathf.RoundToInt(timeoutSeconds));

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                string backendError = ReadBackendError(www.downloadHandler.text);
                onError?.Invoke(backendError ?? MapWebRequestError(www));
                yield break;
            }

            // Parse structured JSON (#16) - mọi lỗi parse đều là lỗi kỹ thuật, không crash
            AIGradingResponse resp = ParseResponse(www.downloadHandler.text, out string parseErr);
            if (resp == null)
            {
                onError?.Invoke(parseErr);
                yield break;
            }
            onSuccess?.Invoke(resp);
        }
    }

    // The backend must accept answerText and look up the rubric by questionID.
    public void SubmitTextEssay(string questionID, string answerText,
        Action<AIGradingResponse> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrWhiteSpace(questionID))
        {
            onError?.Invoke("INVALID_QUESTION");
            return;
        }
        if (string.IsNullOrWhiteSpace(answerText))
        {
            onError?.Invoke("EMPTY_ANSWER");
            return;
        }
        if (answerText.Length > 5000)
        {
            onError?.Invoke("ANSWER_TOO_LONG");
            return;
        }
        StartCoroutine(mode == Mode.Mock
            ? MockCoroutine(questionID, "TEXT_ANSWER", onSuccess, onError)
            : RealCoroutine(questionID, null, onSuccess, onError, answerText.Trim()));
    }

    private static bool IsValidResponse(AIGradingResponse response)
    {
        if (response == null) return false;
        // Also rejects {success:true,message:"history saved"} from an old deployment.
        return !float.IsNaN(response.score) && !float.IsInfinity(response.score)
            && !float.IsNaN(response.maxScore) && !float.IsInfinity(response.maxScore)
            && response.maxScore > 0f && response.score >= 0f
            && response.score <= response.maxScore;
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
            // Chuẩn hóa trường bắt buộc
            r.correctPoints = r.correctPoints ?? new List<string>();
            r.missingPoints = r.missingPoints ?? new List<string>();
            r.feedback = string.IsNullOrEmpty(r.feedback) ? "" : r.feedback;
            r.suggestedAnswer = string.IsNullOrEmpty(r.suggestedAnswer) ? "" : r.suggestedAnswer;
            if (!IsValidResponse(r))
            {
                error = "Kết quả chấm không hợp lệ. Kiểm tra Apps Script đã triển khai phiên bản chấm AI mới.";
                return null;
            }
            if (r.readable) r.percentage = 100f * r.score / r.maxScore;
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

    /// <summary>Đổi lỗi UnityWebRequest thành mã lỗi ngắn cho UI (#21).</summary>
    private static string MapWebRequestError(UnityWebRequest www)
    {
        switch (www.result)
        {
            case UnityWebRequest.Result.ConnectionError:
                return Application.internetReachability == NetworkReachability.NotReachable
                    ? "NO_INTERNET"
                    : "CONNECTION_ERROR";
            case UnityWebRequest.Result.ProtocolError:
                return "BACKEND_ERROR_" + www.responseCode;
            case UnityWebRequest.Result.DataProcessingError:
                return "INVALID_AI_RESPONSE";
            default:
                return "UNKNOWN_ERROR";
        }
    }

    // ================================================================
    // CHẾ ĐỘ MOCK (chưa có backend - test đủ luồng trong Editor)
    // ================================================================
    private IEnumerator MockCoroutine(string questionID, string imageBase64,
                                      Action<AIGradingResponse> onSuccess, Action<string> onError)
    {
        Debug.Log($"[AIGradingClient][MOCK] Giả lập chấm bài {questionID} ({imageBase64.Length} ký tự base64)...");
        yield return new WaitForSecondsRealtime(mockDelaySeconds);

        // "Ảnh mờ" giả lập luồng #17: base64 bắt đầu bằng "BLURRY"
        if (imageBase64.StartsWith("BLURRY", StringComparison.OrdinalIgnoreCase))
        {
            onSuccess?.Invoke(new AIGradingResponse
            {
                readable = false,
                feedback = "Không thể đọc rõ bài làm. Hãy chụp lại ảnh."
            });
            yield break;
        }

        float score = mockFixedScore ? Mathf.Clamp(mockScore, 0f, 10f) : UnityEngine.Random.Range(0f, 10f);
        float max = 10f;
        float pct = score / max * 100f;

        List<string> correctPoints = new List<string>();
        List<string> missingPoints = new List<string>();
        string feedback = null, suggested = null;

        if (pct >= 90f) { correctPoints.Add("Trình bày đầy đủ các ý chính"); suggested = "Hoàn thành toàn bộ yêu cầu của đề."; }
        else if (pct >= 70f)
        {
            correctPoints.Add("Xác định đúng các bước chính");
            missingPoints.Add("Thiếu một vài chi tiết phụ");
            feedback = "Bài làm tốt, cần bổ sung thêm chi tiết.";
        }
        else if (pct >= 50f)
        {
            correctPoints.Add("Nắm được ý cơ bản của bài");
            missingPoints.Add("Thiếu phép lai / sơ đồ rõ ràng");
            feedback = "Bài làm đạt yêu cầu tối thiểu, cần làm rõ hơn.";
        }
        else
        {
            missingPoints.Add("Chưa đạt các tiêu chí chính trong rubric");
            feedback = "Bài làm chưa đạt - xem lại đáp án gợi ý.";
        }

        onSuccess?.Invoke(new AIGradingResponse
        {
            readable = true,
            score = score,
            maxScore = max,
            percentage = pct,
            correctPoints = correctPoints,
            missingPoints = missingPoints,
            feedback = feedback ?? "Bài làm đúng phần chính.",
            suggestedAnswer = suggested ?? "Xem lại nội dung bài học."
        });
    }
}



