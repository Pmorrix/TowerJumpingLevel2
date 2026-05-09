using UnityEngine;

public sealed class MenuFirstLandingDebug : MonoBehaviour
{
    [SerializeField] private Transform targetPlayer;
    [SerializeField] private GameObject menuJumpSimRoot;

    private bool _logged;

    private void OnCollisionEnter(Collision collision)
    {
        if (_logged)
            return;

        if (targetPlayer != null)
        {
            if (collision.transform != targetPlayer)
                return;
        }
        else
        {
            if (!collision.transform.CompareTag("Player"))
                return;
        }

        _logged = true;
        Debug.Log("El player ha caído sobre el edificio por primera vez.");
        menuJumpSimRoot.SetActive(true);
    }
}