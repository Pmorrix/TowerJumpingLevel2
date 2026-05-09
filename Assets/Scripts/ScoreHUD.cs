using TMPro;
using UnityEngine;

public class ScoreHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private ScoreManager scoreManager;

    private void Awake()
    {
        if (scoreManager == null)
            scoreManager = FindAnyObjectByType<ScoreManager>();
    }

    private void OnEnable()
    {
        if (scoreManager == null) return;

        scoreManager.OnScoreChanged += HandleScoreChanged;
     //   scoreManager.OnComboChanged += HandleComboChanged;
    }

    private void OnDisable()
    {
        if (scoreManager == null) return;

        scoreManager.OnScoreChanged -= HandleScoreChanged;
     //   scoreManager.OnComboChanged -= HandleComboChanged;
    }

    private void Start()
    {
        // Refresco inicial
        if (scoreManager != null)
        {
            HandleScoreChanged(scoreManager.Score);
           // HandleComboChanged(scoreManager.Combo);
        }
    }

    private void HandleScoreChanged(int s)
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {s:000000}";
    }

    //private void HandleComboChanged(int c)
    //{
    //    if (comboText != null)
    //        comboText.text = $"COMBO: {c:000000}";
    //}
}
