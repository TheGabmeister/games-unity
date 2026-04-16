using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] int _damage = 1;
    [SerializeField] bool _piercing;

    public event Action Destroyed;

    int _environmentLayer;

    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        _environmentLayer = LayerMask.NameToLayer("Environment");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == _environmentLayer)
        {
            Destroy(gameObject);   // walls stop all projectiles, including piercing
            return;
        }

        var hurtBox = other.GetComponentInParent<HurtBox>();
        if (hurtBox)
            hurtBox.ReceiveHit(_damage, transform.position);

        if (!_piercing)
            Destroy(gameObject);
    }

    void OnDestroy() => Destroyed?.Invoke();
}
