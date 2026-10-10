using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = "Assets/Scripts/QuizManager.cs";
        string content = File.ReadAllText(path, Encoding.UTF8);

        // 1. TimeScale Safety
        string timeScaleOrig = @"// Hiển thị Popup Hết Máu có 2 nút: Chơi lại & Thoát
            ShowGameOverPopup();
        }
    }";
        string timeScaleRep = @"// Hiển thị Popup Hết Máu có 2 nút: Chơi lại & Thoát
            ShowGameOverPopup();
            
            if (Time.timeScale == 0f)
            {
                StartCoroutine(SafetyTimeScaleReset());
            }
        }
    }

    private System.Collections.IEnumerator SafetyTimeScaleReset()
    {
        yield return new UnityEngine.WaitForSecondsRealtime(10f);
        if (Time.timeScale == 0f)
        {
            UnityEngine.Debug.LogWarning(""[QuizManager] Safety: timeScale van = 0 sau 10s, reset ve 1"");
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }";
        content = content.Replace(timeScaleOrig, timeScaleRep);

        // 2. Accuracy Fix
        string accOrig = @"string sheetLesson = PlayerPrefs.GetString(""QuizLesson"", ""B1_001"");
            StartCoroutine(GoogleSheetDataManager.Instance.SubmitProgressToSheet(userID, ""KHTN6"", sheetLesson, currentScore, 100f));";
        string accRep = @"string sheetLesson = PlayerPrefs.GetString(""Sinh_QuizLesson"", PlayerPrefs.GetString(""QuizLesson"", ""B1_001""));
            if (!sheetLesson.StartsWith(""B"") || !sheetLesson.Contains(""_""))
            {
                System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(sheetLesson, @""[Bb][aà]i\s*(\d+)"");
                int lessonIdx = 1;
                if (m.Success) int.TryParse(m.Groups[1].Value, out lessonIdx);
                sheetLesson = $""B1_00{lessonIdx}"";
            }
            int totalQ = questionList != null ? questionList.Count : 0;
            int correctQ = PlayerPrefs.GetInt(""Sinh_CorrectCount"", 0);
            float accuracy = totalQ > 0 ? (float)correctQ / totalQ * 100f : 100f;
            StartCoroutine(GoogleSheetDataManager.Instance.SubmitProgressToSheet(userID, ""KHTN6"", sheetLesson, currentScore, accuracy));";
        content = content.Replace(accOrig, accRep);

        // 3. Exit button
        content = content.Replace(
            "Time.timeScale = 1f;\r\n            SceneManager.LoadScene(\"LessonList 1\");",
            "Time.timeScale = 1f;\r\n            if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack(); else SceneManager.LoadScene(\"LessonList 1\");"
        );
        content = content.Replace(
            "Time.timeScale = 1f;\n            SceneManager.LoadScene(\"LessonList 1\");",
            "Time.timeScale = 1f;\n            if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack(); else SceneManager.LoadScene(\"LessonList 1\");"
        );

        File.WriteAllText(path, content, new UTF8Encoding(false));
    }
}
