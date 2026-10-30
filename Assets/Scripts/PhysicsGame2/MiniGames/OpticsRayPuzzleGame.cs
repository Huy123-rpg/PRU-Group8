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
    /// Mini-game Chương II: Ánh sáng – DẪN ĐƯỜNG ÁNH SÁNG (LIGHT PATH CHALLENGE).
    /// Đáp ứng 6 bài học qua 3 Game Mode:
    /// - MODE A (Refraction & TIR): C2_L5 (Khúc xạ), C2_L6 (Phản xạ toàn phần)
    /// - MODE B (Prism): C2_L7 (Lăng kính & tán sắc ánh sáng)
    /// - MODE C (Lens): C2_L8 (Thấu kính hội tụ), C2_L9 (Đo tiêu cự), C2_L10 (Kính lúp)
    /// </summary>
    public class OpticsRayPuzzleGame : MonoBehaviour
    {
        public enum OpticsMode
        {
            Refraction,     // C2_L5
            TotalReflection,// C2_L6
            Prism,          // C2_L7
            LensFocus,      // C2_L8
            LensMeasure,    // C2_L9
            Magnifier       // C2_L10
        }

        // === PARAMETERS ===
        private string _lessonId = "C2_L5";
        private string _lessonTitle = "Khúc xạ ánh sáng";
        private OpticsMode _currentMode = OpticsMode.Refraction;

        // Mode A: Góc tới i (độ)
        private float _angleIncidence = 20f; // Default FAIL cho cả L5 và L6
        private const float N1_AIR = 1.0f;
        private const float N2_GLASS = 1.5f;

        // Mode B: Góc xoay lăng kính theta (độ)
        private float _prismAngle = -30f; // Default FAIL cho L7

        // Mode C - L8: Vị trí thấu kính X (px)
        private float _lensPosX = -200f; // Default FAIL cho L8

        // Mode C - L9: Vị trí màn hứng X (px)
        private float _screenPosX = 240f; // Default FAIL cho L9 (d' = 30cm, f != 20cm)

        // Mode C - L10: Vị trí vật X (px)
        private float _objectPosX = -250f; // Default FAIL cho L10 (d = 25cm > f = 15cm)

        // Hit state
        private bool _isTargetHit = false;
        private bool _isCompleted = false;
        private bool _storyGateActive = false;
        private bool _enteredViaNormalFlow = false;
        public bool IsStoryGateActive => _storyGateActive || StoryFlowController.IsStoryModalOpen;

        // UI Root & References
        private Canvas _canvas;
        private GameObject _hudRoot;
        private GameObject _tutorialModal;
        private GameObject _successModal;

        // Panels & Text
        private TextMeshProUGUI _txtLessonBadge;
        private TextMeshProUGUI _txtMissionTitle;
        private TextMeshProUGUI _txtMissionDesc;
        private TextMeshProUGUI _txtPhysicsReadout;
        private TextMeshProUGUI _txtFeedback;
        private Coroutine _feedbackCoroutine;

        // Optical Chamber & Components
        private RectTransform _chamberRt;
        private RectTransform _emitterRt;
        private RectTransform _interfaceLineRt;
        private RectTransform _normalLineRt;
        private RectTransform _prismRt;
        private RectTransform _lensRt;
        private RectTransform _screenRt;
        private RectTransform _objectRt;
        private RectTransform _targetCrystalRt;
        private Image _crystalGlowImg;
        private TextMeshProUGUI _crystalBadgeTxt;

        // Controls
        private Slider _sliderControl;
        private TextMeshProUGUI _sliderValTxt;
        private TextMeshProUGUI _sliderLabelTxt;

        // Ray Segments Pool
        private Transform _rayContainer;
        private readonly List<Image> _rayPool = new List<Image>();

        // Colors
        private static readonly Color ColorNavyBg = new Color(0.06f, 0.11f, 0.22f, 0.94f);
        private static readonly Color ColorGold = new Color(0.96f, 0.82f, 0.38f, 1f);
        private static readonly Color ColorGoldBorder = new Color(0.85f, 0.70f, 0.28f, 0.85f);
        private static readonly Color ColorIncidentRay = new Color(1.0f, 0.93f, 0.45f, 0.95f); // Vàng sáng
        private static readonly Color ColorRefractedRay = new Color(0.15f, 0.85f, 1.0f, 0.95f); // Xanh cyan
        private static readonly Color ColorReflectedRay = new Color(1.0f, 0.75f, 0.20f, 0.95f); // Vàng cam
        private static readonly Color ColorSuccessGreen = new Color(0.30f, 0.95f, 0.45f, 1f);
        private static readonly Color ColorAlertRed = new Color(1.0f, 0.45f, 0.40f, 1f);

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
            ResetPuzzleToInitialState();

            var save = PhysicsSaveManager.Load();
            if (_enteredViaNormalFlow && save != null && !save.hasSeenCore2Intro)
            {
                if (_tutorialModal != null) _tutorialModal.SetActive(false);
                SetStoryGate(true);
                StoryFlowController.TryShowGameplayCoreIntro(
                    2,
                    customSave: null,
                    onIntroFinishedOrSkipped: () =>
                    {
                        SetStoryGate(false);
                        var updatedSave = PhysicsSaveManager.Load();
                        bool shouldShowTutorial = updatedSave == null || !updatedSave.hasSeenOpticsTutorial;
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
                bool shouldShowTutorial = save == null || !save.hasSeenOpticsTutorial;
                if (_tutorialModal != null)
                {
                    _tutorialModal.SetActive(shouldShowTutorial);
                }
            }
        }

        public void SetStoryGate(bool active)
        {
            _storyGateActive = active;
            if (_sliderControl != null) _sliderControl.interactable = !active;
        }

        private void Update()
        {
            if (IsStoryGateActive) return;

            // Hiệu ứng nhịp thở nhẹ cho Tinh thể mục tiêu
            if (_crystalGlowImg != null)
            {
                float pulse = 0.5f + 0.35f * Mathf.Sin(Time.unscaledTime * 3.5f);
                Color c = _isTargetHit ? ColorSuccessGreen : ColorGold;
                c.a = pulse;
                _crystalGlowImg.color = c;
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
                if (string.IsNullOrEmpty(id) || !id.StartsWith("C2_"))
                {
                    PhysicsGameSession.Instance.SelectChapter(2);
                    PhysicsGameSession.Instance.SelectLesson("C2_L5", "Khúc xạ ánh sáng");
                }
            }
        }

        // =========================================================================
        // KHỞI TẠO BÀI HỌC & GAME MODE
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
                _lessonId = "C2_L5";
                _lessonTitle = "Khúc xạ ánh sáng";
            }

            switch (_lessonId)
            {
                case "C2_L5":
                    _currentMode = OpticsMode.Refraction;
                    _lessonTitle = "Bài 5. Khúc xạ ánh sáng";
                    break;
                case "C2_L6":
                    _currentMode = OpticsMode.TotalReflection;
                    _lessonTitle = "Bài 6. Phản xạ toàn phần";
                    break;
                case "C2_L7":
                    _currentMode = OpticsMode.Prism;
                    _lessonTitle = "Bài 7. Lăng kính";
                    break;
                case "C2_L8":
                    _currentMode = OpticsMode.LensFocus;
                    _lessonTitle = "Bài 8. Thấu kính hội tụ";
                    break;
                case "C2_L9":
                    _currentMode = OpticsMode.LensMeasure;
                    _lessonTitle = "Bài 9. Thực hành đo tiêu cự";
                    break;
                case "C2_L10":
                    _currentMode = OpticsMode.Magnifier;
                    _lessonTitle = "Bài 10. Kính lúp";
                    break;
                default:
                    _currentMode = OpticsMode.Refraction;
                    _lessonTitle = "Bài 5. Khúc xạ ánh sáng";
                    _lessonId = "C2_L5";
                    break;
            }

            if (_txtLessonBadge != null)
            {
                _txtLessonBadge.text = _lessonTitle;
            }

            ConfigureUIForMode();
        }

        private void ResetPuzzleToInitialState()
        {
            _isCompleted = false;
            _isTargetHit = false;

            // Khởi tạo các giá trị MẶC ĐỊNH Ở TRẠNG THÁI FAIL (Không bao giờ auto-win)
            switch (_currentMode)
            {
                case OpticsMode.Refraction:
                    _angleIncidence = 20f; // Target ở i = 50 độ
                    if (_sliderControl != null) { _sliderControl.minValue = 0f; _sliderControl.maxValue = 80f; _sliderControl.value = _angleIncidence; }
                    break;

                case OpticsMode.TotalReflection:
                    _angleIncidence = 25f; // < ic (41.8 độ), khúc xạ ra ngoài, target ở i = 60 độ
                    if (_sliderControl != null) { _sliderControl.minValue = 0f; _sliderControl.maxValue = 80f; _sliderControl.value = _angleIncidence; }
                    break;

                case OpticsMode.Prism:
                    _prismAngle = -35f; // Target ở theta = +10 độ
                    if (_sliderControl != null) { _sliderControl.minValue = -45f; _sliderControl.maxValue = 45f; _sliderControl.value = _prismAngle; }
                    break;

                case OpticsMode.LensFocus:
                    _lensPosX = -200f; // F' = 0px, target ở 220px
                    if (_sliderControl != null) { _sliderControl.minValue = -260f; _sliderControl.maxValue = 120f; _sliderControl.value = _lensPosX; }
                    break;

                case OpticsMode.LensMeasure:
                    _screenPosX = 240f; // d' = 30cm, f = 15cm != 20cm, target ở d' = 60cm (480px)
                    if (_sliderControl != null) { _sliderControl.minValue = 160f; _sliderControl.maxValue = 520f; _sliderControl.value = _screenPosX; }
                    break;

                case OpticsMode.Magnifier:
                    _objectPosX = -250f; // d = 25cm > f = 15cm, ảnh thật, target ở d < 15cm ([-120, -40]px)
                    if (_sliderControl != null) { _sliderControl.minValue = -320f; _sliderControl.maxValue = -40f; _sliderControl.value = _objectPosX; }
                    break;
            }

            UpdateApparatusVisibility();
            CalculateAndRenderOptics();
            ShowFeedback("Hãy điều chỉnh dụng cụ quang học và nhấn <b>KIỂM TRA MỤC TIÊU</b>.", new Color(0.85f, 0.9f, 1f, 0.9f));
        }

        private void ConfigureUIForMode()
        {
            switch (_currentMode)
            {
                case OpticsMode.Refraction:
                    _txtMissionTitle.text = "NHIỆM VỤ: KHÚC XẠ ÁNH SÁNG";
                    _txtMissionDesc.text = "Điều chỉnh <b>góc tới i</b> để tia khúc xạ qua mặt phân cách (n1=1.0 sang n2=1.5) chạm vào Tinh thể năng lượng.";
                    if (_sliderLabelTxt != null) _sliderLabelTxt.text = "Góc tới (i):";
                    break;

                case OpticsMode.TotalReflection:
                    _txtMissionTitle.text = "NHIỆM VỤ: PHẢN XẠ TOÀN PHẦN";
                    _txtMissionDesc.text = "Tia sáng truyền từ thủy tinh (n1=1.5) ra không khí (n2=1.0). Tăng <b>góc tới i > ic</b> để xảy ra phản xạ toàn phần và dẫn tia sáng chạm Tinh thể.";
                    if (_sliderLabelTxt != null) _sliderLabelTxt.text = "Góc tới (i):";
                    break;

                case OpticsMode.Prism:
                    _txtMissionTitle.text = "NHIỆM VỤ: LĂNG KÍNH QUANG HỌC";
                    _txtMissionDesc.text = "Xoay <b>lăng kính tam giác</b> để điều chỉnh góc lệch của chùm tia ló và dẫn ánh sáng chiếu trúng Tinh thể.";
                    if (_sliderLabelTxt != null) _sliderLabelTxt.text = "Góc xoay lăng kính (θ):";
                    break;

                case OpticsMode.LensFocus:
                    _txtMissionTitle.text = "NHIỆM VỤ: TIÊU ĐIỂM THẤU KÍNH";
                    _txtMissionDesc.text = "Di chuyển <b>thấu kính hội tụ</b> theo trục chính sao cho tiêu điểm <b>F'</b> trùng khớp với Tinh thể mục tiêu.";
                    if (_sliderLabelTxt != null) _sliderLabelTxt.text = "Vị trí thấu kính (X):";
                    break;

                case OpticsMode.LensMeasure:
                    _txtMissionTitle.text = "NHIỆM VỤ: THỰC HÀNH ĐO TIÊU CỰ";
                    _txtMissionDesc.text = "Vật đặt cách thấu kính d = 30 cm. Di chuyển <b>màn hứng</b> để thu được ảnh rõ nét nhất và xác định tiêu cự f = 20 cm.";
                    if (_sliderLabelTxt != null) _sliderLabelTxt.text = "Vị trí màn hứng (d'):";
                    break;

                case OpticsMode.Magnifier:
                    _txtMissionTitle.text = "NHIỆM VỤ: NGUYÊN LÝ KÍNH LÚP";
                    _txtMissionDesc.text = "Kính lúp có tiêu cự f = 15 cm. Kéo <b>vật vào trong khoảng tiêu cự (d < f)</b> để quan sát ảnh ảo phóng đại.";
                    if (_sliderLabelTxt != null) _sliderLabelTxt.text = "Vị trí vật (d):";
                    break;
            }
        }

        private void UpdateApparatusVisibility()
        {
            bool isModeA = _currentMode == OpticsMode.Refraction || _currentMode == OpticsMode.TotalReflection;
            bool isModeB = _currentMode == OpticsMode.Prism;
            bool isModeC = _currentMode == OpticsMode.LensFocus || _currentMode == OpticsMode.LensMeasure || _currentMode == OpticsMode.Magnifier;

            if (_interfaceLineRt != null) _interfaceLineRt.gameObject.SetActive(isModeA);
            if (_normalLineRt != null) _normalLineRt.gameObject.SetActive(isModeA || isModeC);
            if (_emitterRt != null) _emitterRt.gameObject.SetActive(isModeA || isModeB);
            if (_prismRt != null) _prismRt.gameObject.SetActive(isModeB);
            if (_lensRt != null) _lensRt.gameObject.SetActive(isModeC);
            if (_screenRt != null) _screenRt.gameObject.SetActive(_currentMode == OpticsMode.LensMeasure);
            if (_objectRt != null) _objectRt.gameObject.SetActive(_currentMode == OpticsMode.LensMeasure || _currentMode == OpticsMode.Magnifier);

            // Vị trí Target Crystal theo từng bài
            if (_targetCrystalRt != null)
            {
                switch (_currentMode)
                {
                    case OpticsMode.Refraction:
                        // Target ở môi trường 2: r = 30.71 độ (ứng với i = 50 độ), bán kính R = 380px
                        // r_rad = 30.71 * deg2rad, x = 380 * cos(r), y = -380 * sin(r)
                        _targetCrystalRt.anchoredPosition = new Vector2(326f, -194f);
                        break;

                    case OpticsMode.TotalReflection:
                        // Target bên trong môi trường 1: phản xạ tại góc 60 độ, x = -380 * cos(60) = -190, y = -380 * sin(60) = -329
                        // Hoặc đặt ở tọa độ phản xạ thuận lợi trong chamber
                        _targetCrystalRt.anchoredPosition = new Vector2(-190f, -220f);
                        break;

                    case OpticsMode.Prism:
                        _targetCrystalRt.anchoredPosition = new Vector2(380f, -270f);
                        break;

                    case OpticsMode.LensFocus:
                        _targetCrystalRt.anchoredPosition = new Vector2(220f, 0f); // Trên trục chính
                        break;

                    case OpticsMode.LensMeasure:
                        _targetCrystalRt.anchoredPosition = new Vector2(480f, 0f); // Vị trí màn hứng chuẩn (d' = 60cm -> 480px)
                        break;

                    case OpticsMode.Magnifier:
                        _targetCrystalRt.anchoredPosition = new Vector2(0f, 60f); // Tại thấu kính
                        break;
                }
            }
        }

        // =========================================================================
        // TÍNH TOÁN VẬT LÝ & HIỂN THỊ TIA SÁNG THỜI GIAN THỰC
        // =========================================================================
        private void CalculateAndRenderOptics()
        {
            ClearRays();
            _isTargetHit = false;

            switch (_currentMode)
            {
                case OpticsMode.Refraction:
                    SolveRefractionMode();
                    break;

                case OpticsMode.TotalReflection:
                    SolveTotalReflectionMode();
                    break;

                case OpticsMode.Prism:
                    SolvePrismMode();
                    break;

                case OpticsMode.LensFocus:
                    SolveLensFocusMode();
                    break;

                case OpticsMode.LensMeasure:
                    SolveLensMeasureMode();
                    break;

                case OpticsMode.Magnifier:
                    SolveMagnifierMode();
                    break;
            }

            // Cập nhật trạng thái crystal
            if (_crystalBadgeTxt != null)
            {
                _crystalBadgeTxt.text = _isTargetHit ? "<color=#55FF88>TRÚNG ĐÍCH!</color>" : "TINH THỂ";
            }
        }

        // --- MODE A1: C2_L5 KHÚC XẠ ÁNH SÁNG ---
        private void SolveRefractionMode()
        {
            float i = Mathf.Clamp(_angleIncidence, 0f, 80f);
            float iRad = i * Mathf.Deg2Rad;

            // Snell: n1 * sin(i) = n2 * sin(r) -> sin(r) = (n1/n2) * sin(i)
            float sinR = Mathf.Clamp((N1_AIR / N2_GLASS) * Mathf.Sin(iRad), -1f, 1f);
            float rRad = Mathf.Asin(sinR);
            float r = rRad * Mathf.Rad2Deg;

            // Vị trí Emitter bên trái: bán kính R = 340px, góc tới i so với pháp tuyến (trục X)
            // Tia tới xuất phát từ (-340 * cos(i), 340 * sin(i)) hướng về (0, 0)
            Vector2 origin = Vector2.zero;
            Vector2 emitterPos = new Vector2(-340f * Mathf.Cos(iRad), 340f * Mathf.Sin(iRad));
            if (_emitterRt != null)
            {
                _emitterRt.anchoredPosition = emitterPos;
                float emitterAngle = Mathf.Atan2(-emitterPos.y, -emitterPos.x) * Mathf.Rad2Deg;
                _emitterRt.localRotation = Quaternion.Euler(0, 0, emitterAngle);
            }

            // Tia tới (Vàng)
            DrawRay(emitterPos, origin, ColorIncidentRay, 5f);

            // Tia khúc xạ (Cyan) sang môi trường 2: hướng (cos(r), -sin(r)) với độ dài 420px
            Vector2 refractedDir = new Vector2(Mathf.Cos(rRad), -Mathf.Sin(rRad)).normalized;
            Vector2 refractedEnd = origin + refractedDir * 420f;
            DrawRay(origin, refractedEnd, ColorRefractedRay, 5f);

            // Kiểm tra hit target (target tại Vector2(326, -194), ứng với r_target = 30.71 độ)
            Vector2 targetPos = _targetCrystalRt != null ? _targetCrystalRt.anchoredPosition : new Vector2(326f, -194f);
            float distToTarget = DistancePointToSegment(targetPos, origin, refractedEnd);
            _isTargetHit = distToTarget <= 35f;

            if (_txtPhysicsReadout != null)
            {
                _txtPhysicsReadout.text = $"<b>Định luật Snell:</b> n₁ · sin(i) = n₂ · sin(r)\n" +
                                          $"Chiết suất: n₁ = 1.0 (Không khí), n₂ = 1.5 (Thủy tinh)\n" +
                                          $"<b>Góc tới:</b> <color=#FFE600>i = {i:F1}°</color>   |   " +
                                          $"<b>Góc khúc xạ:</b> <color=#00E5FF>r = {r:F1}°</color>\n" +
                                          $"<i>Nhận xét: n₁ < n₂ nên r < i (Tia khúc xạ lệch về phía pháp tuyến).</i>";
            }
        }

        // --- MODE A2: C2_L6 PHẢN XẠ TOÀN PHẦN ---
        private void SolveTotalReflectionMode()
        {
            float i = Mathf.Clamp(_angleIncidence, 0f, 80f);
            float iRad = i * Mathf.Deg2Rad;

            // n1 = 1.5 (thủy tinh), n2 = 1.0 (không khí)
            // Góc giới hạn: ic = asin(n2 / n1) = asin(1.0 / 1.5) approx 41.81 độ
            float ic = Mathf.Asin(N1_AIR / N2_GLASS) * Mathf.Rad2Deg;
            bool isTIR = i > ic;

            // Emitter nằm trong môi trường 1 (bên trái)
            Vector2 origin = Vector2.zero;
            Vector2 emitterPos = new Vector2(-340f * Mathf.Cos(iRad), 340f * Mathf.Sin(iRad));
            if (_emitterRt != null)
            {
                _emitterRt.anchoredPosition = emitterPos;
                float emitterAngle = Mathf.Atan2(-emitterPos.y, -emitterPos.x) * Mathf.Rad2Deg;
                _emitterRt.localRotation = Quaternion.Euler(0, 0, emitterAngle);
            }

            // Tia tới (Vàng)
            DrawRay(emitterPos, origin, ColorIncidentRay, 5f);

            if (!isTIR)
            {
                // Khúc xạ ra không khí
                float sinR = Mathf.Clamp((N2_GLASS / N1_AIR) * Mathf.Sin(iRad), -1f, 1f);
                float rRad = Mathf.Asin(sinR);
                float r = rRad * Mathf.Rad2Deg;
                Vector2 refractedDir = new Vector2(Mathf.Cos(rRad), -Mathf.Sin(rRad)).normalized;
                Vector2 refractedEnd = origin + refractedDir * 380f;
                DrawRay(origin, refractedEnd, ColorRefractedRay, 4f);

                // Tia phản xạ một phần yếu bên trong
                Vector2 partialReflectDir = new Vector2(-Mathf.Cos(iRad), -Mathf.Sin(iRad)).normalized;
                DrawRay(origin, origin + partialReflectDir * 200f, new Color(ColorReflectedRay.r, ColorReflectedRay.g, ColorReflectedRay.b, 0.35f), 2f);

                _isTargetHit = false; // Target nằm bên trong buồng quang học, cần tia phản xạ toàn phần mạnh

                if (_txtPhysicsReadout != null)
                {
                    _txtPhysicsReadout.text = $"<b>Góc tới:</b> <color=#FFE600>i = {i:F1}°</color>   |   " +
                                              $"<b>Góc giới hạn:</b> <color=#FF8800>ic ≈ {ic:F1}°</color>\n" +
                                              $"Trạng thái: <color=#00E5FF>i ≤ ic → Ánh sáng bị khúc xạ ra ngoài (r = {r:F1}°)</color>\n" +
                                              $"<i>Cần tăng góc tới i > 41.8° để triệt tiêu tia khúc xạ và tạo phản xạ toàn phần.</i>";
                }
            }
            else
            {
                // PHẢN XẠ TOÀN PHẦN BÊN TRONG MÔI TRƯỜNG 1
                // Góc phản xạ = góc tới: tia phản xạ đi theo hướng (-cos(i), -sin(i))
                Vector2 reflectedDir = new Vector2(-Mathf.Cos(iRad), -Mathf.Sin(iRad)).normalized;
                Vector2 reflectedEnd = origin + reflectedDir * 380f;
                DrawRay(origin, reflectedEnd, ColorReflectedRay, 6f); // Tia phản xạ toàn phần rất sáng

                // Hit target bên trong chamber tại Vector2(-190, -220) (ứng với góc xấp xỉ 60 độ)
                Vector2 targetPos = _targetCrystalRt != null ? _targetCrystalRt.anchoredPosition : new Vector2(-190f, -220f);
                float distToTarget = DistancePointToSegment(targetPos, origin, reflectedEnd);
                _isTargetHit = distToTarget <= 35f;

                if (_txtPhysicsReadout != null)
                {
                    _txtPhysicsReadout.text = $"<b>Góc tới:</b> <color=#FFE600>i = {i:F1}°</color>   |   " +
                                              $"<b>Góc giới hạn:</b> <color=#FF8800>ic ≈ {ic:F1}°</color>\n" +
                                              $"Trạng thái: <color=#55FF88>PHẢN XẠ TOÀN PHẦN (i > ic = {ic:F1}°)!</color>\n" +
                                              $"<b>Góc phản xạ:</b> i' = i = {i:F1}°   |   Không có tia khúc xạ thoát ra ngoài.";
                }
            }
        }

        // --- MODE B: C2_L7 LĂNG KÍNH (KHÚC XẠ 2 MẶT & ĐỊNH LUẬT SNELL CHUẨN) ---
        private void SolvePrismMode()
        {
            float theta = Mathf.Clamp(_prismAngle, -45f, 45f);
            if (_prismRt != null)
            {
                _prismRt.localRotation = Quaternion.Euler(0, 0, theta);
            }

            Vector2 sourcePos = new Vector2(-420f, 0f);
            Vector2 prismIn = new Vector2(-35f, 0f);
            Vector2 prismOut = new Vector2(35f, -10f);

            if (_emitterRt != null)
            {
                _emitterRt.anchoredPosition = sourcePos;
                _emitterRt.localRotation = Quaternion.identity;
            }

            // Tia tới vào lăng kính (Trắng/Vàng)
            DrawRay(sourcePos, prismIn, ColorIncidentRay, 5f);

            // KHÚC XẠ 2 MẶT SỬ DỤNG ĐỊNH LUẬT SNELL (Sách giáo khoa Vật lý 9)
            // Lăng kính tam giác đều có góc chiết quang A = 60°, chiết suất n = 1.5 trong không khí (n0 = 1.0)
            const float A_deg = 60f;
            const float nPrism = 1.5f;

            // 1. Khúc xạ tại mặt vào (Face 1):
            // Góc tới: i1 = 30° + theta (được giới hạn từ 5° đến 85° để tránh tia song song mặt lăng kính)
            float i1 = Mathf.Clamp(30f + theta, 5f, 85f);
            float i1Rad = i1 * Mathf.Deg2Rad;

            // n1 · sin(i1) = n2 · sin(r1) -> sin(r1) = sin(i1) / nPrism
            float sinR1 = Mathf.Clamp(Mathf.Sin(i1Rad) / nPrism, -1f, 1f);
            float r1Rad = Mathf.Asin(sinR1);
            float r1 = r1Rad * Mathf.Rad2Deg;

            // 2. Góc tới tại mặt ra (Face 2):
            // Theo định lý hình học lăng kính: r1 + i2 = A -> i2 = A - r1
            float i2 = A_deg - r1;
            float i2Rad = i2 * Mathf.Deg2Rad;

            // Góc giới hạn phản xạ toàn phần của thủy tinh n=1.5 ra không khí: ic = asin(1/1.5) ≈ 41.81°
            float ic = Mathf.Asin(1f / nPrism) * Mathf.Rad2Deg;
            bool isTirAtExit = i2 > ic;

            // Tia truyền bên trong lăng kính
            DrawRay(prismIn, prismOut, new Color(1f, 1f, 0.8f, 0.85f), 4f);

            if (isTirAtExit)
            {
                // Phản xạ toàn phần tại mặt thứ hai
                Vector2 internalReflectDir = new Vector2(0f, -1f);
                DrawRay(prismOut, prismOut + internalReflectDir * 140f, new Color(ColorReflectedRay.r, ColorReflectedRay.g, ColorReflectedRay.b, 0.5f), 3f);
                _isTargetHit = false;

                if (_txtPhysicsReadout != null)
                {
                    _txtPhysicsReadout.text = $"<b>Lăng kính (Góc chiết quang A = 60°, n = 1.5):</b>\n" +
                                              $"Mặt vào: i₁ = {i1:F1}°, r₁ = {r1:F1}°   |   " +
                                              $"Mặt ra: i₂ = {i2:F1}° > ic (41.8°)\n" +
                                              $"Trạng thái: <color=#FFAA00>Phản xạ toàn phần tại mặt ra (Tia sáng không ló ra ngoài)</color>\n" +
                                              $"<i>Hãy xoay lăng kính theo chiều kim đồng hồ để giảm i₂.</i>";
                }
            }
            else
            {
                // Khúc xạ tại mặt ra (Face 2):
                // n2 · sin(i2) = n1 · sin(r2) -> sin(r2) = nPrism · sin(i2)
                float sinR2 = Mathf.Clamp(nPrism * Mathf.Sin(i2Rad), -1f, 1f);
                float r2Rad = Mathf.Asin(sinR2);
                float r2 = r2Rad * Mathf.Rad2Deg;

                // Góc lệch của tia ló so với tia tới: D = i1 + r2 - A
                float D = i1 + r2 - A_deg;
                float outgoingAngleRad = -D * Mathf.Deg2Rad;

                // Tia trung tâm chuẩn (Vàng/Trắng)
                Vector2 centralDir = new Vector2(Mathf.Cos(outgoingAngleRad), Mathf.Sin(outgoingAngleRad)).normalized;
                Vector2 centralEnd = prismOut + centralDir * 420f;
                DrawRay(prismOut, centralEnd, ColorIncidentRay, 5f);

                // Dải tia tán sắc minh họa quang phổ (Đỏ n=1.48 lệch ít hơn, Lam n=1.52 lệch nhiều hơn)
                float r1Red = Mathf.Asin(Mathf.Clamp(Mathf.Sin(i1Rad) / 1.48f, -1f, 1f));
                float i2Red = A_deg * Mathf.Deg2Rad - r1Red;
                float r2Red = Mathf.Asin(Mathf.Clamp(1.48f * Mathf.Sin(i2Red), -1f, 1f));
                float D_red = (i1Rad + r2Red - A_deg * Mathf.Deg2Rad);
                Vector2 redDir = new Vector2(Mathf.Cos(-D_red), Mathf.Sin(-D_red)).normalized;
                DrawRay(prismOut, prismOut + redDir * 400f, new Color(1f, 0.25f, 0.25f, 0.75f), 3f);

                float r1Blue = Mathf.Asin(Mathf.Clamp(Mathf.Sin(i1Rad) / 1.52f, -1f, 1f));
                float i2Blue = A_deg * Mathf.Deg2Rad - r1Blue;
                float r2Blue = Mathf.Asin(Mathf.Clamp(1.52f * Mathf.Sin(i2Blue), -1f, 1f));
                float D_blue = (i1Rad + r2Blue - A_deg * Mathf.Deg2Rad);
                Vector2 blueDir = new Vector2(Mathf.Cos(-D_blue), Mathf.Sin(-D_blue)).normalized;
                DrawRay(prismOut, prismOut + blueDir * 400f, new Color(0.25f, 0.65f, 1f, 0.75f), 3f);

                // Hit Target (Target tại Vector2(380, -270))
                Vector2 targetPos = _targetCrystalRt != null ? _targetCrystalRt.anchoredPosition : new Vector2(380f, -270f);
                float distToTarget = DistancePointToSegment(targetPos, prismOut, centralEnd);
                _isTargetHit = distToTarget <= 35f;

                if (_txtPhysicsReadout != null)
                {
                    _txtPhysicsReadout.text = $"<b>Định luật khúc xạ qua lăng kính (A = 60°, n = 1.5):</b>\n" +
                                              $"Mặt vào: n₁·sin(i₁) = n₂·sin(r₁) → i₁ = {i1:F1}°, r₁ = {r1:F1}°   |   " +
                                              $"Mặt ra: n₂·sin(i₂) = n₁·sin(r₂) → i₂ = {i2:F1}°, r₂ = {r2:F1}°\n" +
                                              $"<b>Góc lệch của tia ló:</b> D = i₁ + r₂ - A = <color=#FFE600>{D:F1}°</color> (Lệch về phía đáy lăng kính)\n" +
                                              $"<i>Hiện tượng tán sắc: Chiết suất khác nhau theo bước sóng làm ánh sáng trắng phân tách thành dải màu.</i>";
                }
            }
        }

        // --- MODE C1: C2_L8 THẤU KÍNH HỘI TỤ (PARALLEL RAYS -> FOCAL POINT F') ---
        private void SolveLensFocusMode()
        {
            float lensX = Mathf.Clamp(_lensPosX, -260f, 120f);
            if (_lensRt != null)
            {
                _lensRt.anchoredPosition = new Vector2(lensX, 0f);
            }

            const float f = 200f; // Tiêu cự 200px
            Vector2 fPrimePos = new Vector2(lensX + f, 0f);

            // 3 chùm tia song song từ nguồn bên trái
            float[] rayYOffsets = { 50f, 0f, -50f };
            for (int i = 0; i < rayYOffsets.Length; i++)
            {
                float y = rayYOffsets[i];
                Vector2 start = new Vector2(-480f, y);
                Vector2 atLens = new Vector2(lensX, y);

                // Tia tới song song trục chính
                DrawRay(start, atLens, ColorIncidentRay, 4f);

                // Tia ló hội tụ qua tiêu điểm F' và kéo dài ra sau
                Vector2 dir = (fPrimePos - atLens).normalized;
                Vector2 rayEnd = atLens + dir * 360f;
                DrawRay(atLens, rayEnd, ColorRefractedRay, 4f);
            }

            // Target tại X = 220px trên trục chính
            Vector2 targetPos = _targetCrystalRt != null ? _targetCrystalRt.anchoredPosition : new Vector2(220f, 0f);
            float distToTarget = Mathf.Abs(fPrimePos.x - targetPos.x);
            _isTargetHit = distToTarget <= 25f;

            if (_txtPhysicsReadout != null)
            {
                _txtPhysicsReadout.text = $"<b>Thấu kính hội tụ:</b> Tiêu cự f = 200 px\n" +
                                          $"Vị trí thấu kính: X = {lensX:F0} px   |   " +
                                          $"<b>Tiêu điểm F':</b> <color=#FFE600>X = {fPrimePos.x:F0} px</color>\n" +
                                          $"Khoảng cách tới mục tiêu (X = {targetPos.x:F0} px): " +
                                          (_isTargetHit ? "<color=#55FF88>0 px (Trùng khớp!)</color>" : $"<color=#FFAA00>{distToTarget:F0} px</color>");
            }
        }

        // --- MODE C2: C2_L9 ĐO TIÊU CỰ THỰC HÀNH ---
        private void SolveLensMeasureMode()
        {
            float screenX = Mathf.Clamp(_screenPosX, 160f, 520f);
            if (_screenRt != null)
            {
                _screenRt.anchoredPosition = new Vector2(screenX, 0f);
            }

            // Thấu kính cố định tại X = 0
            if (_lensRt != null) _lensRt.anchoredPosition = Vector2.zero;

            // Vật AB tại X = -240px (d = 30cm, tỉ lệ 1cm = 8px)
            const float scale = 8f;
            float d_cm = 30f;
            float dPrime_cm = screenX / scale;
            float objX = -d_cm * scale; // -240px
            float objH = 60f; // Chiều cao vật

            if (_objectRt != null) _objectRt.anchoredPosition = new Vector2(objX, 0f);

            // Tiêu cự thực tế của thấu kính: f_true = 20cm
            // Vị trí ảnh nét lý thuyết: d' = (d * f) / (d - f) = (30 * 20) / (30 - 20) = 60cm -> 480px
            const float f_true = 20f;
            float idealScreenX = 60f * scale; // 480px

            // Công thức thấu kính: 1/f = 1/d + 1/d' -> f = (d * d') / (d + d')
            // Guard epsilon chống chia cho 0 nếu d + d' gần 0
            float dSum = d_cm + dPrime_cm;
            float calculatedF = dSum > 0.001f ? (d_cm * dPrime_cm) / dSum : 0f;

            // Vẽ tia 1: Từ đỉnh vật song song trục chính -> gặp thấu kính tại (0, objH) -> qua F' (f_true*8, 0) -> đến màn
            Vector2 objTop = new Vector2(objX, objH);
            Vector2 lensTop = new Vector2(0f, objH);
            Vector2 fPrime = new Vector2(f_true * scale, 0f);

            DrawRay(objTop, lensTop, ColorIncidentRay, 4f);
            Vector2 ray1Dir = (fPrime - lensTop).normalized;
            float t1 = Mathf.Abs(ray1Dir.x) > 0.001f ? (screenX - lensTop.x) / ray1Dir.x : 0f;
            Vector2 ray1End = lensTop + ray1Dir * t1;
            DrawRay(lensTop, ray1End, ColorRefractedRay, 4f);

            // Vẽ tia 2: Từ đỉnh vật truyền thẳng qua quang tâm O (0, 0) -> đến màn
            Vector2 ray2Dir = (Vector2.zero - objTop).normalized;
            float t2 = Mathf.Abs(ray2Dir.x) > 0.001f ? (screenX - objTop.x) / ray2Dir.x : 0f;
            Vector2 ray2End = objTop + ray2Dir * t2;
            DrawRay(objTop, ray2End, ColorRefractedRay, 4f);

            // Sai số vị trí màn so với vị trí nét (480px)
            float deltaX = Mathf.Abs(screenX - idealScreenX);
            _isTargetHit = deltaX <= 16f; // Sai số +- 2cm (16px)

            // Độ nhòe của ảnh trên màn
            float blur = Mathf.Abs(ray1End.y - ray2End.y);

            if (_txtPhysicsReadout != null)
            {
                _txtPhysicsReadout.text = $"<b>Thực nghiệm quang học:</b> Khoảng cách vật d = {d_cm:F1} cm\n" +
                                          $"Khoảng cách ảnh: <b>d' = {dPrime_cm:F1} cm</b>   |   " +
                                          $"Tiêu cự tính toán: <b>f = (d·d')/(d+d') = {calculatedF:F1} cm</b>\n" +
                                          $"Trạng thái ảnh: " + (_isTargetHit
                                              ? "<color=#55FF88>ẢNH RÕ NÉT TRÊN MÀN (f = 20.0 cm)!</color>"
                                              : $"<color=#FFAA00>Ảnh mờ (Độ lệch tia: {blur:F0} px) - Hãy kéo màn đến d' = 60 cm</color>");
            }
        }

        // --- MODE C3: C2_L10 KÍNH LÚP (MAGNIFIER d < f) ---
        private void SolveMagnifierMode()
        {
            float objX = Mathf.Clamp(_objectPosX, -320f, -40f);
            if (_objectRt != null) _objectRt.anchoredPosition = new Vector2(objX, 0f);
            if (_lensRt != null) _lensRt.anchoredPosition = Vector2.zero;

            const float f_cm = 15f; // Tiêu cự 15cm (150px)
            float d_cm = Mathf.Abs(objX) / 10f; // Scale 1cm = 10px

            // Điều kiện kính lúp SGK Vật lý 9: Vật đặt trong khoảng tiêu cự (d < f)
            // Áp dụng cho toàn bộ khoảng kéo: 4.0 cm <= d < 15.0 cm
            // Tại tiêu điểm d ≈ f (15 cm): ảnh ở vô cực, không phải trạng thái ảnh rõ kính lúp thông thường
            bool isAtFocalBoundary = Mathf.Abs(d_cm - f_cm) < 0.2f;
            bool isInsideFocal = (d_cm < f_cm) && !isAtFocalBoundary;

            _isTargetHit = d_cm >= 4.0f && d_cm < f_cm && !isAtFocalBoundary;

            // Vẽ tia sáng
            Vector2 objTop = new Vector2(objX, 50f);
            Vector2 lensTop = new Vector2(0f, 50f);
            Vector2 fPrime = new Vector2(f_cm * 10f, 0f);

            // Tia song song
            DrawRay(objTop, lensTop, ColorIncidentRay, 4f);
            Vector2 ray1Dir = (fPrime - lensTop).normalized;
            DrawRay(lensTop, lensTop + ray1Dir * 320f, ColorRefractedRay, 4f);

            // Tia qua quang tâm
            Vector2 ray2Dir = (Vector2.zero - objTop).normalized;
            DrawRay(objTop, objTop + ray2Dir * 360f, ColorRefractedRay, 4f);

            // Kéo dài đường nét đứt về phía sau nếu d < f để minh họa ảnh ảo
            if (isInsideFocal)
            {
                // Guard epsilon chống division by zero khi f - d tiến gần 0
                float focalDiff = Mathf.Max(0.01f, f_cm - d_cm);
                float k = focalDiff > 0.05f ? (f_cm / focalDiff) : 99f;
                float virtualImageX = -((d_cm * f_cm) / focalDiff) * 10f;
                Vector2 virtualTop = new Vector2(Mathf.Max(-500f, virtualImageX), Mathf.Min(250f, 50f * k));

                DrawRay(lensTop, virtualTop, new Color(0.2f, 0.8f, 1f, 0.45f), 2f);
                DrawRay(Vector2.zero, virtualTop, new Color(0.2f, 0.8f, 1f, 0.45f), 2f);

                if (_txtPhysicsReadout != null)
                {
                    _txtPhysicsReadout.text = $"<b>Kính lúp:</b> Tiêu cự f = {f_cm:F0} cm\n" +
                                              $"Khoảng cách vật: <color=#FFE600>d = {d_cm:F1} cm</color> (4.0 cm ≤ d < 15.0 cm)\n" +
                                              $"Đặc điểm ảnh: <color=#55FF88>Ảnh ảo, cùng chiều, phóng đại k = {k:F1} lần</color>\n" +
                                              $"<i>Điều kiện hoạt động của kính lúp hoàn toàn thỏa mãn!</i>";
                }
            }
            else if (isAtFocalBoundary)
            {
                if (_txtPhysicsReadout != null)
                {
                    _txtPhysicsReadout.text = $"<b>Kính lúp:</b> Tiêu cự f = {f_cm:F0} cm\n" +
                                              $"Khoảng cách vật: <color=#FFAA00>d = {d_cm:F1} cm ≈ f (Tại tiêu điểm)</color>\n" +
                                              $"Đặc điểm ảnh: <color=#FFAA00>Tia ló song song, ảnh ở vô cực (Không quan sát được như kính lúp)</color>\n" +
                                              $"<i>Hãy dịch vật lại gần thấu kính hơn (d < 15 cm).</i>";
                }
            }
            else
            {
                if (_txtPhysicsReadout != null)
                {
                    _txtPhysicsReadout.text = $"<b>Kính lúp:</b> Tiêu cự f = {f_cm:F0} cm\n" +
                                              $"Khoảng cách vật: <color=#FFAA00>d = {d_cm:F1} cm</color> (d > f)\n" +
                                              $"Đặc điểm ảnh: <color=#FF8866>Ảnh thật ngược chiều (Không phải nguyên lý kính lúp)</color>\n" +
                                              $"<i>Cần kéo vật vào trong khoảng tiêu cự (d < 15 cm).</i>";
                }
            }
        }

        // =========================================================================
        // KIỂM TRA MỤC TIÊU & REWARD FLOW
        // =========================================================================
        public void OnCheckClicked()
        {
            if (IsStoryGateActive) return;
            if (_isCompleted) return;

            if (_isTargetHit)
            {
                _isCompleted = true;
                PlaySoundSafe(true);
                ShowFeedback("<b>XUẤT SẮC!</b> Tia sáng đã chạm đích thành công!", ColorSuccessGreen);
                StartCoroutine(TriggerSuccessSequence());
            }
            else
            {
                PlaySoundSafe(false);
                string failMsg = GetFailHintForCurrentMode();
                ShowFeedback(failMsg, ColorAlertRed);
            }
        }

        private string GetFailHintForCurrentMode()
        {
            switch (_currentMode)
            {
                case OpticsMode.Refraction:
                    return "Tia khúc xạ chưa trúng Tinh thể. Hãy thay đổi góc tới i để chỉnh hướng tia khúc xạ.";
                case OpticsMode.TotalReflection:
                    return "Tia sáng chưa phản xạ toàn phần trúng đích. Hãy đảm bảo i > 41.8° và tia phản xạ hướng đúng Tinh thể.";
                case OpticsMode.Prism:
                    return "Tia ló chưa trúng Tinh thể. Hãy xoay lăng kính để chỉnh góc lệch.";
                case OpticsMode.LensFocus:
                    return "Tiêu điểm F' chưa trùng với Tinh thể. Hãy di chuyển thấu kính để F' rơi đúng vị trí.";
                case OpticsMode.LensMeasure:
                    return "Ảnh trên màn còn mờ. Hãy dịch chuyển màn hứng tới vị trí d' = 60 cm.";
                case OpticsMode.Magnifier:
                    return "Vật chưa nằm trong khoảng tiêu cự (d < f = 15 cm) để tạo ảnh ảo phóng đại.";
                default:
                    return "Chưa đạt yêu cầu bài học. Hãy điều chỉnh lại dụng cụ quang học.";
            }
        }

        private IEnumerator TriggerSuccessSequence()
        {
            yield return new WaitForSeconds(0.6f);

            var save = PhysicsSaveManager.Load() ?? new PhysicsSaveData();

            // 1. Lưu Explore Progress & Chapter 2 Gameplay Progress
            save.MarkExploreLessonCompleted(_lessonId);
            save.MarkGameplayChapterCompleted("chapter_2");
            save.hasSeenOpticsTutorial = true;

            // 2. Phân bổ Reward theo từng lesson (mirror / prism / lens)
            string rewardItemId = GetRewardItemForLesson(_lessonId);
            bool isFirstUnlock = InventoryManager.UnlockItem(rewardItemId, save);

            // 3. Đánh giá Final Progression (cấp Energy Crystal nếu bài này hoàn tất toàn bộ hành trình)
            bool crystalGranted = PhysicsFinalProgression.EvaluateFinalProgression(save);

            // 4. Đánh giá thành tích & Lưu dữ liệu
            if (!crystalGranted)
            {
                AchievementManager.EvaluateAchievements(save);
                PhysicsSaveManager.Save(save);
                if (PhysicsGameSession.Instance != null)
                {
                    PhysicsGameSession.Instance.ReloadSaveData();
                }
            }

            // 5. Mở Success Modal
            ShowSuccessModal(rewardItemId, isFirstUnlock);
        }

        private string GetRewardItemForLesson(string lessonId)
        {
            switch (lessonId)
            {
                case "C2_L5":
                case "C2_L6":
                    return "mirror"; // Gương quang học
                case "C2_L7":
                    return "prism";  // Lăng kính
                case "C2_L8":
                case "C2_L9":
                case "C2_L10":
                default:
                    return "lens";   // Thấu kính
            }
        }

        // =========================================================================
        // HINT & RESET & BACK
        // =========================================================================
        public void OnHintClicked()
        {
            if (IsStoryGateActive) return;
            string hint = "";
            switch (_lessonId)
            {
                case "C2_L5":
                    hint = "<b>GỢI Ý:</b> Thay đổi góc tới để thay đổi góc khúc xạ theo định luật Snell: n₁·sin(i) = n₂·sin(r).";
                    break;
                case "C2_L6":
                    hint = "<b>GỢI Ý:</b> Phản xạ toàn phần xảy ra khi ánh sáng đi từ môi trường chiết suất lớn sang nhỏ và góc tới lớn hơn góc giới hạn (i > 41.8°).";
                    break;
                case "C2_L7":
                    hint = "<b>GỢI Ý:</b> Xoay lăng kính để thay đổi hướng tia ló. Lăng kính làm lệch tia sáng về phía đáy.";
                    break;
                case "C2_L8":
                    hint = "<b>GỢI Ý:</b> Tia song song trục chính qua thấu kính hội tụ sẽ đi qua tiêu điểm F'. Hãy đưa F' đến vị trí Tinh thể.";
                    break;
                case "C2_L9":
                    hint = "<b>GỢI Ý:</b> Di chuyển màn đến khi ảnh hội tụ rõ nét nhất (d' = 60 cm). Áp dụng 1/f = 1/d + 1/d'.";
                    break;
                case "C2_L10":
                    hint = "<b>GỢI Ý:</b> Kính lúp hoạt động khi vật nằm trong khoảng tiêu cự (d < f) của thấu kính hội tụ.";
                    break;
                default:
                    hint = "<b>GỢI Ý:</b> Quan sát sự thay đổi hướng truyền của tia sáng để hoàn thành thí nghiệm.";
                    break;
            }

            ShowFeedback(hint, ColorGold);
        }

        public void OnResetClicked()
        {
            if (IsStoryGateActive) return;
            ResetPuzzleToInitialState();
            ShowFeedback("Đã đưa thí nghiệm về vị trí ban đầu.", Color.white);
        }

        public void OnBackClicked()
        {
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.NavigateTo(RouteId.ChapterMap2);
            }
            else if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.GameplayOptics);
            }
        }

        // =========================================================================
        // RAY SEGMENT POOL & DRAWING
        // =========================================================================
        private void ClearRays()
        {
            for (int i = 0; i < _rayPool.Count; i++)
            {
                if (_rayPool[i] != null) _rayPool[i].gameObject.SetActive(false);
            }
        }

        private void DrawRay(Vector2 start, Vector2 end, Color color, float thickness)
        {
            if (_rayContainer == null) return;

            Image rayImg = GetAvailableRaySegment();
            RectTransform rt = rayImg.rectTransform;

            Vector2 dir = end - start;
            float length = dir.magnitude;
            if (length < 0.1f) return;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            rt.anchoredPosition = start;
            rt.sizeDelta = new Vector2(length, thickness);
            rt.localRotation = Quaternion.Euler(0, 0, angle);
            rayImg.color = color;
            rayImg.gameObject.SetActive(true);
        }

        private Image GetAvailableRaySegment()
        {
            for (int i = 0; i < _rayPool.Count; i++)
            {
                if (!_rayPool[i].gameObject.activeSelf)
                {
                    return _rayPool[i];
                }
            }

            // Tạo segment mới nếu pool hết
            GameObject go = new GameObject("RaySegment_" + _rayPool.Count);
            go.transform.SetParent(_rayContainer, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f); // Quay quanh điểm bắt đầu

            Image img = go.AddComponent<Image>();
            img.raycastTarget = false;
            img.color = ColorIncidentRay;

            _rayPool.Add(img);
            return img;
        }

        private float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lenSq = ab.sqrMagnitude;
            if (lenSq < 0.0001f) return (p - a).magnitude;

            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lenSq);
            Vector2 projection = a + t * ab;
            return (p - projection).magnitude;
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
            // Dự án hiện tại không có Audio Asset / AudioMixer thực tế,
            // do đó chạy ngầm không phát âm thanh để tránh giả định tài nguyên và loại bỏ 100% rủi ro NullReference.
        }

        // =========================================================================
        // XÂY DỰNG GIAO DIỆN (AURELIA NAVY & GOLD)
        // =========================================================================
        private void BuildGameplayUIIfMissing()
        {
            Transform existing = _canvas.transform.Find("OpticsHUD");
            if (existing != null)
            {
                _hudRoot = existing.gameObject;
                return;
            }

            _hudRoot = new GameObject("OpticsHUD");
            _hudRoot.transform.SetParent(_canvas.transform, false);
            RectTransform hudRt = _hudRoot.AddComponent<RectTransform>();
            hudRt.anchorMin = Vector2.zero;
            hudRt.anchorMax = Vector2.one;
            hudRt.sizeDelta = Vector2.zero;

            // 1. TOP HEADER & NAVIGATION
            BuildTopHeader();

            // 2. CỘT TRÁI: HƯỚNG DẪN & NHIỆM VỤ
            BuildLeftPanel();

            // 3. CỘT PHẢI / TRUNG TÂM: BUỒNG QUANG HỌC
            BuildChamberPanel();

            // 4. MODALS (TUTORIAL & SUCCESS)
            BuildModals();
        }

        private void BuildTopHeader()
        {
            // Back Button
            CreateButton(_hudRoot.transform, "Btn_Back", new Vector2(-860, 485), new Vector2(140, 45), "Quay lại", OnBackClicked);

            // Title Banner
            GameObject titleGo = new GameObject("TitleBanner");
            titleGo.transform.SetParent(_hudRoot.transform, false);
            RectTransform titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0, 485);
            titleRt.sizeDelta = new Vector2(900, 45);

            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "HỌC VIỆN AURELIA · DẪN ĐƯỜNG ÁNH SÁNG";
            titleTmp.fontSize = 26;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = ColorGold;

            // Lesson Badge
            GameObject badgeGo = new GameObject("LessonBadge");
            badgeGo.transform.SetParent(_hudRoot.transform, false);
            RectTransform bRt = badgeGo.AddComponent<RectTransform>();
            bRt.anchoredPosition = new Vector2(0, 445);
            bRt.sizeDelta = new Vector2(600, 30);
            _txtLessonBadge = badgeGo.AddComponent<TextMeshProUGUI>();
            _txtLessonBadge.text = _lessonTitle;
            _txtLessonBadge.fontSize = 18;
            _txtLessonBadge.fontStyle = FontStyles.Bold;
            _txtLessonBadge.alignment = TextAlignmentOptions.Center;
            _txtLessonBadge.color = new Color(0.3f, 0.85f, 1f, 1f);
        }

        private void BuildLeftPanel()
        {
            GameObject leftPanel = CreateNavyPanel(_hudRoot.transform, "MissionPanel", new Vector2(-680, -30), new Vector2(460, 680));
            CreatePanelHeader(leftPanel.transform, "NHIỆM VỤ QUANG HỌC", new Vector2(0, 305));

            // Mission Title
            GameObject mTitleGo = new GameObject("MissionTitle");
            mTitleGo.transform.SetParent(leftPanel.transform, false);
            RectTransform mtRt = mTitleGo.AddComponent<RectTransform>();
            mtRt.anchoredPosition = new Vector2(0, 245);
            mtRt.sizeDelta = new Vector2(400, 35);
            _txtMissionTitle = mTitleGo.AddComponent<TextMeshProUGUI>();
            _txtMissionTitle.fontSize = 18;
            _txtMissionTitle.fontStyle = FontStyles.Bold;
            _txtMissionTitle.alignment = TextAlignmentOptions.Center;
            _txtMissionTitle.color = ColorGold;

            // Mission Desc
            GameObject mDescGo = new GameObject("MissionDesc");
            mDescGo.transform.SetParent(leftPanel.transform, false);
            RectTransform mdRt = mDescGo.AddComponent<RectTransform>();
            mdRt.anchoredPosition = new Vector2(0, 165);
            mdRt.sizeDelta = new Vector2(410, 105);
            _txtMissionDesc = mDescGo.AddComponent<TextMeshProUGUI>();
            _txtMissionDesc.fontSize = 16;
            _txtMissionDesc.alignment = TextAlignmentOptions.TopLeft;
            _txtMissionDesc.color = Color.white;

            // Feedback Box
            GameObject feedbackBox = CreateNavyPanel(leftPanel.transform, "FeedbackBox", new Vector2(0, 15), new Vector2(420, 140));
            feedbackBox.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.16f, 0.95f);

            GameObject fbTxtGo = new GameObject("FeedbackText");
            fbTxtGo.transform.SetParent(feedbackBox.transform, false);
            RectTransform fbRt = fbTxtGo.AddComponent<RectTransform>();
            fbRt.sizeDelta = new Vector2(390, 120);
            _txtFeedback = fbTxtGo.AddComponent<TextMeshProUGUI>();
            _txtFeedback.fontSize = 15;
            _txtFeedback.alignment = TextAlignmentOptions.Center;
            _txtFeedback.color = new Color(0.9f, 0.95f, 1f, 0.9f);
            _txtFeedback.text = "Điều chỉnh dụng cụ quang học và bấm KIỂM TRA.";

            // Action Buttons
            CreateButton(leftPanel.transform, "Btn_Check", new Vector2(0, -115), new Vector2(400, 52), "KIỂM TRA MỤC TIÊU", OnCheckClicked, true);
            CreateButton(leftPanel.transform, "Btn_Hint", new Vector2(-110, -190), new Vector2(180, 45), "GỢI Ý", OnHintClicked);
            CreateButton(leftPanel.transform, "Btn_Reset", new Vector2(110, -190), new Vector2(180, 45), "LÀM LẠI", OnResetClicked);

            // Ghi chú bản quyền Học viện
            GameObject noteGo = new GameObject("AureliaNote");
            noteGo.transform.SetParent(leftPanel.transform, false);
            RectTransform nRt = noteGo.AddComponent<RectTransform>();
            nRt.anchoredPosition = new Vector2(0, -285);
            nRt.sizeDelta = new Vector2(400, 40);
            var nTmp = noteGo.AddComponent<TextMeshProUGUI>();
            nTmp.fontSize = 13;
            nTmp.alignment = TextAlignmentOptions.Center;
            nTmp.color = new Color(0.6f, 0.7f, 0.85f, 0.7f);
            nTmp.text = "Bộ dụng cụ thực hành Vật lý 9 chuẩn SGK\nHọc viện Ma thuật & Khoa học Aurelia";
        }

        private void BuildChamberPanel()
        {
            GameObject chamberGo = CreateNavyPanel(_hudRoot.transform, "OpticalChamber", new Vector2(230, -30), new Vector2(1360, 680));
            _chamberRt = chamberGo.GetComponent<RectTransform>();

            CreatePanelHeader(chamberGo.transform, "BUỒNG QUANG HỌC THỰC NGHIỆM", new Vector2(0, 305));

            // Physics Realtime Readout Box
            GameObject readoutBox = CreateNavyPanel(chamberGo.transform, "ReadoutBox", new Vector2(0, 240), new Vector2(1300, 80));
            readoutBox.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.16f, 0.85f);

            GameObject roTxtGo = new GameObject("ReadoutText");
            roTxtGo.transform.SetParent(readoutBox.transform, false);
            RectTransform roRt = roTxtGo.AddComponent<RectTransform>();
            roRt.sizeDelta = new Vector2(1260, 70);
            _txtPhysicsReadout = roTxtGo.AddComponent<TextMeshProUGUI>();
            _txtPhysicsReadout.fontSize = 15;
            _txtPhysicsReadout.alignment = TextAlignmentOptions.Center;
            _txtPhysicsReadout.color = Color.white;

            // Optical Bench Canvas / Chamber Inner Workspace
            GameObject benchWorkspace = new GameObject("BenchWorkspace");
            benchWorkspace.transform.SetParent(chamberGo.transform, false);
            RectTransform wsRt = benchWorkspace.AddComponent<RectTransform>();
            wsRt.anchoredPosition = new Vector2(0, -20);
            wsRt.sizeDelta = new Vector2(1300, 400);

            // Container cho Rays
            GameObject raysGo = new GameObject("RaysContainer");
            raysGo.transform.SetParent(benchWorkspace.transform, false);
            RectTransform rRt = raysGo.AddComponent<RectTransform>();
            rRt.sizeDelta = Vector2.zero;
            _rayContainer = raysGo.transform;

            // Pháp tuyến / Trục chính (Normal line)
            GameObject normalGo = new GameObject("NormalLine");
            normalGo.transform.SetParent(benchWorkspace.transform, false);
            _normalLineRt = normalGo.AddComponent<RectTransform>();
            _normalLineRt.anchoredPosition = Vector2.zero;
            _normalLineRt.sizeDelta = new Vector2(1260, 2);
            Image normalImg = normalGo.AddComponent<Image>();
            normalImg.color = new Color(0.4f, 0.6f, 0.85f, 0.4f);

            // Mặt phân cách môi trường (Interface line)
            GameObject interfaceGo = new GameObject("InterfaceLine");
            interfaceGo.transform.SetParent(benchWorkspace.transform, false);
            _interfaceLineRt = interfaceGo.AddComponent<RectTransform>();
            _interfaceLineRt.anchoredPosition = Vector2.zero;
            _interfaceLineRt.sizeDelta = new Vector2(4, 380);
            Image interfaceImg = interfaceGo.AddComponent<Image>();
            interfaceImg.color = new Color(0.85f, 0.7f, 0.3f, 0.85f);

            // Nguồn phát tia (Emitter)
            GameObject emitterGo = new GameObject("OpticalEmitter");
            emitterGo.transform.SetParent(benchWorkspace.transform, false);
            _emitterRt = emitterGo.AddComponent<RectTransform>();
            _emitterRt.sizeDelta = new Vector2(40, 24);
            Image emitterImg = emitterGo.AddComponent<Image>();
            emitterImg.color = ColorGold;

            // Lăng kính (Prism)
            GameObject prismGo = new GameObject("PrismApparatus");
            prismGo.transform.SetParent(benchWorkspace.transform, false);
            _prismRt = prismGo.AddComponent<RectTransform>();
            _prismRt.anchoredPosition = Vector2.zero;
            _prismRt.sizeDelta = new Vector2(80, 80);
            Image prismImg = prismGo.AddComponent<Image>();
            prismImg.color = new Color(0.2f, 0.7f, 1f, 0.4f);
            var prismOutline = prismGo.AddComponent<Outline>();
            prismOutline.effectColor = ColorGold;
            prismOutline.effectDistance = new Vector2(2, 2);

            // Thấu kính (Lens)
            GameObject lensGo = new GameObject("LensApparatus");
            lensGo.transform.SetParent(benchWorkspace.transform, false);
            _lensRt = lensGo.AddComponent<RectTransform>();
            _lensRt.anchoredPosition = Vector2.zero;
            _lensRt.sizeDelta = new Vector2(20, 220);
            Image lensImg = lensGo.AddComponent<Image>();
            lensImg.color = new Color(0.3f, 0.85f, 1f, 0.7f);
            var lensOutline = lensGo.AddComponent<Outline>();
            lensOutline.effectColor = ColorGold;
            lensOutline.effectDistance = new Vector2(1, -1);

            // Màn hứng ảnh (Screen)
            GameObject screenGo = new GameObject("ScreenApparatus");
            screenGo.transform.SetParent(benchWorkspace.transform, false);
            _screenRt = screenGo.AddComponent<RectTransform>();
            _screenRt.anchoredPosition = new Vector2(480, 0);
            _screenRt.sizeDelta = new Vector2(16, 200);
            Image screenImg = screenGo.AddComponent<Image>();
            screenImg.color = new Color(1f, 1f, 1f, 0.85f);
            var screenOutline = screenGo.AddComponent<Outline>();
            screenOutline.effectColor = ColorGold;
            screenOutline.effectDistance = new Vector2(2, 2);

            // Vật sáng AB (Object)
            GameObject objGo = new GameObject("ObjectApparatus");
            objGo.transform.SetParent(benchWorkspace.transform, false);
            _objectRt = objGo.AddComponent<RectTransform>();
            _objectRt.anchoredPosition = new Vector2(-240, 0);
            _objectRt.sizeDelta = new Vector2(14, 80);
            Image objImg = objGo.AddComponent<Image>();
            objImg.color = new Color(1f, 0.4f, 0.2f, 0.95f);

            // Tinh thể năng lượng (Target Crystal)
            GameObject crystalGo = new GameObject("TargetCrystal");
            crystalGo.transform.SetParent(benchWorkspace.transform, false);
            _targetCrystalRt = crystalGo.AddComponent<RectTransform>();
            _targetCrystalRt.anchoredPosition = new Vector2(300, -150);
            _targetCrystalRt.sizeDelta = new Vector2(48, 48);

            // Crystal Glow
            GameObject cGlowGo = new GameObject("Glow");
            cGlowGo.transform.SetParent(crystalGo.transform, false);
            RectTransform cgRt = cGlowGo.AddComponent<RectTransform>();
            cgRt.sizeDelta = new Vector2(76, 76);
            _crystalGlowImg = cGlowGo.AddComponent<Image>();
            _crystalGlowImg.color = new Color(ColorGold.r, ColorGold.g, ColorGold.b, 0.5f);

            // Crystal Core
            Image crystalImg = crystalGo.AddComponent<Image>();
            crystalImg.color = ColorGold;

            // Crystal Badge
            GameObject cBadgeGo = new GameObject("CrystalBadge");
            cBadgeGo.transform.SetParent(crystalGo.transform, false);
            RectTransform cbRt = cBadgeGo.AddComponent<RectTransform>();
            cbRt.anchoredPosition = new Vector2(0, -36);
            cbRt.sizeDelta = new Vector2(120, 24);
            _crystalBadgeTxt = cBadgeGo.AddComponent<TextMeshProUGUI>();
            _crystalBadgeTxt.text = "TINH THỂ";
            _crystalBadgeTxt.fontSize = 12;
            _crystalBadgeTxt.fontStyle = FontStyles.Bold;
            _crystalBadgeTxt.alignment = TextAlignmentOptions.Center;
            _crystalBadgeTxt.color = Color.white;

            // Bottom Controller Bar (Slider & Buttons)
            BuildChamberBottomControls(chamberGo.transform);
        }

        private void BuildChamberBottomControls(Transform parent)
        {
            GameObject ctrlBar = new GameObject("BottomControls");
            ctrlBar.transform.SetParent(parent, false);
            RectTransform cbRt = ctrlBar.AddComponent<RectTransform>();
            cbRt.anchoredPosition = new Vector2(0, -270);
            cbRt.sizeDelta = new Vector2(1300, 70);

            // Slider Container
            _sliderControl = CreateParameterSlider(
                ctrlBar.transform,
                "Điều chỉnh:",
                "",
                0f, 80f, _angleIncidence,
                new Vector2(-60, 0),
                out _sliderValTxt,
                out _sliderLabelTxt,
                val =>
                {
                    OnSliderValueChanged(val);
                });

            // Quick Step Buttons for precision
            CreateButton(ctrlBar.transform, "Btn_StepMinus", new Vector2(470, 0), new Vector2(70, 45), "◀ 1°", () =>
            {
                if (IsStoryGateActive) return;
                if (_sliderControl != null) _sliderControl.value -= 1f;
            });

            CreateButton(ctrlBar.transform, "Btn_StepPlus", new Vector2(560, 0), new Vector2(70, 45), "1° ▶", () =>
            {
                if (IsStoryGateActive) return;
                if (_sliderControl != null) _sliderControl.value += 1f;
            });
        }

        private void OnSliderValueChanged(float val)
        {
            if (IsStoryGateActive) return;
            switch (_currentMode)
            {
                case OpticsMode.Refraction:
                case OpticsMode.TotalReflection:
                    _angleIncidence = val;
                    if (_sliderValTxt != null) _sliderValTxt.text = $"{_angleIncidence:F1}°";
                    break;

                case OpticsMode.Prism:
                    _prismAngle = val;
                    if (_sliderValTxt != null) _sliderValTxt.text = $"{_prismAngle:F1}°";
                    break;

                case OpticsMode.LensFocus:
                    _lensPosX = val;
                    if (_sliderValTxt != null) _sliderValTxt.text = $"{_lensPosX:F0} px";
                    break;

                case OpticsMode.LensMeasure:
                    _screenPosX = val;
                    float dPrime_cm = val / 8f;
                    if (_sliderValTxt != null) _sliderValTxt.text = $"{dPrime_cm:F1} cm";
                    break;

                case OpticsMode.Magnifier:
                    _objectPosX = val;
                    float d_cm = Mathf.Abs(val) / 10f;
                    if (_sliderValTxt != null) _sliderValTxt.text = $"{d_cm:F1} cm";
                    break;
            }

            CalculateAndRenderOptics();
        }

        // =========================================================================
        // MODALS (TUTORIAL & SUCCESS)
        // =========================================================================
        private void BuildModals()
        {
            // 1. TUTORIAL MODAL
            _tutorialModal = new GameObject("OpticsTutorialModal");
            _tutorialModal.transform.SetParent(_canvas.transform, false);
            RectTransform tutModalRt = _tutorialModal.AddComponent<RectTransform>();
            tutModalRt.anchorMin = Vector2.zero;
            tutModalRt.anchorMax = Vector2.one;
            tutModalRt.sizeDelta = Vector2.zero;

            Image tutDim = _tutorialModal.AddComponent<Image>();
            tutDim.color = new Color(0.02f, 0.04f, 0.08f, 0.85f);

            GameObject tutCard = CreateNavyPanel(_tutorialModal.transform, "TutorialCard", Vector2.zero, new Vector2(720, 440));
            CreatePanelHeader(tutCard.transform, "CÁCH CHƠI · DẪN ĐƯỜNG ÁNH SÁNG", new Vector2(0, 180));

            GameObject tutDescGo = new GameObject("TutorialDesc");
            tutDescGo.transform.SetParent(tutCard.transform, false);
            RectTransform tdRt = tutDescGo.AddComponent<RectTransform>();
            tdRt.anchoredPosition = new Vector2(0, 40);
            tdRt.sizeDelta = new Vector2(620, 200);

            var tutDescTmp = tutDescGo.AddComponent<TextMeshProUGUI>();
            tutDescTmp.fontSize = 18;
            tutDescTmp.lineSpacing = 8;
            tutDescTmp.color = Color.white;
            tutDescTmp.text = "<b>MỤC TIÊU:</b> Điều chỉnh dụng cụ quang học để dẫn tia sáng tới Tinh thể năng lượng.\n\n" +
                              "<b>1.</b> Kéo thanh trượt hoặc phím điều chỉnh để thay đổi góc và vị trí dụng cụ.\n" +
                              "<b>2.</b> Quan sát quy luật khúc xạ, phản xạ toàn phần và hội tụ của tia sáng.\n" +
                              "<b>3.</b> Nhấn <b>KIỂM TRA MỤC TIÊU</b> khi tia sáng đã chiếu trúng Tinh thể.";

            CreateButton(tutCard.transform, "Btn_Understand", new Vector2(0, -145), new Vector2(240, 50), "ĐÃ HIỂU", CloseTutorialModal, true);
            _tutorialModal.SetActive(false);

            // 2. SUCCESS MODAL
            _successModal = new GameObject("OpticsSuccessModal");
            _successModal.transform.SetParent(_canvas.transform, false);
            RectTransform sucModalRt = _successModal.AddComponent<RectTransform>();
            sucModalRt.anchorMin = Vector2.zero;
            sucModalRt.anchorMax = Vector2.one;
            sucModalRt.sizeDelta = Vector2.zero;

            Image sucDim = _successModal.AddComponent<Image>();
            sucDim.color = new Color(0.02f, 0.04f, 0.08f, 0.9f);

            GameObject sucCard = CreateNavyPanel(_successModal.transform, "SuccessCard", Vector2.zero, new Vector2(760, 480));
            CreatePanelHeader(sucCard.transform, "★ HOÀN THÀNH THÍ NGHIỆM ★", new Vector2(0, 200));

            GameObject sucMsgGo = new GameObject("SuccessMessage");
            sucMsgGo.transform.SetParent(sucCard.transform, false);
            RectTransform smRt = sucMsgGo.AddComponent<RectTransform>();
            smRt.anchoredPosition = new Vector2(0, 80);
            smRt.sizeDelta = new Vector2(660, 160);
            var sucMsgTmp = sucMsgGo.AddComponent<TextMeshProUGUI>();
            sucMsgTmp.fontSize = 18;
            sucMsgTmp.alignment = TextAlignmentOptions.Center;
            sucMsgTmp.color = Color.white;
            sucMsgTmp.name = "SuccessMsgText";

            GameObject rewardGo = new GameObject("RewardNotification");
            rewardGo.transform.SetParent(sucCard.transform, false);
            RectTransform rNotifRt = rewardGo.AddComponent<RectTransform>();
            rNotifRt.anchoredPosition = new Vector2(0, -45);
            rNotifRt.sizeDelta = new Vector2(660, 50);
            var rewardTmp = rewardGo.AddComponent<TextMeshProUGUI>();
            rewardTmp.fontSize = 18;
            rewardTmp.fontStyle = FontStyles.Bold;
            rewardTmp.alignment = TextAlignmentOptions.Center;
            rewardTmp.color = ColorGold;
            rewardTmp.name = "RewardText";

            CreateButton(sucCard.transform, "Btn_Continue", new Vector2(-140, -165), new Vector2(220, 50), "TIẾP TỤC", CloseSuccessModalAndContinue, true);
            CreateButton(sucCard.transform, "Btn_Replay", new Vector2(140, -165), new Vector2(220, 50), "LÀM LẠI", CloseSuccessModalAndReplay);
            _successModal.SetActive(false);
        }

        private void ShowSuccessModal(string rewardItemId, bool isFirstUnlock)
        {
            if (_successModal == null) return;

            Transform card = _successModal.transform.Find("SuccessCard");
            if (card != null)
            {
                var msgText = card.Find("SuccessMessage")?.GetComponent<TextMeshProUGUI>();
                if (msgText != null)
                {
                    msgText.text = $"Chúc mừng bạn đã hoàn thành xuất sắc thí nghiệm <b>{_lessonTitle}</b>!\n\n" +
                                   $"Tia sáng quang học đã được dẫn đường chính xác và kích hoạt thành công Tinh thể Năng lượng của Học viện Aurelia.";
                }

                var rewardText = card.Find("RewardNotification")?.GetComponent<TextMeshProUGUI>();
                if (rewardText != null)
                {
                    var item = PhysicsItemData.Get(rewardItemId);
                    string itemName = item != null ? item.displayName : rewardItemId;
                    rewardText.text = isFirstUnlock
                        ? $"<color=#55FF88>★ MỞ KHÓA VẬT PHẨM MỚI:</color> <b>{itemName}</b> đã thêm vào Kho đồ!"
                        : $"<color=#FFD700>✓ ĐÃ THU THẬP:</color> {itemName} (đã có trong Kho đồ).";
                }
            }

            _successModal.SetActive(true);
        }

        public void CloseTutorialModal()
        {
            if (_tutorialModal != null) _tutorialModal.SetActive(false);
            var save = PhysicsSaveManager.Load();
            if (save != null && !save.hasSeenOpticsTutorial)
            {
                save.hasSeenOpticsTutorial = true;
                PhysicsSaveManager.Save(save);
            }
        }

        public void CloseSuccessModalAndContinue()
        {
            if (_successModal != null) _successModal.SetActive(false);
            Transform targetCanvas = _canvas != null ? _canvas.transform : null;
            StoryFlowController.TryShowPostGameplayOrPendingStories(
                completedChapterHint: 2,
                customSave: null,
                onAllFinished: OnBackClicked,
                canvasTransform: targetCanvas);
        }

        public void CloseSuccessModalAndReplay()
        {
            if (_successModal != null) _successModal.SetActive(false);
            ResetPuzzleToInitialState();
        }

        // =========================================================================
        // UI HELPERS
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
            rt.sizeDelta = new Vector2(400, 35);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 20;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = ColorGold;
        }

        private Slider CreateParameterSlider(
            Transform parent,
            string label,
            string unit,
            float min,
            float max,
            float initial,
            Vector2 pos,
            out TextMeshProUGUI valueText,
            out TextMeshProUGUI labelText,
            Action<float> onValueChanged)
        {
            GameObject container = new GameObject("ParamSlider");
            container.transform.SetParent(parent, false);
            RectTransform cRt = container.AddComponent<RectTransform>();
            cRt.anchoredPosition = pos;
            cRt.sizeDelta = new Vector2(850, 60);

            // Label
            GameObject labelGo = new GameObject("Label");
            labelGo.transform.SetParent(container.transform, false);
            RectTransform lRt = labelGo.AddComponent<RectTransform>();
            lRt.anchoredPosition = new Vector2(-280, 0);
            lRt.sizeDelta = new Vector2(240, 35);
            labelText = labelGo.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 18;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = ColorGold;

            // Value Text
            GameObject valGo = new GameObject("Value");
            valGo.transform.SetParent(container.transform, false);
            RectTransform vRt = valGo.AddComponent<RectTransform>();
            vRt.anchoredPosition = new Vector2(320, 0);
            vRt.sizeDelta = new Vector2(140, 35);
            valueText = valGo.AddComponent<TextMeshProUGUI>();
            valueText.text = $"{initial:F1} {unit}";
            valueText.fontSize = 20;
            valueText.fontStyle = FontStyles.Bold;
            valueText.alignment = TextAlignmentOptions.Left;
            valueText.color = Color.white;

            // Slider
            GameObject sliderGo = new GameObject("Slider");
            sliderGo.transform.SetParent(container.transform, false);
            RectTransform sRt = sliderGo.AddComponent<RectTransform>();
            sRt.anchoredPosition = new Vector2(50, 0);
            sRt.sizeDelta = new Vector2(380, 20);

            Slider slider = sliderGo.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = initial;

            Image trackImg = sliderGo.AddComponent<Image>();
            trackImg.color = new Color(0.15f, 0.22f, 0.35f, 1f);

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGo.transform, false);
            RectTransform faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0, 0.25f);
            faRt.anchorMax = new Vector2(1, 0.75f);
            faRt.sizeDelta = Vector2.zero;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fRt = fill.AddComponent<RectTransform>();
            fRt.sizeDelta = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = ColorGold;
            slider.fillRect = fRt;

            // Handle
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderGo.transform, false);
            RectTransform haRt = handleArea.AddComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero;
            haRt.anchorMax = Vector2.one;
            haRt.sizeDelta = Vector2.zero;

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            RectTransform hRt = handle.AddComponent<RectTransform>();
            hRt.sizeDelta = new Vector2(30, 30);
            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;
            slider.handleRect = hRt;
            slider.targetGraphic = handleImg;

            slider.onValueChanged.AddListener(val => onValueChanged?.Invoke(val));
            return slider;
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
            img.color = isGold ? ColorGold : new Color(0.12f, 0.22f, 0.42f, 0.95f);

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
            tmp.fontSize = isGold ? 18 : 16;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = isGold ? new Color(0.08f, 0.12f, 0.24f, 1f) : Color.white;

            btn.onClick.AddListener(() => onClick?.Invoke());
            btnGo.AddComponent<UIButtonFeedback>();

            return btnGo;
        }
    }
}
