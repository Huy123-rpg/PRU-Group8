using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Gắn vào nút Back ở góc trên-trái mỗi Panel / Scene
/// Hỗ trợ cả Single-Scene Panel Switching và Multi-Scene Loading
/// </summary>
[RequireComponent(typeof(Button))]
public class BackButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnBackClicked);
    }

    private void OnBackClicked()
    {
        string currentContext = transform.parent != null ? transform.parent.name : SceneManager.GetActiveScene().name;
        
        if (NavigationManager.Instance != null)
        {
            NavigationManager.Instance.GoBackFrom(currentContext);
        }
        else
        {
            Debug.LogWarning("Chưa tìm thấy NavigationManager Singleton!");
        }
    }
}
