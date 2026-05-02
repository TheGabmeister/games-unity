using UnityEngine;

public class Projectile : MonoBehaviour
{
    private WeaponData _weapon;
    private Entity _target;
    private Entity _shooter;
    private Vector3 _targetPos;
    private float _arcHeight;
    private Vector3 _startPos;
    private float _journeyLength;
    private float _traveled;

    public void Init(WeaponData weapon, Entity target, Entity shooter, Vector3 targetPos)
    {
        _weapon = weapon;
        _target = target;
        _shooter = shooter;
        _targetPos = targetPos;
        _startPos = transform.position;
        _journeyLength = Vector3.Distance(_startPos, _targetPos);

        if (weapon.Projectile.Scatter > 0f)
        {
            Vector2 offset = Random.insideUnitCircle * weapon.Projectile.Scatter;
            _targetPos += new Vector3(offset.x, offset.y, 0f);
        }

        if (weapon.Projectile.Type == ProjectileType.Ballistic)
            _arcHeight = _journeyLength * 0.3f;
    }

    void Update()
    {
        Vector3 dest = _targetPos;

        if (_weapon.Projectile.Type == ProjectileType.Homing && _target != null && !_target.IsDead)
            dest = _target.transform.position;

        float speed = _weapon.Projectile.Speed;
        _traveled += speed * Time.deltaTime;

        if (_weapon.Projectile.Type == ProjectileType.Ballistic && _journeyLength > 0f)
        {
            float t = Mathf.Clamp01(_traveled / _journeyLength);
            Vector3 flat = Vector3.Lerp(_startPos, dest, t);
            float arc = _arcHeight * 4f * t * (1f - t);
            transform.position = flat + Vector3.up * arc;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, dest, speed * Time.deltaTime);
        }

        Vector2 dir = (dest - transform.position).normalized;
        if (dir.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (Vector3.Distance(transform.position, dest) < 0.15f)
            Impact();
    }

    void Impact()
    {
        DamageSystem.ApplyDamageAtPoint(transform.position, _weapon.Damage, _weapon.Warhead, _shooter, _target);
        Destroy(gameObject);
    }
}
