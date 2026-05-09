using System;

public static class GameSession
{
    public const int DefaultScore = 0;
    public const int DefaultLives = 3;
    public const int FirstCampaignLevel = 1;
    public const int FinalCampaignLevel = 3;

    public const string MenuSceneName = "Menu";
    public const string Level1SceneName = "Scene Level 1";
    public const string Level2SceneName = "Scene Level 2";
    public const string Level3SceneName = "Scene Level 3";
    public const string BonusSceneName = "Scene Bonus 1";

    public static int CurrentScore { get; private set; } = DefaultScore;
    public static int CurrentLives { get; private set; } = DefaultLives;
    public static int CurrentLevel { get; private set; } = FirstCampaignLevel;
    public static bool HasActiveRun { get; private set; }

    public static void StartNewRun()
    {
        CurrentScore = DefaultScore;
        CurrentLives = DefaultLives;
        CurrentLevel = FirstCampaignLevel;
        HasActiveRun = true;
    }

    public static void ResetSession()
    {
        CurrentScore = DefaultScore;
        CurrentLives = DefaultLives;
        CurrentLevel = FirstCampaignLevel;
        HasActiveRun = false;
    }

    public static void SetProgress(int score, int lives, int level)
    {
        CurrentScore = Math.Max(0, score);
        CurrentLives = Math.Max(0, lives);
        CurrentLevel = ClampCampaignLevel(level);
        HasActiveRun = true;
    }

    public static void EnsureSceneContext(int fallbackLevel)
    {
        int safeLevel = ClampCampaignLevel(fallbackLevel);

        if (!HasActiveRun)
        {
            CurrentScore = DefaultScore;
            CurrentLives = DefaultLives;
            CurrentLevel = safeLevel;
            HasActiveRun = true;
            return;
        }

        CurrentLevel = safeLevel;
        CurrentScore = Math.Max(0, CurrentScore);
        CurrentLives = Math.Max(0, CurrentLives);
    }

    public static void ApplyToScene(ScoreManager scoreManager, LivesTextDisplay livesDisplay, int fallbackLevel)
    {
        EnsureSceneContext(fallbackLevel);

        if (scoreManager != null)
            scoreManager.SetScore(CurrentScore);

        if (livesDisplay != null)
            livesDisplay.SetLives(CurrentLives);
    }

    public static string GetSceneNameForLevel(int level)
    {
        switch (ClampCampaignLevel(level))
        {
            case 1:
                return Level1SceneName;
            case 2:
                return Level2SceneName;
            case 3:
                return Level3SceneName;
            default:
                return Level1SceneName;
        }
    }

    private static int ClampCampaignLevel(int level)
    {
        if (level < FirstCampaignLevel)
            return FirstCampaignLevel;

        if (level > FinalCampaignLevel)
            return FinalCampaignLevel;

        return level;
    }
}
