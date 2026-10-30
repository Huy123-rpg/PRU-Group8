#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class SetupMenuUI : EditorWindow
{
    [MenuItem("Tools/Tạo Giao Diện Chọn Chương-Bài")]
    public static void CreateMenuUI()
    {
        MainMenuManager manager = Object.FindAnyObjectByType<MainMenuManager>();
        if (manager == null)
        {
            Debug.LogError("Không tìm thấy MainMenuManager trong scene! Hãy mở Scene Menu trước.");
            return;
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không có Canvas trong Scene!");
            return;
        }

        // Tạo 3 Panel
        GameObject chapterPanel = CreatePanel("ChapterPanel", canvas.transform);
        GameObject lessonPanel = CreatePanel("LessonPanel", canvas.transform);
        GameObject modePanel = CreatePanel("ModePanel", canvas.transform);

        // Tạo Title
        CreateText("Title", chapterPanel.transform, "CHỌN CHƯƠNG", new Vector2(0, 300));
        CreateText("Title", lessonPanel.transform, "CHỌN BÀI", new Vector2(0, 300));
        CreateText("Title", modePanel.transform, "CHỌN PHƯƠNG THỨC", new Vector2(0, 300));

        // Tạo Container (Grid Layout)
        GameObject chapterContainer = CreateContainer("ChapterContainer", chapterPanel.transform);
        GameObject lessonContainer = CreateContainer("LessonContainer", lessonPanel.transform);

        // Nút Back
        CreateButton("BtnBack", chapterPanel.transform, "Quay Lại", new Vector2(-400, 300), "OnBackToSubjectClicked", manager);
        CreateButton("BtnBack", lessonPanel.transform, "Quay Lại", new Vector2(-400, 300), "OnBackToChapterClicked", manager);
        CreateButton("BtnBack", modePanel.transform, "Quay Lại", new Vector2(-400, 300), "OnBackToLessonClicked", manager);

        // Nút Mode
        CreateButton("BtnMultipleChoice", modePanel.transform, "TRẮC NGHIỆM", new Vector2(-200, 0), "SelectMode", manager, "MultipleChoice", new Vector2(300, 100));
        CreateButton("BtnEssay", modePanel.transform, "TỰ LUẬN", new Vector2(200, 0), "SelectMode", manager, "EssayImage", new Vector2(300, 100));

        // Prefab Button
        GameObject prefabBase = CreateButton("PrefabBtn", null, "Button", Vector2.zero, "", null);
        prefabBase.SetActive(false);

        // Gán vào Manager
        manager.chapterPanel = chapterPanel;
        manager.lessonPanel = lessonPanel;
        manager.modePanel = modePanel;
        manager.chapterContainer = chapterContainer.transform;
        manager.lessonContainer = lessonContainer.transform;
        manager.buttonPrefab = prefabBase;

        // Ẩn hết
        chapterPanel.SetActive(false);
        lessonPanel.SetActive(false);
        modePanel.SetActive(false);

        EditorUtility.SetDirty(manager);
        Debug.Log("Tạo giao diện thành công! Bạn có thể Play và bấm vào Môn Sinh Học.");
    }

    private static GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.9f);
        return panel;
    }

    private static void CreateText(string name, Transform parent, string text, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI txt = go.AddComponent<TextMeshProUGUI>();
        txt.text = text;
        txt.fontSize = 60;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(800, 100);
    }

    private static GameObject CreateContainer(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(800, 600);
        
        GridLayoutGroup grid = go.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(350, 100);
        grid.spacing = new Vector2(20, 20);
        grid.childAlignment = TextAnchor.UpperCenter;
        return go;
    }

    private static GameObject CreateButton(string name, Transform parent, string text, Vector2 pos, string methodName, MainMenuManager manager, string stringParam = "", Vector2 size = default)
    {
        GameObject go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        if (size != default) rt.sizeDelta = size; else rt.sizeDelta = new Vector2(200, 60);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 1f);

        Button btn = go.AddComponent<Button>();

        GameObject txtGo = new GameObject("Text");
        txtGo.transform.SetParent(go.transform, false);
        TextMeshProUGUI txt = txtGo.AddComponent<TextMeshProUGUI>();
        txt.text = text;
        txt.color = Color.white;
        txt.alignment = TextAlignmentOptions.Center;
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero; txtRt.offsetMax = Vector2.zero;

        if (manager != null && !string.IsNullOrEmpty(methodName))
        {
            if (stringParam == "")
            {
                var action = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), manager, methodName) as UnityEngine.Events.UnityAction;
                if (action != null) UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, action);
            }
            else
            {
                var action = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction<string>), manager, methodName) as UnityEngine.Events.UnityAction<string>;
                if (action != null) UnityEditor.Events.UnityEventTools.AddStringPersistentListener(btn.onClick, action, stringParam);
            }
        }

        return go;
    }
}
#endif
