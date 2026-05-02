using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public static class GenerateScenes
{
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
        var rifleData = AssetDatabase.LoadAssetAtPath<UnitData>("Assets/_Project/Data/Units/RifleInfantry.asset");
        var tankData = AssetDatabase.LoadAssetAtPath<UnitData>("Assets/_Project/Data/Units/LightTank.asset");
        var rangerData = AssetDatabase.LoadAssetAtPath<UnitData>("Assets/_Project/Data/Units/Ranger.asset");

        if (unitPrefab != null)
        {
            SpawnUnit(unitPrefab, new Vector3(5.5f, 5.5f, 0f), 0, rifleData);
            SpawnUnit(unitPrefab, new Vector3(7.5f, 5.5f, 0f), 0, tankData);
            SpawnUnit(unitPrefab, new Vector3(6.5f, 7.5f, 0f), 0, rangerData);

            SpawnUnit(unitPrefab, new Vector3(33.5f, 33.5f, 0f), 1, rifleData);
            SpawnUnit(unitPrefab, new Vector3(35.5f, 33.5f, 0f), 1, tankData);
            SpawnUnit(unitPrefab, new Vector3(34.5f, 35.5f, 0f), 1, rangerData);
        }

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"Gameplay scene created at {path}");
    }

    static void SpawnUnit(GameObject prefab, Vector3 position, int playerIndex, UnitData unitData)
    {
        var unit = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        unit.transform.position = position;

        if (unitData != null)
            unit.name = $"{unitData.DisplayName} (P{playerIndex})";

        var so = new SerializedObject(unit.GetComponent<Entity>());
        so.FindProperty("_ownerPlayerIndex").intValue = playerIndex;
        so.FindProperty("_unitData").objectReferenceValue = unitData;
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
            new("Assets/_Project/Scenes/Gameplay.unity", true)
        };
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("Build settings updated: Gameplay (0)");
    }

    public static void GenerateAll()
    {
        GenerateGameplay();
        UpdateBuildSettings();
    }
}
