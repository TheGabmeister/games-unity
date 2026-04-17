using UnityEditor;
using UnityEngine;
using static EnemyGeneratorCore;

public static class JungleEnemyGenerator
{
    const string BasePath = "Assets/_Project/Enemies/Jungle";
    const string KyunnbyunnPrefabPath = "Assets/_Project/Enemies/Recurring/Kyunnbyunn/Kyunnbyunn.prefab";

    [MenuItem("Tools/MegamanX4/Generate Jungle Enemies")]
    public static void Generate()
    {
        GenerateKillFisher();
        GenerateMetalGabyoall();
        GenerateObiiru();
        GenerateMegaNest();
        GenerateSpiderCore();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Jungle enemies generated.");
    }

    static void GenerateKillFisher()
    {
        var go = NewEnemyRoot(BasePath, "KillFisher", hp: 10, contactDamage: 10, isTrigger: true);

        var hook = BuildKillFisherHook(go);

        var ai = go.AddComponent<KillFisher>();
        SetField(ai, "_hook", hook.transform);
        SetField(ai, "_dropDistance", 2.5f);
        SetField(ai, "_dropSpeed", 6f);
        SetField(ai, "_retractSpeed", 4f);
        SetField(ai, "_dangleDuration", 0.8f);
        SetField(ai, "_retractedPause", 1.2f);

        SavePrefab(go, BasePath, "KillFisher");
    }

    static GameObject BuildKillFisherHook(GameObject parent)
    {
        var hook = new GameObject("Hook");
        hook.transform.SetParent(parent.transform, false);
        hook.transform.localPosition = new Vector3(0f, -0.2f, 0f);
        hook.layer = parent.layer;

        var sr = hook.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite($"{BasePath}/KillFisher/KillFisher_Hook.svg");
        sr.sharedMaterial = LoadMaterial(VectorMaterialPath);
        sr.sortingOrder = -1;

        var box = hook.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        if (sr.sprite) box.size = sr.sprite.bounds.size;

        var hit = hook.AddComponent<HitBox>();
        SetField(hit, "_damage", 15);

        return hook;
    }

    static void GenerateMetalGabyoall()
    {
        var go = NewEnemyRoot(BasePath, "MetalGabyoall", hp: 1, contactDamage: 20, isTrigger: true);

        // Invulnerable hazard: strip Health/HurtBox/DestroyOnDepleted/DamageFlash; keep HitBox for contact damage.
        Object.DestroyImmediate(go.GetComponent<DestroyOnDepleted>());
        Object.DestroyImmediate(go.GetComponent<DamageFlash>());
        Object.DestroyImmediate(go.GetComponent<HurtBox>());
        Object.DestroyImmediate(go.GetComponent<Health>());

        AddGravity(go);

        var patrol = go.AddComponent<PatrolWalk>();
        SetField(patrol, "_speed", 1.5f);

        SavePrefab(go, BasePath, "MetalGabyoall");
    }

    static void GenerateObiiru()
    {
        var go = NewEnemyRoot(BasePath, "Obiiru", hp: 15, contactDamage: 15, isTrigger: true);

        var ai = go.AddComponent<Obiiru>();
        SetField(ai, "_ropeLength", 2f);
        SetField(ai, "_swingAngle", 50f);
        SetField(ai, "_swingPeriod", 2f);

        SavePrefab(go, BasePath, "Obiiru");
    }

    static void GenerateMegaNest()
    {
        var go = NewEnemyRoot(BasePath, "MegaNest", hp: 40, contactDamage: 10, isTrigger: true);

        var ai = go.AddComponent<MegaNest>();
        SetField(ai, "_spawnPrefab", LoadPrefab(KyunnbyunnPrefabPath));
        SetField(ai, "_spawnInterval", 3f);
        SetField(ai, "_maxActive", 3);
        SetField(ai, "_initialDelay", 1.5f);

        SavePrefab(go, BasePath, "MegaNest");
    }

    static void GenerateSpiderCore()
    {
        var go = NewEnemyRoot(BasePath, "SpiderCore", hp: 30, contactDamage: 20, isTrigger: true);

        var detector = go.AddComponent<PlayerDetector>();
        SetField(detector, "_range", 6f);
        SetField(detector, "_requireLineOfSight", false);

        var swoop = go.AddComponent<SwoopAttack>();
        SetField(swoop, "_swoopSpeed", 7f);
        SetField(swoop, "_returnSpeed", 5f);
        SetField(swoop, "_cooldown", 3f);
        SetField(swoop, "_arrivalDistance", 0.5f);

        SavePrefab(go, BasePath, "SpiderCore");
    }
}
