using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class FogManager : MonoBehaviour
{
    [SerializeField] private TileBase _shroudTile;

    private FogState[] _fog;
    private FogState[] _prevFog;
    private int _width;
    private int _height;
    private Tilemap _tilemap;
    private int _frameCounter;
    private const int UpdateInterval = 4;

    private static readonly Dictionary<int, Vector2Int[]> CircleLUT = new();

    public static FogManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        var map = MapManager.Instance;
        if (map == null) return;

        _width = map.Width;
        _height = map.Height;

        _fog = new FogState[_width * _height];
        _prevFog = new FogState[_width * _height];

        CreateTilemap();
        InitTilemap();
        RefreshVision();
        SyncTilemap();
    }

    void CreateTilemap()
    {
        var grid = FindFirstObjectByType<Grid>();
        if (grid == null) return;

        var fogGO = new GameObject("FogTilemap");
        fogGO.transform.SetParent(grid.transform);
        fogGO.transform.localPosition = Vector3.zero;

        _tilemap = fogGO.AddComponent<Tilemap>();
        var renderer = fogGO.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = 100;
    }

    void InitTilemap()
    {
        if (_tilemap == null || _shroudTile == null) return;

        for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
                _tilemap.SetTile(new Vector3Int(x, y, 0), _shroudTile);
    }

    void Update()
    {
        _frameCounter++;
        if (_frameCounter < UpdateInterval) return;
        _frameCounter = 0;

        RefreshVision();
        SyncTilemap();
    }

    void RefreshVision()
    {
        int localPlayer = PlayerManager.Instance.LocalPlayer.PlayerIndex;
        var player = PlayerManager.Instance.GetPlayer(localPlayer);

        for (int i = player.OwnedEntities.Count - 1; i >= 0; i--)
        {
            var entity = player.OwnedEntities[i];
            if (entity == null || entity.IsDead) continue;
            if (entity.UnitData == null) continue;

            int sight = Mathf.RoundToInt(entity.UnitData.SightRange);
            if (sight <= 0) continue;

            if (entity.IsBuilding && entity.OccupiedCells != null)
            {
                foreach (var cell in entity.OccupiedCells)
                    RevealCircle(cell, sight);
            }
            else
            {
                RevealCircle(entity.Cell, sight);
            }
        }

        ApplyGapGenerators(localPlayer);
    }

    void RevealCircle(Vector2Int center, int radius)
    {
        var offsets = GetCircleOffsets(radius);
        foreach (var offset in offsets)
        {
            int x = center.x + offset.x;
            int y = center.y + offset.y;
            if (x < 0 || x >= _width || y < 0 || y >= _height) continue;
            _fog[y * _width + x] = FogState.Visible;
        }
    }

    void ApplyGapGenerators(int localPlayer)
    {
        for (int p = 0; p < PlayerManager.Instance.PlayerCount; p++)
        {
            if (!PlayerManager.Instance.AreEnemies(localPlayer, p)) continue;

            var enemyPlayer = PlayerManager.Instance.GetPlayer(p);
            foreach (var entity in enemyPlayer.OwnedEntities)
            {
                if (entity == null || entity.IsDead) continue;
                if (entity.UnitData == null) continue;
                if (entity.UnitData.DisplayName != "Gap Generator") continue;
                if (entity.UnitData.RequiresPower && PowerManager.Instance != null
                    && PowerManager.Instance.IsLowPower(p)) continue;

                var offsets = GetCircleOffsets(10);
                if (entity.OccupiedCells != null)
                {
                    foreach (var cell in entity.OccupiedCells)
                    {
                        foreach (var offset in offsets)
                        {
                            int x = cell.x + offset.x;
                            int y = cell.y + offset.y;
                            if (x < 0 || x >= _width || y < 0 || y >= _height) continue;
                            _fog[y * _width + x] = FogState.Shroud;
                        }
                    }
                }
            }
        }
    }

    void SyncTilemap()
    {
        if (_tilemap == null) return;

        for (int i = 0; i < _fog.Length; i++)
        {
            if (_fog[i] == _prevFog[i]) continue;

            int x = i % _width;
            int y = i / _width;
            var pos = new Vector3Int(x, y, 0);

            _tilemap.SetTile(pos, _fog[i] == FogState.Shroud ? _shroudTile : null);
            _prevFog[i] = _fog[i];
        }
    }

    public FogState GetFogState(Vector2Int cell)
    {
        if (cell.x < 0 || cell.x >= _width || cell.y < 0 || cell.y >= _height)
            return FogState.Shroud;
        return _fog[cell.y * _width + cell.x];
    }

    public bool IsExplored(Vector2Int cell)
    {
        return GetFogState(cell) == FogState.Visible;
    }

    public void RevealCell(Vector2Int cell)
    {
        if (cell.x < 0 || cell.x >= _width || cell.y < 0 || cell.y >= _height) return;
        _fog[cell.y * _width + cell.x] = FogState.Visible;
    }

    static Vector2Int[] GetCircleOffsets(int radius)
    {
        if (CircleLUT.TryGetValue(radius, out var cached))
            return cached;

        var offsets = new List<Vector2Int>();
        int r2 = radius * radius;
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx * dx + dy * dy <= r2)
                    offsets.Add(new Vector2Int(dx, dy));
            }
        }

        var result = offsets.ToArray();
        CircleLUT[radius] = result;
        return result;
    }
}
