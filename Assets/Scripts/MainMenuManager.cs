using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI selectedSubjectText;

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

    public void SelectSubject(string subjectName)
    {
        if (selectedSubjectText != null)
        {
            selectedSubjectText.text = "Môn đã chọn: " + subjectName;
        }

        Debug.Log("Chuẩn bị vào màn chơi cho môn: " + subjectName);

        string lower = subjectName.ToLower();
        if (lower.Contains("physic") || lower.Contains("vật lý") || lower.Contains("vật lí"))
        {
            SceneManager.LoadScene("Physics");
        }
    }

    public void OnBackToLoginClicked()
    {
        // Handle returning to login screen
        LoginManager loginManager = FindObjectOfType<LoginManager>(true);
        if (loginManager != null && loginManager.usePanelSwitching)
        {
            if (loginManager.menuGamePanel != null) loginManager.menuGamePanel.SetActive(false);
            if (loginManager.loginPanel != null) loginManager.loginPanel.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene("SampleScene"); // Or Login scene name
        }
    }
}
