using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] private float baseScorePerSecond = 10f;
    [SerializeField] private int boosterBonus = 100;

    [Header("Temporary Score Multiplier")]
    [SerializeField] private float scoreMultiplier = 2f;

    public int Score { get; private set; }

    public event Action<int> OnScoreChanged;

    private float _scoreFloat;
    private int _lastScoreNotified = -1;

    private float _originalBaseScorePerSecond;
    private bool _scoreMultiplierActive;

    public static bool canAddScore = true;

    private void Awake()
    {
        _originalBaseScorePerSecond = baseScorePerSecond;
        SetCanAddScore(false);
    }

    private void Update()
    {
        if (!canAddScore) return;

        _scoreFloat += baseScorePerSecond * Time.deltaTime;
        int newScore = Mathf.FloorToInt(_scoreFloat);

        if (newScore != Score)
        {
            Score = newScore;
            NotifyScoreIfNeeded();
        }
    }

    public void OnBoosterUsed()
    {
        _scoreFloat += boosterBonus;

        int newScore = Mathf.FloorToInt(_scoreFloat);
        if (newScore != Score)
        {
            Score = newScore;
            NotifyScoreIfNeeded();
        }
    }

    public void OnFail()
    {
    }

    private void NotifyScoreIfNeeded()
    {
        if (Score == _lastScoreNotified) return;

        _lastScoreNotified = Score;
        OnScoreChanged?.Invoke(Score);
    }

    public static void SetCanAddScore(bool value)
    {
        canAddScore = value;
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;

        _scoreFloat += amount;

        int newScore = Mathf.FloorToInt(_scoreFloat);
        if (newScore != Score)
        {
            Score = newScore;
            NotifyScoreIfNeeded();
        }
    }

    public void SubScore(int amount)
    {
        if (amount < 0) return;

        _scoreFloat -= amount;

        if (_scoreFloat < 0f)
            _scoreFloat = 0f;

        int newScore = Mathf.FloorToInt(_scoreFloat);

        if (newScore != Score)
        {
            Score = newScore;
            NotifyScoreIfNeeded();
        }
    }

    public void SetScore(int value)
    {
        int v = Mathf.Max(0, value);

        _scoreFloat = v;
        Score = v;

        _lastScoreNotified = Score;
        OnScoreChanged?.Invoke(Score);
    }

    public void ActivateScoreMultiplier()
    {
        if (_scoreMultiplierActive)
            return;

        baseScorePerSecond = _originalBaseScorePerSecond * scoreMultiplier;
        _scoreMultiplierActive = true;
    }

    public void DeactivateScoreMultiplier()
    {
        if (!_scoreMultiplierActive)
            return;

        baseScorePerSecond = _originalBaseScorePerSecond;
        _scoreMultiplierActive = false;
    }
}
