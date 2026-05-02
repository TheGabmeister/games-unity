using UnityEngine;

public class Harvester : MonoBehaviour
{
    private const int MaxBails = 28;
    private const int NearScanRadius = 6;
    private const int FarScanRadius = 48;
    private const float HarvestTickInterval = 0.5f;
    private const float DepositTickInterval = 0.15f;
    private const float IdleRescanInterval = 5f;

    private Entity _entity;
    private Mover _mover;
    private Refinery _targetRefinery;

    private HarvesterState _state = HarvesterState.Idle;
    private int _bails;
    private int _cargoValue;
    private Vector2Int _harvestTarget;
    private float _tickTimer;
    private float _idleTimer;
    private bool _hasManualTarget;

    public HarvesterState State => _state;
    public int Bails => _bails;
    public bool IsFull => _bails >= MaxBails;

    void Awake()
    {
        _entity = GetComponent<Entity>();
        _mover = GetComponent<Mover>();
    }

    void Start()
    {
        SeekOre();
    }

    void Update()
    {
        if (_entity.IsDead) return;

        switch (_state)
        {
            case HarvesterState.SeekOre:
                UpdateSeekOre();
                break;
            case HarvesterState.Harvesting:
                UpdateHarvesting();
                break;
            case HarvesterState.ReturnToRefinery:
                UpdateReturnToRefinery();
                break;
            case HarvesterState.Depositing:
                UpdateDepositing();
                break;
            case HarvesterState.Idle:
                UpdateIdle();
                break;
        }
    }

    public void SendToOre(Vector2Int cell)
    {
        _hasManualTarget = true;
        _harvestTarget = cell;
        _state = HarvesterState.SeekOre;
        _mover.MoveTo(cell);
    }

    public void SendToRefinery(Refinery refinery)
    {
        if (refinery == null) return;
        _targetRefinery = refinery;
        _state = HarvesterState.ReturnToRefinery;
        _mover.MoveTo(refinery.DockCell);
    }

    public void Stop()
    {
        if (_targetRefinery != null && _state == HarvesterState.Depositing)
            _targetRefinery.Undock(this);
        _targetRefinery = null;
        _state = HarvesterState.Idle;
        _idleTimer = 0f;
    }

    public void OnRefineryDestroyed()
    {
        if (_state == HarvesterState.Depositing)
        {
            _targetRefinery = null;
            ReturnToRefinery();
        }
        else if (_state == HarvesterState.ReturnToRefinery)
        {
            _targetRefinery = null;
            ReturnToRefinery();
        }
    }

    void SeekOre()
    {
        _state = HarvesterState.SeekOre;
        _hasManualTarget = false;

        var oreCell = MapManager.Instance.FindNearestOre(_entity.Cell, NearScanRadius);
        if (oreCell == null)
            oreCell = MapManager.Instance.FindNearestOre(_entity.Cell, FarScanRadius);

        if (oreCell.HasValue)
        {
            _harvestTarget = oreCell.Value;
            _mover.MoveTo(oreCell.Value);
        }
        else
        {
            _state = HarvesterState.Idle;
            _idleTimer = 0f;
        }
    }

    void UpdateSeekOre()
    {
        if (_mover.IsMoving) return;

        if (MapManager.Instance.HasOre(_entity.Cell))
        {
            _state = HarvesterState.Harvesting;
            _tickTimer = 0f;
            return;
        }

        var adjacent = FindAdjacentOre(_entity.Cell);
        if (adjacent.HasValue)
        {
            _harvestTarget = adjacent.Value;
            _mover.MoveTo(adjacent.Value);
            return;
        }

        if (_bails > 0)
        {
            ReturnToRefinery();
            return;
        }

        _state = HarvesterState.Idle;
        _idleTimer = 0f;
    }

