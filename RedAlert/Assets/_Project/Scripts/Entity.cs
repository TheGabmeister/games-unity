using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Health))]
public class Entity : MonoBehaviour
{
    [SerializeField] private int _ownerPlayerIndex;
    [SerializeField] private UnitData _unitData;

    private Health _health;
    private List<Vector2Int> _occupiedCells;

    public int OwnerPlayerIndex => _ownerPlayerIndex;
    public UnitData UnitData => _unitData;
    public string EntityName => _unitData != null ? _unitData.DisplayName : "";
    public Vector2Int Cell { get; private set; }
    public Health Health => _health;
    public bool IsDead { get; private set; }
    public bool IsBuilding => _unitData != null && _unitData.Category == UnitCategory.Building;
    public ArmorType Armor => _unitData != null ? _unitData.Armor : ArmorType.None;
    public IReadOnlyList<Vector2Int> OccupiedCells => _occupiedCells;

    void Awake()
    {
        _health = GetComponent<Health>();
    }

    void Start()
    {
        if (_unitData != null && _unitData.Sprite != null
            && TryGetComponent<SpriteRenderer>(out var sr))
        {
            sr.sprite = _unitData.Sprite;
        }

        if (_health != null && _unitData != null)
        {
            _health.Initialize(_unitData.MaxHP);
            _health.OnDeath += Die;
        }

        Cell = MapManager.Instance.WorldToCell(transform.position);
        RegisterCells();

        if (IsBuilding && (_unitData.FootprintX > 1 || _unitData.FootprintY > 1))
        {
            transform.position = new Vector3(
                Cell.x + _unitData.FootprintX * 0.5f,
                Cell.y + _unitData.FootprintY * 0.5f,
                0f);
        }

        var player = PlayerManager.Instance.GetPlayer(_ownerPlayerIndex);
        player.OwnedEntities.Add(this);
    }

    void RegisterCells()
    {
        _occupiedCells = new List<Vector2Int>();

        if (IsBuilding && (_unitData.FootprintX > 1 || _unitData.FootprintY > 1))
        {
            for (int dx = 0; dx < _unitData.FootprintX; dx++)
            {
                for (int dy = 0; dy < _unitData.FootprintY; dy++)
                {
                    var cell = new Vector2Int(Cell.x + dx, Cell.y + dy);
                    MapManager.Instance.RegisterEntity(cell, this);
                    _occupiedCells.Add(cell);
                }
            }
        }
        else
        {
            MapManager.Instance.RegisterEntity(Cell, this);
            _occupiedCells.Add(Cell);
        }
    }

    void UnregisterCells()
    {
        if (_occupiedCells != null)
        {
            foreach (var cell in _occupiedCells)
                MapManager.Instance?.UnregisterEntity(cell);
        }
        else
        {
            MapManager.Instance?.UnregisterEntity(Cell);
        }
    }

    void OnDestroy()
    {
        if (_health != null)
            _health.OnDeath -= Die;

        UnregisterCells();

        var player = PlayerManager.Instance?.GetPlayer(_ownerPlayerIndex);
        player?.OwnedEntities.Remove(this);

        if (_unitData != null && _unitData.StorageCapacity > 0 && EconomyManager.Instance != null)
            EconomyManager.Instance.RecalculateStorage(_ownerPlayerIndex);

        if (IsBuilding && PowerManager.Instance != null)
            PowerManager.Instance.Recalculate(_ownerPlayerIndex);
    }

    public void SetCell(Vector2Int newCell)
    {
        MapManager.Instance.UnregisterEntity(Cell);
        Cell = newCell;
        MapManager.Instance.RegisterEntity(Cell, this);
        if (_occupiedCells != null && _occupiedCells.Count == 1)
            _occupiedCells[0] = Cell;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        if (_health != null)
            _health.TakeDamage(damage);
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;

        if (_unitData != null && _unitData.DeathSound != null && SfxManager.Instance != null)
            SfxManager.Instance.PlaySound(_unitData.DeathSound);

        if (_unitData != null && _unitData.ExplodesOnDeath && _unitData.DeathWarhead != null)
            DamageSystem.ApplySplash(transform.position, 50, _unitData.DeathWarhead, this);

        if (_unitData != null && _unitData.IsCrewedVehicle && _unitData.BailOutUnit != null)
            SpawnBailOut();

        Destroy(gameObject);
    }

    void SpawnBailOut()
    {
        if (_unitData.BailOutUnit.Prefab == null) return;

        Vector2Int spawnCell = FindAdjacentFreeCell();

        Vector3 pos = MapManager.Instance.CellToWorld(spawnCell);
        var go = Instantiate(_unitData.BailOutUnit.Prefab, pos, Quaternion.identity);
        go.name = _unitData.BailOutUnit.DisplayName;

        var entity = go.GetComponent<Entity>();
        entity.InitRuntime(_ownerPlayerIndex, _unitData.BailOutUnit);

        if (go.TryGetComponent<SpriteRenderer>(out var sr))
            sr.color = PlayerManager.Instance.GetPlayer(_ownerPlayerIndex).Color;
    }

    public Vector2Int FindAdjacentFreeCell()
    {
        for (int dx = -1; dx <= _unitData.FootprintX; dx++)
        {
            for (int dy = -1; dy <= _unitData.FootprintY; dy++)
            {
                if (dx >= 0 && dx < _unitData.FootprintX && dy >= 0 && dy < _unitData.FootprintY)
                    continue;

                var candidate = new Vector2Int(Cell.x + dx, Cell.y + dy);
                if (!MapManager.Instance.IsInBounds(candidate)) continue;
                if (MapManager.Instance.GetEntityAt(candidate) != null) continue;
                if (TerrainMovement.GetSpeedMultiplier(LocomotionType.Foot,
                    MapManager.Instance.GetTerrain(candidate)) > 0f)
                    return candidate;
            }
        }

        return Cell;
    }

    public void TransferOwnership(int newPlayerIndex)
    {
        if (newPlayerIndex == _ownerPlayerIndex) return;

        int oldPlayer = _ownerPlayerIndex;

        PlayerManager.Instance.GetPlayer(oldPlayer).OwnedEntities.Remove(this);
        _ownerPlayerIndex = newPlayerIndex;
        PlayerManager.Instance.GetPlayer(newPlayerIndex).OwnedEntities.Add(this);

        if (TryGetComponent<SpriteRenderer>(out var sr))
            sr.color = PlayerManager.Instance.GetPlayer(newPlayerIndex).Color;

        if (IsBuilding)
        {
            PowerManager.Instance?.Recalculate(oldPlayer);
            PowerManager.Instance?.Recalculate(newPlayerIndex);
        }

        if (_unitData != null && _unitData.StorageCapacity > 0)
        {
            EconomyManager.Instance?.RecalculateStorage(oldPlayer);
            EconomyManager.Instance?.RecalculateStorage(newPlayerIndex);
        }
    }

    public void InitRuntime(int ownerIndex, UnitData data)
    {
        _ownerPlayerIndex = ownerIndex;
        _unitData = data;
    }
}
