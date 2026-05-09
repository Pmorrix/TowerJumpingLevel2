using UnityEngine;
using TMPro;

public class BonusResultsUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject bonusIntroPanel;   // BonusPanel (GO)
    [SerializeField] private GameObject resultsPanel;      // BonusTotalPanel (Resultados)
    [SerializeField] private GameObject sellingPanel;      // SellingPanel (debe estar OFF aquí)

    [Header("HUD (optional)")]
    [Tooltip("Root del HUD de bonus (por ejemplo BonusTxt o su parent). Se apagará al mostrar resultados.")]
    [SerializeField] private GameObject bonusHudTextRoot;

    [Header("Texts (Results)")]
    [SerializeField] private TMP_Text scoreTxt;
    [SerializeField] private TMP_Text bonusTxt;
    [SerializeField] private TMP_Text totalTxt;

    [Header("Sources (optional, auto-resolve if null)")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private BonusPrizeManager bonusPrizeManager;

    [Header("Format")]
    [SerializeField] private int padDigits = 6;

    [Header("Debug")]
    [SerializeField] private bool logValues = true;

    private bool _shown;

    public void ShowResults()
    {
        if (_shown) return;
        _shown = true;

        // ✅ FORZAR ESTADO UI ÚNICO
        if (bonusIntroPanel != null) bonusIntroPanel.SetActive(false);
        if (bonusHudTextRoot != null) bonusHudTextRoot.SetActive(false);
        if (sellingPanel != null) sellingPanel.SetActive(false);

        GameObject panelToShow = resultsPanel != null ? resultsPanel : gameObject;
        panelToShow.SetActive(true);

        // Resolver managers si faltan
        if (scoreManager == null) scoreManager = FindAnyObjectByType<ScoreManager>();
        if (bonusPrizeManager == null) bonusPrizeManager = FindAnyObjectByType<BonusPrizeManager>();

        int collectedBonus = bonusPrizeManager != null ? bonusPrizeManager.BonusTotal : 0;
        int perfectBonus = bonusPrizeManager != null ? bonusPrizeManager.PerfectBonusPoints : 0;
        int bonus = collectedBonus + perfectBonus;

        int scoreBase = GameSession.CurrentScore;
        int total = scoreBase + bonus;

        if (scoreManager != null)
            scoreManager.SetScore(total);

        GameSession.SetProgress(total, GameSession.CurrentLives, GameSession.CurrentLevel);

        if (logValues)
            Debug.Log($"[BonusResultsUI] scoreBase={scoreBase} collectedBonus={collectedBonus} perfectBonus={perfectBonus} total={total}");

        if (scoreTxt != null) scoreTxt.text = scoreBase.ToString().PadLeft(padDigits, '0');
        if (bonusTxt != null) bonusTxt.text = bonus.ToString().PadLeft(padDigits, '0');
        if (totalTxt != null) totalTxt.text = total.ToString().PadLeft(padDigits, '0');
    }
}
