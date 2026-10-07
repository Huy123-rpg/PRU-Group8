#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[InitializeOnLoad]
public static class AutoSyncBuildScenes
{
    static AutoSyncBuildScenes()
    {
        EditorApplication.delayCall += Sync;
    }

    [MenuItem("Tools/Đồng bộ Scene Build Settings")]
    public static void Sync()
    {
        string[] scenePaths = new string[]
        {
            "Assets/Scenes/SampleScene.unity",
            "Assets/Scenes/MainMenu/MainMenu.unity",
            "Assets/Scenes/Physics/ChapterList.unity",
            "Assets/Scenes/Physics/LessonList.unity",
            "Assets/Scenes/CharacterSelection.unity",
            "Assets/Scenes/Physics/Physics.unity",
            "Assets/Scenes/Physics/QuizScene.unity",
            "Assets/Scenes/Physics/QuizScene_TuLuan.unity",
            "Assets/SinhHoc/Scenes/SinhHoc.unity",
            "Assets/Scenes/Scene1_Login.unity",
            "Assets/Scenes/Scene2_MainMenu.unity",
            "Assets/Scenes/Scene3_LessonType.unity",
            "Assets/Scenes/Scene4_ChapterSelect.unity",
            "Assets/Scenes/Scene5_DogSelect.unity",
            "Assets/Scenes/Scene6_Race.unity",
            "Assets/Scenes/Scene7_Results.unity",
            "Assets/Scenes/Sinh/ChapterList 1.unity",
            "Assets/Scenes/Sinh/LessonList 1.unity",
            "Assets/Scenes/Sinh/SinhScene.unity"
        };

        var list = new List<EditorBuildSettingsScene>();
        foreach (var path in scenePaths)
        {
            if (File.Exists(path))
            {
                list.Add(new EditorBuildSettingsScene(path, true));
            }
        }

        EditorBuildSettings.scenes = list.ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log($"<color=green>[AutoSyncBuildScenes] ✅ Đã đồng bộ thành công {list.Count} scenes vào Build Settings!</color>");
    }
}
#endif
