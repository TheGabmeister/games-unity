using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public static class GenerateScenes
{
    public static void GenerateInit()
    {
        string path = "Assets/_Project/Scenes/Init.unity";
        PrefabGeneratorUtils.EnsureFolder("Assets/_Project/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGO = new GameObject("Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGO.transform.position = new Vector3(0, 0, -10);
        camGO.tag = "MainCamera";

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"Init scene created at {path}");
    }

    public static void GenerateGameplay()
    {
        string path = "Assets/_Project/Scenes/Gameplay.unity";
        PrefabGeneratorUtils.EnsureFolder("Assets/_Project/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 10;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGO.transform.position = new Vector3(20, 20, -10);
        camGO.tag = "MainCamera";
        camGO.AddComponent<RTSCamera>();

        var gridGO = new GameObject("Grid");
        gridGO.AddComponent<Grid>();

        var tilemapGO = new GameObject("Tilemap");
        tilemapGO.transform.SetParent(gridGO.transform, false);
        tilemapGO.AddComponent<Tilemap>();
        var renderer = tilemapGO.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = 0;

        var systemsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Systems.prefab");
        if (systemsPrefab != null)
            PrefabUtility.InstantiatePrefab(systemsPrefab);

        var unitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Units/PlaceholderUnit.prefab");
        if (unitPrefab != null)
        {
            SpawnUnit(unitPrefab, new Vector3(5.5f, 5.5f, 0f), 0);
            SpawnUnit(unitPrefab, new Vector3(7.5f, 5.5f, 0f), 0);
            SpawnUnit(unitPrefab, new Vector3(6.5f, 7.5f, 0f), 0);

            SpawnUnit(unitPrefab, new Vector3(33.5f, 33.5f, 0f), 1);
            SpawnUnit(unitPrefab, new Vector3(35.5f, 33.5f, 0f), 1);
            SpawnUnit(unitPrefab, new Vector3(34.5f, 35.5f, 0f), 1);
        }

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"Gameplay scene created at {path}");
    }

    static void SpawnUnit(GameObject prefab, Vector3 position, int playerIndex)
    {
        var unit = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        unit.transform.position = position;

        var so = new SerializedObject(unit.GetComponent<Entity>());
        so.FindProperty("_ownerPlayerIndex").intValue = playerIndex;
        so.FindProperty("_entityName").stringValue = "Placeholder";
        so.ApplyModifiedPropertiesWithoutUndo();

        var sr = unit.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color color = playerIndex == 0
                ? new Color(0.2f, 0.6f, 1f)
                : new Color(1f, 0.3f, 0.3f);
            sr.color = color;
        }
    }

    public static void UpdateBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>
        {
            new("Assets/_Project/Scenes/Init.unity", true),
            new("Assets/_Project/Scenes/Gameplay.unity", true)
        };
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("Build settings updated: Init (0), Gameplay (1)");
    }

    public static void GenerateAll()
    {
        GenerateInit();
        GenerateGameplay();
        UpdateBuildSettings();
    }
}
