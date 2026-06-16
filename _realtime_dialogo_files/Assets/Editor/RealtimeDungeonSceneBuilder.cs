using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class RealtimeDungeonSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";

    public static void BuildScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateWorld();
        RealtimeDungeonChat chat = CreateChatUI();

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        Selection.activeObject = chat.gameObject;
    }

    private static void CreateWorld()
    {
        RenderSettings.ambientLight = new Color(0.18f, 0.16f, 0.19f);

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 3.1f, -7.6f);
        cameraObject.transform.rotation = Quaternion.Euler(22f, 0f, 0f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.055f, 0.052f, 0.065f);

        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.15f;
        lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Suelo de la sala";
        floor.transform.localScale = new Vector3(2.2f, 1f, 2.2f);
        floor.GetComponent<Renderer>().sharedMaterial = CreateMaterial("MAT_Suelo", new Color(0.16f, 0.15f, 0.17f));

        GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.name = "Muro del calabozo";
        backWall.transform.position = new Vector3(0f, 1.7f, 2.5f);
        backWall.transform.localScale = new Vector3(6.5f, 3.4f, 0.28f);
        backWall.GetComponent<Renderer>().sharedMaterial = CreateMaterial("MAT_Piedra", new Color(0.28f, 0.28f, 0.31f));

        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Puerta de piedra con runas";
        door.transform.position = new Vector3(0f, 1.45f, 2.28f);
        door.transform.localScale = new Vector3(1.8f, 2.7f, 0.18f);
        door.GetComponent<Renderer>().sharedMaterial = CreateMaterial("MAT_Puerta", new Color(0.20f, 0.20f, 0.22f));

        GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npc.name = "Amo del Calabozo";
        npc.transform.position = new Vector3(-2.35f, 1f, 0.95f);
        npc.transform.localScale = new Vector3(0.75f, 1.05f, 0.75f);
        npc.GetComponent<Renderer>().sharedMaterial = CreateMaterial("MAT_AmoDelCalabozo", new Color(0.46f, 0.04f, 0.08f));

        CreateTorch(new Vector3(-1.65f, 1.85f, 2.05f), "Antorcha izquierda");
        CreateTorch(new Vector3(1.65f, 1.85f, 2.05f), "Antorcha derecha");
    }

    private static RealtimeDungeonChat CreateChatUI()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        CreateEventSystem();

        GameObject chatObject = new GameObject("RealtimeDungeonChat");
        RealtimeDungeonChat chat = chatObject.AddComponent<RealtimeDungeonChat>();

        GameObject panel = CreateUIObject("ChatPanel", canvasObject.transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.045f, 0.045f, 0.055f, 0.92f);
        SetRect(panel.GetComponent<RectTransform>(), new Vector2(0.06f, 0.05f), new Vector2(0.94f, 0.72f));

        Text title = CreateText("TitleText", panel.transform, font, "Amo del Calabozo - ChatGPT en tiempo real", 34, FontStyle.Bold, TextAnchor.MiddleLeft);
        title.color = new Color(1f, 0.84f, 0.44f);
        SetRect(title.rectTransform, new Vector2(0.035f, 0.88f), new Vector2(0.965f, 0.98f));

        Text statusText = CreateText("StatusText", panel.transform, font, "", 20, FontStyle.Italic, TextAnchor.MiddleLeft);
        statusText.color = new Color(0.74f, 0.78f, 0.88f);
        SetRect(statusText.rectTransform, new Vector2(0.035f, 0.80f), new Vector2(0.965f, 0.88f));

        InputField apiKeyInput = CreateInputField(
            "ApiKeyInput",
            panel.transform,
            font,
            "Pega aqui tu OpenAI API key",
            19,
            new Vector2(0.035f, 0.705f),
            new Vector2(0.68f, 0.785f));
        apiKeyInput.contentType = InputField.ContentType.Password;

        Toggle rememberToggle = CreateToggle("RememberKeyToggle", panel.transform, font, "Guardar clave localmente", new Vector2(0.70f, 0.705f), new Vector2(0.965f, 0.785f));

        ScrollRect scrollRect = CreateScrollArea(panel.transform, font, out Text conversationText);
        SetRect(scrollRect.GetComponent<RectTransform>(), new Vector2(0.035f, 0.275f), new Vector2(0.965f, 0.685f));

        InputField playerInput = CreateInputField(
            "PlayerInput",
            panel.transform,
            font,
            "Escribe tu accion: examino las runas, pregunto por la puerta, enciendo una antorcha...",
            22,
            new Vector2(0.035f, 0.07f),
            new Vector2(0.72f, 0.245f));
        playerInput.lineType = InputField.LineType.MultiLineNewline;

        Button sendButton = CreateButton("SendButton", panel.transform, font, "Enviar", new Vector2(0.745f, 0.155f), new Vector2(0.965f, 0.245f), new Color(0.86f, 0.12f, 0.16f));
        Button clearButton = CreateButton("ClearButton", panel.transform, font, "Reiniciar", new Vector2(0.745f, 0.07f), new Vector2(0.965f, 0.14f), new Color(0.26f, 0.27f, 0.31f));

        UnityEventTools.AddPersistentListener(sendButton.onClick, chat.SendPlayerMessage);
        UnityEventTools.AddPersistentListener(clearButton.onClick, chat.ResetConversation);

        SerializedObject serialized = new SerializedObject(chat);
        SetReference(serialized, "apiKeyInput", apiKeyInput);
        SetReference(serialized, "rememberKeyToggle", rememberToggle);
        SetReference(serialized, "conversationText", conversationText);
        SetReference(serialized, "statusText", statusText);
        SetReference(serialized, "playerInput", playerInput);
        SetReference(serialized, "sendButton", sendButton);
        SetReference(serialized, "clearButton", clearButton);
        SetReference(serialized, "scrollRect", scrollRect);
        serialized.ApplyModifiedPropertiesWithoutUndo();

        return chat;
    }

    private static ScrollRect CreateScrollArea(Transform parent, Font font, out Text conversationText)
    {
        GameObject scrollObject = CreateUIObject("ConversationScroll", parent);
        Image background = scrollObject.AddComponent<Image>();
        background.color = new Color(0.09f, 0.09f, 0.105f, 0.94f);

        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        GameObject viewport = CreateUIObject("Viewport", scrollObject.transform);
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.02f);
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        SetRect(viewport.GetComponent<RectTransform>(), new Vector2(0.02f, 0.04f), new Vector2(0.98f, 0.96f));

        GameObject content = CreateUIObject("Content", viewport.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 900f);

        conversationText = CreateText("ConversationText", content.transform, font, "", 22, FontStyle.Normal, TextAnchor.UpperLeft);
        conversationText.color = Color.white;
        conversationText.horizontalOverflow = HorizontalWrapMode.Wrap;
        conversationText.verticalOverflow = VerticalWrapMode.Overflow;
        SetRect(conversationText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f));

        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = contentRect;
        Scrollbar verticalScrollbar = CreateVerticalScrollbar(scrollObject.transform);
        scrollRect.verticalScrollbar = verticalScrollbar;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        scrollRect.verticalScrollbarSpacing = -8f;

        return scrollRect;
    }

    private static Scrollbar CreateVerticalScrollbar(Transform parent)
    {
        GameObject scrollbarObject = CreateUIObject("VerticalScrollbar", parent);
        RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1f, 0f);
        scrollbarRect.anchorMax = new Vector2(1f, 1f);
        scrollbarRect.pivot = new Vector2(1f, 0.5f);
        scrollbarRect.anchoredPosition = Vector2.zero;
        scrollbarRect.sizeDelta = new Vector2(18f, 0f);

        Image background = scrollbarObject.AddComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0.12f);

        Scrollbar scrollbar = scrollbarObject.AddComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;

        GameObject handleObject = CreateUIObject("Handle", scrollbarObject.transform);
        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.offsetMin = new Vector2(3f, 3f);
        handleRect.offsetMax = new Vector2(-3f, -3f);

        Image handleImage = handleObject.AddComponent<Image>();
        handleImage.color = new Color(0.86f, 0.12f, 0.16f, 0.95f);

        scrollbar.targetGraphic = handleImage;
        scrollbar.handleRect = handleRect;

        return scrollbar;
    }

    private static InputField CreateInputField(string name, Transform parent, Font font, string placeholderText, int size, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = CreateUIObject(name, parent);
        SetRect(go.GetComponent<RectTransform>(), anchorMin, anchorMax);

        Image image = go.AddComponent<Image>();
        image.color = new Color(0.94f, 0.94f, 0.96f, 0.96f);

        InputField input = go.AddComponent<InputField>();
        input.targetGraphic = image;

        Text text = CreateText("Text", go.transform, font, "", size, FontStyle.Normal, TextAnchor.UpperLeft);
        text.color = new Color(0.08f, 0.08f, 0.1f);
        SetRect(text.rectTransform, new Vector2(0.025f, 0.12f), new Vector2(0.975f, 0.88f));

        Text placeholder = CreateText("Placeholder", go.transform, font, placeholderText, size, FontStyle.Italic, TextAnchor.UpperLeft);
        placeholder.color = new Color(0.45f, 0.45f, 0.50f);
        SetRect(placeholder.rectTransform, new Vector2(0.025f, 0.12f), new Vector2(0.975f, 0.88f));

        input.textComponent = text;
        input.placeholder = placeholder;

        return input;
    }

    private static Button CreateButton(string name, Transform parent, Font font, string label, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject go = CreateUIObject(name, parent);
        SetRect(go.GetComponent<RectTransform>(), anchorMin, anchorMax);

        Image image = go.AddComponent<Image>();
        image.color = color;

        Button button = go.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.25f);
        colors.disabledColor = new Color(0.25f, 0.25f, 0.27f);
        button.colors = colors;

        Text text = CreateText("Label", go.transform, font, label, 23, FontStyle.Bold, TextAnchor.MiddleCenter);
        text.color = Color.white;
        SetRect(text.rectTransform, new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.92f));

        return button;
    }

    private static Toggle CreateToggle(string name, Transform parent, Font font, string label, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = CreateUIObject(name, parent);
        SetRect(go.GetComponent<RectTransform>(), anchorMin, anchorMax);

        Toggle toggle = go.AddComponent<Toggle>();

        GameObject bg = CreateUIObject("Background", go.transform);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.92f, 0.92f, 0.95f);
        SetRect(bg.GetComponent<RectTransform>(), new Vector2(0f, 0.22f), new Vector2(0.12f, 0.78f));

        GameObject check = CreateUIObject("Checkmark", bg.transform);
        Image checkImage = check.AddComponent<Image>();
        checkImage.color = new Color(0.86f, 0.12f, 0.16f);
        SetRect(check.GetComponent<RectTransform>(), new Vector2(0.22f, 0.22f), new Vector2(0.78f, 0.78f));

        Text text = CreateText("Label", go.transform, font, label, 18, FontStyle.Normal, TextAnchor.MiddleLeft);
        text.color = Color.white;
        SetRect(text.rectTransform, new Vector2(0.16f, 0f), new Vector2(1f, 1f));

        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;
        return toggle;
    }

    private static Text CreateText(string name, Transform parent, Font font, string value, int size, FontStyle style, TextAnchor anchor)
    {
        GameObject go = CreateUIObject(name, parent);
        Text text = go.AddComponent<Text>();
        text.text = value;
        text.font = font;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetReference(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
#if ENABLE_INPUT_SYSTEM
        eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
    }

    private static void CreateTorch(Vector3 position, string name)
    {
        GameObject torch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        torch.name = name;
        torch.transform.position = position;
        torch.transform.localScale = new Vector3(0.08f, 0.45f, 0.08f);
        torch.GetComponent<Renderer>().sharedMaterial = CreateMaterial("MAT_Antorcha", new Color(0.33f, 0.14f, 0.04f));

        GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.name = name + " - llama";
        flame.transform.position = position + new Vector3(0f, 0.48f, -0.04f);
        flame.transform.localScale = new Vector3(0.22f, 0.32f, 0.22f);
        flame.GetComponent<Renderer>().sharedMaterial = CreateMaterial("MAT_Llama", new Color(1f, 0.42f, 0.08f));

        GameObject lightObject = new GameObject(name + " - luz");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.58f, 0.25f);
        light.intensity = 3f;
        light.range = 4.5f;
        lightObject.transform.position = flame.transform.position;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.name = name;
        material.color = color;
        return material;
    }
}
