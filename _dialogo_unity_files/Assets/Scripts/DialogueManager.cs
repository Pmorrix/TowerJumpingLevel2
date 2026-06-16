using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Text sceneText;
    [SerializeField] private Text stateText;
    [SerializeField] private Button optionButton1;
    [SerializeField] private Button optionButton2;
    [SerializeField] private Button optionButton3;
    [SerializeField] private Button resetButton;
    [SerializeField] private Text optionLabel1;
    [SerializeField] private Text optionLabel2;
    [SerializeField] private Text optionLabel3;

    [Header("Contenido generado por ChatGPT")]
    [TextArea(3, 8)]
    [SerializeField] private string initialScene =
        "El Amo del Calabozo levanta una mano y la antorcha se apaga.\n" +
        "Frente a ti hay una puerta de piedra cubierta de runas antiguas.\n" +
        "Una voz grave susurra desde el otro lado:\n" +
        "\"Solo quien entienda el miedo podra cruzar.\"";

    [SerializeField] private string option1 = "Examinar las runas.";
    [SerializeField] private string option2 = "Llamar a la voz.";
    [SerializeField] private string option3 = "Empujar la puerta.";

    [TextArea(2, 6)]
    [SerializeField] private string result1 =
        "Descubres que las runas forman un acertijo sobre la valentia. No parecen peligrosas, pero si antiguas.";

    [TextArea(2, 6)]
    [SerializeField] private string result2 =
        "La voz responde con una risa suave. No te da la solucion, pero pronuncia una palabra: \"recuerda\".";

    [TextArea(2, 6)]
    [SerializeField] private string result3 =
        "La puerta vibra, pero una corriente fria te obliga a retroceder. La fuerza no parece ser el camino.";

    [TextArea(2, 5)]
    [SerializeField] private string nextState =
        "Ahora sabes que la puerta no se abre con fuerza, sino con una respuesta.";

    private bool dialogueEnded;

    private void Start()
    {
        ShowInitialScene();
    }

    public void ChooseOption1()
    {
        ResolveOption(result1);
    }

    public void ChooseOption2()
    {
        ResolveOption(result2);
    }

    public void ChooseOption3()
    {
        ResolveOption(result3);
    }

    public void ResetDialogue()
    {
        ShowInitialScene();
    }

    private void ShowInitialScene()
    {
        dialogueEnded = false;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        SetText(sceneText, initialScene);
        SetText(stateText, "Elige una respuesta.");
        SetText(optionLabel1, option1);
        SetText(optionLabel2, option2);
        SetText(optionLabel3, option3);

        SetOptionsInteractable(true);

        if (resetButton != null)
        {
            resetButton.gameObject.SetActive(false);
        }
    }

    private void ResolveOption(string result)
    {
        if (dialogueEnded)
        {
            return;
        }

        dialogueEnded = true;
        SetText(sceneText, result + "\n\n" + nextState);
        SetText(stateText, "Consecuencia generada. Puedes reiniciar la escena narrativa.");
        SetOptionsInteractable(false);

        if (resetButton != null)
        {
            resetButton.gameObject.SetActive(true);
        }
    }

    private void SetOptionsInteractable(bool value)
    {
        if (optionButton1 != null) optionButton1.interactable = value;
        if (optionButton2 != null) optionButton2.interactable = value;
        if (optionButton3 != null) optionButton3.interactable = value;
    }

    private void SetText(Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }
}
