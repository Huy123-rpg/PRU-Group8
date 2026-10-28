using System.Text.RegularExpressions;
using System.IO;
using System.Text;

// 1. GoogleSheetDataManager
string p1 = "Assets/Scripts/Core/GoogleSheetDataManager.cs";
string c1 = File.ReadAllText(p1, Encoding.UTF8);
c1 = c1.Replace("Instance = this;", "Instance = this;\n            gameObject.AddComponent<ProgressSyncManager>();");
c1 = c1.Replace("public string postResultWebAppUrl = \"\";", "public string postResultWebAppUrl = \"https://script.google.com/macros/s/AKfycbzrMTiHuO-u1omw69Qt8BK25FahlXH1769Y8NylNp6avADXgXPOrFAVIvgasn6I4Aqgzg/exec\";");

string newMethod = @"

    public System.Collections.IEnumerator SubmitProgressToSheet(string userID, string monID, string baiID, int score, float accuracy, System.Action<bool> onComplete = null)
    {
        if (string.IsNullOrEmpty(postResultWebAppUrl))
        {
            UnityEngine.Debug.LogWarning("[GoogleSheetDataManager] Chưa cấu hình link Web App, không thể gửi điểm!");
            onComplete?.Invoke(false);
            yield break;
        }

        UnityEngine.WWWForm form = new UnityEngine.WWWForm();
        form.AddField("action", "update_progress");
        form.AddField("userID", userID);
        form.AddField("monID", monID);
        form.AddField("baiID", baiID);
        form.AddField("score", score.ToString());
        form.AddField("accuracy", accuracy.ToString("F2"));
        form.AddField("status", "Đã hoàn thành");

        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Post(postResultWebAppUrl, form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                if (ProgressSyncManager.Instance != null)
                {
                    ProgressSyncManager.Instance.StartCoroutine(ProgressSyncManager.Instance.FetchProgressFromSheet());
                }
                onComplete?.Invoke(true);
            }
            else
            {
                onComplete?.Invoke(false);
            }
        }
    }
";
c1 = Regex.Replace(c1, @"#endregion\s*\}", "#endregion" + newMethod + "\n}");
File.WriteAllText(p1, c1, Encoding.UTF8);


// 2. MainMenuManager
string p2 = "Assets/Scripts/MainMenuManager.cs";
string c2 = File.ReadAllText(p2, Encoding.UTF8);
string m2_find = @"private static string BuildProgressLine\(string subject, string chapterID, string lessonID\)\s*\{[\s\S]*?return \$"Progress[\s\S]*?\}";
string m2_rep = @"
    private static string BuildProgressLine(string subject, string chapterID, string lessonID)
    {
        KnowledgeProgressManager.LessonStats s = KnowledgeProgressManager.GetStats(subject, chapterID, lessonID);
        string username = GameSession.Instance != null && !string.IsNullOrEmpty(GameSession.Instance.username) ? GameSession.Instance.username : "HS001";
        string baiID = $"B{chapterID}_00{lessonID}"; 
        string monID = "KHTN6";
        if (subject.Contains("Chem") || subject.Contains("Hóa")) monID = "KHTN7";
        if (subject.Contains("Phy") || subject.Contains("Lý")) monID = "KHTN8";

        int attempts = 0;
        int highScore = 0;
        if (ProgressSyncManager.Instance != null)
        {
            var p = ProgressSyncManager.Instance.GetProgress(username, monID, baiID);
            if (p != null)
            {
                attempts = p.soLanLam;
                highScore = p.diemCaoNhat;
            }
        }
        string result = " ";
        if (attempts > 0 || highScore > 0) result = $"Số lượt làm: {attempts} | Điểm cao nhất: {highScore}";
        else if (s != null && s.OverallProgress > 0) result = $"Progress: {s.OverallProgress:0}% | Trắc nghiệm: {s.McAccuracy:0}%";
        return result;
    }";
c2 = Regex.Replace(c2, m2_find, m2_rep);
File.WriteAllText(p2, c2, Encoding.UTF8);


// 3. PhysicsMenuManager
string p3 = "Assets/Scripts/UI/PhysicsMenuManager.cs";
string c3 = File.ReadAllText(p3, Encoding.UTF8);
string m3_find = @"tmp\.text = currentLesson;(?:\s*//.*?)?";
string m3_rep = @"tmp.text = currentLesson;
                        string username = GameSession.Instance != null && !string.IsNullOrEmpty(GameSession.Instance.username) ? GameSession.Instance.username : "HS001";
                        string baiID = $"B{GetCurrentChapterIndex()}_00{i+1}";
                        string monID = "KHTN8"; 
                        int attempts = 0, highScore = 0;
                        if (ProgressSyncManager.Instance != null) {
                            var p = ProgressSyncManager.Instance.GetProgress(username, monID, baiID);
                            if (p != null) { attempts = p.soLanLam; highScore = p.diemCaoNhat; }
                        }
                        if (attempts > 0 || highScore > 0) tmp.text += $"\n<size=16><color=#FFD700>Số lượt làm: {attempts} | Điểm cao nhất: {highScore}</color></size>";
