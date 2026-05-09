using System.Collections;
using UnityEngine;

public sealed class MenuJumpSim : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform rightTop;
    [SerializeField] private Transform leftTop;
    [SerializeField] private Animator animator;

    [Header("Timing")]
    [SerializeField] private float jumpDuration = 1.0f;
    [SerializeField] private float pauseOnTop = 0.35f;

    [Header("Arc")]
    [SerializeField] private float arcHeight = 1.8f;

    [Header("Pre-jump rotation")]
    [SerializeField] private bool rotateBeforeJump = true;
    [SerializeField] private float rotateDuration = 0.22f;

    [Header("Animator Params")]
    [SerializeField] private string jumpTriggerName = "JumpTrigger";
    [SerializeField] private string verticalSpeedName = "VerticalSpeed";
    [SerializeField] private string isGroundedName = "IsGrounded";

    [Header("Optional polish")]
    [SerializeField] private Vector3 rotationEulerOffset = Vector3.zero;

    private Coroutine _routine;

    private int _jumpTriggerHash;
    private int _verticalSpeedHash;
    private int _isGroundedHash;

    private void Awake()
    {
        _jumpTriggerHash = Animator.StringToHash(jumpTriggerName);
        _verticalSpeedHash = Animator.StringToHash(verticalSpeedName);
        _isGroundedHash = Animator.StringToHash(isGroundedName);
    }

    private void OnEnable()
    {
        if (player == null || rightTop == null || leftTop == null)
            return;

        LogAnimatorParameters();

        StopSim();
        _routine = StartCoroutine(Loop());
    }

    private void OnDisable()
    {
        StopSim();
    }

    public void StopSim()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
    }

    private IEnumerator Loop()
    {
        player.position = rightTop.position;

        SetGrounded(true);
        SetVerticalSpeed(0f);

        if (rotateBeforeJump)
            yield return RotateTowards(leftTop.position);

        while (true)
        {
            yield return new WaitForSeconds(pauseOnTop);

            if (rotateBeforeJump)
                yield return RotateTowards(leftTop.position);

            yield return Jump(rightTop.position, leftTop.position);

            yield return new WaitForSeconds(pauseOnTop);

            if (rotateBeforeJump)
                yield return RotateTowards(rightTop.position);

            yield return Jump(leftTop.position, rightTop.position);
        }
    }

    private IEnumerator RotateTowards(Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - player.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            yield break;

        Quaternion startRot = player.rotation;
        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up) * Quaternion.Euler(rotationEulerOffset);

        float duration = Mathf.Max(0.0001f, rotateDuration);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float u = Mathf.Clamp01(t);
            u = u * u * (3f - 2f * u);

            player.rotation = Quaternion.Slerp(startRot, targetRot, u);
            yield return null;
        }

        player.rotation = targetRot;
    }

    private IEnumerator Jump(Vector3 from, Vector3 to)
    {
        float t = 0f;
        float previousY = from.y;

        TriggerJump();
        SetGrounded(false);

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, jumpDuration);
            float u = Mathf.Clamp01(t);

            Vector3 pos = Vector3.Lerp(from, to, u);
            float arc = 4f * u * (1f - u);
            pos.y += arc * arcHeight;

            float verticalSpeed = (pos.y - previousY) / Mathf.Max(Time.deltaTime, 0.0001f);
            SetVerticalSpeed(verticalSpeed);

            player.position = pos;
            previousY = pos.y;

            yield return null;
        }

        player.position = to;
        SetVerticalSpeed(0f);
        SetGrounded(true);
    }

    private void TriggerJump()
    {
        if (animator == null)
            return;

        animator.SetTrigger(_jumpTriggerHash);
    }

    private void SetVerticalSpeed(float value)
    {
        if (animator == null)
            return;

        animator.SetFloat(_verticalSpeedHash, value);
    }

    private void SetGrounded(bool value)
    {
        if (animator == null)
            return;

        animator.SetBool(_isGroundedHash, value);
    }

    private void LogAnimatorParameters()
    {
        if (animator == null)
        {
            Debug.LogError("MenuJumpSim: animator es NULL");
            return;
        }

        Debug.Log($"Animator usado por MenuJumpSim: {animator.name}");
        Debug.Log($"Controller runtime: {animator.runtimeAnimatorController?.name}");

        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            Debug.Log($"Param: {p.name} | Type: {p.type}");
        }
    }
}