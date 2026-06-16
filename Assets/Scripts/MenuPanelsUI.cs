#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MenuPanelsUI : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("Root de opciones (suele ser 'Options' o 'MainMenuRoot' según tu escena)")]
    [SerializeField] private GameObject menuOptionsRoot;
    [SerializeField] private GameObject menuHeaderRoot;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject playerToHide;

    [Header("Panel Buttons")]
    [SerializeField] private Button controlsCloseButton;
    [SerializeField] private Button creditsCloseButton;

    [Header("Menu Option SFX")]
    [SerializeField] private MenuControllerUI menuOptionSfxSource;
    [SerializeField] private AudioClip menuOptionClip;
    [SerializeField, Range(0f, 3f)] private float menuOptionVolume = 1f;

    [Header("Continue SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip continueClip;
    [SerializeField, Range(0f, 3f)] private float continueVolume = 1f;

    [Header("Music Toggle")]
    [SerializeField] private GameObject musicToggleRoot;
    [SerializeField] private string menuHeaderName = "Header";
    [SerializeField] private string musicToggleName = "MusicToggle";
    [SerializeField] private float musicToggleRevealDelay = 1.15f;
    [SerializeField] private bool forceMusicOnWhenEnteringMenu = true;

    private Coroutine _musicToggleRevealRoutine;
    private float _lastMenuOptionSfxTime;
    private bool _playerWasActiveBeforePanel;
    private bool _playerHiddenByPanel;

    private void Awake()
    {
        if (forceMusicOnWhenEnteringMenu)
            GameAudio.SetMusicEnabled(true);

        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (menuOptionSfxSource == null)
            menuOptionSfxSource = FindAnyObjectByType<MenuControllerUI>();

        EnsureSfxSource();
        PrepareMusicToggleReveal();

        if (controlsCloseButton != null)
        {
            controlsCloseButton.onClick.RemoveAllListeners();
            controlsCloseButton.onClick.AddListener(ClosePanels);
        }

        if (creditsCloseButton != null)
        {
            creditsCloseButton.onClick.RemoveAllListeners();
            creditsCloseButton.onClick.AddListener(ClosePanels);
        }
    }

    private void Update()
    {
        bool escapePressed = Input.GetKeyDown(KeyCode.Escape);
        bool closeKeyPressed =
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Space);

        if (IsAnyPanelOpen())
        {
            if (escapePressed || closeKeyPressed)
                ClosePanels();

            return;
        }

        if (escapePressed)
            QuitGame();
    }

    public void NewGame()
    {
        PlayMenuOptionSfx();
        Time.timeScale = 1f;
        ScoreManager.SetCanAddScore(false);
        GameSession.StartNewRun();
        SceneManager.LoadScene(GameSession.GetSceneNameForLevel(GameSession.FirstCampaignLevel));
    }

    public void OpenControls()
    {
        PlayMenuOptionSfx();
        ShowOnly(controlsPanel);
    }

    public void OpenCredits()
    {
        PlayMenuOptionSfx();
        ShowOnly(creditsPanel);
    }

    public void ClosePanels()
    {
        PlayContinueSfx();

        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        if (menuOptionsRoot != null)
            menuOptionsRoot.SetActive(true);

        SetMenuHeaderVisible(true);
        RestorePlayerVisibility();
        ScheduleMusicToggleReveal();
    }

    public void QuitGame()
    {

        Debug.Log("Quit Game requested.");
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowOnly(GameObject panelToShow)
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        if (menuOptionsRoot != null)
            menuOptionsRoot.SetActive(false);

        SetMenuHeaderVisible(false);
        StopMusicToggleReveal();
        SetMusicToggleVisible(false);
        HidePlayerForPanel();

        if (panelToShow != null)
            panelToShow.SetActive(true);
    }

    private void HidePlayerForPanel()
    {
        GameObject player = GetPlayerToHide();

        if (player == null)
            return;

        if (!_playerHiddenByPanel)
        {
            _playerWasActiveBeforePanel = player.activeSelf;
            _playerHiddenByPanel = true;
        }

        player.SetActive(false);
    }

    private void RestorePlayerVisibility()
    {
        if (!_playerHiddenByPanel)
            return;

        GameObject player = GetPlayerToHide();

        if (player != null)
            player.SetActive(_playerWasActiveBeforePanel);

        _playerHiddenByPanel = false;
    }

    private GameObject GetPlayerToHide()
    {
        if (playerToHide != null)
            return playerToHide;

        playerToHide = GameObject.Find("Player");

        if (playerToHide == null)
            playerToHide = GameObject.FindGameObjectWithTag("Player");

        return playerToHide;
    }

    private void PrepareMusicToggleReveal()
    {
        if (musicToggleRoot == null)
            musicToggleRoot = FindInactiveChildByName(transform.root, musicToggleName);

        if (musicToggleRoot == null)
            return;

        if (musicToggleRoot.GetComponent<MusicToggleButtonText>() == null)
            musicToggleRoot.AddComponent<MusicToggleButtonText>();

        musicToggleRoot.SetActive(false);

        ScheduleMusicToggleReveal();
    }

    private void ScheduleMusicToggleReveal()
    {
        StopMusicToggleReveal();

        _musicToggleRevealRoutine = StartCoroutine(RevealMusicToggleAfterDelay());
    }

    private void StopMusicToggleReveal()
    {
        if (_musicToggleRevealRoutine == null)
            return;

        StopCoroutine(_musicToggleRevealRoutine);
        _musicToggleRevealRoutine = null;
    }

    private IEnumerator RevealMusicToggleAfterDelay()
    {
        float delay = Mathf.Max(0f, musicToggleRevealDelay);

        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        SetMusicToggleVisible(!IsAnyPanelOpen());

        _musicToggleRevealRoutine = null;
    }

    private void SetMusicToggleVisible(bool visible)
    {
        if (musicToggleRoot != null)
            musicToggleRoot.SetActive(visible);
    }

    private void SetMenuHeaderVisible(bool visible)
    {
        if (menuHeaderRoot == null)
            menuHeaderRoot = FindInactiveChildByName(transform.root, menuHeaderName);

        if (menuHeaderRoot != null)
            menuHeaderRoot.SetActive(visible);
    }

    private GameObject FindInactiveChildByName(Transform root, string targetName)
    {
        if (root == null || string.IsNullOrWhiteSpace(targetName))
            return null;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name == targetName)
                return children[i].gameObject;
        }

        return null;
    }

    public bool IsAnyPanelOpen()
    {
        return (controlsPanel != null && controlsPanel.activeSelf)
            || (creditsPanel != null && creditsPanel.activeSelf);
    }

    private void PlayContinueSfx()
    {
        if (continueClip == null || sfxSource == null)
            return;

        GameAudio.PlaySfx(sfxSource, continueClip, Mathf.Clamp01(continueVolume));
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

    private void PlayMenuOptionSfx()
    {
        TryPlayMenuOptionSfx();
    }

    public bool TryPlayMenuOptionSfx()
    {
        if (menuOptionClip == null)
            return false;

        if (Time.unscaledTime - _lastMenuOptionSfxTime < 0.05f)
            return true;

        EnsureSfxSource();

        if (sfxSource == null)
            return false;

        _lastMenuOptionSfxTime = Time.unscaledTime;
        GameAudio.PlaySfx(sfxSource, menuOptionClip, Mathf.Clamp01(menuOptionVolume));
        return true;
    }
}
