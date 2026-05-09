using UnityEngine;
using TMPro;

public class GradualFontSizeIncrease : MonoBehaviour
{
    [Tooltip("Componente TextMeshProUGUI a modificar. Si se deja vacío, se intentará obtener en el mismo GameObject.")]
    [SerializeField] private TextMeshProUGUI targetText;

    [Tooltip("Tamaño de fuente inicial (por ejemplo 10).")]
    [SerializeField] private float startSize = 10f;

    [Tooltip("Tamaño de fuente final (por ejemplo 190).")]
    [SerializeField] private float endSize = 190f;

    [Tooltip("Segundos que tarda en pasar de startSize a endSize.")]
    [SerializeField] private float duration = 5f;

    [Tooltip("Si está activado, al llegar al tamaño final se reinicia y vuelve a crecer desde el inicio.")]
    [SerializeField] private bool loop = false;

    [Tooltip("Si está activado, inicia automáticamente al habilitar el objeto.")]
    [SerializeField] private bool playOnEnable = true;

    [Tooltip("Objeto que contiene el menú")]
    [SerializeField] private GameObject menuObject;
    
    [Tooltip("Objeto que contiene los edificios")]
    [SerializeField] private GameObject buildingsObject;

    [Tooltip("Objeto que contiene el player")]
    [SerializeField] private GameObject playerObject;

    [Tooltip("Objeto que contiene la animacion del player")]
    [SerializeField] private GameObject playerAnimationObject;

    // Tiempo acumulado desde el inicio de la animación.
    private float elapsed;

    // Control interno de si está corriendo la animación.
    private bool isPlaying;

    private void Reset()
    {
        // Al agregar el script, intenta auto-asignar el TMP en el mismo objeto.
        targetText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        // Asegura referencia si no se asignó.
        if (targetText == null)
            targetText = GetComponent<TextMeshProUGUI>();

        if (playOnEnable)
            Play();
    }

    private void Update()
    {
        if (!isPlaying || targetText == null)
            return;

        // Evita división por cero.
        if (duration <= 0f)
        {
            targetText.fontSize = endSize;
            isPlaying = loop;   // Si hay loop, se mantiene "jugando" y se reinicia abajo.
            elapsed = 0f;
            return;
        }

        // Avanza el tiempo.
        elapsed += Time.deltaTime;

        // Normaliza el progreso 0..1.
        float t = Mathf.Clamp01(elapsed / duration);

        // Interpola suavemente el tamaño de fuente.
        targetText.fontSize = Mathf.Lerp(startSize, endSize, t);

        // Si llegó al final...
        if (t >= 1f)
        {
            if (loop)
            {
                // Reinicia para repetir.
                elapsed = 0f;
                targetText.fontSize = startSize;
            }
            else
            {
                // Detiene la animación.
                isPlaying = false;
                menuObject.SetActive(true);
                buildingsObject.SetActive(true);
                playerObject.SetActive(true);
                playerAnimationObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Inicia la animación desde el tamaño inicial.
    /// </summary>
    public void Play()
    {
        elapsed = 0f;
        isPlaying = true;

        if (targetText != null)
            targetText.fontSize = startSize;
    }

    /// <summary>
    /// Detiene la animación en el tamaño actual.
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
    }
}