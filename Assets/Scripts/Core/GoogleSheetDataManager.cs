using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Quản lý truy vấn dữ liệu từ Google Sheets:
/// 1. Đồng bộ Tài khoản (Username & Password & Role)
/// 2. Ngân hàng câu hỏi trắc nghiệm & tự luận
/// 3. Gửi kết quả thi đấu & đẩy câu hỏi mới lên Sheet
/// </summary>
public class GoogleSheetDataManager : MonoBehaviour
{
    private static GoogleSheetDataManager _instance;
    public static GoogleSheetDataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GoogleSheetDataManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GoogleSheetDataManager_Auto");
                    _instance = go.AddComponent<GoogleSheetDataManager>();
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("Google Sheet URLs (CSV Export)")]
    [Tooltip("Link CSV Sheet Tài khoản (Cột A: Username, Cột B: Password, Cột C: Role)")]
    public string accountsSheetCsvUrl = "";

    [Tooltip("Link CSV Sheet Câu hỏi (tab NganHangCauHoi 18 cột)")]
    public string questionsSheetCsvUrl = "https://docs.google.com/spreadsheets/d/1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY/export?format=csv&gid=286121556";

    [Header("Google Apps Script Web App URL (Để Gửi Kết Quả & Đẩy Câu Hỏi)")]
    [Tooltip("URL Web App triển khai từ Google Apps Script để nhận POST request")]
    public string postResultWebAppUrl = "https://script.google.com/macros/s/AKfycbyfLFooFWxOWfMqVu1rZDfbCUOY0OnmtQb83-J5wNr0U_8I-SXMEpIaWG_2gC1XSNOyRQ/exec";

    private Dictionary<string, UserAccount> accountDatabase = new Dictionary<string, UserAccount>(StringComparer.OrdinalIgnoreCase);
    private List<QuestionData> globalQuestionBank = new List<QuestionData>();

    private void Awake()
    {
        postResultWebAppUrl = "https://script.google.com/macros/s/AKfycbyfLFooFWxOWfMqVu1rZDfbCUOY0OnmtQb83-J5wNr0U_8I-SXMEpIaWG_2gC1XSNOyRQ/exec";
        questionsSheetCsvUrl = "https://docs.google.com/spreadsheets/d/1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY/export?format=csv&gid=286121556";

        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (UnityEngine.Object.FindFirstObjectByType<ProgressSyncManager>() == null)
            {
                UnityEngine.GameObject psmGO = new UnityEngine.GameObject("ProgressSyncManager_Auto");
                psmGO.AddComponent<ProgressSyncManager>();
                DontDestroyOnLoad(psmGO);
            }
        }
        else if (_instance != this)
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
                    UserRole r = UserRole.Student;
                    if (u.StartsWith("gv", StringComparison.OrdinalIgnoreCase) || 
                        u.StartsWith("admin", StringComparison.OrdinalIgnoreCase) || 
                        u.StartsWith("teacher", StringComparison.OrdinalIgnoreCase))
                    {
                        r = UserRole.Teacher;
                    }
                    if (cols.Length >= 3 && cols[2].Trim().Trim('"').Equals("Teacher", StringComparison.OrdinalIgnoreCase))
                    {
                        r = UserRole.Teacher;
                    }

                    UserAccount acc = new UserAccount { username = u, password = p, role = r };
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
        accountDatabase["admin"] = new UserAccount { username = "admin", password = "123456", role = UserRole.Teacher, fullName = "Thầy Quản Trị" };
        accountDatabase["teacher"] = new UserAccount { username = "teacher", password = "123456", role = UserRole.Teacher, fullName = "Cô Giáo Thảo" };
        accountDatabase["student"] = new UserAccount { username = "student", password = "123456", role = UserRole.Student, fullName = "Em Học Sinh" };
        accountDatabase["pru"] = new UserAccount { username = "pru", password = "123456", role = UserRole.Student, fullName = "Nhóm 8 PRU213" };
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
        if (string.IsNullOrEmpty(csvContent)) return;

        List<List<string>> rows = ParseCsvToRows(csvContent);
        if (rows.Count <= 1) return;

        // Xác định vị trí các cột qua dòng tiêu đề (Header)
        List<string> header = rows[0];
        int colCauHoiID = -1, colBaiID = -1, colMucDo = -1, colNoiDung = -1;
        int colDapAnA = -1, colDapAnB = -1, colDapAnC = -1, colDapAnD = -1, colDapAnDung = -1;
        int colTenChuong = -1, colTenBai = -1, colGiaiThich = -1;

        for (int c = 0; c < header.Count; c++)
        {
            string h = header[c].Trim().ToUpperInvariant();
            if (h.Contains("CAUHOIID") || h == "ID") colCauHoiID = c;
            else if (h.Contains("BAIID") || h == "BAI") colBaiID = c;
            else if (h.Contains("TENCHUONG") || h.Contains("CHUONG")) colTenChuong = c;
            else if (h.Contains("TENBAI")) colTenBai = c;
            else if (h.Contains("MUCDO") || h.Contains("DIFFICULTY")) colMucDo = c;
            else if (h.Contains("NOIDUNG") || h.Contains("QUESTION") || h.Contains("DEBAI")) colNoiDung = c;
            else if (h == "DAPANA" || h == "A" || h.Contains("OPTIONA")) colDapAnA = c;
            else if (h == "DAPANB" || h == "B" || h.Contains("OPTIONB")) colDapAnB = c;
            else if (h == "DAPANC" || h == "C" || h.Contains("OPTIONC")) colDapAnC = c;
            else if (h == "DAPAND" || h == "D" || h.Contains("OPTIOND")) colDapAnD = c;
            else if (h.Contains("DAPANDUNG") || h == "ANSWER" || h.Contains("CORRECT")) colDapAnDung = c;
            else if (h.Contains("GIAITHICH") || h.Contains("EXPLAIN")) colGiaiThich = c;
        }

        // Nếu không nhận diện được qua header, dùng fallback theo số cột
        bool is18Cols = header.Count >= 18;
        bool is14Cols = header.Count >= 14 && !is18Cols;

        for (int r = 1; r < rows.Count; r++)
        {
            List<string> cols = rows[r];
            if (cols.Count < 6) continue;

            string qText = "";
            string optA = "", optB = "", optC = "", optD = "";
            string correctLetter = "A";
            string explanation = "";

            if (colNoiDung >= 0 && colNoiDung < cols.Count)
            {
                qText = cols[colNoiDung];
                optA = (colDapAnA >= 0 && colDapAnA < cols.Count) ? cols[colDapAnA] : "";
                optB = (colDapAnB >= 0 && colDapAnB < cols.Count) ? cols[colDapAnB] : "";
                optC = (colDapAnC >= 0 && colDapAnC < cols.Count) ? cols[colDapAnC] : "";
                optD = (colDapAnD >= 0 && colDapAnD < cols.Count) ? cols[colDapAnD] : "";
                correctLetter = (colDapAnDung >= 0 && colDapAnDung < cols.Count) ? cols[colDapAnDung] : "A";
                explanation = (colGiaiThich >= 0 && colGiaiThich < cols.Count) ? cols[colGiaiThich] : "";
            }
            else if (is18Cols)
            {
                // STT(0), CauHoiID(1), ChuongId(2), TenChuong(3), BaiID(4), TenBai(5), Loai(6), MucDo(7), NoiDung(8), A(9), B(10), C(11), D(12), Dung(13), GiaiThich(14)
                qText = cols[8];
                optA = cols[9];
                optB = cols[10];
                optC = cols[11];
                optD = cols[12];
                correctLetter = cols[13];
                if (cols.Count > 14) explanation = cols[14];
            }
            else if (is14Cols)
            {
                // CauHoiID(0), BaiID(1), MucDo(2), NoiDung(3), HinhAnh(4), A(5), B(6), C(7), D(8), Dung(9), GiaiThich(10)
                qText = cols[3];
                optA = cols[5];
                optB = cols[6];
                optC = cols[7];
                optD = cols[8];
                correctLetter = cols[9];
                if (cols.Count > 10) explanation = cols[10];
            }
            else
            {
                // Legacy fallback: 0:Subj, 1:Chap, 2:Text, 3:A, 4:B, 5:C, 6:D, 7:Correct
                qText = cols[2];
                optA = cols[3];
                optB = cols[4];
                optC = cols[5];
                optD = cols[6];
                correctLetter = cols[7];
            }

            if (string.IsNullOrWhiteSpace(qText)) continue;

            int correctIdx = 0;
            string cleanAns = correctLetter.Trim().ToUpperInvariant();
            if (cleanAns == "B" || cleanAns == "1") correctIdx = 1;
            else if (cleanAns == "C" || cleanAns == "2") correctIdx = 2;
            else if (cleanAns == "D" || cleanAns == "3") correctIdx = 3;

            QuestionData q = ScriptableObject.CreateInstance<QuestionData>();
            q.questionText = qText;
            q.options = new string[] { optA, optB, optC, optD };
            q.correctOptionIndex = correctIdx;
            q.subject = SubjectType.Physics;

            globalQuestionBank.Add(q);
        }

        if (GameSession.Instance != null)
        {
            GameSession.Instance.loadedQuestions = new List<QuestionData>(globalQuestionBank);
        }

        Debug.Log($"[GoogleSheet] ✅ Đã nạp thành công {globalQuestionBank.Count} câu hỏi từ Sheet.");
    }

    private static List<List<string>> ParseCsvToRows(string csvText)
    {
        List<List<string>> result = new List<List<string>>();
        if (string.IsNullOrEmpty(csvText)) return result;

        List<string> currentRow = new List<string>();
        System.Text.StringBuilder currentCell = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < csvText.Length; i++)
        {
            char c = csvText[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < csvText.Length && csvText[i + 1] == '"')
                    {
                        currentCell.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    currentCell.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    currentRow.Add(currentCell.ToString().Trim());
                    currentCell.Clear();
                }
                else if (c == '\r')
                {
                    // bỏ qua
                }
                else if (c == '\n')
                {
                    currentRow.Add(currentCell.ToString().Trim());
                    currentCell.Clear();
                    result.Add(currentRow);
                    currentRow = new List<string>();
                }
                else
                {
                    currentCell.Append(c);
                }
            }
        }

        if (currentCell.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(currentCell.ToString().Trim());
            result.Add(currentRow);
        }

        return result;
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
            Debug.LogWarning("[GoogleSheetDataManager] Chưa cấu hình URL Web App, không thể gửi điểm!");
            onComplete?.Invoke(false);
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("action", "update_progress"); // Khớp với logic doPost trong Apps Script
        form.AddField("userID", userID);
        form.AddField("monID", monID);
        form.AddField("baiID", baiID);
        form.AddField("score", score.ToString());
        form.AddField("accuracy", accuracy.ToString("F2"));
        form.AddField("status", "Đã hoàn thành");

        Debug.Log($"[GoogleSheetDataManager] Đang gửi kết quả: User={userID}, Mon={monID}, Bai={baiID}, Diem={score}");

        using (UnityWebRequest www = UnityWebRequest.Post(postResultWebAppUrl, form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[GoogleSheetDataManager] Gửi kết quả thành công!");
                // Cập nhật lại UI thông qua ProgressSyncManager
                if (ProgressSyncManager.Instance != null)
                {
                    StartCoroutine(ProgressSyncManager.Instance.FetchProgressFromSheet());
                }
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogError("[GoogleSheetDataManager] Lỗi khi gửi kết quả: " + www.error);
                onComplete?.Invoke(false);
            }
        }
    }

    /// <summary>
    /// Đẩy câu hỏi mới do Giáo viên tạo lên Google Sheets (tab NganHangCauHoi chuẩn 18 cột)
    /// </summary>
    public IEnumerator SubmitQuestionToSheet(ScienceQuest.Quiz.QuestionData q, Action<bool, string> onComplete = null)
    {
        if (string.IsNullOrEmpty(postResultWebAppUrl))
        {
            Debug.LogWarning("[GoogleSheetDataManager] Chưa cấu hình URL Web App để đẩy câu hỏi!");
            onComplete?.Invoke(false, "Chưa cấu hình URL Google Apps Script Web App!");
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("action", "add_question");
        form.AddField("stt", q.stt > 0 ? q.stt.ToString() : "");
        form.AddField("cauHoiID", q.questionID ?? "");
        form.AddField("chuongId", !string.IsNullOrEmpty(q.chuongId) ? q.chuongId : "chuong1");
        form.AddField("tenChuong", !string.IsNullOrEmpty(q.tenChuong) ? q.tenChuong : (q.chapter ?? ""));
        form.AddField("baiID", q.baiID ?? "bai2");
        form.AddField("tenBai", !string.IsNullOrEmpty(q.tenBai) ? q.tenBai : "");
        form.AddField("loaiCauHoi", !string.IsNullOrEmpty(q.loaiCauHoi) ? q.loaiCauHoi : "BaiHoc");
        form.AddField("mucDo", !string.IsNullOrEmpty(q.mucDo) ? q.mucDo : "Nhận biết");
        form.AddField("noiDung", q.questionText ?? "");
        form.AddField("hinhAnh", q.hinhAnh ?? "");
        form.AddField("dapAnA", (q.answers != null && q.answers.Length > 0) ? q.answers[0] : "");
        form.AddField("dapAnB", (q.answers != null && q.answers.Length > 1) ? q.answers[1] : "");
        form.AddField("dapAnC", (q.answers != null && q.answers.Length > 2) ? q.answers[2] : "");
        form.AddField("dapAnD", (q.answers != null && q.answers.Length > 3) ? q.answers[3] : "");

        string correctLetter = "A";
        if (!string.IsNullOrEmpty(q.loaiCauHoi) && (q.loaiCauHoi.Contains("Tự luận") || q.loaiCauHoi.Contains("TuLuan")))
        {
            correctLetter = !string.IsNullOrEmpty(q.dapAnThamChieu) ? q.dapAnThamChieu : (!string.IsNullOrEmpty(q.explanation) ? q.explanation : "Đáp án tham chiếu");
        }
        else
        {
            if (q.correctAnswerIndex == 1) correctLetter = "B";
            else if (q.correctAnswerIndex == 2) correctLetter = "C";
            else if (q.correctAnswerIndex == 3) correctLetter = "D";
        }
        form.AddField("dapAnDung", correctLetter);

        form.AddField("giaiThich", q.explanation ?? "");
        form.AddField("meoGhiNho", q.meoGhiNho ?? "");
        form.AddField("trangThai", q.trangThai ?? "Xuất bản");
        form.AddField("giaoVienID", q.giaoVienID ?? "GV001");
        form.AddField("dapAnThamChieu", q.dapAnThamChieu ?? "");
        form.AddField("tieuChiCham", q.tieuChiCham ?? "");

        Debug.Log($"[GoogleSheetDataManager] 🚀 Đang đẩy câu hỏi 18 cột lên Sheet: ID={q.questionID}, MucDo={q.mucDo}, Bai={q.baiID}, NoiDung={q.questionText}");

        using (UnityWebRequest www = UnityWebRequest.Post(postResultWebAppUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string respText = www.downloadHandler.text;
                Debug.Log($"[GoogleSheetDataManager] ✅ Đẩy câu hỏi lên Sheet thành công! Phản hồi: {respText}");
                onComplete?.Invoke(true, respText);
            }
            else
            {
                Debug.LogError($"[GoogleSheetDataManager] ❌ Lỗi khi đẩy câu hỏi: {www.error}");
                onComplete?.Invoke(false, www.error);
            }
        }
    }

    #endregion
}
