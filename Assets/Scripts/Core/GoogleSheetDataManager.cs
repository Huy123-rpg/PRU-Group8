using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Quản lý truy vấn dữ liệu từ Google Sheets:
/// 1. Đồng bộ Tài khoản (Username & Password)
/// 2. Ngân hàng câu hỏi trắc nghiệm
/// 3. Gửi kết quả thi đấu lên Sheet
/// </summary>
public class GoogleSheetDataManager : MonoBehaviour
{
    public static GoogleSheetDataManager Instance { get; private set; }

    [Header("Google Sheet URLs (CSV Export)")]
    [Tooltip("Link CSV Sheet Tài khoản (Cột A: Username, Cột B: Password)")]
    public string accountsSheetCsvUrl = "";

    [Tooltip("Link CSV Sheet Câu hỏi (Cột: Subject, Chapter, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectIndex)")]
    public string questionsSheetCsvUrl = "";

    [Header("Google Apps Script Web App URL (Để Gửi Kết Quả)")]
    [Tooltip("URL Web App triển khai từ Google Apps Script để nhận POST request kết quả")]
    public string postResultWebAppUrl = "https://script.google.com/macros/s/AKfycbzrMTiHuO-u1omw69Qt8BK25FahlXH1769Y8NylNp6avADXgXPOrFAVIvgasn6I4Aqgzg/exec";

    private Dictionary<string, UserAccount> accountDatabase = new Dictionary<string, UserAccount>(StringComparer.OrdinalIgnoreCase);
    private List<QuestionData> globalQuestionBank = new List<QuestionData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            gameObject.AddComponent<ProgressSyncManager>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        AddFallbackDefaultAccounts();

