using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SubjectCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Subject Info")]
    public string subjectName = "Biology";

    [Header("Sprites")]
    public Image cardImage;
    public Sprite normalSprite;
    public Sprite selectedSprite;

    [Header("Animation Settings")]
    public float hoverScale = 1.05f;
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
        if (cardImage == null)
        {
            cardImage = GetComponent<Image>();
        }
        SetNormalState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectedSprite != null && cardImage != null)
        {
            cardImage.sprite = selectedSprite;
        }
        transform.localScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetNormalState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Đã chọn môn học: " + subjectName);
        MainMenuManager.Instance?.SelectSubject(subjectName);
    }

    public void SetNormalState()
    {
        if (normalSprite != null && cardImage != null)
        {
            cardImage.sprite = normalSprite;
        }
        transform.localScale = originalScale;
    }
}
