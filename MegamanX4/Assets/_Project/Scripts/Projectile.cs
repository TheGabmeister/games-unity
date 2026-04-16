using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] bool piercing;

    public event Action Destroyed;

    int environmentLayer;

    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        environmentLayer = LayerMask.NameToLayer("Environment");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == environmentLayer)
        {
            Destroy(gameObject);   // walls stop all projectiles, including piercing
            return;
        }

        var health = other.GetComponentInParent<Health>();
        if (health)
            health.ApplyDamage(damage, transform.position);

        if (!piercing)
            Destroy(gameObject);
    }

    void OnDestroy() => Destroyed?.Invoke();
}
