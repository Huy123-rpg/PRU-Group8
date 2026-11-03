using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Mini-game Chương IV: Điện từ – PHÒNG CẢM ỨNG AURELIA (AURELIA INDUCTION LAB).
    /// Hỗ trợ 2 Gameplay Mode tương ứng với 2 bài học:
    /// - MODE A (Induction Lab): C4_L14 (Cảm ứng điện từ. Nguyên tắc tạo ra dòng điện xoay chiều).
    /// - MODE B (AC Application Lab): C4_L15 (Tác dụng của dòng điện xoay chiều & Máy biến áp).
    /// </summary>
    public class ElectromagnetismLabGame : MonoBehaviour
    {
        public enum LabMode
        {
            InductionLab,     // C4_L14
            ACApplicationLab  // C4_L15
        }

        public enum ACDeviceType
        {
            Heater,        // Tác dụng nhiệt
            Electromagnet, // Tác dụng từ
            Lamp,          // Tác dụng quang
            Transformer    // Máy biến áp
        }

        // === PARAMETERS & LESSON STATE ===
        private string _lessonId = "C4_L14";
        private string _lessonTitle = "Cảm ứng điện từ. Dòng điện xoay chiều";
        private LabMode _currentMode = LabMode.InductionLab;
        private int _currentChallengeIndex = 0;

        // Common State
        private bool _isSwitchOn = false;
        private bool _isObjectiveMet = false;
        private bool _isCompleted = false;
        private bool _storyGateActive = false;
        private bool _enteredViaNormalFlow = false;
        public bool IsStoryGateActive => _storyGateActive || StoryFlowController.IsStoryModalOpen;

        // === MODE A: INDUCTION LAB STATE ===
        private float _magnetPosX = -200f; // Vị trí nam châm (px), Coil tại X = 60px
        private const float COIL_POS_X = 60f;
        private float _prevMagnetPosX = -200f;
        private float _previousFlux = 0f; // Từ thông qua cuộn dây ở frame trước (Faraday/Lenz proxy)
        private int _poleDirection = 1; // +1: N (Bắc) hướng vào coil, -1: S (Nam) hướng vào coil
        private float _inducedCurrent = 0f; // Normalized -1.0f đến +1.0f
        private float _galvanometerAngle = 0f; // Góc kim điện kế (-55° đến +55°)
        private bool _isDraggingMagnet = false;

        // Mode A Objectives tracking
        private bool _hasObservedInduction = false;
        private bool _hasPositiveDeflection = false;
        private bool _hasNegativeDeflection = false;
        private bool _hasGeneratedAC = false;
        private readonly HashSet<int> _completedSubChallengesA = new HashSet<int>();

        // AC Generator State (Challenge 3 of C4_L14)
        private float _rotorAngle = 0f;
        private float _generatorSpeed = 0f; // Vòng/giây
        private readonly List<Image> _waveformPool = new List<Image>();
        private const int WAVEFORM_POINTS = 28;

        // === MODE B: AC APPLICATION STATE ===
        private ACDeviceType _selectedDevice = ACDeviceType.Heater;
        private float _deviceActiveProgress = 0f; // 0.0f đến 1.0f (Nhiệt độ / Cường độ từ / Độ sáng)
        private readonly HashSet<int> _completedSubChallengesB = new HashSet<int>();

        // Transformer Parameters (Mini Challenge 4 of C4_L15)
        private const float U1_VOLTAGE = 220f;
        private const float N1_TURNS = 1000f;
        private readonly float[] _n2Options = { 100f, 500f, 2000f };
        private int _selectedN2Idx = 0; // Default 100 vòng -> U2 = 22V (FAIL)

        // === UI ROOT & REFERENCES ===
        private Canvas _canvas;
        private GameObject _hudRoot;
        private GameObject _tutorialModal;
        private GameObject _successModal;

        // Header & Mission
        private TextMeshProUGUI _txtLessonBadge;
        private TextMeshProUGUI _txtMissionTitle;
        private TextMeshProUGUI _txtMissionDesc;
        private TextMeshProUGUI _txtChallengeCounter;

        // Readout & Feedback
        private TextMeshProUGUI _txtReadoutMain;
        private TextMeshProUGUI _txtReadoutSecondary;
        private TextMeshProUGUI _txtReadoutFormula;
        private TextMeshProUGUI _txtFeedback;
        private Coroutine _feedbackCoroutine;

        // Mode A Visual Elements
        private GameObject _panelModeAWorkspace;
        private RectTransform _magnetRt;
        private TextMeshProUGUI _txtMagnetPole;
        private Image _magnetPoleImg;
        private RectTransform _coilRt;
        private RectTransform _needleRt;
        private TextMeshProUGUI _txtInductionLevel;
        private GameObject _generatorRoot;
        private RectTransform _rotorVisualRt;
        private Transform _waveformContainer;

        // Mode B Visual Elements
        private GameObject _panelModeBWorkspace;
        private TextMeshProUGUI _txtDeviceStateHeader;
        private Image _deviceProgressBar;
        private TextMeshProUGUI _txtDeviceEffectValue;
        private Image _deviceVisualEffectImg;
        private GameObject _transformerControlsGo;
        private readonly List<Button> _btnN2Selectors = new List<Button>();
        private readonly List<Button> _btnDevicePickers = new List<Button>();

        // Dynamic Mode Panels
        private GameObject _panelModeAControls;
        private GameObject _panelModeBControls;

        // Colors (Aurelia Academy Theme)
        private static readonly Color ColorNavyBg = new Color(0.06f, 0.11f, 0.22f, 0.94f);
        private static readonly Color ColorGold = new Color(0.96f, 0.82f, 0.38f, 1f);
        private static readonly Color ColorGoldBorder = new Color(0.85f, 0.70f, 0.28f, 0.85f);
        private static readonly Color ColorElectricalCyan = new Color(0.00f, 0.90f, 1.00f, 1f);
        private static readonly Color ColorMagPurple = new Color(0.70f, 0.35f, 1.00f, 1f);
        private static readonly Color ColorPoleNorth = new Color(0.95f, 0.25f, 0.25f, 1f); // N (Đỏ)
        private static readonly Color ColorPoleSouth = new Color(0.20f, 0.50f, 0.95f, 1f); // S (Xanh)
        private static readonly Color ColorSuccessGreen = new Color(0.30f, 0.95f, 0.45f, 1f);
        private static readonly Color ColorAlertRed = new Color(1.0f, 0.42f, 0.40f, 1f);
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
            if (_enteredViaNormalFlow && save != null && !save.hasSeenCore4Intro)
            {
                if (_tutorialModal != null) _tutorialModal.SetActive(false);
                SetStoryGate(true);
                StoryFlowController.TryShowGameplayCoreIntro(
                    4,
                    customSave: null,
                    onIntroFinishedOrSkipped: () =>
                    {
                        SetStoryGate(false);
                        var updatedSave = PhysicsSaveManager.Load();
                        bool shouldShowTutorial = updatedSave == null || !updatedSave.hasSeenElectromagnetismTutorial;
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
                bool shouldShowTutorial = save == null || !save.hasSeenElectromagnetismTutorial;
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

            float dt = Mathf.Max(Time.unscaledDeltaTime, 0.001f);

            if (_currentMode == LabMode.InductionLab)
            {
                UpdateInductionLabFrame(dt);
            }
            else if (_currentMode == LabMode.ACApplicationLab)
            {
                UpdateACApplicationFrame(dt);
            }
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }

        private void EnsureSessionFallback()
        {
            if (PhysicsGameSession.Instance != null)
            {
                string id = PhysicsGameSession.Instance.SelectedLessonId;
                if (string.IsNullOrEmpty(id) || !id.StartsWith("C4_"))
                {
                    PhysicsGameSession.Instance.SelectChapter(4);
                    PhysicsGameSession.Instance.SelectLesson("C4_L14", "Cảm ứng điện từ. Nguyên tắc tạo ra dòng điện xoay chiều");
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
                _lessonId = "C4_L14";
                _lessonTitle = "Cảm ứng điện từ. Nguyên tắc tạo ra dòng điện xoay chiều";
            }

            switch (_lessonId)
            {
                case "C4_L15":
                    _currentMode = LabMode.ACApplicationLab;
                    break;
                case "C4_L14":
                default:
                    _currentMode = LabMode.InductionLab;
                    break;
            }

            _currentChallengeIndex = 0;
            _completedSubChallengesA.Clear();
            _completedSubChallengesB.Clear();

            float dist = Mathf.Abs(_magnetPosX - COIL_POS_X);
            float proximity = 1f - Mathf.Clamp01(dist / 360f);
            _previousFlux = _poleDirection * proximity;

            ConfigureModePanels();
        }

        private void ConfigureModePanels()
        {
            if (_panelModeAControls != null) _panelModeAControls.SetActive(_currentMode == LabMode.InductionLab);
            if (_panelModeBControls != null) _panelModeBControls.SetActive(_currentMode == LabMode.ACApplicationLab);
            if (_panelModeAWorkspace != null) _panelModeAWorkspace.SetActive(_currentMode == LabMode.InductionLab);
            if (_panelModeBWorkspace != null) _panelModeBWorkspace.SetActive(_currentMode == LabMode.ACApplicationLab);

            if (_txtLessonBadge != null)
            {
                string modeTag = _currentMode == LabMode.InductionLab ? "PHÒNG CẢM ỨNG ĐIỆN TỪ" : "TRẠM ỨNG DỤNG AC";
                _txtLessonBadge.text = $"<b>{_lessonId}</b> · {_lessonTitle}  [<color=#00E5FF>{modeTag}</color>]";
            }
        }

        // =========================================================================
        // RESET & SWITCH TOGGLE
        // =========================================================================
        public void ResetCurrentChallenge(bool showFeedback = true)
        {
            if (showFeedback && IsStoryGateActive) return;
            _isCompleted = false;
            _isSwitchOn = false;
            _isObjectiveMet = false;

            if (_currentMode == LabMode.InductionLab)
            {
                _magnetPosX = -220f;
                _prevMagnetPosX = -220f;
                _inducedCurrent = 0f;
                _galvanometerAngle = 0f;

                float dist = Mathf.Abs(_magnetPosX - COIL_POS_X);
                float proximity = 1f - Mathf.Clamp01(dist / 360f);
                _previousFlux = _poleDirection * proximity;

                _hasObservedInduction = false;
                _hasPositiveDeflection = false;
                _hasNegativeDeflection = false;
                _hasGeneratedAC = false;
                _generatorSpeed = 0f;
                _rotorAngle = 0f;

                if (_magnetRt != null) _magnetRt.anchoredPosition = new Vector2(_magnetPosX, 0f);
            }
            else if (_currentMode == LabMode.ACApplicationLab)
            {
                _selectedDevice = (_currentChallengeIndex == 3) ? ACDeviceType.Transformer : (ACDeviceType)_currentChallengeIndex;
                _deviceActiveProgress = 0f;
                _selectedN2Idx = 0; // 100 vòng (U2 = 22V -> Fail)
            }

            UpdatePhysicsAndVisuals();

            if (showFeedback)
            {
                ShowFeedback("Đã đưa thiết bị thí nghiệm về trạng thái ban đầu.", Color.white);
            }
        }

        public void ToggleSwitch()
        {
            if (IsStoryGateActive) return;
            _isSwitchOn = !_isSwitchOn;
            UpdatePhysicsAndVisuals();

            string st = _isSwitchOn ? "<color=#55FF88>ĐÃ CẤP NGUỒN AC</color>" : "<color=#FFAA00>ĐÃ NGẮT NGUỒN AC</color>";
            ShowFeedback($"Công tắc: {st}", _isSwitchOn ? ColorElectricalCyan : ColorGold);
        }

        public void NextChallenge()
        {
            if (IsStoryGateActive) return;
            int maxIdx = GetMaxChallengeIndex();
            _currentChallengeIndex = (_currentChallengeIndex + 1) % (maxIdx + 1);
            ResetCurrentChallenge(false);
            ShowFeedback($"Chuyển sang Thử thách #{_currentChallengeIndex + 1}.", ColorGold);
        }

        public void PrevChallenge()
        {
            if (IsStoryGateActive) return;
            int maxIdx = GetMaxChallengeIndex();
            _currentChallengeIndex = (_currentChallengeIndex - 1 + (maxIdx + 1)) % (maxIdx + 1);
            ResetCurrentChallenge(false);
            ShowFeedback($"Chuyển sang Thử thách #{_currentChallengeIndex + 1}.", ColorGold);
        }

        private int GetMaxChallengeIndex()
        {
            switch (_currentMode)
            {
                case LabMode.InductionLab: return 2;        // 3 challenges (Induction, Reverse, Generator)
                case LabMode.ACApplicationLab: return 3;    // 4 challenges (Heater, Magnet, Lamp, Transformer)
                default: return 0;
            }
        }

        // =========================================================================
        // MODE A: TƯƠNG TÁC NAM CHÂM & MÁY PHÁT (C4_L14)
        // =========================================================================
        public void ToggleMagnetPole()
        {
            if (IsStoryGateActive) return;
            _poleDirection *= -1;
            float dist = Mathf.Abs(_magnetPosX - COIL_POS_X);
            float proximity = 1f - Mathf.Clamp01(dist / 360f);
            _previousFlux = _poleDirection * proximity; // Tránh spike khi đổi cực lúc nam châm đứng yên
            UpdateMagnetPoleVisual();
            ShowFeedback($"Đã đổi cực nam châm: Cực <b>{(_poleDirection > 0 ? "N (Bắc)" : "S (Nam)")}</b> đang hướng vào cuộn dây.", ColorGold);
        }

        private void UpdateMagnetPoleVisual()
        {
            if (_txtMagnetPole != null)
            {
                _txtMagnetPole.text = _poleDirection > 0 ? "N ➔ S" : "S ➔ N";
            }
            if (_magnetPoleImg != null)
            {
                _magnetPoleImg.color = _poleDirection > 0 ? ColorPoleNorth : ColorPoleSouth;
            }
        }

        public void OnMagnetDragDelta(float deltaX)
        {
            if (IsStoryGateActive) return;
            if (_currentMode != LabMode.InductionLab) return;
            if (_currentChallengeIndex == 2) return; // Challenge 3 dùng Generator

            _isDraggingMagnet = true;
            _prevMagnetPosX = _magnetPosX;
            _magnetPosX = Mathf.Clamp(_magnetPosX + deltaX, -300f, 220f);

            if (_magnetRt != null)
            {
                _magnetRt.anchoredPosition = new Vector2(_magnetPosX, 0f);
            }
        }

        public void OnMagnetEndDrag()
        {
            _isDraggingMagnet = false;
        }

        public void SetGeneratorSpeed(float speed)
        {
            if (IsStoryGateActive) return;
            _generatorSpeed = Mathf.Clamp(speed, 0f, 4f);
            ShowFeedback($"Tốc độ quay máy phát: {_generatorSpeed:F1} vòng/s.", ColorElectricalCyan);
        }

        private void UpdateInductionLabFrame(float dt)
        {
            if (_currentChallengeIndex == 2) // Challenge 3: AC Generator
            {
                if (_generatorSpeed > 0.05f)
                {
                    _rotorAngle += _generatorSpeed * 360f * dt;
                    if (_rotorAngle >= 360f * 1.5f) _hasGeneratedAC = true;

                    // I(t) = Imax * sin(angle)
                    float rad = _rotorAngle * Mathf.Deg2Rad;
                    _inducedCurrent = Mathf.Sin(rad);
                }
                else
                {
                    _inducedCurrent = 0f;
                }

                if (_rotorVisualRt != null)
                {
                    _rotorVisualRt.localRotation = Quaternion.Euler(0, 0, -_rotorAngle);
                }

                UpdateACWaveform();
            }
            else // Challenges 1 & 2: Magnet drag with Rate of Change of Magnetic Flux (dFlux / dt)
            {
                float safeDt = Mathf.Max(dt, 0.001f);
                float dist = Mathf.Abs(_magnetPosX - COIL_POS_X);
                float proximity = 1f - Mathf.Clamp01(dist / 360f);
                float currentFlux = _poleDirection * proximity;

                if (_isDraggingMagnet)
                {
                    float fluxRate = (currentFlux - _previousFlux) / safeDt;
                    _previousFlux = currentFlux;
                    fluxRate = Mathf.Clamp(fluxRate, -10f, 10f); // Guard against spike

                    // Dòng cảm ứng tỉ lệ với tốc độ biến thiên từ thông dFlux/dt
                    if (Mathf.Abs(fluxRate) > 0.02f && proximity > 0.03f)
                    {
                        _inducedCurrent = Mathf.Clamp(fluxRate * 1.6f, -1f, 1f);
                        _hasObservedInduction = true;

                        if (_inducedCurrent > 0.25f) _hasPositiveDeflection = true;
                        if (_inducedCurrent < -0.25f) _hasNegativeDeflection = true;
                    }
                    else
                    {
                        _inducedCurrent = Mathf.Lerp(_inducedCurrent, 0f, dt * 8f);
                    }
                }
                else
                {
                    // Đứng yên -> Từ thông không biến thiên (dFlux/dt = 0) -> Dòng cảm ứng về 0
                    _previousFlux = currentFlux;
                    _inducedCurrent = Mathf.Lerp(_inducedCurrent, 0f, dt * 8f);
                }
            }

            // Cập nhật kim điện kế Galvanometer (góc lệch -55° đến +55°)
            float targetAngle = -_inducedCurrent * 55f;
            _galvanometerAngle = Mathf.Lerp(_galvanometerAngle, targetAngle, dt * 12f);

            if (_needleRt != null)
            {
                _needleRt.localRotation = Quaternion.Euler(0, 0, _galvanometerAngle);
            }

            UpdatePhysicsAndVisuals();
        }

        private void UpdateACWaveform()
        {
            if (_waveformContainer == null) return;

            // Vẽ dải sóng xoay chiều dạng hình sin
            float tBase = _rotorAngle * Mathf.Deg2Rad;
            for (int i = 0; i < _waveformPool.Count; i++)
            {
                Image pt = _waveformPool[i];
                if (pt == null) continue;

                float sampleAngle = tBase - (i * 0.22f);
                float sampleVal = (_generatorSpeed > 0.05f) ? Mathf.Sin(sampleAngle) : 0f;

                RectTransform rt = pt.rectTransform;
                float x = -130f + (i * 10f);
                float y = sampleVal * 32f;
                rt.anchoredPosition = new Vector2(x, y);

                Color c = sampleVal > 0.1f ? ColorSuccessGreen :
                          sampleVal < -0.1f ? ColorElectricalCyan : ColorGold;
                pt.color = c;
                pt.gameObject.SetActive(true);
            }
        }

        // =========================================================================
        // MODE B: TƯƠNG TÁC THIẾT BỊ AC & MÁY BIẾN ÁP (C4_L15)
        // =========================================================================
        public void SelectACDevice(ACDeviceType deviceType)
        {
            if (IsStoryGateActive) return;
            _selectedDevice = deviceType;
            _deviceActiveProgress = 0f;
            UpdatePhysicsAndVisuals();
            ShowFeedback($"Đã gắn thiết bị vào cổng AC: <b>{GetDeviceDisplayName(deviceType)}</b>.", ColorElectricalCyan);
        }

        public void SelectTransformerN2(int index)
        {
            if (IsStoryGateActive) return;
            if (index < 0 || index >= _n2Options.Length) return;
            _selectedN2Idx = index;
            UpdatePhysicsAndVisuals();
        }

        private string GetDeviceDisplayName(ACDeviceType d)
        {
            switch (d)
            {
                case ACDeviceType.Heater: return "Máy sưởi nhiệt Aurelia";
                case ACDeviceType.Electromagnet: return "Nam châm điện ma thuật";
                case ACDeviceType.Lamp: return "Đèn phát sáng tinh tú";
                case ACDeviceType.Transformer: return "Máy biến áp Aurelia";
                default: return "";
            }
        }

        private void UpdateACApplicationFrame(float dt)
        {
            // Thiết bị chỉ kích hoạt khi Switch AC đóng
            if (_isSwitchOn)
            {
                _deviceActiveProgress = Mathf.Min(1.0f, _deviceActiveProgress + dt * 0.8f);
            }
            else
            {
                _deviceActiveProgress = Mathf.Max(0.0f, _deviceActiveProgress - dt * 1.5f);
            }

            // Visual bar fill
            if (_deviceProgressBar != null)
            {
                _deviceProgressBar.fillAmount = _deviceActiveProgress;
            }

            if (_txtDeviceEffectValue != null)
            {
                _txtDeviceEffectValue.text = $"Cường độ kích hoạt: {(_deviceActiveProgress * 100f):F0}%";
            }

            UpdatePhysicsAndVisuals();
        }

        // =========================================================================
        // TÍNH TOÁN VẬT LÝ & CẬP NHẬT UI
        // =========================================================================
        private void UpdatePhysicsAndVisuals()
        {
            if (_currentMode == LabMode.InductionLab)
            {
                EvaluateInductionLabPhysics();
            }
            else
            {
                EvaluateACApplicationPhysics();
            }

            UpdateSelectorHighlights();
        }

        private void EvaluateInductionLabPhysics()
        {
            bool isCh3 = (_currentChallengeIndex == 2);
            if (_generatorRoot != null) _generatorRoot.SetActive(isCh3);
            if (_magnetRt != null) _magnetRt.gameObject.SetActive(!isCh3);

            string missionTitle = "";
            string missionDesc = "";
            _isObjectiveMet = false;

            if (_currentChallengeIndex == 0)
            {
                missionTitle = "Thử thách 1: Tạo dòng điện cảm ứng";
                missionDesc = "Kéo nam châm di chuyển lại gần hoặc qua cuộn dây để tạo dòng điện cảm ứng.\n" +
                              "<i>Dòng điện cảm ứng xuất hiện khi từ thông qua cuộn dây biến đổi.</i>";
                _isObjectiveMet = _hasObservedInduction;
            }
            else if (_currentChallengeIndex == 1)
            {
                missionTitle = "Thử thách 2: Đảo chiều dòng điện cảm ứng";
                missionDesc = "Di chuyển nam châm qua cuộn dây hoặc kéo vào / rút ra để kim điện kế lệch sang cả 2 phía (+ và -).\n" +
                              "<i>Từ thông tăng và giảm sẽ làm chiều dòng điện cảm ứng thay đổi.</i>";
                _isObjectiveMet = _hasPositiveDeflection && _hasNegativeDeflection;
            }
            else // Challenge 2 (Index 2): AC Generator
            {
                missionTitle = "Thử thách 3: Tạo dòng điện xoay chiều";
                missionDesc = "Tăng tốc độ quay máy phát để rotor quay đều, tạo ra dòng điện đổi chiều tuần hoàn theo thời gian.";
                _isObjectiveMet = _hasGeneratedAC && _generatorSpeed > 0.4f;
            }

            if (_txtMissionTitle != null) _txtMissionTitle.text = missionTitle;
            if (_txtMissionDesc != null) _txtMissionDesc.text = missionDesc;
            if (_txtChallengeCounter != null)
            {
                string stA = _completedSubChallengesA.Contains(_currentChallengeIndex) ? "<color=#55FF88>[ĐÃ ĐẠT]</color>" : "<color=#FFAA00>[CHƯA ĐẠT]</color>";
                _txtChallengeCounter.text = $"Nhiệm vụ {_currentChallengeIndex + 1}/3 {stA} · Đã đạt: {_completedSubChallengesA.Count}/3";
            }

            // Magnitude string
            float mag = Mathf.Abs(_inducedCurrent);
            string levelStr = mag < 0.05f ? "<color=#888888>Không có</color>" :
                              mag < 0.35f ? "<color=#FFE600>Yếu</color>" :
                              mag < 0.70f ? "<color=#00E5FF>Trung bình</color>" :
                              "<color=#55FF88>Mạnh</color>";

            string dirStr = _inducedCurrent > 0.05f ? "<color=#55FF88>Dương (+ lệch phải)</color>" :
                            _inducedCurrent < -0.05f ? "<color=#00E5FF>Âm (- lệch trái)</color>" :
                            "<color=#888888>0 (Đứng yên)</color>";

            if (_txtReadoutMain != null)
            {
                _txtReadoutMain.text = $"Trạng thái nam châm: <color=#FFE600>{(_isDraggingMagnet ? "Đang di chuyển" : "Đứng yên")}</color>\n" +
                                       $"Cực hướng vào cuộn: <color=#{(_poleDirection > 0 ? "FF5555" : "5588FF")}>{(_poleDirection > 0 ? "Cực N (Bắc)" : "Cực S (Nam)")}</color>\n" +
                                       $"Mức cảm ứng: {levelStr}\n" +
                                       $"Chiều dòng điện: {dirStr}";
            }

            if (_txtReadoutSecondary != null)
            {
                if (isCh3)
                {
                    _txtReadoutSecondary.text = $"<b>Tốc độ quay:</b> {_generatorSpeed:F1} vòng/s\n" +
                                                $"<b>Góc quay rotor:</b> {((int)_rotorAngle % 360)}°\n" +
                                                $"<b>Dạng sóng:</b> <color=#00E5FF>Xoay chiều hình sin I ∝ sin(ωt)</color>";
                }
                else
                {
                    _txtReadoutSecondary.text = $"<b>Góc kim điện kế:</b> {_galvanometerAngle:F1}°\n" +
                                                $"<b>Lệch chiều dương (+):</b> {(_hasPositiveDeflection ? "<color=#55FF88>ĐÃ ĐẠT</color>" : "<color=#888888>Chưa</color>")}\n" +
                                                $"<b>Lệch chiều âm (-):</b> {(_hasNegativeDeflection ? "<color=#55FF88>ĐÃ ĐẠT</color>" : "<color=#888888>Chưa</color>")}";
                }
            }

            if (_txtReadoutFormula != null)
            {
                _txtReadoutFormula.text = "<b>Hiện tượng cảm ứng điện từ:</b>\n" +
                                          "Dòng điện cảm ứng xuất hiện khi từ thông qua cuộn dây biến đổi.\n" +
                                          "Từ thông tăng và giảm sẽ làm chiều dòng điện cảm ứng thay đổi.";
            }
        }

        private void EvaluateACApplicationPhysics()
        {
            string missionTitle = "";
            string missionDesc = "";
            _isObjectiveMet = false;

            if (_currentChallengeIndex == 0)
            {
                missionTitle = "Thử thách 1: Tác dụng nhiệt của dòng điện xoay chiều";
                missionDesc = "Chọn <b>Máy sưởi Aurelia</b>, bật nguồn AC để dây dẫn nóng đỏ và tăng nhiệt độ.";
                _isObjectiveMet = _isSwitchOn && _selectedDevice == ACDeviceType.Heater && _deviceActiveProgress > 0.7f;
            }
            else if (_currentChallengeIndex == 1)
            {
                missionTitle = "Thử thách 2: Tác dụng từ của dòng điện xoay chiều";
                missionDesc = "Chọn <b>Nam châm điện</b>, bật nguồn AC để cuộn dây sinh ra từ trường hút các mảnh kim loại.";
                _isObjectiveMet = _isSwitchOn && _selectedDevice == ACDeviceType.Electromagnet && _deviceActiveProgress > 0.7f;
            }
            else if (_currentChallengeIndex == 2)
            {
                missionTitle = "Thử thách 3: Tác dụng phát sáng của dòng điện xoay chiều";
                missionDesc = "Chọn <b>Đèn tinh tú</b>, bật nguồn AC để dòng điện chạy qua phát sáng rực rỡ.";
                _isObjectiveMet = _isSwitchOn && _selectedDevice == ACDeviceType.Lamp && _deviceActiveProgress > 0.7f;
            }
            else // Challenge 3 (Index 3): Transformer
            {
                missionTitle = "Thử thách 4: Máy biến áp Aurelia (U2 = 110 V)";
                missionDesc = "Máy biến áp có U1 = 220 V, cuộn sơ cấp N1 = 1000 vòng.\n" +
                              "Hãy chọn số vòng cuộn thứ cấp N2 để tạo hiệu điện thế đầu ra <b>U2 = 110 V</b>.";

                float n2 = _n2Options[_selectedN2Idx];
                float u2 = U1_VOLTAGE * (n2 / N1_TURNS);
                _isObjectiveMet = _isSwitchOn && _selectedDevice == ACDeviceType.Transformer && Mathf.Abs(u2 - 110f) < 0.1f;
            }

            if (_txtMissionTitle != null) _txtMissionTitle.text = missionTitle;
            if (_txtMissionDesc != null) _txtMissionDesc.text = missionDesc;
            if (_txtChallengeCounter != null)
            {
                string stB = _completedSubChallengesB.Contains(_currentChallengeIndex) ? "<color=#55FF88>[ĐÃ ĐẠT]</color>" : "<color=#FFAA00>[CHƯA ĐẠT]</color>";
                _txtChallengeCounter.text = $"Nhiệm vụ {_currentChallengeIndex + 1}/4 {stB} · Đã đạt: {_completedSubChallengesB.Count}/4";
            }

            bool isTransformer = (_selectedDevice == ACDeviceType.Transformer);
            if (_transformerControlsGo != null) _transformerControlsGo.SetActive(isTransformer);

            string switchStr = _isSwitchOn ? "<color=#55FF88>BẬT (ON)</color>" : "<color=#FFAA00>TẮT (OFF)</color>";
            if (_txtReadoutMain != null)
            {
                _txtReadoutMain.text = $"Nguồn AC: {switchStr}\n" +
                                       $"Thiết bị chọn: <color=#00E5FF>{GetDeviceDisplayName(_selectedDevice)}</color>\n" +
                                       $"Mức hoạt động: <color={(_isSwitchOn ? "#55FF88" : "#888888")}>{(_deviceActiveProgress * 100f):F0}%</color>";
            }

            if (_txtReadoutSecondary != null)
            {
                if (isTransformer)
                {
                    float n2 = _n2Options[_selectedN2Idx];
                    float u2 = _isSwitchOn ? (U1_VOLTAGE * (n2 / N1_TURNS)) : 0f;
                    _txtReadoutSecondary.text = $"<b>Cuộn sơ cấp:</b> U1 = 220 V | N1 = 1000 vòng\n" +
                                                $"<b>Cuộn thứ cấp:</b> N2 = {n2:F0} vòng\n" +
                                                $"<b>Điện áp đầu ra:</b> <color=#FFE600>U2 = {u2:F1} V</color>";
                }
                else
                {
                    string effectName = _selectedDevice == ACDeviceType.Heater ? $"Mức phát nhiệt: {(_deviceActiveProgress * 100f):F0}%" :
                                        _selectedDevice == ACDeviceType.Electromagnet ? $"Cường độ từ trường: {(_deviceActiveProgress * 100f):F0}%" + (_deviceActiveProgress > 0.7f ? " (Đã hút kim loại)" : "") :
                                        $"Mức phát sáng: {(_deviceActiveProgress * 100f):F0}%";

                    _txtReadoutSecondary.text = $"<b>Hiệu ứng quan sát:</b>\n" +
                                                $"<color=#FFE600>{effectName}</color>\n" +
                                                $"<b>Tình trạng:</b> {(_isObjectiveMet ? "<color=#55FF88>ĐẠT MỤC TIÊU</color>" : "<color=#FFAA00>CHƯA ĐẠT</color>")}";
                }
            }

            if (_txtReadoutFormula != null)
            {
                if (isTransformer)
                {
                    _txtReadoutFormula.text = "<b>Máy biến áp lý tưởng:</b>\n" +
                                              "U1 / U2 = N1 / N2  ➔  U2 = U1 · N2 / N1";
                }
                else
                {
                    _txtReadoutFormula.text = "<b>Tác dụng của dòng điện xoay chiều:</b>\n" +
                                              "Dòng điện xoay chiều có tác dụng nhiệt, từ và phát sáng.";
                }
            }
        }

        // =========================================================================
        // KIỂM TRA MỤC TIÊU & FLOW THẮNG / THUA (SUB-CHALLENGE GATING)
        // =========================================================================
        public void OnCheckClicked()
        {
            if (IsStoryGateActive) return;
            if (_isCompleted) return;

            if (_currentMode == LabMode.InductionLab)
            {
                if (_currentChallengeIndex == 0) // Sub-challenge 1: Tạo dòng cảm ứng
                {
                    if (_hasObservedInduction)
                    {
                        _completedSubChallengesA.Add(0);
                        PlaySoundSafe(true);
                        ShowFeedback("<b>XUẤT SẮC!</b> Đã tạo thành công dòng điện cảm ứng. Đang chuyển sang Thử thách 2: Đảo chiều dòng điện...", ColorSuccessGreen);
                        StartCoroutine(AdvanceSubChallengeRoutine(1));
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        ShowFeedback("Nam châm chưa chuyển động qua cuộn dây. Hãy kéo nam châm lại gần cuộn dây.", ColorAlertRed);
                    }
                }
                else if (_currentChallengeIndex == 1) // Sub-challenge 2: Đảo chiều dòng cảm ứng
                {
                    if (_hasPositiveDeflection && _hasNegativeDeflection)
                    {
                        _completedSubChallengesA.Add(1);
                        PlaySoundSafe(true);
                        ShowFeedback("<b>XUẤT SẮC!</b> Đã quan sát đủ 2 chiều dòng điện cảm ứng. Đang chuyển sang Thử thách 3: Máy phát điện xoay chiều...", ColorSuccessGreen);
                        StartCoroutine(AdvanceSubChallengeRoutine(2));
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        ShowFeedback("Cần quan sát dòng điện đổi chiều theo cả 2 hướng (kéo vào và rút ra, hoặc đi xuyên qua tâm cuộn dây).", ColorAlertRed);
                    }
                }
                else // Sub-challenge 3: AC Generator
                {
                    if (_hasGeneratedAC && _generatorSpeed > 0.4f)
                    {
                        _completedSubChallengesA.Add(2);
                        if (_completedSubChallengesA.Contains(0) && _completedSubChallengesA.Contains(1) && _completedSubChallengesA.Contains(2))
                        {
                            TriggerSuccessFlow();
                        }
                        else
                        {
                            PlaySoundSafe(false);
                            ShowFeedback("Đã tạo được dòng điện xoay chiều, nhưng cần hoàn thành cả Thử thách 1 và 2 trước đó để hoàn tất bài học!", ColorAlertRed);
                        }
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        ShowFeedback("Máy phát chưa quay đủ tốc độ để tạo dòng điện xoay chiều tuần hoàn.", ColorAlertRed);
                    }
                }
            }
            else // LabMode.ACApplicationLab
            {
                if (!_isSwitchOn)
                {
                    PlaySoundSafe(false);
                    ShowFeedback("Hãy đóng công tắc nguồn AC trước.", ColorAlertRed);
                    return;
                }

                if (_currentChallengeIndex == 0) // Sub-challenge 1: Heater
                {
                    if (_selectedDevice == ACDeviceType.Heater && _deviceActiveProgress > 0.7f)
                    {
                        _completedSubChallengesB.Add(0);
                        PlaySoundSafe(true);
                        ShowFeedback("<b>XUẤT SẮC!</b> Đã kích hoạt tác dụng nhiệt của dòng điện xoay chiều. Đang chuyển sang Thử thách 2: Tác dụng từ...", ColorSuccessGreen);
                        StartCoroutine(AdvanceSubChallengeRoutine(1));
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        ShowFeedback("Chưa chọn Máy sưởi hoặc chưa cấp đủ nhiệt lượng.", ColorAlertRed);
                    }
                }
                else if (_currentChallengeIndex == 1) // Sub-challenge 2: Electromagnet
                {
                    if (_selectedDevice == ACDeviceType.Electromagnet && _deviceActiveProgress > 0.7f)
                    {
                        _completedSubChallengesB.Add(1);
                        PlaySoundSafe(true);
                        ShowFeedback("<b>XUẤT SẮC!</b> Đã kích hoạt tác dụng từ của dòng điện xoay chiều. Đang chuyển sang Thử thách 3: Tác dụng phát sáng...", ColorSuccessGreen);
                        StartCoroutine(AdvanceSubChallengeRoutine(2));
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        ShowFeedback("Chưa chọn Nam châm điện hoặc từ trường chưa kích hoạt.", ColorAlertRed);
                    }
                }
                else if (_currentChallengeIndex == 2) // Sub-challenge 3: Lamp
                {
                    if (_selectedDevice == ACDeviceType.Lamp && _deviceActiveProgress > 0.7f)
                    {
                        _completedSubChallengesB.Add(2);
                        PlaySoundSafe(true);
                        ShowFeedback("<b>XUẤT SẮC!</b> Đã kích hoạt tác dụng phát sáng của dòng điện xoay chiều. Đang chuyển sang Thử thách 4: Máy biến áp Aurelia...", ColorSuccessGreen);
                        StartCoroutine(AdvanceSubChallengeRoutine(3));
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        ShowFeedback("Chưa chọn Đèn tinh tú hoặc đèn chưa phát sáng.", ColorAlertRed);
                    }
                }
                else // Sub-challenge 4: Transformer
                {
                    float n2 = _n2Options[_selectedN2Idx];
                    float u2 = U1_VOLTAGE * (n2 / N1_TURNS);

                    if (_selectedDevice == ACDeviceType.Transformer && Mathf.Abs(u2 - 110f) < 0.1f)
                    {
                        _completedSubChallengesB.Add(3);
                        if (_completedSubChallengesB.Contains(0) && _completedSubChallengesB.Contains(1) && _completedSubChallengesB.Contains(2) && _completedSubChallengesB.Contains(3))
                        {
                            TriggerSuccessFlow();
                        }
                        else
                        {
                            PlaySoundSafe(false);
                            ShowFeedback("Đã đạt điện áp 110 V, nhưng cần hoàn thành đủ cả 4 thử thách (Nhiệt, Từ, Quang, Biến áp) để hoàn tất bài học!", ColorAlertRed);
                        }
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        ShowFeedback($"Điện áp đầu ra hiện tại là U2 = {u2:F1} V (Yêu cầu U2 = 110 V). Hãy chọn lại số vòng cuộn thứ cấp N2.", ColorAlertRed);
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

            string successMsg = _currentMode == LabMode.InductionLab ?
                "HIỆN TƯỢNG ĐIỆN TỪ ĐÃ ĐƯỢC CHỨNG MINH THÀNH CÔNG!" :
                "ỨNG DỤNG DÒNG ĐIỆN XOAY CHIỀU ĐÃ HOÀN TẤT XUẤT SẮC!";

            ShowFeedback($"<b>XUẤT SẮC!</b> {successMsg}", ColorSuccessGreen);
            StartCoroutine(TriggerSuccessSequence());
        }

        private IEnumerator TriggerSuccessSequence()
        {
            yield return new WaitForSeconds(0.6f);

            var save = PhysicsSaveManager.Load() ?? new PhysicsSaveData();

            // 1. Ghi nhận bài học và chương
            save.MarkExploreLessonCompleted(_lessonId);
            save.MarkGameplayChapterCompleted("chapter_4");

            // 2. Kiểm tra điều kiện mở khóa vật phẩm duy nhất của Chương IV: 'magnet' (Cần C4_L14 VÀ C4_L15)
            bool isMagnetUnlocked = false;
            if (save.IsExploreLessonCompleted("C4_L14") && save.IsExploreLessonCompleted("C4_L15"))
            {
                isMagnetUnlocked = InventoryManager.UnlockItem("magnet", save);
            }

            // 3. Đánh giá Final Progression (cấp Energy Crystal nếu bài này hoàn tất toàn bộ hành trình)
            bool crystalGranted = PhysicsFinalProgression.EvaluateFinalProgression(save);

            // 4. Đánh giá thành tựu & Lưu an toàn
            if (!crystalGranted)
            {
                AchievementManager.EvaluateAchievements(save);
                PhysicsSaveManager.Save(save);
                if (PhysicsGameSession.Instance != null)
                {
                    PhysicsGameSession.Instance.ReloadSaveData();
                }
            }

            // 5. Hiển thị modal chúc mừng
            ShowSuccessModal(isMagnetUnlocked);
        }

        private void ShowSuccessModal(bool isMagnetUnlocked)
        {
            if (_successModal == null) return;

            foreach (Transform child in _successModal.transform)
            {
                Destroy(child.gameObject);
            }

            string lessonSuccessTitle = _currentMode == LabMode.InductionLab ?
                "CẢM ỨNG ĐIỆN TỪ ĐÃ ĐƯỢC KIỂM CHỨNG!" :
                "ỨNG DỤNG ĐIỆN TỪ HOÀN TẤT!";

            CreatePanelHeader(_successModal.transform, lessonSuccessTitle, new Vector2(0, 185));

            GameObject bodyGo = new GameObject("SuccessBody");
            bodyGo.transform.SetParent(_successModal.transform, false);
            RectTransform bRt = bodyGo.AddComponent<RectTransform>();
            bRt.anchoredPosition = new Vector2(0, 25);
            bRt.sizeDelta = new Vector2(640, 240);

            var bTmp = bodyGo.AddComponent<TextMeshProUGUI>();
            string itemNotice = isMagnetUnlocked ?
                "<color=#FFE600>★ CHÚC MỪNG: Bạn đã nhận được vật phẩm quý giá: NAM CHÂM (MAGNET) vì hoàn thành trọn bộ Chương IV!</color>" :
                "Tiến trình thực hành Chương IV đã được lưu an toàn. Hãy hoàn thành nốt bài còn lại để nhận Nam châm!";

            bTmp.text = $"<b>Chúc mừng bạn đã hoàn thành bài học: {_lessonTitle}!</b>\n\n" +
                        $"• Tiến trình khám phá: <color=#55FF88>Đã ghi nhận</color>\n" +
                        $"• Huy hiệu thực hành Chương IV: <color=#55FF88>Đã lưu</color>\n" +
                        $"• {itemNotice}\n\n" +
                        $"<i>Học viện Aurelia ghi nhận thành tích xuất sắc của bạn trong việc làm chủ hiện tượng cảm ứng điện từ!</i>";
            bTmp.fontSize = 16;
            bTmp.lineSpacing = 6;
            bTmp.color = Color.white;

            CreateButton(_successModal.transform, "Btn_Continue", new Vector2(-140, -175), new Vector2(220, 50), "TIẾP TỤC HỌC", () =>
            {
                _successModal.SetActive(false);
                Transform targetCanvas = _canvas != null ? _canvas.transform : null;
                StoryFlowController.TryShowPostGameplayOrPendingStories(
                    completedChapterHint: 4,
                    customSave: null,
                    onAllFinished: OnBackClicked,
                    canvasTransform: targetCanvas);
            }, true);

            CreateButton(_successModal.transform, "Btn_Replay", new Vector2(140, -175), new Vector2(200, 50), "LÀM LẠI", () =>
            {
                _successModal.SetActive(false);
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
            if (_currentMode == LabMode.InductionLab)
            {
                if (_currentChallengeIndex == 0)
                    hint = "<b>GỢI Ý:</b> Nam châm đứng yên không tạo dòng cảm ứng. Hãy làm từ trường qua cuộn dây thay đổi bằng cách kéo nam châm di chuyển.";
                else if (_currentChallengeIndex == 1)
                    hint = "<b>GỢI Ý:</b> Thử đổi chiều chuyển động của nam châm (kéo vào rồi kéo ra, hoặc bấm nút ĐỔI CỰC N/S).";
                else
                    hint = "<b>GỢI Ý:</b> Dòng điện xoay chiều đổi chiều tuần hoàn khi rotor quay. Hãy tăng tốc độ quay máy phát.";
            }
            else // ACApplicationLab
            {
                if (_currentChallengeIndex == 0)
                    hint = "<b>GỢI Ý:</b> Dòng điện chạy qua vật dẫn có thể làm vật dẫn nóng lên (tác dụng nhiệt). Hãy chọn Máy sưởi và bật nguồn AC.";
                else if (_currentChallengeIndex == 1)
                    hint = "<b>GỢI Ý:</b> Dòng điện xoay chiều chạy qua cuộn dây tạo ra tác dụng từ. Hãy chọn Nam châm điện để hút kim loại.";
                else if (_currentChallengeIndex == 2)
                    hint = "<b>GỢI Ý:</b> Dòng điện xoay chiều có thể làm thiết bị phát sáng. Hãy chọn Đèn tinh tú và bật nguồn.";
                else
                    hint = "<b>GỢI Ý:</b> Tỉ số hiệu điện thế bằng tỉ số số vòng dây trong máy biến áp lý tưởng: U1 / U2 = N1 / N2. Để có 110 V từ 220 V, cần chọn N2 = 500 vòng.";
            }

            ShowFeedback(hint, ColorGold);
        }

        public void OnBackClicked()
        {
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.NavigateTo(RouteId.ChapterMap4);
            }
            else if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.GameplayElectromagnetism);
            }
        }

        // =========================================================================
        // UI FEEDBACK & AUDIO
        // =========================================================================
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
            // Tích hợp âm thanh an toàn cho tương lai (Future Integration).
            // Không tạo AudioClip giả, không PlayOneShot(null).
        }

        // =========================================================================
        // XÂY DỰNG GIAO DIỆN (AURELIA NAVY & GOLD)
        // =========================================================================
        private void BuildGameplayUIIfMissing()
        {
            Transform existing = _canvas.transform.Find("ElectromagnetismHUD");
            if (existing != null)
            {
                _hudRoot = existing.gameObject;
                return;
            }

            _hudRoot = new GameObject("ElectromagnetismHUD");
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
            titleTmp.text = "HỌC VIỆN AURELIA · PHÒNG CẢM ỨNG ĐIỆN TỪ";
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
            _txtLessonBadge.text = "<b>C4_L14</b> · Cảm ứng điện từ. Dòng điện xoay chiều";
            _txtLessonBadge.fontSize = 17;
            _txtLessonBadge.alignment = TextAlignmentOptions.Center;
            _txtLessonBadge.color = Color.white;
        }

        private void BuildLeftPanel()
        {
            GameObject leftPanel = CreateNavyPanel(_hudRoot.transform, "LeftPanel", new Vector2(-650, 20), new Vector2(430, 780));

            // Mission Card
            GameObject missionCard = CreateNavyPanel(leftPanel.transform, "MissionCard", new Vector2(0, 260), new Vector2(390, 210));
            CreatePanelHeader(missionCard.transform, "NHIỆM VỤ THỰC NGHIỆM", new Vector2(0, 80));

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
            _txtMissionTitle.color = ColorElectricalCyan;

            GameObject mDescGo = new GameObject("MissionDesc");
            mDescGo.transform.SetParent(missionCard.transform, false);
            RectTransform mdRt = mDescGo.AddComponent<RectTransform>();
            mdRt.anchoredPosition = new Vector2(0, -38);
            mdRt.sizeDelta = new Vector2(360, 75);
            _txtMissionDesc = mDescGo.AddComponent<TextMeshProUGUI>();
            _txtMissionDesc.fontSize = 14;
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

            CreatePanelHeader(_panelModeAControls.transform, "ĐIỀU KHIỂN TỪ TRƯỜNG", new Vector2(0, 190));
            CreateButton(_panelModeAControls.transform, "Btn_TogglePole", new Vector2(0, 140), new Vector2(280, 45), "ĐỔI CỰC N / S", ToggleMagnetPole);

            CreatePanelHeader(_panelModeAControls.transform, "MÁY PHÁT ĐIỆN XOAY CHIỀU", new Vector2(0, 75));
            CreateButton(_panelModeAControls.transform, "Btn_GenStop", new Vector2(-120, 25), new Vector2(80, 38), "DỪNG", () => SetGeneratorSpeed(0f));
            CreateButton(_panelModeAControls.transform, "Btn_GenSlow", new Vector2(-30, 25), new Vector2(80, 38), "1 V/S", () => SetGeneratorSpeed(1f));
            CreateButton(_panelModeAControls.transform, "Btn_GenMed", new Vector2(55, 25), new Vector2(80, 38), "2 V/S", () => SetGeneratorSpeed(2f));
            CreateButton(_panelModeAControls.transform, "Btn_GenFast", new Vector2(140, 25), new Vector2(75, 38), "3 V/S", () => SetGeneratorSpeed(3f));

            GameObject aGuideGo = new GameObject("AGuide");
            aGuideGo.transform.SetParent(_panelModeAControls.transform, false);
            RectTransform agRt = aGuideGo.AddComponent<RectTransform>();
            agRt.anchoredPosition = new Vector2(0, -90);
            agRt.sizeDelta = new Vector2(360, 150);
            var agTmp = aGuideGo.AddComponent<TextMeshProUGUI>();
            agTmp.text = "<b>HƯỚNG DẪN THAO TÁC:</b>\n" +
                         "• <b>Kéo nam châm:</b> Dùng chuột kéo thỏi nam châm lại gần hoặc xuyên qua cuộn dây.\n" +
                         "• <b>Kim điện kế:</b> Lệch phải khi dòng điện dương, lệch trái khi dòng điện âm.\n" +
                         "• <b>Đổi cực:</b> Nhấn nút [ĐỔI CỰC N/S] để đảo chiều đường sức từ.";
            agTmp.fontSize = 14;
            agTmp.lineSpacing = 6;
            agTmp.color = new Color(0.85f, 0.90f, 0.98f, 0.90f);

            // === MODE B CONTROLS ===
            _panelModeBControls = new GameObject("ModeBControls");
            _panelModeBControls.transform.SetParent(leftPanel.transform, false);
            RectTransform bRt2 = _panelModeBControls.AddComponent<RectTransform>();
            bRt2.anchoredPosition = new Vector2(0, -90);
            bRt2.sizeDelta = new Vector2(390, 440);

            CreatePanelHeader(_panelModeBControls.transform, "CHỌN THIẾT BỊ THỬ NGHIỆM", new Vector2(0, 190));
            _btnDevicePickers.Clear();
            _btnDevicePickers.Add(CreateButton(_panelModeBControls.transform, "Btn_DevHeater", new Vector2(0, 145), new Vector2(300, 38), "MÁY SƯỞI (Tác dụng nhiệt)", () => SelectACDevice(ACDeviceType.Heater)).GetComponent<Button>());
            _btnDevicePickers.Add(CreateButton(_panelModeBControls.transform, "Btn_DevMag", new Vector2(0, 100), new Vector2(300, 38), "NAM CHÂM ĐIỆN (Tác dụng từ)", () => SelectACDevice(ACDeviceType.Electromagnet)).GetComponent<Button>());
            _btnDevicePickers.Add(CreateButton(_panelModeBControls.transform, "Btn_DevLamp", new Vector2(0, 55), new Vector2(300, 38), "ĐÈN TINH TÚ (Tác dụng quang)", () => SelectACDevice(ACDeviceType.Lamp)).GetComponent<Button>());
            _btnDevicePickers.Add(CreateButton(_panelModeBControls.transform, "Btn_DevTrans", new Vector2(0, 10), new Vector2(300, 38), "MÁY BIẾN ÁP AURELIA", () => SelectACDevice(ACDeviceType.Transformer)).GetComponent<Button>());

            // Transformer Controls subpanel
            _transformerControlsGo = new GameObject("TransformerControls");
            _transformerControlsGo.transform.SetParent(_panelModeBControls.transform, false);
            RectTransform tcRt = _transformerControlsGo.AddComponent<RectTransform>();
            tcRt.anchoredPosition = new Vector2(0, -90);
            tcRt.sizeDelta = new Vector2(380, 130);

            CreatePanelHeader(_transformerControlsGo.transform, "SỐ VÒNG CUỘN THỨ CẤP (N2)", new Vector2(0, 40));
            _btnN2Selectors.Clear();
            for (int i = 0; i < _n2Options.Length; i++)
            {
                int idx = i;
                float x = -115f + (i * 115f);
                var btn = CreateButton(_transformerControlsGo.transform, $"Btn_N2_{i}", new Vector2(x, -5), new Vector2(95, 40), $"{_n2Options[i]:F0} vòng", () => SelectTransformerN2(idx));
                _btnN2Selectors.Add(btn.GetComponent<Button>());
            }
        }

        private void BuildExperimentWorkspace()
        {
            GameObject boardGo = CreateNavyPanel(_hudRoot.transform, "ExperimentWorkspace", new Vector2(20, 70), new Vector2(860, 580));

            // Workspace Header
            GameObject bHeader = new GameObject("WorkspaceHeader");
            bHeader.transform.SetParent(boardGo.transform, false);
            RectTransform bhRt = bHeader.AddComponent<RectTransform>();
            bhRt.anchoredPosition = new Vector2(0, 260);
            bhRt.sizeDelta = new Vector2(600, 30);
            var txtBoardStatus = bHeader.AddComponent<TextMeshProUGUI>();
            txtBoardStatus.text = "KHÔNG GIAN THỰC NGHIỆM ĐIỆN TỪ";
            txtBoardStatus.fontSize = 17;
            txtBoardStatus.fontStyle = FontStyles.Bold;
            txtBoardStatus.alignment = TextAlignmentOptions.Center;
            txtBoardStatus.color = ColorGold;

            // =========================================================================
            // WORKSPACE MODE A (INDUCTION LAB)
            // =========================================================================
            _panelModeAWorkspace = new GameObject("ModeAWorkspace");
            _panelModeAWorkspace.transform.SetParent(boardGo.transform, false);
            RectTransform maRt = _panelModeAWorkspace.AddComponent<RectTransform>();
            maRt.anchorMin = Vector2.zero;
            maRt.anchorMax = Vector2.one;
            maRt.sizeDelta = Vector2.zero;

            // 1. Galvanometer Dial
            GameObject galvGo = CreateNavyPanel(_panelModeAWorkspace.transform, "Galvanometer", new Vector2(0, 140), new Vector2(260, 150));
            CreatePanelHeader(galvGo.transform, "ĐIỆN KẾ (GALVANOMETER)", new Vector2(0, 55));

            // Dial scale line
            GameObject scaleGo = new GameObject("ScaleLine");
            scaleGo.transform.SetParent(galvGo.transform, false);
            RectTransform scRt = scaleGo.AddComponent<RectTransform>();
            scRt.anchoredPosition = new Vector2(0, 15);
            scRt.sizeDelta = new Vector2(200, 25);
            var scTmp = scaleGo.AddComponent<TextMeshProUGUI>();
            scTmp.text = "<color=#00E5FF>◀ -50</color>   <b>0</b>   <color=#55FF88>+50 ▶</color>";
            scTmp.fontSize = 14;
            scTmp.alignment = TextAlignmentOptions.Center;

            // Needle
            GameObject needleGo = new GameObject("Needle");
            needleGo.transform.SetParent(galvGo.transform, false);
            _needleRt = needleGo.AddComponent<RectTransform>();
            _needleRt.anchoredPosition = new Vector2(0, -35);
            _needleRt.sizeDelta = new Vector2(4, 75);
            _needleRt.pivot = new Vector2(0.5f, 0f); // Quay quanh đáy kim
            Image needleImg = needleGo.AddComponent<Image>();
            needleImg.color = ColorGold;

            // 2. Fixed Coil
            GameObject coilGo = CreateNavyPanel(_panelModeAWorkspace.transform, "FixedCoil", new Vector2(COIL_POS_X, -40), new Vector2(130, 160));
            _coilRt = coilGo.GetComponent<RectTransform>();
            Image coilImg = coilGo.GetComponent<Image>();
            coilImg.color = new Color(0.20f, 0.12f, 0.08f, 0.95f); // Màu đồng ánh kim
            var coilOutline = coilGo.GetComponent<Outline>();
            coilOutline.effectColor = new Color(0.9f, 0.6f, 0.2f, 1f);

            GameObject coilTxtGo = new GameObject("CoilLabel");
            coilTxtGo.transform.SetParent(coilGo.transform, false);
            RectTransform clRt = coilTxtGo.AddComponent<RectTransform>();
            clRt.sizeDelta = new Vector2(120, 120);
            var clTmp = coilTxtGo.AddComponent<TextMeshProUGUI>();
            clTmp.text = "CUỘN DÂY\n(COIL)\n\nN = 500 vòng";
            clTmp.fontSize = 14;
            clTmp.fontStyle = FontStyles.Bold;
            clTmp.alignment = TextAlignmentOptions.Center;
            clTmp.color = ColorGold;

            // 3. Draggable Magnet
            GameObject magnetGo = CreateNavyPanel(_panelModeAWorkspace.transform, "DraggableMagnet", new Vector2(_magnetPosX, -40), new Vector2(170, 75));
            _magnetRt = magnetGo.GetComponent<RectTransform>();
            _magnetPoleImg = magnetGo.GetComponent<Image>();
            _magnetPoleImg.color = ColorPoleNorth;

            GameObject mTxtGo = new GameObject("MagnetText");
            mTxtGo.transform.SetParent(magnetGo.transform, false);
            RectTransform mtRt = mTxtGo.AddComponent<RectTransform>();
            mtRt.sizeDelta = _magnetRt.sizeDelta;
            _txtMagnetPole = mTxtGo.AddComponent<TextMeshProUGUI>();
            _txtMagnetPole.text = "N ➔ S\n<size=12>KÉO NAM CHÂM</size>";
            _txtMagnetPole.fontSize = 17;
            _txtMagnetPole.fontStyle = FontStyles.Bold;
            _txtMagnetPole.alignment = TextAlignmentOptions.Center;
            _txtMagnetPole.color = Color.white;

            // Event Trigger for Dragging Magnet
            EventTrigger trigger = magnetGo.AddComponent<EventTrigger>();
            var dragEntry = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            dragEntry.callback.AddListener((data) =>
            {
                var pData = (PointerEventData)data;
                OnMagnetDragDelta(pData.delta.x);
            });
            trigger.triggers.Add(dragEntry);

            var endDragEntry = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
            endDragEntry.callback.AddListener((data) => OnMagnetEndDrag());
            trigger.triggers.Add(endDragEntry);

            // 4. AC Generator Visualization (Challenge 3)
            _generatorRoot = CreateNavyPanel(_panelModeAWorkspace.transform, "ACGeneratorRoot", new Vector2(0, -60), new Vector2(680, 200));
            CreatePanelHeader(_generatorRoot.transform, "MÁY PHÁT ĐIỆN XOAY CHIỀU & DẠNG SÓNG AC", new Vector2(0, 75));

            // Rotor
            GameObject rotorGo = CreateNavyPanel(_generatorRoot.transform, "RotorVisual", new Vector2(-220, -15), new Vector2(110, 110));
            _rotorVisualRt = rotorGo.GetComponent<RectTransform>();
            GameObject rTxtGo = new GameObject("RotorText");
            rTxtGo.transform.SetParent(rotorGo.transform, false);
            RectTransform rtTxt = rTxtGo.AddComponent<RectTransform>();
            rtTxt.sizeDelta = _rotorVisualRt.sizeDelta;
            var rTmp = rTxtGo.AddComponent<TextMeshProUGUI>();
            rTmp.text = "ROTOR\n[N | S]";
            rTmp.fontSize = 14;
            rTmp.fontStyle = FontStyles.Bold;
            rTmp.alignment = TextAlignmentOptions.Center;
            rTmp.color = ColorGold;

            // Waveform Container
            GameObject waveGo = new GameObject("WaveformContainer");
            waveGo.transform.SetParent(_generatorRoot.transform, false);
            RectTransform wRt = waveGo.AddComponent<RectTransform>();
            wRt.anchoredPosition = new Vector2(120, -15);
            wRt.sizeDelta = new Vector2(300, 100);
            _waveformContainer = waveGo.transform;

            _waveformPool.Clear();
            for (int i = 0; i < WAVEFORM_POINTS; i++)
            {
                GameObject ptGo = new GameObject($"WavePt_{i}");
                ptGo.transform.SetParent(_waveformContainer, false);
                RectTransform ptRt = ptGo.AddComponent<RectTransform>();
                ptRt.sizeDelta = new Vector2(6, 6);
                Image ptImg = ptGo.AddComponent<Image>();
                ptImg.color = ColorElectricalCyan;
                _waveformPool.Add(ptImg);
            }

            _generatorRoot.SetActive(false);

            // =========================================================================
            // WORKSPACE MODE B (AC APPLICATION LAB)
            // =========================================================================
            _panelModeBWorkspace = new GameObject("ModeBWorkspace");
            _panelModeBWorkspace.transform.SetParent(boardGo.transform, false);
            RectTransform mbRt = _panelModeBWorkspace.AddComponent<RectTransform>();
            mbRt.anchorMin = Vector2.zero;
            mbRt.anchorMax = Vector2.one;
            mbRt.sizeDelta = Vector2.zero;

            // Device Display Card
            GameObject devCard = CreateNavyPanel(_panelModeBWorkspace.transform, "DeviceDisplayCard", new Vector2(0, 30), new Vector2(640, 320));
            CreatePanelHeader(devCard.transform, "THIẾT BỊ HOẠT ĐỘNG", new Vector2(0, 125));

            GameObject effTitle = new GameObject("EffectTitle");
            effTitle.transform.SetParent(devCard.transform, false);
            RectTransform etRt = effTitle.AddComponent<RectTransform>();
            etRt.anchoredPosition = new Vector2(0, 85);
            etRt.sizeDelta = new Vector2(500, 30);
            _txtDeviceStateHeader = effTitle.AddComponent<TextMeshProUGUI>();
            _txtDeviceStateHeader.text = "Máy sưởi nhiệt Aurelia";
            _txtDeviceStateHeader.fontSize = 17;
            _txtDeviceStateHeader.fontStyle = FontStyles.Bold;
            _txtDeviceStateHeader.alignment = TextAlignmentOptions.Center;
            _txtDeviceStateHeader.color = ColorElectricalCyan;

            // Effect Image Center
            GameObject effImgGo = new GameObject("EffectVisual");
            effImgGo.transform.SetParent(devCard.transform, false);
            RectTransform eiRt = effImgGo.AddComponent<RectTransform>();
            eiRt.anchoredPosition = new Vector2(0, 5);
            eiRt.sizeDelta = new Vector2(130, 90);
            _deviceVisualEffectImg = effImgGo.AddComponent<Image>();
            _deviceVisualEffectImg.color = ColorBtnNormal;

            // Progress Bar Container
            GameObject progBarGo = CreateNavyPanel(devCard.transform, "ProgressBarContainer", new Vector2(0, -75), new Vector2(480, 24));
            GameObject fillGo = new GameObject("BarFill");
            fillGo.transform.SetParent(progBarGo.transform, false);
            RectTransform bfRt = fillGo.AddComponent<RectTransform>();
            bfRt.anchorMin = Vector2.zero;
            bfRt.anchorMax = Vector2.one;
            bfRt.sizeDelta = Vector2.zero;
            _deviceProgressBar = fillGo.AddComponent<Image>();
            _deviceProgressBar.type = Image.Type.Filled;
            _deviceProgressBar.fillMethod = Image.FillMethod.Horizontal;
            _deviceProgressBar.fillAmount = 0f;
            _deviceProgressBar.color = ColorGold;

            GameObject effValGo = new GameObject("EffectValueText");
            effValGo.transform.SetParent(devCard.transform, false);
            RectTransform evRt = effValGo.AddComponent<RectTransform>();
            evRt.anchoredPosition = new Vector2(0, -115);
            evRt.sizeDelta = new Vector2(500, 30);
            _txtDeviceEffectValue = effValGo.AddComponent<TextMeshProUGUI>();
            _txtDeviceEffectValue.text = "Cường độ tác dụng: 0%";
            _txtDeviceEffectValue.fontSize = 15;
            _txtDeviceEffectValue.alignment = TextAlignmentOptions.Center;
            _txtDeviceEffectValue.color = Color.white;

            _panelModeBWorkspace.SetActive(false);
        }

        private void BuildRightPanel()
        {
            GameObject rightPanel = CreateNavyPanel(_hudRoot.transform, "RightPanel", new Vector2(670, 20), new Vector2(400, 780));
            CreatePanelHeader(rightPanel.transform, "THÔNG SỐ VẬT LÝ ĐIỆN TỪ", new Vector2(0, 345));

            // Main Readout Card
            GameObject mainReadoutCard = CreateNavyPanel(rightPanel.transform, "MainReadoutCard", new Vector2(0, 210), new Vector2(360, 210));
            GameObject mrTxtGo = new GameObject("MainText");
            mrTxtGo.transform.SetParent(mainReadoutCard.transform, false);
            RectTransform mrRt = mrTxtGo.AddComponent<RectTransform>();
            mrRt.sizeDelta = new Vector2(330, 180);
            _txtReadoutMain = mrTxtGo.AddComponent<TextMeshProUGUI>();
            _txtReadoutMain.fontSize = 15;
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
            _txtReadoutSecondary.fontSize = 14;
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
            _txtReadoutFormula.fontSize = 14;
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
            _txtFeedback.text = "Chào mừng tới Phòng cảm ứng Aurelia. Hãy thực hiện thao tác theo nhiệm vụ!";
            _txtFeedback.fontSize = 16;
            _txtFeedback.fontStyle = FontStyles.Bold;
            _txtFeedback.alignment = TextAlignmentOptions.Center;
            _txtFeedback.color = new Color(0.9f, 0.95f, 1f, 0.9f);

            CreateButton(_hudRoot.transform, "Btn_Hint", new Vector2(-260, -320), new Vector2(170, 50), "GỢI Ý", OnHintClicked);
            CreateButton(_hudRoot.transform, "Btn_Reset", new Vector2(-70, -320), new Vector2(170, 50), "LÀM LẠI", () => ResetCurrentChallenge(true));
            CreateButton(_hudRoot.transform, "Btn_ToggleSwitch", new Vector2(120, -320), new Vector2(170, 50), "NGUỒN AC", ToggleSwitch);
            CreateButton(_hudRoot.transform, "Btn_Check", new Vector2(310, -320), new Vector2(170, 50), "KIỂM TRA", OnCheckClicked, true);
        }

        private void BuildModals()
        {
            // 1. TUTORIAL MODAL
            _tutorialModal = CreateNavyPanel(_hudRoot.transform, "TutorialModal", Vector2.zero, new Vector2(720, 480));
            CreatePanelHeader(_tutorialModal.transform, "HƯỚNG DẪN · PHÒNG CẢM ỨNG AURELIA", new Vector2(0, 180));

            GameObject tutBodyGo = new GameObject("TutorialBody");
            tutBodyGo.transform.SetParent(_tutorialModal.transform, false);
            RectTransform tbRt = tutBodyGo.AddComponent<RectTransform>();
            tbRt.anchoredPosition = new Vector2(0, 15);
            tbRt.sizeDelta = new Vector2(620, 260);

            var tbTmp = tutBodyGo.AddComponent<TextMeshProUGUI>();
            tbTmp.text = "<b>Chào mừng pháp sư đến với Phòng Cảm Ứng Aurelia!</b>\n\n" +
                         "1. <b>Di chuyển nam châm hoặc vận hành thiết bị:</b> Kéo nam châm qua cuộn dây hoặc điều chỉnh máy phát AC.\n" +
                         "2. <b>Quan sát dòng điện & hiệu ứng:</b> Theo dõi kim điện kế, dạng sóng xoay chiều hoặc tác dụng nhiệt/từ/quang.\n" +
                         "3. <b>Hoàn thành nhiệm vụ:</b> Đạt đủ các tiêu chí thực nghiệm vật lý.\n" +
                         "4. <b>Bấm KIỂM TRA:</b> Xác nhận thành tích để mở khóa phần thưởng danh giá của Chương IV!";
            tbTmp.fontSize = 17;
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
            save.hasSeenElectromagnetismTutorial = true;
            PhysicsSaveManager.Save(save);

            if (_tutorialModal != null) _tutorialModal.SetActive(false);
        }

        private void UpdateSelectorHighlights()
        {
            // Mode B Device buttons
            for (int i = 0; i < _btnDevicePickers.Count; i++)
            {
                var img = _btnDevicePickers[i].GetComponent<Image>();
                if (img != null)
                {
                    bool isSel = (i == (int)_selectedDevice);
                    img.color = isSel ? ColorBtnSelected : ColorBtnNormal;
                }
            }

            // Mode B Transformer N2 buttons
            for (int i = 0; i < _btnN2Selectors.Count; i++)
            {
                var img = _btnN2Selectors[i].GetComponent<Image>();
                if (img != null)
                {
                    bool isSel = (i == _selectedN2Idx);
                    img.color = isSel ? ColorBtnSelected : ColorBtnNormal;
                }
            }
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
            rt.sizeDelta = new Vector2(380, 32);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 17;
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
            tmp.fontSize = isGold ? 17 : 14;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = isGold ? new Color(0.08f, 0.12f, 0.24f, 1f) : Color.white;

            btn.onClick.AddListener(() => onClick?.Invoke());
            btnGo.AddComponent<UIButtonFeedback>();

            return btnGo;
        }
    }
}
