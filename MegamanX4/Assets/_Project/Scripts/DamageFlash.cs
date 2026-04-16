using UnityEngine;

[RequireComponent(typeof(Health))]
public class DamageFlash : MonoBehaviour
{
    [SerializeField] SpriteRenderer target;
    [SerializeField] float period = 0.08f;

    Health health;
    bool flashing;
    float flashStart;

    void Awake() => health = GetComponent<Health>();

    void OnEnable() => health.InvulnerabilityChanged += OnInvulnerabilityChanged;

    void OnDisable()
    {
        health.InvulnerabilityChanged -= OnInvulnerabilityChanged;
        if (target) target.enabled = true;
    }

    void OnInvulnerabilityChanged(bool on)
    {
        flashing = on;
        flashStart = Time.time;
        if (!flashing && target) target.enabled = true;
    }

    void Update()
    {
        if (!flashing || !target) return;
        bool phase = Mathf.FloorToInt((Time.time - flashStart) / period) % 2 == 0;
        target.enabled = phase;
    }
}
