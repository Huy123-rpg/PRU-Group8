using UnityEngine;

public enum QuestionType
{
    MultipleChoice,
    EssayImage
}

public class GameSessionData
{
    // Giá trị nội bộ phải dùng thống nhất.
    // UI có thể hiển thị "Sinh Học",
    // nhưng dữ liệu dùng "Biology".
    public static string SelectedSubject = "Biology";

    public static string SelectedChapterID = "C11";
    public static string SelectedLessonID = "B36";

    public static QuestionType SelectedQuestionType =
        QuestionType.MultipleChoice;

    public static void SetBiologySession(
        string chapterID,
        string lessonID,
        QuestionType questionType)
    {
        SelectedSubject = "Biology";
        SelectedChapterID = chapterID;
        SelectedLessonID = lessonID;
        SelectedQuestionType = questionType;

        Debug.Log(
            $"[GameSessionData] Session = " +
            $"{SelectedSubject} - " +
            $"{SelectedChapterID} - " +
            $"{SelectedLessonID} - " +
            $"{SelectedQuestionType}"
        );
    }

    public static void ResetSession()
    {
        SelectedSubject = "";
        SelectedChapterID = "";
        SelectedLessonID = "";
        SelectedQuestionType =
            QuestionType.MultipleChoice;
    }
}