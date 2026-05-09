using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToNextLevel : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private LivesTextDisplay livesDisplay;

    public static int ResolveNextLevel(int currentLevel)
    {
        if (currentLevel < GameSession.BonusAfterLevel)
            return currentLevel + 1;

        if (currentLevel == GameSession.BonusAfterLevel)
            return 0;

        if (currentLevel < GameSession.FinalCampaignLevel)
            return currentLevel + 1;

        return 0;
    }

    public static string ResolveNextSceneName(int currentLevel)
    {
        int nextLevel = ResolveNextLevel(currentLevel);
        if (nextLevel > 0)
            return GameSession.GetSceneNameForLevel(nextLevel);

        if (currentLevel == GameSession.BonusAfterLevel)
            return GameSession.BonusSceneName;

        return GameSession.MenuSceneName;
    }

    public void ContinueRun()
    {
        int score = scoreManager != null ? scoreManager.Score : GameSession.CurrentScore;
        int lives = livesDisplay != null ? livesDisplay.CurrentLives : GameSession.CurrentLives;

        LoadNext(score, lives);
    }

    public static void LoadNext(int score, int lives)
    {
        Time.timeScale = 1f;
        ScoreManager.SetCanAddScore(false);

        int nextLevel = ResolveNextLevel(GameSession.CurrentLevel);
        if (nextLevel > 0)
        {
            GameSession.SetProgress(score, lives, nextLevel);
            SceneManager.LoadScene(GameSession.GetSceneNameForLevel(nextLevel));
            return;
        }

        if (GameSession.CurrentLevel == GameSession.BonusAfterLevel)
        {
            GameSession.SetProgress(score, lives, GameSession.BonusAfterLevel);
            SceneManager.LoadScene(GameSession.BonusSceneName);
            return;
        }

        GameSession.ResetSession();
        SceneManager.LoadScene(GameSession.MenuSceneName);
    }
}
