using UnityEngine;
using TMPro;

public class ComboUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject comboRoot;
    [SerializeField] private TextMeshProUGUI comboTxt;

    [Header("Timing")]
    [SerializeField] private float showDuration = 1.2f;

    private void Awake()
    {
        EnsureRefs();
        SetVisible(false);
    }

    /// <summary>
    /// Muestra el combo en formato "Xn COMBO" durante un instante.
    /// Solo debe llamarse con n >= 2.
    /// </summary>
    public void ShowCombo(int comboCount)
    {
        EnsureRefs();

        if (comboTxt == null || comboCount < 2)
            return;

        comboTxt.text = $"X{comboCount} COMBO";
        SetVisible(true);

        CancelInvoke();
        Invoke(nameof(Hide), showDuration);
    }

    public void ShowCombo(int comboCount, int bonusPoints)
    {
        EnsureRefs();

        if (comboTxt == null || comboCount < 2)
            return;

        comboTxt.text = bonusPoints > 0
            ? $"X{comboCount} COMBO +{bonusPoints}"
            : $"X{comboCount} COMBO";
        SetVisible(true);

        CancelInvoke();
        Invoke(nameof(Hide), showDuration);
    }

    private void Hide()
    {
        SetVisible(false);
    }

    /// <summary>
    /// Fuerza la ocultación inmediata (muerte, timeout, reset).
    /// </summary>
    public void ForceHide()
    {
        CancelInvoke();
        SetVisible(false);
    }

    private void EnsureRefs()
    {
        if (comboTxt == null)
            comboTxt = GetComponentInChildren<TextMeshProUGUI>(true);

        if (comboRoot == null && comboTxt != null)
            comboRoot = comboTxt.gameObject;
    }

    private void SetVisible(bool visible)
    {
        if (comboRoot != null)
        {
            comboRoot.SetActive(visible);
            return;
        }

        if (comboTxt != null)
            comboTxt.gameObject.SetActive(visible);
    }
}
