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
            Button("Scenes (Init + Gameplay)", GenerateScenes.GenerateAll);
        });

        Section("Generate All (Phase 1)", () =>
        {
            EditorGUILayout.HelpBox(
                "Runs all Phase 1 generators in order:\n" +
                "Terrain tiles → Test map → Unit sprites → Unit prefab → Systems prefab → Scenes",
                MessageType.Info);
            if (GUILayout.Button("Generate All", GUILayout.Height(30)))
                GenerateAllPhase1();
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

    private static void GenerateAllPhase1()
    {
        GenerateTerrainTiles.Generate();
        GenerateTestMap.Generate();
        GenerateUnitPrefab.GenerateSprites();
        GenerateUnitPrefab.GeneratePrefab();
        GenerateSystemsPrefab.Generate();
        GenerateScenes.GenerateAll();
    }
}
