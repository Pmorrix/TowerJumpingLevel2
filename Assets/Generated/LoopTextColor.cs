using System.Collections;
using UnityEngine;
using TMPro;

public class LoopTextColor : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Text component to change color. If left empty, will try to get it from this GameObject.")]
    [SerializeField] private TextMeshProUGUI targetText;

    [Header("Colors")]
    [Tooltip("Four colors to loop through. If size is not 4, script will still loop through available colors.")]
    [SerializeField] private Color[] colors = new Color[4]
    {
        Color.white,
        Color.red,
        Color.green,
        Color.blue
    };

    [Header("Timing")]
    [Tooltip("Seconds between color changes.")]
    [Min(0.01f)]
    [SerializeField] private float intervalSeconds = 0.5f;

    [Tooltip("If true, changes color instantly on start. If false, waits intervalSeconds before first change.")]
    [SerializeField] private bool changeImmediately = true;

    private Coroutine loopRoutine;
    private int index;

    private void Awake()
    {
        // Auto-assign Text if not set in inspector.
        if (targetText == null)
            targetText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        // Start looping when enabled.
        StartLoop();
    }

    private void OnDisable()
    {
        // Stop looping when disabled to avoid running coroutines on inactive objects.
        StopLoop();
    }

    private void StartLoop()
    {
        if (loopRoutine != null)
            StopCoroutine(loopRoutine);

        loopRoutine = StartCoroutine(ColorLoop());
    }

    private void StopLoop()
    {
        if (loopRoutine != null)
        {
            StopCoroutine(loopRoutine);
            loopRoutine = null;
        }
    }

    private IEnumerator ColorLoop()
    {
        // Safety checks.
        if (targetText == null || colors == null || colors.Length == 0)
            yield break;

        // Reset index so it always loops predictably.
        index = 0;

        // Optionally apply the first color immediately.
        if (changeImmediately)
        {
            targetText.color = colors[index];
            index = (index + 1) % colors.Length;
        }

        // Loop forever while this component is enabled.
        while (true)
        {
            yield return new WaitForSeconds(intervalSeconds);

            // Apply next color and advance index.
            targetText.color = colors[index];
            index = (index + 1) % colors.Length;
        }
    }
}