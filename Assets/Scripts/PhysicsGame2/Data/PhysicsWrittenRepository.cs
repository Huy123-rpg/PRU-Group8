using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    [Serializable]
    public class PhysicsWrittenQuestion
    {
        public string id;
        public int chapterId;
        public string chapterName;
        public string lessonId;
        public string lessonName;
        public string teacherId;
        public string teacherName;
        public string mucDo;
        public string question;
        public string[] keywords;
        public string sampleAnswer;
        public string explanation;

        public PhysicsWrittenQuestion(string id, int chapterId, string chapterName, string lessonId, string lessonName,
                                      string teacherId, string teacherName, string mucDo,
                                      string question, string[] keywords, string sampleAnswer, string explanation = "")
        {
            this.id = id;
            this.chapterId = chapterId;
            this.chapterName = chapterName;
            this.lessonId = lessonId;
            this.lessonName = lessonName;
            this.teacherId = teacherId;
            this.teacherName = teacherName;
            this.mucDo = mucDo;
            this.question = question;
            this.keywords = keywords;
            this.sampleAnswer = sampleAnswer;
            this.explanation = explanation;
        }
    }

    [Serializable]
    public class PhysicsWrittenQuestionListWrapper
    {
        public List<PhysicsWrittenQuestion> questions;
    }

    /// <summary>
    /// Repository quản lý ngân hàng 64 câu hỏi Tự luận Vật lý 9 (4 câu / bài tương ứng 4 Giáo viên: Minh, Lan, Duy, Ha).
    /// Tự động nạp từ Resources/QuizData/physics_written_questions.json.
    /// </summary>
    public static class PhysicsWrittenRepository
    {
        private static List<PhysicsWrittenQuestion> _questions = null;
        private static bool _isLoaded = false;

        public static void EnsureLoaded()
        {
            if (_isLoaded && _questions != null && _questions.Count > 0) return;

            _questions = new List<PhysicsWrittenQuestion>();

            // 1. Nạp từ Resources/QuizData/physics_written_questions
            TextAsset jsonAsset = Resources.Load<TextAsset>("QuizData/physics_written_questions");
            if (jsonAsset != null && !string.IsNullOrEmpty(jsonAsset.text))
            {
                try
                {
                    PhysicsWrittenQuestionListWrapper wrapper = JsonUtility.FromJson<PhysicsWrittenQuestionListWrapper>(jsonAsset.text);
                    if (wrapper != null && wrapper.questions != null && wrapper.questions.Count > 0)
                    {
                        _questions = wrapper.questions;
                        _isLoaded = true;
                        Debug.Log($"[PhysicsWrittenRepository] Đã nạp thành công {_questions.Count} câu hỏi tự luận theo giáo viên!");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[PhysicsWrittenRepository] Lỗi parse JSON tự luận: {ex.Message}");
                }
            }

            _isLoaded = true;
        }

        /// <summary>
        /// Lấy câu hỏi tự luận theo bài học và giáo viên đang chọn (Minh: Nền tảng, Lan: Ứng dụng, Duy: Tính toán nâng cao, Ha: Tổng hợp).
        /// </summary>
        public static PhysicsWrittenQuestion GetQuestionForLessonAndTeacher(int chapterId, string lessonId, string teacherId)
        {
            EnsureLoaded();

            if (_questions == null || _questions.Count == 0) return null;

            // 1. Khớp chính xác cả lessonId và teacherId
            if (!string.IsNullOrEmpty(lessonId))
            {
                var exact = _questions.Find(q =>
                    string.Equals(q.lessonId, lessonId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(q.teacherId, teacherId, StringComparison.OrdinalIgnoreCase));
                if (exact != null) return exact;

                // 2. Nếu không tìm thấy giáo viên đó, lấy bất kỳ câu nào thuộc lessonId
                var lessonMatch = _questions.Find(q => string.Equals(q.lessonId, lessonId, StringComparison.OrdinalIgnoreCase));
                if (lessonMatch != null) return lessonMatch;
            }

            // 3. Khớp theo chapterId và teacherId
            var chapterTeacherMatch = _questions.Find(q =>
                q.chapterId == chapterId &&
                string.Equals(q.teacherId, teacherId, StringComparison.OrdinalIgnoreCase));
            if (chapterTeacherMatch != null) return chapterTeacherMatch;

            // 4. Khớp theo chapterId
            var chapterMatch = _questions.Find(q => q.chapterId == chapterId);
            if (chapterMatch != null) return chapterMatch;

            return _questions[0];
        }

        public static PhysicsWrittenQuestion GetQuestion(int chapterId, string lessonId = null)
        {
            string teacherId = PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SelectedTeacherId : "Minh";
            return GetQuestionForLessonAndTeacher(chapterId, lessonId, teacherId);
        }

        public static List<PhysicsWrittenQuestion> GetAllQuestionsForLesson(string lessonId)
        {
            EnsureLoaded();
            if (_questions == null) return new List<PhysicsWrittenQuestion>();
            return _questions.FindAll(q => string.Equals(q.lessonId, lessonId, StringComparison.OrdinalIgnoreCase));
        }

        public static List<PhysicsWrittenQuestion> GetAllQuestionsForChapter(int chapterId)
        {
            EnsureLoaded();
            if (_questions == null) return new List<PhysicsWrittenQuestion>();
            return _questions.FindAll(q => q.chapterId == chapterId);
        }
    }
}
