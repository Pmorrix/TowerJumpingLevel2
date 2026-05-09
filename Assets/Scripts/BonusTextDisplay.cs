using UnityEngine;
using TMPro;

public class BonusTextDisplay : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text bonusText;

    [Header("Source")]
    [Tooltip("Manager que acumula el bonus (BonusTotal).")]
    [SerializeField] private BonusPrizeManager bonusPrizeManager;

    [Header("Format")]
    [SerializeField] private string prefix = "BONUS: ";
    [Tooltip("Número mínimo de dígitos (ej: 5 -> 00000).")]
    [SerializeField] private int padDigits = 5;

    private void Awake()
    {
        if (bonusText == null)
            bonusText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (bonusPrizeManager != null)
            bonusPrizeManager.OnBonusTotalChanged += HandleBonusChanged;
    }

    private void Start()
    {
        // Refresco inicial por si el bonus ya tiene valor
        Refresh();
    }

    private void OnDisable()
    {
        if (bonusPrizeManager != null)
            bonusPrizeManager.OnBonusTotalChanged -= HandleBonusChanged;
    }

    private void HandleBonusChanged(int newTotal)
    {
        SetText(newTotal);
    }

    private void Refresh()
    {
        if (bonusPrizeManager == null)
        {
            // Si no hay manager, deja 0 por defecto
            SetText(0);
            return;
        }

        SetText(bonusPrizeManager.BonusTotal);
    }

    private void SetText(int value)
    {
        if (bonusText == null)
            return;

        string number = padDigits > 0 ? value.ToString().PadLeft(padDigits, '0') : value.ToString();
        bonusText.text = prefix + number;
    }
}