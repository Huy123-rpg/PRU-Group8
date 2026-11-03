using System;
using System.Collections.Generic;

namespace ScienceQuest.PhysicsGame2
{
    [Serializable]
    public class MechanicsChallenge
    {
        public string id;
        public string title;
        public string targetDescription;
        public string hintText;

        // Delegate hoặc rule check
        public Func<float, float, float, float, float, float, (bool isSuccess, string feedback)> evaluateFunc;

        public MechanicsChallenge(
            string id,
            string title,
            string targetDescription,
            string hintText,
            Func<float, float, float, float, float, float, (bool, string)> evaluateFunc)
        {
            this.id = id;
            this.title = title;
            this.targetDescription = targetDescription;
            this.hintText = hintText;
            this.evaluateFunc = evaluateFunc;
        }

        public static List<MechanicsChallenge> GetPresets()
        {
            return new List<MechanicsChallenge>
            {
                // Thử thách 1: Tổng cơ năng 180 - 220 J
                new MechanicsChallenge(
                    "ch_mechanics_1",
                    "Thử thách 1: Phục hồi Cơ năng",
                    "Đạt tổng cơ năng W trong khoảng 180 J đến 220 J.",
                    "Gợi ý: Thử chọn khối lượng m = 2 kg, độ cao h = 5 m và vận tốc v = 10 m/s.",
                    (m, h, v, wd, wt, w) =>
                    {
                        if (w < 180f)
                        {
                            float diff = 180f - w;
                            return (false, $"Cơ năng còn thiếu {diff:F1} J (hiện tại: {w:F1} J). Hãy thử tăng độ cao hoặc vận tốc!");
                        }
                        if (w > 220f)
                        {
                            float diff = w - 220f;
                            return (false, $"Cơ năng vượt quá mục tiêu {diff:F1} J (hiện tại: {w:F1} J). Hãy giảm bớt độ cao hoặc vận tốc!");
                        }
                        return (true, "Xuất sắc! Cơ năng đã nằm trọn vẹn trong khoảng chuẩn 180 J – 220 J.");
                    }
                ),

                // Thử thách 2: Wđ >= 100 J và Wt >= 50 J
                new MechanicsChallenge(
                    "ch_mechanics_2",
                    "Thử thách 2: Năng lượng Kép",
                    "Đạt Động năng Wđ ≥ 100 J và Thế năng Wt ≥ 50 J.",
                    "Gợi ý: Động năng tăng rất nhanh khi vận tốc tăng (v²). Hãy tăng vận tốc trước, sau đó chỉnh độ cao h vừa đủ.",
                    (m, h, v, wd, wt, w) =>
                    {
                        if (wd < 100f && wt < 50f)
                            return (false, "Cả Động năng và Thế năng đều chưa đạt yêu cầu! Hãy tăng cả vận tốc và độ cao.");
                        if (wd < 100f)
                            return (false, $"Động năng còn thiếu {(100f - wd):F1} J (hiện tại: {wd:F1} J). Hãy tăng vận tốc v!");
                        if (wt < 50f)
                            return (false, $"Thế năng còn thiếu {(50f - wt):F1} J (hiện tại: {wt:F1} J). Hãy nâng cao vật thể h!");
                        return (true, "Tuyệt vời! Cả hai dạng năng lượng đều đã vượt ngưỡng yêu cầu.");
                    }
                ),

                // Thử thách 3: Wt > Wđ và W >= 200 J
                new MechanicsChallenge(
                    "ch_mechanics_3",
                    "Thử thách 3: Thế năng Ưu thế",
                    "Thế năng lớn hơn Động năng (Wt > Wđ) và Tổng cơ năng W ≥ 200 J.",
                    "Gợi ý: Tăng độ cao h lên cao và giữ vận tốc v ở mức trung bình để thế năng chiếm phần lớn cơ năng.",
                    (m, h, v, wd, wt, w) =>
                    {
                        if (wt <= wd)
                            return (false, $"Thế năng ({wt:F1} J) chưa lớn hơn Động năng ({wd:F1} J). Hãy tăng độ cao hoặc giảm vận tốc!");
                        if (w < 200f)
                            return (false, $"Thế năng đã lớn hơn nhưng Tổng cơ năng chưa đạt 200 J (hiện tại: {w:F1} J). Hãy tăng khối lượng m!");
                        return (true, "Chính xác! Thế năng đã áp đảo Động năng và tổng cơ năng đạt trên 200 J.");
                    }
                ),

                // Thử thách 4: Wđ > Wt, W <= 300 J và Wđ >= 120 J
                new MechanicsChallenge(
                    "ch_mechanics_4",
                    "Thử thách 4: Động năng Cực hạn",
                    "Động năng lớn hơn Thế năng (Wđ > Wt), Wđ ≥ 120 J và Tổng cơ năng W ≤ 300 J.",
                    "Gợi ý: Vận tốc cao giúp động năng lớn. Nhưng để tổng cơ năng không vượt quá 300 J, hãy giữ độ cao h thấp (0-3m).",
                    (m, h, v, wd, wt, w) =>
                    {
                        if (wd < 120f)
                            return (false, $"Động năng chưa đạt 120 J (hiện tại: {wd:F1} J). Hãy tăng thêm vận tốc v!");
                        if (wd <= wt)
                            return (false, $"Động năng ({wd:F1} J) chưa lớn hơn Thế năng ({wt:F1} J). Hãy hạ bớt độ cao h!");
                        if (w > 300f)
                            return (false, $"Tổng cơ năng đã vượt quá 300 J ({w:F1} J). Hãy giảm bớt khối lượng hoặc độ cao!");
                        return (true, "Thành công! Vật thể chuyển động với động năng vượt trội trong tầm kiểm soát.");
                    }
                ),

                // Thử thách 5: Cân bằng đối xứng (|Wđ - Wt| <= 10 J và W >= 150 J)
                new MechanicsChallenge(
                    "ch_mechanics_5",
                    "Thử thách 5: Cân bằng Tuyệt đối",
                    "Động năng xấp xỉ Thế năng (|Wđ - Wt| ≤ 10 J) và Tổng cơ năng W ≥ 150 J.",
                    "Gợi ý: Tìm cặp giá trị sao cho 0.5 × v² ≈ 10 × h. Ví dụ v = 10 m/s (v² = 100) thì h = 5 m (10×5 = 50).",
                    (m, h, v, wd, wt, w) =>
                    {
                        float diff = Math.Abs(wd - wt);
                        if (diff > 10f)
                            return (false, $"Chênh lệch giữa Động năng và Thế năng còn {diff:F1} J (yêu cầu ≤ 10 J). Hãy điều chỉnh cho cân bằng!");
                        if (w < 150f)
                            return (false, $"Hai dạng năng lượng đã cân bằng nhưng Tổng cơ năng chưa đạt 150 J (hiện tại: {w:F1} J). Hãy tăng khối lượng m!");
                        return (true, "Hoàn hảo! Trạng thái cân bằng năng lượng tuyệt đối đã được thiết lập.");
                    }
                )
            };
        }
    }
}
