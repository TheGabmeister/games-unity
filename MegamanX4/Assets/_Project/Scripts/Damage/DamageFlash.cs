using UnityEngine;

[RequireComponent(typeof(Health))]
public class DamageFlash : MonoBehaviour
{
    [SerializeField] SpriteRenderer _target;
    [SerializeField] Color _flashColor = Color.red;
    [SerializeField] float _flashDuration = 0.1f;

    Health _health;
    Color _originalColor;
    float _flashEndTime;
    bool _flashing;

    void Awake()
    {
        _health = GetComponent<Health>();
        if (!_target) _target = GetComponent<SpriteRenderer>();
    }

    void OnEnable() => _health.Damaged += OnDamaged;

    void OnDisable()
    {
        _health.Damaged -= OnDamaged;
        if (_flashing && _target) _target.color = _originalColor;
        _flashing = false;
    }

    void OnDamaged(int amount, Vector2 sourcePosition)
    {
        if (!_target) return;
        if (!_flashing) _originalColor = _target.color;
        _target.color = _flashColor;
        _flashing = true;
        _flashEndTime = Time.time + _flashDuration;
    }

    void Update()
    {
        if (!_flashing) return;
        if (Time.time < _flashEndTime) return;
        if (_target) _target.color = _originalColor;
        _flashing = false;
    }
}
