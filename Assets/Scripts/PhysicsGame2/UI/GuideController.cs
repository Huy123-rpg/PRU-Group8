using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển toàn bộ logic giao diện Scene 16. Guide.
    /// Tái sử dụng 2 nút hiện có:
    /// - Quay lại: Tải 01_MainMenu.
    /// - Tiếp theo: Đánh dấu hasViewedGuide = true, lưu tiến trình và tải 02_CharacterSelection.
    /// </summary>
    public class GuideController : MonoBehaviour
    {
        [Header("=== BUTTONS (Tự động tìm nếu để trống) ===")]
        [SerializeField] private Button btnBack; // Quay lại
        [SerializeField] private Button btnNext; // Tiếp theo

        private void Awake()
        {
            FindUIReferences();
            BindButtonEvents();
        }

        private void FindUIReferences()
        {
            if (btnBack == null)
            {
                var go = GameObject.Find("Quay lại") ?? GameObject.Find("quaylai") ?? GameObject.Find("Button (1)");
                if (go != null) btnBack = go.GetComponent<Button>();
            }

            if (btnNext == null)
            {
                var go = GameObject.Find("Tiếp theo") ?? GameObject.Find("tieptheo") ?? GameObject.Find("Button");
                if (go != null) btnNext = go.GetComponent<Button>();
            }

            // Fallback theo tọa độ: nút bên trái (X < 0) là Quay lại, nút bên phải (X > 0) là Tiếp theo
            if (btnBack == null || btnNext == null)
            {
                Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include);
                foreach (var b in buttons)
                {
                    if (b.transform.localPosition.x < 0 && btnBack == null)
                    {
                        btnBack = b;
                    }
                    else if (b.transform.localPosition.x > 0 && btnNext == null)
                    {
                        btnNext = b;
                    }
                }
            }

            CleanButtonRaycasts(btnBack);
            CleanButtonRaycasts(btnNext);
        }

        private void CleanButtonRaycasts(Button btn)
        {
            if (btn == null) return;

            var img = btn.GetComponent<Image>();
            if (img != null) img.raycastTarget = true;

            var feedback = btn.GetComponent<UIButtonFeedback>();
            if (feedback != null) Destroy(feedback);

            foreach (var tmp in btn.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
            {
                if (tmp != null) tmp.raycastTarget = false;
            }
        }

        private void BindButtonEvents()
        {
            if (btnBack != null)
            {
                UIHoverPulseEffect.Attach(btnBack.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackClicked);
            }

            if (btnNext != null)
            {
                UIHoverPulseEffect.Attach(btnNext.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
                btnNext.onClick.RemoveAllListeners();
                btnNext.onClick.AddListener(OnNextClicked);
            }
        }

        private void OnBackClicked()
        {
            Debug.Log("[GuideController] Quay lại 01_MainMenu.");
            PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.MainMenu);
        }

        private void OnNextClicked()
        {
            Debug.Log("[GuideController] Tiếp theo -> Kiểm tra cốt truyện mở đầu Aurelia Intro và chuyển sang 02_CharacterSelection.");
            Canvas c = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            StoryFlowController.HandleGuideCompletion(
                customSave: null,
                navigateToCharacterSelection: () =>
                {
                    if (PhysicsGameSession.Instance != null)
                        PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.CharacterSelection);
                },
                canvasTransform: c != null ? c.transform : null);
        }
    }
}
