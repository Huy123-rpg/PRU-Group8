using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class ProgressSyncManager : MonoBehaviour
{
    public static ProgressSyncManager Instance { get; private set; }

    public string progressSheetCsvUrl = "https://docs.google.com/spreadsheets/d/1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY/export?format=csv&gid=1079761384";

    public class UserProgress
    {
        public string thoiGian;
        public string userID;
        public string monID;
        public string baiID;
        public int soLanLam = 1;
        public int diemCaoNhat;
        public int diemGanNhat;
        public float tyLeDung;
        public string trangThai = "Đã hoàn thành";
    }

    // Key format: UserID_MonID_BaiID
    [NonSerialized]
    public Dictionary<string, UserProgress> progressData = new Dictionary<string, UserProgress>();

    private void Awake()
    {
        progressSheetCsvUrl = "https://docs.google.com/spreadsheets/d/1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY/export?format=csv&gid=1079761384";

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(FetchProgressFromSheet());
    }

    public IEnumerator FetchProgressFromSheet(Action<bool> onComplete = null)
    {
        if (string.IsNullOrEmpty(progressSheetCsvUrl))
        {
            onComplete?.Invoke(false);
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequest.Get(progressSheetCsvUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("[ProgressSyncManager] Error downloading progress data: " + www.error);
                onComplete?.Invoke(false);
            }
            else
            {
                ParseProgressCSV(www.downloadHandler.text);
                onComplete?.Invoke(true);
            }
        }
    }

    private void ParseProgressCSV(string csvText)
    {
        progressData.Clear();
        if (string.IsNullOrEmpty(csvText)) return;

        string[] lines = csvText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1) return; // Chỉ có header

        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 4) continue;

            try
            {
                UserProgress p = new UserProgress();
                // Format: Thời Gian (0), UserID (1), MonID (2), BaiID (3), Điểm (4), Tỷ Lệ Đúng (5)
                p.thoiGian = cols[0].Trim().Trim('"');
                p.userID = cols.Length > 1 ? cols[1].Trim().Trim('"') : "Học viên";
                p.monID = cols.Length > 2 ? cols[2].Trim().Trim('"') : "Vật Lý";
                p.baiID = cols.Length > 3 ? cols[3].Trim().Trim('"') : "bai2";

                if (cols.Length > 4) int.TryParse(cols[4].Trim().Trim('"'), out p.diemCaoNhat);
                p.diemGanNhat = p.diemCaoNhat;

                if (cols.Length > 5)
                {
                    string accStr = cols[5].Trim().Trim('"').Replace(',', '.');
                    float.TryParse(accStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out p.tyLeDung);
                }

                if (cols.Length > 6)
                {
                    p.trangThai = cols[6].Trim().Trim('"');
                }

                string key = $"{p.userID}_{p.monID}_{p.baiID}";
                if (progressData.TryGetValue(key, out UserProgress existing))
                {
                    existing.soLanLam++;
                    if (p.diemCaoNhat > existing.diemCaoNhat) existing.diemCaoNhat = p.diemCaoNhat;
                    existing.diemGanNhat = p.diemGanNhat;
                    existing.tyLeDung = p.tyLeDung;
                    existing.thoiGian = p.thoiGian;
                }
                else
                {
                    progressData[key] = p;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ProgressSyncManager] Lỗi đọc dòng " + i + ": " + e.Message);
            }
        }
        Debug.Log($"[ProgressSyncManager] ✅ Đã nạp thành công {progressData.Count} bản ghi tiến độ từ tab gid=1079761384.");
    }

    public UserProgress GetProgress(string userID, string monID, string baiID)
    {
        string key = $"{userID}_{monID}_{baiID}";
        if (progressData.TryGetValue(key, out UserProgress p))
        {
            return p;
        }
        return null;
    }
}
