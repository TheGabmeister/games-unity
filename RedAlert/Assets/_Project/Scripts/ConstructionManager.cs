using UnityEngine;
using System.Collections.Generic;

public class ConstructionManager : MonoBehaviour
{
    [SerializeField] private FactionData _alliedFaction;
    [SerializeField] private FactionData _sovietFaction;

    private readonly Dictionary<int, BuildQueue> _structureQueues = new();

    public static ConstructionManager Instance { get; private set; }

    public System.Action OnBuildStateChanged;

    void Awake()
    {
        Instance = this;
    }

    public FactionData GetFactionData(Faction faction)
    {
        return faction == Faction.Allied ? _alliedFaction : _sovietFaction;
    }

    public BuildQueue GetStructureQueue(int playerIndex)
    {
        if (!_structureQueues.TryGetValue(playerIndex, out var queue))
        {
            queue = new BuildQueue();
            _structureQueues[playerIndex] = queue;
        }
        return queue;
    }

    public bool CanBuild(UnitData item, int playerIndex)
    {
        if (item.Prerequisites == null || item.Prerequisites.Length == 0)
            return true;

        var player = PlayerManager.Instance.GetPlayer(playerIndex);
        foreach (var prereq in item.Prerequisites)
        {
            bool found = false;
            foreach (var entity in player.OwnedEntities)
            {
                if (entity == null || entity.IsDead) continue;
                if (entity.UnitData == prereq)
                {
                    found = true;
                    break;
                }
            }
            if (!found) return false;
        }
        return true;
    }

    public bool HasBuildingOfType(UnitData type, int playerIndex)
    {
        var player = PlayerManager.Instance.GetPlayer(playerIndex);
        foreach (var entity in player.OwnedEntities)
        {
            if (entity == null || entity.IsDead) continue;
            if (entity.UnitData == type) return true;
        }
        return false;
    }

    public void StartBuild(UnitData item, int playerIndex)
    {
        var queue = GetStructureQueue(playerIndex);
        if (queue.CurrentItem != null) return;

        queue.CurrentItem = item;
        queue.Progress = 0f;
        queue.TotalCost = item.Cost;
        queue.CostSpent = 0;
        queue.State = BuildState.Building;
        OnBuildStateChanged?.Invoke();
    }

    public void CancelBuild(int playerIndex)
    {
        var queue = GetStructureQueue(playerIndex);
        if (queue.CurrentItem == null) return;

        if (queue.State == BuildState.Ready)
        {
            EconomyManager.Instance.AddCredits(playerIndex, queue.TotalCost);
            PlacementManager.Instance?.ExitPlacement();
        }

        queue.CurrentItem = null;
        queue.Progress = 0f;
        queue.State = BuildState.None;
        OnBuildStateChanged?.Invoke();
    }

    void Update()
    {
        int localPlayer = PlayerManager.Instance.LocalPlayer.PlayerIndex;
        UpdateQueue(localPlayer);
    }

    void UpdateQueue(int playerIndex)
    {
        var queue = GetStructureQueue(playerIndex);
        if (queue.CurrentItem == null || queue.State != BuildState.Building) return;

        float buildTime = queue.CurrentItem.Cost / 1000f * 48f;
        if (buildTime <= 0f) buildTime = 1f;

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
            PlacementManager.Instance?.EnterPlacement(queue.CurrentItem, playerIndex);
            OnBuildStateChanged?.Invoke();
        }
    }

    public int CountBuildingsOfType(UnitData type, int playerIndex)
    {
        var player = PlayerManager.Instance.GetPlayer(playerIndex);
        int count = 0;
        foreach (var entity in player.OwnedEntities)
        {
            if (entity == null || entity.IsDead) continue;
            if (entity.UnitData == type) count++;
        }
        return count;
    }
}

public enum BuildState
{
    None,
    Building,
    Ready
}

public class BuildQueue
{
    public UnitData CurrentItem;
    public float Progress;
    public int TotalCost;
    public int CostSpent;
    public BuildState State = BuildState.None;
}
