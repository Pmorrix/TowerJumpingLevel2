using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Collider))]
public class NewBuildingTimeController : MonoBehaviour
{
    [Header("Time")]
    [SerializeField] private float maxTime = 3f;

    [Header("Rules")]
    [Tooltip("Si está activo, este edificio no consume tiempo ni se desactiva.")]
    [SerializeField] private bool immuneToTime = false;

    [Header("Visual")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private int materialIndex = 1;
    [SerializeField] private string colorProperty = "_BaseColor";

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color midColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;
    [SerializeField] private Color disabledColor = Color.black;

    [Header("Sink")]
    [SerializeField] private bool sinkBeforeDisable = true;
    [SerializeField] private float sinkDistance = 3f;
    [SerializeField] private float sinkSpeed = 2f;

    [Header("SFX")]
    [SerializeField] private AudioClip expiredClip;
    [SerializeField] private float expiredVolume = 1f;

    [Header("Sink Tilt")]
    [SerializeField] private bool tiltWhileSinking = true;
    [SerializeField] private float tiltAngle = 12f;
    [SerializeField] private float tiltSpeed = 4f;
    [SerializeField] private bool tiltOnX = false;
    [SerializeField] private bool tiltOnZ = true;
    [SerializeField] private bool randomTiltDirection = true;

    [Header("Respawn Rise")]
    [SerializeField] private float respawnRiseSpeed = 4f;

    private float remainingTime;
    private bool playerOnTop;
    private bool disabled;
    private bool rising;
    private bool forcedCountdown;
    private bool sinking;
    private Coroutine delayedDisableRoutine;

    private MaterialPropertyBlock _mpb;
    private Collider _collider;
    private Rigidbody _rb;

    private Vector3 startPosition;
    private Vector3 sinkTargetPosition;

    private Quaternion startRotation;
    private Quaternion sinkTargetRotation;
    private float tiltDirectionSign = 1f;

    public bool IsImmuneToTime => immuneToTime;

    public void StartForcedCountdown(float overrideMaxTime = -1f)
    {
        if (disabled || immuneToTime)
            return;

        if (overrideMaxTime > 0f)
            maxTime = overrideMaxTime;

        remainingTime = maxTime;
        forcedCountdown = true;
        UpdateColor();
    }

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        _mpb = new MaterialPropertyBlock();
        _collider = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();

        startPosition = transform.position;
        startRotation = transform.rotation;

        ResetBuilding();
    }

    private void Update()
    {
        if (rising)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                startPosition,
                respawnRiseSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                startRotation,
                tiltSpeed * Mathf.Abs(tiltAngle) * Time.deltaTime
            );

            bool reachedPosition = Vector3.Distance(transform.position, startPosition) < 0.01f;
            bool reachedRotation = Quaternion.Angle(transform.rotation, startRotation) < 0.5f;

            if (reachedPosition && reachedRotation)
            {
                transform.position = startPosition;
                transform.rotation = startRotation;
                rising = false;
            }

