using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Mini-game Chương V: Năng lượng với cuộc sống – TRUNG TÂM NĂNG LƯỢNG AURELIA (AURELIA ENERGY NEXUS).
    /// Hỗ trợ 2 Gameplay Mode tương ứng với 2 bài học SGK Lớp 9:
    /// - MODE A (Earth Energy & Fossil Lab): C5_L16 (Vòng năng lượng trên Trái Đất. Năng lượng hóa thạch).
    /// - MODE B (Renewable Grid Lab): C5_L17 (Một số dạng năng lượng tái tạo).
    /// </summary>
    public class EnergyNexusGame : MonoBehaviour
    {
        public enum NexusMode
        {
            EarthEnergyFossil, // C5_L16
            RenewableGrid      // C5_L17
        }

        public enum FossilFuelLevel
        {
            Low,    // Output = 35, Fuel = 25, Impact = 20% (FAIL: Thiếu điện)
            Medium, // Output = 70, Fuel = 50, Impact = 50% (PASS: 70-80)
            High    // Output = 100, Fuel = 80, Impact = 85% (FAIL: Quá dư thừa)
        }

        public enum RenewableSourceType
        {
            Solar,
            Wind,
            Hydro
        }

        // === PARAMETERS & LESSON STATE ===
        private string _lessonId = "C5_L16";
        private string _lessonTitle = "Vòng năng lượng trên Trái Đất. Năng lượng hóa thạch";
        private NexusMode _currentMode = NexusMode.EarthEnergyFossil;
        private int _currentChallengeIndex = 0; // 0, 1, 2 tương ứng 3 sub-challenges

        // Sub-challenge completion gating
        private readonly HashSet<int> _completedSubChallengesA = new HashSet<int>();
        private readonly HashSet<int> _completedSubChallengesB = new HashSet<int>();

        private bool _isCompleted = false;
        private bool _storyGateActive = false;
        private bool _enteredViaNormalFlow = false;
        public bool IsStoryGateActive => _storyGateActive || StoryFlowController.IsStoryModalOpen;

        // === MODE A (C5_L16) STATE ===
        // Sub 1: Energy Origin (Target: 0:Sun, 1:Biomass, 2:Organic Matter, 3:Fossil Fuel)
        private readonly string[] ORIGIN_NAMES = { "Năng lượng Mặt Trời", "Thực vật / Sinh khối", "Hợp chất hữu cơ cổ đại", "Nhiên liệu hóa thạch" };
        private readonly int[] _originSlots = { -1, -1, -1, -1 }; // -1 nghĩa là slot trống
        private int _selectedOriginCardIdx = -1;

        // Sub 2: Energy Transformation Chain (Target: 0:Hóa năng, 1:Nhiệt năng, 2:Cơ năng, 3:Điện năng)
        private readonly string[] TRANSFORMATION_NAMES = { "Hóa năng (Nhiên liệu)", "Nhiệt năng (Buồng đốt)", "Cơ năng (Turbine hơi)", "Điện năng (Máy phát)" };
        private readonly int[] _transSlots = { -1, -1, -1, -1 };
        private int _selectedTransCardIdx = -1;

        // Sub 3: Fossil Power Management
        private FossilFuelLevel _fossilFuelLevel = FossilFuelLevel.Low; // Default Low -> Output 35 < 70 -> FAIL

        // === MODE B (C5_L17) STATE ===
        // Sub 1: Renewable Source Lab (All 3 connected)
        private bool _isSolarConnected = false;
        private bool _isWindConnected = false;
        private bool _isHydroConnected = false;

        // Sub 2: Weather Matching
        private int _weatherScenarioIdx = 0; // 0: Sun 90%, 1: Wind 90%, 2: Water 90%
        private readonly HashSet<int> _completedWeatherScenarios = new HashSet<int>();
        private RenewableSourceType _selectedMatchingSource = RenewableSourceType.Solar;
        private bool _isMatchingSourceActive = false; // Default false -> Output 0 -> FAIL

        // Public getters for tests & telemetry
        public bool IsSolarConnected => _isSolarConnected;
        public bool IsWindConnected => _isWindConnected;
        public bool IsHydroConnected => _isHydroConnected;
        public int WeatherScenarioIndex => _weatherScenarioIdx;
        public RenewableSourceType SelectedMatchingSource => _selectedMatchingSource;
        public bool IsMatchingSourceActive => _isMatchingSourceActive;
        public IReadOnlyCollection<int> CompletedWeatherScenarios => _completedWeatherScenarios;
        public IReadOnlyCollection<int> CompletedSubChallengesB => _completedSubChallengesB;
        public IReadOnlyCollection<int> CompletedSubChallengesA => _completedSubChallengesA;
        public int CurrentChallengeIndex => _currentChallengeIndex;
        public NexusMode CurrentMode => _currentMode;
        public bool IsLessonCompleted => _isCompleted;

        // Sub 3: Microgrid Balance
        // Weather: Sun = 0.8, Wind = 0.5, Water = 0.7
        // Unit outputs: Solar = 24.0, Wind = 17.5, Hydro = 28.0
        private int _solarModuleCount = 0;
        private int _windModuleCount = 0;
        private int _hydroModuleCount = 0;
        private const int MAX_MODULE_BUDGET = 6;
        private const float DEMAND_MICROGRID = 100f;

        // Visual animation timers
        private float _visualAnimTimer = 0f;

        // === UI REFERENCES ===
        private Canvas _canvas;
        private GameObject _hudRoot;
        private GameObject _tutorialModal;
        private GameObject _successModal;

        // Header & Mission
        private TextMeshProUGUI _txtLessonBadge;
        private TextMeshProUGUI _txtMissionTitle;
        private TextMeshProUGUI _txtMissionDesc;
        private TextMeshProUGUI _txtChallengeCounter;

        // Readouts & Feedback
        private TextMeshProUGUI _txtReadoutMain;
        private TextMeshProUGUI _txtReadoutSecondary;
        private TextMeshProUGUI _txtReadoutFormula;
        private TextMeshProUGUI _txtFeedback;
        private Coroutine _feedbackCoroutine;

        // Workspace Panels
        private GameObject _panelModeAWorkspace;
        private GameObject _panelModeBWorkspace;
        private GameObject _panelModeAControls;
        private GameObject _panelModeBControls;

        // Mode A UI elements
        private GameObject _modeASub1Root;
        private GameObject _modeASub2Root;
        private GameObject _modeASub3Root;
        private readonly TextMeshProUGUI[] _txtOriginSlots = new TextMeshProUGUI[4];
        private readonly Button[] _btnOriginCards = new Button[4];
        private readonly TextMeshProUGUI[] _txtTransSlots = new TextMeshProUGUI[4];
        private readonly Button[] _btnTransCards = new Button[4];
        private TextMeshProUGUI _txtFossilOutputVal;
        private TextMeshProUGUI _txtFossilImpactVal;
        private Image _imgFossilImpactBar;
        private readonly List<Button> _btnFossilLevelList = new List<Button>();

        // Mode B UI elements
        private GameObject _modeBSub1Root;
        private GameObject _modeBSub2Root;
        private GameObject _modeBSub3Root;
        private Image _imgSolarConnectStatus;
        private Image _imgWindConnectStatus;
        private Image _imgHydroConnectStatus;
        private TextMeshProUGUI _txtWeatherCondition;
        private TextMeshProUGUI _txtWeatherOutputVal;
        private readonly List<Button> _btnWeatherScenarioList = new List<Button>();
        private readonly List<Button> _btnWeatherSourceList = new List<Button>();
        private TextMeshProUGUI _txtMicrogridCounters;
        private TextMeshProUGUI _txtMicrogridSupplyDemand;
        private Image _imgPowerBalanceBar;
        private Image _energyCoreVisualImg;

        // Theme Colors
        private static readonly Color ColorNavyBg = new Color(0.06f, 0.11f, 0.22f, 0.94f);
        private static readonly Color ColorGold = new Color(0.96f, 0.82f, 0.38f, 1f);
        private static readonly Color ColorGoldBorder = new Color(0.85f, 0.70f, 0.28f, 0.85f);
        private static readonly Color ColorEnergyCyan = new Color(0.00f, 0.88f, 1.00f, 1f);
        private static readonly Color ColorRenewableGreen = new Color(0.25f, 0.90f, 0.45f, 1f);
        private static readonly Color ColorFossilAmber = new Color(1.00f, 0.65f, 0.20f, 1f);
        private static readonly Color ColorAlertRed = new Color(1.0f, 0.40f, 0.38f, 1f);
        private static readonly Color ColorBtnNormal = new Color(0.12f, 0.20f, 0.38f, 0.95f);
        private static readonly Color ColorBtnSelected = new Color(0.85f, 0.68f, 0.22f, 0.95f);

        private void Awake()
        {
            _enteredViaNormalFlow = StoryFlowController.IsNormalGameplayEntry();
            EnsureEventSystem();
            EnsureSessionFallback();

            _canvas = FindAnyObjectByType<Canvas>();
            if (_canvas != null)
            {
                BuildGameplayUIIfMissing();
            }
        }

        private void Start()
        {
            InitializeLessonMode();
            ResetCurrentChallenge(false);

            var save = PhysicsSaveManager.Load();
            if (_enteredViaNormalFlow && save != null && !save.hasSeenCore5Intro)
            {
                if (_tutorialModal != null) _tutorialModal.SetActive(false);
                SetStoryGate(true);
                StoryFlowController.TryShowGameplayCoreIntro(
                    5,
                    customSave: null,
                    onIntroFinishedOrSkipped: () =>
                    {
                        SetStoryGate(false);
                        var updatedSave = PhysicsSaveManager.Load();
                        bool shouldShowTutorial = updatedSave == null || !updatedSave.hasSeenEnergyTutorial;
                        if (_tutorialModal != null)
                        {
                            _tutorialModal.SetActive(shouldShowTutorial);
                        }
                    },
                    canvasTransform: _canvas != null ? _canvas.transform : null,
                    forceNormalFlow: true);
            }
            else
            {
                bool shouldShowTutorial = save == null || !save.hasSeenEnergyTutorial;
                if (_tutorialModal != null)
                {
                    _tutorialModal.SetActive(shouldShowTutorial);
                }
            }
        }

        public void SetStoryGate(bool active)
        {
            _storyGateActive = active;
        }

        private void Update()
        {
            if (IsStoryGateActive) return;

            _visualAnimTimer += Time.unscaledDeltaTime;

            // Cập nhật hoạt ảnh nhấp nháy / xoay nhẹ của Energy Core
            if (_energyCoreVisualImg != null)
            {
                float pulse = 0.85f + Mathf.PingPong(_visualAnimTimer * 0.8f, 0.15f);
                _energyCoreVisualImg.transform.localScale = Vector3.one * pulse;
            }
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }

        private void EnsureSessionFallback()
        {
            if (PhysicsGameSession.Instance != null)
            {
                string id = PhysicsGameSession.Instance.SelectedLessonId;
                if (string.IsNullOrEmpty(id) || !id.StartsWith("C5_"))
                {
                    PhysicsGameSession.Instance.SelectChapter(5);
                    PhysicsGameSession.Instance.SelectLesson("C5_L16", "Vòng năng lượng trên Trái Đất. Năng lượng hóa thạch");
                }
            }
        }

        // =========================================================================
        // KHỞI TẠO BÀI HỌC & GAMEPLAY MODE
        // =========================================================================
        private void InitializeLessonMode()
        {
            if (PhysicsGameSession.Instance != null && !string.IsNullOrEmpty(PhysicsGameSession.Instance.SelectedLessonId))
            {
                _lessonId = PhysicsGameSession.Instance.SelectedLessonId;
                _lessonTitle = PhysicsGameSession.Instance.SelectedLessonTitle;
            }
            else
            {
                _lessonId = "C5_L16";
                _lessonTitle = "Vòng năng lượng trên Trái Đất. Năng lượng hóa thạch";
            }

            switch (_lessonId)
            {
                case "C5_L17":
                    _currentMode = NexusMode.RenewableGrid;
                    break;
                case "C5_L16":
                default:
                    _currentMode = NexusMode.EarthEnergyFossil;
                    break;
            }

            _currentChallengeIndex = 0;
            _completedSubChallengesA.Clear();
            _completedSubChallengesB.Clear();
            _completedWeatherScenarios.Clear();

            ConfigureModePanels();
        }

        private void ConfigureModePanels()
        {
            if (_panelModeAControls != null) _panelModeAControls.SetActive(_currentMode == NexusMode.EarthEnergyFossil);
            if (_panelModeBControls != null) _panelModeBControls.SetActive(_currentMode == NexusMode.RenewableGrid);
            if (_panelModeAWorkspace != null) _panelModeAWorkspace.SetActive(_currentMode == NexusMode.EarthEnergyFossil);
            if (_panelModeBWorkspace != null) _panelModeBWorkspace.SetActive(_currentMode == NexusMode.RenewableGrid);

            if (_txtLessonBadge != null)
            {
                string modeTag = _currentMode == NexusMode.EarthEnergyFossil ? "VÒNG NĂNG LƯỢNG TRÁI ĐẤT" : "LƯỚI NĂNG LƯỢNG TÁI TẠO";
                _txtLessonBadge.text = $"<b>{_lessonId}</b> · {_lessonTitle}  [<color=#00E5FF>{modeTag}</color>]";
            }
        }

        // =========================================================================
        // RESET & CHALLENGE NAVIGATION
        // =========================================================================
        public void ResetCurrentChallenge(bool showFeedback = true)
        {
            if (showFeedback && IsStoryGateActive) return;
            _isCompleted = false;

            if (_currentMode == NexusMode.EarthEnergyFossil)
            {
                if (_currentChallengeIndex == 0) // Sub 1
                {
                    for (int i = 0; i < _originSlots.Length; i++) _originSlots[i] = -1;
                    _selectedOriginCardIdx = -1;
                }
                else if (_currentChallengeIndex == 1) // Sub 2
                {
                    for (int i = 0; i < _transSlots.Length; i++) _transSlots[i] = -1;
                    _selectedTransCardIdx = -1;
                }
                else // Sub 3
                {
                    _fossilFuelLevel = FossilFuelLevel.Low; // Default Low (Output 35 -> FAIL)
                }
            }
            else // NexusMode.RenewableGrid
            {
                if (_currentChallengeIndex == 0) // Sub 1
                {
                    _isSolarConnected = false;
                    _isWindConnected = false;
                    _isHydroConnected = false;
                }
                else if (_currentChallengeIndex == 1) // Sub 2
                {
                    // Reset kịch bản hiện tại nhưng KHÔNG xóa các kịch bản đã hoàn thành trước đó trong session
                    _weatherScenarioIdx = GetNextUncompletedWeatherScenario();
                    _selectedMatchingSource = RenewableSourceType.Solar;
                    _isMatchingSourceActive = false; // Default inactive -> Output 0 -> FAIL
                }
                else // Sub 3: Microgrid
                {
                    _solarModuleCount = 0;
                    _windModuleCount = 0;
                    _hydroModuleCount = 0; // Default 0 modules -> Output 0 -> FAIL
                }
            }

            UpdatePhysicsAndVisuals();

            if (showFeedback)
            {
                ShowFeedback("Đã đưa thử thách hiện tại về trạng thái ban đầu.", Color.white);
            }
        }

        public void NextChallenge()
        {
            if (IsStoryGateActive) return;
            int maxAllowed = _currentMode == NexusMode.EarthEnergyFossil ? _completedSubChallengesA.Count : _completedSubChallengesB.Count;
            if (_currentChallengeIndex >= maxAllowed)
            {
                ShowFeedback("Hãy hoàn thành thử thách hiện tại trước khi tiếp tục!", ColorAlertRed);
                return;
            }

            _currentChallengeIndex = Mathf.Clamp(_currentChallengeIndex + 1, 0, 2);
            ResetCurrentChallenge(false);
            ShowFeedback($"Chuyển sang Thử thách #{_currentChallengeIndex + 1}.", ColorGold);
        }

        public void PrevChallenge()
        {
            if (IsStoryGateActive) return;
            if (_currentChallengeIndex > 0)
            {
                _currentChallengeIndex--;
                ResetCurrentChallenge(false);
                ShowFeedback($"Chuyển sang Thử thách #{_currentChallengeIndex + 1}.", ColorGold);
            }
        }

        // =========================================================================
        // MODE A: TƯƠNG TÁC THỬ THÁCH
        // =========================================================================
        public void SelectOriginCard(int cardIdx)
        {
            if (IsStoryGateActive) return;
            _selectedOriginCardIdx = cardIdx;
            ShowFeedback($"Đã chọn thẻ: <b>{ORIGIN_NAMES[cardIdx]}</b>. Hãy bấm vào một ô trống để đặt thẻ.", ColorEnergyCyan);
            UpdatePhysicsAndVisuals();
        }

        public void PlaceOriginSlot(int slotIdx)
        {
            if (IsStoryGateActive) return;
            if (_selectedOriginCardIdx >= 0)
            {
                // Kiểm tra xem thẻ đã ở slot nào chưa, nếu có thì gỡ bỏ
                for (int i = 0; i < _originSlots.Length; i++)
                {
                    if (_originSlots[i] == _selectedOriginCardIdx) _originSlots[i] = -1;
                }

                _originSlots[slotIdx] = _selectedOriginCardIdx;
                _selectedOriginCardIdx = -1;
                ShowFeedback($"Đã đặt thẻ vào Vị trí #{slotIdx + 1}.", ColorGold);
            }
            else
            {
                // Nhấn vào ô đang có thẻ để tháo thẻ ra khay
                if (_originSlots[slotIdx] >= 0)
                {
                    int removed = _originSlots[slotIdx];
                    _originSlots[slotIdx] = -1;
                    ShowFeedback($"Đã đưa thẻ <b>{ORIGIN_NAMES[removed]}</b> về khay.", ColorGold);
                }
            }
            UpdatePhysicsAndVisuals();
        }

        public void SelectTransCard(int cardIdx)
        {
            if (IsStoryGateActive) return;
            _selectedTransCardIdx = cardIdx;
            ShowFeedback($"Đã chọn dạng năng lượng: <b>{TRANSFORMATION_NAMES[cardIdx]}</b>. Bấm vào một giai đoạn để sắp xếp.", ColorEnergyCyan);
            UpdatePhysicsAndVisuals();
        }

        public void PlaceTransSlot(int slotIdx)
        {
            if (IsStoryGateActive) return;
            if (_selectedTransCardIdx >= 0)
            {
                for (int i = 0; i < _transSlots.Length; i++)
                {
                    if (_transSlots[i] == _selectedTransCardIdx) _transSlots[i] = -1;
                }

                _transSlots[slotIdx] = _selectedTransCardIdx;
                _selectedTransCardIdx = -1;
                ShowFeedback($"Đã xếp vào Giai đoạn #{slotIdx + 1}.", ColorGold);
            }
            else
            {
                if (_transSlots[slotIdx] >= 0)
                {
                    int removed = _transSlots[slotIdx];
                    _transSlots[slotIdx] = -1;
                    ShowFeedback($"Đã đưa <b>{TRANSFORMATION_NAMES[removed]}</b> về khay lựa chọn.", ColorGold);
                }
            }
            UpdatePhysicsAndVisuals();
        }

        public void SelectFossilFuelLevel(FossilFuelLevel level)
        {
            if (IsStoryGateActive) return;
            _fossilFuelLevel = level;
            UpdatePhysicsAndVisuals();
            ShowFeedback($"Đã chỉnh mức cấp nhiên liệu hóa thạch: <b>{level}</b>.", ColorFossilAmber);
        }

        // =========================================================================
        // MODE B: TƯƠNG TÁC THỬ THÁCH
        // =========================================================================
        public void ToggleSolarConnection()
        {
            if (IsStoryGateActive) return;
            _isSolarConnected = !_isSolarConnected;
            UpdatePhysicsAndVisuals();
            ShowFeedback($"Pin mặt trời: {(_isSolarConnected ? "<color=#55FF88>ĐÃ KẾT NỐI</color>" : "<color=#FFAA00>ĐÃ NGẮT</color>")}", ColorEnergyCyan);
        }

        public void ToggleWindConnection()
        {
            if (IsStoryGateActive) return;
            _isWindConnected = !_isWindConnected;
            UpdatePhysicsAndVisuals();
            ShowFeedback($"Turbine gió: {(_isWindConnected ? "<color=#55FF88>ĐÃ KẾT NỐI</color>" : "<color=#FFAA00>ĐÃ NGẮT</color>")}", ColorEnergyCyan);
        }

        public void ToggleHydroConnection()
        {
            if (IsStoryGateActive) return;
            _isHydroConnected = !_isHydroConnected;
            UpdatePhysicsAndVisuals();
            ShowFeedback($"Turbine thủy điện: {(_isHydroConnected ? "<color=#55FF88>ĐÃ KẾT NỐI</color>" : "<color=#FFAA00>ĐÃ NGẮT</color>")}", ColorEnergyCyan);
        }

        public void SelectWeatherScenario(int scenarioIdx)
        {
            if (IsStoryGateActive) return;
            int maxAllowed = _completedWeatherScenarios.Count;
            if (scenarioIdx > maxAllowed)
            {
                ShowFeedback("Cần hoàn thành các kịch bản thời tiết trước đó theo thứ tự!", ColorAlertRed);
                return;
            }

            _weatherScenarioIdx = Mathf.Clamp(scenarioIdx, 0, 2);
            _isMatchingSourceActive = false;
            UpdatePhysicsAndVisuals();
            ShowFeedback($"Đã chọn Kịch bản thời tiết #{_weatherScenarioIdx + 1}.", ColorGold);
        }

        private int GetNextUncompletedWeatherScenario()
        {
            if (!_completedWeatherScenarios.Contains(0)) return 0;
            if (!_completedWeatherScenarios.Contains(1)) return 1;
            if (!_completedWeatherScenarios.Contains(2)) return 2;
            return 2;
        }

        public void SelectMatchingSource(RenewableSourceType source)
        {
            if (IsStoryGateActive) return;
            _selectedMatchingSource = source;
            _isMatchingSourceActive = true;
            UpdatePhysicsAndVisuals();
            ShowFeedback($"Đã kích hoạt nguồn cấp chính: <b>{GetSourceName(source)}</b>.", ColorRenewableGreen);
        }

        public void AdjustModuleCount(RenewableSourceType source, int delta)
        {
            if (IsStoryGateActive) return;
            int currentTotal = _solarModuleCount + _windModuleCount + _hydroModuleCount;
            if (delta > 0 && currentTotal >= MAX_MODULE_BUDGET)
            {
                ShowFeedback($"Đã đạt ngân sách tối đa {MAX_MODULE_BUDGET} modules!", ColorAlertRed);
                return;
            }

            switch (source)
            {
                case RenewableSourceType.Solar:
                    _solarModuleCount = Mathf.Clamp(_solarModuleCount + delta, 0, 3);
                    break;
                case RenewableSourceType.Wind:
                    _windModuleCount = Mathf.Clamp(_windModuleCount + delta, 0, 3);
                    break;
                case RenewableSourceType.Hydro:
                    _hydroModuleCount = Mathf.Clamp(_hydroModuleCount + delta, 0, 3);
                    break;
            }

            UpdatePhysicsAndVisuals();
        }

        private string GetSourceName(RenewableSourceType s)
        {
            switch (s)
            {
                case RenewableSourceType.Solar: return "Pin mặt trời (Solar)";
                case RenewableSourceType.Wind: return "Turbine gió (Wind)";
                case RenewableSourceType.Hydro: return "Thủy điện (Hydro)";
                default: return "";
            }
        }

        // =========================================================================
        // TÍNH TOÁN VẬT LÝ & CẬP NHẬT UI
        // =========================================================================
        private void UpdatePhysicsAndVisuals()
        {
            if (_currentMode == NexusMode.EarthEnergyFossil)
            {
                EvaluateModeAPhysics();
            }
            else
            {
                EvaluateModeBPhysics();
            }

            UpdateHighlightButtons();
        }

        private void EvaluateModeAPhysics()
        {
            if (_modeASub1Root != null) _modeASub1Root.SetActive(_currentChallengeIndex == 0);
            if (_modeASub2Root != null) _modeASub2Root.SetActive(_currentChallengeIndex == 1);
            if (_modeASub3Root != null) _modeASub3Root.SetActive(_currentChallengeIndex == 2);

            string title = "";
            string desc = "";

            if (_currentChallengeIndex == 0) // Sub 1
            {
                title = "Thử thách 1: Nguồn gốc năng lượng hóa thạch";
                desc = "Xếp 4 thẻ theo đúng vòng năng lượng từ Mặt Trời đến Nhiên liệu hóa thạch:\n" +
                       "<i>Mặt Trời ➔ Sinh khối / Thực vật ➔ Hợp chất hữu cơ cổ đại ➔ Nhiên liệu hóa thạch</i>\n" +
                       "<size=12>Quá trình hình thành nhiên liệu hóa thạch diễn ra trong thời gian địa chất rất dài.</size>";

                // Cập nhật text slot Sub 1
                for (int i = 0; i < 4; i++)
                {
                    if (_txtOriginSlots[i] != null)
                    {
                        int card = _originSlots[i];
                        _txtOriginSlots[i].text = card >= 0 ? $"<b>{ORIGIN_NAMES[card]}</b>" : "<color=#667799>[Nhấn để đặt thẻ]</color>";
                        _txtOriginSlots[i].color = card >= 0 ? ColorGold : Color.gray;
                    }
                }

                if (_txtReadoutMain != null)
                {
                    _txtReadoutMain.text = "<b>Bản chất nguồn gốc năng lượng:</b>\n" +
                                           "Năng lượng hóa học trong nhiên liệu hóa thạch có nguồn gốc gián tiếp từ năng lượng Mặt Trời " +
                                           "được thực vật và sinh vật tích lũy qua hàng triệu năm.";
                }
                if (_txtReadoutSecondary != null)
                {
                    int placedCount = 0;
                    for (int i = 0; i < 4; i++) if (_originSlots[i] >= 0) placedCount++;
                    _txtReadoutSecondary.text = $"Số thẻ đã xếp: <b>{placedCount}/4</b>\n" +
                                                $"Thẻ đang chọn: {( _selectedOriginCardIdx >= 0 ? ORIGIN_NAMES[_selectedOriginCardIdx] : "<color=#888888>Chưa chọn</color>" )}";
                }
                if (_txtReadoutFormula != null)
                {
                    _txtReadoutFormula.text = "<b>Vòng tuần hoàn năng lượng:</b>\nQuang năng Mặt Trời ➔ Hóa năng sinh vật ➔ Nhiên liệu hóa thạch";
                }
            }
            else if (_currentChallengeIndex == 1) // Sub 2
            {
                title = "Thử thách 2: Chuỗi chuyển hóa năng lượng nhà máy nhiệt điện";
                desc = "Sắp xếp đúng trình tự chuỗi biến đổi năng lượng từ nhiên liệu đến điện năng tiêu thụ:\n" +
                       "<b>Hóa năng ➔ Nhiệt năng ➔ Cơ năng ➔ Điện năng</b>\n" +
                       "<size=12>Turbine và máy phát chuyển hóa cơ năng thành điện năng (không tự sinh năng lượng).</size>";

                for (int i = 0; i < 4; i++)
                {
                    if (_txtTransSlots[i] != null)
                    {
                        int card = _transSlots[i];
                        _txtTransSlots[i].text = card >= 0 ? $"<b>{TRANSFORMATION_NAMES[card]}</b>" : "<color=#667799>[Nhấn để đặt]</color>";
                        _txtTransSlots[i].color = card >= 0 ? ColorEnergyCyan : Color.gray;
                    }
                }

                if (_txtReadoutMain != null)
                {
                    _txtReadoutMain.text = "<b>Định luật bảo toàn năng lượng:</b>\n" +
                                           "Năng lượng không tự nhiên sinh ra cũng không tự nhiên mất đi, " +
                                           "nó chỉ chuyển hóa từ dạng này sang dạng khác hoặc truyền từ vật này sang vật khác.";
                }
                if (_txtReadoutSecondary != null)
                {
                    int placedCount = 0;
                    for (int i = 0; i < 4; i++) if (_transSlots[i] >= 0) placedCount++;
                    _txtReadoutSecondary.text = $"Giai đoạn đã hoàn thành: <b>{placedCount}/4</b>\n" +
                                                $"Trạng thái máy phát: {(_transSlots[3] == 3 ? "<color=#55FF88>Sẵn sàng phát điện</color>" : "<color=#FFAA00>Chưa đồng bộ</color>")}";
                }
                if (_txtReadoutFormula != null)
                {
                    _txtReadoutFormula.text = "<b>Chuỗi chuyển hóa năng lượng:</b>\nHóa năng (Than/Khí) ➔ Nhiệt năng (Lò hơi) ➔ Cơ năng (Turbine) ➔ Điện năng (Máy phát)";
                }
            }
            else // Sub 3
            {
                title = "Thử thách 3: Vận hành nhà máy nhiệt điện Aurelia";
                desc = "Nhu cầu lưới điện: <b>70 đơn vị năng lượng</b>.\n" +
                       "Hãy chọn mức cấp nhiên liệu để đáp ứng nhu cầu (70–80 đơn vị) mà không gây dư thừa lãng phí quá mức.";

                float output = _fossilFuelLevel == FossilFuelLevel.Low ? 35f :
                               _fossilFuelLevel == FossilFuelLevel.Medium ? 70f : 100f;
                float impact = _fossilFuelLevel == FossilFuelLevel.Low ? 20f :
                               _fossilFuelLevel == FossilFuelLevel.Medium ? 50f : 85f;

                if (_txtFossilOutputVal != null) _txtFossilOutputVal.text = $"Sản lượng phát: <b>{output:F0} / 70 đơn vị</b>";
                if (_txtFossilImpactVal != null) _txtFossilImpactVal.text = $"Tác động môi trường: <b>{impact:F0}%</b>";
                if (_imgFossilImpactBar != null) _imgFossilImpactBar.fillAmount = impact / 100f;

                string stOutput = (output >= 70f && output <= 80f) ? "<color=#55FF88>ĐẠT YÊU CẦU CUNG CẤP (70)</color>" :
                                  output < 70f ? "<color=#FFAA00>THIẾU ĐIỆN (CẦN TĂNG)</color>" : "<color=#FF5555>DƯ THỪA QUÁ MỨC</color>";

                if (_txtReadoutMain != null)
                {
                    _txtReadoutMain.text = $"Nhu cầu phụ tải: <b>70 đơn vị</b>\n" +
                                           $"Sản lượng mô phỏng: <b>{output:F0} đơn vị</b> ({stOutput})\n" +
                                           $"Mức tác động môi trường: <b>{impact:F0}%</b>";
                }
                if (_txtReadoutSecondary != null)
                {
                    _txtReadoutSecondary.text = "<b>Đặc điểm năng lượng hóa thạch:</b>\n" +
                                                "• Là nguồn năng lượng không tái tạo trong thang thời gian con người.\n" +
                                                "• Trữ lượng hữu hạn và quá trình khai thác / sử dụng phát thải tác động môi trường.";
                }
                if (_txtReadoutFormula != null)
                {
                    _txtReadoutFormula.text = "<b>Cân bằng năng lượng:</b> Sản lượng phát ≥ Nhu cầu tải (Target: 70 - 80 đơn vị)";
                }
            }

            if (_txtMissionTitle != null) _txtMissionTitle.text = title;
            if (_txtMissionDesc != null) _txtMissionDesc.text = desc;
            if (_txtChallengeCounter != null)
            {
                string stA = _completedSubChallengesA.Contains(_currentChallengeIndex) ? "<color=#55FF88>[ĐÃ ĐẠT]</color>" : "<color=#FFAA00>[CHƯA ĐẠT]</color>";
                _txtChallengeCounter.text = $"Nhiệm vụ {_currentChallengeIndex + 1}/3 {stA} · Đã đạt: {_completedSubChallengesA.Count}/3";
            }
        }

        private void EvaluateModeBPhysics()
        {
            if (_modeBSub1Root != null) _modeBSub1Root.SetActive(_currentChallengeIndex == 0);
            if (_modeBSub2Root != null) _modeBSub2Root.SetActive(_currentChallengeIndex == 1);
            if (_modeBSub3Root != null) _modeBSub3Root.SetActive(_currentChallengeIndex == 2);

            string title = "";
            string desc = "";

            if (_currentChallengeIndex == 0) // Sub 1: Renewable Sources Lab
            {
                title = "Thử thách 1: Kết nối các nguồn năng lượng tái tạo";
                desc = "Kết nối đầy đủ 3 nguồn năng lượng xanh vào Lõi Năng Lượng (Energy Nexus):\n" +
                       "• <b>Pin mặt trời</b> (Quang năng ➔ Điện năng)\n" +
                       "• <b>Turbine gió</b> (Động năng không khí ➔ Điện năng)\n" +
                       "• <b>Turbine thủy điện</b> (Cơ năng dòng nước ➔ Điện năng)";

                if (_imgSolarConnectStatus != null) _imgSolarConnectStatus.color = _isSolarConnected ? ColorRenewableGreen : Color.gray;
                if (_imgWindConnectStatus != null) _imgWindConnectStatus.color = _isWindConnected ? ColorRenewableGreen : Color.gray;
                if (_imgHydroConnectStatus != null) _imgHydroConnectStatus.color = _isHydroConnected ? ColorRenewableGreen : Color.gray;

                int connectedCount = (_isSolarConnected ? 1 : 0) + (_isWindConnected ? 1 : 0) + (_isHydroConnected ? 1 : 0);

                if (_txtReadoutMain != null)
                {
                    _txtReadoutMain.text = $"Pin mặt trời (Solar): {(_isSolarConnected ? "<color=#55FF88>ĐÃ KẾT NỐI</color>" : "<color=#888888>Chưa</color>")}\n" +
                                           $"Turbine gió (Wind): {(_isWindConnected ? "<color=#55FF88>ĐÃ KẾT NỐI</color>" : "<color=#888888>Chưa</color>")}\n" +
                                           $"Turbine thủy điện (Hydro): {(_isHydroConnected ? "<color=#55FF88>ĐÃ KẾT NỐI</color>" : "<color=#888888>Chưa</color>")}";
                }
                if (_txtReadoutSecondary != null)
                {
                    _txtReadoutSecondary.text = $"Tổng nguồn kết nối: <b>{connectedCount}/3</b>\n" +
                                                $"Trạng thái Energy Nexus: {(connectedCount == 3 ? "<color=#55FF88>KẾT NỐI HOÀN TẤT</color>" : "<color=#FFAA00>ĐANG CHỜ KẾT NỐI</color>")}";
                }
                if (_txtReadoutFormula != null)
                {
                    _txtReadoutFormula.text = "<b>Năng lượng tái tạo:</b> Nguồn năng lượng liên tục được bổ sung tự nhiên từ Mặt Trời, gió và dòng nước.";
                }
            }
            else if (_currentChallengeIndex == 1) // Sub 2: Weather Matching
            {
                title = "Thử thách 2: Lựa chọn nguồn theo điều kiện thời tiết";
                desc = "Thời tiết biến đổi làm công suất các nguồn tái tạo thay đổi.\n" +
                       "Hãy chọn nguồn phát điện tối ưu tương ứng với kịch bản thời tiết để đạt công suất mô phỏng ≥ 70 đơn vị.\n" +
                       $"<size=12>Tiến độ kịch bản thời tiết: <b>{_completedWeatherScenarios.Count}/3</b> (Kịch bản hiện tại: #{_weatherScenarioIdx + 1})</size>";

                // Các kịch bản thời tiết (Mô hình kịch bản giáo dục độc lập - Simulation Units, không phải MW thực tế):
                // Scen 0: Sun 90%, Wind 20%, Water 50% -> Solar tối ưu (Output 72.0 đv)
                // Scen 1: Sun 15%, Wind 90%, Water 40% -> Wind tối ưu (Output 72.0 đv)
                // Scen 2: Sun 20%, Wind 25%, Water 90% -> Hydro tối ưu (Output 72.0 đv)
                string weatherStr = _weatherScenarioIdx == 0 ? "Kịch bản 1: Nắng gắt (Mặt Trời: 90% | Gió: 20% | Nước: 50%)" :
                                    _weatherScenarioIdx == 1 ? "Kịch bản 2: Gió bão (Mặt Trời: 15% | Gió: 90% | Nước: 40%)" :
                                    "Kịch bản 3: Mưa lũ nguồn (Mặt Trời: 20% | Gió: 25% | Nước: 90%)";

                string scenStatus = _completedWeatherScenarios.Contains(_weatherScenarioIdx) ? "<color=#55FF88>[ĐÃ HOÀN THÀNH KỊCH BẢN NÀY]</color>" : "<color=#FFAA00>[CHƯA KIỂM TRA ĐẠT]</color>";

                float factor = 0f;
                if (_weatherScenarioIdx == 0)
                    factor = _selectedMatchingSource == RenewableSourceType.Solar ? 0.90f : (_selectedMatchingSource == RenewableSourceType.Wind ? 0.20f : 0.50f);
                else if (_weatherScenarioIdx == 1)
                    factor = _selectedMatchingSource == RenewableSourceType.Wind ? 0.90f : (_selectedMatchingSource == RenewableSourceType.Solar ? 0.15f : 0.40f);
                else
                    factor = _selectedMatchingSource == RenewableSourceType.Hydro ? 0.90f : (_selectedMatchingSource == RenewableSourceType.Solar ? 0.20f : 0.25f);

                float currentOutput = _isMatchingSourceActive ? (factor * 80f) : 0f;

                if (_txtWeatherCondition != null) _txtWeatherCondition.text = $"{weatherStr}\n{scenStatus}";
                if (_txtWeatherOutputVal != null) _txtWeatherOutputVal.text = $"Công suất mô phỏng: <b>{currentOutput:F1} đơn vị</b> (Simulation Units - Yêu cầu ≥ 70)";

                bool isOptimal = currentOutput >= 70f;

                if (_txtReadoutMain != null)
                {
                    _txtReadoutMain.text = $"Nguồn đang chọn: <b>{GetSourceName(_selectedMatchingSource)}</b>\n" +
                                           $"Trạng thái phát điện: {(_isMatchingSourceActive ? "<color=#55FF88>ĐANG PHÁT</color>" : "<color=#888888>NGẮT</color>")}\n" +
                                           $"Công suất mô phỏng: <b>{currentOutput:F1} đơn vị</b> (Simulation Units, không phải MW)";
                }
                if (_txtReadoutSecondary != null)
                {
                    _txtReadoutSecondary.text = $"Đánh giá tính phù hợp: {(isOptimal ? "<color=#55FF88>RẤT TỐI ƯU (ĐẠT CHỈ TIÊU)</color>" : "<color=#FFAA00>CHƯA PHÙ HỢP VỚI THỜI TIẾT HIỆN TẠI</color>")}\n" +
                                                $"<i>Không có một nguồn duy nhất luôn tốt nhất trong mọi thời điểm; cần linh hoạt theo điều kiện tự nhiên.</i>";
                }
                if (_txtReadoutFormula != null)
                {
                    _txtReadoutFormula.text = "<b>Mô hình kịch bản thời tiết:</b> Công suất chuẩn hóa (Simulation Units) phụ thuộc vào điều kiện bức xạ mặt trời, lưu tốc gió hoặc lưu lượng dòng nước.";
                }
            }
            else // Sub 3: Microgrid Balance
            {
                title = "Thử thách 3: Điều độ vi lưới năng lượng Aurelia";
                desc = "Nhu cầu vi lưới: <b>100 đơn vị năng lượng</b>.\n" +
                       "Điều kiện tự nhiên: Nắng 80% (Solar=24.0/máy), Gió 50% (Wind=17.5/máy), Dòng chảy 70% (Hydro=28.0/máy).\n" +
                       "<b>Mục tiêu:</b> Lắp đặt tối đa <b>6 modules</b> để tạo tổng sản lượng <b>100 ≤ Tổng phát ≤ 120</b>.";

                float solarOut = _solarModuleCount * 30f * 0.8f;   // 24.0
                float windOut = _windModuleCount * 35f * 0.5f;     // 17.5
                float hydroOut = _hydroModuleCount * 40f * 0.7f;   // 28.0
                float totalOut = solarOut + windOut + hydroOut;
                int totalModules = _solarModuleCount + _windModuleCount + _hydroModuleCount;

                if (_txtMicrogridCounters != null)
                {
                    _txtMicrogridCounters.text = $"Solar: {_solarModuleCount} ({solarOut:F1}) | Wind: {_windModuleCount} ({windOut:F1}) | Hydro: {_hydroModuleCount} ({hydroOut:F1})\n" +
                                                 $"Tổng số modules: <b>{totalModules} / {MAX_MODULE_BUDGET}</b>";
                }

                float balance = totalOut - DEMAND_MICROGRID;
                string stBalance = (totalOut >= 100f && totalOut <= 120f && totalModules <= MAX_MODULE_BUDGET) ?
                    "<color=#55FF88>VI LƯỚI CÂN BẰNG HOÀN HẢO</color>" :
                    totalOut < 100f ? "<color=#FFAA00>THIẾU HỤT CÔNG SUẤT</color>" : "<color=#FF5555>QUÁ TẢI CÔNG SUẤT (DƯ THỪA)</color>";

                if (_txtMicrogridSupplyDemand != null)
                {
                    _txtMicrogridSupplyDemand.text = $"Tổng cung: <b>{totalOut:F1}</b> | Nhu cầu: <b>100.0</b> | Chênh lệch: <b>{(balance >= 0 ? "+" : "")}{balance:F1}</b>\n{stBalance}";
                }

                if (_imgPowerBalanceBar != null)
                {
                    _imgPowerBalanceBar.fillAmount = Mathf.Clamp01(totalOut / 120f);
                    _imgPowerBalanceBar.color = (totalOut >= 100f && totalOut <= 120f) ? ColorRenewableGreen :
                                                totalOut < 100f ? ColorFossilAmber : ColorAlertRed;
                }

                if (_txtReadoutMain != null)
                {
                    _txtReadoutMain.text = $"Nhu cầu lưới: <b>100.0 đơn vị</b>\n" +
                                           $"Sản lượng vi lưới: <b>{totalOut:F1} đơn vị</b>\n" +
                                           $"Ngân sách lắp đặt: <b>{totalModules} / {MAX_MODULE_BUDGET} modules</b>";
                }
                if (_txtReadoutSecondary != null)
                {
                    _txtReadoutSecondary.text = $"Đánh giá điều độ: {stBalance}\n" +
                                                $"<i>Sự kết hợp đa dạng các nguồn năng lượng tái tạo giúp lưới điện vận hành ổn định và bền vững.</i>";
                }
                if (_txtReadoutFormula != null)
                {
                    _txtReadoutFormula.text = "<b>Cân bằng vi lưới:</b> Tổng công suất phát = Σ (Số module × Công suất cơ sở × Hệ số thời tiết)";
                }
            }

            if (_txtMissionTitle != null) _txtMissionTitle.text = title;
            if (_txtMissionDesc != null) _txtMissionDesc.text = desc;
            if (_txtChallengeCounter != null)
            {
                string stB = _completedSubChallengesB.Contains(_currentChallengeIndex) ? "<color=#55FF88>[ĐÃ ĐẠT]</color>" : "<color=#FFAA00>[CHƯA ĐẠT]</color>";
                _txtChallengeCounter.text = $"Nhiệm vụ {_currentChallengeIndex + 1}/3 {stB} · Đã đạt: {_completedSubChallengesB.Count}/3";
            }
        }

        // =========================================================================
        // KIỂM TRA MỤC TIÊU & FLOW THẮNG / THUA (SUB-CHALLENGE GATING)
        // =========================================================================
        public void OnCheckClicked()
        {
            if (IsStoryGateActive) return;
            if (_isCompleted) return;

            if (_currentMode == NexusMode.EarthEnergyFossil)
            {
                if (_currentChallengeIndex == 0) // Sub 1: Energy Origin
                {
                    // Target: 0:Sun, 1:Biomass, 2:Organic Matter, 3:Fossil Fuel
                    bool isCorrect = _originSlots[0] == 0 && _originSlots[1] == 1 && _originSlots[2] == 2 && _originSlots[3] == 3;
                    if (isCorrect)
                    {
                        _completedSubChallengesA.Add(0);
                        PlaySoundSafe(true);
                        ShowFeedback("<b>XUẤT SẮC!</b> Đã xác định đúng nguồn gốc năng lượng hóa thạch. Đang chuyển sang Thử thách 2: Chuỗi chuyển hóa...", ColorRenewableGreen);
                        StartCoroutine(AdvanceSubChallengeRoutine(1));
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        ShowFeedback("Thứ tự vòng năng lượng chưa đúng. Hãy sắp xếp: Mặt Trời ➔ Sinh khối ➔ Hợp chất hữu cơ ➔ Nhiên liệu hóa thạch.", ColorAlertRed);
                    }
                }
                else if (_currentChallengeIndex == 1) // Sub 2: Transformation Chain
                {
                    // Target: 0:Hóa năng, 1:Nhiệt năng, 2:Cơ năng, 3:Điện năng
                    bool isCorrect = _transSlots[0] == 0 && _transSlots[1] == 1 && _transSlots[2] == 2 && _transSlots[3] == 3;
                    if (isCorrect)
                    {
                        _completedSubChallengesA.Add(1);
                        PlaySoundSafe(true);
                        ShowFeedback("<b>XUẤT SẮC!</b> Chuỗi chuyển hóa năng lượng hoàn toàn chính xác. Đang chuyển sang Thử thách 3: Vận hành nhà máy...", ColorRenewableGreen);
                        StartCoroutine(AdvanceSubChallengeRoutine(2));
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        ShowFeedback("Chuỗi chuyển hóa chưa chính xác. Hãy xếp: Hóa năng ➔ Nhiệt năng ➔ Cơ năng ➔ Điện năng.", ColorAlertRed);
                    }
                }
                else // Sub 3: Fossil Management
                {
                    float output = _fossilFuelLevel == FossilFuelLevel.Low ? 35f :
                                   _fossilFuelLevel == FossilFuelLevel.Medium ? 70f : 100f;
                    bool isOptimal = output >= 70f && output <= 80f;

                    if (isOptimal)
                    {
                        _completedSubChallengesA.Add(2);
                        if (_completedSubChallengesA.Contains(0) && _completedSubChallengesA.Contains(1) && _completedSubChallengesA.Contains(2))
                        {
                            TriggerSuccessFlow();
                        }
                        else
                        {
                            PlaySoundSafe(false);
                            ShowFeedback("Đã đạt công suất nhiệt điện, nhưng bạn cần hoàn thành cả Thử thách 1 và 2 trước đó để hoàn tất bài học!", ColorAlertRed);
                        }
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        string err = output < 70f ? "Sản lượng 35 đơn vị chưa đủ đáp ứng nhu cầu 70 đơn vị." : "Sản lượng 100 đơn vị gây dư thừa quá mức và phát thải cao.";
                        ShowFeedback(err + " Hãy điều chỉnh lại mức cấp nhiên liệu tối ưu.", ColorAlertRed);
                    }
                }
            }
            else // NexusMode.RenewableGrid
            {
                if (_currentChallengeIndex == 0) // Sub 1: 3 Sources connected
                {
                    bool allConnected = _isSolarConnected && _isWindConnected && _isHydroConnected;
                    if (allConnected)
                    {
                        _completedSubChallengesB.Add(0);
                        PlaySoundSafe(true);
                        ShowFeedback("<b>XUẤT SẮC!</b> Đã kết nối đủ cả 3 nguồn năng lượng tái tạo (Pin mặt trời, Turbine gió, Thủy điện). Đang chuyển sang Thử thách 2: Lựa chọn theo thời tiết...", ColorRenewableGreen);

                        bool canCoroutine = false;
                        try
                        {
                            if (Application.isPlaying && isActiveAndEnabled)
                            {
                                StartCoroutine(AdvanceSubChallengeRoutine(1));
                                canCoroutine = true;
                            }
                        }
                        catch { canCoroutine = false; }

                        if (!canCoroutine)
                        {
                            _currentChallengeIndex = 1;
                            ResetCurrentChallenge(false);
                        }
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        int connectedCount = (_isSolarConnected ? 1 : 0) + (_isWindConnected ? 1 : 0) + (_isHydroConnected ? 1 : 0);
                        ShowFeedback($"Chưa đủ 3 nguồn (hiện có: {connectedCount}/3). Cần kết nối đầy đủ cả 3 nguồn (Pin mặt trời, Turbine gió, Thủy điện) vào hệ thống.", ColorAlertRed);
                    }
                }
                else if (_currentChallengeIndex == 1) // Sub 2: Weather Matching
                {
                    if (!_isMatchingSourceActive)
                    {
                        PlaySoundSafe(false);
                        ShowFeedback("Chưa kích hoạt nguồn năng lượng nào! Hãy chọn và bật một nguồn tái tạo.", ColorAlertRed);
                        return;
                    }

                    if (_weatherScenarioIdx > _completedWeatherScenarios.Count)
                    {
                        PlaySoundSafe(false);
                        ShowFeedback("Cần hoàn thành các kịch bản thời tiết trước đó theo thứ tự!", ColorAlertRed);
                        return;
                    }

                    bool isCorrectSource = false;
                    if (_weatherScenarioIdx == 0) // Scenario A: Sunny (Sun 90%, Wind 20%, Water 50%) -> Solar
                    {
                        isCorrectSource = (_selectedMatchingSource == RenewableSourceType.Solar);
                    }
                    else if (_weatherScenarioIdx == 1) // Scenario B: Windy (Sun 15%, Wind 90%, Water 40%) -> Wind
                    {
                        isCorrectSource = (_selectedMatchingSource == RenewableSourceType.Wind);
                    }
                    else if (_weatherScenarioIdx == 2) // Scenario C: High Water (Sun 20%, Wind 25%, Water 90%) -> Hydro
                    {
                        isCorrectSource = (_selectedMatchingSource == RenewableSourceType.Hydro);
                    }

                    if (!isCorrectSource)
                    {
                        PlaySoundSafe(false);
                        ShowFeedback("Nguồn này chưa phù hợp nhất với điều kiện hiện tại. Hãy quan sát mức nắng, gió và dòng nước.", ColorAlertRed);
                        return;
                    }

                    // Kịch bản hiện tại ĐẠT!
                    _completedWeatherScenarios.Add(_weatherScenarioIdx);
                    PlaySoundSafe(true);

                    if (_weatherScenarioIdx == 0)
                    {
                        _weatherScenarioIdx = 1;
                        _isMatchingSourceActive = false;
                        ShowFeedback("<b>CHÍNH XÁC!</b> Pin mặt trời tối ưu nhất trong điều kiện nắng gắt (90%). Chuyển sang Kịch bản 2: Gió lớn...", ColorRenewableGreen);
                        UpdatePhysicsAndVisuals();
                    }
                    else if (_weatherScenarioIdx == 1)
                    {
                        _weatherScenarioIdx = 2;
                        _isMatchingSourceActive = false;
                        ShowFeedback("<b>CHÍNH XÁC!</b> Turbine gió tối ưu nhất trong điều kiện gió bão (90%). Chuyển sang Kịch bản 3: Mưa lũ nguồn...", ColorRenewableGreen);
                        UpdatePhysicsAndVisuals();
                    }
                    else if (_weatherScenarioIdx == 2)
                    {
                        if (_completedWeatherScenarios.Contains(0) && _completedWeatherScenarios.Contains(1) && _completedWeatherScenarios.Contains(2))
                        {
                            _completedSubChallengesB.Add(1); // Sub 2 chính thức hoàn thành!
                            ShowFeedback("<b>XUẤT SẮC!</b> Đã hoàn thành cả 3 kịch bản thời tiết. Đang chuyển sang Thử thách 3: Điều độ vi lưới...", ColorRenewableGreen);

                            bool canCoroutine = false;
                            try
                            {
                                if (Application.isPlaying && isActiveAndEnabled)
                                {
                                    StartCoroutine(AdvanceSubChallengeRoutine(2));
                                    canCoroutine = true;
                                }
                            }
                            catch { canCoroutine = false; }

                            if (!canCoroutine)
                            {
                                _currentChallengeIndex = 2;
                                ResetCurrentChallenge(false);
                            }
                        }
                        else
                        {
                            _weatherScenarioIdx = GetNextUncompletedWeatherScenario();
                            _isMatchingSourceActive = false;
                            ShowFeedback("Hãy hoàn thành đủ cả 3 kịch bản thời tiết theo thứ tự!", ColorAlertRed);
                            UpdatePhysicsAndVisuals();
                        }
                    }
                }
                else // Sub 3: Microgrid Balance
                {
                    float solarOut = _solarModuleCount * 30f * 0.8f;
                    float windOut = _windModuleCount * 35f * 0.5f;
                    float hydroOut = _hydroModuleCount * 40f * 0.7f;
                    float totalOut = solarOut + windOut + hydroOut;
                    int totalModules = _solarModuleCount + _windModuleCount + _hydroModuleCount;

                    bool isBalanced = totalOut >= 100f && totalOut <= 120f && totalModules <= MAX_MODULE_BUDGET;

                    if (isBalanced)
                    {
                        _completedSubChallengesB.Add(2);
                        if (_completedSubChallengesB.Contains(0) && _completedSubChallengesB.Contains(1) && _completedSubChallengesB.Contains(2))
                        {
                            TriggerSuccessFlow();
                        }
                        else
                        {
                            PlaySoundSafe(false);
                            ShowFeedback("Vi lưới đã cân bằng, nhưng bạn cần hoàn thành cả Thử thách 1 và 2 trước đó để hoàn tất bài học!", ColorAlertRed);
                        }
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        string err = totalModules > MAX_MODULE_BUDGET ? $"Đã vượt quá ngân sách {MAX_MODULE_BUDGET} modules." :
                                     totalOut < 100f ? $"Tổng công suất ({totalOut:F1}) chưa đáp ứng đủ nhu cầu 100 đơn vị." :
                                     $"Tổng công suất ({totalOut:F1}) vượt quá ngưỡng cho phép 120 đơn vị.";
                        ShowFeedback(err + " Hãy phối hợp lại số lượng modules Solar, Wind và Hydro.", ColorAlertRed);
                    }
                }
            }
        }

        private IEnumerator AdvanceSubChallengeRoutine(int nextIndex)
        {
            yield return new WaitForSeconds(1.2f);
            _currentChallengeIndex = nextIndex;
            ResetCurrentChallenge(false);
            ShowFeedback($"Đã chuyển sang Thử thách #{_currentChallengeIndex + 1}.", ColorGold);
        }

        private void TriggerSuccessFlow()
        {
            _isCompleted = true;
            PlaySoundSafe(true);

            string successMsg = _currentMode == NexusMode.EarthEnergyFossil ?
                "CHUỖI NĂNG LƯỢNG TRÁI ĐẤT ĐÃ ĐƯỢC LẬP THÀNH CÔNG!" :
                "VI LƯỚI NĂNG LƯỢNG TÁI TẠO ĐÃ ĐƯỢC ĐIỀU ĐỘ XUẤT SẮC!";

            ShowFeedback($"<b>XUẤT SẮC!</b> {successMsg}", ColorRenewableGreen);

            bool canCoroutine = false;
            try
            {
                if (Application.isPlaying && isActiveAndEnabled)
                {
                    StartCoroutine(TriggerSuccessSequence());
                    canCoroutine = true;
                }
            }
            catch { canCoroutine = false; }

            if (!canCoroutine)
            {
                ApplyLessonRewards();
            }
        }

        public void ApplyLessonRewards()
        {
            var save = PhysicsSaveManager.Load() ?? new PhysicsSaveData();

            save.MarkExploreLessonCompleted(_lessonId);
            save.MarkGameplayChapterCompleted("chapter_5");

            if (_lessonId == "C5_L16")
            {
                InventoryManager.UnlockItem("turbine", save);
            }
            else if (_lessonId == "C5_L17")
            {
                InventoryManager.UnlockItem("solar_panel", save);
            }

            // Đánh giá hành trình cuối cùng (Final Progression)
            bool crystalGranted = PhysicsFinalProgression.EvaluateFinalProgression(save);

            if (!crystalGranted)
            {
                AchievementManager.EvaluateAchievements(save);
                PhysicsSaveManager.Save(save);
                if (PhysicsGameSession.Instance != null)
                {
                    PhysicsGameSession.Instance.ReloadSaveData();
                }
            }
        }

        private IEnumerator TriggerSuccessSequence()
        {
            yield return new WaitForSeconds(0.6f);

            var save = PhysicsSaveManager.Load() ?? new PhysicsSaveData();

            save.MarkExploreLessonCompleted(_lessonId);
            save.MarkGameplayChapterCompleted("chapter_5");

            string unlockedItemName = "";
            bool isItemUnlocked = false;

            if (_lessonId == "C5_L16")
            {
                isItemUnlocked = InventoryManager.UnlockItem("turbine", save);
                if (isItemUnlocked) unlockedItemName = "Turbine phát điện";
            }
            else if (_lessonId == "C5_L17")
            {
                isItemUnlocked = InventoryManager.UnlockItem("solar_panel", save);
                if (isItemUnlocked) unlockedItemName = "Tấm pin mặt trời (Solar Panel)";
            }

            // Đánh giá hành trình cuối cùng (Final Progression)
            bool crystalGrantedOnTrigger = PhysicsFinalProgression.EvaluateFinalProgression(save);

            if (!crystalGrantedOnTrigger)
            {
                AchievementManager.EvaluateAchievements(save);
                PhysicsSaveManager.Save(save);
                if (PhysicsGameSession.Instance != null)
                {
                    PhysicsGameSession.Instance.ReloadSaveData();
                }
            }

            ShowSuccessModal(isItemUnlocked, unlockedItemName);
        }

        private void ShowSuccessModal(bool isItemUnlocked, string unlockedItemName)
        {
            if (_successModal == null) return;

            foreach (Transform child in _successModal.transform)
            {
                Destroy(child.gameObject);
            }

            string modalHeader = _currentMode == NexusMode.EarthEnergyFossil ?
                "NĂNG LƯỢNG TRÁI ĐẤT ĐÃ HOÀN TẤT!" :
                "LƯỚI NĂNG LƯỢNG TÁI TẠO ĐÃ HOÀN TẤT!";

            CreatePanelHeader(_successModal.transform, modalHeader, new Vector2(0, 185));

            GameObject bodyGo = new GameObject("SuccessBody");
            bodyGo.transform.SetParent(_successModal.transform, false);
            RectTransform bRt = bodyGo.AddComponent<RectTransform>();
            bRt.anchoredPosition = new Vector2(0, 25);
            bRt.sizeDelta = new Vector2(640, 240);

            var bTmp = bodyGo.AddComponent<TextMeshProUGUI>();
            string itemNotice = isItemUnlocked ?
                $"<color=#FFE600>★ CHÚC MỪNG: Bạn đã nhận được vật phẩm mới: {unlockedItemName}!</color>" :
                "Tiến trình thực hành đã được lưu an toàn vào Học viện Aurelia.";

            bTmp.text = $"<b>Chúc mừng bạn đã hoàn thành bài học: {_lessonTitle}!</b>\n\n" +
                        $"• Tiến trình khám phá: <color=#55FF88>Đã ghi nhận bài {_lessonId}</color>\n" +
                        $"• Huy hiệu thực hành Chương V: <color=#55FF88>Đã hoàn thành</color>\n" +
                        $"• {itemNotice}\n\n" +
                        $"<i>Lõi Năng Lượng Aurelia đã được tiếp thêm sức mạnh nhờ khả năng làm chủ các chuỗi chuyển hóa và điều độ lưới điện tái tạo của bạn!</i>";
            bTmp.fontSize = 16;
            bTmp.lineSpacing = 6;
            bTmp.color = Color.white;

            CreateButton(_successModal.transform, "Btn_Continue", new Vector2(-140, -175), new Vector2(220, 50), "TIẾP TỤC HỌC", () =>
            {
                _successModal.SetActive(false);
                Transform targetCanvas = _canvas != null ? _canvas.transform : null;
                bool core5CompleteBefore = StoryFlowController.IsCoreOfficiallyComplete(5) && !(PhysicsGameSession.Instance != null && PhysicsGameSession.Instance.SaveData != null && PhysicsGameSession.Instance.SaveData.hasSeenCore5RestoredStory);
                StoryFlowController.TryShowPostGameplayOrPendingStories(
                    completedChapterHint: 5,
                    customSave: null,
                    onAllFinished: () =>
                    {
                        if (PhysicsFinalProgression.DidNavigateFromFinalModal)
                        {
                            return;
                        }
                        if (core5CompleteBefore && PhysicsGameSession.Instance != null)
                        {
                            PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.ChapterSelection);
                        }
                        else
                        {
                            OnBackClicked();
                        }
                    },
                    canvasTransform: targetCanvas);
            }, true);

            CreateButton(_successModal.transform, "Btn_Replay", new Vector2(140, -175), new Vector2(200, 50), "LÀM LẠI", () =>
            {
                _successModal.SetActive(false);
                _currentChallengeIndex = 0;
                ResetCurrentChallenge(true);
            });

            _successModal.SetActive(true);
        }

        // =========================================================================
        // HINT & BACK
        // =========================================================================
        public void OnHintClicked()
        {
            if (IsStoryGateActive) return;
            string hint = "";
            if (_currentMode == NexusMode.EarthEnergyFossil)
            {
                if (_currentChallengeIndex == 0)
                    hint = "<b>GỢI Ý:</b> Năng lượng trong nhiên liệu hóa thạch có nguồn gốc gián tiếp từ năng lượng Mặt Trời qua thực vật và sinh vật cổ đại.";
                else if (_currentChallengeIndex == 1)
                    hint = "<b>GỢI Ý:</b> Nhà máy điện chuyển hóa năng lượng qua nhiều dạng: Hóa năng ➔ Nhiệt năng ➔ Cơ năng ➔ Điện năng.";
                else
                    hint = "<b>GỢI Ý:</b> Cấp quá ít nhiên liệu sẽ thiếu công suất (Low: 35), nhưng vận hành dư thừa (High: 100) gây lãng phí và ô nhiễm. Mức Trung bình (70) là tối ưu.";
            }
            else
            {
                if (_currentChallengeIndex == 0)
                    hint = "<b>GỢI Ý:</b> Pin mặt trời khai thác bức xạ ánh sáng, turbine gió khai thác động năng không khí, thủy điện khai thác dòng nước. Hãy kết nối cả 3 nguồn!";
                else if (_currentChallengeIndex == 1)
                    hint = "<b>GỢI Ý:</b> Trời nắng gắt chọn Pin mặt trời; Gió bão chọn Turbine gió; Mưa lũ lớn chọn Thủy điện để đạt công suất cao nhất.";
                else
                    hint = "<b>GỢI Ý:</b> Kết hợp nhiều nguồn khác nhau (ví dụ: 2 Solar + 2 Wind + 1 Hydro = 111.0 hoặc 1 Solar + 2 Wind + 2 Hydro = 115.0) để đạt trong khoảng 100 - 120 đơn vị.";
            }

            ShowFeedback(hint, ColorGold);
        }

        public void OnBackClicked()
        {
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.NavigateTo(RouteId.ChapterMap5);
            }
            else if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.GameplayEnergy);
            }
        }

        private void ShowFeedback(string message, Color color)
        {
            if (_txtFeedback == null) return;
            _txtFeedback.text = message;
            _txtFeedback.color = color;

            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(ResetFeedbackColorRoutine());
        }

        private IEnumerator ResetFeedbackColorRoutine()
        {
            yield return new WaitForSeconds(6.0f);
            if (_txtFeedback != null)
            {
                _txtFeedback.color = new Color(0.9f, 0.95f, 1f, 0.9f);
            }
        }

        private void PlaySoundSafe(bool isSuccess)
        {
            // Tích hợp âm thanh an toàn (Safe audio hook).
            // Không tạo AudioClip giả, không PlayOneShot(null).
        }

        // =========================================================================
        // XÂY DỰNG GIAO DIỆN (AURELIA NAVY & GOLD THEME)
        // =========================================================================
        private void BuildGameplayUIIfMissing()
        {
            Transform existing = _canvas.transform.Find("EnergyNexusHUD");
            if (existing != null)
            {
                _hudRoot = existing.gameObject;
                return;
            }

            _hudRoot = new GameObject("EnergyNexusHUD");
            _hudRoot.transform.SetParent(_canvas.transform, false);
            RectTransform hudRt = _hudRoot.AddComponent<RectTransform>();
            hudRt.anchorMin = Vector2.zero;
            hudRt.anchorMax = Vector2.one;
            hudRt.sizeDelta = Vector2.zero;

            BuildTopHeader();
            BuildLeftPanel();
            BuildExperimentWorkspace();
            BuildRightPanel();
            BuildBottomBar();
            BuildModals();
        }

        private void BuildTopHeader()
        {
            CreateButton(_hudRoot.transform, "Btn_Back", new Vector2(-860, 485), new Vector2(140, 45), "Quay lại", OnBackClicked);

            // Title Banner
            GameObject titleGo = new GameObject("TitleBanner");
            titleGo.transform.SetParent(_hudRoot.transform, false);
            RectTransform titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0, 485);
            titleRt.sizeDelta = new Vector2(900, 45);

            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "HỌC VIỆN AURELIA · TRUNG TÂM NĂNG LƯỢNG";
            titleTmp.fontSize = 26;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = ColorGold;

            // Lesson Badge
            GameObject badgeGo = new GameObject("LessonBadge");
            badgeGo.transform.SetParent(_hudRoot.transform, false);
            RectTransform bRt = badgeGo.AddComponent<RectTransform>();
            bRt.anchoredPosition = new Vector2(0, 445);
            bRt.sizeDelta = new Vector2(900, 30);

            _txtLessonBadge = badgeGo.AddComponent<TextMeshProUGUI>();
            _txtLessonBadge.text = "<b>C5_L16</b> · Vòng năng lượng trên Trái Đất. Năng lượng hóa thạch";
            _txtLessonBadge.fontSize = 17;
            _txtLessonBadge.alignment = TextAlignmentOptions.Center;
            _txtLessonBadge.color = Color.white;
        }

        private void BuildLeftPanel()
        {
            GameObject leftPanel = CreateNavyPanel(_hudRoot.transform, "LeftPanel", new Vector2(-650, 20), new Vector2(430, 780));

            // Mission Card
            GameObject missionCard = CreateNavyPanel(leftPanel.transform, "MissionCard", new Vector2(0, 260), new Vector2(390, 210));
            CreatePanelHeader(missionCard.transform, "NHIỆM VỤ NĂNG LƯỢNG", new Vector2(0, 80));

            GameObject counterGo = new GameObject("Counter");
            counterGo.transform.SetParent(missionCard.transform, false);
            RectTransform cRt = counterGo.AddComponent<RectTransform>();
            cRt.anchoredPosition = new Vector2(0, 52);
            cRt.sizeDelta = new Vector2(350, 25);
            _txtChallengeCounter = counterGo.AddComponent<TextMeshProUGUI>();
            _txtChallengeCounter.text = "Nhiệm vụ 1/3";
            _txtChallengeCounter.fontSize = 14;
            _txtChallengeCounter.alignment = TextAlignmentOptions.Center;
            _txtChallengeCounter.color = ColorGold;

            GameObject mTitleGo = new GameObject("MissionTitle");
            mTitleGo.transform.SetParent(missionCard.transform, false);
            RectTransform mtRt = mTitleGo.AddComponent<RectTransform>();
            mtRt.anchoredPosition = new Vector2(0, 22);
            mtRt.sizeDelta = new Vector2(360, 35);
            _txtMissionTitle = mTitleGo.AddComponent<TextMeshProUGUI>();
            _txtMissionTitle.fontSize = 16;
            _txtMissionTitle.fontStyle = FontStyles.Bold;
            _txtMissionTitle.alignment = TextAlignmentOptions.Center;
            _txtMissionTitle.color = ColorEnergyCyan;

            GameObject mDescGo = new GameObject("MissionDesc");
            mDescGo.transform.SetParent(missionCard.transform, false);
            RectTransform mdRt = mDescGo.AddComponent<RectTransform>();
            mdRt.anchoredPosition = new Vector2(0, -38);
            mdRt.sizeDelta = new Vector2(360, 75);
            _txtMissionDesc = mDescGo.AddComponent<TextMeshProUGUI>();
            _txtMissionDesc.fontSize = 13;
            _txtMissionDesc.alignment = TextAlignmentOptions.Center;
            _txtMissionDesc.color = new Color(0.9f, 0.95f, 1f, 0.9f);

            CreateButton(missionCard.transform, "Btn_PrevCh", new Vector2(-155, 52), new Vector2(40, 30), "◀", PrevChallenge);
            CreateButton(missionCard.transform, "Btn_NextCh", new Vector2(155, 52), new Vector2(40, 30), "▶", NextChallenge);

            // === MODE A CONTROLS ===
            _panelModeAControls = new GameObject("ModeAControls");
            _panelModeAControls.transform.SetParent(leftPanel.transform, false);
            RectTransform aRt = _panelModeAControls.AddComponent<RectTransform>();
            aRt.anchoredPosition = new Vector2(0, -90);
            aRt.sizeDelta = new Vector2(390, 440);

            CreatePanelHeader(_panelModeAControls.transform, "KHAY THẺ & ĐIỀU ĐỘ NHIÊN LIỆU", new Vector2(0, 190));

            // Khay thẻ Mode A (Dùng chung cho Sub 1 & Sub 2)
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                float y = 140f - (i * 48f);
                _btnOriginCards[i] = CreateButton(_panelModeAControls.transform, $"Btn_OriginCard_{i}", new Vector2(0, y), new Vector2(350, 40), ORIGIN_NAMES[i], () => SelectOriginCard(idx)).GetComponent<Button>();
            }

            // Sub 3 Fossil Selector
            GameObject fossilSelGo = CreateNavyPanel(_panelModeAControls.transform, "FossilSelectors", new Vector2(0, -130), new Vector2(370, 100));
            CreatePanelHeader(fossilSelGo.transform, "MỨC CẤP NHIÊN LIỆU HÓA THẠCH (SUB 3)", new Vector2(0, 32));
            _btnFossilLevelList.Clear();
            _btnFossilLevelList.Add(CreateButton(fossilSelGo.transform, "Btn_FossilLow", new Vector2(-110, -15), new Vector2(95, 38), "THẤP (35)", () => SelectFossilFuelLevel(FossilFuelLevel.Low)).GetComponent<Button>());
            _btnFossilLevelList.Add(CreateButton(fossilSelGo.transform, "Btn_FossilMed", new Vector2(0, -15), new Vector2(95, 38), "VỪA (70)", () => SelectFossilFuelLevel(FossilFuelLevel.Medium)).GetComponent<Button>());
            _btnFossilLevelList.Add(CreateButton(fossilSelGo.transform, "Btn_FossilHigh", new Vector2(110, -15), new Vector2(95, 38), "CAO (100)", () => SelectFossilFuelLevel(FossilFuelLevel.High)).GetComponent<Button>());

            // === MODE B CONTROLS ===
            _panelModeBControls = new GameObject("ModeBControls");
            _panelModeBControls.transform.SetParent(leftPanel.transform, false);
            RectTransform bRt2 = _panelModeBControls.AddComponent<RectTransform>();
            bRt2.anchoredPosition = new Vector2(0, -90);
            bRt2.sizeDelta = new Vector2(390, 440);

            CreatePanelHeader(_panelModeBControls.transform, "ĐIỀU KHIỂN NGUỒN TÁI TẠO", new Vector2(0, 190));

            // Sub 1: Nút kết nối 3 nguồn
            CreateButton(_panelModeBControls.transform, "Btn_ToggleSolar", new Vector2(-115, 135), new Vector2(105, 38), "PIN SOLAR", ToggleSolarConnection);
            CreateButton(_panelModeBControls.transform, "Btn_ToggleWind", new Vector2(0, 135), new Vector2(105, 38), "TURBINE GIÓ", ToggleWindConnection);
            CreateButton(_panelModeBControls.transform, "Btn_ToggleHydro", new Vector2(115, 135), new Vector2(105, 38), "THỦY ĐIỆN", ToggleHydroConnection);

            // Sub 2: Nút kịch bản thời tiết
            CreatePanelHeader(_panelModeBControls.transform, "KỊCH BẢN THỜI TIẾT (SUB 2)", new Vector2(0, 75));
            _btnWeatherScenarioList.Clear();
            _btnWeatherScenarioList.Add(CreateButton(_panelModeBControls.transform, "Btn_Scen0", new Vector2(-115, 25), new Vector2(105, 36), "1. NẮNG GẮT", () => SelectWeatherScenario(0)).GetComponent<Button>());
            _btnWeatherScenarioList.Add(CreateButton(_panelModeBControls.transform, "Btn_Scen1", new Vector2(0, 25), new Vector2(105, 36), "2. GIÓ LỚN", () => SelectWeatherScenario(1)).GetComponent<Button>());
            _btnWeatherScenarioList.Add(CreateButton(_panelModeBControls.transform, "Btn_Scen2", new Vector2(115, 25), new Vector2(105, 36), "3. MƯA LŨ", () => SelectWeatherScenario(2)).GetComponent<Button>());

            _btnWeatherSourceList.Clear();
            _btnWeatherSourceList.Add(CreateButton(_panelModeBControls.transform, "Btn_ActSolar", new Vector2(-115, -20), new Vector2(105, 36), "BẬT SOLAR", () => SelectMatchingSource(RenewableSourceType.Solar)).GetComponent<Button>());
            _btnWeatherSourceList.Add(CreateButton(_panelModeBControls.transform, "Btn_ActWind", new Vector2(0, -20), new Vector2(105, 36), "BẬT WIND", () => SelectMatchingSource(RenewableSourceType.Wind)).GetComponent<Button>());
            _btnWeatherSourceList.Add(CreateButton(_panelModeBControls.transform, "Btn_ActHydro", new Vector2(115, -20), new Vector2(105, 36), "BẬT HYDRO", () => SelectMatchingSource(RenewableSourceType.Hydro)).GetComponent<Button>());

            // Sub 3: Nút điều độ vi lưới
            CreatePanelHeader(_panelModeBControls.transform, "ĐIỀU ĐỘ VI LƯỚI TÁI TẠO (SUB 3)", new Vector2(0, -75));
            CreateButton(_panelModeBControls.transform, "Btn_SolarSub", new Vector2(-140, -125), new Vector2(35, 35), "-", () => AdjustModuleCount(RenewableSourceType.Solar, -1));
            CreateButton(_panelModeBControls.transform, "Btn_SolarAdd", new Vector2(-90, -125), new Vector2(35, 35), "+", () => AdjustModuleCount(RenewableSourceType.Solar, 1));

            CreateButton(_panelModeBControls.transform, "Btn_WindSub", new Vector2(-25, -125), new Vector2(35, 35), "-", () => AdjustModuleCount(RenewableSourceType.Wind, -1));
            CreateButton(_panelModeBControls.transform, "Btn_WindAdd", new Vector2(25, -125), new Vector2(35, 35), "+", () => AdjustModuleCount(RenewableSourceType.Wind, 1));

            CreateButton(_panelModeBControls.transform, "Btn_HydroSub", new Vector2(90, -125), new Vector2(35, 35), "-", () => AdjustModuleCount(RenewableSourceType.Hydro, -1));
            CreateButton(_panelModeBControls.transform, "Btn_HydroAdd", new Vector2(140, -125), new Vector2(35, 35), "+", () => AdjustModuleCount(RenewableSourceType.Hydro, 1));
        }

        private void BuildExperimentWorkspace()
        {
            GameObject boardGo = CreateNavyPanel(_hudRoot.transform, "ExperimentWorkspace", new Vector2(20, 70), new Vector2(860, 580));

            // Central Energy Core Visual
            GameObject coreGo = new GameObject("CentralEnergyCore");
            coreGo.transform.SetParent(boardGo.transform, false);
            RectTransform coreRt = coreGo.AddComponent<RectTransform>();
            coreRt.anchoredPosition = new Vector2(0, 160);
            coreRt.sizeDelta = new Vector2(120, 120);

            _energyCoreVisualImg = coreGo.AddComponent<Image>();
            _energyCoreVisualImg.color = ColorEnergyCyan;
            var coreOutline = coreGo.AddComponent<Outline>();
            coreOutline.effectColor = ColorGold;
            coreOutline.effectDistance = new Vector2(3, -3);

            GameObject cTxtGo = new GameObject("CoreLabel");
            cTxtGo.transform.SetParent(coreGo.transform, false);
            RectTransform ctRt = cTxtGo.AddComponent<RectTransform>();
            ctRt.sizeDelta = coreRt.sizeDelta;
            var ctTmp = cTxtGo.AddComponent<TextMeshProUGUI>();
            ctTmp.text = "ENERGY\nNEXUS\nLÕI NĂNG LƯỢNG";
            ctTmp.fontSize = 13;
            ctTmp.fontStyle = FontStyles.Bold;
            ctTmp.alignment = TextAlignmentOptions.Center;
            ctTmp.color = Color.white;

            // Workspace Header
            GameObject bHeader = new GameObject("WorkspaceHeader");
            bHeader.transform.SetParent(boardGo.transform, false);
            RectTransform bhRt = bHeader.AddComponent<RectTransform>();
            bhRt.anchoredPosition = new Vector2(0, 260);
            bhRt.sizeDelta = new Vector2(600, 30);
            var txtBoardStatus = bHeader.AddComponent<TextMeshProUGUI>();
            txtBoardStatus.text = "TRUNG TÂM NĂNG LƯỢNG AURELIA";
            txtBoardStatus.fontSize = 17;
            txtBoardStatus.fontStyle = FontStyles.Bold;
            txtBoardStatus.alignment = TextAlignmentOptions.Center;
            txtBoardStatus.color = ColorGold;

            // =========================================================================
            // WORKSPACE MODE A (EARTH ENERGY & FOSSIL LAB)
            // =========================================================================
            _panelModeAWorkspace = new GameObject("ModeAWorkspace");
            _panelModeAWorkspace.transform.SetParent(boardGo.transform, false);
            RectTransform maRt = _panelModeAWorkspace.AddComponent<RectTransform>();
            maRt.anchorMin = Vector2.zero;
            maRt.anchorMax = Vector2.one;
            maRt.sizeDelta = Vector2.zero;

            // Sub 1: Energy Origin Root
            _modeASub1Root = CreateNavyPanel(_panelModeAWorkspace.transform, "OriginSlotsContainer", new Vector2(0, -60), new Vector2(800, 240));
            CreatePanelHeader(_modeASub1Root.transform, "VÒNG NĂNG LƯỢNG MẶT TRỜI ĐẾN NHIÊN LIỆU HÓA THẠCH", new Vector2(0, 90));

            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                float x = -285f + (i * 190f);
                GameObject slotGo = CreateNavyPanel(_modeASub1Root.transform, $"OriginSlot_{i}", new Vector2(x, -10), new Vector2(170, 110));

                GameObject tagGo = new GameObject("Tag");
                tagGo.transform.SetParent(slotGo.transform, false);
                RectTransform tRt = tagGo.AddComponent<RectTransform>();
                tRt.anchoredPosition = new Vector2(0, 35);
                tRt.sizeDelta = new Vector2(150, 25);
                var tTmp = tagGo.AddComponent<TextMeshProUGUI>();
                tTmp.text = $"GIAI ĐOẠN {i + 1}";
                tTmp.fontSize = 12;
                tTmp.alignment = TextAlignmentOptions.Center;
                tTmp.color = ColorGold;

                GameObject valGo = new GameObject("Value");
                valGo.transform.SetParent(slotGo.transform, false);
                RectTransform vRt = valGo.AddComponent<RectTransform>();
                vRt.anchoredPosition = new Vector2(0, -10);
                vRt.sizeDelta = new Vector2(160, 60);
                _txtOriginSlots[i] = valGo.AddComponent<TextMeshProUGUI>();
                _txtOriginSlots[i].fontSize = 14;
                _txtOriginSlots[i].fontStyle = FontStyles.Bold;
                _txtOriginSlots[i].alignment = TextAlignmentOptions.Center;

                Button btnSlot = slotGo.AddComponent<Button>();
                btnSlot.onClick.AddListener(() => PlaceOriginSlot(idx));
            }

            // Sub 2: Transformation Chain Root
            _modeASub2Root = CreateNavyPanel(_panelModeAWorkspace.transform, "TransSlotsContainer", new Vector2(0, -60), new Vector2(800, 240));
            CreatePanelHeader(_modeASub2Root.transform, "CHUỖI CHUYỂN HÓA NĂNG LƯỢNG TRONG NHÀ MÁY NHIỆT ĐIỆN", new Vector2(0, 90));

            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                float x = -285f + (i * 190f);
                GameObject slotGo = CreateNavyPanel(_modeASub2Root.transform, $"TransSlot_{i}", new Vector2(x, -10), new Vector2(170, 110));

                GameObject tagGo = new GameObject("Tag");
                tagGo.transform.SetParent(slotGo.transform, false);
                RectTransform tRt = tagGo.AddComponent<RectTransform>();
                tRt.anchoredPosition = new Vector2(0, 35);
                tRt.sizeDelta = new Vector2(150, 25);
                var tTmp = tagGo.AddComponent<TextMeshProUGUI>();
                tTmp.text = $"BƯỚC {i + 1}";
                tTmp.fontSize = 12;
                tTmp.alignment = TextAlignmentOptions.Center;
                tTmp.color = ColorEnergyCyan;

                GameObject valGo = new GameObject("Value");
                valGo.transform.SetParent(slotGo.transform, false);
                RectTransform vRt = valGo.AddComponent<RectTransform>();
                vRt.anchoredPosition = new Vector2(0, -10);
                vRt.sizeDelta = new Vector2(160, 60);
                _txtTransSlots[i] = valGo.AddComponent<TextMeshProUGUI>();
                _txtTransSlots[i].fontSize = 14;
                _txtTransSlots[i].fontStyle = FontStyles.Bold;
                _txtTransSlots[i].alignment = TextAlignmentOptions.Center;

                Button btnSlot = slotGo.AddComponent<Button>();
                btnSlot.onClick.AddListener(() => PlaceTransSlot(idx));
            }

            // Sub 3: Fossil Management Root
            _modeASub3Root = CreateNavyPanel(_panelModeAWorkspace.transform, "FossilManagementContainer", new Vector2(0, -60), new Vector2(800, 240));
            CreatePanelHeader(_modeASub3Root.transform, "ĐIỀU ĐỘ NHÀ MÁY NHIỆT ĐIỆN & TÁC ĐỘNG MÔI TRƯỜNG", new Vector2(0, 90));

            GameObject foTxtGo = new GameObject("FossilOutputText");
            foTxtGo.transform.SetParent(_modeASub3Root.transform, false);
            RectTransform foRt = foTxtGo.AddComponent<RectTransform>();
            foRt.anchoredPosition = new Vector2(0, 35);
            foRt.sizeDelta = new Vector2(600, 35);
            _txtFossilOutputVal = foTxtGo.AddComponent<TextMeshProUGUI>();
            _txtFossilOutputVal.fontSize = 18;
            _txtFossilOutputVal.fontStyle = FontStyles.Bold;
            _txtFossilOutputVal.alignment = TextAlignmentOptions.Center;
            _txtFossilOutputVal.color = ColorGold;

            GameObject fiTxtGo = new GameObject("FossilImpactText");
            fiTxtGo.transform.SetParent(_modeASub3Root.transform, false);
            RectTransform fiRt = fiTxtGo.AddComponent<RectTransform>();
            fiRt.anchoredPosition = new Vector2(0, -5);
            fiRt.sizeDelta = new Vector2(600, 30);
            _txtFossilImpactVal = fiTxtGo.AddComponent<TextMeshProUGUI>();
            _txtFossilImpactVal.fontSize = 15;
            _txtFossilImpactVal.alignment = TextAlignmentOptions.Center;
            _txtFossilImpactVal.color = ColorFossilAmber;

            // Impact Progress Bar
            GameObject ibContainer = CreateNavyPanel(_modeASub3Root.transform, "ImpactBarContainer", new Vector2(0, -50), new Vector2(480, 22));
            GameObject ibFill = new GameObject("Fill");
            ibFill.transform.SetParent(ibContainer.transform, false);
            RectTransform ibfRt = ibFill.AddComponent<RectTransform>();
            ibfRt.anchorMin = Vector2.zero;
            ibfRt.anchorMax = Vector2.one;
            ibfRt.sizeDelta = Vector2.zero;
            _imgFossilImpactBar = ibFill.AddComponent<Image>();
            _imgFossilImpactBar.type = Image.Type.Filled;
            _imgFossilImpactBar.fillMethod = Image.FillMethod.Horizontal;
            _imgFossilImpactBar.fillAmount = 0.2f;
            _imgFossilImpactBar.color = ColorAlertRed;

            // =========================================================================
            // WORKSPACE MODE B (RENEWABLE GRID LAB)
            // =========================================================================
            _panelModeBWorkspace = new GameObject("ModeBWorkspace");
            _panelModeBWorkspace.transform.SetParent(boardGo.transform, false);
            RectTransform mbRt = _panelModeBWorkspace.AddComponent<RectTransform>();
            mbRt.anchorMin = Vector2.zero;
            mbRt.anchorMax = Vector2.one;
            mbRt.sizeDelta = Vector2.zero;

            // Sub 1: Renewable Sources Lab
            _modeBSub1Root = CreateNavyPanel(_panelModeBWorkspace.transform, "RenewableSourcesContainer", new Vector2(0, -60), new Vector2(800, 240));
            CreatePanelHeader(_modeBSub1Root.transform, "KẾT NỐI BA NGUỒN NĂNG LƯỢNG TÁI TẠO", new Vector2(0, 90));

            GameObject sCard = CreateNavyPanel(_modeBSub1Root.transform, "SolarCard", new Vector2(-240, -10), new Vector2(210, 120));
            CreatePanelHeader(sCard.transform, "PIN MẶT TRỜI", new Vector2(0, 40));
            GameObject sStatGo = new GameObject("Stat");
            sStatGo.transform.SetParent(sCard.transform, false);
            RectTransform ssRt = sStatGo.AddComponent<RectTransform>();
            ssRt.anchoredPosition = new Vector2(0, -15);
            ssRt.sizeDelta = new Vector2(180, 40);
            _imgSolarConnectStatus = sStatGo.AddComponent<Image>();
            _imgSolarConnectStatus.color = Color.gray;

            GameObject wCard = CreateNavyPanel(_modeBSub1Root.transform, "WindCard", new Vector2(0, -10), new Vector2(210, 120));
            CreatePanelHeader(wCard.transform, "TURBINE GIÓ", new Vector2(0, 40));
            GameObject wStatGo = new GameObject("Stat");
            wStatGo.transform.SetParent(wCard.transform, false);
            RectTransform wsRt = wStatGo.AddComponent<RectTransform>();
            wsRt.anchoredPosition = new Vector2(0, -15);
            wsRt.sizeDelta = new Vector2(180, 40);
            _imgWindConnectStatus = wStatGo.AddComponent<Image>();
            _imgWindConnectStatus.color = Color.gray;

            GameObject hCard = CreateNavyPanel(_modeBSub1Root.transform, "HydroCard", new Vector2(240, -10), new Vector2(210, 120));
            CreatePanelHeader(hCard.transform, "THỦY ĐIỆN", new Vector2(0, 40));
            GameObject hStatGo = new GameObject("Stat");
            hStatGo.transform.SetParent(hCard.transform, false);
            RectTransform hsRt = hStatGo.AddComponent<RectTransform>();
            hsRt.anchoredPosition = new Vector2(0, -15);
            hsRt.sizeDelta = new Vector2(180, 40);
            _imgHydroConnectStatus = hStatGo.AddComponent<Image>();
            _imgHydroConnectStatus.color = Color.gray;

            // Sub 2: Weather Matching
            _modeBSub2Root = CreateNavyPanel(_panelModeBWorkspace.transform, "WeatherMatchingContainer", new Vector2(0, -60), new Vector2(800, 240));
            CreatePanelHeader(_modeBSub2Root.transform, "TRẠM DỰ BÁO & ĐIỀU ĐỘ THỜI TIẾT", new Vector2(0, 90));

            GameObject wcGo = new GameObject("WeatherCondText");
            wcGo.transform.SetParent(_modeBSub2Root.transform, false);
            RectTransform wcRt = wcGo.AddComponent<RectTransform>();
            wcRt.anchoredPosition = new Vector2(0, 30);
            wcRt.sizeDelta = new Vector2(700, 35);
            _txtWeatherCondition = wcGo.AddComponent<TextMeshProUGUI>();
            _txtWeatherCondition.fontSize = 16;
            _txtWeatherCondition.fontStyle = FontStyles.Bold;
            _txtWeatherCondition.alignment = TextAlignmentOptions.Center;
            _txtWeatherCondition.color = ColorGold;

            GameObject woGo = new GameObject("WeatherOutputText");
            woGo.transform.SetParent(_modeBSub2Root.transform, false);
            RectTransform woRt = woGo.AddComponent<RectTransform>();
            woRt.anchoredPosition = new Vector2(0, -25);
            woRt.sizeDelta = new Vector2(700, 35);
            _txtWeatherOutputVal = woGo.AddComponent<TextMeshProUGUI>();
            _txtWeatherOutputVal.fontSize = 17;
            _txtWeatherOutputVal.alignment = TextAlignmentOptions.Center;
            _txtWeatherOutputVal.color = ColorRenewableGreen;

            // Sub 3: Microgrid Balance
            _modeBSub3Root = CreateNavyPanel(_panelModeBWorkspace.transform, "MicrogridContainer", new Vector2(0, -60), new Vector2(800, 240));
            CreatePanelHeader(_modeBSub3Root.transform, "ĐIỀU ĐỘ VI LƯỚI TÁI TẠO (DEMAND = 100)", new Vector2(0, 90));

            GameObject mgcGo = new GameObject("CountersText");
            mgcGo.transform.SetParent(_modeBSub3Root.transform, false);
            RectTransform mgcRt = mgcGo.AddComponent<RectTransform>();
            mgcRt.anchoredPosition = new Vector2(0, 40);
            mgcRt.sizeDelta = new Vector2(700, 35);
            _txtMicrogridCounters = mgcGo.AddComponent<TextMeshProUGUI>();
            _txtMicrogridCounters.fontSize = 16;
            _txtMicrogridCounters.alignment = TextAlignmentOptions.Center;
            _txtMicrogridCounters.color = Color.white;

            // Power balance bar
            GameObject pbContainer = CreateNavyPanel(_modeBSub3Root.transform, "PowerBalanceContainer", new Vector2(0, -10), new Vector2(520, 24));
            GameObject pbFill = new GameObject("Fill");
            pbFill.transform.SetParent(pbContainer.transform, false);
            RectTransform pbfRt = pbFill.AddComponent<RectTransform>();
            pbfRt.anchorMin = Vector2.zero;
            pbfRt.anchorMax = Vector2.one;
            pbfRt.sizeDelta = Vector2.zero;
            _imgPowerBalanceBar = pbFill.AddComponent<Image>();
            _imgPowerBalanceBar.type = Image.Type.Filled;
            _imgPowerBalanceBar.fillMethod = Image.FillMethod.Horizontal;
            _imgPowerBalanceBar.fillAmount = 0.5f;
            _imgPowerBalanceBar.color = ColorRenewableGreen;

            GameObject msdGo = new GameObject("SupplyDemandText");
            msdGo.transform.SetParent(_modeBSub3Root.transform, false);
            RectTransform msdRt = msdGo.AddComponent<RectTransform>();
            msdRt.anchoredPosition = new Vector2(0, -60);
            msdRt.sizeDelta = new Vector2(700, 45);
            _txtMicrogridSupplyDemand = msdGo.AddComponent<TextMeshProUGUI>();
            _txtMicrogridSupplyDemand.fontSize = 15;
            _txtMicrogridSupplyDemand.fontStyle = FontStyles.Bold;
            _txtMicrogridSupplyDemand.alignment = TextAlignmentOptions.Center;
            _txtMicrogridSupplyDemand.color = ColorEnergyCyan;

            _panelModeBWorkspace.SetActive(false);
        }

        private void BuildRightPanel()
        {
            GameObject rightPanel = CreateNavyPanel(_hudRoot.transform, "RightPanel", new Vector2(670, 20), new Vector2(400, 780));
            CreatePanelHeader(rightPanel.transform, "THÔNG SỐ LÕI NĂNG LƯỢNG", new Vector2(0, 345));

            // Main Readout Card
            GameObject mainReadoutCard = CreateNavyPanel(rightPanel.transform, "MainReadoutCard", new Vector2(0, 210), new Vector2(360, 210));
            GameObject mrTxtGo = new GameObject("MainText");
            mrTxtGo.transform.SetParent(mainReadoutCard.transform, false);
            RectTransform mrRt = mrTxtGo.AddComponent<RectTransform>();
            mrRt.sizeDelta = new Vector2(330, 180);
            _txtReadoutMain = mrTxtGo.AddComponent<TextMeshProUGUI>();
            _txtReadoutMain.fontSize = 14;
            _txtReadoutMain.lineSpacing = 8;
            _txtReadoutMain.alignment = TextAlignmentOptions.Left;
            _txtReadoutMain.color = Color.white;

            // Secondary Readout Card
            GameObject secReadoutCard = CreateNavyPanel(rightPanel.transform, "SecReadoutCard", new Vector2(0, -30), new Vector2(360, 210));
            GameObject srTxtGo = new GameObject("SecText");
            srTxtGo.transform.SetParent(secReadoutCard.transform, false);
            RectTransform srRt = srTxtGo.AddComponent<RectTransform>();
            srRt.sizeDelta = new Vector2(330, 180);
            _txtReadoutSecondary = srTxtGo.AddComponent<TextMeshProUGUI>();
            _txtReadoutSecondary.fontSize = 13;
            _txtReadoutSecondary.lineSpacing = 8;
            _txtReadoutSecondary.alignment = TextAlignmentOptions.Left;
            _txtReadoutSecondary.color = Color.white;

            // Formula Card
            GameObject formulaCard = CreateNavyPanel(rightPanel.transform, "FormulaCard", new Vector2(0, -250), new Vector2(360, 160));
            CreatePanelHeader(formulaCard.transform, "NGUYÊN LÝ VẬT LÝ", new Vector2(0, 55));

            GameObject fTxtGo = new GameObject("FormulaText");
            fTxtGo.transform.SetParent(formulaCard.transform, false);
            RectTransform fRt = fTxtGo.AddComponent<RectTransform>();
            fRt.anchoredPosition = new Vector2(0, -20);
            fRt.sizeDelta = new Vector2(330, 95);
            _txtReadoutFormula = fTxtGo.AddComponent<TextMeshProUGUI>();
            _txtReadoutFormula.fontSize = 13;
            _txtReadoutFormula.alignment = TextAlignmentOptions.Center;
            _txtReadoutFormula.color = ColorGold;
        }

        private void BuildBottomBar()
        {
            GameObject fbGo = new GameObject("FeedbackText");
            fbGo.transform.SetParent(_hudRoot.transform, false);
            RectTransform fbRt = fbGo.AddComponent<RectTransform>();
            fbRt.anchoredPosition = new Vector2(20, -265);
            fbRt.sizeDelta = new Vector2(850, 40);

            _txtFeedback = fbGo.AddComponent<TextMeshProUGUI>();
            _txtFeedback.text = "Chào mừng tới Trung tâm Năng lượng Aurelia. Hãy thực hiện thao tác theo nhiệm vụ!";
            _txtFeedback.fontSize = 16;
            _txtFeedback.fontStyle = FontStyles.Bold;
            _txtFeedback.alignment = TextAlignmentOptions.Center;
            _txtFeedback.color = new Color(0.9f, 0.95f, 1f, 0.9f);

            CreateButton(_hudRoot.transform, "Btn_Hint", new Vector2(-200, -320), new Vector2(170, 50), "GỢI Ý", OnHintClicked);
            CreateButton(_hudRoot.transform, "Btn_Reset", new Vector2(0, -320), new Vector2(170, 50), "LÀM LẠI", () => ResetCurrentChallenge(true));
            CreateButton(_hudRoot.transform, "Btn_Check", new Vector2(200, -320), new Vector2(170, 50), "KIỂM TRA", OnCheckClicked, true);
        }

        private void BuildModals()
        {
            // 1. TUTORIAL MODAL
            _tutorialModal = CreateNavyPanel(_hudRoot.transform, "TutorialModal", Vector2.zero, new Vector2(720, 480));
            CreatePanelHeader(_tutorialModal.transform, "HƯỚNG DẪN · TRUNG TÂM NĂNG LƯỢNG AURELIA", new Vector2(0, 180));

            GameObject tutBodyGo = new GameObject("TutorialBody");
            tutBodyGo.transform.SetParent(_tutorialModal.transform, false);
            RectTransform tbRt = tutBodyGo.AddComponent<RectTransform>();
            tbRt.anchoredPosition = new Vector2(0, 15);
            tbRt.sizeDelta = new Vector2(620, 260);

            var tbTmp = tutBodyGo.AddComponent<TextMeshProUGUI>();
            tbTmp.text = "<b>Chào mừng pháp sư đến với Trung Tâm Năng Lượng Aurelia!</b>\n\n" +
                         "1. <b>Xây dựng & vận hành các nguồn năng lượng:</b> Sắp xếp các vòng biến đổi năng lượng hoặc thiết lập các nguồn tái tạo.\n" +
                         "2. <b>Theo dõi quá trình chuyển hóa:</b> Quan sát sự biến đổi từ quang năng, hóa năng, nhiệt năng sang cơ năng và điện năng.\n" +
                         "3. <b>Cân bằng nguồn cung và nhu cầu:</b> Điều độ nguồn phát điện phù hợp với tải tiêu thụ của học viện.\n" +
                         "4. <b>Bấm KIỂM TRA:</b> Hoàn thành trọn vẹn các thử thách để nhận trang bị quý giá của Chương V!";
            tbTmp.fontSize = 16;
            tbTmp.lineSpacing = 8;
            tbTmp.color = Color.white;

            CreateButton(_tutorialModal.transform, "Btn_Understood", new Vector2(0, -170), new Vector2(220, 50), "ĐÃ HIỂU", OnTutorialClosed, true);
            _tutorialModal.SetActive(false);

            // 2. SUCCESS MODAL
            _successModal = CreateNavyPanel(_hudRoot.transform, "SuccessModal", Vector2.zero, new Vector2(740, 500));
            _successModal.SetActive(false);
        }

        private void OnTutorialClosed()
        {
            var save = PhysicsSaveManager.Load() ?? new PhysicsSaveData();
            save.hasSeenEnergyTutorial = true;
            PhysicsSaveManager.Save(save);

            if (_tutorialModal != null) _tutorialModal.SetActive(false);
        }

        private void UpdateHighlightButtons()
        {
            // Mode A Fossil buttons
            for (int i = 0; i < _btnFossilLevelList.Count; i++)
            {
                var img = _btnFossilLevelList[i].GetComponent<Image>();
                if (img != null)
                {
                    bool isSel = (i == (int)_fossilFuelLevel);
                    img.color = isSel ? ColorBtnSelected : ColorBtnNormal;
                }
            }

            // Mode B Weather Scenario buttons
            int maxAllowedScen = _completedWeatherScenarios.Count;
            for (int i = 0; i < _btnWeatherScenarioList.Count; i++)
            {
                var btn = _btnWeatherScenarioList[i];
                if (btn != null)
                {
                    btn.interactable = (i <= maxAllowedScen);
                }
                var img = btn != null ? btn.GetComponent<Image>() : null;
                if (img != null)
                {
                    bool isSel = (i == _weatherScenarioIdx);
                    bool isDone = _completedWeatherScenarios.Contains(i);
                    img.color = isSel ? ColorBtnSelected : (isDone ? new Color(0.12f, 0.38f, 0.22f, 0.95f) : (i <= maxAllowedScen ? ColorBtnNormal : new Color(0.1f, 0.12f, 0.18f, 0.5f)));
                }
            }

            // Mode B Weather Source buttons
            for (int i = 0; i < _btnWeatherSourceList.Count; i++)
            {
                var img = _btnWeatherSourceList[i].GetComponent<Image>();
                if (img != null)
                {
                    bool isSel = _isMatchingSourceActive && (i == (int)_selectedMatchingSource);
                    img.color = isSel ? ColorRenewableGreen : ColorBtnNormal;
                }
            }
        }

        // === TEST HELPER HOOKS ===
        public void SetModeForTest(NexusMode mode, string lessonId)
        {
            _currentMode = mode;
            _lessonId = lessonId;
            _currentChallengeIndex = 0;
            _completedSubChallengesA.Clear();
            _completedSubChallengesB.Clear();
            _completedWeatherScenarios.Clear();
            _isCompleted = false;
        }

        public void SetSubChallengeIndexForTest(int idx)
        {
            _currentChallengeIndex = idx;
        }

        public void SetRenewableSourcesForTest(bool solar, bool wind, bool hydro)
        {
            _isSolarConnected = solar;
            _isWindConnected = wind;
            _isHydroConnected = hydro;
        }

        public void SetWeatherScenarioIndexForTest(int idx)
        {
            _weatherScenarioIdx = idx;
        }

        public void SetMatchingSourceForTest(RenewableSourceType src, bool active)
        {
            _selectedMatchingSource = src;
            _isMatchingSourceActive = active;
        }

        public void AddCompletedWeatherScenarioForTest(int idx)
        {
            _completedWeatherScenarios.Add(idx);
        }

        public void AddCompletedSubChallengeBForTest(int idx)
        {
            _completedSubChallengesB.Add(idx);
        }

        public void SetMicrogridModulesForTest(int solar, int wind, int hydro)
        {
            _solarModuleCount = solar;
            _windModuleCount = wind;
            _hydroModuleCount = hydro;
        }

        // =========================================================================
        // UI HELPERS (AURELIA ACADEMY STYLING)
        // =========================================================================
        private GameObject CreateNavyPanel(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.color = ColorNavyBg;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = ColorGoldBorder;
            outline.effectDistance = new Vector2(2, -2);

            return go;
        }

        private void CreatePanelHeader(Transform parent, string text, Vector2 pos)
        {
            GameObject go = new GameObject("Header_" + text);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(500, 32);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 15;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = ColorGold;
        }

        private GameObject CreateButton(
            Transform parent,
            string name,
            Vector2 pos,
            Vector2 size,
            string label,
            Action onClick,
            bool isGold = false)
        {
            GameObject btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);

            RectTransform rt = btnGo.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = btnGo.AddComponent<Image>();
            img.color = isGold ? ColorGold : ColorBtnNormal;

            Button btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;

            var outline = btnGo.AddComponent<Outline>();
            outline.effectColor = isGold ? new Color(1f, 0.95f, 0.7f, 0.8f) : ColorGoldBorder;
            outline.effectDistance = new Vector2(1, -1);

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            RectTransform tRt = textGo.AddComponent<RectTransform>();
            tRt.sizeDelta = size;

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = isGold ? 17 : 13;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = isGold ? new Color(0.08f, 0.12f, 0.24f, 1f) : Color.white;

            btn.onClick.AddListener(() => onClick?.Invoke());
            btnGo.AddComponent<UIButtonFeedback>();

            return btnGo;
        }
    }
}
