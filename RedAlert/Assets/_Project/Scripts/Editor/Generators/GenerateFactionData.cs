using UnityEditor;
using UnityEngine;

public static class GenerateFactionData
{
    const string DataDir = "Assets/_Project/Data";
    const string UnitDir = "Assets/_Project/Data/Units";

    public static void Generate()
    {
        PrefabGeneratorUtils.EnsureFolder(DataDir);

        GenerateAllied();
        GenerateSoviet();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated Allied + Soviet FactionData assets");
    }

    static void GenerateAllied()
    {
        string path = $"{DataDir}/AlliedFaction.asset";
        var data = LoadOrCreate<FactionData>(path);
        data.Faction = Faction.Allied;

        data.BuildableStructures = new[]
        {
            Load("PowerPlant"),
            Load("AdvancedPower"),
            Load("Barracks"),
            Load("OreRefinery"),
            Load("OreSilo"),
            Load("WarFactory"),
            Load("RadarDome"),
            Load("Helipad"),
            Load("NavalYard"),
            Load("AlliedTech"),
            Load("ServiceDepot"),
            Load("Pillbox"),
            Load("CamoPillbox"),
            Load("GunTurret"),
            Load("AAGun"),
            Load("GapGenerator"),
            Load("Chronosphere"),
            Load("MissileSilo"),
            Load("Wall"),
        };

        data.BuildableUnits = new[]
        {
            Load("RifleInfantry"),
            Load("RocketSoldier"),
            Load("LightTank"),
            Load("Ranger"),
            Load("Artillery"),
            Load("OreTruck"),
        };

        EditorUtility.SetDirty(data);
    }

    static void GenerateSoviet()
    {
        string path = $"{DataDir}/SovietFaction.asset";
        var data = LoadOrCreate<FactionData>(path);
        data.Faction = Faction.Soviet;

        data.BuildableStructures = new[]
        {
            Load("PowerPlant"),
            Load("AdvancedPower"),
            Load("Barracks"),
            Load("OreRefinery"),
            Load("OreSilo"),
            Load("WarFactory"),
            Load("RadarDome"),
            Load("Kennel"),
            Load("SubPen"),
            Load("Airfield"),
            Load("Helipad"),
            Load("SovietTech"),
            Load("ServiceDepot"),
            Load("FlameTurret"),
            Load("TeslaCoil"),
            Load("SAMSite"),
            Load("AAGun"),
            Load("IronCurtain"),
            Load("MissileSilo"),
            Load("Wall"),
        };

        data.BuildableUnits = new[]
        {
            Load("RifleInfantry"),
            Load("RocketSoldier"),
            Load("HeavyTank"),
            Load("Artillery"),
            Load("OreTruck"),
        };

        EditorUtility.SetDirty(data);
    }

    static UnitData Load(string name)
    {
        return AssetDatabase.LoadAssetAtPath<UnitData>($"{UnitDir}/{name}.asset");
    }

    static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;

        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }
}
