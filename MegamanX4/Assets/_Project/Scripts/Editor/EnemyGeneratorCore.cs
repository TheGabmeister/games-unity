using System.IO;
using UnityEditor;
using UnityEngine;

public static class EnemyGeneratorCore
{
    public const string ProjectilePrefabPath = "Assets/_Project/Defaults/ProjectileDefault.prefab";
    public const string VectorMaterialPath = "Packages/com.unity.vectorgraphics/Runtime/Materials/Unlit_Vector.mat";

    public static GameObject NewEnemyRoot(string basePath, string name, int hp, int contactDamage, bool isTrigger)
    {
        var go = new GameObject(name) { layer = Layers.Enemy };

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite($"{basePath}/{name}/{name}.svg");
        sr.sharedMaterial = LoadMaterial(VectorMaterialPath);

        var box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = isTrigger;
        if (sr.sprite) box.size = sr.sprite.bounds.size;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var health = go.AddComponent<Health>();
        SetField(health, "_maxHealth", hp);
        SetField(health, "_invulnerabilityDuration", 0f);

        go.AddComponent<HurtBox>();

        var hit = go.AddComponent<HitBox>();
        SetField(hit, "_damage", contactDamage);

        go.AddComponent<DestroyOnDepleted>();

        var flash = go.AddComponent<DamageFlash>();
        SetField(flash, "_target", sr);

        return go;
    }

    public static void AddGravity(GameObject go)
    {
        var sr = go.GetComponent<SpriteRenderer>();
        var gravity = go.AddComponent<Gravity>();
        if (sr && sr.sprite)
            SetField(gravity, "_halfHeight", sr.sprite.bounds.size.y * 0.5f);
    }

    public static GameObject AddMuzzle(GameObject parent, Vector3 localPosition)
    {
        return AddMuzzle(parent, localPosition, Quaternion.identity);
    }

    public static GameObject AddMuzzle(GameObject parent, Vector3 localPosition, Quaternion localRotation)
    {
        var muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(parent.transform, false);
        muzzle.transform.localPosition = localPosition;
        muzzle.transform.localRotation = localRotation;
        return muzzle;
    }

    public static Sprite LoadSprite(string path)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (!sprite) Debug.LogWarning($"Sprite not found at {path}");
        return sprite;
    }

    public static GameObject LoadPrefab(string path)
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (!go) Debug.LogWarning($"Prefab not found at {path}");
        return go;
    }

    public static Material LoadMaterial(string path)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!mat) Debug.LogWarning($"Material not found at {path}");
        return mat;
    }

    public static void SavePrefab(GameObject go, string basePath, string subfolder)
    {
        string dir = $"{basePath}/{subfolder}";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string path = $"{dir}/{subfolder}.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    public static void SetField(Object target, string fieldName, object value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop == null)
        {
            Debug.LogError($"Field '{fieldName}' not found on {target.GetType().Name}");
            return;
        }
        switch (value)
        {
            case System.Enum e: prop.enumValueIndex = System.Convert.ToInt32(e); break;
            case int i: prop.intValue = i; break;
            case float f: prop.floatValue = f; break;
            case bool b: prop.boolValue = b; break;
            case string s: prop.stringValue = s; break;
            case Vector2 v: prop.vector2Value = v; break;
            case Vector3 v3: prop.vector3Value = v3; break;
            case Object o: prop.objectReferenceValue = o; break;
            default: prop.boxedValue = value; break;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
