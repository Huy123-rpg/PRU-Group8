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

        content = content.Replace(
            "if (SceneLoader.Instance != null)\r\n                            SceneLoader.Instance.LoadScene(\"SampleScene\");\r\n                        else\r\n                            SceneManager.LoadScene(\"SampleScene\");",
            "if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack();\r\n                        else if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(\"Scene3_LessonType\");\r\n                        else SceneManager.LoadScene(\"Scene3_LessonType\");"
        );
        content = content.Replace(
            "if (SceneLoader.Instance != null)\n                            SceneLoader.Instance.LoadScene(\"SampleScene\");\n                        else\n                            SceneManager.LoadScene(\"SampleScene\");",
            "if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack();\n                        else if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(\"Scene3_LessonType\");\n                        else SceneManager.LoadScene(\"Scene3_LessonType\");"
        );

        content = content.Replace(
            "SceneLoader.Instance.LoadScene(\"ChapterList\");",
            "if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack(); else if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(\"ChapterList\"); else SceneManager.LoadScene(\"ChapterList\");"
        );

        File.WriteAllText(path, content, new UTF8Encoding(false));
    }
}
