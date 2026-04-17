using UnityEditor;
using UnityEngine;
using static EnemyGeneratorCore;

public static class SkyLagoonEnemyGenerator
{
    const string BasePath = "Assets/_Project/Enemies/SkyLagoon";

    [MenuItem("Tools/MegamanX4/Generate Sky Lagoon Enemies")]
    public static void Generate()
    {
        GenerateTonboroidS();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Sky Lagoon enemies generated.");
    }

    static void GenerateTonboroidS()
    {
        var go = NewEnemyRoot(BasePath, "TonboroidS", hp: 1, contactDamage: 3, isTrigger: true);

        var hover = go.AddComponent<HoverSine>();
        SetField(hover, "_amplitude", 1f);
        SetField(hover, "_frequency", 0.5f);

        var detector = go.AddComponent<PlayerDetector>();
        SetField(detector, "_range", 7f);
        SetField(detector, "_requireLineOfSight", false);

        var swoop = go.AddComponent<SwoopAttack>();
        SetField(swoop, "_swoopSpeed", 8f);
        SetField(swoop, "_returnSpeed", 6f);
        SetField(swoop, "_cooldown", 3f);
        SetField(swoop, "_arrivalDistance", 0.5f);

        SavePrefab(go, BasePath, "TonboroidS");
    }
}
