using UnityEngine;
using System.Collections.Generic;

public class Mover : MonoBehaviour
{
    private Entity _entity;
    private List<Vector2Int> _path;
    private int _pathIndex;
    private float _waitTimer;
    private const float WaitBeforeRepath = 0.5f;

    public bool IsMoving => _path != null && _pathIndex < _path.Count;

    void Awake()
    {
        _entity = GetComponent<Entity>();
    }

    public void MoveTo(Vector2Int target)
    {
        if (_entity.UnitData == null) return;

        var path = Pathfinder.FindPath(_entity.Cell, target, _entity.UnitData.Locomotion);
        if (path == null || path.Count == 0)
        {
            _path = null;
            return;
        }

        _path = path;
        _pathIndex = 0;
        _waitTimer = 0f;
    }

    public void Stop()
    {
        _path = null;
    }

    void Update()
    {
        if (!IsMoving) return;

        Vector2Int nextCell = _path[_pathIndex];

        Entity occupant = MapManager.Instance.GetEntityAt(nextCell);
        if (occupant != null && occupant != _entity)
        {
            if (_entity.UnitData != null && _entity.UnitData.IsCrusher
                && occupant.UnitData != null && occupant.UnitData.Category == UnitCategory.Infantry
                && PlayerManager.Instance.AreEnemies(_entity.OwnerPlayerIndex, occupant.OwnerPlayerIndex))
            {
                occupant.Die();
            }
            else
            {
                _waitTimer += Time.deltaTime;
                if (_waitTimer >= WaitBeforeRepath)
                {
                    _waitTimer = 0f;
                    var goal = _path[^1];
                    _path = null;
                    MoveTo(goal);
                }
                return;
            }
        }

        _waitTimer = 0f;

        Vector3 targetPos = MapManager.Instance.CellToWorld(nextCell);
        float speedMult = TerrainMovement.GetSpeedMultiplier(
            _entity.UnitData.Locomotion,
            MapManager.Instance.GetTerrain(nextCell));
        float speed = _entity.UnitData.BaseSpeed * speedMult;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            transform.position = targetPos;
            _entity.SetCell(nextCell);
            _pathIndex++;
        }
    }
}
