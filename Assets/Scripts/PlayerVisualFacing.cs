using UnityEngine;

public class PlayerVisualFacing : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform visualRoot;

    [Header("Angles")]
    [SerializeField] private float frontY = 0f;
    [SerializeField] private float backY = 180f;
    [SerializeField] private float leftY = 90f;
    [SerializeField] private float rightY = -90f;

    [Header("Input")]
    [SerializeField] private float deadZone = 0.01f;

    private float _currentY;

    private void Awake()
    {
        ResetFacingToFront();
    }

    private void OnEnable()
    {
        ResetFacingToFront();
    }

    private void Update()
    {
        if (visualRoot == null)
            return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Prioridad: lateral > vertical
        if (horizontal < -deadZone)
        {
            _currentY = leftY;
            ApplyRotation(_currentY);
        }
        else if (horizontal > deadZone)
        {
            _currentY = rightY;
            ApplyRotation(_currentY);
        }
        else if (vertical > deadZone)
        {
            _currentY = backY;
            ApplyRotation(_currentY);
        }
        else if (vertical < -deadZone)
        {
            _currentY = frontY;
            ApplyRotation(_currentY);
        }
        // Si no hay input, mantiene la última orientación.
    }

    public void ResetFacingToFront()
    {
        _currentY = frontY;
        ApplyRotation(_currentY);
    }

    public void FaceLeft()
    {
        _currentY = leftY;
        ApplyRotation(_currentY);
    }

    public void FaceRight()
    {
        _currentY = rightY;
        ApplyRotation(_currentY);
    }

    public void FaceForward()
    {
        _currentY = frontY;
        ApplyRotation(_currentY);
    }

    public void FaceBack()
    {
        _currentY = backY;
        ApplyRotation(_currentY);
    }
    private void ApplyRotation(float yAngle)
    {
        if (visualRoot == null)
            return;

        Vector3 euler = visualRoot.localEulerAngles;
        euler.y = yAngle;
        visualRoot.localEulerAngles = euler;
    }
}