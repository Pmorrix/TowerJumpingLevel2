using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RealtimeDungeonChat : MonoBehaviour
{
    [Header("OpenAI")]
    [SerializeField] private string apiUrl = "https://api.openai.com/v1/responses";
    [SerializeField] private string model = "gpt-4.1-mini";
    [SerializeField] private int maxOutputTokens = 220;

    [TextArea(5, 12)]
    [SerializeField] private string systemPrompt =
        "Eres el Amo del Calabozo de una aventura conversacional de fantasia medieval.\n" +
        "Hablas en español claro, con tono misterioso pero apto para todos los publicos.\n" +
        "Responde siempre como narrador y NPC, no como asistente tecnico.\n" +
        "Cada respuesta debe tener 2 partes:\n" +
        "1. una consecuencia narrativa breve de la accion del jugador\n" +
        "2. una nueva pregunta o decision para continuar la aventura\n" +
        "No escribas respuestas largas. Maximo 120 palabras.\n" +
        "No menciones que eres una IA ni que usas un modelo de lenguaje.";

    [Header("UI")]
    [SerializeField] private InputField apiKeyInput;
    [SerializeField] private Toggle rememberKeyToggle;
    [SerializeField] private Text conversationText;
    [SerializeField] private Text statusText;
    [SerializeField] private InputField playerInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Escena inicial")]
    [TextArea(3, 8)]
    [SerializeField] private string openingMessage =
        "La puerta de piedra se cierra a tu espalda.\n" +
        "Una figura encapuchada alza la mirada desde el fondo de la sala.\n\n" +
        "\"Bienvenido, viajero. Dime que haces, y el calabozo respondera.\"";

    private const string ApiKeyPrefsKey = "ClaseIA.OpenAI.ApiKey";

    private readonly StringBuilder transcript = new StringBuilder();
    private bool requestInProgress;

    private void Start()
    {

        if (sendButton != null)
        {
            sendButton.onClick.AddListener(SendPlayerMessage);
        }
        else
        {
        }

        if (clearButton != null)
        {
            clearButton.onClick.AddListener(ResetConversation);
        }
        else
        {
        }


        string savedKey = PlayerPrefs.GetString(ApiKeyPrefsKey, "");
        if (apiKeyInput != null)
        {
            apiKeyInput.text = savedKey;
        }

        if (rememberKeyToggle != null)
        {
            rememberKeyToggle.isOn = !string.IsNullOrWhiteSpace(savedKey);
        }

        EnsureVerticalScrollbar();
        PrepareScrollBox();
        ResetConversation();
    }

    public void SendPlayerMessage()
    {

        if (requestInProgress || playerInput == null)
        {
            return;
        }

        string message = playerInput.text.Trim();

        if (string.IsNullOrWhiteSpace(message))
        {
            SetStatus("Escribe una accion antes de enviar.");
            return;
        }

        string apiKey = apiKeyInput != null ? apiKeyInput.text.Trim() : "";
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            SetStatus("Falta la API key de OpenAI.");
            return;
        }

        SaveApiKeyIfNeeded(apiKey);
        playerInput.text = "";
        AppendMessage("Jugador", message);
        StartCoroutine(SendToOpenAI(apiKey, message));
    }

    public void ResetConversation()
    {
        transcript.Clear();
        AppendMessage("Amo del Calabozo", openingMessage);
        SetStatus("Escribe una accion y pulsa Enviar.");
        SetInteractable(true);
    }

    private IEnumerator SendToOpenAI(string apiKey, string latestPlayerMessage)
    {
        requestInProgress = true;
        SetInteractable(false);
        SetStatus("El Amo del Calabozo esta pensando...");

        string input = BuildConversationInput(latestPlayerMessage);
        string json = BuildRequestJson(input);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            string responseText = request.downloadHandler != null ? request.downloadHandler.text : "";

            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorMessage = OpenAIResponseTextExtractor.Extract(responseText);
                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = request.error;
                }

                AppendMessage("Sistema", "Error de API: " + errorMessage);
                SetStatus("No se pudo obtener respuesta. Revisa API key, internet, modelo y consola.");
            }
            else
            {
                string output = OpenAIResponseTextExtractor.Extract(responseText);
                if (string.IsNullOrWhiteSpace(output))
                {
                    output = "La API respondio, pero no se pudo leer el texto de salida.";
                }

                AppendMessage("Amo del Calabozo", output.Trim());
                SetStatus("Respuesta recibida. Puedes continuar.");
            }
        }

        requestInProgress = false;
        SetInteractable(true);
        FocusPlayerInput();
    }

    private string BuildConversationInput(string latestPlayerMessage)
    {
        return
            "Historial de la aventura hasta ahora:\n" +
            transcript +
            "\nUltima accion del jugador:\n" +
            latestPlayerMessage +
            "\n\nContinua la aventura con una respuesta breve y una nueva decision.";
    }

    private string BuildRequestJson(string input)
    {
        OpenAIResponseRequest request = new OpenAIResponseRequest
        {
            model = model,
            instructions = systemPrompt,
            input = input,
            max_output_tokens = Mathf.Max(40, maxOutputTokens)
        };

        return JsonUtility.ToJson(request);
    }

    private void AppendMessage(string speaker, string message)
    {
        if (transcript.Length > 0)
        {
            transcript.AppendLine();
            transcript.AppendLine();
        }

        transcript.AppendLine(speaker + ":");
        transcript.AppendLine(message);

        if (conversationText != null)
        {
            conversationText.text = transcript.ToString();
            UpdateScrollContent();
        }

        StartCoroutine(ScrollToBottomNextFrame());
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        UpdateScrollContent();

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void UpdateScrollContent()
    {
        if (conversationText == null || scrollRect == null || scrollRect.content == null)
        {
            return;
        }

        RectTransform viewport = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();
        if (viewport == null)
        {
            return;
        }

        ContentSizeFitter fitter = scrollRect.content.GetComponent<ContentSizeFitter>();
        if (fitter != null)
        {
            Destroy(fitter);
        }

        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

        RectTransform contentRect = scrollRect.content;
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;

        float viewportHeight = Mathf.Max(1f, viewport.rect.height);
        float contentHeight = Mathf.Max(viewportHeight, conversationText.preferredHeight + 28f);
        contentRect.sizeDelta = new Vector2(0f, contentHeight);

        RectTransform textRect = conversationText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.offsetMin = new Vector2(18f, 12f);
        textRect.offsetMax = new Vector2(-26f, -12f);
    }

    private void PrepareScrollBox()
    {
        if (scrollRect == null)
        {
            return;
        }

        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

        if (scrollRect.content != null)
        {
            ContentSizeFitter fitter = scrollRect.content.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                Destroy(fitter);
            }

            scrollRect.content.anchorMin = new Vector2(0f, 1f);
            scrollRect.content.anchorMax = new Vector2(1f, 1f);
            scrollRect.content.pivot = new Vector2(0.5f, 1f);
            scrollRect.content.anchoredPosition = Vector2.zero;
        }

        if (conversationText != null)
        {
            conversationText.alignment = TextAnchor.UpperLeft;
            conversationText.horizontalOverflow = HorizontalWrapMode.Wrap;
            conversationText.verticalOverflow = VerticalWrapMode.Overflow;
        }
    }

    private void EnsureVerticalScrollbar()
    {
        if (scrollRect == null)
        {
            return;
        }

        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        scrollRect.verticalScrollbarSpacing = -8f;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        if (scrollRect.verticalScrollbar != null)
        {
            return;
        }

        Transform existing = scrollRect.transform.Find("VerticalScrollbar");
        Scrollbar scrollbar = existing != null ? existing.GetComponent<Scrollbar>() : null;

        if (scrollbar == null)
        {
            GameObject scrollbarObject = new GameObject("VerticalScrollbar");
            scrollbarObject.transform.SetParent(scrollRect.transform, false);

            RectTransform scrollbarRect = scrollbarObject.AddComponent<RectTransform>();
            scrollbarRect.anchorMin = new Vector2(1f, 0f);
            scrollbarRect.anchorMax = new Vector2(1f, 1f);
            scrollbarRect.pivot = new Vector2(1f, 0.5f);
            scrollbarRect.anchoredPosition = Vector2.zero;
            scrollbarRect.sizeDelta = new Vector2(18f, 0f);

            Image background = scrollbarObject.AddComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.12f);

            scrollbar = scrollbarObject.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            GameObject handleObject = new GameObject("Handle");
            handleObject.transform.SetParent(scrollbarObject.transform, false);

            RectTransform handleRect = handleObject.AddComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.offsetMin = new Vector2(3f, 3f);
            handleRect.offsetMax = new Vector2(-3f, -3f);

            Image handleImage = handleObject.AddComponent<Image>();
            handleImage.color = new Color(0.86f, 0.12f, 0.16f, 0.95f);

            scrollbar.targetGraphic = handleImage;
            scrollbar.handleRect = handleRect;
        }

        scrollRect.verticalScrollbar = scrollbar;
    }

    private void SetStatus(string value)
    {
        if (statusText != null)
        {
            statusText.text = value;
        }
    }

    private void SetInteractable(bool value)
    {
        if (sendButton != null) sendButton.interactable = value;
        if (clearButton != null) clearButton.interactable = value;
        if (playerInput != null) playerInput.interactable = value;
        if (apiKeyInput != null) apiKeyInput.interactable = value;
        if (rememberKeyToggle != null) rememberKeyToggle.interactable = value;
    }

    private void SaveApiKeyIfNeeded(string apiKey)
    {
        if (rememberKeyToggle != null && rememberKeyToggle.isOn)
        {
            PlayerPrefs.SetString(ApiKeyPrefsKey, apiKey);
            PlayerPrefs.Save();
        }
        else
        {
            PlayerPrefs.DeleteKey(ApiKeyPrefsKey);
        }
    }

    private void FocusPlayerInput()
    {
        if (playerInput != null)
        {
            playerInput.ActivateInputField();
        }
    }

    [System.Serializable]
    private class OpenAIResponseRequest
    {
        public string model;
        public string instructions;
        public string input;
        public int max_output_tokens;
    }
}
