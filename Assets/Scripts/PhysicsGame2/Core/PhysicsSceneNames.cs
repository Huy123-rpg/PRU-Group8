namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Hằng số định danh 16 scene trong Physics Game 2.
    /// Giúp tránh magic strings và đảm bảo navigation chính xác 100%.
    /// </summary>
    public static class PhysicsSceneNames
    {
        public const string MainMenu = "01_MainMenu";
        public const string CharacterSelection = "02_CharacterSelection";
        public const string TeacherSelection = "03_TeacherSelection";
        public const string ChapterSelection = "04_ChapterSelection";
        public const string LessonSelection = "05_LessonSelection";

        // 5 Scene Gameplay mô phỏng theo 5 chương
        public const string GameplayMechanics = "06_Chương I";           // Chương I: Năng lượng cơ học
        public const string GameplayOptics = "07_Chương II";                 // Chương II: Ánh sáng
        public const string GameplayElectricity = "08_Chương III";       // Chương III: Điện
        public const string GameplayElectromagnetism = "09_Chương IV 1"; // Chương IV: Điện từ
        public const string GameplayEnergy = "10_Chương V 2";                 // Chương V: Năng lượng với cuộc sống

        public const string ChapterMap1After = "27. Chương I after";
        public const string ChapterMap2After = "28. Chương II after 1";
        public const string ChapterMap3After = "29. Chương III after";
        public const string ChapterMap4After = "31. Chương V after 2"; // Swapped in disk
        public const string ChapterMap5After = "30. Chương IV after 1"; // Swapped in disk

        // 3 Scene bài tập
        public const string Quiz = "11_Quiz";
        public const string WeeklyQuiz = "11.1_Quiz 1";
        public const string WrittenAnswer = "12_WrittenAnswer";

        // Hệ thống bổ trợ
        public const string Inventory = "13_Inventory";
        public const string Settings = "14_Settings";
        public const string Achievements = "15.1_Achievements (màn 2)";
        public const string Guide = "16. Guide";
        public const string LessonType = "17. Type"; // Màn chọn hình thức học
        public const string Theory = "18. Theory";   // Màn xem lý thuyết bài học
        public const string QuizResult = "19. Result_TN"; // Màn kết quả trắc nghiệm (TN)
        public const string WrittenResult = "19.1. Result_TL"; // Màn kết quả tự luận (TL)
        public const string QuizReview = "20. Detail Result TN"; // Màn xem lời giải chi tiết trắc nghiệm
        public const string WrittenReview = "20.1 Detail Result TL"; // Màn xem chi tiết câu tự luận
        public const string TodayStudy = "22. Buoi hoc hom nay";
        public const string StudySummary = TodayStudy; // Quy về Buoi hoc hom nay theo đặc tả (không thêm màn tổng kết riêng)
        public const string WeeklyLeaderboard = "23. Bảng xếp hạng";
        public const string WeeklyRank = WeeklyLeaderboard;
        public const string LearningProgress = "24. Tiến bộ học tập";
        public const string LearningManagement = "25. Quản lý học tập của giáo viên";
        public const string TeacherStudyManagement = LearningManagement;
        public const string WeeklyResult = "32. Result_Tự luận"; // Dùng cho kết quả thử thách tuần
        public const string DifficultySelection = "33. Chọn độ khó";
        public const string HostMenu = "Scene2_MainMenu"; // Menu chọn môn học của nhóm

        /// <summary>
        /// Lấy tên scene gameplay tương ứng với chương (1 -> 5).
        /// </summary>
        public static string GetGameplaySceneForChapter(int chapterNumber)
        {
            switch (chapterNumber)
            {
                case 1: return GameplayMechanics;
                case 2: return GameplayOptics;
                case 3: return GameplayElectricity;
                case 4: return GameplayElectromagnetism;
                case 5: return GameplayEnergy;
                default: return GameplayMechanics;
            }
        }

        /// <summary>
        /// Lấy scene quay lại mặc định theo quy tắc luồng game trong spec:
        /// 02 Character -> Main Menu
        /// 03 Teacher -> Character
        /// 04 Chapter -> Teacher
        /// 05 Lesson -> Chapter
        /// Gameplay (06-10) -> Lesson
        /// Quiz (11) -> Lesson
        /// Written (12) -> Lesson
        /// Inventory (13) -> previousScene hoặc MainMenu
        /// Settings (14) -> previousScene hoặc MainMenu
        /// Achievements (15) -> previousScene hoặc MainMenu
        /// Guide (16) -> MainMenu
        /// </summary>
        public static string GetDefaultBackScene(string currentSceneName, string previousSceneName = null)
        {
            switch (currentSceneName)
            {
                case CharacterSelection:
                    return MainMenu;

                case TeacherSelection:
                    return CharacterSelection;

                case ChapterSelection:
                    return TeacherSelection;

                case LessonSelection:
                    return ChapterSelection;

                case GameplayMechanics:
                case GameplayOptics:
                case GameplayElectricity:
                case GameplayElectromagnetism:
                case GameplayEnergy:
                case Quiz:
                case WrittenAnswer:
                    return LessonType; // Hoặc LessonSelection

                case Theory:
                    return LessonType;

                case QuizResult:
                    return LessonType;

                case QuizReview:
                    return QuizResult;

                case WrittenReview:
                    return WrittenAnswer;

                case LessonType:
                    return !string.IsNullOrEmpty(previousSceneName) ? previousSceneName : GameplayElectricity;

                case Inventory:
                case Settings:
                case Achievements:
                    return !string.IsNullOrEmpty(previousSceneName) ? previousSceneName : MainMenu;

                case Guide:
                    return MainMenu;

                case TodayStudy:
                case WeeklyLeaderboard:
                case LearningProgress:
                case LearningManagement:
                    return MainMenu;

                default:
                    return MainMenu;
            }
        }
    }
}
