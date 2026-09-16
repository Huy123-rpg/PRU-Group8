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
        public static GameManager Instance { get; private set; }

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
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SubscribeToSceneEvents();
            }
            else
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
        /// Thêm Coins cho người chơi
        /// </summary>
        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            coins += amount;
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
            OnEXPChanged?.Invoke(playerEXP);

            // Công thức tính EXP nâng cấp đơn giản: Level * 100
            int expRequired = playerLevel * 100;
            if (playerEXP >= expRequired)
            {
                playerEXP -= expRequired;
                playerLevel++;
                Debug.Log($"[GameManager] CHÚC MỪNG! Người chơi đã đạt Level {playerLevel}!");
                OnLevelUp?.Invoke(playerLevel);
            }
        }

        /// <summary>
        /// Reset lại dữ liệu khi người chơi tạo game mới
        /// </summary>
        public void ResetProgress()
        {
            playerLevel = 1;
            playerEXP = 0;
            coins = 0;
            OnCoinsChanged?.Invoke(coins);
            OnEXPChanged?.Invoke(playerEXP);
            OnLevelUp?.Invoke(playerLevel);
        }
    }
}
