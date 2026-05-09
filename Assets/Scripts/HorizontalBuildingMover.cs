using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class HorizontalBuildingMover : MonoBehaviour
{
    [Header("Limits")]
    [SerializeField] private Transform leftBoundary;
    [SerializeField] private Transform rightBoundary;

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float contactPadding = 0.02f;
    [SerializeField] private bool startMovingRight = true;
    [SerializeField] private bool stopWhenColliderDisabled = true;

    [Header("Passenger")]
    [SerializeField] private bool carryPlayer = true;
    [SerializeField] private float topContactTolerance = 0.15f;

    private Collider selfCollider;
    private Collider leftCollider;
    private Collider rightCollider;
    private Rigidbody carriedPlayer;
    private int direction;

    private void Awake()
    {
        selfCollider = GetComponent<Collider>();
        leftCollider = leftBoundary != null ? leftBoundary.GetComponentInChildren<Collider>() : null;
        rightCollider = rightBoundary != null ? rightBoundary.GetComponentInChildren<Collider>() : null;
        direction = startMovingRight ? 1 : -1;
    }

    private void FixedUpdate()
    {
        if (speed <= 0f)
            return;

        if (stopWhenColliderDisabled && selfCollider != null && !selfCollider.enabled)
            return;

        Vector3 previousPosition = transform.position;
        Vector3 position = previousPosition;
        position.x += direction * speed * Time.fixedDeltaTime;
        transform.position = position;

        Vector3 delta = transform.position - previousPosition;
        MoveCarriedPlayer(delta);

        CheckLimits();
    }

    private void CheckLimits()
    {
        if (selfCollider == null)
            return;

        Bounds selfBounds = selfCollider.bounds;

        if (direction < 0 && leftCollider != null)
        {
            Bounds leftBounds = leftCollider.bounds;
            float minX = leftBounds.max.x + selfBounds.extents.x + contactPadding;

            if (selfBounds.min.x <= leftBounds.max.x + contactPadding)
            {
                SetWorldX(minX);
                direction = 1;
            }
        }
        else if (direction > 0 && rightCollider != null)
        {
            Bounds rightBounds = rightCollider.bounds;
            float maxX = rightBounds.min.x - selfBounds.extents.x - contactPadding;

            if (selfBounds.max.x >= rightBounds.min.x - contactPadding)
            {
                SetWorldX(maxX);
                direction = -1;
            }
        }
    }

    private void SetWorldX(float x)
    {
        Vector3 previousPosition = transform.position;
        Vector3 position = transform.position;
        position.x = x;
        transform.position = position;

        MoveCarriedPlayer(transform.position - previousPosition);
    }

    private void MoveCarriedPlayer(Vector3 delta)
    {
        if (!carryPlayer || carriedPlayer == null || delta.sqrMagnitude <= 0f)
            return;

        carriedPlayer.MovePosition(carriedPlayer.position + delta);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!carryPlayer || !IsPlayerCollision(collision))
            return;

        if (IsCollisionOnTop(collision))
            carriedPlayer = collision.rigidbody;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.rigidbody != null && collision.rigidbody == carriedPlayer)
            carriedPlayer = null;
    }

    private bool IsPlayerCollision(Collision collision)
    {
        if (collision.rigidbody == null)
            return false;

        Transform root = collision.rigidbody.transform.root;
        return root != null && root.CompareTag("Player");
    }

    private bool IsCollisionOnTop(Collision collision)
    {
        if (selfCollider == null)
            return false;

        float topY = selfCollider.bounds.max.y;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);
            if (contact.point.y >= topY - topContactTolerance)
                return true;
        }

        return false;
    }

    private void OnValidate()
    {
        speed = Mathf.Max(0f, speed);
        contactPadding = Mathf.Max(0f, contactPadding);
        topContactTolerance = Mathf.Max(0f, topContactTolerance);
    }
}
