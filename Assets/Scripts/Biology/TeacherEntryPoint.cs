using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.Biology
{
    /// <summary>
    /// Nút điều hướng Trung tâm GV và Chọn môn.
    /// Theo yêu cầu người dùng: Đã loại bỏ hoàn toàn 2 nút này trên ChapterList 1 và LessonList 1.
    /// </summary>
    public class TeacherEntryPoint : MonoBehaviour
    {
        public const string TeacherSceneName = TeacherNavigation.TeacherScene;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitializeOnLoad()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Theo yêu cầu: Loại bỏ hoàn toàn 2 nút Trung tâm GV và Chọn môn ở màn chọn chương và chọn bài
            return;
        }

        private void Start()
        {
            Destroy(gameObject);
        }
    }
}
