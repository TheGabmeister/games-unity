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
        SavePrefab("NameController", NameControllerPrefabPath, go =>
        {
            go.AddComponent<NameController>();
        });
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
