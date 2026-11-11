using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển toàn bộ logic giao diện Scene 14_Settings theo thiết kế Aurelia Academy.
    /// Quản lý 2 công tắc độc lập (Toggle switches):
    /// 1. Nhạc nền (BGM Toggle)
    /// 2. Hiệu ứng âm thanh (SFX Toggle)
    /// Hỗ trợ đầy đủ 4 trạng thái trực quan độc lập (ON/ON, ON/OFF, OFF/ON, OFF/OFF).
    /// Nút 'Lưu cài đặt': Lưu trạng thái vào PlayerPrefs và PhysicsSaveData.
    /// Nút 'Quay lại': Trở về MainMenu (hoặc màn hình trước). Nếu chưa bấm Lưu, tự động hoàn tác về trạng thái đã lưu.
    /// </summary>
    public class SettingsUIController : MonoBehaviour
    {
        [Header("=== TOGGLES ===")]
        [SerializeField] private Button btnToggleMusic;
        [SerializeField] private Image imgToggleMusic;
        [SerializeField] private Button btnToggleSfx;
        [SerializeField] private Image imgToggleSfx;

        [Header("=== BUTTONS ===")]
        [SerializeField] private Button btnSave;
        [SerializeField] private Button btnBack;

        [Header("=== TOGGLE SPRITES ===")]
        [SerializeField] private Sprite spriteToggleOn;
        [SerializeField] private Sprite spriteToggleOff;

        [Header("=== FEEDBACK UI ===")]
        [SerializeField] private TextMeshProUGUI feedbackText;
        private Coroutine _feedbackCoroutine;

        // Trạng thái hiện thời trên UI
        private bool _currentMusic = true;
        private bool _currentSfx = true;

        // Trạng thái đã lưu thực tế
        private bool _savedMusic = true;
        private bool _savedSfx = true;

        public bool CurrentMusic => _currentMusic;
        public bool CurrentSfx => _currentSfx;
        public bool SavedMusic => _savedMusic;
        public bool SavedSfx => _savedSfx;

        public Button BtnToggleMusic => btnToggleMusic;
        public Button BtnToggleSfx => btnToggleSfx;
        public Button BtnSave => btnSave;
        public Button BtnBack => btnBack;

        private void Awake()
        {
            EnsureEventSystem();
            FindUIReferences();
            LoadSpritesIfMissing();
            SetupButtonCallbacks();
            BuildFeedbackTextIfMissing();
        }

        private void Start()
        {
            LoadSavedSettingsToUI();
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

        public void FindUIReferences()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. Công tắc Nhạc nền: Toggle_Music hoặc Image (3)
            if (btnToggleMusic == null)
            {
                Transform t = canvas.transform.Find("Toggle_Music") ?? canvas.transform.Find("Image (3)");
                if (t != null)
                {
                    btnToggleMusic = t.GetComponent<Button>() ?? t.gameObject.AddComponent<Button>();
                    imgToggleMusic = t.GetComponent<Image>();
                }
            }
            if (imgToggleMusic == null && btnToggleMusic != null)
            {
                imgToggleMusic = btnToggleMusic.GetComponent<Image>();
            }

            // 2. Công tắc Hiệu ứng âm thanh: Toggle_SFX hoặc Image (4)
            if (btnToggleSfx == null)
            {
                Transform t = canvas.transform.Find("Toggle_SFX") ?? canvas.transform.Find("Image (4)");
                if (t != null)
                {
                    btnToggleSfx = t.GetComponent<Button>() ?? t.gameObject.AddComponent<Button>();
                    imgToggleSfx = t.GetComponent<Image>();
                }
            }
            if (imgToggleSfx == null && btnToggleSfx != null)
            {
                imgToggleSfx = btnToggleSfx.GetComponent<Image>();
            }

            // 3. Nút Lưu: Btn_Save hoặc Image (pos.y < -200, pos.x < 0)
            if (btnSave == null)
            {
                Transform t = canvas.transform.Find("Btn_Save") ?? canvas.transform.Find("luu");
                if (t == null)
                {
                    foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                    {
                        RectTransform rt = img.rectTransform;
                        if (rt.anchoredPosition.y < -200f && rt.anchoredPosition.x < -100f && rt.sizeDelta.x > 250f)
                        {
                            t = img.transform;
                            break;
                        }
                    }
                }
                if (t != null)
                {
                    btnSave = t.GetComponent<Button>() ?? t.gameObject.AddComponent<Button>();
                }
            }

            // 4. Nút Quay lại: Btn_Back hoặc Image (5) (pos.y < -200, pos.x > 0)
            if (btnBack == null)
            {
                Transform t = canvas.transform.Find("Btn_Back") ?? canvas.transform.Find("quaylai") ?? canvas.transform.Find("Image (5)");
                if (t == null)
                {
                    foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                    {
                        RectTransform rt = img.rectTransform;
                        if (rt.anchoredPosition.y < -200f && rt.anchoredPosition.x > 100f && rt.sizeDelta.x > 250f)
                        {
                            t = img.transform;
                            break;
                        }
                    }
                }
                if (t != null)
                {
                    btnBack = t.GetComponent<Button>() ?? t.gameObject.AddComponent<Button>();
                }
            }

            // Đảm bảo raycastTarget cho các nút và toggle
            ConfigureRaycast(btnToggleMusic);
            ConfigureRaycast(btnToggleSfx);
            ConfigureRaycast(btnSave);
            ConfigureRaycast(btnBack);
        }

        private void ConfigureRaycast(Button btn)
        {
            if (btn == null) return;
            btn.interactable = true;
            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = true;
                btn.targetGraphic = img;
            }
            btn.transition = Selectable.Transition.ColorTint;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.05f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = colors;
        }

        private void LoadSpritesIfMissing()
        {
            if (spriteToggleOn == null)
            {
                spriteToggleOn = Resources.Load<Sprite>("UI/Toggle_On");
                if (spriteToggleOn == null && imgToggleMusic != null)
                {
                    spriteToggleOn = imgToggleMusic.sprite;
                }
            }

            if (spriteToggleOff == null)
            {
                spriteToggleOff = Resources.Load<Sprite>("UI/Toggle_Off");
            }
        }

        private void SetupButtonCallbacks()
        {
            if (btnToggleMusic != null)
            {
                btnToggleMusic.onClick.RemoveAllListeners();
                btnToggleMusic.onClick.AddListener(OnToggleMusicClicked);
            }

            if (btnToggleSfx != null)
            {
                btnToggleSfx.onClick.RemoveAllListeners();
                btnToggleSfx.onClick.AddListener(OnToggleSfxClicked);
            }

            if (btnSave != null)
            {
                btnSave.onClick.RemoveAllListeners();
                btnSave.onClick.AddListener(OnSaveClicked);
                UIHoverPulseEffect.Attach(btnSave.gameObject, hover: 1.05f, minPulse: 1.02f, maxPulse: 1.06f, outline: true);
            }

            if (btnBack != null)
            {
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackClicked);
                UIHoverPulseEffect.Attach(btnBack.gameObject, hover: 1.05f, minPulse: 1.02f, maxPulse: 1.06f, outline: true);
            }
        }

        public void LoadSavedSettingsToUI()
        {
            _savedMusic = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
            _savedSfx = PlayerPrefs.GetInt("SfxEnabled", 1) == 1;

            _currentMusic = _savedMusic;
            _currentSfx = _savedSfx;

            UpdateToggleVisuals();

            // Áp dụng tức thì trạng thái đã lưu vào hệ thống âm thanh
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.ApplyMusicEnabled(_currentMusic);
                SettingsManager.Instance.ApplySfxEnabled(_currentSfx);
            }
        }

        private void UpdateToggleVisuals()
        {
            if (imgToggleMusic != null)
            {
                imgToggleMusic.sprite = _currentMusic ? spriteToggleOn : spriteToggleOff;
            }

            if (imgToggleSfx != null)
            {
                imgToggleSfx.sprite = _currentSfx ? spriteToggleOn : spriteToggleOff;
            }
        }

        public void OnToggleMusicClicked()
        {
            _currentMusic = !_currentMusic;
            UpdateToggleVisuals();

            // Phát hiện tức thì âm thanh thật
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.ApplyMusicEnabled(_currentMusic);
                SettingsManager.Instance.PlaySFX();
            }

            Debug.Log($"[SettingsUIController] Toggle Music clicked: {_currentMusic}");
        }

        public void OnToggleSfxClicked()
        {
            _currentSfx = !_currentSfx;
            UpdateToggleVisuals();

            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.ApplySfxEnabled(_currentSfx);
                SettingsManager.Instance.PlaySFX();
            }

            Debug.Log($"[SettingsUIController] Toggle SFX clicked: {_currentSfx}");
        }

        public void OnSaveClicked()
        {
            _savedMusic = _currentMusic;
            _savedSfx = _currentSfx;

            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SaveSettings(_savedMusic, _savedSfx);
                SettingsManager.Instance.PlaySFX();
            }
            else
            {
                PlayerPrefs.SetInt("MusicEnabled", _savedMusic ? 1 : 0);
                PlayerPrefs.SetInt("SfxEnabled", _savedSfx ? 1 : 0);
                PlayerPrefs.Save();
            }

            ShowFeedback("Đã lưu cài đặt");
            Debug.Log($"[SettingsUIController] Đã lưu: Music={_savedMusic}, SFX={_savedSfx}");
        }

        public void OnBackClicked()
        {
            // Nếu người chơi đổi công tắc nhưng chưa bấm Lưu -> khôi phục trạng thái đã lưu trước đó
            if (_currentMusic != _savedMusic || _currentSfx != _savedSfx)
            {
                _currentMusic = _savedMusic;
                _currentSfx = _savedSfx;
                UpdateToggleVisuals();

                if (SettingsManager.Instance != null)
                {
                    SettingsManager.Instance.ApplyMusicEnabled(_savedMusic);
                    SettingsManager.Instance.ApplySfxEnabled(_savedSfx);
                }
                Debug.Log("[SettingsUIController] Hủy thay đổi, khôi phục cài đặt trước đó.");
            }

            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.PlaySFX();
            }

            // Trở về Main Menu hoặc route trước
            if (NavigationService.Instance != null &&
                NavigationService.Instance.PreviousRoute != RouteId.None &&
                NavigationService.Instance.PreviousRoute != RouteId.Settings)
            {
                NavigationService.Instance.NavigateTo(NavigationService.Instance.PreviousRoute);
            }
            else
            {
                if (NavigationService.Instance != null)
                {
                    NavigationService.Instance.NavigateTo(RouteId.MainMenu);
                }
                else if (PhysicsGameSession.Instance != null)
                {
                    PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.MainMenu);
                }
                else
                {
                    SceneManager.LoadScene(PhysicsSceneNames.MainMenu);
                }
            }
        }

        private void BuildFeedbackTextIfMissing()
        {
            if (feedbackText != null) return;

            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Transform existing = canvas.transform.Find("SaveFeedbackText");
            if (existing != null)
            {
                feedbackText = existing.GetComponent<TextMeshProUGUI>();
                return;
            }

            GameObject feedbackGo = new GameObject("SaveFeedbackText");
            feedbackGo.transform.SetParent(canvas.transform, false);
            RectTransform rt = feedbackGo.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -350);
            rt.sizeDelta = new Vector2(500, 50);

            feedbackText = feedbackGo.AddComponent<TextMeshProUGUI>();
            feedbackText.fontSize = 26;
            feedbackText.fontStyle = FontStyles.Bold;
            feedbackText.alignment = TextAlignmentOptions.Center;
            feedbackText.color = new Color(0.98f, 0.85f, 0.35f, 1f); // Màu vàng hoàng gia Aurelia
            feedbackText.text = "";
            feedbackText.gameObject.SetActive(false);
        }

        private void ShowFeedback(string message)
        {
            if (feedbackText == null) return;

            feedbackText.text = message;
            feedbackText.gameObject.SetActive(true);

            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(HideFeedbackRoutine());
        }

        private IEnumerator HideFeedbackRoutine()
        {
            yield return new WaitForSeconds(2.0f);
            if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        }
    }
}
