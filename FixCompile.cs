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

        // Find the start of SetupBackButton
        int startIndex = content.IndexOf("private void SetupBackButton()");
        if (startIndex == -1) return;

        // Find the end of SetupBackButton (the #endregion for Sinh Scene is right after it, wait no, Sinh Scene region is after)
        int endIndex = content.IndexOf("#endregion", startIndex);
        
        string newMethod = @"private void SetupBackButton()
        {
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            bool hasBack = false;
            foreach (Button btn in allButtons)
            {
                if (popupChonCheDo != null && btn.transform.IsChildOf(popupChonCheDo.transform))
                    continue;

                string btnName = btn.gameObject.name.ToLower();
                TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
                string text = tmp != null ? tmp.text.ToLower() : """";

                if (btnName == ""btn_back"" || btnName == ""image_back"" || text.Contains(""quay lại"") || text.Contains(""back""))
                {
                    if (tmp != null)
                    {
                        tmp.enableAutoSizing = false;
                        tmp.fontSize = 32f;
                        tmp.fontStyle = FontStyles.Bold;
                        tmp.color = new Color(0.24f, 0.14f, 0.08f, 1f);
                    }

                    btn.interactable = true;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log(""[BiologyMenuManager] ← Quay về ChapterList 1"");
                        if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack(); else SceneManager.LoadScene(""ChapterList 1"");
                    });
                    Debug.Log($""[BiologyMenuManager] ✅ Đã gắn sự kiện nút Back chính vào {btn.gameObject.name}"");
                    hasBack = true;
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
        }

        ";
        
        content = content.Substring(0, startIndex) + newMethod + content.Substring(endIndex);
        
        File.WriteAllText(path, content, new UTF8Encoding(false));
    }
}
