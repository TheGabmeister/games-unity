using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BusterShot : MonoBehaviour
{
    [SerializeField] float speed = 18f;
    [SerializeField] float lifetime = 0.6f;
    [SerializeField] int damage = 1;
    [SerializeField] LayerMask hitLayers = ~0;

    Rigidbody2D rb;

    public event Action Destroyed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }

    public void Fire(int direction)
    {
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * Mathf.Sign(direction);
        transform.localScale = s;
        rb.linearVelocity = new Vector2(speed * Mathf.Sign(direction), 0f);
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((hitLayers.value & (1 << other.gameObject.layer)) == 0) return;

        var targetHealth = other.GetComponentInParent<Health>();
        if (targetHealth)
            targetHealth.ApplyDamage(damage);

        Destroy(gameObject);
    }

    void OnDestroy() => Destroyed?.Invoke();
}
