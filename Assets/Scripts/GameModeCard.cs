using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameModeCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public enum GameModeType
    {
        ObstacleRunner,
        Survival
    }

    [Header("Mode Type")]
    public GameModeType modeType = GameModeType.ObstacleRunner;

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
        // These cards also have a Button whose onClick is wired by BiologyGameMenu.
        // Let that handler run once instead of requesting the same scene twice.
        Button button = GetComponent<Button>();
        if (button != null && button.enabled) return;
        if (modeType == GameModeType.ObstacleRunner)
        {
            Debug.Log("[GameModeCard] 🏃 Người dùng chọn: Game Vượt Chướng Ngại Vật");
            BiologyGameMenu.Instance?.OnObstacleRunnerSelected();
        }
        else
        {
            Debug.Log("[GameModeCard] ⛺ Người dùng chọn: Game Sinh Tồn");
            BiologyGameMenu.Instance?.OnSurvivalGameSelected();
        }
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
