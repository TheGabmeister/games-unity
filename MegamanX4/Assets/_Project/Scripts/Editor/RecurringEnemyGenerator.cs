using UnityEditor;
using UnityEngine;
using static EnemyGeneratorCore;

public static class RecurringEnemyGenerator
{
    const string BasePath = "Assets/_Project/Enemies/Recurring";

    [MenuItem("Tools/MegamanX4/Generate Recurring Enemies")]
    public static void Generate()
    {
        GenerateKnotBeretG();
        GenerateKnotBeretB();
        GenerateSpikeMarl();
        GenerateKyunnbyunn();
        GenerateBlastRaster();
        GenerateHoverGunner();
        GenerateGigaDeath();
        GeneratePlasmaCannon();
        GenerateBattonBoneB81();
        GenerateMettaurD2();
        GenerateSpikyMkII();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Recurring enemies generated.");
    }

    static void GenerateKnotBeretG()
    {
        var go = NewEnemyRoot(BasePath, "KnotBeretG", hp: 1, contactDamage: 2, isTrigger: true);
        var muzzle = AddMuzzle(go, new Vector3(0.3f, 0.1f, 0));

        AddGravity(go);

        var detector = go.AddComponent<PlayerDetector>();
        SetField(detector, "_range", 10f);

        var shoot = go.AddComponent<EnemyShoot>();
        SetField(shoot, "_projectilePrefab", LoadPrefab(ProjectilePrefabPath));
        SetField(shoot, "_muzzle", muzzle.transform);
        SetField(shoot, "_burstCount", 1);
        SetField(shoot, "_burstInterval", 0.15f);
        SetField(shoot, "_cooldown", 2.5f);

        SavePrefab(go, BasePath, "KnotBeretG");
    }

    static void GenerateKnotBeretB()
    {
        var go = NewEnemyRoot(BasePath, "KnotBeretB", hp: 1, contactDamage: 2, isTrigger: true);
        var muzzle = AddMuzzle(go, new Vector3(0.3f, 0.1f, 0));

        AddGravity(go);

        var patrol = go.AddComponent<PatrolWalk>();
        SetField(patrol, "_speed", 2f);

        var detector = go.AddComponent<PlayerDetector>();
        SetField(detector, "_range", 8f);

        var shoot = go.AddComponent<EnemyShoot>();
        SetField(shoot, "_projectilePrefab", LoadPrefab(ProjectilePrefabPath));
        SetField(shoot, "_muzzle", muzzle.transform);
        SetField(shoot, "_burstCount", 3);
        SetField(shoot, "_burstInterval", 0.12f);
        SetField(shoot, "_cooldown", 2f);

        SavePrefab(go, BasePath, "KnotBeretB");
    }

    static void GenerateSpikeMarl()
    {
        var go = NewEnemyRoot(BasePath, "SpikeMarl", hp: 1, contactDamage: 4, isTrigger: false);
        var hurtBox = go.GetComponent<HurtBox>();

        var drop = go.AddComponent<DropTrigger>();
        SetField(drop, "_detectionWidth", 2f);
        SetField(drop, "_detectionRange", 20f);
        SetField(drop, "_fallGravityScale", 3f);
        SetField(drop, "_hurtBoxToEnableOnDrop", hurtBox);

        SavePrefab(go, BasePath, "SpikeMarl");
    }

    static void GenerateKyunnbyunn()
    {
        var go = NewEnemyRoot(BasePath, "Kyunnbyunn", hp: 1, contactDamage: 2, isTrigger: true);

        var move = go.AddComponent<MoveForward>();
        SetField(move, "_speed", 6f);

        var hover = go.AddComponent<HoverSine>();
        SetField(hover, "_amplitude", 1.5f);
        SetField(hover, "_frequency", 3f);

        var life = go.AddComponent<Lifetime>();
        SetField(life, "_duration", 10f);

        SavePrefab(go, BasePath, "Kyunnbyunn");
    }

