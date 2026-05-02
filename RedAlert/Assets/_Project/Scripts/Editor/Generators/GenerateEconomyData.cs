using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class GenerateEconomyData
{
    const string UnitDir = "Assets/_Project/Data/Units";
    const string SpriteDir = "Assets/_Project/Sprites";
    const string TileDir = "Assets/_Project/Tiles";
    const string PrefabDir = "Assets/_Project/Prefabs";

    public static void GenerateOverlayTiles()
    {
        PrefabGeneratorUtils.EnsureFolder(TileDir);
        string overlayDir = $"{SpriteDir}/Overlays";

        string[] oreNames = { "OreDensity1", "OreDensity2", "OreDensity3", "OreDensity4" };
        string[] gemNames = { "GemDensity1", "GemDensity2", "GemDensity3", "GemDensity4" };

        foreach (var name in oreNames)
            ImportSprite($"{overlayDir}/{name}.png");
        foreach (var name in gemNames)
            ImportSprite($"{overlayDir}/{name}.png");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        foreach (var name in oreNames)
            CreateTile(name, $"{overlayDir}/{name}.png");
        foreach (var name in gemNames)
            CreateTile(name, $"{overlayDir}/{name}.png");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated 8 overlay tiles (4 ore + 4 gem density)");
    }

    public static void GenerateBuildingSprites()
    {
        string dir = $"{SpriteDir}/Buildings";
        PrefabGeneratorUtils.EnsureFolder(dir);

        ImportSprite($"{dir}/OreRefinery.png");
        ImportSprite($"{dir}/OreSilo.png");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated building sprites");
    }

    public static void GenerateOreUnit()
    {
        PrefabGeneratorUtils.EnsureFolder(UnitDir);
        string spriteDir = $"{SpriteDir}/Units";

        ImportSprite($"{spriteDir}/OreTruck.png");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var deathSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Sounds/Combat/UnitDeath.wav");
        var rifleData = AssetDatabase.LoadAssetAtPath<UnitData>($"{UnitDir}/RifleInfantry.asset");

        var oreTruck = LoadOrCreate<UnitData>($"{UnitDir}/OreTruck.asset");
        oreTruck.DisplayName = "Ore Truck";
        oreTruck.Sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{spriteDir}/OreTruck.png");
        oreTruck.Category = UnitCategory.Vehicle;
        oreTruck.Faction = Faction.Allied;
        oreTruck.Cost = 1400;
        oreTruck.Locomotion = LocomotionType.Wheeled;
        oreTruck.BaseSpeed = 4f;
        oreTruck.MaxHP = 600;
        oreTruck.Armor = ArmorType.Heavy;
        oreTruck.PrimaryWeapon = null;
        oreTruck.SightRange = 4f;
        oreTruck.IsCrusher = false;
        oreTruck.NoMovingFire = false;
        oreTruck.IsCrewedVehicle = true;
        oreTruck.ExplodesOnDeath = false;
        oreTruck.DeathWarhead = null;
        oreTruck.DeathSound = deathSound;
        oreTruck.BailOutUnit = rifleData;
        EditorUtility.SetDirty(oreTruck);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated Ore Truck UnitData");
    }

    public static void GenerateBuildingData()
    {
        PrefabGeneratorUtils.EnsureFolder(UnitDir);
        string spriteDir = $"{SpriteDir}/Buildings";

        var oreTruck = AssetDatabase.LoadAssetAtPath<UnitData>($"{UnitDir}/OreTruck.asset");

        var refinery = LoadOrCreate<UnitData>($"{UnitDir}/OreRefinery.asset");
        refinery.DisplayName = "Ore Refinery";
        refinery.Sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{spriteDir}/OreRefinery.png");
        refinery.Category = UnitCategory.Building;
        refinery.Faction = Faction.Allied;
        refinery.Cost = 2000;
        refinery.MaxHP = 900;
        refinery.Armor = ArmorType.Wood;
        refinery.SightRange = 5f;
        refinery.FootprintX = 3;
        refinery.FootprintY = 3;
        refinery.StorageCapacity = 2000;
        refinery.RequiresPower = true;
        refinery.FreeUnit = oreTruck;
        EditorUtility.SetDirty(refinery);

        var silo = LoadOrCreate<UnitData>($"{UnitDir}/OreSilo.asset");
        silo.DisplayName = "Ore Silo";
        silo.Sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{spriteDir}/OreSilo.png");
        silo.Category = UnitCategory.Building;
        silo.Faction = Faction.Allied;
        silo.Cost = 150;
        silo.MaxHP = 300;
        silo.Armor = ArmorType.Wood;
        silo.SightRange = 4f;
        silo.FootprintX = 2;
        silo.FootprintY = 2;
        silo.StorageCapacity = 1500;
        silo.RequiresPower = false;
        silo.FreeUnit = null;
        EditorUtility.SetDirty(silo);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated building UnitData assets (Refinery, Silo)");
    }

    public static void GenerateOreTruckPrefab()
    {
        string prefabDir = $"{PrefabDir}/Units";
        PrefabGeneratorUtils.EnsureFolder(prefabDir);

        var unitData = AssetDatabase.LoadAssetAtPath<UnitData>($"{UnitDir}/OreTruck.asset");
        if (unitData == null)
        {
            Debug.LogError("OreTruck UnitData not found. Run Generate Ore Unit first.");
            return;
        }

        var circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/UI/SelectionCircle.png");
        var barBGSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/UI/HealthBarBG.png");
        var barFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/UI/HealthBarFill.png");

        string prefabPath = $"{prefabDir}/OreTruck.prefab";

        PrefabGeneratorUtils.SavePrefab("OreTruck", prefabPath, root =>
        {
            var sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = unitData.Sprite;
            sr.sortingOrder = 10;

            var entity = root.AddComponent<Entity>();
            root.AddComponent<Mover>();
            root.AddComponent<Harvester>();
            root.AddComponent<SelfHeal>();
            var selectable = root.AddComponent<Selectable>();

            var entitySO = new SerializedObject(entity);
            entitySO.FindProperty("_unitData").objectReferenceValue = unitData;
            entitySO.ApplyModifiedPropertiesWithoutUndo();

            var selfHeal = root.GetComponent<SelfHeal>();
            var shSO = new SerializedObject(selfHeal);
            shSO.FindProperty("_healInterval").floatValue = 2f;
            shSO.FindProperty("_healAmount").intValue = 1;
            shSO.FindProperty("_maxHealRatio").floatValue = 0.5f;
            shSO.ApplyModifiedPropertiesWithoutUndo();

            ConfigureSelectable(root, selectable, circleSprite, barBGSprite, barFillSprite);
        });

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        unitData.Prefab = prefab;
        EditorUtility.SetDirty(unitData);
        AssetDatabase.SaveAssets();
        Debug.Log("Generated Ore Truck prefab");
    }

    public static void GenerateBuildingPrefabs()
    {
        string buildingPrefabDir = $"{PrefabDir}/Buildings";
        PrefabGeneratorUtils.EnsureFolder(buildingPrefabDir);

        var circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/UI/SelectionCircle.png");
        var barBGSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/UI/HealthBarBG.png");
        var barFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/UI/HealthBarFill.png");

        GenerateBuildingPrefab("OreRefinery", buildingPrefabDir, true, circleSprite, barBGSprite, barFillSprite);
        GenerateBuildingPrefab("OreSilo", buildingPrefabDir, false, circleSprite, barBGSprite, barFillSprite);
    }

    static void GenerateBuildingPrefab(string name, string dir, bool isRefinery,
        Sprite circleSprite, Sprite barBGSprite, Sprite barFillSprite)
    {
        var unitData = AssetDatabase.LoadAssetAtPath<UnitData>($"{UnitDir}/{name}.asset");
        if (unitData == null)
        {
            Debug.LogError($"{name} UnitData not found. Run Generate Building Data first.");
            return;
        }

        string path = $"{dir}/{name}.prefab";

        PrefabGeneratorUtils.SavePrefab(name, path, root =>
        {
            var sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = unitData.Sprite;
            sr.sortingOrder = 5;

            var entity = root.AddComponent<Entity>();

            if (isRefinery)
                root.AddComponent<Refinery>();
            else
                root.AddComponent<Silo>();

            var selectable = root.AddComponent<Selectable>();

            var entitySO = new SerializedObject(entity);
            entitySO.FindProperty("_unitData").objectReferenceValue = unitData;
            entitySO.ApplyModifiedPropertiesWithoutUndo();

            ConfigureSelectable(root, selectable, circleSprite, barBGSprite, barFillSprite);
        });

        unitData.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        EditorUtility.SetDirty(unitData);
        AssetDatabase.SaveAssets();
        Debug.Log($"Generated {name} prefab");
    }

    static void ConfigureSelectable(GameObject root, Selectable selectable,
        Sprite circleSprite, Sprite barBGSprite, Sprite barFillSprite)
    {
        var circleGO = new GameObject("SelectionCircle");
        circleGO.transform.SetParent(root.transform, false);
        circleGO.transform.localPosition = new Vector3(0f, -0.3f, 0f);
        var circleSR = circleGO.AddComponent<SpriteRenderer>();
        circleSR.sprite = circleSprite;
        circleSR.sortingOrder = 4;
        circleSR.enabled = false;

        var healthBarGO = new GameObject("HealthBar");
        healthBarGO.transform.SetParent(root.transform, false);
        healthBarGO.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        var healthBar = healthBarGO.AddComponent<HealthBar>();

        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(healthBarGO.transform, false);
        var bgSR = bgGO.AddComponent<SpriteRenderer>();
        bgSR.sprite = barBGSprite;
        bgSR.sortingOrder = 20;

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(healthBarGO.transform, false);
        fillGO.transform.localPosition = Vector3.zero;
        var fillSR = fillGO.AddComponent<SpriteRenderer>();
        fillSR.sprite = barFillSprite;
        fillSR.color = Color.green;
        fillSR.sortingOrder = 21;

        var selectSO = new SerializedObject(selectable);
        selectSO.FindProperty("_selectionCircle").objectReferenceValue = circleSR;
        selectSO.FindProperty("_healthBar").objectReferenceValue = healthBar;
        selectSO.ApplyModifiedPropertiesWithoutUndo();

        var hbSO = new SerializedObject(healthBar);
        hbSO.FindProperty("_barFill").objectReferenceValue = fillGO.transform;
        hbSO.FindProperty("_fillRenderer").objectReferenceValue = fillSR;
        hbSO.ApplyModifiedPropertiesWithoutUndo();

        healthBarGO.SetActive(false);
    }

    public static void GenerateAll()
    {
        GenerateOverlayTiles();
        GenerateBuildingSprites();
        GenerateOreUnit();
        GenerateBuildingData();
        GenerateOreTruckPrefab();
        GenerateBuildingPrefabs();
    }

    static void CreateTile(string name, string spritePath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            Debug.LogError($"Sprite not found at {spritePath}");
            return;
        }

        string tilePath = $"{TileDir}/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (existing != null)
        {
            existing.sprite = sprite;
            EditorUtility.SetDirty(existing);
        }
        else
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            AssetDatabase.CreateAsset(tile, tilePath);
        }
    }

    static void ImportSprite(string path)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null)
        {
            Debug.LogError($"Sprite not found at {path} — run Tools/export_sprites.sh first");
            return;
        }
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 64;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
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
