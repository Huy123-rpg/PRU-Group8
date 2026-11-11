using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>Connects the five study screens to the existing save/session systems.</summary>
    [DisallowMultipleComponent]
    public sealed class StudyScreensController : MonoBehaviour
    {
        private readonly List<TextMeshProUGUI> _texts = new List<TextMeshProUGUI>();
        private readonly List<TMP_Dropdown> _filters = new List<TMP_Dropdown>();
        private readonly HashSet<Button> _boundButtons = new HashSet<Button>();
        private readonly Dictionary<TextMeshProUGUI, string> _initialText = new Dictionary<TextMeshProUGUI, string>();
        private readonly Dictionary<TextMeshProUGUI, Color> _initialTextColor = new Dictionary<TextMeshProUGUI, Color>();
        private readonly Dictionary<int, Image> _durationIndicators = new Dictionary<int, Image>();
        private readonly Dictionary<string, Image> _goalIndicators = new Dictionary<string, Image>();
        private readonly Dictionary<int, TextMeshProUGUI> _durationLabels = new Dictionary<int, TextMeshProUGUI>();
        private readonly Dictionary<string, TextMeshProUGUI> _goalLabels = new Dictionary<string, TextMeshProUGUI>();
        private readonly Dictionary<Image, Color> _choiceIndicatorBaseColors = new Dictionary<Image, Color>();
        private int _selectedDurationThisVisit;
        private string _selectedGoalThisVisit;
        private UnityAction<int> _refreshFilters;
        private int _selectedChapter;
        private string _selectedLessonId;
        private string _selectedDifficulty;
        private int _selectedPeriod;

        // Cached UI references for Scene 24 (Tiến bộ học tập)
        private TextMeshProUGUI _progressChapterText;
        private TextMeshProUGUI _progressLessonText;
        private TextMeshProUGUI _progressDifficultyText;
        private TextMeshProUGUI _progressPrevScoreText;
        private TextMeshProUGUI _progressCurrentScoreText;
        private TextMeshProUGUI _progressBestScoreText;
        private TextMeshProUGUI _progressBar1PctText;
        private TextMeshProUGUI _progressBar2PctText;
        private TextMeshProUGUI _progressTrendLine1;
        private TextMeshProUGUI _progressTrendLine2;
        private Image _progressBar1Fill;
        private Image _progressBar2Fill;
        private Image _progressTrendArrow;

        private void Awake()
        {
            var activeScene = gameObject.scene;
            if (!IsStudyScreenScene(activeScene.name))
            {
                Debug.LogWarning($"[StudyScreensController] Disabled outside the five study screens: '{activeScene.name}'.");
                enabled = false;
                return;
            }

            foreach (var controller in FindObjectsByType<StudyScreensController>(FindObjectsInactive.Include))
            {
                if (controller != null && controller != this && controller.gameObject.scene == activeScene)
                {
                    Debug.LogWarning($"[StudyScreensController] Duplicate controller removed from scene '{activeScene.name}'.");
                    Destroy(this);
                    return;
                }
            }

            bool hasSceneEventSystem = FindObjectsByType<EventSystem>(FindObjectsInactive.Include)
                .Any(eventSystem => eventSystem != null && eventSystem.gameObject.scene == activeScene);
            if (!hasSceneEventSystem)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<InputSystemUIInputModule>();
            }

            _texts.Clear();
            _texts.AddRange(FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include)
                .Where(text => text != null && text.gameObject.scene == activeScene));
            _initialText.Clear();
            _initialTextColor.Clear();
            foreach (var text in _texts)
            {
                if (text != null)
                {
                    _initialText[text] = text.text;
                    _initialTextColor[text] = text.color;
                }
            }

            _filters.Clear();
            _filters.AddRange(FindObjectsByType<TMP_Dropdown>(FindObjectsInactive.Include)
                .Where(dropdown => dropdown != null && dropdown.gameObject.scene == activeScene));

            if (IsLeaderboard) AddWeekNavigation();
            if (IsToday) SetupTodayScreen(activeScene);
            if (IsProgress) SetupProgressScreen(activeScene);
            BindSceneButtons();
            ConfigureStudyScreenRaycasts(activeScene);

            _refreshFilters = _ => Render();
            foreach (var dropdown in _filters)
            {
                dropdown.onValueChanged.RemoveListener(_refreshFilters);
                dropdown.onValueChanged.AddListener(_refreshFilters);
            }
            Render();
        }

        private PhysicsSaveData Save => PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SaveData : PhysicsSaveManager.Load();
        private string SceneName => gameObject.scene.name;

        private static bool IsStudyScreenScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return false;
            if (sceneName == PhysicsSceneNames.ProgressOverview || sceneName.Contains("Tổng quan") || sceneName.Contains("Tong quan") || sceneName.StartsWith("24.1"))
                return false;

            return sceneName.StartsWith("22.") || sceneName.Contains("Buoi hoc") ||
                   sceneName.StartsWith("23.") || sceneName.Contains("xếp hạng") || sceneName.Contains("xep hang") ||
                   sceneName.StartsWith("24.") || sceneName.Contains("Tiến bộ") || sceneName.Contains("Tien bo") ||
                   sceneName.StartsWith("25.") || sceneName.Contains("Quản lý") || sceneName.Contains("Quan ly") ||
                   sceneName.StartsWith("26.") || sceneName.Contains("Tổng kết") || sceneName.Contains("Tong ket") ||
                   sceneName == PhysicsSceneNames.TodayStudy || sceneName == PhysicsSceneNames.WeeklyLeaderboard ||
                   sceneName == PhysicsSceneNames.LearningProgress || sceneName == PhysicsSceneNames.LearningManagement ||
                   sceneName == PhysicsSceneNames.StudySummary;
        }

        private bool IsToday => SceneName.StartsWith("22.") || SceneName.Contains("Buoi hoc") || SceneName == PhysicsSceneNames.TodayStudy;
        private bool IsLeaderboard => SceneName.StartsWith("23.") || SceneName.Contains("xếp hạng") || SceneName.Contains("xep hang") || SceneName == PhysicsSceneNames.WeeklyLeaderboard;
        private bool IsProgress => (SceneName.StartsWith("24.") && !SceneName.StartsWith("24.1")) || SceneName.Contains("Tiến bộ") || SceneName.Contains("Tien bo") || SceneName == PhysicsSceneNames.LearningProgress;
        private bool IsManagement => SceneName.StartsWith("25.") || SceneName.Contains("Quản lý") || SceneName.Contains("Quan ly") || SceneName == PhysicsSceneNames.LearningManagement;
        private bool IsSummary => SceneName.StartsWith("26.") || SceneName.Contains("Tổng kết") || SceneName.Contains("Tong ket") || SceneName == PhysicsSceneNames.StudySummary;

        private void BindSceneButtons()
        {
            foreach (var text in _texts)
            {
                if (text == null) continue;
                string label = text.text.ToLowerInvariant();

                if (IsToday)
                {
                    if (label.Contains("15 ph")) BindDurationChoice(text, 15);
                    else if (label.Contains("30 ph")) BindDurationChoice(text, 30);
                    else if (label.Contains("45 ph")) BindDurationChoice(text, 45);
                    else if (label.Contains("bắt đầu buổi")) BindLabel(text, StartNewStudy);
                    else if (label.Contains("tiếp tục bài đang học") || label.Contains("tiếp tục buổi") || label.Contains("chọn bài học")) BindLabel(text, ResumeStudy);
                    else if (label.Contains("tự chọn bài khác") || label.Contains("chọn bài khác")) BindLabel(text, ChooseAnotherLesson);
                    else if (label.Contains("gợi ý ôn tập") || label.Contains("tiến độ") || label.Contains("tiến bộ")) BindLabel(text, () => Load(PhysicsSceneNames.LearningProgress));
                    else if (label.Contains("xếp hạng")) BindLabel(text, () => Load(PhysicsSceneNames.WeeklyLeaderboard));
                    else if (label.Contains("quản lý")) BindLabel(text, () => Load(PhysicsSceneNames.LearningManagement));
                    else if (label.Contains("ôn bài cũ") || label.Contains("học bài mới") || label.Contains("luyện vận dụng")) BindGoalChoice(text);
                    else if (label.Contains("quay lại")) BindLabel(text, () => Load(PhysicsSceneNames.MainMenu));
                }
                else if (IsLeaderboard)
                {
                    if (label.Contains("tham gia")) BindLabel(text, JoinChallenge);
                    else if (label.Contains("tuần trước") || label.Contains("previous week")) BindLabel(text, () => ChangeWeek(-1));
                    else if (label.Contains("tuần sau") || label.Contains("next week")) BindLabel(text, () => ChangeWeek(1));
                    else if (label.Contains("quay lại")) BindLabel(text, ReturnToPreviousOrTodayStudy);
                }
                else if (IsProgress)
                {
                    // Selectors and action buttons for Progress screen are managed in SetupProgressScreen
                }
                else if (IsManagement)
                {
                    if (label.Contains("thử thách tuần")) BindLabel(text, () => Load(PhysicsSceneNames.WeeklyLeaderboard));
                    else if (label.Contains("buổi học")) BindLabel(text, () => Load(PhysicsSceneNames.TodayStudy));
                    else if (label.Contains("phút")) BindLabel(text, CyclePlanMinutes);
                    else if (label.Contains("kết thúc khi hết giờ") || label.Contains("kết thức khi hết giờ")) BindLabel(text, ToggleEndAtTime);
                    else if (label.Contains("quay lại")) BindLabel(text, ReturnToPreviousOrTodayStudy);
                    else if (label.Contains("lưu") || label.Contains("thiết lập")) BindLabel(text, SaveSettings);
                    else if (label.Contains("google sheets") || label.Contains("đồng bộ dữ liệu")) BindLabel(text, ShowNoSyncSupport);
                }
                else if (IsSummary)
                {
                    if (label.Contains("xem tiến độ") || label.Contains("tiến độ")) BindLabel(text, () => Load(PhysicsSceneNames.LearningProgress));
                    else if (label.Contains("kết thúc") || label.Contains("về trang chủ") || label.Contains("hoàn tất")) BindLabel(text, FinishAndReturn);
                    else if (label.Contains("quay lại")) BindLabel(text, () => Load(PhysicsSceneNames.TodayStudy));
                }
            }
        }

        private static void Bind(Button button, UnityAction action)
        {
            if (button == null) return;
            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        private Button BindLabel(TextMeshProUGUI label, UnityAction action)
        {
            if (label == null || label.gameObject.scene != gameObject.scene) return null;

            // 1. Ensure the label itself has a Button component so clicking directly on the text ALWAYS works
            var buttonOnLabel = label.GetComponent<Button>();
            if (buttonOnLabel == null) buttonOnLabel = label.gameObject.AddComponent<Button>();
            buttonOnLabel.targetGraphic = label;
            label.raycastTarget = true;
            if (_boundButtons.Add(buttonOnLabel)) Bind(buttonOnLabel, action);

            // 2. Also locate the background box / frame Image behind this label and make it clickable too
            Image bgImage = FindSiblingButtonImage(label) ?? FindClosestContainingImage(label);
            if (bgImage != null && bgImage.gameObject != label.gameObject)
            {
                var buttonOnBg = bgImage.GetComponent<Button>();
                if (buttonOnBg == null) buttonOnBg = bgImage.gameObject.AddComponent<Button>();
                buttonOnBg.targetGraphic = bgImage;
                bgImage.raycastTarget = true;
                if (_boundButtons.Add(buttonOnBg)) Bind(buttonOnBg, action);
                UIHoverPulseEffect.Attach(bgImage.gameObject, hover: 1.05f, minPulse: 1.02f, maxPulse: 1.07f, outline: true);
            }
            else
            {
                UIHoverPulseEffect.Attach(label.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: false);
            }

            return buttonOnLabel;
        }

        private void BindDurationChoice(TextMeshProUGUI label, int minutes)
        {
            FitText(label, 18f, 28f);
            _durationLabels[minutes] = label;
            var button = BindLabel(label, () =>
            {
                _selectedDurationThisVisit = minutes;
                SetMinutes(minutes);
                UpdateTodayDurationVisuals(minutes);
            });
            ConfigureChoiceButton(button);

            // Find choice indicator (border/frame or radio indicator)
            var indicator = FindSiblingButtonImage(label) ?? FindClosestContainingImage(label) ?? FindChoiceIndicator(label) ?? CreateChoiceIndicator(label);
            RegisterChoiceIndicatorColor(indicator);
            if (indicator != null) _durationIndicators[minutes] = indicator;
        }

        private void BindGoalChoice(TextMeshProUGUI label)
        {
            FitText(label, 16f, 26f);
            string goal = label.text.IndexOf("Luyện", StringComparison.OrdinalIgnoreCase) >= 0 ? "Luyện vận dụng" :
                label.text.IndexOf("Học bài mới", StringComparison.OrdinalIgnoreCase) >= 0 ? "Học bài mới" : "Ôn bài cũ";
            _goalLabels[goal] = label;
            var button = BindLabel(label, () => SetGoal(goal));
            ConfigureChoiceButton(button);

            var indicator = FindSiblingButtonImage(label) ?? FindClosestContainingImage(label) ?? FindChoiceIndicator(label) ?? CreateChoiceIndicator(label);
            RegisterChoiceIndicatorColor(indicator);
            if (indicator != null) _goalIndicators[goal] = indicator;
        }

        private static void ConfigureChoiceButton(Button button)
        {
            if (button == null) return;
            button.transition = Selectable.Transition.None;
        }

        private void RegisterChoiceIndicatorColor(Image indicator)
        {
            if (indicator != null && !_choiceIndicatorBaseColors.ContainsKey(indicator))
                _choiceIndicatorBaseColors[indicator] = indicator.color;
        }

        private Image FindChoiceIndicator(TextMeshProUGUI label)
        {
            RectTransform labelRect = label.rectTransform;
            if (labelRect == null) return null;
            GetWorldRect(labelRect, out float labelMinX, out float labelMinY, out float labelMaxX, out float labelMaxY);
            float labelCenterX = (labelMinX + labelMaxX) * 0.5f;
            float labelCenterY = (labelMinY + labelMaxY) * 0.5f;
            float labelHeight = Mathf.Max(1f, labelMaxY - labelMinY);
            Image best = null;
            float bestScore = float.MaxValue;

            foreach (var image in FindObjectsByType<Image>(FindObjectsInactive.Include))
            {
                if (image == null || image.gameObject.scene != label.gameObject.scene || image.transform.parent != labelRect.parent ||
                    image.GetComponentInParent<Selectable>() != null)
                    continue;

                RectTransform imageRect = image.rectTransform;
                GetWorldRect(imageRect, out float minX, out float minY, out float maxX, out float maxY);
                float width = maxX - minX;
                float height = maxY - minY;
                float sizeRatio = width / Mathf.Max(1f, height);
                if (width < labelHeight * 0.55f || width > labelHeight * 1.55f ||
                    height < labelHeight * 0.55f || height > labelHeight * 1.55f ||
                    sizeRatio < 0.65f || sizeRatio > 1.35f)
                    continue;

                float centerX = (minX + maxX) * 0.5f;
                float centerY = (minY + maxY) * 0.5f;
                float horizontalGap = labelCenterX - centerX;
                float verticalGap = Mathf.Abs(labelCenterY - centerY);
                if (horizontalGap < labelHeight * 0.75f || horizontalGap > labelHeight * 4f || verticalGap > labelHeight * 1.1f)
                    continue;

                float score = horizontalGap + verticalGap * 2f;
                if (score >= bestScore) continue;
                best = image;
                bestScore = score;
            }
            return best;
        }

        private Image CreateChoiceIndicator(TextMeshProUGUI label)
        {
            Image sample = FindObjectsByType<Image>(FindObjectsInactive.Include)
                .FirstOrDefault(image => image != null && image.gameObject.scene == label.gameObject.scene &&
                    image.transform.parent == label.rectTransform.parent &&
                    image.rectTransform.rect.width >= 32f && image.rectTransform.rect.width <= 56f &&
                    image.rectTransform.rect.height >= 32f && image.rectTransform.rect.height <= 56f && image.sprite != null);
            if (sample == null) return null;

            var indicatorObject = new GameObject($"{label.text.Trim()} Choice Indicator", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            indicatorObject.transform.SetParent(label.rectTransform.parent, false);
            var rect = (RectTransform)indicatorObject.transform;
            rect.anchorMin = label.rectTransform.anchorMin;
            rect.anchorMax = label.rectTransform.anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            float diameter = Mathf.Min(44f, label.rectTransform.rect.height * 0.9f);
            rect.sizeDelta = new Vector2(diameter, diameter);
            rect.anchoredPosition = label.rectTransform.anchoredPosition +
                new Vector2(-label.rectTransform.rect.width * 0.5f - diameter * 0.75f, -label.rectTransform.rect.height * 0.5f);
            var image = indicatorObject.GetComponent<Image>();
            image.sprite = sample.sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            indicatorObject.transform.SetAsLastSibling();
            return image;
        }

        private static Image FindClosestContainingImage(TextMeshProUGUI label)
        {
            var labelRect = label.rectTransform;
            if (labelRect == null) return null;
            GetWorldRect(labelRect, out float labelMinX, out float labelMinY, out float labelMaxX, out float labelMaxY);
            float labelCenterX = (labelMinX + labelMaxX) * 0.5f;
            float labelCenterY = (labelMinY + labelMaxY) * 0.5f;

            Image best = null;
            float bestArea = float.MaxValue;
            foreach (var image in FindObjectsByType<Image>(FindObjectsInactive.Include))
            {
                if (image == null || image.gameObject.scene != label.gameObject.scene ||
                    image.transform == label.transform || image.GetComponent<Canvas>() != null)
                    continue;

                var imageRect = image.rectTransform;
                if (imageRect == null) continue;
                GetWorldRect(imageRect, out float imgMinX, out float imgMinY, out float imgMaxX, out float imgMaxY);
                float imgW = imgMaxX - imgMinX;
                float imgH = imgMaxY - imgMinY;
                float imgArea = imgW * imgH;

                if (labelCenterX < imgMinX || labelCenterX > imgMaxX || labelCenterY < imgMinY || labelCenterY > imgMaxY)
                    continue;
                if (imgArea > 1920f * 1080f * 0.25f || imgArea < 2000f) continue;

                float overlapW = Mathf.Max(0f, Mathf.Min(labelMaxX, imgMaxX) - Mathf.Max(labelMinX, imgMinX));
                float overlapH = Mathf.Max(0f, Mathf.Min(labelMaxY, imgMaxY) - Mathf.Max(labelMinY, imgMinY));
                if (overlapW <= 0f || overlapH <= 0f) continue;
                if (imgArea >= bestArea) continue;
                best = image;
                bestArea = imgArea;
            }
            return best;
        }

        private static Image FindSiblingButtonImage(TextMeshProUGUI label)
        {
            var labelRect = label.rectTransform;
            if (labelRect == null || labelRect.parent == null) return null;

            GetWorldRect(labelRect, out float labelMinX, out float labelMinY, out float labelMaxX, out float labelMaxY);
            float labelArea = Mathf.Max(1f, (labelMaxX - labelMinX) * (labelMaxY - labelMinY));
            float labelCenterX = (labelMinX + labelMaxX) * 0.5f;
            float labelCenterY = (labelMinY + labelMaxY) * 0.5f;
            Image best = null;
            float bestArea = float.MaxValue;

            foreach (var image in labelRect.parent.GetComponentsInChildren<Image>(true))
            {
                if (image == null || image.transform.parent != labelRect.parent || image.transform == label.transform)
                    continue;

                var imageRect = image.rectTransform;
                if (imageRect == null) continue;
                GetWorldRect(imageRect, out float imageMinX, out float imageMinY, out float imageMaxX, out float imageMaxY);
                float imageArea = Mathf.Max(0f, (imageMaxX - imageMinX) * (imageMaxY - imageMinY));
                if (imageArea > 1920f * 1080f * 0.25f || imageArea < 2000f || imageArea >= bestArea) continue;

                bool containsCenter = labelCenterX >= imageMinX && labelCenterX <= imageMaxX &&
                                      labelCenterY >= imageMinY && labelCenterY <= imageMaxY;
                if (!containsCenter)
                {
                    float overlapWidth = Mathf.Max(0f, Mathf.Min(labelMaxX, imageMaxX) - Mathf.Max(labelMinX, imageMinX));
                    float overlapHeight = Mathf.Max(0f, Mathf.Min(labelMaxY, imageMaxY) - Mathf.Max(labelMinY, imageMinY));
                    float overlapRatio = overlapWidth * overlapHeight / labelArea;
                    if (overlapRatio < 0.25f) continue;
                }

                best = image;
                bestArea = imageArea;
            }
            return best;
        }

        private static void GetWorldRect(RectTransform rect, out float minX, out float minY, out float maxX, out float maxY)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            minX = corners.Min(point => point.x);
            minY = corners.Min(point => point.y);
            maxX = corners.Max(point => point.x);
            maxY = corners.Max(point => point.y);
        }

        private static void ConfigureStudyScreenRaycasts(Scene scene)
        {
            var selectableGraphics = new HashSet<Graphic>();
            foreach (var selectable in FindObjectsByType<Selectable>(FindObjectsInactive.Include))
            {
                if (selectable == null || selectable.gameObject.scene != scene) continue;
                if (selectable.targetGraphic != null) selectableGraphics.Add(selectable.targetGraphic);
                foreach (var g in selectable.GetComponentsInChildren<Graphic>(true))
                    selectableGraphics.Add(g);
            }

            foreach (var graphic in FindObjectsByType<Graphic>(FindObjectsInactive.Include))
            {
                if (graphic == null || graphic.gameObject.scene != scene) continue;
                if (selectableGraphics.Contains(graphic)) continue;
                graphic.raycastTarget = false;
            }
        }

        private static void FitText(TextMeshProUGUI tmp, float minSize = 14f, float maxSize = 24f)
        {
            if (tmp == null) return;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = minSize;
            tmp.fontSizeMax = maxSize;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
        }

        private void Render()
        {
            if (Save == null) return;
            if (IsToday) RenderToday();
            else if (IsLeaderboard) RenderLeaderboard();
            else if (IsProgress) RenderProgress();
            else if (IsManagement) RenderManagement();
            else if (IsSummary) RenderSummary();

            if (IsToday) UpdateChoiceHighlights();
        }

        private void UpdateChoiceHighlights()
        {
            int currentDuration = _selectedDurationThisVisit > 0 ? _selectedDurationThisVisit : (Save.plannedStudyMinutes > 0 ? Save.plannedStudyMinutes : 30);
            string currentGoal = !string.IsNullOrEmpty(_selectedGoalThisVisit) ? _selectedGoalThisVisit : (!string.IsNullOrEmpty(Save.studyGoal) ? Save.studyGoal : "Ôn bài cũ");

            if (IsToday)
            {
                UpdateTodayDurationVisuals(currentDuration);
            }

            foreach (var choice in _durationIndicators)
                SetChoiceHighlight(choice.Value, currentDuration == choice.Key);

            foreach (var choice in _goalIndicators)
                SetChoiceHighlight(choice.Value, string.Equals(currentGoal, choice.Key, StringComparison.OrdinalIgnoreCase));

            // Also highlight text label colors for crystal clear feedback
            foreach (var kv in _durationLabels)
            {
                if (kv.Value != null)
                {
                    bool sel = currentDuration == kv.Key;
                    kv.Value.color = sel ? new Color(0.2f, 0.95f, 1f, 1f) : (_initialTextColor.TryGetValue(kv.Value, out var c) ? c : Color.white);
                }
            }

            foreach (var kv in _goalLabels)
            {
                if (kv.Value != null)
                {
                    bool sel = string.Equals(currentGoal, kv.Key, StringComparison.OrdinalIgnoreCase);
                    kv.Value.color = sel ? new Color(0.2f, 0.95f, 1f, 1f) : (_initialTextColor.TryGetValue(kv.Value, out var c) ? c : Color.white);
                }
            }
        }

        private void SetChoiceHighlight(Image indicator, bool selected)
        {
            if (indicator == null) return;
            if (!_choiceIndicatorBaseColors.TryGetValue(indicator, out Color baseColor))
            {
                baseColor = indicator.color;
                _choiceIndicatorBaseColors[indicator] = baseColor;
            }

            indicator.color = selected
                ? Color.Lerp(baseColor, new Color(0.15f, 0.95f, 1f, Mathf.Max(0.85f, baseColor.a)), 0.75f)
                : baseColor;
        }

        private void RenderToday()
        {
            var save = Save;
            var active = save != null ? save.activeStudySession : null;
            PhysicsStudySessionRecord latest = null;
            if (save != null && save.studySessions != null)
            {
                latest = save.studySessions.LastOrDefault(x => x != null && !x.isActive && (x.answeredCount > 0 || x.activeSeconds > 0));
            }
            PhysicsQuizHistoryEntry latestQuiz = null;
            if (latest == null && save != null && save.quizHistory != null)
            {
                latestQuiz = save.quizHistory.LastOrDefault(q => q != null && q.isComplete);
            }

            // 1. Right panel top header ("BUỔI HỌC GẦN NHẤT")
            var rightHeader = FindContaining("Buổi học gần nhất").FirstOrDefault() ?? FindContaining("BUỔI HỌC GẦN NHẤT").FirstOrDefault();
            if (rightHeader != null)
            {
                FitText(rightHeader, 14f, 20f);
                if (active != null && active.isActive)
                    rightHeader.text = $"Buổi học đang dở · {FormatDuration(active.activeSeconds)} đã học";
                else
                    rightHeader.text = "BUỔI HỌC GẦN NHẤT";
            }

            // 2. Tìm 2 text bên panel phải: Câu đúng và Thời gian (tọa độ x > 450)
            TextMeshProUGUI stat1 = null;
            TextMeshProUGUI stat2 = null;
            foreach (var t in _texts)
            {
                if (t == null) continue;
                Vector2 pos = t.rectTransform.anchoredPosition;
                if (pos.x > 450f)
                {
                    if (pos.y > -50f && stat1 == null) stat1 = t;
                    else if (pos.y <= -50f && stat2 == null) stat2 = t;
                }
            }
            if (stat1 == null) stat1 = FindContaining("Câu đúng:").FirstOrDefault() ?? FindContaining("Chưa có kết").FirstOrDefault();
            if (stat2 == null) stat2 = FindContaining("Thời gian:").FirstOrDefault() ?? FindContaining("Thời gian mục tiêu").FirstOrDefault();

            if (stat1 != null)
            {
                stat1.raycastTarget = false;
                stat1.enableAutoSizing = true;
                stat1.fontSizeMin = 14f;
                stat1.fontSizeMax = 28f;
                stat1.alignment = TextAlignmentOptions.Center;

                if (latest != null && latest.answeredCount > 0)
                    stat1.text = $"{latest.correctCount}/{latest.answeredCount}";
                else if (latestQuiz != null && latestQuiz.totalCount > 0)
                    stat1.text = $"{latestQuiz.correctCount}/{latestQuiz.totalCount}";
                else
                    stat1.text = "--/--"; // Không hiển thị số mẫu cố định 6/10
            }

            if (stat2 != null)
            {
                stat2.raycastTarget = false;
                stat2.enableAutoSizing = true;
                stat2.fontSizeMin = 14f;
                stat2.fontSizeMax = 28f;
                stat2.alignment = TextAlignmentOptions.Center;

                if (latest != null && latest.activeSeconds > 0)
                {
                    int mins = Mathf.Max(1, Mathf.RoundToInt(latest.activeSeconds / 60f));
                    stat2.text = $"{mins} phút";
                }
                else if (latestQuiz != null && latestQuiz.elapsedSeconds > 0)
                {
                    int mins = Mathf.Max(1, Mathf.RoundToInt(latestQuiz.elapsedSeconds / 60f));
                    stat2.text = $"{mins} phút";
                }
                else
                {
                    stat2.text = "--"; // Không hiển thị số mẫu cố định 20 phút
                }
            }

            // 4. Recommendation text
            var recText = FindContaining("Ôn lại").FirstOrDefault();
            if (recText != null) FitText(recText, 14f, 22f);

            // 5. Bottom banner on left panel: Lesson & Chapter labels
            var lessonLabel = FindContaining("Bài đang học:").FirstOrDefault() ??
                              FindContaining("Bài gần nhất:").FirstOrDefault() ??
                              FindContaining("Buổi học đang tạm dừng").FirstOrDefault() ??
                              FindContaining("Chưa có bài").FirstOrDefault();

            var chapterLabel = _texts.FirstOrDefault(t => t != null && t != lessonLabel &&
                _initialText.TryGetValue(t, out var orig) && orig.IndexOf("Chương II", StringComparison.OrdinalIgnoreCase) >= 0);

            if (lessonLabel != null)
            {
                FitText(lessonLabel, 16f, 22f);
                if (active != null && active.isActive)
                {
                    if (active.lessonIds == null) active.lessonIds = new List<string>();
                    if (active.lessonIds.Count > 0)
                    {
                        string curId = active.lessonIds[active.lessonIds.Count - 1];
                        lessonLabel.text = "Bài đang học: " + GetLessonTitle(curId);
                        lessonLabel.rectTransform.anchoredPosition = new Vector2(-223.65f, -282f);
                        if (chapterLabel != null)
                        {
                            chapterLabel.text = GetChapterTitleForLesson(curId);
                            FitText(chapterLabel, 13f, 18f);
                            chapterLabel.rectTransform.anchoredPosition = new Vector2(-248.85f, -320f);
                        }
                    }
                    else
                    {
                        lessonLabel.text = "Buổi học đang tạm dừng — chọn Tiếp tục để học.";
                        lessonLabel.rectTransform.anchoredPosition = new Vector2(-223.65f, -298f);
                        if (chapterLabel != null) chapterLabel.text = "";
                    }
                }
                else
                {
                    if (latest != null && latest.lessonIds != null && latest.lessonIds.Count > 0)
                    {
                        string lastId = latest.lessonIds[latest.lessonIds.Count - 1];
                        lessonLabel.text = "Bài gần nhất: " + GetLessonTitle(lastId);
                        lessonLabel.rectTransform.anchoredPosition = new Vector2(-223.65f, -282f);
                        if (chapterLabel != null)
                        {
                            chapterLabel.text = GetChapterTitleForLesson(lastId);
                            FitText(chapterLabel, 13f, 18f);
                            chapterLabel.rectTransform.anchoredPosition = new Vector2(-248.85f, -320f);
                        }
                    }
                    else
                    {
                        lessonLabel.text = "Chưa có bài đang học. Chọn bài để bắt đầu.";
                        lessonLabel.rectTransform.anchoredPosition = new Vector2(-223.65f, -298f);
                        if (chapterLabel != null) chapterLabel.text = "";
                    }
                }
            }

            // Cập nhật tên nút "Tiếp tục bài đang học" nếu lần đầu chưa học bài nào
            bool hasLearnedAny = !string.IsNullOrEmpty(Save.lastLessonId) ||
                (active != null && active.lessonIds != null && active.lessonIds.Count > 0) ||
                (latest != null && latest.lessonIds != null && latest.lessonIds.Count > 0);

            var resumeBtn = _texts.FirstOrDefault(t => t != null && _initialText.TryGetValue(t, out var orig) &&
                orig.IndexOf("Tiếp tục bài", StringComparison.OrdinalIgnoreCase) >= 0);
            if (resumeBtn != null)
            {
                resumeBtn.text = hasLearnedAny ? "Tiếp tục bài đang học" : "Chọn bài học";
                FitText(resumeBtn, 18f, 26f);
            }

            // Fit action button texts so they fit cleanly
            foreach (var t in _texts)
            {
                if (t == null) continue;
                string s = t.text.ToLowerInvariant();
                if (s.Contains("tiếp tục bài") || s.Contains("tự chọn bài") || s.Contains("chọn bài học"))
                    FitText(t, 18f, 26f);
                else if (s.Contains("quay lại"))
                    FitText(t, 18f, 28f);
            }
        }

        private void RenderLeaderboard()
        {
            var weekId = CurrentWeekId(Save.selectedChallengeWeekOffset);
            var titleText = FindContaining("TUẦN HIỆN TẠI").FirstOrDefault();
            if (titleText != null)
            {
                FitText(titleText, 18f, 26f);
                titleText.text = "THỬ THÁCH CÁ NHÂN · " + weekId;
            }

            var subjectText = FindContaining("Chủ đề:").FirstOrDefault();
            if (subjectText != null)
            {
                FitText(subjectText, 14f, 22f);
                subjectText.text = "Thử thách tuần: dữ liệu nhiều người chơi chưa được kết nối.";
            }

            var userStatus = FindContaining("Bạn: Chưa tham gia").FirstOrDefault() ?? FindContaining("Bạn: Đã tham gia").FirstOrDefault();
            if (userStatus != null)
            {
                FitText(userStatus, 14f, 22f);
                userStatus.text = Save.joinedChallengeWeekIds.Contains(weekId)
                    ? "Bạn: Đã tham gia thử thách cá nhân · chưa có bộ đề riêng"
                    : "Bạn: Chưa tham gia thử thách cá nhân";
            }

            SetContaining("Lịch sử tuần", "Lịch sử thử thách cá nhân");
            int daysToMonday = ((int)DayOfWeek.Monday - (int)DateTime.Now.DayOfWeek + 7) % 7;
            if (daysToMonday == 0) daysToMonday = 7;
            SetExactInitial("Còn 3 ngày", $"Còn {daysToMonday} ngày trong tuần");

            foreach (string sample in new[] { "Gia Bảo", "Hoàng Nam", "Minh Anh", "Thu Hà", "1", "2", "3", "4", "80", "90", "100" })
                SetAllExactInitial(sample, "—");
            SetContaining("Điểm", "Điểm cá nhân");

            var backBtn = FindContaining("Quay lại").FirstOrDefault();
            if (backBtn != null) FitText(backBtn, 18f, 28f);
            var joinBtn = FindContaining("Tham gia").FirstOrDefault();
            if (joinBtn != null) FitText(joinBtn, 18f, 28f);
        }

        private void RenderProgress()
        {
            InitProgressSelections();

            // 1. Selector labels
            if (_progressChapterText != null)
            {
                var ch = PhysicsCurriculumData.GetChapter(_selectedChapter);
                _progressChapterText.text = $"Chương {ch.romanNumber}: {ch.chapterName}";
                FitText(_progressChapterText, 14f, 22f);
            }
            if (_progressLessonText != null)
            {
                _progressLessonText.text = GetLessonTitle(_selectedLessonId);
                FitText(_progressLessonText, 14f, 22f);
            }
            if (_progressDifficultyText != null)
            {
                _progressDifficultyText.text = GetDifficultyDisplayName(_selectedDifficulty);
                FitText(_progressDifficultyText, 14f, 22f);
            }

            // 2. Filter quiz history with the 5 criteria
            var matching = FilterProgressHistory();

            var current = matching.FirstOrDefault();
            var previous = matching.Skip(1).FirstOrDefault();
            var best = matching.OrderByDescending(x => x.scorePercent).FirstOrDefault();

            // 3. Top Score Boxes (Lần trước, Lần này, Cao nhất)
            if (_progressPrevScoreText != null)
            {
                _progressPrevScoreText.text = previous != null ? $"{previous.scorePercent}/100" : "—";
                FitText(_progressPrevScoreText, 18f, 32f);
            }
            if (_progressCurrentScoreText != null)
            {
                _progressCurrentScoreText.text = current != null ? $"{current.scorePercent}/100" : "—";
                FitText(_progressCurrentScoreText, 18f, 32f);
            }
            if (_progressBestScoreText != null)
            {
                _progressBestScoreText.text = best != null ? $"{best.scorePercent}/100" : "—";
                FitText(_progressBestScoreText, 18f, 32f);
            }

            // 4. Progress Bars
            // Bar 1 (Previous)
            if (_progressBar1Fill != null)
            {
                _progressBar1Fill.type = Image.Type.Filled;
                _progressBar1Fill.fillMethod = Image.FillMethod.Horizontal;
                _progressBar1Fill.fillOrigin = (int)Image.OriginHorizontal.Left;
                _progressBar1Fill.rectTransform.sizeDelta = new Vector2(473.27f, _progressBar1Fill.rectTransform.sizeDelta.y);
                _progressBar1Fill.rectTransform.anchoredPosition = new Vector2(-158f, _progressBar1Fill.rectTransform.anchoredPosition.y);
                _progressBar1Fill.fillAmount = previous != null ? previous.scorePercent / 100f : 0f;
                _progressBar1Fill.gameObject.SetActive(previous != null && previous.scorePercent > 0);
            }
            if (_progressBar1PctText != null)
            {
                _progressBar1PctText.text = previous != null ? $"{previous.scorePercent}%" : "";
                FitText(_progressBar1PctText, 18f, 30f);
            }

            // Bar 2 (Current)
            if (_progressBar2Fill != null)
            {
                _progressBar2Fill.type = Image.Type.Filled;
                _progressBar2Fill.fillMethod = Image.FillMethod.Horizontal;
                _progressBar2Fill.fillOrigin = (int)Image.OriginHorizontal.Left;
                _progressBar2Fill.rectTransform.sizeDelta = new Vector2(473.27f, _progressBar2Fill.rectTransform.sizeDelta.y);
                _progressBar2Fill.rectTransform.anchoredPosition = new Vector2(-158f, _progressBar2Fill.rectTransform.anchoredPosition.y);
                _progressBar2Fill.fillAmount = current != null ? current.scorePercent / 100f : 0f;
                _progressBar2Fill.gameObject.SetActive(current != null && current.scorePercent > 0);
            }
            if (_progressBar2PctText != null)
            {
                _progressBar2PctText.text = current != null ? $"{current.scorePercent}%" : "";
                FitText(_progressBar2PctText, 18f, 30f);
            }

            // 5. Trend Area (Arrow & 2 Text Lines)
            if (current != null && previous != null)
            {
                int delta = current.scorePercent - previous.scorePercent;
                if (delta > 0)
                {
                    if (_progressTrendArrow != null)
                    {
                        _progressTrendArrow.gameObject.SetActive(true);
                        _progressTrendArrow.rectTransform.localEulerAngles = Vector3.zero;
                        _progressTrendArrow.rectTransform.localScale = Vector3.one;
                        _progressTrendArrow.color = Color.white;
                    }
                    if (_progressTrendLine1 != null)
                    {
                        _progressTrendLine1.rectTransform.anchoredPosition = new Vector2(441.42f, -186f);
                        _progressTrendLine1.rectTransform.sizeDelta = new Vector2(200f, 50f);
                        _progressTrendLine1.text = $"Tăng {delta} điểm";
                        _progressTrendLine1.color = new Color(1f, 0.92f, 0.65f, 1f);
                        _progressTrendLine1.alignment = TextAlignmentOptions.Center;
                        FitText(_progressTrendLine1, 14f, 22f);
                    }
                    if (_progressTrendLine2 != null)
                    {
                        _progressTrendLine2.gameObject.SetActive(true);
                        _progressTrendLine2.rectTransform.anchoredPosition = new Vector2(441.42f, -236f);
                        _progressTrendLine2.rectTransform.sizeDelta = new Vector2(200f, 50f);
                        _progressTrendLine2.text = "phần trăm";
                        _progressTrendLine2.color = new Color(1f, 0.92f, 0.65f, 1f);
                        _progressTrendLine2.alignment = TextAlignmentOptions.Center;
                        FitText(_progressTrendLine2, 14f, 22f);
                    }
                }
                else if (delta < 0)
                {
                    if (_progressTrendArrow != null)
                    {
                        _progressTrendArrow.gameObject.SetActive(true);
                        _progressTrendArrow.rectTransform.localEulerAngles = Vector3.zero;
                        _progressTrendArrow.rectTransform.localScale = new Vector3(1f, -1f, 1f); // Vertical flip: arrow points DOWN, no horizontal shift
                        _progressTrendArrow.color = new Color(1f, 0.35f, 0.35f, 1f);
                    }
                    if (_progressTrendLine1 != null)
                    {
                        _progressTrendLine1.rectTransform.anchoredPosition = new Vector2(441.42f, -186f);
                        _progressTrendLine1.rectTransform.sizeDelta = new Vector2(200f, 50f);
                        _progressTrendLine1.text = $"Giảm {Mathf.Abs(delta)} điểm";
                        _progressTrendLine1.color = new Color(1f, 0.5f, 0.5f, 1f);
                        _progressTrendLine1.alignment = TextAlignmentOptions.Center;
                        FitText(_progressTrendLine1, 14f, 22f);
                    }
                    if (_progressTrendLine2 != null)
                    {
                        _progressTrendLine2.gameObject.SetActive(true);
                        _progressTrendLine2.rectTransform.anchoredPosition = new Vector2(441.42f, -236f);
                        _progressTrendLine2.rectTransform.sizeDelta = new Vector2(200f, 50f);
                        _progressTrendLine2.text = "phần trăm";
                        _progressTrendLine2.color = new Color(1f, 0.5f, 0.5f, 1f);
                        _progressTrendLine2.alignment = TextAlignmentOptions.Center;
                        FitText(_progressTrendLine2, 14f, 22f);
                    }
                }
                else
                {
                    if (_progressTrendArrow != null) _progressTrendArrow.gameObject.SetActive(false);
                    if (_progressTrendLine1 != null)
                    {
                        _progressTrendLine1.rectTransform.anchoredPosition = new Vector2(441.42f, -186f);
                        _progressTrendLine1.rectTransform.sizeDelta = new Vector2(200f, 50f);
                        _progressTrendLine1.text = "Không thay đổi";
                        _progressTrendLine1.color = new Color(0.85f, 0.85f, 0.85f, 1f);
                        _progressTrendLine1.alignment = TextAlignmentOptions.Center;
                        FitText(_progressTrendLine1, 14f, 22f);
                    }
                    if (_progressTrendLine2 != null)
                    {
                        _progressTrendLine2.gameObject.SetActive(true);
                        _progressTrendLine2.rectTransform.anchoredPosition = new Vector2(441.42f, -236f);
                        _progressTrendLine2.rectTransform.sizeDelta = new Vector2(200f, 50f);
                        _progressTrendLine2.text = "0 điểm phần trăm";
                        _progressTrendLine2.color = new Color(0.85f, 0.85f, 0.85f, 1f);
                        _progressTrendLine2.alignment = TextAlignmentOptions.Center;
                        FitText(_progressTrendLine2, 14f, 22f);
                    }
                }
            }
            else if (current != null)
            {
                if (_progressTrendArrow != null) _progressTrendArrow.gameObject.SetActive(false);
                if (_progressTrendLine1 != null)
                {
                    _progressTrendLine1.rectTransform.anchoredPosition = new Vector2(441.42f, -186f);
                    _progressTrendLine1.rectTransform.sizeDelta = new Vector2(200f, 50f);
                    _progressTrendLine1.text = "Cần thêm lượt";
                    _progressTrendLine1.color = new Color(0.85f, 0.85f, 0.85f, 1f);
                    _progressTrendLine1.alignment = TextAlignmentOptions.Center;
                    FitText(_progressTrendLine1, 14f, 22f);
                }
                if (_progressTrendLine2 != null)
                {
                    _progressTrendLine2.gameObject.SetActive(true);
                    _progressTrendLine2.rectTransform.anchoredPosition = new Vector2(441.42f, -236f);
                    _progressTrendLine2.rectTransform.sizeDelta = new Vector2(200f, 50f);
                    _progressTrendLine2.text = "để so sánh";
                    _progressTrendLine2.color = new Color(0.85f, 0.85f, 0.85f, 1f);
                    _progressTrendLine2.alignment = TextAlignmentOptions.Center;
                    FitText(_progressTrendLine2, 14f, 22f);
                }
            }
            else
            {
                if (_progressTrendArrow != null) _progressTrendArrow.gameObject.SetActive(false);
                if (_progressTrendLine1 != null)
                {
                    _progressTrendLine1.rectTransform.anchoredPosition = new Vector2(441.42f, -200f);
                    _progressTrendLine1.rectTransform.sizeDelta = new Vector2(250f, 40f);
                    _progressTrendLine1.text = "Chưa có kết quả học tập";
                    _progressTrendLine1.color = new Color(0.85f, 0.85f, 0.85f, 1f);
                    _progressTrendLine1.alignment = TextAlignmentOptions.Center;
                    FitText(_progressTrendLine1, 13f, 18f);
                }
                if (_progressTrendLine2 != null)
                {
                    _progressTrendLine2.text = "";
                    _progressTrendLine2.gameObject.SetActive(false);
                }
            }
        }

        private void RenderManagement()
        {
            var history = (Save.quizHistory ?? new List<PhysicsQuizHistoryEntry>()).Where(x => x != null).ToList();
            var sheets = FindContaining("Google Sheets").FirstOrDefault();
            if (sheets != null) { FitText(sheets, 14f, 22f); sheets.text = "Google Sheets: Chưa hỗ trợ trong phiên bản này"; }

            var sync = FindContaining("Đồng bộ dữ liệu").FirstOrDefault();
            if (sync != null) { FitText(sync, 14f, 22f); sync.text = "Dữ liệu đã lưu trên máy · chưa đồng bộ"; }

            SetContaining("25 phút", $"{Save.plannedStudyMinutes} phút");
            SetExactInitial("Ánh sáng", "Chưa có thử thách cá nhân cấu hình");
            SetExactInitial("Hoàng Nam", "Người học cục bộ");
            SetExactInitial("Minh Anh", "—");
            SetExactInitial("Thu Hà", "—");

            foreach (var text in _initialText.Where(x => x.Value.IndexOf("hết giờ", StringComparison.OrdinalIgnoreCase) >= 0).Select(x => x.Key))
            {
                FitText(text, 16f, 26f);
                text.text = "Kết thúc khi hết giờ: " + (Save.endStudyWhenTimeReached ? "Bật" : "Tắt");
            }

            foreach (string sample in new[] { "Phản xạ toàn phần", "Lăng kính", "Mạch điện" }) SetAllExactInitial(sample, "—");
            var latestCompleted = history.LastOrDefault(x => x.isComplete);
            SetContaining("90", latestCompleted != null ? latestCompleted.scorePercent.ToString() : "—");
            RenderHistoryRows(history);

            foreach (var t in _texts)
            {
                if (t == null) continue;
                string s = t.text.ToLowerInvariant();
                if (s.Contains("quay lại") || s.Contains("thử thách tuần") || s.Contains("lưu cấu hình"))
                    FitText(t, 16f, 26f);
            }
        }

        private void AddWeekNavigation()
        {
            var title = _initialText.FirstOrDefault(x => x.Value.IndexOf("TUẦN HIỆN TẠI", StringComparison.OrdinalIgnoreCase) >= 0).Key;
            if (title == null) return;
            AddWeekArrow(title, "WeekPrevious", "‹", -1);
            AddWeekArrow(title, "WeekNext", "›", 1);
        }

        private void AddWeekArrow(TextMeshProUGUI title, string name, string glyph, int direction)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(title.transform.parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = title.rectTransform.anchorMin;
            rt.pivot = title.rectTransform.pivot;
            rt.anchoredPosition = title.rectTransform.anchoredPosition + new Vector2(direction * (title.rectTransform.rect.width * .5f + 30f), 0f);
            rt.sizeDelta = new Vector2(36f, 36f);
            var image = go.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, .75f);
            image.raycastTarget = true;
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => ChangeWeek(direction));
            _boundButtons.Add(button);

            var textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(go.transform, false);
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var label = textObject.GetComponent<TextMeshProUGUI>();
            label.text = glyph;
            label.font = title.font;
            label.fontSize = 24f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = title.color;
            label.raycastTarget = false;
        }

        private void RenderSummary()
        {
            var session = Save.activeStudySession;
            if (session == null) session = Save.studySessions != null ? Save.studySessions.LastOrDefault() : null;
            if (session == null)
            {
                SetContaining("25 phút", "Chưa có buổi học đang mở");
                SetContaining("2 bài", "0 bài");
                SetContaining("18 câu", "0 câu");
                return;
            }
            SetContaining("25 phút", FormatDuration(session.activeSeconds));
            SetContaining("2 bài", $"{(session.lessonIds != null ? session.lessonIds.Count : 0)} bài");
            SetContaining("18 câu", $"{session.answeredCount} câu đã trả lời");
            SetContaining("8/10 câu đúng", $"{session.correctCount}/{session.quizAnsweredCount} câu trắc nghiệm đúng");
            SetContaining("6/8 câu đúng", $"{session.correctCount}/{session.quizAnsweredCount} câu trắc nghiệm đúng");
            var lessonRows = _initialText.Where(x => x.Value.Trim() == "Phản xạ toàn phần" || x.Value.Trim() == "Lăng kính").Select(x => x.Key).ToList();
            var lessonNames = (session.lessonIds ?? new List<string>()).Select(GetLessonTitle).ToList();
            for (int i = 0; i < lessonRows.Count; i++)
            {
                FitText(lessonRows[i], 14f, 22f);
                lessonRows[i].text = i < lessonNames.Count ? lessonNames[i] : "—";
            }

            var feedback = FindContaining("Bạn đã hoàn thành mục tiêu").FirstOrDefault();
            if (feedback != null)
            {
                FitText(feedback, 14f, 22f);
                feedback.text = session.answeredCount > 0
                    ? $"Bạn đã làm {session.answeredCount} câu; {session.ungradedAnsweredCount} câu tự luận đang chờ tự đối chiếu. Mục tiêu: {session.plannedMinutes} phút."
                    : "Buổi học đã kết thúc. Hãy chọn một bài để tiếp tục luyện tập.";
            }

            foreach (var t in _texts)
            {
                if (t == null) continue;
                string s = t.text.ToLowerInvariant();
                if (s.Contains("xem tiến độ") || s.Contains("kết thúc buổi học"))
                    FitText(t, 16f, 26f);
            }
        }

        private void RenderHistoryRows(List<PhysicsQuizHistoryEntry> entries)
        {
            var scroll = EnsureHistoryScroll();
            if (scroll == null || scroll.content == null) return;
            var content = scroll.content;
            for (int i = content.childCount - 1; i >= 0; i--)
                if (content.GetChild(i).name.StartsWith("RuntimeStudyHistory")) Destroy(content.GetChild(i).gameObject);
            string[] lines = entries.Count == 0
                ? new[] { "Chưa có lịch sử học tập." }
                : entries.OrderByDescending(x => x.completedAtUtc).Take(20).Select(x => x.isComplete
                    ? $"{x.completedAtUtc}  ·  {GetLessonTitle(x.lessonId)}  ·  {x.activityType}  ·  {x.scorePercent}%  ·  {x.correctCount}/{x.totalCount} câu"
                    : $"{x.completedAtUtc}  ·  {GetLessonTitle(x.lessonId)}  ·  {x.activityType}  ·  Chưa hoàn tất ({x.correctCount + x.wrongCount}/{x.totalCount} câu)").ToArray();
            float rowHeight = 40f;
            for (int i = 0; i < lines.Length; i++)
            {
                var row = new GameObject("RuntimeStudyHistory", typeof(RectTransform));
                row.transform.SetParent(content, false);
                var rt = row.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(.5f, 1f);
                rt.anchoredPosition = new Vector2(0f, -i * rowHeight);
                rt.sizeDelta = new Vector2(0f, rowHeight);
                var tmp = row.AddComponent<TextMeshProUGUI>();
                tmp.text = lines[i];
                tmp.font = _texts.FirstOrDefault(x => x != null)?.font;
                tmp.fontSize = 15;
                tmp.color = new Color(.08f, .12f, .22f, 1f);
                tmp.alignment = TextAlignmentOptions.MidlineLeft;
                tmp.margin = new Vector4(10f, 0f, 8f, 0f);
                tmp.raycastTarget = false;
            }
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(160f, lines.Length * rowHeight));
            scroll.verticalNormalizedPosition = 1f;
        }

        private ScrollRect EnsureHistoryScroll()
        {
            var viewport = GameObject.Find("RuntimeStudyHistoryViewport");
            if (viewport == null)
            {
                var table = GameObject.Find("Image (8)");
                if (table == null || table.scene != SceneManager.GetActiveScene()) return null;
                viewport = new GameObject("RuntimeStudyHistoryViewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
                viewport.transform.SetParent(table.transform, false);
                var viewportRect = viewport.GetComponent<RectTransform>();
                viewportRect.anchorMin = viewportRect.anchorMax = new Vector2(.5f, .5f);
                viewportRect.pivot = new Vector2(.5f, .5f);
                viewportRect.anchoredPosition = new Vector2(-11f, -154f);
                viewportRect.sizeDelta = new Vector2(1132f, 160f);
                var image = viewport.GetComponent<Image>();
                image.color = new Color(1f, 1f, 1f, 0f);
                image.raycastTarget = true;
                foreach (string sampleRow in new[] { "HistoryTable (2)", "HistoryTable (3)" })
                {
                    var sample = GameObject.Find(sampleRow);
                    if (sample != null && sample.scene == SceneManager.GetActiveScene()) sample.SetActive(false);
                }
                var contentObject = new GameObject("RuntimeStudyHistoryContent", typeof(RectTransform));
                contentObject.transform.SetParent(viewport.transform, false);
                var contentRect = contentObject.GetComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0f, 1f);
                contentRect.anchorMax = new Vector2(1f, 1f);
                contentRect.pivot = new Vector2(.5f, 1f);
                contentRect.sizeDelta = new Vector2(0f, 160f);
                var scroll = viewport.GetComponent<ScrollRect>();
                scroll.viewport = viewportRect;
                scroll.content = contentRect;
                scroll.horizontal = false;
                scroll.vertical = true;
                scroll.movementType = ScrollRect.MovementType.Clamped;
                return scroll;
            }
            return viewport.GetComponent<ScrollRect>();
        }

        private IEnumerable<PhysicsQuizHistoryEntry> FilterHistory()
        {
            IEnumerable<PhysicsQuizHistoryEntry> q = (Save.quizHistory ?? new List<PhysicsQuizHistoryEntry>()).Where(x => x != null && x.isComplete);
            if (_selectedChapter > 0) q = q.Where(x => x.chapterId == _selectedChapter);
            if (!string.IsNullOrEmpty(_selectedLessonId)) q = q.Where(x => x.lessonId == _selectedLessonId);
            if (!string.IsNullOrEmpty(_selectedDifficulty)) q = q.Where(x => x.assessmentKey != null && x.assessmentKey.Split('|').Contains(_selectedDifficulty, StringComparer.OrdinalIgnoreCase));
            if (_selectedPeriod == 1) q = q.Where(x => DateTime.TryParse(x.completedAtUtc, out var d) && d.ToUniversalTime() >= DateTime.UtcNow.AddDays(-7));
            if (_selectedPeriod == 2)
            {
                var latestSession = Save.studySessions != null ? Save.studySessions.LastOrDefault(x => x != null && !x.isActive) : null;
                q = latestSession != null ? q.Where(x => x.sessionId == latestSession.sessionId) : q.OrderByDescending(x => x.completedAtUtc).Take(1);
            }
            if (_filters.Count > 0)
            {
                var chapter = _filters[0];
                if (chapter.options.Count > 0 && int.TryParse(chapter.options[chapter.value].text, out int ch)) q = q.Where(x => x.chapterId == ch);
            }
            if (_filters.Count > 1)
            {
                var period = _filters[_filters.Count - 1];
                if (period.value == 1) q = q.Where(x => DateTime.TryParse(x.completedAtUtc, out var d) && d.ToUniversalTime() >= DateTime.UtcNow.AddDays(-7));
            }
            return q.OrderByDescending(x => x.completedAtUtc).ToList();
        }

        private static bool IsEquivalent(PhysicsQuizHistoryEntry a, PhysicsQuizHistoryEntry b)
        {
            return a != null && b != null && a.lessonId == b.lessonId && a.activityType == b.activityType &&
                   a.totalCount == b.totalCount && a.assessmentKey == b.assessmentKey;
        }

        private static bool TryGetChapter(string label, out int chapter)
        {
            chapter = 0;
            if (!label.Contains("chương")) return false;
            for (int i = 1; i <= 5; i++)
            {
                var data = PhysicsCurriculumData.GetChapter(i);
                if (label.Contains($"chương {i}") || label.Contains("chương " + data.romanNumber.ToLowerInvariant()) || label.Contains(data.chapterName.ToLowerInvariant()))
                {
                    chapter = i;
                    return true;
                }
            }
            return false;
        }

        private static bool TryGetLesson(string label, out string lessonId)
        {
            lessonId = null;
            for (int i = 1; i <= 5; i++)
            {
                foreach (var lesson in PhysicsCurriculumData.GetLessons(i))
                {
                    if (label.Contains(lesson.lessonTitle.ToLowerInvariant()) || label.Contains(lesson.DisplayName.ToLowerInvariant()))
                    {
                        lessonId = lesson.lessonId;
                        return true;
                    }
                }
            }
            return false;
        }

        private void SetMinutes(int minutes)
        {
            _selectedDurationThisVisit = minutes;
            PhysicsGameSession.Instance.ConfigureStudyPlan(minutes, Save.studyGoal);
            Render();
        }

        private void SetGoal(string goal)
        {
            _selectedGoalThisVisit = goal;
            PhysicsGameSession.Instance.ConfigureStudyPlan(Save.plannedStudyMinutes, goal);
            Render();
        }

        private void StartNewStudy()
        {
            PhysicsGameSession.Instance.FinishStudySession();
            PhysicsGameSession.Instance.StartOrResumeStudySession();
            Load(PhysicsSceneNames.ChapterSelection);
        }

        private void ResumeStudy()
        {
            PhysicsGameSession.Instance.StartOrResumeStudySession();
            if (Save.lastLessonId == PhysicsQuestionRepository.WHOLE_BOOK_REVIEW_ID)
            {
                PhysicsGameSession.Instance.SelectLesson(PhysicsQuestionRepository.WHOLE_BOOK_REVIEW_ID, "Tổng ôn cả quyển · chạy thử lễ hội");
                PhysicsGameSession.Instance.SelectMode(GameMode.Quiz);
                Load(PhysicsSceneNames.Quiz);
                return;
            }

            if (string.IsNullOrEmpty(Save.lastLessonId))
            {
                // Lần đầu chưa học bài nào: chuyển đến 04. Chọn chương
                Load(PhysicsSceneNames.ChapterSelection);
            }
            else
            {
                // Đã có bài học gần nhất: chuyển đến 17. Chọn hình thức
                Load(PhysicsSceneNames.LessonType);
            }
        }

        private void ChooseAnotherLesson() => Load(PhysicsSceneNames.ChapterSelection);

        private void ReturnToPreviousOrTodayStudy()
        {
            if (IsProgress)
            {
                Debug.Log("[StudyScreensController] Quay lại từ màn Tiến bộ (24) -> Màn Tổng quan tiến độ (RouteId.ProgressOverview).");
                if (NavigationService.Instance != null)
                    NavigationService.Instance.NavigateTo(RouteId.ProgressOverview);
                else
                    Load(PhysicsSceneNames.ProgressOverview);
                return;
            }

            string prev = PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.PreviousSceneName : "";
            if (!string.IsNullOrEmpty(prev) &&
                prev != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name &&
                prev != PhysicsSceneNames.WeeklyLeaderboard &&
                prev != PhysicsSceneNames.WeeklyRank &&
                prev != PhysicsSceneNames.LearningProgress &&
                prev != PhysicsSceneNames.LearningManagement &&
                prev != PhysicsSceneNames.TeacherStudyManagement)
            {
                Load(prev);
            }
            else
            {
                if (NavigationService.Instance != null)
                    NavigationService.Instance.NavigateTo(RouteId.MainMenu);
                else
                    Load(PhysicsSceneNames.MainMenu);
            }
        }

        private void PracticeNowFromProgress()
        {
            if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.StartOrResumeStudySession();
                PhysicsGameSession.Instance.SelectChapter(_selectedChapter);
                string title = GetLessonTitle(_selectedLessonId);
                PhysicsGameSession.Instance.SelectLesson(_selectedLessonId, title);
                PhysicsGameSession.Instance.SelectMode(GameMode.Quiz);
            }

            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.Selection.ChapterId = _selectedChapter;
                NavigationService.Instance.Selection.LessonId = _selectedLessonId;
                NavigationService.Instance.Selection.LessonTitle = GetLessonTitle(_selectedLessonId);
                string currentMode = NavigationService.Instance.Selection.Mode ?? "MCQ";
                bool isEssayMode = (currentMode == "ESSAY" || currentMode == "WRITTEN" || currentMode == "TỰ LUẬN");

                string diffCode = "TH";
                if (_selectedDifficulty != null)
                {
                    string sd = _selectedDifficulty.ToLowerInvariant();
                    if (sd.Contains("easy") || sd.Contains("nhận") || sd == "1" || sd == "nb") diffCode = "NB";
                    else if (sd.Contains("hard") || sd.Contains("vận") || sd == "3" || sd == "vd") diffCode = "VD";
                }
                NavigationService.Instance.Selection.Difficulty = diffCode;
                NavigationService.Instance.Attempt.Source = AttemptSource.PROGRESS_PRACTICE;
                NavigationService.Instance.Attempt.AttemptId = Guid.NewGuid().ToString("N");
                NavigationService.Instance.Attempt.IsFinished = false;
                NavigationService.Instance.ReturnContext.OriginRoute = RouteId.MainMenu;

                if (isEssayMode)
                {
                    NavigationService.Instance.Selection.Mode = "ESSAY";
                    NavigationService.Instance.NavigateTo(RouteId.EssayAnswer);
                }
                else
                {
                    NavigationService.Instance.Selection.Mode = "MCQ";
                    NavigationService.Instance.NavigateTo(RouteId.PracticeQuiz);
                }
            }
            else
            {
                Load(PhysicsSceneNames.Quiz);
            }
        }

        private void JoinChallenge()
        {
            string weekId = CurrentWeekId(Save.selectedChallengeWeekOffset);
            if (!Save.joinedChallengeWeekIds.Contains(weekId)) Save.joinedChallengeWeekIds.Add(weekId);
            PhysicsGameSession.Instance.SaveProgress();

            // Màn 11.1 chạy bộ 10 câu hỏi của thử thách tuần (RouteId.WeeklyQuiz)
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.Attempt.Source = AttemptSource.WEEKLY_RANKED;
                NavigationService.Instance.NavigateTo(RouteId.WeeklyQuiz);
            }
            else if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.SelectMode(GameMode.Quiz);
                PhysicsGameSession.Instance.SelectLesson(PhysicsQuestionRepository.WHOLE_BOOK_REVIEW_ID, $"Thử thách tuần {weekId}");
                Load(PhysicsSceneNames.WeeklyQuiz);
            }
            else
            {
                Render();
            }
        }

        private void ChangeWeek(int offset)
        {
            Save.selectedChallengeWeekOffset += offset;
            PhysicsGameSession.Instance.SaveProgress();
            Render();
        }

        private void SaveSettings()
        {
            foreach (var dropdown in _filters)
            {
                if (dropdown.options.Count > 0 && (dropdown.options[dropdown.value].text.Contains("15") || dropdown.options[dropdown.value].text.Contains("30") || dropdown.options[dropdown.value].text.Contains("45")))
                {
                    int minutes = dropdown.options[dropdown.value].text.Contains("15") ? 15 : dropdown.options[dropdown.value].text.Contains("45") ? 45 : 30;
                    PhysicsGameSession.Instance.ConfigureStudyPlan(minutes, Save.studyGoal);
                }
            }
            PhysicsGameSession.Instance.ConfigureStudyPlan(Save.plannedStudyMinutes, Save.studyGoal);
            SetContaining("Dữ liệu đã lưu trên máy", "Cấu hình đã lưu trên máy");
        }

        private void CyclePlanMinutes()
        {
            int next = Save.plannedStudyMinutes == 15 ? 30 : Save.plannedStudyMinutes == 30 ? 45 : 15;
            PhysicsGameSession.Instance.ConfigureStudyPlan(next, Save.studyGoal);
            Render();
        }

        private void ToggleEndAtTime()
        {
            Save.endStudyWhenTimeReached = !Save.endStudyWhenTimeReached;
            PhysicsGameSession.Instance.SaveProgress();
            Render();
        }

        private void ShowNoSyncSupport()
        {
            SetContaining("Đồng bộ dữ liệu", "Google Sheets chưa được hỗ trợ · dữ liệu đã lưu trên máy");
        }

        private void FinishAndReturn()
        {
            PhysicsGameSession.Instance.FinishStudySession();
            Load(PhysicsSceneNames.MainMenu);
        }

        private void Load(string scene)
        {
            if (PhysicsGameSession.Instance != null)
                PhysicsGameSession.Instance.LoadScene(scene);
            else
                SceneManager.LoadScene(scene);
        }

        private List<TextMeshProUGUI> FindContaining(string value) => _texts.Where(t => t != null && _initialText.TryGetValue(t, out var original) && original.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        private void SetExactInitial(string original, string value)
        {
            var text = _initialText.FirstOrDefault(x => x.Value.Trim() == original).Key;
            if (text != null) text.text = value;
        }
        private void SetAllExactInitial(string original, string value)
        {
            foreach (var item in _initialText)
                if (item.Value.Trim() == original) item.Key.text = value;
        }
        private void SetContaining(string oldValue, string newValue)
        {
            var text = FindContaining(oldValue).FirstOrDefault();
            if (text != null) text.text = newValue;
        }

        private static string CurrentWeekId(int offset)
        {
            DateTime day = DateTime.Now.Date.AddDays(offset * 7);
            return $"{ISOWeek.GetYear(day)}-W{ISOWeek.GetWeekOfYear(day):D2}";
        }

        public static string GetLessonTitle(string lessonId)
        {
            if (string.IsNullOrEmpty(lessonId)) return "Bài học";
            if (lessonId == PhysicsQuestionRepository.WHOLE_BOOK_REVIEW_ID) return "Tổng ôn cả quyển";

            int num = ExtractLessonNumber(lessonId);

            // 1. Direct match with CurriculumLessonInfo
            for (int ch = 1; ch <= 5; ch++)
            {
                foreach (var l in PhysicsCurriculumData.GetLessons(ch))
                {
                    if (string.Equals(l.lessonId, lessonId, StringComparison.OrdinalIgnoreCase) ||
                        NormalizeLessonId(l.lessonId) == NormalizeLessonId(lessonId))
                        return l.DisplayName;
                }
            }

            // 2. Number-based match (e.g. "bai13" -> 13 -> matches "Bài 13" / "C3_L13")
            if (num > 0)
            {
                for (int ch = 1; ch <= 5; ch++)
                {
                    foreach (var l in PhysicsCurriculumData.GetLessons(ch))
                    {
                        if (ExtractLessonNumber(l.lessonNumber) == num || ExtractLessonNumber(l.lessonId) == num)
                            return l.DisplayName;
                    }
                }
            }

            return lessonId;
        }

        public static int GetChapterForLesson(string lessonId)
        {
            if (string.IsNullOrEmpty(lessonId)) return 0;

            // 1. Kiểm tra tiền tố trực tiếp C1_, C2_, C3_, C4_, C5_ hoặc C1_TEST, test_c1...
            var mCh = System.Text.RegularExpressions.Regex.Match(lessonId, @"^[Cc](\d+)");
            if (mCh.Success && int.TryParse(mCh.Groups[1].Value, out int chPrefix) && chPrefix >= 1 && chPrefix <= 5)
            {
                return chPrefix;
            }

            var mTestCh = System.Text.RegularExpressions.Regex.Match(lessonId, @"test_[Cc](\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (mTestCh.Success && int.TryParse(mTestCh.Groups[1].Value, out int chTest) && chTest >= 1 && chTest <= 5)
            {
                return chTest;
            }

            // 2. So khớp trực tiếp mã bài với PhysicsCurriculumData
            for (int ch = 1; ch <= 5; ch++)
            {
                foreach (var l in PhysicsCurriculumData.GetLessons(ch))
                {
                    if (string.Equals(l.lessonId, lessonId, StringComparison.OrdinalIgnoreCase) ||
                        NormalizeLessonId(l.lessonId) == NormalizeLessonId(lessonId))
                    {
                        return ch;
                    }
                }
            }

            // 3. Tìm theo số thứ tự bài học duy nhất (ExtractLessonNumber)
            int num = ExtractLessonNumber(lessonId);
            if (num > 0)
            {
                for (int ch = 1; ch <= 5; ch++)
                {
                    foreach (var l in PhysicsCurriculumData.GetLessons(ch))
                    {
                        if (ExtractLessonNumber(l.lessonNumber) == num || ExtractLessonNumber(l.lessonId) == num)
                        {
                            return ch;
                        }
                    }
                }
            }

            return 0;
        }

        public static bool AreLessonsMatching(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;
            if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase)) return true;
            if (NormalizeLessonId(a) == NormalizeLessonId(b)) return true;

            // Nếu cả hai đều xác định được chương, chương phải khớp tuyệt đối
            int cha = GetChapterForLesson(a);
            int chb = GetChapterForLesson(b);
            if (cha > 0 && chb > 0 && cha != chb) return false;

            int na = ExtractLessonNumber(a);
            int nb = ExtractLessonNumber(b);
            if (na > 0 && nb > 0 && na == nb) return true;
            return false;
        }

        public static int ExtractLessonNumber(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;

            // 1. Khớp mẫu [Ll](\d+) (ví dụ: C2_L5 -> 5, C1_L2 -> 2, L14 -> 14, C3_L13 -> 13)
            var mL = System.Text.RegularExpressions.Regex.Match(s, @"[Ll](\d+)");
            if (mL.Success && int.TryParse(mL.Groups[1].Value, out int nL)) return nL;

            // 2. Khớp mẫu bai/bài (\d+) (ví dụ: bai5 -> 5, Bài 13 -> 13, Bài 5 -> 5)
            var mBai = System.Text.RegularExpressions.Regex.Match(s, @"(?:bai|bài)\s*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (mBai.Success && int.TryParse(mBai.Groups[1].Value, out int nBai)) return nBai;

            // 3. Fallback: lấy cụm số cuối cùng trong chuỗi để tránh lấy nhầm số chương C1, C2...
            var allDigits = System.Text.RegularExpressions.Regex.Matches(s, @"\d+");
            if (allDigits.Count > 0 && int.TryParse(allDigits[allDigits.Count - 1].Value, out int nLast)) return nLast;

            return 0;
        }

        public static string NormalizeLessonId(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Trim().ToLowerInvariant().Replace(" ", "").Replace("_", "");
        }

        public static string GetDifficultyDisplayName(string diff)
        {
            if (string.IsNullOrEmpty(diff)) return "Thông hiểu";
            string s = diff.ToLowerInvariant();
            if (s.Contains("easy") || s.Contains("nhận") || s == "1" || s == "nb") return "Nhận biết";
            if (s.Contains("hard") || s.Contains("vận") || s == "3" || s == "vd") return "Vận dụng";
            return "Thông hiểu";
        }

        public static bool IsDifficultyMatching(string assessmentKey, string targetDiff)
        {
            if (string.IsNullOrEmpty(targetDiff)) return true;
            if (string.IsNullOrEmpty(assessmentKey)) return true;
            string ak = assessmentKey.ToLowerInvariant();
            string td = targetDiff.ToLowerInvariant();

            if (td.Contains("easy") || td.Contains("nhận") || td == "1" || td == "nb")
                return ak.Contains("easy") || ak.Contains("nhận") || ak.Contains("nhan_biet") || ak.Contains("nb") || ak.Contains("1");
            if (td.Contains("hard") || td.Contains("vận") || td == "3" || td == "vd")
                return ak.Contains("hard") || ak.Contains("vận") || ak.Contains("van_dung") || ak.Contains("vd") || ak.Contains("3");

            return ak.Contains("medium") || ak.Contains("thông") || ak.Contains("thong_hieu") || ak.Contains("th") || ak.Contains("2");
        }

        private List<PhysicsQuizHistoryEntry> FilterProgressHistory()
        {
            var history = (Save != null && Save.quizHistory != null)
                ? Save.quizHistory.Where(x => x != null && (x.isComplete || (x.totalCount > 0 && x.correctCount + x.wrongCount >= x.totalCount))).ToList()
                : new List<PhysicsQuizHistoryEntry>();

            var filtered = new List<PhysicsQuizHistoryEntry>();
            foreach (var entry in history)
            {
                // 1. Chapter check (ưu tiên chuẩn hóa theo mã bài trước để khắc phục các bản ghi cũ bị lưu sai chapterId)
                int entryCh = GetChapterForLesson(entry.lessonId);
                if (entryCh <= 0) entryCh = entry.chapterId;
                if (_selectedChapter > 0 && entryCh > 0 && entryCh != _selectedChapter)
                    continue;

                // 2. Lesson check
                if (!string.IsNullOrEmpty(_selectedLessonId) && !AreLessonsMatching(entry.lessonId, _selectedLessonId))
                    continue;

                // 3. Activity / format check (Quiz / Trắc nghiệm vs Tự luận theo Selection.Mode)
                string targetMode = NavigationService.Instance?.Selection?.Mode ?? "";
                if (!string.IsNullOrEmpty(targetMode))
                {
                    string tm = targetMode.ToUpperInvariant();
                    string act = (entry.activityType ?? "").ToLowerInvariant();
                    bool isEntryEssay = act.Contains("tự luận") || act.Contains("written") || act.Contains("essay");
                    if ((tm == "ESSAY" || tm == "WRITTEN" || tm == "TỰ LUẬN") && !isEntryEssay)
                        continue;
                    if ((tm == "MCQ" || tm == "QUIZ" || tm == "TRẮC NGHIỆM") && isEntryEssay)
                        continue;
                }

                // 4. Difficulty check
                if (!string.IsNullOrEmpty(_selectedDifficulty) && !IsDifficultyMatching(entry.assessmentKey, _selectedDifficulty))
                    continue;

                filtered.Add(entry);
            }

            return filtered.OrderByDescending(x => ParseCompletedTime(x.completedAtUtc)).ToList();
        }

        private static DateTime ParseCompletedTime(string timeStr)
        {
            if (string.IsNullOrEmpty(timeStr)) return DateTime.MinValue;
            if (DateTime.TryParse(timeStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dt))
                return dt;
            if (DateTime.TryParse(timeStr, out dt))
                return dt;
            return DateTime.MinValue;
        }

        private void SetupProgressScreen(Scene activeScene)
        {
            if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.ReloadSaveData();
            }

            var canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Reset references
            _progressChapterText = null;
            _progressLessonText = null;
            _progressDifficultyText = null;
            _progressPrevScoreText = null;
            _progressCurrentScoreText = null;
            _progressBestScoreText = null;
            _progressBar1PctText = null;
            _progressBar2PctText = null;
            _progressTrendLine1 = null;
            _progressTrendLine2 = null;
            _progressBar1Fill = null;
            _progressBar2Fill = null;
            _progressTrendArrow = null;

            // 1. Locate TextMeshProUGUI elements by anchoredPosition
            foreach (var tmp in _texts)
            {
                if (tmp == null || tmp.gameObject.scene != activeScene) continue;
                Vector2 pos = tmp.rectTransform.anchoredPosition;
                float x = pos.x;
                float y = pos.y;

                if (y > 300f && y < 400f)
                {
                    if (x < -200f) _progressChapterText = tmp;
                    else if (x < 400f) _progressLessonText = tmp;
                    else _progressDifficultyText = tmp;
                }
                else if (y > -100f && y < 0f)
                {
                    if (x < -300f) _progressPrevScoreText = tmp;
                    else if (x < 300f) _progressCurrentScoreText = tmp;
                    else _progressBestScoreText = tmp;
                }
                else if (x > 150f && x < 350f && y > -180f && y < -100f)
                {
                    _progressBar1PctText = tmp;
                }
                else if (x > 150f && x < 350f && y < -180f && y > -280f)
                {
                    _progressBar2PctText = tmp;
                }
                else if (x > 380f && y > -210f && y < -150f)
                {
                    _progressTrendLine1 = tmp;
                }
                else if (x > 380f && y <= -210f && y > -270f)
                {
                    _progressTrendLine2 = tmp;
                }
            }

            // 2. Locate Images (Fills and Arrow)
            // 2a. Deactivate Image (10) duplicate board so its children cannot interfere or render ghosts
            foreach (var rt in canvas.GetComponentsInChildren<RectTransform>(true))
            {
                if (rt != null && rt.name == "Image (10)")
                {
                    rt.gameObject.SetActive(false);
                }
            }

            // 2b. Specifically locate fills and trend arrow from Image (14) (the main active fantasy board)
            Transform mainBoard = canvas.transform.Find("Image (14)");
            if (mainBoard != null)
            {
                foreach (Transform child in mainBoard)
                {
                    var img = child.GetComponent<Image>();
                    if (img == null) continue;
                    string cName = child.name;
                    Vector2 pos = img.rectTransform.anchoredPosition;

                    if (cName == "Image (16)" || (pos.x < -200f && Mathf.Abs(pos.y - (-106.47f)) < 30f))
                    {
                        _progressBar1Fill = img;
                    }
                    else if (cName == "Image (18)" || (pos.x < -100f && Mathf.Abs(pos.y - (-204.15f)) < 30f))
                    {
                        _progressBar2Fill = img;
                    }
                    else if (cName == "Image (19)" || (Mathf.Abs(pos.x - 366f) < 30f && Mathf.Abs(pos.y - (-162.55f)) < 30f))
                    {
                        _progressTrendArrow = img;
                    }
                }
            }

            // Fallback if not found directly
            if (_progressTrendArrow == null || _progressBar1Fill == null || _progressBar2Fill == null)
            {
                foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                {
                    if (img == null || img.gameObject.scene != activeScene) continue;
                    string goName = img.gameObject.name;
                    bool isUnderMainBoard = img.transform.parent != null && img.transform.parent.name == "Image (14)";

                    if (_progressBar1Fill == null && goName == "Image (16)" && isUnderMainBoard)
                    {
                        _progressBar1Fill = img;
                    }
                    else if (_progressBar2Fill == null && goName == "Image (18)" && isUnderMainBoard)
                    {
                        _progressBar2Fill = img;
                    }
                    else if (_progressTrendArrow == null && goName == "Image (19)" && isUnderMainBoard)
                    {
                        _progressTrendArrow = img;
                    }
                }
            }

            // 3. Setup Buttons for Selectors (bind label + background frame)
            BindProgressSelectorButton(_progressChapterText, new Vector2(-556.65f, 341f), OnProgressCycleChapter);
            BindProgressSelectorButton(_progressLessonText, new Vector2(20f, 336.27f), OnProgressCycleLesson);
            BindProgressSelectorButton(_progressDifficultyText, new Vector2(579f, 336.27f), OnProgressCycleDifficulty);

            // 4. Setup "Luyện tập ngay" button
            BindProgressPracticeButton(canvas);

            // 5. Setup "Quay lại" button
            BindProgressBackButton(canvas);

            // 6. Initialize selections from session / save / defaults
            InitProgressSelections();
        }

        private void BindProgressSelectorButton(TextMeshProUGUI label, Vector2 approximateFramePos, UnityAction action)
        {
            if (label != null)
            {
                label.raycastTarget = true;
                var btnOnLabel = label.GetComponent<Button>() ?? label.gameObject.AddComponent<Button>();
                btnOnLabel.targetGraphic = label;
                btnOnLabel.onClick.RemoveAllListeners();
                btnOnLabel.onClick.AddListener(action);
                _boundButtons.Add(btnOnLabel);
            }

            var canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            foreach (var rt in canvas.GetComponentsInChildren<RectTransform>(true))
            {
                if (rt.gameObject == canvas.gameObject) continue;
                if (Vector2.Distance(rt.anchoredPosition, approximateFramePos) < 50f)
                {
                    var img = rt.GetComponent<Image>();
                    if (img != null)
                    {
                        img.raycastTarget = true;
                        var btn = rt.GetComponent<Button>() ?? rt.gameObject.AddComponent<Button>();
                        btn.targetGraphic = img;
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(action);
                        _boundButtons.Add(btn);
                        break;
                    }
                }
            }
        }

        private void BindProgressPracticeButton(Canvas canvas)
        {
            GameObject practiceGo = null;
            foreach (var rt in canvas.GetComponentsInChildren<RectTransform>(true))
            {
                if (rt.gameObject == canvas.gameObject) continue;
                Vector2 pos = rt.anchoredPosition;
                if (Mathf.Abs(pos.y - (-435f)) < 60f && Mathf.Abs(pos.x) < 200f)
                {
                    var btn = rt.GetComponent<Button>();
                    var img = rt.GetComponent<Image>();
                    if (btn != null || img != null || rt.name.Contains("17"))
                    {
                        practiceGo = rt.gameObject;
                        break;
                    }
                }
            }
            if (practiceGo != null)
            {
                var img = practiceGo.GetComponent<Image>();
                if (img != null) img.raycastTarget = true;
                var btn = practiceGo.GetComponent<Button>() ?? practiceGo.AddComponent<Button>();
                btn.targetGraphic = img;
                btn.interactable = true;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(PracticeNowFromProgress);
                _boundButtons.Add(btn);
            }

            var practiceTmp = _texts.FirstOrDefault(t => t != null && t.rectTransform.anchoredPosition.y < -350f && t.text.ToLowerInvariant().Contains("luyện"));
            if (practiceTmp != null)
            {
                practiceTmp.raycastTarget = true;
                var btn = practiceTmp.GetComponent<Button>() ?? practiceTmp.gameObject.AddComponent<Button>();
                btn.targetGraphic = practiceTmp;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(PracticeNowFromProgress);
                _boundButtons.Add(btn);
            }
        }

        private void BindProgressBackButton(Canvas canvas)
        {
            foreach (var rt in canvas.GetComponentsInChildren<RectTransform>(true))
            {
                if (rt.gameObject == canvas.gameObject) continue;
                Vector2 pos = rt.anchoredPosition;
                if (pos.x < -700f && pos.y > 400f)
                {
                    var img = rt.GetComponent<Image>();
                    if (img != null) img.raycastTarget = true;
                    var btn = rt.GetComponent<Button>() ?? rt.gameObject.AddComponent<Button>();
                    btn.targetGraphic = img;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(ReturnToPreviousOrTodayStudy);
                    _boundButtons.Add(btn);
                }
            }

            var backTmp = _texts.FirstOrDefault(t => t != null && t.rectTransform.anchoredPosition.x < -600f && t.rectTransform.anchoredPosition.y > 350f && t.text.ToLowerInvariant().Contains("quay"));
            if (backTmp != null)
            {
                backTmp.raycastTarget = true;
                var btn = backTmp.GetComponent<Button>() ?? backTmp.gameObject.AddComponent<Button>();
                btn.targetGraphic = backTmp;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ReturnToPreviousOrTodayStudy);
                _boundButtons.Add(btn);
            }
        }

        private void OnProgressCycleChapter()
        {
            _selectedChapter = (_selectedChapter % 5) + 1;
            var lessons = PhysicsCurriculumData.GetLessons(_selectedChapter);
            _selectedLessonId = lessons.Count > 0 ? lessons[0].lessonId : "";
            SaveProgressFilterState();
            RenderProgress();
        }

        private void OnProgressCycleLesson()
        {
            var lessons = PhysicsCurriculumData.GetLessons(_selectedChapter);
            if (lessons.Count == 0) return;
            int curIdx = lessons.FindIndex(l => AreLessonsMatching(l.lessonId, _selectedLessonId));
            int nextIdx = (curIdx + 1) % lessons.Count;
            _selectedLessonId = lessons[nextIdx].lessonId;
            SaveProgressFilterState();
            RenderProgress();
        }

        private void OnProgressCycleDifficulty()
        {
            string d = _selectedDifficulty != null ? _selectedDifficulty.ToLowerInvariant() : "";
            if (d.Contains("easy") || d.Contains("nhận") || d == "1" || d == "nb")
                _selectedDifficulty = "Medium";
            else if (d.Contains("medium") || d.Contains("thông") || d == "2" || d == "th")
                _selectedDifficulty = "Hard";
            else
                _selectedDifficulty = "Easy";

            SaveProgressFilterState();
            RenderProgress();
        }

        private void SaveProgressFilterState()
        {
            if (Save != null)
            {
                Save.progressFilterChapterId = _selectedChapter;
                Save.progressFilterLessonId = _selectedLessonId;
                Save.progressFilterDifficulty = _selectedDifficulty;
                Save.lastChapterId = _selectedChapter;
                Save.lastLessonId = _selectedLessonId;
                PhysicsSaveManager.Save(Save);
            }
            if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.SelectChapter(_selectedChapter);
                PhysicsGameSession.Instance.SelectLesson(_selectedLessonId, GetLessonTitle(_selectedLessonId));
            }
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.Selection.ChapterId = _selectedChapter;
                NavigationService.Instance.Selection.LessonId = _selectedLessonId;
                NavigationService.Instance.Selection.LessonTitle = GetLessonTitle(_selectedLessonId);
                NavigationService.Instance.Selection.Difficulty = _selectedDifficulty;
            }
        }

        private void InitProgressSelections()
        {
            // Nạp dữ liệu mới nhất từ file JSON để đảm bảo tính bền vững
            if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.ReloadSaveData();
            }

            string chosenLesson = "";
            int chosenChapter = 0;
            string chosenDiff = "";

            // 0. Ưu tiên cao nhất: Lựa chọn từ NavigationService.Instance.Selection nếu có (được truyền trực tiếp từ màn Tổng quan)
            if (NavigationService.Instance != null && NavigationService.Instance.Selection != null &&
                !string.IsNullOrEmpty(NavigationService.Instance.Selection.LessonId))
            {
                chosenLesson = NavigationService.Instance.Selection.LessonId;
                chosenChapter = NavigationService.Instance.Selection.ChapterId > 0 ? NavigationService.Instance.Selection.ChapterId : GetChapterForLesson(chosenLesson);
                chosenDiff = !string.IsNullOrEmpty(NavigationService.Instance.Selection.Difficulty) ? NavigationService.Instance.Selection.Difficulty : "NB";
            }
            // 1. Ưu tiên 1: Lựa chọn đã lưu của màn Tiến độ
            else if (Save != null && !string.IsNullOrEmpty(Save.progressFilterLessonId))
            {
                chosenLesson = Save.progressFilterLessonId;
                chosenChapter = Save.progressFilterChapterId;
                chosenDiff = Save.progressFilterDifficulty;
            }
            // 2. Ưu tiên 2: Bài học hoàn thành gần nhất đã lưu trong SaveData
            else if (Save != null && !string.IsNullOrEmpty(Save.lastLessonId))
            {
                chosenLesson = Save.lastLessonId;
                chosenChapter = Save.lastChapterId;
                chosenDiff = Save.lastDifficulty;
            }

            // 3. Ưu tiên 3: Lượt thi hoàn thành gần nhất trong quizHistory
            if (string.IsNullOrEmpty(chosenLesson) && Save != null && Save.quizHistory != null)
            {
                var latest = Save.quizHistory.LastOrDefault(x => x != null && (x.isComplete || (x.totalCount > 0 && x.correctCount + x.wrongCount >= x.totalCount)));
                if (latest != null && !string.IsNullOrEmpty(latest.lessonId))
                {
                    chosenLesson = latest.lessonId;
                    chosenChapter = latest.chapterId > 0 ? latest.chapterId : GetChapterForLesson(latest.lessonId);
                    chosenDiff = !string.IsNullOrEmpty(latest.assessmentKey) ? latest.assessmentKey : "NB";
                }
            }

            // 4. Ưu tiên 4: Chưa có bất kỳ lịch sử nào -> chọn bài đầu tiên của chương trình (C1_L2)
            if (string.IsNullOrEmpty(chosenLesson))
            {
                chosenChapter = 1;
                chosenLesson = "C1_L2";
                chosenDiff = "NB";
            }

            _selectedLessonId = chosenLesson;

            // Xác định chuẩn xác Chương theo mã bài học đã chọn
            int lCh = GetChapterForLesson(_selectedLessonId);
            if (lCh > 0)
            {
                _selectedChapter = lCh;
            }
            else if (chosenChapter > 0)
            {
                _selectedChapter = chosenChapter;
            }
            else
            {
                _selectedChapter = 1;
            }

            // Đảm bảo bài học thuộc chương đang chọn
            var chLessons = PhysicsCurriculumData.GetLessons(_selectedChapter);
            if (chLessons.Count > 0 && !chLessons.Any(l => AreLessonsMatching(l.lessonId, _selectedLessonId)))
            {
                _selectedLessonId = chLessons[0].lessonId;
            }

            _selectedDifficulty = !string.IsNullOrEmpty(chosenDiff) ? chosenDiff : "Easy";

            // Đồng bộ sang Session & NavigationService
            if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.SelectChapter(_selectedChapter);
                PhysicsGameSession.Instance.SelectLesson(_selectedLessonId, GetLessonTitle(_selectedLessonId));
            }
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.Selection.ChapterId = _selectedChapter;
                NavigationService.Instance.Selection.LessonId = _selectedLessonId;
                NavigationService.Instance.Selection.LessonTitle = GetLessonTitle(_selectedLessonId);
                NavigationService.Instance.Selection.Difficulty = _selectedDifficulty;
            }
        }

        private static string GetChapterTitleForLesson(string lessonId)
        {
            if (string.IsNullOrEmpty(lessonId)) return "";
            for (int ch = 1; ch <= 5; ch++)
            {
                var lessons = PhysicsCurriculumData.GetLessons(ch);
                if (lessons.Any(l => l.lessonId == lessonId || AreLessonsMatching(l.lessonId, lessonId)))
                {
                    var chData = PhysicsCurriculumData.GetChapter(ch);
                    return $"Chương {chData.romanNumber}: {chData.chapterName}";
                }
            }
            return "";
        }

        private void SetupTodayScreen(Scene activeScene)
        {
            var canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Xóa triệt để các shortcut thừa nếu có (Bảng xếp hạng, Tiến bộ, Quản lý)
            var oldShortcuts = canvas.transform.Find("StudyScreenNavShortcuts");
            if (oldShortcuts != null) Destroy(oldShortcuts.gameObject);

            // Mặc định tắt raycastTarget trên các object nền, khung, tiêu đề
            RectTransform[] allRts = canvas.GetComponentsInChildren<RectTransform>(true);
            foreach (var rt in allRts)
            {
                if (rt.gameObject == canvas.gameObject) continue;
                var img = rt.GetComponent<Image>();
                if (img != null)
                {
                    img.raycastTarget = false;
                }
            }

            // Tìm 5 nút bấm chính trên màn 22 theo tên hoặc tọa độ
            GameObject goBack = null;
            GameObject go15 = null;
            GameObject go30 = null;
            GameObject go45 = null;
            GameObject goStart = null;

            foreach (var rt in allRts)
            {
                if (rt.gameObject == canvas.gameObject) continue;
                Vector2 pos = rt.anchoredPosition;

                // Nút Quay lại (pos x ~ -807, y ~ 453 hoặc tên chứa Quay)
                if (goBack == null && (rt.name.Contains("Quay") || (pos.x < -700f && pos.y > 350f)))
                {
                    goBack = rt.gameObject;
                }
                // Nút 15 phút (x ~ -610, y ~ 0)
                else if (Mathf.Abs(pos.x - (-610f)) < 80f && Mathf.Abs(pos.y) < 80f)
                {
                    go15 = rt.gameObject;
                }
                // Nút 30 phút (x ~ -229, y ~ 0)
                else if (Mathf.Abs(pos.x - (-229f)) < 80f && Mathf.Abs(pos.y) < 80f)
                {
                    go30 = rt.gameObject;
                }
                // Nút 45 phút (x ~ -365, y ~ -158)
                else if (Mathf.Abs(pos.x - (-365f)) < 80f && Mathf.Abs(pos.y - (-158f)) < 80f)
                {
                    go45 = rt.gameObject;
                }
                // Nút Bắt đầu học (x ~ 0, y ~ -413)
                else if (Mathf.Abs(pos.x) < 120f && Mathf.Abs(pos.y - (-413f)) < 80f)
                {
                    goStart = rt.gameObject;
                }
            }

            // Gắn Button và sự kiện cho nút Quay lại
            if (goBack != null)
            {
                var img = goBack.GetComponent<Image>();
                if (img != null) img.raycastTarget = true;
                var btn = goBack.GetComponent<Button>() ?? goBack.AddComponent<Button>();
                btn.targetGraphic = img;
                btn.interactable = true;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    Debug.Log("[StudyScreensController] Bấm Quay lại từ màn 22 -> MainMenu.");
                    Load(PhysicsSceneNames.MainMenu);
                });
                _boundButtons.Add(btn);
            }

            // Gắn Button cho các lựa chọn thời gian
            BindTodayDurationButton(go15, 15);
            BindTodayDurationButton(go30, 30);
            BindTodayDurationButton(go45, 45);

            // Gắn Button cho nút Bắt đầu học
            if (goStart != null)
            {
                var img = goStart.GetComponent<Image>();
                if (img != null) img.raycastTarget = true;
                var btn = goStart.GetComponent<Button>() ?? goStart.AddComponent<Button>();
                btn.targetGraphic = img;
                btn.interactable = true;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnTodayStartStudyClicked);
                _boundButtons.Add(btn);
            }

            // Mặc định thời gian là 30 phút theo đặc tả 6.3 nếu chưa chọn
            int currentMins = (Save != null && Save.plannedStudyMinutes > 0) ? Save.plannedStudyMinutes : 30;
            _selectedDurationThisVisit = currentMins;
            UpdateTodayDurationVisuals(currentMins);
        }

        private void BindTodayDurationButton(GameObject go, int minutes)
        {
            if (go == null) return;
            var img = go.GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = true;
                RegisterChoiceIndicatorColor(img);
                _durationIndicators[minutes] = img;
            }
            var btn = go.GetComponent<Button>() ?? go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.interactable = true;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                Debug.Log($"[StudyScreensController] Chọn thời lượng học: {minutes} phút.");
                _selectedDurationThisVisit = minutes;
                if (Save != null)
                {
                    Save.plannedStudyMinutes = minutes;
                    PhysicsSaveManager.Save(Save);
                }
                if (PhysicsGameSession.Instance != null)
                {
                    PhysicsGameSession.Instance.ConfigureStudyPlan(minutes, Save != null ? Save.studyGoal : null);
                }
                UpdateTodayDurationVisuals(minutes);
            });
            _boundButtons.Add(btn);
        }

        private void UpdateTodayDurationVisuals(int selectedMinutes)
        {
            foreach (var kv in _durationIndicators)
            {
                if (kv.Value == null) continue;
                bool isSelected = (kv.Key == selectedMinutes);
                kv.Value.color = isSelected ? Color.white : new Color(0.72f, 0.72f, 0.72f, 0.95f);

                var outline = kv.Value.GetComponent<Outline>();
                if (isSelected)
                {
                    if (outline == null) outline = kv.Value.gameObject.AddComponent<Outline>();
                    outline.enabled = true;
                    outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f); // Aurelia Gold
                    outline.effectDistance = new Vector2(4f, -4f);
                }
                else if (outline != null)
                {
                    outline.enabled = false;
                }
            }
        }

        private void OnTodayStartStudyClicked()
        {
            Debug.Log("[StudyScreensController] Bắt đầu học -> Chuyển sang 04_ChapterSelection.");
            int mins = _selectedDurationThisVisit > 0 ? _selectedDurationThisVisit : (Save != null && Save.plannedStudyMinutes > 0 ? Save.plannedStudyMinutes : 30);
            if (Save != null)
            {
                Save.plannedStudyMinutes = mins;
                PhysicsSaveManager.Save(Save);
            }

            if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.ConfigureStudyPlan(mins, Save != null ? Save.studyGoal : null);
                PhysicsGameSession.Instance.StartOrResumeStudySession();
                if (PhysicsGameSession.Instance.SaveData != null && PhysicsGameSession.Instance.SaveData.activeStudySession != null)
                {
                    PhysicsGameSession.Instance.SaveData.activeStudySession.plannedMinutes = mins;
                    PhysicsGameSession.Instance.SaveProgress();
                }
            }

            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.NavigateTo(RouteId.ChapterSelection);
            }
            else
            {
                Load(PhysicsSceneNames.ChapterSelection);
            }
        }

        private static string FormatDuration(float seconds) => $"{Mathf.FloorToInt(seconds / 60f)} phút {Mathf.FloorToInt(seconds % 60f):D2} giây";
        private static string Trend(int delta) => delta > 0 ? $"Tăng {delta} điểm phần trăm" : delta < 0 ? $"Giảm {Mathf.Abs(delta)} điểm phần trăm" : "Không đổi";
    }
}
