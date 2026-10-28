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
            // --- Cụm 7 Scene Luồng Đua Chó & Hóa Học ---
            "Assets/Scenes/Scene1_Login.unity",
            "Assets/Scenes/Scene2_MainMenu.unity",
            "Assets/Scenes/Scene3_LessonType.unity",
            "Assets/Scenes/Scene4_ChapterSelect.unity",
            "Assets/Scenes/Scene5_DogSelect.unity",
            "Assets/Scenes/Scene6_Race.unity",
            "Assets/Scenes/Scene7_Results.unity",

            // --- Menu & Học Viện ---
            "Assets/Scenes/SampleScene.unity",
            "Assets/Scenes/MainMenu/MainMenu.unity",
            "Assets/Scenes/Academy/Academy.unity",
            "Assets/Scenes/Chemistry/Chemistry.unity",
            "Assets/Scenes/Biology/Biology.unity",
            "Assets/Scenes/CharacterSelection.unity",

            // --- Cụm Sinh Học ---
            "Assets/Scenes/Sinh/ChapterList 1.unity",
            "Assets/Scenes/Sinh/LessonList 1.unity",
            "Assets/Scenes/Sinh/SinhScene.unity",
            "Assets/SinhHoc/Scenes/SinhHoc.unity",

            // --- Cụm Vật Lý Game 1 (Bắn Vịt & Đấu Boss) ---
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

            // --- Cụm Vật Lý Game 2 (Phiêu Lưu Kiến Thức) ---
            "Assets/Scenes/Physics/Game2/01_MainMenu.unity",
            "Assets/Scenes/Physics/Game2/02_CharacterSelection.unity",
            "Assets/Scenes/Physics/Game2/03_TeacherSelection.unity",
            "Assets/Scenes/Physics/Game2/04_ChapterSelection.unity",
            "Assets/Scenes/Physics/Game2/05_LessonSelection.unity",
            "Assets/Scenes/Physics/Game2/06_Gameplay_Mechanics.unity",
            "Assets/Scenes/Physics/Game2/07_Gameplay_Optics.unity",
            "Assets/Scenes/Physics/Game2/08_Gameplay_Electricity.unity",
            "Assets/Scenes/Physics/Game2/09_Gameplay_Electromagnetism.unity",
            "Assets/Scenes/Physics/Game2/10_Gameplay_Energy.unity",
            "Assets/Scenes/Physics/Game2/11_Quiz.unity",
            "Assets/Scenes/Physics/Game2/12_WrittenAnswer.unity",
            "Assets/Scenes/Physics/Game2/13_Inventory.unity",
            "Assets/Scenes/Physics/Game2/14_Settings.unity",
            "Assets/Scenes/Physics/Game2/15_Achievements.unity"
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
