using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = "Assets/Scripts/UI/PhysicsMenuManager.cs";
        string content = File.ReadAllText(path, Encoding.UTF8);

        // 1. Fix IsLoggedIn
        content = content.Replace("LoginManager.IsLoggedIn = false;", "// LoginManager.IsLoggedIn = false;");

        // 2. Fix Chapter UI
        string regex = @"if \(btn\.gameObject\.name\.Contains\(""5""\)\) chapterName = ""Chương V: Năng lượng với cuộc sống"";\s*\}";
        string replacement = @"if (btn.gameObject.name.Contains(""5"")) chapterName = ""Chương V: Năng lượng với cuộc sống"";
                    }

                    if (!string.IsNullOrEmpty(chapterName))
                    {
                        string username = GameSession.Instance != null && !string.IsNullOrEmpty(GameSession.Instance.username) ? GameSession.Instance.username : ""HS001"";
                        string monID = ""KHTN8"";
                        int chapIndex = 1;
                        if (chapterName.Contains(""II"")) chapIndex = 2;
                        if (chapterName.Contains(""III"")) chapIndex = 3;
                        if (chapterName.Contains(""IV"")) chapIndex = 4;
                        if (chapterName.Contains(""V"") && !chapterName.Contains(""IV"")) chapIndex = 5;

                        int totalAttempts = 0;
                        int maxScore = 0;
                        if (ProgressSyncManager.Instance != null)
                        {
                            for (int i = 1; i <= 9; i++)
                            {
                                var p = ProgressSyncManager.Instance.GetProgress(username, monID, string.Format(""B{0}_00{1}"", chapIndex, i));
                                if (p != null)
                                {
                                    totalAttempts += p.soLanLam;
                                    if (p.diemCaoNhat > maxScore) maxScore = p.diemCaoNhat;
                                }
                            }
                        }
                        
                        tmp.text = chapterName;
                        if (totalAttempts > 0 || maxScore > 0)
                        {
                            tmp.text += string.Format(""\n<size=16><color=#FFD700>Điểm cao nhất: {0} | Tổng lượt làm: {1}</color></size>"", maxScore, totalAttempts);
                        }
                    }";
        
        content = Regex.Replace(content, regex, replacement);
        File.WriteAllText(path, content, new UTF8Encoding(false));
    }
}
