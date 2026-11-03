using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Component hiệu ứng Hover Pulse dùng chung cho tất cả các đối tượng tương tác trong game Aurelia:
    /// - Khi chuột chưa vào: Hiển thị đúng kích thước và vị trí ban đầu (Scale 100%).
    /// - Khi di chuột vào (hoặc Select): Phóng to mượt từ 100% lên ~108%, nhấp nhô nhẹ giữa 105% và 110%.
    /// - Chuyển động liên tục, mềm mại, không giật (dùng Time.unscaledTime / unscaledDeltaTime).
    /// - Hiệu ứng viền sáng (Outline Glow) nổi bật, tinh tế màu vàng hoàng gia Aurelia Gold.
    /// - Khi chuột rời khỏi: Dừng nhấp nhô, thu nhỏ mượt về đúng kích thước và vị trí ban đầu (không để lại sai lệch).
    /// - Khi click chọn (Selected): Giữ trạng thái viền sáng và scale nổi bật, các đối tượng khác trở về bình thường.
    /// - Tương thích Canvas Scaler, Button.onClick, và hỗ trợ thiết bị không chuột (trạng thái press / select).
    /// - Hỗ trợ cả UI (IPointerEnterHandler) và đối tượng trong game có Collider (OnMouseEnter/Exit).
    /// </summary>
    [DisallowMultipleComponent]
    public class UIHoverPulseEffect : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler,
        ISelectHandler, IDeselectHandler
    {
        [Header("=== SCALE SETTINGS ===")]
        [Tooltip("Hệ số phóng to khi di chuột vào (mặc định 1.08 = 108%)")]
        [SerializeField] private float hoverScale = 1.08f;

        [Tooltip("Biên độ nhấp nhô tối thiểu khi hover (1.05 = 105%)")]
        [SerializeField] private float pulseMinScale = 1.05f;

        [Tooltip("Biên độ nhấp nhô tối đa khi hover (1.10 = 110%)")]
        [SerializeField] private float pulseMaxScale = 1.10f;

        [Tooltip("Tốc độ chu kỳ nhấp nhô (mềm mại, không giật)")]
        [SerializeField] private float pulseFrequency = 3.2f;

        [Tooltip("Tốc độ chuyển đổi giữa các trạng thái phóng to/thu nhỏ")]
        [SerializeField] private float transitionSpeed = 10.0f;

        [Tooltip("Hệ số scale khi ấn giữ (click/touch feedback)")]
        [SerializeField] private float pressScale = 0.96f;

        [Tooltip("Hệ số scale duy trì khi đối tượng đang ở trạng thái Selected")]
        [SerializeField] private float selectedScale = 1.08f;

        [Header("=== OUTLINE / GLOW SETTINGS ===")]
        [Tooltip("Bật hiệu ứng viền phát sáng khi hover hoặc click chọn")]
        [SerializeField] private bool enableOutlineGlow = true;

        [Tooltip("Màu viền sáng khi hover")]
        [SerializeField] private Color hoverGlowColor = new Color(1f, 0.92f, 0.45f, 0.85f);

        [Tooltip("Màu viền sáng hoàng gia khi được click chọn (Aurelia Gold)")]
        [SerializeField] private Color selectedGlowColor = new Color(0.96f, 0.82f, 0.38f, 1f);

        [Tooltip("Khoảng cách viền phát sáng")]
        [SerializeField] private Vector2 outlineDistance = new Vector2(4f, -4f);

        [Header("=== STATE (READ ONLY IN PLAY MODE) ===")]
        [SerializeField] private bool isHovered = false;
        [SerializeField] private bool isPressed = false;
        [SerializeField] private bool isSelected = false;

        private Vector3 _initialScale = Vector3.one;
        private Vector3 _targetScale = Vector3.one;
        private float _pulseTimer = 0f;
        private Outline _outline;
        private bool _hasOriginalOutline = false;
        private Color _originalOutlineColor;
        private bool _isInitialized = false;

        public bool IsSelected => isSelected;
        public bool IsHovered => isHovered;

        private void Awake()
        {
            InitializeIfNeeded();
        }

        private void Start()
        {
            InitializeIfNeeded();
        }

        public void InitializeIfNeeded()
        {
            if (_isInitialized) return;

            _initialScale = transform.localScale;
            // Tránh trường hợp scale ban đầu bằng 0 do animation load trước đó
            if (_initialScale.sqrMagnitude < 0.001f)
            {
                _initialScale = Vector3.one;
            }

            _targetScale = isSelected ? (_initialScale * selectedScale) : _initialScale;

            if (enableOutlineGlow)
            {
                _outline = GetComponent<Outline>();
                if (_outline != null)
                {
                    _hasOriginalOutline = true;
                    _originalOutlineColor = _outline.effectColor;
                }
            }

            _isInitialized = true;
            UpdateVisualState(instant: true);
        }

        private void OnEnable()
        {
            if (_isInitialized)
            {
                _pulseTimer = 0f;
                _targetScale = isSelected ? (_initialScale * selectedScale) : _initialScale;
                transform.localScale = _targetScale;
                UpdateVisualState(instant: true);
            }
        }

        private void OnDisable()
        {
            // Phục hồi nguyên trạng 100% khi tắt hoặc chuyển màn, không để lại sai lệch
            isHovered = false;
            isPressed = false;
            transform.localScale = isSelected ? (_initialScale * selectedScale) : _initialScale;

            if (_outline != null)
            {
                if (isSelected)
                {
                    _outline.enabled = true;
                    _outline.effectColor = selectedGlowColor;
                }
                else if (_hasOriginalOutline)
                {
                    _outline.enabled = true;
                    _outline.effectColor = _originalOutlineColor;
                }
                else
                {
                    _outline.enabled = false;
                }
            }
        }

        private void Update()
        {
            // Tính toán target scale theo trạng thái hiện tại
            if (isPressed)
            {
                _targetScale = _initialScale * pressScale;
            }
            else if (isHovered)
            {
                // Chu kỳ nhấp nhô nhẹ nhàng, liên tục, mượt mà giữa pulseMinScale (105%) và pulseMaxScale (110%)
                _pulseTimer += Time.unscaledDeltaTime;
                float sinVal = (Mathf.Sin(_pulseTimer * pulseFrequency) + 1f) * 0.5f; // 0.0 -> 1.0
                float currentPulse = Mathf.Lerp(pulseMinScale, pulseMaxScale, sinVal);
                _targetScale = _initialScale * currentPulse;
            }
            else if (isSelected)
            {
                // Khi đang được chọn và không hover: giữ kích thước nổi bật nhẹ nhàng
                _targetScale = _initialScale * selectedScale;
            }
            else
            {
                // Khi chuột đã rời khỏi: trở về kích thước ban đầu 100%
                _targetScale = _initialScale;
                _pulseTimer = 0f;
            }

            // Chuyển động nội suy mượt mà không phụ thuộc fps hay timeScale
            float lerpFactor = Mathf.Clamp01(Time.unscaledDeltaTime * transitionSpeed);
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, lerpFactor);

            // Khớp chính xác khi đã tiệm cận để không để lại sai lệch dù là số thập phân nhỏ
            if (!isHovered && Vector3.Distance(transform.localScale, _targetScale) < 0.001f)
            {
                transform.localScale = _targetScale;
            }
        }

        /// <summary>
        /// Đặt trạng thái click chọn cho đối tượng (Selected).
        /// Khi được chọn: Giữ viền sáng và phóng to nổi bật.
        /// Khi bỏ chọn: Trở về bình thường.
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisualState(instant: false);
        }

        /// <summary>
        /// Cập nhật hiển thị outline glow và scale mục tiêu
        /// </summary>
        private void UpdateVisualState(bool instant = false)
        {
            if (enableOutlineGlow)
            {
                if (_outline == null)
                {
                    _outline = GetComponent<Outline>();
                    if (_outline == null && (isHovered || isSelected))
                    {
                        _outline = gameObject.AddComponent<Outline>();
                        _outline.effectDistance = outlineDistance;
                    }
                }

                if (_outline != null)
                {
                    if (isSelected)
                    {
                        _outline.enabled = true;
                        _outline.effectColor = selectedGlowColor;
                        _outline.effectDistance = outlineDistance;
                    }
                    else if (isHovered)
                    {
                        _outline.enabled = true;
                        _outline.effectColor = hoverGlowColor;
                        _outline.effectDistance = outlineDistance;
                    }
                    else
                    {
                        if (_hasOriginalOutline)
                        {
                            _outline.enabled = true;
                            _outline.effectColor = _originalOutlineColor;
                        }
                        else
                        {
                            _outline.enabled = false;
                        }
                    }
                }
            }

            if (instant)
            {
                _targetScale = isSelected ? (_initialScale * selectedScale) : _initialScale;
                transform.localScale = _targetScale;
            }
        }

        // =====================================================================
        // UI POINTER / SELECT EVENTS (EventSystem / Mouse / Touch / Gamepad)
        // =====================================================================

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            UpdateVisualState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            isPressed = false;
            UpdateVisualState();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            // Thiết bị không có chuột (bàn phím / tay cầm / điều hướng accessibility)
            isHovered = true;
            UpdateVisualState();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            isHovered = false;
            isPressed = false;
            UpdateVisualState();
        }

        // =====================================================================
        // COLLIDER EVENTS (Dành cho đối tượng thế giới 2D/3D có Collider)
        // =====================================================================

        private void OnMouseEnter()
        {
            isHovered = true;
            UpdateVisualState();
        }

        private void OnMouseExit()
        {
            isHovered = false;
            isPressed = false;
            UpdateVisualState();
        }

        private void OnMouseDown()
        {
            isPressed = true;
        }

        private void OnMouseUp()
        {
            isPressed = false;
        }

        // =====================================================================
        // HELPER STATIC METHODS
        // =====================================================================

        /// <summary>
        /// Gắn hoặc lấy component UIHoverPulseEffect trên một GameObject.
        /// </summary>
        public static UIHoverPulseEffect Attach(GameObject go, float hover = 1.08f, float minPulse = 1.05f, float maxPulse = 1.10f, bool outline = true)
        {
            if (go == null) return null;

            var comp = go.GetComponent<UIHoverPulseEffect>();
            if (comp == null) comp = go.AddComponent<UIHoverPulseEffect>();

            comp.hoverScale = hover;
            comp.pulseMinScale = minPulse;
            comp.pulseMaxScale = maxPulse;
            comp.enableOutlineGlow = outline;
            comp.InitializeIfNeeded();

            return comp;
        }
    }
}
