using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public static class GeneratePrefabs
{
    private const string BootstrapperPrefabPath = PrefabGeneratorUtils.PrefabDir + "/Bootstrapper.prefab";
    private const string AudioSystemPrefabPath = PrefabGeneratorUtils.PrefabDir + "/AudioSystem.prefab";
    private const string GameManagerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/GameManager.prefab";
    private const string ScreenFaderPrefabPath = PrefabGeneratorUtils.PrefabDir + "/ScreenFader.prefab";
    private const string SplashscreenControllerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/SplashscreenController.prefab";
    private const string IntroControllerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/IntroController.prefab";
    private const string PlayerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/Player.prefab";
    private const string AgumonPrefabPath = PrefabGeneratorUtils.PrefabDir + "/Agumon.prefab";
    private const string NPCPrefabPath = PrefabGeneratorUtils.PrefabDir + "/NPC.prefab";
    private const string InputManagerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/InputManager.prefab";
    private const string SceneLoaderPrefabPath = PrefabGeneratorUtils.PrefabDir + "/SceneLoader.prefab";
    private const string TestDialoguePath = "Assets/_Project/Data/TestDialogue.asset";
    private const string BootstrapConfigPath = "Assets/_Project/Resources/BootstrapConfig.asset";
    private const string Zone1DataPath = "Assets/_Project/Data/Zones/Zone1.asset";
    private const string Zone2DataPath = "Assets/_Project/Data/Zones/Zone2.asset";
    private const string Zone1ScenePath = "Assets/_Project/Scenes/Zones/Zone1.unity";
    private const string Zone2ScenePath = "Assets/_Project/Scenes/Zones/Zone2.unity";

    private const string BootstrapScenePath = "Assets/_Project/Scenes/_Bootstrap.unity";
    private const string SplashscreenScenePath = "Assets/_Project/Scenes/_Splashscreen.unity";
    private const string IntroScenePath = "Assets/_Project/Scenes/_Intro.unity";
    private const string MainMenuScenePath = "Assets/_Project/Scenes/_MainMenu.unity";
    private const string NameScenePath = "Assets/_Project/Scenes/_Name.unity";
    private const string GameplayScenePath = "Assets/_Project/Scenes/_Gameplay.unity";
    private const string IntroVideoPath = "Assets/_Project/Videos/IntroVideo.mp4";
    private const string PlayerModelPath = "Assets/_Project/Player/Player.fbx";
    private const string AgumonModelPath = "Assets/_Project/Digimons/Agumon/Agumon.fbx";

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate Bootstrapper")]
    public static void GenerateBootstrapper()
    {
        PrefabGeneratorUtils.SavePrefab("Bootstrapper", BootstrapperPrefabPath, go =>
        {
            //go.AddComponent<Bootstrapper>();
        });
    }

    [MenuItem("Tools/DigimonWorld/Data/Generate BootstrapConfig")]
    public static void GenerateBootstrapConfig()
    {
        PrefabGeneratorUtils.EnsureFolder("Assets/_Project/Resources");

        BootstrapConfig config = ScriptableObject.CreateInstance<BootstrapConfig>();

        SerializedObject so = new SerializedObject(config);
        PrefabGeneratorUtils.SetSceneReference(so, "_bootstrapScene", BootstrapScenePath);
        PrefabGeneratorUtils.SetSceneReference(so, "_gameplayScene", GameplayScenePath);
        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(config, BootstrapConfigPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"BootstrapConfig asset generated at {BootstrapConfigPath}");
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate AudioSystem")]
    public static void GenerateAudioSystem()
    {
        PrefabGeneratorUtils.SavePrefab("AudioSystem", AudioSystemPrefabPath, go => go.AddComponent<AudioSystem>());
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate InputManager")]
    public static void GenerateInputManager()
    {
        PrefabGeneratorUtils.SavePrefab("InputManager", InputManagerPrefabPath, go => go.AddComponent<InputManager>());
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate SceneLoader")]
    public static void GenerateSceneLoader()
    {
        PrefabGeneratorUtils.SavePrefab("SceneLoader", SceneLoaderPrefabPath, go => go.AddComponent<SceneLoader>());
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate ScreenFader")]
    public static void GenerateScreenFader()
    {
        PrefabGeneratorUtils.EnsureFolder(PrefabGeneratorUtils.PrefabDir);

        GameObject root = new GameObject("ScreenFader");
        try
        {
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            GameObject imageGo = new GameObject("FadeImage", typeof(RectTransform));
            imageGo.transform.SetParent(root.transform, false);
            Image fadeImage = imageGo.AddComponent<Image>();
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            fadeImage.raycastTarget = false;
            RectTransform rt = imageGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            ScreenFader fader = root.AddComponent<ScreenFader>();

            SerializedObject so = new SerializedObject(fader);
            so.FindProperty("_fadeImage").objectReferenceValue = fadeImage;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, ScreenFaderPrefabPath, out bool success);
            if (!success)
            {
                Debug.LogError($"Failed to save prefab at {ScreenFaderPrefabPath}");
                return;
            }
        }
        finally
        {
            Object.DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Prefab generated at {ScreenFaderPrefabPath}");
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate GameManager")]
    public static void GenerateGameManager()
    {
        PrefabGeneratorUtils.SavePrefab("GameManager", GameManagerPrefabPath, go => go.AddComponent<GameManager>());

        GameObject screenFaderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ScreenFaderPrefabPath);
        ScreenFader screenFader = screenFaderPrefab != null ? screenFaderPrefab.GetComponent<ScreenFader>() : null;
        if (screenFader == null)
            Debug.LogWarning($"ScreenFader prefab not found at {ScreenFaderPrefabPath}. Generate it first. _screenFader will be empty.");

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameManagerPrefabPath);
        GameManager gm = prefab.GetComponent<GameManager>();
        SerializedObject so = new SerializedObject(gm);

        PrefabGeneratorUtils.SetSceneReference(so, "_splashscreenScene", SplashscreenScenePath);
        PrefabGeneratorUtils.SetSceneReference(so, "_introScene", IntroScenePath);
        PrefabGeneratorUtils.SetSceneReference(so, "_mainMenuScene", MainMenuScenePath);
        PrefabGeneratorUtils.SetSceneReference(so, "_nameScene", NameScenePath);
        PrefabGeneratorUtils.SetSceneReference(so, "_gameplayScene", GameplayScenePath);

        ZoneData zone1 = AssetDatabase.LoadAssetAtPath<ZoneData>(Zone1DataPath);
        ZoneData zone2 = AssetDatabase.LoadAssetAtPath<ZoneData>(Zone2DataPath);

        if (zone1 != null)
            so.FindProperty("_startingZone").objectReferenceValue = zone1;
        else
            Debug.LogWarning($"Zone1 data not found at {Zone1DataPath}. Run 'Tools/DigimonWorld/Data/Generate ZoneData Assets' first.");

        SerializedProperty allZones = so.FindProperty("_allZones");
        int zoneCount = 0;
        if (zone1 != null) zoneCount++;
        if (zone2 != null) zoneCount++;
        allZones.arraySize = zoneCount;
        int idx = 0;
        if (zone1 != null) allZones.GetArrayElementAtIndex(idx++).objectReferenceValue = zone1;
        if (zone2 != null) allZones.GetArrayElementAtIndex(idx++).objectReferenceValue = zone2;

        if (screenFader != null)
            so.FindProperty("_screenFader").objectReferenceValue = screenFader;

        so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate SplashscreenController")]
    public static void GenerateSplashscreenController()
    {
        PrefabGeneratorUtils.SavePrefab("SplashscreenController", SplashscreenControllerPrefabPath, go =>
        {
            go.AddComponent<SplashscreenController>();
        });
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate IntroController")]
    public static void GenerateIntroController()
    {
        PrefabGeneratorUtils.SavePrefab("IntroController", IntroControllerPrefabPath, go =>
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

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate Player")]
    public static void GeneratePlayer()
    {
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerModelPath);
        if (modelAsset == null)
        {
            Debug.LogError($"Player model not found at {PlayerModelPath}.");
            return;
        }

        PrefabGeneratorUtils.EnsureFolder(PrefabGeneratorUtils.PrefabDir);

        GameObject root = new GameObject("Player");
        try
        {
            CharacterController cc = root.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 1f, 0f);
            cc.height = 2f;
            cc.radius = 0.5f;

            root.AddComponent<PlayerController>();

            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            model.name = "PlayerModel";
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = Vector3.zero;

            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath, out bool success);
            if (!success)
            {
                Debug.LogError($"Failed to save prefab at {PlayerPrefabPath}");
                return;
            }
        }
        finally
        {
            Object.DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Prefab generated at {PlayerPrefabPath}");
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate Agumon")]
    public static void GenerateAgumon()
    {
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AgumonModelPath);
        if (modelAsset == null)
        {
            Debug.LogError($"Agumon model not found at {AgumonModelPath}.");
            return;
        }

        PrefabGeneratorUtils.EnsureFolder(PrefabGeneratorUtils.PrefabDir);

        GameObject root = new GameObject("Agumon");
        try
        {
            CharacterController cc = root.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 0.5f, 0f);
            cc.height = 1f;
            cc.radius = 0.3f;

            root.AddComponent<DigimonFollow>();

            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            model.name = "AgumonModel";
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = Vector3.zero;

            Material agumonMat = PrefabGeneratorUtils.CreateOrLoadMaterial("Assets/_Project/Digimons/Agumon/Agumon.mat", new Color(1f, 0.8f, 0.2f));
            PrefabGeneratorUtils.ApplyMaterialToRenderers(model, agumonMat);

            PrefabUtility.SaveAsPrefabAsset(root, AgumonPrefabPath, out bool success);
            if (!success)
            {
                Debug.LogError($"Failed to save prefab at {AgumonPrefabPath}");
                return;
            }
        }
        finally
        {
            Object.DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Prefab generated at {AgumonPrefabPath}");
    }

    [MenuItem("Tools/DigimonWorld/Data/Generate TestDialogue")]
    public static void GenerateTestDialogue()
    {
        PrefabGeneratorUtils.EnsureFolder("Assets/_Project/Data");

        DialogueData dialogue = ScriptableObject.CreateInstance<DialogueData>();

        SerializedObject so = new SerializedObject(dialogue);
        SerializedProperty lines = so.FindProperty("_lines");
        lines.arraySize = 3;

        lines.GetArrayElementAtIndex(0).FindPropertyRelative("Speaker").stringValue = "Jijimon";
        lines.GetArrayElementAtIndex(0).FindPropertyRelative("Text").stringValue = "Ah, you must be the new Tamer! Welcome to File City.";

        lines.GetArrayElementAtIndex(1).FindPropertyRelative("Speaker").stringValue = "Jijimon";
        lines.GetArrayElementAtIndex(1).FindPropertyRelative("Text").stringValue = "Things have been rough since the Digimon started losing their memories...";

        lines.GetArrayElementAtIndex(2).FindPropertyRelative("Speaker").stringValue = "Jijimon";
        lines.GetArrayElementAtIndex(2).FindPropertyRelative("Text").stringValue = "Please, help us rebuild this city. Talk to the Digimon and bring them back!";

        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(dialogue, TestDialoguePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"TestDialogue asset generated at {TestDialoguePath}");
    }

    [MenuItem("Tools/DigimonWorld/Data/Generate ZoneData Assets")]
    public static void GenerateZoneData()
    {
        PrefabGeneratorUtils.EnsureFolder("Assets/_Project/Data/Zones");

        CreateZoneData(Zone1DataPath, Zone1ScenePath, new Vector3(0f, 10f, -10f));
        CreateZoneData(Zone2DataPath, Zone2ScenePath, new Vector3(0f, 12f, -15f));
    }

    private static void CreateZoneData(string assetPath, string scenePath, Vector3 cameraPosition)
    {
        ZoneData zone = ScriptableObject.CreateInstance<ZoneData>();

        SerializedObject so = new SerializedObject(zone);
        PrefabGeneratorUtils.SetSceneReference(so, "_scene", scenePath);
        so.FindProperty("_cameraPosition").vector3Value = cameraPosition;
        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(zone, assetPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"ZoneData asset generated at {assetPath}");
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate NPC")]
    public static void GenerateNPC()
    {
        PrefabGeneratorUtils.EnsureFolder(PrefabGeneratorUtils.PrefabDir);

        GameObject root = new GameObject("NPC");
        try
        {
            GameObject model = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            model.name = "Model";
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = Vector3.zero;

            Object.DestroyImmediate(model.GetComponent<CapsuleCollider>());
            CapsuleCollider col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 1f, 0f);
            col.height = 2f;
            col.radius = 0.5f;

            Material npcMat = PrefabGeneratorUtils.CreateOrLoadMaterial("Assets/_Project/Props/NPC.mat", new Color(0.2f, 0.4f, 0.9f));
            model.GetComponent<Renderer>().sharedMaterial = npcMat;

            NPCInteractable npc = root.AddComponent<NPCInteractable>();

            GameObject promptGo = new GameObject("PromptText");
            promptGo.transform.SetParent(root.transform, false);
            promptGo.transform.localPosition = new Vector3(0f, 2.2f, 0f);

            TextMeshPro tmp = promptGo.AddComponent<TextMeshPro>();
            tmp.text = "Talk";
            tmp.fontSize = 4;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.GetComponent<RectTransform>().sizeDelta = new Vector2(3f, 1f);

            SerializedObject so = new SerializedObject(npc);
            so.FindProperty("_promptText").objectReferenceValue = tmp;

            DialogueData testDialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(TestDialoguePath);
            if (testDialogue != null)
                so.FindProperty("_dialogue").objectReferenceValue = testDialogue;
            else
                Debug.LogWarning($"TestDialogue not found at {TestDialoguePath}. Run 'Tools/DigimonWorld/Data/Generate TestDialogue' first.");

            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, NPCPrefabPath, out bool success);
            if (!success)
            {
                Debug.LogError($"Failed to save prefab at {NPCPrefabPath}");
                return;
            }
        }
        finally
        {
            Object.DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Prefab generated at {NPCPrefabPath}");
    }
}
