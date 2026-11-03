using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển logic giao diện Scene 14_Settings.
    /// Quản lý 3 thanh trượt: Âm lượng nhạc, Âm lượng hiệu ứng và Độ sáng màn hình.
    /// Hỗ trợ kéo thả chuột trực tiếp trên thanh artwork hiện có.
    /// Lưu giá trị an toàn vào PhysicsSaveData và phản hồi trực quan.
    /// </summary>
    public class SettingsUIController : MonoBehaviour
    {
        [Header("=== BUTTONS ===")]
        [SerializeField] private Button btnSave;
        [SerializeField] private Button btnBack;

        [Header("=== SLIDER REFERENCES ===")]
        [SerializeField] private RectTransform trackMusic;
        [SerializeField] private RectTransform knobMusic;

        [SerializeField] private RectTransform trackSfx;
        [SerializeField] private RectTransform knobSfx;

        [SerializeField] private RectTransform trackBrightness;
        [SerializeField] private RectTransform knobBrightness;

        [Header("=== FEEDBACK UI ===")]
        private TextMeshProUGUI _feedbackText;
        private Coroutine _feedbackCoroutine;

        // Giá trị hiện thời trên UI (0.0 đến 1.0)
        private float _currentMusic = 0.7f;
        private float _currentSfx = 0.8f;
        private float _currentBrightness = 1.0f;

        private const float KNOB_MIN_X = -157f;
        private const float KNOB_MAX_X = 157f;

        private void Awake()
        {
            EnsureEventSystem();
            FindUIReferences();
            SetupButtons();
            SetupDraggableSliders();
            BuildFeedbackTextIfMissing();
        }

        private void Start()
        {
            LoadSettingsToUI();
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

        private void FindUIReferences()
        {
            // 1. Nút Lưu (luu)
            GameObject luuGo = GameObject.Find("luu");
            if (luuGo != null)
            {
                btnSave = luuGo.GetComponent<Button>();
                if (btnSave == null) btnSave = luuGo.AddComponent<Button>();
                Image img = luuGo.GetComponent<Image>();
                if (img != null) { img.raycastTarget = true; btnSave.targetGraphic = img; }
            }

            // 2. Nút Quay lại (quaylai)
            GameObject backGo = GameObject.Find("quaylai");
            if (backGo != null)
            {
                btnBack = backGo.GetComponent<Button>();
                if (btnBack == null) btnBack = backGo.AddComponent<Button>();
                Image img = backGo.GetComponent<Image>();
                if (img != null) { img.raycastTarget = true; btnBack.targetGraphic = img; }
            }

            // 3. Thanh Âm lượng nhạc: chinh
            GameObject musicGo = GameObject.Find("chinh");
            if (musicGo != null)
            {
                trackMusic = musicGo.GetComponent<RectTransform>();
                Transform nut = musicGo.transform.Find("nut");
                if (nut != null) knobMusic = nut.GetComponent<RectTransform>();
            }

            // 4. Thanh Âm lượng hiệu ứng: chinh (1)
            GameObject sfxGo = GameObject.Find("chinh (1)");
            if (sfxGo != null)
            {
                trackSfx = sfxGo.GetComponent<RectTransform>();
                Transform nut = sfxGo.transform.Find("nut");
                if (nut != null) knobSfx = nut.GetComponent<RectTransform>();
            }

            // 5. Thanh Độ sáng: chinh (2)
            GameObject brightGo = GameObject.Find("chinh (2)");
            if (brightGo != null)
            {
                trackBrightness = brightGo.GetComponent<RectTransform>();
                Transform nut = brightGo.transform.Find("nut");
                if (nut != null) knobBrightness = nut.GetComponent<RectTransform>();
            }
        }

        private void SetupButtons()
        {
            if (btnSave != null)
            {
                btnSave.onClick.RemoveAllListeners();
                btnSave.onClick.AddListener(OnSaveClicked);
                var fb = btnSave.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);
                UIHoverPulseEffect.Attach(btnSave.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
            }

            if (btnBack != null)
            {
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackClicked);
                var fb = btnBack.GetComponent<UIButtonFeedback>();
                if (fb != null) Destroy(fb);
                UIHoverPulseEffect.Attach(btnBack.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
            }
        }

        private void SetupDraggableSliders()
        {
            SetupSliderTrack(trackMusic, knobMusic, val =>
            {
                _currentMusic = val;
                if (SettingsManager.Instance != null) SettingsManager.Instance.ApplyMusicVolume(val);
            });

            SetupSliderTrack(trackSfx, knobSfx, val =>
            {
                _currentSfx = val;
                if (SettingsManager.Instance != null) SettingsManager.Instance.ApplySFXVolume(val);
            });

            SetupSliderTrack(trackBrightness, knobBrightness, val =>
            {
                // Độ sáng tối thiểu là 0.2 để không làm tối đen hoàn toàn màn hình
                _currentBrightness = Mathf.Lerp(0.2f, 1.0f, val);
                if (SettingsManager.Instance != null) SettingsManager.Instance.ApplyBrightness(_currentBrightness);
            });
        }

        private void SetupSliderTrack(RectTransform track, RectTransform knob, Action<float> onValueChanged)
        {
            if (track == null) return;

            // Đảm bảo Image trên track có thể nhận click chuột
            Image img = track.GetComponent<Image>();
            if (img != null) img.raycastTarget = true;

            var dragHandler = track.gameObject.GetComponent<UISliderDragHandler>();
            if (dragHandler == null) dragHandler = track.gameObject.AddComponent<UISliderDragHandler>();

            dragHandler.Initialize(track, knob, KNOB_MIN_X, KNOB_MAX_X, onValueChanged);
        }

        private void LoadSettingsToUI()
        {
            var save = PhysicsSaveManager.Load();
            if (save != null && save.settings != null)
            {
                _currentMusic = save.settings.musicVolume;
                _currentSfx = save.settings.sfxVolume;
                _currentBrightness = save.settings.brightness;
            }
            else
            {
                _currentMusic = 0.7f;
                _currentSfx = 0.8f;
                _currentBrightness = 1.0f;
            }

            // Cập nhật vị trí hiển thị của các nút trượt (knob)
            SetKnobPosition(knobMusic, _currentMusic);
            SetKnobPosition(knobSfx, _currentSfx);

            float brightNormalized = Mathf.InverseLerp(0.2f, 1.0f, _currentBrightness);
            SetKnobPosition(knobBrightness, brightNormalized);

            // Áp dụng độ sáng ngay
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.ApplyBrightness(_currentBrightness);
            }
        }

        private void SetKnobPosition(RectTransform knob, float normalizedVal)
        {
            if (knob == null) return;
            float clamped = Mathf.Clamp01(normalizedVal);
            float targetX = Mathf.Lerp(KNOB_MIN_X, KNOB_MAX_X, clamped);
            knob.anchoredPosition = new Vector2(targetX, knob.anchoredPosition.y);
        }

        private void OnSaveClicked()
        {
            var save = PhysicsSaveManager.Load();
            if (save != null)
            {
                if (save.settings == null) save.settings = new GameSettingsData();
                save.settings.musicVolume = _currentMusic;
                save.settings.sfxVolume = _currentSfx;
                save.settings.brightness = _currentBrightness;

                PhysicsSaveManager.Save(save);
                PhysicsGameSession.Instance.ReloadSaveData();

                if (SettingsManager.Instance != null)
                {
                    SettingsManager.Instance.ApplyAllSettingsFromSave();
                }

                Debug.Log($"[SettingsUIController] Đã lưu cài đặt: Music={_currentMusic:F2}, SFX={_currentSfx:F2}, Brightness={_currentBrightness:F2}");
                ShowFeedback("Đã lưu cài đặt");
            }
        }

        private void OnBackClicked()
        {
            Debug.Log("[SettingsUIController] Quay lại màn hình trước hoặc Main Menu.");
            if (NavigationService.Instance != null &&
                NavigationService.Instance.PreviousRoute != RouteId.None &&
                NavigationService.Instance.PreviousRoute != RouteId.Settings)
            {
                NavigationService.Instance.NavigateTo(NavigationService.Instance.PreviousRoute);
            }
            else
            {
                PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.MainMenu);
            }
        }

        private void BuildFeedbackTextIfMissing()
        {
            if (_feedbackText != null) return;

            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Transform existing = canvas.transform.Find("SaveFeedbackText");
            if (existing != null)
            {
                _feedbackText = existing.GetComponent<TextMeshProUGUI>();
                return;
            }

            GameObject feedbackGo = new GameObject("SaveFeedbackText");
            feedbackGo.transform.SetParent(canvas.transform, false);
            RectTransform rt = feedbackGo.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -350);
            rt.sizeDelta = new Vector2(500, 50);

            _feedbackText = feedbackGo.AddComponent<TextMeshProUGUI>();
            _feedbackText.fontSize = 24;
            _feedbackText.fontStyle = FontStyles.Bold;
            _feedbackText.alignment = TextAlignmentOptions.Center;
            _feedbackText.color = new Color(0.3f, 0.95f, 0.45f, 1f); // Xanh lá thông báo
            _feedbackText.text = "";
            _feedbackText.gameObject.SetActive(false);
        }

        private void ShowFeedback(string message)
        {
            if (_feedbackText == null) return;

            _feedbackText.text = message;
            _feedbackText.gameObject.SetActive(true);

            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(HideFeedbackRoutine());
        }

        private IEnumerator HideFeedbackRoutine()
        {
            yield return new WaitForSeconds(2.0f);
            if (_feedbackText != null) _feedbackText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Component hỗ trợ kéo thả chuột hoặc nhấp chuột trên thanh trượt cài đặt.
    /// </summary>
    public class UISliderDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        private RectTransform _track;
        private RectTransform _knob;
        private float _minX;
        private float _maxX;
        private Action<float> _onValueChanged;

        public void Initialize(RectTransform track, RectTransform knob, float minX, float maxX, Action<float> onValueChanged)
        {
            _track = track;
            _knob = knob;
            _minX = minX;
            _maxX = maxX;
            _onValueChanged = onValueChanged;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            UpdatePositionFromPointer(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdatePositionFromPointer(eventData);
        }

        private void UpdatePositionFromPointer(PointerEventData eventData)
        {
            if (_track == null || _knob == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_track, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

            float clampedX = Mathf.Clamp(localPoint.x, _minX, _maxX);
            _knob.anchoredPosition = new Vector2(clampedX, _knob.anchoredPosition.y);

            float normalized = Mathf.InverseLerp(_minX, _maxX, clampedX);
            _onValueChanged?.Invoke(normalized);
        }
    }
}
