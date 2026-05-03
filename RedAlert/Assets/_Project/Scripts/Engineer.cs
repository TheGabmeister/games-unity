using UnityEngine;

public class Engineer : MonoBehaviour
{
    private Entity _entity;
    private Mover _mover;
    private Entity _targetBuilding;
    private const float DockDistance = 1.5f;

    void Awake()
    {
        _entity = GetComponent<Entity>();
        _mover = GetComponent<Mover>();
    }

    public void SendToBuilding(Entity building)
    {
        if (building == null || !building.IsBuilding) return;
        _targetBuilding = building;

        var adjacent = building.FindAdjacentFreeCell();
        _mover.MoveTo(adjacent);
    }

    void Update()
    {
        if (_entity.IsDead || _targetBuilding == null) return;

        if (_targetBuilding.IsDead)
        {
            _targetBuilding = null;
            return;
        }

        if (_mover.IsMoving) return;

        float dist = Vector2Int.Distance(_entity.Cell, _targetBuilding.Cell);
        if (dist > DockDistance + _targetBuilding.UnitData.FootprintX)
        {
            var adjacent = _targetBuilding.FindAdjacentFreeCell();
            _mover.MoveTo(adjacent);
            return;
        }

        if (PlayerManager.Instance.AreEnemies(_entity.OwnerPlayerIndex, _targetBuilding.OwnerPlayerIndex))
            CaptureOrDamage();
        else
            RepairBuilding();
    }

    void RepairBuilding()
    {
        var health = _targetBuilding.Health;
        health.Heal(health.MaxHP);
        _targetBuilding = null;
        _entity.Die();
    }

    void CaptureOrDamage()
    {
        var health = _targetBuilding.Health;
        int damageAmount = health.MaxHP / 3;

        if (health.Ratio <= 0.25f)
        {
            _targetBuilding.TransferOwnership(_entity.OwnerPlayerIndex);
            health.Heal(health.MaxHP);
            _targetBuilding = null;
            _entity.Die();
            return;
        }

        health.TakeDamage(damageAmount);
        _targetBuilding = null;
        _entity.Die();
    }
}
