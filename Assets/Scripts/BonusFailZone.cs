using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BonusFailZone : MonoBehaviour
{
    [SerializeField] private BonusEndController endController;
    [SerializeField] private string playerTag = "Player";

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (endController == null)
            endController = FindAnyObjectByType<BonusEndController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (endController == null)
            return;

        if (!other.CompareTag(playerTag))
            return;

        Debug.Log("Player entered bonus fail zone, ending bonus level with reason PlayerFellToFloor.");

        endController.End(BonusEndController.EndReason.PlayerFellToFloor);
    }
}
