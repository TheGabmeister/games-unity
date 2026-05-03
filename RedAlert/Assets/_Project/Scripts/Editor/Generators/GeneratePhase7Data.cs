using UnityEditor;
using UnityEngine;

public static class GeneratePhase7Data
{
    const string UnitDir = "Assets/_Project/Data/Units";
    const string WeaponDir = "Assets/_Project/Data/Weapons";
    const string ProjectileDir = "Assets/_Project/Data/Projectiles";
    const string WarheadDir = "Assets/_Project/Data/Warheads";
    const string SpriteDir = "Assets/_Project/Sprites/Units";
    const string SoundDir = "Assets/_Project/Sounds/Combat";

    public static void GenerateNewWeapons()
    {
        PrefabGeneratorUtils.EnsureFolder(WeaponDir);

        var invisible = Load<ProjectileData>($"{ProjectileDir}/Invisible.asset");
        var cannon = Load<ProjectileData>($"{ProjectileDir}/Cannon.asset");
        var heatSeeker = Load<ProjectileData>($"{ProjectileDir}/HeatSeeker.asset");
        var aaMissile = Load<ProjectileData>($"{ProjectileDir}/AAMissile.asset");
        var lobbed = Load<ProjectileData>($"{ProjectileDir}/Lobbed.asset");
        var fireball = Load<ProjectileData>($"{ProjectileDir}/Fireball.asset");

        var sa = Load<WarheadData>($"{WarheadDir}/SA.asset");
        var he = Load<WarheadData>($"{WarheadDir}/HE.asset");
        var ap = Load<WarheadData>($"{WarheadDir}/AP.asset");
        var fire = Load<WarheadData>($"{WarheadDir}/Fire.asset");
        var hollowPoint = Load<WarheadData>($"{WarheadDir}/HollowPoint.asset");
        var super = Load<WarheadData>($"{WarheadDir}/Super.asset");
        var organic = Load<WarheadData>($"{WarheadDir}/Organic.asset");

        var rifleFire = Load<AudioClip>($"{SoundDir}/RifleFire.wav");
        var cannonFire = Load<AudioClip>($"{SoundDir}/CannonFire.wav");
        var rocketFire = Load<AudioClip>($"{SoundDir}/RocketFire.wav");

        // New projectile types
        PrefabGeneratorUtils.EnsureFolder(ProjectileDir);
        var torpedoProj = CreateProjectile("Torpedo", "Torpedo", ProjectileType.Homing, 6f, 0f, false, true);

        // New weapons
        Weapon("Grenade", "Grenade", 50, 4f, 2.5f, 1, lobbed, he, cannonFire);
        Weapon("Flamethrower", "Flamethrower", 35, 3f, 1.5f, 3, fireball, fire, null);
        Weapon("DualColt", "Dual Colt .45", 25, 5f, 0.5f, 2, invisible, hollowPoint, rifleFire);
        Weapon("DogJaw", "Dog Jaw", 100, 1.5f, 0.8f, 1, invisible, organic, null);
        Weapon("TeslaZap", "Tesla Zap", 75, 5f, 2.5f, 1, invisible, super, null);
        Weapon("M60mg", "M60 Machine Gun", 15, 4f, 0.6f, 5, invisible, sa, rifleFire);
        Weapon("120mm", "120mm Cannon", 50, 6f, 2.2f, 1, cannon, ap, cannonFire);
        Weapon("MammothTusk", "Mammoth Tusk", 35, 7f, 2.5f, 2, heatSeeker, he, rocketFire);
        Weapon("V2Rocket", "V2 Rocket", 200, 10f, 4f, 1, lobbed, he, rocketFire);
        Weapon("DepthCharge", "Depth Charge", 40, 5f, 2f, 1, lobbed, ap, cannonFire);
        Weapon("StingerMissile", "Stinger Missile", 30, 6f, 2f, 1, heatSeeker, he, rocketFire);
        Weapon("NavalGun8in", "8-inch Naval Gun", 100, 12f, 3f, 2, cannon, ap, cannonFire);
        Weapon("Torpedo", "Torpedo", 50, 7f, 3f, 2, torpedoProj, ap, rocketFire);
        Weapon("GunboatGun", "Gunboat Cannon", 20, 5f, 1.5f, 1, cannon, sa, cannonFire);
        Weapon("SubMissile", "Sub-launched Missile", 60, 8f, 3.5f, 2, heatSeeker, he, rocketFire);
        Weapon("HellfireMissile", "Hellfire Missile", 50, 6f, 2f, 1, heatSeeker, ap, rocketFire);
        Weapon("Chaingun", "Chaingun", 15, 4f, 0.5f, 6, invisible, sa, rifleFire);
        Weapon("MaverickMissile", "Maverick Missile", 60, 5f, 2.5f, 1, heatSeeker, ap, rocketFire);
        Weapon("YakMG", "Yak Machine Gun", 15, 4f, 0.4f, 4, invisible, sa, rifleFire);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated Phase 7 weapons and projectiles");
    }

