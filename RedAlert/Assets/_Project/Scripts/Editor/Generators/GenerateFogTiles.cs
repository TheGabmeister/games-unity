using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class GenerateFogTiles
{
    public static void Generate()
    {
        string tileDir = "Assets/_Project/Tiles";
        string spriteDir = "Assets/_Project/Sprites/Overlays";
        PrefabGeneratorUtils.EnsureFolder(tileDir);
        PrefabGeneratorUtils.EnsureFolder(spriteDir);

        string path = $"{spriteDir}/Shroud.png";

        var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        var pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.black;
        tex.SetPixels(pixels);
        tex.Apply();

        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogError("Shroud sprite not found after import");
            return;
        }

        string tilePath = $"{tileDir}/Shroud.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (existing != null)
        {
            existing.sprite = sprite;
            EditorUtility.SetDirty(existing);
        }
        else
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            AssetDatabase.CreateAsset(tile, tilePath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated shroud tile");
    }
}
