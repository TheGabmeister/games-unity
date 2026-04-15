using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] int maxHealth = 3;

    int currentHealth;
    bool initialized;

    public int CurrentHealth
    {
        get
        {
            EnsureInitialized();
            return currentHealth;
        }
    }

    void Awake() => ResetHealth();

    void OnEnable()
    {
        if (!initialized)
            ResetHealth();
    }

    void OnValidate() => maxHealth = Mathf.Max(1, maxHealth);

    public void ApplyDamage(int amount)
    {
        EnsureInitialized();

        if (amount <= 0 || currentHealth <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (currentHealth == 0)
            Destroy(gameObject);
    }

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
