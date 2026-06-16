using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public sealed class MenuControllerUI : MonoBehaviour
{
    [Header("Options (Top -> Bottom)")]
    [SerializeField] private MenuOptionUI[] options;

    [Header("Visual")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color activeColor = Color.yellow;
    [SerializeField] private bool activeUsesAsterisks = true;

    [Header("Input")]
    [SerializeField] private KeyCode upKey = KeyCode.UpArrow;
    [SerializeField] private KeyCode downKey = KeyCode.DownArrow;
    [SerializeField] private KeyCode selectKey = KeyCode.Space;

    [Header("Selection Events (same order as options)")]
    [SerializeField] private UnityEvent[] onSelect;

    [Header("Start")]
    [SerializeField] private int startIndex = 0;

    [Header("Optional refs")]
    [SerializeField] private MenuPanelsUI panelsUI;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip selectClip;

    [SerializeField, Range(0f, 3f)] private float moveVolume = 1f;
    [SerializeField, Range(0f, 3f)] private float selectVolume = 1f;

    [SerializeField] private float sfxMinInterval = 0.05f;

    private int _index;
    private float _lastSfxTime;

    private void Awake()
    {
        _index = Mathf.Clamp(startIndex, 0, (options?.Length ?? 1) - 1);

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        GameAudio.ConfigureSfxSource(sfxSource);

        WireButtons();
        RefreshVisuals();
    }

    private void OnEnable()
    {
        RefreshVisuals();
    }

    private void Update()
    {
        if (panelsUI != null && panelsUI.IsAnyPanelOpen())
            return;

        if (options == null || options.Length == 0)
            return;

        if (Input.GetKeyDown(upKey))
            Move(-1);

        if (Input.GetKeyDown(downKey))
            Move(+1);

        if (Input.GetKeyDown(selectKey)|| Input.GetKeyDown(KeyCode.Space))
            Confirm();
    }

    private void WireButtons()
    {
        if (options == null)
            return;

        for (int i = 0; i < options.Length; i++)
        {
            int idx = i;
            MenuOptionUI opt = options[i];

            if (opt == null)
                continue;

            opt.Init(this, idx);
            // MenuOptionUI handles pointer clicks so mouse and keyboard use the same selection path.
            opt.SetButtonComponentEnabled(false);
        }
    }

    public void HoverSelect(int index)
    {
        if (panelsUI != null && panelsUI.IsAnyPanelOpen())
            return;

        if (index < 0 || index >= options.Length)
            return;

        SetIndex(index);
    }

    public void PointerClickSelect(int clickedIndex)
    {
        if (panelsUI != null && panelsUI.IsAnyPanelOpen())
            return;

        ExecuteOption(clickedIndex);
    }

    private void Confirm()
    {
        ExecuteOption(_index);
    }

    private void ExecuteOption(int index)
    {
        if (options == null || index < 0 || index >= options.Length)
            return;

        SetIndex(index);

        if (!TryPlaySelectSfxThroughPanels())
            PlaySelectSfx();

        if (TryInvokeOnSelect(index))
            return;

        if (TryInvokeBuiltInAction(index))
            return;
    }

    private bool TryInvokeOnSelect(int index)
    {
        if (onSelect == null || index >= onSelect.Length)
            return false;

        UnityEvent evt = onSelect[index];

        if (evt == null)
            return false;

        if (evt.GetPersistentEventCount() <= 0)
            return false;

        evt.Invoke();
        return true;
    }

    private bool TryInvokeBuiltInAction(int index)
    {
        if (panelsUI == null)
            return false;

        string key = NormalizeKey(options[index].OptionText);

        switch (key)
        {
            case "NEW GAME":
            case "START":
            case "PLAY":
                panelsUI.NewGame();
                return true;

            case "CONTROLS":
            case "CONTROLES":
            case "OPTIONS":
            case "OPCIONES":
                panelsUI.OpenControls();
                return true;

            case "CREDITS":
            case "CREDITOS":
                panelsUI.OpenCredits();
                return true;

            case "QUIT":
            case "EXIT":
            case "SALIR":
                panelsUI.QuitGame();
                return true;
        }

        return false;
    }

    private string NormalizeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        string normalized = value.Trim().ToUpperInvariant();

        normalized = RemoveDiacritics(normalized);
        normalized = normalized.Replace('*', ' ');
        normalized = normalized.Replace('_', ' ');
        normalized = normalized.Replace('-', ' ');

        while (normalized.Contains("  "))
            normalized = normalized.Replace("  ", " ");

        return normalized.Trim();
    }

    private string RemoveDiacritics(string text)
    {
        string normalized = text.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();

        foreach (char c in normalized)
        {
            UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);

            if (uc != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private void Move(int delta)
    {
        int count = options.Length;

        int next = (_index + delta) % count;

        if (next < 0)
            next += count;

        SetIndex(next);
    }

    private void SetIndex(int newIndex)
    {
        int clamped = Mathf.Clamp(newIndex, 0, options.Length - 1);

        if (clamped == _index)
            return;

        _index = clamped;

        if (panelsUI == null || !panelsUI.IsAnyPanelOpen())
            PlayMoveSfx();

        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        if (options == null)
            return;

        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] == null)
                continue;

            options[i].ApplyVisual(
                i == _index,
                normalColor,
                activeColor,
                activeUsesAsterisks
            );
        }
    }

    private void PlayMoveSfx()
    {
        if (moveClip == null || sfxSource == null)
            return;

        if (Time.unscaledTime - _lastSfxTime < sfxMinInterval)
            return;

        _lastSfxTime = Time.unscaledTime;

        GameAudio.PlaySfx(sfxSource, moveClip, moveVolume);
    }

    private void PlaySelectSfx()
    {
        if (selectClip == null || sfxSource == null)
            return;

        if (Time.unscaledTime - _lastSfxTime < sfxMinInterval)
            return;

        _lastSfxTime = Time.unscaledTime;

        GameAudio.PlaySfx(sfxSource, selectClip, selectVolume);
    }

    private bool TryPlaySelectSfxThroughPanels()
    {
        return panelsUI != null && panelsUI.TryPlayMenuOptionSfx();
    }

    public void PlayOptionSelectSfx()
    {
        PlaySelectSfx();
    }
}
