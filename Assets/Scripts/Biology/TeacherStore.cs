using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace ScienceQuest.Biology
{
    /// <summary>
    /// Nơi lưu trữ và truy xuất dữ liệu do giảng viên tạo (bài học + câu hỏi).
    ///
    /// Dữ liệu được lưu ở 2 nơi:
    ///   1. JSON tại persistentDataPath (dữ liệu làm việc của ứng dụng).
    ///   2. File CSV trong thư mục dữ liệu để giảng viên có thể mở bằng Excel / đồng bộ sang Google Sheet.
    ///
    /// Ngoài ra store có thể xuất CSV chuẩn tương thích với QuizManager để đưa vào game.
    /// </summary>
    public static class TeacherStore
    {
        // ----- Đường dẫn -----
        public static string DataFolder => Path.Combine(Application.persistentDataPath, "BiologyTeacher");
        public static string JsonFilePath => Path.Combine(DataFolder, "teacher_lessons.json");
        public static string CsvFilePath => Path.Combine(DataFolder, "teacher_lessons.csv");
        public static string ScoreExcelPath => Path.Combine(DataFolder, "KetQua_SinhHoc.xlsx");

        private static TeacherData _cache;

        // ----- Load / Save -----

        /// <summary>
        /// Nạp toàn bộ dữ liệu giảng viên (có cache trong RAM).
        /// </summary>
        public static TeacherData Load()
        {
            if (_cache != null) return _cache;

            _cache = new TeacherData();

            try
            {
                if (File.Exists(JsonFilePath))
                {
                    string json = File.ReadAllText(JsonFilePath, Encoding.UTF8);
                    TeacherData parsed = JsonUtility.FromJson<TeacherData>(json);
                    if (parsed != null && parsed.lessons != null)
                    {
                        _cache = parsed;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[TeacherStore] Không đọc được file JSON giảng viên: {e.Message}");
            }

            return _cache;
        }

        /// <summary>
        /// Lưu toàn bộ dữ liệu giảng viên ra JSON + đồng bộ file CSV.
        /// </summary>
        public static bool Save()
        {
            if (_cache == null) return false;

            try
            {
                if (!Directory.Exists(DataFolder))
                {
                    Directory.CreateDirectory(DataFolder);
                }

                string json = JsonUtility.ToJson(_cache, true);
                File.WriteAllText(JsonFilePath, json, Encoding.UTF8);

                ExportCsv();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[TeacherStore] Lỗi lưu dữ liệu giảng viên: {e.Message}");
                return false;
            }
        }

        // ----- CRUD bài học -----

        public static List<TeacherLesson> GetLessons()
        {
            return Load().lessons;
        }

        public static List<TeacherLesson> GetLessonsByChapter(string chapterId)
        {
            List<TeacherLesson> result = new List<TeacherLesson>();
            string norm = Normalize(chapterId);
            foreach (var lesson in GetLessons())
            {
                if (string.IsNullOrEmpty(norm) || Normalize(lesson.chapterId) == norm)
                {
                    result.Add(lesson);
                }
            }
            return result;
        }

        /// <summary>
        /// Tạo bài học mới. Trả về null nếu tiêu đề rỗng.
        /// </summary>
        /// <param name="chapterTitle">Tiêu đề chủ đề (dùng làm khoá lọc câu hỏi cho học sinh).</param>
        public static TeacherLesson CreateLesson(string chapterTitle, string title, string mode)
        {
            if (string.IsNullOrEmpty(title)) return null;

            TeacherData data = Load();
            TeacherLesson lesson = new TeacherLesson
            {
                lessonId = GenerateId("B", data.lessons.Count + 1),
                chapterId = string.IsNullOrEmpty(chapterTitle) ? "Chủ đề Sinh học" : chapterTitle,
                title = title.Trim(),
                mode = mode
            };

            data.lessons.Add(lesson);
            Save();
            return lesson;
        }

        public static bool DeleteLesson(string lessonId)
        {
            TeacherData data = Load();
            int removed = data.lessons.RemoveAll(l => l.lessonId == lessonId);
            if (removed > 0) Save();
            return removed > 0;
        }

        /// <summary>
        /// Thêm câu hỏi vào bài. Trả về null nếu nội dung câu hỏi rỗng.
        /// </summary>
        public static TeacherQuestion AddQuestion(string lessonId, TeacherQuestion question)
        {
            if (question == null || string.IsNullOrEmpty(question.questionText)) return null;

            TeacherLesson lesson = FindLesson(lessonId);
            if (lesson == null) return null;

            // Dùng tiêu đề bài làm khoá lọc để học sinh chọn đúng bài khi vào game
            question.lessonId = lesson.title;
            question.chapterId = lesson.chapterId;
            if (string.IsNullOrEmpty(question.questionId))
            {
                question.questionId = GenerateId("Q", lesson.questions.Count + 1);
            }

            lesson.questions.Add(question);
            Save();
            return question;
        }

        public static bool DeleteQuestion(string lessonId, string questionId)
        {
            TeacherLesson lesson = FindLesson(lessonId);
            if (lesson == null) return false;

            int removed = lesson.questions.RemoveAll(q => q.questionId == questionId);
            if (removed > 0) Save();
            return removed > 0;
        }

        public static TeacherLesson FindLesson(string lessonId)
        {
            foreach (var lesson in GetLessons())
            {
                if (lesson.lessonId == lessonId) return lesson;
            }
            return null;
        }

        /// <summary>
        /// Sinh mã bài/câu hỏi không trùng lặp.
        /// </summary>
        private static string GenerateId(string prefix, int index)
        {
            return $"{prefix}_GV_{index:000}_{DateTime.Now:HHmmssfff}";
        }

        // ----- Xuất CSV -----

        /// <summary>
        /// Xuất toàn bộ câu hỏi của giảng viên ra CSV (mở được bằng Excel / Google Sheets).
        /// Cấu trúc cột tương thích với bộ lọc hiện có của QuizManager.
        /// </summary>
        public static bool ExportCsv()
        {
            try
            {
                if (!Directory.Exists(DataFolder)) Directory.CreateDirectory(DataFolder);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Question_ID,Chapter_ID,Lesson_ID,Question_Text,Option_A,Option_B,Option_C,Option_D,Correct_Answer,Mode,Keywords_TuLuan,Explanation,Difficulty,Time_Limit_Sec,HP_Bonus_Percent");

                foreach (var lesson in GetLessons())
                {
                    foreach (var q in lesson.questions)
                    {
                        sb.AppendLine(string.Join(",", new[]
                        {
                            Csv(q.questionId),
                            Csv(q.chapterId),
                            Csv(q.lessonId),
                            Csv(q.questionText),
                            Csv(q.ansA),
                            Csv(q.ansB),
                            Csv(q.ansC),
                            Csv(q.ansD),
                            Csv(q.correctAnswer),
                            Csv(q.mode),
                            Csv(q.keywordsTuLuan),
                            Csv(q.explain),
                            Csv(q.difficulty),
                            q.timeLimit.ToString("0", CultureInfo.InvariantCulture),
                            q.hpBonus.ToString("0", CultureInfo.InvariantCulture)
                        }));
                    }
                }

                File.WriteAllText(CsvFilePath, sb.ToString(), Encoding.UTF8);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[TeacherStore] Lỗi xuất CSV giảng viên: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Bọc giá trị theo chuẩn CSV (nháy kép khi chứa dấu phẩy hoặc xuống dòng).
        /// </summary>
        private static string Csv(string value)
        {
            string v = value ?? "";
            if (v.Contains(",") || v.Contains("\"") || v.Contains("\n") || v.Contains("\r"))
            {
                return "\"" + v.Replace("\"", "\"\"") + "\"";
            }
            return v;
        }

        private static string Normalize(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace(":", "").Replace(".", "").Replace(" ", "").Replace("_", "").ToLower();
        }
    }
}
