using UnityEngine;

/// <summary>
/// ScriptableObject chứa dữ liệu cho 1 câu hỏi
/// </summary>
[CreateAssetMenu(fileName = "NewQuestionData", menuName = "Game Data/Question Data")]
public class QuestionData : ScriptableObject
{
    [TextArea(2, 5)]
    public string questionText = "Nội dung câu hỏi...";
    
    [Header("4 Lựa chọn đáp án")]
    public string[] options = new string[4];

    [Tooltip("Chỉ số đáp án đúng (0 đến 3)")]
    [Range(0, 3)]
    public int correctOptionIndex = 0;

    [Header("Phân loại")]
    public SubjectType subject;

    [Header("Tự Luận (Essay)")]
    [Tooltip("Loại câu hỏi: Trắc nghiệm hoặc Tự luận")]
    public string loaiCauHoi = "Trắc nghiệm";

    [TextArea(2, 4)]
    [Tooltip("Đáp án tham chiếu để AI chấm điểm tự luận")]
    public string dapAnThamChieu = "";

    [TextArea(2, 4)]
    [Tooltip("Tiêu chí chấm điểm (rubric) cho AI")]
    public string tieuChiCham = "";
}
