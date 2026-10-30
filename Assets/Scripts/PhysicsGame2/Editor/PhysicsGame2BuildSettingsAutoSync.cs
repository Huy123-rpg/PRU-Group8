#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2.Editor
{
    /// <summary>
    /// Tự động đồng bộ các scene Physics Game 2 vào EditorBuildSettings.scenes
    /// ngay khi Unity Editor khởi động hoặc compile xong scripts.
    /// Giúp SceneManager.LoadScene() luôn hoạt động 100% trong Play Mode mà không bị Unity 6 chặn.
    /// </summary>
    [InitializeOnLoad]
    public static class PhysicsGame2BuildSettingsAutoSync
    {
        private static readonly string[] RequiredScenePaths = new string[]
        {
            "Assets/Scenes/Scene2_MainMenu.unity",
            "Assets/Scenes/Physics/Game2/01_MainMenu.unity",
            "Assets/Scenes/Physics/Game2/02_CharacterSelection.unity",
            "Assets/Scenes/Physics/Game2/03_TeacherSelection.unity",
            "Assets/Scenes/Physics/Game2/04_ChapterSelection.unity",
            "Assets/Scenes/Physics/Game2/05_LessonSelection.unity",
            "Assets/Scenes/Physics/Game2/06_Chương I.unity",
            "Assets/Scenes/Physics/Game2/07_Chương II.unity",
            "Assets/Scenes/Physics/Game2/08_Chương III.unity",
            "Assets/Scenes/Physics/Game2/09_Chương IV 1.unity",
            "Assets/Scenes/Physics/Game2/10_Chương V 2.unity",
            "Assets/Scenes/Physics/Game2/11_Quiz.unity",
            "Assets/Scenes/Physics/Game2/11.1_Quiz 1.unity",
            "Assets/Scenes/Physics/Game2/12_WrittenAnswer.unity",
            "Assets/Scenes/Physics/Game2/13_Inventory.unity",
            "Assets/Scenes/Physics/Game2/14_Settings.unity",
            "Assets/Scenes/Physics/Game2/14.2_Settings 1.unity",
            "Assets/Scenes/Physics/Game2/15.1_Achievements (màn 2).unity",
            "Assets/Scenes/Physics/Game2/15.3_Achievements 1.unity",
            "Assets/Scenes/Physics/Game2/15.4_Achievements.unity",
            "Assets/Scenes/Physics/Game2/15_Achievements 1.unity",
            "Assets/Scenes/Physics/Game2/16. Guide.unity",
            "Assets/Scenes/Physics/Game2/17. Type.unity",
            "Assets/Scenes/Physics/Game2/18. Theory.unity",
            "Assets/Scenes/Physics/Game2/19. Result_TN.unity",
            "Assets/Scenes/Physics/Game2/19.1. Result_TL.unity",
            "Assets/Scenes/Physics/Game2/20. Detail Result TN.unity",
            "Assets/Scenes/Physics/Game2/20.1 Detail Result TL.unity",
            "Assets/Scenes/Physics/Game2/22. Buoi hoc hom nay.unity",
            "Assets/Scenes/Physics/Game2/23. Bảng xếp hạng.unity",
            "Assets/Scenes/Physics/Game2/24. Tiến bộ học tập.unity",
            "Assets/Scenes/Physics/Game2/25. Quản lý học tập của giáo viên.unity",
            "Assets/Scenes/Physics/Game2/27. Chương I after.unity",
            "Assets/Scenes/Physics/Game2/28. Chương II after 1.unity",
            "Assets/Scenes/Physics/Game2/29. Chương III after.unity",
            "Assets/Scenes/Physics/Game2/30. Chương IV after 1.unity",
            "Assets/Scenes/Physics/Game2/31. Chương V after 2.unity",
            "Assets/Scenes/Physics/Game2/32. Result_Tự luận.unity",
            "Assets/Scenes/Physics/Game2/33. Chọn độ khó.unity",
            "Assets/Scenes/Physics/Game2/34. Đối kháng.unity",
            "Assets/Scenes/Physics/Game2/34.1. Đối kháng 1.unity",
            "Assets/Scenes/Physics/Game2/34.2. Đối kháng 2.unity"
        };

        static PhysicsGame2BuildSettingsAutoSync()
        {
            SyncScenes();
            EditorApplication.delayCall += SyncScenes;
        }

        [MenuItem("Tools/Physics Game 2/Sync Build Settings Scenes")]
        public static void SyncScenes()
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool modified = false;

            // Đảm bảo toàn bộ scene hiện có có mặt và luôn bật enabled = true
            foreach (string path in RequiredScenePaths)
            {
                int existingIndex = list.FindIndex(s => s.path == path);
                if (existingIndex < 0)
                {
                    list.Add(new EditorBuildSettingsScene(path, true));
                    modified = true;
                    Debug.Log($"[PhysicsGame2BuildSettingsAutoSync] Đã thêm scene vào Build Settings: {path}");
                }
                else if (!list[existingIndex].enabled)
                {
                    list[existingIndex].enabled = true;
                    modified = true;
                    Debug.Log($"[PhysicsGame2BuildSettingsAutoSync] Đã bật lại scene trong Build Settings: {path}");
                }
            }

            if (modified)
            {
                EditorBuildSettings.scenes = list.ToArray();
                Debug.Log("[PhysicsGame2BuildSettingsAutoSync] Đã đồng bộ các scene Game 2 vào EditorBuildSettings.");
            }
        }
    }
}
#endif
