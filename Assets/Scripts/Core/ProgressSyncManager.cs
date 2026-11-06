using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class ProgressSyncManager : MonoBehaviour
{
    public static ProgressSyncManager Instance { get; private set; }

    [Tooltip("Link CSV Sheet Tiến độ học tập (gid=11169486)")]
    public string progressSheetCsvUrl = "https://docs.google.com/spreadsheets/d/1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY/export?format=csv&gid=11169486";

    [Tooltip("Link CSV Sheet Lịch sử làm bài (gid=1079761384)")]
    public string historySheetCsvUrl = "https://docs.google.com/spreadsheets/d/1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY/export?format=csv&gid=1079761384";

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
        progressSheetCsvUrl = "https://docs.google.com/spreadsheets/d/1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY/export?format=csv&gid=11169486";
        historySheetCsvUrl = "https://docs.google.com/spreadsheets/d/1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY/export?format=csv&gid=1079761384";

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
        bool successProgress = false;
        bool successHistory = false;

        // 1. Tải Tiến độ học tập (gid=11169486)
        if (!string.IsNullOrEmpty(progressSheetCsvUrl))
        {
            using (UnityWebRequest www = UnityWebRequest.Get(progressSheetCsvUrl))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    ParseProgressCSV(www.downloadHandler.text);
                    successProgress = true;
                }
                else
                {
                    Debug.LogWarning("[ProgressSyncManager] Error downloading progress (gid=11169486): " + www.error);
                }
            }
        }

        // 2. Tải Lịch sử làm bài (gid=1079761384)
        if (!string.IsNullOrEmpty(historySheetCsvUrl))
        {
            using (UnityWebRequest www = UnityWebRequest.Get(historySheetCsvUrl))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    ParseHistoryCSV(www.downloadHandler.text);
                    successHistory = true;
                }
                else
                {
                    Debug.LogWarning("[ProgressSyncManager] Error downloading history (gid=1079761384): " + www.error);
                }
            }
        }

        onComplete?.Invoke(successProgress || successHistory);
    }

    /// <summary>
    /// Parse CSV tab Tiến độ học tập (TienDoID,UserID,MonID,BaiID,SoLanLam,DiemCaoNhat,DiemGanNhat,TyLeDung,TrangThai,LanHocCuoi)
    /// </summary>
    private void ParseProgressCSV(string csvText)
    {
        if (string.IsNullOrEmpty(csvText)) return;
        string[] lines = csvText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1) return;

        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 4) continue;

            try
            {
                UserProgress p = new UserProgress();
                // TienDoID(0), UserID(1), MonID(2), BaiID(3), SoLanLam(4), DiemCaoNhat(5), DiemGanNhat(6), TyLeDung(7), TrangThai(8), LanHocCuoi(9)
                p.userID = cols.Length > 1 ? cols[1].Trim().Trim('"') : "Học viên";
                p.monID = cols.Length > 2 ? cols[2].Trim().Trim('"') : "Vật lý";
                p.baiID = cols.Length > 3 ? cols[3].Trim().Trim('"') : "bai1";

                if (cols.Length > 4) int.TryParse(cols[4].Trim().Trim('"'), out p.soLanLam);

                if (cols.Length > 5)
                {
                    string dStr = cols[5].Trim().Trim('"').Replace("%", "");
                    int.TryParse(dStr, out p.diemCaoNhat);
                }

                if (cols.Length > 6)
                {
                    string dStr = cols[6].Trim().Trim('"').Replace("%", "");
                    int.TryParse(dStr, out p.diemGanNhat);
                }

                if (cols.Length > 7)
                {
                    string accStr = cols[7].Trim().Trim('"').Replace(',', '.').Replace("%", "");
                    if (float.TryParse(accStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float acc))
                    {
                        p.tyLeDung = (acc <= 1f && acc > 0f) ? acc * 100f : acc;
                    }
                }

                if (cols.Length > 8) p.trangThai = cols[8].Trim().Trim('"');
                if (cols.Length > 9) p.thoiGian = cols[9].Trim().Trim('"');

                string key = $"{p.userID}_{p.monID}_{p.baiID}";
                progressData[key] = p;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ProgressSyncManager] Lỗi đọc dòng progress {i}: {ex.Message}");
            }
        }
        Debug.Log($"[ProgressSyncManager] ✅ Đã tải thành công {progressData.Count} bản ghi tiến độ từ tab gid=11169486.");
    }

    /// <summary>
    /// Parse CSV tab Lịch sử làm bài (Thời Gian,UserID,MonID,BaiID,Điểm,Tỷ Lệ Đúng)
    /// </summary>
    private void ParseHistoryCSV(string csvText)
    {
        if (string.IsNullOrEmpty(csvText)) return;
        string[] lines = csvText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1) return;

        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 4) continue;

            try
            {
                UserProgress p = new UserProgress();
                // Thời Gian (0), UserID (1), MonID (2), BaiID (3), Điểm (4), Tỷ Lệ Đúng (5)
                p.thoiGian = cols[0].Trim().Trim('"');
                p.userID = cols.Length > 1 ? cols[1].Trim().Trim('"') : "Học viên";
                p.monID = cols.Length > 2 ? cols[2].Trim().Trim('"') : "Vật lý";
                p.baiID = cols.Length > 3 ? cols[3].Trim().Trim('"') : "bai1";

                if (cols.Length > 4) int.TryParse(cols[4].Trim().Trim('"'), out p.diemCaoNhat);
                p.diemGanNhat = p.diemCaoNhat;

                if (cols.Length > 5)
                {
                    string accStr = cols[5].Trim().Trim('"').Replace(',', '.').Replace("%", "");
                    if (float.TryParse(accStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float acc))
                    {
                        p.tyLeDung = (acc <= 1f && acc > 0f) ? acc * 100f : acc;
                    }
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
            catch (Exception ex)
            {
                Debug.LogWarning($"[ProgressSyncManager] Lỗi đọc dòng history {i}: {ex.Message}");
            }
        }
        Debug.Log($"[ProgressSyncManager] ✅ Đã nạp thành công lịch sử làm bài từ tab gid=1079761384 (tổng {progressData.Count} bản ghi).");
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
