using System;
using System.Collections.Generic;

namespace ScienceQuest.Biology
{
    /// <summary>
    /// Một câu hỏi do giảng viên tạo trong Trung tâm quản lý môn Sinh học.
    /// Cấu trúc cố ý giống với lớp Question của QuizManager để chuyển đổi dễ dàng.
    /// </summary>
    [Serializable]
    public class TeacherQuestion
    {
        public string questionId = "";
        public string chapterId = "";
        public string lessonId = "";
        public string questionText = "";
        public string ansA = "", ansB = "", ansC = "", ansD = "";
        public string correctAnswer = "";   // Trắc nghiệm: "A".."D" | Tự luận: chuỗi đáp án chuẩn
        public string mode = "TracNghiem";  // TracNghiem | TuLuan
        public string keywordsTuLuan = "";  // Các từ khóa cách nhau bằng dấu ";"
        public string explain = "";
        public string difficulty = "Nhận biết";
        public string hinhAnh = "";
        public float timeLimit = 15f;
        public float hpBonus = 20f;
    }

    /// <summary>
    /// Một bài học do giảng viên tạo (mỗi bài thuộc 1 chủ đề Sinh học).
    /// </summary>
    [Serializable]
    public class TeacherLesson
    {
        public string lessonId = "";
        public string chapterId = "";
        public string title = "";
        public string mode = "TracNghiem";  // Chế độ mặc định khi học sinh vào bài này
        public List<TeacherQuestion> questions = new List<TeacherQuestion>();
    }

    /// <summary>
    /// Toàn bộ dữ liệu giảng viên (tất cả bài đã tạo), lưu thành 1 file JSON.
    /// </summary>
    [Serializable]
    public class TeacherData
    {
        public List<TeacherLesson> lessons = new List<TeacherLesson>();
    }
}
