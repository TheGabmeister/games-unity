using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GenerateBootstrapScene
{
    private const string SceneDir = "Assets/_Project/Scenes";
    private const string ScenePath = SceneDir + "/_Bootstrap.unity";
    private const string BootstrapperPrefabPath = "Assets/_Project/Prefabs/Bootstrapper.prefab";

    [MenuItem("Tools/DigimonWorld/Generate Bootstrap Scene")]
    public static void Generate()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapperPrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"Bootstrapper prefab not found at {BootstrapperPrefabPath}. Run 'Tools/DigimonWorld/Generate Bootstrap Prefabs' first.");
            return;
        }

        EnsureFolder(SceneDir);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        PrefabUtility.InstantiatePrefab(prefab, scene);

        bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
        if (!saved)
        {
            Debug.LogError($"Failed to save scene at {ScenePath}");
            return;
        }

        AddSceneToBuildSettings(ScenePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Bootstrap scene generated at {ScenePath}");
    }

    private static void AddSceneToBuildSettings(string scenePath)
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
        newScenes[0] = new EditorBuildSettingsScene(scenePath, true);
        System.Array.Copy(scenes, 0, newScenes, 1, scenes.Length);
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
