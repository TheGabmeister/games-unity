using System.IO;
using UnityEditor;
using UnityEngine;

public static class BusterShotPrefabGenerator
{
    const string SpritesFolder = "Assets/_Project/Player/Shots";
    const string PrefabsFolder = "Assets/_Project/Player/Shots/Prefabs";
    const int EnvironmentAndEnemyMask = (1 << 6) | (1 << 7);

    readonly struct Variant
    {
        public readonly string Name;
        public readonly string SpritePath;
        public readonly float Speed;
        public readonly float Lifetime;
        public readonly int Damage;
        public readonly float Radius;

        public Variant(string name, string sprite, float speed, float lifetime, int damage, float radius)
        {
            Name = name;
            SpritePath = sprite;
            Speed = speed;
            Lifetime = lifetime;
            Damage = damage;
            Radius = radius;
        }
    }

    static readonly Variant[] Variants =
    {
        new("Small", $"{SpritesFolder}/MegamanX_Shot_Small.svg", 18f, 0.6f, 1, 0.08f),
        new("Semi",  $"{SpritesFolder}/MegamanX_Shot_Semi.svg",  16f, 0.8f, 2, 0.15f),
        new("Full",  $"{SpritesFolder}/MegamanX_Shot_Full.svg",  14f, 1.2f, 3, 0.25f),
    };

    [MenuItem("Tools/MegamanX4/Generate Buster Shot Prefabs")]
    public static void Generate()
    {
        if (!Directory.Exists(PrefabsFolder))
            Directory.CreateDirectory(PrefabsFolder);

        if (!EditorUtility.DisplayDialog(
                "Generate Buster Shot Prefabs",
                $"This will create/overwrite 3 prefabs in {PrefabsFolder}. Continue?",
                "Generate", "Cancel"))
            return;

        int generated = 0;
        foreach (var v in Variants)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(v.SpritePath);
            if (!sprite)
            {
                Debug.LogError($"Sprite not found at {v.SpritePath}. Skipping {v.Name}.");
                continue;
            }

            var go = new GameObject($"BusterShot_{v.Name}");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 10;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.useFullKinematicContacts = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = v.Radius;

            var shot = go.AddComponent<BusterShot>();
            var so = new SerializedObject(shot);
            so.FindProperty("speed").floatValue = v.Speed;
            so.FindProperty("lifetime").floatValue = v.Lifetime;
            so.FindProperty("damage").intValue = v.Damage;
            so.FindProperty("hitLayers").intValue = EnvironmentAndEnemyMask;
            so.ApplyModifiedPropertiesWithoutUndo();

            var path = $"{PrefabsFolder}/BusterShot_{v.Name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            generated++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Generated {generated} buster shot prefabs at {PrefabsFolder}.");
    }
}
