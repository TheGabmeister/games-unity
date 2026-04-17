using UnityEngine;

public class BlastRaster : MonoBehaviour
{
    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] int _shotCount = 8;
    [SerializeField] float _fireInterval = 2.5f;
    [SerializeField] float _initialDelay = 1f;
    [SerializeField] float _rotationOffset = 0f;

    float _cooldownTimer;

    void Awake() => _cooldownTimer = _initialDelay;

    void Update()
    {
        _cooldownTimer -= Time.deltaTime;
        if (_cooldownTimer > 0f) return;

        FireRadial(_shotCount);
        _cooldownTimer = _fireInterval;
    }

    void FireRadial(int n)
    {
        if (!_projectilePrefab || n <= 0) return;

        float step = 360f / n;
        for (int i = 0; i < n; i++)
        {
            float angle = _rotationOffset + step * i;
            Quaternion rot = Quaternion.Euler(0f, 0f, angle);
            Instantiate(_projectilePrefab, transform.position, rot);
        }
    }
}
