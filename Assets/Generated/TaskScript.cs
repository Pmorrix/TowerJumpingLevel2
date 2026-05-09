using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Replace the class summary once you provide the task details.
/// </summary>
public class TaskScript : MonoBehaviour
{
    [Header("Example Settings")]
    [Tooltip("Example exposed float setting.")]
    [SerializeField] private float exampleValue = 1f;

    [Header("Example Events")]
    [Tooltip("Example event invoked by this script.")]
    [SerializeField] private UnityEvent onExampleEvent;

    private void Awake()
    {
        // Called when the script instance is being loaded.
        // Initialize references and internal state here.
    }

    private void Start()
    {
        // Called before the first frame update.
        // Use this for initialization that depends on other objects being initialized.
    }

    private void Update()
    {
        // Called once per frame.
        // Put per-frame logic here (input, timers, movement, etc.).
    }

    // Example public method you can call from other scripts or UnityEvents.
    public void TriggerExample()
    {
        // Demonstrates how to use an exposed value and event.
        if (exampleValue > 0f)
        {
            onExampleEvent?.Invoke();
        }
    }
}