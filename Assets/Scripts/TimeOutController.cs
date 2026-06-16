using UnityEngine;

public class TimeOutController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PhaseManager phaseManager;
    [SerializeField] private LivesTextDisplay livesDisplay;
    [SerializeField] private PlayerRespawnOnFloor playerRespawn;
    [SerializeField] private GameObject timeOutPanel;
    [SerializeField] private GameOverController gameOverController;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private PauseSimpleUI continueSfxSource;

    private bool _handled;

    // 🔹 SUSCRIPCIÓN AL EVENTO
    private void OnEnable()
    {
        if (phaseManager != null)
            phaseManager.OnTimeUp += HandleTimeOut;

        if (livesDisplay != null)
            livesDisplay.OnLivesDepleted += HandleLivesDepleted;
    }

    private void OnDisable()
    {
        if (phaseManager != null)
            phaseManager.OnTimeUp -= HandleTimeOut;

        if (livesDisplay != null)
            livesDisplay.OnLivesDepleted -= HandleLivesDepleted;
    }

    /// <summary>
    /// Se ejecuta cuando el Exit TAX llega a 0
    /// </summary>
    private void HandleTimeOut()
    {
        if (_handled) return;

        if (livesDisplay != null && livesDisplay.CurrentLives <= 0)
        {
            _handled = true;

            if (timeOutPanel != null)
                timeOutPanel.SetActive(false);

            if (gameOverController != null)
                gameOverController.TriggerGameOver();

            return;
        }

        _handled = true;

        // Congelar juego
        Time.timeScale = 0f;

        StopMusic();

        // Mostrar panel
        if (timeOutPanel != null)
        {
            timeOutPanel.SetActive(true);
            timeOutPanel.transform.SetAsLastSibling();
        }
    }

    private void HandleLivesDepleted()
    {
        _handled = true;

        if (timeOutPanel != null)
            timeOutPanel.SetActive(false);

        if (gameOverController != null)
            gameOverController.TriggerGameOver();
    }

    /// <summary>
    /// Botón CONTINUE del TimeOutPanel
    /// </summary>
    public void OnContinue()
    {
        GameAudio.TryPlayContinueSfx(ref continueSfxSource);

        // Reanudar tiempo
        Time.timeScale = 1f;

        // Perder una vida
        if (livesDisplay != null)
            livesDisplay.LoseLife();

        // Cerrar panel
        if (timeOutPanel != null)
            timeOutPanel.SetActive(false);

        // Si quedan vidas → resetear fase
        if (livesDisplay != null && livesDisplay.CurrentLives > 0)
        {
            _handled = false;

            if (phaseManager != null)
                phaseManager.ResetPhase();

            RespawnPlayerAtStart();
            ResumeMusicIfNeeded();
        }
        else
        {
            // Última vida → Game Over
            if (gameOverController != null)
                gameOverController.TriggerGameOver();
        }
    }

    private void StopMusic()
    {
        if (musicSource != null)
        {
            GameAudio.StopMusic(musicSource);
            return;
        }

        GameAudio.StopAllMusic();
    }

    private void RespawnPlayerAtStart()
    {
        if (playerRespawn == null)
            playerRespawn = FindAnyObjectByType<PlayerRespawnOnFloor>();

        if (playerRespawn != null)
            playerRespawn.RespawnAtDropPoint();
    }

    private void ResumeMusicIfNeeded()
    {
        if (musicSource != null)
        {
            GameAudio.ApplyMusicEnabled(musicSource);
            return;
        }

        GameAudio.RouteSceneAudioSources();
    }
}
