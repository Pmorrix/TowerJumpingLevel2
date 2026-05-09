using UnityEngine;

public sealed class MessagePanelLock : MonoBehaviour
{
    [Header("Player lock")]
    [Tooltip("Arrastra aquí los scripts del Player que quieras desactivar mientras el panel esté activo.")]
    [SerializeField] private Behaviour[] playerBehavioursToDisable;

    private void OnEnable()
    {
        // Detener el tiempo
        Time.timeScale = 0f;

        // Deshabilitar scripts del player
        if (playerBehavioursToDisable != null)
        {
            for (int i = 0; i < playerBehavioursToDisable.Length; i++)
                if (playerBehavioursToDisable[i] != null)
                    playerBehavioursToDisable[i].enabled = false;
        }
    }

    private void OnDisable()
    {
        // Restaurar tiempo
        Time.timeScale = 1f;

        // Restaurar scripts del player
        if (playerBehavioursToDisable != null)
        {
            for (int i = 0; i < playerBehavioursToDisable.Length; i++)
                if (playerBehavioursToDisable[i] != null)
                    playerBehavioursToDisable[i].enabled = true;
        }
    }
}
