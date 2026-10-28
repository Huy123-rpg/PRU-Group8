using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý logic cho Scene 2 — Main Menu (Chọn Môn Học)
/// Tự động liên kết các Thẻ Môn Học & Nút bấm
/// </summary>
public class MainMenuUIController : MonoBehaviour
{
    [Header("Subject Cards")]
    public SelectableCard bioCard;
    public SelectableCard chemCard;
    public SelectableCard phyCard;

    [Header("Alternative Buttons")]
    public Button bioButton;
    public Button chemButton;
    public Button phyButton;

    [Header("Subject Data Assets (ScriptableObjects)")]
    public SubjectData bioData;
    public SubjectData chemData;
    public SubjectData phyData;

    private bool hasProceeded = false;

    private void Awake()
    {
        AutoBindComponents();
    }

    private void Start()
    {
        AutoBindComponents();

        // 1. Đăng ký sự kiện qua SelectableCard
        if (bioCard != null) bioCard.OnCardSelected += (id) => SelectAndProceed(bioData);
        if (chemCard != null) chemCard.OnCardSelected += (id) => SelectAndProceed(chemData);
        if (phyCard != null) phyCard.OnCardSelected += (id) => SelectAndProceed(phyData);

        // 2. Đăng ký sự kiện qua Button component
        if (bioButton != null) bioButton.onClick.AddListener(() => SelectAndProceed(bioData));
        if (chemButton != null) chemButton.onClick.AddListener(() => SelectAndProceed(chemData));
        if (phyButton != null) phyButton.onClick.AddListener(() => SelectAndProceed(phyData));

        // Tải/Đồng bộ câu hỏi từ Google Sheet ẩn bên dưới nền
        if (GoogleSheetDataManager.Instance != null)
        {
            StartCoroutine(GoogleSheetDataManager.Instance.FetchQuestionsFromSheet());
        }
    }

    private void AutoBindComponents()
    {
        Transform searchRoot = transform.parent != null ? transform.parent : transform;

        SelectableCard[] cards = searchRoot.GetComponentsInChildren<SelectableCard>(true);
        if (bioCard == null && cards.Length > 0) bioCard = cards[0];
        if (chemCard == null && cards.Length > 1) chemCard = cards[1];
        if (phyCard == null && cards.Length > 2) phyCard = cards[2];

        Button[] buttons = searchRoot.GetComponentsInChildren<Button>(true);
        if (bioButton == null && buttons.Length > 0) bioButton = buttons[0];
        if (chemButton == null && buttons.Length > 1) chemButton = buttons[1];
        if (phyButton == null && buttons.Length > 2) phyButton = buttons[2];
    }

    private void SelectAndProceed(SubjectData subject)
    {
        if (hasProceeded) return;
        hasProceeded = true;

        if (GameSession.Instance != null && subject != null)
        {
            GameSession.Instance.selectedSubject = subject;
        }

        string subjectName = subject != null ? subject.subjectName : "Môn Học";
        string lower = subjectName.ToLower();

        // 1. Phân luồng môn Vật Lý
        if (lower.Contains("physic") || lower.Contains("vật lý") || lower.Contains("vật lí") || lower.Contains("phy"))
        {
            Debug.Log($"[MainMenu] ⚡ Đã chọn môn Vật Lý: {subjectName}. Nạp GameSelection...");
            if (Application.CanStreamedLevelBeLoaded("GameSelection"))
            {
                SceneManager.LoadScene("GameSelection");
            }
            else if (Application.CanStreamedLevelBeLoaded("01_MainMenu"))
            {
                SceneManager.LoadScene("01_MainMenu");
            }
            else
            {
                SceneManager.LoadScene(0);
            }
            return;
        }

        // 2. Phân luồng môn Sinh Học
        if (lower.Contains("bio") || lower.Contains("sinh"))
        {
            Debug.Log($"[MainMenu] 🌿 Đã chọn môn Sinh Học: {subjectName}. Nạp ChapterList 1...");
            if (Application.CanStreamedLevelBeLoaded("ChapterList 1"))
            {
                SceneManager.LoadScene("ChapterList 1");
            }
            else if (Application.CanStreamedLevelBeLoaded("SinhHoc"))
            {
                SceneManager.LoadScene("SinhHoc");
            }
            else
            {
                SceneManager.LoadScene(0);
            }
            return;
        }

        // 3. Phân luồng môn Hóa Học (Chemistry)
        Debug.Log($"[MainMenu] 🧪 Đã chọn môn Hóa Học: {subjectName}. Chuyển sang Scene 3 (LessonType)...");
        if (NavigationManager.Instance != null)
        {
            NavigationManager.Instance.LoadScene(NavigationManager.SCENE_LESSON_TYPE);
        }
        else
        {
            Debug.LogWarning("[MainMenu] NavigationManager chưa sẵn sàng, nạp trực tiếp Scene3_LessonType");
            SceneManager.LoadScene("Scene3_LessonType");
        }
    }
}
