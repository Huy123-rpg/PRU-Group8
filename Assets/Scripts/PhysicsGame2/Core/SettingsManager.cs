using System;
using UnityEngine;
using UnityEngine.UI;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Quản lý việc áp dụng cài đặt toàn cục (Độ sáng màn hình, Âm lượng nhạc và Âm lượng hiệu ứng).
    /// Duy trì Brightness Overlay bất tử (DontDestroyOnLoad) trên mọi Scene mà không gây duplicate.
    /// Hoạt động an toàn ngay cả khi dự án chưa có AudioMixer hay tệp âm thanh.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        private static SettingsManager _instance;
        public static SettingsManager Instance => _instance;

        private Canvas _brightnessCanvas;
        private Image _brightnessOverlay;
        private const float MAX_DARKNESS = 0.65f;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureBrightnessOverlay();
            ApplyAllSettingsFromSave();
        }

        private void Start()
        {
            ApplyAllSettingsFromSave();
        }

        public void ApplyAllSettingsFromSave()
        {
            var save = PhysicsSaveManager.Load();
            if (save == null || save.settings == null) return;

            ApplyBrightness(save.settings.brightness);
            ApplyMusicVolume(save.settings.musicVolume);
            ApplySFXVolume(save.settings.sfxVolume);
        }

        private void EnsureBrightnessOverlay()
        {
            if (_brightnessOverlay != null) return;

            // 1. Tạo Canvas riêng biệt cho lớp tối màn hình
            GameObject canvasGo = new GameObject("Persistent_BrightnessCanvas");
            canvasGo.transform.SetParent(transform, false);

            _brightnessCanvas = canvasGo.AddComponent<Canvas>();
            _brightnessCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _brightnessCanvas.sortingOrder = 999; // Hiển thị trên nền và gameplay, dưới modal đặc biệt nếu cần

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Không cần GraphicRaycaster trên canvas này vì overlay phải Ignore Raycast hoàn toàn

            // 2. Tạo Image Overlay màu đen
            GameObject overlayGo = new GameObject("DarknessOverlay");
            overlayGo.transform.SetParent(canvasGo.transform, false);

            RectTransform rt = overlayGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            _brightnessOverlay = overlayGo.AddComponent<Image>();
            _brightnessOverlay.color = new Color(0f, 0f, 0f, 0f);
            _brightnessOverlay.raycastTarget = false; // Bắt buộc: KHÔNG CHẶN CLICK
        }

        /// <summary>
        /// Áp dụng độ sáng màn hình (từ 0.2 đến 1.0).
        /// Giá trị 1.0 tương ứng với độ sáng tối đa (alpha = 0).
        /// </summary>
        public void ApplyBrightness(float brightnessValue)
        {
            EnsureBrightnessOverlay();
            if (_brightnessOverlay == null) return;

            // Nếu giá trị chưa thiết lập (<= 0), mặc định là 1.0f (độ sáng tối đa theo thiết kế gốc)
            if (brightnessValue <= 0.001f) brightnessValue = 1.0f;

            float clamped = Mathf.Clamp(brightnessValue, 0.2f, 1.0f);
            float targetAlpha = (1.0f - clamped) * MAX_DARKNESS;

            _brightnessOverlay.color = new Color(0f, 0f, 0f, targetAlpha);
            _brightnessOverlay.gameObject.SetActive(targetAlpha > 0.001f);
        }

        /// <summary>
        /// Áp dụng âm lượng nhạc nền (Music Volume: 0.0 - 1.0).
        /// Sẵn sàng kết nối khi AudioSource/AudioMixer được bổ sung.
        /// </summary>
        public void ApplyMusicVolume(float volume)
        {
            float clamped = Mathf.Clamp01(volume);
            // Hiện tại project chưa có audio files, lưu trữ giá trị an toàn
            // Khi có AudioSource background music, sẽ set: audioSource.volume = clamped;
        }

        /// <summary>
        /// Áp dụng âm lượng hiệu ứng âm thanh (SFX Volume: 0.0 - 1.0).
        /// Sẵn sàng kết nối khi AudioSource/SoundEffectManager được bổ sung.
        /// </summary>
        public void ApplySFXVolume(float volume)
        {
            float clamped = Mathf.Clamp01(volume);
            // Hiện tại project chưa có audio files, lưu trữ giá trị an toàn
        }
    }
}
