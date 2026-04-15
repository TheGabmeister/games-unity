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

        currentHealth = Mathf.Max(0, currentHealth - amount);
        Damaged?.Invoke(amount);

        if (currentHealth == 0)
            HandleDepleted();
    }

    public void RestoreFullHealth() => ResetHealth();

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
