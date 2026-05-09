using UnityEngine;

/// <summary>
/// Gestiona el sistema de combos:
/// Un combo ocurre cuando el jugador (en estado de boost) cae/sobrepasa un edificio marcado con "circulos".
/// Si cae sobre otro edificio válido consecutivo, el combo incrementa (x1, x2, x3...).
/// Si deja de caer sobre edificios válidos durante un tiempo o repite el mismo edificio, el combo se rompe.
/// </summary>
public class ComboManager : MonoBehaviour
{
    [Header("Referencias")]

    [Tooltip("Transform del jugador que debe caer sobre los edificios.")]
    [SerializeField] private Transform player;

    [Tooltip("Script/Behaviour que indica si el jugador está en estado de boost. Puede ser cualquier MonoBehaviour con una propiedad/field bool llamado 'IsBoosting' o 'isBoosting'.")]
    [SerializeField] private MonoBehaviour boostStateProvider;

    [Tooltip("Distancia máxima para considerar que el jugador 'cayó sobre' el edificio (raycast hacia abajo).")]
    [SerializeField] private float groundCheckDistance = 5f;

    [Tooltip("Capas consideradas como 'edificio con círculos' para el combo.")]
    [SerializeField] private LayerMask buildingWithCirclesMask;

    [Header("Combo")]

    [Tooltip("Tiempo máximo (segundos) entre aterrizajes válidos para mantener el combo. Si se excede, el combo se reinicia.")]
    [SerializeField] private float comboGraceTime = 1.0f;

    [Tooltip("Tiempo mínimo (segundos) entre conteos de combo para evitar múltiples incrementos en el mismo aterrizaje.")]
    [SerializeField] private float landingCooldown = 0.15f;

    [Tooltip("Si está activado, repetir el mismo edificio consecutivamente no incrementa el combo y lo rompe.")]
    [SerializeField] private bool breakComboOnSameBuilding = true;

    [Header("Debug")]

    [Tooltip("Si está activado, dibuja el raycast de chequeo hacia abajo.")]
    [SerializeField] private bool debugDrawRay = true;

    [Tooltip("Color del raycast cuando no hay impacto.")]
    [SerializeField] private Color debugRayColorMiss = Color.red;

    [Tooltip("Color del raycast cuando hay impacto en edificio válido.")]
    [SerializeField] private Color debugRayColorHit = Color.green;

    // Estado interno del combo (valor xN)
    public int CurrentCombo { get; private set; } = 0;

    // Control interno para ventana de tiempo del combo
    private float _timeSinceLastValidLanding = Mathf.Infinity;

    // Control de cooldown para evitar múltiples incrementos por el mismo contacto
    private float _landingCooldownTimer = 0f;

    // Para detectar aterrizaje (transición aire->tierra)
    private bool _wasGroundedLastFrame = false;

    // Último edificio válido tocado (para evitar repetir)
    private Collider _lastBuildingCollider = null;

    // Cache de reflect para leer "IsBoosting" o "isBoosting" sin depender de componentes custom
    private System.Reflection.PropertyInfo _boostProp;
    private System.Reflection.FieldInfo _boostField;

