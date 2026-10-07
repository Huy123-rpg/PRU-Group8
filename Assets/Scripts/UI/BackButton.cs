using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Gắn vào nút Back ở góc trên-trái mỗi Panel / Scene
/// Hỗ trợ cả Single-Scene Panel Switching và Multi-Scene Loading
/// </summary>
[RequireComponent(typeof(Button))]
public class BackButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnBackClicked);
    }

    private void OnBackClicked()
    {
        // 1. Nếu đang ở màn chọn Bài học trong Scene Chọn Chương, quay lại danh sách Chương
        ChapterSelectUIController chapterController = FindAnyObjectByType<ChapterSelectUIController>();
        if (chapterController != null && chapterController.IsInLessonView)
        {
            chapterController.BackToChapterList();
            return;
        }

        // 2. Mặc định quay lại Scene trước đó
        string currentContext = transform.parent != null ? transform.parent.name : SceneManager.GetActiveScene().name;
        
        if (NavigationManager.Instance != null)
        {
            NavigationManager.Instance.GoBackFrom(currentContext);
        }
        else
        {
            Debug.LogWarning("Chưa tìm thấy NavigationManager Singleton!");
        }
    }
}
