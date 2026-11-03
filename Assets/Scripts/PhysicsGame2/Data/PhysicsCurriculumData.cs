using System;
using System.Collections.Generic;

namespace ScienceQuest.PhysicsGame2
{
    [Serializable]
    public class CurriculumLessonInfo
    {
        public string lessonId;      // e.g. "C1_L1", "C2_L5", "C1_TEST"
        public string lessonNumber;  // e.g. "Bài 2", "Bài 3", "TEST"
        public string lessonTitle;   // e.g. "Động năng. Thế năng"
        public bool isTest;
        public bool isRequiredForChapterCompletion;

        public CurriculumLessonInfo(string id, string number, string title, bool test = false, bool requiredForChapterCompletion = false)
        {
            lessonId = id;
            lessonNumber = number;
            lessonTitle = title;
            isTest = test;
            isRequiredForChapterCompletion = requiredForChapterCompletion;
        }

        public string DisplayName => isTest ? lessonTitle : $"{lessonNumber}. {lessonTitle}";
    }

    [Serializable]
    public class CurriculumChapterInfo
    {
        public int chapterNumber;    // 1 -> 5
        public string romanNumber;   // "I", "II", "III", "IV", "V"
        public string chapterName;   // "Năng lượng cơ học", "Ánh sáng", ...
        public List<CurriculumLessonInfo> lessons = new List<CurriculumLessonInfo>();

        public CurriculumChapterInfo(int number, string roman, string name)
        {
            chapterNumber = number;
            romanNumber = roman;
            chapterName = name;
        }

        public string FullHeader => $"Chương {romanNumber}. {chapterName}";
    }

    /// <summary>
    /// Nguồn dữ liệu chuẩn (Source of Truth) về 5 Chương và các bài học Vật lý 9 cho Aurelia Academy.
    /// </summary>
    public static class PhysicsCurriculumData
    {
        private static readonly Dictionary<int, CurriculumChapterInfo> _chapters = new Dictionary<int, CurriculumChapterInfo>();

        static PhysicsCurriculumData()
        {
            // ==========================================
            // CHƯƠNG I – Năng lượng cơ học
            // ==========================================
            var ch1 = new CurriculumChapterInfo(1, "I", "Năng lượng cơ học");
            ch1.lessons.Add(new CurriculumLessonInfo("C1_L2", "Bài 2", "Động năng. Thế năng"));
            ch1.lessons.Add(new CurriculumLessonInfo("C1_L3", "Bài 3", "Cơ năng"));
            ch1.lessons.Add(new CurriculumLessonInfo("C1_L4", "Bài 4", "Công và công suất"));
            ch1.lessons.Add(new CurriculumLessonInfo("C1_TEST", "TEST", "TEST tổng hợp Chương I", true, true));
            _chapters.Add(1, ch1);

            // ==========================================
            // CHƯƠNG II – Ánh sáng
            // ==========================================
            var ch2 = new CurriculumChapterInfo(2, "II", "Ánh sáng");
            ch2.lessons.Add(new CurriculumLessonInfo("C2_L5", "Bài 5", "Khúc xạ ánh sáng"));
            ch2.lessons.Add(new CurriculumLessonInfo("C2_L6", "Bài 6", "Phản xạ toàn phần"));
            ch2.lessons.Add(new CurriculumLessonInfo("C2_L7", "Bài 7", "Lăng kính"));
            ch2.lessons.Add(new CurriculumLessonInfo("C2_L8", "Bài 8", "Thấu kính"));
            ch2.lessons.Add(new CurriculumLessonInfo("C2_L9", "Bài 9", "Thực hành đo tiêu cự của thấu kính hội tụ"));
            ch2.lessons.Add(new CurriculumLessonInfo("C2_L10", "Bài 10", "Kính lúp. Bài tập thấu kính"));
            ch2.lessons.Add(new CurriculumLessonInfo("C2_TEST", "TEST", "TEST tổng hợp Chương II", true, true));
            _chapters.Add(2, ch2);

            // ==========================================
            // CHƯƠNG III – Điện
            // ==========================================
            var ch3 = new CurriculumChapterInfo(3, "III", "Điện");
            ch3.lessons.Add(new CurriculumLessonInfo("C3_L11", "Bài 11", "Điện trở. Định luật Ohm"));
            ch3.lessons.Add(new CurriculumLessonInfo("C3_L12", "Bài 12", "Đoạn mạch nối tiếp, song song"));
            ch3.lessons.Add(new CurriculumLessonInfo("C3_L13", "Bài 13", "Năng lượng của dòng điện và công suất điện"));
            ch3.lessons.Add(new CurriculumLessonInfo("C3_TEST", "TEST", "TEST tổng hợp Chương III", true, true));
            _chapters.Add(3, ch3);

            // ==========================================
            // CHƯƠNG IV – Điện từ
            // ==========================================
            var ch4 = new CurriculumChapterInfo(4, "IV", "Điện từ");
            ch4.lessons.Add(new CurriculumLessonInfo("C4_L14", "Bài 14", "Cảm ứng điện từ. Nguyên tắc tạo ra dòng điện xoay chiều"));
            ch4.lessons.Add(new CurriculumLessonInfo("C4_L15", "Bài 15", "Tác dụng của dòng điện xoay chiều"));
            ch4.lessons.Add(new CurriculumLessonInfo("C4_TEST", "TEST", "TEST tổng hợp Chương IV", true, true));
            _chapters.Add(4, ch4);

            // ==========================================
            // CHƯƠNG V – Năng lượng với cuộc sống
            // ==========================================
            var ch5 = new CurriculumChapterInfo(5, "V", "Năng lượng với cuộc sống");
            ch5.lessons.Add(new CurriculumLessonInfo("C5_L16", "Bài 16", "Vòng năng lượng trên Trái Đất. Năng lượng hóa thạch"));
            ch5.lessons.Add(new CurriculumLessonInfo("C5_L17", "Bài 17", "Một số dạng năng lượng tái tạo"));
            ch5.lessons.Add(new CurriculumLessonInfo("C5_TEST", "TEST", "TEST tổng hợp Chương V", true, true));
            _chapters.Add(5, ch5);
        }

        public static CurriculumChapterInfo GetChapter(int chapterNumber)
        {
            if (_chapters.TryGetValue(chapterNumber, out var info))
            {
                return info;
            }
            return _chapters[1];
        }

        public static List<CurriculumLessonInfo> GetLessons(int chapterNumber)
        {
            var ch = GetChapter(chapterNumber);
            return ch != null ? ch.lessons : new List<CurriculumLessonInfo>();
        }

        public static string GetChapterHeader(int chapterNumber)
        {
            var ch = GetChapter(chapterNumber);
            return ch != null ? ch.FullHeader : "Chương I. Năng lượng cơ học";
        }
    }
}
