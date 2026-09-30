using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.Quiz
{
    /// <summary>
    /// Lớp dữ liệu chứa thông tin của một câu hỏi trắc nghiệm.
    /// </summary>
    [Serializable]
    public class QuestionData
    {
        // Mã câu hỏi (ví dụ: "PHY_CH1_Q01")
        public string questionID;
        
        // Nội dung câu hỏi
        public string questionText;
        
        // Mảng chứa 4 đáp án
        public string[] answers;
        
        // Vị trí đáp án đúng (từ 0 đến 3)
        public int correctAnswerIndex;
        
        // Chương hoặc chủ đề của câu hỏi
        public string chapter;
        
        // Độ khó của câu hỏi ("Easy", "Medium", "Hard")
        public string difficulty;
        
        // Lời giải thích hiển thị sau khi trả lời
        public string explanation;
    }

    /// <summary>
    /// Lớp bọc (wrapper) danh sách câu hỏi, dùng để đọc dữ liệu từ JSON bằng JsonUtility.
    /// </summary>
    [Serializable]
    public class QuestionDataList
    {
        // Danh sách các câu hỏi
        public List<QuestionData> questions;
    }
}
