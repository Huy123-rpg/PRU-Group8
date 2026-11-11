using System;
using UnityEngine;
using UnityEngine.UI;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Quản lý cài đặt toàn cục: Nhạc nền (BGM), Hiệu ứng âm thanh (SFX) và Độ sáng màn hình.
    /// Singleton DontDestroyOnLoad duy nhất xuyên suốt toàn bộ vòng đời game.
    /// Điều khiển âm thanh thực tế qua AudioSource riêng biệt cho BGM và SFX.
    /// Đảm bảo BGM ON/OFF và SFX ON/OFF hoạt động độc lập, tuyệt đối không tạm dừng toàn cục.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        private static SettingsManager _instance;
        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<SettingsManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SettingsManager");
                        _instance = go.AddComponent<SettingsManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("=== AUDIO SOURCES ===")]
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource _sfxSource;

        [Header("=== AUDIO CLIPS ===")]
        [SerializeField] private AudioClip _bgmClip;
        [SerializeField] private AudioClip _clickSfxClip;

        [Header("=== VOLUME CONFIG (20-30% STANDARD) ===")]
        [SerializeField, Range(0f, 1f)] private float _bgmVolume = 0.25f; // Mặc định 25%, êm dịu, không chói tai
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 0.70f;

        [Header("=== BRIGHTNESS OVERLAY ===")]
        private Canvas _brightnessCanvas;
        private Image _brightnessOverlay;
        private const float MAX_DARKNESS = 0.65f;

        public bool IsMusicEnabled { get; private set; } = true;
        public bool IsSfxEnabled { get; private set; } = true;
        public float Brightness { get; private set; } = 1.0f;
        public float BgmVolume => _bgmVolume;
        public string CurrentBgmName => _bgmSource != null && _bgmSource.clip != null ? _bgmSource.clip.name : "(Không phát - Chờ file MP3)";

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureAudioSources();
            EnsureBrightnessOverlay();
            LoadAndApplyAllSettings();
        }

        private void Start()
        {
            LoadAndApplyAllSettings();
        }

        public void EnsureAudioSources()
        {
            if (_bgmSource == null)
            {
                _bgmSource = gameObject.AddComponent<AudioSource>();
                _bgmSource.playOnAwake = false;
                _bgmSource.loop = true;
                _bgmSource.priority = 10;
                _bgmSource.volume = _bgmVolume;
            }

            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
                _sfxSource.playOnAwake = false;
                _sfxSource.loop = false;
                _sfxSource.priority = 20;
                _sfxSource.volume = _sfxVolume;
            }

            if (_clickSfxClip == null)
            {
                _clickSfxClip = CreateProceduralClickClip();
            }

            // Tuyệt đối không tạo hay phát nhạc synth/procedural BGM cũ
            TryLoadBgmClip();
        }

        /// <summary>
        /// Nạp file nhạc nền MP3 được cấp phép hợp lệ từ thư mục Assets/Audio/PhysicsGame2/BGM/ hoặc Assets/Audio/BGM/.
        /// Nếu chưa có file MP3, giữ im lặng chứ tuyệt đối không sinh nhạc giả / synth chói tai.
        /// </summary>
        public void TryLoadBgmClip()
        {
            if (_bgmClip != null)
            {
                if (_bgmSource != null && _bgmSource.clip == null)
                {
                    _bgmSource.clip = _bgmClip;
                    _bgmSource.loop = true;
                    _bgmSource.volume = _bgmVolume;
                }
                return;
            }

            // 1. Tìm trong Resources (chuẩn 100% cho mọi build: Android, Standalone PC, Editor)
            AudioClip resClip = null;
            AudioClip[] allBgmInResources = Resources.LoadAll<AudioClip>("Audio/BGM");
            if (allBgmInResources != null && allBgmInResources.Length > 0)
            {
                resClip = allBgmInResources[0];
            }

            if (resClip == null)
            {
                resClip = Resources.Load<AudioClip>("Audio/BGM/Ocean_Morning")
                    ?? Resources.Load<AudioClip>("Audio/BGM/Ocean Morning")
                    ?? Resources.Load<AudioClip>("BGM/Ocean_Morning")
                    ?? Resources.Load<AudioClip>("Ocean_Morning")
                    ?? Resources.Load<AudioClip>("Ocean Morning");
            }

            if (resClip != null)
            {
                _bgmClip = resClip;
                if (_bgmSource != null)
                {
                    _bgmSource.clip = resClip;
                    _bgmSource.loop = true;
                    _bgmSource.volume = _bgmVolume;
                    if (IsMusicEnabled && !_bgmSource.isPlaying)
                    {
                        _bgmSource.Play();
                    }
                }
                Debug.Log($"[SettingsManager] Đã nạp thành công BGM AudioClip từ Resources: '{resClip.name}' (Âm lượng: {_bgmVolume:P0})");
                return;
            }

#if UNITY_EDITOR
            // 2. Trong Unity Editor: Quét trực tiếp các thư mục BGM của dự án
            string[] searchDirs = new string[] {
                "Assets/Audio/PhysicsGame2/BGM",
                "Assets/Audio/BGM",
                "Assets/Audio"
            };
            foreach (var dir in searchDirs)
            {
                if (System.IO.Directory.Exists(dir))
                {
                    var files = System.IO.Directory.GetFiles(dir, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                    foreach (var f in files)
                    {
                        string ext = System.IO.Path.GetExtension(f).ToLowerInvariant();
                        if (ext == ".mp3" || ext == ".wav" || ext == ".ogg")
                        {
                            string assetPath = f.Replace('\\', '/');
                            AudioClip loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                            if (loaded != null)
                            {
                                _bgmClip = loaded;
                                if (_bgmSource != null)
                                {
                                    _bgmSource.clip = loaded;
                                    _bgmSource.loop = true;
                                    _bgmSource.volume = _bgmVolume;
                                }
                                Debug.Log($"[SettingsManager] Đã nạp thành công BGM từ Asset: {assetPath}");
                                return;
                            }
                        }
                    }
                }
            }
#endif

            // 3. Quét runtime trực tiếp trên đĩa thông qua UnityWebRequestMultimedia (nếu người dùng vừa chép file vào)
            StartCoroutine(LoadBgmFileFromDiskRoutine());
        }

        private System.Collections.IEnumerator LoadBgmFileFromDiskRoutine()
        {
            string[] searchDirs = new string[] {
                System.IO.Path.Combine(Application.dataPath, "Audio/PhysicsGame2/BGM"),
                System.IO.Path.Combine(Application.dataPath, "Resources/Audio/BGM"),
                System.IO.Path.Combine(Application.dataPath, "Audio/BGM"),
                System.IO.Path.Combine(Application.dataPath, "Audio")
            };

            string targetFile = null;
            AudioType targetType = AudioType.MPEG;

            foreach (var dir in searchDirs)
            {
                if (System.IO.Directory.Exists(dir))
                {
                    var files = System.IO.Directory.GetFiles(dir, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                    foreach (var f in files)
                    {
                        string ext = System.IO.Path.GetExtension(f).ToLowerInvariant();
                        if (ext == ".mp3")
                        {
                            targetFile = f;
                            targetType = AudioType.MPEG;
                            break;
                        }
                        else if (ext == ".ogg")
                        {
                            targetFile = f;
                            targetType = AudioType.OGGVORBIS;
                            break;
                        }
                        else if (ext == ".wav")
                        {
                            targetFile = f;
                            targetType = AudioType.WAV;
                            break;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(targetFile)) break;
            }

            if (string.IsNullOrEmpty(targetFile))
            {
                Debug.Log("[SettingsManager] Đã loại bỏ hoàn toàn nhạc synth cũ. Chưa phát hiện file MP3 tại Assets/Audio/PhysicsGame2/BGM/. Đang chờ file nhạc MP3.");
                yield break;
            }

            string uri = "file://" + targetFile.Replace('\\', '/');
            using (UnityEngine.Networking.UnityWebRequest uwr = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(uri, targetType))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(uwr);
                    if (clip != null)
                    {
                        clip.name = System.IO.Path.GetFileNameWithoutExtension(targetFile);
                        _bgmClip = clip;
                        if (_bgmSource != null)
                        {
                            _bgmSource.clip = clip;
                            _bgmSource.loop = true;
                            _bgmSource.volume = _bgmVolume;
                            if (IsMusicEnabled && !_bgmSource.isPlaying)
                            {
                                _bgmSource.Play();
                            }
                        }
                        Debug.Log($"[SettingsManager] Đã nạp thành công BGM MP3 từ đĩa: {targetFile} (Âm lượng: {_bgmVolume:P0})");
                    }
                }
                else
                {
                    Debug.LogWarning($"[SettingsManager] Không thể nạp audio file '{targetFile}': {uwr.error}");
                }
            }
        }

        private AudioClip CreateProceduralClickClip()
        {
            int sampleRate = 44100;
            float duration = 0.08f;
            int totalSamples = (int)(sampleRate * duration);
            float[] data = new float[totalSamples];
            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * 40f);
                float freq1 = Mathf.Sin(2f * Mathf.PI * 880f * t);
                float freq2 = Mathf.Sin(2f * Mathf.PI * 1320f * t) * 0.5f;
                data[i] = (freq1 + freq2) * 0.35f * envelope;
            }
            AudioClip clip = AudioClip.Create("Default_Click_SFX", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// Bật/tắt nhạc nền (BGM). Không ảnh hưởng tới SFX.
        /// </summary>
        public void ApplyMusicEnabled(bool enabled)
        {
            IsMusicEnabled = enabled;
            EnsureAudioSources();

            if (_bgmSource != null)
            {
                _bgmSource.mute = !enabled;
                if (enabled)
                {
                    _bgmSource.volume = _bgmVolume;
                    if (_bgmSource.clip == null)
                    {
                        TryLoadBgmClip();
                    }
                    if (!_bgmSource.isPlaying && _bgmSource.clip != null)
                    {
                        _bgmSource.Play();
                    }
                }
                else
                {
                    _bgmSource.Pause();
                }
            }
            Debug.Log($"[SettingsManager] ApplyMusicEnabled: {enabled} (Volume: {_bgmVolume:P0})");
        }

        /// <summary>
        /// Bật/tắt hiệu ứng âm thanh (SFX). Không ảnh hưởng tới BGM.
        /// </summary>
        public void ApplySfxEnabled(bool enabled)
        {
            IsSfxEnabled = enabled;
            EnsureAudioSources();

            if (_sfxSource != null)
            {
                _sfxSource.mute = !enabled;
            }
            Debug.Log($"[SettingsManager] ApplySfxEnabled: {enabled}");
        }

        /// <summary>
        /// Phát tiếng click hoặc SFX khi tương tác nút bấm.
        /// </summary>
        public void PlaySFX(AudioClip clip = null)
        {
            if (!IsSfxEnabled) return;
            EnsureAudioSources();

            AudioClip clipToPlay = clip ?? _clickSfxClip;
            if (clipToPlay != null && _sfxSource != null)
            {
                _sfxSource.PlayOneShot(clipToPlay, _sfxVolume);
            }
        }

        /// <summary>
        /// Phát nhạc nền xuyên scene.
        /// </summary>
        public void PlayBGM(AudioClip clip = null)
        {
            EnsureAudioSources();
            if (clip != null)
            {
                _bgmClip = clip;
                _bgmSource.clip = clip;
                _bgmSource.loop = true;
                _bgmSource.volume = _bgmVolume;
            }
            else if (_bgmSource.clip == null)
            {
                TryLoadBgmClip();
            }

            if (IsMusicEnabled && _bgmSource != null && _bgmSource.clip != null)
            {
                _bgmSource.mute = false;
                _bgmSource.volume = _bgmVolume;
                if (!_bgmSource.isPlaying)
                {
                    _bgmSource.Play();
                }
            }
        }

        /// <summary>
        /// Lưu trạng thái cài đặt vào PlayerPrefs và PhysicsSaveData.
        /// </summary>
        public void SaveSettings(bool musicEnabled, bool sfxEnabled)
        {
            IsMusicEnabled = musicEnabled;
            IsSfxEnabled = sfxEnabled;

            PlayerPrefs.SetInt("MusicEnabled", musicEnabled ? 1 : 0);
            PlayerPrefs.SetInt("SfxEnabled", sfxEnabled ? 1 : 0);
            PlayerPrefs.Save();

            // Đồng bộ vào PhysicsSaveData
            var save = PhysicsSaveManager.Load();
            if (save != null)
            {
                if (save.settings == null) save.settings = new GameSettingsData();
                save.settings.musicVolume = musicEnabled ? _bgmVolume : 0.0f;
                save.settings.sfxVolume = sfxEnabled ? _sfxVolume : 0.0f;
                PhysicsSaveManager.Save(save);
                PhysicsGameSession.Instance?.ReloadSaveData();
            }

            ApplyMusicEnabled(musicEnabled);
            ApplySfxEnabled(sfxEnabled);

            Debug.Log($"[SettingsManager] Đã lưu: MusicEnabled={musicEnabled}, SfxEnabled={sfxEnabled} (BGM Volume: {_bgmVolume:P0})");
        }

        /// <summary>
        /// Khôi phục trạng thái đã lưu trước đó nếu người chơi hủy/quay lại mà chưa lưu.
        /// </summary>
        public void RestorePreviousSettings()
        {
            LoadAndApplyAllSettings();
        }

        public void LoadAndApplyAllSettings()
        {
            IsMusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
            IsSfxEnabled = PlayerPrefs.GetInt("SfxEnabled", 1) == 1;

            var save = PhysicsSaveManager.Load();
            if (save != null && save.settings != null)
            {
                ApplyBrightness(save.settings.brightness);
            }
            else
            {
                ApplyBrightness(1.0f);
            }

            ApplyMusicEnabled(IsMusicEnabled);
            ApplySfxEnabled(IsSfxEnabled);
        }

        public void ApplyAllSettingsFromSave()
        {
            LoadAndApplyAllSettings();
        }

        public void ApplyMusicVolume(float volume)
        {
            float clamped = Mathf.Clamp01(volume);
            ApplyMusicEnabled(clamped > 0.05f);
        }

        public void ApplySFXVolume(float volume)
        {
            float clamped = Mathf.Clamp01(volume);
            ApplySfxEnabled(clamped > 0.05f);
        }

        private void EnsureBrightnessOverlay()
        {
            if (_brightnessOverlay != null) return;

            GameObject canvasGo = new GameObject("Persistent_BrightnessCanvas");
            canvasGo.transform.SetParent(transform, false);

            _brightnessCanvas = canvasGo.AddComponent<Canvas>();
            _brightnessCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _brightnessCanvas.sortingOrder = 999;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject overlayGo = new GameObject("DarknessOverlay");
            overlayGo.transform.SetParent(canvasGo.transform, false);

            RectTransform rt = overlayGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            _brightnessOverlay = overlayGo.AddComponent<Image>();
            _brightnessOverlay.color = new Color(0f, 0f, 0f, 0f);
            _brightnessOverlay.raycastTarget = false;
        }

        public void ApplyBrightness(float brightnessValue)
        {
            EnsureBrightnessOverlay();
            if (_brightnessOverlay == null) return;

            if (brightnessValue <= 0.001f) brightnessValue = 1.0f;
            Brightness = Mathf.Clamp(brightnessValue, 0.2f, 1.0f);
            float targetAlpha = (1.0f - Brightness) * MAX_DARKNESS;

            _brightnessOverlay.color = new Color(0f, 0f, 0f, targetAlpha);
            _brightnessOverlay.gameObject.SetActive(targetAlpha > 0.001f);
        }
    }
}
