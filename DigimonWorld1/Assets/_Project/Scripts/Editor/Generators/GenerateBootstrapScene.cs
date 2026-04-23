using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GenerateBootstrapScene
{
    private const string SceneDir = "Assets/_Project/Scenes";
    private const string BootstrapScenePath = SceneDir + "/_Bootstrap.unity";
    private const string SplashscreenScenePath = SceneDir + "/_Splashscreen.unity";
    private const string MainMenuScenePath = SceneDir + "/_MainMenu.unity";
    private const string NameScenePath = SceneDir + "/_Name.unity";
    private const string AudioSystemPrefabPath = "Assets/_Project/Prefabs/AudioSystem.prefab";
    private const string GameManagerPrefabPath = "Assets/_Project/Prefabs/GameManager.prefab";

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
        GenerateEmptyScene(SplashscreenScenePath, "_Splashscreen");
    }

    [MenuItem("Tools/DigimonWorld/Scenes/Generate MainMenu Scene")]
    public static void GenerateMainMenu()
    {
        GenerateEmptyScene(MainMenuScenePath, "_MainMenu");
    }

    [MenuItem("Tools/DigimonWorld/Scenes/Generate Name Scene")]
    public static void GenerateName()
    {
        GenerateEmptyScene(NameScenePath, "_Name");
    }

    [MenuItem("Tools/DigimonWorld/Scenes/Generate All Scenes")]
    public static void GenerateAll()
    {
        GenerateBootstrap();
        GenerateSplashscreen();
        GenerateMainMenu();
        GenerateName();
    }

    private static void GenerateEmptyScene(string scenePath, string displayName)
    {
        EnsureFolder(SceneDir);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        if (!SaveScene(scene, scenePath)) return;

        AppendSceneToBuildSettings(scenePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"{displayName} scene generated at {scenePath}");
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
