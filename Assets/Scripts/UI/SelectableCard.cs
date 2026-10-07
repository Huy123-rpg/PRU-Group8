using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Script dùng chung cho các Card chọn (Subject, LessonType, ChapterRow, Dog)
/// Hiệu ứng Hover: Đổi sang selectedSprite & Phóng to nhẹ khi rê chuột.
/// An toàn bộ nhớ & Tương thích 100% với Multi-Scene Loading (Không bị lỗi MissingReferenceException).
/// </summary>
public class SelectableCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Định danh Card")]
    public string cardId;
    public string groupName = "DefaultGroup";

    [Header("Sprites")]
    public Image targetImage;
    public Sprite normalSprite;
    public Sprite selectedSprite;

    [Header("Trạng thái & Hiệu ứng Hover / Select")]
    public bool isSelected = false;
    public bool isLocked = false;
    [Tooltip("Tỷ lệ phóng to khi hover chuột (VD: 1.08 lần)")]
    public float hoverScaleMultiplier = 1.08f;
    [Tooltip("Tỷ lệ phóng to khi ĐƯỢC CHỌN (VD: 1.15 lần)")]
    public float selectedScaleMultiplier = 1.15f;

    // Sự kiện bắn ra ID khi card được chọn
    public event Action<string> OnCardSelected;

    private static readonly Dictionary<string, List<SelectableCard>> groups = new Dictionary<string, List<SelectableCard>>();
    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticGroups()
    {
        groups.Clear();
    }

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
        originalScale = transform.localScale;

        // Tự động liên kết sự kiện nếu GameObject chứa Button component
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(SelectThisCard);
        }
    }

    private void OnEnable()
    {
        RegisterCard();
        UpdateScaleImmediate();
        UpdateVisuals();
    }

    private void OnDisable()
    {
        UnregisterCard();
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }
    }

    private void UpdateScaleImmediate()
    {
        if (this == null || gameObject == null) return;
        if (isSelected)
            transform.localScale = originalScale * selectedScaleMultiplier;
        else
            transform.localScale = originalScale;
    }

    private void RegisterCard()
    {
        if (string.IsNullOrEmpty(groupName)) return;

        if (!groups.ContainsKey(groupName))
        {
            groups[groupName] = new List<SelectableCard>();
        }

        // Làm sạch các thẻ cũ đã bị Destroy khỏi Group khi nạp Scene mới
        groups[groupName].RemoveAll(c => c == null);

        if (!groups[groupName].Contains(this))
        {
            groups[groupName].Add(this);
        }
    }

    private void UnregisterCard()
    {
        if (!string.IsNullOrEmpty(groupName) && groups.ContainsKey(groupName))
        {
            groups[groupName].Remove(this);
            groups[groupName].RemoveAll(c => c == null);
        }
    }

    #region --- EVENT SYSTEM HANDLERS (HOVER & CLICK) ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isLocked || this == null) return;

        // 1. Phóng to khi rê chuột vào
        float targetMultiplier = isSelected ? selectedScaleMultiplier * 1.04f : hoverScaleMultiplier;
        AnimateScale(originalScale * targetMultiplier);

        // 2. Chuyển sang sprite Highlight (selectedSprite) khi Hover
        if (targetImage != null && selectedSprite != null)
        {
            try
            {
                targetImage.sprite = selectedSprite;
            }
            catch (MissingReferenceException) { }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isLocked || this == null) return;

        // 1. Trở về kích thước tương ứng
        float targetMultiplier = isSelected ? selectedScaleMultiplier : 1.0f;
        AnimateScale(originalScale * targetMultiplier);

        // 2. Nếu thẻ chưa được chọn chính thức -> Trả về normalSprite
        if (!isSelected && targetImage != null && normalSprite != null)
        {
            try
            {
                targetImage.sprite = normalSprite;
            }
            catch (MissingReferenceException) { }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isLocked || this == null) return;

        SelectThisCard();
    }

    private void AnimateScale(Vector3 targetScale)
    {
        if (this == null || gameObject == null || !gameObject.activeInHierarchy)
        {
            if (this != null) transform.localScale = targetScale;
            return;
        }

        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(SmoothScaleCoroutine(targetScale));
    }

    private IEnumerator SmoothScaleCoroutine(Vector3 targetScale)
    {
        float duration = 0.12f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            if (this == null || gameObject == null) yield break;

            elapsed += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
            yield return null;
        }

        if (this != null) transform.localScale = targetScale;
    }

    #endregion

    /// <summary>
    /// Kích hoạt chọn card này và bỏ chọn các card khác trong cùng group
    /// </summary>
    public void SelectThisCard()
    {
        if (isLocked || this == null) return;

        if (!string.IsNullOrEmpty(groupName) && groups.ContainsKey(groupName))
        {
            // Xóa bỏ các reference đã bị Destroy khỏi danh sách
            groups[groupName].RemoveAll(c => c == null);

            foreach (var card in groups[groupName])
            {
                if (card != null && card != this)
                {
                    card.SetSelectedState(false);
                }
            }
        }

        SetSelectedState(true);
        OnCardSelected?.Invoke(cardId);
    }

    public void SetSelectedState(bool selected)
    {
        if (this == null) return;

        isSelected = selected;
        UpdateVisuals();

        float targetMultiplier = isSelected ? selectedScaleMultiplier : 1.0f;
        AnimateScale(originalScale * targetMultiplier);
    }

    private void UpdateVisuals()
    {
        if (this == null || targetImage == null) return;

        try
        {
            if (isSelected && selectedSprite != null)
            {
                targetImage.sprite = selectedSprite;
            }
            else if (!isSelected && normalSprite != null)
            {
                targetImage.sprite = normalSprite;
            }
        }
        catch (MissingReferenceException)
        {
            // Bỏ qua nếu Image component vừa bị hủy trong quá trình chuyển Scene
        }
    }

    public void SetLockedState(bool locked, Sprite lockedSprite = null)
    {
        if (this == null) return;

        isLocked = locked;
        if (isLocked)
        {
            isSelected = false;
            if (targetImage != null)
            {
                try
                {
                    if (lockedSprite != null)
                    {
                        targetImage.sprite = lockedSprite;
                    }
                    else
                    {
                        targetImage.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
                    }
                }
                catch (MissingReferenceException) { }
            }
        }
        else
        {
            if (targetImage != null)
            {
                try
                {
                    targetImage.color = Color.white;
                }
                catch (MissingReferenceException) { }
            }
            UpdateVisuals();
        }
    }
}
