using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] private int _ownerPlayerIndex;
    [SerializeField] private UnitData _unitData;

    private HealthBar _healthBar;

    public int OwnerPlayerIndex => _ownerPlayerIndex;
    public UnitData UnitData => _unitData;
    public string EntityName => _unitData != null ? _unitData.DisplayName : "";
    public Vector2Int Cell { get; private set; }
    public HealthBar HealthBar => _healthBar;
    public bool IsDead { get; private set; }

    void Awake()
    {
        _healthBar = GetComponentInChildren<HealthBar>(true);
    }

    void Start()
    {
        if (_unitData != null && _unitData.Sprite != null
            && TryGetComponent<SpriteRenderer>(out var sr))
        {
            sr.sprite = _unitData.Sprite;
        }

        if (_healthBar != null && _unitData != null)
        {
            _healthBar.Initialize(_unitData.MaxHP);
            _healthBar.OnDeath += Die;
        }

        Cell = MapManager.Instance.WorldToCell(transform.position);
        MapManager.Instance.RegisterEntity(Cell, this);

        var player = PlayerManager.Instance.GetPlayer(_ownerPlayerIndex);
        player.OwnedEntities.Add(this);
    }

    void OnDestroy()
    {
        if (_healthBar != null)
            _healthBar.OnDeath -= Die;

        MapManager.Instance?.UnregisterEntity(Cell);

        var player = PlayerManager.Instance?.GetPlayer(_ownerPlayerIndex);
        player?.OwnedEntities.Remove(this);
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
        if (_healthBar != null)
            _healthBar.TakeDamage(damage);
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
        var go = new GameObject(_unitData.BailOutUnit.DisplayName);
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = _unitData.BailOutUnit.Sprite;

        var entity = go.AddComponent<Entity>();
        entity.InitRuntime(_ownerPlayerIndex, _unitData.BailOutUnit);

        go.AddComponent<Mover>();
        go.AddComponent<Attacker>();
    }

    public void InitRuntime(int ownerIndex, UnitData data)
    {
        _ownerPlayerIndex = ownerIndex;
        _unitData = data;
    }
}
