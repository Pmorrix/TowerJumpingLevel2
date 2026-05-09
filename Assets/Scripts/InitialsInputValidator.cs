using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InitialsInputValidator : MonoBehaviour
{
    [SerializeField] TMP_InputField input;
    [SerializeField] Button continueBtn;

    void Start()
    {
        continueBtn.interactable = false;
        input.onValueChanged.AddListener(Check);
    }

    void Check(string text)
    {
        continueBtn.interactable = text.Length == 3;
    }
}