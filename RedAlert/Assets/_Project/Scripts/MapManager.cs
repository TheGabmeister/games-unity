using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [SerializeField] private MapData _mapData;
    [SerializeField] private TileBase _clearTile;
    [SerializeField] private TileBase _roadTile;
    [SerializeField] private TileBase _roughTile;
    [SerializeField] private TileBase _sandTile;
    [SerializeField] private TileBase _waterTile;
    [SerializeField] private TileBase _oreTile;
    [SerializeField] private TileBase _gemsTile;

    private TerrainType[,] _grid;
    private Tilemap _tilemap;
    private readonly Dictionary<Vector2Int, Entity> _entityGrid = new();

    public int Width => _mapData != null ? _mapData.Width : 0;
    public int Height => _mapData != null ? _mapData.Height : 0;

    public static MapManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        _tilemap = FindFirstObjectByType<Tilemap>();
        if (_tilemap == null || _mapData == null) return;
        BuildGrid();
        RenderMap();
    }

    void BuildGrid()
    {
        _grid = new TerrainType[_mapData.Width, _mapData.Height];
        for (int x = 0; x < _mapData.Width; x++)
            for (int y = 0; y < _mapData.Height; y++)
                _grid[x, y] = _mapData.GetCell(x, y);
    }

    void RenderMap()
    {
        _tilemap.ClearAllTiles();
        for (int x = 0; x < _mapData.Width; x++)
        {
            for (int y = 0; y < _mapData.Height; y++)
            {
                TileBase tile = GetTileForTerrain(_grid[x, y]);
                if (tile != null)
                    _tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    TileBase GetTileForTerrain(TerrainType terrain)
    {
        return terrain switch
        {
            TerrainType.Clear => _clearTile,
            TerrainType.Road => _roadTile,
            TerrainType.Rough => _roughTile,
            TerrainType.Sand => _sandTile,
            TerrainType.Water => _waterTile,
            TerrainType.Ore => _oreTile,
            TerrainType.Gems => _gemsTile,
            _ => _clearTile
        };
    }

    public TerrainType GetTerrain(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return TerrainType.Water;
        return _grid[x, y];
    }

    public TerrainType GetTerrain(Vector2Int cell) => GetTerrain(cell.x, cell.y);

    public Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);
    }

    public Vector2Int WorldToCell(Vector3 world)
    {
        return new Vector2Int(Mathf.FloorToInt(world.x), Mathf.FloorToInt(world.y));
    }

    public bool IsInBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Height;
    }

    public void RegisterEntity(Vector2Int cell, Entity entity)
    {
        _entityGrid[cell] = entity;
    }

    public void UnregisterEntity(Vector2Int cell)
    {
        _entityGrid.Remove(cell);
    }

    public Entity GetEntityAt(Vector2Int cell)
    {
        _entityGrid.TryGetValue(cell, out Entity entity);
        return entity;
    }
}
