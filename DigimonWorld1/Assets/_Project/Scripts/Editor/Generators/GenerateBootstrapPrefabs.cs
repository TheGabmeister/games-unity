using Eflatun.SceneReference;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.Video;

public static class GenerateBootstrapPrefabs
{
    private const string PrefabDir = "Assets/_Project/Prefabs";
    private const string BootstrapperPrefabPath = PrefabDir + "/Bootstrapper.prefab";
    private const string AudioSystemPrefabPath = PrefabDir + "/AudioSystem.prefab";
    private const string GameManagerPrefabPath = PrefabDir + "/GameManager.prefab";
    private const string SplashscreenControllerPrefabPath = PrefabDir + "/SplashscreenController.prefab";
    private const string IntroControllerPrefabPath = PrefabDir + "/IntroController.prefab";
    private const string MainMenuControllerPrefabPath = PrefabDir + "/MainMenuController.prefab";
    private const string NameControllerPrefabPath = PrefabDir + "/NameController.prefab";

    private const string SplashscreenScenePath = "Assets/_Project/Scenes/_Splashscreen.unity";
    private const string IntroScenePath = "Assets/_Project/Scenes/_Intro.unity";
    private const string MainMenuScenePath = "Assets/_Project/Scenes/_MainMenu.unity";
    private const string NameScenePath = "Assets/_Project/Scenes/_Name.unity";
    private const string GameplayScenePath = "Assets/_Project/Scenes/_Gameplay.unity";
    private const string IntroVideoPath = "Assets/_Project/Videos/IntroVideo.mp4";

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate Bootstrapper")]
    public static void GenerateBootstrapper()
    {
        SavePrefab("Bootstrapper", BootstrapperPrefabPath, go =>
        {
            //go.AddComponent<Bootstrapper>();
        });
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate AudioSystem")]
    public static void GenerateAudioSystem()
    {
        SavePrefab("AudioSystem", AudioSystemPrefabPath, go => go.AddComponent<AudioSystem>());
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate GameManager")]
    public static void GenerateGameManager()
    {
        SavePrefab("GameManager", GameManagerPrefabPath, go => go.AddComponent<GameManager>());

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameManagerPrefabPath);
        GameManager gm = prefab.GetComponent<GameManager>();
        SerializedObject so = new SerializedObject(gm);

        SetSceneReference(so, "_splashscreenScene", SplashscreenScenePath);
        SetSceneReference(so, "_introScene", IntroScenePath);
        SetSceneReference(so, "_mainMenuScene", MainMenuScenePath);
        SetSceneReference(so, "_nameScene", NameScenePath);
        SetSceneReference(so, "_gameplayScene", GameplayScenePath);

        so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate SplashscreenController")]
    public static void GenerateSplashscreenController()
    {
        SavePrefab("SplashscreenController", SplashscreenControllerPrefabPath, go =>
        {
            go.AddComponent<SplashscreenController>();
        });
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate IntroController")]
    public static void GenerateIntroController()
    {
        SavePrefab("IntroController", IntroControllerPrefabPath, go =>
        {
            VideoPlayer vp = go.AddComponent<VideoPlayer>();
            vp.playOnAwake = true;
            vp.renderMode = VideoRenderMode.CameraNearPlane;

            VideoClip clip = AssetDatabase.LoadAssetAtPath<VideoClip>(IntroVideoPath);
            if (clip != null)
                vp.clip = clip;
            else
                Debug.LogWarning($"Intro video not found at {IntroVideoPath} — VideoPlayer clip will be empty.");

            IntroController controller = go.AddComponent<IntroController>();

            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("_videoPlayer").objectReferenceValue = vp;
            so.ApplyModifiedPropertiesWithoutUndo();
        });
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate MainMenuController")]
    public static void GenerateMainMenuController()
    {
        EnsureFolder(PrefabDir);

        GameObject root = new GameObject("MainMenuController");
        try
        {
            // Canvas
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            root.AddComponent<GraphicRaycaster>();

            // EventSystem
            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.transform.SetParent(root.transform, false);
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();

            // Press Start Panel
            GameObject pressStartPanel = CreatePanel("PressStartPanel", root.transform);

            TMP_Text pressStartTitle = CreateText("Title", pressStartPanel.transform,
                "DIGIMON WORLD", 72, FontStyles.Bold, TextAlignmentOptions.Center,
                new Vector2(0f, 80f), new Vector2(800f, 100f));
            pressStartTitle.color = Color.white;

            TMP_Text pressStartText = CreateText("PressStartText", pressStartPanel.transform,
                "Press Start", 36, FontStyles.Normal, TextAlignmentOptions.Center,
                new Vector2(0f, -60f), new Vector2(400f, 50f));
            pressStartText.color = Color.white;

            // Menu Panel (hidden by default)
            GameObject menuPanel = CreatePanel("MenuPanel", root.transform);
            menuPanel.SetActive(false);

            TMP_Text menuTitle = CreateText("Title", menuPanel.transform,
                "DIGIMON WORLD", 72, FontStyles.Bold, TextAlignmentOptions.Center,
                new Vector2(0f, 160f), new Vector2(800f, 100f));
            menuTitle.color = Color.white;

            string[] labels = { "New Game", "Continue Game", "Delete Game", "Battle Mode" };
            TMP_Text[] optionTexts = new TMP_Text[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                string prefix = (i == 0) ? "> " : "  ";
                TMP_Text optionText = CreateText(labels[i].Replace(" ", ""), menuPanel.transform,
                    prefix + labels[i], 36, FontStyles.Normal, TextAlignmentOptions.MidlineLeft,
                    new Vector2(-60f, 40f - i * 50f), new Vector2(400f, 50f));
                optionText.color = (i == 0) ? Color.white : Color.gray;
                optionTexts[i] = optionText;
            }

            // Controller
            MainMenuController controller = root.AddComponent<MainMenuController>();

            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("_pressStartPanel").objectReferenceValue = pressStartPanel;
            so.FindProperty("_menuPanel").objectReferenceValue = menuPanel;
            so.FindProperty("_pressStartText").objectReferenceValue = pressStartText;

            SerializedProperty menuOptionsProp = so.FindProperty("_menuOptionTexts");
            menuOptionsProp.arraySize = optionTexts.Length;
            for (int i = 0; i < optionTexts.Length; i++)
                menuOptionsProp.GetArrayElementAtIndex(i).objectReferenceValue = optionTexts[i];

            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, MainMenuControllerPrefabPath, out bool success);
            if (!success)
            {
                Debug.LogError($"Failed to save prefab at {MainMenuControllerPrefabPath}");
                return;
            }
        }
        finally
        {
            Object.DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Prefab generated at {MainMenuControllerPrefabPath}");
    }

    private static GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return panel;
    }

    private static TMP_Text CreateText(string name, Transform parent, string text,
        int fontSize, FontStyles fontStyle, TextAlignmentOptions alignment,
        Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = fontStyle;
        tmp.alignment = alignment;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        return tmp;
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate NameController")]
    public static void GenerateNameController()
    {
        EnsureFolder(PrefabDir);

        GameObject root = new GameObject("NameController");
        try
        {
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            root.AddComponent<GraphicRaycaster>();

            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.transform.SetParent(root.transform, false);
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();

            CreateText("Title", root.transform,
                "Enter Names", 48, FontStyles.Bold, TextAlignmentOptions.Center,
                new Vector2(0f, 200f), new Vector2(600f, 70f));

            CreateText("PlayerNameLabel", root.transform,
                "Player Name", 28, FontStyles.Normal, TextAlignmentOptions.MidlineLeft,
                new Vector2(-100f, 80f), new Vector2(300f, 40f));

            TMP_InputField playerNameInput = CreateInputField("PlayerNameInput", root.transform,
                new Vector2(0f, 30f), new Vector2(400f, 50f));

            CreateText("DigimonNameLabel", root.transform,
                "Digimon Name", 28, FontStyles.Normal, TextAlignmentOptions.MidlineLeft,
                new Vector2(-100f, -40f), new Vector2(300f, 40f));

            TMP_InputField digimonNameInput = CreateInputField("DigimonNameInput", root.transform,
                new Vector2(0f, -90f), new Vector2(400f, 50f));

            // Confirm button
            GameObject buttonGo = new GameObject("ConfirmButton", typeof(RectTransform));
            buttonGo.transform.SetParent(root.transform, false);
            Image buttonImage = buttonGo.AddComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            Button button = buttonGo.AddComponent<Button>();
            RectTransform buttonRt = buttonGo.GetComponent<RectTransform>();
            buttonRt.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRt.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRt.anchoredPosition = new Vector2(0f, -180f);
            buttonRt.sizeDelta = new Vector2(200f, 50f);

            TMP_Text buttonText = CreateText("Text", buttonGo.transform,
                "Confirm", 28, FontStyles.Bold, TextAlignmentOptions.Center,
                Vector2.zero, new Vector2(200f, 50f));
            buttonText.color = Color.white;

            // Controller
            NameController controller = root.AddComponent<NameController>();

            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("_playerNameInput").objectReferenceValue = playerNameInput;
            so.FindProperty("_digimonNameInput").objectReferenceValue = digimonNameInput;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Wire button onClick to controller.OnConfirm
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                button.onClick,
                new UnityEngine.Events.UnityAction(controller.OnConfirm));

            PrefabUtility.SaveAsPrefabAsset(root, NameControllerPrefabPath, out bool success);
            if (!success)
            {
                Debug.LogError($"Failed to save prefab at {NameControllerPrefabPath}");
                return;
            }
        }
        finally
        {
            Object.DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Prefab generated at {NameControllerPrefabPath}");
    }

    private static TMP_InputField CreateInputField(string name, Transform parent,
        Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        Image bg = go.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        TMP_InputField inputField = go.AddComponent<TMP_InputField>();

        // Text area
        GameObject textArea = new GameObject("Text Area", typeof(RectTransform));
        textArea.transform.SetParent(go.transform, false);
        RectTransform textAreaRt = textArea.GetComponent<RectTransform>();
        textAreaRt.anchorMin = Vector2.zero;
        textAreaRt.anchorMax = Vector2.one;
        textAreaRt.offsetMin = new Vector2(10f, 0f);
        textAreaRt.offsetMax = new Vector2(-10f, 0f);
        textArea.AddComponent<RectMask2D>();

        // Placeholder
        GameObject placeholderGo = new GameObject("Placeholder", typeof(RectTransform));
        placeholderGo.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI placeholder = placeholderGo.AddComponent<TextMeshProUGUI>();
        placeholder.text = "Enter name...";
        placeholder.fontSize = 24;
        placeholder.fontStyle = FontStyles.Italic;
        placeholder.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
        RectTransform placeholderRt = placeholderGo.GetComponent<RectTransform>();
        placeholderRt.anchorMin = Vector2.zero;
        placeholderRt.anchorMax = Vector2.one;
        placeholderRt.offsetMin = Vector2.zero;
        placeholderRt.offsetMax = Vector2.zero;

        // Input text
        GameObject textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI inputText = textGo.AddComponent<TextMeshProUGUI>();
        inputText.fontSize = 24;
        inputText.color = Color.white;
        inputText.alignment = TextAlignmentOptions.MidlineLeft;
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        inputField.textViewport = textAreaRt;
        inputField.textComponent = inputText;
        inputField.placeholder = placeholder;
        inputField.characterLimit = 8;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        return inputField;
    }

    private static void SetSceneReference(SerializedObject so, string fieldName, string scenePath)
    {
        string guid = AssetDatabase.AssetPathToGUID(scenePath);
        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogWarning($"Scene not found at {scenePath} — {fieldName} will be empty. Generate scenes first.");
            return;
        }

        SerializedProperty prop = so.FindProperty(fieldName);
        SerializedProperty guidProp = prop.FindPropertyRelative("guid");
        SerializedProperty assetProp = prop.FindPropertyRelative("asset");

        guidProp.stringValue = guid;
        assetProp.objectReferenceValue = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
    }

    private static void SavePrefab(string name, string path, System.Action<GameObject> configure)
    {
        EnsureFolder(PrefabDir);

        GameObject source = new GameObject(name);
        try
        {
            configure(source);
            PrefabUtility.SaveAsPrefabAsset(source, path, out bool success);
            if (!success)
            {
                Debug.LogError($"Failed to save prefab at {path}");
                return;
            }
        }
        finally
        {
            Object.DestroyImmediate(source);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Prefab generated at {path}");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
