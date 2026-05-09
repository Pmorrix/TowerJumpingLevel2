using System.Collections;
using UnityEngine;

public class TimeOutPanelUI : MonoBehaviour
{
    [Header("Overlay")]
    [SerializeField] private CanvasGroup overlayCanvasGroup;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float startAlpha = 0f;
    [SerializeField] private float targetAlpha = 1f;

    [Header("Header")]
    [SerializeField] private RectTransform headerTxt;
    [SerializeField] private float headerPopDuration = 0.22f;
    [SerializeField] private float headerOvershoot = 1.08f;

    [Header("Body")]
    [SerializeField] private RectTransform bodyTxt;
    [SerializeField] private float bodyPopDuration = 0.20f;
    [SerializeField] private float bodyOvershoot = 1.08f;
    [SerializeField] private float delayAfterHeader = 0.08f;

    [Header("Continue")]
    [SerializeField] private RectTransform continueBtn;
    [SerializeField] private float continuePopDuration = 0.18f;
    [SerializeField] private float continueOvershoot = 1.08f;
    [SerializeField] private float delayAfterBody = 0.08f;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip headerAppearClip;
    [SerializeField] private float headerAppearVolume = 1f;

    private Coroutine _sequenceRoutine;

    private Vector3 _headerBaseScale = Vector3.one;
    private Vector3 _bodyBaseScale = Vector3.one;
    private Vector3 _continueBaseScale = Vector3.one;

    private bool _baseScalesCached;
    private NewGameButtonPulse _continuePulse;

    private void Awake()
    {
        CacheBaseScales();
        EnsureSfxSource();
        CacheContinuePulse();
    }

    private void OnEnable()
    {
        if (_sequenceRoutine != null)
            StopCoroutine(_sequenceRoutine);

        CacheBaseScales();
        EnsureSfxSource();
        CacheContinuePulse();
        SetContinuePulseEnabled(false);
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

        _headerBaseScale = headerTxt != null ? headerTxt.localScale : Vector3.one;
        _bodyBaseScale = bodyTxt != null ? bodyTxt.localScale : Vector3.one;
        _continueBaseScale = continueBtn != null ? continueBtn.localScale : Vector3.one;

        _baseScalesCached = true;
    }

    private void PrepareInitialState()
    {
        if (overlayCanvasGroup != null)
            overlayCanvasGroup.alpha = startAlpha;

        if (headerTxt != null)
            headerTxt.localScale = Vector3.zero;

        if (bodyTxt != null)
            bodyTxt.localScale = Vector3.zero;

        if (continueBtn != null)
            continueBtn.localScale = Vector3.zero;
    }

    private IEnumerator SequenceRoutine()
    {
        if (overlayCanvasGroup != null)
            yield return FadeRoutine();

        if (headerTxt != null)
        {
            PlaySfx(headerAppearClip, headerAppearVolume);
            yield return PopRoutine(headerTxt, _headerBaseScale, headerPopDuration, headerOvershoot);
        }

        if (delayAfterHeader > 0f)
            yield return new WaitForSecondsRealtime(delayAfterHeader);

        if (bodyTxt != null)
            yield return PopRoutine(bodyTxt, _bodyBaseScale, bodyPopDuration, bodyOvershoot);

        if (delayAfterBody > 0f)
            yield return new WaitForSecondsRealtime(delayAfterBody);

        if (continueBtn != null)
            yield return PopRoutine(continueBtn, _continueBaseScale, continuePopDuration, continueOvershoot);

        SetContinuePulseEnabled(true);

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

    private void CacheContinuePulse()
    {
        if (_continuePulse == null && continueBtn != null)
            _continuePulse = continueBtn.GetComponent<NewGameButtonPulse>();
    }

    private void SetContinuePulseEnabled(bool enabled)
    {
        if (_continuePulse != null)
            _continuePulse.enabled = enabled;
    }
}
