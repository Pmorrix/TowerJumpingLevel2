using UnityEngine;

public class PlayerTransformMovement : MonoBehaviour
{
    [Header("Movimiento")]

    [SerializeField]
    [Tooltip("Velocidad base de movimiento del jugador.")]
    private float moveSpeed = 5f;

    [SerializeField]
    [Tooltip("Multiplicador de velocidad al mantener presionada la tecla de correr.")]
    private float sprintMultiplier = 1.5f;

    [SerializeField]
    [Tooltip("Tecla usada para correr.")]
    private KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Rotación")]

    [SerializeField]
    [Tooltip("Velocidad con la que el jugador rota hacia la dirección de movimiento.")]
    private float rotationSpeed = 720f;

    [SerializeField]
    [Tooltip("Si está activado, el jugador rota hacia la dirección en la que se mueve.")]
    private bool rotateTowardsMovement = true;

    [Header("Referencia de dirección")]

    [SerializeField]
    [Tooltip("Si está activado, el movimiento se calcula relativo a la cámara principal.")]
    private bool moveRelativeToCamera = false;

    private void Update()
    {
        // Obtenemos la entrada horizontal y vertical usando el sistema clásico de Input.
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Creamos un vector de dirección en el plano XZ.
        Vector3 inputDirection = new Vector3(horizontalInput, 0f, verticalInput);

        // Normalizamos para evitar que el movimiento diagonal sea más rápido.
        if (inputDirection.sqrMagnitude > 1f)
        {
            inputDirection.Normalize();
        }

        // Si no hay entrada, no movemos ni rotamos al jugador.
        if (inputDirection.sqrMagnitude <= 0f)
        {
            return;
        }

        Vector3 movementDirection = inputDirection;

        // Si el movimiento es relativo a la cámara, convertimos la dirección según la orientación de la cámara.
        if (moveRelativeToCamera && Camera.main != null)
        {
            Transform cameraTransform = Camera.main.transform;

            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            // Eliminamos la inclinación vertical para mantener el movimiento en el suelo.
            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            movementDirection = cameraForward * inputDirection.z + cameraRight * inputDirection.x;
        }

        // Calculamos la velocidad final, aplicando sprint si corresponde.
        float currentSpeed = moveSpeed;

        if (Input.GetKey(sprintKey))
        {
            currentSpeed *= sprintMultiplier;
        }

        // Movemos el jugador directamente usando su Transform.
        transform.position += movementDirection.normalized * currentSpeed * Time.deltaTime;

        // Rotamos el jugador hacia la dirección de movimiento si está activado.
        if (rotateTowardsMovement)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection.normalized, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}