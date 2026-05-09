using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostersManager : MonoBehaviour
{
    [Header("Edificios que tienen booster")]
    [Tooltip("Solo los edificios que tienen booster visual asignado (BuildingBooster).")]
    [SerializeField] private List<BuildingBooster> boosterBuildings = new List<BuildingBooster>();

    [Header("Reglas")]
    [SerializeField] private int maxActive = 3;
    [SerializeField] private float activeDuration = 6f;
    [SerializeField] private bool autoStart = true;

    private Coroutine _loop;

    private void Awake()
    {
        // Asegurar que todos los boosters están apagados al inicio
        foreach (var bb in boosterBuildings)
        {
            if (bb != null && bb.boosterRoot != null)
                bb.boosterRoot.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (autoStart)
            StartLoop();
    }

    private void OnDisable()
    {
        StopLoop();
    }

    public void StartLoop()
    {
        if (_loop != null) return;
        _loop = StartCoroutine(LoopRoutine());
    }

    public void StopLoop()
    {
        if (_loop == null) return;
        StopCoroutine(_loop);
        _loop = null;
    }

    private IEnumerator LoopRoutine()
    {
        while (true)
        {
            ActivateRandomSet();
            yield return new WaitForSeconds(activeDuration);
        }
    }

    private void ActivateRandomSet()
    {
        int n = boosterBuildings.Count;
        if (n == 0) return;

        // Apagar todos
        foreach (var bb in boosterBuildings)
        {
            if (bb != null && bb.boosterRoot != null)
                bb.boosterRoot.SetActive(false);
        }

        // Mezclar
        for (int i = 0; i < n; i++)
        {
            int j = Random.Range(i, n);
            var temp = boosterBuildings[i];
            boosterBuildings[i] = boosterBuildings[j];
            boosterBuildings[j] = temp;
        }

        // Activar los primeros K
        int k = Mathf.Min(maxActive, n);
        for (int i = 0; i < k; i++)
        {
            var bb = boosterBuildings[i];
            if (bb != null && bb.boosterRoot != null)
                bb.boosterRoot.SetActive(true);
        }
    }

    /// <summary>
    /// Devuelve true si el booster de ESTE edificio está activo.
    /// </summary>
    public bool IsBoosterActive(BuildingBooster bb)
    {
        if (bb == null || bb.boosterRoot == null)
            return false;

        return bb.boosterRoot.activeSelf;
    }
}
