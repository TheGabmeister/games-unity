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
            Button("Fog Tiles (Shroud + Fog)", GenerateFogTiles.Generate);
            Button("Test Map (40x40)", GenerateTestMap.Generate);
        });

        Section("Sprites", () =>
        {
            Button("Unit Sprites (UI)", GenerateUnitPrefab.GenerateSprites);
            Button("Projectile Sprites", GenerateCombatData.GenerateProjectileSprites);
            Button("Cursor Textures (7)", GenerateCursors.Generate);
        });

        Section("Data", () =>
        {
            Button("Warheads (8)", GenerateCombatData.GenerateWarheads);
            Button("Projectiles (6)", GenerateCombatData.GenerateProjectiles);
            Button("Weapons (5)", GenerateCombatData.GenerateWeapons);
            Button("Unit Data (6 units)", GenerateCombatData.GenerateUnitData);
            Button("All Combat Data", GenerateCombatData.GenerateAll);
        });

        Section("Economy", () =>
        {
            Button("Overlay Tiles (ore/gem density)", GenerateEconomyData.GenerateOverlayTiles);
            Button("Economy Sprites", GenerateEconomyData.GenerateBuildingSprites);
            Button("Ore Truck Unit", GenerateEconomyData.GenerateOreUnit);
            Button("Economy Data (Refinery, Silo)", GenerateEconomyData.GenerateBuildingData);
            Button("Ore Truck Prefab", GenerateEconomyData.GenerateOreTruckPrefab);
            Button("All Economy", GenerateEconomyData.GenerateAll);
        });

        Section("Buildings", () =>
        {
            Button("Building Sprites (import)", GenerateBuildingData.GenerateSprites);
            Button("Building Data (all buildings)", GenerateBuildingData.Generate);
            Button("Faction Data (Allied + Soviet)", GenerateFactionData.Generate);
            Button("Building Prefabs (all)", GenerateBuildingPrefabs.Generate);
        });

        Section("Phase 7 — Units", () =>
        {
            Button("Import Sprite Sheets", GeneratePhase7Data.ImportSpriteSheets);
            Button("Phase 7 Weapons + Projectiles", GeneratePhase7Data.GenerateNewWeapons);
            Button("Phase 7 Unit Data (29 units)", GeneratePhase7Data.GenerateNewUnits);
            Button("Wire Direction Sprites", GeneratePhase7Data.WireDirectionSprites);
            Button("Phase 7 All", GeneratePhase7Data.GenerateAll);
        });

        Section("UI", () =>
        {
            Button("Build Slot Prefab", GenerateSidebarPrefab.GenerateBuildSlotPrefab);
            Button("Sidebar Canvas Prefab", GenerateSidebarPrefab.Generate);
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
                "Terrain → Sprites → Data → Buildings → UI → Prefabs → Scene",
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
        GenerateFogTiles.Generate();
        GenerateTestMap.Generate();
        GenerateUnitPrefab.GenerateSprites();
        GenerateCursors.Generate();
        GenerateCombatData.GenerateAll();
        GenerateEconomyData.GenerateAll();
        GeneratePhase7Data.GenerateAll();
        GenerateBuildingData.GenerateSprites();
        GenerateBuildingData.Generate();
        GenerateFactionData.Generate();
        GenerateUnitPrefab.GeneratePrefab();
        GenerateUnitPrefabs.Generate();
        GenerateBuildingPrefabs.Generate();
        GenerateSidebarPrefab.GenerateBuildSlotPrefab();
        GenerateSidebarPrefab.Generate();
        GenerateSystemsPrefab.Generate();
        GenerateScenes.GenerateAll();
    }
}
