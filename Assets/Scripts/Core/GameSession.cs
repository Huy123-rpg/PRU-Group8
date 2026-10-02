using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton quản lý session dữ liệu xuyên suốt các scene (DontDestroyOnLoad - Safe Singleton)
/// </summary>
public class GameSession : MonoBehaviour
{
    private static GameSession instance;
    public static GameSession Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameSession>();
                if (instance == null)
                {
                    GameObject go = new GameObject("[GameSession]");
                    instance = go.AddComponent<GameSession>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("Thông tin người dùng hiện tại")]
    public UserAccount currentUser;
    public string username => currentUser != null ? currentUser.username : "Học viên";
    public bool IsTeacher => currentUser != null && currentUser.role == UserRole.Teacher;

    [Header("Lựa chọn trong game")]
    public SubjectData selectedSubject;
    public LessonType lessonType = LessonType.MultipleChoice;
    public ChapterData selectedChapter;
    public int selectedDogId = 1;

    [Header("Kho câu hỏi tải từ Google Sheet")]
    public List<QuestionData> loadedQuestions = new List<QuestionData>();

    [Header("Kết quả đua & Bảng xếp hạng từ Sheet")]
    public RaceResult lastRaceResult;
    public List<RaceResult> leaderboardHistory = new List<RaceResult>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void SetUserSession(UserAccount account)
    {
        currentUser = account;
    }

    public void ResetSession()
    {
        currentUser = null;
        selectedSubject = null;
        selectedChapter = null;
        selectedDogId = 1;
        lastRaceResult = null;
        loadedQuestions.Clear();
    }
}
