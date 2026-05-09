using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BonusPrizeNode : MonoBehaviour
{
    [Header("Bonus Visual (child object)")]
    [Tooltip("El GameObject hijo del 'premio' encima del edificio (se activa/desactiva).")]
    [SerializeField] private GameObject bonusVisual;

    [Header("Detection")]
    [Tooltip("False = colisión física (OnCollisionEnter). True = trigger (OnTriggerEnter).")]
    [SerializeField] private bool useTriggerInsteadOfCollision = false;

    private BonusPrizeManager _manager;

    public bool HasBonusVisual => bonusVisual != null;
    public bool IsBonusActive => bonusVisual != null && bonusVisual.activeSelf;

    public void Bind(BonusPrizeManager manager)
    {
        _manager = manager;
    }

    public void SetBonusActive(bool active)
    {
        if (bonusVisual == null) return;
        bonusVisual.SetActive(active);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (useTriggerInsteadOfCollision)
            return;

        if (_manager == null || bonusVisual == null || !bonusVisual.activeSelf)
            return;

        if (!collision.collider.CompareTag(_manager.PlayerTag))
            return;

        _manager.CollectFrom(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useTriggerInsteadOfCollision)
            return;

        if (_manager == null || bonusVisual == null || !bonusVisual.activeSelf)
            return;

        if (!other.CompareTag(_manager.PlayerTag))
            return;

        _manager.CollectFrom(this);
    }
}