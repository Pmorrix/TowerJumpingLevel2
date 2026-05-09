using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class CenterReturnBuildingMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float centerX = 0f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool startMovingToCenter = true;
    [SerializeField] private bool stopWhenColliderDisabled = true;

    [Header("Passenger")]
    [SerializeField] private bool carryPlayer = true;
    [SerializeField] private float topContactTolerance = 0.15f;

    private Collider selfCollider;
    private Rigidbody carriedPlayer;
    private Vector3 homePosition;
    private bool movingToCenter;

    private void Awake()
    {
        selfCollider = GetComponent<Collider>();
        homePosition = transform.position;
        movingToCenter = startMovingToCenter;
    }

    private void FixedUpdate()
    {
        if (speed <= 0f)
            return;

        if (stopWhenColliderDisabled && selfCollider != null && !selfCollider.enabled)
            return;

        Vector3 target = movingToCenter
            ? new Vector3(centerX, homePosition.y, homePosition.z)
            : homePosition;

        Vector3 previousPosition = transform.position;
        Vector3 nextPosition = Vector3.MoveTowards(previousPosition, target, speed * Time.fixedDeltaTime);

        transform.position = nextPosition;
        MoveCarriedPlayer(nextPosition - previousPosition);

        if (Vector3.Distance(nextPosition, target) <= 0.01f)
            movingToCenter = !movingToCenter;
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
        topContactTolerance = Mathf.Max(0f, topContactTolerance);
    }
}
