using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.IO;
using UnityEngine.InputSystem;

public class SceneSetup
{
    [MenuItem("FF1/Setup Phase 1 Scenes")]
    static void SetupPhase1Scenes()
    {
        // Ensure directories exist
        EnsureDirectory("Assets/_Project/Scenes");
        EnsureDirectory("Assets/_Project/Data");

        // Create TilePalette asset if needed
        string palettePath = "Assets/_Project/Data/DefaultTilePalette.asset";
        TilePalette palette = AssetDatabase.LoadAssetAtPath<TilePalette>(palettePath);
        if (palette == null)
        {
            palette = ScriptableObject.CreateInstance<TilePalette>();
            AssetDatabase.CreateAsset(palette, palettePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[SceneSetup] Created DefaultTilePalette asset");
        }

        // Create scenes
        CreateBootScene();
        CreateTitleScene();
        CreateExplorationScene(palette);

        // Add scenes to build settings
        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene("Assets/_Project/Scenes/Boot.unity", true),
            new EditorBuildSettingsScene("Assets/_Project/Scenes/Title.unity", true),
            new EditorBuildSettingsScene("Assets/_Project/Scenes/Exploration.unity", true)
        };
        EditorBuildSettings.scenes = scenes.ToArray();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[SceneSetup] Phase 1 scenes created and added to Build Settings.");
        EditorUtility.DisplayDialog("Scene Setup Complete",
            "Boot, Title, and Exploration scenes have been created.\n\n" +
            "Build Settings have been updated.\n\n" +
            "Open Boot scene to start the game.",
            "OK");
    }

    static void CreateBootScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // --- GameManager hierarchy ---
        var gmGO = new GameObject("GameManager");
        gmGO.AddComponent<GameManager>();

        var stateGO = new GameObject("StateManager");
        stateGO.transform.SetParent(gmGO.transform);
        stateGO.AddComponent<GameStateManager>();

        var audioGO = new GameObject("Audio");
        audioGO.transform.SetParent(gmGO.transform);
        audioGO.AddComponent<AudioManager>();

        var sceneLoaderGO = new GameObject("SceneLoader");
        sceneLoaderGO.transform.SetParent(gmGO.transform);
        sceneLoaderGO.AddComponent<SceneLoader>();

        var inputGO = new GameObject("Input");
        inputGO.transform.SetParent(gmGO.transform);
        var inputMgr = inputGO.AddComponent<InputManager>();

        var saveGO = new GameObject("SaveManager");
        saveGO.transform.SetParent(gmGO.transform);
        saveGO.AddComponent<SaveManager>();

        var dataGO = new GameObject("DataRepository");
        dataGO.transform.SetParent(gmGO.transform);
        dataGO.AddComponent<DataRepository>();

        // --- FadeCanvas ---
        var fadeGO = new GameObject("FadeCanvas");
        fadeGO.AddComponent<FadeOverlay>();

        // --- Wire InputActionAsset to InputManager ---
        var inputActionsAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(
            "Assets/_Project/Input/InputSystem_Actions.inputactions");
        if (inputActionsAsset != null)
        {
            var inputSO = new SerializedObject(inputMgr);
            inputSO.FindProperty("inputActions").objectReferenceValue = inputActionsAsset;
            inputSO.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("[SceneSetup] Wired InputActionAsset to InputManager");
        }
        else
        {
            Debug.LogWarning("[SceneSetup] Could not find InputSystem_Actions.inputactions");
        }

        // --- DebugCanvas (child of GameManager for DontDestroyOnLoad persistence) ---
        var debugCanvasGO = new GameObject("DebugCanvas");
        debugCanvasGO.transform.SetParent(gmGO.transform);
        var debugCanvas = debugCanvasGO.AddComponent<Canvas>();
        debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        debugCanvas.sortingOrder = 998;
        var debugScaler = debugCanvasGO.AddComponent<CanvasScaler>();
        debugScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        debugScaler.referenceResolution = new Vector2(1920, 1080);
        debugCanvasGO.AddComponent<GraphicRaycaster>();

        var overlayGO = new GameObject("DebugOverlay");
        overlayGO.transform.SetParent(debugCanvasGO.transform, false);
        overlayGO.AddComponent<RectTransform>();
        overlayGO.AddComponent<DebugOverlay>();