";
c3 = Regex.Replace(c3, m3_find, m3_rep);
File.WriteAllText(p3, c3, Encoding.UTF8);


// 4. BiologyMenuManager
string p4 = "Assets/Scripts/UI/BiologyMenuManager.cs";
string c4 = File.ReadAllText(p4, Encoding.UTF8);
string m4_find = @"tmp\.text = assignedLesson;(?:\s*//.*?)?";
string m4_rep = @"tmp.text = assignedLesson;
                        string username = GameSession.Instance != null && !string.IsNullOrEmpty(GameSession.Instance.username) ? GameSession.Instance.username : "HS001";
                        string baiNum = "1";
                        if (btnName.Contains("bai2")) baiNum = "2";
                        else if (btnName.Contains("bai3")) baiNum = "3";
                        else if (btnName.Contains("bai4")) baiNum = "4";
                        string baiID = $"B1_00{baiNum}"; 
                        string monID = "KHTN6"; 
                        int attempts = 0, highScore = 0;
                        if (ProgressSyncManager.Instance != null) {
                            var p = ProgressSyncManager.Instance.GetProgress(username, monID, baiID);
                            if (p != null) { attempts = p.soLanLam; highScore = p.diemCaoNhat; }
                        }
                        if (attempts > 0 || highScore > 0) tmp.text += $"\n<size=20><color=#FFD700>Số lượt làm: {attempts} | Điểm cao nhất: {highScore}</color></size>";
";
c4 = Regex.Replace(c4, m4_find, m4_rep);
File.WriteAllText(p4, c4, Encoding.UTF8);


// 5. QuizManager (Biology)
string p5 = "Assets/Scripts/QuizManager.cs";
string c5 = File.ReadAllText(p5, Encoding.UTF8);
string m5_find = @"highScore = currentScore;\s*PlayerPrefs\.SetInt\("Sinh_HighScore", highScore\);\s*\}";
string m5_rep = @"highScore = currentScore;
            PlayerPrefs.SetInt("Sinh_HighScore", highScore);
        }

        if (GoogleSheetDataManager.Instance != null)
        {
            string userID = GameSession.Instance != null && !string.IsNullOrEmpty(GameSession.Instance.username) ? GameSession.Instance.username : "HS001";
            string lesson = PlayerPrefs.GetString("QuizLesson", "B1_001");
            GoogleSheetDataManager.Instance.StartCoroutine(GoogleSheetDataManager.Instance.SubmitProgressToSheet(userID, "KHTN6", lesson, currentScore, 100f));
        }
";
c5 = Regex.Replace(c5, m5_find, m5_rep);
File.WriteAllText(p5, c5, Encoding.UTF8);


// 6. Quiz/QuizManager (Physics/Chem)
string p6 = "Assets/Scripts/Quiz/QuizManager.cs";
string c6 = File.ReadAllText(p6, Encoding.UTF8);
string m6_find = @"ShowResultPanel\(\);\s*\}";
string m6_rep = @"ShowResultPanel();

            if (GoogleSheetDataManager.Instance != null)
            {
                string userID = GameSession.Instance != null && !string.IsNullOrEmpty(GameSession.Instance.username) ? GameSession.Instance.username : "HS001";
                string monID = "KHTN8"; 
                string baiID = PlayerPrefs.GetString("QuizLesson", "B1_001");
                if (baiID.Contains("Bài 1") || baiID.Contains("Bai 1")) baiID = "B1_001";
                else if (baiID.Contains("Bài 2") || baiID.Contains("Bai 2")) baiID = "B1_002";
                else if (baiID.Contains("Bài 3") || baiID.Contains("Bai 3")) baiID = "B1_003";
                else baiID = "B1_001";
                float accuracy = currentQuestions.Count > 0 ? (float)correctCount / currentQuestions.Count : 0f;
                GoogleSheetDataManager.Instance.StartCoroutine(GoogleSheetDataManager.Instance.SubmitProgressToSheet(userID, monID, baiID, totalScore, accuracy));
            }
        }";
c6 = Regex.Replace(c6, m6_find, m6_rep);
File.WriteAllText(p6, c6, Encoding.UTF8);

