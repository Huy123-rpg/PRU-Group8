using System;
using System.Collections.Generic;

namespace ScienceQuest.PhysicsGame2
{
    [Serializable]
    public class PhysicsAchievementData
    {
        public string id;
        public string title;
        public string description;
        public int orderIndex; // 1 đến 7

        public PhysicsAchievementData(string id, string title, string description, int orderIndex)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.orderIndex = orderIndex;
        }

        // =========================================================================
        // 7 THÀNH TÍCH CHÍNH THỨC CỦA HỌC VIỆN AURELIA
        // =========================================================================
        private static readonly Dictionary<string, PhysicsAchievementData> _achievements = new Dictionary<string, PhysicsAchievementData>
        {
            {
                "complete_chapter_1", new PhysicsAchievementData(
                    "complete_chapter_1",
                    "Hoàn thành chương I",
                    "Vượt qua tất cả bài học và bài kiểm tra thuộc Chương I: Năng lượng cơ học.",
                    1
                )
            },
            {
                "quiz_50", new PhysicsAchievementData(
                    "quiz_50",
                    "Giải 50 câu trắc nghiệm",
                    "Đã tham gia làm từ 50 câu hỏi trắc nghiệm trở lên.",
                    2
                )
            },
            {
                "collect_all_items", new PhysicsAchievementData(
                    "collect_all_items",
                    "Thu thập tất cả vật phẩm",
                    "Khám phá và thu thập trọn bộ 12 vật phẩm độc đáo trong kho đồ.",
                    3
                )
            },
            {
                "complete_all_chapters", new PhysicsAchievementData(
                    "complete_all_chapters",
                    "Hoàn thành toàn bộ chương",
                    "Hoàn thành toàn bộ bài học của cả 5 chương trình học Vật lý 9.",
                    4
                )
            },
            {
                "discover_secret", new PhysicsAchievementData(
                    "discover_secret",
                    "Khám phá bí mật học viện",
                    "Tìm ra bí mật cổ xưa ẩn giấu tại Học viện Aurelia.",
                    5
                )
            },
            {
                "correct_100", new PhysicsAchievementData(
                    "correct_100",
                    "100 câu trả lời chính xác",
                    "Tích lũy được 100 câu trả lời trắc nghiệm chính xác.",
                    6
                )
            },
            {
                "aurelia_master", new PhysicsAchievementData(
                    "aurelia_master",
                    "Bậc thầy Aurelia",
                    "Mở khóa toàn bộ 6 danh hiệu thành tích trước, đạt danh hiệu Bậc thầy Vật lý.",
                    7
                )
            }
        };

        public static PhysicsAchievementData Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return _achievements.TryGetValue(id, out var data) ? data : null;
        }

        public static List<PhysicsAchievementData> GetAll()
        {
            var list = new List<PhysicsAchievementData>(_achievements.Values);
            list.Sort((a, b) => a.orderIndex.CompareTo(b.orderIndex));
            return list;
        }

        public static int TotalCount => _achievements.Count;
    }
}