        if (!string.IsNullOrEmpty(questionsSheetCsvUrl))
        {
            StartCoroutine(FetchQuestionsFromSheet((success) =>
            {
                if (success)
                {
                    Debug.Log($"<color=green>[GoogleSheet] Đã tải thành công {globalQuestionBank.Count} câu hỏi từ Sheet khi Start!</color>");
                }
            }));
        }
    }

    #region --- 1. QUẢN LÝ TÀI KHOẢN ---

    public IEnumerator FetchAccountsFromSheet(Action<bool> onComplete = null)
    {
        if (string.IsNullOrEmpty(accountsSheetCsvUrl))
        {
            onComplete?.Invoke(false);
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequest.Get(accountsSheetCsvUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string csvText = www.downloadHandler.text;
                    ParseAccountsCsv(csvText);
                    onComplete?.Invoke(true);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GoogleSheet] Lỗi đọc CSV tài khoản: {ex.Message}");
                    onComplete?.Invoke(false);
                }
            }
            else
            {
                onComplete?.Invoke(false);
            }
        }
    }

    private void ParseAccountsCsv(string csvContent)
    {
        accountDatabase.Clear();
        AddFallbackDefaultAccounts();

        string[] lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            string[] cols = line.Split(',');
            if (cols.Length >= 2)
            {
                string u = cols[0].Trim().Trim('"');
                string p = cols[1].Trim().Trim('"');

                if (u.Equals("username", StringComparison.OrdinalIgnoreCase) || u.Equals("taikhoan", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!string.IsNullOrEmpty(u))
                {
                    UserAccount acc = new UserAccount { username = u, password = p };
                    accountDatabase[u] = acc;
                }
            }
        }
        Debug.Log($"[GoogleSheet] Đã tải thành công {accountDatabase.Count} tài khoản từ Sheet.");
    }

    public bool Authenticate(string username, string password, out UserAccount matchedAccount)
    {
        matchedAccount = null;
        if (accountDatabase.TryGetValue(username, out UserAccount acc))
        {
            if (acc.password == password)
            {
                matchedAccount = acc;
                return true;
            }
        }
        return false;
    }

    private void AddFallbackDefaultAccounts()
    {
        accountDatabase["admin"] = new UserAccount { username = "admin", password = "123456" };
        accountDatabase["student"] = new UserAccount { username = "student", password = "123456" };
        accountDatabase["pru"] = new UserAccount { username = "pru", password = "123456" };
    }

    #endregion

    #region --- 2. QUẢN LÝ CÂU HỎI TỪ SHEET ---

    public IEnumerator FetchQuestionsFromSheet(Action<bool> onComplete = null)
    {
        if (string.IsNullOrEmpty(questionsSheetCsvUrl))
        {
            onComplete?.Invoke(false);
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequest.Get(questionsSheetCsvUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string csvText = www.downloadHandler.text;
                    ParseQuestionsCsv(csvText);
                    onComplete?.Invoke(true);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GoogleSheet] Lỗi đọc CSV câu hỏi: {ex.Message}");
                    onComplete?.Invoke(false);
                }
            }
            else
            {
                onComplete?.Invoke(false);
            }
        }
    }

    private void ParseQuestionsCsv(string csvContent)
    {
        globalQuestionBank.Clear();

        string[] lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            string[] cols = line.Split(',');
            if (cols.Length >= 8)
            {
                string subjStr = cols[0].Trim().Trim('"');
                int.TryParse(cols[1].Trim().Trim('"'), out int chapterNum);
                string qText = cols[2].Trim().Trim('"');
                string optA = cols[3].Trim().Trim('"');
                string optB = cols[4].Trim().Trim('"');
                string optC = cols[5].Trim().Trim('"');
                string optD = cols[6].Trim().Trim('"');
                int.TryParse(cols[7].Trim().Trim('"'), out int correctIdx);

                if (qText.Equals("QuestionText", StringComparison.OrdinalIgnoreCase)) continue;

                SubjectType subject = SubjectType.Biology;
                if (subjStr.StartsWith("Hóa", StringComparison.OrdinalIgnoreCase) || subjStr.Equals("Chemistry", StringComparison.OrdinalIgnoreCase))
                    subject = SubjectType.Chemistry;
                else if (subjStr.StartsWith("Vật", StringComparison.OrdinalIgnoreCase) || subjStr.StartsWith("Vat", StringComparison.OrdinalIgnoreCase) || subjStr.Equals("Physics", StringComparison.OrdinalIgnoreCase))
                    subject = SubjectType.Physics;

                QuestionData q = ScriptableObject.CreateInstance<QuestionData>();
                q.questionText = qText;
                q.options = new string[] { optA, optB, optC, optD };
                q.correctOptionIndex = Mathf.Clamp(correctIdx, 0, 3);
                q.subject = subject;

                globalQuestionBank.Add(q);
            }
        }

        if (GameSession.Instance != null)
        {
            GameSession.Instance.loadedQuestions = new List<QuestionData>(globalQuestionBank);
        }

        Debug.Log($"[GoogleSheet] Đã nạp thành công {globalQuestionBank.Count} câu hỏi từ Sheet.");
    }

    public List<QuestionData> GetQuestionsForSubject(SubjectType subject)
    {
        List<QuestionData> result = new List<QuestionData>();
        foreach (var q in globalQuestionBank)
        {
            if (q.subject == subject)
            {
                result.Add(q);
            }
        }
        return result;
    }

    #endregion

    #region --- 3. GỬI KẾT QUẢ VỀ SHEET ---

    public IEnumerator SubmitResultToSheet(RaceResult result, Action<bool> onComplete = null)
    {
        if (string.IsNullOrEmpty(postResultWebAppUrl))
        {
            onComplete?.Invoke(false);
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("username", GameSession.Instance != null ? GameSession.Instance.username : "Player");
        form.AddField("subject", GameSession.Instance != null && GameSession.Instance.selectedSubject != null ? GameSession.Instance.selectedSubject.subjectName : "Môn học");
        form.AddField("rank", result.rank);
        form.AddField("totalScore", result.totalScore);
        form.AddField("correctAnswers", result.correctAnswers);
        form.AddField("wrongAnswers", result.wrongAnswers);
        form.AddField("completionTime", result.completionTime.ToString("F1"));
        form.AddField("date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        using (UnityWebRequest www = UnityWebRequest.Post(postResultWebAppUrl, form))
        {
            yield return www.SendWebRequest();
            onComplete?.Invoke(www.result == UnityWebRequest.Result.Success);
        }
    }

    public IEnumerator SubmitProgressToSheet(string userID, string monID, string baiID, int score, float accuracy, Action<bool> onComplete = null)
    {
        if (string.IsNullOrEmpty(postResultWebAppUrl))
        {
            Debug.LogWarning("[GoogleSheetDataManager] Chua cau hinh link Web App, khong the gui diem!");
            onComplete?.Invoke(false);
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("action", "update_progress"); // Khop voi logic doPost trong Apps Script
        form.AddField("userID", userID);
        form.AddField("monID", monID);
        form.AddField("baiID", baiID);
        form.AddField("score", score.ToString());
        form.AddField("accuracy", accuracy.ToString("F2"));
        form.AddField("status", "Da hoan thanh");

        Debug.Log($"[GoogleSheetDataManager] Dang gui ket qua: User={userID}, Mon={monID}, Bai={baiID}, Diem={score}");

        using (UnityWebRequest www = UnityWebRequest.Post(postResultWebAppUrl, form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[GoogleSheetDataManager] Gui ket qua thanh cong!");
                // Cap nhat lai UI thong qua ProgressSyncManager
                if (ProgressSyncManager.Instance != null)
                {
                    StartCoroutine(ProgressSyncManager.Instance.FetchProgressFromSheet());
                }
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogError("[GoogleSheetDataManager] Loi khi gui ket qua: " + www.error);
                onComplete?.Invoke(false);
            }
        }
    }

    #endregion
}
