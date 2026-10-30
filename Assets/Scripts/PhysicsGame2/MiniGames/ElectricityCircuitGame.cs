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
    /// Mini-game Chương III: Điện – XƯỞNG MẠCH ĐIỆN AURELIA (AURELIA CIRCUIT LAB).
    /// Hỗ trợ 3 Gameplay Mode tương ứng với 3 bài học:
    /// - MODE A (Ohm Lab): C3_L11 (Điện trở. Định luật Ohm) -> I = U / R.
    /// - MODE B (Circuit Builder): C3_L12 (Đoạn mạch nối tiếp, song song) -> Real Terminal Connection & Derived Topology.
    /// - MODE C (Power Lab): C3_L13 (Năng lượng của dòng điện và công suất điện) -> P = U*I, A = P*t.
    /// </summary>
    public class ElectricityCircuitGame : MonoBehaviour
    {
        public enum CircuitMode
        {
            OhmLab,         // C3_L11
            CircuitBuilder, // C3_L12
            PowerLab        // C3_L13
        }

        public enum CircuitTopology
        {
            None,
            Series,
            Parallel,
            Invalid
        }

        [Serializable]
        public struct CircuitConnection
        {
            public string nodeA;
            public string nodeB;

            public CircuitConnection(string a, string b)
            {
                nodeA = a;
                nodeB = b;
            }

            public bool Matches(string a, string b)
            {
                return (nodeA == a && nodeB == b) || (nodeA == b && nodeB == a);
            }

            public bool Contains(string node)
            {
                return nodeA == node || nodeB == node;
            }
        }

        // === TERMINAL IDENTIFIERS ===
        public const string T_BAT_POS = "BAT_POS";
        public const string T_BAT_NEG = "BAT_NEG";
        public const string T_SW_A    = "SW_A";
        public const string T_SW_B    = "SW_B";
        public const string T_R1_A    = "R1_A";
        public const string T_R1_B    = "R1_B";
        public const string T_R2_A    = "R2_A";
        public const string T_R2_B    = "R2_B";
        public const string T_J_POS   = "J_POS";
        public const string T_J_NEG   = "J_NEG";

        // === PARAMETERS & LESSON STATE ===
        private string _lessonId = "C3_L11";
        private string _lessonTitle = "Điện trở. Định luật Ohm";
        private CircuitMode _currentMode = CircuitMode.OhmLab;
        private int _currentChallengeIndex = 0;

        // Switch & State
        private bool _isSwitchOn = false;
        private bool _isObjectiveMet = false;
        private bool _isCompleted = false;
        private bool _storyGateActive = false;
        private bool _enteredViaNormalFlow = false;
        public bool IsStoryGateActive => _storyGateActive || StoryFlowController.IsStoryModalOpen;

        // === MODE A: OHM LAB PARAMETERS ===
        private readonly float[] _ohmVOptions = { 3f, 6f, 9f, 12f };
        private readonly float[] _ohmROptions = { 5f, 10f, 15f, 20f };
        private int _selectedVoltageIdx = 0;   // Default 3V
        private int _selectedResistanceIdx = 3;// Default 20Ω (Initial I = 3/20 = 0.15A -> FAIL)

        // === MODE B: CIRCUIT BUILDER REAL CONNECTION PARAMETERS ===
        private const float CIRCUIT_U = 12f;
        private const float R1_VAL = 10f;
        private const float R2_VAL = 20f;
        private CircuitTopology _currentTopology = CircuitTopology.None;
        private readonly List<CircuitConnection> _connections = new List<CircuitConnection>();
        private string _selectedTerminal = null;

        private readonly Dictionary<string, Vector2> _terminalPositions = new Dictionary<string, Vector2>
        {
            { T_BAT_POS, new Vector2(-190f, 75f) },
            { T_BAT_NEG, new Vector2(-190f, 5f) },
            { T_SW_A,    new Vector2(-190f, -80f) },
            { T_SW_B,    new Vector2(-190f, -140f) },
            { T_J_POS,   new Vector2(-15f, 30f) },
            { T_J_NEG,   new Vector2(315f, -30f) },
            { T_R1_A,    new Vector2(75f, 110f) },
            { T_R1_B,    new Vector2(235f, 110f) },
            { T_R2_A,    new Vector2(75f, -110f) },
            { T_R2_B,    new Vector2(235f, -110f) }
        };

        private readonly Dictionary<string, Button> _terminalButtons = new Dictionary<string, Button>();

        // === MODE C: POWER LAB PARAMETERS ===
        private readonly float[] _powerUOptions = { 6f, 9f, 12f };
        private readonly float[] _deviceCurrents = { 1f, 2f, 3f };
        private readonly string[] _deviceNames = { "Đèn Aurelia (1 A)", "Máy sưởi Aurelia (2 A)", "Bộ hấp thụ năng lượng (3 A)" };
        private readonly float[] _timeOptions = { 5f, 10f, 15f };
        private int _selectedPVoltageIdx = 0; // Default 6V
        private int _selectedDeviceIdx = 0;   // Default Lamp (1A)
        private int _selectedTimeIdx = 0;     // Default 5s (Initial A = 6*1*5 = 30J -> FAIL)

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

        // Circuit Board & Visual Components
        private RectTransform _circuitBoardRt;
        private Transform _wireContainer;
        private readonly List<Image> _wirePool = new List<Image>();

        // Board Elements
        private TextMeshProUGUI _txtBoardStatus;
        private Image _switchIndicatorImg;
        private TextMeshProUGUI _switchStateTxt;
        private Image _batteryVisualImg;
        private TextMeshProUGUI _batteryVoltTxt;
        private Image _resistor1VisualImg;
        private TextMeshProUGUI _resistor1Txt;
        private Image _resistor2VisualImg;
        private TextMeshProUGUI _resistor2Txt;
        private Image _ammeterVisualImg;
        private TextMeshProUGUI _ammeterTxt;
        private Image _loadDeviceImg;
        private TextMeshProUGUI _loadDeviceTxt;

        // Dynamic Mode Panels
        private GameObject _panelModeAControls;
        private GameObject _panelModeBControls;
        private GameObject _panelModeCControls;

        // Mode A Buttons
        private readonly List<Button> _btnVoltageSelectors = new List<Button>();
        private readonly List<Button> _btnResistanceSelectors = new List<Button>();

        // Mode B UI Texts
        private TextMeshProUGUI _txtWireCount;
        private TextMeshProUGUI _txtSelectedTerminalInfo;

        // Mode C Buttons
        private readonly List<Button> _btnPVoltageSelectors = new List<Button>();
        private readonly List<Button> _btnDeviceSelectors = new List<Button>();
        private readonly List<Button> _btnTimeSelectors = new List<Button>();

        // Colors (Aurelia Academy Theme)
        private static readonly Color ColorNavyBg = new Color(0.06f, 0.11f, 0.22f, 0.94f);
        private static readonly Color ColorGold = new Color(0.96f, 0.82f, 0.38f, 1f);
        private static readonly Color ColorGoldBorder = new Color(0.85f, 0.70f, 0.28f, 0.85f);
        private static readonly Color ColorElectricalCyan = new Color(0.00f, 0.90f, 1.00f, 1f);
        private static readonly Color ColorWireInactive = new Color(0.20f, 0.28f, 0.40f, 0.90f);
        private static readonly Color ColorWireActive = new Color(0.00f, 0.95f, 1.00f, 0.98f);
        private static readonly Color ColorSuccessGreen = new Color(0.30f, 0.95f, 0.45f, 1f);
        private static readonly Color ColorAlertRed = new Color(1.0f, 0.42f, 0.40f, 1f);
        private static readonly Color ColorBtnNormal = new Color(0.12f, 0.20f, 0.38f, 0.95f);
        private static readonly Color ColorBtnSelected = new Color(0.85f, 0.68f, 0.22f, 0.95f);
        private static readonly Color ColorTerminalNormal = new Color(0.10f, 0.16f, 0.28f, 1f);
        private static readonly Color ColorTerminalSelected = new Color(1.00f, 0.90f, 0.20f, 1f);

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
            if (_enteredViaNormalFlow && save != null && !save.hasSeenCore3Intro)
            {
                if (_tutorialModal != null) _tutorialModal.SetActive(false);
                SetStoryGate(true);
                StoryFlowController.TryShowGameplayCoreIntro(
                    3,
                    customSave: null,
                    onIntroFinishedOrSkipped: () =>
                    {
                        SetStoryGate(false);
                        var updatedSave = PhysicsSaveManager.Load();
                        bool shouldShowTutorial = updatedSave == null || !updatedSave.hasSeenElectricityTutorial;
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
                bool shouldShowTutorial = save == null || !save.hasSeenElectricityTutorial;
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

            // Hiệu ứng nhịp thở nhẹ cho dòng điện khi Switch ON
            if (_isSwitchOn && _isObjectiveMet && _switchIndicatorImg != null)
            {
                float pulse = 0.7f + 0.3f * Mathf.Sin(Time.unscaledTime * 5f);
                _switchIndicatorImg.color = new Color(0.3f, 1f, 0.5f, pulse);
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
                if (string.IsNullOrEmpty(id) || !id.StartsWith("C3_"))
                {
                    PhysicsGameSession.Instance.SelectChapter(3);
                    PhysicsGameSession.Instance.SelectLesson("C3_L11", "Điện trở. Định luật Ohm");
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
                _lessonId = "C3_L11";
                _lessonTitle = "Điện trở. Định luật Ohm";
            }

            switch (_lessonId)
            {
                case "C3_L12":
                    _currentMode = CircuitMode.CircuitBuilder;
                    break;
                case "C3_L13":
                    _currentMode = CircuitMode.PowerLab;
                    break;
                case "C3_L11":
                default:
                    _currentMode = CircuitMode.OhmLab;
                    break;
            }

            _currentChallengeIndex = 0;
            ConfigureModePanels();
        }

        private void ConfigureModePanels()
        {
            if (_panelModeAControls != null) _panelModeAControls.SetActive(_currentMode == CircuitMode.OhmLab);
            if (_panelModeBControls != null) _panelModeBControls.SetActive(_currentMode == CircuitMode.CircuitBuilder);
            if (_panelModeCControls != null) _panelModeCControls.SetActive(_currentMode == CircuitMode.PowerLab);

            // Bật/tắt các terminal buttons trên Circuit Board
            bool isModeB = (_currentMode == CircuitMode.CircuitBuilder);
            foreach (var kvp in _terminalButtons)
            {
                if (kvp.Value != null) kvp.Value.gameObject.SetActive(isModeB);
            }

            // Cập nhật badge bài học
            if (_txtLessonBadge != null)
            {
                string modeTag = _currentMode == CircuitMode.OhmLab ? "PHÒNG THÍ NGHIỆM OHM" :
                                 _currentMode == CircuitMode.CircuitBuilder ? "LẮP MẠCH ĐIỆN" : "QUẢN LÝ ĐIỆN NĂNG";
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

            if (_currentMode == CircuitMode.OhmLab)
            {
                _selectedVoltageIdx = 0;    // 3V
                _selectedResistanceIdx = 3; // 20Ω -> I = 0.15A (Fail all 3 challenges)
            }
            else if (_currentMode == CircuitMode.CircuitBuilder)
            {
                _connections.Clear();
                _selectedTerminal = null;
                _currentTopology = CircuitTopology.None;
                HighlightTerminalButtons();
            }
            else if (_currentMode == CircuitMode.PowerLab)
            {
                _selectedPVoltageIdx = 0; // 6V
                _selectedDeviceIdx = 0;   // Lamp (1A)
                _selectedTimeIdx = 0;     // 5s -> A = 30J (Fail both challenges)
            }

            UpdateCircuitPhysicsAndVisuals();

            if (showFeedback)
            {
                ShowFeedback("Đã đưa xưởng mạch điện về trạng thái ban đầu.", Color.white);
            }
        }

        public void ToggleSwitch()
        {
            if (IsStoryGateActive) return;
            _isSwitchOn = !_isSwitchOn;
            UpdateCircuitPhysicsAndVisuals();

            string st = _isSwitchOn ? "<color=#55FF88>ĐÃ ĐÓNG CÔNG TẮC</color> (Mạch thông suốt)" : "<color=#FFAA00>ĐÃ NGẮT CÔNG TẮC</color> (Dòng điện ngắt)";
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
                case CircuitMode.OhmLab: return 2;         // 3 challenges (0, 1, 2)
                case CircuitMode.CircuitBuilder: return 1; // 2 challenges (Series, Parallel)
                case CircuitMode.PowerLab: return 1;       // 2 challenges (Energy range, Exact Power)
                default: return 0;
            }
        }

        // =========================================================================
        // MODE A: TƯƠNG TÁC ĐỊNH LUẬT OHM
        // =========================================================================
        public void SelectOhmVoltage(int index)
        {
            if (IsStoryGateActive) return;
            if (index < 0 || index >= _ohmVOptions.Length) return;
            _selectedVoltageIdx = index;
            UpdateCircuitPhysicsAndVisuals();
        }

        public void SelectOhmResistance(int index)
        {
            if (IsStoryGateActive) return;
            if (index < 0 || index >= _ohmROptions.Length) return;
            _selectedResistanceIdx = index;
            UpdateCircuitPhysicsAndVisuals();
        }

        // =========================================================================
        // MODE B: TERMINAL CONNECTION SYSTEM (C3_L12 REAL CIRCUIT GAMEPLAY)
        // =========================================================================
        public bool HasConnection(string a, string b)
        {
            for (int i = 0; i < _connections.Count; i++)
            {
                if (_connections[i].Matches(a, b)) return true;
            }
            return false;
        }

        public bool AddConnection(string nodeA, string nodeB)
        {
            if (string.IsNullOrEmpty(nodeA) || string.IsNullOrEmpty(nodeB)) return false;
            if (nodeA == nodeB) return false; // Không cho A-A

            for (int i = 0; i < _connections.Count; i++)
            {
                if (_connections[i].Matches(nodeA, nodeB)) return false; // Không duplicate A-B / B-A
            }

            _connections.Add(new CircuitConnection(nodeA, nodeB));
            return true;
        }

        public bool ToggleConnection(string nodeA, string nodeB)
        {
            if (string.IsNullOrEmpty(nodeA) || string.IsNullOrEmpty(nodeB)) return false;
            if (nodeA == nodeB) return false;

            for (int i = 0; i < _connections.Count; i++)
            {
                if (_connections[i].Matches(nodeA, nodeB))
                {
                    _connections.RemoveAt(i);
                    return true; // Đã tháo dây
                }
            }

            _connections.Add(new CircuitConnection(nodeA, nodeB));
            return true; // Đã nối dây mới
        }

        public void OnTerminalClicked(string terminalId)
        {
            if (IsStoryGateActive) return;
            if (_currentMode != CircuitMode.CircuitBuilder) return;

            if (string.IsNullOrEmpty(_selectedTerminal))
            {
                _selectedTerminal = terminalId;
                HighlightTerminalButtons();
                ShowFeedback($"Đã chọn chốt <b>{terminalId}</b>. Hãy chọn chốt thứ hai để nối dây.", ColorGold);
            }
            else
            {
                if (_selectedTerminal == terminalId)
                {
                    // Click lại cùng 1 terminal -> hủy chọn
                    _selectedTerminal = null;
                    HighlightTerminalButtons();
                    ShowFeedback($"Đã hủy chọn chốt {terminalId}.", Color.white);
                }
                else
                {
                    string tA = _selectedTerminal;
                    string tB = terminalId;
                    _selectedTerminal = null;

                    bool wasExisting = HasConnection(tA, tB);
                    ToggleConnection(tA, tB);
                    HighlightTerminalButtons();
                    UpdateCircuitPhysicsAndVisuals();

                    if (wasExisting)
                    {
                        ShowFeedback($"Đã tháo dây nối giữa <b>{tA}</b> và <b>{tB}</b>.", Color.white);
                    }
                    else
                    {
                        ShowFeedback($"Đã nối dây: <b>{tA} ↔ {tB}</b>.", ColorElectricalCyan);
                    }
                }
            }
        }

        public void DisconnectAllWires()
        {
            if (IsStoryGateActive) return;
            _connections.Clear();
            _selectedTerminal = null;
            HighlightTerminalButtons();
            UpdateCircuitPhysicsAndVisuals();
            ShowFeedback("Đã tháo toàn bộ dây dẫn trên bảng mạch.", Color.white);
        }

        private void HighlightTerminalButtons()
        {
            foreach (var kvp in _terminalButtons)
            {
                if (kvp.Value == null) continue;
                var img = kvp.Value.GetComponent<Image>();
                if (img != null)
                {
                    bool isSel = (kvp.Key == _selectedTerminal);
                    img.color = isSel ? ColorTerminalSelected : ColorTerminalNormal;
                }
            }
        }

        // =========================================================================
        // TOPOLOGY DERIVATION & CIRCUIT CLOSED VALIDATION
        // =========================================================================
        public CircuitTopology DetermineTopology()
        {
            if (_connections.Count == 0) return CircuitTopology.None;

            if (CheckSeriesTopology()) return CircuitTopology.Series;
            if (CheckParallelTopology()) return CircuitTopology.Parallel;

            return CircuitTopology.Invalid;
        }

        public bool CheckSeriesTopology()
        {
            // Mạch nối tiếp gồm đúng 4 đoạn dây, không dùng chốt phân nhánh J_POS/J_NEG
            if (_connections.Count != 4) return false;

            for (int i = 0; i < _connections.Count; i++)
            {
                if (_connections[i].Contains(T_J_POS) || _connections[i].Contains(T_J_NEG))
                    return false;
            }

            // Nguồn BAT_POS kết nối với công tắc
            string swOutTerminal = null;
            if (HasConnection(T_BAT_POS, T_SW_A)) swOutTerminal = T_SW_B;
            else if (HasConnection(T_BAT_POS, T_SW_B)) swOutTerminal = T_SW_A;
            else return false;

            // Chain 1: Nguồn -> Switch -> R1 -> R2 -> BAT_NEG
            if (HasConnection(swOutTerminal, T_R1_A))
            {
                if (HasConnection(T_R1_B, T_R2_A) && HasConnection(T_R2_B, T_BAT_NEG)) return true;
                if (HasConnection(T_R1_B, T_R2_B) && HasConnection(T_R2_A, T_BAT_NEG)) return true;
            }
            if (HasConnection(swOutTerminal, T_R1_B))
            {
                if (HasConnection(T_R1_A, T_R2_A) && HasConnection(T_R2_B, T_BAT_NEG)) return true;
                if (HasConnection(T_R1_A, T_R2_B) && HasConnection(T_R2_A, T_BAT_NEG)) return true;
            }

            // Chain 2: Nguồn -> Switch -> R2 -> R1 -> BAT_NEG
            if (HasConnection(swOutTerminal, T_R2_A))
            {
                if (HasConnection(T_R2_B, T_R1_A) && HasConnection(T_R1_B, T_BAT_NEG)) return true;
                if (HasConnection(T_R2_B, T_R1_B) && HasConnection(T_R1_A, T_BAT_NEG)) return true;
            }
            if (HasConnection(swOutTerminal, T_R2_B))
            {
                if (HasConnection(T_R2_A, T_R1_A) && HasConnection(T_R1_B, T_BAT_NEG)) return true;
                if (HasConnection(T_R2_A, T_R1_B) && HasConnection(T_R1_A, T_BAT_NEG)) return true;
            }

            return false;
        }

        public bool CheckParallelTopology()
        {
            // Mạch song song chia sẻ 2 chốt J_POS và J_NEG, gồm đúng 7 đoạn dây
            if (_connections.Count != 7) return false;

            // 1. Nguồn kết nối công tắc
            string swOutTerminal = null;
            if (HasConnection(T_BAT_POS, T_SW_A)) swOutTerminal = T_SW_B;
            else if (HasConnection(T_BAT_POS, T_SW_B)) swOutTerminal = T_SW_A;
            else return false;

            // 2. Công tắc đến chốt phân nhánh J_POS
            if (!HasConnection(swOutTerminal, T_J_POS)) return false;

            // 3. Nhánh R1 kết nối giữa J_POS và J_NEG
            bool r1Connected = (HasConnection(T_J_POS, T_R1_A) && HasConnection(T_R1_B, T_J_NEG)) ||
                               (HasConnection(T_J_POS, T_R1_B) && HasConnection(T_R1_A, T_J_NEG));
            if (!r1Connected) return false;

            // 4. Nhánh R2 kết nối giữa J_POS và J_NEG
            bool r2Connected = (HasConnection(T_J_POS, T_R2_A) && HasConnection(T_R2_B, T_J_NEG)) ||
                               (HasConnection(T_J_POS, T_R2_B) && HasConnection(T_R2_A, T_J_NEG));
            if (!r2Connected) return false;

            // 5. Chốt J_NEG kết nối về cực âm BAT_NEG
            if (!HasConnection(T_J_NEG, T_BAT_NEG)) return false;

            return true;
        }

        public bool IsCircuitClosed()
        {
            CircuitTopology topo = DetermineTopology();
            return topo == CircuitTopology.Series || topo == CircuitTopology.Parallel;
        }

        // =========================================================================
        // MODE C: TƯƠNG TÁC QUẢN LÝ ĐIỆN NĂNG
        // =========================================================================
        public void SelectPowerVoltage(int index)
        {
            if (IsStoryGateActive) return;
            if (index < 0 || index >= _powerUOptions.Length) return;
            _selectedPVoltageIdx = index;
            UpdateCircuitPhysicsAndVisuals();
        }

        public void SelectPowerDevice(int index)
        {
            if (IsStoryGateActive) return;
            if (index < 0 || index >= _deviceCurrents.Length) return;
            _selectedDeviceIdx = index;
            UpdateCircuitPhysicsAndVisuals();
        }

        public void SelectPowerTime(int index)
        {
            if (IsStoryGateActive) return;
            if (index < 0 || index >= _timeOptions.Length) return;
            _selectedTimeIdx = index;
            UpdateCircuitPhysicsAndVisuals();
        }

        // =========================================================================
        // TÍNH TOÁN VẬT LÝ REALTIME & CẬP NHẬT UI
        // =========================================================================
        private void UpdateCircuitPhysicsAndVisuals()
        {
            switch (_currentMode)
            {
                case CircuitMode.OhmLab:
                    EvaluateOhmLabPhysics();
                    break;
                case CircuitMode.CircuitBuilder:
                    EvaluateCircuitBuilderPhysics();
                    break;
                case CircuitMode.PowerLab:
                    EvaluatePowerLabPhysics();
                    break;
            }

            UpdateSelectorButtonHighlights();
            UpdateVisualComponents();
            DrawCircuitWires();
        }

        private void EvaluateOhmLabPhysics()
        {
            float u = _ohmVOptions[_selectedVoltageIdx];
            float r = _ohmROptions[_selectedResistanceIdx];

            // Guard division by zero
            float i = 0f;
            if (_isSwitchOn && r > 0.001f)
            {
                i = u / r;
            }

            // Challenge Objective
            _isObjectiveMet = false;
            string missionTitle = "";
            string missionDesc = "";

            if (_currentChallengeIndex == 0)
            {
                missionTitle = "Thử thách 1: Dòng điện I = 0.60 A";
                missionDesc = "Điều chỉnh nguồn U và điện trở R để tạo dòng điện đúng <b>0.60 A</b> khi đóng công tắc.";
                _isObjectiveMet = _isSwitchOn && Mathf.Abs(i - 0.60f) < 0.02f;
            }
            else if (_currentChallengeIndex == 1)
            {
                missionTitle = "Thử thách 2: Dòng điện I = 0.90 A";
                missionDesc = "Tìm tổ hợp U và R để ampe kế chỉ đúng <b>0.90 A</b>.";
                _isObjectiveMet = _isSwitchOn && Mathf.Abs(i - 0.90f) < 0.02f;
            }
            else // Challenge 2
            {
                missionTitle = "Thử thách 3: Giữ dòng điện an toàn (0.55 A - 0.65 A)";
                missionDesc = "Duy trì dòng điện trong khoảng <b>0.55 A ≤ I ≤ 0.65 A</b>.";
                _isObjectiveMet = _isSwitchOn && (i >= 0.55f && i <= 0.65f);
            }

            // Update Texts
            if (_txtMissionTitle != null) _txtMissionTitle.text = missionTitle;
            if (_txtMissionDesc != null) _txtMissionDesc.text = missionDesc;
            if (_txtChallengeCounter != null) _txtChallengeCounter.text = $"Nhiệm vụ {_currentChallengeIndex + 1}/3";

            if (_txtReadoutMain != null)
            {
                string switchStr = _isSwitchOn ? "<color=#55FF88>ĐÓNG (ON)</color>" : "<color=#FFAA00>NGẮT (OFF)</color>";
                _txtReadoutMain.text = $"Trạng thái: {switchStr}\n" +
                                       $"Hiệu điện thế: <color=#FFE600>U = {u:F1} V</color>\n" +
                                       $"Điện trở: <color=#00E5FF>R = {r:F0} Ω</color>\n" +
                                       $"Dòng điện: <color={(_isSwitchOn ? "#55FF88" : "#888888")}>I = {i:F2} A</color>";
            }

            if (_txtReadoutSecondary != null)
            {
                _txtReadoutSecondary.text = $"<b>Ampe kế:</b> {i:F2} A\n" +
                                            $"<b>Vôn kế:</b> {u:F1} V\n" +
                                            $"<b>Tình trạng:</b> {(_isObjectiveMet ? "<color=#55FF88>ĐẠT MỤC TIÊU</color>" : "<color=#FFAA00>CHƯA ĐẠT</color>")}";
            }

            if (_txtReadoutFormula != null)
            {
                _txtReadoutFormula.text = "<b>Định luật Ohm:</b>\n" +
                                          "I = U / R  |  U = I · R  |  R = U / I";
            }
        }

        private void EvaluateCircuitBuilderPhysics()
        {
            _currentTopology = DetermineTopology();

            float rTotal = 0f;
            float iTotal = 0f;
            float i1 = 0f;
            float i2 = 0f;

            if (_currentTopology == CircuitTopology.Series)
            {
                rTotal = R1_VAL + R2_VAL; // 30Ω
                if (_isSwitchOn && rTotal > 0.001f)
                {
                    iTotal = CIRCUIT_U / rTotal; // 12 / 30 = 0.40A
                    i1 = iTotal;
                    i2 = iTotal;
                }
            }
            else if (_currentTopology == CircuitTopology.Parallel)
            {
                rTotal = (R1_VAL * R2_VAL) / (R1_VAL + R2_VAL); // 6.6667Ω
                if (_isSwitchOn && rTotal > 0.001f)
                {
                    iTotal = CIRCUIT_U / rTotal;  // 1.80A
                    i1 = CIRCUIT_U / R1_VAL;       // 12 / 10 = 1.20A
                    i2 = CIRCUIT_U / R2_VAL;       // 12 / 20 = 0.60A
                }
            }

            // Challenge Objective
            _isObjectiveMet = false;
            string missionTitle = "";
            string missionDesc = "";

            if (_currentChallengeIndex == 0)
            {
                missionTitle = "Thử thách A: Lắp Mạch Nối Tiếp";
                missionDesc = "Nối các chốt (Terminal) để mắc R1 và R2 <b>NỐI TIẾP</b> với nguồn 12 V và đóng công tắc.\n" +
                              "Mục tiêu: Một đường dòng điện duy nhất, R_tđ = 30 Ω, I = 0.40 A.";
                _isObjectiveMet = _isSwitchOn && _currentTopology == CircuitTopology.Series;
            }
            else // Challenge 1: Parallel
            {
                missionTitle = "Thử thách B: Lắp Mạch Song Song";
                missionDesc = "Nối các chốt qua chốt phân nhánh J+/J- để mắc R1 và R2 <b>SONG SONG</b>.\n" +
                              "Mục tiêu: R1 và R2 chung hai điểm đầu/cuối, R_tđ ≈ 6.67 Ω, I_tổng = 1.80 A.";
                _isObjectiveMet = _isSwitchOn && _currentTopology == CircuitTopology.Parallel;
            }

            // Update Texts
            if (_txtMissionTitle != null) _txtMissionTitle.text = missionTitle;
            if (_txtMissionDesc != null) _txtMissionDesc.text = missionDesc;
            if (_txtChallengeCounter != null) _txtChallengeCounter.text = $"Cấu hình {_currentChallengeIndex + 1}/2";

            string topoName = _currentTopology == CircuitTopology.Series ? "NỐI TIẾP" :
                              _currentTopology == CircuitTopology.Parallel ? "SONG SONG" :
                              _connections.Count == 0 ? "CHƯA NỐI DÂY" : "MẠCH HỞ / KHÔNG HỢP LỆ";

            if (_txtWireCount != null)
            {
                _txtWireCount.text = $"Số dây đã nối: <b>{_connections.Count}</b> đoạn dây";
            }
            if (_txtSelectedTerminalInfo != null)
            {
                _txtSelectedTerminalInfo.text = string.IsNullOrEmpty(_selectedTerminal) ?
                    "Chốt đang chọn: <i>[Chưa chọn]</i>" :
                    $"Chốt đang chọn: <color=#FFE600><b>{_selectedTerminal}</b></color>";
            }

            if (_txtReadoutMain != null)
            {
                string switchStr = _isSwitchOn ? "<color=#55FF88>ĐÓNG (ON)</color>" : "<color=#FFAA00>NGẮT (OFF)</color>";
                _txtReadoutMain.text = $"Kiểu mạch: <color=#00E5FF>{topoName}</color>\n" +
                                       $"Công tắc: {switchStr}\n" +
                                       $"Điện trở tương đương: <color=#FFE600>R_tđ = {(rTotal > 0 ? rTotal.ToString("F2") : "--")} Ω</color>\n" +
                                       $"Dòng điện tổng: <color={(_isSwitchOn && rTotal > 0 ? "#55FF88" : "#888888")}>I_tổng = {iTotal:F2} A</color>";
            }

            if (_txtReadoutSecondary != null)
            {
                if (_currentTopology == CircuitTopology.Parallel && _isSwitchOn)
                {
                    _txtReadoutSecondary.text = $"<b>Nhánh 1:</b> I1 = {i1:F2} A (R1 = 10 Ω)\n" +
                                                $"<b>Nhánh 2:</b> I2 = {i2:F2} A (R2 = 20 Ω)\n" +
                                                $"<b>Kiểm chứng:</b> I1 + I2 = {(i1 + i2):F2} A";
                }
                else if (_currentTopology == CircuitTopology.Series && _isSwitchOn)
                {
                    _txtReadoutSecondary.text = $"<b>Dòng điện qua R1:</b> I1 = {i1:F2} A\n" +
                                                $"<b>Dòng điện qua R2:</b> I2 = {i2:F2} A\n" +
                                                $"<b>Đặc điểm:</b> I = I1 = I2 = {iTotal:F2} A";
                }
                else
                {
                    _txtReadoutSecondary.text = $"<b>Dây nối:</b> {_connections.Count} đoạn dây\n" +
                                                $"<b>Mạch kín:</b> {(IsCircuitClosed() ? "<color=#55FF88>ĐÃ KÍN</color>" : "<color=#FFAA00>HỞ MẠCH</color>")}\n" +
                                                $"<b>Tình trạng:</b> {(_isObjectiveMet ? "<color=#55FF88>ĐẠT MỤC TIÊU</color>" : "<color=#FFAA00>CHƯA ĐẠT</color>")}";
                }
            }

            if (_txtReadoutFormula != null)
            {
                _txtReadoutFormula.text = "<b>Mạch nối tiếp:</b> R_tđ = R1 + R2 | I = I1 = I2\n" +
                                          "<b>Mạch song song:</b> 1/R_tđ = 1/R1 + 1/R2 | I = I1 + I2";
            }
        }

        private void EvaluatePowerLabPhysics()
        {
            float u = _powerUOptions[_selectedPVoltageIdx];
            float i = _deviceCurrents[_selectedDeviceIdx];
            float t = _timeOptions[_selectedTimeIdx];

            float p = 0f;
            float a = 0f;

            if (_isSwitchOn)
            {
                p = u * i;    // Watt
                a = p * t;    // Joule
            }

            // Challenge Objective
            _isObjectiveMet = false;
            string missionTitle = "";
            string missionDesc = "";

            if (_currentChallengeIndex == 0)
            {
                missionTitle = "Thử thách 1: Cấp điện năng (200 J ≤ A ≤ 300 J)";
                missionDesc = "Lựa chọn nguồn U, thiết bị tải và thời gian t để điện năng tiêu thụ đạt trong khoảng <b>200 J đến 300 J</b>.";
                _isObjectiveMet = _isSwitchOn && (a >= 200f && a <= 300f);
            }
            else // Challenge 1: Exact 24W
            {
                missionTitle = "Thử thách 2: Tạo công suất chính xác P = 24 W";
                missionDesc = "Phối hợp hiệu điện thế U và thiết bị tiêu thụ để công suất điện đạt đúng <b>24 W</b>.";
                _isObjectiveMet = _isSwitchOn && Mathf.Abs(p - 24f) < 0.1f;
            }

            // Update Texts
            if (_txtMissionTitle != null) _txtMissionTitle.text = missionTitle;
            if (_txtMissionDesc != null) _txtMissionDesc.text = missionDesc;
            if (_txtChallengeCounter != null) _txtChallengeCounter.text = $"Nhiệm vụ {_currentChallengeIndex + 1}/2";

            if (_txtReadoutMain != null)
            {
                string switchStr = _isSwitchOn ? "<color=#55FF88>ĐÓNG (ON)</color>" : "<color=#FFAA00>NGẮT (OFF)</color>";
                _txtReadoutMain.text = $"Trạng thái: {switchStr}\n" +
                                       $"Nguồn cấp: <color=#FFE600>U = {u:F1} V</color>\n" +
                                       $"Thiết bị: <color=#00E5FF>{_deviceNames[_selectedDeviceIdx]}</color>\n" +
                                       $"Thời gian: <color=#FFFFFF>t = {t:F0} s</color>";
            }

            if (_txtReadoutSecondary != null)
            {
                _txtReadoutSecondary.text = $"<b>Công suất điện (P):</b> <color={(_isSwitchOn ? "#FFE600" : "#888888")}>{p:F1} W</color>\n" +
                                            $"<b>Điện năng tiêu thụ (A):</b> <color={(_isSwitchOn ? "#55FF88" : "#888888")}>{a:F0} J</color>\n" +
                                            $"<b>Tình trạng:</b> {(_isObjectiveMet ? "<color=#55FF88>ĐẠT MỤC TIÊU</color>" : "<color=#FFAA00>CHƯA ĐẠT</color>")}";
            }

            if (_txtReadoutFormula != null)
            {
                _txtReadoutFormula.text = "<b>Công suất & Điện năng:</b>\n" +
                                          "P = U · I (W)  |  A = P · t = U · I · t (J)";
            }
        }

        // =========================================================================
        // KIỂM TRA MỤC TIÊU & FLOW THẮNG / THUA
        // =========================================================================
        public void OnCheckClicked()
        {
            if (IsStoryGateActive) return;
            if (_isCompleted) return;

            // 1. Phải bật công tắc
            if (!_isSwitchOn)
            {
                PlaySoundSafe(false);
                ShowFeedback("Hãy đóng công tắc trước.", ColorAlertRed);
                return;
            }

            // 2. Kiểm tra theo từng mode
            if (_currentMode == CircuitMode.OhmLab)
            {
                float u = _ohmVOptions[_selectedVoltageIdx];
                float r = _ohmROptions[_selectedResistanceIdx];
                float i = u / r;

                if (_currentChallengeIndex == 0) // Target 0.60A
                {
                    if (Mathf.Abs(i - 0.60f) < 0.02f)
                    {
                        TriggerSuccessFlow();
                    }
                    else if (i < 0.60f)
                    {
                        PlaySoundSafe(false);
                        ShowFeedback($"Dòng điện còn nhỏ (I = {i:F2} A < 0.60 A). Hãy tăng U hoặc giảm R.", ColorAlertRed);
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        ShowFeedback($"Dòng điện đang quá lớn (I = {i:F2} A > 0.60 A). Hãy giảm U hoặc tăng R.", ColorAlertRed);
                    }
                }
                else if (_currentChallengeIndex == 1) // Target 0.90A
                {
                    if (Mathf.Abs(i - 0.90f) < 0.02f)
                    {
                        TriggerSuccessFlow();
                    }
                    else if (i < 0.90f)
                    {
                        PlaySoundSafe(false);
                        ShowFeedback($"Dòng điện còn nhỏ (I = {i:F2} A < 0.90 A). Hãy tăng U hoặc giảm R.", ColorAlertRed);
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        ShowFeedback($"Dòng điện đang quá lớn (I = {i:F2} A > 0.90 A). Hãy giảm U hoặc tăng R.", ColorAlertRed);
                    }
                }
                else // Challenge 2: Range 0.55A - 0.65A
                {
                    if (i >= 0.55f && i <= 0.65f)
                    {
                        TriggerSuccessFlow();
                    }
                    else if (i < 0.55f)
                    {
                        PlaySoundSafe(false);
                        ShowFeedback($"Dòng điện còn nhỏ (I = {i:F2} A). Hãy tăng U hoặc giảm R.", ColorAlertRed);
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        ShowFeedback($"Dòng điện đang quá lớn (I = {i:F2} A). Hãy giảm U hoặc tăng R.", ColorAlertRed);
                    }
                }
            }
            else if (_currentMode == CircuitMode.CircuitBuilder)
            {
                CircuitTopology detected = DetermineTopology();

                if (_connections.Count == 0)
                {
                    PlaySoundSafe(false);
                    ShowFeedback("Mạch chưa kín.", ColorAlertRed);
                    return;
                }

                if (_currentChallengeIndex == 0) // Yêu cầu nối tiếp
                {
                    if (detected == CircuitTopology.Series)
                    {
                        TriggerSuccessFlow();
                    }
                    else if (detected == CircuitTopology.Parallel)
                    {
                        PlaySoundSafe(false);
                        ShowFeedback("Bạn đã tạo hai nhánh. Nhiệm vụ này cần một đường dòng điện duy nhất.", ColorAlertRed);
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        if (_connections.Count < 4)
                            ShowFeedback("Mạch đang hở. Hãy kiểm tra các đầu dây.", ColorAlertRed);
                        else
                            ShowFeedback("Cách nối hiện tại chưa tạo thành mạch hợp lệ.", ColorAlertRed);
                    }
                }
                else // Yêu cầu song song
                {
                    if (detected == CircuitTopology.Parallel)
                    {
                        TriggerSuccessFlow();
                    }
                    else if (detected == CircuitTopology.Series)
                    {
                        PlaySoundSafe(false);
                        ShowFeedback("R1 và R2 chưa cùng nối giữa hai điểm chung.", ColorAlertRed);
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        if (_connections.Count < 7)
                            ShowFeedback("Mạch đang hở. Hãy kiểm tra các đầu dây.", ColorAlertRed);
                        else
                            ShowFeedback("Cách nối hiện tại chưa tạo thành mạch hợp lệ.", ColorAlertRed);
                    }
                }
            }
            else if (_currentMode == CircuitMode.PowerLab)
            {
                float u = _powerUOptions[_selectedPVoltageIdx];
                float i = _deviceCurrents[_selectedDeviceIdx];
                float t = _timeOptions[_selectedTimeIdx];
                float p = u * i;
                float a = p * t;

                if (_currentChallengeIndex == 0) // 200J <= A <= 300J
                {
                    if (a >= 200f && a <= 300f)
                    {
                        TriggerSuccessFlow();
                    }
                    else if (a < 200f)
                    {
                        PlaySoundSafe(false);
                        ShowFeedback($"Điện năng chưa đạt mục tiêu (A = {a:F0} J < 200 J). Hãy tăng U, chọn tải lớn hơn hoặc tăng thời gian.", ColorAlertRed);
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        ShowFeedback($"Điện năng đang vượt giới hạn (A = {a:F0} J > 300 J). Hãy giảm bớt U hoặc thời gian.", ColorAlertRed);
                    }
                }
                else // P = 24W
                {
                    if (Mathf.Abs(p - 24f) < 0.1f)
                    {
                        TriggerSuccessFlow();
                    }
                    else if (p < 24f)
                    {
                        PlaySoundSafe(false);
                        ShowFeedback($"Công suất chưa đạt 24 W (P = {p:F1} W). Hãy tăng U hoặc đổi thiết bị tiêu thụ lớn hơn.", ColorAlertRed);
                    }
                    else
                    {
                        PlaySoundSafe(false);
                        ShowFeedback($"Công suất vượt quá 24 W (P = {p:F1} W). Hãy giảm U hoặc chọn thiết bị có dòng điện nhỏ hơn.", ColorAlertRed);
                    }
                }
            }
        }

        private void TriggerSuccessFlow()
        {
            _isCompleted = true;
            PlaySoundSafe(true);

            string successMsg = _currentMode == CircuitMode.OhmLab ? "ĐỊNH LUẬT OHM ĐÃ ĐƯỢC KIỂM CHỨNG!" :
                                _currentMode == CircuitMode.CircuitBuilder ? "MẠCH ĐIỆN HOẠT ĐỘNG HOÀN HẢO!" :
                                "NĂNG LƯỢNG ĐÃ ĐƯỢC CẤP CHÍNH XÁC!";
            ShowFeedback($"<b>XUẤT SẮC!</b> {successMsg}", ColorSuccessGreen);

            StartCoroutine(TriggerSuccessSequence());
        }

        private IEnumerator TriggerSuccessSequence()
        {
            yield return new WaitForSeconds(0.6f);

            var save = PhysicsSaveManager.Load() ?? new PhysicsSaveData();

            // 1. Ghi nhận bài học và chương
            save.MarkExploreLessonCompleted(_lessonId);
            save.MarkGameplayChapterCompleted("chapter_3");

            // 2. Mở khóa vật phẩm bài học
            string rewardItemId = GetRewardItemForCurrentLesson();
            bool isNewItem = InventoryManager.UnlockItem(rewardItemId, save);

            // 3. Kiểm tra điều kiện mở khóa BONUS 'wire' (Cần hoàn thành cả 3 bài: C3_L11, C3_L12, C3_L13)
            bool isWireBonusUnlocked = false;
            if (save.IsExploreLessonCompleted("C3_L11") &&
                save.IsExploreLessonCompleted("C3_L12") &&
                save.IsExploreLessonCompleted("C3_L13"))
            {
                isWireBonusUnlocked = InventoryManager.UnlockItem("wire", save);
            }

            // 4. Đánh giá Final Progression (cấp Energy Crystal nếu bài này hoàn tất toàn bộ hành trình)
            bool crystalGranted = PhysicsFinalProgression.EvaluateFinalProgression(save);

            // 5. Đánh giá thành tựu & Lưu an toàn
            if (!crystalGranted)
            {
                AchievementManager.EvaluateAchievements(save);
                PhysicsSaveManager.Save(save);
                if (PhysicsGameSession.Instance != null)
                {
                    PhysicsGameSession.Instance.ReloadSaveData();
                }
            }

            // 6. Hiển thị modal chúc mừng
            ShowSuccessModal(rewardItemId, isNewItem, isWireBonusUnlocked);
        }

        private string GetRewardItemForCurrentLesson()
        {
            switch (_lessonId)
            {
                case "C3_L11": return "battery";       // Pin
                case "C3_L12": return "switch";        // Công tắc
                case "C3_L13": return "electrical_box";// Hộp điện
                default: return "battery";
            }
        }

        // =========================================================================
        // HINT & BACK
        // =========================================================================
        public void OnHintClicked()
        {
            if (IsStoryGateActive) return;
            string hint = "";
            switch (_currentMode)
            {
                case CircuitMode.OhmLab:
                    hint = "<b>GỢI Ý:</b> Dòng điện tăng khi hiệu điện thế tăng hoặc điện trở giảm (I = U / R). Hãy quan sát ampe kế sau khi đóng công tắc.";
                    break;
                case CircuitMode.CircuitBuilder:
                    if (_currentChallengeIndex == 0)
                        hint = "<b>GỢI Ý:</b> Mạch nối tiếp chỉ có một đường đi từ cực dương về cực âm. Nối lần lượt qua Công tắc, R1, R2 rồi về cực âm.";
                    else
                        hint = "<b>GỢI Ý:</b> Hai điện trở song song phải có hai đầu nối chung cùng hai điểm. Hãy nối công tắc vào J+, từ J+ rẽ 2 nhánh qua R1/R2 vào J-, rồi từ J- về cực âm.";
                    break;
                case CircuitMode.PowerLab:
                    if (_currentChallengeIndex == 0)
                        hint = "<b>GỢI Ý:</b> Điện năng A = P · t = U · I · t. Hãy thử kết hợp nguồn 12 V với máy sưởi 2 A trong 10 s (A = 240 J).";
                    else
                        hint = "<b>GỢI Ý:</b> Công suất điện được tính bằng P = U · I. Để có 24 W, ta có thể dùng U = 12 V và tải có I = 2 A.";
                    break;
            }

            ShowFeedback(hint, ColorGold);
        }

        public void OnBackClicked()
        {
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.NavigateTo(RouteId.ChapterMap3);
            }
            else if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.GameplayElectricity);
            }
        }

        // =========================================================================
        // VẼ DÂY DẪN (WIRE RENDERING WITH REUSABLE POOL)
        // =========================================================================
        private void ClearWires()
        {
            for (int i = 0; i < _wirePool.Count; i++)
            {
                if (_wirePool[i] != null) _wirePool[i].gameObject.SetActive(false);
            }
        }

        private void DrawCircuitWires()
        {
            ClearWires();
            if (_wireContainer == null) return;

            Color wireColor = (_isSwitchOn && _isObjectiveMet) ? ColorWireActive : ColorWireInactive;
            float thickness = (_isSwitchOn && _isObjectiveMet) ? 5f : 3.5f;

            if (_currentMode == CircuitMode.OhmLab)
            {
                // Mạch Ohm Lab: Vòng khép kín hình chữ nhật
                Vector2 batPos = new Vector2(-220f, 0f);
                Vector2 rPos = new Vector2(0f, 140f);
                Vector2 aPos = new Vector2(220f, 0f);
                Vector2 swPos = new Vector2(0f, -140f);

                DrawWire(batPos + new Vector2(0, 30), new Vector2(batPos.x, rPos.y), wireColor, thickness);
                DrawWire(new Vector2(batPos.x, rPos.y), rPos + new Vector2(-50, 0), wireColor, thickness);

                DrawWire(rPos + new Vector2(50, 0), new Vector2(aPos.x, rPos.y), wireColor, thickness);
                DrawWire(new Vector2(aPos.x, rPos.y), aPos + new Vector2(0, 35), wireColor, thickness);

                DrawWire(aPos + new Vector2(0, -35), new Vector2(aPos.x, swPos.y), wireColor, thickness);
                DrawWire(new Vector2(aPos.x, swPos.y), swPos + new Vector2(50, 0), wireColor, thickness);

                DrawWire(swPos + new Vector2(-50, 0), new Vector2(batPos.x, swPos.y), wireColor, thickness);
                DrawWire(new Vector2(batPos.x, swPos.y), batPos + new Vector2(0, -30), wireColor, thickness);
            }
            else if (_currentMode == CircuitMode.CircuitBuilder)
            {
                // Vẽ chính xác các đoạn dây dựa trên danh sách _connections của người chơi
                for (int i = 0; i < _connections.Count; i++)
                {
                    var conn = _connections[i];
                    if (_terminalPositions.TryGetValue(conn.nodeA, out Vector2 pA) &&
                        _terminalPositions.TryGetValue(conn.nodeB, out Vector2 pB))
                    {
                        DrawWire(pA, pB, wireColor, thickness);
                    }
                }
            }
            else if (_currentMode == CircuitMode.PowerLab)
            {
                // Mạch Power Lab
                Vector2 pBat = new Vector2(-220f, 0f);
                Vector2 pSw = new Vector2(0f, 130f);
                Vector2 pLoad = new Vector2(220f, 0f);
                Vector2 pBottom = new Vector2(0f, -130f);

                DrawWire(pBat + new Vector2(0, 35), new Vector2(pBat.x, pSw.y), wireColor, thickness);
                DrawWire(new Vector2(pBat.x, pSw.y), pSw + new Vector2(-45, 0), wireColor, thickness);

                DrawWire(pSw + new Vector2(45, 0), new Vector2(pLoad.x, pSw.y), wireColor, thickness);
                DrawWire(new Vector2(pLoad.x, pSw.y), pLoad + new Vector2(0, 45), wireColor, thickness);

                DrawWire(pLoad + new Vector2(0, -45), new Vector2(pLoad.x, pBottom.y), wireColor, thickness);
                DrawWire(new Vector2(pLoad.x, pBottom.y), new Vector2(pBat.x, pBottom.y), wireColor, thickness);
                DrawWire(new Vector2(pBat.x, pBottom.y), pBat + new Vector2(0, -35), wireColor, thickness);
            }
        }

        private void DrawWire(Vector2 start, Vector2 end, Color color, float thickness)
        {
            if (_wireContainer == null) return;

            Image wireImg = GetAvailableWireSegment();
            RectTransform rt = wireImg.rectTransform;

            Vector2 dir = end - start;
            float length = dir.magnitude;
            if (length < 0.1f) return;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            rt.anchoredPosition = start;
            rt.sizeDelta = new Vector2(length, thickness);
            rt.localRotation = Quaternion.Euler(0, 0, angle);
            wireImg.color = color;
            wireImg.gameObject.SetActive(true);
        }

        private Image GetAvailableWireSegment()
        {
            for (int i = 0; i < _wirePool.Count; i++)
            {
                if (!_wirePool[i].gameObject.activeSelf)
                {
                    return _wirePool[i];
                }
            }

            GameObject go = new GameObject("WireSegment_" + _wirePool.Count);
            go.transform.SetParent(_wireContainer, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);

            Image img = go.AddComponent<Image>();
            img.color = ColorWireInactive;
            img.raycastTarget = false;

            _wirePool.Add(img);
            return img;
        }

        // =========================================================================
        // CẬP NHẬT GIAO DIỆN LINH KIỆN TRÊN BẢNG MẠCH
        // =========================================================================
        private void UpdateVisualComponents()
        {
            // Switch UI
            if (_switchIndicatorImg != null)
            {
                _switchIndicatorImg.color = _isSwitchOn ? ColorSuccessGreen : ColorAlertRed;
            }
            if (_switchStateTxt != null)
            {
                _switchStateTxt.text = _isSwitchOn ? "CÔNG TẮC: ĐÓNG" : "CÔNG TẮC: MỞ";
                _switchStateTxt.color = _isSwitchOn ? ColorSuccessGreen : ColorGold;
            }

            if (_currentMode == CircuitMode.OhmLab)
            {
                float u = _ohmVOptions[_selectedVoltageIdx];
                float r = _ohmROptions[_selectedResistanceIdx];
                float i = _isSwitchOn ? (u / r) : 0f;

                if (_batteryVoltTxt != null) _batteryVoltTxt.text = $"[ + | - ]\n{u:F1} V";
                if (_resistor1Txt != null) _resistor1Txt.text = $"R = {r:F0} Ω";
                if (_ammeterTxt != null) _ammeterTxt.text = $"A\n{i:F2} A";

                if (_batteryVisualImg != null) _batteryVisualImg.gameObject.SetActive(true);
                if (_resistor1VisualImg != null) _resistor1VisualImg.gameObject.SetActive(true);
                if (_resistor2VisualImg != null) _resistor2VisualImg.gameObject.SetActive(false);
                if (_ammeterVisualImg != null) _ammeterVisualImg.gameObject.SetActive(true);
                if (_loadDeviceImg != null) _loadDeviceImg.gameObject.SetActive(false);
            }
            else if (_currentMode == CircuitMode.CircuitBuilder)
            {
                if (_batteryVoltTxt != null) _batteryVoltTxt.text = "Nguồn điện\n12.0 V";
                if (_resistor1Txt != null) _resistor1Txt.text = "R1 = 10 Ω";
                if (_resistor2Txt != null) _resistor2Txt.text = "R2 = 20 Ω";

                if (_batteryVisualImg != null) _batteryVisualImg.gameObject.SetActive(true);
                if (_resistor1VisualImg != null) _resistor1VisualImg.gameObject.SetActive(true);
                if (_resistor2VisualImg != null) _resistor2VisualImg.gameObject.SetActive(true);
                if (_ammeterVisualImg != null) _ammeterVisualImg.gameObject.SetActive(false);
                if (_loadDeviceImg != null) _loadDeviceImg.gameObject.SetActive(false);
            }
            else if (_currentMode == CircuitMode.PowerLab)
            {
                float u = _powerUOptions[_selectedPVoltageIdx];
                float p = _isSwitchOn ? (u * _deviceCurrents[_selectedDeviceIdx]) : 0f;

                if (_batteryVoltTxt != null) _batteryVoltTxt.text = $"Nguồn cấp\n{u:F1} V";
                if (_loadDeviceTxt != null) _loadDeviceTxt.text = $"{_deviceNames[_selectedDeviceIdx]}\n{p:F1} W";

                if (_batteryVisualImg != null) _batteryVisualImg.gameObject.SetActive(true);
                if (_resistor1VisualImg != null) _resistor1VisualImg.gameObject.SetActive(false);
                if (_resistor2VisualImg != null) _resistor2VisualImg.gameObject.SetActive(false);
                if (_ammeterVisualImg != null) _ammeterVisualImg.gameObject.SetActive(false);
                if (_loadDeviceImg != null) _loadDeviceImg.gameObject.SetActive(true);
            }
        }

        private void UpdateSelectorButtonHighlights()
        {
            // Mode A U buttons
            for (int i = 0; i < _btnVoltageSelectors.Count; i++)
            {
                var img = _btnVoltageSelectors[i].GetComponent<Image>();
                if (img != null) img.color = (i == _selectedVoltageIdx) ? ColorBtnSelected : ColorBtnNormal;
            }

            // Mode A R buttons
            for (int i = 0; i < _btnResistanceSelectors.Count; i++)
            {
                var img = _btnResistanceSelectors[i].GetComponent<Image>();
                if (img != null) img.color = (i == _selectedResistanceIdx) ? ColorBtnSelected : ColorBtnNormal;
            }

            // Mode C U buttons
            for (int i = 0; i < _btnPVoltageSelectors.Count; i++)
            {
                var img = _btnPVoltageSelectors[i].GetComponent<Image>();
                if (img != null) img.color = (i == _selectedPVoltageIdx) ? ColorBtnSelected : ColorBtnNormal;
            }

            // Mode C Device buttons
            for (int i = 0; i < _btnDeviceSelectors.Count; i++)
            {
                var img = _btnDeviceSelectors[i].GetComponent<Image>();
                if (img != null) img.color = (i == _selectedDeviceIdx) ? ColorBtnSelected : ColorBtnNormal;
            }

            // Mode C Time buttons
            for (int i = 0; i < _btnTimeSelectors.Count; i++)
            {
                var img = _btnTimeSelectors[i].GetComponent<Image>();
                if (img != null) img.color = (i == _selectedTimeIdx) ? ColorBtnSelected : ColorBtnNormal;
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
            Transform existing = _canvas.transform.Find("ElectricityHUD");
            if (existing != null)
            {
                _hudRoot = existing.gameObject;
                return;
            }

            _hudRoot = new GameObject("ElectricityHUD");
            _hudRoot.transform.SetParent(_canvas.transform, false);
            RectTransform hudRt = _hudRoot.AddComponent<RectTransform>();
            hudRt.anchorMin = Vector2.zero;
            hudRt.anchorMax = Vector2.one;
            hudRt.sizeDelta = Vector2.zero;

            BuildTopHeader();
            BuildLeftPanel();
            BuildCircuitBoard();
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
            titleTmp.text = "HỌC VIỆN AURELIA · XƯỞNG MẠCH ĐIỆN";
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
            _txtLessonBadge.text = "<b>C3_L11</b> · Điện trở. Định luật Ohm";
            _txtLessonBadge.fontSize = 17;
            _txtLessonBadge.alignment = TextAlignmentOptions.Center;
            _txtLessonBadge.color = Color.white;
        }

        private void BuildLeftPanel()
        {
            GameObject leftPanel = CreateNavyPanel(_hudRoot.transform, "LeftPanel", new Vector2(-650, 20), new Vector2(430, 780));

            // Mission Card
            GameObject missionCard = CreateNavyPanel(leftPanel.transform, "MissionCard", new Vector2(0, 260), new Vector2(390, 210));
            CreatePanelHeader(missionCard.transform, "NHIỆM VỤ THỰC HÀNH", new Vector2(0, 80));

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

            // Nút chuyển challenge ◀ ▶
            CreateButton(missionCard.transform, "Btn_PrevCh", new Vector2(-155, 52), new Vector2(40, 30), "◀", PrevChallenge);
            CreateButton(missionCard.transform, "Btn_NextCh", new Vector2(155, 52), new Vector2(40, 30), "▶", NextChallenge);

            // === MODE A CONTROLS (Ohm Lab) ===
            _panelModeAControls = new GameObject("ModeAControls");
            _panelModeAControls.transform.SetParent(leftPanel.transform, false);
            RectTransform aRt = _panelModeAControls.AddComponent<RectTransform>();
            aRt.anchoredPosition = new Vector2(0, -90);
            aRt.sizeDelta = new Vector2(390, 440);

            CreatePanelHeader(_panelModeAControls.transform, "NGUỒN ĐIỆN (U)", new Vector2(0, 190));
            _btnVoltageSelectors.Clear();
            for (int i = 0; i < _ohmVOptions.Length; i++)
            {
                int idx = i;
                float x = -135f + (i * 90f);
                var btnGo = CreateButton(_panelModeAControls.transform, $"Btn_V_{i}", new Vector2(x, 145), new Vector2(80, 42), $"{_ohmVOptions[i]:F0} V", () => SelectOhmVoltage(idx));
                _btnVoltageSelectors.Add(btnGo.GetComponent<Button>());
            }

            CreatePanelHeader(_panelModeAControls.transform, "ĐIỆN TRỞ THỬ NGHIỆM (R)", new Vector2(0, 75));
            _btnResistanceSelectors.Clear();
            for (int i = 0; i < _ohmROptions.Length; i++)
            {
                int idx = i;
                float x = -135f + (i * 90f);
                var btnGo = CreateButton(_panelModeAControls.transform, $"Btn_R_{i}", new Vector2(x, 30), new Vector2(80, 42), $"{_ohmROptions[i]:F0} Ω", () => SelectOhmResistance(idx));
                _btnResistanceSelectors.Add(btnGo.GetComponent<Button>());
            }

            GameObject ohmNoteGo = new GameObject("OhmNote");
            ohmNoteGo.transform.SetParent(_panelModeAControls.transform, false);
            RectTransform onRt = ohmNoteGo.AddComponent<RectTransform>();
            onRt.anchoredPosition = new Vector2(0, -80);
            onRt.sizeDelta = new Vector2(360, 110);
            var onTmp = ohmNoteGo.AddComponent<TextMeshProUGUI>();
            onTmp.text = "<i>Mẹo thao tác:</i>\n" +
                         "• Chọn hiệu điện thế nguồn U\n" +
                         "• Chọn giá trị điện trở R\n" +
                         "• Bấm nút <b>ĐÓNG CÔNG TẮC</b> bên dưới\n" +
                         "• Quan sát dòng điện và bấm <b>KIỂM TRA</b>.";
            onTmp.fontSize = 14;
            onTmp.alignment = TextAlignmentOptions.Center;
            onTmp.color = new Color(0.8f, 0.85f, 0.95f, 0.85f);

            // === MODE B CONTROLS (Circuit Builder - Real Terminal Wiring) ===
            _panelModeBControls = new GameObject("ModeBControls");
            _panelModeBControls.transform.SetParent(leftPanel.transform, false);
            RectTransform bCtrlRt = _panelModeBControls.AddComponent<RectTransform>();
            bCtrlRt.anchoredPosition = new Vector2(0, -90);
            bCtrlRt.sizeDelta = new Vector2(390, 440);

            CreatePanelHeader(_panelModeBControls.transform, "CÔNG CỤ NỐI MẠCH", new Vector2(0, 190));

            // Wire Counter Badge
            GameObject wcGo = new GameObject("WireCountBadge");
            wcGo.transform.SetParent(_panelModeBControls.transform, false);
            RectTransform wcRt = wcGo.AddComponent<RectTransform>();
            wcRt.anchoredPosition = new Vector2(0, 145);
            wcRt.sizeDelta = new Vector2(360, 32);
            _txtWireCount = wcGo.AddComponent<TextMeshProUGUI>();
            _txtWireCount.text = "Số dây đã nối: <b>0</b> đoạn dây";
            _txtWireCount.fontSize = 16;
            _txtWireCount.fontStyle = FontStyles.Bold;
            _txtWireCount.alignment = TextAlignmentOptions.Center;
            _txtWireCount.color = ColorElectricalCyan;

            // Selected Terminal Info
            GameObject stGo = new GameObject("SelectedTerminalInfo");
            stGo.transform.SetParent(_panelModeBControls.transform, false);
            RectTransform stRt = stGo.AddComponent<RectTransform>();
            stRt.anchoredPosition = new Vector2(0, 110);
            stRt.sizeDelta = new Vector2(360, 30);
            _txtSelectedTerminalInfo = stGo.AddComponent<TextMeshProUGUI>();
            _txtSelectedTerminalInfo.text = "Chốt đang chọn: <i>[Chưa chọn]</i>";
            _txtSelectedTerminalInfo.fontSize = 15;
            _txtSelectedTerminalInfo.alignment = TextAlignmentOptions.Center;
            _txtSelectedTerminalInfo.color = Color.white;

            // Button Tháo Dây
            CreateButton(_panelModeBControls.transform, "Btn_ClearWires", new Vector2(0, 55), new Vector2(280, 45), "THÁO TOÀN BỘ DÂY", DisconnectAllWires);

            // Hướng dẫn tương tác Terminal
            GameObject guideGo = new GameObject("TerminalGuide");
            guideGo.transform.SetParent(_panelModeBControls.transform, false);
            RectTransform gRt = guideGo.AddComponent<RectTransform>();
            gRt.anchoredPosition = new Vector2(0, -75);
            gRt.sizeDelta = new Vector2(360, 180);
            var gTmp = guideGo.AddComponent<TextMeshProUGUI>();
            gTmp.text = "<b>HƯỚNG DẪN NỐI DÂY:</b>\n" +
                        "1. <b>Bấm chọn chốt đầu tiên</b> trên bảng mạch (chốt sẽ sáng vàng rực).\n" +
                        "2. <b>Bấm chốt thứ hai</b> để kéo dây dẫn giữa 2 chốt.\n" +
                        "3. <b>Bấm lại cặp chốt</b> đã nối để tháo đoạn dây đó.\n" +
                        "4. Đóng công tắc và bấm <b>KIỂM TRA</b> khi hoàn tất.";
            gTmp.fontSize = 14;
            gTmp.lineSpacing = 6;
            gTmp.color = new Color(0.85f, 0.90f, 0.98f, 0.90f);

            // === MODE C CONTROLS (Power Lab) ===
            _panelModeCControls = new GameObject("ModeCControls");
            _panelModeCControls.transform.SetParent(leftPanel.transform, false);
            RectTransform cCtrlRt = _panelModeCControls.AddComponent<RectTransform>();
            cCtrlRt.anchoredPosition = new Vector2(0, -90);
            cCtrlRt.sizeDelta = new Vector2(390, 440);

            CreatePanelHeader(_panelModeCControls.transform, "NGUỒN CẤP (U)", new Vector2(0, 190));
            _btnPVoltageSelectors.Clear();
            for (int i = 0; i < _powerUOptions.Length; i++)
            {
                int idx = i;
                float x = -120f + (i * 120f);
                var btnGo = CreateButton(_panelModeCControls.transform, $"Btn_PU_{i}", new Vector2(x, 145), new Vector2(100, 40), $"{_powerUOptions[i]:F0} V", () => SelectPowerVoltage(idx));
                _btnPVoltageSelectors.Add(btnGo.GetComponent<Button>());
            }

            CreatePanelHeader(_panelModeCControls.transform, "THIẾT BỊ TIÊU THỤ", new Vector2(0, 95));
            _btnDeviceSelectors.Clear();
            string[] shortDev = { "Đèn (1 A)", "Máy sưởi (2 A)", "Hấp thụ (3 A)" };
            for (int i = 0; i < shortDev.Length; i++)
            {
                int idx = i;
                float y = 50f - (i * 44f);
                var btnGo = CreateButton(_panelModeCControls.transform, $"Btn_PDev_{i}", new Vector2(0, y), new Vector2(320, 38), shortDev[i], () => SelectPowerDevice(idx));
                _btnDeviceSelectors.Add(btnGo.GetComponent<Button>());
            }

            CreatePanelHeader(_panelModeCControls.transform, "THỜI GIAN HOẠT ĐỘNG (t)", new Vector2(0, -95));
            _btnTimeSelectors.Clear();
            for (int i = 0; i < _timeOptions.Length; i++)
            {
                int idx = i;
                float x = -120f + (i * 120f);
                var btnGo = CreateButton(_panelModeCControls.transform, $"Btn_PTime_{i}", new Vector2(x, -140), new Vector2(100, 40), $"{_timeOptions[i]:F0} s", () => SelectPowerTime(idx));
                _btnTimeSelectors.Add(btnGo.GetComponent<Button>());
            }
        }

        private void BuildCircuitBoard()
        {
            GameObject boardGo = CreateNavyPanel(_hudRoot.transform, "CircuitBoard", new Vector2(20, 70), new Vector2(860, 580));
            _circuitBoardRt = boardGo.GetComponent<RectTransform>();

            // Board Header
            GameObject bHeader = new GameObject("BoardHeader");
            bHeader.transform.SetParent(boardGo.transform, false);
            RectTransform bhRt = bHeader.AddComponent<RectTransform>();
            bhRt.anchoredPosition = new Vector2(0, 260);
            bhRt.sizeDelta = new Vector2(600, 30);
            _txtBoardStatus = bHeader.AddComponent<TextMeshProUGUI>();
            _txtBoardStatus.text = "BẢNG MẠCH THỬ NGHIỆM AURELIA";
            _txtBoardStatus.fontSize = 17;
            _txtBoardStatus.fontStyle = FontStyles.Bold;
            _txtBoardStatus.alignment = TextAlignmentOptions.Center;
            _txtBoardStatus.color = ColorGold;

            // Wire Container
            GameObject wireGo = new GameObject("WireContainer");
            wireGo.transform.SetParent(boardGo.transform, false);
            RectTransform wRt = wireGo.AddComponent<RectTransform>();
            wRt.anchorMin = Vector2.zero;
            wRt.anchorMax = Vector2.one;
            wRt.sizeDelta = Vector2.zero;
            _wireContainer = wireGo.transform;

            // 1. Battery Visual
            _batteryVisualImg = CreateComponentCard(boardGo.transform, "BatteryVisual", new Vector2(-240, 40), new Vector2(130, 95), "Nguồn điện\n12.0 V", out _batteryVoltTxt);

            // 2. Resistor 1 Visual
            _resistor1VisualImg = CreateComponentCard(boardGo.transform, "Resistor1Visual", new Vector2(155, 110), new Vector2(145, 65), "R1 = 10 Ω", out _resistor1Txt);

            // 3. Resistor 2 Visual
            _resistor2VisualImg = CreateComponentCard(boardGo.transform, "Resistor2Visual", new Vector2(155, -110), new Vector2(145, 65), "R2 = 20 Ω", out _resistor2Txt);

            // 4. Ammeter Visual (Mode A)
            _ammeterVisualImg = CreateComponentCard(boardGo.transform, "AmmeterVisual", new Vector2(220, 0), new Vector2(110, 80), "A\n0.00 A", out _ammeterTxt);

            // 5. Load Device Visual (Mode C)
            _loadDeviceImg = CreateComponentCard(boardGo.transform, "LoadDeviceVisual", new Vector2(220, 0), new Vector2(140, 85), "Thiết bị tải", out _loadDeviceTxt);

            // 6. Knife Switch Visual
            GameObject swGo = new GameObject("SwitchVisual");
            swGo.transform.SetParent(boardGo.transform, false);
            RectTransform swRt = swGo.AddComponent<RectTransform>();
            swRt.anchoredPosition = new Vector2(-240, -110);
            swRt.sizeDelta = new Vector2(130, 90);

            _switchIndicatorImg = swGo.AddComponent<Image>();
            _switchIndicatorImg.color = ColorAlertRed;
            var swOutline = swGo.AddComponent<Outline>();
            swOutline.effectColor = ColorGoldBorder;
            swOutline.effectDistance = new Vector2(1, -1);

            Button swBtn = swGo.AddComponent<Button>();
            swBtn.onClick.AddListener(ToggleSwitch);
            swGo.AddComponent<UIButtonFeedback>();

            GameObject swTxtGo = new GameObject("Text");
            swTxtGo.transform.SetParent(swGo.transform, false);
            RectTransform stRt = swTxtGo.AddComponent<RectTransform>();
            stRt.sizeDelta = swRt.sizeDelta;
            _switchStateTxt = swTxtGo.AddComponent<TextMeshProUGUI>();
            _switchStateTxt.text = "CÔNG TẮC: MỞ";
            _switchStateTxt.fontSize = 15;
            _switchStateTxt.fontStyle = FontStyles.Bold;
            _switchStateTxt.alignment = TextAlignmentOptions.Center;
            _switchStateTxt.color = ColorGold;

            // 7. BUILD 10 TERMINAL BUTTONS FOR MODE B
            _terminalButtons.Clear();
            CreateTerminalButton(boardGo.transform, T_BAT_POS, "BAT+", _terminalPositions[T_BAT_POS]);
            CreateTerminalButton(boardGo.transform, T_BAT_NEG, "BAT-", _terminalPositions[T_BAT_NEG]);
            CreateTerminalButton(boardGo.transform, T_SW_A,    "SW_A", _terminalPositions[T_SW_A]);
            CreateTerminalButton(boardGo.transform, T_SW_B,    "SW_B", _terminalPositions[T_SW_B]);
            CreateTerminalButton(boardGo.transform, T_J_POS,   "J+",   _terminalPositions[T_J_POS]);
            CreateTerminalButton(boardGo.transform, T_J_NEG,   "J-",   _terminalPositions[T_J_NEG]);
            CreateTerminalButton(boardGo.transform, T_R1_A,    "R1_A", _terminalPositions[T_R1_A]);
            CreateTerminalButton(boardGo.transform, T_R1_B,    "R1_B", _terminalPositions[T_R1_B]);
            CreateTerminalButton(boardGo.transform, T_R2_A,    "R2_A", _terminalPositions[T_R2_A]);
            CreateTerminalButton(boardGo.transform, T_R2_B,    "R2_B", _terminalPositions[T_R2_B]);
        }

        private void CreateTerminalButton(Transform parent, string terminalId, string displayLabel, Vector2 pos)
        {
            GameObject termGo = new GameObject($"Term_{terminalId}");
            termGo.transform.SetParent(parent, false);

            RectTransform rt = termGo.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(38, 38);

            Image img = termGo.AddComponent<Image>();
            img.color = ColorTerminalNormal;

            var outline = termGo.AddComponent<Outline>();
            outline.effectColor = ColorGold;
            outline.effectDistance = new Vector2(2, -2);

            Button btn = termGo.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => OnTerminalClicked(terminalId));
            termGo.AddComponent<UIButtonFeedback>();

            GameObject txtGo = new GameObject("Label");
            txtGo.transform.SetParent(termGo.transform, false);
            RectTransform tRt = txtGo.AddComponent<RectTransform>();
            tRt.sizeDelta = rt.sizeDelta;

            var tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.text = displayLabel;
            tmp.fontSize = 11;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            _terminalButtons[terminalId] = btn;
        }

        private Image CreateComponentCard(Transform parent, string name, Vector2 pos, Vector2 size, string defaultText, out TextMeshProUGUI textRef)
        {
            GameObject cardGo = new GameObject(name);
            cardGo.transform.SetParent(parent, false);
            RectTransform rt = cardGo.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = cardGo.AddComponent<Image>();
            img.color = new Color(0.10f, 0.18f, 0.32f, 0.95f);

            var outline = cardGo.AddComponent<Outline>();
            outline.effectColor = ColorGoldBorder;
            outline.effectDistance = new Vector2(1, -1);

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(cardGo.transform, false);
            RectTransform tRt = textGo.AddComponent<RectTransform>();
            tRt.sizeDelta = size;

            textRef = textGo.AddComponent<TextMeshProUGUI>();
            textRef.text = defaultText;
            textRef.fontSize = 15;
            textRef.fontStyle = FontStyles.Bold;
            textRef.alignment = TextAlignmentOptions.Center;
            textRef.color = Color.white;

            return img;
        }

        private void BuildRightPanel()
        {
            GameObject rightPanel = CreateNavyPanel(_hudRoot.transform, "RightPanel", new Vector2(670, 20), new Vector2(400, 780));

            CreatePanelHeader(rightPanel.transform, "SỐ ĐO VẬT LÝ REALTIME", new Vector2(0, 345));

            // Main Readout Card
            GameObject mainReadoutCard = CreateNavyPanel(rightPanel.transform, "MainReadoutCard", new Vector2(0, 210), new Vector2(360, 210));
            GameObject mrTxtGo = new GameObject("MainText");
            mrTxtGo.transform.SetParent(mainReadoutCard.transform, false);
            RectTransform mrRt = mrTxtGo.AddComponent<RectTransform>();
            mrRt.sizeDelta = new Vector2(330, 180);
            _txtReadoutMain = mrTxtGo.AddComponent<TextMeshProUGUI>();
            _txtReadoutMain.fontSize = 16;
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
            _txtReadoutSecondary.fontSize = 15;
            _txtReadoutSecondary.lineSpacing = 8;
            _txtReadoutSecondary.alignment = TextAlignmentOptions.Left;
            _txtReadoutSecondary.color = Color.white;

            // Formula Card
            GameObject formulaCard = CreateNavyPanel(rightPanel.transform, "FormulaCard", new Vector2(0, -250), new Vector2(360, 160));
            CreatePanelHeader(formulaCard.transform, "CÔNG THỨC TRỌNG TÂM", new Vector2(0, 55));

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
            // Feedback text
            GameObject fbGo = new GameObject("FeedbackText");
            fbGo.transform.SetParent(_hudRoot.transform, false);
            RectTransform fbRt = fbGo.AddComponent<RectTransform>();
            fbRt.anchoredPosition = new Vector2(20, -265);
            fbRt.sizeDelta = new Vector2(850, 40);

            _txtFeedback = fbGo.AddComponent<TextMeshProUGUI>();
            _txtFeedback.text = "Chào mừng tới Xưởng mạch điện Aurelia. Hãy thiết lập mạch và đóng công tắc!";
            _txtFeedback.fontSize = 16;
            _txtFeedback.fontStyle = FontStyles.Bold;
            _txtFeedback.alignment = TextAlignmentOptions.Center;
            _txtFeedback.color = new Color(0.9f, 0.95f, 1f, 0.9f);

            // Action Buttons
            CreateButton(_hudRoot.transform, "Btn_Hint", new Vector2(-260, -320), new Vector2(170, 50), "GỢI Ý", OnHintClicked);
            CreateButton(_hudRoot.transform, "Btn_Reset", new Vector2(-70, -320), new Vector2(170, 50), "LÀM LẠI", () => ResetCurrentChallenge(true));
            CreateButton(_hudRoot.transform, "Btn_ToggleSwitch", new Vector2(120, -320), new Vector2(170, 50), "ĐÓNG / MỞ", ToggleSwitch);
            CreateButton(_hudRoot.transform, "Btn_Check", new Vector2(310, -320), new Vector2(170, 50), "KIỂM TRA", OnCheckClicked, true);
        }

        private void BuildModals()
        {
            // 1. TUTORIAL MODAL
            _tutorialModal = CreateNavyPanel(_hudRoot.transform, "TutorialModal", Vector2.zero, new Vector2(720, 480));
            CreatePanelHeader(_tutorialModal.transform, "HƯỚNG DẪN · XƯỞNG MẠCH ĐIỆN AURELIA", new Vector2(0, 180));

            GameObject tutBodyGo = new GameObject("TutorialBody");
            tutBodyGo.transform.SetParent(_tutorialModal.transform, false);
            RectTransform tbRt = tutBodyGo.AddComponent<RectTransform>();
            tbRt.anchoredPosition = new Vector2(0, 15);
            tbRt.sizeDelta = new Vector2(620, 260);

            var tbTmp = tutBodyGo.AddComponent<TextMeshProUGUI>();
            tbTmp.text = "<b>Chào mừng pháp sư đến với Xưởng Mạch Điện Aurelia!</b>\n\n" +
                         "1. <b>Lắp ráp hoặc chọn thông số:</b> Bấm các chốt (Terminal) để kéo dây nối mạch hoặc chọn nguồn/tải.\n" +
                         "2. <b>Đóng công tắc:</b> Cho phép dòng điện chạy qua các dây dẫn để tạo mạch kín.\n" +
                         "3. <b>Quan sát số đo:</b> Đối chiếu số đo Ampe kế, Vôn kế và Oát kế với yêu cầu.\n" +
                         "4. <b>Bấm KIỂM TRA:</b> Hoàn thành nhiệm vụ để nhận các linh kiện ma thuật quý hiếm!";
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
            save.hasSeenElectricityTutorial = true;
            PhysicsSaveManager.Save(save);

            if (_tutorialModal != null) _tutorialModal.SetActive(false);
        }

        private void ShowSuccessModal(string rewardItemId, bool isNewItem, bool isWireBonusUnlocked)
        {
            if (_successModal == null) return;

            foreach (Transform child in _successModal.transform)
            {
                Destroy(child.gameObject);
            }

            string lessonSuccessTitle = _currentMode == CircuitMode.OhmLab ? "ĐỊNH LUẬT OHM ĐÃ ĐƯỢC KIỂM CHỨNG!" :
                                        _currentMode == CircuitMode.CircuitBuilder ? "MẠCH ĐIỆN HOẠT ĐỘNG HOÀN HẢO!" :
                                        "NĂNG LƯỢNG ĐÃ ĐƯỢC CẤP CHÍNH XÁC!";

            CreatePanelHeader(_successModal.transform, lessonSuccessTitle, new Vector2(0, 185));

            GameObject bodyGo = new GameObject("SuccessBody");
            bodyGo.transform.SetParent(_successModal.transform, false);
            RectTransform bRt = bodyGo.AddComponent<RectTransform>();
            bRt.anchoredPosition = new Vector2(0, 25);
            bRt.sizeDelta = new Vector2(640, 240);

            var bTmp = bodyGo.AddComponent<TextMeshProUGUI>();
            string itemNotice = isNewItem ? $"<color=#55FF88>Nhận vật phẩm mới: {rewardItemId.ToUpper()}</color>" : $"Vật phẩm: {rewardItemId.ToUpper()} (Đã sở hữu)";
            string wireNotice = isWireBonusUnlocked ? "\n<color=#FFE600>★ ĐẶC BIỆT: Mở khóa vật phẩm thưởng WIRE (Dây dẫn) vì hoàn thành trọn bộ Chương III!</color>" : "";

            bTmp.text = $"<b>Chúc mừng bạn đã hoàn thành bài học: {_lessonTitle}!</b>\n\n" +
                        $"• Tiến trình khám phá: <color=#55FF88>Đã ghi nhận</color>\n" +
                        $"• Huy hiệu thực hành Chương III: <color=#55FF88>Đã lưu</color>\n" +
                        $"• {itemNotice}{wireNotice}\n\n" +
                        $"<i>Học viện Aurelia ghi nhận thành tích xuất sắc của bạn trong việc làm chủ dòng điện!</i>";
            bTmp.fontSize = 16;
            bTmp.lineSpacing = 6;
            bTmp.color = Color.white;

            CreateButton(_successModal.transform, "Btn_Continue", new Vector2(-140, -175), new Vector2(220, 50), "TIẾP TỤC HỌC", () =>
            {
                _successModal.SetActive(false);
                Transform targetCanvas = _canvas != null ? _canvas.transform : null;
                StoryFlowController.TryShowPostGameplayOrPendingStories(
                    completedChapterHint: 3,
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
            tmp.fontSize = isGold ? 17 : 15;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = isGold ? new Color(0.08f, 0.12f, 0.24f, 1f) : Color.white;

            btn.onClick.AddListener(() => onClick?.Invoke());
            btnGo.AddComponent<UIButtonFeedback>();

            return btnGo;
        }
    }
}
