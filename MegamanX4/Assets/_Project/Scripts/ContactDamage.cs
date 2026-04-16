using UnityEngine;

[DisallowMultipleComponent]
public class ContactDamage : MonoBehaviour
{
    [SerializeField] int damageAmount = 1;

    void OnValidate() => damageAmount = Mathf.Max(1, damageAmount);

    void OnTriggerEnter2D(Collider2D other) => TryDamage(other);

    void OnCollisionEnter2D(Collision2D collision) => TryDamage(collision.collider);

    void TryDamage(Collider2D other)
    {
        if (!other)
            return;

        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer == -1)
            return;

        Transform root = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform.root;
        if (!root || root.gameObject.layer != playerLayer)
            return;

        var health = other.GetComponentInParent<Health>();
        if (!health)
            return;

        health.ApplyDamage(damageAmount, transform.position);
    }
}
