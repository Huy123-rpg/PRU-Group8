using UnityEngine;
using ScienceQuest.UI;

namespace ScienceQuest.Interaction
{
    /// <summary>
    /// InteractionController - Phát hiện các vật thể IInteractable gần người chơi và xử lý nút ấn tương tác (E).
    /// Phân công: M2 - Gameplay Programmer
    /// </summary>
    public class InteractionController : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactRadius = 1.5f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private IInteractable currentInteractable;

        private void Update()
        {
            DetectInteractable();
            HandleInput();
        }

        private void DetectInteractable()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactableLayer);
            
            IInteractable foundInteractable = null;
            float closestDistance = float.MaxValue;

            foreach (var col in colliders)
            {
                if (col.TryGetComponent<IInteractable>(out var interactable))
                {
                    float distance = Vector2.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        foundInteractable = interactable;
                    }
                }
            }

            currentInteractable = foundInteractable;

            // Cập nhật dòng nhắc nhở tương tác trên UIManager
            if (UIManager.Instance != null)
            {
                if (currentInteractable != null)
                {
                    UIManager.Instance.ShowInteractionPrompt(currentInteractable.GetInteractionPrompt());
                }
                else
                {
                    UIManager.Instance.HideInteractionPrompt();
                }
            }
        }

        private void HandleInput()
        {
            if (currentInteractable != null && Input.GetKeyDown(interactKey))
            {
                Debug.Log($"[InteractionController] Đã thực hiện tương tác với object qua phím {interactKey}");
                currentInteractable.Interact();
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Hiển thị bán kính tương tác trong Scene view của Unity Editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
