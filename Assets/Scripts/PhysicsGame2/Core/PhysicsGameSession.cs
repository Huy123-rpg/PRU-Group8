using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Singleton quản lý phiên chơi hiện tại (Session State) xuyên suốt tất cả scene trong Physics Game 2.
    /// Tự động sống qua các lệnh LoadScene bằng DontDestroyOnLoad.
    /// </summary>
    public class PhysicsGameSession : MonoBehaviour
    {
        private static PhysicsGameSession _instance;

        public static PhysicsGameSession Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<PhysicsGameSession>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("[PhysicsGameSession]");
                        _instance = go.AddComponent<PhysicsGameSession>();
                        if (Application.isPlaying) DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("=== PHIÊN CHƠI HIỆN TẠI (RUNTIME SESSION) ===")]
        [SerializeField] private string selectedCharacterId = "Lyra";
        [SerializeField] private string selectedTeacherId = "Minh";
        [SerializeField] private int selectedChapterId = 2;
        [SerializeField] private string selectedLessonId = "C2_L6";
        [SerializeField] private string selectedLessonTitle = "Bài 6: Phản xạ toàn phần";
        [SerializeField] private GameMode selectedMode = GameMode.None;

        [Header("=== NAVIGATION STATE ===")]
        [SerializeField] private string currentSceneName = "";
        [SerializeField] private string previousSceneName = "";

        // Dữ liệu lưu trữ (Save Data)
        private PhysicsSaveData _saveData;
        private bool _studyPaused;
        private float _saveTick;
        private bool _studyWarningShown;
        private bool _studyExpiredNoticeShown;

        // Public Getters
        public string SelectedCharacterId => selectedCharacterId;
        public string SelectedTeacherId => selectedTeacherId;
        public int SelectedChapterId => selectedChapterId;
        public string SelectedLessonId => selectedLessonId;
        public string SelectedLessonTitle => selectedLessonTitle;
        public GameMode SelectedMode => selectedMode;
        public string CurrentSceneName => currentSceneName;
        public string PreviousSceneName => previousSceneName;
        public PhysicsSaveData SaveData => _saveData ??= PhysicsSaveManager.Load();
        public bool StudyTimeLimitReached => SaveData != null && SaveData.endStudyWhenTimeReached &&
            SaveData.activeStudySession != null && SaveData.activeStudySession.isActive &&
            SaveData.activeStudySession.activeSeconds >= SaveData.activeStudySession.plannedMinutes * 60f;

        public float RemainingStudySeconds
        {
            get
            {
                var study = SaveData != null ? SaveData.activeStudySession : null;
                if (study == null || !study.isActive) return 0f;
                return Mathf.Max(0f, study.plannedMinutes * 60f - study.activeSeconds);
            }
        }
        public bool IsStudySessionActive => SaveData != null && SaveData.activeStudySession != null && SaveData.activeStudySession.isActive;
        public bool IsStudyPaused => _studyPaused;
        public void PauseStudySession() => _studyPaused = true;
        public void ResumeStudySession() => _studyPaused = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticInstance()
        {
            _instance = null;
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
                InitializeSession();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            string activeSceneName = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(activeSceneName))
            {
                PhysicsGameBootstrap.AttachControllerForScene(activeSceneName);
            }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void InitializeSession()
        {
            _saveData = PhysicsSaveManager.Load();
            _studyPaused = _saveData != null && _saveData.activeStudySession != null && _saveData.activeStudySession.isActive;
            currentSceneName = SceneManager.GetActiveScene().name;

            // Đồng bộ trạng thái ban đầu từ SaveData nếu có
            if (_saveData != null)
            {
                if (!string.IsNullOrEmpty(_saveData.selectedCharacterId))
                    selectedCharacterId = _saveData.selectedCharacterId;

                if (!string.IsNullOrEmpty(_saveData.selectedTeacherId))
                    selectedTeacherId = _saveData.selectedTeacherId;

                if (_saveData.lastChapterId > 0)
                    selectedChapterId = _saveData.lastChapterId;
                if (!string.IsNullOrEmpty(_saveData.lastLessonId))
                {
                    selectedLessonId = _saveData.lastLessonId;
                    if (selectedLessonId == PhysicsQuestionRepository.WHOLE_BOOK_REVIEW_ID)
                    {
                        selectedLessonTitle = "Tổng ôn cả quyển · chạy thử lễ hội";
                    }
                    else
                    {
                        var lesson = PhysicsCurriculumData.GetLessons(selectedChapterId).Find(x => x.lessonId == selectedLessonId);
                        if (lesson != null) selectedLessonTitle = lesson.DisplayName;
                    }
                }
            }
        }

        private void Update()
        {
            var study = SaveData != null ? SaveData.activeStudySession : null;
            if (study == null || !study.isActive || _studyPaused) return;
            study.activeSeconds += Time.unscaledDeltaTime;
            float remaining = study.plannedMinutes * 60f - study.activeSeconds;
            if (SaveData.endStudyWhenTimeReached && remaining <= 60f && remaining > 0f && !_studyWarningShown)
            {
                _studyWarningShown = true;
                ShowStudyTimeNotice("Còn khoảng một phút trong buổi học.");
            }
            if (StudyTimeLimitReached && !_studyExpiredNoticeShown)
            {
                _studyExpiredNoticeShown = true;
                ShowStudyTimeNotice("Đã hết giờ. Trả lời câu hiện tại rồi chọn kết thúc buổi.");
            }
            if (StudyTimeLimitReached)
            {
                string scene = SceneManager.GetActiveScene().name;
                bool assessmentInProgress = scene == PhysicsSceneNames.Quiz || scene == PhysicsSceneNames.WrittenAnswer ||
                    scene == PhysicsSceneNames.QuizResult || scene == PhysicsSceneNames.QuizReview || scene == PhysicsSceneNames.WrittenReview ||
                    scene == PhysicsSceneNames.GameplayMechanics || scene == PhysicsSceneNames.GameplayOptics ||
                    scene == PhysicsSceneNames.GameplayElectricity || scene == PhysicsSceneNames.GameplayElectromagnetism ||
                    scene == PhysicsSceneNames.GameplayEnergy ||
                    scene == PhysicsSceneNames.StudySummary;
                if (!assessmentInProgress)
                {
                    FinishStudySession(endedByTime: true);
                    LoadScene(PhysicsSceneNames.StudySummary);
                    return;
                }
            }
            _saveTick += Time.unscaledDeltaTime;
            if (_saveTick >= 5f)
            {
                _saveTick = 0f;
                SaveProgress();
            }
        }

        private void OnApplicationPause(bool paused)
        {
            _studyPaused = paused;
            if (!paused && SaveData != null && SaveData.activeStudySession != null && SaveData.activeStudySession.isActive)
                _studyPaused = true; // Returning to an interrupted session requires an explicit resume action.
            SaveProgress();
        }

        private void OnApplicationQuit() => SaveProgress();

        private void ShowStudyTimeNotice(string message)
        {
            Canvas canvas = FindObjectsByType<Canvas>(FindObjectsInactive.Include)
                .FirstOrDefault(x => x != null && x.gameObject.scene == SceneManager.GetActiveScene());
            if (canvas == null) { Debug.Log(message); return; }
            var notice = new GameObject("StudyTimeNotice", typeof(RectTransform), typeof(TextMeshProUGUI));
            notice.transform.SetParent(canvas.transform, false);
            var rect = notice.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(0f, -280f);
            rect.sizeDelta = new Vector2(780f, 64f);
            var text = notice.GetComponent<TextMeshProUGUI>();
            text.font = canvas.GetComponentInChildren<TextMeshProUGUI>(true)?.font;
            text.fontSize = 22f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(.12f, .17f, .25f, 1f);
            text.text = message;
            text.raycastTarget = false;
            Destroy(notice, 5f);
        }

        public void ConfigureStudyPlan(int minutes, string goal)
        {
            var save = SaveData;
            if (save == null) return;
            save.plannedStudyMinutes = minutes == 15 || minutes == 45 ? minutes : 30;
            if (!string.IsNullOrWhiteSpace(goal)) save.studyGoal = goal;
            if (save.activeStudySession != null && save.activeStudySession.isActive)
            {
                save.activeStudySession.plannedMinutes = save.plannedStudyMinutes;
                save.activeStudySession.goal = save.studyGoal;
            }
            SaveProgress();
        }

        public PhysicsStudySessionRecord StartOrResumeStudySession()
        {
            var save = SaveData;
            if (save == null) return null;
            if (save.plannedStudyMinutes <= 0) save.plannedStudyMinutes = 30;
            if (save.activeStudySession == null || !save.activeStudySession.isActive)
            {
                save.activeStudySession = new PhysicsStudySessionRecord
                {
                    sessionId = Guid.NewGuid().ToString("N"),
                    startedAtUtc = DateTime.UtcNow.ToString("o"),
                    plannedMinutes = save.plannedStudyMinutes,
                    goal = save.studyGoal,
                    isActive = true
                };
                save.studySessions.Add(save.activeStudySession);
            }
            else
            {
                save.activeStudySession.plannedMinutes = save.plannedStudyMinutes;
                if (!string.IsNullOrWhiteSpace(save.studyGoal))
                {
                    save.activeStudySession.goal = save.studyGoal;
                }
            }
            _studyPaused = false;
            _studyWarningShown = false;
            _studyExpiredNoticeShown = false;
            SaveProgress();
            return save.activeStudySession;
        }

        public void FinishStudySession(bool endedByTime = false)
        {
            var save = SaveData;
            var study = save != null ? save.activeStudySession : null;
            if (study == null || !study.isActive) return;
            study.isActive = false;
            study.endedByTime = endedByTime;
            study.endedAtUtc = DateTime.UtcNow.ToString("o");
            _studyPaused = true;
            SaveProgress();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            previousSceneName = currentSceneName;
            currentSceneName = scene.name;

            // Đảm bảo Controller của Scene luôn được gắn ngay lập tức (kể cả khi Unity vừa hot-reload script)
            PhysicsGameBootstrap.AttachControllerForScene(scene.name);

            // Cập nhật lại vị trí scene gần nhất vào SaveData (ngoại trừ các scene tạm như Settings/Guide)
            if (scene.name != PhysicsSceneNames.Settings &&
                scene.name != PhysicsSceneNames.Guide &&
                scene.name != PhysicsSceneNames.Inventory &&
                scene.name != PhysicsSceneNames.MainMenu)
            {
                if (_saveData != null)
                {
                    _saveData.lastSceneName = scene.name;
                    _saveData.lastChapterId = selectedChapterId;
                    _saveData.lastLessonId = selectedLessonId;
                    SaveProgress();
                }
            }

            Debug.Log($"[PhysicsGameSession] Đã chuyển Scene: '{currentSceneName}' (Scene trước: '{previousSceneName}')");
        }

        // ==========================================
        // CÁC HÀM THIẾT LẬP TRẠNG THÁI PHIÊN CHƠI
        // ==========================================

        public void SelectCharacter(string characterId)
        {
            selectedCharacterId = characterId;
            if (_saveData != null)
            {
                _saveData.selectedCharacterId = characterId;
                SaveProgress();
            }
            Debug.Log($"[PhysicsGameSession] Đã chọn nhân vật: {characterId}");
        }

        public void SelectTeacher(string teacherId)
        {
            selectedTeacherId = teacherId;
            if (_saveData != null)
            {
                _saveData.selectedTeacherId = teacherId;
                SaveProgress();
            }
            Debug.Log($"[PhysicsGameSession] Đã chọn giáo viên: {teacherId}");
        }

        public void SelectChapter(int chapterNumber)
        {
            selectedChapterId = Mathf.Clamp(chapterNumber, 1, 5);
            if (_saveData != null)
            {
                _saveData.lastChapterId = selectedChapterId;
            }
            Debug.Log($"[PhysicsGameSession] Đã chọn chương: {selectedChapterId}");
        }

        public void SelectLesson(string lessonId, string lessonTitle = "")
        {
            selectedLessonId = lessonId;
            selectedLessonTitle = string.IsNullOrEmpty(lessonTitle) ? lessonId : lessonTitle;
            if (_saveData != null)
            {
                _saveData.lastLessonId = lessonId;
                var study = _saveData.activeStudySession;
                if (study != null && study.isActive)
                {
                    if (study.lessonIds == null) study.lessonIds = new List<string>();
                    if (!string.IsNullOrEmpty(lessonId) && !study.lessonIds.Contains(lessonId)) study.lessonIds.Add(lessonId);
                }
                SaveProgress();
            }
            Debug.Log($"[PhysicsGameSession] Đã chọn bài học: {lessonId} - {selectedLessonTitle}");
        }

        public void SelectMode(GameMode mode)
        {
            selectedMode = mode;
            Debug.Log($"[PhysicsGameSession] Đã chọn chế độ: {mode}");
        }

        public void MarkGuideAsViewed()
        {
            if (SaveData != null)
            {
                SaveData.hasViewedGuide = true;
                SaveProgress();
            }
            Debug.Log("[PhysicsGameSession] Đã đánh dấu đã xem hướng dẫn (hasViewedGuide = true).");
        }

        // ==========================================
        // NAVIGATION HELPER
        // ==========================================

        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[PhysicsGameSession] Tên scene cần tải bị trống!");
                return;
            }

