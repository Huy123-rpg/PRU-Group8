using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI selectedSubjectText;

    [Header("Scenes Phân hệ Sinh học (Folder Sinh)")]
    [Tooltip("Scene dành cho Học sinh (Player)")]
    public string bioStudentSceneName = "ChapterList 1";
    [Tooltip("Scene dành cho Giảng viên (Teacher)")]
    public string bioTeacherSceneName = "TeacherCenter";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SelectSubject(string subjectName)
    {
        if (selectedSubjectText != null)
        {
            selectedSubjectText.text = "Môn đã chọn: " + subjectName;
        }

        Debug.Log("Chuẩn bị vào màn chơi cho môn: " + subjectName);

        string lower = subjectName.ToLower();
        if (lower.Contains("physic") || lower.Contains("vật lý") || lower.Contains("vật lí"))
        {
            SceneManager.LoadScene("Physics");
        }
        else if (lower.Contains("sinh") || lower.Contains("bio"))
        {
            // Click vào phần Sinh học là tự động chọn scene theo Role
            EnterSinhSubject();
        }
    }

    /// <summary>
    /// Vào môn Sinh học (Folder "Sinh") và tự động chuyển scene theo role:
    /// - Học sinh (Player): Vào ChapterList 1 để chơi như bình thường.
    /// - Giảng viên (Teacher): Vào TeacherCenter để xem thống kê điểm số và thêm/quản lý câu hỏi.
    /// </summary>
    public void EnterSinhSubject()
    {
        bool isTeacher = LoginManager.IsCurrentTeacher;
        string currentUser = LoginManager.CurrentUsername;

        Debug.Log($"[MainMenuManager] Chọn môn Sinh học. User: '{currentUser}', IsTeacher: {isTeacher}");

        if (isTeacher)
        {
            Debug.Log($"[MainMenuManager] 🧑‍🏫 Giảng viên '{currentUser}' -> Mở Trung tâm Quản lý & Thống kê ({bioTeacherSceneName})");
            if (Application.CanStreamedLevelBeLoaded(bioTeacherSceneName))
            {
                SceneManager.LoadScene(bioTeacherSceneName);
            }
            else
            {
                Debug.LogError($"[MainMenuManager] Không tìm thấy scene '{bioTeacherSceneName}'. Hãy thêm scene vào Build Settings!");
                SceneManager.LoadScene(bioStudentSceneName);
            }
        }
        else
        {
            Debug.Log($"[MainMenuManager] 🎮 Học sinh '{currentUser}' (Player) -> Vào màn chơi như bình thường ({bioStudentSceneName})");
            SceneManager.LoadScene(bioStudentSceneName);
        }
    }

    public void OnBackToLoginClicked()
    {
        // Handle returning to login screen
        LoginManager loginManager = FindObjectOfType<LoginManager>(true);
        if (loginManager != null && loginManager.usePanelSwitching)
        {
            if (loginManager.menuGamePanel != null) loginManager.menuGamePanel.SetActive(false);
            if (loginManager.loginPanel != null) loginManager.loginPanel.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene("SampleScene"); // Or Login scene name
        }
    }
}
