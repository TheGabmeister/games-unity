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

        Section("Phase 1 — Foundation", () =>
        {
            Button("Terrain Tile Sprites + Tiles", GenerateTerrainTiles.Generate);
            Button("Test Map (40x40)", GenerateTestMap.Generate);
            Button("Unit Sprites", GenerateUnitPrefab.GenerateSprites);
            Button("Unit Prefab", GenerateUnitPrefab.GeneratePrefab);
            Button("Systems Prefab", GenerateSystemsPrefab.Generate);
            Button("Gameplay Scene", GenerateScenes.GenerateAll);
        });

        Section("Phase 2 — Movement", () =>
        {
            Button("Unit Data (Rifle, Tank, Ranger)", GenerateUnitData.Generate);
        });

        Section("Generate All", () =>
        {
            EditorGUILayout.HelpBox(
                "Runs all generators in order:\n" +
                "Terrain tiles → Test map → Unit sprites → Unit data →\n" +
                "Unit prefab → Systems prefab → Gameplay scene",
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
        GenerateUnitData.Generate();
        GenerateUnitPrefab.GeneratePrefab();
        GenerateSystemsPrefab.Generate();
        GenerateScenes.GenerateAll();
    }
}
