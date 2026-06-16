using System;
using System.Collections.Generic;
using UnityEngine;

public class BonusPrizeManager : MonoBehaviour
{
    [Header("Buildings Source")]
    [Tooltip("Parent que contiene los edificios candidatos (hijos directos).")]
    [SerializeField] private Transform buildsRoot;

    [Header("Score")]
    [Tooltip("Referencia al ScoreManager para sumar puntos al recoger bonus.")]
    [SerializeField] private ScoreManager scoreManager;

    [Tooltip("En Bonus: desactiva el score por tiempo (solo suma por premios).")]
    [SerializeField] private bool disableTimeScoreInBonus = true;

    [Header("SFX")]
    [Tooltip("AudioClip de campanilla al recoger premio.")]
    [SerializeField] private AudioClip prizeBellSfx;

    [Tooltip("AudioSource para reproducir el SFX. Si es null, se creará uno en runtime.")]
    [SerializeField] private AudioSource sfxSource;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 0.9f;

    [Header("Rules")]
    [SerializeField] private int bonusPerPickup = 500;
    [SerializeField] private bool autoPickInitialIfNoneActive = true;

    [Header("Combo")]
    [Tooltip("Puntos extra que suma cada premio consecutivo del bonus.")]
    [SerializeField] private int comboBonusStep = 250;

    [Tooltip("Tope de puntos por premio con combo. Si es 0 o menor, no hay tope.")]
    [SerializeField] private int maxBonusPerPickup = 1500;

    [Tooltip("UI opcional para mostrar Xn COMBO.")]
    [SerializeField] private ComboUIController comboUI;

    [Header("Perfect Bonus")]
    [Tooltip("Premios que hay que recoger para conseguir el bonus perfecto.")]
    [SerializeField] private int perfectPickupTarget = 5;

    [Tooltip("Puntos extra al alcanzar el objetivo de premios recogidos.")]
    [SerializeField] private int perfectBonusPoints = 5000;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    public int BonusTotal { get; private set; }
    public int PickupsCollected => _comboCount;
    public bool HasPerfectBonus => perfectPickupTarget > 0 && _comboCount >= perfectPickupTarget;
    public int PerfectBonusPoints => HasPerfectBonus ? Mathf.Max(0, perfectBonusPoints) : 0;

    public event Action<int> OnBonusTotalChanged;

    private readonly List<BonusPrizeNode> _nodes = new();
    private BonusPrizeNode _currentActive;
    private int _comboCount;

    public string PlayerTag => playerTag;

    private bool _prevCanAddScore;
    private bool _timeScoreOverridden;

    private void Awake()
    {
        // AudioSource mínimo para OneShot (no 3D, no bucle)
        EnsureSfxSource();

        if (scoreManager == null)
            scoreManager = FindAnyObjectByType<ScoreManager>();

        if (comboUI == null)
            comboUI = FindAnyObjectByType<ComboUIController>(FindObjectsInactive.Include);

        if (disableTimeScoreInBonus)
        {
            _prevCanAddScore = ScoreManager.canAddScore;
            ScoreManager.canAddScore = false;
            _timeScoreOverridden = true;
        }

        CacheNodes();

        if (autoPickInitialIfNoneActive)
            EnsureOneActiveBonus();
    }

    private void OnDestroy()
    {
        if (_timeScoreOverridden)
            ScoreManager.canAddScore = _prevCanAddScore;
    }

    private void EnsureSfxSource()
    {
        if (sfxSource != null)
        {
            GameAudio.ConfigureSfxSource(sfxSource);
            return;
        }

        // Crea un AudioSource dedicado en este mismo GO
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f; // 2D
        sfxSource.volume = 1f;       // volumen final se controla en PlayOneShot
        GameAudio.ConfigureSfxSource(sfxSource);
    }

    private void CacheNodes()
    {
        _nodes.Clear();
        _currentActive = null;

        if (buildsRoot == null)
        {
            Debug.LogError("[BonusPrizeManager] buildsRoot is NULL.");
            return;
        }

        int count = buildsRoot.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = buildsRoot.GetChild(i);
            if (child == null) continue;

            var node = child.GetComponent<BonusPrizeNode>();
            if (node == null) continue;

            node.Bind(this);
            _nodes.Add(node);

            if (node.IsBonusActive)
                _currentActive = node;
        }

        if (_nodes.Count == 0)
            Debug.LogWarning("[BonusPrizeManager] No se encontraron BonusPrizeNode bajo buildsRoot.");
    }

    private void EnsureOneActiveBonus()
    {
        if (_currentActive != null && _currentActive.IsBonusActive)
            return;

        var next = PickRandomNode(exclude: null);
        if (next != null)
        {
            next.SetBonusActive(true);
            _currentActive = next;
        }
    }

    internal void CollectFrom(BonusPrizeNode collectedNode)
    {
        if (collectedNode == null) return;
        if (!collectedNode.IsBonusActive) return;

        // SFX (campanilla)
        PlayPrizeSfx();

        _comboCount++;
        int pickupScore = GetPickupScoreForCurrentCombo();

        if (_comboCount >= 2 && comboUI != null)
            comboUI.ShowCombo(_comboCount, pickupScore);

        // Bonus local (para UI BONUS)
        BonusTotal += pickupScore;
        OnBonusTotalChanged?.Invoke(BonusTotal);

        // Score global SOLO por premio
        if (scoreManager != null)
            scoreManager.AddScore(pickupScore);

        // Apagar bonus actual
        collectedNode.SetBonusActive(false);

        // Encender en otro edificio aleatorio (activo)
        var next = PickRandomNode(exclude: collectedNode);
        if (next != null)
        {
            next.SetBonusActive(true);
            _currentActive = next;
        }
        else
        {
            _currentActive = null;
        }
    }

    private void PlayPrizeSfx()
    {
        if (prizeBellSfx == null || sfxSource == null)
            return;

        GameAudio.PlaySfx(sfxSource, prizeBellSfx, Mathf.Clamp01(sfxVolume));
    }

    private int GetPickupScoreForCurrentCombo()
    {
        int baseScore = Mathf.Max(0, bonusPerPickup);
        int step = Mathf.Max(0, comboBonusStep);
        int score = baseScore + Mathf.Max(0, _comboCount - 1) * step;

        if (maxBonusPerPickup > 0)
        {
            int cap = Mathf.Max(baseScore, maxBonusPerPickup);
            score = Mathf.Min(score, cap);
        }

        return score;
    }

    private BonusPrizeNode PickRandomNode(BonusPrizeNode exclude)
    {
        var candidates = ListPool<BonusPrizeNode>.Get();
        try
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                var n = _nodes[i];
                if (n == null) continue;
                if (n == exclude) continue;
                if (!n.gameObject.activeInHierarchy) continue;
                if (!n.HasBonusVisual) continue;

                candidates.Add(n);
            }

            if (candidates.Count == 0)
                return null;

            int idx = UnityEngine.Random.Range(0, candidates.Count);
            return candidates[idx];
        }
        finally
        {
            ListPool<BonusPrizeNode>.Release(candidates);
        }
    }

    private static class ListPool<T>
    {
        private static readonly Stack<List<T>> Pool = new();

        public static List<T> Get() => Pool.Count > 0 ? Pool.Pop() : new List<T>(32);

        public static void Release(List<T> list)
        {
            list.Clear();
            Pool.Push(list);
        }
    }
}