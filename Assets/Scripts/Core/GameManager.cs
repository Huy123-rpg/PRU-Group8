using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScienceQuest.Core
{
    /// <summary>
    /// GameManager - Singleton đơn giản quản lý trạng thái chung của game (Level, EXP, Coins, CurrentScene).
    /// Phân công: M1 - Core Programmer / Team Lead
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }
        private static GameManager _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("GameManager");
                _instance = go.AddComponent<GameManager>();
                DontDestroyOnLoad(go);
            }
        }

        [Header("Player Progress State")]
        [SerializeField] private int playerLevel = 1;
        [SerializeField] private int playerEXP = 0;
        [SerializeField] private int coins = 0;
        [SerializeField] private string currentSceneName = "MainMenu";

        // Properties công khai để truy cập thông tin (Read-only từ bên ngoài)
        public int PlayerLevel => playerLevel;
        public int PlayerEXP => playerEXP;
        public int Coins => coins;
        public string CurrentSceneName => currentSceneName;

        // Delegates / Events thông báo khi dữ liệu thay đổi
        public System.Action<int> OnCoinsChanged;
        public System.Action<int> OnEXPChanged;
        public System.Action<int> OnLevelUp;
        public System.Action<string> OnSceneChanged;

        private void Awake()
        {
            // Thiết lập Singleton pattern đơn giản
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadProgress();
                SubscribeToSceneEvents();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            currentSceneName = SceneManager.GetActiveScene().name;
        }

        private void SubscribeToSceneEvents()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            currentSceneName = scene.name;
            OnSceneChanged?.Invoke(currentSceneName);
            Debug.Log($"[GameManager] Đã chuyển sang Scene: {currentSceneName}");
        }

        /// <summary>
        /// Lấy danh hiệu theo Level hiện tại
        /// </summary>
        public string GetCurrentTitle()
        {
            switch (playerLevel)
            {
                case 1: return "Tập sự Vật Lý";
                case 2: return "Học sinh giỏi Vật Lý";
                case 3: return "Nhà Bác học trẻ";
                case 4: return "Bậc thầy Vật Lý";
                default: return "Thiên tài Vật Lý";
            }
        }

        /// <summary>
        /// EXP cần để lên Level tiếp theo: Level 1 cần 50, Level 2 cần 100, Level 3 cần 150...
        /// </summary>
        public int GetExpRequiredForNextLevel()
        {
            return playerLevel * 50;
        }

        /// <summary>
        /// Kiểm tra xem Chương có được mở khóa chưa - Tất cả các chương luôn mở khóa tự do
        /// </summary>
        public bool IsChapterUnlocked(int chapterIndex)
        {
            return true;
        }

        /// <summary>
        /// Thêm Coins cho người chơi
        /// </summary>
        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            coins += amount;
            SaveProgress();
            Debug.Log($"[GameManager] +{amount} Coins. Tổng Coins: {coins}");
            OnCoinsChanged?.Invoke(coins);
        }

        /// <summary>
        /// Thêm Kinh nghiệm (EXP) và tự động lên cấp nếu đủ điều kiện
        /// </summary>
        public void AddEXP(int amount)
        {
            if (amount <= 0) return;
            playerEXP += amount;
            Debug.Log($"[GameManager] +{amount} EXP. Tổng EXP: {playerEXP}");

            int expRequired = GetExpRequiredForNextLevel();
            while (playerEXP >= expRequired)
            {
                playerEXP -= expRequired;
                playerLevel++;
                Debug.Log($"[GameManager] 🎉 CHÚC MỪNG! Lên Level {playerLevel}: {GetCurrentTitle()}!");
                OnLevelUp?.Invoke(playerLevel);
                expRequired = GetExpRequiredForNextLevel();
            }

            SaveProgress();
            OnEXPChanged?.Invoke(playerEXP);
        }

        /// <summary>
        /// Lưu tiến trình vào PlayerPrefs
        /// </summary>
        public void SaveProgress()
        {
            PlayerPrefs.SetInt("PlayerLevel", playerLevel);
            PlayerPrefs.SetInt("PlayerEXP", playerEXP);
            PlayerPrefs.SetInt("PlayerCoins", coins);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Tải tiến trình từ PlayerPrefs
        /// </summary>
        public void LoadProgress()
        {
            playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
            playerEXP = PlayerPrefs.GetInt("PlayerEXP", 0);
            coins = PlayerPrefs.GetInt("PlayerCoins", 0);
        }

        /// <summary>
        /// Reset lại dữ liệu khi người chơi tạo game mới
        /// </summary>
        public void ResetProgress()
        {
            playerLevel = 1;
            playerEXP = 0;
            coins = 0;
            SaveProgress();
            OnCoinsChanged?.Invoke(coins);
            OnEXPChanged?.Invoke(playerEXP);
            OnLevelUp?.Invoke(playerLevel);
        }
    }
}
