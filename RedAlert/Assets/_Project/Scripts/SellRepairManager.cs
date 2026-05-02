using UnityEngine;

public class SellRepairManager : MonoBehaviour
{
    private Camera _cam;
    private Entity _repairingBuilding;
    private const float RepairTickInterval = 0.1f;
    private float _repairTimer;

    public bool SellMode { get; private set; }
    public bool RepairMode { get; private set; }

    public static SellRepairManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        _cam = Camera.main;
    }

    public void ToggleSellMode()
    {
        SellMode = !SellMode;
        if (SellMode) RepairMode = false;
    }

    public void ToggleRepairMode()
    {
        RepairMode = !RepairMode;
        if (RepairMode) SellMode = false;
    }

    public void ClearModes()
    {
        SellMode = false;
        RepairMode = false;
        _repairingBuilding = null;
    }

    void Update()
    {
        if (InputManager.Instance == null) return;
        int localPlayer = PlayerManager.Instance.LocalPlayer.PlayerIndex;

        if (SellMode && InputManager.Instance.Select.WasPressedThisFrame())
        {
            TrySell(localPlayer);
        }
        else if (RepairMode && InputManager.Instance.Select.WasPressedThisFrame())
        {
            TryStartRepair(localPlayer);
        }

        if (_repairingBuilding != null)
            UpdateRepair(localPlayer);
    }

    void TrySell(int playerIndex)
    {
        Vector3 world = _cam.ScreenToWorldPoint(InputManager.Instance.MousePosition);
        Vector2Int cell = MapManager.Instance.WorldToCell(world);
        Entity entity = MapManager.Instance.GetEntityAt(cell);

        if (entity == null || entity.IsDead) return;
        if (entity.OwnerPlayerIndex != playerIndex) return;
        if (!entity.IsBuilding) return;

        float hpRatio = entity.Health.Ratio;
        int refund = Mathf.RoundToInt(entity.UnitData.Cost * 0.5f * hpRatio);
        EconomyManager.Instance.AddCredits(playerIndex, refund);

        if (entity.UnitData.IsCrewedVehicle && entity.UnitData.BailOutUnit != null)
        {
            Vector2Int spawnCell = entity.FindAdjacentFreeCell();
            var bailOut = entity.UnitData.BailOutUnit;
            if (bailOut.Prefab != null)
            {
                Vector3 pos = MapManager.Instance.CellToWorld(spawnCell);
                var go = Object.Instantiate(bailOut.Prefab, pos, Quaternion.identity);
                go.name = bailOut.DisplayName;
                var spawnedEntity = go.GetComponent<Entity>();
                spawnedEntity.InitRuntime(playerIndex, bailOut);
                if (go.TryGetComponent<SpriteRenderer>(out var sr))
                    sr.color = PlayerManager.Instance.GetPlayer(playerIndex).Color;
            }
        }

        entity.Die();
        SellMode = false;
    }

    void TryStartRepair(int playerIndex)
    {
        Vector3 world = _cam.ScreenToWorldPoint(InputManager.Instance.MousePosition);
        Vector2Int cell = MapManager.Instance.WorldToCell(world);
        Entity entity = MapManager.Instance.GetEntityAt(cell);

        if (entity == null || entity.IsDead) return;
        if (entity.OwnerPlayerIndex != playerIndex) return;
        if (!entity.IsBuilding) return;
        if (entity.Health.Ratio >= 1f) return;

        if (_repairingBuilding == entity)
        {
            _repairingBuilding = null;
            return;
        }

        _repairingBuilding = entity;
        RepairMode = false;
    }

    void UpdateRepair(int playerIndex)
    {
        if (_repairingBuilding == null || _repairingBuilding.IsDead)
        {
            _repairingBuilding = null;
            return;
        }

        if (_repairingBuilding.Health.Ratio >= 1f)
        {
            _repairingBuilding = null;
            return;
        }

        _repairTimer += Time.deltaTime;
        if (_repairTimer < RepairTickInterval) return;
        _repairTimer = 0f;

        float totalRepairCost = _repairingBuilding.UnitData.Cost * 0.2f;
        float costPerHP = totalRepairCost / _repairingBuilding.Health.MaxHP;
        int healAmount = 7;
        int cost = Mathf.Max(1, Mathf.RoundToInt(costPerHP * healAmount));

        if (!EconomyManager.Instance.SpendCredits(playerIndex, cost))
            return;

        _repairingBuilding.Health.Heal(healAmount);
    }
}
