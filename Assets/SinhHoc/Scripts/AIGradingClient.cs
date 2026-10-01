// ============================================================
// AIGradingClient.cs
// Cầu nối UNITY → BACKEND API → AI VISION → BACKEND → UNITY (#22).
//
// • KHÔNG BAO GIỜ chứa AI API Key. Key chỉ nằm ở Backend / env var.
// • Unity chỉ gửi: { questionID, imageBase64 } (#23)
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
    [Tooltip("Endpoint chấm bài, VD: https://your-backend.com/api/grade")]
    public string backendUrl = "https://your-backend.example.com/api/grade";
    [Tooltip("Giây tối đa chờ backend phản hồi")]
    public float timeoutSeconds = 45f;

    [Header("Mock Settings (chế độ Mock)")]
    [Tooltip("Giả lập độ trễ AI (giây)")]
    public float mockDelaySeconds = 2.5f;
    [Tooltip("Mock luôn trả điểm 8/10 (tắt = random 5..10)")]
    public bool mockFixedScore = true;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        if (string.IsNullOrEmpty(imageBase64))
        {
            onError?.Invoke("INVALID_IMAGE");
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
        public string questionID;
        public string imageBase64;
    }

    private IEnumerator RealCoroutine(string questionID, string imageBase64,
                                      Action<AIGradingResponse> onSuccess, Action<string> onError)
    {
        string json = JsonUtility.ToJson(new GradeRequest { questionID = questionID, imageBase64 = imageBase64 });
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(backendUrl, "POST"))
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

    private AIGradingResponse ParseResponse(string text, out string error)
    {
        error = null;
        if (string.IsNullOrEmpty(text))
        {
            error = "INVALID_AI_RESPONSE";
            return null;
        }
        try
        {
            AIGradingResponse r = JsonUtility.FromJson<AIGradingResponse>(text);
            if (r == null)
            {
                error = "INVALID_AI_RESPONSE";
                return null;
            }
            // Chuẩn hóa trường bắt buộc
            r.correctPoints = r.correctPoints ?? new List<string>();
            r.missingPoints = r.missingPoints ?? new List<string>();
            r.feedback = string.IsNullOrEmpty(r.feedback) ? "" : r.feedback;
            r.suggestedAnswer = string.IsNullOrEmpty(r.suggestedAnswer) ? "" : r.suggestedAnswer;
            return r;
        }
        catch (Exception e)
        {
            error = "INVALID_AI_RESPONSE: " + e.Message;
            return null;
        }
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

        float score = mockFixedScore ? 8f : UnityEngine.Random.Range(5f, 10f);
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
