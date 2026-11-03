using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Cấu trúc dữ liệu nội dung cốt truyện Aurelia Academy dùng chung cho StoryModalController.
    /// </summary>
    [Serializable]
    public class StoryContent
    {
        public string id;
        public string title;
        public string speaker;
        public string body;
        public string mission;
        public string rewardTitle;
        public string rewardDescription;
        public string buttonText = "TIẾP TỤC";
        public bool allowCloseButton = false;

        [NonSerialized] public Sprite speakerPortrait;
        [NonSerialized] public Action onContinue;
    }

    /// <summary>
    /// Nguồn dữ liệu cốt truyện chính thức (Source of Truth) của Aurelia Academy.
    /// </summary>
    public static class AureliaStoryCatalog
    {
        // =====================================================================
        // 1. MỞ ĐẦU GAME – PROLOGUE 3 HỘP THOẠI (03_TeacherSelection -> 22. Buoi hoc hom nay)
        // =====================================================================
        public static StoryContent CreatePrologueNarrator(Action onContinue = null)
        {
            return new StoryContent
            {
                id = "prologue_narrator",
                title = "LỄ HỘI ÁNH SAO",
                speaker = "NGƯỜI DẪN",
                body = "Ngày mai là Lễ hội Ánh Sao. Nhưng cơn bão đêm qua đã làm nhiều hệ thống của Aurelia ngừng hoạt động.",
                buttonText = "TIẾP THEO",
                allowCloseButton = false,
                onContinue = onContinue
            };
        }

        public static StoryContent CreatePrologueTeacherMission(string teacherId, Action onContinue = null, Sprite portrait = null)
        {
            return new StoryContent
            {
                id = "prologue_teacher_mission",
                title = "NHIỆM VỤ KHÔI PHỤC",
                speaker = GetTeacherDisplayName(teacherId),
                body = "Năm khu vực đang cần được khôi phục. Thầy cô sẽ hướng dẫn em học những kiến thức cần thiết để giúp mọi người.",
                buttonText = "TIẾP THEO",
                speakerPortrait = portrait,
                allowCloseButton = false,
                onContinue = onContinue
            };
        }

        public static StoryContent CreatePrologueTeacherChoice(string teacherId, Action onContinue = null, Sprite portrait = null)
        {
            return new StoryContent
            {
                id = "prologue_teacher_choice",
                title = "TỰ DO KHÁM PHÁ",
                speaker = GetTeacherDisplayName(teacherId),
                body = "Em muốn bắt đầu ở khu vực nào cũng được. Mỗi phần hoàn thành sẽ đưa chúng ta đến gần đêm hội hơn.",
                buttonText = "BẮT ĐẦU HÀNH TRÌNH",
                speakerPortrait = portrait,
                allowCloseButton = false,
                onContinue = onContinue
            };
        }

        public static string GetTeacherDisplayName(string teacherId)
        {
            string normalized = (teacherId ?? "Minh").Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "lan": return "CÔ LAN";
                case "duy": return "THẦY DUY";
                case "ha": return "CÔ HÀ";
                default: return "THẦY MINH";
            }
        }

        public static StoryContent CreateAureliaCrisisIntro(Action onContinue = null)
        {
            return CreatePrologueNarrator(onContinue);
        }

        // =====================================================================
        // 2. XÁC NHẬN NHÂN VẬT (02_CharacterSelection -> 03_TeacherSelection)
        // =====================================================================
        public const string CHARACTER_CONFIRMED_TITLE = "Học viên đã được xác nhận.";
        public const string CHARACTER_CONFIRMED_BODY = "Bây giờ hãy chọn một giáo viên hướng dẫn cho hành trình của bạn.";

        // =====================================================================
        // 3. LỜI THOẠI RIÊNG CỦA 4 GIÁO VIÊN (03_TeacherSelection -> 04_ChapterSelection)
        // =====================================================================
        public static StoryContent CreateTeacherIntro(string teacherId, Action onContinue = null, Sprite portrait = null)
        {
            string normalized = (teacherId ?? "Minh").Trim();

            switch (normalized.ToLowerInvariant())
            {
                case "lan":
                    return new StoryContent
                    {
                        id = "teacher_intro_lan",
                        title = "GIÁO VIÊN HƯỚNG DẪN",
                        speaker = "CÔ LAN",
                        body = "“Kiến thức Vật lý không chỉ nằm trong công thức.\n\n" +
                               "Điều quan trọng là em phải hiểu hiện tượng xảy ra như thế nào và vì sao nó xảy ra.\n\n" +
                               "Chúng ta sẽ vừa học, vừa quan sát và từng bước đưa Aurelia hoạt động trở lại.”",
                        buttonText = "EM SẴN SÀNG",
                        speakerPortrait = portrait,
                        onContinue = onContinue
                    };

                case "duy":
                    return new StoryContent
                    {
                        id = "teacher_intro_duy",
                        title = "GIÁO VIÊN HƯỚNG DẪN",
                        speaker = "THẦY DUY",
                        body = "“Các Physics Core sẽ không được khôi phục chỉ bằng việc ghi nhớ công thức.\n\n" +
                               "Em sẽ phải vận dụng kiến thức vào những tình huống khó hơn.\n\n" +
                               "Nếu đã sẵn sàng đối mặt với thử thách, chúng ta bắt đầu.”",
                        buttonText = "EM SẴN SÀNG",
                        speakerPortrait = portrait,
                        onContinue = onContinue
                    };

                case "ha":
                    return new StoryContent
                    {
                        id = "teacher_intro_ha",
                        title = "GIÁO VIÊN HƯỚNG DẪN",
                        speaker = "CÔ HÀ",
                        body = "“Mỗi bài toán đều có nhiều cách để tiếp cận.\n\n" +
                               "Có lúc em cần nhớ kiến thức cơ bản, có lúc phải phân tích sâu hơn và kết nối nhiều ý tưởng với nhau.\n\n" +
                               "Hãy cùng tìm ra cách học phù hợp nhất với em.”",
                        buttonText = "EM SẴN SÀNG",
                        speakerPortrait = portrait,
                        onContinue = onContinue
                    };

                case "minh":
                default:
                    return new StoryContent
                    {
                        id = "teacher_intro_minh",
                        title = "GIÁO VIÊN HƯỚNG DẪN",
                        speaker = "THẦY MINH",
                        body = "“Chào mừng em đến Aurelia.\n\n" +
                               "Chúng ta sẽ bắt đầu từ những kiến thức cơ bản nhất. Đừng vội vàng – nếu hiểu đúng nguyên lý, những thử thách khó hơn cũng sẽ trở nên dễ dàng.\n\n" +
                               "Hãy cùng khôi phục các Physics Core.”",
                        buttonText = "EM SẴN SÀNG",
                        speakerPortrait = portrait,
                        onContinue = onContinue
                    };
            }
        }

        // =====================================================================
        // 4. CORE I – V INTRO STORY (Hiển thị khi chọn chương)
        // =====================================================================
        public static StoryContent CreateCoreIntro(int chapterNumber, Action onContinue = null)
        {
            switch (chapterNumber)
            {
                case 1:
                    return new StoryContent
                    {
                        id = "core_1_intro",
                        title = "CHƯƠNG I: NĂNG LƯỢNG CƠ HỌC",
                        mission = "Sửa cơ cấu cầu vận chuyển",
                        body = "Cơ cấu cầu vận chuyển của học viện đang bị hỏng. Hãy tìm hiểu về năng lượng cơ học để khôi phục hoạt động của cây cầu.",
                        buttonText = "VÀO HỌC",
                        onContinue = onContinue
                    };

                case 2:
                    return new StoryContent
                    {
                        id = "core_2_intro",
                        title = "CHƯƠNG II: ÁNH SÁNG",
                        mission = "Khôi phục hệ thống trình diễn ánh sáng",
                        body = "Hệ thống trình diễn ánh sáng đang bị gián đoạn. Hãy tìm hiểu đường truyền ánh sáng để giúp sân khấu hoạt động trở lại.",
                        buttonText = "VÀO HỌC",
                        onContinue = onContinue
                    };

                case 3:
                    return new StoryContent
                    {
                        id = "core_3_intro",
                        title = "CHƯƠNG III: ĐIỆN",
                        mission = "Sửa các mô hình điện trong phòng học",
                        body = "Các mô hình điện trong phòng học đang mất điện. Hãy tìm hiểu về mạch điện và định luật Ohm để sửa chữa các thiết bị.",
                        buttonText = "VÀO HỌC",
                        onContinue = onContinue
                    };

                case 4:
                    return new StoryContent
                    {
                        id = "core_4_intro",
                        title = "CHƯƠNG IV: ĐIỆN TỪ",
                        mission = "Khôi phục chuông điện từ",
                        body = "Chuông điện từ của học viện đã ngừng kêu. Hãy tìm hiểu về từ trường và cảm ứng điện từ để khôi phục tiếng chuông.",
                        buttonText = "VÀO HỌC",
                        onContinue = onContinue
                    };

                case 5:
                default:
                    return new StoryContent
                    {
                        id = "core_5_intro",
                        title = "CHƯƠNG V: NĂNG LƯỢNG VỚI CUỘC SỐNG",
                        mission = "Chuẩn bị nguồn và kế hoạch năng lượng",
                        body = "Học viện cần chuẩn bị nguồn năng lượng cho lễ hội. Hãy lập kế hoạch sử dụng các nguồn năng lượng bền vững.",
                        buttonText = "VÀO HỌC",
                        onContinue = onContinue
                    };
            }
        }

        // =====================================================================
        // 5. CORE I – V RESTORED STORY (Khi hoàn thành bài kiểm tra bắt buộc của chương)
        //    Reward chỉ hiển thị nếu vật phẩm ĐÃ THỰC SỰ UNLOCK trong SaveData.
        // =====================================================================
        public static StoryContent CreateCoreRestored(int chapterNumber, PhysicsSaveData save, Action onContinue = null)
        {
            ResolveUnlockedChapterReward(chapterNumber, save, out string rewardTitle, out string rewardDesc);

            switch (chapterNumber)
            {
                case 1:
                    return new StoryContent
                    {
                        id = "core_1_restored",
                        title = "KHU VỰC CƠ HỌC ĐÃ PHỤC HỒI",
                        body = "Cây cầu đã hoạt động! Vật tư có thể được đưa vào sân trường.",
                        rewardTitle = rewardTitle,
                        rewardDescription = rewardDesc,
                        buttonText = "TIẾP TỤC",
                        onContinue = onContinue
                    };

                case 2:
                    return new StoryContent
                    {
                        id = "core_2_restored",
                        title = "HỆ THỐNG ÁNH SÁNG ĐÃ PHỤC HỒI",
                        body = "Những dải màu đã trở lại. Sân khấu đang sẵn sàng cho đêm hội.",
                        rewardTitle = rewardTitle,
                        rewardDescription = rewardDesc,
                        buttonText = "TIẾP TỤC",
                        onContinue = onContinue
                    };

                case 3:
                    return new StoryContent
                    {
                        id = "core_3_restored",
                        title = "MÔ HÌNH ĐIỆN ĐÃ PHỤC HỒI",
                        body = "Các mô hình điện đã hoạt động. Những nhóm học sinh có thể tiếp tục chuẩn bị.",
                        rewardTitle = rewardTitle,
                        rewardDescription = rewardDesc,
                        buttonText = "TIẾP TỤC",
                        onContinue = onContinue
                    };

                case 4:
                    return new StoryContent
                    {
                        id = "core_4_restored",
                        title = "CHUÔNG ĐIỆN TỪ ĐÃ PHỤC HỒI",
                        body = "Tiếng chuông đã vang lên. Học viện đang dần trở lại nhịp sống.",
                        rewardTitle = rewardTitle,
                        rewardDescription = rewardDesc,
                        buttonText = "TIẾP TỤC",
                        onContinue = onContinue
                    };

                case 5:
                default:
                    return new StoryContent
                    {
                        id = "core_5_restored",
                        title = "KẾ HOẠCH NĂNG LƯỢNG ĐÃ HOÀN TẤT",
                        body = "Kế hoạch năng lượng đã sẵn sàng cho lễ hội.",
                        rewardTitle = rewardTitle,
                        rewardDescription = rewardDesc,
                        buttonText = "TIẾP TỤC",
                        onContinue = onContinue
                    };
            }
        }

        // =====================================================================
        // 6. ENERGY CRYSTAL STORY (Đoạn cao trào trước Final Completion Modal)
        // =====================================================================
        public static StoryContent CreateEnergyCrystalConvergence(Action onContinue = null)
        {
            return new StoryContent
            {
                id = "energy_crystal_convergence",
                title = "NĂNG LƯỢNG ĐANG HỘI TỤ...",
                body = "Những thiết bị mà bạn thu thập được bắt đầu phản ứng với nhau.\n\n" +
                       "Năng lượng từ năm Physics Core hội tụ tại trung tâm Aurelia.",
                buttonText = "TIẾP TỤC",
                onContinue = onContinue
            };
        }

        public static StoryContent CreateEnergyCrystalRestored(PhysicsSaveData save, Action onContinue = null)
        {
            bool crystalUnlocked = save != null && save.IsItemUnlocked(PhysicsFinalProgression.FINAL_ITEM_ID);
            return new StoryContent
            {
                id = "energy_crystal_restored",
                title = "TINH THỂ AURELIA ĐÃ ĐƯỢC TÁI TẠO",
                body = "Tinh thể trung tâm đã hấp thụ năng lượng từ cả năm Core.\n\n" +
                       "Aurelia Academy đã sẵn sàng hoạt động trở lại.",
                rewardTitle = crystalUnlocked ? "ENERGY CRYSTAL" : "",
                rewardDescription = crystalUnlocked ? "Tinh thể năng lượng trung tâm của Học viện Aurelia." : "",
                buttonText = "HOÀN TẤT KHÔI PHỤC",
                onContinue = onContinue
            };
        }

        // =====================================================================
        // KIỂM TRA REWARD THẬT TỪ SAVEDATA (KHÔNG BAO GIỜ FAKE REWARD)
        // =====================================================================
        public static void ResolveUnlockedChapterReward(int chapterNumber, PhysicsSaveData save, out string rewardTitle, out string rewardDesc)
        {
            rewardTitle = "";
            rewardDesc = "";
            if (save == null) return;

            switch (chapterNumber)
            {
                case 1:
                    if (save.IsItemUnlocked("gear"))
                    {
                        rewardTitle = "BÁNH RĂNG";
                        rewardDesc = "Một bộ phận quan trọng trong cơ cấu vận hành của Aurelia.";
                    }
                    break;

                case 2:
                {
                    List<string> unlocked = new List<string>();
                    if (save.IsItemUnlocked("mirror")) unlocked.Add("GƯƠNG");
                    if (save.IsItemUnlocked("lens")) unlocked.Add("THẤU KÍNH");
                    if (save.IsItemUnlocked("prism")) unlocked.Add("LĂNG KÍNH");
                    if (unlocked.Count > 0)
                    {
                        rewardTitle = string.Join(" – ", unlocked);
                        rewardDesc = "Thiết bị quang học đã hiệu chỉnh để dẫn truyền ánh sáng tại Aurelia.";
                    }
                    break;
                }

                case 3:
                {
                    List<string> unlocked = new List<string>();
                    if (save.IsItemUnlocked("battery")) unlocked.Add("Pin");
                    if (save.IsItemUnlocked("switch")) unlocked.Add("Công tắc");
                    if (save.IsItemUnlocked("electrical_box")) unlocked.Add("Hộp điện");
                    if (save.IsItemUnlocked("ammeter")) unlocked.Add("Ampe kế");
                    if (save.IsItemUnlocked("voltmeter")) unlocked.Add("Vôn kế");
                    if (save.IsItemUnlocked("wire")) unlocked.Add("Dây dẫn");
                    if (unlocked.Count > 0)
                    {
                        rewardTitle = string.Join(" – ", unlocked);
                        rewardDesc = "Các thiết bị giúp khôi phục và vận hành mạng điện của Aurelia.";
                    }
                    break;
                }

                case 4:
                    if (save.IsItemUnlocked("magnet"))
                    {
                        rewardTitle = "NAM CHÂM";
                        rewardDesc = "Một thiết bị quan trọng để kích hoạt hệ thống cảm ứng của Aurelia.";
                    }
                    break;

                case 5:
                {
                    List<string> unlocked = new List<string>();
                    if (save.IsItemUnlocked("turbine")) unlocked.Add("Turbine");
                    if (save.IsItemUnlocked("solar_panel")) unlocked.Add("Solar Panel");
                    if (unlocked.Count > 0)
                    {
                        rewardTitle = string.Join(" – ", unlocked);
                        rewardDesc = "Hệ thống tái tạo cung cấp nguồn năng lượng bền vững cho Aurelia.";
                    }
                    break;
                }
            }
        }
    }
}
