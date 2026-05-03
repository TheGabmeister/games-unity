using UnityEngine;

public class CommandManager : MonoBehaviour
{
    private Camera _cam;

    public static CommandManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        _cam = Camera.main;
    }

    void Update()
    {
        if (InputManager.Instance == null) return;

        if (InputManager.Instance.Command.WasPressedThisFrame())
        {
            if (InputManager.Instance.IsCtrlHeld)
                HandleForceFireCommand();
            else if (InputManager.Instance.IsAltHeld)
                HandleForceMoveCommand();
            else if (InputManager.Instance.IsAttackMoveHeld)
                HandleAttackMoveCommand();
            else
                HandleContextCommand();
        }

        if (InputManager.Instance.Stop.WasPressedThisFrame())
            HandleStop();

        if (InputManager.Instance.Guard.WasPressedThisFrame())
            HandleGuard();

        if (InputManager.Instance.Scatter.WasPressedThisFrame())
            HandleScatter();
    }

    Vector2Int GetTargetCell()
    {
        Vector3 world = _cam.ScreenToWorldPoint(InputManager.Instance.MousePosition);
        return MapManager.Instance.WorldToCell(world);
    }

    void HandleContextCommand()
    {
        var selected = SelectionManager.Instance.Selected;
        if (selected.Count == 0) return;

        Vector2Int targetCell = GetTargetCell();
        Entity targetEntity = MapManager.Instance.GetEntityAt(targetCell);

        if (targetEntity != null && !targetEntity.IsDead)
        {
            int localPlayer = PlayerManager.Instance.LocalPlayer.PlayerIndex;
            bool isEnemy = PlayerManager.Instance.AreEnemies(localPlayer, targetEntity.OwnerPlayerIndex);

            if (isEnemy && targetEntity.IsBuilding)
            {
                foreach (var selectable in selected)
                {
                    var engineer = selectable.GetComponent<Engineer>();
                    if (engineer != null)
                    {
                        engineer.SendToBuilding(targetEntity);
                        continue;
                    }

                    var spy = selectable.GetComponent<Spy>();
                    if (spy != null)
                    {
                        spy.Infiltrate(targetEntity);
                        continue;
                    }

                    var c4 = selectable.GetComponent<C4Placer>();
                    if (c4 != null)
                    {
                        c4.PlantC4(targetEntity);
                        continue;
                    }

                    var attacker = selectable.GetComponent<Attacker>();
                    if (attacker != null)
                        attacker.AttackTarget(targetEntity);
                    else
                    {
                        var mover = selectable.GetComponent<Mover>();
                        if (mover != null)
                            mover.MoveTo(targetCell);
                    }
                }
                return;
            }

            if (isEnemy)
            {
                foreach (var selectable in selected)
                {
                    var attacker = selectable.GetComponent<Attacker>();
                    if (attacker != null)
                        attacker.AttackTarget(targetEntity);
                    else
                    {
                        var mover = selectable.GetComponent<Mover>();
                        if (mover != null)
                            mover.MoveTo(targetCell);
                    }
                }
                return;
            }

            if (targetEntity.IsBuilding)
            {
                bool handled = false;
                foreach (var selectable in selected)
                {
                    var engineer = selectable.GetComponent<Engineer>();
                    if (engineer != null)
                    {
                        engineer.SendToBuilding(targetEntity);
                        handled = true;
                    }
                }
                if (handled) return;
            }

            var refinery = targetEntity.GetComponent<Refinery>();
            if (refinery != null)
            {
                foreach (var selectable in selected)
                {
                    var harvester = selectable.GetComponent<Harvester>();
                    if (harvester != null)
                        harvester.SendToRefinery(refinery);
                }
                return;
            }
        }

        bool hasOre = MapManager.Instance.HasOre(targetCell);

        foreach (var selectable in selected)
        {
            var harvester = selectable.GetComponent<Harvester>();
            if (harvester != null && hasOre)
            {
                harvester.SendToOre(targetCell);
                continue;
            }

            var attacker = selectable.GetComponent<Attacker>();
            if (attacker != null)
                attacker.ClearOrders();

            var mover = selectable.GetComponent<Mover>();
            if (mover != null)
                mover.MoveTo(targetCell);
        }
    }

    void HandleForceFireCommand()
    {
        var selected = SelectionManager.Instance.Selected;
        if (selected.Count == 0) return;

        Vector2Int targetCell = GetTargetCell();

        foreach (var selectable in selected)
        {
            var attacker = selectable.GetComponent<Attacker>();
            if (attacker != null)
                attacker.ForceFireAt(targetCell);
        }
    }

    void HandleForceMoveCommand()
    {
        var selected = SelectionManager.Instance.Selected;
        if (selected.Count == 0) return;

        Vector2Int targetCell = GetTargetCell();

        foreach (var selectable in selected)
        {
            var attacker = selectable.GetComponent<Attacker>();
            if (attacker != null)
                attacker.ClearOrders();

            var mover = selectable.GetComponent<Mover>();
            if (mover != null)
                mover.MoveTo(targetCell);
        }
    }

    void HandleAttackMoveCommand()
    {
        var selected = SelectionManager.Instance.Selected;
        if (selected.Count == 0) return;

        Vector2Int targetCell = GetTargetCell();

        foreach (var selectable in selected)
        {
            var attacker = selectable.GetComponent<Attacker>();
            if (attacker != null)
                attacker.AttackMoveTo(targetCell);
            else
            {
                var mover = selectable.GetComponent<Mover>();
                if (mover != null)
                    mover.MoveTo(targetCell);
            }
        }
    }

    void HandleStop()
    {
        var selected = SelectionManager.Instance.Selected;
        if (selected.Count == 0) return;

        foreach (var selectable in selected)
        {
            var mover = selectable.GetComponent<Mover>();
            if (mover != null)
                mover.Stop();

            var attacker = selectable.GetComponent<Attacker>();
            if (attacker != null)
                attacker.ClearOrders();

            var harvester = selectable.GetComponent<Harvester>();
            if (harvester != null)
                harvester.Stop();
        }
    }

    void HandleGuard()
    {
        var selected = SelectionManager.Instance.Selected;
        if (selected.Count == 0) return;

        foreach (var selectable in selected)
        {
            var attacker = selectable.GetComponent<Attacker>();
            if (attacker != null)
                attacker.SetGuard();
        }
    }

    void HandleScatter()
    {
        var selected = SelectionManager.Instance.Selected;
        if (selected.Count == 0) return;

        foreach (var selectable in selected)
        {
            var entity = selectable.GetComponent<Entity>();
            var mover = selectable.GetComponent<Mover>();
            if (entity == null || mover == null) continue;

            var attacker = selectable.GetComponent<Attacker>();
            if (attacker != null)
                attacker.ClearOrders();

            Vector2Int randomAdj = GetRandomWalkableNeighbor(entity);
            if (randomAdj != entity.Cell)
                mover.MoveTo(randomAdj);
        }
    }

    Vector2Int GetRandomWalkableNeighbor(Entity entity)
    {
        int startDx = Random.Range(-1, 2);
        int startDy = Random.Range(-1, 2);

        for (int i = 0; i < 8; i++)
        {
            int dx = ((startDx + 1 + (i % 3)) % 3) - 1;
            int dy = ((startDy + 1 + (i / 3)) % 3) - 1;
            if (dx == 0 && dy == 0) continue;

            var candidate = new Vector2Int(entity.Cell.x + dx, entity.Cell.y + dy);
            if (MapManager.Instance.GetEntityAt(candidate) != null) continue;

            var locomotion = entity.UnitData != null ? entity.UnitData.Locomotion : LocomotionType.Foot;
            if (TerrainMovement.GetSpeedMultiplier(locomotion,
                MapManager.Instance.GetTerrain(candidate)) > 0f)
                return candidate;
        }

        return entity.Cell;
    }
}
