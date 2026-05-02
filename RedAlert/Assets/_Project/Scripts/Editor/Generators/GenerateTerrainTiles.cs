using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class GenerateTerrainTiles
{
    private static readonly string[] TerrainNames =
        { "Clear", "Road", "Rough", "Sand", "Water", "Ore", "Gems" };

    public static void Generate()
    {
        string spriteDir = "Assets/_Project/Sprites/Terrain";
        string tileDir = "Assets/_Project/Tiles";
        PrefabGeneratorUtils.EnsureFolder(tileDir);

        foreach (string name in TerrainNames)
            ImportAsSprite($"{spriteDir}/{name}.png");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        foreach (string name in TerrainNames)
            CreateTile($"{spriteDir}/{name}.png", $"{tileDir}/{name}.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Generated {TerrainNames.Length} terrain tiles");
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

    static void CreateTile(string spritePath, string tilePath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            Debug.LogError($"Sprite not found at {spritePath}");
            return;
        }

        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        AssetDatabase.CreateAsset(tile, tilePath);
    }
}
