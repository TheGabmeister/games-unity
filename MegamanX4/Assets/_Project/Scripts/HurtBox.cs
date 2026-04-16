using UnityEngine;

[DisallowMultipleComponent]
public class HurtBox : MonoBehaviour
{
    Health _health;

    void Awake() => _health = GetComponentInParent<Health>();

    public void ReceiveHit(int damage, Vector2 sourcePosition)
    {
        if (!_health) return;
        _health.ApplyDamage(damage, sourcePosition);
    }
}
