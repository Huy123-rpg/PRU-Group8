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
