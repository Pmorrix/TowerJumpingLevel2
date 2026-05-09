using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class NewPhaseManager : MonoBehaviour
{
    [Header("Goal")]
    [SerializeField] private NewBuildingGoalController newGoalBuilding;
    [SerializeField] private float goalEnableTime = 5f;

    [Header("Countdown (1000 -> 0)")]
    [SerializeField] private int startValue = 1000;
    [SerializeField] private float decayPerSecond = 10f;

    [Header("HUD")]
    [SerializeField] private TMP_Text timeTxt;

    [Header("Level Complete")]
    [Tooltip("Panel root de 'Nivel completado'. Controlado por CanvasGroup.")]
    [SerializeField] private GameObject levelCompletePanelRoot;
    [SerializeField] private NextLevelPanelUI nextLevelPanelUI;

    [Header("Score")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private NewPlayerLanding playerLanding;

    [Header("Level Bonus")]
    [SerializeField] private int levelBonus = 200;

    [Header("Time → Score (Exit TAX)")]
    [SerializeField] private bool visualTimeToScoreTransfer = true;
    [SerializeField] private int timeRemainingScoreMultiplier = 1;
    [SerializeField] private int transferStep = 10;
    [SerializeField] private float transferTickSeconds = 0.02f;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip goalEnabledClip;
    [SerializeField] private AudioClip goalReachedClip;
    [SerializeField] private AudioClip goalEffectBellClip;
    [SerializeField] private AudioSource musicSource;

    [Header("Behaviour")]
    [SerializeField] private bool freezeOnComplete = true;

    [Header("Player Stop On Goal")]
    [SerializeField] private Rigidbody playerRigidbody;

    [Header("Player Control On Goal")]
    [SerializeField] private MonoBehaviour[] playerBehavioursToDisable;

    [Header("Booster Stop On Goal")]
    [SerializeField] private GameObject boosterRoot;

    [Header("Goal Auto Jump")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float goalJumpDuration = 0.30f;
    [SerializeField] private float goalJumpArcHeight = 0.75f;
    [SerializeField] private float goalJumpEndYOffset = 0.18f;

    [Header("Player Visual On Goal")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private float goalFacingY = 0f;

    [Header("Goal Celebrate Jump")]
    [SerializeField] private float celebrateJumpDuration = 0.30f;
    [SerializeField] private float celebrateJumpHeight = 0.70f;
    [SerializeField] private float delayAfterCelebrateJump = 0.10f;

    [Header("Animator Params")]
    [SerializeField] private string groundedParam = "IsGrounded";
    [SerializeField] private string verticalSpeedParam = "VerticalSpeed";
    [SerializeField] private string moveXParam = "MoveX";

    [Header("Animator State Names")]
    [SerializeField] private string jumpStateName = "Jump";
    [SerializeField] private string fallStateName = "Fall";
    [SerializeField] private string idleStateName = "Idle";

    [Header("Goal Effect")]
    [SerializeField] private GameObject goalEffect;

    [Header("Player Visibility")]
    [SerializeField] private GameObject player;

    public event Action OnTimeUp;

    private float _timer;
    private bool _timerStarted;
    private bool _timerPaused;
    private bool _goalEnabled;
    private bool _completed;
    private bool _timeUp;

    private int _finalScoreAfterTax;
    private int _finalScoreWithBonus;

    private bool _cachedPlayerPhysicsState;
    private bool _playerWasKinematic;
    private bool _playerUsedGravity;

    private Coroutine _goalEnableRoutine;
    private bool _goalPendingUntilPlayerLeaves;

    public bool TimerStarted => _timerStarted;
    public bool TimerRunning => _timerStarted && !_timerPaused;
    public bool GoalEnabled => _goalEnabled;
    public bool IsCompleted => _completed;
    public bool IsTimeUp => _timeUp;

    private void Awake()
    {
        GameAudio.ConfigureSfxSource(sfxSource);
        GameAudio.ConfigureMusicSource(musicSource);
    }

    public int CurrentCountdownValue
    {
        get
        {
            if (!_timerStarted)
                return startValue;

            int v = Mathf.FloorToInt(startValue - _timer * decayPerSecond);
            return Mathf.Clamp(v, 0, startValue);
        }
    }

    private void Start()
    {
        ResetPhase();
    }

    private void Update()
    {
        if (_completed || _timeUp)
            return;

        if (!_timerStarted || _timerPaused)
            return;

        if (timeTxt != null)
            timeTxt.text = CurrentCountdownValue.ToString("D4");

        _timer += Time.deltaTime;

        if (CurrentCountdownValue <= 0)
        {
            _timeUp = true;
            StopGoalEnableRoutine();
            DisableGoal();

            ScoreManager.SetCanAddScore(false);
            OnTimeUp?.Invoke();
        }
    }

    public void OnExitBuildingLeft()
    {
        if (_completed || _timeUp)
            return;

        if (!_timerStarted)
        {
            _timerStarted = true;
            _timer = 0f;
        }

        _timerPaused = false;

        if (_goalEnabled || _goalEnableRoutine != null || _goalPendingUntilPlayerLeaves)
            return;

        StartGoalEnableRoutine();
    }

    public void OnPlayerTouchedGoal(Vector3 goalTopCenter)
    {
        if (_completed || _timeUp || !_goalEnabled)
            return;

        _completed = true;
        DisableGoal();

        GameAudio.StopMusic(musicSource);

        if (playerLanding != null)
            playerLanding.CloseComboOnGoal();

        ScoreManager.SetCanAddScore(false);

        DisablePlayerControlOnGoal();
        StopPlayerImmediately();
        StopBoosterOnGoal();
        RefreshPlayerAnimator();
        SetPlayerFacingOnGoal();

        if (sfxSource != null && goalReachedClip != null)
            GameAudio.PlaySfx(sfxSource, goalReachedClip);

        StartCoroutine(GoalSequence(goalTopCenter));
    }

    public void OnPlayerExitedGoalArea()
    {
        if (_completed || _timeUp || _goalEnabled)
            return;

        if (!_goalPendingUntilPlayerLeaves)
            return;

        if (newGoalBuilding != null && newGoalBuilding.IsPlayerOnGoal)
            return;

        _goalPendingUntilPlayerLeaves = false;
        EnableGoalNow();
    }

    private void StartGoalEnableRoutine()
    {
        StopGoalEnableRoutine();

        float delay = Mathf.Max(0f, goalEnableTime);

        if (delay <= 0f)
        {
            TryEnableGoalAfterDelay();
            return;
        }

        _goalEnableRoutine = StartCoroutine(EnableGoalAfterDelay(delay));
    }

    private IEnumerator EnableGoalAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        _goalEnableRoutine = null;
        TryEnableGoalAfterDelay();
    }

    private void TryEnableGoalAfterDelay()
    {
        if (_completed || _timeUp || _timerPaused)
            return;

        if (newGoalBuilding != null && newGoalBuilding.IsPlayerOnGoal)
        {
            _goalPendingUntilPlayerLeaves = true;
            return;
        }

        EnableGoalNow();
    }

    private void EnableGoalNow()
    {
        if (_completed || _timeUp)
            return;

        if (newGoalBuilding != null && newGoalBuilding.IsPlayerOnGoal)
        {
            _goalPendingUntilPlayerLeaves = true;
            return;
        }

        _goalPendingUntilPlayerLeaves = false;
        _goalEnabled = true;

        if (newGoalBuilding != null)
            newGoalBuilding.SetGoalEnabled(true);

        PlayGoalEnabledSfx();
    }

    private void StopGoalEnableRoutine()
    {
        if (_goalEnableRoutine == null)
            return;

        StopCoroutine(_goalEnableRoutine);
        _goalEnableRoutine = null;
    }

    public void DisableGoal()
    {
        StopGoalEnableRoutine();

        _goalPendingUntilPlayerLeaves = false;
        _goalEnabled = false;

        if (newGoalBuilding != null)
            newGoalBuilding.SetGoalEnabled(false);
    }

    /// <summary>
    /// Tras morir:
    /// - pausa el TAX manteniendo su valor actual
    /// - apaga el goal
    /// - bloquea la suma de score
    /// </summary>
    public void ResetSpawnStateAfterRespawn()
    {
        if (_completed || _timeUp)
            return;

        _timerPaused = true;
        DisableGoal();

        if (timeTxt != null)
            timeTxt.text = CurrentCountdownValue.ToString("D4");

        ScoreManager.SetCanAddScore(false);
    }

    private IEnumerator GoalSequence(Vector3 goalTopCenter)
    {
        yield return JumpPlayerToGoalCenter(goalTopCenter);

        RefreshPlayerAnimator();
        SetPlayerFacingOnGoal();

        yield return PlayCelebrateJumpInPlace();

        if (player != null)
            player.SetActive(false);

        PlayGoalEffect();

        float delay = Mathf.Max(0f, delayAfterCelebrateJump);
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        if (freezeOnComplete)
            Time.timeScale = 0f;

        if (visualTimeToScoreTransfer)
            StartCoroutine(TransferTimeToScoreVisual());
        else
            TransferTimeToScoreInstant();
    }

    private void DisablePlayerControlOnGoal()
    {
        if (playerBehavioursToDisable != null)
        {
            for (int i = 0; i < playerBehavioursToDisable.Length; i++)
            {
                if (playerBehavioursToDisable[i] != null)
                    playerBehavioursToDisable[i].enabled = false;
            }
        }

        RefreshPlayerAnimator();
    }

    private void RefreshPlayerAnimator()
    {
        if (playerAnimator == null)
            return;

        playerAnimator.Rebind();
        playerAnimator.Update(0f);
        PlayAnimatorIdleState();
    }

    private void PlayAnimatorState(string stateName)
    {
        if (playerAnimator == null)
            return;

        if (string.IsNullOrWhiteSpace(stateName))
            return;

        playerAnimator.Play(stateName, 0, 0f);
        playerAnimator.Update(0f);
    }

    private void PlayAnimatorJumpState()
    {
        if (playerAnimator == null)
            return;

        playerAnimator.SetBool(groundedParam, false);
        playerAnimator.SetFloat(verticalSpeedParam, 1f);
        playerAnimator.SetFloat(moveXParam, 0f);
        PlayAnimatorState(jumpStateName);
    }

    private void PlayAnimatorFallState()
    {
        if (playerAnimator == null)
            return;

        playerAnimator.SetBool(groundedParam, false);
        playerAnimator.SetFloat(verticalSpeedParam, -1f);
        playerAnimator.SetFloat(moveXParam, 0f);
        PlayAnimatorState(fallStateName);
    }

    private void PlayAnimatorIdleState()
    {
        if (playerAnimator == null)
            return;

        playerAnimator.SetBool(groundedParam, true);
        playerAnimator.SetFloat(verticalSpeedParam, 0f);
        playerAnimator.SetFloat(moveXParam, 0f);
        PlayAnimatorState(idleStateName);
    }

    private void SetPlayerFacingOnGoal()
    {
        if (playerTransform == null)
            return;

        playerTransform.eulerAngles = new Vector3(0f, goalFacingY, 0f);
    }

    private void StopPlayerImmediately()
    {
        if (playerRigidbody == null)
            return;

        if (!_cachedPlayerPhysicsState)
        {
            _playerWasKinematic = playerRigidbody.isKinematic;
            _playerUsedGravity = playerRigidbody.useGravity;
            _cachedPlayerPhysicsState = true;
        }

        if (!playerRigidbody.isKinematic)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        playerRigidbody.useGravity = false;
        playerRigidbody.isKinematic = true;
    }

    private void StopBoosterOnGoal()
    {
        if (boosterRoot != null)
            boosterRoot.SetActive(false);
    }

    private void PlayGoalEffect()
    {
        if (goalEffect != null)
            goalEffect.SetActive(true);

        if (sfxSource != null && goalEffectBellClip != null)
            GameAudio.PlaySfx(sfxSource, goalEffectBellClip);
    }

    private void PlayGoalEnabledSfx()
    {
        if (sfxSource != null && goalEnabledClip != null)
            GameAudio.PlaySfx(sfxSource, goalEnabledClip);
    }

    private IEnumerator JumpPlayerToGoalCenter(Vector3 goalTopCenter)
    {
        if (playerTransform == null)
            yield break;

        float duration = Mathf.Max(0.05f, goalJumpDuration);
        float arcHeight = Mathf.Max(0f, goalJumpArcHeight);

        Vector3 startPos = playerTransform.position;
        Vector3 endPos = goalTopCenter + Vector3.up * goalJumpEndYOffset;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float u = Mathf.Clamp01(t);

            Vector3 pos = Vector3.Lerp(startPos, endPos, u);
            float arc = 4f * u * (1f - u);
            pos.y += arc * arcHeight;

            playerTransform.position = pos;
            yield return null;
        }

        playerTransform.position = endPos;
    }

    private IEnumerator PlayCelebrateJumpInPlace()
    {
        if (playerTransform == null)
            yield break;

        float duration = Mathf.Max(0.05f, celebrateJumpDuration);
        float halfDuration = duration * 0.5f;
        float height = Mathf.Max(0f, celebrateJumpHeight);

        Vector3 startPos = playerTransform.position;
        Vector3 apexPos = startPos + Vector3.up * height;

        PlayAnimatorJumpState();

        float t = 0f;
        while (t < halfDuration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / halfDuration);
            playerTransform.position = Vector3.Lerp(startPos, apexPos, u);
            yield return null;
        }

        PlayAnimatorFallState();

        t = 0f;
        while (t < halfDuration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / halfDuration);
            playerTransform.position = Vector3.Lerp(apexPos, startPos, u);
            yield return null;
        }

        playerTransform.position = startPos;
        PlayAnimatorIdleState();
    }

    private void TransferTimeToScoreInstant()
    {
        int timeLeft = CurrentCountdownValue;
        int mult = Mathf.Max(1, timeRemainingScoreMultiplier);
        int tax = timeLeft * mult;

        if (scoreManager != null)
            scoreManager.SubScore(tax);

        _finalScoreAfterTax = scoreManager != null ? scoreManager.Score : 0;

        if (scoreManager != null)
            scoreManager.AddScore(levelBonus);

        _finalScoreWithBonus = scoreManager != null ? scoreManager.Score : 0;

        ShowLevelCompletePanel();
    }

    private IEnumerator TransferTimeToScoreVisual()
    {
        if (scoreManager == null)
        {
            ShowLevelCompletePanel();
            yield break;
        }

        int timeLeft = CurrentCountdownValue;
        int mult = Mathf.Max(1, timeRemainingScoreMultiplier);
        int step = Mathf.Max(1, transferStep);
        float tick = Mathf.Max(0.01f, transferTickSeconds);

        while (timeLeft > 0)
        {
            int delta = Mathf.Min(step, timeLeft);
            timeLeft -= delta;

            if (timeTxt != null)
                timeTxt.text = timeLeft.ToString("D4");

            scoreManager.SubScore(delta * mult);
            yield return new WaitForSecondsRealtime(tick);
        }

        _timer = startValue / decayPerSecond;

        if (timeTxt != null)
            timeTxt.text = "0000";

        _finalScoreAfterTax = scoreManager.Score;

        scoreManager.AddScore(levelBonus);
        _finalScoreWithBonus = scoreManager.Score;

        ShowLevelCompletePanel();
    }

    private void ShowLevelCompletePanel()
    {
        if (levelCompletePanelRoot != null)
        {
            levelCompletePanelRoot.SetActive(true);
            levelCompletePanelRoot.transform.SetAsLastSibling();
        }

        if (nextLevelPanelUI != null)
        {
            Debug.Log($"Score before tax: {scoreManager.Score + CalculateExitTaxPoints()}");
            Debug.Log($"TAX applied: {CalculateExitTaxPoints()}");
            Debug.Log($"Final score after tax: {_finalScoreAfterTax}");
            Debug.Log($"Level bonus: {levelBonus}");
            Debug.Log($"Final score with bonus: {_finalScoreWithBonus}");

            nextLevelPanelUI.SetValues(
                _finalScoreAfterTax,
                levelBonus,
                _finalScoreWithBonus
            );
        }
    }

    public void ResetPhase()
    {
        Time.timeScale = 1f;

        StopGoalEnableRoutine();

        _timer = 0f;
        _timerStarted = false;
        _timerPaused = false;
        _goalEnabled = false;
        _goalPendingUntilPlayerLeaves = false;
        _completed = false;
        _timeUp = false;
        _finalScoreAfterTax = 0;
        _finalScoreWithBonus = 0;

        ScoreManager.SetCanAddScore(false);

        if (newGoalBuilding != null)
            newGoalBuilding.SetGoalEnabled(false);

        if (levelCompletePanelRoot != null)
            levelCompletePanelRoot.SetActive(false);

        if (timeTxt != null)
            timeTxt.text = startValue.ToString("D4");

        if (goalEffect != null)
            goalEffect.SetActive(false);

        if (playerRigidbody != null && _cachedPlayerPhysicsState)
        {
            playerRigidbody.isKinematic = _playerWasKinematic;
            playerRigidbody.useGravity = _playerUsedGravity;

            if (!playerRigidbody.isKinematic)
            {
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }
        }

        if (playerBehavioursToDisable != null)
        {
            for (int i = 0; i < playerBehavioursToDisable.Length; i++)
            {
                if (playerBehavioursToDisable[i] != null)
                    playerBehavioursToDisable[i].enabled = true;
            }
        }

        _cachedPlayerPhysicsState = false;
    }

    public int CalculateExitTaxPoints()
    {
        int timeLeft = CurrentCountdownValue;
        int mult = Mathf.Max(1, timeRemainingScoreMultiplier);
        return timeLeft * mult;
    }

    public int ApplyExitTaxInstant()
    {
        if (scoreManager == null)
            return 0;

        int tax = CalculateExitTaxPoints();
        if (tax > 0)
            scoreManager.SubScore(tax);

        return tax;
    }

    public void PlayExitTaxVisualThen(Action onCompleted)
    {
        if (scoreManager == null)
        {
            onCompleted?.Invoke();
            return;
        }

        StartCoroutine(ExitTaxVisualRoutine(onCompleted));
    }

    private IEnumerator ExitTaxVisualRoutine(Action onCompleted)
    {
        int timeLeft = CurrentCountdownValue;
        int mult = Mathf.Max(1, timeRemainingScoreMultiplier);
        int step = Mathf.Max(1, transferStep);
        float tick = Mathf.Max(0.001f, transferTickSeconds);

        while (timeLeft > 0)
        {
            int delta = Mathf.Min(step, timeLeft);
            timeLeft -= delta;

            if (timeTxt != null)
                timeTxt.text = timeLeft.ToString("D4");

            scoreManager.SubScore(delta * mult);

            yield return new WaitForSecondsRealtime(tick);
        }

        _timer = startValue / decayPerSecond;

        if (timeTxt != null)
            timeTxt.text = "0000";

        onCompleted?.Invoke();
    }
}
