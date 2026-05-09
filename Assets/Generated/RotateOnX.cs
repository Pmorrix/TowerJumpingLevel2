using UnityEngine;

public class RotateOnX : MonoBehaviour
{
    [Tooltip("Velocidad de rotación en grados por segundo sobre el eje X.")]
    [SerializeField] private float rotationSpeed = 90f;

    private void Update()
    {
        // Gira el objeto sobre sí mismo en el eje X.
        transform.Rotate(rotationSpeed * Time.deltaTime, 0f, 0f, Space.Self);
    }
}