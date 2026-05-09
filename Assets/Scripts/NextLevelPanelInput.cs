using UnityEngine;

public sealed class NextLevelPanelInput : MonoBehaviour
{
    [SerializeField] private LevelLoader levelLoader;

    private bool _isActive;

    private void OnEnable()
    {
        _isActive = true;
    }

    private void OnDisable()
    {
        _isActive = false;
    }

    private void Update()
    {
        if (!_isActive)
            return;

        if (Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Space))
        {
            Continue();
        }
    }

    public void Continue()
    {
        if (levelLoader != null)
            levelLoader.LoadNextLevel();
    }
}
