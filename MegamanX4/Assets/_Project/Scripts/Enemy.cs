using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
public class Enemy : MonoBehaviour
{
    Health health;

    void Awake() => health = GetComponent<Health>();

    void OnEnable()
    {
        if (!health)
            health = GetComponent<Health>();

        health.Depleted += OnDepleted;
    }

    void OnDisable()
    {
        if (health)
            health.Depleted -= OnDepleted;
    }

    void OnDepleted() => Destroy(gameObject);
}
