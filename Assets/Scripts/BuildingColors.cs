using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class BuildingColors : MonoBehaviour
{
    [Header("Base Color Property")]
    [SerializeField] private string baseColorProperty = "_BaseColor";

    [Header("Renderer")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Material 0")]
    [ColorUsage(true, true)]
    [SerializeField] private Color color0 = Color.white;

    [Header("Material 1")]
    [ColorUsage(true, true)]
    [SerializeField] private Color color1 = Color.white;

    [Header("Material 2")]
    [ColorUsage(true, true)]
    [SerializeField] private Color color2 = Color.white;

    private readonly MaterialPropertyBlock[] _blocks = new MaterialPropertyBlock[3];

    private void Awake()
    {
        Init();
        ApplyColors();
    }

    private void Start()
    {
        Init();
        ApplyColors();
    }

    private void OnValidate()
    {
        Init();
        ApplyColors();
    }

    private void Init()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        for (int i = 0; i < _blocks.Length; i++)
        {
            if (_blocks[i] == null)
                _blocks[i] = new MaterialPropertyBlock();
        }
    }

    public void ApplyColors()
    {
        SetColor(0, color0);
        SetColor(1, color1);
        SetColor(2, color2);
    }

    private void SetColor(int materialIndex, Color color)
    {
        if (targetRenderer == null)
            return;

        Material[] mats = targetRenderer.sharedMaterials;
        if (mats == null || materialIndex < 0 || materialIndex >= mats.Length)
            return;

        if (mats[materialIndex] == null || !mats[materialIndex].HasProperty(baseColorProperty))
            return;

        targetRenderer.GetPropertyBlock(_blocks[materialIndex], materialIndex);
        _blocks[materialIndex].SetColor(baseColorProperty, color);
        targetRenderer.SetPropertyBlock(_blocks[materialIndex], materialIndex);
    }
}