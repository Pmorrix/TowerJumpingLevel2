using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class BlinkingText : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Optional explicit TextMeshProUGUI reference. If left null, the script will try to find a TextMeshProUGUI on the same GameObject.")]
    [SerializeField] private TMPro.TextMeshProUGUI targetText;

    [Header("Blink Settings")]
    [Tooltip("Seconds the text stays visible.")]
    [Min(0f)]
    [SerializeField] private float onDuration = 0.5f;

    [Tooltip("Seconds the text stays hidden.")]
    [Min(0f)]
    [SerializeField] private float offDuration = 0.5f;

    [Tooltip("If true, blinking begins automatically in OnEnable.")]
    [SerializeField] private bool playOnEnable = true;

    [Tooltip("If true, the text starts in the visible state; otherwise starts hidden.")]
    [SerializeField] private bool startVisible = true;

    private float timer;
    private bool isVisible;
    private bool isPlaying;

    private void Reset()
    {
        // Auto-assign the TextMeshProUGUI component when the script is added.
        targetText = GetComponent<TextMeshProUGUI>();
    }

    private void Awake()
    {
        // If not assigned in the inspector, try to get it from the same GameObject.
        if (targetText == null)
            targetText = GetComponent<TextMeshProUGUI>();

        // Initialize state.
        isVisible = startVisible;
        ApplyVisibility(isVisible);
    }

    private void OnEnable()
    {
        // Optionally start blinking when enabled.
        if (playOnEnable)
            Play();
        else
            Stop(); // Ensures stable state when not playing.
    }

    private void Update()
    {
        // If not playing or no target assigned, do nothing.
        if (!isPlaying || targetText == null)
            return;

        // Guard against zero durations to avoid division-by-zero style behavior.
        float currentDuration = isVisible ? onDuration : offDuration;
        if (currentDuration <= 0f)
        {
            // If duration is zero, toggle every frame (fast blink).
            Toggle();
            return;
        }

        // Advance timer.
        timer += Time.unscaledDeltaTime; // Unscaled so it still blinks when timescale is 0 (e.g., paused).
        if (timer >= currentDuration)
        {
            timer = 0f;
            Toggle();
        }
    }

    /// <summary>
    /// Start blinking from the current visible/hidden state.
    /// </summary>
    public void Play()
    {
        isPlaying = true;
        timer = 0f;
        // Ensure current state is applied immediately.
        ApplyVisibility(isVisible);
    }

    /// <summary>
    /// Stop blinking and keep the current state.
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
        timer = 0f;
        ApplyVisibility(isVisible);
    }

    /// <summary>
    /// Stop blinking and force a visible state.
    /// </summary>
    public void StopAndShow()
    {
        isPlaying = false;
        timer = 0f;
        isVisible = true;
        ApplyVisibility(true);
    }

    /// <summary>
    /// Stop blinking and force a hidden state.
    /// </summary>
    public void StopAndHide()
    {
        isPlaying = false;
        timer = 0f;
        isVisible = false;
        ApplyVisibility(false);
    }

    private void Toggle()
    {
        isVisible = !isVisible;
        ApplyVisibility(isVisible);
    }

    private void ApplyVisibility(bool visible)
    {
        // If there's no Text component, nothing to apply.
        if (targetText == null)
            return;

        // Simplest approach: enable/disable the Text component.
        // Alternatively, you could fade alpha; this keeps it straightforward.
        targetText.enabled = visible;
    }
}