using UnityEngine;

public class PlayerBoosterEffect : MonoBehaviour
{
    [Header("FX Root")]
    [SerializeField] private GameObject boosterFxRoot;

    [Header("Options")]
    [SerializeField] private bool includeInactiveChildren = true;
    [SerializeField] private bool hideRootWhenInactive = true;
    [SerializeField] private bool clearOnStop = true;

    [Header("Runtime")]
    [SerializeField] private bool boosterActive = false;

    private ParticleSystem[] _particleSystems = new ParticleSystem[0];

    private void Awake()
    {
        CacheParticleSystems();
        ApplyState(false, true);
    }

    private void OnDisable()
    {
        ApplyState(false, true);
    }

    [ContextMenu("Cache Particle Systems")]
    public void CacheParticleSystems()
    {
        if (boosterFxRoot == null)
        {
            _particleSystems = new ParticleSystem[0];
            return;
        }

        _particleSystems = boosterFxRoot.GetComponentsInChildren<ParticleSystem>(includeInactiveChildren);
    }

    public void SetBoosterActive(bool active)
    {
        if (_particleSystems == null || _particleSystems.Length == 0)
            CacheParticleSystems();

        if (boosterActive == active)
            return;

        boosterActive = active;
        ApplyState(active, false);
    }

    private void ApplyState(bool active, bool forceClear)
    {
        if (boosterFxRoot == null)
            return;

        if (active)
        {
            if (!boosterFxRoot.activeSelf)
                boosterFxRoot.SetActive(true);

            for (int i = 0; i < _particleSystems.Length; i++)
            {
                ParticleSystem ps = _particleSystems[i];
                if (ps == null) continue;

                ps.Clear(true);
                ps.Play(true);
            }

            return;
        }

        for (int i = 0; i < _particleSystems.Length; i++)
        {
            ParticleSystem ps = _particleSystems[i];
            if (ps == null) continue;

            if (clearOnStop || forceClear)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            else
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (hideRootWhenInactive)
            boosterFxRoot.SetActive(false);
    }
}