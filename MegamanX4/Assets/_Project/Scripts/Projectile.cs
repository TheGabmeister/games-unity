using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] bool _piercing;
    [SerializeField] float _lifetime = 1f;

    float _timer;

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
            Destroy(gameObject);
            return;
        }

        if (!_piercing)
            Destroy(gameObject);
    }

    void OnDestroy() => Destroyed?.Invoke();

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _lifetime) Destroy(gameObject);
    }
}
