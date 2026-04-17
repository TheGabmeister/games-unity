using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
public class DestroyOnDepleted : MonoBehaviour
{
    Health _health;

    void Awake() => _health = GetComponent<Health>();

    void OnEnable()
    {
        if (!_health)
            _health = GetComponent<Health>();

        _health.Depleted += OnDepleted;
    }

    void OnDisable()
    {
        if (_health)
            _health.Depleted -= OnDepleted;
    }

    void OnDepleted() => Destroy(gameObject);
}
