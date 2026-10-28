using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScienceQuest.Core
{
    /// <summary>
    /// SceneLoader - Quản lý việc chuyển đổi giữa các Scene trong game bằng SceneManager.
    /// Phân công: M1 - Core Programmer / Team Lead
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        // Tên các Scene chuẩn trong dự án SCIENCE QUEST – KHTN 8
        public const string SCENE_LOGIN = "Scene1_Login";
        public const string SCENE_MAIN_MENU = "Scene2_MainMenu";
        public const string SCENE_ACADEMY = "Academy";
        public const string SCENE_PHYSICS = "07_DuckShooting";
        public const string SCENE_CHEMISTRY = "Scene3_LessonType";
        public const string SCENE_BIOLOGY = "ChapterList 1";
        public const string SCENE_CHARACTER_SELECTION = "02_CharacterSelection";

        // Scene Quiz trắc nghiệm
        public const string SCENE_QUIZ_PHYSICS = "07_DuckShooting";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitializeOnLoad()
        {
            if (FindAnyObjectByType<SceneLoader>() == null)
            {
                GameObject go = new GameObject("SceneLoader");
                go.AddComponent<SceneLoader>();
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Chuyển scene theo tên (load đồng bộ an toàn)
        /// </summary>
        /// <param name="sceneName">Tên scene cần nạp (Ví dụ: "01_MainMenu")</param>
        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[SceneLoader] Tên Scene không hợp lệ!");
                return;
            }

            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.Log($"[SceneLoader] Đang nạp Scene: {sceneName}...");
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning($"[SceneLoader] ⚠️ Scene '{sceneName}' không có trong Build Settings! Đang tìm fallback...");
                if (sceneName.Contains("Physic") && Application.CanStreamedLevelBeLoaded("01_MainMenu"))
                    SceneManager.LoadScene("01_MainMenu");
                else if (sceneName.Contains("Bio") && Application.CanStreamedLevelBeLoaded("ChapterList 1"))
                    SceneManager.LoadScene("ChapterList 1");
                else if (Application.CanStreamedLevelBeLoaded("Scene2_MainMenu"))
                    SceneManager.LoadScene("Scene2_MainMenu");
                else if (Application.CanStreamedLevelBeLoaded("MainMenu"))
                    SceneManager.LoadScene("MainMenu");
                else
                    SceneManager.LoadScene(0);
            }
        }

        /// <summary>
        /// Chuyển sang Main Menu
        /// </summary>
        public void LoadMainMenu() => LoadScene(SCENE_MAIN_MENU);

        /// <summary>
        /// Chuyển sang khu vực Học viện Academy
        /// </summary>
        public void LoadAcademy() => LoadScene(SCENE_ACADEMY);

        /// <summary>
        /// Chuyển sang khu vực Vật lý
        /// </summary>
        public void LoadPhysics() => LoadScene(SCENE_PHYSICS);

        /// <summary>
        /// Chuyển sang khu vực Hóa học
        /// </summary>
        public void LoadChemistry() => LoadScene(SCENE_CHEMISTRY);

        /// <summary>
        /// Chuyển sang khu vực Sinh học
        /// </summary>
        public void LoadBiology() => LoadScene(SCENE_BIOLOGY);

        /// <summary>
        /// Chuyển sang màn hình Quiz Vật lý (tích hợp trực tiếp vào màn Bắn Vịt)
        /// </summary>
        /// <param name="chapter">Tên chương muốn quiz (để trống = random tất cả)</param>
        public void LoadQuizPhysics(string chapter = "")
        {
            PlayerPrefs.SetString("QuizChapter", chapter);
            PlayerPrefs.Save();
            LoadScene(SCENE_PHYSICS);
        }
    }
}
