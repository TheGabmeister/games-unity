using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class GenerateBootstrapScene
{
    private const string SceneDir = "Assets/_Project/Scenes";
    private const string BootstrapScenePath = SceneDir + "/_Bootstrap.unity";
    private const string SplashscreenScenePath = SceneDir + "/_Splashscreen.unity";
    private const string IntroScenePath = SceneDir + "/_Intro.unity";
    private const string MainMenuScenePath = SceneDir + "/_MainMenu.unity";
    private const string NameScenePath = SceneDir + "/_Name.unity";
    private const string GameplayScenePath = SceneDir + "/_Gameplay.unity";

    private const string PrefabDir = "Assets/_Project/Prefabs";
    private const string AudioSystemPrefabPath = PrefabDir + "/AudioSystem.prefab";
    private const string GameManagerPrefabPath = PrefabDir + "/GameManager.prefab";
    private const string SplashscreenControllerPrefabPath = PrefabDir + "/SplashscreenController.prefab";
    private const string IntroControllerPrefabPath = PrefabDir + "/IntroController.prefab";
    private const string MainMenuControllerPrefabPath = PrefabDir + "/MainMenuController.prefab";
    private const string NameControllerPrefabPath = PrefabDir + "/NameController.prefab";

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

        EnsureFolder(SceneDir);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        PrefabUtility.InstantiatePrefab(audioSystemPrefab, scene);
        PrefabUtility.InstantiatePrefab(gameManagerPrefab, scene);

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
        GenerateEmptyScene(GameplayScenePath, "_Gameplay");
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
    }

    private static void GenerateSceneWithPrefab(string scenePath, string prefabPath, string displayName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found at {prefabPath}. Generate prefabs first.");
            return;
        }

        EnsureFolder(SceneDir);

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
        EnsureFolder(SceneDir);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera(scene);

        if (!SaveScene(scene, scenePath)) return;

        AppendSceneToBuildSettings(scenePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"{displayName} scene generated at {scenePath}");
    }

    private static void CreateCamera(Scene scene)
    {
        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        camGo.AddComponent<UniversalAdditionalCameraData>();
        SceneManager.MoveGameObjectToScene(camGo, scene);
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
