using UnityEngine;

public sealed class DogRunAnimator : MonoBehaviour
{
    public Animator animator;
    public Transform visualRoot;
    public Transform[] legs;
    public SpriteRenderer[] coloredParts;
    public GameObject boostRoot;
    public SpriteRenderer boostStreak;
    public Transform boostBadge;

    [Range(0f, 0.3f)] public float boostTint = 0.12f;
    public bool IsRunning { get; private set; }
    public bool IsBoosted { get; private set; }

    private static readonly int Running = Animator.StringToHash("Running");
    private static readonly int RunSpeed = Animator.StringToHash("RunSpeed");
    private Color[] baseColors;
    private Vector3 streakScale;
    private Vector3 badgePosition;
    private float fxPhase;

    private void Awake() { CacheRestPose(); }

    private void CacheRestPose()
    {
        if (baseColors != null) return;
        baseColors = new Color[coloredParts != null ? coloredParts.Length : 0];
        for (int i = 0; i < baseColors.Length; i++)
            if (coloredParts[i] != null) baseColors[i] = coloredParts[i].color;
        if (boostStreak != null) streakScale = boostStreak.transform.localScale;
        if (boostBadge != null) badgePosition = boostBadge.localPosition;
    }

    public void TickRace(bool running, bool boosted, float speedMultiplier, float deltaTime)
    {
        CacheRestPose();
        IsRunning = running;
        IsBoosted = boosted;
        if (animator != null)
        {
            animator.SetBool(Running, running);
            animator.SetFloat(RunSpeed, Mathf.Clamp(speedMultiplier, 0.6f, 2.5f));
            // Freeze the current stride while an essay is open, including body bob.
            animator.speed = running ? 1f : 0f;
        }
        if (boostRoot != null && boostRoot.activeSelf != boosted) boostRoot.SetActive(boosted);
        if (!boosted) { RestoreColors(); return; }
        if (running) fxPhase += Mathf.Max(0f, deltaTime);

        float pulse = Mathf.Sin(fxPhase * 18f);
        if (boostStreak != null)
        {
            boostStreak.transform.localScale = new Vector3(streakScale.x * (1f + 0.14f * pulse), streakScale.y, streakScale.z);
            boostStreak.color = new Color(1f, 1f, 1f, 0.85f + 0.1f * pulse);
        }
        if (boostBadge != null)
        {
            boostBadge.localPosition = badgePosition + Vector3.up * (0.04f * Mathf.Sin(fxPhase * 6f));
            boostBadge.localRotation = Quaternion.Euler(0f, 0f, 8f * Mathf.Sin(fxPhase * 8f));
        }
        for (int i = 0; i < baseColors.Length; i++)
            if (coloredParts[i] != null)
                coloredParts[i].color = Color.Lerp(baseColors[i], new Color(1f, 0.92f, 0.6f, baseColors[i].a), boostTint);
    }

    public void StopAndReset()
    {
        CacheRestPose();
        IsRunning = false;
        IsBoosted = false;
        fxPhase = 0f;
        if (boostRoot != null) boostRoot.SetActive(false);
        if (animator != null && animator.isActiveAndEnabled)
        {
            animator.speed = 1f;
            animator.SetBool(Running, false);
            animator.Play("Idle", 0, 0f);
            animator.Update(0f);
            animator.speed = 0f;
        }
        if (visualRoot != null) { visualRoot.localPosition = Vector3.zero; visualRoot.localRotation = Quaternion.identity; }
        if (legs != null)
            foreach (Transform leg in legs) if (leg != null) leg.localRotation = Quaternion.identity;
        RestoreColors();
    }

    private void RestoreColors()
    {
        if (baseColors == null) return;
        for (int i = 0; i < baseColors.Length; i++)
            if (coloredParts[i] != null) coloredParts[i].color = baseColors[i];
    }
}
