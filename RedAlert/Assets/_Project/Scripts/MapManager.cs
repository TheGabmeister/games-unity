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

    [Header("Tilemaps")]
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private Tilemap _oreOverlayTilemap;

    [Header("Ore Overlay")]
    [SerializeField] private TileBase[] _oreDensityTiles;
    [SerializeField] private TileBase[] _gemDensityTiles;

    private TerrainType[,] _grid;
    private int[,] _oreDensity;
    private TerrainType[,] _oreType;
    private float _regrowthTimer;
    private const float RegrowthInterval = 120f;
    private const int MaxDensity = 4;
    private const int OreValuePerBail = 25;
    private const int GemValuePerBail = 50;

    private readonly Dictionary<Vector2Int, Entity> _entityGrid = new();

    public int Width => _mapData != null ? _mapData.Width : 0;
    public int Height => _mapData != null ? _mapData.Height : 0;

    public static MapManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        if (_tilemap == null)
            _tilemap = FindFirstObjectByType<Tilemap>();
        if (_tilemap == null || _mapData == null) return;
        BuildGrid();
        RenderMap();
        RenderOreOverlay();
    }

    void Update()
    {
        _regrowthTimer += Time.deltaTime;
        if (_regrowthTimer >= RegrowthInterval)
        {
            _regrowthTimer = 0f;
            GrowOre();
        }
    }

    void BuildGrid()
    {
        _grid = new TerrainType[_mapData.Width, _mapData.Height];
        _oreDensity = new int[_mapData.Width, _mapData.Height];
        _oreType = new TerrainType[_mapData.Width, _mapData.Height];

        for (int x = 0; x < _mapData.Width; x++)
        {
            for (int y = 0; y < _mapData.Height; y++)
            {
                var terrain = _mapData.GetCell(x, y);
                if (terrain == TerrainType.Ore || terrain == TerrainType.Gems)
                {
                    _grid[x, y] = terrain;
                    _oreDensity[x, y] = MaxDensity;
                    _oreType[x, y] = terrain;
                }
                else
                {
                    _grid[x, y] = terrain;
                    _oreDensity[x, y] = 0;
                    _oreType[x, y] = TerrainType.Clear;
                }
            }
        }
    }

    void RenderMap()
    {
        _tilemap.ClearAllTiles();
        for (int x = 0; x < _mapData.Width; x++)
        {
            for (int y = 0; y < _mapData.Height; y++)
            {
                var terrain = _grid[x, y];
                TileBase tile;
                if (terrain == TerrainType.Ore || terrain == TerrainType.Gems)
                    tile = GetTileForTerrain(TerrainType.Clear);
                else
                    tile = GetTileForTerrain(terrain);

                if (tile != null)
                    _tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    void RenderOreOverlay()
    {
        if (_oreOverlayTilemap == null) return;
        _oreOverlayTilemap.ClearAllTiles();

        for (int x = 0; x < _mapData.Width; x++)
            for (int y = 0; y < _mapData.Height; y++)
                UpdateOreTile(x, y);
    }

    void UpdateOreTile(int x, int y)
    {
        if (_oreOverlayTilemap == null) return;

        var pos = new Vector3Int(x, y, 0);
        int density = _oreDensity[x, y];

        if (density <= 0)
        {
            _oreOverlayTilemap.SetTile(pos, null);
            return;
        }

        int tileIndex = density - 1;
        if (_oreType[x, y] == TerrainType.Gems && _gemDensityTiles != null && tileIndex < _gemDensityTiles.Length)
            _oreOverlayTilemap.SetTile(pos, _gemDensityTiles[tileIndex]);
        else if (_oreType[x, y] == TerrainType.Ore && _oreDensityTiles != null && tileIndex < _oreDensityTiles.Length)
            _oreOverlayTilemap.SetTile(pos, _oreDensityTiles[tileIndex]);
    }

    void GrowOre()
    {
        var growCandidates = new List<Vector2Int>();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (_oreType[x, y] != TerrainType.Ore) continue;

                if (_oreDensity[x, y] > 0 && _oreDensity[x, y] < MaxDensity)
                {
                    _oreDensity[x, y]++;
                    _grid[x, y] = TerrainType.Ore;
                    UpdateOreTile(x, y);
                }

                if (_oreDensity[x, y] >= MaxDensity)
                    growCandidates.Add(new Vector2Int(x, y));
            }
        }

        foreach (var cell in growCandidates)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = cell.x + dx;
                    int ny = cell.y + dy;
                    if (!IsInBounds(new Vector2Int(nx, ny))) continue;
                    if (_oreDensity[nx, ny] > 0) continue;

                    var baseTerrain = _oreType[nx, ny] == TerrainType.Clear
                        ? _grid[nx, ny] : _oreType[nx, ny];
                    if (baseTerrain == TerrainType.Water || baseTerrain == TerrainType.Road) continue;

                    _oreDensity[nx, ny] = 1;
                    _oreType[nx, ny] = TerrainType.Ore;
                    _grid[nx, ny] = TerrainType.Ore;
                    UpdateOreTile(nx, ny);
                }
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

    public int GetOreDensity(Vector2Int cell)
    {
        if (!IsInBounds(cell)) return 0;
        return _oreDensity[cell.x, cell.y];
    }

    public TerrainType GetOreType(Vector2Int cell)
    {
        if (!IsInBounds(cell)) return TerrainType.Clear;
        return _oreType[cell.x, cell.y];
    }

    public int HarvestBail(Vector2Int cell)
    {
        if (!IsInBounds(cell)) return 0;
        if (_oreDensity[cell.x, cell.y] <= 0) return 0;

        int value = _oreType[cell.x, cell.y] == TerrainType.Gems ? GemValuePerBail : OreValuePerBail;

        _oreDensity[cell.x, cell.y]--;
        if (_oreDensity[cell.x, cell.y] <= 0)
        {
            _grid[cell.x, cell.y] = TerrainType.Clear;
            if (_oreType[cell.x, cell.y] == TerrainType.Gems)
                _oreType[cell.x, cell.y] = TerrainType.Clear;
        }

        UpdateOreTile(cell.x, cell.y);
        return value;
    }

    public bool HasOre(Vector2Int cell)
    {
        if (!IsInBounds(cell)) return false;
        return _oreDensity[cell.x, cell.y] > 0;
    }

    public Vector2Int? FindNearestOre(Vector2Int from, int radius)
    {
        float bestDist = float.MaxValue;
        Vector2Int? best = null;

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                int x = from.x + dx;
                int y = from.y + dy;
                var cell = new Vector2Int(x, y);
                if (!IsInBounds(cell)) continue;
                if (_oreDensity[x, y] <= 0) continue;

                float dist = Mathf.Abs(dx) + Mathf.Abs(dy);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = cell;
                }
            }
        }

        return best;
    }

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
