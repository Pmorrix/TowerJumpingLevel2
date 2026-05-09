using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class BuildingPulse : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Target")]
    [SerializeField] private int materialIndex = 1;
    [SerializeField] private string colorProperty = "_BaseColor";

    [Header("Pulse")]
    [SerializeField] private bool pulseActive = true;

    [ColorUsage(true, true)]
    [SerializeField] private Color targetColor = Color.green;

    [Min(0.01f)]
    [SerializeField] private float speed = 2f;

    private MaterialPropertyBlock _block;
    private Color _startColor = Color.white;
    private bool _hasStartColor;

    private void Awake()
    {
        Init();
        CacheStartColor();
        ApplyCurrentColor();
    }

    private void OnEnable()
    {
        Init();
        CacheStartColor();
        ApplyCurrentColor();
    }

    private void OnValidate()
    {
        Init();
        CacheStartColor();
        ApplyCurrentColor();
    }

    private void Update()
    {
        if (targetRenderer == null || !pulseActive)
            return;

        float t = Mathf.PingPong(Time.time * speed, 1f);
        Color currentColor = Color.Lerp(_startColor, targetColor, t);
        SetColor(currentColor);
    }

    private void Init()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (_block == null)
            _block = new MaterialPropertyBlock();
    }

    private void CacheStartColor()
    {
        if (targetRenderer == null)
            return;

        Material[] mats = targetRenderer.sharedMaterials;
        if (mats == null || materialIndex < 0 || materialIndex >= mats.Length)
            return;

        Material mat = mats[materialIndex];
        if (mat == null || !mat.HasProperty(colorProperty))
            return;

        _startColor = mat.GetColor(colorProperty);
        _hasStartColor = true;
    }

    private void ApplyCurrentColor()
    {
        if (!_hasStartColor)
            return;

        if (!pulseActive)
        {
            SetColor(_startColor);
            return;
        }

        float t = Mathf.PingPong(Time.time * speed, 1f);
        Color currentColor = Color.Lerp(_startColor, targetColor, t);
        SetColor(currentColor);
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