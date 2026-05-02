using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public static class GenerateTerrainTiles
{
    private static readonly (string name, Color color)[] TerrainColors =
    {
        ("Clear", new Color(0.4f, 0.7f, 0.3f)),
        ("Road", new Color(0.6f, 0.6f, 0.6f)),
        ("Rough", new Color(0.5f, 0.35f, 0.2f)),
        ("Sand", new Color(0.85f, 0.75f, 0.5f)),
        ("Water", new Color(0.2f, 0.4f, 0.8f)),
        ("Ore", new Color(0.8f, 0.6f, 0.1f)),
        ("Gems", new Color(0.6f, 0.2f, 0.8f))
    };

    public static void Generate()
    {
        string spriteDir = "Assets/_Project/Sprites/Terrain";
        string tileDir = "Assets/_Project/Tiles";
        PrefabGeneratorUtils.EnsureFolder(spriteDir);
        PrefabGeneratorUtils.EnsureFolder(tileDir);

        foreach (var (name, color) in TerrainColors)
        {
            string pngPath = $"{spriteDir}/{name}.png";
            CreateSolidPNG(pngPath, 64, color);
            ImportAsSprite(pngPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        foreach (var (name, _) in TerrainColors)
        {
            string spritePath = $"{spriteDir}/{name}.png";
            string tilePath = $"{tileDir}/{name}.asset";
            CreateTile(spritePath, tilePath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Generated {TerrainColors.Length} terrain tiles");
    }

    static void CreateSolidPNG(string path, int size, Color color)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();

        string fullPath = Path.Combine(Application.dataPath, "..", path).Replace("\\", "/");
        string dir = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
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
