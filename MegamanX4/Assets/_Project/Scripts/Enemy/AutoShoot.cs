using UnityEngine;

public class AutoShoot : MonoBehaviour
{
    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] Transform _muzzle;
    [SerializeField] float _interval = 2f;

    float _timer;

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < _interval) return;

        _timer = 0f;
        Fire();
    }

    void Fire()
    {
        if (!_projectilePrefab) return;
        var spawnPoint = _muzzle ? _muzzle : transform;
        Instantiate(_projectilePrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
