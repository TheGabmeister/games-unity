using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BusterShot : MonoBehaviour
{
    [SerializeField] float _speed = 18f;
    [SerializeField] float _lifetime = 0.6f;
    [SerializeField] int _damage = 1;
    [SerializeField] LayerMask _hitLayers = ~0;

    Rigidbody2D _rb;

    public event Action Destroyed;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;
    }

    public void Fire()
    {
        _rb.linearVelocity = (Vector2)(transform.right * _speed);
        Destroy(gameObject, _lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((_hitLayers.value & (1 << other.gameObject.layer)) == 0) return;

        var targetHealth = other.GetComponentInParent<Health>();
        if (targetHealth)
            targetHealth.ApplyDamage(_damage);

        Destroy(gameObject);
    }

    void OnDestroy() => Destroyed?.Invoke();
}
