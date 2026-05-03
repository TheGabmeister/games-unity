using UnityEngine;

public class C4Placer : MonoBehaviour
{
    private Entity _entity;
    private Mover _mover;
    private Entity _targetBuilding;
    private bool _planted;
    private float _detonationTimer;
    private const float DockDistance = 1.5f;
    private const float C4Delay = 1.8f;

    void Awake()
    {
        _entity = GetComponent<Entity>();
        _mover = GetComponent<Mover>();
    }

    public void PlantC4(Entity building)
    {
        if (building == null || !building.IsBuilding) return;
        _targetBuilding = building;
        _planted = false;

        var adjacent = building.FindAdjacentFreeCell();
        _mover.MoveTo(adjacent);
    }

    void Update()
    {
        if (_entity.IsDead) return;

        if (_planted)
        {
            _detonationTimer -= Time.deltaTime;
            if (_detonationTimer <= 0f)
                Detonate();
            return;
        }

        if (_targetBuilding == null) return;

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

        _planted = true;
        _detonationTimer = C4Delay;
    }

    void Detonate()
    {
        _planted = false;

        if (_targetBuilding != null && !_targetBuilding.IsDead)
            _targetBuilding.Health.TakeDamage(_targetBuilding.Health.CurrentHP);

        _targetBuilding = null;
    }
}
