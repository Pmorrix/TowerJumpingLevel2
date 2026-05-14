using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PlayerJump : MonoBehaviour
{
    [Header("Jump (SMB Style)")]
    [SerializeField] private float jumpImpulse = 7f;
    [SerializeField] private float jumpCutGravityMultiplier = 2.5f;
    [SerializeField] private float fallGravityMultiplier = 2.0f;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.04f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float groundCheckExtraDistance = 0.05f;
    [SerializeField] private float groundNormalYThreshold = 0.5f;
    [SerializeField] private int contactsBufferSize = 8;

    [Header("UI")]
    [Tooltip("Arrastra aquí el TextMeshProUGUI del Canvas: boostersTxt")]
    [SerializeField] private TMP_Text boostersTxt;

    [Header("Booster = Triple Jump")]
    [Tooltip("Saltos totales sin booster (1 = salto normal)")]
    [SerializeField] private int normalTotalJumps = 1;

    [Tooltip("Saltos totales cuando se consume booster (3 = triple salto)")]
    [SerializeField] private int boostedTotalJumps = 3;

    [Tooltip("Si es true, el booster se consume al hacer el primer salto desde un edificio y habilita saltos aéreos extra hasta aterrizar.")]
    [SerializeField] private bool consumeBoosterOnFirstJump = true;

    [Header("Booster VFX")]
    [SerializeField] private PlayerBoosterEffect boosterVisual;

    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip boosterActivateClip;
    [SerializeField] private AudioSource audioSource;

    private Rigidbody _rb;
    private Collider _col;
    private RaycastHit[] _groundHits;
    private float _baseGravity;

    // Estado salto
    private bool _jumpHeld;
    private float _lastGroundedTime;

    // Contador de saltos
    private int _jumpsUsed = 0;
    private int _currentTotalJumps = 1;

    // Booster 0/1 (reserva)
    private bool _hasBooster = false;

    // Estado booster activo (debe durar hasta aterrizar si se consumió)
    private bool _boosterStateActive = false;

    // Cache visual/gameplay
    private bool _isGroundedNow;
    private bool _wasGroundedLastFrame;
    private bool _jumpStartedThisFrame;

    public bool HasBooster => _hasBooster;
    public bool IsBoosterStateActive => _boosterStateActive;

    // API pública para animación
    public bool IsGroundedNow => _isGroundedNow;
    public float VerticalSpeed => _rb != null ? _rb.linearVelocity.y : 0f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
        _groundHits = new RaycastHit[Mathf.Max(1, contactsBufferSize)];
        _baseGravity = Physics.gravity.y;

        if (boosterVisual == null)
            boosterVisual = GetComponent<PlayerBoosterEffect>();

        ResetBoosterCharges();
        ResetJumpStateOnGround();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        GameAudio.ConfigureSfxSource(audioSource);
    }

    private void Update()
    {
        _jumpStartedThisFrame = false;

        _isGroundedNow = CheckGrounded();

        if (_isGroundedNow)
        {
            _lastGroundedTime = Time.time;

            // Solo reiniciar al aterrizar de verdad.
            if (!_wasGroundedLastFrame)
                ResetJumpStateOnGround();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jumpHeld = true;
            TryJump();
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            _jumpHeld = false;
        }

        ApplyVariableGravity();

        _wasGroundedLastFrame = _isGroundedNow;
    }

    // Consumido por el bridge visual
    public bool ConsumeJumpStartedThisFrame()
    {
        if (!_jumpStartedThisFrame)
            return false;

        _jumpStartedThisFrame = false;
        return true;
    }

    public void MobileJumpDown()
    {
        _jumpHeld = true;
        TryJump();
    }

    public void MobileJumpUp()
    {
        _jumpHeld = false;
    }

    // ─────────────────────────────────────────────
    // API booster 0/1
    // ─────────────────────────────────────────────

    public bool TryGrantBooster()
    {
        if (_hasBooster)
            return false;

        _hasBooster = true;
        RefreshBoosterVfx();
        RefreshBoostersUI();
        return true;
    }

    public void ResetBoosterCharges()
    {
        _hasBooster = false;
        _boosterStateActive = false;
        RefreshBoosterVfx();
        RefreshBoostersUI();
    }

    public void AddBoosterCharge(int amount = 1)
    {
        if (amount <= 0)
            return;

        TryGrantBooster();
    }

    private void RefreshBoosterVfx()
    {
        if (boosterVisual == null)
            return;

        bool active = _hasBooster || _boosterStateActive;
        boosterVisual.SetBoosterActive(active);
    }

    private void RefreshBoostersUI()
    {
        if (boostersTxt == null)
            return;

        if (_boosterStateActive)
        {
            int remainingAirJumps = Mathf.Max(0, _currentTotalJumps - _jumpsUsed);
            boostersTxt.text = remainingAirJumps.ToString();
            return;
        }

        boostersTxt.text = _hasBooster ? "1" : "0";
    }

    // ─────────────────────────────────────────────
    // Lógica de salto
    // ─────────────────────────────────────────────

    private void TryJump()
    {
        bool canUseCoyote =
            !_isGroundedNow &&
            !_boosterStateActive &&
            coyoteTime > 0f &&
            (Time.time - _lastGroundedTime) <= coyoteTime;

        // Salto normal: desde suelo o dentro del coyote time.
        if (_isGroundedNow || canUseCoyote)
        {
            if (consumeBoosterOnFirstJump && _hasBooster)
            {
                ConsumeBoosterAndEnableBoostedJumps();
            }
            else
            {
                _currentTotalJumps = Mathf.Max(1, normalTotalJumps);
                _boosterStateActive = false;
                RefreshBoosterVfx();
            }

            PerformJump();
            _jumpsUsed = 1;
            RefreshBoostersUI();
            return;
        }

        // En el aire solo se permite salto si el booster ya está activo.
        if (_boosterStateActive && _jumpsUsed < _currentTotalJumps)
        {
            PerformJump();
            _jumpsUsed++;
            RefreshBoostersUI();
        }
    }

    private void ConsumeBoosterAndEnableBoostedJumps()
    {
        _hasBooster = false;
        _boosterStateActive = true;
        _currentTotalJumps = Mathf.Max(1, boostedTotalJumps);
        RefreshBoosterVfx();
        RefreshBoostersUI();
    }

    private void PerformJump()
    {
        ResetVerticalVelocity();
        _rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);

        _jumpStartedThisFrame = true;
        _isGroundedNow = false;
        _wasGroundedLastFrame = false;

        if (!_boosterStateActive)
            PlayJumpSFX();
        else
            PlayBoosterActivateSFX();
    }

    private void PlayJumpSFX()
    {
        if (jumpClip != null && audioSource != null)
            GameAudio.PlaySfx(audioSource, jumpClip);
    }

    private void PlayBoosterActivateSFX()
    {
        if (boosterActivateClip != null && audioSource != null)
            GameAudio.PlaySfx(audioSource, boosterActivateClip);
    }

    private void ApplyVariableGravity()
    {
        float vy = _rb.linearVelocity.y;

        if (vy > 0f)
        {
            if (!_jumpHeld)
            {
                _rb.AddForce(
                    Vector3.up * (_baseGravity * (jumpCutGravityMultiplier - 1f)),
                    ForceMode.Acceleration
                );
            }
        }
        else if (vy < 0f)
        {
            _rb.AddForce(
                Vector3.up * (_baseGravity * (fallGravityMultiplier - 1f)),
                ForceMode.Acceleration
            );
        }
    }

    private bool CheckGrounded()
    {
        if (_col == null)
            return false;

        Bounds bounds = _col.bounds;

        Vector3 origin = new Vector3(
            bounds.center.x,
            bounds.min.y + 0.05f,
            bounds.center.z
        );

        float distance = 0.12f;

        if (Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            distance,
            groundLayers,
            QueryTriggerInteraction.Ignore))
        {
            if (hit.collider == _col)
                return false;

            if (hit.normal.y < groundNormalYThreshold)
                return false;

            return true;
        }

        return false;
    }

    private void ResetJumpStateOnGround()
    {
        _jumpsUsed = 0;
        _currentTotalJumps = Mathf.Max(1, normalTotalJumps);

        if (_boosterStateActive)
        {
            _boosterStateActive = false;
            RefreshBoosterVfx();
        }

        RefreshBoostersUI();
    }

    private void ResetVerticalVelocity()
    {
        Vector3 v = _rb.linearVelocity;
        v.y = 0f;
        _rb.linearVelocity = v;
    }
}
