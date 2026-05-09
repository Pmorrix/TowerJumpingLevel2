using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class NewGameButtonPulse : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Referencia al botón de 'Nueva partida'. Si se deja vacío, se intentará usar el componente Button en este mismo GameObject.")]
    [SerializeField] private Button newGameButton;

    [Header("Pulse Settings")]
    [Tooltip("Escala mínima del pulso (1 = escala original).")]
    [SerializeField] private float minScale = 0.95f;

    [Tooltip("Escala máxima del pulso (1 = escala original).")]
    [SerializeField] private float maxScale = 1.08f;

    [Tooltip("Velocidad del pulso (ciclos por segundo).")]
    [SerializeField] private float pulseSpeed = 1.25f;

    [Tooltip("Si está activo, el pulso se detiene cuando el botón no es interactuable.")]
    [SerializeField] private bool stopWhenNotInteractable = true;

    [Tooltip("Si está activo, el pulso se detiene mientras el puntero está encima del botón (hover).")]
    [SerializeField] private bool pauseOnHover = false;

    [Header("Advanced")]
    [Tooltip("Reinicia la escala al deshabilitar el objeto.")]
    [SerializeField] private bool resetScaleOnDisable = true;

    private RectTransform _rectTransform;
    private Vector3 _baseScale;
    private bool _isPointerOver;

    private void Reset()
    {
        // Auto-asignación en el editor si el script está en el mismo objeto del Button.
        newGameButton = GetComponent<Button>();
    }

    private void Awake()
    {
        // Si no se asignó manualmente, intenta encontrar el botón en este GameObject.
        if (newGameButton == null)
            newGameButton = GetComponent<Button>();

        // Usa el RectTransform del botón si existe; si no, usa el del propio objeto.
        _rectTransform = (newGameButton != null) ? newGameButton.GetComponent<RectTransform>() : GetComponent<RectTransform>();

        if (_rectTransform != null)
            _baseScale = _rectTransform.localScale;
    }

    private void OnEnable()
    {
        // Guarda la escala base al habilitar (por si cambió en tiempo de ejecución).
        if (_rectTransform != null)
            _baseScale = _rectTransform.localScale;
    }

    private void Update()
    {
        if (_rectTransform == null)
            return;

        // Opcional: detener pulso si el botón no es interactuable.
        if (stopWhenNotInteractable && newGameButton != null && !newGameButton.interactable)
        {
            _rectTransform.localScale = _baseScale;
            return;
        }

        // Opcional: pausar pulso si el puntero está encima del botón.
        if (pauseOnHover && _isPointerOver)
        {
            _rectTransform.localScale = _baseScale;
            return;
        }

        // Genera un pulso suave con una onda senoidal:
        // t oscila entre 0 y 1.
        float t = (Mathf.Sin(Time.unscaledTime * (Mathf.PI * 2f) * pulseSpeed) + 1f) * 0.5f;

        // Interpola la escala entre minScale y maxScale.
        float scale = Mathf.Lerp(minScale, maxScale, t);

        // Aplica el pulso multiplicando por la escala base.
        _rectTransform.localScale = _baseScale * scale;
    }

    // Métodos públicos opcionales para que otros scripts puedan controlar el hover sin usar interfaces extra.
    public void SetPointerOver(bool isOver)
    {
        _isPointerOver = isOver;
    }

    private void OnDisable()
    {
        // Restaura la escala al deshabilitar.
        if (resetScaleOnDisable && _rectTransform != null)
            _rectTransform.localScale = _baseScale;
    }
}