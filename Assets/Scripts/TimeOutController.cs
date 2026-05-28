using UnityEngine;

public class TimeOutController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private NewPhaseManager newPhaseManager;
    [SerializeField] private LivesTextDisplay livesDisplay;
    [SerializeField] private NewPlayerRespawnOnFloor playerRespawn;
    [SerializeField] private GameObject timeOutPanel;
    [SerializeField] private GameOverController gameOverController;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private PauseSimpleUI continueSfxSource;

    private bool _handled;

    // 🔹 SUSCRIPCIÓN AL EVENTO
    private void OnEnable()
    {
        if (newPhaseManager != null)
            newPhaseManager.OnTimeUp += HandleTimeOut;
    }

    private void OnDisable()
    {
        if (newPhaseManager != null)
            newPhaseManager.OnTimeUp -= HandleTimeOut;
    }

    /// <summary>
    /// Se ejecuta cuando el Exit TAX llega a 0
    /// </summary>
    private void HandleTimeOut()
    {
        if (_handled) return;
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

            if (newPhaseManager != null)
                newPhaseManager.ResetPhase();

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
            playerRespawn = FindAnyObjectByType<NewPlayerRespawnOnFloor>();

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
