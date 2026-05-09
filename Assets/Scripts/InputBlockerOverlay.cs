using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public sealed class InputBlockerOverlay : UIBehaviour
{
    [SerializeField] private bool block = true;

    protected override void Awake()
    {
        base.Awake();
        var img = GetComponent<Image>();
        img.raycastTarget = true;   // ✅ intercepta todo
        // Color puede ser transparente, lo importante es raycastTarget.
    }

    public void SetBlocked(bool blocked)
    {
        block = blocked;
        var img = GetComponent<Image>();
        if (img != null) img.raycastTarget = blocked;
    }
}