    public static void GenerateNewUnits()
    {
        PrefabGeneratorUtils.EnsureFolder(UnitDir);

        var deathSound = Load<AudioClip>($"{SoundDir}/UnitDeath.wav");
        var heWarhead = Load<WarheadData>($"{WarheadDir}/HE.asset");
        var nukeWarhead = Load<WarheadData>($"{WarheadDir}/Nuke.asset");

        var rifleData = Load<UnitData>($"{UnitDir}/RifleInfantry.asset");

        // Load all weapons
        var grenade = Load<WeaponData>($"{WeaponDir}/Grenade.asset");
        var flamethrower = Load<WeaponData>($"{WeaponDir}/Flamethrower.asset");
        var dualColt = Load<WeaponData>($"{WeaponDir}/DualColt.asset");
        var dogJaw = Load<WeaponData>($"{WeaponDir}/DogJaw.asset");
        var teslaZap = Load<WeaponData>($"{WeaponDir}/TeslaZap.asset");
        var m60 = Load<WeaponData>($"{WeaponDir}/M60mg.asset");
        var w120 = Load<WeaponData>($"{WeaponDir}/120mm.asset");
        var mammothTusk = Load<WeaponData>($"{WeaponDir}/MammothTusk.asset");
        var v2rocket = Load<WeaponData>($"{WeaponDir}/V2Rocket.asset");
        var depthCharge = Load<WeaponData>($"{WeaponDir}/DepthCharge.asset");
        var navalGun = Load<WeaponData>($"{WeaponDir}/NavalGun8in.asset");
        var torpedo = Load<WeaponData>($"{WeaponDir}/Torpedo.asset");
        var gunboatGun = Load<WeaponData>($"{WeaponDir}/GunboatGun.asset");
        var subMissile = Load<WeaponData>($"{WeaponDir}/SubMissile.asset");
        var hellfireM = Load<WeaponData>($"{WeaponDir}/HellfireMissile.asset");
        var chaingun = Load<WeaponData>($"{WeaponDir}/Chaingun.asset");
        var maverick = Load<WeaponData>($"{WeaponDir}/MaverickMissile.asset");
        var yakMG = Load<WeaponData>($"{WeaponDir}/YakMG.asset");

        // ---- NEW INFANTRY ----
        Unit("Engineer", "Engineer", UnitCategory.Infantry, Faction.Allied,
            LocomotionType.Foot, 3f, 25, ArmorType.None, null, 2f, 500,
            false, false, false, false, null, deathSound, null);

        Unit("Grenadier", "Grenadier", UnitCategory.Infantry, Faction.Soviet,
            LocomotionType.Foot, 3f, 50, ArmorType.None, grenade, 4f, 160,
            false, false, false, false, null, deathSound, null);

        Unit("Flamethrower", "Flamethrower", UnitCategory.Infantry, Faction.Soviet,
            LocomotionType.Foot, 3f, 40, ArmorType.None, flamethrower, 3f, 300,
            false, false, false, false, null, deathSound, null);

        Unit("Tanya", "Tanya", UnitCategory.Infantry, Faction.Allied,
            LocomotionType.Foot, 5f, 100, ArmorType.None, dualColt, 5f, 1200,
            false, false, false, false, null, deathSound, null);

        Unit("AttackDog", "Attack Dog", UnitCategory.Infantry, Faction.Soviet,
            LocomotionType.Foot, 6f, 20, ArmorType.None, dogJaw, 5f, 200,
            false, false, false, false, null, deathSound, null);

        Unit("Medic", "Field Medic", UnitCategory.Infantry, Faction.Allied,
            LocomotionType.Foot, 3f, 80, ArmorType.None, null, 3f, 800,
            false, false, false, false, null, deathSound, null);

        Unit("ShockTrooper", "Shock Trooper", UnitCategory.Infantry, Faction.Soviet,
            LocomotionType.Foot, 3f, 80, ArmorType.None, teslaZap, 5f, 1200,
            false, false, false, false, null, deathSound, null);

        Unit("Spy", "Spy", UnitCategory.Infantry, Faction.Allied,
            LocomotionType.Foot, 5f, 25, ArmorType.None, null, 4f, 500,
            false, false, false, false, null, deathSound, null);

        // ---- NEW VEHICLES ----
        Unit("MCV", "MCV", UnitCategory.Vehicle, Faction.Allied,
            LocomotionType.Tracked, 4f, 600, ArmorType.Heavy, null, 4f, 2500,
            false, false, true, false, null, deathSound, rifleData);

        Unit("MammothTank", "Mammoth Tank", UnitCategory.Vehicle, Faction.Soviet,
            LocomotionType.Tracked, 3.5f, 600, ArmorType.Heavy, w120, 6f, 1700,
            true, false, true, false, null, deathSound, rifleData);

        Unit("APC", "APC", UnitCategory.Vehicle, Faction.Allied,
            LocomotionType.Tracked, 6f, 200, ArmorType.Heavy, m60, 4f, 800,
            true, false, true, false, null, deathSound, rifleData);

        Unit("V2Launcher", "V2 Launcher", UnitCategory.Vehicle, Faction.Soviet,
            LocomotionType.Tracked, 4f, 150, ArmorType.Light, v2rocket, 4f, 700,
            true, true, false, false, null, deathSound, null);

        Unit("MineLayer", "Mine Layer", UnitCategory.Vehicle, Faction.Allied,
            LocomotionType.Tracked, 5f, 100, ArmorType.Light, null, 4f, 800,
            false, false, true, false, null, deathSound, rifleData);

        Unit("RadarJammer", "Radar Jammer", UnitCategory.Vehicle, Faction.Allied,
            LocomotionType.Wheeled, 5f, 110, ArmorType.Light, null, 6f, 600,
            false, false, true, false, null, deathSound, rifleData);

        Unit("TeslaTank", "Tesla Tank", UnitCategory.Vehicle, Faction.Soviet,
            LocomotionType.Tracked, 4.5f, 300, ArmorType.Heavy, teslaZap, 5f, 1500,
            true, false, true, false, null, deathSound, rifleData);

        Unit("ChronoTank", "Chrono Tank", UnitCategory.Vehicle, Faction.Allied,
            LocomotionType.Tracked, 5f, 200, ArmorType.Heavy, Load<WeaponData>($"{WeaponDir}/75mm.asset"), 6f, 2400,
            false, false, true, false, null, deathSound, rifleData);

        Unit("DemoTruck", "Demo Truck", UnitCategory.Vehicle, Faction.Soviet,
            LocomotionType.Wheeled, 5f, 200, ArmorType.Light, null, 4f, 2400,
            false, false, false, true, nukeWarhead, deathSound, null);

        Unit("PhaseTransport", "Phase Transport", UnitCategory.Vehicle, Faction.Allied,
            LocomotionType.Tracked, 5f, 150, ArmorType.Light, Load<WeaponData>($"{WeaponDir}/Dragon.asset"), 5f, 2400,
            false, false, true, false, null, deathSound, rifleData);

        // ---- NAVAL ----
        Unit("Destroyer", "Destroyer", UnitCategory.Naval, Faction.Allied,
            LocomotionType.Float, 7f, 400, ArmorType.Heavy, depthCharge, 6f, 1000,
            false, false, false, false, null, deathSound, null);

        Unit("Cruiser", "Cruiser", UnitCategory.Naval, Faction.Allied,
            LocomotionType.Float, 4f, 700, ArmorType.Heavy, navalGun, 7f, 2000,
            false, false, false, false, null, deathSound, null);

        Unit("Submarine", "Submarine", UnitCategory.Naval, Faction.Soviet,
            LocomotionType.Float, 4f, 120, ArmorType.Light, torpedo, 4f, 950,
            false, false, false, false, null, deathSound, null);

        Unit("Gunboat", "Gunboat", UnitCategory.Naval, Faction.Allied,
            LocomotionType.Float, 7f, 200, ArmorType.Light, gunboatGun, 5f, 500,
            false, false, false, false, null, deathSound, null);

        Unit("TransportShip", "Naval Transport", UnitCategory.Naval, Faction.Allied,
            LocomotionType.Float, 6f, 300, ArmorType.Heavy, null, 5f, 700,
            false, false, false, false, null, deathSound, null);

        Unit("MissileSub", "Missile Sub", UnitCategory.Naval, Faction.Soviet,
            LocomotionType.Float, 4f, 150, ArmorType.Light, subMissile, 5f, 1800,
            false, false, false, false, null, deathSound, null);

        // ---- AIRCRAFT ----
        Unit("Longbow", "Longbow", UnitCategory.Aircraft, Faction.Allied,
            LocomotionType.Fly, 6f, 125, ArmorType.Light, hellfireM, 4f, 1200,
            false, false, false, false, null, deathSound, null);

        Unit("Hind", "Hind", UnitCategory.Aircraft, Faction.Soviet,
            LocomotionType.Fly, 5f, 125, ArmorType.Light, chaingun, 4f, 1200,
            false, false, false, false, null, deathSound, null);

        Unit("MiG", "MiG", UnitCategory.Aircraft, Faction.Soviet,
            LocomotionType.Fly, 9f, 40, ArmorType.Light, maverick, 3f, 1200,
            false, false, false, false, null, deathSound, null);

        Unit("Yak", "Yak", UnitCategory.Aircraft, Faction.Soviet,
            LocomotionType.Fly, 9f, 50, ArmorType.Light, yakMG, 3f, 800,
            false, false, false, false, null, deathSound, null);

        Unit("Chinook", "Chinook", UnitCategory.Aircraft, Faction.Allied,
            LocomotionType.Fly, 5f, 90, ArmorType.Light, null, 3f, 1500,
            false, false, false, false, null, deathSound, null);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated Phase 7 unit data (29 new units)");
    }

