using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = "Assets/Scripts/UI/BiologyMenuManager.cs";
        string content = File.ReadAllText(path, Encoding.UTF8);

        // 1. ChapterList 1 -> Menu
        content = content.Replace(
            "string returnScene = Application.CanStreamedLevelBeLoaded(\"SampleScene\") ? \"SampleScene\" : \"MainMenu\";\n                        SceneManager.LoadScene(returnScene);",
            "if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack(); else { string returnScene = Application.CanStreamedLevelBeLoaded(\"SampleScene\") ? \"SampleScene\" : \"MainMenu\"; SceneManager.LoadScene(returnScene); }"
        );
        content = content.Replace(
            "string returnScene = Application.CanStreamedLevelBeLoaded(\"SampleScene\") ? \"SampleScene\" : \"MainMenu\";\r\n                        SceneManager.LoadScene(returnScene);",
            "if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack(); else { string returnScene = Application.CanStreamedLevelBeLoaded(\"SampleScene\") ? \"SampleScene\" : \"MainMenu\"; SceneManager.LoadScene(returnScene); }"
        );

        // 2. LessonList 1 -> ChapterList 1
        content = content.Replace(
            "SceneManager.LoadScene(\"ChapterList 1\");",
            "if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack(); else SceneManager.LoadScene(\"ChapterList 1\");"
        );

        // 3. SinhScene -> LessonList 1
        content = content.Replace(
            "Time.timeScale = 1f;\n                    SceneManager.LoadScene(\"LessonList 1\");",
            "Time.timeScale = 1f;\n                    if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack(); else SceneManager.LoadScene(\"LessonList 1\");"
        );
        content = content.Replace(
            "Time.timeScale = 1f;\r\n                    SceneManager.LoadScene(\"LessonList 1\");",
            "Time.timeScale = 1f;\r\n                    if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack(); else SceneManager.LoadScene(\"LessonList 1\");"
        );

        File.WriteAllText(path, content, new UTF8Encoding(false));
    }
}
