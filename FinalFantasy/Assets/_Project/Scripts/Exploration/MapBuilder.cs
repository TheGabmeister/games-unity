using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(GridData))]
public class MapBuilder : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] ProceduralTileFactory tileFactory;
    [SerializeField] TilePalette palette;

    GridData gridData;
    Dictionary<TileType, TileBase> tileAssets = new();

    public Vector2Int PlayerStartPosition => new Vector2Int(10, 10);

    void Awake()
    {
        gridData = GetComponent<GridData>();
    }

    public void BuildTestMap()
    {
        if (tileFactory != null && palette != null)
            tileFactory.Initialize(palette);

        int w = 20, h = 20;
        gridData.Initialize(w, h);

        // Create tile assets from factory sprites
        foreach (TileType type in System.Enum.GetValues(typeof(TileType)))
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = tileFactory.GetSprite(type);
            tileAssets[type] = tile;
        }

        // Fill with grass
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                SetMapTile(x, y, TileType.Grass,
                    TilePassability.OnFoot | TilePassability.AirshipLand | TilePassability.AirshipFly);

        // Walls around border
        for (int x = 0; x < w; x++)
        {
            SetMapTile(x, 0, TileType.Wall, TilePassability.None);
            SetMapTile(x, h - 1, TileType.Wall, TilePassability.None);
        }
        for (int y = 0; y < h; y++)
        {
            SetMapTile(0, y, TileType.Wall, TilePassability.None);
            SetMapTile(w - 1, y, TileType.Wall, TilePassability.None);
        }

        // Water pond at (7,7) to (9,9)
        for (int x = 7; x <= 9; x++)
            for (int y = 7; y <= 9; y++)
                SetMapTile(x, y, TileType.Water,
                    TilePassability.Ship | TilePassability.AirshipFly);

        // Forest patches
        SetMapTile(3, 3, TileType.Forest,
            TilePassability.OnFoot | TilePassability.AirshipFly);
        SetMapTile(4, 3, TileType.Forest,
            TilePassability.OnFoot | TilePassability.AirshipFly);
        SetMapTile(3, 4, TileType.Forest,
            TilePassability.OnFoot | TilePassability.AirshipFly);
        SetMapTile(15, 15, TileType.Forest,
            TilePassability.OnFoot | TilePassability.AirshipFly);
        SetMapTile(16, 15, TileType.Forest,
            TilePassability.OnFoot | TilePassability.AirshipFly);

        // Mountains
        SetMapTile(5, 14, TileType.Mountain, TilePassability.AirshipFly);
        SetMapTile(6, 14, TileType.Mountain, TilePassability.AirshipFly);
        SetMapTile(5, 15, TileType.Mountain, TilePassability.AirshipFly);
    }

    void SetMapTile(int x, int y, TileType type, TilePassability pass)
    {
        var pos = new Vector2Int(x, y);
        gridData.SetTile(pos, type, pass);

        if (tilemap != null && tileAssets.ContainsKey(type))
            tilemap.SetTile(new Vector3Int(x, y, 0), tileAssets[type]);
    }
}
