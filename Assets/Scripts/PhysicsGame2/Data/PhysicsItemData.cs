using System;
using System.Collections.Generic;

namespace ScienceQuest.PhysicsGame2
{
    [Serializable]
    public class PhysicsItemData
    {
        public string id;
        public string displayName;
        public string description;
        public string chapterAssociation;
        public string sourceHint;

        public PhysicsItemData(string id, string displayName, string description, string chapterAssociation, string sourceHint)
        {
            this.id = id;
            this.displayName = displayName;
            this.description = description;
            this.chapterAssociation = chapterAssociation;
            this.sourceHint = sourceHint;
        }

        // =========================================================================
        // 12 VẬT PHẨM CHÍNH THỨC CỦA HỌC VIỆN AURELIA (PHÍCH VẬT LÝ 9)
        // =========================================================================
        private static readonly Dictionary<string, PhysicsItemData> _items = new Dictionary<string, PhysicsItemData>
        {
            {
                "electrical_box", new PhysicsItemData(
                    "electrical_box", "Hộp điện",
                    "Thiết bị phân phối và bảo vệ hệ thống mạch điện an toàn trong thí nghiệm.",
                    "Chương III – Điện",
                    "Phần thưởng chuyên đề Đoạn mạch điện"
                )
            },
            {
                "switch", new PhysicsItemData(
                    "switch", "Công tắc",
                    "Khí cụ dùng để đóng ngắt mạch điện thủ công một cách an toàn.",
                    "Chương III – Điện",
                    "Phần thưởng chuyên đề Mạch điện nối tiếp & song song"
                )
            },
            {
                "mirror", new PhysicsItemData(
                    "mirror", "Gương",
                    "Dụng cụ quang học dùng để phản xạ và định hướng các tia sáng.",
                    "Chương II – Ánh sáng",
                    "Phần thưởng chuyên đề Hiện tượng quang học"
                )
            },
            {
                "lens", new PhysicsItemData(
                    "lens", "Thấu kính",
                    "Khối chất trong suốt có khả năng hội tụ hoặc phân kì chùm sáng truyền qua.",
                    "Chương II – Ánh sáng",
                    "Phần thưởng chuyên đề Thấu kính"
                )
            },
            {
                "prism", new PhysicsItemData(
                    "prism", "Lăng kính",
                    "Khối thủy tinh đa diện dùng để tán sắc ánh sáng trắng thành dải màu cầu vồng.",
                    "Chương II – Ánh sáng",
                    "Phần thưởng chuyên đề Tán sắc ánh sáng"
                )
            },
            {
                "magnet", new PhysicsItemData(
                    "magnet", "Nam châm",
                    "Vật liệu từ tính tạo ra từ trường phục vụ thí nghiệm cảm ứng điện từ.",
                    "Chương IV – Điện từ",
                    "Phần thưởng chuyên đề Hiện tượng cảm ứng điện từ"
                )
            },
            {
                "turbine", new PhysicsItemData(
                    "turbine", "Turbine",
                    "Tua-bin biến đổi động năng của dòng khí hoặc nước thành cơ năng quay máy phát điện.",
                    "Chương V – Năng lượng với cuộc sống",
                    "Phần thưởng chuyên đề Thủy điện & Năng lượng gió"
                )
            },
            {
                "solar_panel", new PhysicsItemData(
                    "solar_panel", "Tấm pin mặt trời",
                    "Tấm quang điện biến đổi trực tiếp năng lượng bức xạ mặt trời thành dòng điện.",
                    "Chương V – Năng lượng với cuộc sống",
                    "Phần thưởng chuyên đề Năng lượng tái tạo"
                )
            },
            {
                "battery", new PhysicsItemData(
                    "battery", "Pin",
                    "Nguồn điện hóa học một chiều cung cấp hiệu điện thế ổn định cho mạch dẫn.",
                    "Chương III – Điện",
                    "Phần thưởng chuyên đề Năng lượng của dòng điện"
                )
            },
            {
                "wire", new PhysicsItemData(
                    "wire", "Dây dẫn",
                    "Dây kim loại dẫn điện có điện trở suất thấp phục vụ kết nối mạch.",
                    "Chương III – Điện",
                    "Phần thưởng chuyên đề Định luật Ohm"
                )
            },
            {
                "gear", new PhysicsItemData(
                    "gear", "Bánh răng",
                    "Cơ cấu truyền động cơ học giúp thay đổi tốc độ và momen lực quay.",
                    "Chương I – Năng lượng cơ học",
                    "Phần thưởng chuyên đề Công và Công suất cơ học"
                )
            },
            {
                "energy_crystal", new PhysicsItemData(
                    "energy_crystal", "Tinh thể năng lượng",
                    "Bảo vật thần bí chứa nguồn năng lượng thuần khiết tối thượng của Học viện Aurelia.",
                    "Toàn bộ 5 Chương",
                    "Phần thưởng tối hậu khi hoàn thành toàn bộ khóa học"
                )
            }
        };

        public static PhysicsItemData Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return _items.TryGetValue(id, out var data) ? data : null;
        }

        public static List<PhysicsItemData> GetAll()
        {
            return new List<PhysicsItemData>(_items.Values);
        }

        public static IEnumerable<PhysicsItemData> AllItems => _items.Values;

        public static int TotalCount => _items.Count;
    }
}
