using UnityEngine;
using UnityEngine.UI;
using ScienceQuest.Core;

namespace ScienceQuest.UI
{
    /// <summary>
    /// UIManager - Quản lý giao diện người dùng HUD (Level, EXP, Coins, Interaction Prompt, Scene Transition buttons).
    /// Phân công: M3 - UI/UX Designer / Programmer
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("HUD References (Optional UI Text Binding)")]
        [SerializeField] private Text sceneNameText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text expText;
        [SerializeField] private Text coinsText;
        [SerializeField] private Text interactionPromptText;
        [SerializeField] private GameObject interactionPromptPanel;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                // Đăng ký nhận sự kiện cập nhật từ GameManager
                GameManager.Instance.OnCoinsChanged += UpdateCoinsUI;
                GameManager.Instance.OnEXPChanged += UpdateEXPUI;
                GameManager.Instance.OnLevelUp += UpdateLevelUI;
                GameManager.Instance.OnSceneChanged += UpdateSceneNameUI;

                // Khởi tạo các giá trị ban đầu
                UpdateCoinsUI(GameManager.Instance.Coins);
                UpdateEXPUI(GameManager.Instance.PlayerEXP);
                UpdateLevelUI(GameManager.Instance.PlayerLevel);
                UpdateSceneNameUI(GameManager.Instance.CurrentSceneName);
            }

            HideInteractionPrompt();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnCoinsChanged -= UpdateCoinsUI;
                GameManager.Instance.OnEXPChanged -= UpdateEXPUI;
                GameManager.Instance.OnLevelUp -= UpdateLevelUI;
                GameManager.Instance.OnSceneChanged -= UpdateSceneNameUI;
            }
        }

        public void UpdateSceneNameUI(string sceneName)
        {
            if (sceneNameText != null)
                sceneNameText.text = $"Khu vực: {sceneName}";
        }

        public void UpdateLevelUI(int level)
        {
            if (levelText != null)
                levelText.text = $"Level: {level}";
        }

        public void UpdateEXPUI(int exp)
        {
            if (expText != null)
                expText.text = $"EXP: {exp}";
        }

        public void UpdateCoinsUI(int coins)
        {
            if (coinsText != null)
                coinsText.text = $"Coins: {coins}";
        }

        public void ShowInteractionPrompt(string message)
        {
            if (interactionPromptPanel != null)
                interactionPromptPanel.SetActive(true);

            if (interactionPromptText != null)
                interactionPromptText.text = message;
        }

        public void HideInteractionPrompt()
        {
            if (interactionPromptPanel != null)
                interactionPromptPanel.SetActive(false);
        }

        // ==========================================
        // UI BUTTON EVENT HANDLERS (Gắn vào Button OnClick)
        // ==========================================

        public void Btn_LoadMainMenu() => SceneLoader.Instance?.LoadMainMenu();
        public void Btn_LoadAcademy() => SceneLoader.Instance?.LoadAcademy();
        public void Btn_LoadPhysics() => SceneLoader.Instance?.LoadPhysics();
        public void Btn_LoadChemistry() => SceneLoader.Instance?.LoadChemistry();
        public void Btn_LoadBiology() => SceneLoader.Instance?.LoadBiology();
    }
}
