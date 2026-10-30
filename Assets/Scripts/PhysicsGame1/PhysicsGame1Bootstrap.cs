using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScienceQuest.PhysicsGame1
{
    /// <summary>
    /// Tự động gắn Controller tương ứng vào Scene khi scene được nạp.
    /// Giúp toàn bộ các màn chơi của Game 1 hoạt động trơn tru 100% mà không cần phải chỉnh sửa thủ công các file .unity
    /// </summary>
    public static class PhysicsGame1Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneLoadCallback()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            string sName = scene.name;

            switch (sName)
            {
                case "GameSelection":
                    EnsureController<Physics_GameSelectionController>(sName);
                    break;
                case "01_MainMenu":
                    EnsureController<PhysicsGame1_MainMenuController>(sName);
                    break;
                case "02_CharacterSelection":
                    EnsureController<PhysicsGame1_CharacterSelectController>(sName);
                    break;
                case "03_LevelSelection":
                    EnsureController<PhysicsGame1_LevelSelectController>(sName);
                    break;
                case "04_ChapterSelection":
                    EnsureController<PhysicsGame1_ChapterSelectController>(sName);
                    break;
                case "05_LessonSelection":
                    EnsureController<PhysicsGame1_LessonSelectController>(sName);
                    break;
                case "06_GameModeSelect":
                    EnsureController<PhysicsGame1_GameModeController>(sName);
                    break;
                case "07_DuckShooting":
                    EnsureController<PhysicsGame1_DuckShootingController>(sName);
                    break;
                case "08_ResultScene":
                    EnsureController<PhysicsGame1_ResultController>(sName);
                    break;
                case "09_AnswerReview":
                    EnsureController<PhysicsGame1_AnswerReviewController>(sName);
                    break;
                case "10_Settings":
                    EnsureController<PhysicsGame1_SettingsController>(sName);
                    break;
                case "11_Achievements":
                    EnsureController<PhysicsGame1_AchievementsController>(sName);
                    break;
                case "12_Theory":
                    EnsureController<PhysicsGame1_TheoryController>(sName);
                    break;
                case "13_VersusSelection":
                    EnsureController<PhysicsGame1_VersusSelectController>(sName);
                    break;
                case "14_VersusGame":
                    EnsureController<PhysicsGame1_VersusGameController>(sName);
                    break;
                case "15_VersusResult":
                    EnsureController<PhysicsGame1_VersusResultController>(sName);
                    break;
                case "16_DashboardTeacher":
                    EnsureController<PhysicsGame1_TeacherDashboardController>(sName);
                    break;
            }
        }

        private static void EnsureController<T>(string sceneName) where T : MonoBehaviour
        {
            if (UnityEngine.Object.FindFirstObjectByType<T>() == null)
            {
                GameObject host = new GameObject($"[AutoHost]_{typeof(T).Name}");
                host.AddComponent<T>();
                Debug.Log($"[PhysicsGame1Bootstrap] 🚀 Đã tự động kích hoạt {typeof(T).Name} cho scene {sceneName}");
            }
        }
    }
}
