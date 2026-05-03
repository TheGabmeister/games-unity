using UnityEngine;

public class Attacker : MonoBehaviour
{
    private Entity _entity;
    private Mover _mover;
    private Entity _target;
    private Vector2Int? _forceFireCell;
    private bool _guarding;
    private Vector2Int _guardCell;
    private Vector2Int? _attackMoveDest;
    private float _cooldownTimer;
    private int _burstRemaining;
    private float _burstTimer;
    private const float BurstDelay = 0.1f;

    public Entity Target => _target;
    public bool IsGuarding => _guarding;

    void Awake()
    {
        _entity = GetComponent<Entity>();
        _mover = GetComponent<Mover>();
    }

    public void AttackTarget(Entity target)
    {
        _target = target;
        _forceFireCell = null;
        _guarding = false;
        _attackMoveDest = null;
    }

    public void ForceFireAt(Vector2Int cell)
    {
        _forceFireCell = cell;
        _target = MapManager.Instance.GetEntityAt(cell);
        _guarding = false;
        _attackMoveDest = null;
    }

    public void SetGuard()
    {
        _guarding = true;
        _guardCell = _entity.Cell;
        _target = null;
        _forceFireCell = null;
        _attackMoveDest = null;
        _mover?.Stop();
    }

    public void AttackMoveTo(Vector2Int dest)
    {
        _attackMoveDest = dest;
        _target = null;
        _forceFireCell = null;
        _guarding = false;
        _mover?.MoveTo(dest);
    }

    public void ClearOrders()
    {
        _target = null;
        _forceFireCell = null;
        _guarding = false;
        _attackMoveDest = null;
    }

    void Update()
    {
        if (_entity.IsDead) return;
        if (_entity.UnitData == null || _entity.UnitData.PrimaryWeapon == null) return;

        _cooldownTimer -= Time.deltaTime;

        if (_target != null && _target.IsDead)
        {
            _target = null;
            _forceFireCell = null;
            if (_attackMoveDest.HasValue && _mover != null)
                _mover.MoveTo(_attackMoveDest.Value);
        }

        if (_target == null && !_forceFireCell.HasValue)
            _target = FindNearestEnemy();

        if (_target == null && !_forceFireCell.HasValue) return;

        WeaponData weapon = _entity.UnitData.PrimaryWeapon;
        float range = weapon.Range;

        Vector2Int targetCell = _forceFireCell ?? _target.Cell;
        float dist = Vector2.Distance(
            new Vector2(_entity.Cell.x, _entity.Cell.y),
            new Vector2(targetCell.x, targetCell.y));

        if (dist > range)
        {
            if (_forceFireCell.HasValue || _target != null)
            {
                if (!_guarding && _mover != null && !_mover.IsMoving)
                    _mover.MoveTo(targetCell);
            }
            else
            {
                _target = null;
            }
            return;
        }

        if (_entity.UnitData.NoMovingFire && _mover != null && _mover.IsMoving)
            return;

        if (_attackMoveDest.HasValue && _mover != null && _mover.IsMoving)
            _mover.Stop();

        if (_burstRemaining > 0)
        {
            _burstTimer -= Time.deltaTime;
            if (_burstTimer <= 0f)
            {
                FireOneShot(targetCell);
                _burstRemaining--;
                _burstTimer = BurstDelay;
            }
            return;
        }

        if (_cooldownTimer <= 0f)
        {
            _burstRemaining = weapon.Burst;
            _burstTimer = 0f;
            _cooldownTimer = weapon.ROF;
        }
    }

    void FireOneShot(Vector2Int targetCell)
    {
        WeaponData weapon = _entity.UnitData.PrimaryWeapon;
        Vector3 targetPos = MapManager.Instance.CellToWorld(targetCell);

        if (weapon.FireSound != null)
            SfxManager.Instance?.PlaySound(weapon.FireSound);

        if (weapon.Projectile.Type == ProjectileType.Hitscan)
        {
            DamageSystem.ApplyDamageAtPoint(targetPos, weapon.Damage, weapon.Warhead, _entity, _target);
        }
        else
        {
            var go = new GameObject("Projectile");
            go.transform.position = transform.position;
            if (weapon.Projectile.Sprite != null)
            {
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = weapon.Projectile.Sprite;
                sr.sortingOrder = 10;
            }
            var proj = go.AddComponent<Projectile>();
            proj.Init(weapon, _target, _entity, targetPos);
        }
    }

    Entity FindNearestEnemy()
    {
        if (PlayerManager.Instance == null) return null;

        WeaponData weapon = _entity.UnitData.PrimaryWeapon;
        float range = weapon.Range;
        float bestDist = float.MaxValue;
        Entity best = null;

        for (int p = 0; p < PlayerManager.Instance.PlayerCount; p++)
        {
            if (!PlayerManager.Instance.AreEnemies(_entity.OwnerPlayerIndex, p)) continue;
            var player = PlayerManager.Instance.GetPlayer(p);
            for (int i = player.OwnedEntities.Count - 1; i >= 0; i--)
            {
                var entity = player.OwnedEntities[i];
                if (entity == null || entity.IsDead) continue;

                var spy = entity.GetComponent<Spy>();
                if (spy != null && spy.IsDisguised) continue;

                float dist = Vector2.Distance(
                    new Vector2(_entity.Cell.x, _entity.Cell.y),
                    new Vector2(entity.Cell.x, entity.Cell.y));

                if (dist <= range && dist < bestDist)
                {
                    bestDist = dist;
                    best = entity;
                }
            }
        }

        return best;
    }
}
