using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Định danh route độc lập và bền vững cho toàn bộ 34 màn Aurelia Academy.
    /// Không bao giờ dùng Build Index từ số đầu tên file.
    /// </summary>
    public enum RouteId
    {
        None = 0,
        MainMenu = 1,
        CharacterSelection = 2,
        StudySessionSetup = 3,
        ChapterSelection = 4,
        LessonSelection = 5,

        ChapterMap1 = 6,
        ChapterMap2 = 7,
        ChapterMap3 = 8,
        ChapterMap4 = 9,
        ChapterMap5 = 10,

        ChapterMap1After = 11,
        ChapterMap2After = 12,
        ChapterMap3After = 13,
        ChapterMap4After = 14, // Scene 31 (Tháp Cảm ứng) - swapped in disk
        ChapterMap5After = 15, // Scene 30 (Vườn Năng lượng) - swapped in disk

        ModeSelection = 16,
        Theory = 17,
        DifficultySelection = 18,

        PracticeQuiz = 19,
        WeeklyQuiz = 20,
        EssayAnswer = 21,

        QuizResult = 22,   // Scene 19. Result_TN (Trắc nghiệm)
        EssayResult = 23,  // Scene 19.1. Result_TL (Tự luận)
        WeeklyResult = 24, // Scene 32 (Title says TRẮC NGHIỆM)

        QuizReview = 25,
        EssayReview = 26,

        WeeklyLeaderboard = 27,
        LearningProgress = 28,
        Achievements = 29,
        Settings = 30,
        HostMenu = 31
    }

    /// <summary>
    /// Bảng ánh xạ SceneCatalog từ RouteId sang SceneAsset / SceneName chuẩn trên disk:
    /// - Scene 19 là QuizResult (Trắc nghiệm)
    /// - Scene 19.1 là EssayResult (Tự luận)
    /// - Scene 32 là WeeklyResult
    /// - Scene 30 là Chương V after (Vườn Năng Lượng)
    /// - Scene 31 là Chương IV after (Tháp Cảm Ứng)
    /// </summary>
    public static class SceneCatalog
    {
        private static readonly Dictionary<RouteId, string> RouteToSceneName = new Dictionary<RouteId, string>
        {
            { RouteId.MainMenu, "01_MainMenu" },
            { RouteId.CharacterSelection, "02_CharacterSelection" },
            { RouteId.StudySessionSetup, "22. Buoi hoc hom nay" },
            { RouteId.ChapterSelection, "04_ChapterSelection" },
            { RouteId.LessonSelection, "05_LessonSelection" },

            { RouteId.ChapterMap1, "06_Chương I" },
            { RouteId.ChapterMap2, "07_Chương II" },
            { RouteId.ChapterMap3, "08_Chương III" },
            { RouteId.ChapterMap4, "09_Chương IV 1" },
            { RouteId.ChapterMap5, "10_Chương V 2" },

            { RouteId.ChapterMap1After, "27. Chương I after" },
            { RouteId.ChapterMap2After, "28. Chương II after 1" },
            { RouteId.ChapterMap3After, "29. Chương III after" },
            { RouteId.ChapterMap4After, "31. Chương V after 2" }, // Nội dung là Tháp Cảm Ứng (Chương IV)
            { RouteId.ChapterMap5After, "30. Chương IV after 1" }, // Nội dung là Vườn Năng Lượng (Chương V)

            { RouteId.ModeSelection, "17. Type" },
            { RouteId.Theory, "18. Theory" },
            { RouteId.DifficultySelection, "33. Chọn độ khó" },

            { RouteId.PracticeQuiz, "11_Quiz" },
            { RouteId.WeeklyQuiz, "11.1_Quiz 1" },
            { RouteId.EssayAnswer, "12_WrittenAnswer" },

            { RouteId.QuizResult, "19. Result_TN" },    // Màn kết quả trắc nghiệm (TN)
            { RouteId.EssayResult, "19.1. Result_TL" },  // Màn kết quả tự luận (TL)
            { RouteId.WeeklyResult, "32. Result_Tự luận" }, // Dùng bố cục scene 32 cho kết quả tuần

            { RouteId.QuizReview, "20. Detail Result TN" },
            { RouteId.EssayReview, "20.1 Detail Result TL" },

            { RouteId.WeeklyLeaderboard, "23. Bảng xếp hạng" },
            { RouteId.LearningProgress, "24. Tiến bộ học tập" },
            { RouteId.Achievements, "15.1_Achievements (màn 2)" },
            { RouteId.Settings, "14_Settings" },
            { RouteId.HostMenu, "Scene2_MainMenu" } // Menu chọn môn học của nhóm
        };

        private static readonly Dictionary<string, RouteId> SceneNameToRoute = new Dictionary<string, RouteId>(StringComparer.OrdinalIgnoreCase);

        static SceneCatalog()
        {
            foreach (var kvp in RouteToSceneName)
            {
                SceneNameToRoute[kvp.Value] = kvp.Key;
                // Aliases for filenames with differences
                if (kvp.Value.Contains("Chương"))
                {
                    SceneNameToRoute[kvp.Value.Replace("Chương", "Chuong")] = kvp.Key;
                }
            }
            SceneNameToRoute["14.2_Settings 1"] = RouteId.Settings;
            SceneNameToRoute["15.3_Achievements 1"] = RouteId.Achievements;
            SceneNameToRoute["15.4_Achievements"] = RouteId.Achievements;
            SceneNameToRoute["15_Achievements 1"] = RouteId.Achievements;
        }

        public static string GetSceneName(RouteId routeId)
        {
            if (RouteToSceneName.TryGetValue(routeId, out string sceneName))
            {
                return sceneName;
            }
            Debug.LogError($"[SceneCatalog] Không tìm thấy SceneName cho RouteId: {routeId}");
            return "01_MainMenu";
        }

        public static RouteId GetRouteId(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return RouteId.None;
            if (SceneNameToRoute.TryGetValue(sceneName, out RouteId routeId))
            {
                return routeId;
            }
            return RouteId.None;
        }

        public static RouteId GetChapterMapRoute(int chapterIndex, bool isCompleted = false)
        {
            if (chapterIndex < 1) chapterIndex = 1;
            if (chapterIndex > 5) chapterIndex = 5;

            if (isCompleted)
            {
                switch (chapterIndex)
                {
                    case 1: return RouteId.ChapterMap1After;
                    case 2: return RouteId.ChapterMap2After;
                    case 3: return RouteId.ChapterMap3After;
                    case 4: return RouteId.ChapterMap4After;
                    case 5: return RouteId.ChapterMap5After;
                }
            }

            switch (chapterIndex)
            {
                case 1: return RouteId.ChapterMap1;
                case 2: return RouteId.ChapterMap2;
                case 3: return RouteId.ChapterMap3;
                case 4: return RouteId.ChapterMap4;
                case 5: return RouteId.ChapterMap5;
                default: return RouteId.ChapterMap1;
            }
        }
    }
}
