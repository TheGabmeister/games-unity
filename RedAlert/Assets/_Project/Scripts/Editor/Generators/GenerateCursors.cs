using UnityEditor;
using UnityEngine;

public static class GenerateCursors
{
    private const int Size = 32;
    private const string Dir = "Assets/_Project/Sprites/Cursors";

    public static void Generate()
    {
        PrefabGeneratorUtils.EnsureFolder(Dir);

        GenerateSelectCursor();
        GenerateMoveCursor();
        GenerateAttackCursor();
        GenerateHarvestCursor();
        GenerateNoGoCursor();
        GenerateSellCursor();
        GenerateRepairCursor();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        ImportAll();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated 7 cursor textures");
    }

    static void GenerateSelectCursor()
    {
        var tex = CreateBlank();
        // Arrow pointer — top-left origin
        for (int i = 0; i < 16; i++)
        {
            SetPixel(tex, 0, i, Color.white);
            SetPixel(tex, 1, i, Color.white);
        }
        for (int i = 0; i < 12; i++)
            SetPixel(tex, i, 0, Color.white);
        for (int i = 0; i < 10; i++)
            SetPixel(tex, i, i, Color.white);
        // Fill
        for (int y = 1; y < 14; y++)
            for (int x = 2; x < y; x++)
                SetPixel(tex, x, y, Color.green);
        Save(tex, "Select");
    }

    static void GenerateMoveCursor()
    {
        var tex = CreateBlank();
        int c = 15;
        // Four arrows pointing outward from center
        for (int i = 0; i < 10; i++)
        {
            SetPixel(tex, c, c - i, Color.green); // up
            SetPixel(tex, c, c + i, Color.green); // down
            SetPixel(tex, c - i, c, Color.green); // left
            SetPixel(tex, c + i, c, Color.green); // right
        }
        // Arrowheads
        for (int i = 0; i < 4; i++)
        {
            SetPixel(tex, c - i, c - 8 + i, Color.green);
            SetPixel(tex, c + i, c - 8 + i, Color.green);
            SetPixel(tex, c - i, c + 8 - i, Color.green);
            SetPixel(tex, c + i, c + 8 - i, Color.green);
            SetPixel(tex, c - 8 + i, c - i, Color.green);
            SetPixel(tex, c - 8 + i, c + i, Color.green);
            SetPixel(tex, c + 8 - i, c - i, Color.green);
            SetPixel(tex, c + 8 - i, c + i, Color.green);
        }
        Save(tex, "Move");
    }

    static void GenerateAttackCursor()
    {
        var tex = CreateBlank();
        int c = 15;
        Color red = Color.red;
        // Crosshair
        for (int i = 4; i <= 12; i++)
        {
            SetPixel(tex, c, c - i, red);
            SetPixel(tex, c, c + i, red);
            SetPixel(tex, c - i, c, red);
            SetPixel(tex, c + i, c, red);
        }
        // Circle
        for (int a = 0; a < 360; a += 5)
        {
            float rad = a * Mathf.Deg2Rad;
            int x = c + Mathf.RoundToInt(Mathf.Cos(rad) * 8);
            int y = c + Mathf.RoundToInt(Mathf.Sin(rad) * 8);
            SetPixel(tex, x, y, red);
        }
        Save(tex, "Attack");
    }

    static void GenerateHarvestCursor()
    {
        var tex = CreateBlank();
        Color gold = new Color(1f, 0.85f, 0.2f);
        // Dollar sign
        int c = 15;
        for (int y = -8; y <= 8; y++)
            SetPixel(tex, c, c + y, gold);
        for (int x = -4; x <= 4; x++)
        {
            SetPixel(tex, c + x, c - 6, gold);
            SetPixel(tex, c + x, c, gold);
            SetPixel(tex, c + x, c + 6, gold);
        }
        for (int y = -6; y <= 0; y++)
            SetPixel(tex, c - 4, c + y, gold);
        for (int y = 0; y <= 6; y++)
            SetPixel(tex, c + 4, c + y, gold);
        Save(tex, "Harvest");
    }

    static void GenerateNoGoCursor()
    {
        var tex = CreateBlank();
        int c = 15;
        Color red = Color.red;
        // Circle with slash
        for (int a = 0; a < 360; a += 3)
        {
            float rad = a * Mathf.Deg2Rad;
            int x = c + Mathf.RoundToInt(Mathf.Cos(rad) * 10);
            int y = c + Mathf.RoundToInt(Mathf.Sin(rad) * 10);
            SetPixel(tex, x, y, red);
        }
        for (int i = -7; i <= 7; i++)
            SetPixel(tex, c + i, c - i, red);
        Save(tex, "NoGo");
    }

    static void GenerateSellCursor()
    {
        var tex = CreateBlank();
        Color yellow = Color.yellow;
        int c = 15;
        // Down arrow (sell = give away)
        for (int i = -8; i <= 8; i++)
            SetPixel(tex, c, c + i, yellow);
        for (int i = 0; i < 6; i++)
        {
            SetPixel(tex, c - i, c + 8 - i, yellow);
            SetPixel(tex, c + i, c + 8 - i, yellow);
        }
        // S
        for (int x = -6; x <= -2; x++)
        {
            SetPixel(tex, c + x, c - 8, yellow);
            SetPixel(tex, c + x, c - 4, yellow);
            SetPixel(tex, c + x, c, yellow);
        }
        SetPixel(tex, c - 6, c - 7, yellow);
        SetPixel(tex, c - 6, c - 6, yellow);
        SetPixel(tex, c - 6, c - 5, yellow);
        SetPixel(tex, c - 2, c - 3, yellow);
        SetPixel(tex, c - 2, c - 2, yellow);
        SetPixel(tex, c - 2, c - 1, yellow);
        Save(tex, "Sell");
    }

    static void GenerateRepairCursor()
    {
        var tex = CreateBlank();
        Color cyan = Color.cyan;
        int c = 15;
        // Wrench shape — vertical bar + angled top
        for (int y = -4; y <= 8; y++)
        {
            SetPixel(tex, c, c + y, cyan);
            SetPixel(tex, c + 1, c + y, cyan);
        }
        for (int x = -3; x <= 4; x++)
        {
            SetPixel(tex, c + x, c - 4, cyan);
            SetPixel(tex, c + x, c - 6, cyan);
        }
        SetPixel(tex, c - 3, c - 5, cyan);
        SetPixel(tex, c + 4, c - 5, cyan);
        Save(tex, "Repair");
    }

    static Texture2D CreateBlank()
    {
        var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        var pixels = new Color[Size * Size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        tex.SetPixels(pixels);
        return tex;
    }

    static void SetPixel(Texture2D tex, int x, int y, Color color)
    {
        if (x < 0 || x >= Size || y < 0 || y >= Size) return;
        tex.SetPixel(x, Size - 1 - y, color);
    }

    static void Save(Texture2D tex, string name)
    {
        tex.Apply();
        string path = $"{Dir}/{name}.png";
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);
    }

    static void ImportAll()
    {
        string[] names = { "Select", "Move", "Attack", "Harvest", "NoGo", "Sell", "Repair" };
        foreach (var name in names)
        {
            string path = $"{Dir}/{name}.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Cursor;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.alphaIsTransparency = true;
            importer.isReadable = true;
            importer.SaveAndReimport();
        }
    }
}
