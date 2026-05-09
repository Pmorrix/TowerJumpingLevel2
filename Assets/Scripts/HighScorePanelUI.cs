using System.Collections;
using UnityEngine;

public class HighScorePanelUI : MonoBehaviour
{
    [Header("Overlay")]
    [SerializeField] private CanvasGroup overlayCanvasGroup;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float startAlpha = 0f;
    [SerializeField] private float targetAlpha = 1f;

    [Header("GO Text")]
    [SerializeField] private RectTransform goText;
    [SerializeField] private float goPopDuration = 0.22f;
    [SerializeField] private float goOvershoot = 1.08f;

    [Header("Top Block")]
    [SerializeField] private RectTransform topBlock;
    [SerializeField] private float topBlockPopDuration = 0.20f;
    [SerializeField] private float topBlockOvershoot = 1.08f;
    [SerializeField] private float delayAfterGoText = 0.08f;

    [Header("HS Text")]
    [SerializeField] private RectTransform hsText;
    [SerializeField] private float hsTextPopDuration = 0.20f;
    [SerializeField] private float hsTextOvershoot = 1.08f;
    [SerializeField] private float delayAfterTopBlock = 0.08f;

    [Header("Initials Block")]
    [SerializeField] private RectTransform initialsBlock;
    [SerializeField] private float initialsBlockPopDuration = 0.18f;
    [SerializeField] private float initialsBlockOvershoot = 1.08f;
    [SerializeField] private float delayAfterHsText = 0.08f;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip highScoreAppearClip;
    [SerializeField] private float highScoreAppearVolume = 1f;

    private Coroutine _sequenceRoutine;

    private Vector3 _goBaseScale = Vector3.one;
    private Vector3 _topBlockBaseScale = Vector3.one;
    private Vector3 _hsTextBaseScale = Vector3.one;
    private Vector3 _initialsBlockBaseScale = Vector3.one;

    private bool _baseScalesCached;

    private void Awake()
    {
        CacheBaseScales();
        EnsureSfxSource();
    }

    private void OnEnable()
    {
        if (_sequenceRoutine != null)
            StopCoroutine(_sequenceRoutine);

        CacheBaseScales();
        EnsureSfxSource();
        PrepareInitialState();

        _sequenceRoutine = StartCoroutine(SequenceRoutine());
    }

    private void OnDisable()
    {
        if (_sequenceRoutine != null)
        {
            StopCoroutine(_sequenceRoutine);
            _sequenceRoutine = null;
        }
    }

    private void CacheBaseScales()
    {
        if (_baseScalesCached)
            return;

        _goBaseScale = goText != null ? goText.localScale : Vector3.one;
        _topBlockBaseScale = topBlock != null ? topBlock.localScale : Vector3.one;
        _hsTextBaseScale = hsText != null ? hsText.localScale : Vector3.one;
        _initialsBlockBaseScale = initialsBlock != null ? initialsBlock.localScale : Vector3.one;

        _baseScalesCached = true;
    }

    private void PrepareInitialState()
    {
        if (overlayCanvasGroup != null)
            overlayCanvasGroup.alpha = startAlpha;

        if (goText != null)
            goText.localScale = Vector3.zero;

        if (topBlock != null)
            topBlock.localScale = Vector3.zero;

        if (hsText != null)
            hsText.localScale = Vector3.zero;

        if (initialsBlock != null)
            initialsBlock.localScale = Vector3.zero;
    }

    private IEnumerator SequenceRoutine()
    {
        if (overlayCanvasGroup != null)
            yield return FadeRoutine();

        if (goText != null)
            yield return PopRoutine(goText, _goBaseScale, goPopDuration, goOvershoot);

        if (delayAfterGoText > 0f)
            yield return new WaitForSecondsRealtime(delayAfterGoText);

        if (topBlock != null)
            yield return PopRoutine(topBlock, _topBlockBaseScale, topBlockPopDuration, topBlockOvershoot);

        if (delayAfterTopBlock > 0f)
            yield return new WaitForSecondsRealtime(delayAfterTopBlock);

        if (hsText != null)
        {
            PlaySfx(highScoreAppearClip, highScoreAppearVolume);
            yield return PopRoutine(hsText, _hsTextBaseScale, hsTextPopDuration, hsTextOvershoot);
        }

        if (delayAfterHsText > 0f)
            yield return new WaitForSecondsRealtime(delayAfterHsText);

        if (initialsBlock != null)
            yield return PopRoutine(initialsBlock, _initialsBlockBaseScale, initialsBlockPopDuration, initialsBlockOvershoot);

        _sequenceRoutine = null;
    }

    private IEnumerator FadeRoutine()
    {
        float duration = Mathf.Max(0.0001f, fadeDuration);
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);
            overlayCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        overlayCanvasGroup.alpha = targetAlpha;
    }

    private IEnumerator PopRoutine(RectTransform target, Vector3 baseScale, float duration, float overshoot)
    {
        float safeDuration = Mathf.Max(0.0001f, duration);
        float time = 0f;
        Vector3 overshootScale = baseScale * Mathf.Max(1f, overshoot);

        while (time < safeDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / safeDuration);

            Vector3 scale;

            if (t < 0.75f)
            {
                float t1 = t / 0.75f;
                t1 = EaseOutBack01(t1);
                scale = Vector3.LerpUnclamped(Vector3.zero, overshootScale, t1);
            }
            else
            {
                float t2 = (t - 0.75f) / 0.25f;
                t2 = EaseOutCubic01(t2);
                scale = Vector3.LerpUnclamped(overshootScale, baseScale, t2);
            }

            target.localScale = scale;
            yield return null;
        }

        target.localScale = baseScale;
    }

    private float EaseOutCubic01(float t)
    {
        t = Mathf.Clamp01(t);
        float inv = 1f - t;
        return 1f - (inv * inv * inv);
    }

    private float EaseOutBack01(float t)
    {
        t = Mathf.Clamp01(t);
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private void PlaySfx(AudioClip clip, float volume)
    {
        if (clip == null || sfxSource == null)
            return;

        GameAudio.PlaySfx(sfxSource, clip, Mathf.Clamp01(volume));
    }

    private void EnsureSfxSource()
    {
        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
        GameAudio.ConfigureSfxSource(sfxSource);
    }
}