    void UpdateHarvesting()
    {
        if (IsFull)
        {
            ReturnToRefinery();
            return;
        }

        _tickTimer += Time.deltaTime;
        if (_tickTimer < HarvestTickInterval) return;
        _tickTimer = 0f;

        if (!MapManager.Instance.HasOre(_entity.Cell))
        {
            var adjacent = FindAdjacentOre(_entity.Cell);
            if (adjacent.HasValue)
            {
                _harvestTarget = adjacent.Value;
                _state = HarvesterState.SeekOre;
                _mover.MoveTo(adjacent.Value);
            }
            else if (_bails > 0)
            {
                ReturnToRefinery();
            }
            else
            {
                SeekOre();
            }
            return;
        }

        int value = MapManager.Instance.HarvestBail(_entity.Cell);
        if (value > 0)
        {
            _bails++;
            _cargoValue += value;
        }
    }

    void ReturnToRefinery()
    {
        _state = HarvesterState.ReturnToRefinery;
        _targetRefinery = FindNearestRefinery();

        if (_targetRefinery == null)
        {
            _state = HarvesterState.Idle;
            _idleTimer = 0f;
            return;
        }

        _mover.MoveTo(_targetRefinery.DockCell);
    }

    void UpdateReturnToRefinery()
    {
        if (_targetRefinery == null || _targetRefinery.GetComponent<Entity>().IsDead)
        {
            _targetRefinery = null;
            ReturnToRefinery();
            return;
        }

        float dist = Vector2Int.Distance(_entity.Cell, _targetRefinery.DockCell);
        if (dist <= 1.5f)
        {
            _mover.Stop();
            if (_targetRefinery.TryDock(this))
            {
                _state = HarvesterState.Depositing;
                _tickTimer = 0f;
            }
        }
        else if (!_mover.IsMoving)
        {
            _mover.MoveTo(_targetRefinery.DockCell);
        }
    }

    void UpdateDepositing()
    {
        if (_targetRefinery == null || _targetRefinery.GetComponent<Entity>().IsDead)
        {
            _targetRefinery = null;
            if (_bails > 0)
                ReturnToRefinery();
            else
                SeekOre();
            return;
        }

        _tickTimer += Time.deltaTime;
        if (_tickTimer < DepositTickInterval) return;
        _tickTimer = 0f;

        if (_bails <= 0)
        {
            _targetRefinery.Undock(this);
            _targetRefinery = null;
            _cargoValue = 0;
            SeekOre();
            return;
        }

        int valueThisBail = _bails == 1 ? _cargoValue : _cargoValue / _bails;
        EconomyManager.Instance.AddCredits(_entity.OwnerPlayerIndex, valueThisBail);
        _bails--;
        _cargoValue -= valueThisBail;
    }

    void UpdateIdle()
    {
        _idleTimer += Time.deltaTime;
        if (_idleTimer < IdleRescanInterval) return;
        _idleTimer = 0f;

        if (_bails > 0)
        {
            ReturnToRefinery();
            return;
        }

        SeekOre();
    }

    Refinery FindNearestRefinery()
    {
        var player = PlayerManager.Instance.GetPlayer(_entity.OwnerPlayerIndex);
        float bestDist = float.MaxValue;
        Refinery best = null;

        foreach (var entity in player.OwnedEntities)
        {
            if (entity == null || entity.IsDead) continue;
            var refinery = entity.GetComponent<Refinery>();
            if (refinery == null) continue;

            float dist = Vector2Int.Distance(_entity.Cell, entity.Cell);
            bool preferUnoccupied = !refinery.IsOccupied;

            if (best == null
                || (preferUnoccupied && best.IsOccupied)
                || (preferUnoccupied == !best.IsOccupied && dist < bestDist))
            {
                bestDist = dist;
                best = refinery;
            }
        }

        return best;
    }

    Vector2Int? FindAdjacentOre(Vector2Int center)
    {
        float bestDist = float.MaxValue;
        Vector2Int? best = null;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                var cell = new Vector2Int(center.x + dx, center.y + dy);
                if (!MapManager.Instance.IsInBounds(cell)) continue;
                if (!MapManager.Instance.HasOre(cell)) continue;

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
}

public enum HarvesterState
{
    Idle,
    SeekOre,
    Harvesting,
    ReturnToRefinery,
    Depositing
}
