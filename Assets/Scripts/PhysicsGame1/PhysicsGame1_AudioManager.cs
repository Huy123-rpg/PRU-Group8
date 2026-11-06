using System;
using UnityEngine;

namespace ScienceQuest.PhysicsGame1
{
    /// <summary>
    /// Quản lý toàn bộ hệ thống âm thanh cho Physics Game 1 (Bắn Vịt):
    /// 1. Tự động kiểm tra nạp file âm thanh thực từ Resources/Audio (nếu có).
    /// 2. Tự động tạo sóng âm thủ công (Procedural Audio Synthesizer) bằng mã C# nếu chưa có file asset,
    ///    đáp ứng 100% âm thanh chân thực (tiếng súng, quạu quạu vịt kêu, nổ bom, đóng băng, đúng/sai, BGM).
    /// 3. Kết nối trực tiếp với cài đặt BGM/SFX và DontDestroyOnLoad để đồng bộ xuyên suốt các màn.
    /// </summary>
    public class PhysicsGame1_AudioManager : MonoBehaviour
    {
        public static PhysicsGame1_AudioManager Instance { get; private set; }

        private AudioSource _bgmSource;
        private AudioSource _sfxSource;

        // Bộ đệm AudioClip
        private AudioClip _clipGunshot;
        private AudioClip _clipDuckQuack;
        private AudioClip _clipDuckHit;
        private AudioClip _clipExplosion;
        private AudioClip _clipFreeze;
        private AudioClip _clipCorrect;
        private AudioClip _clipWrong;
        private AudioClip _clipClick;
        private AudioClip _clipCountdownBeep;
        private AudioClip _clipCountdownFinal;
        private AudioClip _clipBGM;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance == null)
            {
                GameObject go = new GameObject("[PhysicsGame1_AudioManager]");
                Instance = go.AddComponent<PhysicsGame1_AudioManager>();
                DontDestroyOnLoad(go);
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
                GenerateOrLoadAudioClips();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAudioSources()
        {
            // BGM Source
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
            _bgmSource.volume = 0.45f;

            // SFX Source
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
            _sfxSource.volume = 0.85f;
        }

        private void Start()
        {
            ApplySettings();
        }

        public void ApplySettings()
        {
            bool bgmOn = PhysicsGame1Session.Instance != null ? PhysicsGame1Session.Instance.BGMEnabled : (PlayerPrefs.GetInt("PhysicsGame1_BGM", 1) == 1);
            bool sfxOn = PhysicsGame1Session.Instance != null ? PhysicsGame1Session.Instance.SFXEnabled : (PlayerPrefs.GetInt("PhysicsGame1_SFX", 1) == 1);

            if (_bgmSource != null)
            {
                _bgmSource.mute = !bgmOn;
                if (bgmOn && !_bgmSource.isPlaying && _clipBGM != null)
                {
                    _bgmSource.clip = _clipBGM;
                    _bgmSource.Play();
                }
                else if (!bgmOn && _bgmSource.isPlaying)
                {
                    _bgmSource.Stop();
                }
            }

            if (_sfxSource != null)
            {
                _sfxSource.mute = !sfxOn;
            }
        }

        #region ===== NẠP HOẶC TỔNG HỢP ÂM THANH (AUDIO SYNTHESIS) =====

        private void GenerateOrLoadAudioClips()
        {
            // 1. Tiếng súng nổ (Gunshot)
            _clipGunshot = Resources.Load<AudioClip>("Audio/gunshot") 
                        ?? Resources.Load<AudioClip>("Audio/Gunshot") 
                        ?? SynthesizeGunshot();

            // 2. Tiếng vịt kêu / trúng đạn (Quack / Hit)
            _clipDuckQuack = Resources.Load<AudioClip>("Audio/quack") 
                          ?? Resources.Load<AudioClip>("Audio/DuckQuack") 
                          ?? SynthesizeDuckQuack();

            _clipDuckHit = Resources.Load<AudioClip>("Audio/duck_hit") 
                        ?? SynthesizeDuckHit();

            // 3. Tiếng bom nổ (Explosion)
            _clipExplosion = Resources.Load<AudioClip>("Audio/explosion") 
                          ?? Resources.Load<AudioClip>("Audio/Bomb") 
                          ?? SynthesizeExplosion();

            // 4. Tiếng đóng băng (Freeze / Ice chime)
            _clipFreeze = Resources.Load<AudioClip>("Audio/freeze") 
                       ?? Resources.Load<AudioClip>("Audio/Ice") 
                       ?? SynthesizeFreeze();

            // 5. Chuông trả lời đúng (Correct Ding)
            _clipCorrect = Resources.Load<AudioClip>("Audio/correct") 
                        ?? Resources.Load<AudioClip>("Audio/Ding") 
                        ?? SynthesizeCorrectDing();

            // 6. Còi trả lời sai (Wrong Buzzer)
            _clipWrong = Resources.Load<AudioClip>("Audio/wrong") 
                      ?? Resources.Load<AudioClip>("Audio/Buzzer") 
                      ?? SynthesizeWrongBuzzer();

            // 7. Tiếng bấm nút (Click)
            _clipClick = Resources.Load<AudioClip>("Audio/click") 
                      ?? SynthesizeClick();

            // 8. Đếm ngược (Beep)
            _clipCountdownBeep = SynthesizeTone(750f, 0.12f);
            _clipCountdownFinal = SynthesizeTone(1200f, 0.35f);

            // 9. Nhạc nền (BGM Ambient Loop)
            _clipBGM = Resources.Load<AudioClip>("Audio/BGM/bgm_main")
                    ?? Resources.Load<AudioClip>("Audio/bgm") 
                    ?? Resources.Load<AudioClip>("Audio/BGM") 
                    ?? SynthesizeCalmBGM();
        }

        private AudioClip SynthesizeGunshot()
        {
            int sampleRate = 44100;
            float duration = 0.28f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            System.Random rand = new System.Random(42);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 22f); // Suy giảm cực nhanh
                float noise = ((float)rand.NextDouble() * 2f - 1f);
                float crack = Mathf.Sin(2f * Mathf.PI * (280f - t * 600f) * t); // Tiếng nổ đanh
                samples[i] = Mathf.Clamp((noise * 0.75f + crack * 0.55f) * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Synth_Gunshot", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip SynthesizeDuckQuack()
        {
            int sampleRate = 44100;
            float duration = 0.32f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                // Tiếng quạc quạc hai nhịp vui nhộn
                float env = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
                float freq = 380f + Mathf.Sin(t * 35f) * 60f;
                float form1 = Mathf.Sin(2f * Mathf.PI * freq * t);
                float form2 = Mathf.Sin(2f * Mathf.PI * (freq * 2.1f) * t) * 0.5f;
                float form3 = Mathf.Sin(2f * Mathf.PI * (freq * 3.2f) * t) * 0.25f;
                samples[i] = Mathf.Clamp((form1 + form2 + form3) * env * 0.65f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Synth_DuckQuack", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip SynthesizeDuckHit()
        {
            int sampleRate = 44100;
            float duration = 0.22f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            System.Random rand = new System.Random(1337);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 18f);
                float pop = Mathf.Sin(2f * Mathf.PI * (450f - t * 900f) * t);
                float noise = ((float)rand.NextDouble() * 2f - 1f) * 0.3f;
                samples[i] = Mathf.Clamp((pop * 0.7f + noise) * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Synth_DuckHit", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip SynthesizeExplosion()
        {
            int sampleRate = 44100;
            float duration = 0.65f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            System.Random rand = new System.Random(999);

            float lastNoise = 0f;
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 6.5f);
                float rawNoise = (float)rand.NextDouble() * 2f - 1f;
                // Low-pass filter tạo tiếng ầm trầm vang
                lastNoise = Mathf.Lerp(lastNoise, rawNoise, 0.12f);
                float subBoom = Mathf.Sin(2f * Mathf.PI * (75f - t * 65f) * t);
                samples[i] = Mathf.Clamp((lastNoise * 1.2f + subBoom * 0.6f) * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Synth_Explosion", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip SynthesizeFreeze()
        {
            int sampleRate = 44100;
            float duration = 0.55f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 5.5f) * Mathf.Sin(Mathf.Clamp01(t * 8f) * Mathf.PI * 0.5f);
                // Tiếng chuông pha lê lấp lánh băng tuyết
                float c1 = Mathf.Sin(2f * Mathf.PI * 1760f * t);
                float c2 = Mathf.Sin(2f * Mathf.PI * 2640f * t) * 0.6f;
                float c3 = Mathf.Sin(2f * Mathf.PI * 3520f * t) * 0.35f;
                samples[i] = Mathf.Clamp((c1 + c2 + c3) * env * 0.5f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Synth_Freeze", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip SynthesizeCorrectDing()
        {
            int sampleRate = 44100;
            float duration = 0.45f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            // Hợp âm Do - Mi - Sol (C5 - E5 - G5) tươi sáng
            float f1 = 523.25f; // C5
            float f2 = 659.25f; // E5
            float f3 = 783.99f; // G5

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 6f);
                float s1 = Mathf.Sin(2f * Mathf.PI * f1 * t);
                float s2 = (t > 0.08f) ? Mathf.Sin(2f * Mathf.PI * f2 * t) : 0f;
                float s3 = (t > 0.16f) ? Mathf.Sin(2f * Mathf.PI * f3 * t) * 1.2f : 0f;
                samples[i] = Mathf.Clamp((s1 * 0.4f + s2 * 0.45f + s3 * 0.55f) * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Synth_CorrectDing", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip SynthesizeWrongBuzzer()
        {
            int sampleRate = 44100;
            float duration = 0.38f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 5f);
                // Dissonant saw wave
                float b1 = Mathf.Sin(2f * Mathf.PI * 140f * t) > 0f ? 1f : -1f;
                float b2 = Mathf.Sin(2f * Mathf.PI * 132f * t) > 0f ? 1f : -1f;
                samples[i] = Mathf.Clamp((b1 + b2) * 0.35f * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Synth_WrongBuzzer", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip SynthesizeClick()
        {
            int sampleRate = 44100;
            float duration = 0.05f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 70f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * 1100f * t) * env * 0.6f;
            }

            AudioClip clip = AudioClip.Create("Synth_Click", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip SynthesizeTone(float frequency, float duration)
        {
            int sampleRate = 44100;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * env * 0.5f;
            }

            AudioClip clip = AudioClip.Create($"Synth_Tone_{frequency}", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip SynthesizeCalmBGM()
        {
            int sampleRate = 22050; // Giảm mẫu để nhẹ ram
            float duration = 6.0f; // Vòng lặp 6 giây êm dịu
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            // Giai điệu nhẹ nhàng Ngũ cung (Pentatonic)
            float[] notes = new float[] { 261.63f, 293.66f, 329.63f, 392.00f, 440.00f, 523.25f };

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float noteIdx = (t * 2.5f) % notes.Length;
                float baseFreq = notes[(int)noteIdx];

                float wave = Mathf.Sin(2f * Mathf.PI * baseFreq * t) * 0.12f;
                wave += Mathf.Sin(2f * Mathf.PI * (baseFreq * 0.5f) * t) * 0.15f; // Bass ấm
                samples[i] = Mathf.Clamp(wave, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Synth_CalmBGM", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        #endregion

        #region ===== CÁC PHƯƠNG THỨC GỌI PHÁT ÂM THANH (PUBLIC API) =====

        public void PlayGunshot()
        {
            PlaySFX(_clipGunshot, 0.9f);
        }

        public void PlayDuckQuack()
        {
            PlaySFX(_clipDuckQuack, 0.75f);
        }

        public void PlayDuckHit()
        {
            PlaySFX(_clipDuckHit, 0.85f);
        }

        public void PlayExplosion()
        {
            PlaySFX(_clipExplosion, 1.0f);
        }

        public void PlayFreeze()
        {
            PlaySFX(_clipFreeze, 0.85f);
        }

        public void PlayCorrect()
        {
            PlaySFX(_clipCorrect, 0.95f);
        }

        public void PlayWrong()
        {
            PlaySFX(_clipWrong, 0.85f);
        }

        public void PlayButtonClick()
        {
            PlaySFX(_clipClick, 0.5f);
        }

        public void PlayVictory()
        {
            PlayCorrect();
        }

        public void PlayGameOver()
        {
            PlayWrong();
        }

        public void PlayCountdownBeep(bool isFinal = false)
        {
            PlaySFX(isFinal ? _clipCountdownFinal : _clipCountdownBeep, 0.9f);
        }

        public void PlayBGM()
        {
            if (_bgmSource != null && _clipBGM != null)
            {
                _bgmSource.clip = _clipBGM;
                bool bgmOn = PhysicsGame1Session.Instance != null ? PhysicsGame1Session.Instance.BGMEnabled : true;
                _bgmSource.mute = !bgmOn;
                if (bgmOn && !_bgmSource.isPlaying)
                {
                    _bgmSource.Play();
                }
            }
        }

        public void StopBGM()
        {
            if (_bgmSource != null) _bgmSource.Stop();
        }

        private void PlaySFX(AudioClip clip, float volumeScale = 1.0f)
        {
            if (clip == null || _sfxSource == null) return;
            bool sfxOn = PhysicsGame1Session.Instance != null ? PhysicsGame1Session.Instance.SFXEnabled : true;
            if (!sfxOn) return;

            _sfxSource.PlayOneShot(clip, volumeScale);
        }

        #endregion
    }
}
