using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LivesTextDisplay livesDisplay;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Score / Tax")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private PhaseManager phaseManager;
    [SerializeField] private GameOverPanelUI gameOverPanelUI;

    [Header("Disable Player Control")]
    [SerializeField] private MonoBehaviour[] behavioursToDisable;

    [Header("HighScore")]
    [SerializeField] private HighScoreSystem highScoreSystem;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip continueClip;
    [SerializeField] private float continueVolume = 1f;
    [SerializeField] private float continueLoadDelay = 0.08f;
    [SerializeField] private PauseSimpleUI continueSfxSource;

    private bool _gameOver;
    private bool _taxApplied;
    private bool _loadingMenu;

    private void Awake()
    {
        EnsureSfxSource();
    }

    private void OnEnable()
    {
        if (livesDisplay != null)
            livesDisplay.OnLivesDepleted += OnLivesDepletedDelayed;
    }

    private void OnDisable()
    {
        if (livesDisplay != null)
            livesDisplay.OnLivesDepleted -= OnLivesDepletedDelayed;
    }

    private void OnLivesDepletedDelayed()
    {
        StartCoroutine(GameOverNextFrame());
    }

    private IEnumerator GameOverNextFrame()
    {
        yield return null;
        HandleGameOver();
    }

    private void HandleGameOver()
    {
        if (_gameOver) return;
        _gameOver = true;

        // Detener score
        ScoreManager.SetCanAddScore(false);

        // Desactivar control del player
        if (behavioursToDisable != null)
        {
            for (int i = 0; i < behavioursToDisable.Length; i++)
            {
                if (behavioursToDisable[i] != null)
                    behavioursToDisable[i].enabled = false;
            }
        }

        // Congelar juego (el TAX visual usa tiempo real dentro de PhaseManager)
        Time.timeScale = 0f;

        // Aplicar TAX visual (solo una vez) y luego mostrar panel
        if (!_taxApplied && phaseManager != null)
        {
            _taxApplied = true;

            phaseManager.PlayExitTaxVisualThen(() =>
            {
                int finalScore = (scoreManager != null) ? scoreManager.Score : 0;

                if (gameOverPanel != null)
                    gameOverPanel.SetActive(true);

                if (gameOverPanelUI != null)
                    gameOverPanelUI.Show(finalScore);

                // Si hay highscore, HighScoreSystem hará el swap de paneles
                if (highScoreSystem != null)
                    highScoreSystem.HandleGameOverFinalScore(finalScore);
                else
                    Debug.LogWarning("[GameOverController] highScoreSystem is NULL (Inspector).");
            });

            return;
        }

        // Fallback si no hay PhaseManager (no TAX)
        int fallbackScore = (scoreManager != null) ? scoreManager.Score : 0;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverPanelUI != null)
            gameOverPanelUI.Show(fallbackScore);

        if (highScoreSystem != null)
            highScoreSystem.HandleGameOverFinalScore(fallbackScore);
        else
            Debug.LogWarning("[GameOverController] highScoreSystem is NULL (Inspector).");
    }

    // Botón Continue → Menu
    public void Restart()
    {
        if (_loadingMenu)
            return;

        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        _loadingMenu = true;

        EnsureSfxSource();

        bool playedSfx = GameAudio.TryPlayContinueSfx(ref continueSfxSource, sfxSource, continueClip, Mathf.Clamp01(continueVolume));

        if (playedSfx && continueLoadDelay > 0f)
            yield return new WaitForSecondsRealtime(continueLoadDelay);

        Time.timeScale = 1f;
        ScoreManager.SetCanAddScore(false);
        GameSession.ResetSession();
        SceneManager.LoadScene(GameSession.MenuSceneName);
    }

    // Por si quieres dispararlo manualmente desde otros scripts
    public void TriggerGameOver()
    {
        HandleGameOver();
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
