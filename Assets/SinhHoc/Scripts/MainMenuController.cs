using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private const string SceneRoot = "Assets/SinhHoc/Scenes/";

    public void OpenChapterSelect() => Load("ChapterSelect");
    public void OpenMainMenu() => Load("MainMenu");
    public void BackToChapters() => Load("ChapterSelect");
    public void BackToLessons() => Load("LessonSelect");

    public void OpenChapter(string chapterID)
    {
        if (CurriculumCatalog.GetChapter(GameSessionData.SelectedSubject, chapterID) == null)
        {
            Debug.LogError("Không tìm thấy chương: " + chapterID);
            return;
        }

        GameSessionData.SelectedChapterID = chapterID;
        GameSessionData.SelectedLessonID = null;
        Load("LessonSelect");
    }

    // Retained for the Chapter 11 button already wired in the Inspector.
    public void OpenChapter11() => OpenChapter("C11");

    public void OpenLesson(string lessonID)
    {
        if (CurriculumCatalog.GetLesson(GameSessionData.SelectedSubject,
                GameSessionData.SelectedChapterID, lessonID) == null)
        {
            Debug.LogError("Bài không thuộc chương đã chọn: " + lessonID);
            return;
        }

        GameSessionData.SelectedLessonID = lessonID;
        Load("ModeSelect");
    }

    public void StartMultipleChoice()
    {
        GameSessionData.SelectedQuestionType = QuestionType.MultipleChoice;
        Load("SinhHoc");
    }

    public void StartEssay()
    {
        GameSessionData.SelectedQuestionType = QuestionType.EssayImage;
        Load("EssayMaze");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static void Load(string sceneName)
    {
        SceneManager.LoadScene(SceneRoot + sceneName + ".unity");
    }
}
