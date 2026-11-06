using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// AudioManager - Quản lý NHẠC NỀN (BGM loop) và HIỆU ỨNG ÂM THANH (SFX) cho màn chơi Sinh.
///
/// Hiện scene chưa có bất kỳ AudioSource nào, nên manager này tự tạo chúng ở Awake.
///   - BGM: 1 AudioSource loop, chạy suốt màn chơi. Bật/tắt bằng nút "nhac" trong UI.
///   - SFX: pool vài AudioSource nhỏ, phát sự kiện ngắn (chạm chướng ngại, boss hiện, đúng/sai, game over).
///
/// Asset âm thanh: gán AudioClip trong Inspector (bỏ trống thì manager vẫn chạy, chỉ im lặng, KHÔNG crash).
///   Người chơi tự tải file thật (mp3/ogg) rồi gán vào các field bên dưới / trong Inspector.
///
/// Gắn script này lên 1 object (tạo object mới "AudioManager" trong scene) - không cần gán nút nào;
///   các nút UI "nhac"/"amthanh" chỉ cần gọi AudioManager.Instance.ToggleBGM() / ToggleSFX() nếu muốn.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Nhạc nền (BGM)")]
    [Tooltip("File nhạc nền (loop). Để trống = im lặng, không báo lỗi.")]
    public AudioClip bgmClip;
    public float bgmVolume = 0.5f;

    [Header("Hiệu ứng (SFX)")]
    public AudioClip sfxObstacle;   // chạm chướng ngại vật
    public AudioClip sfxBoss;       // boss xuất hiện / mở quiz
    public AudioClip sfxCorrect;    // trả lời đúng
    public AudioClip sfxWrong;       // trả lời sai
    public AudioClip sfxGameOver;    // hết máu
    [Range(0.1f, 1f)] public float sfxVolume = 0.8f;

    [Header("Cài đặt")]
    [Range(1, 8)] public int sfxPoolSize = 4; // số AudioSource SFX chạy song song

    private AudioSource bgmSource;
    private AudioSource[] sfxPool;
    private int sfxIndex = 0;

    private bool bgmEnabled = true;  // người chơi có muốn nghe nhạc không
    private bool sfxEnabled = true;  // người chơi có muốn nghe hiệu ứng không
    private bool warnedNoClip = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 1. Nhạc nền (loop)
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;
        if (bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.Play();
        }

        // 2. Pool SFX (phát ngắn, không loop)
        sfxPool = new AudioSource[sfxPoolSize];
        for (int i = 0; i < sfxPoolSize; i++)
        {
            sfxPool[i] = gameObject.AddComponent<AudioSource>();
            sfxPool[i].playOnAwake = false;
            sfxPool[i].volume = sfxVolume;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ============ BGM ============
    public void ToggleBGM()
    {
        bgmEnabled = !bgmEnabled;
        if (bgmSource == null) return;
        if (bgmEnabled && bgmSource.clip != null)
        {
            if (!bgmSource.isPlaying) bgmSource.Play();
        }
        else if (!bgmEnabled)
        {
            bgmSource.Pause();
        }
    }

    // ============ SFX (gọi theo sự kiện) ============
    public void PlayObstacleHit()  => PlaySfx(sfxObstacle);
    public void PlayBoss()         => PlaySfx(sfxBoss);
    public void PlayCorrect()      => PlaySfx(sfxCorrect);
    public void PlayWrong()        => PlaySfx(sfxWrong);
    public void PlayGameOver()     => PlaySfx(sfxGameOver);
    public void PlayGeneric()      => PlaySfx(sfxCorrect); // dùng chung cho "chọn đáp án / bấm nút"

    public void ToggleSFX()
    {
        sfxEnabled = !sfxEnabled;
    }

    private void PlaySfx(AudioClip clip)
    {
        if (!sfxEnabled) return;
        if (sfxPool == null || sfxPool.Length == 0) return;

        // Ưu tiên AudioClip gán vào; nếu chưa gán asset thật thì bỏ qua (không crash)
        if (clip == null)
        {
            if (!warnedNoClip)
            {
                warnedNoClip = true;
                Debug.Log("[AudioManager] Chưa có AudioClip - để im lặng. Hãy gán file âm thanh (BGM + SFX) trong Inspector của AudioManager_Sinh.");
            }
            return;
        }

        AudioSource src = sfxPool[sfxIndex];
        sfxIndex = (sfxIndex + 1) % sfxPool.Length;
        src.clip = clip;
        src.PlayOneShot(clip, sfxVolume);
    }
}
