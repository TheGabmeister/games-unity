using UnityEngine;

[DisallowMultipleComponent]
public class HitBox : MonoBehaviour
{
    [SerializeField] int _damage = 1;

    void OnValidate() => _damage = Mathf.Max(1, _damage);

    void OnTriggerEnter2D(Collider2D other) => TryHit(other);

    void OnCollisionEnter2D(Collision2D collision) => TryHit(collision.collider);

    void TryHit(Collider2D other)
    {
        if (!other) return;

        var hurtBox = other.GetComponentInParent<HurtBox>();
        if (!hurtBox) return;

        hurtBox.ReceiveHit(_damage, transform.position);
    }
}
