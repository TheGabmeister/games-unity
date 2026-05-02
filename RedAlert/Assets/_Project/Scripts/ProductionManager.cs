using UnityEngine;
using System.Collections.Generic;

public class ProductionManager : MonoBehaviour
{
    private readonly Dictionary<UnitCategory, UnitBuildQueue> _queues = new();

    public static ProductionManager Instance { get; private set; }
    public System.Action OnProductionStateChanged;

    void Awake()
    {
        Instance = this;
    }

    public UnitBuildQueue GetQueue(UnitCategory category)
    {
        if (!_queues.TryGetValue(category, out var queue))
        {
            queue = new UnitBuildQueue { Category = category };
            _queues[category] = queue;
        }
        return queue;
    }

    public void StartProduction(UnitData unit, int playerIndex)
    {
        var queue = GetQueue(unit.Category);
        if (queue.CurrentItem != null) return;

        queue.CurrentItem = unit;
        queue.PlayerIndex = playerIndex;
        queue.Progress = 0f;
        queue.TotalCost = unit.Cost;
        queue.CostSpent = 0;
        queue.State = BuildState.Building;
        OnProductionStateChanged?.Invoke();
    }

    public void CancelProduction(UnitCategory category, int playerIndex)
    {
        var queue = GetQueue(category);
        if (queue.CurrentItem == null) return;

        queue.CurrentItem = null;
        queue.Progress = 0f;
        queue.State = BuildState.None;
        OnProductionStateChanged?.Invoke();
    }

    void Update()
    {
        int localPlayer = PlayerManager.Instance.LocalPlayer.PlayerIndex;
        foreach (var kvp in _queues)
            UpdateQueue(kvp.Value, localPlayer);
    }

    void UpdateQueue(UnitBuildQueue queue, int playerIndex)
    {
        if (queue.CurrentItem == null || queue.State != BuildState.Building) return;
        if (queue.PlayerIndex != playerIndex) return;

        int factoryCount = CountFactories(queue.CurrentItem.Category, playerIndex);
        if (factoryCount <= 0) return;

        float buildTime = queue.CurrentItem.Cost / 1000f * 48f;
        if (buildTime <= 0f) buildTime = 1f;
        buildTime /= factoryCount;

        float creditRate = queue.TotalCost / buildTime;
        int creditThisFrame = Mathf.CeilToInt(creditRate * Time.deltaTime);
        int remaining = queue.TotalCost - queue.CostSpent;
        creditThisFrame = Mathf.Min(creditThisFrame, remaining);

        if (creditThisFrame > 0 && !EconomyManager.Instance.SpendCredits(playerIndex, creditThisFrame))
            return;

        queue.CostSpent += creditThisFrame;
        queue.Progress = (float)queue.CostSpent / queue.TotalCost;

        if (queue.CostSpent >= queue.TotalCost)
        {
            queue.Progress = 1f;
            queue.State = BuildState.Ready;
            TrySpawnUnit(queue, playerIndex);
        }
    }

    void TrySpawnUnit(UnitBuildQueue queue, int playerIndex)
    {
        var factory = FindPrimaryFactory(queue.CurrentItem.Category, playerIndex);
        if (factory == null) return;

        Vector2Int exitCell = factory.Cell + factory.UnitData.ExitCellOffset;
        if (MapManager.Instance.GetEntityAt(exitCell) != null)
            return;

        SpawnUnit(queue.CurrentItem, exitCell, playerIndex);

        queue.CurrentItem = null;
        queue.Progress = 0f;
        queue.State = BuildState.None;
        OnProductionStateChanged?.Invoke();
    }

    void SpawnUnit(UnitData unitData, Vector2Int cell, int playerIndex)
    {
        if (unitData.Prefab == null) return;

        Vector3 pos = MapManager.Instance.CellToWorld(cell);
        var go = Object.Instantiate(unitData.Prefab, pos, Quaternion.identity);
        go.name = $"{unitData.DisplayName} (P{playerIndex})";

        var entity = go.GetComponent<Entity>();
        entity.InitRuntime(playerIndex, unitData);

        if (go.TryGetComponent<SpriteRenderer>(out var sr))
            sr.color = PlayerManager.Instance.GetPlayer(playerIndex).Color;
    }

    void LateUpdate()
    {
        int localPlayer = PlayerManager.Instance.LocalPlayer.PlayerIndex;
        foreach (var kvp in _queues)
        {
            var queue = kvp.Value;
            if (queue.State == BuildState.Ready && queue.CurrentItem != null)
                TrySpawnUnit(queue, localPlayer);
        }
    }

    int CountFactories(UnitCategory unitCategory, int playerIndex)
    {
        var player = PlayerManager.Instance.GetPlayer(playerIndex);
        int count = 0;
        foreach (var entity in player.OwnedEntities)
        {
            if (entity == null || entity.IsDead) continue;
            if (entity.UnitData == null) continue;
            if (entity.UnitData.ProducesCategory == unitCategory) count++;
        }
        return count;
    }

    Entity FindPrimaryFactory(UnitCategory unitCategory, int playerIndex)
    {
        var player = PlayerManager.Instance.GetPlayer(playerIndex);
        Entity first = null;
        foreach (var entity in player.OwnedEntities)
        {
            if (entity == null || entity.IsDead) continue;
            if (entity.UnitData == null) continue;
            if (entity.UnitData.ProducesCategory != unitCategory) continue;

            var primary = entity.GetComponent<PrimaryBuilding>();
            if (primary != null && primary.IsPrimary) return entity;
            if (first == null) first = entity;
        }
        return first;
    }
}

public class UnitBuildQueue
{
    public UnitCategory Category;
    public UnitData CurrentItem;
    public int PlayerIndex;
    public float Progress;
    public int TotalCost;
    public int CostSpent;
    public BuildState State = BuildState.None;
}
