using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Grass,
    Forest,
    Mountain,
    Water,
    River,
    Desert,
    TownEntrance,
    DungeonEntrance,
    Floor,
    Wall,
    Lava,
    Chest
}

public class ProceduralTileFactory : MonoBehaviour
{
    [SerializeField] TilePalette palette;

    const int TileSize = 16;
    const float PPU = 16f;

    Dictionary<TileType, Sprite> spriteCache = new();

    public void Initialize(TilePalette tilePalette)
    {
        palette = tilePalette;
        GenerateAllSprites();
    }

    void GenerateAllSprites()
    {
        foreach (TileType type in System.Enum.GetValues(typeof(TileType)))
            spriteCache[type] = CreateSprite(type);
    }

    public Sprite GetSprite(TileType type)
    {
        if (!spriteCache.ContainsKey(type))
            spriteCache[type] = CreateSprite(type);
        return spriteCache[type];
    }

    Sprite CreateSprite(TileType type)
    {
        var tex = new Texture2D(TileSize, TileSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        var pixels = new Color32[TileSize * TileSize];

        switch (type)
        {
            case TileType.Grass:
                FillGrass(pixels);
                break;
            case TileType.Forest:
                FillForest(pixels);
                break;
            case TileType.Mountain:
                FillMountain(pixels);
                break;
            case TileType.Water:
                FillWater(pixels);
                break;
            case TileType.River:
                FillRiver(pixels);
                break;
            case TileType.Desert:
                FillDesert(pixels);
                break;
            case TileType.TownEntrance:
                FillTownEntrance(pixels);
                break;
            case TileType.DungeonEntrance:
                FillDungeonEntrance(pixels);
                break;
            case TileType.Floor:
                FillFlat(pixels, palette.Floor);
                break;
            case TileType.Wall:
                FillWall(pixels);
                break;
            case TileType.Lava:
                FillLava(pixels);
                break;
            case TileType.Chest:
                FillChest(pixels);
                break;
        }

        tex.SetPixels32(pixels);
        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, TileSize, TileSize),
            new Vector2(0.5f, 0.5f),
            PPU
        );
    }

    // --- Tile painters ---

    void FillFlat(Color32[] pixels, Color color)
    {
        var c32 = ToColor32(color);
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = c32;
    }

    void FillGrass(Color32[] pixels)
    {
        var baseColor = ToColor32(palette.Grass);
        var altColor = ToColor32(palette.GrassAlt);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = baseColor;

        // Scatter a few darker "grass blade" pixels using a deterministic pattern
        int[] scatterX = { 2, 7, 12, 5, 10, 14, 1, 9 };
        int[] scatterY = { 3, 8, 5, 13, 1, 11, 7, 14 };
        for (int i = 0; i < scatterX.Length; i++)
        {
            int idx = scatterY[i] * TileSize + scatterX[i];
            pixels[idx] = altColor;
        }
    }

    void FillForest(Color32[] pixels)
    {
        var baseColor = ToColor32(palette.Forest);
        var darkGreen = new Color32(15, 50, 15, 255);

        // Fill background with forest color
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = baseColor;

        // Draw 3 small upward-pointing triangles
        DrawTriangleUp(pixels, 3, 4, 4, darkGreen);   // left tree
        DrawTriangleUp(pixels, 8, 3, 5, darkGreen);   // center tree (slightly bigger)
        DrawTriangleUp(pixels, 12, 5, 4, darkGreen);  // right tree
    }

    void DrawTriangleUp(Color32[] pixels, int cx, int baseY, int height, Color32 color)
    {
        for (int row = 0; row < height; row++)
        {
            int y = baseY + row;
            if (y < 0 || y >= TileSize) continue;
            int halfWidth = (height - row);
            for (int dx = -halfWidth; dx <= halfWidth; dx++)
            {
                int x = cx + dx;
                if (x >= 0 && x < TileSize)
                    pixels[y * TileSize + x] = color;
            }
        }
    }

    void FillMountain(Color32[] pixels)
    {
        var baseColor = ToColor32(palette.Mountain);
        var peakColor = new Color32(180, 130, 80, 255);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = baseColor;

        // Zigzag peaks across top third
        int[] peakY = { 12, 13, 14, 13, 12, 11, 12, 13, 14, 15, 14, 13, 12, 11, 10, 11 };
        for (int x = 0; x < TileSize && x < peakY.Length; x++)
        {
            for (int y = peakY[x]; y < TileSize; y++)
            {
                if (y >= 0 && y < TileSize)
                    pixels[y * TileSize + x] = peakColor;
            }
        }
    }

    void FillWater(Color32[] pixels)
    {
        var baseColor = ToColor32(palette.Water);
        var waveColor = ToColor32(palette.WaterAlt);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = baseColor;

        // Two horizontal wavy lines using sine pattern
        for (int x = 0; x < TileSize; x++)
        {
            int wave1Y = 5 + Mathf.RoundToInt(Mathf.Sin(x * 0.8f) * 1.2f);
            int wave2Y = 11 + Mathf.RoundToInt(Mathf.Sin(x * 0.8f + 2f) * 1.2f);

            if (wave1Y >= 0 && wave1Y < TileSize)
                pixels[wave1Y * TileSize + x] = waveColor;
            if (wave2Y >= 0 && wave2Y < TileSize)
                pixels[wave2Y * TileSize + x] = waveColor;
        }
    }

    void FillRiver(Color32[] pixels)
    {
        var baseColor = ToColor32(palette.River);
        var borderColor = new Color32(40, 80, 160, 255);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = baseColor;

        // Darker borders on left (x=0,1) and right (x=14,15)
        for (int y = 0; y < TileSize; y++)
        {
            pixels[y * TileSize + 0] = borderColor;
            pixels[y * TileSize + 1] = borderColor;
            pixels[y * TileSize + 14] = borderColor;
            pixels[y * TileSize + 15] = borderColor;
        }
    }

    void FillDesert(Color32[] pixels)
    {
        var baseColor = ToColor32(palette.Desert);
        var dotColor = new Color32(200, 170, 80, 255);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = baseColor;

        // Scattered dot pattern
        int[] dotX = { 2, 6, 10, 14, 4, 8, 12, 1, 9, 13 };
        int[] dotY = { 2, 5, 8, 11, 13, 1, 4, 7, 10, 14 };
        for (int i = 0; i < dotX.Length; i++)
        {
            int idx = dotY[i] * TileSize + dotX[i];
            if (idx >= 0 && idx < pixels.Length)
                pixels[idx] = dotColor;
        }
    }

    void FillTownEntrance(Color32[] pixels)
    {
        var baseColor = ToColor32(palette.TownEntrance);
        var archColor = new Color32(100, 100, 110, 255);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = baseColor;

        // Draw an arch/door outline
        // Vertical pillars on sides
        for (int y = 0; y < 12; y++)
        {
            pixels[y * TileSize + 4] = archColor;
            pixels[y * TileSize + 5] = archColor;
            pixels[y * TileSize + 10] = archColor;
            pixels[y * TileSize + 11] = archColor;
        }
        // Arch top
        for (int x = 4; x <= 11; x++)
        {
            pixels[11 * TileSize + x] = archColor;
            pixels[12 * TileSize + x] = archColor;
        }
        // Darker interior for the door opening
        var doorColor = new Color32(60, 60, 70, 255);
        for (int y = 0; y < 11; y++)
            for (int x = 6; x <= 9; x++)
                pixels[y * TileSize + x] = doorColor;
    }

    void FillDungeonEntrance(Color32[] pixels)
    {
        var baseColor = ToColor32(palette.DungeonEntrance);
        var caveColor = new Color32(20, 20, 25, 255);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = baseColor;

        // Cave mouth shape: oval-ish dark area in center
        for (int y = 1; y < 12; y++)
        {
            int halfWidth = (y < 6) ? y : (12 - y);
            halfWidth = Mathf.Max(halfWidth, 1);
            for (int dx = -halfWidth; dx <= halfWidth; dx++)
            {
                int x = 8 + dx;
                if (x >= 0 && x < TileSize)
                    pixels[y * TileSize + x] = caveColor;
            }
        }
    }

    void FillWall(Color32[] pixels)
    {
        var baseColor = ToColor32(palette.Wall);
        var lineColor = new Color32(50, 50, 60, 255);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = baseColor;

        // Horizontal mortar lines at y=4, y=11
        for (int x = 0; x < TileSize; x++)
        {
            pixels[4 * TileSize + x] = lineColor;
            pixels[11 * TileSize + x] = lineColor;
        }
        // Vertical mortar line offset per row band (brick pattern)
        for (int y = 0; y < 4; y++)
            pixels[y * TileSize + 8] = lineColor;
        for (int y = 5; y < 11; y++)
            pixels[y * TileSize + 4] = lineColor;
        for (int y = 12; y < TileSize; y++)
            pixels[y * TileSize + 12] = lineColor;
    }

    void FillLava(Color32[] pixels)
    {
        var baseColor = ToColor32(palette.Lava);
        var brightColor = new Color32(255, 140, 40, 255);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = baseColor;

        // Bright highlight streaks
        for (int x = 0; x < TileSize; x++)
        {
            int waveY = 7 + Mathf.RoundToInt(Mathf.Sin(x * 1.2f) * 2f);
            if (waveY >= 0 && waveY < TileSize)
                pixels[waveY * TileSize + x] = brightColor;
        }
    }

    void FillChest(Color32[] pixels)
    {
        var floorColor = ToColor32(palette.ChestFloor);
        var chestColor = ToColor32(palette.Chest);
        var lidColor = new Color32(180, 120, 50, 255);

        // Floor background
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = floorColor;

        // Chest body: 8x6 rectangle centered
        for (int y = 4; y < 10; y++)
            for (int x = 4; x < 12; x++)
                pixels[y * TileSize + x] = chestColor;

        // Lid highlight on top row of chest
        for (int x = 4; x < 12; x++)
            pixels[9 * TileSize + x] = lidColor;

        // Keyhole pixel in center
        pixels[6 * TileSize + 8] = new Color32(40, 30, 10, 255);
    }

    static Color32 ToColor32(Color c)
    {
        return new Color32(
            (byte)(c.r * 255),
            (byte)(c.g * 255),
            (byte)(c.b * 255),
            (byte)(c.a * 255)
        );
    }
}
