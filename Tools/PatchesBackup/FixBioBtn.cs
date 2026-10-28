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

        // 1. Add Auto-generate logic to SetupChapterListScene
        string chapterRegex = @"Debug\.Log\(\""\[BiologyMenuManager\] ✅ Đã cấu hình xong toàn bộ Chủ đề Sinh học trong ChapterList 1\""\);";
        string chapterReplacement = @"
            // Tự động sinh nút Back nếu trong Scene không có
            bool hasBackButton = false;
            foreach (Button b in allButtons) { if (b.gameObject.name.ToLower().Contains(""back"")) { hasBackButton = true; break; } }
            if (!hasBackButton)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject btnObj = new GameObject(""Btn_Back_Auto"");
                    btnObj.transform.SetParent(canvas.transform, false);
                    btnObj.transform.SetAsLastSibling();
                    RectTransform rt = btnObj.AddComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0f, 1f); rt.anchoredPosition = new Vector2(30f, -30f);
                    rt.sizeDelta = new Vector2(180f, 60f);
                    UnityEngine.UI.Image img = btnObj.AddComponent<UnityEngine.UI.Image>();
                    img.color = new Color(0.9f, 0.8f, 0.6f, 1f);
                    Button btn = btnObj.AddComponent<Button>();
                    btn.onClick.AddListener(() => {
                        if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack(); else { string returnScene = Application.CanStreamedLevelBeLoaded(""SampleScene"") ? ""SampleScene"" : ""MainMenu""; SceneManager.LoadScene(returnScene); }
                    });
                    GameObject textObj = new GameObject(""Text"");
                    textObj.transform.SetParent(btnObj.transform, false);
                    RectTransform textRt = textObj.AddComponent<RectTransform>();
                    textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one;
                    textRt.sizeDelta = Vector2.zero;
                    TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
                    txt.text = ""← Quay lại""; txt.fontSize = 28f; txt.fontStyle = FontStyles.Bold; txt.color = new Color(0.24f, 0.14f, 0.08f, 1f);
                    txt.alignment = TextAlignmentOptions.Center;
                }
            }
            Debug.Log(""[BiologyMenuManager] ✅ Đã cấu hình xong toàn bộ Chủ đề Sinh học trong ChapterList 1"");";
        
        content = Regex.Replace(content, chapterRegex, chapterReplacement);

        // 2. Add Auto-generate logic to SetupBackButton (for LessonList 1)
        string lessonRegex = @"private void SetupBackButton\(\)\s*\{\s*Button\[\] allButtons = FindObjectsByType<Button>\(FindObjectsSortMode\.None\);\s*bool hasBack = false;\s*foreach";
        // Wait, the original starts with:
        string lessonRegexOrig = @"private void SetupBackButton\(\)\s*\{\s*Button\[\] allButtons = FindObjectsByType<Button>\(FindObjectsSortMode\.None\);\s*foreach";
        
        string lessonReplacement = @"private void SetupBackButton()
        {
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            bool hasBack = false;
            foreach";
            
        content = Regex.Replace(content, lessonRegexOrig, lessonReplacement);

        string lessonEndRegex = @"Debug\.Log\(\$""\[BiologyMenuManager\] ✅ Đã gắn sự kiện nút Back chính vào \{btn\.gameObject\.name\}""\);\s*\}\s*\}\s*\}";
        string lessonEndReplacement = @"Debug.Log($""[BiologyMenuManager] ✅ Đã gắn sự kiện nút Back chính vào {btn.gameObject.name}"");
                        hasBack = true;
                    }
                }
            }

            if (!hasBack)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject btnObj = new GameObject(""Btn_Back_Auto"");
                    btnObj.transform.SetParent(canvas.transform, false);
                    btnObj.transform.SetAsLastSibling();
                    RectTransform rt = btnObj.AddComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0f, 1f); rt.anchoredPosition = new Vector2(30f, -30f);
                    rt.sizeDelta = new Vector2(180f, 60f);
                    UnityEngine.UI.Image img = btnObj.AddComponent<UnityEngine.UI.Image>();
                    img.color = new Color(0.9f, 0.8f, 0.6f, 1f);
                    Button btn = btnObj.AddComponent<Button>();
                    btn.onClick.AddListener(() => {
                        if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack(); else SceneManager.LoadScene(""ChapterList 1"");
                    });
                    GameObject textObj = new GameObject(""Text"");
                    textObj.transform.SetParent(btnObj.transform, false);
                    RectTransform textRt = textObj.AddComponent<RectTransform>();
                    textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one;
                    textRt.sizeDelta = Vector2.zero;
                    TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
                    txt.text = ""← Quay lại""; txt.fontSize = 28f; txt.fontStyle = FontStyles.Bold; txt.color = new Color(0.24f, 0.14f, 0.08f, 1f);
                    txt.alignment = TextAlignmentOptions.Center;
                }
            }
        }";
        
        content = Regex.Replace(content, lessonEndRegex, lessonEndReplacement);

        File.WriteAllText(path, content, new UTF8Encoding(false));
    }
}
