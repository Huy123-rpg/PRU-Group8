using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScienceQuest.Core
{
    /// <summary>
    /// Hệ thống quản lý âm thanh (BGM & SFX) xuyên suốt các màn chơi
    /// Tự động khởi tạo và tồn tại qua DontDestroyOnLoad.
    /// Tự động tắt/tạm dừng nhạc khi vào các môn học khác (Vật Lý, Sinh Học).
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<AudioManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("[AudioManager]");
                        _instance = go.AddComponent<AudioManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }
        private static AudioManager _instance;

        [Header("Audio Sources")]
        private AudioSource bgmSource;
        private AudioSource sfxSource;
        private AudioSource ambientSource;

        [Header("Audio Clips")]
        public AudioClip bgmClip;
        public AudioClip correctClip;
        public AudioClip wrongClip;
        public AudioClip dogBarkClip;
        public AudioClip cheerClip;

        [Range(0f, 1f)] public float bgmVolume = 0.5f;
        [Range(0f, 1f)] public float sfxVolume = 0.9f;
        [Range(0f, 1f)] public float ambientVolume = 0.6f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("[AudioManager]");
                _instance = go.AddComponent<AudioManager>();
                DontDestroyOnLoad(go);
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            LoadAudioClips();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void InitializeAudioSources()
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = bgmVolume;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;

            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = false;
            ambientSource.playOnAwake = false;
            ambientSource.volume = ambientVolume;
        }

        private void LoadAudioClips()
        {
            if (bgmClip == null) bgmClip = Resources.Load<AudioClip>("Audio/BGM/bgm_main");
            if (correctClip == null) correctClip = Resources.Load<AudioClip>("Audio/SFX/sfx_correct");
            if (wrongClip == null) wrongClip = Resources.Load<AudioClip>("Audio/SFX/sfx_wrong");
            if (dogBarkClip == null) dogBarkClip = Resources.Load<AudioClip>("Audio/SFX/sfx_dog_bark");
            if (cheerClip == null) cheerClip = Resources.Load<AudioClip>("Audio/SFX/sfx_cheer");
        }

        private void Start()
        {
            CheckAndPlayBGMForScene(SceneManager.GetActiveScene().name);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CheckAndPlayBGMForScene(scene.name);
        }

        /// <summary>
        /// Phát nhạc nền cho các scene thuộc luồng chung & Hóa học (ngoại trừ Lý & Sinh)
        /// </summary>
        public void CheckAndPlayBGMForScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;

            string lower = sceneName.ToLower();

            // Loại trừ hoàn toàn các màn thuộc môn Vật Lý và Sinh Học
            bool isExcludedSubject = lower.Contains("physics") || 
                                     lower.Contains("sinh") || 
                                     lower.Contains("biology") ||
                                     lower.Contains("duckshooting") ||
                                     lower.Contains("versus");

            if (isExcludedSubject)
            {
                StopBGM();
                return;
            }

            // Danh sách các scene được hỗ trợ phát BGM chung
            // Bao gồm: Scene1_Login -> Scene7_Results, MainMenu, Academy, Chemistry, v.v.
            PlayBGM();
        }

        public void PlayBGM()
        {
            if (bgmClip == null)
            {
                LoadAudioClips();
            }

            if (bgmClip != null && bgmSource != null)
            {
                if (bgmSource.clip != bgmClip || !bgmSource.isPlaying)
                {
                    bgmSource.clip = bgmClip;
                    bgmSource.volume = bgmVolume;
                    bgmSource.Play();
                }
            }
        }

        public void StopBGM()
        {
            if (bgmSource != null && bgmSource.isPlaying)
            {
                bgmSource.Stop();
            }
        }

        public void PlayCorrectSFX()
        {
            PlaySFX(correctClip);
        }

        public void PlayWrongSFX()
        {
            PlaySFX(wrongClip);
        }

        public void PlayDogBarkSFX()
        {
            PlaySFX(dogBarkClip, 0.7f);
        }

        public void PlayCheerSFX()
        {
            if (cheerClip != null && ambientSource != null)
            {
                ambientSource.PlayOneShot(cheerClip, ambientVolume);
            }
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1.0f)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
            }
        }

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            if (bgmSource != null) bgmSource.volume = bgmVolume;
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null) sfxSource.volume = sfxVolume;
        }
    }
}

