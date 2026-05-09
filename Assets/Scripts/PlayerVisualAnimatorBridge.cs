using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerVisualAnimatorBridge : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerJump playerJump;
    [SerializeField] private Animator visualAnimator;
    [SerializeField] private Rigidbody rb;

    [Header("Animator Params")]
    [SerializeField] private string groundedParam = "IsGrounded";
    [SerializeField] private string verticalSpeedParam = "VerticalSpeed";
    [SerializeField] private string jumpTriggerParam = "JumpTrigger";
    [SerializeField] private string moveXParam = "MoveX";

    [Header("Move")]
    [SerializeField] private float moveDeadZone = 0.05f;
    [SerializeField] private bool useAbsoluteHorizontalSpeed = true;

    private int _groundedHash;
    private int _verticalSpeedHash;
    private int _jumpTriggerHash;
    private int _moveXHash;

    private void Awake()
    {
        if (playerJump == null)
            playerJump = GetComponent<PlayerJump>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        _groundedHash = Animator.StringToHash(groundedParam);
        _verticalSpeedHash = Animator.StringToHash(verticalSpeedParam);
        _jumpTriggerHash = Animator.StringToHash(jumpTriggerParam);
        _moveXHash = Animator.StringToHash(moveXParam);
    }

    private void Reset()
    {
        playerJump = GetComponent<PlayerJump>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (playerJump == null || visualAnimator == null || rb == null)
            return;

        visualAnimator.SetBool(_groundedHash, playerJump.IsGroundedNow);
        visualAnimator.SetFloat(_verticalSpeedHash, playerJump.VerticalSpeed);

        float moveX = rb.linearVelocity.x;

        if (useAbsoluteHorizontalSpeed)
            moveX = Mathf.Abs(moveX);

        if (moveX < moveDeadZone)
            moveX = 0f;

        visualAnimator.SetFloat(_moveXHash, moveX);

        if (playerJump.ConsumeJumpStartedThisFrame())
            visualAnimator.SetTrigger(_jumpTriggerHash);
    }
}