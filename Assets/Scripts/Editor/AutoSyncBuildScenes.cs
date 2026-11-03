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
            "Assets/Scenes/Scene1_Login.unity",
            "Assets/Scenes/Scene2_MainMenu.unity",
            "Assets/Scenes/Scene3_LessonType.unity",
            "Assets/Scenes/Scene4_ChapterSelect.unity",
            "Assets/Scenes/Scene5_DogSelect.unity",
            "Assets/Scenes/Scene6_Race.unity",
            "Assets/Scenes/Scene7_Results.unity",
            "Assets/Scenes/SampleScene.unity",
            "Assets/Scenes/MainMenu/MainMenu.unity",
            "Assets/Scenes/Academy/Academy.unity",
            "Assets/Scenes/Chemistry/Chemistry.unity",
            "Assets/Scenes/Biology/Biology.unity",
            "Assets/Scenes/CharacterSelection.unity",
            "Assets/Scenes/Sinh/ChapterList 1.unity",
            "Assets/Scenes/Sinh/LessonList 1.unity",
            "Assets/Scenes/Sinh/SinhScene.unity",
            "Assets/SinhHoc/Scenes/SinhHoc.unity",
            "Assets/Scenes/Physics/GameSelection.unity",
            "Assets/Scenes/Physics/Game1/01_MainMenu.unity",
            "Assets/Scenes/Physics/Game1/02_CharacterSelection.unity",
            "Assets/Scenes/Physics/Game1/03_LevelSelection.unity",
            "Assets/Scenes/Physics/Game1/04_ChapterSelection.unity",
            "Assets/Scenes/Physics/Game1/05_LessonSelection.unity",
            "Assets/Scenes/Physics/Game1/06_GameModeSelect.unity",
            "Assets/Scenes/Physics/Game1/07_DuckShooting.unity",
            "Assets/Scenes/Physics/Game1/08_ResultScene.unity",
            "Assets/Scenes/Physics/Game1/09_AnswerReview.unity",
            "Assets/Scenes/Physics/Game1/10_Settings.unity",
            "Assets/Scenes/Physics/Game1/11_Achievements.unity",
            "Assets/Scenes/Physics/Game1/12_Theory.unity",
            "Assets/Scenes/Physics/Game1/13_VersusSelection.unity",
            "Assets/Scenes/Physics/Game1/14_VersusGame.unity",
            "Assets/Scenes/Physics/Game1/15_VersusResult.unity",
            "Assets/Scenes/Physics/Game1/16_DashboardTeacher.unity",
            "Assets/Scenes/Physics/Game2/01_MainMenu.unity",
            "Assets/Scenes/Physics/Game2/02_CharacterSelection.unity",
            "Assets/Scenes/Physics/Game2/03_TeacherSelection.unity",
            "Assets/Scenes/Physics/Game2/04_ChapterSelection.unity",
            "Assets/Scenes/Physics/Game2/05_LessonSelection.unity",
            "Assets/Scenes/Physics/Game2/11_Quiz.unity",
            "Assets/Scenes/Physics/Game2/12_WrittenAnswer.unity",
            "Assets/Scenes/Physics/Game2/13_Inventory.unity",
            "Assets/Scenes/Physics/Game2/14_Settings.unity",
            "Assets/Scenes/Physics/Game2/15.4_Achievements.unity",
            "Assets/Scenes/Physics/Game2/16. Guide.unity",
            "Assets/Scenes/Physics/Game2/17. Type.unity",
            "Assets/Scenes/Physics/Game2/18. Theory.unity",
            "Assets/Scenes/Physics/Game2/19. Result_TN.unity",
            "Assets/Scenes/Physics/Game2/20. Detail Result TN.unity",
            "Assets/Scenes/Physics/Game2/22. Buoi hoc hom nay.unity",
            "Assets/Scenes/Physics/Game2/11.1_Quiz 1.unity",
            "Assets/Scenes/Physics/Game2/14.2_Settings 1.unity",
            "Assets/Scenes/Physics/Game2/15.3_Achievements 1.unity",
            "Assets/Scenes/Physics/Game2/15_Achievements 1.unity",
            "Assets/Scenes/Physics/Game2/19.1. Result_TL.unity",
            "Assets/Scenes/Physics/Game2/20.1 Detail Result TL.unity",
        };

        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool modified = false;

        foreach (var path in scenePaths)
        {
            if (File.Exists(path))
            {
                int idx = list.FindIndex(s => s.path == path);
                if (idx < 0)
                {
                    list.Add(new EditorBuildSettingsScene(path, true));
                    modified = true;
                }
                else if (!list[idx].enabled)
                {
                    list[idx].enabled = true;
                    modified = true;
                }
            }
        }

        if (modified)
        {
            EditorBuildSettings.scenes = list.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log($"<color=green>[AutoSyncBuildScenes] ✅ Đã đồng bộ thành công {list.Count} scenes vào Build Settings!</color>");
        }
    }
}
#endif
