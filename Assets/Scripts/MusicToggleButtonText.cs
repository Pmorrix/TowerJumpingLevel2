using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicToggleButtonText : MonoBehaviour
{
    [SerializeField] private Button targetButton;
    [SerializeField] private TextMeshProUGUI buttonLabel;

    [Header("Text")]
    [SerializeField] private string onText = "MUSIC: ON";
    [SerializeField] private string offText = "MUSIC: OFF";

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;

    [Header("Pause SFX")]
    [SerializeField] private PauseSimpleUI pauseSfxSource;

    private bool isMusicOn = true;

    private void Awake()
    {
        if (targetButton == null)
            targetButton = GetComponent<Button>();

        if (buttonLabel == null)
            buttonLabel = GetComponentInChildren<TextMeshProUGUI>();

        if (musicSource == null)
            musicSource = FindMenuMusicSource();

        if (pauseSfxSource == null)
            pauseSfxSource = FindAnyObjectByType<PauseSimpleUI>();

        isMusicOn = GameAudio.IsMusicEnabled;
        GameAudio.ApplyMusicEnabled(musicSource);
        UpdateLabel();
    }

    private void OnEnable()
    {
        if (targetButton != null)
        {
            targetButton.onClick.RemoveListener(Toggle);
            targetButton.onClick.AddListener(Toggle);
        }

        isMusicOn = GameAudio.IsMusicEnabled;
        GameAudio.ApplyMusicEnabled(musicSource);
        UpdateLabel();
    }

    private void OnDisable()
    {
        if (targetButton != null)
            targetButton.onClick.RemoveListener(Toggle);
    }

    private void Toggle()
    {
        GameAudio.TryPlayContinueSfx(ref pauseSfxSource);

        isMusicOn = !isMusicOn;

        GameAudio.SetMusicEnabled(isMusicOn);
        GameAudio.ApplyMusicEnabled(musicSource);

        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (buttonLabel != null)
            buttonLabel.text = isMusicOn ? onText : offText;
    }

    private AudioSource FindMenuMusicSource()
    {
        GameObject musicGo = GameObject.Find("MenuMusic");
        if (musicGo == null)
            return null;

        AudioSource source = musicGo.GetComponent<AudioSource>();
        GameAudio.ConfigureMusicSource(source);
        return source;
    }
}
