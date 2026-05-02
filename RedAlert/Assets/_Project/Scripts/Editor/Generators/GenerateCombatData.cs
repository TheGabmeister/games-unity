using UnityEditor;
using UnityEngine;

public static class GenerateCombatData
{
    const string WarheadDir = "Assets/_Project/Data/Warheads";
    const string ProjectileDir = "Assets/_Project/Data/Projectiles";
    const string WeaponDir = "Assets/_Project/Data/Weapons";
    const string SoundDir = "Assets/_Project/Sounds/Combat";

    public static void GenerateProjectileSprites()
    {
        string dir = "Assets/_Project/Sprites/Projectiles";
        PrefabGeneratorUtils.EnsureFolder(dir);

        CreateProjectilePNG($"{dir}/Bullet.png", 8, 8, new Color(1f, 1f, 0.6f));
        CreateProjectilePNG($"{dir}/Shell.png", 12, 8, new Color(0.7f, 0.7f, 0.7f));
        CreateProjectilePNG($"{dir}/Rocket.png", 16, 6, new Color(0.9f, 0.3f, 0.2f));
        CreateProjectilePNG($"{dir}/Fireball.png", 12, 12, new Color(1f, 0.5f, 0f));

        ImportAsSprite($"{dir}/Bullet.png");
        ImportAsSprite($"{dir}/Shell.png");
        ImportAsSprite($"{dir}/Rocket.png");
        ImportAsSprite($"{dir}/Fireball.png");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated projectile sprites");
    }

