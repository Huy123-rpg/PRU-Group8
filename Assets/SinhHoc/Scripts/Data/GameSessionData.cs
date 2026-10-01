using UnityEngine;

public enum QuestionType
{
    MultipleChoice,
    EssayImage
}

public class GameSessionData
{
    public static string SelectedSubject = "Biology";
    public static string SelectedChapterID = "C11";
    public static string SelectedLessonID = "B39";
    public static QuestionType SelectedQuestionType = QuestionType.MultipleChoice;
    
    // Clear data if needed
    public static void ResetSession()
    {
        SelectedSubject = "";
        SelectedChapterID = "";
        SelectedLessonID = "";
        SelectedQuestionType = QuestionType.MultipleChoice;
    }
}
