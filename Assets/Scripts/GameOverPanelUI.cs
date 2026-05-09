using System.Collections;
using UnityEngine;
using TMPro;

public class GameOverPanelUI : MonoBehaviour
{
    [Header("Overlay")]
    [SerializeField] private CanvasGroup overlayCanvasGroup;
    [SerializeField] private float overlayFadeDuration = 0.20f;
    [SerializeField] private float overlayStartAlpha = 0f;
    [SerializeField] private float overlayTargetAlpha = 1f;

    [Header("Texts")]
    [SerializeField] private TMP_Text scoreTxt;

    [Header("Animated Groups")]
    [Tooltip("Padre del título GAME OVER")]
    [SerializeField] private RectTransform titleRoot;

    [Tooltip("Padre del bloque SCORE (label + valor)")]
    [SerializeField] private RectTransform scoreRoot;

    [Tooltip("Botón CONTINUE o su root")]
    [SerializeField] private RectTransform continueRoot;

    [Header("Intro Animation")]
    [SerializeField] private float titlePopDuration = 0.22f;
    [SerializeField] private float delayAfterTitle = 0.10f;
    [SerializeField] private float scorePopDuration = 0.20f;
    [SerializeField] private float delayAfterScore = 0.08f;
    [SerializeField] private float continuePopDuration = 0.18f;

    [Header("Pop Scale")]
    [SerializeField] private float popOvershoot = 1.08f;

    [Header("Tax Visual (like NextLevel)")]
    [SerializeField] private int taxStep = 10;
    [SerializeField] private float taxTickSeconds = 0.02f;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip titleAppearClip;
    [SerializeField] private float titleAppearVolume = 1f;

    private Coroutine _taxRoutine;
    private Coroutine _introRoutine;

    private Vector3 _titleBaseScale = Vector3.one;
    private Vector3 _scoreBaseScale = Vector3.one;
    private Vector3 _continueBaseScale = Vector3.one;

    private bool _baseScalesCached;

    private void Awake()
    {
        CacheBaseScales();
        EnsureSfxSource();
    }

    private void OnEnable()
    {
        CacheBaseScales();
        EnsureSfxSource();
        GameAudio.StopAllMusic(sfxSource);
        StopAllRunningCoroutines();
        PrepareHiddenState();
        _introRoutine = StartCoroutine(IntroRoutine());
    }

    private void OnDisable()
    {
        StopAllRunningCoroutines();
    }

    /// <summary>
    /// Mantiene compatibilidad con el flujo actual.
    /// Ahora solo actualiza el score; la intro arranca en OnEnable.
    /// </summary>
    public void Show(int score)
    {
        SetScoreInstant(score);
    }

    public void SetScore(int score)
    {
        SetScoreInstant(score);
    }

    public void SetScoreInstant(int score)
    {
        if (scoreTxt != null)
            scoreTxt.text = score.ToString("D6");
    }

    public void ShowAllInstant()
    {
        CacheBaseScales();

        if (overlayCanvasGroup != null)
            overlayCanvasGroup.alpha = overlayTargetAlpha;

        if (titleRoot != null)
            titleRoot.localScale = _titleBaseScale;

        if (scoreRoot != null)
            scoreRoot.localScale = _scoreBaseScale;

        if (continueRoot != null)
            continueRoot.localScale = _continueBaseScale;
    }

    /// <summary>
    /// Anima el descuento visual del TAX sobre el score.
    /// startScore -> endScore (descendente), con step/tick en realtime.
    /// </summary>
    public void PlayTaxVisual(int startScore, int endScore)
    {
        if (_taxRoutine != null)
            StopCoroutine(_taxRoutine);

        _taxRoutine = StartCoroutine(TaxRoutine(startScore, endScore));
    }

    private IEnumerator IntroRoutine()
    {
        if (overlayCanvasGroup != null)
            yield return FadeOverlayRoutine();

        if (titleRoot != null)
        {
            PlaySfx(titleAppearClip, titleAppearVolume);
            yield return PopRoutine(titleRoot, _titleBaseScale, titlePopDuration);
        }

        if (delayAfterTitle > 0f)
            yield return new WaitForSecondsRealtime(delayAfterTitle);

        if (scoreRoot != null)
            yield return PopRoutine(scoreRoot, _scoreBaseScale, scorePopDuration);

        if (delayAfterScore > 0f)
            yield return new WaitForSecondsRealtime(delayAfterScore);

        if (continueRoot != null)
            yield return PopRoutine(continueRoot, _continueBaseScale, continuePopDuration);

        _introRoutine = null;
    }

    private void PrepareHiddenState()
    {
        if (overlayCanvasGroup != null)
            overlayCanvasGroup.alpha = overlayStartAlpha;

        if (titleRoot != null)
            titleRoot.localScale = Vector3.zero;

        if (scoreRoot != null)
            scoreRoot.localScale = Vector3.zero;

        if (continueRoot != null)
            continueRoot.localScale = Vector3.zero;
    }

    private IEnumerator FadeOverlayRoutine()
    {
        float duration = Mathf.Max(0.0001f, overlayFadeDuration);
        float time = 0f;

        overlayCanvasGroup.alpha = overlayStartAlpha;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);
            overlayCanvasGroup.alpha = Mathf.Lerp(overlayStartAlpha, overlayTargetAlpha, t);
            yield return null;
        }

        overlayCanvasGroup.alpha = overlayTargetAlpha;
    }

    private IEnumerator TaxRoutine(int startScore, int endScore)
    {
        int current = startScore;
        int step = Mathf.Max(1, taxStep);
        float tick = Mathf.Max(0.001f, taxTickSeconds);

        if (scoreTxt != null)
            scoreTxt.text = current.ToString("D6");

        while (current > endScore)
        {
            current = Mathf.Max(endScore, current - step);

            if (scoreTxt != null)
                scoreTxt.text = current.ToString("D6");

            yield return new WaitForSecondsRealtime(tick);
        }

        if (scoreTxt != null)
            scoreTxt.text = endScore.ToString("D6");

        _taxRoutine = null;
    }

    private IEnumerator PopRoutine(RectTransform target, Vector3 baseScale, float duration)
    {
        float safeDuration = Mathf.Max(0.0001f, duration);
        float time = 0f;
        Vector3 overshootScale = baseScale * Mathf.Max(1f, popOvershoot);

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

    private void CacheBaseScales()
    {
        if (_baseScalesCached)
            return;

        _titleBaseScale = titleRoot != null ? titleRoot.localScale : Vector3.one;
        _scoreBaseScale = scoreRoot != null ? scoreRoot.localScale : Vector3.one;
        _continueBaseScale = continueRoot != null ? continueRoot.localScale : Vector3.one;

        _baseScalesCached = true;
    }

    private void StopAllRunningCoroutines()
    {
        if (_taxRoutine != null)
        {
            StopCoroutine(_taxRoutine);
            _taxRoutine = null;
        }

        if (_introRoutine != null)
        {
            StopCoroutine(_introRoutine);
            _introRoutine = null;
        }
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
