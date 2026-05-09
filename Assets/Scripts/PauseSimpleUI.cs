using UnityEngine;
using UnityEngine.UI;

public sealed class PauseSimpleUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;   // Panel raiz (desactivado por defecto)
    [SerializeField] private Button continueButton;   // Boton CONTINUE

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip pauseClip;
    [SerializeField] private AudioClip resumeClip;
    [SerializeField] private float sfxVolume = 1f;

    [Header("Player lock while paused")]
    [Tooltip("Opcional. Si esta vacio, busca scripts de input del Player en el objeto con tag Player.")]
    [SerializeField] private Behaviour[] playerBehavioursToDisable;

    private bool _paused;
    private bool _playerBehavioursLocked;
    private bool[] _playerBehaviourWasEnabled;

    private void Awake()
    {
        EnsureSfxSource();

        // Estado inicial consistente
        SetPaused(false);

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(Resume);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (_paused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        SetPaused(true);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    public void QuitGame()
    {
        RestorePlayerBehaviours();
        Time.timeScale = 1f;

        Debug.Log("Quit Game requested from pause menu.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void PlayContinueSfx()
    {
        TryPlayContinueSfx();
    }

    public bool TryPlayContinueSfx()
    {
        return PlayPauseSFX(false);
    }

    private void SetPaused(bool paused)
    {
        bool stateChanged = _paused != paused;
        _paused = paused;

        if (pausePanel != null)
            pausePanel.SetActive(paused);

        if (stateChanged)
            PlayPauseSFX(paused);

        Time.timeScale = paused ? 0f : 1f;

        if (paused)
            DisablePlayerBehaviours();
        else
            RestorePlayerBehaviours();

        // Opcional (PC): cursor visible cuando pausa
        if (paused)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnDisable()
    {
        // Seguridad: si desactivan este objeto en runtime, evita quedarte congelado.
        RestorePlayerBehaviours();
        Time.timeScale = 1f;
    }

    private void DisablePlayerBehaviours()
    {
        if (_playerBehavioursLocked)
            return;

        Behaviour[] behaviours = GetPlayerBehavioursToDisable();
        if (behaviours == null || behaviours.Length == 0)
            return;

        _playerBehaviourWasEnabled = new bool[behaviours.Length];

        for (int i = 0; i < behaviours.Length; i++)
        {
            Behaviour behaviour = behaviours[i];
            if (behaviour == null)
                continue;

            _playerBehaviourWasEnabled[i] = behaviour.enabled;
            behaviour.enabled = false;
        }

        _playerBehavioursLocked = true;
    }

    private void RestorePlayerBehaviours()
    {
        if (!_playerBehavioursLocked)
            return;

        Behaviour[] behaviours = GetPlayerBehavioursToDisable();
        if (behaviours != null && _playerBehaviourWasEnabled != null)
        {
            int count = Mathf.Min(behaviours.Length, _playerBehaviourWasEnabled.Length);
            for (int i = 0; i < count; i++)
            {
                if (behaviours[i] != null)
                    behaviours[i].enabled = _playerBehaviourWasEnabled[i];
            }
        }

        _playerBehaviourWasEnabled = null;
        _playerBehavioursLocked = false;
    }

    private Behaviour[] GetPlayerBehavioursToDisable()
    {
        if (playerBehavioursToDisable != null && playerBehavioursToDisable.Length > 0)
            return playerBehavioursToDisable;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return playerBehavioursToDisable;

        playerBehavioursToDisable = new Behaviour[]
        {
            player.GetComponent<PlayerMove>(),
            player.GetComponent<PlayerJump>(),
            player.GetComponent<PlayerVisualFacing>()
        };

        return playerBehavioursToDisable;
    }

    private bool PlayPauseSFX(bool paused)
    {
        AudioClip clip = paused ? pauseClip : resumeClip;

        if (clip == null || sfxSource == null)
            return false;

        GameAudio.PlaySfx(sfxSource, clip, Mathf.Clamp01(sfxVolume));
        return true;
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
