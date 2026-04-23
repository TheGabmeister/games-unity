using UnityEditor;
using UnityEngine;

public static class GenerateBootstrapPrefabs
{
    private const string PrefabDir = "Assets/_Project/Prefabs";
    private const string BootstrapperPrefabPath = PrefabDir + "/Bootstrapper.prefab";

    [MenuItem("Tools/DigimonWorld/Generate Bootstrap Prefabs")]
    public static void Generate()
    {
        EnsureFolder(PrefabDir);

        GameObject source = new GameObject("Bootstrapper");
        try
        {
            //source.AddComponent<Bootstrapper>();
            PrefabUtility.SaveAsPrefabAsset(source, BootstrapperPrefabPath, out bool success);
            if (!success)
            {
                Debug.LogError($"Failed to save prefab at {BootstrapperPrefabPath}");
                return;
            }
        }
        finally
        {
            Object.DestroyImmediate(source);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Bootstrap prefabs generated at {PrefabDir}");
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
