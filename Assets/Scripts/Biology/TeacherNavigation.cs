using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ScienceQuest.Biology
{
    /// <summary>
    /// Điều hướng dùng chung cho luồng giảng viên môn Sinh.
    /// Nút thoát luôn về màn CHỌN MÔN (SampleScene / MenuGame), không logout.
    /// </summary>
    public static class TeacherNavigation
    {
        public const string SubjectSelectScene = "Scene2_MainMenu";
        public const string TeacherScene = "TeacherCenter";
        public const string ChapterListScene = "ChapterList 1";
        public const string LessonListScene = "LessonList 1";
        public const string OverlayCanvasName = "TeacherNavCanvas";

        /// <summary>
        /// Quay về màn chọn môn. Không xoá session, không hiện lại form đăng nhập.
        /// </summary>
        public static void GoToSubjectSelect()
        {
            LoginManager.SkipLoginOnStart = true;

            if (Application.CanStreamedLevelBeLoaded(SubjectSelectScene))
            {
                SceneManager.LoadScene(SubjectSelectScene);
                return;
            }

            Debug.LogWarning("[TeacherNavigation] Không tìm thấy scene chọn môn (SampleScene).");
        }

        public static void GoToTeacherCenter()
        {
            if (Application.CanStreamedLevelBeLoaded(TeacherScene))
            {
                SceneManager.LoadScene(TeacherScene);
                return;
            }

            Debug.LogError("[TeacherNavigation] Không tìm thấy scene TeacherCenter trong Build Settings.");
        }

        public static void GoToChapterList()
        {
            if (Application.CanStreamedLevelBeLoaded(ChapterListScene))
            {
                SceneManager.LoadScene(ChapterListScene);
            }
        }

        /// <summary>
        /// Tắt canvas overlay của scene khác để chúng không che nút điều hướng.
        /// </summary>
        public static void IsolateOverlayCanvas(Canvas keep)
        {
            if (keep == null) return;

            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Canvas other in canvases)
            {
                if (other == null || other == keep) continue;
                if (other.transform.IsChildOf(keep.transform)) continue;
                if (other.renderMode != RenderMode.ScreenSpaceOverlay) continue;

                GraphicRaycaster raycaster = other.GetComponent<GraphicRaycaster>();
                if (raycaster != null) raycaster.enabled = false;
                other.enabled = false;
            }
        }
    }
}
