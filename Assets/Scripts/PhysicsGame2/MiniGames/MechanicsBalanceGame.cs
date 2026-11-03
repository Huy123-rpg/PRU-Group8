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
    /// Mini-game Chương I: Năng lượng cơ học – Energy Balance (Cân bằng năng lượng).
    /// Người chơi tương tác thực tế với 3 đại lượng: Khối lượng (m), Độ cao (h), Vận tốc (v).
    /// Tính toán và trực quan hóa thời gian thực: Động năng (Wđ), Thế năng (Wt), Cơ năng (W).
    /// </summary>
    public class MechanicsBalanceGame : MonoBehaviour
    {
        [Header("=== PHYSICS PARAMETERS ===")]
        private float _mass = 1f;      // 1 đến 10 kg (neutral initial)
        private float _height = 1f;    // 0 đến 10 m (neutral initial)
        private float _velocity = 2f; // 0 đến 15 m/s (neutral initial)

        private const float G = 10f; // Gia tốc trọng trường chuẩn bài tập SGK lớp 9

        // Kết quả năng lượng
        private float _kineticEnergy;
        private float _potentialEnergy;
        private float _mechanicalEnergy;

        // Quản lý thử thách
        private List<MechanicsChallenge> _challenges;
        private int _currentChallengeIndex = 0;

        // UI References
        private Canvas _canvas;
        private GameObject _hudRoot;
        private GameObject _tutorialModal;
        private GameObject _successModal;

        // Controls
        private Slider _sliderMass;
        private TextMeshProUGUI _txtMassVal;
        private Slider _sliderHeight;
        private TextMeshProUGUI _txtHeightVal;
        private Slider _sliderVelocity;
        private TextMeshProUGUI _txtVelocityVal;

        // Energy Displays
        private TextMeshProUGUI _txtKinetic;
        private TextMeshProUGUI _txtPotential;
        private TextMeshProUGUI _txtMechanical;

        private Image _barFillKinetic;
        private Image _barFillPotential;
        private Image _barFillTotal;

        // Mission UI
        private TextMeshProUGUI _txtChallengeTitle;
        private TextMeshProUGUI _txtChallengeDesc;
        private TextMeshProUGUI _txtFeedback;
        private Coroutine _feedbackCoroutine;

        // Visualization
        private RectTransform _orbTransform;
        private Image _orbGlow;
        private TextMeshProUGUI _orbHeightBadge;

        // Colors Aurelia Navy & Gold
        private static readonly Color ColorNavyBg = new Color(0.06f, 0.11f, 0.22f, 0.92f);
        private static readonly Color ColorGold = new Color(0.96f, 0.82f, 0.38f, 1f);
        private static readonly Color ColorGoldBorder = new Color(0.85f, 0.70f, 0.28f, 0.85f);
        private static readonly Color ColorCyanKinetic = new Color(0.20f, 0.75f, 1.0f, 1f);
        private static readonly Color ColorAmberPotential = new Color(1.0f, 0.65f, 0.20f, 1f);
        private static readonly Color ColorGreenSuccess = new Color(0.30f, 0.95f, 0.45f, 1f);
        private static readonly Color ColorRedAlert = new Color(1.0f, 0.45f, 0.40f, 1f);

        private bool _storyGateActive = false;
        private bool _enteredViaNormalFlow = false;
        public bool IsStoryGateActive => _storyGateActive || StoryFlowController.IsStoryModalOpen;

        public void SetStoryGate(bool blocked)
        {
            _storyGateActive = blocked;
            if (_hudRoot != null)
            {
                CanvasGroup cg = _hudRoot.GetComponent<CanvasGroup>();
                if (cg == null) cg = _hudRoot.AddComponent<CanvasGroup>();
                cg.interactable = !blocked;
                cg.blocksRaycasts = !blocked;
            }
        }

        private void Awake()
        {
            _enteredViaNormalFlow = StoryFlowController.IsNormalGameplayEntry();
            EnsureEventSystem();
            EnsureSessionFallback();
            _challenges = MechanicsChallenge.GetPresets();

            _canvas = FindAnyObjectByType<Canvas>();
            if (_canvas != null)
            {
                BuildGameplayUIIfMissing();
            }
        }

        private void Start()
        {
            UpdateChallengeDisplay();
            CalculatePhysics();

            if (_tutorialModal != null)
            {
                _tutorialModal.SetActive(false);
            }

            Transform targetCanvas = _canvas != null ? _canvas.transform : null;
            SetStoryGate(true);
            bool introShown = StoryFlowController.TryShowGameplayCoreIntro(
                chapterNumber: 1,
                customSave: null,
                onIntroFinishedOrSkipped: () => SetStoryGate(false),
                canvasTransform: targetCanvas,
                forceNormalFlow: _enteredViaNormalFlow);

            if (!introShown)
            {
                SetStoryGate(false);
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
                if (PhysicsGameSession.Instance.SelectedChapterId <= 0)
                {
                    PhysicsGameSession.Instance.SelectChapter(1);
                }
            }
        }

        // =========================================================================
        // TÍNH TOÁN VẬT LÝ THỜI GIAN THỰC (DETERMINISTIC PHYSICS 9)
        // =========================================================================
        private void CalculatePhysics()
        {
            // Wđ = 1/2 * m * v^2
            _kineticEnergy = 0.5f * _mass * _velocity * _velocity;

            // Wt = m * g * h
            _potentialEnergy = _mass * G * _height;

            // W = Wđ + Wt
            _mechanicalEnergy = _kineticEnergy + _potentialEnergy;

            // Cập nhật UI
            if (_txtKinetic != null)
                _txtKinetic.text = $"<b>ĐỘNG NĂNG (Wđ):</b>  <color=#33C0FF>{_kineticEnergy:F1} J</color>\n<size=16><color=#C0D0E0>Wđ = ½ · m · v²  =  ½ · {_mass:F0} · {_velocity:F0}²</color></size>";

            if (_txtPotential != null)
                _txtPotential.text = $"<b>THẾ NĂNG (Wt):</b>  <color=#FFA533>{_potentialEnergy:F1} J</color>\n<size=16><color=#C0D0E0>Wt = m · g · h  =  {_mass:F0} · 10 · {_height:F1}</color></size>";

            if (_txtMechanical != null)
                _txtMechanical.text = $"<b>TỔNG CƠ NĂNG (W):</b>  <color=#FFD700>{_mechanicalEnergy:F1} J</color>\n<size=16><color=#C0D0E0>W = Wđ + Wt  =  {_kineticEnergy:F1} + {_potentialEnergy:F1}</color></size>";

            // Cập nhật Energy Bars chuẩn hóa theo cực đại lý thuyết (m_max=10, h_max=10, v_max=15, g=10):
            // Wđ_max = 0.5 * 10 * 15² = 1125 J
            // Wt_max = 10 * 10 * 10 = 1000 J
            // W_max = 1125 + 1000 = 2125 J
            const float maxKineticEnergy = 1125f;
            const float maxPotentialEnergy = 1000f;
            const float maxMechanicalEnergy = 2125f;

            if (_barFillKinetic != null)
                _barFillKinetic.fillAmount = Mathf.Clamp01(_kineticEnergy / maxKineticEnergy);

            if (_barFillPotential != null)
                _barFillPotential.fillAmount = Mathf.Clamp01(_potentialEnergy / maxPotentialEnergy);

            if (_barFillTotal != null)
                _barFillTotal.fillAmount = Mathf.Clamp01(_mechanicalEnergy / maxMechanicalEnergy);

            // Trực quan hóa vật thể năng lượng (Orb):
            // Độ cao h (0 -> 10m) map tương ứng với Y từ -150px đến +150px trong khung mô phỏng
            if (_orbTransform != null)
            {
                float targetY = Mathf.Lerp(-140f, 140f, _height / 10f);
                _orbTransform.anchoredPosition = new Vector2(0, targetY);

                // Khối lượng m thay đổi nhẹ kích thước Orb (scale 0.85 -> 1.35)
                float scale = Mathf.Lerp(0.85f, 1.35f, (_mass - 1f) / 9f);
                _orbTransform.localScale = new Vector3(scale, scale, 1f);

                if (_orbHeightBadge != null)
                {
                    _orbHeightBadge.text = $"h = {_height:F1} m";
                }
            }

            // Glow của Orb sáng mạnh theo vận tốc v
            if (_orbGlow != null)
            {
                float glowAlpha = Mathf.Lerp(0.2f, 0.95f, _velocity / 15f);
                _orbGlow.color = new Color(ColorGold.r, ColorGold.g, ColorGold.b, glowAlpha);
            }
        }

        // =========================================================================
        // LOGIC KIỂM TRA THỬ THÁCH & REWARD
        // =========================================================================
        public void OnCheckClicked()
        {
            if (IsStoryGateActive) return;
            if (_challenges == null || _challenges.Count == 0) return;
            var currentChallenge = _challenges[_currentChallengeIndex];

            var result = currentChallenge.evaluateFunc(_mass, _height, _velocity, _kineticEnergy, _potentialEnergy, _mechanicalEnergy);

            if (result.isSuccess)
            {
                ShowFeedback(result.feedback, ColorGreenSuccess);
                StartCoroutine(TriggerSuccessSequence(result.feedback));
            }
            else
            {
                ShowFeedback(result.feedback, ColorRedAlert);
            }
        }

        private IEnumerator TriggerSuccessSequence(string successMsg)
        {
            yield return new WaitForSeconds(0.8f);

            var save = PhysicsSaveManager.Load() ?? new PhysicsSaveData();

            // 1. Ghi nhận hoàn thành mini-game Chương I
            save.MarkGameplayChapterCompleted("chapter_1");

            // 2. Mở khóa phần thưởng Bánh răng (gear)
            bool isFirstUnlock = InventoryManager.UnlockItem("gear", save);

            // 3. Đánh giá Final Progression (cấp Energy Crystal nếu Ch1 là phần cuối cùng còn thiếu)
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

            ShowSuccessModal(isFirstUnlock);
        }

        public void OnHintClicked()
        {
            if (IsStoryGateActive) return;
            if (_challenges == null || _challenges.Count == 0) return;
            var currentChallenge = _challenges[_currentChallengeIndex];
            ShowFeedback(currentChallenge.hintText, ColorGold);
        }

        public void OnResetClicked()
        {
            if (IsStoryGateActive) return;
            _mass = 1f;
            _height = 1f;
            _velocity = 2f;

            if (_sliderMass != null) _sliderMass.value = _mass;
            if (_sliderHeight != null) _sliderHeight.value = _height;
            if (_sliderVelocity != null) _sliderVelocity.value = _velocity;

            if (_txtMassVal != null) _txtMassVal.text = $"{_mass:F0} kg";
            if (_txtHeightVal != null) _txtHeightVal.text = $"{_height:F1} m";
            if (_txtVelocityVal != null) _txtVelocityVal.text = $"{_velocity:F0} m/s";

            CalculatePhysics();
            ShowFeedback("Đã đưa các đại lượng về trạng thái chuẩn ban đầu (m=1kg, h=1m, v=2m/s).", Color.white);
        }

        public void OnNextChallengeClicked()
        {
            if (IsStoryGateActive) return;
            if (_successModal != null) _successModal.SetActive(false);
            _currentChallengeIndex = (_currentChallengeIndex + 1) % _challenges.Count;
            UpdateChallengeDisplay();
            OnResetClicked();
        }

        public void OnPrevChallengeClicked()
        {
            if (IsStoryGateActive) return;
            if (_successModal != null) _successModal.SetActive(false);
            _currentChallengeIndex = (_currentChallengeIndex - 1 + _challenges.Count) % _challenges.Count;
            UpdateChallengeDisplay();
            OnResetClicked();
        }

        private void UpdateChallengeDisplay()
        {
            if (_challenges == null || _challenges.Count == 0) return;
            var challenge = _challenges[_currentChallengeIndex];

            if (_txtChallengeTitle != null)
                _txtChallengeTitle.text = $"{challenge.title} ({_currentChallengeIndex + 1}/{_challenges.Count})";

            if (_txtChallengeDesc != null)
                _txtChallengeDesc.text = challenge.targetDescription;

            if (_txtFeedback != null)
                _txtFeedback.text = "Điều chỉnh các đại lượng bên trái và nhấn <b>KIỂM TRA</b> khi sẵn sàng.";
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

        public void OnBackClicked()
        {
            Debug.Log("[MechanicsBalanceGame] Quay lại Scene 06_Chương I.");
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.NavigateTo(RouteId.ChapterMap1);
            }
            else if (PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.GameplayMechanics);
            }
        }

        // =========================================================================
        // SUCCESS MODAL & TUTORIAL MODAL
        // =========================================================================
        private void ShowSuccessModal(bool isFirstUnlock)
        {
            if (_successModal == null) return;

            Transform card = _successModal.transform.Find("SuccessCard");
            if (card != null)
            {
                var statsText = card.Find("StatsText")?.GetComponent<TextMeshProUGUI>();
                if (statsText != null)
                {
                    statsText.text = $"<b>Động năng Wđ:</b>  <color=#33C0FF>{_kineticEnergy:F1} J</color>\n" +
                                     $"<b>Thế năng Wt:</b>  <color=#FFA533>{_potentialEnergy:F1} J</color>\n" +
                                     $"<b>Tổng cơ năng W:</b>  <color=#FFD700>{_mechanicalEnergy:F1} J</color>";
                }

                var rewardText = card.Find("RewardText")?.GetComponent<TextMeshProUGUI>();
                if (rewardText != null)
                {
                    rewardText.text = isFirstUnlock
                        ? "<color=#55FF88>★ PHẦN THƯỞNG MỚI:</color> <b>Bánh răng (gear)</b> đã được thêm vào Kho đồ!"
                        : "<color=#FFD700>✓ ĐÃ THU THẬP:</color> Bánh răng cơ học (đã có trong Kho đồ).";
                }
            }

            _successModal.SetActive(true);
        }

        public void CloseTutorialModal()
        {
            if (_tutorialModal != null)
            {
                _tutorialModal.SetActive(false);
            }

            var save = PhysicsSaveManager.Load();
            if (save != null && !save.hasSeenMechanicsTutorial)
            {
                save.hasSeenMechanicsTutorial = true;
                PhysicsSaveManager.Save(save);
            }
        }

        public void CloseSuccessModalAndContinue()
        {
            if (_successModal != null) _successModal.SetActive(false);
            Transform targetCanvas = _canvas != null ? _canvas.transform : null;
            StoryFlowController.TryShowPostGameplayOrPendingStories(
                completedChapterHint: 1,
                customSave: null,
                onAllFinished: OnBackClicked,
                canvasTransform: targetCanvas);
        }

        public void CloseSuccessModalAndReplay()
        {
            if (_successModal != null) _successModal.SetActive(false);
            OnNextChallengeClicked();
        }

        // =========================================================================
        // XÂY DỰNG GIAO DIỆN RUNTIME AURELIA THEME
        // =========================================================================
        private void BuildGameplayUIIfMissing()
        {
            Transform existing = _canvas.transform.Find("MechanicsHUD");
            if (existing != null)
            {
                _hudRoot = existing.gameObject;
                return;
            }

            _hudRoot = new GameObject("MechanicsHUD");
            _hudRoot.transform.SetParent(_canvas.transform, false);
            RectTransform hudRt = _hudRoot.AddComponent<RectTransform>();
            hudRt.anchorMin = Vector2.zero;
            hudRt.anchorMax = Vector2.one;
            hudRt.sizeDelta = Vector2.zero;

            // 1. TOP HEADER & BACK BUTTON
            BuildHeaderAndMission();

            // 2. CỘT TRÁI: ĐIỀU KHIỂN (SLIDERS m, h, v)
            BuildControlPanel();

            // 3. CỘT GIỮA: MÔ PHỎNG VẬT THỂ (ENERGY ORB)
            BuildSimulationPanel();

            // 4. CỘT PHẢI: TÍNH TOÁN & ENERGY BARS
            BuildEnergyAndActionPanel();

            // 5. MODAL HƯỚNG DẪN & MODAL CHIẾN THẮNG
            BuildModals();
        }

        private void BuildHeaderAndMission()
        {
            // Back Button
            GameObject btnBackGo = CreateButton(_hudRoot.transform, "Btn_Back", new Vector2(-860, 480), new Vector2(140, 50), "Quay lại", OnBackClicked);

            // Title Banner
            GameObject titleGo = new GameObject("TitleBanner");
            titleGo.transform.SetParent(_hudRoot.transform, false);
            RectTransform titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0, 480);
            titleRt.sizeDelta = new Vector2(900, 50);

            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "HỌC VIỆN AURELIA · CÂN BẰNG NĂNG LƯỢNG";
            titleTmp.fontSize = 28;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = ColorGold;

            // Mission Panel
            GameObject missionGo = CreateNavyPanel(_hudRoot.transform, "MissionPanel", new Vector2(0, 400), new Vector2(1000, 95));

            // Prev / Next Challenge Buttons
            CreateButton(missionGo.transform, "Btn_Prev", new Vector2(-440, 0), new Vector2(60, 45), "◀", OnPrevChallengeClicked);
            CreateButton(missionGo.transform, "Btn_Next", new Vector2(440, 0), new Vector2(60, 45), "▶", OnNextChallengeClicked);

            // Challenge Title
            GameObject chTitleGo = new GameObject("ChallengeTitle");
            chTitleGo.transform.SetParent(missionGo.transform, false);
            RectTransform chTitleRt = chTitleGo.AddComponent<RectTransform>();
            chTitleRt.anchoredPosition = new Vector2(0, 20);
            chTitleRt.sizeDelta = new Vector2(750, 30);
            _txtChallengeTitle = chTitleGo.AddComponent<TextMeshProUGUI>();
            _txtChallengeTitle.fontSize = 20;
            _txtChallengeTitle.fontStyle = FontStyles.Bold;
            _txtChallengeTitle.alignment = TextAlignmentOptions.Center;
            _txtChallengeTitle.color = ColorGold;

            // Challenge Description
            GameObject chDescGo = new GameObject("ChallengeDesc");
            chDescGo.transform.SetParent(missionGo.transform, false);
            RectTransform chDescRt = chDescGo.AddComponent<RectTransform>();
            chDescRt.anchoredPosition = new Vector2(0, -15);
            chDescRt.sizeDelta = new Vector2(800, 35);
            _txtChallengeDesc = chDescGo.AddComponent<TextMeshProUGUI>();
            _txtChallengeDesc.fontSize = 18;
            _txtChallengeDesc.alignment = TextAlignmentOptions.Center;
            _txtChallengeDesc.color = Color.white;
        }

        private void BuildControlPanel()
        {
            GameObject ctrlPanel = CreateNavyPanel(_hudRoot.transform, "ControlPanel", new Vector2(-600, -20), new Vector2(480, 700));

            // Panel Title
            CreatePanelHeader(ctrlPanel.transform, "ĐIỀU KHIỂN ĐẠI LƯỢNG", new Vector2(0, 310));

            // Slider 1: Khối lượng m (1 - 10 kg)
            _sliderMass = CreateParameterSlider(ctrlPanel.transform, "Khối lượng (m):", "kg", 1f, 10f, _mass, new Vector2(0, 200),
                out _txtMassVal, val =>
                {
                    _mass = Mathf.Round(val);
                    if (_txtMassVal != null) _txtMassVal.text = $"{_mass:F0} kg";
                    CalculatePhysics();
                });

            // Slider 2: Độ cao h (0 - 10 m)
            _sliderHeight = CreateParameterSlider(ctrlPanel.transform, "Độ cao (h):", "m", 0f, 10f, _height, new Vector2(0, 50),
                out _txtHeightVal, val =>
                {
                    _height = Mathf.Round(val * 2f) / 2f; // Step 0.5m
                    if (_txtHeightVal != null) _txtHeightVal.text = $"{_height:F1} m";
                    CalculatePhysics();
                });

            // Slider 3: Vận tốc v (0 - 15 m/s)
            _sliderVelocity = CreateParameterSlider(ctrlPanel.transform, "Vận tốc (v):", "m/s", 0f, 15f, _velocity, new Vector2(0, -100),
                out _txtVelocityVal, val =>
                {
                    _velocity = Mathf.Round(val);
                    if (_txtVelocityVal != null) _txtVelocityVal.text = $"{_velocity:F0} m/s";
                    CalculatePhysics();
                });

            // Ghi chú công thức cơ sở
            GameObject noteGo = new GameObject("PhysicsNote");
            noteGo.transform.SetParent(ctrlPanel.transform, false);
            RectTransform noteRt = noteGo.AddComponent<RectTransform>();
            noteRt.anchoredPosition = new Vector2(0, -260);
            noteRt.sizeDelta = new Vector2(420, 100);
            var noteTmp = noteGo.AddComponent<TextMeshProUGUI>();
            noteTmp.fontSize = 15;
            noteTmp.alignment = TextAlignmentOptions.Center;
            noteTmp.color = new Color(0.8f, 0.85f, 0.95f, 0.85f);
            noteTmp.text = "<b>Quy chuẩn Vật lý 9:</b>\n" +
                           "Gia tốc g = 10 m/s²\n" +
                           "Độ cao h tính từ mặt đất chuẩn (0m)\n" +
                           "Khối lượng m đo bằng kilôgam (kg)";
        }

        private void BuildSimulationPanel()
        {
            GameObject simPanel = CreateNavyPanel(_hudRoot.transform, "SimulationFrame", new Vector2(0, -20), new Vector2(380, 700));
            CreatePanelHeader(simPanel.transform, "MÔ PHỎNG VẬT THỂ", new Vector2(0, 310));

            // Khung biểu diễn độ cao
            GameObject frameTrack = new GameObject("HeightRuler");
            frameTrack.transform.SetParent(simPanel.transform, false);
            RectTransform rulerRt = frameTrack.AddComponent<RectTransform>();
            rulerRt.anchoredPosition = new Vector2(0, 20);
            rulerRt.sizeDelta = new Vector2(6, 360);
            Image rulerImg = frameTrack.AddComponent<Image>();
            rulerImg.color = new Color(0.3f, 0.45f, 0.7f, 0.5f);

            // Mốc 10m
            CreateRulerMarker(simPanel.transform, "10 m (Cực đại)", new Vector2(80, 160));
            CreateRulerMarker(simPanel.transform, "5 m (Trung bình)", new Vector2(80, 20));
            CreateRulerMarker(simPanel.transform, "0 m (Mặt đất)", new Vector2(80, -120));

            // Energy Orb (Vật thể năng lượng)
            GameObject orbGo = new GameObject("EnergyOrb");
            orbGo.transform.SetParent(simPanel.transform, false);
            _orbTransform = orbGo.AddComponent<RectTransform>();
            _orbTransform.anchoredPosition = new Vector2(0, 0);
            _orbTransform.sizeDelta = new Vector2(70, 70);

            // Glow Aura
            GameObject glowGo = new GameObject("OrbGlow");
            glowGo.transform.SetParent(orbGo.transform, false);
            RectTransform glowRt = glowGo.AddComponent<RectTransform>();
            glowRt.sizeDelta = new Vector2(100, 100);
            _orbGlow = glowGo.AddComponent<Image>();
            _orbGlow.color = new Color(ColorGold.r, ColorGold.g, ColorGold.b, 0.5f);

            // Core Orb
            Image coreImg = orbGo.AddComponent<Image>();
            coreImg.color = ColorGold;

            // Height Badge
            GameObject badgeGo = new GameObject("HeightBadge");
            badgeGo.transform.SetParent(orbGo.transform, false);
            RectTransform badgeRt = badgeGo.AddComponent<RectTransform>();
            badgeRt.anchoredPosition = new Vector2(0, -45);
            badgeRt.sizeDelta = new Vector2(100, 25);
            _orbHeightBadge = badgeGo.AddComponent<TextMeshProUGUI>();
            _orbHeightBadge.fontSize = 14;
            _orbHeightBadge.fontStyle = FontStyles.Bold;
            _orbHeightBadge.alignment = TextAlignmentOptions.Center;
            _orbHeightBadge.color = Color.white;

            // Dòng thông tin động năng dưới chân
            GameObject kineticLabelGo = new GameObject("SimFooterText");
            kineticLabelGo.transform.SetParent(simPanel.transform, false);
            RectTransform kRt = kineticLabelGo.AddComponent<RectTransform>();
            kRt.anchoredPosition = new Vector2(0, -280);
            kRt.sizeDelta = new Vector2(340, 60);
            var footerTmp = kineticLabelGo.AddComponent<TextMeshProUGUI>();
            footerTmp.fontSize = 14;
            footerTmp.alignment = TextAlignmentOptions.Center;
            footerTmp.color = new Color(0.75f, 0.85f, 1f, 0.8f);
            footerTmp.text = "Vật thể nâng lên $\\rightarrow$ Thế năng tích lũy\n" +
                             "Vận tốc tăng $\\rightarrow$ Động năng phát quang rực rỡ";
        }

        private void BuildEnergyAndActionPanel()
        {
            GameObject rightPanel = CreateNavyPanel(_hudRoot.transform, "EnergyPanel", new Vector2(600, -20), new Vector2(480, 700));
            CreatePanelHeader(rightPanel.transform, "NĂNG LƯỢNG & ĐÁNH GIÁ", new Vector2(0, 310));

            // 1. Text Động năng
            GameObject txtKdGo = new GameObject("Txt_Kinetic");
            txtKdGo.transform.SetParent(rightPanel.transform, false);
            RectTransform kRt = txtKdGo.AddComponent<RectTransform>();
            kRt.anchoredPosition = new Vector2(0, 245);
            kRt.sizeDelta = new Vector2(420, 50);
            _txtKinetic = txtKdGo.AddComponent<TextMeshProUGUI>();
            _txtKinetic.fontSize = 17;

            // Bar Động năng
            _barFillKinetic = CreateEnergyBar(rightPanel.transform, "Bar_Kinetic", new Vector2(0, 205), ColorCyanKinetic);

            // 2. Text Thế năng
            GameObject txtPtGo = new GameObject("Txt_Potential");
            txtPtGo.transform.SetParent(rightPanel.transform, false);
            RectTransform pRt = txtPtGo.AddComponent<RectTransform>();
            pRt.anchoredPosition = new Vector2(0, 150);
            pRt.sizeDelta = new Vector2(420, 50);
            _txtPotential = txtPtGo.AddComponent<TextMeshProUGUI>();
            _txtPotential.fontSize = 17;

            // Bar Thế năng
            _barFillPotential = CreateEnergyBar(rightPanel.transform, "Bar_Potential", new Vector2(0, 110), ColorAmberPotential);

            // 3. Text Cơ năng
            GameObject txtMcGo = new GameObject("Txt_Mechanical");
            txtMcGo.transform.SetParent(rightPanel.transform, false);
            RectTransform mRt = txtMcGo.AddComponent<RectTransform>();
            mRt.anchoredPosition = new Vector2(0, 50);
            mRt.sizeDelta = new Vector2(420, 50);
            _txtMechanical = txtMcGo.AddComponent<TextMeshProUGUI>();
            _txtMechanical.fontSize = 18;

            // Bar Tổng Cơ năng
            _barFillTotal = CreateEnergyBar(rightPanel.transform, "Bar_Total", new Vector2(0, 10), ColorGold);

            // 4. Feedback Box
            GameObject feedbackBox = CreateNavyPanel(rightPanel.transform, "FeedbackBox", new Vector2(0, -90), new Vector2(430, 85));
            _txtFeedback = feedbackBox.AddComponent<TextMeshProUGUI>();
            _txtFeedback.fontSize = 15;
            _txtFeedback.alignment = TextAlignmentOptions.Center;
            _txtFeedback.color = Color.white;
            _txtFeedback.text = "Điều chỉnh các đại lượng bên trái và nhấn <b>KIỂM TRA</b> khi sẵn sàng.";

            // 5. Action Buttons
            CreateButton(rightPanel.transform, "Btn_Check", new Vector2(0, -180), new Vector2(300, 50), "★  KIỂM TRA", OnCheckClicked, true);
            CreateButton(rightPanel.transform, "Btn_Hint", new Vector2(-110, -250), new Vector2(170, 42), "GỢI Ý", OnHintClicked);
            CreateButton(rightPanel.transform, "Btn_Reset", new Vector2(110, -250), new Vector2(170, 42), "LÀM LẠI", OnResetClicked);
        }

        private void BuildModals()
        {
            // --- 1. MODAL HƯỚNG DẪN (TUTORIAL) ---
            _tutorialModal = new GameObject("TutorialModal");
            _tutorialModal.transform.SetParent(_canvas.transform, false);
            RectTransform tutRt = _tutorialModal.AddComponent<RectTransform>();
            tutRt.anchorMin = Vector2.zero;
            tutRt.anchorMax = Vector2.one;
            tutRt.sizeDelta = Vector2.zero;

            // Nền mờ chặn click
            Image blockerTut = _tutorialModal.AddComponent<Image>();
            blockerTut.color = new Color(0, 0, 0, 0.65f);

            GameObject tutCard = CreateNavyPanel(_tutorialModal.transform, "TutorialCard", Vector2.zero, new Vector2(720, 460));

            CreatePanelHeader(tutCard.transform, "HƯỚNG DẪN CÂN BẰNG NĂNG LƯỢNG", new Vector2(0, 180));

            GameObject tutBody = new GameObject("BodyText");
            tutBody.transform.SetParent(tutCard.transform, false);
            RectTransform tbRt = tutBody.AddComponent<RectTransform>();
            tbRt.anchoredPosition = new Vector2(0, 20);
            tbRt.sizeDelta = new Vector2(620, 260);
            var tbTmp = tutBody.AddComponent<TextMeshProUGUI>();
            tbTmp.fontSize = 18;
            tbTmp.lineSpacing = 15;
            tbTmp.color = Color.white;
            tbTmp.text = "<b>Chào mừng bạn đến với Phòng Thí nghiệm Cơ năng Aurelia!</b>\n\n" +
                         "1. <b>Kéo thanh trượt</b> bên trái để thay đổi Khối lượng (m), Độ cao (h) và Vận tốc (v).\n" +
                         "2. <b>Quan sát</b> Động năng và Thế năng biến đổi theo thời gian thực.\n" +
                         "3. <b>Mục tiêu:</b> Thiết lập các đại lượng sao cho thỏa mãn thử thách trên thanh nhiệm vụ.\n" +
                         "4. Nhấn <b>KIỂM TRA</b> để nhận chứng nhận và mở khóa trang bị đặc biệt.";

            CreateButton(tutCard.transform, "Btn_Understood", new Vector2(0, -170), new Vector2(220, 48), "ĐÃ HIỂU", CloseTutorialModal, true);
            _tutorialModal.SetActive(false);

            // --- 2. MODAL CHIẾN THẮNG (SUCCESS) ---
            _successModal = new GameObject("SuccessModal");
            _successModal.transform.SetParent(_canvas.transform, false);
            RectTransform sucRt = _successModal.AddComponent<RectTransform>();
            sucRt.anchorMin = Vector2.zero;
            sucRt.anchorMax = Vector2.one;
            sucRt.sizeDelta = Vector2.zero;

            Image blockerSuc = _successModal.AddComponent<Image>();
            blockerSuc.color = new Color(0, 0, 0, 0.70f);

            GameObject sucCard = CreateNavyPanel(_successModal.transform, "SuccessCard", Vector2.zero, new Vector2(760, 520));

            CreatePanelHeader(sucCard.transform, "HOÀN THÀNH XUẤT SẮC!", new Vector2(0, 205));

            GameObject congText = new GameObject("CongText");
            congText.transform.SetParent(sucCard.transform, false);
            RectTransform cRt = congText.AddComponent<RectTransform>();
            cRt.anchoredPosition = new Vector2(0, 140);
            cRt.sizeDelta = new Vector2(650, 40);
            var cTmp = congText.AddComponent<TextMeshProUGUI>();
            cTmp.fontSize = 20;
            cTmp.alignment = TextAlignmentOptions.Center;
            cTmp.color = ColorGreenSuccess;
            cTmp.text = "Bạn đã khôi phục trạng thái Cân bằng Năng lượng thành công!";

            // Thống kê năng lượng
            GameObject statsGo = new GameObject("StatsText");
            statsGo.transform.SetParent(sucCard.transform, false);
            RectTransform stRt = statsGo.AddComponent<RectTransform>();
            stRt.anchoredPosition = new Vector2(0, 45);
            stRt.sizeDelta = new Vector2(600, 110);
            var stTmp = statsGo.AddComponent<TextMeshProUGUI>();
            stTmp.fontSize = 19;
            stTmp.lineSpacing = 12;
            stTmp.alignment = TextAlignmentOptions.Center;
            stTmp.color = Color.white;

            // Phần thưởng
            GameObject rewGo = new GameObject("RewardText");
            rewGo.transform.SetParent(sucCard.transform, false);
            RectTransform rRt = rewGo.AddComponent<RectTransform>();
            rRt.anchoredPosition = new Vector2(0, -65);
            rRt.sizeDelta = new Vector2(650, 60);
            var rTmp = rewGo.AddComponent<TextMeshProUGUI>();
            rTmp.fontSize = 18;
            rTmp.alignment = TextAlignmentOptions.Center;
            rTmp.color = ColorGold;

            // Buttons
            CreateButton(sucCard.transform, "Btn_Continue", new Vector2(-150, -180), new Vector2(240, 50), "TIẾP TỤC HỌC", CloseSuccessModalAndContinue, true);
            CreateButton(sucCard.transform, "Btn_NextChallenge", new Vector2(150, -180), new Vector2(240, 50), "THỬ THÁCH TIẾP", CloseSuccessModalAndReplay);

            _successModal.SetActive(false);
        }

        // =========================================================================
        // UI HELPERS (AURELIA STYLING)
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

            // Border viền vàng
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
            Action<float> onValueChanged)
        {
            GameObject container = new GameObject("Param_" + label);
            container.transform.SetParent(parent, false);
            RectTransform cRt = container.AddComponent<RectTransform>();
            cRt.anchoredPosition = pos;
            cRt.sizeDelta = new Vector2(420, 110);

            // Title
            GameObject titleGo = new GameObject("Label");
            titleGo.transform.SetParent(container.transform, false);
            RectTransform tRt = titleGo.AddComponent<RectTransform>();
            tRt.anchoredPosition = new Vector2(-100, 35);
            tRt.sizeDelta = new Vector2(200, 30);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = label;
            titleTmp.fontSize = 18;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = ColorGold;

            // Value Badge
            GameObject valGo = new GameObject("Value");
            valGo.transform.SetParent(container.transform, false);
            RectTransform vRt = valGo.AddComponent<RectTransform>();
            vRt.anchoredPosition = new Vector2(130, 35);
            vRt.sizeDelta = new Vector2(120, 30);
            valueText = valGo.AddComponent<TextMeshProUGUI>();
            valueText.text = $"{initial:F0} {unit}";
            valueText.fontSize = 20;
            valueText.fontStyle = FontStyles.Bold;
            valueText.alignment = TextAlignmentOptions.Right;
            valueText.color = Color.white;

            // Slider Background Track
            GameObject sliderGo = new GameObject("Slider");
            sliderGo.transform.SetParent(container.transform, false);
            RectTransform sRt = sliderGo.AddComponent<RectTransform>();
            sRt.anchoredPosition = new Vector2(0, -10);
            sRt.sizeDelta = new Vector2(340, 20);

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

            // Handle Slide Area
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

            // Minus / Plus Buttons
            CreateButton(container.transform, "Btn_Minus", new Vector2(-190, -10), new Vector2(34, 34), "−", () =>
            {
                if (IsStoryGateActive) return;
                slider.value = Mathf.Max(slider.minValue, slider.value - (slider.maxValue > 10 ? 1f : 0.5f));
            });

            CreateButton(container.transform, "Btn_Plus", new Vector2(190, -10), new Vector2(34, 34), "+", () =>
            {
                if (IsStoryGateActive) return;
                slider.value = Mathf.Min(slider.maxValue, slider.value + (slider.maxValue > 10 ? 1f : 0.5f));
            });

            slider.onValueChanged.AddListener(val =>
            {
                if (IsStoryGateActive) return;
                onValueChanged?.Invoke(val);
            });
            return slider;
        }

        private Image CreateEnergyBar(Transform parent, string name, Vector2 pos, Color fillColor)
        {
            GameObject barGo = new GameObject(name);
            barGo.transform.SetParent(parent, false);
            RectTransform bRt = barGo.AddComponent<RectTransform>();
            bRt.anchoredPosition = pos;
            bRt.sizeDelta = new Vector2(420, 18);

            // Nền thanh
            Image bgImg = barGo.AddComponent<Image>();
            bgImg.color = new Color(0.12f, 0.18f, 0.28f, 1f);

            var outline = barGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.4f, 0.5f, 0.7f, 0.4f);
            outline.effectDistance = new Vector2(1, -1);

            // Thanh Fill
            GameObject fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(barGo.transform, false);
            RectTransform fRt = fillGo.AddComponent<RectTransform>();
            fRt.anchorMin = Vector2.zero;
            fRt.anchorMax = Vector2.one;
            fRt.sizeDelta = Vector2.zero;

            Image fillImg = fillGo.AddComponent<Image>();
            fillImg.color = fillColor;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0;
            fillImg.fillAmount = 0.5f;

            return fillImg;
        }

        private void CreateRulerMarker(Transform parent, string label, Vector2 pos)
        {
            GameObject markerGo = new GameObject("Marker_" + label);
            markerGo.transform.SetParent(parent, false);
            RectTransform mRt = markerGo.AddComponent<RectTransform>();
            mRt.anchoredPosition = pos;
            mRt.sizeDelta = new Vector2(140, 25);

            var tmp = markerGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 14;
            tmp.color = new Color(0.7f, 0.8f, 0.95f, 0.7f);
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
