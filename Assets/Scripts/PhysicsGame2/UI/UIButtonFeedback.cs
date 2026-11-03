using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Component hỗ trợ hiệu ứng phản hồi nhẹ (Hover / Press) cho các Button và Hotspot UI.
    /// Giúp tương tác trực quan mà không cần chỉnh sửa Sprite gốc hay tạo Animator phức tạp.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float hoverScale = 1.03f;
        [SerializeField] private float pressScale = 0.97f;
        [SerializeField] private float animSpeed = 12f;

        private Vector3 _originalScale;
        private Vector3 _targetScale;
        private bool _isPointerInside = false;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _targetScale = _originalScale;
        }

        private void OnDisable()
        {
            transform.localScale = _originalScale;
            _targetScale = _originalScale;
            _isPointerInside = false;
        }

        private void Update()
        {
            if (transform.localScale != _targetScale)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.unscaledDeltaTime * animSpeed);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerInside = true;
            _targetScale = _originalScale * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerInside = false;
            _targetScale = _originalScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _targetScale = _originalScale * pressScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _targetScale = _isPointerInside ? (_originalScale * hoverScale) : _originalScale;
        }
    }
}
