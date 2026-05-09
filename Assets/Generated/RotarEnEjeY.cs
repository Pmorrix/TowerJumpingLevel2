using UnityEngine;

/// <summary>
/// Hace que el objeto gire sobre sí mismo en el eje Y.
/// </summary>
public class RotarEnEjeY : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Velocidad de rotación en grados por segundo sobre el eje Y.")]
    private float velocidadRotacion = 90f;

    private void Update()
    {
        // Rota el objeto sobre su propio eje Y usando el tiempo entre frames
        transform.Rotate(0f, velocidadRotacion * Time.deltaTime, 0f, Space.Self);
    }
}