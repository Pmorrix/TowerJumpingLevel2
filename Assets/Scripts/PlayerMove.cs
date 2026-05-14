using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    [Header("Horizontal Move")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Lanes (Z positions)")]
    [Tooltip("Posiciones en Z de cada carril. Ej: -2, 0, 2")]
    [SerializeField] public float[] laneZPositions = new float[] { -2f, 0f, 2f };

    [Tooltip("Carril inicial (índice dentro de laneZPositions).")]
    [SerializeField] private int startLaneIndex = 1;

    /*
    [Header("Lane Smooth")]
    [Tooltip("Tiempo de suavizado para llegar al carril objetivo (segundos).")]
    [SerializeField] private float laneSmoothTime = 0.08f;

    [Tooltip("Velocidad máxima en Z durante el cambio de carril.")]
    [SerializeField] private float laneMaxSpeed = 30f;
    */

    private Rigidbody _rb;
    private int _laneIndex;
    private float _targetLaneZ;
    // private float _zSmoothVelocity;
    private float _xInput;
    private float _mobileHorizontalRaw;
    private bool _hasMobileHorizontalInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        if (laneZPositions == null || laneZPositions.Length == 0)
            laneZPositions = new float[] { 0f };

        _laneIndex = Mathf.Clamp(startLaneIndex, 0, laneZPositions.Length - 1);
        _targetLaneZ = laneZPositions[_laneIndex];

        Vector3 p = _rb.position;
        p.z = _targetLaneZ;
        _rb.position = p;
    }

    private void Update()
    {
        // Input horizontal (X)
        _xInput = _hasMobileHorizontalInput
            ? _mobileHorizontalRaw
            : -Input.GetAxisRaw("Horizontal");

        // Cambio de carril por pulsación
        if (Input.GetKeyDown(KeyCode.UpArrow))
            ChangeLane(-1);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            ChangeLane(+1);
    }

    public void MobileMoveLeft()
    {
        _mobileHorizontalRaw = 1f;
        _hasMobileHorizontalInput = true;
    }

    public void MobileMoveRight()
    {
        _mobileHorizontalRaw = -1f;
        _hasMobileHorizontalInput = true;
    }

    public void MobileStopHorizontal()
    {
        _mobileHorizontalRaw = 0f;
        _hasMobileHorizontalInput = false;
    }

    public void MobileLaneForward()
    {
        ChangeLane(-1);
    }

    public void MobileLaneBack()
    {
        ChangeLane(+1);
    }

    private void FixedUpdate()
    {
        // Movimiento horizontal físico (X)
        Vector3 velocity = _rb.linearVelocity;
        velocity.x = _xInput * moveSpeed;
        velocity.z = 0f; // evitar deriva lateral en Z
        _rb.linearVelocity = velocity;

        /*
        // Suavizado de carril (Z)
        float currentZ = _rb.position.z;
        float nextZ = Mathf.SmoothDamp(
            currentZ,
            _targetLaneZ,
            ref _zSmoothVelocity,
            laneSmoothTime,
            laneMaxSpeed,
            Time.fixedDeltaTime
        );

        Vector3 nextPosition = _rb.position;
        nextPosition.z = nextZ;
        _rb.MovePosition(nextPosition);
        */
    }

    private void ChangeLane(int delta)
    {
        if (laneZPositions == null || laneZPositions.Length == 0)
            return;

        _laneIndex = Mathf.Clamp(_laneIndex + delta, 0, laneZPositions.Length - 1);
        _targetLaneZ = laneZPositions[_laneIndex];

        Vector3 p = _rb.position;
        p.z = _targetLaneZ;
        _rb.position = p;
    }
}
