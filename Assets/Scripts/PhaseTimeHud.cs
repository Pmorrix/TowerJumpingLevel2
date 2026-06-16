using TMPro;
using UnityEngine;

public sealed class PhaseTimeHud : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PhaseManager phaseManager;
    [SerializeField] private TMP_Text timeTxt; // asigna TimeTxt

    private void Awake()
    {
        if (timeTxt == null)
        {
            var go = GameObject.Find("TimeTxt");
            if (go != null) timeTxt = go.GetComponent<TMP_Text>();
        }
    }

    private void Update()
    {
        if (phaseManager == null || timeTxt == null)
            return;

        //timeTxt.text ="" + phaseManager.CurrentCountdownValue.ToString("D5");
    }
}
