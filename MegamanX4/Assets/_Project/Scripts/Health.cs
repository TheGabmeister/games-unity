using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int maxHealth = 3;

    int currentHealth;
    bool initialized;

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

    public event Action<int> Damaged;
    public event Action<int> Healed;
    public event Action<int, int> HealthChanged;
    public event Action Depleted;

    protected virtual void Awake() => ResetHealth();

    protected virtual void OnEnable()
    {
        if (!initialized)
            ResetHealth();
    }

    protected virtual void OnValidate() => maxHealth = Mathf.Max(1, maxHealth);

    public virtual void ApplyDamage(int amount)
    {
        EnsureInitialized();

        if (amount <= 0 || currentHealth <= 0)
            return;

        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        int appliedDamage = previousHealth - currentHealth;
        if (appliedDamage <= 0)
            return;

        Damaged?.Invoke(appliedDamage);
        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0)
            HandleDepleted();
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
