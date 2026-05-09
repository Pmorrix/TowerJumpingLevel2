using System;
using UnityEngine;
using TMPro;

public class LivesTextDisplay : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text livesText;

    [Header("Config")]
    [SerializeField] private int startingLives = 3;

    [Header("Score")]
    [SerializeField] private ScoreManager scoreManager; // 👈 referencia directa

    public event Action OnLivesDepleted;

    private int currentLives;
    public int CurrentLives => currentLives;

    // ─────────────────────────────────────────────
    // SCORE POR VIDA
    // ─────────────────────────────────────────────
    private int scoreAtLifeStart;

    private void Awake()
    {
        currentLives = Mathf.Max(0, startingLives);

        if (livesText == null)
            livesText = GetComponent<TMP_Text>();

        UpdateLivesText();

        MarkLifeStart();

        if (currentLives == 0)
            OnLivesDepleted?.Invoke();
    }

    // ─────────────────────────────────────────────
    // VIDA
    // ─────────────────────────────────────────────
    public void SetLives(int lives)
    {
        currentLives = Mathf.Max(0, lives);
        UpdateLivesText();

        MarkLifeStart();

        if (currentLives == 0)
            OnLivesDepleted?.Invoke();
    }

    public void LoseLife()
    {
        if (currentLives <= 0)
            return;

        currentLives = Mathf.Max(0, currentLives - 1);
        UpdateLivesText();

        if (currentLives == 0)
            OnLivesDepleted?.Invoke();
        else
            MarkLifeStart();
    }

    public void GainLife()
    {
        currentLives += 1;
        UpdateLivesText();
        MarkLifeStart();
    }

    // ─────────────────────────────────────────────
    // SCORE POR VIDA
    // ─────────────────────────────────────────────

    /// <summary>
    /// Marca el inicio de una vida (snapshot del score actual).
    /// </summary>
    public void MarkLifeStart()
    {
        if (scoreManager == null)
            return;

        scoreAtLifeStart = scoreManager.Score;
    }

    /// <summary>
    /// Devuelve cuántos puntos se han ganado en esta vida
    /// y deja preparado el marcador para la siguiente.
    /// </summary>
    public int ConsumeScoreGainedThisLife()
    {
        if (scoreManager == null)
            return 0;

        int currentScore = scoreManager.Score;
        int delta = Mathf.Max(0, currentScore - scoreAtLifeStart);

        scoreAtLifeStart = currentScore;
        return delta;
    }

    // ─────────────────────────────────────────────
    // UI
    // ─────────────────────────────────────────────
    private void UpdateLivesText()
    {
        if (livesText == null)
            return;

        livesText.text = $"Lives: {currentLives}";
    }
}
