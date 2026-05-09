using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MessagePanel : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private TMP_Text headerTxt;
    [SerializeField] private TMP_Text bodyTxt;
    [SerializeField] private TMP_Text buttonTxt;
    [SerializeField] private Button continueBtn;

    [Header("Optional")]
    [SerializeField] private bool bringToFront = true;
    [SerializeField] private CanvasGroup canvasGroup;

    private Action _onContinue;
    private bool _isOpen;

    private void Awake()
    {
        if (continueBtn != null)
            continueBtn.onClick.AddListener(HandleContinueClicked);

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        HideImmediate();
    }

    public void Show(string header, string body, string buttonText, Action onContinue)
    {
        _isOpen = true;
        _onContinue = onContinue;

        if (headerTxt != null) headerTxt.text = header ?? string.Empty;
        if (bodyTxt != null) bodyTxt.text = body ?? string.Empty;
        if (buttonTxt != null) buttonTxt.text = string.IsNullOrEmpty(buttonText) ? "Continuar" : buttonText;

        Debug.Log($"[MessagePanel] Show() gameObject={gameObject.name} activeSelf={gameObject.activeSelf} activeInHierarchy={gameObject.activeInHierarchy}");

        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (bringToFront && transform.parent != null)
            transform.SetAsLastSibling();

        if (continueBtn != null)
        {
            continueBtn.interactable = true;
            continueBtn.Select();
        }

        Debug.Log($"[MessagePanel] After Show activeSelf={gameObject.activeSelf} activeInHierarchy={gameObject.activeInHierarchy}");
    }

    public void Hide()
    {
        if (!_isOpen)
            return;

        _isOpen = false;
        _onContinue = null;

        gameObject.SetActive(false);
    }

    private void HideImmediate()
    {
        _isOpen = false;
        _onContinue = null;
        gameObject.SetActive(false);
    }

    private void HandleContinueClicked()
    {
        if (!_isOpen)
            return;

        if (continueBtn != null)
            continueBtn.interactable = false;

        var cb = _onContinue;
        Hide();
        cb?.Invoke();
    }
}
