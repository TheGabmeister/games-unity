using UnityEditor;
using UnityEngine;
using static EnemyGeneratorCore;

public static class CyberSpaceEnemyGenerator
{
    const string BasePath = "Assets/_Project/Enemies/CyberSpace";

    [MenuItem("Tools/MegamanX4/Generate CyberSpace Enemies")]
    public static void Generate()
    {
        GenerateMiruToraeru();
        GenerateTriScan();
        GenerateProtecton();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CyberSpace enemies generated.");
    }

    static void GenerateMiruToraeru()
    {
        var go = NewEnemyRoot(BasePath, "MiruToraeru", hp: 15, contactDamage: 15, isTrigger: true);
        var muzzle = AddMuzzle(go, new Vector3(0.3f, 0f, 0f));

        var ai = go.AddComponent<MiruToraeru>();
        SetField(ai, "_spriteRenderer", go.GetComponent<SpriteRenderer>());
        SetField(ai, "_hurtBox", go.GetComponent<HurtBox>());
        SetField(ai, "_muzzle", muzzle.transform);
        SetField(ai, "_projectilePrefab", LoadPrefab(ProjectilePrefabPath));
        SetField(ai, "_hiddenDuration", 1.2f);
        SetField(ai, "_appearDuration", 0.3f);
        SetField(ai, "_attackDuration", 0.4f);
        SetField(ai, "_disappearDuration", 0.3f);
        SetField(ai, "_teleportOffset", 3.5f);
        SetField(ai, "_teleportHeight", 1f);
        SetField(ai, "_playerSearchRange", 14f);

        SavePrefab(go, BasePath, "MiruToraeru");
    }

    static void GenerateTriScan()
    {
        var go = NewEnemyRoot(BasePath, "TriScan", hp: 20, contactDamage: 10, isTrigger: true);

        var hover = go.AddComponent<HoverSine>();
        SetField(hover, "_amplitude", 0.2f);
        SetField(hover, "_frequency", 0.6f);

        var emitterRoot = new GameObject("EmitterRoot");
        emitterRoot.transform.SetParent(go.transform, false);

        var beam0 = BuildTriScanBeam(emitterRoot, 0f);
        var beam1 = BuildTriScanBeam(emitterRoot, 120f);
        var beam2 = BuildTriScanBeam(emitterRoot, 240f);

        var ai = go.AddComponent<TriScan>();
        SetField(ai, "_emitterRoot", emitterRoot.transform);
        SetField(ai, "_idleDuration", 1.5f);
        SetField(ai, "_fireDuration", 1.5f);
        SetField(ai, "_rotationSpeed", 120f);
        SetBeamsArray(ai, new[] { beam0, beam1, beam2 });

        SavePrefab(go, BasePath, "TriScan");
    }

    static GameObject BuildTriScanBeam(GameObject emitterRoot, float angleDegrees)
    {
        const float EmitterDistance = 0.38f; // emitter node offset from TriScan center (from SVG: 38 px / 100 ppu)
        const float BeamHalfLength = 1.92f;  // half of beam sprite length (from SVG: 384 px / 100 / 2)
        float offsetFromCenter = EmitterDistance + BeamHalfLength;

        float rad = angleDegrees * Mathf.Deg2Rad;
        var beam = new GameObject($"Beam_{Mathf.RoundToInt(angleDegrees)}");
        beam.transform.SetParent(emitterRoot.transform, false);
        beam.transform.localPosition = new Vector3(Mathf.Cos(rad) * offsetFromCenter, Mathf.Sin(rad) * offsetFromCenter, 0f);
        beam.transform.localRotation = Quaternion.Euler(0f, 0f, angleDegrees);
        beam.layer = emitterRoot.layer;

        var sr = beam.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite($"{BasePath}/TriScan/TriScan_Beam.svg");
        sr.sharedMaterial = LoadMaterial(VectorMaterialPath);
        sr.sortingOrder = 1;

        var box = beam.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        if (sr.sprite) box.size = sr.sprite.bounds.size;

        var hit = beam.AddComponent<HitBox>();
        SetField(hit, "_damage", 15);

        beam.SetActive(false);
        return beam;
    }

    static void SetBeamsArray(TriScan ai, GameObject[] beams)
    {
        var so = new SerializedObject(ai);
        var prop = so.FindProperty("_beams");
        prop.arraySize = beams.Length;
        for (int i = 0; i < beams.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = beams[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void GenerateProtecton()
    {
        var go = NewEnemyRoot(BasePath, "Protecton", hp: 25, contactDamage: 15, isTrigger: true);
        var muzzle = AddMuzzle(go, new Vector3(0.5f, 0.1f, 0f));

        AddGravity(go);

        go.AddComponent<Protecton>();

        var detector = go.AddComponent<PlayerDetector>();
        SetField(detector, "_range", 10f);
        SetField(detector, "_coneAngle", 120f);
        SetField(detector, "_requireLineOfSight", false);

        var shoot = go.AddComponent<EnemyShoot>();
        SetField(shoot, "_projectilePrefab", LoadPrefab(ProjectilePrefabPath));
        SetField(shoot, "_muzzle", muzzle.transform);
        SetField(shoot, "_burstCount", 1);
        SetField(shoot, "_burstInterval", 0.15f);
        SetField(shoot, "_cooldown", 2f);

        SavePrefab(go, BasePath, "Protecton");
    }
}
