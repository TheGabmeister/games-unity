using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class GenerateScenes
{
    private const string SceneDir = "Assets/_Project/Scenes";
    private const string BootstrapScenePath = SceneDir + "/_Bootstrap.unity";
    private const string SplashscreenScenePath = SceneDir + "/_Splashscreen.unity";
    private const string IntroScenePath = SceneDir + "/_Intro.unity";
    private const string MainMenuScenePath = SceneDir + "/_MainMenu.unity";
    private const string NameScenePath = SceneDir + "/_Name.unity";
    private const string GameplayScenePath = SceneDir + "/_Gameplay.unity";
    private const string ZoneDir = SceneDir + "/Zones";
    private const string Zone1ScenePath = ZoneDir + "/Zone1.unity";
    private const string Zone2ScenePath = ZoneDir + "/Zone2.unity";
    private const string Zone1DataPath = "Assets/_Project/Data/Zones/Zone1.asset";
    private const string Zone2DataPath = "Assets/_Project/Data/Zones/Zone2.asset";

    private const string AudioSystemPrefabPath = PrefabGeneratorUtils.PrefabDir + "/AudioSystem.prefab";
    private const string GameManagerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/GameManager.prefab";
    private const string ScreenFaderPrefabPath = PrefabGeneratorUtils.PrefabDir + "/ScreenFader.prefab";
    private const string SceneLoaderPrefabPath = PrefabGeneratorUtils.PrefabDir + "/SceneLoader.prefab";
    private const string InputManagerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/InputManager.prefab";
    private const string DialogueManagerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/DialogueManager.prefab";
    private const string TimeSystemPrefabPath = PrefabGeneratorUtils.PrefabDir + "/TimeSystem.prefab";
    private const string HUDPrefabPath = PrefabGeneratorUtils.PrefabDir + "/HUD.prefab";
    private const string CareSystemPrefabPath = PrefabGeneratorUtils.PrefabDir + "/CareSystem.prefab";
    private const string InventoryPrefabPath = PrefabGeneratorUtils.PrefabDir + "/Inventory.prefab";
    private const string NPCPrefabPath = PrefabGeneratorUtils.PrefabDir + "/NPC.prefab";
    private const string SplashscreenControllerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/SplashscreenController.prefab";
    private const string IntroControllerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/IntroController.prefab";
    private const string MainMenuControllerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/MainMenuController.prefab";
    private const string NameControllerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/NameController.prefab";
    private const string PlayerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/Player.prefab";
    private const string PartnerDigimonPrefabPath = PrefabGeneratorUtils.PrefabDir + "/PartnerDigimon.prefab";

    [MenuItem("Tools/DigimonWorld/Scenes/Generate Bootstrap Scene")]
    public static void GenerateBootstrap()
    {
        GameObject audioSystemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AudioSystemPrefabPath);
        if (audioSystemPrefab == null)
        {
            Debug.LogError($"AudioSystem prefab not found at {AudioSystemPrefabPath}. Run 'Tools/DigimonWorld/Prefabs/Generate AudioSystem' first.");
            return;
        }

        GameObject gameManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameManagerPrefabPath);
        if (gameManagerPrefab == null)
        {
            Debug.LogError($"GameManager prefab not found at {GameManagerPrefabPath}. Run 'Tools/DigimonWorld/Prefabs/Generate GameManager' first.");
            return;
        }

        GameObject screenFaderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ScreenFaderPrefabPath);
        if (screenFaderPrefab == null)
        {
            Debug.LogError($"ScreenFader prefab not found at {ScreenFaderPrefabPath}. Run 'Tools/DigimonWorld/Prefabs/Generate ScreenFader' first.");
            return;
        }

        GameObject sceneLoaderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SceneLoaderPrefabPath);
        if (sceneLoaderPrefab == null)
        {
            Debug.LogError($"SceneLoader prefab not found at {SceneLoaderPrefabPath}. Run 'Tools/DigimonWorld/Prefabs/Generate SceneLoader' first.");
            return;
        }

        PrefabGeneratorUtils.EnsureFolder(SceneDir);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        PrefabUtility.InstantiatePrefab(audioSystemPrefab, scene);
        PrefabUtility.InstantiatePrefab(gameManagerPrefab, scene);
        PrefabUtility.InstantiatePrefab(screenFaderPrefab, scene);
        PrefabUtility.InstantiatePrefab(sceneLoaderPrefab, scene);

        if (!SaveScene(scene, BootstrapScenePath)) return;

        AddSceneToBuildSettingsAtIndex(BootstrapScenePath, 0);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Bootstrap scene generated at {BootstrapScenePath}");
    }

    [MenuItem("Tools/DigimonWorld/Scenes/Generate Splashscreen Scene")]
    public static void GenerateSplashscreen()
    {
        GenerateSceneWithPrefab(SplashscreenScenePath, SplashscreenControllerPrefabPath, "_Splashscreen");
    }

    [MenuItem("Tools/DigimonWorld/Scenes/Generate Intro Scene")]
    public static void GenerateIntro()
    {
        GenerateSceneWithPrefab(IntroScenePath, IntroControllerPrefabPath, "_Intro");
    }

    [MenuItem("Tools/DigimonWorld/Scenes/Generate MainMenu Scene")]
    public static void GenerateMainMenu()
    {
        GenerateSceneWithPrefab(MainMenuScenePath, MainMenuControllerPrefabPath, "_MainMenu");
    }

    [MenuItem("Tools/DigimonWorld/Scenes/Generate Name Scene")]
    public static void GenerateName()
    {
        GenerateSceneWithPrefab(NameScenePath, NameControllerPrefabPath, "_Name");
    }

    [MenuItem("Tools/DigimonWorld/Scenes/Generate Gameplay Scene")]
    public static void GenerateGameplay()
    {
        GameObject inputManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(InputManagerPrefabPath);
        if (inputManagerPrefab == null)
        {
            Debug.LogError($"InputManager prefab not found at {InputManagerPrefabPath}. Run 'Tools/DigimonWorld/Prefabs/Generate InputManager' first.");
            return;
        }

        GameObject dialogueManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DialogueManagerPrefabPath);
        if (dialogueManagerPrefab == null)
        {
            Debug.LogError($"DialogueManager prefab not found at {DialogueManagerPrefabPath}. Run 'Tools/DigimonWorld/Prefabs/Generate DialogueManager' first.");
            return;
        }

        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (playerPrefab == null)
        {
            Debug.LogError($"Player prefab not found at {PlayerPrefabPath}. Run 'Tools/DigimonWorld/Prefabs/Generate Player' first.");
            return;
        }

        GameObject partnerDigimonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PartnerDigimonPrefabPath);
        if (partnerDigimonPrefab == null)
        {
            Debug.LogError($"PartnerDigimon prefab not found at {PartnerDigimonPrefabPath}. Run 'Tools/DigimonWorld/Prefabs/Generate PartnerDigimon' first.");
            return;
        }

        GameObject timeSystemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TimeSystemPrefabPath);
        if (timeSystemPrefab == null)
        {
            Debug.LogError($"TimeSystem prefab not found at {TimeSystemPrefabPath}. Run 'Tools/DigimonWorld/Prefabs/Generate TimeSystem' first.");
            return;
        }

        GameObject hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HUDPrefabPath);
        if (hudPrefab == null)
        {
            Debug.LogError($"HUD prefab not found at {HUDPrefabPath}. Run 'Tools/DigimonWorld/Prefabs/Generate HUD' first.");
            return;
        }

        GameObject careSystemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CareSystemPrefabPath);
        if (careSystemPrefab == null)
        {
            Debug.LogError($"CareSystem prefab not found at {CareSystemPrefabPath}. Run 'Tools/DigimonWorld/Prefabs/Generate CareSystem' first.");
            return;
        }

        GameObject inventoryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(InventoryPrefabPath);
        if (inventoryPrefab == null)
        {
            Debug.LogError($"Inventory prefab not found at {InventoryPrefabPath}. Run 'Tools/DigimonWorld/Prefabs/Generate Inventory' first.");
            return;
        }

        PrefabGeneratorUtils.EnsureFolder(SceneDir);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        PrefabUtility.InstantiatePrefab(inputManagerPrefab, scene);
        PrefabUtility.InstantiatePrefab(dialogueManagerPrefab, scene);
        PrefabUtility.InstantiatePrefab(timeSystemPrefab, scene);
        PrefabUtility.InstantiatePrefab(hudPrefab, scene);
        PrefabUtility.InstantiatePrefab(careSystemPrefab, scene);
        PrefabUtility.InstantiatePrefab(inventoryPrefab, scene);

        GameObject camGo = CreateCamera(scene);
        camGo.transform.position = new Vector3(0f, 10f, -10f);
        GameplayCamera gameCam = camGo.AddComponent<GameplayCamera>();

        GameObject playerGo = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
        playerGo.transform.position = Vector3.zero;
        PlayerController playerCtrl = playerGo.GetComponent<PlayerController>();

        SerializedObject camSo = new SerializedObject(gameCam);
        camSo.FindProperty("_target").objectReferenceValue = playerGo.transform;
        camSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject playerSo = new SerializedObject(playerCtrl);
        playerSo.FindProperty("_cameraTransform").objectReferenceValue = camGo.transform;
        playerSo.ApplyModifiedPropertiesWithoutUndo();

        // Partner Digimon
        GameObject partnerGo = (GameObject)PrefabUtility.InstantiatePrefab(partnerDigimonPrefab, scene);
        partnerGo.transform.position = new Vector3(-1.5f, 0f, -1f);
        DigimonFollow digimonFollow = partnerGo.GetComponent<DigimonFollow>();

        SerializedObject digimonSo = new SerializedObject(digimonFollow);
        digimonSo.FindProperty("_target").objectReferenceValue = playerGo.transform;
        digimonSo.ApplyModifiedPropertiesWithoutUndo();

        if (!SaveScene(scene, GameplayScenePath)) return;

        AppendSceneToBuildSettings(GameplayScenePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"_Gameplay scene generated at {GameplayScenePath}");
    }

    [MenuItem("Tools/DigimonWorld/Scenes/Generate Zone1 Scene")]
    public static void GenerateZone1()
    {
        GameObject npcPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NPCPrefabPath);
        if (npcPrefab == null)
        {
            Debug.LogError($"NPC prefab not found at {NPCPrefabPath}. Run 'Tools/DigimonWorld/Prefabs/Generate NPC' first.");
            return;
        }

        PrefabGeneratorUtils.EnsureFolder(ZoneDir);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Red ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(3f, 1f, 3f);
        ground.GetComponent<Renderer>().sharedMaterial = PrefabGeneratorUtils.CreateOrLoadMaterial("Assets/_Project/Props/Zone1Ground.mat", new Color(0.7f, 0.2f, 0.2f));
        SceneManager.MoveGameObjectToScene(ground, scene);

        // Tall cylinders
        for (int i = 0; i < 5; i++)
        {
            GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.name = $"Pillar_{i}";
            cyl.transform.position = new Vector3(-4f + i * 2f, 1.5f, 3f);
            cyl.transform.localScale = new Vector3(0.5f, 3f, 0.5f);
            cyl.GetComponent<Renderer>().sharedMaterial = PrefabGeneratorUtils.CreateOrLoadMaterial("Assets/_Project/Props/Zone1Pillar.mat", new Color(0.9f, 0.5f, 0.1f));
            SceneManager.MoveGameObjectToScene(cyl, scene);
        }

        // Large sphere landmark
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Landmark";
        sphere.transform.position = new Vector3(0f, 2f, 8f);
        sphere.transform.localScale = new Vector3(4f, 4f, 4f);
        sphere.GetComponent<Renderer>().sharedMaterial = PrefabGeneratorUtils.CreateOrLoadMaterial("Assets/_Project/Props/Zone1Landmark.mat", new Color(0.9f, 0.1f, 0.1f));
        SceneManager.MoveGameObjectToScene(sphere, scene);

        // Test Interactable
        GameObject interactable = GameObject.CreatePrimitive(PrimitiveType.Cube);
        interactable.name = "TestInteractable";
        interactable.transform.position = new Vector3(0f, 0.5f, 3f);
        SceneManager.MoveGameObjectToScene(interactable, scene);

        TestInteractable testInteractable = interactable.AddComponent<TestInteractable>();

        GameObject promptGo = new GameObject("PromptText");
        promptGo.transform.SetParent(interactable.transform, false);
        promptGo.transform.localPosition = new Vector3(0f, 1.2f, 0f);

        TextMeshPro tmp = promptGo.AddComponent<TextMeshPro>();
        tmp.text = "Press E";
        tmp.fontSize = 4;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.GetComponent<RectTransform>().sizeDelta = new Vector2(3f, 1f);

        SerializedObject testSo = new SerializedObject(testInteractable);
        testSo.FindProperty("_promptText").objectReferenceValue = tmp;
        testSo.ApplyModifiedPropertiesWithoutUndo();

        // NPC
        GameObject npcGo = (GameObject)PrefabUtility.InstantiatePrefab(npcPrefab, scene);
        npcGo.transform.position = new Vector3(5f, 0f, 3f);

        // Zone trigger to Zone2
        CreateZoneTrigger(scene, "To Zone2", new Vector3(-10f, 1f, 0f), Zone2DataPath,
            new Color(0.2f, 0.4f, 0.9f, 0.5f));

        if (!SaveScene(scene, Zone1ScenePath)) return;
        AppendSceneToBuildSettings(Zone1ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Zone1 scene generated at {Zone1ScenePath}");
    }

    [MenuItem("Tools/DigimonWorld/Scenes/Generate Zone2 Scene")]
    public static void GenerateZone2()
    {
        PrefabGeneratorUtils.EnsureFolder(ZoneDir);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Blue ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(3f, 1f, 3f);
        ground.GetComponent<Renderer>().sharedMaterial = PrefabGeneratorUtils.CreateOrLoadMaterial("Assets/_Project/Props/Zone2Ground.mat", new Color(0.2f, 0.2f, 0.7f));
        SceneManager.MoveGameObjectToScene(ground, scene);

        // Scattered cubes
        for (int i = 0; i < 6; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"Block_{i}";
            float x = (i % 3 - 1) * 4f;
            float z = (i / 3) * 5f + 2f;
            cube.transform.position = new Vector3(x, 0.5f, z);
            cube.transform.localScale = new Vector3(1.5f, 1f + i * 0.3f, 1.5f);
            cube.GetComponent<Renderer>().sharedMaterial = PrefabGeneratorUtils.CreateOrLoadMaterial("Assets/_Project/Props/Zone2Block.mat", new Color(0.1f, 0.7f, 0.9f));
            SceneManager.MoveGameObjectToScene(cube, scene);
        }

        // Capsule tower landmark
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = "Landmark";
        capsule.transform.position = new Vector3(0f, 3f, 10f);
        capsule.transform.localScale = new Vector3(2f, 6f, 2f);
        capsule.GetComponent<Renderer>().sharedMaterial = PrefabGeneratorUtils.CreateOrLoadMaterial("Assets/_Project/Props/Zone2Landmark.mat", new Color(0.1f, 0.1f, 0.9f));
        SceneManager.MoveGameObjectToScene(capsule, scene);

        // Zone trigger to Zone1
        CreateZoneTrigger(scene, "To Zone1", new Vector3(-10f, 1f, 0f), Zone1DataPath,
            new Color(0.9f, 0.2f, 0.2f, 0.5f));

        if (!SaveScene(scene, Zone2ScenePath)) return;
        AppendSceneToBuildSettings(Zone2ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Zone2 scene generated at {Zone2ScenePath}");
    }

    [MenuItem("Tools/DigimonWorld/Scenes/Generate All Scenes")]
    public static void GenerateAll()
    {
        GenerateBootstrap();
        GenerateSplashscreen();
        GenerateIntro();
        GenerateMainMenu();
        GenerateName();
        GenerateGameplay();
        GenerateZone1();
        GenerateZone2();
    }

    private static void GenerateSceneWithPrefab(string scenePath, string prefabPath, string displayName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found at {prefabPath}. Generate prefabs first.");
            return;
        }

        PrefabGeneratorUtils.EnsureFolder(SceneDir);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera(scene);
        PrefabUtility.InstantiatePrefab(prefab, scene);

        if (!SaveScene(scene, scenePath)) return;

        AppendSceneToBuildSettings(scenePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"{displayName} scene generated at {scenePath}");
    }

    private static void GenerateEmptyScene(string scenePath, string displayName)
    {
        PrefabGeneratorUtils.EnsureFolder(SceneDir);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera(scene);

        if (!SaveScene(scene, scenePath)) return;

        AppendSceneToBuildSettings(scenePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"{displayName} scene generated at {scenePath}");
    }

    private static GameObject CreateCamera(Scene scene)
    {
        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        camGo.AddComponent<UniversalAdditionalCameraData>();
        SceneManager.MoveGameObjectToScene(camGo, scene);
        return camGo;
    }

    private static bool SaveScene(Scene scene, string scenePath)
    {
        bool saved = EditorSceneManager.SaveScene(scene, scenePath);
        if (!saved)
        {
            Debug.LogError($"Failed to save scene at {scenePath}");
        }
        return saved;
    }

    private static void AddSceneToBuildSettingsAtIndex(string scenePath, int index)
    {
        var scenes = EditorBuildSettings.scenes;
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].path == scenePath)
            {
                if (!scenes[i].enabled)
                {
                    scenes[i].enabled = true;
                    EditorBuildSettings.scenes = scenes;
                }
                return;
            }
        }

        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        if (index > 0)
            System.Array.Copy(scenes, 0, newScenes, 0, index);
        newScenes[index] = new EditorBuildSettingsScene(scenePath, true);
        if (index < scenes.Length)
            System.Array.Copy(scenes, index, newScenes, index + 1, scenes.Length - index);
        EditorBuildSettings.scenes = newScenes;
    }

    private static void AppendSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var entry in scenes)
        {
            if (entry.path == scenePath)
            {
                if (!entry.enabled)
                {
                    entry.enabled = true;
                    EditorBuildSettings.scenes = scenes;
                }
                return;
            }
        }

        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        System.Array.Copy(scenes, 0, newScenes, 0, scenes.Length);
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
    }

    private static void CreateZoneTrigger(Scene scene, string name, Vector3 position,
        string zoneDataPath, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = position;
        go.transform.localScale = new Vector3(2f, 3f, 2f);
        SceneManager.MoveGameObjectToScene(go, scene);

        BoxCollider col = go.GetComponent<BoxCollider>();
        col.isTrigger = true;

        Material mat = PrefabGeneratorUtils.CreateOrLoadMaterial(
            $"Assets/_Project/Props/{name.Replace(" ", "")}.mat", color);
        go.GetComponent<Renderer>().sharedMaterial = mat;

        ZoneTrigger trigger = go.AddComponent<ZoneTrigger>();

        ZoneData zoneData = AssetDatabase.LoadAssetAtPath<ZoneData>(zoneDataPath);
        if (zoneData != null)
        {
            SerializedObject so = new SerializedObject(trigger);
            so.FindProperty("_destinationZone").objectReferenceValue = zoneData;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            Debug.LogWarning($"ZoneData not found at {zoneDataPath}. Run 'Tools/DigimonWorld/Data/Generate ZoneData Assets' first.");
        }
    }
}
