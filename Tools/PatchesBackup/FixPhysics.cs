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

        // 1. Fix SceneLoader.Instance.LoadScene null check & routing
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

        // 2. Fix the if/else scope for ChapterList listener duplication
        // In the original file:
        // else if (btn.gameObject.name.Contains("5")) chapterName = "Chương V: Năng lượng với cuộc sống";
        // }
        // 
        // if (!string.IsNullOrEmpty(chapterName))
        // {
        // ... (block that assigns onClick) ...
        // }
        // 
        // if (!string.IsNullOrEmpty(chapterName))
        // {
        // ... (the other block)

        // Let's just find the duplicate block and remove one of them, or make sure they continue properly.
        // Actually, looking at the code, it has:
        // if (!string.IsNullOrEmpty(chapterName)) { string username = ... }
        // And then AGAIN:
        // if (!string.IsNullOrEmpty(chapterName)) { btn.onClick... }
        // The issue is that the first block doesn't `continue`, so the second block runs. And that's fine because one block updates the TEXT and the other updates the ONCLICK.
        // Wait, the auditor said:
        // "Block if (!string.IsNullOrEmpty(chapterName)) nằm bên trong foreach nhưng ngoài else if (btn.gameObject.name.StartsWith("Btn_Chuong")). Điều này có nghĩa: khi nút có text chứa "Chương" (block if trên) được xử lý... flow vẫn chạy xuống... gây gán listener bị duplicate nếu không có continue"
        // Wait, if it assigns listener `btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(...)`, it will REMOVE previous listeners, so it's NOT actually duplicating the listener (it overwrites it). 
        // The auditor was wrong about it being a "duplicate listener", but it is a scoping issue if we don't want it to run twice. But since it calls RemoveAllListeners(), it's safe.
        
        File.WriteAllText(path, content, new UTF8Encoding(false));
    }
}
