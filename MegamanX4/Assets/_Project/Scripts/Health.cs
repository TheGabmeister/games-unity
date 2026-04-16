using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int maxHealth = 3;
    [SerializeField] float invulnerabilityDuration = 1f;

    int currentHealth;
    bool initialized;
    float invulnerableUntil;
    bool wasInvulnerable;

    public int MaxHealth
    {
        get
        {
            EnsureInitialized();
            return maxHealth;
        }
    }

    public int CurrentHealth
    {
        get
        {
            EnsureInitialized();
            return currentHealth;
        }
    }

    public bool IsDepleted => CurrentHealth <= 0;
    public bool IsInvulnerable => Time.time < invulnerableUntil;
    public float InvulnerabilityDuration => invulnerabilityDuration;

    public event Action<int, Vector2> Damaged;
    public event Action<int> Healed;
    public event Action<int, int> HealthChanged;
    public event Action Depleted;
    public event Action<bool> InvulnerabilityChanged;

    protected virtual void Awake() => ResetHealth();

    protected virtual void OnEnable()
    {
        if (!initialized)
            ResetHealth();
    }

    protected virtual void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        invulnerabilityDuration = Mathf.Max(0f, invulnerabilityDuration);
    }

    protected virtual void Update()
    {
        bool now = IsInvulnerable;
        if (now == wasInvulnerable) return;
        wasInvulnerable = now;
        InvulnerabilityChanged?.Invoke(now);
    }

    public void ApplyDamage(int amount) => ApplyDamage(amount, transform.position);

    public virtual void ApplyDamage(int amount, Vector2 sourcePosition)
    {
        EnsureInitialized();

        if (IsInvulnerable || amount <= 0 || currentHealth <= 0)
            return;

        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        int appliedDamage = previousHealth - currentHealth;
        if (appliedDamage <= 0)
            return;

        Damaged?.Invoke(appliedDamage, sourcePosition);
        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0)
        {
            HandleDepleted();
            return;
        }

        if (invulnerabilityDuration > 0f)
            invulnerableUntil = Time.time + invulnerabilityDuration;
    }

    public virtual void Heal(int amount)
    {
        EnsureInitialized();

        if (amount <= 0 || currentHealth >= maxHealth)
            return;

        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        int restoredHealth = currentHealth - previousHealth;
        if (restoredHealth <= 0)
            return;

        Healed?.Invoke(restoredHealth);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void RestoreFullHealth()
    {
        EnsureInitialized();

        if (currentHealth >= maxHealth)
            return;

        int restoredHealth = maxHealth - currentHealth;
        currentHealth = maxHealth;
        Healed?.Invoke(restoredHealth);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    protected virtual void HandleDepleted() => Depleted?.Invoke();

    void EnsureInitialized()
    {
        if (!initialized)
            ResetHealth();
    }

    void ResetHealth()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;
        initialized = true;
    }
}
