using UnityEditor;
using UnityEngine;

public static class GenerateUnitData
{
    public static void Generate()
    {
        string dir = "Assets/_Project/Data/Units";
        string spriteDir = "Assets/_Project/Sprites/Units";
        PrefabGeneratorUtils.EnsureFolder(dir);

        ImportAsSprite($"{spriteDir}/RifleInfantry.png");
        ImportAsSprite($"{spriteDir}/LightTank.png");
        ImportAsSprite($"{spriteDir}/Ranger.png");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        CreateUnitData(dir, "RifleInfantry", "Rifle Infantry", LocomotionType.Foot, 3f,
            AssetDatabase.LoadAssetAtPath<Sprite>($"{spriteDir}/RifleInfantry.png"));
        CreateUnitData(dir, "LightTank", "Light Tank", LocomotionType.Tracked, 5f,
            AssetDatabase.LoadAssetAtPath<Sprite>($"{spriteDir}/LightTank.png"));
        CreateUnitData(dir, "Ranger", "Ranger", LocomotionType.Wheeled, 7f,
            AssetDatabase.LoadAssetAtPath<Sprite>($"{spriteDir}/Ranger.png"));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated 3 UnitData assets with sprites");
    }

    static void CreateUnitData(string dir, string fileName, string displayName,
        LocomotionType locomotion, float speed, Sprite sprite)
    {
        string path = $"{dir}/{fileName}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<UnitData>(path);

        if (existing != null)
        {
            existing.DisplayName = displayName;
            existing.Sprite = sprite;
            existing.Locomotion = locomotion;
            existing.BaseSpeed = speed;
            EditorUtility.SetDirty(existing);
        }
        else
        {
            var data = ScriptableObject.CreateInstance<UnitData>();
            data.DisplayName = displayName;
            data.Sprite = sprite;
            data.Locomotion = locomotion;
            data.BaseSpeed = speed;
            AssetDatabase.CreateAsset(data, path);
        }
    }

    static void ImportAsSprite(string path)
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