    static void GenerateBlastRaster()
    {
        var go = NewEnemyRoot(BasePath, "BlastRaster", hp: 10, contactDamage: 10, isTrigger: true);

        var hover = go.AddComponent<HoverSine>();
        SetField(hover, "_amplitude", 0.3f);
        SetField(hover, "_frequency", 0.8f);

        var raster = go.AddComponent<BlastRaster>();
        SetField(raster, "_projectilePrefab", LoadPrefab(ProjectilePrefabPath));
        SetField(raster, "_shotCount", 8);
        SetField(raster, "_fireInterval", 2.5f);
        SetField(raster, "_initialDelay", 1f);
        SetField(raster, "_rotationOffset", 22.5f);

        SavePrefab(go, BasePath, "BlastRaster");
    }

    static void GenerateHoverGunner()
    {
        var go = NewEnemyRoot(BasePath, "HoverGunner", hp: 25, contactDamage: 15, isTrigger: true);
        var muzzle = AddMuzzle(go, new Vector3(0.4f, -0.2f, 0), Quaternion.Euler(0f, 0f, -45f));

        var hover = go.AddComponent<HoverSine>();
        SetField(hover, "_amplitude", 0.4f);
        SetField(hover, "_frequency", 1f);

        var track = go.AddComponent<TrackPlayer>();
        SetField(track, "_maxSpeed", 3f);
        SetField(track, "_detectionRadius", 14f);

        var detector = go.AddComponent<PlayerDetector>();
        SetField(detector, "_range", 12f);
        SetField(detector, "_requireLineOfSight", false);

        var shoot = go.AddComponent<EnemyShoot>();
        SetField(shoot, "_projectilePrefab", LoadPrefab(ProjectilePrefabPath));
        SetField(shoot, "_muzzle", muzzle.transform);
        SetField(shoot, "_burstCount", 1);
        SetField(shoot, "_burstInterval", 0.15f);
        SetField(shoot, "_cooldown", 1.5f);

        SavePrefab(go, BasePath, "HoverGunner");
    }

    static void GenerateGigaDeath()
    {
        var go = NewEnemyRoot(BasePath, "GigaDeath", hp: 50, contactDamage: 25, isTrigger: true);
        var muzzle = AddMuzzle(go, new Vector3(0f, -1f, 0), Quaternion.Euler(0f, 0f, -90f));

        var drift = go.AddComponent<MoveVertical>();
        SetField(drift, "_speed", 0.3f);
        SetField(drift, "_direction", MoveVertical.Direction.Down);

        var track = go.AddComponent<TrackPlayer>();
        SetField(track, "_maxSpeed", 1.2f);
        SetField(track, "_detectionRadius", 20f);

        var shoot = go.AddComponent<AutoShoot>();
        SetField(shoot, "_projectilePrefab", LoadPrefab(ProjectilePrefabPath));
        SetField(shoot, "_muzzle", muzzle.transform);
        SetField(shoot, "_interval", 3f);

        SavePrefab(go, BasePath, "GigaDeath");
    }

    static void GeneratePlasmaCannon()
    {
        var go = NewEnemyRoot(BasePath, "PlasmaCannon", hp: 1, contactDamage: 0, isTrigger: true);

        // Invulnerable turret: beam is the only damage source. Strip body Health/HurtBox/HitBox/DamageFlash/DestroyOnDepleted.
        Object.DestroyImmediate(go.GetComponent<DestroyOnDepleted>());
        Object.DestroyImmediate(go.GetComponent<DamageFlash>());
        Object.DestroyImmediate(go.GetComponent<HitBox>());
        Object.DestroyImmediate(go.GetComponent<HurtBox>());
        Object.DestroyImmediate(go.GetComponent<Health>());

        var beam = BuildPlasmaBeam(go);
        var telegraph = BuildPlasmaTelegraph(go);

        var cannon = go.AddComponent<PlasmaCannon>();
        SetField(cannon, "_beam", beam);
        SetField(cannon, "_chargeTelegraph", telegraph.GetComponent<SpriteRenderer>());
        SetField(cannon, "_idleDuration", 0.4f);
        SetField(cannon, "_chargeDuration", 1f);
        SetField(cannon, "_fireDuration", 1f);
        SetField(cannon, "_cooldownDuration", 1f);

        SavePrefab(go, BasePath, "PlasmaCannon");
    }

