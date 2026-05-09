using UnityEngine;

public class RotarCubo : MonoBehaviour
{
    [Tooltip("Velocidad de rotación en grados por segundo para cada eje (X, Y, Z).")]
    [SerializeField] private Vector3 velocidadRotacion = new Vector3(0f, 90f, 0f);

    [Tooltip("Si está activado, la rotación se aplica en el espacio local del objeto. Si no, en el espacio global.")]
    [SerializeField] private bool usarEspacioLocal = true;

    private void Update()
    {
        // DeltaTime asegura que la rotación sea consistente sin importar los FPS.
        Vector3 deltaRotacion = velocidadRotacion * Time.deltaTime;

        // Aplica la rotación según el espacio elegido.
        transform.Rotate(deltaRotacion, usarEspacioLocal ? Space.Self : Space.World);
    }
}