using UnityEngine;
using ScienceQuest.Core;

namespace ScienceQuest.Interaction
{
    public enum InteractionType
    {
        Message,
        LoadScene,
        Reward
    }

    /// <summary>
    /// InteractableObject - Component mẫu cài đặt IInteractable dùng cho NPC, Cửa chuyển màn, Vật thể Khoa học.
    /// Phân công: M2 - Gameplay Programmer
    /// </summary>
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        [Header("Interaction Configuration")]
        [SerializeField] private string promptMessage = "Nhấn E để tương tác";
        [SerializeField] private InteractionType interactionType = InteractionType.Message;

        [Header("Options for Message / Dialogue")]
        [TextArea(2, 4)]
        [SerializeField] private string dialogueText = "Chào mừng bạn đến với Science Quest KHTN 8!";

        [Header("Options for Load Scene")]
        [SerializeField] private string targetSceneName = "Academy";

        [Header("Options for Reward")]
        [SerializeField] private int rewardCoins = 10;
        [SerializeField] private int rewardEXP = 20;

        public string GetInteractionPrompt()
        {
            return promptMessage;
        }

        public void Interact()
        {
            switch (interactionType)
            {
                case InteractionType.Message:
                    Debug.Log($"[InteractableObject - {gameObject.name}] Thông điệp: {dialogueText}");
                    break;

                case InteractionType.LoadScene:
                    Debug.Log($"[InteractableObject - {gameObject.name}] Chuyển tới scene: {targetSceneName}");
                    if (SceneLoader.Instance != null)
                    {
                        SceneLoader.Instance.LoadScene(targetSceneName);
                    }
                    else
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
                    }
                    break;

                case InteractionType.Reward:
                    Debug.Log($"[InteractableObject - {gameObject.name}] Nhận phần thưởng: {rewardCoins} Coins & {rewardEXP} EXP");
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.AddCoins(rewardCoins);
                        GameManager.Instance.AddEXP(rewardEXP);
                    }
                    break;
            }
        }
    }
}
