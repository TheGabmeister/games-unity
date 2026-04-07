using UnityEngine;

[System.Flags]
public enum TilePassability
{
    None        = 0,
    OnFoot      = 1 << 0,
    Canoe       = 1 << 1,
    Ship        = 1 << 2,
    AirshipLand = 1 << 3,
    AirshipFly  = 1 << 4,
}

public class GridData : MonoBehaviour
{
    int width;
    int height;
    TileType[] tiles;
    TilePassability[] passability;

    public int Width => width;
    public int Height => height;

    public void Initialize(int w, int h)
    {
        width = w;
        height = h;
        tiles = new TileType[w * h];
        passability = new TilePassability[w * h];
    }

    public bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public bool IsPassable(Vector2Int pos, TilePassability mode = TilePassability.OnFoot)
    {
        if (!IsInBounds(pos)) return false;
        return (passability[pos.y * width + pos.x] & mode) != 0;
    }

    public TileType GetTileType(Vector2Int pos)
    {
        if (!IsInBounds(pos)) return TileType.Wall;
        return tiles[pos.y * width + pos.x];
    }

    public void SetTile(Vector2Int pos, TileType type, TilePassability pass)
    {
        if (!IsInBounds(pos)) return;
        int idx = pos.y * width + pos.x;
        tiles[idx] = type;
        passability[idx] = pass;
    }
}