#if UNITY_EDITOR
            EnsureSceneInBuildSettings(sceneName);
#endif

            try
            {
                SceneManager.LoadScene(sceneName);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PhysicsGameSession] Không thể tải Scene '{sceneName}': {ex.Message}");
            }
        }

#if UNITY_EDITOR
        private static void EnsureSceneInBuildSettings(string sceneName)
        {
            int index = SceneUtility.GetBuildIndexByScenePath(sceneName);
            if (index < 0)
            {
                var list = new System.Collections.Generic.List<UnityEditor.EditorBuildSettingsScene>(UnityEditor.EditorBuildSettings.scenes);
                string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:Scene {sceneName}");
                foreach (string guid in guids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    if (System.IO.Path.GetFileNameWithoutExtension(path).Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (!list.Exists(s => s.path == path))
                        {
                            list.Add(new UnityEditor.EditorBuildSettingsScene(path, true));
                            UnityEditor.EditorBuildSettings.scenes = list.ToArray();
                            Debug.Log($"[PhysicsGameSession] Đã tự động thêm '{path}' vào EditorBuildSettings.");
                        }
                        break;
                    }
                }
            }
        }
#endif

        public void NavigateBack(string fallbackScene = null)
        {
            string targetBackScene = PhysicsSceneNames.GetDefaultBackScene(currentSceneName, previousSceneName);
            if (!string.IsNullOrEmpty(fallbackScene))
            {
                targetBackScene = fallbackScene;
            }

            LoadScene(targetBackScene);
        }

        public void SaveProgress()
        {
            if (_saveData != null)
            {
                PhysicsSaveManager.Save(_saveData);
            }
        }

        public void ReloadSaveData()
        {
            _saveData = PhysicsSaveManager.Load();
        }

        public void ResetSession()
        {
            selectedCharacterId = "Lyra";
            selectedTeacherId = "Minh";
            selectedChapterId = 1;
            selectedLessonId = "";
            selectedLessonTitle = "";
            selectedMode = GameMode.None;
            previousSceneName = "";
            CurrentQuizAttempt = null;
        }

        // ==========================================
        // QUIZ ATTEMPT RUNTIME DATA
        // ==========================================

        public QuizAttemptData CurrentQuizAttempt { get; set; }

        public void ClearQuizAttempt()
        {
            CurrentQuizAttempt = null;
            Debug.Log("[PhysicsGameSession] Đã xóa dữ liệu lượt thi hiện tại (CurrentQuizAttempt).");
        }
    }

    [Serializable]
    public class QuizAttemptAnswer
    {
        public string questionId;
        public string questionText;
        public List<string> options = new List<string>();
        public int selectedAnswerIndex;
        public int correctAnswerIndex;
        public bool wasCorrect;
        // False for self-assessed written answers that have no automatic grading.
        public bool isGraded = true;
        public string explanation;
        public string keyKnowledge;
    }

    [Serializable]
    public class QuizAttemptData
    {
        public string lessonId;
        public string lessonTitle;
        public List<QuizAttemptAnswer> answers = new List<QuizAttemptAnswer>();
        public int correctCount;
        public int wrongCount;
        public int unansweredCount;
        public int score;
        public float elapsedSeconds;
    }
}
