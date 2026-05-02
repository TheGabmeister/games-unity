using UnityEngine;

[RequireComponent(typeof(Health))]
public class Entity : MonoBehaviour
{
    [SerializeField] private int _ownerPlayerIndex;
    [SerializeField] private UnitData _unitData;

    private Health _health;

    public int OwnerPlayerIndex => _ownerPlayerIndex;
    public UnitData UnitData => _unitData;
    public string EntityName => _unitData != null ? _unitData.DisplayName : "";
    public Vector2Int Cell { get; private set; }
    public Health Health => _health;
    public bool IsDead { get; private set; }
    public bool IsBuilding => _unitData != null && _unitData.Category == UnitCategory.Building;
    public ArmorType Armor => _unitData != null ? _unitData.Armor : ArmorType.None;

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
        MapManager.Instance.RegisterEntity(Cell, this);

        var player = PlayerManager.Instance.GetPlayer(_ownerPlayerIndex);
        player.OwnedEntities.Add(this);
    }

    void OnDestroy()
    {
        if (_health != null)
            _health.OnDeath -= Die;

        MapManager.Instance?.UnregisterEntity(Cell);

        var player = PlayerManager.Instance?.GetPlayer(_ownerPlayerIndex);
        player?.OwnedEntities.Remove(this);

        if (_unitData != null && _unitData.StorageCapacity > 0 && EconomyManager.Instance != null)
            EconomyManager.Instance.RecalculateStorage(_ownerPlayerIndex);
    }

    public void SetCell(Vector2Int newCell)
    {
        MapManager.Instance.UnregisterEntity(Cell);
        Cell = newCell;
        MapManager.Instance.RegisterEntity(Cell, this);
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

        Vector2Int spawnCell = Cell;
        bool found = false;

        for (int dx = -1; dx <= 1 && !found; dx++)
        {
            for (int dy = -1; dy <= 1 && !found; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                var candidate = new Vector2Int(Cell.x + dx, Cell.y + dy);
                if (MapManager.Instance.GetEntityAt(candidate) == null &&
                    TerrainMovement.GetSpeedMultiplier(LocomotionType.Foot,
                        MapManager.Instance.GetTerrain(candidate)) > 0f)
                {
                    spawnCell = candidate;
                    found = true;
                }
            }
        }

        Vector3 pos = MapManager.Instance.CellToWorld(spawnCell);
        var go = Instantiate(_unitData.BailOutUnit.Prefab, pos, Quaternion.identity);
        go.name = _unitData.BailOutUnit.DisplayName;

        var entity = go.GetComponent<Entity>();
        entity.InitRuntime(_ownerPlayerIndex, _unitData.BailOutUnit);

        if (go.TryGetComponent<SpriteRenderer>(out var sr))
            sr.color = PlayerManager.Instance.GetPlayer(_ownerPlayerIndex).Color;
    }

    public void InitRuntime(int ownerIndex, UnitData data)
    {
        _ownerPlayerIndex = ownerIndex;
        _unitData = data;
    }
}
