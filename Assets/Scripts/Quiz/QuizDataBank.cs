using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.Quiz
{
    /// <summary>
    /// Lớp quản lý ngân hàng câu hỏi, tải dữ liệu từ file JSON.
    /// </summary>
    public class QuizDataBank
    {
        // Danh sách lưu trữ toàn bộ câu hỏi
        private List<QuestionData> _questions = new List<QuestionData>();

        // Tổng số câu hỏi trong ngân hàng
        public int TotalQuestionCount => _questions.Count;

        /// <summary>
        /// Khởi tạo và tự động tải dữ liệu JSON.
        /// </summary>
        public QuizDataBank()
        {
            LoadData();
        }

        /// <summary>
        /// Tải dữ liệu JSON từ thư mục Resources.
        /// </summary>
        private void LoadData()
        {
            // Đọc file JSON từ Resources/QuizData/physics_questions
            TextAsset jsonFile = Resources.Load<TextAsset>("QuizData/physics_questions");
            
            if (jsonFile != null)
            {
                try
                {
                    // Phân tích cú pháp chuỗi JSON thành đối tượng QuestionDataList
                    QuestionDataList dataList = JsonUtility.FromJson<QuestionDataList>(jsonFile.text);
                    
                    if (dataList != null && dataList.questions != null)
                    {
                        _questions = dataList.questions;
                        Debug.Log($"Đã tải thành công {_questions.Count} câu hỏi từ ngân hàng dữ liệu.");
                    }
                    else
                    {
                        Debug.LogWarning("Không tìm thấy danh sách câu hỏi trong file JSON, hoặc cấu trúc JSON không khớp.");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Lỗi khi phân tích dữ liệu JSON: {e.Message}");
                }
            }
            else
            {
                Debug.LogError("Không tìm thấy file QuizData/physics_questions trong thư mục Resources.");
            }
        }

        /// <summary>
        /// Lấy toàn bộ danh sách câu hỏi.
        /// </summary>
        public List<QuestionData> GetAllQuestions()
        {
            return new List<QuestionData>(_questions);
        }

        /// <summary>
        /// Lọc danh sách câu hỏi theo chương.
        /// </summary>
        public List<QuestionData> GetQuestionsByChapter(string chapter)
        {
            List<QuestionData> filteredQuestions = new List<QuestionData>();
            
            // Chuẩn hóa tên chương tìm kiếm
            string searchChapter = chapter.Replace(":", "").Replace(".", "").Replace(" ", "").ToLower();

            foreach (var q in _questions)
            {
                string qChapter = q.chapter.Replace(":", "").Replace(".", "").Replace(" ", "").ToLower();
                if (qChapter.Contains(searchChapter) || searchChapter.Contains(qChapter) || string.IsNullOrEmpty(searchChapter))
                {
                    filteredQuestions.Add(q);
                }
            }
            
            return filteredQuestions;
        }

        /// <summary>
        /// Lấy danh sách câu hỏi ngẫu nhiên từ toàn bộ ngân hàng.
        /// </summary>
        public List<QuestionData> GetRandomQuestions(int count)
        {
            return GetRandomQuestionsFromList(_questions, count);
        }

        /// <summary>
        /// Lấy danh sách câu hỏi ngẫu nhiên theo một chương cụ thể.
        /// </summary>
        public List<QuestionData> GetRandomQuestionsByChapter(string chapter, int count)
        {
            List<QuestionData> filteredQuestions = GetQuestionsByChapter(chapter);
            return GetRandomQuestionsFromList(filteredQuestions, count);
        }

        /// <summary>
        /// Hàm hỗ trợ: Trộn danh sách và lấy ra 'count' câu hỏi ngẫu nhiên sử dụng thuật toán Fisher-Yates.
        /// </summary>
        private List<QuestionData> GetRandomQuestionsFromList(List<QuestionData> sourceList, int count)
        {
            if (sourceList == null || sourceList.Count == 0)
            {
                Debug.LogWarning("Danh sách câu hỏi nguồn trống, không thể lấy câu hỏi ngẫu nhiên.");
                return new List<QuestionData>();
            }

            // Tạo bản sao để tránh thay đổi danh sách gốc
            List<QuestionData> shuffledList = new List<QuestionData>(sourceList);

            // Thuật toán xáo trộn Fisher-Yates
            for (int i = shuffledList.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                // Hoán đổi vị trí i và j
                QuestionData temp = shuffledList[i];
                shuffledList[i] = shuffledList[j];
                shuffledList[j] = temp;
            }

            // Trả về số lượng câu hỏi mong muốn, không vượt quá số lượng hiện có
            int returnCount = Mathf.Min(count, shuffledList.Count);
            return shuffledList.GetRange(0, returnCount);
        }
    }
}