    public static void GenerateWarheads()
    {
        PrefabGeneratorUtils.EnsureFolder(WarheadDir);

        var explosionSmall = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SoundDir}/ExplosionSmall.wav");
        var explosionLarge = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SoundDir}/ExplosionLarge.wav");

        CreateWarhead("SA", "Small Arms", 1f, 0.6f, 0.6f, 0.4f, 0.3f, 3, false, null);
        CreateWarhead("HE", "High Explosive", 1f, 0.8f, 0.6f, 0.3f, 0.8f, 6, true, explosionLarge);
        CreateWarhead("AP", "Armor Piercing", 0.6f, 0.8f, 0.8f, 1f, 0.6f, 4, true, explosionSmall);
        CreateWarhead("Fire", "Incendiary", 1f, 1f, 0.8f, 0.1f, 0.1f, 8, false, explosionLarge);
        CreateWarhead("HollowPoint", "Hollow Point", 2f, 0.1f, 0.1f, 0.1f, 0.1f, 1, false, null);
        CreateWarhead("Super", "Super", 1.5f, 1f, 0.8f, 0.6f, 1f, 6, true, explosionLarge);
        CreateWarhead("Organic", "Organic", 1f, 0f, 0f, 0f, 0f, 0, false, null);
        CreateWarhead("Nuke", "Nuclear", 1f, 1f, 1f, 1f, 1f, 6, true, explosionLarge);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated 8 WarheadData assets");
    }

    public static void GenerateProjectiles()
    {
        PrefabGeneratorUtils.EnsureFolder(ProjectileDir);
        string spriteDir = "Assets/_Project/Sprites/Projectiles";

        var bulletSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{spriteDir}/Bullet.png");
        var shellSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{spriteDir}/Shell.png");
        var rocketSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{spriteDir}/Rocket.png");
        var fireballSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{spriteDir}/Fireball.png");

        CreateProjectile("Invisible", "Invisible (Hitscan)", ProjectileType.Hitscan, 0f, 0f, true, true, null);
        CreateProjectile("Cannon", "Cannon Shell", ProjectileType.Direct, 12f, 0f, false, true, shellSprite);
        CreateProjectile("HeatSeeker", "Heat Seeker", ProjectileType.Homing, 8f, 2f, false, true, rocketSprite);
        CreateProjectile("AAMissile", "AA Missile", ProjectileType.Homing, 10f, 0f, true, false, rocketSprite);
        CreateProjectile("Lobbed", "Lobbed Shell", ProjectileType.Ballistic, 8f, 1f, false, true, shellSprite);
        CreateProjectile("Fireball", "Fireball", ProjectileType.Direct, 10f, 0f, false, true, fireballSprite);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated 6 ProjectileData assets");
    }

    public static void GenerateWeapons()
    {
        PrefabGeneratorUtils.EnsureFolder(WeaponDir);

        var rifleFire = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SoundDir}/RifleFire.wav");
        var cannonFire = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SoundDir}/CannonFire.wav");
        var rocketFire = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SoundDir}/RocketFire.wav");

        var invisible = LoadProjectile("Invisible");
        var cannon = LoadProjectile("Cannon");
        var heatSeeker = LoadProjectile("HeatSeeker");
        var lobbed = LoadProjectile("Lobbed");

        var sa = LoadWarhead("SA");
        var ap = LoadWarhead("AP");
        var he = LoadWarhead("HE");

        CreateWeapon("Rifle", "M1 Rifle", 15, 4f, 0.8f, 1, invisible, sa, rifleFire);
        CreateWeapon("Dragon", "Dragon Missile", 30, 5f, 2f, 1, heatSeeker, ap, rocketFire);
        CreateWeapon("75mm", "75mm Cannon", 25, 4.75f, 1.5f, 1, cannon, ap, cannonFire);
        CreateWeapon("105mm", "105mm Cannon", 40, 5.75f, 2f, 1, cannon, ap, cannonFire);
        CreateWeapon("155mm", "155mm Howitzer", 150, 8f, 3f, 1, lobbed, he, cannonFire);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated 5 WeaponData assets");
    }

    public static void GenerateUnitData()
    {
        string unitDir = "Assets/_Project/Data/Units";
        string spriteDir = "Assets/_Project/Sprites/Units";
        PrefabGeneratorUtils.EnsureFolder(unitDir);

        ImportUnitSprite($"{spriteDir}/RifleInfantry.png");
        ImportUnitSprite($"{spriteDir}/LightTank.png");
        ImportUnitSprite($"{spriteDir}/Ranger.png");
        ImportUnitSprite($"{spriteDir}/HeavyTank.png");
        ImportUnitSprite($"{spriteDir}/RocketSoldier.png");
        ImportUnitSprite($"{spriteDir}/Artillery.png");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var deathSound = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SoundDir}/UnitDeath.wav");
        var rifle = LoadWeapon("Rifle");
        var dragon = LoadWeapon("Dragon");
        var w75 = LoadWeapon("75mm");
        var w105 = LoadWeapon("105mm");
        var w155 = LoadWeapon("155mm");
        var heWarhead = LoadWarhead("HE");

        var rifleData = LoadOrCreate<UnitData>($"{unitDir}/RifleInfantry.asset");
        SetUnit(rifleData, "Rifle Infantry", LoadSprite(spriteDir, "RifleInfantry"),
            UnitCategory.Infantry, Faction.Allied, LocomotionType.Foot, 3f,
            50, ArmorType.None, rifle, 5f,
            false, false, false, false, null, deathSound, null);

        var rocketData = LoadOrCreate<UnitData>($"{unitDir}/RocketSoldier.asset");
        SetUnit(rocketData, "Rocket Soldier", LoadSprite(spriteDir, "RocketSoldier"),
            UnitCategory.Infantry, Faction.Allied, LocomotionType.Foot, 3f,
            45, ArmorType.None, dragon, 5f,
            false, false, false, false, null, deathSound, null);

        var ltankData = LoadOrCreate<UnitData>($"{unitDir}/LightTank.asset");
        SetUnit(ltankData, "Light Tank", LoadSprite(spriteDir, "LightTank"),
            UnitCategory.Vehicle, Faction.Allied, LocomotionType.Tracked, 5f,
            300, ArmorType.Heavy, w75, 5f,
            true, false, true, false, null, deathSound, rifleData);

        var rangerData = LoadOrCreate<UnitData>($"{unitDir}/Ranger.asset");
        SetUnit(rangerData, "Ranger", LoadSprite(spriteDir, "Ranger"),
            UnitCategory.Vehicle, Faction.Allied, LocomotionType.Wheeled, 7f,
            150, ArmorType.Light, rifle, 5f,
            false, false, true, false, null, deathSound, rifleData);

        var htankData = LoadOrCreate<UnitData>($"{unitDir}/HeavyTank.asset");
        SetUnit(htankData, "Heavy Tank", LoadSprite(spriteDir, "HeavyTank"),
            UnitCategory.Vehicle, Faction.Soviet, LocomotionType.Tracked, 4f,
            400, ArmorType.Heavy, w105, 6f,
            true, false, true, false, null, deathSound, rifleData);

        var artyData = LoadOrCreate<UnitData>($"{unitDir}/Artillery.asset");
        SetUnit(artyData, "Artillery", LoadSprite(spriteDir, "Artillery"),
            UnitCategory.Vehicle, Faction.Soviet, LocomotionType.Tracked, 3.5f,
            75, ArmorType.Light, w155, 7f,
            false, true, false, false, null, deathSound, null);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated 6 UnitData assets with combat stats");
    }

    public static void GenerateAll()
    {
        GenerateProjectileSprites();
        GenerateWarheads();
        GenerateProjectiles();
        GenerateWeapons();
        GenerateUnitData();
    }

    static void SetUnit(UnitData data, string displayName, Sprite sprite,
        UnitCategory category, Faction faction, LocomotionType loco, float speed,
        int maxHP, ArmorType armor, WeaponData weapon, float sight,
        bool isCrusher, bool noMovingFire, bool isCrewed, bool explodesOnDeath,
        WarheadData deathWarhead, AudioClip deathSound, UnitData bailOut)
    {
        data.DisplayName = displayName;
        data.Sprite = sprite;
        data.Category = category;
        data.Faction = faction;
        data.Locomotion = loco;
        data.BaseSpeed = speed;
        data.MaxHP = maxHP;
        data.Armor = armor;
        data.PrimaryWeapon = weapon;
        data.SightRange = sight;
        data.IsCrusher = isCrusher;
        data.NoMovingFire = noMovingFire;
        data.IsCrewedVehicle = isCrewed;
        data.ExplodesOnDeath = explodesOnDeath;
        data.DeathWarhead = deathWarhead;
        data.DeathSound = deathSound;
        data.BailOutUnit = bailOut;
        EditorUtility.SetDirty(data);
    }

    static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;

        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    static Sprite LoadSprite(string dir, string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>($"{dir}/{name}.png");
    }

    static WeaponData LoadWeapon(string name)
    {
        return AssetDatabase.LoadAssetAtPath<WeaponData>($"{WeaponDir}/{name}.asset");
    }

    static WarheadData LoadWarhead(string name)
    {
        return AssetDatabase.LoadAssetAtPath<WarheadData>($"{WarheadDir}/{name}.asset");
    }

    static ProjectileData LoadProjectile(string name)
    {
        return AssetDatabase.LoadAssetAtPath<ProjectileData>($"{ProjectileDir}/{name}.asset");
    }

    static void CreateWarhead(string fileName, string displayName,
        float none, float wood, float light, float heavy, float concrete,
        int spread, bool wallDestroyer, AudioClip impact)
    {
        string path = $"{WarheadDir}/{fileName}.asset";
        var data = LoadOrCreate<WarheadData>(path);
        data.DisplayName = displayName;
        data.ModNone = none;
        data.ModWood = wood;
        data.ModLight = light;
        data.ModHeavy = heavy;
        data.ModConcrete = concrete;
        data.SpreadFactor = spread;
        data.WallDestroyer = wallDestroyer;
        data.ImpactSound = impact;
        EditorUtility.SetDirty(data);
    }

    static void CreateProjectile(string fileName, string displayName,
        ProjectileType type, float speed, float scatter, bool aa, bool ag, Sprite sprite)
    {
        string path = $"{ProjectileDir}/{fileName}.asset";
        var data = LoadOrCreate<ProjectileData>(path);
        data.DisplayName = displayName;
        data.Type = type;
        data.Speed = speed;
        data.Scatter = scatter;
        data.AntiAir = aa;
        data.AntiGround = ag;
        data.Sprite = sprite;
        EditorUtility.SetDirty(data);
    }

    static void CreateWeapon(string fileName, string displayName,
        int damage, float range, float rof, int burst,
        ProjectileData projectile, WarheadData warhead, AudioClip fireSound)
    {
        string path = $"{WeaponDir}/{fileName}.asset";
        var data = LoadOrCreate<WeaponData>(path);
        data.DisplayName = displayName;
        data.Damage = damage;
        data.Range = range;
        data.ROF = rof;
        data.Burst = burst;
        data.Projectile = projectile;
        data.Warhead = warhead;
        data.FireSound = fireSound;
        EditorUtility.SetDirty(data);
    }

    static void CreateProjectilePNG(string path, int width, int height, Color color)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var pixels = new Color[width * height];
        float cx = width / 2f;
        float cy = height / 2f;
        float rx = width / 2f;
        float ry = height / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = (x + 0.5f - cx) / rx;
                float dy = (y + 0.5f - cy) / ry;
                if (dx * dx + dy * dy <= 1f)
                    pixels[y * width + x] = color;
                else
                    pixels[y * width + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        string fullPath = System.IO.Path.Combine(Application.dataPath, "..", path).Replace("\\", "/");
        string dir = System.IO.Path.GetDirectoryName(fullPath);
        if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
        System.IO.File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);
    }

    static void ImportAsSprite(string path)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 64;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    static void ImportUnitSprite(string path)
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
}
