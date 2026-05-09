using UnityEngine;

public class BonusScoreRestore : MonoBehaviour
{
    [Header("Refs (optional, auto-resolve if null)")]
    [SerializeField] private ScoreManager scoreManager;

    [Header("Bonus Rules")]
    [Tooltip("En bonus, el tiempo no suma puntos.")]
    [SerializeField] private bool disableTimeScore = true;

    private void Awake()
    {
        if (scoreManager == null)
            scoreManager = FindAnyObjectByType<ScoreManager>();

        if (scoreManager == null)
        {
            Debug.LogWarning("[BonusScoreRestore] No se encontró ScoreManager en la escena bonus.");
            return;
        }

        scoreManager.SetScore(GameSession.CurrentScore);

        if (disableTimeScore)
            ScoreManager.SetCanAddScore(false);

        Debug.Log($"[BonusScoreRestore] Restored score={GameSession.CurrentScore}");
    }
}
