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

        string prefabDir = "Assets/_Project/Prefabs/Units";
        var riflePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabDir}/RifleInfantry.prefab");
        var rocketPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabDir}/RocketSoldier.prefab");
        var ltankPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabDir}/LightTank.prefab");
        var rangerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabDir}/Ranger.prefab");
        var htankPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabDir}/HeavyTank.prefab");
        var artyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabDir}/Artillery.prefab");

        // Allied forces (player 0) — west side
        if (riflePrefab != null) SpawnUnit(riflePrefab, new Vector3(8.5f, 18.5f, 0f), 0);
        if (riflePrefab != null) SpawnUnit(riflePrefab, new Vector3(8.5f, 20.5f, 0f), 0);
        if (rocketPrefab != null) SpawnUnit(rocketPrefab, new Vector3(8.5f, 22.5f, 0f), 0);
        if (ltankPrefab != null) SpawnUnit(ltankPrefab, new Vector3(10.5f, 19.5f, 0f), 0);
        if (ltankPrefab != null) SpawnUnit(ltankPrefab, new Vector3(10.5f, 21.5f, 0f), 0);
        if (rangerPrefab != null) SpawnUnit(rangerPrefab, new Vector3(9.5f, 23.5f, 0f), 0);

        // Soviet forces (player 1) — east side
        if (riflePrefab != null) SpawnUnit(riflePrefab, new Vector3(30.5f, 18.5f, 0f), 1);
        if (riflePrefab != null) SpawnUnit(riflePrefab, new Vector3(30.5f, 20.5f, 0f), 1);
        if (riflePrefab != null) SpawnUnit(riflePrefab, new Vector3(30.5f, 22.5f, 0f), 1);
        if (htankPrefab != null) SpawnUnit(htankPrefab, new Vector3(28.5f, 19.5f, 0f), 1);
        if (htankPrefab != null) SpawnUnit(htankPrefab, new Vector3(28.5f, 21.5f, 0f), 1);
        if (artyPrefab != null) SpawnUnit(artyPrefab, new Vector3(32.5f, 20.5f, 0f), 1);

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"Gameplay scene created at {path}");
    }

    static void SpawnUnit(GameObject prefab, Vector3 position, int playerIndex)
    {
        var unit = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        unit.transform.position = position;

        var entity = unit.GetComponent<Entity>();
        var unitData = entity != null ? new SerializedObject(entity).FindProperty("_unitData").objectReferenceValue as UnitData : null;
        string displayName = unitData != null ? unitData.DisplayName : prefab.name;
        unit.name = $"{displayName} (P{playerIndex})";

        var so = new SerializedObject(entity);
        so.FindProperty("_ownerPlayerIndex").intValue = playerIndex;
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