    public static void GenerateAll()
    {
        GenerateNewWeapons();
        GenerateNewUnits();
    }

    // --- Helpers ---

    static void Unit(string fileName, string displayName,
        UnitCategory category, Faction faction,
        LocomotionType loco, float speed, int maxHP, ArmorType armor,
        WeaponData weapon, float sight, int cost,
        bool isCrusher, bool noMovingFire, bool isCrewed, bool explodesOnDeath,
        WarheadData deathWarhead, AudioClip deathSound, UnitData bailOut)
    {
        string path = $"{UnitDir}/{fileName}.asset";
        var data = LoadOrCreate<UnitData>(path);
        data.DisplayName = displayName;
        data.Category = category;
        data.Faction = faction;
        data.Locomotion = loco;
        data.BaseSpeed = speed;
        data.MaxHP = maxHP;
        data.Armor = armor;
        data.PrimaryWeapon = weapon;
        data.SightRange = sight;
        data.Cost = cost;
        data.IsCrusher = isCrusher;
        data.NoMovingFire = noMovingFire;
        data.IsCrewedVehicle = isCrewed;
        data.ExplodesOnDeath = explodesOnDeath;
        data.DeathWarhead = deathWarhead;
        data.DeathSound = deathSound;
        data.BailOutUnit = bailOut;

        var sprite = Load<Sprite>($"{SpriteDir}/{fileName}.png");
        if (sprite != null) data.Sprite = sprite;

        EditorUtility.SetDirty(data);
    }

    static void Weapon(string fileName, string displayName,
        int damage, float range, float rof, int burst,
        ProjectileData proj, WarheadData warhead, AudioClip sound)
    {
        string path = $"{WeaponDir}/{fileName}.asset";
        var data = LoadOrCreate<WeaponData>(path);
        data.DisplayName = displayName;
        data.Damage = damage;
        data.Range = range;
        data.ROF = rof;
        data.Burst = burst;
        data.Projectile = proj;
        data.Warhead = warhead;
        data.FireSound = sound;
        EditorUtility.SetDirty(data);
    }

    static ProjectileData CreateProjectile(string fileName, string displayName,
        ProjectileType type, float speed, float scatter, bool aa, bool ag)
    {
        string path = $"{ProjectileDir}/{fileName}.asset";
        var data = LoadOrCreate<ProjectileData>(path);
        data.DisplayName = displayName;
        data.Type = type;
        data.Speed = speed;
        data.Scatter = scatter;
        data.AntiAir = aa;
        data.AntiGround = ag;
        EditorUtility.SetDirty(data);
        return data;
    }

    static T Load<T>(string path) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
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
