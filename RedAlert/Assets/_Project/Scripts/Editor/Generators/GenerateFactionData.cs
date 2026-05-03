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
            Load("Engineer"),
            Load("Medic"),
            Load("Spy"),
            Load("Tanya"),
            Load("LightTank"),
            Load("Ranger"),
            Load("APC"),
            Load("Artillery"),
            Load("MineLayer"),
            Load("ChronoTank"),
            Load("RadarJammer"),
            Load("PhaseTransport"),
            Load("MCV"),
            Load("OreTruck"),
            Load("Longbow"),
            Load("Chinook"),
            Load("Destroyer"),
            Load("Cruiser"),
            Load("Gunboat"),
            Load("TransportShip"),
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
            Load("Grenadier"),
            Load("Flamethrower"),
            Load("Engineer"),
            Load("ShockTrooper"),
            Load("AttackDog"),
            Load("HeavyTank"),
            Load("MammothTank"),
            Load("V2Launcher"),
            Load("TeslaTank"),
            Load("DemoTruck"),
            Load("APC"),
            Load("Artillery"),
            Load("MCV"),
            Load("OreTruck"),
            Load("Hind"),
            Load("MiG"),
            Load("Yak"),
            Load("Chinook"),
            Load("Submarine"),
            Load("MissileSub"),
            Load("TransportShip"),
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
