using UnityEngine;

public class VerticalWaveMotion : MonoBehaviour
{
    [Tooltip("Oscillation speed in cycles per second.")]
    [SerializeField] private float frequency = 1f;

    [Tooltip("Amplitude multiplier. Final amplitude = localScale.y * amplitudeMultiplier.")]
    [SerializeField] private float amplitudeMultiplier = 1f;

    [Tooltip("Optional phase offset in degrees.")]
    [SerializeField] private float phaseOffsetDegrees = 0f;

    private Vector3 startPosition;
    private float angularFrequency;
    private float phaseOffsetRadians;

    private void Awake()
    {
        startPosition = transform.position;
        angularFrequency = Mathf.Max(0f, frequency) * Mathf.PI * 2f;
        phaseOffsetRadians = phaseOffsetDegrees * Mathf.Deg2Rad;
    }

    private void Update()
    {
        float amplitude = transform.localScale.y * amplitudeMultiplier;
        float yOffset = Mathf.Sin(Time.time * angularFrequency + phaseOffsetRadians) * amplitude;
        transform.position = startPosition + new Vector3(0f, yOffset, 0f);
    }

    private void OnDisable()
    {
        transform.position = startPosition;
    }
}