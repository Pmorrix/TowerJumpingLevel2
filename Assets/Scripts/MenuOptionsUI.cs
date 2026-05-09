using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class MenuOptionUI : MonoBehaviour, IPointerEnterHandler
{
    [Header("Refs")]
    [SerializeField] private TMP_Text label;
    [SerializeField] private Button button;

    [Header("Config")]
    [SerializeField] private string optionText = "OPTION";

    public Button Button => button;
    public string OptionText => optionText;

    private MenuControllerUI controller;
    private int optionIndex;

    private void Reset()
    {
        label = GetComponentInChildren<TMP_Text>();
        button = GetComponent<Button>();

        if (string.IsNullOrWhiteSpace(optionText) && label != null)
            optionText = label.text;
    }

    private void Awake()
    {
        if (label == null) label = GetComponentInChildren<TMP_Text>();
        if (button == null) button = GetComponent<Button>();

        if (string.IsNullOrWhiteSpace(optionText) && label != null)
            optionText = label.text;
    }

    public void Init(MenuControllerUI menuController, int index)
    {
        controller = menuController;
        optionIndex = index;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (controller != null)
            controller.HoverSelect(optionIndex);
    }

    public void ApplyVisual(bool isActive, Color normalColor, Color activeColor, bool useAsterisks)
    {
        if (label == null) return;

        label.color = isActive ? activeColor : normalColor;

        if (useAsterisks && isActive)
            label.text = $"* {optionText} *";
        else
            label.text = optionText;
    }
}