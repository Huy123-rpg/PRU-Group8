using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý logic cho Scene 3 — Lesson Type (Chọn Hình Thức Học)
/// 2 Hình thức bài học: 1. Trắc Nghiệm (QuizRaceCard) | 2. Tự Luận (PracticeCard)
/// Tự động tìm kiếm trong Transform Parent để kết nối thẻ chính xác 100%.
/// </summary>
public class LessonTypeUIController : MonoBehaviour
{
    [Header("Mode Selection Cards")]
    public SelectableCard multipleChoiceCard;   // Thẻ Trắc Nghiệm (QuizRaceCard)
    public SelectableCard essayCard;            // Thẻ Tự Luận (PracticeCard)

    [Header("Alternative Buttons (Nút bấm dự phòng)")]
    public Button multipleChoiceButton;
    public Button essayButton;

    private bool hasProceeded = false;

    private void Awake()
    {
        AutoBindComponents();
    }

    private void Start()
    {
        AutoBindComponents();

        // 1. Đăng ký sự kiện qua SelectableCard
        if (multipleChoiceCard != null)
        {
            multipleChoiceCard.OnCardSelected += (id) => SelectModeAndProceed(LessonType.MultipleChoice);
            Debug.Log("[LessonType] Đã kết nối thành công thẻ Trắc Nghiệm: " + multipleChoiceCard.gameObject.name);
        }
        else
        {
            Debug.LogWarning("[LessonType] Cảnh báo: Không tìm thấy thẻ Trắc Nghiệm!");
        }

        if (essayCard != null)
        {
            essayCard.OnCardSelected += (id) => SelectModeAndProceed(LessonType.Essay);
            Debug.Log("[LessonType] Đã kết nối thành công thẻ Tự Luận: " + essayCard.gameObject.name);
        }
        else
        {
            Debug.LogWarning("[LessonType] Cảnh báo: Không tìm thấy thẻ Tự Luận!");
        }

        // 2. Đăng ký sự kiện qua Button component (nếu có)
        if (multipleChoiceButton != null)
        {
            multipleChoiceButton.onClick.AddListener(() => SelectModeAndProceed(LessonType.MultipleChoice));
        }

        if (essayButton != null)
        {
            essayButton.onClick.AddListener(() => SelectModeAndProceed(LessonType.Essay));
        }
    }

    private void AutoBindComponents()
    {
        // Lấy root cha (LessonType) để tìm các thẻ nằm cùng cấp với #Manager
        Transform searchRoot = transform.parent != null ? transform.parent : transform;

        // 1. Tìm thẻ Trắc Nghiệm (QuizRaceCard)
        if (multipleChoiceCard == null)
        {
            Transform t = searchRoot.Find("QuizRaceCard");
            if (t == null) t = searchRoot.Find("QuizCard");
            if (t == null) t = searchRoot.Find("MultipleChoiceCard");
            if (t != null) multipleChoiceCard = t.GetComponent<SelectableCard>();

            if (multipleChoiceCard == null)
            {
                SelectableCard[] cards = searchRoot.GetComponentsInChildren<SelectableCard>(true);
                if (cards.Length > 0) multipleChoiceCard = cards[0];
            }
        }

        // 2. Tìm thẻ Tự Luận (PracticeCard)
        if (essayCard == null)
        {
            Transform t = searchRoot.Find("PracticeCard");
            if (t == null) t = searchRoot.Find("EssayCard");
            if (t != null) essayCard = t.GetComponent<SelectableCard>();

            if (essayCard == null)
            {
                SelectableCard[] cards = searchRoot.GetComponentsInChildren<SelectableCard>(true);
                if (cards.Length > 1) essayCard = cards[1];
            }
        }

        // 3. Tìm Button dự phòng
        if (multipleChoiceButton == null && multipleChoiceCard != null)
        {
            multipleChoiceButton = multipleChoiceCard.GetComponent<Button>();
        }
        if (essayButton == null && essayCard != null)
        {
            essayButton = essayCard.GetComponent<Button>();
        }
    }

    /// <summary>
    /// Lưu hình thức học đã chọn và chuyển sang Scene 4 (Scene4_ChapterSelect)
    /// </summary>
    public void SelectModeAndProceed(LessonType mode)
    {
        if (hasProceeded) return;
        hasProceeded = true;

        if (GameSession.Instance != null)
        {
            GameSession.Instance.lessonType = mode;
        }

        string modeName = mode == LessonType.MultipleChoice ? "Trắc Nghiệm" : "Tự Luận";
        Debug.Log($"[LessonType] Đã chọn hình thức: {modeName}. Chuyển sang Scene 4 (ChapterSelect)...");

        if (NavigationManager.Instance != null)
        {
            NavigationManager.Instance.LoadScene(NavigationManager.SCENE_CHAPTER_SELECT);
        }
        else
        {
            Debug.LogWarning("[LessonType] NavigationManager chưa sẵn sàng, nạp trực tiếp Scene4_ChapterSelect");
            SceneManager.LoadScene("Scene4_ChapterSelect");
        }
    }
}
