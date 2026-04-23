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
    private const string PartnerDigimonPrefabPath = PrefabGeneratorUtils.PrefabDir + "/PartnerDigimon.prefab";
    private const string NPCPrefabPath = PrefabGeneratorUtils.PrefabDir + "/NPC.prefab";
    private const string InputManagerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/InputManager.prefab";
    private const string SceneLoaderPrefabPath = PrefabGeneratorUtils.PrefabDir + "/SceneLoader.prefab";
    private const string TimeSystemPrefabPath = PrefabGeneratorUtils.PrefabDir + "/TimeSystem.prefab";
    private const string CareSystemPrefabPath = PrefabGeneratorUtils.PrefabDir + "/CareSystem.prefab";
    private const string InventoryPrefabPath = PrefabGeneratorUtils.PrefabDir + "/Inventory.prefab";
    private const string TestDialoguePath = "Assets/_Project/Data/TestDialogue.asset";
    private const string DigimonDataDir = "Assets/_Project/Data/Digimons";
    private const string TechniqueDataDir = "Assets/_Project/Data/Techniques";
    private const string ItemDataDir = "Assets/_Project/Data/Items";
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

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate TimeSystem")]
    public static void GenerateTimeSystem()
    {
        PrefabGeneratorUtils.SavePrefab("TimeSystem", TimeSystemPrefabPath, go => go.AddComponent<TimeSystem>());
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate CareSystem")]
    public static void GenerateCareSystem()
    {
        PrefabGeneratorUtils.SavePrefab("CareSystem", CareSystemPrefabPath, go => go.AddComponent<CareSystem>());
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate Inventory")]
    public static void GenerateInventory()
    {
        PrefabGeneratorUtils.SavePrefab("Inventory", InventoryPrefabPath, go =>
        {
            Inventory inv = go.AddComponent<Inventory>();

            string[] itemNames = { "Meat", "GiantMeat", "Medicine", "HappyMushroom" };
            int[] itemCounts = { 5, 2, 3, 2 };

            SerializedObject so = new SerializedObject(inv);
            so.FindProperty("_startingBits").intValue = 500;

            SerializedProperty startingItems = so.FindProperty("_startingItems");
            startingItems.arraySize = itemNames.Length;
            for (int i = 0; i < itemNames.Length; i++)
            {
                string path = $"{ItemDataDir}/{itemNames[i]}.asset";
                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (item != null)
                {
                    SerializedProperty element = startingItems.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("Item").objectReferenceValue = item;
                    element.FindPropertyRelative("Count").intValue = itemCounts[i];
                }
                else
                {
                    Debug.LogWarning($"Item not found at {path}. Run 'Generate Sample Items' first.");
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        });
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

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate PartnerDigimon")]
    public static void GeneratePartnerDigimon()
    {
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AgumonModelPath);
        if (modelAsset == null)
        {
            Debug.LogError($"Agumon model not found at {AgumonModelPath}. Using this as default partner model.");
            return;
        }

        PrefabGeneratorUtils.EnsureFolder(PrefabGeneratorUtils.PrefabDir);

        GameObject root = new GameObject("PartnerDigimon");
        try
        {
            CharacterController cc = root.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 0.5f, 0f);
            cc.height = 1f;
            cc.radius = 0.3f;

            root.AddComponent<DigimonFollow>();
            DigimonInstance instance = root.AddComponent<DigimonInstance>();

            DigimonSpeciesData agumonSpecies = AssetDatabase.LoadAssetAtPath<DigimonSpeciesData>(DigimonDataDir + "/Agumon.asset");
            if (agumonSpecies != null)
            {
                SerializedObject instanceSo = new SerializedObject(instance);
                instanceSo.FindProperty("_species").objectReferenceValue = agumonSpecies;
                instanceSo.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning("Agumon species data not found. Run 'Generate Sample Species' first. _species will be empty.");
            }

            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            model.name = "Model";
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = Vector3.zero;

            Material agumonMat = PrefabGeneratorUtils.CreateOrLoadMaterial("Assets/_Project/Digimons/Agumon/Agumon.mat", new Color(1f, 0.8f, 0.2f));
            PrefabGeneratorUtils.ApplyMaterialToRenderers(model, agumonMat);

            PrefabUtility.SaveAsPrefabAsset(root, PartnerDigimonPrefabPath, out bool success);
            if (!success)
            {
                Debug.LogError($"Failed to save prefab at {PartnerDigimonPrefabPath}");
                return;
            }
        }
        finally
        {
            Object.DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Prefab generated at {PartnerDigimonPrefabPath}");
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

    [MenuItem("Tools/DigimonWorld/Data/Generate Sample Techniques")]
    public static void GenerateSampleTechniques()
    {
        PrefabGeneratorUtils.EnsureFolder(TechniqueDataDir);

        CreateTechnique("PepperBreath", TechniqueCategory.Fire, 12, 110, 3f);
        CreateTechnique("DynamiteCick", TechniqueCategory.Battle, 8, 90, 1.5f);
        CreateTechnique("SonicJab", TechniqueCategory.Battle, 5, 60, 1f);
        CreateTechnique("NovaBlast", TechniqueCategory.Fire, 28, 260, 5f);
        CreateTechnique("MetalClaw", TechniqueCategory.Battle, 15, 150, 1.5f);
        CreateTechnique("IceStatue", TechniqueCategory.Water, 18, 170, 4f);
        CreateTechnique("ElectroShocker", TechniqueCategory.Air, 22, 200, 4f);
        CreateTechnique("PoisonIvy", TechniqueCategory.Earth, 10, 80, 3f);
        CreateTechnique("MegaFlame", TechniqueCategory.Fire, 20, 190, 4.5f);
        CreateTechnique("SpinningNeedle", TechniqueCategory.Machine, 14, 130, 3f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Sample technique assets generated.");
    }

    private static void CreateTechnique(string techniqueName, TechniqueCategory category,
        int mpCost, int power, float range)
    {
        string path = $"{TechniqueDataDir}/{techniqueName}.asset";

        TechniqueData technique = ScriptableObject.CreateInstance<TechniqueData>();

        SerializedObject so = new SerializedObject(technique);
        so.FindProperty("_techniqueName").stringValue = techniqueName;
        so.FindProperty("_category").enumValueIndex = (int)category;
        so.FindProperty("_mpCost").intValue = mpCost;
        so.FindProperty("_power").intValue = power;
        so.FindProperty("_range").floatValue = range;
        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(technique, path);
        Debug.Log($"Technique asset generated at {path}");
    }

    [MenuItem("Tools/DigimonWorld/Data/Generate Sample Species")]
    public static void GenerateSampleSpecies()
    {
        PrefabGeneratorUtils.EnsureFolder(DigimonDataDir);

        CreateSpecies("Botamon", DigimonStage.Fresh, DigimonAttribute.Data,
            300, 100, 10, 10, 10, 10, 48, new string[0]);

        CreateSpecies("Koromon", DigimonStage.InTraining, DigimonAttribute.Data,
            500, 200, 30, 25, 25, 20, 72, new string[0]);

        CreateSpecies("Agumon", DigimonStage.Rookie, DigimonAttribute.Vaccine,
            1000, 500, 80, 60, 70, 50, 384,
            new[] { "PepperBreath", "DynamiteCick", "SonicJab" });

        CreateSpecies("Greymon", DigimonStage.Champion, DigimonAttribute.Vaccine,
            2500, 1200, 200, 150, 130, 120, 384,
            new[] { "PepperBreath", "NovaBlast", "MegaFlame", "MetalClaw" });

        CreateSpecies("MetalGreymon", DigimonStage.Ultimate, DigimonAttribute.Vaccine,
            4500, 2500, 400, 350, 280, 300, 480,
            new[] { "NovaBlast", "MegaFlame", "MetalClaw", "ElectroShocker" });

        CreateSpecies("Gabumon", DigimonStage.Rookie, DigimonAttribute.Data,
            900, 600, 70, 70, 60, 60, 384,
            new[] { "IceStatue", "SonicJab", "DynamiteCick" });

        CreateSpecies("Palmon", DigimonStage.Rookie, DigimonAttribute.Data,
            800, 700, 60, 50, 50, 80, 384,
            new[] { "PoisonIvy", "SpinningNeedle" });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Sample species assets generated.");
    }

    private static void CreateSpecies(string speciesName, DigimonStage stage,
        DigimonAttribute attribute, int hp, int mp, int offense, int defense,
        int speed, int brains, int lifespanHours, string[] techniqueNames)
    {
        string path = $"{DigimonDataDir}/{speciesName}.asset";

        DigimonSpeciesData species = ScriptableObject.CreateInstance<DigimonSpeciesData>();

        SerializedObject so = new SerializedObject(species);
        so.FindProperty("_speciesName").stringValue = speciesName;
        so.FindProperty("_stage").enumValueIndex = (int)stage;
        so.FindProperty("_attribute").enumValueIndex = (int)attribute;
        so.FindProperty("_baseHP").intValue = hp;
        so.FindProperty("_baseMP").intValue = mp;
        so.FindProperty("_baseOffense").intValue = offense;
        so.FindProperty("_baseDefense").intValue = defense;
        so.FindProperty("_baseSpeed").intValue = speed;
        so.FindProperty("_baseBrains").intValue = brains;
        so.FindProperty("_lifespanHours").intValue = lifespanHours;

        SerializedProperty techniques = so.FindProperty("_learnableTechniques");
        techniques.arraySize = techniqueNames.Length;
        for (int i = 0; i < techniqueNames.Length; i++)
        {
            string techPath = $"{TechniqueDataDir}/{techniqueNames[i]}.asset";
            TechniqueData techAsset = AssetDatabase.LoadAssetAtPath<TechniqueData>(techPath);
            if (techAsset != null)
                techniques.GetArrayElementAtIndex(i).objectReferenceValue = techAsset;
            else
                Debug.LogWarning($"Technique not found at {techPath}. Run 'Generate Sample Techniques' first.");
        }

        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(species, path);
        Debug.Log($"Species asset generated at {path}");
    }

    [MenuItem("Tools/DigimonWorld/Data/Generate Sample Items")]
    public static void GenerateSampleItems()
    {
        PrefabGeneratorUtils.EnsureFolder(ItemDataDir);

        CreateItem("Meat", ItemCategory.Food, 100, 50, 10, "Basic food. Reduces hunger.",
            hungerReduction: 15, weightGain: 3);
        CreateItem("GiantMeat", ItemCategory.Food, 500, 250, 10, "Large food. Greatly reduces hunger.",
            hungerReduction: 30, weightGain: 5);
        CreateItem("Sirloin", ItemCategory.Food, 1000, 500, 10, "Premium food. Best hunger reduction.",
            hungerReduction: 50, weightGain: 8);
        CreateItem("DigiMushroom", ItemCategory.Food, 200, 100, 10, "A mushroom that slightly reduces tiredness.",
            hungerReduction: 10, weightGain: 1, tirednessReduction: 10);
        CreateItem("Medicine", ItemCategory.Recovery, 500, 250, 10, "Restores a moderate amount of HP.",
            hpRestore: 500);
        CreateItem("SuperRecovery", ItemCategory.Recovery, 1500, 750, 10, "Fully restores HP and MP.",
            hpRestore: 9999, mpRestore: 9999);
        CreateItem("MPFloppy", ItemCategory.Recovery, 300, 150, 10, "Restores MP.",
            mpRestore: 500);
        CreateItem("HappyMushroom", ItemCategory.Status, 300, 150, 10, "Increases happiness.",
            happinessChange: 10);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Sample item assets generated.");
    }

    private static void CreateItem(string itemName, ItemCategory category, int buyPrice,
        int sellPrice, int maxStack, string description,
        int hungerReduction = 0, int weightGain = 0, int hpRestore = 0,
        int mpRestore = 0, int happinessChange = 0, int disciplineChange = 0,
        int tirednessReduction = 0)
    {
        string path = $"{ItemDataDir}/{itemName}.asset";

        ItemData item = ScriptableObject.CreateInstance<ItemData>();

        SerializedObject so = new SerializedObject(item);
        so.FindProperty("_itemName").stringValue = itemName;
        so.FindProperty("_category").enumValueIndex = (int)category;
        so.FindProperty("_description").stringValue = description;
        so.FindProperty("_buyPrice").intValue = buyPrice;
        so.FindProperty("_sellPrice").intValue = sellPrice;
        so.FindProperty("_maxStack").intValue = maxStack;
        so.FindProperty("_hungerReduction").intValue = hungerReduction;
        so.FindProperty("_weightGain").intValue = weightGain;
        so.FindProperty("_hpRestore").intValue = hpRestore;
        so.FindProperty("_mpRestore").intValue = mpRestore;
        so.FindProperty("_happinessChange").intValue = happinessChange;
        so.FindProperty("_disciplineChange").intValue = disciplineChange;
        so.FindProperty("_tirednessReduction").intValue = tirednessReduction;
        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(item, path);
        Debug.Log($"Item asset generated at {path}");
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
