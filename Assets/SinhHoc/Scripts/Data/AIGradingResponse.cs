using System;
using System.Collections.Generic;

[Serializable]
public class AIEvaluationStep
{
    public int lineNumber;        // Dòng thứ mấy trong bài làm
    public string lineText;       // Nội dung chữ dòng đó
    public bool isCorrect;        // Đúng hay sai
    public float stepScore;       // Điểm của bước này
    public float maxStepScore;    // Điểm tối đa của bước
    public string statusText;     // "Chính xác", "Sai kiến thức", "Thiếu ý", "Cần bổ sung"
    public string errorDetail;    // Lỗi sai cụ thể ở dòng này (nếu có)
}

[Serializable]
public class AIGradingResponse
{
    public bool readable = true;
    public string transcribedText;       // Chữ viết tay AI nhận diện và chép lại
    public float score;
    public float maxScore = 10f;
    public float percentage;
    public List<string> correctPoints;
    public List<string> missingPoints;
    public List<string> errorsIdentified; // Danh sách lỗi sai chi tiết do AI phát hiện
    public List<AIEvaluationStep> stepBreakdown; // Đánh giá chi tiết theo từng dòng/bước
    public string feedback;               // Nhận xét tổng quan
    public string suggestions;            // Gợi ý và giải thích hướng làm đúng
    public string suggestedAnswer;        // Đáp án và lời giải chi tiết chuẩn mực
}
