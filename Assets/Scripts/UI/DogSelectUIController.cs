using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý logic cho Scene 5 — Dog Select (Chọn Chú Chó Đua)
/// Cho phép chọn chú chó thi đấu (1-4), lưu vào GameSession và chuyển sang Scene 6 (Race)
/// </summary>
public class DogSelectUIController : MonoBehaviour
{
    [Header("Dog Selection Cards")]
    public SelectableCard dog1Card; // Chó số 1
    public SelectableCard dog2Card; // Chó số 2
    public SelectableCard dog3Card; // Chó số 3
    public SelectableCard dog4Card; // Chó số 4

    [Header("Start Race Button")]
    public Button startRaceButton;

    private int selectedDogId = 1;
    private bool isNavigating = false;

    private void OnEnable()
    {
        isNavigating = false;
    }

    private void Awake()
    {
        AutoBindComponents();
    }

    private void Start()
    {
        AutoBindComponents();

        // 1. Đăng ký sự kiện qua SelectableCard
        if (dog1Card != null) dog1Card.OnCardSelected += (id) => OnDogSelected(1);
        if (dog2Card != null) dog2Card.OnCardSelected += (id) => OnDogSelected(2);
        if (dog3Card != null) dog3Card.OnCardSelected += (id) => OnDogSelected(3);
        if (dog4Card != null) dog4Card.OnCardSelected += (id) => OnDogSelected(4);

        // 2. Đăng ký sự kiện cho nút Bắt đầu
        if (startRaceButton != null)
        {
            startRaceButton.onClick.AddListener(OnStartRaceClicked);
        }
    }

    private void AutoBindComponents()
    {
        Transform searchRoot = transform.parent != null ? transform.parent : transform;

        SelectableCard[] cards = searchRoot.GetComponentsInChildren<SelectableCard>(true);
        if (dog1Card == null && cards.Length > 0) dog1Card = cards[0];
        if (dog2Card == null && cards.Length > 1) dog2Card = cards[1];
        if (dog3Card == null && cards.Length > 2) dog3Card = cards[2];
        if (dog4Card == null && cards.Length > 3) dog4Card = cards[3];

        if (startRaceButton == null)
        {
            Button[] buttons = searchRoot.GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttons)
            {
                if (btn.name.Contains("Start") || btn.name.Contains("Race") || btn.name.Contains("Play") || btn.name.Contains("Bắt đầu"))
                {
                    startRaceButton = btn;
                    break;
                }
            }
        }
    }

    private void OnDogSelected(int dogId)
    {
        selectedDogId = dogId;

        if (GameSession.Instance != null)
        {
            GameSession.Instance.selectedDogId = selectedDogId;
        }

        Debug.Log($"[DogSelect] Đã chọn Chú Chó #{selectedDogId}");
    }

    /// <summary>
    /// Bắt đầu trận đua -> Chuyển sang Scene 6 (Scene6_Race / DogRacer)
    /// </summary>
    public void OnStartRaceClicked()
    {
        if (isNavigating) return;
        isNavigating = true;

        if (GameSession.Instance != null)
        {
            GameSession.Instance.selectedDogId = selectedDogId;
        }

        Debug.Log($"[DogSelect] Bắt đầu trận đua với Chú Chó #{selectedDogId} -> Chuyển sang Scene 6 (Race)...");

        if (NavigationManager.Instance != null)
        {
            NavigationManager.Instance.LoadScene(NavigationManager.SCENE_RACE);
        }
        else
        {
            Debug.LogWarning("[DogSelect] NavigationManager chưa sẵn sàng, nạp trực tiếp Scene6_Race");
            SceneManager.LoadScene("Scene6_Race");
        }
    }
}
