using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BonusEndController : MonoBehaviour
{
    public enum EndReason
    {
        AllBuildingsDestroyed,
        PlayerFellToFloor
    }

    [Header("Arm")]
    [SerializeField] private bool autoArmOnStart = false;

    [Header("Scene References")]
    [SerializeField] private Transform buildsRoot;

    [Header("Checks (polling)")]
    [SerializeField] private float pollInterval = 0.15f;

    [Header("Audio (Stop Music + SFX)")]
    [SerializeField] private AudioClip failClip;
    [Range(0f, 1f)][SerializeField] private float failVolume = 1f;

    [SerializeField] private AudioClip goalTowerClip;
    [Range(0f, 1f)][SerializeField] private float goalTowerVolume = 1f;

    [Tooltip("AudioSource SOLO para SFX (Fail/GoalTower). No se detiene en StopAllMusic.")]
    [SerializeField] private AudioSource sfxSource;

    [Header("UI")]
    [SerializeField] private BonusResultsUIController bonusResultsUI;

    [Header("Events")]
    public UnityEvent onEnd;
    public UnityEvent onEnd_AllBuildingsDestroyed;
    public UnityEvent onEnd_PlayerFell;

    private bool _armed;
    private bool _ended;
    private float _t;
    private Coroutine _endCo;

    private void Start()
    {
        if (autoArmOnStart)
            Arm();

        EnsureSfxSource();
    }

    private void Update()
    {
        if (!_armed || _ended)
            return;

        _t += Time.deltaTime;
        if (_t < pollInterval)
            return;

        _t = 0f;

        if (CountAliveBuildings() <= 0)
            End(EndReason.AllBuildingsDestroyed);
    }

    public void Arm()
    {
        if (_ended) return;
        _armed = true;
        _t = 0f;
    }

    public void End(EndReason reason)
    {
        if (_ended)
            return;

        _ended = true;

        if (_endCo != null)
            StopCoroutine(_endCo);

        _endCo = StartCoroutine(EndSequenceCo(reason));
    }

    private IEnumerator EndSequenceCo(EndReason reason)
    {
        EnsureSfxSource();

        // 1) FAIL si procede
        if (reason == EndReason.PlayerFellToFloor && failClip != null)
        {
            GameAudio.PlaySfx(sfxSource, failClip, Mathf.Clamp01(failVolume));
            yield return new WaitForSecondsRealtime(failClip.length);
        }

        // 2) Parar música (sin tocar sfxSource)
        StopAllMusicAggressive(exceptSource: sfxSource);

        // 3) GoalTower
        if (goalTowerClip != null)
            GameAudio.PlaySfx(sfxSource, goalTowerClip, Mathf.Clamp01(goalTowerVolume));

        // 4) Mostrar resultados (UNA sola vez)
        if (bonusResultsUI != null)
            bonusResultsUI.ShowResults();
        else
            Debug.LogWarning("[BonusEndController] bonusResultsUI no está asignado.");

        // 5) Eventos
        onEnd?.Invoke();
        if (reason == EndReason.AllBuildingsDestroyed)
            onEnd_AllBuildingsDestroyed?.Invoke();
        else
            onEnd_PlayerFell?.Invoke();
    }

    private void StopAllMusicAggressive(AudioSource exceptSource)
    {
        GameAudio.StopAllMusic(exceptSource);
    }

    private void EnsureSfxSource()
    {
        if (sfxSource != null)
        {
            GameAudio.ConfigureSfxSource(sfxSource);
            return;
        }

        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
        GameAudio.ConfigureSfxSource(sfxSource);
    }

    private int CountAliveBuildings()
    {
        if (buildsRoot == null) return 0;

        int alive = 0;
        int childCount = buildsRoot.childCount;

        for (int i = 0; i < childCount; i++)
        {
            var child = buildsRoot.GetChild(i);
            if (child == null) continue;
            if (!child.gameObject.activeInHierarchy) continue;

            if (child.GetComponent<NewBuildingTimeController>() != null)
                alive++;
        }

        return alive;
    }
}
