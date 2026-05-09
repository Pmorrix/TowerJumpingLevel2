using UnityEngine;

public sealed class PanelPlayerLock : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject messagePanelRoot;

    [Header("Player scripts to disable while panel is shown")]
    [SerializeField] private Behaviour[] behavioursToDisable;

    private bool _wasActive;

    private void Update()
    {
        bool active = messagePanelRoot != null && messagePanelRoot.activeInHierarchy;

        if (active == _wasActive)
            return;

        _wasActive = active;

        if (behavioursToDisable == null)
            return;

        for (int i = 0; i < behavioursToDisable.Length; i++)
        {
            if (behavioursToDisable[i] != null)
                behavioursToDisable[i].enabled = !active;
        }

        // Opcional: bloquear cursor / etc si usas PC
        // Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
        // Cursor.visible = active;
    }
}
