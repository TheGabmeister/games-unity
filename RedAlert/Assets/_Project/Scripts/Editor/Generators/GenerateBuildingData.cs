using UnityEditor;
using UnityEngine;

public static class GenerateBuildingData
{
    const string UnitDir = "Assets/_Project/Data/Units";
    const string SpriteDir = "Assets/_Project/Sprites/Buildings";

    public static void GenerateSprites()
    {
        PrefabGeneratorUtils.EnsureFolder(SpriteDir);

        string[] names = {
            "ConstructionYard", "PowerPlant", "AdvancedPower", "Barracks",
            "WarFactory", "RadarDome", "AlliedTech", "SovietTech",
            "Helipad", "NavalYard", "SubPen", "Airfield",
            "ServiceDepot", "Kennel", "Pillbox", "CamoPillbox",
            "GunTurret", "AAGun", "FlameTurret", "TeslaCoil",
            "SAMSite", "GapGenerator", "Chronosphere", "IronCurtain",
            "MissileSilo", "Wall"
        };

        foreach (var name in names)
            ImportSprite($"{SpriteDir}/{name}.png");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Imported {names.Length} building sprites");
    }

    public static void Generate()
    {
        PrefabGeneratorUtils.EnsureFolder(UnitDir);

        var oreTruck = AssetDatabase.LoadAssetAtPath<UnitData>($"{UnitDir}/OreTruck.asset");
        var rifleData = AssetDatabase.LoadAssetAtPath<UnitData>($"{UnitDir}/RifleInfantry.asset");
        var deathSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Sounds/Combat/UnitDeath.wav");

        // ---- Construction Yard ----
        var cy = CreateBuilding("ConstructionYard", "Construction Yard", Faction.Allied,
            3, 3, 1500, ArmorType.Concrete, 0, 5f, 0, 15, false, false);
        cy.ProducesCategory = UnitCategory.Building;
        cy.ExitCellOffset = new Vector2Int(1, -1);
        EditorUtility.SetDirty(cy);

        // ---- Power Plants ----
        var pp = CreateBuilding("PowerPlant", "Power Plant", Faction.Allied,
            2, 2, 400, ArmorType.Wood, 300, 5f, 100, 0, false, false);
        EditorUtility.SetDirty(pp);

        var ap = CreateBuilding("AdvancedPower", "Adv. Power Plant", Faction.Allied,
            3, 3, 700, ArmorType.Wood, 500, 5f, 200, 0, false, false);
        ap.Prerequisites = new[] { pp };
        EditorUtility.SetDirty(ap);

        // ---- Barracks ----
        var barracks = CreateBuilding("Barracks", "Barracks", Faction.Allied,
            2, 2, 500, ArmorType.Wood, 300, 5f, 0, 10, false, false);
        barracks.Prerequisites = new[] { pp };
        barracks.ProducesCategory = UnitCategory.Infantry;
        barracks.ExitCellOffset = new Vector2Int(0, -1);
        EditorUtility.SetDirty(barracks);

        // ---- Ore Refinery (update existing) ----
        var refinery = AssetDatabase.LoadAssetAtPath<UnitData>($"{UnitDir}/OreRefinery.asset");
        if (refinery != null)
        {
            refinery.PowerConsumed = 30;
            refinery.Prerequisites = new[] { pp };
            EditorUtility.SetDirty(refinery);
        }

        // ---- Ore Silo (update existing) ----
        var silo = AssetDatabase.LoadAssetAtPath<UnitData>($"{UnitDir}/OreSilo.asset");
        if (silo != null)
        {
            silo.Prerequisites = refinery != null ? new[] { refinery } : null;
            EditorUtility.SetDirty(silo);
        }

        // ---- War Factory ----
        var wf = CreateBuilding("WarFactory", "War Factory", Faction.Allied,
            3, 2, 1000, ArmorType.Wood, 2000, 5f, 0, 25, false, false);
        wf.Prerequisites = refinery != null ? new[] { refinery } : null;
        wf.ProducesCategory = UnitCategory.Vehicle;
        wf.ExitCellOffset = new Vector2Int(1, -1);
        wf.FreeUnit = null;
        EditorUtility.SetDirty(wf);

        // ---- Radar Dome ----
        var radar = CreateBuilding("RadarDome", "Radar Dome", Faction.Allied,
            2, 2, 1000, ArmorType.Wood, 1000, 10f, 0, 40, true, false);
        radar.Prerequisites = refinery != null ? new[] { refinery } : null;
        EditorUtility.SetDirty(radar);

        // ---- Allied Tech Center ----
        var atech = CreateBuilding("AlliedTech", "Allied Tech Center", Faction.Allied,
            2, 2, 800, ArmorType.Wood, 1500, 5f, 0, 40, true, false);
        atech.Prerequisites = new[] { radar };
        EditorUtility.SetDirty(atech);

        // ---- Soviet Tech Center ----
        var stech = CreateBuilding("SovietTech", "Soviet Tech Center", Faction.Soviet,
            3, 3, 1200, ArmorType.Wood, 1500, 5f, 0, 40, true, false);
        stech.Prerequisites = new[] { radar };
        EditorUtility.SetDirty(stech);

        // ---- Helipad ----
        var hpad = CreateBuilding("Helipad", "Helipad", Faction.Allied,
            2, 2, 600, ArmorType.Wood, 1500, 5f, 0, 10, true, false);
        hpad.Prerequisites = new[] { radar };
        hpad.ProducesCategory = UnitCategory.Aircraft;
        hpad.ExitCellOffset = new Vector2Int(0, 0);
        EditorUtility.SetDirty(hpad);

        // ---- Naval Yard ----
        var navalYard = CreateBuilding("NavalYard", "Naval Yard", Faction.Allied,
            3, 3, 900, ArmorType.Wood, 650, 5f, 0, 20, false, false);
        navalYard.Prerequisites = new[] { pp };
        navalYard.ProducesCategory = UnitCategory.Naval;
        navalYard.ExitCellOffset = new Vector2Int(1, -1);
        EditorUtility.SetDirty(navalYard);

        // ---- Sub Pen ----
        var subPen = CreateBuilding("SubPen", "Sub Pen", Faction.Soviet,
            3, 3, 900, ArmorType.Wood, 650, 5f, 0, 20, false, false);
        subPen.Prerequisites = new[] { pp };
        subPen.ProducesCategory = UnitCategory.Naval;
        subPen.ExitCellOffset = new Vector2Int(1, -1);
        EditorUtility.SetDirty(subPen);

        // ---- Airfield ----
        var airfield = CreateBuilding("Airfield", "Airfield", Faction.Soviet,
            3, 2, 800, ArmorType.Wood, 600, 5f, 0, 20, true, false);
        airfield.Prerequisites = new[] { radar };
        airfield.ProducesCategory = UnitCategory.Aircraft;
        airfield.ExitCellOffset = new Vector2Int(1, 0);
        EditorUtility.SetDirty(airfield);

        // ---- Service Depot ----
        var depot = CreateBuilding("ServiceDepot", "Service Depot", Faction.Allied,
            3, 2, 800, ArmorType.Wood, 1200, 5f, 0, 15, true, false);
        depot.Prerequisites = new[] { wf };
        EditorUtility.SetDirty(depot);

        // ---- Kennel ----
        var kennel = CreateBuilding("Kennel", "Kennel", Faction.Soviet,
            1, 1, 400, ArmorType.Wood, 200, 4f, 0, 5, false, false);
        kennel.Prerequisites = new[] { barracks };
        kennel.ProducesCategory = UnitCategory.Infantry;
        kennel.ExitCellOffset = new Vector2Int(0, -1);
        EditorUtility.SetDirty(kennel);

        // ---- Defenses ----
        var pillbox = CreateBuilding("Pillbox", "Pillbox", Faction.Allied,
            1, 1, 400, ArmorType.Concrete, 400, 5f, 0, 10, false, false);
        pillbox.Prerequisites = new[] { barracks };
        EditorUtility.SetDirty(pillbox);

        var camoPillbox = CreateBuilding("CamoPillbox", "Camo Pillbox", Faction.Allied,
            1, 1, 400, ArmorType.Concrete, 600, 5f, 0, 10, false, false);
        camoPillbox.Prerequisites = new[] { barracks };
        EditorUtility.SetDirty(camoPillbox);

        var gunTurret = CreateBuilding("GunTurret", "Gun Turret", Faction.Allied,
            1, 1, 400, ArmorType.Concrete, 600, 5f, 0, 20, true, false);
        gunTurret.Prerequisites = new[] { barracks };
        EditorUtility.SetDirty(gunTurret);

        var aaGun = CreateBuilding("AAGun", "AA Gun", Faction.Allied,
            1, 2, 600, ArmorType.Concrete, 600, 5f, 0, 25, true, false);
        aaGun.Prerequisites = new[] { radar };
        EditorUtility.SetDirty(aaGun);

        var flameTurret = CreateBuilding("FlameTurret", "Flame Turret", Faction.Soviet,
            1, 1, 300, ArmorType.Concrete, 600, 4f, 0, 15, false, false);
        flameTurret.Prerequisites = new[] { barracks };
        EditorUtility.SetDirty(flameTurret);

        var teslaCoil = CreateBuilding("TeslaCoil", "Tesla Coil", Faction.Soviet,
            1, 2, 600, ArmorType.Concrete, 1500, 6f, 0, 75, true, false);
        teslaCoil.Prerequisites = new[] { stech };
        EditorUtility.SetDirty(teslaCoil);

        var samSite = CreateBuilding("SAMSite", "SAM Site", Faction.Soviet,
            2, 1, 600, ArmorType.Concrete, 750, 5f, 0, 20, true, false);
        samSite.Prerequisites = new[] { radar };
        EditorUtility.SetDirty(samSite);

        // ---- Special Buildings ----
        var gapGen = CreateBuilding("GapGenerator", "Gap Generator", Faction.Allied,
            1, 2, 600, ArmorType.Wood, 500, 5f, 0, 30, true, false);
        gapGen.Prerequisites = new[] { atech };
        EditorUtility.SetDirty(gapGen);

        var chronosphere = CreateBuilding("Chronosphere", "Chronosphere", Faction.Allied,
            2, 2, 1000, ArmorType.Concrete, 2800, 5f, 0, 50, true, false);
        chronosphere.Prerequisites = new[] { atech };
        EditorUtility.SetDirty(chronosphere);

        var ironCurtain = CreateBuilding("IronCurtain", "Iron Curtain", Faction.Soviet,
            2, 2, 1000, ArmorType.Concrete, 2800, 5f, 0, 50, true, false);
        ironCurtain.Prerequisites = new[] { stech };
        EditorUtility.SetDirty(ironCurtain);

        var mslo = CreateBuilding("MissileSilo", "Missile Silo", Faction.Allied,
            2, 1, 1000, ArmorType.Concrete, 2500, 5f, 0, 100, true, false);
        mslo.Prerequisites = new[] { atech };
        EditorUtility.SetDirty(mslo);

        // ---- Wall ----
        var wall = CreateBuilding("Wall", "Wall", Faction.Allied,
            1, 1, 100, ArmorType.Concrete, 50, 0f, 0, 0, false, true);
        wall.Prerequisites = new[] { barracks };
        EditorUtility.SetDirty(wall);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated all building UnitData assets");
    }

    static UnitData CreateBuilding(string fileName, string displayName, Faction faction,
        int footX, int footY, int maxHP, ArmorType armor, int cost, float sight,
        int powerProduced, int powerConsumed, bool requiresPower, bool isWall)
    {
        string path = $"{UnitDir}/{fileName}.asset";
        var data = LoadOrCreate<UnitData>(path);

        data.DisplayName = displayName;
        data.Category = UnitCategory.Building;
        data.Faction = faction;
        data.Cost = cost;
        data.MaxHP = maxHP;
        data.Armor = armor;
        data.SightRange = sight;
        data.FootprintX = footX;
        data.FootprintY = footY;
        data.PowerProduced = powerProduced;
        data.PowerConsumed = powerConsumed;
        data.RequiresPower = requiresPower;
        data.IsWall = isWall;

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/{fileName}.png");
        if (sprite != null)
            data.Sprite = sprite;

        return data;
    }

    static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;

        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    static void ImportSprite(string path)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 64;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }
}
