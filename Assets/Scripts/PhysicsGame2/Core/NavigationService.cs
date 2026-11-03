using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    public enum AttemptSource
    {
        RESTORATION,
        PROGRESS_PRACTICE,
        WEEKLY_RANKED,
        WEEKLY_PRACTICE,
        Restoration = RESTORATION,
        ProgressPractice = PROGRESS_PRACTICE,
        WeeklyRanked = WEEKLY_RANKED,
        WeeklyPractice = WEEKLY_PRACTICE
    }

    [Serializable]
    public class GameContextData
    {
        public string GameId = "AURELIA_G2";
        public string UserId = "student01";
        public string CharacterId = "kai";
        public string SessionId = "";
        public string ReturnAction = "";
    }

    [Serializable]
    public class SelectionContextData
    {
        public int ChapterId = 1;
        public string LessonId = "bai2";
        public string LessonTitle = "Bài 2. Động năng. Thế năng";
        public string DeviceId = "G2_C1_D01";
        public string Mode = "MCQ"; // "THEORY", "MCQ", "ESSAY"
        public string Difficulty = "NB"; // "NB", "TH", "VD", "VDC"
    }

    [Serializable]
    public class AttemptContextData
    {
        public string AttemptId = "";
        public AttemptSource Source = AttemptSource.RESTORATION;
        public string BankVersion = "1.0";
        public int QuestionCount = 10;
        public List<string> QuestionIds = new List<string>();
        public float ScoreRaw = 0f;
        public float Score10 = 0f;
        public int CorrectCount = 0;
        public int TotalQuestions = 0;
        public float DurationSeconds = 0f;
        public bool IsFinished = false;
        public string SubmittedAtUtc = "";

        // Quiz answers cache for review
        public List<QuizAnswerRecord> Answers = new List<QuizAnswerRecord>();
        // Essay answers cache
        public List<EssayAnswerRecord> EssayAnswers = new List<EssayAnswerRecord>();

        // Properties & aliases for unified attempt access
        public float Score
        {
            get => (Score10 > 0f) ? Score10 : ((ScoreRaw <= 10f && ScoreRaw > 0f) ? ScoreRaw : (ScoreRaw / 10f));
            set
            {
                if (value <= 10f)
                {
                    Score10 = value;
                    ScoreRaw = value * 10f;
                }
                else
                {
                    ScoreRaw = value;
                    Score10 = value / 10f;
                }
            }
        }
        public float ElapsedSeconds
        {
            get => DurationSeconds;
            set => DurationSeconds = value;
        }
        public int WrongCount
        {
            get => Mathf.Max(0, TotalQuestions - CorrectCount);
            set {}
        }
        public string SubmittedAnswer = "";
        public EssayGradingResult GradingResult;
    }

    [Serializable]
    public class QuizAnswerRecord
    {
        public string CauHoiID;
        public int SelectedOptionIndex = -1; // -1 = unselected
        public int CorrectOptionIndex = 0;
        public bool IsCorrect = false;
        public string QuestionText;
        public string[] Options;
        public string Explanation;
        public string KeyKnowledge;
    }

    [Serializable]
    public class EssayAnswerRecord
    {
        public string CauHoiID;
        public string QuestionText;
        public string SampleAnswer;
        public string Explanation;
        public string ConfirmedText = "";
        public float Score = 0f;
        public bool IsGraded = false;
        public bool IsDraftRubric = true;
        public List<GradingStepFeedback> Steps = new List<GradingStepFeedback>();
        public List<string> MissingCriteria = new List<string>();
    }

    [Serializable]
    public class GradingStepFeedback
    {
        public string StepId;
        public string StudentText;
        public string Status; // "correct", "partial", "incorrect", "unassessable"
        public string Explanation;
        public string SuggestedCorrection;

        // Aliases for property compatibility
        public string stepId
        {
            get => StepId;
            set => StepId = value;
        }
        public string studentExcerpt
        {
            get => StudentText;
            set => StudentText = value;
        }
        public string status
        {
            get => Status;
            set => Status = value;
        }
        public string feedback
        {
            get => Explanation;
            set => Explanation = value;
        }
        public float score = 25f;
        public float maxPoints = 25f;
    }

    [Serializable]
    public class EssayStepFeedback : GradingStepFeedback { }

    [Serializable]
    public class ReturnContextData
    {
        public RouteId OriginRoute = RouteId.MainMenu;
        public int ChapterId = 1;
        public string DeviceId = "G2_C1_D01";
        public Vector2 PlayerPosition = Vector2.zero;
        public string SelectedLessonId = "bai2";
    }

    /// <summary>
    /// Singleton NavigationService quản lý điều hướng và truyền tải Context giữa tất cả màn hình.
    /// Khóa riêng nút trong lúc chuyển màn, đảm bảo chỉ một lần chuyển.
    /// Fallback an toàn (DevContext) khi mở riêng một scene trong Editor.
    /// </summary>
    public class NavigationService : MonoBehaviour
    {
        private static NavigationService _instance;

        public static NavigationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<NavigationService>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("[NavigationService]");
                        _instance = go.AddComponent<NavigationService>();
                        if (Application.isPlaying) DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        public GameContextData GameContext = new GameContextData();
        public SelectionContextData Selection = new SelectionContextData();
        public AttemptContextData Attempt = new AttemptContextData();
        public AttemptContextData AttemptContext => Attempt;
        public ReturnContextData ReturnContext = new ReturnContextData();

        private bool _isTransitioning = false;
        private RouteId _currentRoute = RouteId.MainMenu;
        private RouteId _previousRoute = RouteId.None;
        private GameObject _loadingOverlay;

        public bool IsTransitioning => _isTransitioning;
        public RouteId CurrentRoute => _currentRoute;
        public RouteId PreviousRoute => _previousRoute;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _isTransitioning = false;
            ShowLoadingIndicator(false);
            _currentRoute = SceneCatalog.GetRouteId(scene.name);
            Debug.Log($"[MainMenu Flow] [6. SCENE LOADED] Scene '{scene.name}' đã nạp thành công. Route hiện tại: {_currentRoute}");
        }

        /// <summary>
        /// Mở khóa trạng thái chuyển màn nếu bị kẹt do timeout hoặc bất đồng bộ.
        /// </summary>
        public void ResetTransitionLock()
        {
            _isTransitioning = false;
            ShowLoadingIndicator(false);
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
                EnsureDevContext();
                ShowLoadingIndicator(false);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            EnsureDevContext();
            ShowLoadingIndicator(false);
        }

        public void EnsureDevContext()
        {
            GameDataRepository.EnsureInitialized();
            string activeScene = SceneManager.GetActiveScene().name;
            _currentRoute = SceneCatalog.GetRouteId(activeScene);

            // Nếu dữ liệu session chưa có, nạp profile từ save data
            var save = PhysicsSaveManager.Load();
            if (save != null)
            {
                if (!string.IsNullOrEmpty(save.selectedCharacterId))
                    GameContext.CharacterId = save.selectedCharacterId;
            }

            if (string.IsNullOrEmpty(Attempt.AttemptId))
            {
                Attempt.AttemptId = Guid.NewGuid().ToString("N");
            }
        }

        /// <summary>
        /// Chuyển màn an toàn với RouteId.
        /// Ưu tiên nạp đồng bộ ngay lập tức để chuyển màn dứt khoát, không bị drop click hoặc kẹt transition lock.
        /// </summary>
        public void NavigateTo(RouteId targetRoute)
        {
            string sceneName = SceneCatalog.GetSceneName(targetRoute);
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError($"[NavigationService] Không tìm thấy SceneName cho RouteId: {targetRoute}");
                return;
            }

            Debug.Log($"[NavigationService] NavigateTo: {_currentRoute} -> {targetRoute} ({sceneName})");
            _isTransitioning = false;
            ShowLoadingIndicator(false);
            _previousRoute = _currentRoute;
            _currentRoute = targetRoute;
            Time.timeScale = 1f;

            try
            {
                SceneManager.LoadScene(sceneName);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NavigationService] SceneManager.LoadScene({sceneName}) ngoại lệ: {ex.Message}. Thử chuyển qua Coroutine Async...");
                StartCoroutine(DoTransitionRoutine(targetRoute, sceneName));
            }
        }

        public void NavigateBack()
        {
            if (_isTransitioning) return;

            RouteId backRoute = GetDefaultBackRoute(_currentRoute);
            NavigateTo(backRoute);
        }

        private RouteId GetDefaultBackRoute(RouteId current)
        {
            switch (current)
            {
                case RouteId.CharacterSelection:
                case RouteId.StudySessionSetup:
                case RouteId.Achievements:
                case RouteId.Settings:
                case RouteId.WeeklyLeaderboard:
                case RouteId.LearningProgress:
                    return RouteId.MainMenu;

                case RouteId.ChapterSelection:
                    return RouteId.StudySessionSetup;

                case RouteId.LessonSelection:
                    return RouteId.ChapterSelection;

                case RouteId.ChapterMap1:
                case RouteId.ChapterMap2:
                case RouteId.ChapterMap3:
                case RouteId.ChapterMap4:
                case RouteId.ChapterMap5:
                case RouteId.ChapterMap1After:
                case RouteId.ChapterMap2After:
                case RouteId.ChapterMap3After:
                case RouteId.ChapterMap4After:
                case RouteId.ChapterMap5After:
                    return RouteId.ChapterSelection;

                case RouteId.ModeSelection:
                    return SceneCatalog.GetChapterMapRoute(Selection.ChapterId, RestorationService.IsChapterFullyRestored(Selection.ChapterId));

                case RouteId.Theory:
                case RouteId.DifficultySelection:
                    return RouteId.ModeSelection;

                case RouteId.PracticeQuiz:
                case RouteId.EssayAnswer:
                    return RouteId.DifficultySelection;

                case RouteId.QuizResult:
                case RouteId.EssayResult:
                    return SceneCatalog.GetChapterMapRoute(Selection.ChapterId, RestorationService.IsChapterFullyRestored(Selection.ChapterId));

                case RouteId.QuizReview:
                    return RouteId.QuizResult;

                case RouteId.EssayReview:
                    return RouteId.EssayResult;

                case RouteId.WeeklyQuiz:
                    return RouteId.WeeklyLeaderboard;

                case RouteId.WeeklyResult:
                    return RouteId.WeeklyLeaderboard;

                default:
                    return RouteId.MainMenu;
            }
        }

        private IEnumerator DoTransitionRoutine(RouteId targetRoute, string sceneName)
        {
            _isTransitioning = true;
            ShowLoadingIndicator(true);

            _previousRoute = _currentRoute;
            Debug.Log($"[NavigationService] Bắt đầu chuyển màn: {_currentRoute} -> {targetRoute} ({sceneName})");

            try
            {
                AsyncOperation asyncOp = null;
                try
                {
                    asyncOp = SceneManager.LoadSceneAsync(sceneName);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[NavigationService] LoadSceneAsync({sceneName}) ngoại lệ: {ex.Message}. Thử LoadScene đồng bộ...");
                    try
                    {
                        SceneManager.LoadScene(sceneName);
                        _currentRoute = targetRoute;
                        yield break;
                    }
                    catch (Exception syncEx)
                    {
                        Debug.LogError($"[NavigationService] Không thể nạp scene {sceneName}: {syncEx.Message}");
                        yield break;
                    }
                }

                if (asyncOp == null)
                {
                    Debug.LogWarning($"[NavigationService] AsyncOperation null cho {sceneName}. Thử LoadScene đồng bộ...");
                    try
                    {
                        SceneManager.LoadScene(sceneName);
                        _currentRoute = targetRoute;
                        yield break;
                    }
                    catch (Exception syncEx)
                    {
                        Debug.LogError($"[NavigationService] Không thể nạp scene {sceneName}: {syncEx.Message}");
                        yield break;
                    }
                }

                float startTime = Time.realtimeSinceStartup;
                while (!asyncOp.isDone)
                {
                    if (Time.realtimeSinceStartup - startTime > 5.0f)
                    {
                        Debug.LogWarning($"[NavigationService] LoadSceneAsync({sceneName}) timeout quá 5s. Thử LoadScene đồng bộ...");
                        SceneManager.LoadScene(sceneName);
                        break;
                    }
                    yield return null;
                }

                _currentRoute = targetRoute;
                yield return new WaitForSecondsRealtime(0.05f); // Chờ hierarchy scene mới sẵn sàng
                Debug.Log($"[NavigationService] Chuyển màn THÀNH CÔNG sang {targetRoute} ({sceneName}).");
            }
            finally
            {
                // Luôn luôn tắt hoàn toàn Loading Indicator / Dark veil dù có ngoại lệ hay timeout
                _isTransitioning = false;
                ShowLoadingIndicator(false);
            }
        }

        private void ShowLoadingIndicator(bool show)
        {
            if (show)
            {
                if (_loadingOverlay == null)
                {
                    _loadingOverlay = GameObject.Find("NavigationLoadingOverlay");
                }

                if (_loadingOverlay == null)
                {
                    _loadingOverlay = new GameObject("NavigationLoadingOverlay");
                    DontDestroyOnLoad(_loadingOverlay);
                    var canvas = _loadingOverlay.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 9999;
                    _loadingOverlay.AddComponent<GraphicRaycaster>();

                    var panel = new GameObject("Panel");
                    panel.transform.SetParent(_loadingOverlay.transform, false);
                    var rt = panel.AddComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.sizeDelta = Vector2.zero;
                    var img = panel.AddComponent<Image>();
                    img.color = new Color(0.04f, 0.08f, 0.16f, 0.4f); // Subtle dark veil
                    img.raycastTarget = true; // Blocks clicks during transition
                }
                _loadingOverlay.SetActive(true);
            }
            else
            {
                if (_loadingOverlay == null)
                {
                    _loadingOverlay = GameObject.Find("NavigationLoadingOverlay");
                }

                if (_loadingOverlay != null)
                {
                    _loadingOverlay.SetActive(false);
                }

                // Quét tắt an toàn tất cả các Overlay còn sót lại trong DontDestroyOnLoad hoặc Active Scenes
                var allOverlays = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var go in allOverlays)
                {
                    if (go != null && go.name == "NavigationLoadingOverlay" && go.activeSelf)
                    {
                        go.SetActive(false);
                    }
                }
            }
        }
    }
}
