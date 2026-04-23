using UnityEditor;
using UnityEngine;

public static class GenerateBootstrapPrefabs
{
    private const string PrefabDir = "Assets/_Project/Prefabs";
    private const string BootstrapperPrefabPath = PrefabDir + "/Bootstrapper.prefab";
    private const string AudioSystemPrefabPath = PrefabDir + "/AudioSystem.prefab";

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
