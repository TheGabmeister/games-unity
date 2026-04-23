using UnityEditor;
using UnityEngine;

public class GeneratorWindow : EditorWindow
{
    private Vector2 _scrollPos;

    [MenuItem("Tools/DigimonWorld/Generator Window")]
    public static void ShowWindow()
    {
        GetWindow<GeneratorWindow>("DW1 Generators");
    }

    private void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        Section("Data Assets", () =>
        {
            Button("BootstrapConfig", GeneratePrefabs.GenerateBootstrapConfig);
            Button("TestDialogue", GeneratePrefabs.GenerateTestDialogue);
            Button("ZoneData Assets", GeneratePrefabs.GenerateZoneData);
            Button("Sample Techniques", GeneratePrefabs.GenerateSampleTechniques);
            Button("Sample Species", GeneratePrefabs.GenerateSampleSpecies);
            Button("Sample Items", GeneratePrefabs.GenerateSampleItems);
            Button("Sample Training", GeneratePrefabs.GenerateSampleTraining);
            Button("Sample Encounters", GeneratePrefabs.GenerateSampleEncounters);
        });

        Section("Prefabs — Services", () =>
        {
            Button("Bootstrapper", GeneratePrefabs.GenerateBootstrapper);
            Button("AudioSystem", GeneratePrefabs.GenerateAudioSystem);
            Button("SceneLoader", GeneratePrefabs.GenerateSceneLoader);
            Button("ScreenFader", GeneratePrefabs.GenerateScreenFader);
            Button("GameManager", GeneratePrefabs.GenerateGameManager);
            Button("InputManager", GeneratePrefabs.GenerateInputManager);
            Button("TimeSystem", GeneratePrefabs.GenerateTimeSystem);
            Button("CareSystem", GeneratePrefabs.GenerateCareSystem);
            Button("Inventory", GeneratePrefabs.GenerateInventory);
            Button("BattleSystem", GeneratePrefabs.GenerateBattleSystem);
        });

        Section("Prefabs — UI", () =>
        {
            Button("DialogueManager", GenerateDialoguePrefab.Generate);
            Button("HUD", GenerateHUDPrefab.Generate);
            Button("InventoryScreen", GenerateInventoryScreenPrefab.Generate);
            Button("PauseScreen", GeneratePauseScreenPrefab.Generate);
            Button("StatusScreen", GenerateStatusScreenPrefab.Generate);
            Button("BattleUI", GenerateBattleUIPrefab.Generate);
        });

        Section("Prefabs — Controllers", () =>
        {
            Button("SplashscreenController", GeneratePrefabs.GenerateSplashscreenController);
            Button("IntroController", GeneratePrefabs.GenerateIntroController);
            Button("MainMenuController", GenerateMainMenuPrefab.Generate);
            Button("NameController", GenerateNamePrefab.Generate);
        });

        Section("Prefabs — Characters", () =>
        {
            Button("Player", GeneratePrefabs.GeneratePlayer);
            Button("PartnerDigimon", GeneratePrefabs.GeneratePartnerDigimon);
            Button("NPC", GeneratePrefabs.GenerateNPC);
            Button("WildDigimon", GeneratePrefabs.GenerateWildDigimon);
        });

        Section("Prefabs — Interactables", () =>
        {
            Button("TrainingFacility", GeneratePrefabs.GenerateTrainingFacility);
        });

        Section("Scenes", () =>
        {
            Button("Bootstrap", GenerateScenes.GenerateBootstrap);
            Button("Splashscreen", GenerateScenes.GenerateSplashscreen);
            Button("Intro", GenerateScenes.GenerateIntro);
            Button("MainMenu", GenerateScenes.GenerateMainMenu);
            Button("Name", GenerateScenes.GenerateName);
            Button("Gameplay", GenerateScenes.GenerateGameplay);
            Button("Zone1", GenerateScenes.GenerateZone1);
            Button("Zone2", GenerateScenes.GenerateZone2);
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Generate All Scenes", GUILayout.Height(28)))
                GenerateScenes.GenerateAll();
        });

        Section("Generate Everything", () =>
        {
            EditorGUILayout.HelpBox("Runs all data, prefab, and scene generators in the correct order.", MessageType.Info);
            if (GUILayout.Button("Generate All", GUILayout.Height(30)))
                GenerateAll();
        });

        EditorGUILayout.EndScrollView();
    }

    private static void Section(string title, System.Action content)
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        content();
        EditorGUI.indentLevel--;
    }

    private static void Button(string label, System.Action action)
    {
        if (GUILayout.Button(label))
            action();
    }

    private static void GenerateAll()
    {
        GeneratePrefabs.GenerateBootstrapConfig();
        GeneratePrefabs.GenerateTestDialogue();
        GeneratePrefabs.GenerateZoneData();
        GeneratePrefabs.GenerateSampleTechniques();
        GeneratePrefabs.GenerateSampleSpecies();
        GeneratePrefabs.GenerateSampleItems();
        GeneratePrefabs.GenerateSampleTraining();
        GeneratePrefabs.GenerateSampleEncounters();

        GeneratePrefabs.GenerateBootstrapper();
        GeneratePrefabs.GenerateAudioSystem();
        GeneratePrefabs.GenerateSceneLoader();
        GeneratePrefabs.GenerateScreenFader();
        GeneratePrefabs.GenerateGameManager();
        GeneratePrefabs.GenerateInputManager();
        GeneratePrefabs.GenerateTimeSystem();
        GeneratePrefabs.GenerateCareSystem();
        GeneratePrefabs.GenerateInventory();
        GeneratePrefabs.GenerateBattleSystem();

        GenerateDialoguePrefab.Generate();
        GenerateHUDPrefab.Generate();
        GenerateInventoryScreenPrefab.Generate();
        GeneratePauseScreenPrefab.Generate();
        GenerateStatusScreenPrefab.Generate();
        GenerateBattleUIPrefab.Generate();

        GeneratePrefabs.GenerateSplashscreenController();
        GeneratePrefabs.GenerateIntroController();
        GenerateMainMenuPrefab.Generate();
        GenerateNamePrefab.Generate();

        GeneratePrefabs.GeneratePlayer();
        GeneratePrefabs.GeneratePartnerDigimon();
        GeneratePrefabs.GenerateNPC();
        GeneratePrefabs.GenerateWildDigimon();

        GeneratePrefabs.GenerateTrainingFacility();

        GenerateScenes.GenerateAll();
    }
}
