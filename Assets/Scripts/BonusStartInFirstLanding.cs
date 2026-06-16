using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BonusStartInFirstLanding : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BonusSDC controller;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    private bool _triggered;

    private void Reset()
    {
        if (controller == null)
            controller = FindAnyObjectByType<BonusSDC>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_triggered || controller == null)
            return;

        if (!collision.collider.CompareTag(playerTag))
            return;

        _triggered = true;
        controller.StartAll();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered || controller == null)
            return;

        if (!other.CompareTag(playerTag))
            return;

        _triggered = true;
        controller.StartAll();
    }
}
