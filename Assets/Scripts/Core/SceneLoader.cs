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
        public const string SCENE_MAIN_MENU = "MainMenu";
        public const string SCENE_ACADEMY = "Academy";
        public const string SCENE_PHYSICS = "Physics";
        public const string SCENE_CHEMISTRY = "Chemistry";
        public const string SCENE_BIOLOGY = "Biology";

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
        /// Chuyển scene theo tên (load đồng bộ đơn giản)
        /// </summary>
        /// <param name="sceneName">Tên scene cần nạp (Ví dụ: "Academy")</param>
        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[SceneLoader] Tên Scene không hợp lệ!");
                return;
            }

            Debug.Log($"[SceneLoader] Đang nạp Scene: {sceneName}...");
            SceneManager.LoadScene(sceneName);
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
    }
}
