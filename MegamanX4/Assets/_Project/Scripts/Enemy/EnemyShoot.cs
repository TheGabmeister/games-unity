using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerDetector))]
public class EnemyShoot : MonoBehaviour
{
    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] Transform _muzzle;
    [SerializeField] int _burstCount = 3;
    [SerializeField] float _burstInterval = 0.15f;
    [SerializeField] float _cooldown = 2f;

    PlayerDetector _detector;
    PatrolWalk _patrol;
    Coroutine _burstRoutine;
    float _nextBurstTime;

    void Awake()
    {
        _detector = GetComponent<PlayerDetector>();
        _patrol = GetComponent<PatrolWalk>();
    }

    void Update()
    {
        if (_burstRoutine != null) return;
        if (Time.time < _nextBurstTime) return;
        if (!_detector.CanSeePlayer) return;
        _burstRoutine = StartCoroutine(FireBurst());
    }

    IEnumerator FireBurst()
    {
        if (_patrol) _patrol.Pause();

        for (int i = 0; i < _burstCount; i++)
        {
            AimAtPlayer();
            SpawnProjectile();
            if (i < _burstCount - 1)
                yield return new WaitForSeconds(_burstInterval);
        }

        _nextBurstTime = Time.time + _cooldown;
        _burstRoutine = null;

        if (_patrol) _patrol.Resume();
    }

    void AimAtPlayer()
    {
        if (!_muzzle || !_detector.CanSeePlayer) return;
        Vector2 direction = _detector.PlayerPosition - (Vector2)_muzzle.position;
        if (direction.sqrMagnitude < 0.0001f) return;
        _muzzle.right = direction.normalized;
    }

    void SpawnProjectile()
    {
        if (!_projectilePrefab) return;
        var spawn = _muzzle ? _muzzle : transform;
        Instantiate(_projectilePrefab, spawn.position, spawn.rotation);
    }
}
