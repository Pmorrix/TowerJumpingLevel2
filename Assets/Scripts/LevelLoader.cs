using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private LivesTextDisplay livesDisplay;
    [SerializeField] public GameObject player;
    [SerializeField] public GameObject bonusGo;
    [SerializeField] public GameObject bonusTotalPanel;
    [SerializeField] public GameObject sellPanel;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip continueClip;
    [SerializeField, Range(0f, 3f)] private float continueVolume = 0.8f;
    [SerializeField] private float continueLoadDelay = 0.08f;
    [SerializeField] private PauseSimpleUI continueSfxSource;

    private bool _loadingNextLevel;

    private void Start()
    {
        if (bonusGo != null && bonusGo.activeInHierarchy)
            Time.timeScale = 0f;

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;

        GameAudio.ConfigureSfxSource(sfxSource);
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        ScoreManager.SetCanAddScore(false);
        GameSession.ResetSession();
        SceneManager.LoadScene(GameSession.MenuSceneName);
    }

    public void LoadNextLevel()
    {
        if (_loadingNextLevel)
            return;

        int score = scoreManager != null ? scoreManager.Score : GameSession.CurrentScore;
        int lives = livesDisplay != null ? livesDisplay.CurrentLives : GameSession.CurrentLives;

        EnsureSfxSource();

        _loadingNextLevel = true;
        StartCoroutine(LoadNextLevelAfterSfx(score, lives));
    }

    private IEnumerator LoadNextLevelAfterSfx(int score, int lives)
    {
        bool playedSfx = GameAudio.TryPlayContinueSfx(ref continueSfxSource, sfxSource, continueClip, continueVolume);

        if (playedSfx && continueLoadDelay > 0f)
            yield return new WaitForSecondsRealtime(continueLoadDelay);

        GoToNextLevel.LoadNext(score, lives);
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

    public void LoadBonusLevel1Go()
    {
        Time.timeScale = 1f;

        if (bonusGo != null)
            bonusGo.SetActive(false);

        if (player != null)
            player.SetActive(true);
    }

    public void LoadSellPanel()
    {
        if (sellPanel != null)
            sellPanel.SetActive(true);

        if (player != null)
            player.SetActive(false);
    }
}
