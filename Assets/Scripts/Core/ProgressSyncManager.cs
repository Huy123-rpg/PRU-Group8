using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class ProgressSyncManager : MonoBehaviour
{
    public static ProgressSyncManager Instance { get; private set; }

    public string progressSheetCsvUrl = "https://docs.google.com/spreadsheets/d/1gU3bON6ltcIzDhTMEriawUpAAEMDJOylGYtlAY88jww/export?format=csv&gid=464037230";

    public class UserProgress
    {
        public string userID;
        public string monID;
        public string baiID;
        public int soLanLam;
        public int diemCaoNhat;
        public int diemGanNhat;
        public float tyLeDung;
        public string trangThai;
    }

    // Key format: UserID_MonID_BaiID
    public Dictionary<string, UserProgress> progressData = new Dictionary<string, UserProgress>();

    private void Awake()
    {
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
        string[] lines = csvText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length <= 1) return; // Chỉ có header

        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 6) continue;

            try
            {
                UserProgress p = new UserProgress();
                p.userID = cols[1].Trim();
                p.monID = cols[2].Trim();
                p.baiID = cols[3].Trim();
                int.TryParse(cols[4].Trim(), out p.soLanLam);
                int.TryParse(cols[5].Trim(), out p.diemCaoNhat);
                if (cols.Length > 6) int.TryParse(cols[6].Trim(), out p.diemGanNhat);
                
                string key = $"{p.userID}_{p.monID}_{p.baiID}";
                progressData[key] = p;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error parsing row " + i + ": " + e.Message);
            }
        }
        Debug.Log($"[ProgressSyncManager] Parsed {progressData.Count} progress records from Google Sheet.");
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
