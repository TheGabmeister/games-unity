using UnityEditor;
using UnityEngine;

public class GeneratorWindow : EditorWindow
{
    private Vector2 _scrollPos;

    [MenuItem("Tools/RedAlert/Generator Window")]
    public static void ShowWindow()
    {
        GetWindow<GeneratorWindow>("RA Generators");
    }

    private void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        Section("Terrain", () =>
        {
            Button("Tile Sprites + Tiles", GenerateTerrainTiles.Generate);
            Button("Test Map (40x40)", GenerateTestMap.Generate);
        });

        Section("Sprites", () =>
        {
            Button("Unit Sprites (UI)", GenerateUnitPrefab.GenerateSprites);
            Button("Projectile Sprites", GenerateCombatData.GenerateProjectileSprites);
        });

        Section("Data", () =>
        {
            Button("Warheads (8)", GenerateCombatData.GenerateWarheads);
            Button("Projectiles (6)", GenerateCombatData.GenerateProjectiles);
            Button("Weapons (5)", GenerateCombatData.GenerateWeapons);
            Button("Unit Data (6 units)", GenerateCombatData.GenerateUnitData);
            Button("All Combat Data", GenerateCombatData.GenerateAll);
        });

        Section("Prefabs", () =>
        {
            Button("Placeholder Unit Prefab", GenerateUnitPrefab.GeneratePrefab);
            Button("Unit Prefabs (individual)", GenerateUnitPrefabs.Generate);
            Button("Systems Prefab", GenerateSystemsPrefab.Generate);
        });

        Section("Scenes", () =>
        {
            Button("Gameplay Scene", GenerateScenes.GenerateAll);
        });

        Section("Generate All", () =>
        {
            EditorGUILayout.HelpBox(
                "Runs all generators in order:\n" +
                "Terrain → Sprites → Data → Prefabs → Scene",
                MessageType.Info);
            if (GUILayout.Button("Generate All", GUILayout.Height(30)))
                GenerateAll();
        });

        EditorGUILayout.EndScrollView();
    }

    private static void Section(string title, System.Action content)
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        content();
        EditorGUI.indentLevel--;
    }

    private static void Button(string label, System.Action action)
    {
        if (GUILayout.Button(label))
            action();
    }

    private static void GenerateAll()
    {
        GenerateTerrainTiles.Generate();
        GenerateTestMap.Generate();
        GenerateUnitPrefab.GenerateSprites();
        GenerateCombatData.GenerateAll();
        GenerateUnitPrefab.GeneratePrefab();
        GenerateUnitPrefabs.Generate();
        GenerateSystemsPrefab.Generate();
        GenerateScenes.GenerateAll();
    }
}
