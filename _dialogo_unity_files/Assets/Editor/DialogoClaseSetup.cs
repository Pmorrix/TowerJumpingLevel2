using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DialogoClaseSetup
{
    private const string ScenePath = "Assets/Scenes/AmoDelCalabozo_Dialogo.unity";

    [MenuItem("Clase IA Dialogo/Crear escena Amo del Calabozo")]
    public static void CreateDialogueScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateWorld();
        DialogueManager manager = CreateDialogueUI();

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);

        Selection.activeObject = manager.gameObject;
        EditorGUIUtility.PingObject(manager.gameObject);
        Debug.Log("Escena de dialogo creada: " + ScenePath);
    }

    private static void CreateWorld()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 3.2f, -7.2f);
        cameraObject.transform.rotation = Quaternion.Euler(23f, 0f, 0f);
        camera.backgroundColor = new Color(0.08f, 0.08f, 0.1f);

        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightObject.transform.rotation = Quaternion.Euler(45f, -25f, 0f);

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Suelo";
        floor.transform.localScale = new Vector3(1.8f, 1f, 1.8f);

        Material floorMaterial = CreateMaterial("MAT_Suelo_Oscuro", new Color(0.18f, 0.18f, 0.2f));
        floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;

        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Puerta de piedra";
        door.transform.position = new Vector3(0f, 1.6f, 1.6f);
        door.transform.localScale = new Vector3(2.2f, 3.2f, 0.25f);
        door.GetComponent<Renderer>().sharedMaterial = CreateMaterial("MAT_Piedra", new Color(0.28f, 0.28f, 0.31f));

        GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npc.name = "Amo del Calabozo";
        npc.transform.position = new Vector3(-2.2f, 1f, 0.6f);
        npc.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        npc.GetComponent<Renderer>().sharedMaterial = CreateMaterial("MAT_AmoDelCalabozo", new Color(0.45f, 0.07f, 0.09f));

        GameObject torchLeft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        torchLeft.name = "Antorcha izquierda";
        torchLeft.transform.position = new Vector3(-1.55f, 1.9f, 1.25f);
        torchLeft.transform.localScale = new Vector3(0.08f, 0.5f, 0.08f);
        torchLeft.GetComponent<Renderer>().sharedMaterial = CreateMaterial("MAT_Antorcha", new Color(0.25f, 0.12f, 0.05f));

        GameObject torchRight = Object.Instantiate(torchLeft);
        torchRight.name = "Antorcha derecha";
        torchRight.transform.position = new Vector3(1.55f, 1.9f, 1.25f);

        CreatePointLight("Luz antorcha izquierda", torchLeft.transform.position + new Vector3(0f, 0.2f, -0.2f));
        CreatePointLight("Luz antorcha derecha", torchRight.transform.position + new Vector3(0f, 0.2f, -0.2f));
    }

    private static DialogueManager CreateDialogueUI()
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

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
#if ENABLE_INPUT_SYSTEM
        eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif

        GameObject managerObject = new GameObject("DialogueManager");
        DialogueManager manager = managerObject.AddComponent<DialogueManager>();

        GameObject panel = CreateUIObject("DialoguePanel", canvasObject.transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.06f, 0.88f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.08f, 0.06f);
        panelRect.anchorMax = new Vector2(0.92f, 0.46f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Text title = CreateText("TitleText", panel.transform, font, "Amo del Calabozo", 34, FontStyle.Bold, TextAnchor.UpperLeft);
        SetRect(title.rectTransform, new Vector2(0.04f, 0.78f), new Vector2(0.96f, 0.96f));
        title.color = new Color(0.95f, 0.82f, 0.45f);

        Text sceneText = CreateText("SceneText", panel.transform, font, "", 26, FontStyle.Normal, TextAnchor.UpperLeft);
        SetRect(sceneText.rectTransform, new Vector2(0.04f, 0.37f), new Vector2(0.96f, 0.78f));
        sceneText.color = Color.white;

        Text stateText = CreateText("StateText", panel.transform, font, "", 20, FontStyle.Italic, TextAnchor.UpperLeft);
        SetRect(stateText.rectTransform, new Vector2(0.04f, 0.25f), new Vector2(0.96f, 0.35f));
        stateText.color = new Color(0.78f, 0.82f, 0.9f);

        Button b1 = CreateButton("OptionButton1", panel.transform, font, "Opcion 1", new Vector2(0.04f, 0.07f), new Vector2(0.31f, 0.22f));
        Button b2 = CreateButton("OptionButton2", panel.transform, font, "Opcion 2", new Vector2(0.365f, 0.07f), new Vector2(0.635f, 0.22f));
        Button b3 = CreateButton("OptionButton3", panel.transform, font, "Opcion 3", new Vector2(0.69f, 0.07f), new Vector2(0.96f, 0.22f));
        Button reset = CreateButton("ResetButton", panel.transform, font, "Reiniciar dialogo", new Vector2(0.365f, 0.07f), new Vector2(0.635f, 0.22f));
        reset.gameObject.SetActive(false);

        UnityEventTools.AddPersistentListener(b1.onClick, manager.ChooseOption1);
        UnityEventTools.AddPersistentListener(b2.onClick, manager.ChooseOption2);
        UnityEventTools.AddPersistentListener(b3.onClick, manager.ChooseOption3);
        UnityEventTools.AddPersistentListener(reset.onClick, manager.ResetDialogue);

        SerializedObject serialized = new SerializedObject(manager);
        SetReference(serialized, "dialoguePanel", panel);
        SetReference(serialized, "sceneText", sceneText);
        SetReference(serialized, "stateText", stateText);
        SetReference(serialized, "optionButton1", b1);
        SetReference(serialized, "optionButton2", b2);
        SetReference(serialized, "optionButton3", b3);
        SetReference(serialized, "resetButton", reset);
        SetReference(serialized, "optionLabel1", b1.GetComponentInChildren<Text>());
        SetReference(serialized, "optionLabel2", b2.GetComponentInChildren<Text>());
        SetReference(serialized, "optionLabel3", b3.GetComponentInChildren<Text>());
        serialized.ApplyModifiedPropertiesWithoutUndo();

        return manager;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
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

    private static Button CreateButton(string name, Transform parent, Font font, string label, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = CreateUIObject(name, parent);
        SetRect(go.GetComponent<RectTransform>(), anchorMin, anchorMax);

        Image image = go.AddComponent<Image>();
        image.color = new Color(0.86f, 0.12f, 0.16f);

        Button button = go.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.86f, 0.12f, 0.16f);
        colors.highlightedColor = new Color(1f, 0.2f, 0.24f);
        colors.pressedColor = new Color(0.65f, 0.06f, 0.08f);
        colors.disabledColor = new Color(0.28f, 0.28f, 0.3f);
        button.colors = colors;

        Text text = CreateText("Label", go.transform, font, label, 20, FontStyle.Bold, TextAnchor.MiddleCenter);
        SetRect(text.rectTransform, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.92f));
        text.color = Color.white;

        return button;
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

    private static void CreatePointLight(string name, Vector3 position)
    {
        GameObject lightObject = new GameObject(name);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.55f, 0.22f);
        light.intensity = 2.5f;
        light.range = 4f;
        lightObject.transform.position = position;
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
