using UnityEngine;

[RequireComponent(typeof(Health))]
public class DamageFlash : MonoBehaviour
{
    [SerializeField] SpriteRenderer _target;
    [SerializeField] float _period = 0.08f;

    Health _health;
    bool _flashing;
    float _flashStart;

    void Awake() => _health = GetComponent<Health>();

    void OnEnable() => _health.InvulnerabilityChanged += OnInvulnerabilityChanged;

    void OnDisable()
    {
        _health.InvulnerabilityChanged -= OnInvulnerabilityChanged;
        if (_target) _target.enabled = true;
    }

    void OnInvulnerabilityChanged(bool on)
    {
        _flashing = on;
        _flashStart = Time.time;
        if (!_flashing && _target) _target.enabled = true;
    }

    void Update()
    {
        if (!_flashing || !_target) return;
        bool phase = Mathf.FloorToInt((Time.time - _flashStart) / _period) % 2 == 0;
        _target.enabled = phase;
    }
}
