using System.Collections;
using UnityEngine;

public class CameraLandingShake : MonoBehaviour
{
    [Header("Impact")]
    [SerializeField] private float duration = 0f;
    [SerializeField] private float strength = 0.03f;
    [SerializeField] private float vibrato = 1f;

    [Header("Debug")]
    [SerializeField] private KeyCode testKey = KeyCode.K;

    private Vector3 _initialLocalPosition;
    private Coroutine _routine;

    private void Awake()
    {
        _initialLocalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        transform.localPosition = _initialLocalPosition;
    }

    //private void LateUpdate()
    //{
    //    if (Input.GetKeyDown(testKey))
    //        PlayShake();
    //}

    public void PlayShake()
    {
        if (_routine != null)
            StopCoroutine(_routine);

        transform.localPosition = _initialLocalPosition;
        _routine = StartCoroutine(ImpactRoutine());
    }

    private IEnumerator ImpactRoutine()
    {
        float totalDuration = Mathf.Max(0.0001f, duration);

        Vector2 hit2D = Random.insideUnitCircle.normalized * strength;
        Vector3 hitOffset = new Vector3(hit2D.x, hit2D.y, 0f);

        transform.localPosition = _initialLocalPosition + hitOffset;

        float time = 0f;
        float returnSharpness = Mathf.Max(0.01f, vibrato);

        while (time < totalDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / totalDuration);

            // vibrato ahora controla cómo de “rápido” vuelve al origen.
            // 1 = retorno neutro
            // >1 = vuelve más agresivo al principio
            // <1 = vuelve más suave
            float eased = Mathf.Pow(t, returnSharpness);

            transform.localPosition = Vector3.Lerp(
                _initialLocalPosition + hitOffset,
                _initialLocalPosition,
                eased
            );

            yield return null;
        }

        transform.localPosition = _initialLocalPosition;
        _routine = null;
    }
}