    static GameObject BuildPlasmaBeam(GameObject parent)
    {
        var beam = new GameObject("Beam");
        beam.transform.SetParent(parent.transform, false);
        beam.transform.localPosition = new Vector3(-6.6f, 0f, 0f);
        beam.layer = parent.layer;

        var sr = beam.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite($"{BasePath}/PlasmaCannon/PlasmaCannon_Beam.svg");
        sr.sharedMaterial = LoadMaterial(VectorMaterialPath);
        sr.sortingOrder = 1;

        var box = beam.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        if (sr.sprite) box.size = sr.sprite.bounds.size;

        var hit = beam.AddComponent<HitBox>();
        SetField(hit, "_damage", 20);

        beam.SetActive(false);
        return beam;
    }

    static GameObject BuildPlasmaTelegraph(GameObject parent)
    {
        var glow = new GameObject("ChargeTelegraph");
        glow.transform.SetParent(parent.transform, false);
        glow.transform.localPosition = new Vector3(-0.3f, 0f, 0f);

        var sr = glow.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite($"{BasePath}/PlasmaCannon/PlasmaCannon_Charge.svg");
        sr.sharedMaterial = LoadMaterial(VectorMaterialPath);
        sr.sortingOrder = 2;
        sr.enabled = false;
        return glow;
    }

    static void GenerateBattonBoneB81()
    {
        var go = NewEnemyRoot(BasePath, "BattonBoneB81", hp: 10, contactDamage: 15, isTrigger: true);

        var detector = go.AddComponent<PlayerDetector>();
        SetField(detector, "_range", 6f);
        SetField(detector, "_requireLineOfSight", false);

        var ai = go.AddComponent<BattonBone>();
        SetField(ai, "_dropSpeed", 4f);
        SetField(ai, "_dropDuration", 0.35f);
        SetField(ai, "_flySpeed", 4f);
        SetField(ai, "_sineAmplitude", 0.8f);
        SetField(ai, "_sineFrequency", 2.5f);
        SetField(ai, "_spriteRenderer", go.GetComponent<SpriteRenderer>());
        SetField(ai, "_sleepingSprite", LoadSprite($"{BasePath}/BattonBoneB81/BattonBoneB81.svg"));
        SetField(ai, "_flyingSprite", LoadSprite($"{BasePath}/BattonBoneB81/BattonBoneB81_Flying.svg"));

        SavePrefab(go, BasePath, "BattonBoneB81");
    }

    static void GenerateMettaurD2()
    {
        var go = NewEnemyRoot(BasePath, "MettaurD2", hp: 10, contactDamage: 10, isTrigger: true);
        var muzzle = AddMuzzle(go, new Vector3(0f, -0.3f, 0), Quaternion.Euler(0f, 0f, -90f));

        AddGravity(go);

        var ai = go.AddComponent<Mettaur>();
        SetField(ai, "_muzzle", muzzle.transform);
        SetField(ai, "_projectilePrefab", LoadPrefab(ProjectilePrefabPath));
        SetField(ai, "_hideDuration", 1.2f);
        SetField(ai, "_peekDuration", 0.8f);
        SetField(ai, "_spreadStep", 30f);
        SetField(ai, "_shotCount", 3);

        SavePrefab(go, BasePath, "MettaurD2");
    }

    static void GenerateSpikyMkII()
    {
        var go = NewEnemyRoot(BasePath, "SpikyMkII", hp: 20, contactDamage: 20, isTrigger: true);

        AddGravity(go);

        var patrol = go.AddComponent<PatrolWalk>();
        SetField(patrol, "_speed", 3f);

        SavePrefab(go, BasePath, "SpikyMkII");
    }
}
