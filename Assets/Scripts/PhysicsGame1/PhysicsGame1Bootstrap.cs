using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScienceQuest.PhysicsGame1
{
    /// <summary>
    /// Tự động gắn Controller tương ứng vào Scene Physics Game 1 khi scene được load,
    /// bất kể scene được load qua UI flow, SceneLoader hay mở trực tiếp trong Unity Editor.
    /// Đồng thời quét sạch triệt để các UI rác từ DontDestroyOnLoad hay các scene khác.
    /// </summary>
    public static class PhysicsGame1Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneLoadCallback()
        {
            SanitizeSelectableArray();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public static void SanitizeSelectableArray()
        {
            try
            {
                var countField = typeof(UnityEngine.UI.Selectable).GetField("s_SelectableCount", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                var arrayField = typeof(UnityEngine.UI.Selectable).GetField("s_Selectables", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (countField != null && arrayField != null)
                {
                    int count = (int)countField.GetValue(null);
                    var arr = (UnityEngine.UI.Selectable[])arrayField.GetValue(null);
                    if (count < 0 || arr == null || count > arr.Length)
                    {
                        countField.SetValue(null, 0);
                        arrayField.SetValue(null, new UnityEngine.UI.Selectable[10]);
                    }
                }
            }
            catch { }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeActiveScene()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                OnSceneLoaded(activeScene, LoadSceneMode.Single);
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.IsValid() && !string.IsNullOrEmpty(scene.path))
            {
                if (scene.path.IndexOf("Physics/Game2", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    scene.path.IndexOf("Physics\\Game2", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return;
                }
            }

            SanitizeSelectableArray();
            PurgeAllStrayUI();

            string sName = scene.name;

            switch (sName)
            {
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

        public static void PurgeAllStrayUI()
        {
            try
            {
                string currentScene = SceneManager.GetActiveScene().name;
                GameObject[] allGos = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var go in allGos)
                {
                    if (go == null) continue;
                    if (go.hideFlags.HasFlag(HideFlags.NotEditable) || go.hideFlags.HasFlag(HideFlags.HideAndDontSave)) continue;
                    if (!go.scene.IsValid() && go.scene.name != "DontDestroyOnLoad") continue;

                    string gName = go.name;
                    if (gName == "GeminiApiKeyGroup" || gName == "ApiLabel" || gName == "KeyText" ||
                        gName == "NavPanel" || gName == "QuizSystem_Auto" ||
                        gName.StartsWith("GeminiApiKey") || gName.Contains("ApiKeyGroup"))
                    {
                        if (Application.isPlaying) UnityEngine.Object.Destroy(go);
                        else UnityEngine.Object.DestroyImmediate(go);
                    }
                    else if (gName == "BtnTeacherDashboard" && currentScene != "01_MainMenu" && currentScene != "16_DashboardTeacher")
                    {
                        if (Application.isPlaying) UnityEngine.Object.Destroy(go);
                        else UnityEngine.Object.DestroyImmediate(go);
                    }
                    else if (gName == "PageIndicator" && currentScene != "12_Theory")
                    {
                        if (Application.isPlaying) UnityEngine.Object.Destroy(go);
                        else UnityEngine.Object.DestroyImmediate(go);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PhysicsGame1Bootstrap] Lỗi dọn dẹp UI: {ex.Message}");
            }
        }
    }
}
