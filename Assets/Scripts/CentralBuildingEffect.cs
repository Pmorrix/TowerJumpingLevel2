using UnityEngine;
using System.Collections;

public class CentralBuildingEffect : MonoBehaviour
{
    [Header("Effect")]
    [SerializeField] private Color effectColor = Color.magenta;
    [SerializeField] private float duration = 10f;

    [Header("Central Building Visual")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private string buildingColorProperty = "_WindowOnColor";

    [Header("Central Building Destruction")]
    [SerializeField] private BuildingTimeController buildingTimeController;
    [SerializeField] private float collapseDelay = 0.35f;

    private Coroutine currentRoutine;
    private MaterialPropertyBlock _mpb;
    private int _buildingColorPropertyId;
    private bool effectActive;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (buildingTimeController == null)
            buildingTimeController = GetComponent<BuildingTimeController>();

        _mpb = new MaterialPropertyBlock();
        _buildingColorPropertyId = Shader.PropertyToID(buildingColorProperty);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (effectActive)
            return;

        currentRoutine = StartCoroutine(ApplyEffect(collision.gameObject));
    }

    private IEnumerator ApplyEffect(GameObject player)
    {
        effectActive = true;

        if (buildingTimeController != null)
            buildingTimeController.DisableBuildingImmediate(collapseDelay);

        PlayerBoosterEffect boosterVisual = player.GetComponent<PlayerBoosterEffect>();

        //if (boosterVisual != null)
        //    boosterVisual.SetExternalOverrideForDuration(effectColor, duration);

        float timer = 0f;

        while (timer < duration)
        {
            if (targetRenderer != null)
            {
                targetRenderer.GetPropertyBlock(_mpb);
                _mpb.SetColor(_buildingColorPropertyId, effectColor);
                targetRenderer.SetPropertyBlock(_mpb);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (targetRenderer != null)
        {
            targetRenderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(_buildingColorPropertyId, Color.white);
            targetRenderer.SetPropertyBlock(_mpb);
        }

        currentRoutine = null;
        effectActive = false;
    }
}