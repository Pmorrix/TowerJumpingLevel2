using TMPro;
using UnityEngine;

public sealed class PhaseTimeHud : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private NewPhaseManager newPhaseManager;
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
        if (newPhaseManager == null || timeTxt == null)
            return;

        //timeTxt.text ="" + newPhaseManager.CurrentCountdownValue.ToString("D5");
    }
}
