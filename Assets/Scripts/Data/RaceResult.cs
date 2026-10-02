using System;

/// <summary>
/// Lưu trữ kết quả sau khi hoàn tất màn đua
/// </summary>
[Serializable]
public class RaceResult
{
    public int rank = 1;              // Hạng về đích (1 - 4)
    public int totalScore = 0;        // Tổng điểm số
    public int correctAnswers = 0;    // Số câu trả lời đúng
    public int wrongAnswers = 0;      // Số câu trả lời sai
    public float completionTime = 0f; // Thời gian hoàn thành (giây)
}
