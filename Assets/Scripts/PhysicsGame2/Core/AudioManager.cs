using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Facade AudioManager xuyên scene theo đặc tả mục 6.1 (01_Luong_34_Man_Aurelia.md).
    /// Ủy thác trực tiếp tới SettingsManager singleton để đảm bảo chỉ có duy nhất một nguồn phát âm thanh,
    /// không tạo thêm nguồn nhạc mỗi lần quay lại menu, SFX và BGM điều khiển độc lập.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<AudioManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AudioManager");
                        _instance = go.AddComponent<AudioManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        private static AudioManager _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool IsMusicEnabled => SettingsManager.Instance != null && SettingsManager.Instance.IsMusicEnabled;
        public bool IsSfxEnabled => SettingsManager.Instance != null && SettingsManager.Instance.IsSfxEnabled;

        public void PlayBGM(AudioClip clip = null)
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.PlayBGM(clip);
            }
        }

        public void PlaySFX(AudioClip clip = null)
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.PlaySFX(clip);
            }
        }

        public void SetMusicEnabled(bool enabled)
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.ApplyMusicEnabled(enabled);
            }
        }

        public void SetSfxEnabled(bool enabled)
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.ApplySfxEnabled(enabled);
            }
        }
    }
}