    private void Awake()
    {
        // Prepara lectura de estado de boost desde el provider (si se asignó)
        if (boostStateProvider != null)
        {
            var t = boostStateProvider.GetType();

            // Busca propiedad pública/privada llamada IsBoosting o isBoosting
            _boostProp = t.GetProperty("IsBoosting",
                             System.Reflection.BindingFlags.Instance |
                             System.Reflection.BindingFlags.Public |
                             System.Reflection.BindingFlags.NonPublic)
                         ?? t.GetProperty("isBoosting",
                             System.Reflection.BindingFlags.Instance |
                             System.Reflection.BindingFlags.Public |
                             System.Reflection.BindingFlags.NonPublic);

            // Busca field público/privado llamado IsBoosting o isBoosting
            _boostField = t.GetField("IsBoosting",
                              System.Reflection.BindingFlags.Instance |
                              System.Reflection.BindingFlags.Public |
                              System.Reflection.BindingFlags.NonPublic)
                          ?? t.GetField("isBoosting",
                              System.Reflection.BindingFlags.Instance |
                              System.Reflection.BindingFlags.Public |
                              System.Reflection.BindingFlags.NonPublic);
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        // Timers
        _timeSinceLastValidLanding += Time.deltaTime;
        _landingCooldownTimer -= Time.deltaTime;

        // Si se pasó el tiempo de gracia, se rompe el combo
        if (_timeSinceLastValidLanding > comboGraceTime && CurrentCombo > 0)
        {
            ResetCombo();
        }

        // Chequeo de suelo/edificio con raycast hacia abajo
        bool groundedNow = RaycastDown(out RaycastHit hit);

        // Detecta un "aterrizaje": cuando antes no estaba grounded y ahora sí
        bool landedThisFrame = (!_wasGroundedLastFrame && groundedNow);

        if (landedThisFrame)
        {
            TryRegisterLanding(hit);
        }

        _wasGroundedLastFrame = groundedNow;
    }

    /// <summary>
    /// Hace raycast hacia abajo desde el jugador para detectar si está sobre un edificio válido.
    /// </summary>
    private bool RaycastDown(out RaycastHit hit)
    {
        Vector3 origin = player.position;
        Vector3 dir = Vector3.down;

        bool didHit = Physics.Raycast(origin, dir, out hit, groundCheckDistance, buildingWithCirclesMask, QueryTriggerInteraction.Ignore);

        if (debugDrawRay)
        {
            Color c = didHit ? debugRayColorHit : debugRayColorMiss;
            Debug.DrawRay(origin, dir * groundCheckDistance, c);
        }

        return didHit;
    }

    /// <summary>
    /// Intenta registrar un aterrizaje como parte del combo.
    /// Requiere que el jugador esté en boost y que el collider impactado sea válido.
    /// </summary>
    private void TryRegisterLanding(RaycastHit hit)
    {
        // Evita conteos múltiples muy seguidos
        if (_landingCooldownTimer > 0f)
            return;

        // Requiere estado boost activo
        if (!IsBoosting())
        {
            // Si aterriza sin boost, se rompe el combo (opcional: se podría mantener, pero según descripción debe ser en boost)
            if (CurrentCombo > 0)
                ResetCombo();
            return;
        }

        Collider buildingCollider = hit.collider;
        if (buildingCollider == null)
            return;

        // Si repite el mismo edificio consecutivo
        if (breakComboOnSameBuilding && buildingCollider == _lastBuildingCollider)
        {
            ResetCombo();
            _landingCooldownTimer = landingCooldown;
            return;
        }

        // Aterrizaje válido -> incrementa combo
        CurrentCombo = Mathf.Max(CurrentCombo + 1, 1);

        // Reinicia ventana de tiempo para continuar el combo
        _timeSinceLastValidLanding = 0f;

        // Guarda último edificio
        _lastBuildingCollider = buildingCollider;

        // Aplica cooldown de aterrizaje
        _landingCooldownTimer = landingCooldown;

        // Aquí podrías disparar eventos/sonidos/UI desde otro script leyendo CurrentCombo.
        // Debug.Log($"Combo x{CurrentCombo}");
    }

    /// <summary>
    /// Determina si el jugador está en estado de boost consultando el provider.
    /// Si no hay provider asignado, devuelve false.
    /// </summary>
    private bool IsBoosting()
    {
        if (boostStateProvider == null)
            return false;

        // Propiedad
        if (_boostProp != null && _boostProp.PropertyType == typeof(bool))
        {
            try { return (bool)_boostProp.GetValue(boostStateProvider, null); }
            catch { return false; }
        }

        // Field
        if (_boostField != null && _boostField.FieldType == typeof(bool))
        {
            try { return (bool)_boostField.GetValue(boostStateProvider); }
            catch { return false; }
        }

        // Si no existe ninguna propiedad/campo compatible
        return false;
    }

    /// <summary>
    /// Resetea el combo a 0.
    /// </summary>
    public void ResetCombo()
    {
        CurrentCombo = 0;
        _lastBuildingCollider = null;
        _timeSinceLastValidLanding = Mathf.Infinity;
    }
}