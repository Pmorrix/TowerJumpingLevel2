using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NextLevelPanelUI : MonoBehaviour
{
    [Header("Global Intro")]
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private Image overlayImage;
    [SerializeField] private bool doGlobalIntro = true;
    [SerializeField] private float overlayFadeDuration = 0.20f;
    [SerializeField] private float panelIntroDuration = 0.20f;
    [SerializeField] private float panelIntroStartScale = 0.97f;
    [SerializeField] private float delayAfterGlobalIntro = 0.08f;

    [Header("Title Root")]
    [SerializeField] private RectTransform titleRoot;

    [Header("Block Roots")]
    [SerializeField] private RectTransform scoreRoot;
    [SerializeField] private RectTransform bonusRoot;
    [SerializeField] private RectTransform totalRoot;
    [SerializeField] private RectTransform continueRoot;

    [Header("Value Texts")]
    [SerializeField] private TMP_Text scoreTxt;
    [SerializeField] private TMP_Text bonusTxt;
    [SerializeField] private TMP_Text totalTxt;

    [Header("Title Intro")]
    [SerializeField] private bool doTitleIntroScale = true;
    [SerializeField] private float titleIntroDuration = 0.22f;
    [SerializeField] private float delayAfterTitle = 0.18f;

    [Header("Sequence Timing")]
    [SerializeField] private float delayScoreToBonus = 0.20f;
    [SerializeField] private float delayBonusToTotal = 0.20f;
    [SerializeField] private float delayTotalToCount = 0.12f;
    [SerializeField] private float delayAfterTotalPunch = 0.12f;

    [Header("Total Score Animation")]
    [SerializeField] private int totalStep = 10;
    [SerializeField] private float totalTickSeconds = 0.02f;

    [Header("Final Punch")]
    [SerializeField] private bool doFinalPunch = true;
    [SerializeField] private float punchScale = 0.15f;
    [SerializeField] private float punchUpDuration = 0.08f;
    [SerializeField] private float punchDownDuration = 0.12f;

    [Header("Entry Punch")]
    [SerializeField] private bool doScorePunch = true;
    [SerializeField] private bool doBonusPunch = true;
    [SerializeField] private bool doTotalPunch = true;
    [SerializeField] private bool doContinuePunch = true;
    [SerializeField] private float entryPunchScale = 0.10f;
    [SerializeField] private float entryPunchUpDuration = 0.06f;
    [SerializeField] private float entryPunchDownDuration = 0.10f;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip titleAppearClip;
    [SerializeField] private AudioClip scoreAppearClip;
    [SerializeField] private AudioClip bonusAppearClip;
    [SerializeField] private AudioClip totalAppearClip;
    [SerializeField] private AudioClip totalTickClip;
    [SerializeField] private AudioClip totalFinalClip;
    [SerializeField] private float appearVolume = 0.8f;
    [SerializeField] private float tickVolume = 0.35f;
    [SerializeField] private float finalVolume = 1f;
    [SerializeField] private int tickSoundEverySteps = 2;
    [SerializeField] private bool stopMusicWhileShown = true;

    private Coroutine _sequenceRoutine;
    private Coroutine _totalRoutine;
    private Coroutine _punchRoutine;
    private Coroutine _scorePunchRoutine;
    private Coroutine _bonusPunchRoutine;
    private Coroutine _totalPunchRoutine;
    private Coroutine _continuePunchRoutine;

    private Vector3 _originalPanelScale;
    private Vector3 _originalTitleScale;
    private Vector3 _originalScoreRootScale;
    private Vector3 _originalBonusRootScale;
    private Vector3 _originalTotalRootScale;
    private Vector3 _originalContinueRootScale;

    private Color _overlayOriginalColor;

    private void Awake()
    {
        if (panelRoot != null)
            _originalPanelScale = panelRoot.localScale;

        if (titleRoot != null)
            _originalTitleScale = titleRoot.localScale;

        if (scoreRoot != null)
            _originalScoreRootScale = scoreRoot.localScale;

        if (bonusRoot != null)
            _originalBonusRootScale = bonusRoot.localScale;

        if (totalRoot != null)
            _originalTotalRootScale = totalRoot.localScale;

        if (continueRoot != null)
            _originalContinueRootScale = continueRoot.localScale;

        if (overlayImage != null)
            _overlayOriginalColor = overlayImage.color;

        EnsureSfxSource();
    }

    public void SetValues(int scoreAfterTax, int bonus, int finalTotal)
    {
        StopAllRunningCoroutines();
        StopMusicIfNeeded();
        ResetVisualState(scoreAfterTax, bonus);
        _sequenceRoutine = StartCoroutine(PlaySequence(scoreAfterTax, finalTotal));
    }

    private void ResetVisualState(int scoreAfterTax, int bonus)
    {
        if (panelRoot != null)
            panelRoot.localScale = doGlobalIntro
                ? _originalPanelScale * panelIntroStartScale
                : _originalPanelScale;

        if (overlayImage != null)
        {
            Color c = _overlayOriginalColor;
            c.a = doGlobalIntro ? 0f : _overlayOriginalColor.a;
            overlayImage.color = c;
        }

        if (titleRoot != null)
            titleRoot.localScale = doTitleIntroScale ? Vector3.zero : _originalTitleScale;

        if (scoreTxt != null)
            scoreTxt.text = scoreAfterTax.ToString("D6");

        if (bonusTxt != null)
            bonusTxt.text = "+" + bonus.ToString("D6");

        if (totalTxt != null)
            totalTxt.text = scoreAfterTax.ToString("D6");

        if (scoreRoot != null)
        {
            scoreRoot.localScale = _originalScoreRootScale;
            scoreRoot.gameObject.SetActive(false);
        }

        if (bonusRoot != null)
        {
            bonusRoot.localScale = _originalBonusRootScale;
            bonusRoot.gameObject.SetActive(false);
        }

        if (totalRoot != null)
        {
            totalRoot.localScale = _originalTotalRootScale;
            totalRoot.gameObject.SetActive(false);
        }

        if (continueRoot != null)
        {
            continueRoot.localScale = _originalContinueRootScale;
            continueRoot.gameObject.SetActive(false);
        }
    }

    private IEnumerator PlaySequence(int scoreAfterTax, int finalTotal)
    {
        if (doGlobalIntro)
        {
            Coroutine fadeRoutine = null;
            Coroutine scaleRoutine = null;

            if (overlayImage != null)
                fadeRoutine = StartCoroutine(FadeOverlayIn());

            if (panelRoot != null)
                scaleRoutine = StartCoroutine(ScalePanelIn());

            if (fadeRoutine != null)
                yield return fadeRoutine;

            if (scaleRoutine != null)
                yield return scaleRoutine;

            yield return new WaitForSecondsRealtime(delayAfterGlobalIntro);
        }

        if (titleRoot != null)
            PlaySfx(titleAppearClip, appearVolume);

        if (doTitleIntroScale && titleRoot != null)
            yield return ScaleFromZero(titleRoot, _originalTitleScale, titleIntroDuration);

        yield return new WaitForSecondsRealtime(delayAfterTitle);

        if (scoreRoot != null)
        {
            scoreRoot.gameObject.SetActive(true);
            PlaySfx(scoreAppearClip, appearVolume);

            if (doScorePunch)
            {
                if (_scorePunchRoutine != null)
                    StopCoroutine(_scorePunchRoutine);

                _scorePunchRoutine = StartCoroutine(
                    PunchScaleTransform(
                        scoreRoot,
                        _originalScoreRootScale,
                        entryPunchScale,
                        entryPunchUpDuration,
                        entryPunchDownDuration
                    )
                );
            }
        }

        yield return new WaitForSecondsRealtime(delayScoreToBonus);

        if (bonusRoot != null)
        {
            bonusRoot.gameObject.SetActive(true);
            PlaySfx(bonusAppearClip, appearVolume);

            if (doBonusPunch)
            {
                if (_bonusPunchRoutine != null)
                    StopCoroutine(_bonusPunchRoutine);

                _bonusPunchRoutine = StartCoroutine(
                    PunchScaleTransform(
                        bonusRoot,
                        _originalBonusRootScale,
                        entryPunchScale,
                        entryPunchUpDuration,
                        entryPunchDownDuration
                    )
                );
            }
        }

        yield return new WaitForSecondsRealtime(delayBonusToTotal);

        if (totalRoot != null)
        {
            totalRoot.gameObject.SetActive(true);
            PlaySfx(totalAppearClip, appearVolume);

            if (doTotalPunch)
            {
                if (_totalPunchRoutine != null)
                    StopCoroutine(_totalPunchRoutine);

                _totalPunchRoutine = StartCoroutine(
                    PunchScaleTransform(
                        totalRoot,
                        _originalTotalRootScale,
                        entryPunchScale,
                        entryPunchUpDuration,
                        entryPunchDownDuration
                    )
                );
            }
        }

        yield return new WaitForSecondsRealtime(delayTotalToCount);

        _totalRoutine = StartCoroutine(AnimateTotal(scoreAfterTax, finalTotal));
        yield return _totalRoutine;
        _totalRoutine = null;

        yield return new WaitForSecondsRealtime(delayAfterTotalPunch);

        if (continueRoot != null)
        {
            continueRoot.gameObject.SetActive(true);

            if (doContinuePunch)
            {
                if (_continuePunchRoutine != null)
                    StopCoroutine(_continuePunchRoutine);

                _continuePunchRoutine = StartCoroutine(
                    PunchScaleTransform(
                        continueRoot,
                        _originalContinueRootScale,
                        entryPunchScale,
                        entryPunchUpDuration,
                        entryPunchDownDuration
                    )
                );
            }
        }
    }

    private IEnumerator FadeOverlayIn()
    {
        if (overlayImage == null)
            yield break;

        float duration = Mathf.Max(0.0001f, overlayFadeDuration);
        float t = 0f;

        Color start = _overlayOriginalColor;
        start.a = 0f;

        Color end = _overlayOriginalColor;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            k = EaseOut(k);

            overlayImage.color = Color.LerpUnclamped(start, end, k);
            yield return null;
        }

        overlayImage.color = end;
    }

    private IEnumerator ScalePanelIn()
    {
        if (panelRoot == null)
            yield break;

        float duration = Mathf.Max(0.0001f, panelIntroDuration);
        float t = 0f;

        Vector3 startScale = _originalPanelScale * panelIntroStartScale;
        Vector3 endScale = _originalPanelScale;

        panelRoot.localScale = startScale;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            k = EaseOut(k);

            panelRoot.localScale = Vector3.LerpUnclamped(startScale, endScale, k);
            yield return null;
        }

        panelRoot.localScale = endScale;
    }

    private IEnumerator AnimateTotal(int startValue, int endValue)
    {
        int current = startValue;
        float tick = Mathf.Max(0.001f, totalTickSeconds);

        if (totalRoot != null)
            totalRoot.localScale = _originalTotalRootScale;

        int stepsPlayed = 0;

        while (current < endValue)
        {
            int remaining = endValue - current;
            int dynamicStep = Mathf.Max(1, Mathf.Max(totalStep, (endValue - startValue) / 40));

            if (remaining < dynamicStep * 3)
                dynamicStep = Mathf.Max(1, dynamicStep / 2);

            if (remaining < dynamicStep * 2)
                dynamicStep = Mathf.Max(1, dynamicStep / 2);

            current = Mathf.Min(current + dynamicStep, endValue);

            if (totalTxt != null)
                totalTxt.text = current.ToString("D6");

            stepsPlayed++;
            if (tickSoundEverySteps > 0 && stepsPlayed % tickSoundEverySteps == 0)
                PlaySfx(totalTickClip, tickVolume);

            yield return new WaitForSecondsRealtime(tick);
        }

        if (totalTxt != null)
            totalTxt.text = endValue.ToString("D6");

        if (doFinalPunch && totalRoot != null)
        {
            PlaySfx(totalFinalClip, finalVolume);

            if (_punchRoutine != null)
                StopCoroutine(_punchRoutine);

            _punchRoutine = StartCoroutine(
                PunchScaleTransform(
                    totalRoot,
                    _originalTotalRootScale,
                    punchScale,
                    punchUpDuration,
                    punchDownDuration
                )
            );

            yield return _punchRoutine;
            _punchRoutine = null;
        }
    }

    private IEnumerator ScaleFromZero(Transform target, Vector3 targetScale, float duration)
    {
        if (target == null)
            yield break;

        float safeDuration = Mathf.Max(0.0001f, duration);
        float t = 0f;

        target.localScale = Vector3.zero;

        while (t < safeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / safeDuration);
            k = EaseOutBack(k);

            target.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, k);
            yield return null;
        }

        target.localScale = targetScale;
    }

    private IEnumerator PunchScaleTransform(
        Transform target,
        Vector3 originalScale,
        float scaleAmount,
        float upDuration,
        float downDuration)
    {
        if (target == null)
            yield break;

        Vector3 targetScale = originalScale * (1f + scaleAmount);

        float t = 0f;
        while (t < upDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / upDuration);
            k = EaseOut(k);

            target.localScale = Vector3.LerpUnclamped(originalScale, targetScale, k);
            yield return null;
        }

        t = 0f;
        while (t < downDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / downDuration);
            k = EaseIn(k);

            target.localScale = Vector3.LerpUnclamped(targetScale, originalScale, k);
            yield return null;
        }

        target.localScale = originalScale;
    }

    private void StopAllRunningCoroutines()
    {
        if (_sequenceRoutine != null)
        {
            StopCoroutine(_sequenceRoutine);
            _sequenceRoutine = null;
        }

        if (_totalRoutine != null)
        {
            StopCoroutine(_totalRoutine);
            _totalRoutine = null;
        }

        if (_punchRoutine != null)
        {
            StopCoroutine(_punchRoutine);
            _punchRoutine = null;
        }

        if (_scorePunchRoutine != null)
        {
            StopCoroutine(_scorePunchRoutine);
            _scorePunchRoutine = null;
        }

        if (_bonusPunchRoutine != null)
        {
            StopCoroutine(_bonusPunchRoutine);
            _bonusPunchRoutine = null;
        }

        if (_totalPunchRoutine != null)
        {
            StopCoroutine(_totalPunchRoutine);
            _totalPunchRoutine = null;
        }

        if (_continuePunchRoutine != null)
        {
            StopCoroutine(_continuePunchRoutine);
            _continuePunchRoutine = null;
        }
    }

    private float EaseOut(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private float EaseIn(float t)
    {
        return t * t * t;
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private void PlaySfx(AudioClip clip, float volume)
    {
        EnsureSfxSource();
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

    private void StopMusicIfNeeded()
    {
        if (!stopMusicWhileShown)
            return;

        EnsureSfxSource();
        GameAudio.StopAllMusic(sfxSource);
    }
}
