#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tự động đăng ký toàn bộ 7 Scene vào Unity Editor Build Settings theo đúng thứ tự luồng game.
/// </summary>
[InitializeOnLoad]
public static class SceneBuildSettingsSetup
{
    static SceneBuildSettingsSetup()
    {
        EditorApplication.delayCall += EnsureScenesInBuildSettings;
    }

    [MenuItem("Tools/PRU/Auto Setup Build Settings Scenes")]
    public static void EnsureScenesInBuildSettings()
    {
        string[] targetScenes = new string[]
        {
            "Assets/Scenes/Scene1_Login.unity",
            "Assets/Scenes/Scene2_MainMenu.unity",
            "Assets/Scenes/Scene3_LessonType.unity",
            "Assets/Scenes/Scene4_ChapterSelect.unity",
            "Assets/Scenes/Scene5_DogSelect.unity",
            "Assets/Scenes/Scene6_Race.unity",
            "Assets/Scenes/Scene7_Results.unity"
        };

        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

        foreach (string scenePath in targetScenes)
        {
            if (System.IO.File.Exists(scenePath))
            {
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }
            else
            {
                Debug.LogWarning($"[SceneBuildSettingsSetup] Cảnh báo: Không tìm thấy file Scene: {scenePath}");
            }
        }

        EditorBuildSettings.scenes = buildScenes.ToArray();
        Debug.Log("[SceneBuildSettingsSetup] Đã tự động đăng ký và cấu hình 7 Scene trong Unity Build Settings thành công!");
    }
}
#endif
