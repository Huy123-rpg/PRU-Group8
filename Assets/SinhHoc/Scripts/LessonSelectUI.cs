using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LessonSelectUI : MonoBehaviour
{
    [Header("Kéo Title của scene vào đây")]
    public TMP_Text chapterTitle;

    [Header("Kéo MenuManager có MainMenuController vào đây")]
    public MainMenuController menuController;

    [Header("Sáu nút có sẵn, xếp từ trên xuống dưới")]
    public Button[] lessonButtons = new Button[6];

    private void Start()
    {
        if (chapterTitle == null || menuController == null || lessonButtons == null)
        {
            Debug.LogError("LessonSelectUI: chưa gán Title, Menu Controller hoặc Lesson Buttons.", this);
            return;
        }

        string chapterID = GameSessionData.SelectedChapterID;
        // Allows opening LessonSelect directly in the Editor for layout work.
        if (string.IsNullOrEmpty(chapterID))
        {
            chapterID = "C11";
            GameSessionData.SelectedChapterID = chapterID;
        }

        CurriculumChapter chapter = CurriculumCatalog.GetChapter(
            GameSessionData.SelectedSubject, chapterID);

        if (chapter == null)
        {
            foreach (Button button in lessonButtons)
                if (button != null) button.gameObject.SetActive(false);
            chapterTitle.text = "KHÔNG TÌM THẤY CHƯƠNG";
            Debug.LogError("Không tìm thấy chương: " + chapterID, this);
            return;
        }

        if (chapter.lessons.Count > lessonButtons.Length)
        {
            Debug.LogError("LessonSelectUI: chưa đủ ô nút cho tất cả bài trong chương.", this);
            return;
        }

        // Validate all slots before changing the UI.
        foreach (Button button in lessonButtons)
        {
            if (button == null || button.GetComponentInChildren<TMP_Text>(true) == null)
            {
                Debug.LogError("LessonSelectUI: mỗi ô phải có Button và chữ TextMeshPro con.", this);
                return;
            }
        }

        chapterTitle.text = CurriculumCatalog.ChapterShortLabel(chapter);

        for (int i = 0; i < lessonButtons.Length; i++)
        {
            Button button = lessonButtons[i];
            // Replace old Inspector callbacks, including fixed B36...B41 arguments.
            button.onClick = new Button.ButtonClickedEvent();
            bool hasLesson = i < chapter.lessons.Count;
            button.gameObject.SetActive(hasLesson);
            if (!hasLesson) continue;

            CurriculumLesson lesson = chapter.lessons[i];
            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            label.text = "Bài " + lesson.lessonNumber + " — " + lesson.lessonName;
            label.enableAutoSizing = true;
            label.fontSizeMin = 22;
            label.fontSizeMax = 28;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            string lessonID = lesson.lessonID;
            button.onClick.AddListener(() => menuController.OpenLesson(lessonID));
        }
    }
}
