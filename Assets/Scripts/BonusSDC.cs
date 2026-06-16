using System.Collections.Generic;
using UnityEngine;

public class BonusSDC : MonoBehaviour
{
    [Header("Buildings Source")]
    [Tooltip("Parent que contiene los edificios (hijos directos).")]
    [SerializeField] private Transform buildsRoot;

    [Header("Rules")]
    [Tooltip("Si > 0, fuerza maxTime en BuildingTimeController SOLO para el bonus (mismo tiempo para todos).")]
    [SerializeField] private float overrideMaxTime = -1f;

    [Tooltip("Si est� activo, salta edificios inactivos al iniciar.")]
    [SerializeField] private bool skipInactive = true;

    [SerializeField] private BonusEndController endController;

    private readonly List<BuildingTimeController> _buildings = new();
    private bool _started;

    public bool HasStarted => _started;

    private void Awake()
    {
        CacheBuildings();
    }

    private void CacheBuildings()
    {
        _buildings.Clear();

        if (buildsRoot == null)
        {
            Debug.LogError("[BonusSDC] buildsRoot is NULL.");
            return;
        }

        int childCount = buildsRoot.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var child = buildsRoot.GetChild(i);
            if (child == null) continue;

            var btc = child.GetComponent<BuildingTimeController>();
            if (btc == null)
            {
                Debug.LogWarning($"[BonusSDC] '{child.name}' no tiene BuildingTimeController.");
                continue;
            }

            _buildings.Add(btc);
        }
    }

    /// <summary>
    /// Llamar cuando el player aterrice en el primer edificio.
    /// Inicia la destrucci�n de TODOS los edificios a la vez.
    /// </summary>
    public void StartAll()
    {
        if (_started)
            return;

        _started = true;
        if (endController != null)
            endController.Arm();


        // Por si cambi� la jerarqu�a en play:
        CacheBuildings();

        for (int i = 0; i < _buildings.Count; i++)
        {
            var btc = _buildings[i];
            if (btc == null) continue;

            var go = btc.gameObject;
            if (skipInactive && !go.activeInHierarchy)
                continue;

            btc.StartForcedCountdown(overrideMaxTime);
        }
    }
}