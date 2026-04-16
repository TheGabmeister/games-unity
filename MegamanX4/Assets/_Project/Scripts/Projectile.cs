using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Flags]
    public enum HitTargets
    {
        None = 0,
        Player = 1 << 0,
        Enemy = 1 << 1,
        Environment = 1 << 2,
    }

    [SerializeField] int damage = 1;
    [SerializeField] HitTargets hitTargets = HitTargets.Enemy | HitTargets.Environment;
    [SerializeField] bool piercing;

    public event Action Destroyed;

    int hitLayerMask;

    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        hitLayerMask = BuildHitLayerMask(hitTargets);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((hitLayerMask & (1 << other.gameObject.layer)) == 0) return;

        var health = other.GetComponentInParent<Health>();
        if (health)
            health.ApplyDamage(damage, transform.position);

        if (!piercing)
            Destroy(gameObject);
    }

    void OnDestroy() => Destroyed?.Invoke();

    static int BuildHitLayerMask(HitTargets targets)
    {
        int mask = 0;
        if ((targets & HitTargets.Player) != 0) mask |= LayerBit("Player");
        if ((targets & HitTargets.Enemy) != 0) mask |= LayerBit("Enemy");
        if ((targets & HitTargets.Environment) != 0) mask |= LayerBit("Environment");
        return mask;
    }

    static int LayerBit(string name)
    {
        int layer = LayerMask.NameToLayer(name);
        return layer >= 0 ? 1 << layer : 0;
    }
}
