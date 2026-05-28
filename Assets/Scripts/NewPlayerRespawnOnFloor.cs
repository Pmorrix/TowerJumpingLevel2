using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NewPlayerRespawnOnFloor : MonoBehaviour
{
    [Header("Respawn")]
    [SerializeField] private Transform dropPoint;

    [Header("Lives")]
    [SerializeField] private LivesTextDisplay livesDisplay;

    [Header("Booster Reset")]
    [SerializeField] private PlayerJump playerJump;

    [Header("Goal Progress Reset")]
    [SerializeField] private NewPlayerLanding playerLanding;

    [Header("Phase")]
    [SerializeField] private NewPhaseManager newPhaseManager;

    [Header("Visual Facing")]
    [SerializeField] private PlayerVisualFacing playerVisualFacing;

    [Header("SFX")]
    [SerializeField] private AudioClip fallHitClip;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField, Range(0f, 3f)] private float fallHitVolume = 1f;

    private Rigidbody _rb;
    private bool _isRespawning;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        if (playerJump == null)
            playerJump = GetComponent<PlayerJump>();

        if (playerLanding == null)
            playerLanding = GetComponent<NewPlayerLanding>();

        if (playerVisualFacing == null)
            playerVisualFacing = GetComponent<PlayerVisualFacing>();

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        GameAudio.ConfigureSfxSource(sfxSource);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isRespawning)
            return;

        if (!collision.collider.CompareTag("Floor"))
            return;

        _isRespawning = true;

        PlayFallHitSfx();

        if (livesDisplay != null)
            livesDisplay.LoseLife();

        RespawnAtDropPoint();
    }

    public void RespawnAtDropPoint()
    {
        _isRespawning = true;

        if (playerJump != null)
            playerJump.ResetBoosterCharges();

        Respawn();
    }

    private void Respawn()
    {
        if (dropPoint == null)
        {
            Debug.LogError("DropPoint no asignado en NewPlayerRespawnOnFloor");
            _isRespawning = false;
            return;
        }

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        _rb.position = dropPoint.position;
        _rb.rotation = dropPoint.rotation;

        transform.SetPositionAndRotation(dropPoint.position, dropPoint.rotation);

        if (playerLanding != null)
            playerLanding.ResetGoalProgress();

        if (newPhaseManager != null)
            newPhaseManager.ResetSpawnStateAfterRespawn();

        if (playerVisualFacing != null)
            playerVisualFacing.ResetFacingToFront();

        ScoreManager.SetCanAddScore(false);
        Invoke(nameof(ResetTrigger), 0.1f);
    }

    private void PlayFallHitSfx()
    {
        if (fallHitClip == null || sfxSource == null)
            return;

        GameAudio.PlaySfx(sfxSource, fallHitClip, fallHitVolume);
    }

    private void ResetTrigger()
    {
        _isRespawning = false;
    }
}
