using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.Quiz
{
    /// <summary>
    /// Lớp dữ liệu chứa thông tin của một câu hỏi trắc nghiệm & tự luận.
    /// Tương thích chuẩn 14 cột Ngân Hàng Câu Hỏi Google Sheets của trường/nhóm.
    /// </summary>
    [Serializable]
    public class QuestionData
    {
        // Mã câu hỏi (ví dụ: "C9_001", "C7_001", "PHY_CH1_Q01")
        public string questionID;
        
        // Nội dung câu hỏi
        public string questionText;
        
        // Mảng chứa 4 đáp án trắc nghiệm
        public string[] answers;
        
        // Vị trí đáp án đúng (từ 0 đến 3)
        public int correctAnswerIndex;
        
        // Chương hoặc chủ đề của câu hỏi
        public string chapter;
        
        // Độ khó của câu hỏi ("Easy", "Medium", "Hard")
        public string difficulty;
        
        // Lời giải thích hiển thị sau khi trả lời
        public string explanation;

        // --- CÁC THUỘC TÍNH MỞ RỘNG CHO GIÁO VIÊN & GOOGLE SHEET (CHUẨN 18 CỘT) ---
        // Số thứ tự câu hỏi
        public int stt = 0;

        // Mã chương (ví dụ: "chuong1", "chuong2", "tonghop")
        public string chuongId = "chuong1";

        // Tên chương đầy đủ (ví dụ: "Chương I. Năng lượng cơ học")
        public string tenChuong = "Chương I. Năng lượng cơ học";

        // Mã bài học (ví dụ: "B9_001", "bai2", "test_chuong1")
        public string baiID = "bai2";

        // Tên bài học đầy đủ (ví dụ: "Bài 2. Động năng. Thế năng")
        public string tenBai = "Bài 2. Động năng. Thế năng";

        // Mức độ nhận thức: "Nhận biết", "Thông hiểu", "Vận dụng", "Vận dụng cao"
        public string mucDo = "Nhận biết";

        // Hình thức: "Trắc nghiệm" hoặc "Tự luận"
        public string loaiCauHoi = "Trắc nghiệm";

        // Đáp án tham chiếu / bài giải mẫu cho câu hỏi Tự Luận
        public string dapAnThamChieu = "";

        // Tiêu chí chấm từng bước (Rubric) cho câu hỏi Tự Luận
        public string tieuChiCham = "";

        // Mẹo ghi nhớ nhanh
        public string meoGhiNho = "";

        // Trạng thái: "Xuất bản" hoặc "Bản nháp"
        public string trangThai = "Xuất bản";

        // Mã định danh Giáo Viên tạo/biên soạn
        public string giaoVienID = "GV001";

        // Đường dẫn hoặc URL hình ảnh minh họa (nếu có)
        public string hinhAnh = "";
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
