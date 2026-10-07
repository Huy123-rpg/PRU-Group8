using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Quản lý logic cho Scene 1 — Login
/// Hỗ trợ phím TAB để chuyển dòng & Phím ENTER để Đăng Nhập
/// </summary>
public class LoginUIController : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    [Header("Buttons")]
    public Button loginButton;

    [Header("Status & Error Message")]
    public TextMeshProUGUI errorText;

    private void Start()
    {
        if (errorText != null)
        {
            errorText.text = "";
        }

        // Tự động focus vào ô Username khi vừa mở màn hình
        if (usernameInput != null)
        {
            usernameInput.Select();
            usernameInput.ActivateInputField();
        }

        // Đồng bộ danh sách tài khoản từ Google Sheet khi bắt đầu
        if (GoogleSheetDataManager.Instance != null)
        {
            StartCoroutine(GoogleSheetDataManager.Instance.FetchAccountsFromSheet());
        }

        if (usernameInput != null) usernameInput.onValueChanged.AddListener(OnInputChanged);
        if (passwordInput != null) passwordInput.onValueChanged.AddListener(OnInputChanged);
        if (loginButton != null) loginButton.onClick.AddListener(OnLoginClicked);

        ValidateInputs();
    }

    private void Update()
    {
        try
        {
            bool isTabPressed = false;
            bool isEnterPressed = false;

            // 1. Kiểm tra New Input System
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.tabKey.wasPressedThisFrame) isTabPressed = true;
                if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame) isEnterPressed = true;
            }
#endif

            // 2. Kiểm tra Legacy Input System
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Tab)) isTabPressed = true;
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) isEnterPressed = true;
#endif

            // Xử lý phím TAB -> Chuyển focus giữa ô Username và Password
            if (isTabPressed)
            {
                SwitchInputFieldFocus();
            }

            // Xử lý phím ENTER -> Thực hiện Đăng nhập
            if (isEnterPressed && loginButton != null && loginButton.interactable)
            {
                OnLoginClicked();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LoginUIController Update]: {ex.Message}");
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
        else
        {
            usernameInput.Select();
            usernameInput.ActivateInputField();
        }
    }

    private void OnInputChanged(string value)
    {
        ValidateInputs();
        if (errorText != null) errorText.text = "";
    }

    private void ValidateInputs()
    {
        bool isUsernameValid = usernameInput != null && !string.IsNullOrWhiteSpace(usernameInput.text);
        bool isPasswordValid = passwordInput != null && !string.IsNullOrWhiteSpace(passwordInput.text);

        if (loginButton != null)
        {
            loginButton.interactable = isUsernameValid && isPasswordValid;
        }
    }

    private void OnLoginClicked()
    {
        string user = usernameInput.text.Trim();
        string pass = passwordInput.text.Trim();

        if (pass.Length < 6)
        {
            ShowError("Mật khẩu phải chứa ít nhất 6 ký tự!");
            return;
        }

        // Xác thực tài khoản qua GoogleSheetDataManager
        if (GoogleSheetDataManager.Instance != null)
        {
            if (GoogleSheetDataManager.Instance.Authenticate(user, pass, out UserAccount matchedAccount))
            {
                if (GameSession.Instance != null)
                {
                    GameSession.Instance.SetUserSession(matchedAccount);
                }

                if (loginButton != null) loginButton.interactable = false;
                ProceedToMainMenu();
            }
            else
            {
                ShowError("Tài khoản hoặc mật khẩu không chính xác!");
            }
        }
        else
        {
            UserAccount fallbackAcc = new UserAccount { username = user, password = pass };
            if (GameSession.Instance != null) GameSession.Instance.SetUserSession(fallbackAcc);
            ProceedToMainMenu();
        }
    }

    private void ProceedToMainMenu()
    {
        if (NavigationManager.Instance != null)
        {
            NavigationManager.Instance.LoadScene(NavigationManager.SCENE_MAIN_MENU);
        }
        else
        {
            Debug.LogError("Chưa khởi tạo NavigationManager!");
        }
    }

    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.color = Color.red;
        }
    }
}
