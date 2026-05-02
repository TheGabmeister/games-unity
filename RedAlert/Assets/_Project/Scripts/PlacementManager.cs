using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    private UnitData _placingItem;
    private int _placingPlayer;
    private GameObject _ghostObj;
    private SpriteRenderer _ghostRenderer;
    private Camera _cam;
    private bool _isPlacing;

    private const int CYRadius = 16;
    private const int AdjacencyRange = 2;

    private static readonly Color ValidColor = new(0.2f, 1f, 0.2f, 0.5f);
    private static readonly Color InvalidColor = new(1f, 0.2f, 0.2f, 0.5f);

    public static PlacementManager Instance { get; private set; }
    public bool IsPlacing => _isPlacing;

    void Awake()
    {
        Instance = this;
        _cam = Camera.main;
    }

    public void EnterPlacement(UnitData item, int playerIndex)
    {
        _placingItem = item;
        _placingPlayer = playerIndex;
        _isPlacing = true;

        if (_ghostObj == null)
        {
            _ghostObj = new GameObject("PlacementGhost");
            _ghostRenderer = _ghostObj.AddComponent<SpriteRenderer>();
            _ghostRenderer.sortingOrder = 100;
        }

        _ghostRenderer.sprite = item.Sprite;
        _ghostObj.SetActive(true);
    }

    public void ExitPlacement()
    {
        _isPlacing = false;
        _placingItem = null;
        if (_ghostObj != null)
            _ghostObj.SetActive(false);
    }

    void Update()
    {
        if (!_isPlacing || _placingItem == null) return;
        if (InputManager.Instance == null) return;

        Vector3 mouseWorld = _cam.ScreenToWorldPoint(InputManager.Instance.MousePosition);
        Vector2Int cell = MapManager.Instance.WorldToCell(mouseWorld);

        Vector3 ghostPos = MapManager.Instance.CellToWorld(cell);
        if (_placingItem.FootprintX > 1 || _placingItem.FootprintY > 1)
        {
            ghostPos.x += (_placingItem.FootprintX - 1) * 0.5f;
            ghostPos.y += (_placingItem.FootprintY - 1) * 0.5f;
        }
        _ghostObj.transform.position = ghostPos;

        bool valid = IsValidPlacement(cell);
        _ghostRenderer.color = valid ? ValidColor : InvalidColor;

        if (InputManager.Instance.Select.WasPressedThisFrame() && valid)
        {
            PlaceBuilding(cell);
        }
        else if (InputManager.Instance.Command.WasPressedThisFrame())
        {
            ExitPlacement();
        }
    }

    bool IsValidPlacement(Vector2Int origin)
    {
        for (int dx = 0; dx < _placingItem.FootprintX; dx++)
        {
            for (int dy = 0; dy < _placingItem.FootprintY; dy++)
            {
                var cell = new Vector2Int(origin.x + dx, origin.y + dy);
                if (!MapManager.Instance.IsInBounds(cell)) return false;

                var terrain = MapManager.Instance.GetTerrain(cell);
                if (terrain == TerrainType.Water) return false;

                var occupant = MapManager.Instance.GetEntityAt(cell);
                if (occupant != null) return false;
            }
        }

        if (!IsWithinCYRadius(origin)) return false;
        if (!IsAdjacentToFriendlyBuilding(origin)) return false;

        return true;
    }

    bool IsWithinCYRadius(Vector2Int origin)
    {
        var player = PlayerManager.Instance.GetPlayer(_placingPlayer);
        foreach (var entity in player.OwnedEntities)
        {
            if (entity == null || entity.IsDead) continue;
            if (entity.UnitData == null) continue;
            if (entity.UnitData.DisplayName != "Construction Yard") continue;

            float dist = Vector2Int.Distance(origin, entity.Cell);
            if (dist <= CYRadius) return true;
        }
        return false;
    }

    bool IsAdjacentToFriendlyBuilding(Vector2Int origin)
    {
        int range = AdjacencyRange;

        for (int dx = -range; dx < _placingItem.FootprintX + range; dx++)
        {
            for (int dy = -range; dy < _placingItem.FootprintY + range; dy++)
            {
                if (dx >= 0 && dx < _placingItem.FootprintX && dy >= 0 && dy < _placingItem.FootprintY)
                    continue;

                var checkCell = new Vector2Int(origin.x + dx, origin.y + dy);
                if (!MapManager.Instance.IsInBounds(checkCell)) continue;

                var occupant = MapManager.Instance.GetEntityAt(checkCell);
                if (occupant == null) continue;
                if (occupant.OwnerPlayerIndex != _placingPlayer) continue;
                if (occupant.IsBuilding) return true;
            }
        }
        return false;
    }

    void PlaceBuilding(Vector2Int cell)
    {
        if (_placingItem.Prefab == null)
        {
            ExitPlacement();
            return;
        }

        Vector3 pos = new Vector3(
            cell.x + _placingItem.FootprintX * 0.5f,
            cell.y + _placingItem.FootprintY * 0.5f,
            0f);
        var go = Object.Instantiate(_placingItem.Prefab, pos, Quaternion.identity);
        go.name = $"{_placingItem.DisplayName} (P{_placingPlayer})";

        var entity = go.GetComponent<Entity>();
        entity.InitRuntime(_placingPlayer, _placingItem);

        if (go.TryGetComponent<SpriteRenderer>(out var sr))
            sr.color = PlayerManager.Instance.GetPlayer(_placingPlayer).Color;

        ExitPlacement();

        var queue = ConstructionManager.Instance.GetStructureQueue(_placingPlayer);
        queue.CurrentItem = null;
        queue.Progress = 0f;
        queue.State = BuildState.None;
        ConstructionManager.Instance.OnBuildStateChanged?.Invoke();

        PowerManager.Instance?.Recalculate(_placingPlayer);
        EconomyManager.Instance?.RecalculateStorage(_placingPlayer);
    }
}
