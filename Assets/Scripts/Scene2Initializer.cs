using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene2Initializer : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private LivesTextDisplay livesDisplay;
    [SerializeField] private int fallbackLevel = 0;

    private void Awake()
    {
        int levelToApply = fallbackLevel > 0 ? fallbackLevel : ResolveLevelFromSceneName();
        GameSession.ApplyToScene(scoreManager, livesDisplay, levelToApply);
        ScoreManager.SetCanAddScore(false);
    }

    private int ResolveLevelFromSceneName()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case GameSession.Level1SceneName:
                return 1;
            case GameSession.Level2SceneName:
                return 2;
            case GameSession.Level3SceneName:
                return 3;
            default:
                return GameSession.FirstCampaignLevel;
        }
    }
}
