using System;
using System.Collections;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

[Serializable]
public class HighScoreData
{
    public string initials;
    public int score;
}

public sealed class HighScoreSystem : MonoBehaviour
{
    [Header("Panels (Canvas children)")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject highScorePanel;

    [Header("HighScore UI")]
    [SerializeField] private TMP_InputField initialsInput;
    [SerializeField] private TMP_Text highScoreScoreTxt;      // score en HighScorePanel

    // ✅ NUEVO: texto del HUD (Canvas/HUD/HighScore)
    [SerializeField] private TMP_Text hudHighScoreTxt;

    [Header("Config")]
    [SerializeField] private string fileName = "highscore.json";

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip confirmClip;
    [SerializeField] private float confirmVolume = 1f;
    [SerializeField] private float confirmLoadDelay = 0.08f;
    [SerializeField] private PauseSimpleUI continueSfxSource;

    private string _filePath;
    private HighScoreData _data;
    private bool _initialized;

    private int _pendingScore;
    private bool _loadingMenu;

    private void Awake()
    {
        EnsureSfxSource();
        EnsureInitialized();

        // ✅ NUEVO: pintar HUD al iniciar
        UpdateHudHighScore();

        // Estado por defecto: HighScorePanel apagado
        if (highScorePanel != null)
            highScorePanel.SetActive(false);
    }

    private void EnsureInitialized()
    {
        if (_initialized) return;

        _filePath = Path.Combine(Application.persistentDataPath, fileName);
        LoadOrCreate();
        _initialized = true;
    }

    /// <summary>
    /// Llamar desde GameOverController cuando el score final ya es definitivo (después del TAX).
    /// Si hay highscore, apaga GameOverPanel y enciende HighScorePanel.
    /// </summary>
    public void HandleGameOverFinalScore(int finalScore)
    {
        EnsureInitialized();

        if (_data == null)
        {
            Debug.LogWarning("[HighScore] _data es NULL. No se puede comparar.");
            return;
        }

        int jsonScore = _data.score;
        string jsonInitials = string.IsNullOrEmpty(_data.initials) ? "AAA" : _data.initials;

        bool isHighScore = finalScore > jsonScore;

        Debug.Log(Path.Combine(Application.persistentDataPath, "highscore.json"));

        Debug.Log("────────────────────────────────────────");
        Debug.Log($"[HighScore] Path: {_filePath}");
        Debug.Log($"[HighScore] JSON: {jsonInitials} {jsonScore}");
        Debug.Log($"[HighScore] FINAL SCORE: {finalScore}");
        Debug.Log(isHighScore ? "[HighScore] >>> HAY HIGHSCORE <<<" : "[HighScore] NO HAY HIGHSCORE");
        Debug.Log("────────────────────────────────────────");

        if (!isHighScore)
            return;

        // Swap de paneles
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        _pendingScore = finalScore;

        if (highScoreScoreTxt != null)
            highScoreScoreTxt.text = finalScore.ToString("D6");

        if (highScorePanel != null)
            highScorePanel.SetActive(true);

        if (initialsInput != null)
        {
            initialsInput.text = "";
            initialsInput.ActivateInputField();
        }
    }

    /// <summary>
    /// Guardar HS y volver al menú.
    /// </summary>
    public void ConfirmInitials()
    {
        if (_loadingMenu)
            return;

        StartCoroutine(ConfirmInitialsRoutine());
    }

    private IEnumerator ConfirmInitialsRoutine()
    {
        _loadingMenu = true;

        EnsureInitialized();
        if (_data == null)
        {
            _loadingMenu = false;
            yield break;
        }

        string initials = initialsInput != null ? initialsInput.text : "AAA";
        initials = Normalize3(initials);

        _data.initials = initials;
        _data.score = _pendingScore;

        SaveSafe();

        // ✅ NUEVO: actualizar HUD tras guardar (por si se usa en el menú o en otra escena)
        UpdateHudHighScore();

        EnsureSfxSource();

        bool playedSfx = GameAudio.TryPlayContinueSfx(ref continueSfxSource, sfxSource, confirmClip, Mathf.Clamp01(confirmVolume));

        if (playedSfx && confirmLoadDelay > 0f)
            yield return new WaitForSecondsRealtime(confirmLoadDelay);

        if (highScorePanel != null)
            highScorePanel.SetActive(false);

        Time.timeScale = 1f;
        ScoreManager.SetCanAddScore(false);
        GameSession.ResetSession();
        SceneManager.LoadScene(GameSession.MenuSceneName);
    }

    // ✅ NUEVO: pinta el HUD desde _data
    private void UpdateHudHighScore()
    {
        if (hudHighScoreTxt == null || _data == null)
            return;

        hudHighScoreTxt.text = $"HI-sc: {_data.initials} {_data.score:000000}";
    }

    private void LoadOrCreate()
    {
        if (!File.Exists(_filePath))
        {
            _data = new HighScoreData { initials = "AAA", score = 0 };
            SaveSafe();
            return;
        }

        try
        {
            string json = File.ReadAllText(_filePath);
            _data = JsonUtility.FromJson<HighScoreData>(json);

            if (_data == null)
                _data = new HighScoreData { initials = "AAA", score = 0 };

            if (string.IsNullOrEmpty(_data.initials))
                _data.initials = "AAA";
            else
                _data.initials = Normalize3(_data.initials);

            if (_data.score < 0)
                _data.score = 0;
        }
        catch
        {
            _data = new HighScoreData { initials = "AAA", score = 0 };
            SaveSafe();
        }
    }

    private void SaveSafe()
    {
        try
        {
            string json = JsonUtility.ToJson(_data, true);
            File.WriteAllText(_filePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[HighScoreSystem] Error saving highscore JSON: {e.Message}");
        }
    }

    private static string Normalize3(string s)
    {
        if (string.IsNullOrEmpty(s))
            return "AAA";

        s = s.ToUpperInvariant().Trim();

        char[] buf = new char[3] { 'A', 'A', 'A' };
        int w = 0;

        for (int i = 0; i < s.Length && w < 3; i++)
        {
            char c = s[i];
            if (c >= 'A' && c <= 'Z')
                buf[w++] = c;
        }

        return new string(buf);
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
