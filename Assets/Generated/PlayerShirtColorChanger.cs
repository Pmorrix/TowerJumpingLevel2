using UnityEngine;

public class PlayerShirtColorChanger : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Renderer that contains the shirt material you want to recolor. Assign the player's shirt renderer here.")]
    [SerializeField] private Renderer shirtRenderer;

    [Tooltip("Index of the material slot used by the shirt on the renderer.")]
    [SerializeField] private int shirtMaterialIndex = 0;

    [Header("Color Settings")]
    [Tooltip("The color to apply to the shirt.")]
    [SerializeField] private Color shirtColor = Color.blue;

    [Tooltip("Apply the selected color automatically when the scene starts.")]
    [SerializeField] private bool applyOnStart = true;

    [Header("Shader Property")]
    [Tooltip("Name of the color property on the material. Common values are _Color or _BaseColor.")]
    [SerializeField] private string colorPropertyName = "_Color";

    // Stores a unique material instance so changing color does not affect shared assets.
    private Material shirtMaterialInstance;

    private void Awake()
    {
        // If no renderer is assigned manually, try to find one on this GameObject.
        if (shirtRenderer == null)
        {
            shirtRenderer = GetComponent<Renderer>();
        }

        // Create and cache a material instance for safe runtime editing.
        CacheMaterialInstance();
    }

    private void Start()
    {
        // Optionally apply the chosen color as soon as the game starts.
        if (applyOnStart)
        {
            ApplyShirtColor(shirtColor);
        }
    }

    private void OnValidate()
    {
        // Prevent negative material indexes in the inspector.
        if (shirtMaterialIndex < 0)
        {
            shirtMaterialIndex = 0;
        }

        // In play mode, reflect inspector changes immediately.
        if (Application.isPlaying && shirtMaterialInstance != null)
        {
            ApplyShirtColor(shirtColor);
        }
    }

    /// <summary>
    /// Changes the shirt color using the currently configured shader property name.
    /// </summary>
    /// <param name="newColor">The new shirt color.</param>
    public void ApplyShirtColor(Color newColor)
    {
        // Make sure we have a valid material instance before applying color.
        if (shirtMaterialInstance == null)
        {
            CacheMaterialInstance();
        }

        if (shirtMaterialInstance == null)
        {
            Debug.LogWarning("PlayerShirtColorChanger: No valid shirt material found.", this);
            return;
        }

        // Check whether the shader actually supports the requested color property.
        if (!shirtMaterialInstance.HasProperty(colorPropertyName))
        {
            Debug.LogWarning(
                $"PlayerShirtColorChanger: Material does not have a color property named '{colorPropertyName}'.",
                this
            );
            return;
        }

        // Save the color and apply it to the material.
        shirtColor = newColor;
        shirtMaterialInstance.SetColor(colorPropertyName, shirtColor);
    }

    /// <summary>
    /// Convenience method for UI buttons or other scripts.
    /// </summary>
    public void ApplyCurrentInspectorColor()
    {
        ApplyShirtColor(shirtColor);
    }

    /// <summary>
    /// Convenience overload for setting color with RGB values from 0 to 1.
    /// </summary>
    public void ApplyShirtColor(float r, float g, float b)
    {
        ApplyShirtColor(new Color(r, g, b, 1f));
    }

    // Finds the correct material and ensures we edit an instance, not the shared material asset.
    private void CacheMaterialInstance()
    {
        if (shirtRenderer == null)
        {
            return;
        }

        Material[] materials = shirtRenderer.materials;

        // Validate the chosen material slot.
        if (materials == null || materials.Length == 0 || shirtMaterialIndex >= materials.Length)
        {
            Debug.LogWarning("PlayerShirtColorChanger: Invalid shirt material index or no materials found.", this);
            return;
        }

        // Accessing renderer.materials gives instantiated runtime materials.
        shirtMaterialInstance = materials[shirtMaterialIndex];
    }
}