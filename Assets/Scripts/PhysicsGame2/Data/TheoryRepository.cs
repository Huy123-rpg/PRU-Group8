using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Repository cung cấp cấu trúc TheoryLessonData (nhiều trang) cho toàn bộ 16 bài học Vật lý 9 (KNTT Bài 2 -> 17).
    /// Hoạt động 100% offline, chuẩn hóa theo SGK KHTN 9 Kết nối tri thức với cuộc sống.
    /// </summary>
    public static class TheoryRepository
    {
        private static readonly Dictionary<string, TheoryLessonData> _lessons = new Dictionary<string, TheoryLessonData>(StringComparer.OrdinalIgnoreCase);
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;
            BuildCurriculumTheoryData();
            _initialized = true;
        }

        public static bool IsTestLesson(string lessonId)
        {
            if (string.IsNullOrEmpty(lessonId)) return false;
            string upper = lessonId.ToUpper().Trim();
            return upper.EndsWith("_TEST") || upper == "C1_TEST" || upper == "C2_TEST" || upper == "C3_TEST" || upper == "C4_TEST" || upper == "C5_TEST";
        }

        public static TheoryLessonData GetLesson(string lessonId)
        {
            if (!_initialized) Initialize();

            if (string.IsNullOrEmpty(lessonId)) return null;

            if (_lessons.TryGetValue(lessonId, out var data))
            {
                return data;
            }

            string normalized = NormalizeKey(lessonId);
            if (!string.IsNullOrEmpty(normalized) && _lessons.TryGetValue(normalized, out data))
            {
                return data;
            }

            return null;
        }

        private static string NormalizeKey(string id)
        {
            if (string.IsNullOrEmpty(id)) return "";
            string s = id.ToUpper().Trim();
            if (s == "C1_L2" || s == "L2" || s == "B2" || s == "BAI2" || s == "BÀI 2" || s == "BAI 2" || s.Contains("ĐỘNG NĂNG")) return "C1_L2";
            if (s == "C1_L3" || s == "L3" || s == "B3" || s == "BAI3" || s == "BÀI 3" || s == "BAI 3" || s.Contains("CƠ NĂNG")) return "C1_L3";
            if (s == "C1_L4" || s == "L4" || s == "B4" || s == "BAI4" || s == "BÀI 4" || s == "BAI 4" || s.Contains("CÔNG VÀ CÔNG SUẤT")) return "C1_L4";
            if (s == "C2_L5" || s == "L5" || s == "B5" || s == "BAI5" || s == "BÀI 5" || s == "BAI 5" || s.Contains("KHÚC XẠ")) return "C2_L5";
            if (s == "C2_L6" || s == "L6" || s == "B6" || s == "BAI6" || s == "BÀI 6" || s == "BAI 6" || s.Contains("PHẢN XẠ TOÀN PHẦN")) return "C2_L6";
            if (s == "C2_L7" || s == "L7" || s == "B7" || s == "BAI7" || s == "BÀI 7" || s == "BAI 7" || s.Contains("LĂNG KÍNH")) return "C2_L7";
            if (s == "C2_L8" || s == "L8" || s == "B8" || s == "BAI8" || s == "BÀI 8" || s == "BAI 8" || s.Contains("THẤU KÍNH")) return "C2_L8";
            if (s == "C2_L9" || s == "L9" || s == "B9" || s == "BAI9" || s == "BÀI 9" || s == "BAI 9" || s.Contains("ĐO TIÊU CỰ")) return "C2_L9";
            if (s == "C2_L10" || s == "L10" || s == "B10" || s == "BAI10" || s == "BÀI 10" || s == "BAI 10" || s.Contains("KÍNH LÚP")) return "C2_L10";
            if (s == "C3_L11" || s == "L11" || s == "B11" || s == "BAI11" || s == "BÀI 11" || s == "BAI 11" || s.Contains("ĐỊNH LUẬT OHM") || s.Contains("ĐIỆN TRỞ")) return "C3_L11";
            if (s == "C3_L12" || s == "L12" || s == "B12" || s == "BAI12" || s == "BÀI 12" || s == "BAI 12" || s.Contains("NỐI TIẾP") || s.Contains("SONG SONG")) return "C3_L12";
            if (s == "C3_L13" || s == "L13" || s == "B13" || s == "BAI13" || s == "BÀI 13" || s == "BAI 13" || s.Contains("CÔNG SUẤT ĐIỆN") || s.Contains("NĂNG LƯỢNG CỦA DÒNG ĐIỆN")) return "C3_L13";
            if (s == "C4_L14" || s == "L14" || s == "B14" || s == "BAI14" || s == "BÀI 14" || s == "BAI 14" || s.Contains("CẢM ỨNG ĐIỆN TỪ") || s.Contains("DÒNG ĐIỆN XOAY CHIỀU")) return "C4_L14";
            if (s == "C4_L15" || s == "L15" || s == "B15" || s == "BAI15" || s == "BÀI 15" || s == "BAI 15" || s.Contains("TÁC DỤNG CỦA DÒNG ĐIỆN")) return "C4_L15";
            if (s == "C5_L16" || s == "L16" || s == "B16" || s == "BAI16" || s == "BÀI 16" || s == "BAI 16" || s.Contains("VÒNG NĂNG LƯỢNG") || s.Contains("HÓA THẠCH")) return "C5_L16";
            if (s == "C5_L17" || s == "L17" || s == "B17" || s == "BAI17" || s == "BÀI 17" || s == "BAI 17" || s.Contains("TÁI TẠO")) return "C5_L17";
            return s;
        }

        private static void BuildCurriculumTheoryData()
        {
            _lessons.Clear();

            // ==========================================
            // CHƯƠNG I: NĂNG LƯỢNG CƠ HỌC
            // ==========================================

            // C1_L2: Bài 2. Động năng. Thế năng
            var c1_l2 = new TheoryLessonData("C1_L2", "Chương I. Năng lượng cơ học", "Bài 2: Động năng. Thế năng");
            c1_l2.AddPage(
                "ĐỘNG NĂNG LÀ GÌ?",
                "Động năng là dạng năng lượng mà một vật có được do nó đang chuyển động.\n\n" +
                "Vật có khối lượng càng lớn và chuyển động càng nhanh thì động năng của vật càng lớn.\n\n" +
                "Công thức tính động năng:\n" +
                "Wđ = 1/2 * m * v²\n\n" +
                "Trong đó: m là khối lượng của vật (kg), v là tốc độ của vật (m/s), Wđ là động năng (Jun - J).",
                "• Động năng sinh ra do vật đang chuyển động.\n• Tỉ lệ thuận với khối lượng m và bình phương tốc độ v²."
            );
            c1_l2.AddPage(
                "THẾ NĂNG TRỌNG TRƯỜNG",
                "Thế năng trọng trường là dạng năng lượng mà một vật có được khi nó ở một độ cao nhất định so với mặt đất (hoặc vật được chọn làm mốc thế năng).\n\n" +
                "Vật có khối lượng càng lớn và ở vị trí càng cao so với mốc thì thế năng trọng trường càng lớn.\n\n" +
                "Công thức tính thế năng:\n" +
                "Wt = m * g * h\n\n" +
                "Trong đó: m là khối lượng (kg), h là độ cao so với mốc (m), g là gia tốc trọng trường, Wt là thế năng (J).",
                "• Thế năng phụ thuộc vào khối lượng m và độ cao h.\n• Tại mốc thế năng được chọn (thường là mặt đất) thì Wt = 0."
            );
            c1_l2.AddPage(
                "ỨNG DỤNG VÀ THỰC TẾ",
                "1. Nhà máy thủy điện: Nước tích ở hồ chứa trên cao mang thế năng trọng trường lớn. Khi xả van, nước chảy xuống biến thế năng thành động năng làm quay turbine máy phát điện.\n\n" +
                "2. An toàn giao thông: Xe chạy tốc độ cao mang động năng rất lớn. Khi hãm phanh cần quãng đường dài hơn rất nhiều, va chạm mạnh hơn.",
                "• Thế năng dòng nước trên cao sinh công phát điện.\n• Tốc độ tăng gấp 2 lần thì động năng tăng gấp 4 lần."
            );
            _lessons["C1_L2"] = c1_l2;

            // C1_L3: Bài 3. Cơ năng
            var c1_l3 = new TheoryLessonData("C1_L3", "Chương I. Năng lượng cơ học", "Bài 3: Cơ năng");
            c1_l3.AddPage(
                "CƠ NĂNG VÀ BẢO TOÀN",
                "Cơ năng của một vật là tổng của động năng và thế năng:\n" +
                "W = Wđ + Wt = 1/2 * m * v² + m * g * h\n\n" +
                "Định luật bảo toàn cơ năng: Khi một vật chuyển động trong trọng trường chỉ chịu tác dụng của trọng lực (bỏ qua mọi ma sát và lực cản), cơ năng của vật luôn được bảo toàn (không đổi theo thời gian).",
                "• W = Wđ + Wt = hằng số.\n• Trong quá trình chuyển động, động năng tăng thì thế năng giảm và ngược lại."
            );
            c1_l3.AddPage(
                "CHUYỂN HÓA CƠ NĂNG",
                "Trong chuyển động dưới tác dụng của trọng lực:\n\n" +
                "- Khi vật rơi từ trên cao xuống: Độ cao giảm nên thế năng giảm, tốc độ tăng nên động năng tăng (thế năng chuyển hóa thành động năng).\n" +
                "- Khi ném vật lên cao: Độ cao tăng nên thế năng tăng, tốc độ giảm nên động năng giảm (động năng chuyển hóa thành thế năng).\n\n" +
                "- Tại điểm cao nhất: v = 0 -> W = Wt_max = m * g * h_max.\n" +
                "- Tại mặt đất (h = 0): W = Wđ_max = 1/2 * m * v_max².",
                "• Thế năng và động năng chuyển hóa qua lại lẫn nhau.\n• Tại vị trí cao nhất W = Wt_max, tại vị trí thấp nhất W = Wđ_max."
            );
            c1_l3.AddPage(
                "DAO ĐỘNG CON LẮC VÀ THỰC TẾ",
                "Con lắc dao động:\n" +
                "- Tại hai biên: Vật ở vị trí cao nhất, tốc độ bằng 0 nên thế năng cực đại, động năng bằng 0.\n" +
                "- Qua vị trí cân bằng: Vị trí thấp nhất, tốc độ lớn nhất nên động năng cực đại, thế năng cực tiểu.\n\n" +
                "Trong thực tế có ma sát và lực cản không khí, một phần cơ năng chuyển hóa dần thành nhiệt năng khiến dao động tắt dần.",
                "• Con lắc qua vị trí cân bằng có động năng lớn nhất.\n• Có ma sát: Một phần cơ năng chuyển hóa thành nhiệt năng."
            );
            _lessons["C1_L3"] = c1_l3;

            // C1_L4: Bài 4. Công và công suất
            var c1_l4 = new TheoryLessonData("C1_L4", "Chương I. Năng lượng cơ học", "Bài 4: Công và công suất");
            c1_l4.AddPage(
                "CÔNG CƠ HỌC",
                "Công cơ học chỉ được thực hiện khi có lực tác dụng vào vật và làm vật dịch chuyển.\n\n" +
                "Khi lực F cùng hướng với hướng chuyển dời của vật:\n" +
                "A = F * s\n\n" +
                "Trong đó:\n" +
                "- F: Độ lớn lực tác dụng (Niutơn - N).\n" +
                "- s: Quãng đường vật dịch chuyển (mét - m).\n" +
                "- A: Công cơ học (Jun - J). Đơn vị: 1 kJ = 1000 J.",
                "• Điều kiện sinh công: Có lực F tác dụng và có độ dịch chuyển s.\n• Lực vuông góc với phương dịch chuyển thì không sinh công."
            );
            c1_l4.AddPage(
                "CÔNG SUẤT",
                "Công suất là đại lượng đặc trưng cho tốc độ thực hiện công, được xác định bằng công thực hiện được trong một đơn vị thời gian.\n\n" +
                "Công thức tính công suất:\n" +
                "P = A / t\n\n" +
                "Trong đó: A là công thực hiện (J), t là thời gian thực hiện công (s), P là công suất (Oát - W).\n" +
                "Đơn vị công suất: Oát (W), 1 kW = 1000 W, 1 MW = 1.000.000 W.",
                "• P = A / t: Đại lượng đặc trưng cho tốc độ sinh công.\n• Cùng một công A, máy nào thực hiện trong thời gian ngắn hơn thì công suất lớn hơn."
            );
            c1_l4.AddPage(
                "HIỆU SUẤT",
                "Trong thực tế, khi máy móc hoạt động luôn có ma sát hao phí nên công toàn phần luôn lớn hơn công có ích.\n\n" +
                "Công thức tính hiệu suất:\n" +
                "H = (A_ích / A_tp) * 100% = (P_ích / P_tp) * 100%\n\n" +
                "Định luật về công: Không một máy cơ đơn giản nào cho ta lợi về công. Được lợi bao nhiêu lần về lực thì thiệt bấy nhiêu lần về đường đi và ngược lại.",
                "• Hiệu suất H = (A_ích / A_tp) * 100% (luôn nhỏ hơn 100%).\n• Máy cơ đơn giản không cho lợi về công."
            );
            _lessons["C1_L4"] = c1_l4;

            // ==========================================
            // CHƯƠNG II: ÁNH SÁNG
            // ==========================================

            // C2_L5: Bài 5. Khúc xạ ánh sáng
            var c2_l5 = new TheoryLessonData("C2_L5", "Chương II. Ánh sáng", "Bài 5: Khúc xạ ánh sáng");
            c2_l5.AddPage(
                "HIỆN TƯỢNG KHÚC XẠ",
                "Khúc xạ ánh sáng là hiện tượng tia sáng bị gãy khúc tại mặt phân cách giữa hai môi trường trong suốt khác nhau khi truyền xiên góc.\n\n" +
                "Các khái niệm:\n" +
                "- Điểm tới (I): Điểm gặp nhau giữa tia sáng tới và mặt phân cách.\n" +
                "- Pháp tuyến: Đường thẳng vuông góc với mặt phân cách tại điểm tới.\n" +
                "- Góc tới (i): Góc giữa tia tới và pháp tuyến; Góc khúc xạ (r): Góc giữa tia khúc xạ và pháp tuyến.",
                "• Tia khúc xạ nằm trong mặt phẳng tới và ở phía bên kia pháp tuyến so với tia tới.\n• Ánh sáng truyền xiên góc qua 2 môi trường trong suốt bị gãy khúc."
            );
            c2_l5.AddPage(
                "ĐỊNH LUẬT KHÚC XẠ",
                "Định luật khúc xạ ánh sáng:\n" +
                "n1 * sin(i) = n2 * sin(r)\n\n" +
                "Mối quan hệ giữa góc tới i và góc khúc xạ r:\n" +
                "- Khi truyền từ môi trường kém chiết quang sang môi trường chiết quang hơn (n1 < n2, ví dụ từ không khí vào nước): Góc khúc xạ nhỏ hơn góc tới (r < i), tia khúc xạ lại gần pháp tuyến.\n" +
                "- Khi truyền từ môi trường chiết quang hơn sang kém chiết quang hơn (n1 > n2, ví dụ từ nước ra không khí): Góc khúc xạ lớn hơn góc tới (r > i), tia khúc xạ lệch xa pháp tuyến.",
                "• n1 < n2: r < i (tia khúc xạ lại gần pháp tuyến).\n• n1 > n2: r > i (tia khúc xạ lệch xa pháp tuyến)."
            );
            c2_l5.AddPage(
                "HIỆN TƯỢNG THỰC TẾ",
                "1. Ống hút trong ly nước: Trông như bị gãy khúc tại mặt phân cách nước và không khí do tia sáng từ ống hút dưới nước bị khúc xạ khi ra ngoài không khí đến mắt người quan sát.\n\n" +
                "2. Đáy bể bơi: Khi nhìn từ trên bờ xuống, đáy bể bơi dường như nông hơn độ sâu thực tế do ảnh ảo của đáy bể bị nâng lên cao.",
                "• Tia sáng bị khúc xạ tạo ra ảnh ảo của vật trong nước nâng lên cao hơn vị trí thật.\n• Mắt nhìn thấy ảnh ảo chứ không phải vật thật."
            );
            _lessons["C2_L5"] = c2_l5;

            // C2_L6: Bài 6. Phản xạ toàn phần
            var c2_l6 = new TheoryLessonData("C2_L6", "Chương II. Ánh sáng", "Bài 6: Phản xạ toàn phần");
            c2_l6.AddPage(
                "PHẢN XẠ TOÀN PHẦN LÀ GÌ?",
                "Phản xạ toàn phần là hiện tượng toàn bộ tia sáng bị phản xạ trở lại môi trường cũ khi ánh sáng truyền từ môi trường có chiết suất lớn sang môi trường có chiết suất nhỏ và góc tới lớn hơn góc giới hạn.",
                "• Ánh sáng đi từ n1 lớn sang n2 nhỏ.\n• Có điều kiện về góc tới."
            );
            c2_l6.AddPage(
                "ĐIỀU KIỆN XẢY RA",
                "Điều kiện thứ nhất:\nn1 > n2\n\nĐiều kiện thứ hai:\ni > i_gh",
                "Phải thỏa mãn đồng thời cả hai điều kiện."
            );
            c2_l6.AddPage(
                "GÓC GIỚI HẠN",
                "Khi góc khúc xạ bằng 90°, góc tới tương ứng được gọi là góc giới hạn i_gh.\n\nsin(i_gh) = n2 / n1\n\nvới n1 > n2.",
                "i = i_gh:\ntia khúc xạ đi sát mặt phân cách."
            );
            c2_l6.AddPage(
                "BA TRƯỜNG HỢP",
                "i < i_gh:\nvẫn có tia khúc xạ.\n\ni = i_gh:\ntia khúc xạ sát mặt phân cách.\n\ni > i_gh:\nxảy ra phản xạ toàn phần.",
                "Phản xạ toàn phần xảy ra khi i > i_gh."
            );
            c2_l6.AddPage(
                "TÓM TẮT VÀ ỨNG DỤNG",
                "Phản xạ toàn phần là một hiện tượng quang học quan trọng, được ứng dụng trong sợi quang thông tin (Internet cáp quang) và ống nội soi trong y học.",
                "1. n1 > n2\n2. i > i_gh\n3. sin(i_gh) = n2 / n1"
            );
            _lessons["C2_L6"] = c2_l6;

            // C2_L7: Bài 7. Lăng kính
            var c2_l7 = new TheoryLessonData("C2_L7", "Chương II. Ánh sáng", "Bài 7: Lăng kính");
            c2_l7.AddPage(
                "CẤU TẠO LĂNG KÍNH",
                "Lăng kính là một khối chất trong suốt, đồng tính (thường bằng thủy tinh hoặc nhựa trong), có dạng hình lăng trụ tam giác.\n\n" +
                "Các phần tử hình học của lăng kính:\n" +
                "- Hai mặt bên: Các mặt phẳng đón tia sáng tới và cho tia sáng ló ra.\n" +
                "- Đáy: Mặt đối diện với góc chiết quang.\n" +
                "- Góc chiết quang A: Góc tạo bởi hai mặt bên của lăng kính.",
                "• Lăng kính là khối lăng trụ tam giác trong suốt.\n• Tia sáng đơn sắc khi đi qua lăng kính luôn bị lệch về phía đáy."
            );
            c2_l7.AddPage(
                "TÁN SẮC ÁNH SÁNG",
                "Khi chiếu một chùm ánh sáng trắng hẹp qua lăng kính, chùm sáng bị tách thành một dải màu cầu vồng biến thiên liên tục từ Đỏ đến Tím.\n\n" +
                "- Ánh sáng trắng là tập hợp của vô số ánh sáng đơn sắc khác nhau.\n" +
                "- Tia đỏ bị lệch ít nhất, tia tím bị lệch nhiều nhất do chiết suất của thủy tinh đối với ánh sáng tím lớn hơn ánh sáng đỏ (n_tím > n_đỏ).",
                "• Hiện tượng tán sắc phân tách chùm sáng trắng thành nhiều màu đơn sắc.\n• Tia đỏ lệch ít nhất, tia tím lệch nhiều nhất."
            );
            c2_l7.AddPage(
                "ỨNG DỤNG CỦA LĂNG KÍNH",
                "1. Máy quang phổ lăng kính: Phân tích chùm ánh sáng phức tạp thành các thành phần đơn sắc để xác định thành phần hóa học và nhiệt độ nguồn sáng.\n\n" +
                "2. Cầu vồng tự nhiên: Sau cơn mưa, các giọt nước li ti trong không khí đóng vai trò như các lăng kính tự nhiên tán sắc ánh sáng Mặt Trời tạo thành dải cầu vồng nhiều màu sắc.",
                "• Máy quang phổ dùng để phân tích quang phổ ánh sáng.\n• Cầu vồng là hiện tượng tán sắc qua các giọt nước mưa."
            );
            _lessons["C2_L7"] = c2_l7;

            // C2_L8: Bài 8. Thấu kính
            var c2_l8 = new TheoryLessonData("C2_L8", "Chương II. Ánh sáng", "Bài 8: Thấu kính");
            c2_l8.AddPage(
                "PHÂN LOẠI THẤU KÍNH",
                "Thấu kính là khối chất trong suốt giới hạn bởi hai mặt cong hoặc một mặt phẳng và một mặt cong:\n\n" +
                "1. Thấu kính hội tụ (rìa mỏng): Chùm tia tới song song với trục chính cho chùm tia ló hội tụ tại tiêu điểm ảnh F' phía sau thấu kính.\n\n" +
                "2. Thấu kính phân kì (rìa dày): Chùm tia tới song song với trục chính cho chùm tia ló phân kì (loe rộng ra), đường kéo dài của tia ló cắt nhau tại tiêu điểm ảnh F'.",
                "• Thấu kính hội tụ: Phần rìa mỏng hơn phần giữa.\n• Thấu kính phân kì: Phần rìa dày hơn phần giữa."
            );
            c2_l8.AddPage(
                "CÁC TIA SÁNG ĐẶC BIỆT",
                "Đường truyền của 3 tia sáng đặc biệt qua thấu kính hội tụ:\n\n" +
                "1. Tia tới đi qua quang tâm O: Tiếp tục truyền thẳng không đổi hướng.\n" +
                "2. Tia tới song song với trục chính: Cho tia ló đi qua tiêu điểm ảnh F'.\n" +
                "3. Tia tới đi qua tiêu điểm vật F: Cho tia ló song song với trục chính.\n\n" +
                "Với thấu kính phân kì, tia tới song song trục chính cho tia ló có đường kéo dài đi qua tiêu điểm ảnh.",
                "• Tia qua quang tâm O luôn truyền thẳng.\n• Tia song song trục chính cho tia ló (hoặc đường kéo dài) qua tiêu điểm ảnh."
            );
            c2_l8.AddPage(
                "ĐẶC ĐIỂM TẠO ẢNH CỦA THẤU KÍNH",
                "Phân biệt rõ đặc điểm tạo ảnh của từng loại thấu kính:\n\n" +
                "1. Thấu kính hội tụ:\n" +
                "- Khi d > f: Có thể tạo ảnh thật ở phía bên kia thấu kính (ngược chiều với vật).\n" +
                "- Khi d < f: Tạo ảnh ảo, cùng chiều và lớn hơn vật.\n\n" +
                "2. Thấu kính phân kì:\n" +
                "- Với vật thật đặt ở mọi vị trí trước thấu kính: Luôn cho ảnh ảo, cùng chiều và nhỏ hơn vật, nằm trong khoảng tiêu cự của thấu kính.",
                "• Thấu kính hội tụ: d > f tạo ảnh thật ngược chiều; d < f tạo ảnh ảo lớn hơn vật.\n• Thấu kính phân kì: Với vật thật luôn tạo ảnh ảo, cùng chiều và nhỏ hơn vật."
            );
            _lessons["C2_L8"] = c2_l8;

            // C2_L9: Bài 9. Thực hành đo tiêu cự của thấu kính hội tụ
            var c2_l9 = new TheoryLessonData("C2_L9", "Chương II. Ánh sáng", "Bài 9: Thực hành đo tiêu cự của thấu kính hội tụ");
            c2_l9.AddPage(
                "NGUYÊN TẮC ĐO TIÊU CỰ",
                "Phương pháp đo tiêu cự thấu kính hội tụ dựa trên tính chất ảnh thật bằng vật:\n\n" +
                "Khi đặt vật sáng vuông góc với trục chính và dịch chuyển màn hứng ảnh sao cho thu được ảnh thật rõ nét, ngược chiều và có kích thước bằng đúng vật thật, ta có:\n" +
                "- Vật và màn cách đều thấu kính: d = d' = 2f.\n" +
                "- Khoảng cách giữa vật và màn hứng ảnh: L = d + d' = 4f.",
                "• Khi ảnh thật rõ nét bằng đúng kích thước vật: d = d' = 2f.\n• Khoảng cách từ vật sáng đến màn ảnh: L = 4f."
            );
            c2_l9.AddPage(
                "CÔNG THỨC VÀ TIẾN HÀNH",
                "Công thức tính tiêu cự thực hành:\n" +
                "f = L / 4 = (d + d') / 4\n\n" +
                "Các bước tiến hành thí nghiệm:\n" +
                "1. Bố trí vật sáng, thấu kính hội tụ và màn chắn trên giá quang học.\n" +
                "2. Dịch chuyển đồng thời thấu kính và màn để tìm vị trí cho ảnh thật rõ nét nhất và bằng vật.\n" +
                "3. Đo khoảng cách L giữa vật và màn, tính tiêu cự f = L / 4. Lặp lại đo 3 lần để lấy giá trị trung bình.",
                "• Tiêu cự thấu kính: f = L / 4.\n• Đo nhiều lần và lấy giá trị trung bình để giảm sai số phép đo."
            );
            c2_l9.AddPage(
                "LƯU Ý THỰC NGHIỆM",
                "Khi tiến hành thí nghiệm đo tiêu cự:\n\n" +
                "1. Bố trí đồng trục: Vật sáng, tâm của thấu kính và tâm của màn hứng ảnh phải nằm thẳng hàng trên cùng trục quang học.\n\n" +
                "2. Tinh chỉnh độ nét: Phải dịch chuyển màn chắn từ từ từng chút một để ảnh trên màn đạt độ sắc nét tối đa trước khi đọc và ghi nhận khoảng cách L.",
                "• Giữ vật sáng, thấu kính và màn chắn luôn đồng trục quang học.\n• Quan sát vị trí ảnh rõ nét nhất để kết quả đo chính xác."
            );
            _lessons["C2_L9"] = c2_l9;

            // C2_L10: Bài 10. Kính lúp. Bài tập thấu kính
            var c2_l10 = new TheoryLessonData("C2_L10", "Chương II. Ánh sáng", "Bài 10: Kính lúp. Bài tập thấu kính");
            c2_l10.AddPage(
                "KÍNH LÚP LÀ GÌ?",
                "Kính lúp là một thấu kính hội tụ có tiêu cự ngắn (thường từ vài cm đến khoảng 10 cm), được dùng để quan sát các vật nhỏ.\n\n" +
                "Nguyên tắc sử dụng kính lúp:\n" +
                "- Đặt vật cần quan sát trong khoảng tiêu cự của kính lúp (d < f).\n" +
                "- Kính lúp cho ảnh ảo, cùng chiều và lớn hơn vật nằm trong giới hạn nhìn rõ của mắt.",
                "• Kính lúp là thấu kính hội tụ có tiêu cự ngắn f.\n• Đặt vật trong khoảng tiêu cự (d < f) để quan sát được ảnh ảo phóng đại."
            );
            c2_l10.AddPage(
                "SỐ BỘI GIÁC CỦA KÍNH LÚP",
                "Số bội giác (ký hiệu G) đặc trưng cho khả năng phóng đại ảnh của kính lúp đối với mắt người quan sát.\n\n" +
                "Công thức tính số bội giác khi ngắm chừng ở vô cực:\n" +
                "G = 25 / f\n\n" +
                "Trong đó: f là tiêu cự của kính lúp tính bằng xentimét (cm); 25 cm là khoảng cách nhìn rõ ngắn nhất của mắt bình thường.\n" +
                "Ví dụ: Kính lúp có tiêu cự f = 5 cm có số bội giác G = 25 / 5 = 5X.",
                "• G = 25 / f (tiêu cự f tính bằng đơn vị cm).\n• Tiêu cự f càng ngắn thì số bội giác G của kính lúp càng lớn."
            );
            c2_l10.AddPage(
                "BÀI TẬP VẬN DỤNG",
                "Phương pháp giải bài tập kính lúp và thấu kính:\n\n" +
                "- Xác định tiêu cự từ số bội giác: f = 25 / G (cm).\n" +
                "- Khi ngắm chừng ở vô cực: Vật đặt đúng tại tiêu điểm của kính lúp (d = f), mắt người quan sát không phải điều tiết nên không bị mỏi.\n" +
                "- Áp dụng mối quan hệ hình học các tam giác đồng dạng để xác định khoảng cách và chiều cao của ảnh.",
                "• Ngắm chừng ở vô cực: d = f, mắt không phải điều tiết mỏi.\n• Đọc kĩ thông số bội giác ghi trên vành kính (ví dụ: 2.5X, 5X, 10X)."
            );
            _lessons["C2_L10"] = c2_l10;

            // ==========================================
            // CHƯƠNG III: ĐIỆN
            // ==========================================

            // C3_L11: Bài 11. Điện trở. Định luật Ohm
            var c3_l11 = new TheoryLessonData("C3_L11", "Chương III. Điện", "Bài 11: Điện trở. Định luật Ohm");
            c3_l11.AddPage(
                "ĐỊNH LUẬT OHM",
                "Điện trở (R): Đại lượng đặc trưng cho mức độ cản trở dòng điện của vật dẫn. Đơn vị: Ôm (kí hiệu Ω).\n\n" +
                "Định luật Ohm: Cường độ dòng điện chạy qua dây dẫn tỉ lệ thuận với hiệu điện thế đặt vào hai đầu dây và tỉ lệ nghịch với điện trở của dây.\n\n" +
                "Công thức: I = U / R  =>  U = I * R  =>  R = U / I\n\n" +
                "Trong đó: U là hiệu điện thế (V), I là cường độ dòng điện (A), R là điện trở (Ω).",
                "• I = U / R: Hiệu điện thế tăng thì I tăng; điện trở tăng thì I giảm.\n• Đơn vị điện trở là Ôm (Ω), 1 kΩ = 1000 Ω, 1 MΩ = 1.000.000 Ω."
            );
            c3_l11.AddPage(
                "ĐIỆN TRỞ DÂY DẪN",
                "Điện trở của một đoạn dây dẫn hình trụ phụ thuộc vào chiều dài, tiết diện và vật liệu làm dây:\n\n" +
                "Công thức: R = ρ * (l / S)\n\n" +
                "Trong đó:\n" +
                "- ρ: Điện trở suất của vật liệu làm dây dẫn (Ω.m).\n" +
                "- l: Chiều dài đoạn dây dẫn (m).\n" +
                "- S: Tiết diện của đoạn dây dẫn (m²).\n" +
                "Dây càng dài thì điện trở càng lớn; dây có tiết diện càng to thì điện trở càng nhỏ.",
                "• R = ρ * l / S.\n• Chiều dài dây l tăng thì R tăng; tiết diện S tăng thì R giảm."
            );
            c3_l11.AddPage(
                "BIẾN TRỞ TRONG MẠCH ĐIỆN",
                "Biến trở là một điện trở có thể thay đổi trị số, dùng để điều chỉnh cường độ dòng điện trong mạch.\n\n" +
                "Cấu tạo: Gồm một cuộn dây có điện trở suất lớn quấn quanh lõi cách điện và một con chạy trượt tiếp xúc dọc cuộn dây.\n\n" +
                "Khi dịch chuyển con chạy, chiều dài phần dây dẫn có dòng điện chạy qua thay đổi, dẫn đến điện trở thay đổi và dòng điện trong mạch được điều chỉnh.",
                "• Biến trở dùng để điều chỉnh cường độ dòng điện trong mạch.\n• Dịch chuyển con chạy làm thay đổi chiều dài phần dây dẫn dẫn điện."
            );
            _lessons["C3_L11"] = c3_l11;

            // C3_L12: Bài 12. Đoạn mạch nối tiếp, song song
            var c3_l12 = new TheoryLessonData("C3_L12", "Chương III. Điện", "Bài 12: Đoạn mạch nối tiếp, song song");
            c3_l12.AddPage(
                "ĐOẠN MẠCH NỐI TIẾP",
                "Trong đoạn mạch gồm các điện trở mắc nối tiếp:\n\n" +
                "1. Cường độ dòng điện như nhau tại mọi vị trí: I = I1 = I2 = ... = In.\n" +
                "2. Hiệu điện thế giữa hai đầu đoạn mạch bằng tổng các hiệu điện thế thành phần: U = U1 + U2 + ... + Un.\n" +
                "3. Điện trở tương đương bằng tổng các điện trở thành phần: Rtd = R1 + R2 + ... + Rn.\n\n" +
                "Điện trở tương đương của mạch nối tiếp luôn lớn hơn từng điện trở thành phần.",
                "• Nối tiếp: I bằng nhau, U cộng lại, Rtd cộng lại.\n• Rtd = R1 + R2 (luôn lớn hơn từng điện trở thành phần)."
            );
            c3_l12.AddPage(
                "ĐOẠN MẠCH SONG SONG",
                "Trong đoạn mạch gồm các điện trở mắc song song:\n\n" +
                "1. Hiệu điện thế bằng nhau ở hai đầu mỗi nhánh: U = U1 = U2 = ... = Un.\n" +
                "2. Cường độ dòng điện mạch chính bằng tổng dòng điện các nhánh: I = I1 + I2 + ... + In.\n" +
                "3. Nghịch đảo điện trở tương đương: 1/Rtd = 1/R1 + 1/R2.\n" +
                "Với hai điện trở mắc song song: Rtd = (R1 * R2) / (R1 + R2).",
                "• Song song: U bằng nhau, I cộng lại.\n• Hai điện trở song song: Rtd = (R1 * R2) / (R1 + R2) (nhỏ hơn từng R thành phần)."
            );
            c3_l12.AddPage(
                "ỨNG DỤNG MẠNG ĐIỆN GIA ĐÌNH",
                "Trong mạng điện gia đình, các thiết bị điện (đèn, quạt, tivi, tủ lạnh...) luôn được mắc SONG SONG với nhau để:\n" +
                "- Mỗi thiết bị đều nhận đúng hiệu điện thế định mức 220 V.\n" +
                "- Các thiết bị hoạt động độc lập, khi tắt một thiết bị thì các thiết bị khác vẫn hoạt động bình thường.\n\n" +
                "Cầu chì, công tắc và aptomat được mắc NỐI TIẾP với thiết bị để bảo vệ khi xảy ra quá tải hoặc ngắn mạch.",
                "• Thiết bị tiêu thụ điện gia đình mắc song song để dùng chung hiệu điện thế 220 V.\n• Thiết bị bảo vệ và công tắc mắc nối tiếp."
            );
            _lessons["C3_L12"] = c3_l12;

            // C3_L13: Bài 13. Năng lượng của dòng điện và công suất điện (Chuẩn hóa KNTT: 4 trang)
            var c3_l13 = new TheoryLessonData("C3_L13", "Chương III. Điện", "Bài 13: Năng lượng dòng điện & Công suất điện");
            c3_l13.AddPage(
                "NĂNG LƯỢNG ĐIỆN",
                "Dòng điện có năng lượng vì nó có khả năng sinh công và biến đổi thành các dạng năng lượng khác (nhiệt năng, quang năng, cơ năng).\n\n" +
                "Năng lượng điện tiêu thụ của một đoạn mạch được xác định theo công thức:\n" +
                "W = U * I * t\n\n" +
                "Trong đó:\n" +
                "- W: Năng lượng điện tiêu thụ (Jun - J).\n" +
                "- U: Hiệu điện thế giữa hai đầu đoạn mạch (Vôn - V).\n" +
                "- I: Cường độ dòng điện chạy qua mạch (Ampe - A).\n" +
                "- t: Thời gian dòng điện chạy qua mạch (giây - s).",
                "• Dòng điện mang năng lượng điện và có thể chuyển hóa sang dạng năng lượng khác.\n• Công thức tính năng lượng điện: W = U * I * t (đơn vị Jun - J)."
            );
            c3_l13.AddPage(
                "CÔNG SUẤT ĐIỆN",
                "Công suất điện của một đoạn mạch đặc trưng cho tốc độ tiêu thụ năng lượng điện của đoạn mạch đó, xác định bằng năng lượng điện tiêu thụ trong một đơn vị thời gian.\n\n" +
                "Công thức tính công suất điện:\n" +
                "P = W / t  và  P = U * I\n\n" +
                "Trong đó: P là công suất điện, đơn vị đo là Oát (Watt, kí hiệu là W). 1 kW = 1000 W.\n\n" +
                "Lưu ý: Cần phân biệt kí hiệu đại lượng năng lượng điện W (đo bằng J) với kí hiệu đơn vị công suất W (Oát).",
                "• P = W / t = U * I: Đại lượng đặc trưng cho tốc độ tiêu thụ năng lượng điện.\n• Đơn vị đo công suất là Oát (Watt - W), 1 kW = 1000 W."
            );
            c3_l13.AddPage(
                "CÔNG SUẤT ĐỊNH MỨC",
                "Trên mỗi thiết bị điện thường ghi các giá trị định mức. Ví dụ trên bóng đèn ghi: 220 V - 60 W.\n\n" +
                "Ý nghĩa các thông số ghi trên thiết bị:\n" +
                "- 220 V: Hiệu điện thế định mức cần đặt vào hai đầu thiết bị để nó hoạt động bình thường.\n" +
                "- 60 W: Công suất định mức của thiết bị khi hoạt động đúng ở hiệu điện thế định mức 220 V.\n\n" +
                "Nếu dùng hiệu điện thế nhỏ hơn định mức, thiết bị sẽ hoạt động yếu hơn bình thường.",
                "• Số vôn ghi trên thiết bị là hiệu điện thế định mức.\n• Số oát ghi trên thiết bị là công suất định mức khi dùng đúng hiệu điện thế định mức."
            );
            c3_l13.AddPage(
                "ĐO ĐIỆN NĂNG TIÊU THỤ",
                "Lượng điện năng tiêu thụ trong gia đình được đo bằng công tơ điện (đồng hồ điện).\n\n" +
                "Đơn vị đo điện năng thông dụng trên công tơ điện là kilôoát-giờ (kWh), thường được gọi dân dã là 1 số điện.\n\n" +
                "Mối liên hệ giữa kilôoát-giờ và Jun:\n" +
                "1 kWh = 1000 W * 3600 s = 3.600.000 J (hay 3,6 * 10⁶ J).\n\n" +
                "Mỗi số đếm trên công tơ điện ứng với 1 kWh điện năng đã tiêu thụ.",
                "• Công tơ điện dùng để đo năng lượng điện tiêu thụ trong gia đình.\n• 1 số điện = 1 kWh = 3.600.000 J."
            );
            _lessons["C3_L13"] = c3_l13;

            // ==========================================
            // CHƯƠNG IV: ĐIỆN TỪ
            // ==========================================

            // C4_L14: Bài 14. Cảm ứng điện từ. Nguyên tắc tạo ra dòng điện xoay chiều
            var c4_l14 = new TheoryLessonData("C4_L14", "Chương IV. Điện từ", "Bài 14: Cảm ứng điện từ. Dòng điện xoay chiều");
            c4_l14.AddPage(
                "HIỆN TƯỢNG CẢM ỨNG ĐIỆN TỪ",
                "Hiện tượng cảm ứng điện từ là hiện tượng xuất hiện dòng điện cảm ứng trong một cuộn dây dẫn kín khi số đường sức từ xuyên qua tiết diện của cuộn dây biến thiên.\n\n" +
                "Các cách tạo ra dòng điện cảm ứng:\n" +
                "- Đưa nam châm lại gần hoặc ra xa cuộn dây dẫn kín.\n" +
                "- Cho nam châm quay trước cuộn dây hoặc cho cuộn dây quay trong từ trường.\n" +
                "- Đóng hoặc ngắt mạch điện của nam châm điện đặt gần cuộn dây.",
                "• Điều kiện xuất hiện dòng điện cảm ứng: Số đường sức từ qua tiết diện cuộn dây kín biến thiên.\n• Nếu nam châm và cuộn dây đứng yên so với nhau thì không có dòng điện cảm ứng."
            );
            c4_l14.AddPage(
                "DÒNG ĐIỆN XOAY CHIỀU (AC)",
                "Dòng điện xoay chiều (kí hiệu AC) là dòng điện có chiều luân phiên đổi chiều tuần hoàn theo thời gian.\n\n" +
                "Nguyên nhân đổi chiều:\n" +
                "Khi số đường sức từ qua tiết diện cuộn dây tăng thì dòng điện cảm ứng có một chiều; khi số đường sức từ giảm thì dòng điện cảm ứng đổi theo chiều ngược lại.\n\n" +
                "Đặc điểm tại Việt Nam: Dòng điện xoay chiều trong mạng điện sinh hoạt gia đình có tần số 50 Hz (đổi chiều 100 lần trong mỗi giây) và hiệu điện thế 220 V.",
                "• Dòng điện xoay chiều luân phiên đổi chiều tuần hoàn theo thời gian.\n• Điện lưới sinh hoạt ở Việt Nam có tần số 50 Hz, hiệu điện thế 220 V."
            );
            c4_l14.AddPage(
                "MÁY PHÁT ĐIỆN XOAY CHIỀU",
                "Máy phát điện xoay chiều hoạt động dựa trên hiện tượng cảm ứng điện từ, có tác dụng biến đổi cơ năng thành điện năng.\n\n" +
                "Cấu tạo gồm hai bộ phận chính:\n" +
                "1. Rôto: Bộ phận quay (thường là nam châm hoặc cuộn dây dẫn).\n" +
                "2. Stato: Bộ phận đứng yên (phần còn lại tương ứng).\n\n" +
                "Khi rôto quay, số đường sức từ xuyên qua các cuộn dây biến thiên liên tục, sinh ra dòng điện xoay chiều cấp cho mạch ngoài.",
                "• Cấu tạo chính gồm Rôto (phần quay) và Stato (phần đứng yên).\n• Hoạt động dựa trên hiện tượng cảm ứng điện từ, biến cơ năng thành điện năng."
            );
            _lessons["C4_L14"] = c4_l14;

            // C4_L15: Bài 15. Tác dụng của dòng điện xoay chiều (Chuẩn hóa KNTT: 4 trang trọng tâm tác dụng)
            var c4_l15 = new TheoryLessonData("C4_L15", "Chương IV. Điện từ", "Bài 15: Tác dụng của dòng điện xoay chiều");
            c4_l15.AddPage(
                "TÁC DỤNG NHIỆT",
                "Dòng điện xoay chiều khi chạy qua vật dẫn điện có thể làm cho vật dẫn đó nóng lên đến nhiệt độ cao.\n\n" +
                "Ví dụ thiết bị ứng dụng trong đời sống:\n" +
                "- Bàn là điện, ấm đun nước siêu tốc, nồi cơm điện, máy sấy tóc, bình nóng lạnh, bếp điện.\n\n" +
                "Tác dụng nhiệt của dòng điện xoay chiều không phụ thuộc vào chiều của dòng điện chạy qua vật dẫn, dòng điện chạy theo chiều nào thì dây dẫn cũng đều nóng lên.",
                "• Dòng điện xoay chiều chạy qua vật dẫn có thể làm vật dẫn nóng lên.\n• Ứng dụng: Bàn là, ấm đun nước, nồi cơm điện, bình nóng lạnh."
            );
            c4_l15.AddPage(
                "TÁC DỤNG PHÁT SÁNG",
                "Dòng điện xoay chiều có thể làm các thiết bị chiếu sáng và báo hiệu phát sáng.\n\n" +
                "Ví dụ thiết bị phù hợp SGK:\n" +
                "- Đèn sợi đốt: Dòng điện chạy qua làm dây tóc vonfram nóng sáng rực rỡ.\n" +
                "- Đèn huỳnh quang và đèn compact: Dòng điện kích thích hơi thủy ngân phát tia tử ngoại tác dụng lên bột huỳnh quang phát sáng.\n" +
                "- Đèn LED và bút thử điện: Phát sáng khi có dòng điện xoay chiều chạy qua.",
                "• Dòng điện xoay chiều có tác dụng phát sáng.\n• Ứng dụng: Đèn sợi đốt, đèn huỳnh quang, đèn LED, bút thử điện."
            );
            c4_l15.AddPage(
                "TÁC DỤNG TỪ",
                "Khi dòng điện xoay chiều chạy qua dây dẫn hoặc cuộn dây, xung quanh nó sinh ra từ trường.\n\n" +
                "Đặc điểm tác dụng từ của dòng điện xoay chiều:\n" +
                "- Cuộn dây có dòng điện xoay chiều chạy qua lõi sắt có khả năng hút các vật bằng sắt, thép.\n" +
                "- Do dòng điện xoay chiều luân phiên đổi chiều nên từ trường do nó sinh ra cũng biến đổi luân phiên theo thời gian.\n" +
                "- Ứng dụng: Chuông điện xoay chiều, động cơ điện xoay chiều, nam châm điện xoay chiều.",
                "• Dòng điện xoay chiều chạy qua cuộn dây tạo ra từ trường biến đổi.\n• Dòng điện xoay chiều có tác dụng từ: Hút sắt thép, làm quay nam châm."
            );
            c4_l15.AddPage(
                "TÁC DỤNG SINH LÍ VÀ AN TOÀN",
                "Dòng điện xoay chiều khi đi qua cơ thể người và động vật gây ra tác dụng sinh lí rất mạnh, làm co giật các cơ, tim ngừng đập, ngạt thở và tê liệt thần kinh, cực kỳ nguy hiểm đến tính mạng.\n\n" +
                "Quy tắc an toàn điện bắt buộc:\n" +
                "- Tuyệt đối không tiếp xúc trực tiếp với dây dẫn trần hoặc các cực của nguồn điện xoay chiều.\n" +
                "- Chỉ sử dụng các thiết bị có vỏ cách điện an toàn; ngắt aptomat trước khi sửa chữa điện.\n" +
                "- Khi phát hiện người bị điện giật: Phải lập tức ngắt cầu dao hoặc dùng vật cách điện khô tách nạn nhân ra khỏi nguồn điện.",
                "• Dòng điện xoay chiều có tác dụng sinh lí nguy hiểm đến tính mạng con người.\n• Tuyệt đối không tiếp xúc trực tiếp với nguồn điện xoay chiều; tuân thủ nghiêm ngặt quy tắc an toàn."
            );
            _lessons["C4_L15"] = c4_l15;

            // ==========================================
            // CHƯƠNG V: NĂNG LƯỢNG VỚI CUỘC SỐNG
            // ==========================================

            // C5_L16: Bài 16. Vòng năng lượng trên Trái Đất. Năng lượng hoá thạch (Chuẩn hóa KNTT: 4 trang)
            var c5_l16 = new TheoryLessonData("C5_L16", "Chương V. Năng lượng với cuộc sống", "Bài 16: Vòng năng lượng Trái Đất & Năng lượng hoá thạch");
            c5_l16.AddPage(
                "NGUỒN NĂNG LƯỢNG MẶT TRỜI",
                "Mặt Trời là nguồn cung cấp năng lượng sơ cấp quan trọng nhất cho hầu hết các vòng chuyển hóa năng lượng trên Trái Đất.\n\n" +
                "Vai trò của năng lượng Mặt Trời:\n" +
                "- Sưởi ấm bề mặt Trái Đất, làm nước ở các đại dương bay hơi tạo thành mây và mưa, duy trì vòng tuần hoàn của nước.\n" +
                "- Chiếu sáng và đốt nóng các vùng khí quyển không đều nhau, tạo nên sự chênh lệch áp suất sinh ra các dòng gió tự nhiên.",
                "• Năng lượng Mặt Trời là khởi nguồn của nhiều vòng chuyển hóa năng lượng trên Trái Đất.\n• Nhiệt Mặt Trời điều khiển vòng tuần hoàn nước và sinh ra gió."
            );
            c5_l16.AddPage(
                "VÒNG NĂNG LƯỢNG GIỮA CÁC VẬT SỐNG",
                "Trong thế giới sinh vật, năng lượng được chuyển hóa và truyền dẫn liên tục qua các bậc dinh dưỡng:\n\n" +
                "- Cây xanh hấp thụ năng lượng ánh sáng Mặt Trời thực hiện quá trình quang hợp, tổng hợp chất hữu cơ và tích lũy hóa năng.\n" +
                "- Động vật ăn thực vật lấy năng lượng từ cây xanh; động vật ăn thịt tiếp tục lấy năng lượng qua chuỗi thức ăn.\n" +
                "- Khi sinh vật chết đi, xác bị vi sinh vật phân giải, một phần năng lượng thoát ra môi trường dưới dạng nhiệt.",
                "• Thực vật quang hợp tích lũy năng lượng Mặt Trời mở đầu vòng năng lượng sinh vật.\n• Năng lượng truyền qua chuỗi và lưới thức ăn trong hệ sinh thái."
            );
            c5_l16.AddPage(
                "NGUỒN GỐC NHIÊN LIỆU HÓA THẠCH",
                "Nhiên liệu hóa thạch gồm than đá, dầu mỏ và khí thiên nhiên.\n\n" +
                "Nguồn gốc hình thành:\n" +
                "- Hàng trăm triệu năm trước, xác của các loài thực vật và sinh vật cổ đại bị vùi lấp dưới các lớp trầm tích dày trong lòng đất.\n" +
                "- Trải qua hàng triệu năm chịu tác động của áp suất và nhiệt độ cao trong điều kiện thiếu oxy, chúng dần biến đổi thành than đá, dầu mỏ và khí đốt.\n" +
                "- Về bản chất, năng lượng hóa thạch là năng lượng Mặt Trời cổ xưa được tích lũy lại.",
                "• Nhiên liệu hóa thạch (than đá, dầu mỏ, khí đốt) có nguồn gốc từ xác sinh vật cổ đại.\n• Quá trình hình thành đòi hỏi hàng triệu năm trong lòng đất."
            );
            c5_l16.AddPage(
                "TÍNH HỮU HẠN VÀ KHAI THÁC",
                "Đặc điểm của nguồn năng lượng hóa thạch:\n\n" +
                "- Tính hữu hạn: Thời gian hình thành kéo dài hàng triệu năm nhưng con người khai thác với tốc độ rất nhanh, khiến nguồn nhiên liệu này có nguy cơ cạn kiệt trong tương lai gần.\n" +
                "- Tác động môi trường: Đốt cháy nhiên liệu hóa thạch phát thải lượng lớn khí CO2 và các chất độc hại, gây ô nhiễm không khí và gia tăng hiệu ứng nhà kính.\n" +
                "- Cần sử dụng tiết kiệm, hiệu quả và tích cực tìm kiếm năng lượng thay thế.",
                "• Nhiên liệu hóa thạch là nguồn năng lượng không thể tái tạo và đang dần cạn kiệt.\n• Khai thác và sử dụng gây ô nhiễm môi trường, cần sử dụng tiết kiệm và hợp lý."
            );
            _lessons["C5_L16"] = c5_l16;

            // C5_L17: Bài 17. Một số dạng năng lượng tái tạo (Chuẩn hóa KNTT: 4 trang)
            var c5_l17 = new TheoryLessonData("C5_L17", "Chương V. Năng lượng với cuộc sống", "Bài 17: Một số dạng năng lượng tái tạo");
            c5_l17.AddPage(
                "NĂNG LƯỢNG TÁI TẠO LÀ GÌ?",
                "Năng lượng tái tạo là năng lượng từ các nguồn được bổ sung liên tục bởi các quá trình tự nhiên và có khả năng tái tạo trong khoảng thời gian phù hợp với quy mô sử dụng.\n\n" +
                "Khác biệt cơ bản so với nhiên liệu hóa thạch:\n" +
                "- Được tự nhiên phục hồi liên tục qua các chu trình tự nhiên của Trái Đất.\n" +
                "- Có thể khai thác lâu dài nếu được quản lý và sử dụng phù hợp.\n" +
                "- Phát triển năng lượng tái tạo là giải pháp bền vững thay thế dần năng lượng hóa thạch.",
                "• Năng lượng tái tạo được bổ sung liên tục bởi các quá trình tự nhiên.\n• Có khả năng tái tạo và khai thác lâu dài nếu quản lý phù hợp."
            );
            c5_l17.AddPage(
                "MỘT SỐ DẠNG NĂNG LƯỢNG TÁI TẠO",
                "Các dạng năng lượng tái tạo phổ biến theo SGK KHTN 9:\n\n" +
                "1. Năng lượng Mặt Trời: Khai thác qua pin quang điện và thiết bị làm nóng bằng năng lượng mặt trời.\n" +
                "2. Năng lượng gió: Sử dụng turbine gió để biến động năng của gió thành điện năng.\n" +
                "3. Năng lượng dòng nước và sóng biển: Thủy điện, khai thác năng lượng sóng và thủy triều.\n" +
                "4. Năng lượng địa nhiệt: Khai thác nhiệt năng sâu trong lòng Trái Đất.\n" +
                "5. Năng lượng sinh khối: Khai thác từ gỗ, phế phẩm nông nghiệp, khí sinh học (biogas).",
                "• Các dạng chính: Năng lượng Mặt Trời, gió, dòng nước, sóng biển, địa nhiệt, sinh khối.\n• Chuyển hóa thành điện năng và nhiệt năng sạch phục vụ đời sống."
            );
            c5_l17.AddPage(
                "ƯU ĐIỂM",
                "Các ưu điểm chính của năng lượng tái tạo:\n\n" +
                "- Có khả năng tái tạo: Được tự nhiên bổ sung liên tục qua các chu trình sinh thái.\n" +
                "- Giảm phụ thuộc vào nhiên liệu hóa thạch: Góp phần bảo đảm an ninh năng lượng lâu dài cho các quốc gia.\n" +
                "- Thân thiện hơn với môi trường: Thường có mức phát thải thấp hơn trong giai đoạn vận hành, giúp giảm ô nhiễm không khí và áp lực lên khí hậu toàn cầu.\n" +
                "- Có thể khai thác lâu dài nếu có phương thức quản lý và công nghệ phù hợp.",
                "• Nguồn năng lượng có khả năng tái tạo và bổ sung liên tục.\n• Giảm phụ thuộc nhiên liệu hóa thạch, phát thải thấp hơn trong vận hành."
            );
            c5_l17.AddPage(
                "NHƯỢC ĐIỂM / HẠN CHẾ",
                "Những nhược điểm và thách thức khi khai thác năng lượng tái tạo:\n\n" +
                "- Phụ thuộc vào điều kiện tự nhiên và thời tiết: Điện mặt trời phụ thuộc thời gian nắng và mây; điện gió phụ thuộc tốc độ gió thất thường.\n" +
                "- Khả năng khai thác khác nhau theo vùng địa lý: Vùng nhiều nắng, gió hay địa nhiệt mới có hiệu quả cao.\n" +
                "- Chi phí đầu tư ban đầu cao: Cần công nghệ hiện đại, pin tích trữ đắt tiền và hạ tầng lưới điện phù hợp.\n" +
                "- Cần đánh giá tác động môi trường toàn diện: Có thể chiếm dụng diện tích mặt bằng hoặc ảnh hưởng sinh cảnh cục bộ.",
                "• Phụ thuộc vào thời tiết và điều kiện tự nhiên theo từng khu vực địa lý.\n• Chi phí đầu tư ban đầu và pin lưu trữ còn cao, cần hạ tầng phù hợp."
            );
            _lessons["C5_L17"] = c5_l17;
        }
    }
}
