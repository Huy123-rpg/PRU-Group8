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

        string key = Normalize(subjectName);
        Debug.Log("Chuẩn bị vào màn chơi cho môn: " + subjectName);

        // Môn SINH HỌC → scene SinhHoc (game vượt chướng ngoại vật + boss)
        // Nhận cả tên tiếng Việt lẫn tiếng Anh (Biology/Chemistry/Physics)
        if (key.Contains("sinh") || key.Contains("bio"))
        {
            SceneManager.LoadScene("SinhHoc");
        }
        else if (key.Contains("hoa") || key.Contains("chem"))
        {
            Debug.Log("[Menu] Môn Hóa Học - chưa làm (nhóm member khác phụ trách).");
        }
        else if (key.Contains("vat") || key.Contains("ly") || key.Contains("phy"))
        {
            Debug.Log("[Menu] Môn Vật Lý - chưa làm (nhóm member khác phụ trách).");
        }
    }

    /// <summary>Bỏ dấu, về chữ thường để so sánh tên môn dễ dàng hơn.</summary>
    private static string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        string formD = s.Trim().ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (char c in formD)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Replace(" ", "");
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
