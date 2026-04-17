using UnityEngine;

[RequireComponent(typeof(Health))]
public class Protecton : MonoBehaviour
{
    [SerializeField, Range(0f, 360f)] float _shieldConeAngle = 180f;

    Health _health;

    void Awake() => _health = GetComponent<Health>();

    void OnEnable() => _health.Damaged += OnDamaged;

    void OnDisable() => _health.Damaged -= OnDamaged;

    void OnDamaged(int amount, Vector2 sourcePosition)
    {
        Vector2 toSource = sourcePosition - (Vector2)transform.position;
        if (toSource.sqrMagnitude < 0.0001f) return;

        Vector2 forward = (Vector2)transform.right * Mathf.Sign(transform.lossyScale.x);
        float angle = Vector2.Angle(forward, toSource);
        if (angle <= _shieldConeAngle * 0.5f)
            _health.Heal(amount);
    }
}
