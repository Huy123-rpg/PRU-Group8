using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = "Assets/Scripts/Quiz/QuizManager.cs";
        string content = File.ReadAllText(path, Encoding.UTF8);

        string orig = @"string monID = ""KHTN8""; 
                string baiID = PlayerPrefs.GetString(""QuizLesson"", ""B1_001"");
                if (baiID.Contains(""Bài 1"") || baiID.Contains(""Bai 1"")) baiID = ""B1_001"";
                else if (baiID.Contains(""Bài 2"") || baiID.Contains(""Bai 2"")) baiID = ""B1_002"";
                else if (baiID.Contains(""Bài 3"") || baiID.Contains(""Bai 3"")) baiID = ""B1_003"";
                else baiID = ""B1_001"";";
        
        string rep = @"string monID = ""KHTN8"";
                
                string chapName = ScienceQuest.UI.PhysicsMenuManager.SelectedChapterName.ToLower();
                string lessonName = ScienceQuest.UI.PhysicsMenuManager.SelectedLessonName.ToLower();

                int chapIndex = 1;
                if (chapName.Contains(""ii"") && !chapName.Contains(""iii"") && !chapName.Contains(""iv"")) chapIndex = 2;
                else if (chapName.Contains(""iii"")) chapIndex = 3;
                else if (chapName.Contains(""iv"")) chapIndex = 4;
                else if (chapName.Contains(""v"") && !chapName.Contains(""iv"")) chapIndex = 5;

                int lessonIndex = 1;
                System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(lessonName, @""b[aà]i\s*(\d+)"");
                if (m.Success) int.TryParse(m.Groups[1].Value, out lessonIndex);

                string baiID = $""B{chapIndex}_00{lessonIndex}"";

                if (string.IsNullOrEmpty(ScienceQuest.UI.PhysicsMenuManager.SelectedLessonName))
                {
                    string raw = PlayerPrefs.GetString(""QuizLesson"", ""B1_001"");
                    if (raw.StartsWith(""B"") && raw.Contains(""_"")) baiID = raw;
                    else baiID = ""B1_001"";
                }";
                
        content = content.Replace(orig, rep);
        File.WriteAllText(path, content, new UTF8Encoding(false));
    }
}
