using System.Collections;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class BuildingDelayedPulse : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Target Material")]
    [SerializeField] private int materialIndex = 1;

    [Header("Shader Property")]
    [SerializeField] private string colorProperty = "_BaseColor";

    [Header("Pulse")]
    [SerializeField] private float delayBeforeStart = 5f;

    [ColorUsage(true, true)]
    [SerializeField] private Color targetColor = Color.green;

    [Min(0.01f)]
    [SerializeField] private float pulseSpeed = 2f;

    [Header("Debug")]
    [SerializeField] private bool autoApplyInEditor = true;

    private MaterialPropertyBlock _block;
    private Color _baseColor = Color.white;
    private bool _hasBaseColor;

    private Coroutine _delayRoutine;
    private bool _pulseStarted;

    private void Awake()
    {
        Init();
        CacheBaseColor();
        ApplyEditorPreview();
    }

    private void OnEnable()
    {
        Init();
        CacheBaseColor();
        ApplyEditorPreview();
    }

    private void OnValidate()
    {
        Init();
        CacheBaseColor();
        ApplyEditorPreview();
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        if (!_pulseStarted)
            return;

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        Color currentColor = Color.Lerp(_baseColor, targetColor, t);
        SetColor(currentColor);
    }

    public void TriggerDelayedPulse()
    {
        if (!Application.isPlaying)
            return;

        if (_pulseStarted)
            return;

        if (_delayRoutine != null)
            return;

        _delayRoutine = StartCoroutine(StartPulseAfterDelay());
    }

    private IEnumerator StartPulseAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeStart);

        _pulseStarted = true;
        _delayRoutine = null;
    }

    private void Init()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (_block == null)
            _block = new MaterialPropertyBlock();
    }

    private void CacheBaseColor()
    {
        if (targetRenderer == null)
            return;

        Material[] mats = targetRenderer.sharedMaterials;
        if (mats == null || materialIndex < 0 || materialIndex >= mats.Length)
            return;

        Material mat = mats[materialIndex];
        if (mat == null || !mat.HasProperty(colorProperty))
            return;

        _baseColor = mat.GetColor(colorProperty);
        _hasBaseColor = true;
    }

    private void ApplyEditorPreview()
    {
        if (Application.isPlaying)
            return;

        if (!autoApplyInEditor)
            return;

        if (_hasBaseColor)
            SetColor(_baseColor);
    }

    private void SetColor(Color color)
    {
        if (targetRenderer == null)
            return;

        Material[] mats = targetRenderer.sharedMaterials;
        if (mats == null || materialIndex < 0 || materialIndex >= mats.Length)
            return;

        Material mat = mats[materialIndex];
        if (mat == null || !mat.HasProperty(colorProperty))
            return;

        targetRenderer.GetPropertyBlock(_block, materialIndex);
        _block.SetColor(colorProperty, color);
        targetRenderer.SetPropertyBlock(_block, materialIndex);
    }
}