using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Quản lý chuyển cảnh Multi-Scene bất đồng bộ (Option 1):
/// - Load Scene Async không gây đứng game / giật lag.
/// - Màn hình chuyển cảnh Mờ dần (Fade Out / Fade In Canvas).
/// - Tự động nhận diện & kích hoạt ĐÚNG Panel UI cho Scene tương ứng và ẩn tất cả Panel khác.
/// - Quản lý Lịch sử Navigation Stack cho Nút Back tự động quay lại scene trước.
/// - Tự động nhận diện & nạp linh hoạt theo danh sách Scene trong Build Settings.
/// </summary>
public class NavigationManager : MonoBehaviour
{
    private static NavigationManager instance;
    public static NavigationManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<NavigationManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("[NavigationManager]");
                    instance = go.AddComponent<NavigationManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        instance = null;
    }

    [Header("Chuyển Cảnh (Transition Fader)")]
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private bool useFadeTransition = true;

    // Tên của các Screen / Scene theo chuẩn luồng 7 màn
    public const string SCENE_LOGIN = "Scene1_Login";
    public const string SCENE_MAIN_MENU = "Scene2_MainMenu";
    public const string SCENE_LESSON_TYPE = "Scene3_LessonType";
    public const string SCENE_CHAPTER_SELECT = "Scene4_ChapterSelect";
    public const string SCENE_DOG_SELECT = "Scene5_DogSelect";
    public const string SCENE_RACE = "Scene6_Race";
    public const string SCENE_RESULTS = "Scene7_Results";

    // Stack lưu lịch sử chuyển màn để tự động Go Back
    private readonly Stack<string> historyStack = new Stack<string>();
    private bool isLoading = false;
    private CanvasGroup faderCanvasGroup;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            CreateFaderOverlay();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ActivateCorrectPanelForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ActivateCorrectPanelForScene(scene.name);
    }

    /// <summary>
    /// Tự động bật đúng Panel UI tương ứng với từng Scene và ẩn toàn bộ Panel khác (kể cả Panel đang inactive)
    /// </summary>
    public void ActivateCorrectPanelForScene(string sceneName)
    {
        GameObject loginPanel = FindGameObjectInScene("LoginScene", "LoginPanel");
        GameObject menuPanel = FindGameObjectInScene("MenuGame", "MainMenuPanel");
        GameObject lessonTypePanel = FindGameObjectInScene("LessonType", "LessonTypePanel");
        GameObject chapterSelectPanel = FindGameObjectInScene("ChapterSelect", "ChapterSelectPanel");
        GameObject dogSelectPanel = FindGameObjectInScene("DogSelect", "DogSelectPanel", "DogSelected");
        GameObject racePanel = FindGameObjectInScene("DogRacer", "RacePanel");
        GameObject resultsPanel = FindGameObjectInScene("Results", "ResultsPanel");

        // Ẩn tất cả các Panel tìm thấy trong Scene hiện tại
        if (loginPanel != null) loginPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(false);
        if (lessonTypePanel != null) lessonTypePanel.SetActive(false);
        if (chapterSelectPanel != null) chapterSelectPanel.SetActive(false);
        if (dogSelectPanel != null) dogSelectPanel.SetActive(false);
        if (racePanel != null) racePanel.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);

        // Bật duy nhất Panel khớp với Scene hiện tại
        if (sceneName.Contains("Login") || sceneName.Equals(SCENE_LOGIN, System.StringComparison.OrdinalIgnoreCase))
        {
            if (loginPanel != null) loginPanel.SetActive(true);
        }
        else if (sceneName.Contains("MainMenu") || sceneName.Contains("MenuGame") || sceneName.Equals(SCENE_MAIN_MENU, System.StringComparison.OrdinalIgnoreCase))
        {
            if (menuPanel != null) menuPanel.SetActive(true);
        }
        else if (sceneName.Contains("LessonType") || sceneName.Equals(SCENE_LESSON_TYPE, System.StringComparison.OrdinalIgnoreCase))
        {
            if (lessonTypePanel != null) lessonTypePanel.SetActive(true);
        }
        else if (sceneName.Contains("ChapterSelect") || sceneName.Equals(SCENE_CHAPTER_SELECT, System.StringComparison.OrdinalIgnoreCase))
        {
            if (chapterSelectPanel != null) chapterSelectPanel.SetActive(true);
        }
        else if (sceneName.Contains("DogSelect") || sceneName.Equals(SCENE_DOG_SELECT, System.StringComparison.OrdinalIgnoreCase))
        {
            if (dogSelectPanel != null) dogSelectPanel.SetActive(true);
        }
        else if (sceneName.Contains("Race") || sceneName.Equals(SCENE_RACE, System.StringComparison.OrdinalIgnoreCase))
        {
            if (racePanel != null) racePanel.SetActive(true);
        }
        else if (sceneName.Contains("Results") || sceneName.Equals(SCENE_RESULTS, System.StringComparison.OrdinalIgnoreCase))
        {
            if (resultsPanel != null) resultsPanel.SetActive(true);
        }
        else
        {
            // Mặc định cho SampleScene hoặc chưa xác định
            if (loginPanel != null) loginPanel.SetActive(true);
        }
    }

    private GameObject FindGameObjectInScene(params string[] possibleNames)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.isLoaded) return null;

        GameObject[] rootObjects = activeScene.GetRootGameObjects();

        foreach (GameObject root in rootObjects)
        {
            // true = duyệt cả các GameObject đang ở trạng thái SetActive(false)
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in transforms)
            {
                foreach (string name in possibleNames)
                {
                    if (t.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return t.gameObject;
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Tạo UI Fader Overlay phủ toàn màn hình dùng cho chuyển cảnh mờ dần
    /// </summary>
    private void CreateFaderOverlay()
    {
        if (faderCanvasGroup != null) return;

        GameObject canvasGo = new GameObject("[Navigation_FaderCanvas]");
        canvasGo.transform.SetParent(transform);
        
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Luôn nằm trên cùng

        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        faderCanvasGroup = canvasGo.AddComponent<CanvasGroup>();
        faderCanvasGroup.alpha = 0f;
        faderCanvasGroup.blocksRaycasts = false;
        faderCanvasGroup.interactable = false;

        GameObject imageGo = new GameObject("FadeImage");
        imageGo.transform.SetParent(canvasGo.transform, false);
        
        RectTransform rect = imageGo.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        Image img = imageGo.AddComponent<Image>();
        img.color = Color.black;
    }

    /// <summary>
    /// Nạp Scene mới bằng Async + Fader Transition + Lưu Lịch sử Stack
    /// </summary>
    /// <param name="sceneName">Tên scene cần nạp</param>
    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning($"[NavigationManager] Đang chuyển cảnh, bỏ qua yêu cầu nạp: {sceneName}");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        
        // Lưu scene hiện tại vào history nếu khác scene mục tiêu
        if (!string.IsNullOrEmpty(currentScene) && !currentScene.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
        {
            historyStack.Push(currentScene);
        }

        StartCoroutine(Routine_LoadSceneAsync(sceneName));
    }

    /// <summary>
    /// Quay lại Scene trước đó dựa vào History Stack
    /// </summary>
    public void GoBack()
    {
        if (isLoading) return;

        if (historyStack.Count > 0)
        {
            string previousScene = historyStack.Pop();
            Debug.Log($"[NavigationManager] GoBack -> Nạp scene từ History: {previousScene}");
            StartCoroutine(Routine_LoadSceneAsync(previousScene, isBackNavigation: true));
        }
        else
        {
            // Nếu Stack rỗng, suy luận scene kế trước theo logic định sẵn
            string currentScene = SceneManager.GetActiveScene().name;
            string fallback = GetFallbackPreviousScene(currentScene);
            Debug.Log($"[NavigationManager] History rỗng -> Fallback sang: {fallback}");
            if (!string.IsNullOrEmpty(fallback))
            {
                StartCoroutine(Routine_LoadSceneAsync(fallback, isBackNavigation: true));
            }
        }
    }

    /// <summary>
    /// Hỗ trợ hàm GoBackFrom cũ cho BackButton script
    /// </summary>
    public void GoBackFrom(string currentSceneName)
    {
        GoBack();
    }

    private IEnumerator Routine_LoadSceneAsync(string sceneName, bool isBackNavigation = false)
    {
        // Xử lý mapping tên Scene nếu cần thiết
        string resolvedSceneName = ResolveSceneName(sceneName);

        // 1. Bật Fader (Fade Out sang Đen)
        if (useFadeTransition && faderCanvasGroup != null)
        {
            faderCanvasGroup.blocksRaycasts = true;
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                faderCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
                yield return null;
            }
            faderCanvasGroup.alpha = 1f;
        }

        isLoading = true;

        // 2. Bắt đầu Async Load Scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(resolvedSceneName);
        if (asyncLoad == null)
        {
            Debug.LogError($"[NavigationManager] KHÔNG THỂ LOAD SCENE: '{resolvedSceneName}' (Requested: '{sceneName}'). Hãy kiểm tra lại tên Scene trong Build Settings!");
            if (faderCanvasGroup != null)
            {
                faderCanvasGroup.alpha = 0f;
                faderCanvasGroup.blocksRaycasts = false;
            }
            isLoading = false;
            yield break;
        }

        // Chờ scene nạp xong hoàn toàn
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        isLoading = false;

        // 3. Tắt Fader (Fade In từ Đen ra Màn hình mới)
        if (useFadeTransition && faderCanvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                faderCanvasGroup.alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
                yield return null;
            }
            faderCanvasGroup.alpha = 0f;
            faderCanvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Tự động tìm tên Scene tương thích dựa theo danh sách Scene trong Build Settings
    /// </summary>
    private string ResolveSceneName(string requestedName)
    {
        // 1. Kiểm tra trực tiếp tên được yêu cầu
        if (IsSceneInBuildSettings(requestedName)) return requestedName;

        // 2. Kiểm tra các alias/mapping tên phổ biến
        switch (requestedName)
        {
            case SCENE_LOGIN:
            case "Login":
            case "LoginScene":
            case "LoginPanel":
                if (IsSceneInBuildSettings("Scene1_Login")) return "Scene1_Login";
                if (IsSceneInBuildSettings("Login")) return "Login";
                break;

            case SCENE_MAIN_MENU:
            case "MenuGame":
            case "MainMenu":
            case "MainMenuPanel":
                if (IsSceneInBuildSettings("SampleScene")) return "SampleScene";
                if (IsSceneInBuildSettings("Scene2_MainMenu")) return "Scene2_MainMenu";
                if (IsSceneInBuildSettings("MenuGame")) return "MenuGame";
                break;

            case SCENE_LESSON_TYPE:
            case "LessonType":
            case "LessonTypePanel":
                if (IsSceneInBuildSettings("Scene3_LessonType")) return "Scene3_LessonType";
                if (IsSceneInBuildSettings("LessonType")) return "LessonType";
                break;

            case SCENE_CHAPTER_SELECT:
            case "ChapterSelect":
            case "ChapterSelectPanel":
                if (IsSceneInBuildSettings("Scene4_ChapterSelect")) return "Scene4_ChapterSelect";
                if (IsSceneInBuildSettings("ChapterSelect")) return "ChapterSelect";
                break;

            case SCENE_DOG_SELECT:
            case "DogSelect":
            case "DogSelectPanel":
                if (IsSceneInBuildSettings("Scene5_DogSelect")) return "Scene5_DogSelect";
                if (IsSceneInBuildSettings("DogSelect")) return "DogSelect";
                break;

            case SCENE_RACE:
            case "DogRacer":
            case "Race":
            case "RacePanel":
                if (IsSceneInBuildSettings("Scene6_Race")) return "Scene6_Race";
                if (IsSceneInBuildSettings("DogRacer")) return "DogRacer";
                if (IsSceneInBuildSettings("Race")) return "Race";
                break;

            case SCENE_RESULTS:
            case "Results":
            case "ResultsPanel":
                if (IsSceneInBuildSettings("Scene7_Results")) return "Scene7_Results";
                if (IsSceneInBuildSettings("Results")) return "Results";
                break;
        }

        // Mặc định trả về tên gốc được yêu cầu
        return requestedName;
    }

    private bool IsSceneInBuildSettings(string sceneName)
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private string GetFallbackPreviousScene(string currentScene)
    {
        switch (currentScene)
        {
            case SCENE_MAIN_MENU:
            case "MenuGame":
                return SCENE_LOGIN;

            case SCENE_LESSON_TYPE:
            case "LessonType":
                if (IsSceneInBuildSettings("SampleScene")) return "SampleScene";
                return SCENE_MAIN_MENU;

            case SCENE_CHAPTER_SELECT:
            case "ChapterSelect":
                return SCENE_LESSON_TYPE;

            case SCENE_DOG_SELECT:
            case "DogSelect":
                return SCENE_CHAPTER_SELECT;

            case SCENE_RACE:
            case "DogRacer":
                return SCENE_DOG_SELECT;

            case SCENE_RESULTS:
            case "Results":
                return SCENE_CHAPTER_SELECT;

            default:
                return SCENE_LOGIN;
        }
    }
}