            return;
        }

        if (sinking)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                sinkTargetPosition,
                sinkSpeed * Time.deltaTime
            );

            if (tiltWhileSinking)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    sinkTargetRotation,
                    tiltSpeed * Mathf.Abs(tiltAngle) * Time.deltaTime
                );
            }

            bool reachedPosition = Vector3.Distance(transform.position, sinkTargetPosition) < 0.01f;
            bool reachedRotation = !tiltWhileSinking || Quaternion.Angle(transform.rotation, sinkTargetRotation) < 0.5f;

            if (reachedPosition && reachedRotation)
            {
                transform.position = sinkTargetPosition;
                if (tiltWhileSinking)
                    transform.rotation = sinkTargetRotation;

                sinking = false;
                DeactivateBuilding();
            }

            return;
        }

        if (disabled || immuneToTime)
            return;

        if (!playerOnTop && !forcedCountdown)
            return;

        remainingTime -= Time.deltaTime;
        UpdateColor();

        if (remainingTime <= 0f)
            DisableBuilding();
    }

    public void OnPlayerEnter()
    {
        if (disabled)
            return;

        playerOnTop = true;
    }

    public void OnPlayerExit()
    {
        playerOnTop = false;

        if (!disabled)
            SetBuildingColor(normalColor);
    }

    private void UpdateColor()
    {
        float t = Mathf.Clamp01(remainingTime / maxTime);
        Color c;

        if (t > 0.5f)
            c = Color.Lerp(midColor, normalColor, (t - 0.5f) / 0.5f);
        else
            c = Color.Lerp(dangerColor, midColor, t / 0.5f);

        SetBuildingColor(c);
    }

    public void DisableBuildingImmediate(float delay = 0f)
    {
        if (disabled || immuneToTime)
            return;

        // Old behavior: collapseDelay was ignored and the building disabled immediately.
        // DisableBuilding();

        if (delay <= 0f)
        {
            DisableBuilding();
            return;
        }

        if (delayedDisableRoutine != null)
            StopCoroutine(delayedDisableRoutine);

        delayedDisableRoutine = StartCoroutine(DisableBuildingAfterDelay(delay));
    }

    public void DisableBuildingImmediate()
    {
        if (disabled || immuneToTime)
            return;

        DisableBuilding();
    }

    private void DisableBuilding()
    {
        if (disabled)
            return;

        if (delayedDisableRoutine != null)
        {
            StopCoroutine(delayedDisableRoutine);
            delayedDisableRoutine = null;
        }

        disabled = true;
        forcedCountdown = false;
        playerOnTop = false;

        SetBuildingColor(disabledColor);
        PlayExpiredSFX();

        if (_collider != null)
            _collider.enabled = false;

        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
        }

        if (sinkBeforeDisable)
        {
            sinkTargetPosition = startPosition + Vector3.down * sinkDistance;
            PrepareSinkRotation();
            sinking = true;
        }
        else
        {
            DeactivateBuilding();
        }
    }

    private void PrepareSinkRotation()
    {
        if (!tiltWhileSinking)
        {
            sinkTargetRotation = startRotation;
            return;
        }

        tiltDirectionSign = randomTiltDirection ? (Random.value < 0.5f ? -1f : 1f) : 1f;

        Vector3 targetEuler = startRotation.eulerAngles;

        if (tiltOnX)
            targetEuler.x += tiltAngle * tiltDirectionSign;

        if (tiltOnZ)
            targetEuler.z += tiltAngle * tiltDirectionSign;

        sinkTargetRotation = Quaternion.Euler(targetEuler);
    }

    private void DeactivateBuilding()
    {
        gameObject.SetActive(false);
    }

    private void PlayExpiredSFX()
    {
        if (expiredClip == null)
            return;

        GameObject sfxObject = new GameObject($"{name}_ExpiredSFX");
        sfxObject.transform.position = transform.position;

        AudioSource source = sfxObject.AddComponent<AudioSource>();
        GameAudio.ConfigureSfxSource(source);
        source.spatialBlend = 0f;
        source.playOnAwake = false;
        source.PlayOneShot(expiredClip, Mathf.Max(0f, expiredVolume));

        Destroy(sfxObject, expiredClip.length + 0.1f);
    }

    public void ResetBuilding()
    {
        CancelInvoke();

        if (delayedDisableRoutine != null)
        {
            StopCoroutine(delayedDisableRoutine);
            delayedDisableRoutine = null;
        }

        disabled = false;
        playerOnTop = false;
        remainingTime = maxTime;
        forcedCountdown = false;
        sinking = false;
        rising = true;

        if (_collider != null)
            _collider.enabled = true;

        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        SetBuildingColor(normalColor);
    }

    private void SetBuildingColor(Color color)
    {
        if (targetRenderer == null)
            return;

        Material[] mats = targetRenderer.sharedMaterials;
        if (mats == null || materialIndex < 0 || materialIndex >= mats.Length)
            return;

        Material mat = mats[materialIndex];
        if (mat == null || !mat.HasProperty(colorProperty))
            return;

        targetRenderer.GetPropertyBlock(_mpb, materialIndex);
        _mpb.SetColor(colorProperty, color);
        targetRenderer.SetPropertyBlock(_mpb, materialIndex);
    }

    private IEnumerator DisableBuildingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        delayedDisableRoutine = null;

        if (disabled || immuneToTime)
            yield break;

        DisableBuilding();
    }
}
