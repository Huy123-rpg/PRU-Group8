using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ScienceQuest.PhysicsGame2
{
    [Serializable]
    public class BackendEnvelope<T>
    {
        public bool ok;
        public string requestId;
        public string serverTimeUtc;
        public T data;
        public BackendError error;
    }

    [Serializable]
    public class BackendError
    {
        public string code;
        public string message;
        public bool retryable;
    }

    [Serializable]
    public class WeeklyLeaderboardItem
    {
        public string userId;
        public string displayName;
        public float score;
        public int rank;
        public string finalizedAtUtc;
    }

    [Serializable]
    public class WeeklyBoardResponse
    {
        public string weekId;
        public List<WeeklyLeaderboardItem> leaderboard;
        public int myRank;
        public float? myScore;
    }

    /// <summary>
    /// Service trung gian giao tiếp HTTPS Web App Google Apps Script và Google Sheets.
    /// Hỗ trợ:
    /// - Kiểm tra trạng thái health và phiên làm việc
    /// - Đồng bộ hồ sơ (nhân vật, cài đặt)
    /// - Bắt đầu / kết thúc buổi học
    /// - Nộp bài trắc nghiệm / tự luận / Weekly challenge
    /// - Chế độ Local Practice tự động kích hoạt khi offline hoặc khi chưa cấu hình WebApp URL
    /// </summary>
    public class AureliaOnlineService : MonoBehaviour
    {
        private static AureliaOnlineService _instance;

        public static AureliaOnlineService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<AureliaOnlineService>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("[AureliaOnlineService]");
                        _instance = go.AddComponent<AureliaOnlineService>();
                        if (Application.isPlaying) DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private const string PREF_BACKEND_URL = "AURELIA_BACKEND_URL";
        private const string PREF_AUTH_TOKEN = "AURELIA_AUTH_TOKEN";

        [SerializeField]
        private string backendUrl = "";
        private bool _isOnlineMode = false;

        public bool IsOnlineMode => _isOnlineMode;
        public string BackendUrl => backendUrl;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
                LoadConfig();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void LoadConfig()
        {
            backendUrl = PlayerPrefs.GetString(PREF_BACKEND_URL, "");
            _isOnlineMode = !string.IsNullOrEmpty(backendUrl);
        }

        public void SetBackendUrl(string url)
        {
            backendUrl = url != null ? url.Trim() : "";
            PlayerPrefs.SetString(PREF_BACKEND_URL, backendUrl);
            PlayerPrefs.Save();
            _isOnlineMode = !string.IsNullOrEmpty(backendUrl);
        }

        public string GetAuthToken()
        {
            return PlayerPrefs.GetString(PREF_AUTH_TOKEN, "");
        }

        public void SetAuthToken(string token)
        {
            PlayerPrefs.SetString(PREF_AUTH_TOKEN, token != null ? token.Trim() : "");
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Gửi POST request với Envelope chuẩn { action, requestId, idToken, payload }.
        /// </summary>
        public IEnumerator PostActionCoroutine<T>(string action, object payload, Action<bool, T, BackendError> callback)
        {
            if (string.IsNullOrEmpty(backendUrl))
            {
                // Local Practice fallback
                callback?.Invoke(false, default, new BackendError
                {
                    code = "OFFLINE_LOCAL",
                    message = "Hệ thống đang hoạt động ở chế độ Local Practice.",
                    retryable = false
                });
                yield break;
            }

            string requestId = Guid.NewGuid().ToString("N");
            string idToken = GetAuthToken();

            string payloadJson = payload != null ? JsonUtility.ToJson(payload) : "{}";
            // Lắp envelope JSON
            string bodyJson = $"{{\"action\":\"{action}\",\"requestId\":\"{requestId}\",\"idToken\":\"{idToken}\",\"payload\":{payloadJson}}}";

            using (UnityWebRequest req = new UnityWebRequest(backendUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 15; // 15 giây timeout tối đa

                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[AureliaOnlineService] Action {action} thất bại: {req.error}");
                    callback?.Invoke(false, default, new BackendError
                    {
                        code = "NETWORK_ERROR",
                        message = "Không thể kết nối đến máy chủ: " + req.error,
                        retryable = true
                    });
                    yield break;
                }

                try
                {
                    string resText = req.downloadHandler.text;
                    BackendEnvelope<T> env = JsonUtility.FromJson<BackendEnvelope<T>>(resText);
                    if (env != null && env.ok)
                    {
                        callback?.Invoke(true, env.data, null);
                    }
                    else
                    {
                        BackendError err = env?.error ?? new BackendError { code = "UNKNOWN_ERROR", message = "Máy chủ trả về phản hồi không hợp lệ.", retryable = false };
                        callback?.Invoke(false, default, err);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[AureliaOnlineService] Giải mã phản hồi JSON thất bại: {ex.Message}");
                    callback?.Invoke(false, default, new BackendError
                    {
                        code = "PARSE_ERROR",
                        message = "Lỗi giải mã phản hồi từ máy chủ: " + ex.Message,
                        retryable = false
                    });
                }
            }
        }
    }
}