        var consoleGO = new GameObject("DebugConsole");
        consoleGO.transform.SetParent(debugCanvasGO.transform, false);
        consoleGO.AddComponent<RectTransform>();
        consoleGO.AddComponent<DebugConsole>();

        // --- EventSystem (child of GameManager for DontDestroyOnLoad persistence) ---
        var eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.transform.SetParent(gmGO.transform);
        eventSystemGO.AddComponent<EventSystem>();
        var uiModule = eventSystemGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // Wire the InputActionAsset to the UI module
        if (inputActionsAsset != null)
        {
            var uiModuleSO = new SerializedObject(uiModule);
            var actionsAssetProp = uiModuleSO.FindProperty("m_ActionsAsset");
            if (actionsAssetProp != null)
            {
                actionsAssetProp.objectReferenceValue = inputActionsAsset;
                uiModuleSO.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/_Project/Scenes/Boot.unity");
        Debug.Log("[SceneSetup] Boot scene created");
    }

    static void CreateTitleScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // --- Main Camera ---
        var cameraGO = new GameObject("Main Camera");
        cameraGO.tag = "MainCamera";
        var cam = cameraGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.SolidColor;

        // --- TitleCanvas ---
        var titleCanvasGO = new GameObject("TitleCanvas");
        var canvas = titleCanvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var scaler = titleCanvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        titleCanvasGO.AddComponent<GraphicRaycaster>();
        titleCanvasGO.AddComponent<TitleScreenUI>();

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/_Project/Scenes/Title.unity");
        Debug.Log("[SceneSetup] Title scene created");
    }

    static void CreateExplorationScene(TilePalette palette)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // --- Grid ---
        var gridGO = new GameObject("Grid");
        var grid = gridGO.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 1);

        var tilemapGO = new GameObject("Tilemap");
        tilemapGO.transform.SetParent(gridGO.transform);
        var tilemap = tilemapGO.AddComponent<Tilemap>();
        var tilemapRenderer = tilemapGO.AddComponent<TilemapRenderer>();
        tilemapRenderer.sortingOrder = 0;

        // --- MapData ---
        var mapDataGO = new GameObject("MapData");
        var gridData = mapDataGO.AddComponent<GridData>();
        var tileFactory = mapDataGO.AddComponent<ProceduralTileFactory>();
        var mapBuilder = mapDataGO.AddComponent<MapBuilder>();

        // Wire MapBuilder serialized fields via SerializedObject
        var mapBuilderSO = new SerializedObject(mapBuilder);
        mapBuilderSO.FindProperty("tilemap").objectReferenceValue = tilemap;
        mapBuilderSO.FindProperty("tileFactory").objectReferenceValue = tileFactory;
        mapBuilderSO.FindProperty("palette").objectReferenceValue = palette;
        mapBuilderSO.ApplyModifiedPropertiesWithoutUndo();

        // --- Player ---
        var playerGO = new GameObject("Player");
        var playerController = playerGO.AddComponent<PlayerController>();

        // Wire PlayerController.gridData
        var playerSO = new SerializedObject(playerController);
        playerSO.FindProperty("gridData").objectReferenceValue = gridData;
        playerSO.ApplyModifiedPropertiesWithoutUndo();

        // --- Main Camera ---
        var cameraGO = new GameObject("Main Camera");
        cameraGO.tag = "MainCamera";
        var cam = cameraGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cameraGO.transform.position = new Vector3(0, 0, -10);

        var camController = cameraGO.AddComponent<CameraController>();

        // Wire CameraController serialized fields
        var camSO = new SerializedObject(camController);
        camSO.FindProperty("target").objectReferenceValue = playerGO.transform;
        camSO.FindProperty("gridData").objectReferenceValue = gridData;
        camSO.ApplyModifiedPropertiesWithoutUndo();

        // --- ExplorationInitializer ---
        var initGO = new GameObject("ExplorationInitializer");
        var initializer = initGO.AddComponent<ExplorationInitializer>();

        var initSO = new SerializedObject(initializer);
        initSO.FindProperty("mapBuilder").objectReferenceValue = mapBuilder;
        initSO.FindProperty("player").objectReferenceValue = playerController;
        initSO.ApplyModifiedPropertiesWithoutUndo();

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/_Project/Scenes/Exploration.unity");
        Debug.Log("[SceneSetup] Exploration scene created");
    }

    static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            // Split and create parent folders as needed
            string[] parts = path.Split('/');
            string current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
