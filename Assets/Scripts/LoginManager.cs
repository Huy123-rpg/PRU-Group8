using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class LoginManager : MonoBehaviour
{
    [Header("Input UI")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI statusText;
    public Button loginButton;

    [Header("Google Sheet Accounts Sync (Optional)")]
    [Tooltip("Dán link Google Sheet CSV (ví dụ: https://docs.google.com/spreadsheets/d/YOUR_SHEET_ID/export?format=csv)")]
    public string googleSheetCsvUrl = "";
    public bool fetchOnStart = true;

    [Header("Transition Settings")]
    [Tooltip("Tick chọn để bật/tắt Panel trong cùng Scene. Bỏ tick để LoadScene mới.")]
    public bool usePanelSwitching = true;
    public GameObject loginPanel;
    public GameObject menuGamePanel;
    public string menuSceneName = "MenuGame";

    // Dictionary để lưu tài khoản {Username, Password}
    private Dictionary<string, string> accounts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private bool isDownloadingSheet = false;

    private void Start()
    {
        // Khởi tạo các tài khoản mặc định (Offline Fallback)
        AddDefaultAccounts();

        // Tự động tìm kiếm UI nếu chưa kéo thả trong Inspector
        AutoAssignUIReferences();

        // Tải danh sách tài khoản từ Google Sheet nếu có URL
        if (fetchOnStart && !string.IsNullOrEmpty(googleSheetCsvUrl))
        {
            StartCoroutine(FetchAccountsFromGoogleSheet());
        }

        // Tự động focus vào ô Username khi vừa mở màn hình
        if (usernameInput != null)
        {
            usernameInput.Select();
            usernameInput.ActivateInputField();
        }
    }

    private void Update()
    {
        try
        {
            bool isTabPressed = false;
            bool isEnterPressed = false;

            // 1. Kiểm tra New Input System (nếu dự án dùng Package Input System mới)
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.tabKey.wasPressedThisFrame) isTabPressed = true;
                if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame) isEnterPressed = true;
            }
#endif

            // 2. Kiểm tra Legacy Input System (nếu dự án dùng Input Manager cũ)
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Tab)) isTabPressed = true;
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) isEnterPressed = true;
#endif

            // Xử lý sự kiện bàn phím
            if (isTabPressed)
            {
                SwitchInputFieldFocus();
            }

            if (isEnterPressed)
            {
                OnLoginButtonClicked();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LoginManager Update Error]: {ex.Message}");
        }
    }

    /// <summary>
    /// Chuyển con trỏ nhập liệu bằng phím TAB
    /// </summary>
    private void SwitchInputFieldFocus()
    {
        if (usernameInput == null || passwordInput == null) return;

        if (usernameInput.isFocused)
        {
            passwordInput.Select();
            passwordInput.ActivateInputField();
        }
        else if (passwordInput.isFocused)
        {
            usernameInput.Select();
            usernameInput.ActivateInputField();
        }
        else
        {
            usernameInput.Select();
            usernameInput.ActivateInputField();
        }
    }

    /// <summary>
    /// Xử lý Đăng Nhập với Bắt Exception chi tiết
    /// </summary>
    public void OnLoginButtonClicked()
    {
        try
        {
            if (isDownloadingSheet)
            {
                SetStatus("Đang tải dữ liệu tài khoản từ Sheet, vui lòng chờ...", Color.yellow);
                return;
            }

            string username = usernameInput != null ? usernameInput.text.Trim() : "";
            string password = passwordInput != null ? passwordInput.text.Trim() : "";

            // Kiểm tra rỗng
            if (string.IsNullOrEmpty(username))
            {
                SetStatus("Vui lòng nhập tên tài khoản", Color.red);
                FocusInputField(usernameInput);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                SetStatus("Vui lòng nhập mật khẩu", Color.red);
                FocusInputField(passwordInput);
                return;
            }

            // Kiểm tra thông tin tài khoản
            if (AuthenticateUser(username, password))
            {
                SetStatus("Thành Công", Color.green);

                // Disable button để tránh nhấn liên tục
                if (loginButton != null) loginButton.interactable = false;

                // Thực hiện chuyển cảnh sau 0.5s
                Invoke(nameof(TransitionToMenu), 0.5f);
            }
            else
            {
                SetStatus("Try Again", Color.red);
                FocusInputField(passwordInput);
            }
        }
        catch (NullReferenceException nre)
        {
            Debug.LogError($"[Login Exception - Null Reference]: {nre.Message}\n{nre.StackTrace}");
            SetStatus("Lỗi cấu hình UI! Kiểm tra lại Inspector.", Color.red);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Login Exception]: {ex.Message}\n{ex.StackTrace}");
            SetStatus($"Lỗi đăng nhập: {ex.Message}", Color.red);
        }
    }

    /// <summary>
    /// Xác thực username và password
    /// </summary>
    private bool AuthenticateUser(string username, string password)
    {
        if (accounts.TryGetValue(username, out string correctPassword))
        {
            return correctPassword == password;
        }

        return false;
    }

    /// <summary>
    /// Chuyển sang Menu Game
    /// </summary>
    private void TransitionToMenu()
    {
        try
        {
            if (usePanelSwitching)
            {
                // Tự động tìm lại panel nếu null
                if (loginPanel == null) loginPanel = transform.parent != null ? transform.parent.gameObject : GameObject.Find("LoginScene");
                if (menuGamePanel == null) menuGamePanel = GameObject.Find("MenuGame");

                if (loginPanel != null)
                {
                    loginPanel.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("[LoginManager] Không tìm thấy LoginPanel để ẩn.");
                }

                if (menuGamePanel != null)
                {
                    menuGamePanel.SetActive(true);
                }
                else
                {
                    Debug.LogError("[LoginManager] Không tìm thấy MenuGame GameObject để bật!");
                    SetStatus("⚠️ Không tìm thấy GameObject 'MenuGame' trong Canvas!", Color.red);
                    if (loginButton != null) loginButton.interactable = true;
                    return;
                }
            }
            else
            {
                SceneManager.LoadScene(menuSceneName);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Transition Exception]: {ex.Message}\n{ex.StackTrace}");
            SetStatus($"⚠️ Lỗi chuyển màn: {ex.Message}", Color.red);
            if (loginButton != null) loginButton.interactable = true;
        }
    }

    /// <summary>
    /// Thêm tài khoản mặc định (dùng khi offline hoặc chưa config Google Sheet)
    /// </summary>
    private void AddDefaultAccounts()
    {
        accounts.Clear();
        accounts["admin"] = "123456";
        accounts["student"] = "123456";
        accounts["pru"] = "123456";
        accounts["user"] = "123456";
    }

    /// <summary>
    /// Tải danh sách tài khoản từ Google Sheet dạng CSV (Cột A: Username, Cột B: Password)
    /// </summary>
    public IEnumerator FetchAccountsFromGoogleSheet()
    {
        if (string.IsNullOrEmpty(googleSheetCsvUrl)) yield break;

        isDownloadingSheet = true;
        SetStatus("Đang đồng bộ tài khoản từ Google Sheet...", Color.cyan);

        using (UnityWebRequest www = UnityWebRequest.Get(googleSheetCsvUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string csvText = www.downloadHandler.text;
                    ParseCsvAccounts(csvText);
                    SetStatus("Đã đồng bộ tài khoản thành công!", Color.green);
                    Invoke(nameof(ClearStatus), 2.0f);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[CSV Parse Error]: {ex.Message}");
                    SetStatus("⚠️ Lỗi đọc file Sheet CSV. Đang dùng tài khoản mặc định.", Color.yellow);
                }
            }
            else
            {
                Debug.LogWarning($"[Google Sheet Sync Error]: {www.error}. Sử dụng tài khoản offline.");
                SetStatus("⚠️ Không thể kết nối Sheet. Sử dụng tài khoản offline.", Color.yellow);
            }
        }

        isDownloadingSheet = false;
    }

    /// <summary>
    /// Đọc định dạng CSV (dòng 1: header Username,Password; các dòng sau: value)
    /// </summary>
    private void ParseCsvAccounts(string csvContent)
    {
        string[] lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        int addedCount = 0;

        foreach (string line in lines)
        {
            string[] columns = line.Split(',');
            if (columns.Length >= 2)
            {
                string u = columns[0].Trim().Trim('"');
                string p = columns[1].Trim().Trim('"');

                if (u.Equals("username", StringComparison.OrdinalIgnoreCase) || u.Equals("taikhoan", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!string.IsNullOrEmpty(u))
                {
                    accounts[u] = p;
                    addedCount++;
                }
            }
        }
        Debug.Log($"[LoginManager] Đã tải thành công {addedCount} tài khoản từ Google Sheet.");
    }

    /// <summary>
    /// Tự động kết nối UI nếu thiếu tham chiếu Inspector
    /// </summary>
    private void AutoAssignUIReferences()
    {
        if (usernameInput == null)
        {
            GameObject uObj = GameObject.Find("UserName");
            if (uObj != null) usernameInput = uObj.GetComponent<TMP_InputField>();
        }

        if (passwordInput == null)
        {
            GameObject pObj = GameObject.Find("Password");
            if (pObj != null) passwordInput = pObj.GetComponent<TMP_InputField>();
        }

        if (statusText == null)
        {
            GameObject sObj = GameObject.Find("StatusTex");
            if (sObj != null) statusText = sObj.GetComponent<TextMeshProUGUI>();
        }

        if (loginButton == null)
        {
            GameObject bObj = GameObject.Find("LoginButton");
            if (bObj != null) loginButton = bObj.GetComponent<Button>();
        }

        if (loginPanel == null)
        {
            loginPanel = GameObject.Find("LoginScene");
            if (loginPanel == null) loginPanel = GameObject.Find("LoginPanel");
        }

        if (menuGamePanel == null)
        {
            menuGamePanel = GameObject.Find("MenuGame");
        }
    }

    private void SetStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
        Debug.Log($"[Login Status]: {message}");
    }

    private void ClearStatus()
    {
        if (statusText != null) statusText.text = "";
    }

    private void FocusInputField(TMP_InputField inputField)
    {
        if (inputField != null)
        {
            inputField.Select();
            inputField.ActivateInputField();
        }
    }
}
