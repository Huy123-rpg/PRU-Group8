using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Tự động khởi tạo hệ sinh thái Core của Physics Game 2 trước khi bất kỳ Scene nào được load.
    /// Cho phép chạy test trực tiếp từ bất kỳ Scene nào (01 đến 20) trong Unity Editor mà không bị NullReferenceException.
    /// Tự động gắn Controller tương ứng cho tất cả Scene của Physics Game 2 ngay từ lần chạy đầu tiên.
    /// </summary>
    public static class PhysicsGameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            // Đăng ký sớm ngay từ BeforeSceneLoad để luôn chạy đầu tiên khi chuyển scene
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Kích hoạt singleton PhysicsGameSession & NavigationService (DontDestroyOnLoad)
            PhysicsGameSession session = PhysicsGameSession.Instance;
            if (session != null)
            {
                // Pre-warm save data
                PhysicsSaveManager.Load();
            }

            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.ResetTransitionLock();
            }

            if (Object.FindAnyObjectByType<SettingsManager>() == null)
            {
                GameObject settingsGo = new GameObject("SettingsManager");
                settingsGo.AddComponent<SettingsManager>();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void SetupSceneListener()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Gắn controller cho scene khởi đầu
            AttachControllerForScene(SceneManager.GetActiveScene().name);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Đảm bảo không còn overlay tối hay màn mờ loading nào sót lại trên bất kỳ scene nào
            var strayOverlay = GameObject.Find("NavigationLoadingOverlay");
            if (strayOverlay != null) strayOverlay.SetActive(false);

            AttachControllerForScene(scene.name);
        }

        private static GameObject FindSceneCanvasOrCreateAnchor(string fallbackName, string sceneName = null)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
                {
                    if (canvas != null && canvas.gameObject.scene.name == sceneName) return canvas.gameObject;
                }

                var scene = SceneManager.GetSceneByName(sceneName);
                if (scene.IsValid() && scene.isLoaded)
                {
                    foreach (var root in scene.GetRootGameObjects())
                        if (root.name == "Canvas") return root;
                }
            }

            Scene activeScene = SceneManager.GetActiveScene();
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
            foreach (var c in canvases)
            {
                if (c != null && c.gameObject.scene == activeScene)
                {
                    return c.gameObject;
                }
            }

            GameObject found = GameObject.Find("Canvas");
            if (found != null) return found;

            return new GameObject(fallbackName);
        }

        private static bool HasStudyScreensControllerInScene(string sceneName)
        {
            foreach (var controller in Object.FindObjectsByType<StudyScreensController>(FindObjectsInactive.Include))
            {
                if (controller != null && controller.gameObject.scene.name == sceneName) return true;
            }
            return false;
        }

        /// <summary>
        /// Gắn đúng Controller vào scene tương ứng nếu chưa có trong scene hierarchy.
        /// Tránh duplicate controller và đảm bảo luồng hoạt động tự động 100%.
        /// </summary>
        public static void AttachControllerForScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;

            switch (sceneName)
            {
                case PhysicsSceneNames.MainMenu:
                    if (Object.FindAnyObjectByType<MainMenuController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("MainMenuController", sceneName);
                        anchor.AddComponent<MainMenuController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn MainMenuController vào Scene 01_MainMenu.");
                    }
                    break;

                case PhysicsSceneNames.CharacterSelection:
                    if (Object.FindAnyObjectByType<CharacterSelectionController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("CharacterSelectionController", sceneName);
                        anchor.AddComponent<CharacterSelectionController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn CharacterSelectionController vào Scene 02_CharacterSelection.");
                    }
                    break;

                case PhysicsSceneNames.TeacherSelection:
                    if (Object.FindAnyObjectByType<TeacherSelectionController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("TeacherSelectionController", sceneName);
                        anchor.AddComponent<TeacherSelectionController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn TeacherSelectionController vào Scene 03_TeacherSelection.");
                    }
                    break;

                case PhysicsSceneNames.ChapterSelection:
                    if (Object.FindAnyObjectByType<ChapterSelectionController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("ChapterSelectionController", sceneName);
                        anchor.AddComponent<ChapterSelectionController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn ChapterSelectionController vào Scene 04_ChapterSelection.");
                    }
                    break;

                case PhysicsSceneNames.LessonSelection:
                    if (Object.FindAnyObjectByType<LessonSelectionController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("LessonSelectionController", sceneName);
                        anchor.AddComponent<LessonSelectionController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn LessonSelectionController vào Scene 05_LessonSelection.");
                    }
                    break;

                case PhysicsSceneNames.Guide:
                    if (Object.FindAnyObjectByType<GuideController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("GuideController", sceneName);
                        anchor.AddComponent<GuideController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn GuideController vào Scene 16. Guide.");
                    }
                    break;

                case PhysicsSceneNames.LessonType:
                    if (Object.FindAnyObjectByType<LessonTypeController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("LessonTypeController", sceneName);
                        anchor.AddComponent<LessonTypeController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn LessonTypeController vào Scene 17. Type.");
                    }
                    break;

                case PhysicsSceneNames.Theory:
                    if (Object.FindAnyObjectByType<TheoryUIController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("TheoryUIController", sceneName);
                        anchor.AddComponent<TheoryUIController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn TheoryUIController vào Scene 18. Theory.");
                    }
                    break;

                case PhysicsSceneNames.Quiz:
                    if (Object.FindAnyObjectByType<QuizGameController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("QuizGameController", sceneName);
                        anchor.AddComponent<QuizGameController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn QuizGameController vào Scene 11_Quiz.");
                    }
                    break;

                case PhysicsSceneNames.WeeklyQuiz:
                    if (Object.FindAnyObjectByType<WeeklyQuizController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("WeeklyQuizController", sceneName);
                        anchor.AddComponent<WeeklyQuizController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn WeeklyQuizController vào Scene 11.1_Quiz 1.");
                    }
                    break;

                case PhysicsSceneNames.QuizResult: // "19. Result_TN"
                    bool isEssayOnQuizScene = (NavigationService.Instance != null && NavigationService.Instance.Selection.Mode == "ESSAY");
                    if (isEssayOnQuizScene)
                    {
                        if (Object.FindAnyObjectByType<EssayResultController>() == null)
                        {
                            GameObject anchor = FindSceneCanvasOrCreateAnchor("EssayResultController", sceneName);
                            anchor.AddComponent<EssayResultController>();
                            Debug.Log($"[PhysicsGameBootstrap] Đã gắn EssayResultController vào Scene {sceneName}.");
                        }
                    }
                    else
                    {
                        if (Object.FindAnyObjectByType<QuizResultController>() == null)
                        {
                            GameObject anchor = FindSceneCanvasOrCreateAnchor("QuizResultController", sceneName);
                            anchor.AddComponent<QuizResultController>();
                            Debug.Log($"[PhysicsGameBootstrap] Đã gắn QuizResultController vào Scene {sceneName}.");
                        }
                    }
                    break;

                case PhysicsSceneNames.WrittenResult: // "19.1. Result_TL"
                    bool isQuizOnEssayScene = (NavigationService.Instance != null && NavigationService.Instance.Selection.Mode == "QUIZ");
                    if (isQuizOnEssayScene)
                    {
                        if (Object.FindAnyObjectByType<QuizResultController>() == null)
                        {
                            GameObject anchor = FindSceneCanvasOrCreateAnchor("QuizResultController", sceneName);
                            anchor.AddComponent<QuizResultController>();
                            Debug.Log($"[PhysicsGameBootstrap] Đã gắn QuizResultController vào Scene {sceneName}.");
                        }
                    }
                    else
                    {
                        if (Object.FindAnyObjectByType<EssayResultController>() == null)
                        {
                            GameObject anchor = FindSceneCanvasOrCreateAnchor("EssayResultController", sceneName);
                            anchor.AddComponent<EssayResultController>();
                            Debug.Log($"[PhysicsGameBootstrap] Đã gắn EssayResultController vào Scene {sceneName}.");
                        }
                    }
                    break;

                case PhysicsSceneNames.WeeklyResult: // "32. Result_Tự luận"
                    if (Object.FindAnyObjectByType<WeeklyResultController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("WeeklyResultController", sceneName);
                        anchor.AddComponent<WeeklyResultController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn WeeklyResultController vào Scene 32. Result_Tự luận.");
                    }
                    break;

                case PhysicsSceneNames.QuizReview: // "20. Detail Result TN"
                    if (Object.FindAnyObjectByType<QuizReviewController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("QuizReviewController", sceneName);
                        anchor.AddComponent<QuizReviewController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn QuizReviewController vào Scene 20. Detail Result TN.");
                    }
                    break;

                case PhysicsSceneNames.WrittenReview: // "20.1 Detail Result TL"
                    if (Object.FindAnyObjectByType<EssayReviewController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("EssayReviewController", sceneName);
                        anchor.AddComponent<EssayReviewController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn EssayReviewController vào Scene 20.1 Detail Result TL.");
                    }
                    break;

                case PhysicsSceneNames.DifficultySelection: // "33. Chọn độ khó"
                    if (Object.FindAnyObjectByType<DifficultySelectionController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("DifficultySelectionController", sceneName);
                        anchor.AddComponent<DifficultySelectionController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn DifficultySelectionController vào Scene 33. Chọn độ khó.");
                    }
                    break;

                case PhysicsSceneNames.WrittenAnswer:
                    if (Object.FindAnyObjectByType<WrittenAnswerController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("WrittenAnswerController", sceneName);
                        anchor.AddComponent<WrittenAnswerController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn WrittenAnswerController vào Scene 12_WrittenAnswer.");
                    }
                    break;

                case PhysicsSceneNames.Inventory:
                    if (Object.FindAnyObjectByType<InventoryUIController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("InventoryUIController", sceneName);
                        anchor.AddComponent<InventoryUIController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn InventoryUIController vào Scene 13_Inventory.");
                    }
                    break;

                case PhysicsSceneNames.Settings:
                case "14.2_Settings 1":
                    if (Object.FindAnyObjectByType<SettingsUIController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("SettingsUIController", sceneName);
                        anchor.AddComponent<SettingsUIController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn SettingsUIController vào Scene Settings.");
                    }
                    break;

                case PhysicsSceneNames.Achievements:
                case "15.3_Achievements 1":
                case "15.4_Achievements":
                case "15_Achievements 1":
                    if (Object.FindAnyObjectByType<AchievementUIController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("AchievementUIController", sceneName);
                        anchor.AddComponent<AchievementUIController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn AchievementUIController vào Scene Achievements.");
                    }
                    break;

                case PhysicsSceneNames.TodayStudy:
                case PhysicsSceneNames.WeeklyLeaderboard:
                case PhysicsSceneNames.LearningProgress:
                case PhysicsSceneNames.LearningManagement:
                    if (!HasStudyScreensControllerInScene(sceneName))
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("StudyScreensController", sceneName);
                        anchor.AddComponent<StudyScreensController>();
                    }
                    break;

                // 5 Chương gameplay và 5 scene after đều dùng ChapterMapController (WASD, trạm thiết bị, khôi phục)
                case PhysicsSceneNames.GameplayMechanics:
                case PhysicsSceneNames.GameplayOptics:
                case PhysicsSceneNames.GameplayElectricity:
                case PhysicsSceneNames.GameplayElectromagnetism:
                case PhysicsSceneNames.GameplayEnergy:
                case PhysicsSceneNames.ChapterMap1After:
                case PhysicsSceneNames.ChapterMap2After:
                case PhysicsSceneNames.ChapterMap3After:
                case PhysicsSceneNames.ChapterMap4After:
                case PhysicsSceneNames.ChapterMap5After:
                    if (Object.FindAnyObjectByType<ChapterMapController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("ChapterMapController", sceneName);
                        anchor.AddComponent<ChapterMapController>();
                        Debug.Log($"[PhysicsGameBootstrap] Đã gắn ChapterMapController vào Scene gameplay {sceneName}.");
                    }
                    break;

                default:
                    if (sceneName.StartsWith("22.") || sceneName.StartsWith("23.") || sceneName.StartsWith("24.") || sceneName.StartsWith("25.") || sceneName.StartsWith("26."))
                    {
                        if (!HasStudyScreensControllerInScene(sceneName))
                        {
                            GameObject anchor = FindSceneCanvasOrCreateAnchor("StudyScreensController", sceneName);
                            anchor.AddComponent<StudyScreensController>();
                        }
                    }
                    else if (sceneName.Contains("Chương") || sceneName.Contains("Chuong"))
                    {
                        if (Object.FindAnyObjectByType<ChapterMapController>() == null)
                        {
                            GameObject anchor = FindSceneCanvasOrCreateAnchor("ChapterMapController", sceneName);
                            anchor.AddComponent<ChapterMapController>();
                            Debug.Log($"[PhysicsGameBootstrap] Đã gắn ChapterMapController vào Scene gameplay {sceneName}.");
                        }
                    }
                    break;
            }

            AttachSessionTimerIfApplicable(sceneName);
        }

        private static void AttachSessionTimerIfApplicable(string sceneName)
        {
            if (sceneName == PhysicsSceneNames.Theory ||
                sceneName == PhysicsSceneNames.Quiz ||
                sceneName == PhysicsSceneNames.WrittenAnswer ||
                sceneName == PhysicsSceneNames.LessonType ||
                sceneName == PhysicsSceneNames.DifficultySelection ||
                sceneName == PhysicsSceneNames.QuizResult ||
                sceneName == PhysicsSceneNames.WrittenResult ||
                sceneName == PhysicsSceneNames.QuizReview ||
                sceneName == PhysicsSceneNames.WrittenReview)
            {
                if (UnityEngine.Object.FindAnyObjectByType<StudySessionTimerUI>() == null)
                {
                    GameObject anchor = FindSceneCanvasOrCreateAnchor("StudySessionTimerUI", sceneName);
                    anchor.AddComponent<StudySessionTimerUI>();
                }
            }
        }
    }
}
