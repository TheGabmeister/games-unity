using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public static class GenerateSystemsPrefab
{
    public static void Generate()
    {
        string path = "Assets/_Project/Prefabs/Systems.prefab";
        PrefabGeneratorUtils.EnsureFolder("Assets/_Project/Prefabs");

        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        GameObject instance;

        if (existing != null)
        {
            instance = (GameObject)PrefabUtility.InstantiatePrefab(existing);
        }
        else
        {
            instance = new GameObject("Systems");
        }

        try
        {
            AddIfMissing<InputManager>(instance);
            AddIfMissing<PlayerManager>(instance);
            AddIfMissing<MapManager>(instance);
            AddIfMissing<SelectionManager>(instance);

            var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                "Assets/_Project/Input/InputSystem_Actions.inputactions");
            if (inputAsset != null)
            {
                var so = new SerializedObject(instance.GetComponent<InputManager>());
                so.FindProperty("_inputActions").objectReferenceValue = inputAsset;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            WireMapManager(instance);

            PrefabUtility.SaveAsPrefabAsset(instance, path);
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Systems prefab updated");
    }

    static T AddIfMissing<T>(GameObject go) where T : Component
    {
        var existing = go.GetComponent<T>();
        return existing != null ? existing : go.AddComponent<T>();
    }

    static void WireMapManager(GameObject instance)
    {
        var mapManager = instance.GetComponent<MapManager>();
        var so = new SerializedObject(mapManager);

        var mapData = AssetDatabase.LoadAssetAtPath<MapData>("Assets/_Project/Data/TestMap.asset");
        if (mapData != null)
            so.FindProperty("_mapData").objectReferenceValue = mapData;

        string tileDir = "Assets/_Project/Tiles";
        SetTile(so, "_clearTile", $"{tileDir}/Clear.asset");
        SetTile(so, "_roadTile", $"{tileDir}/Road.asset");
        SetTile(so, "_roughTile", $"{tileDir}/Rough.asset");
        SetTile(so, "_sandTile", $"{tileDir}/Sand.asset");
        SetTile(so, "_waterTile", $"{tileDir}/Water.asset");
        SetTile(so, "_oreTile", $"{tileDir}/Ore.asset");
        SetTile(so, "_gemsTile", $"{tileDir}/Gems.asset");

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetTile(SerializedObject so, string property, string path)
    {
        var tile = AssetDatabase.LoadAssetAtPath<TileBase>(path);
        if (tile != null)
            so.FindProperty(property).objectReferenceValue = tile;
    }
}
