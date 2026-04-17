using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int _maxHealth = 10;
    [SerializeField] float _invulnerabilityDuration = 1f;

    int _currentHealth;
    bool _initialized;
    float _invulnerableUntil;
    bool _wasInvulnerable;

    public int MaxHealth
    {
        get
        {
            EnsureInitialized();
            return _maxHealth;
        }
    }

    public int CurrentHealth
    {
        get
        {
            EnsureInitialized();
            return _currentHealth;
        }
    }

    public bool IsDepleted => CurrentHealth <= 0;
    public bool IsInvulnerable => Time.time < _invulnerableUntil;
    public float InvulnerabilityDuration => _invulnerabilityDuration;

    public event Action<int, Vector2> Damaged;
    public event Action<int> Healed;
    public event Action<int, int> HealthChanged;
    public event Action Depleted;
    public event Action<bool> InvulnerabilityChanged;

    protected virtual void Awake() => ResetHealth();

    protected virtual void OnEnable()
    {
        if (!_initialized)
            ResetHealth();
    }

    protected virtual void OnValidate()
    {
        _maxHealth = Mathf.Max(1, _maxHealth);
        _invulnerabilityDuration = Mathf.Max(0f, _invulnerabilityDuration);
    }

    protected virtual void Update()
    {
        bool now = IsInvulnerable;
        if (now == _wasInvulnerable) return;
        _wasInvulnerable = now;
        InvulnerabilityChanged?.Invoke(now);
    }

    public void ApplyDamage(int amount) => ApplyDamage(amount, transform.position);

    public virtual void ApplyDamage(int amount, Vector2 sourcePosition)
    {
        EnsureInitialized();

        if (IsInvulnerable || amount <= 0 || _currentHealth <= 0)
            return;

        int previousHealth = _currentHealth;
        _currentHealth = Mathf.Max(0, _currentHealth - amount);
        int appliedDamage = previousHealth - _currentHealth;
        if (appliedDamage <= 0)
            return;

        Damaged?.Invoke(appliedDamage, sourcePosition);
        HealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (_currentHealth == 0)
        {
            HandleDepleted();
            return;
        }

        if (_invulnerabilityDuration > 0f)
            _invulnerableUntil = Time.time + _invulnerabilityDuration;
    }

    public virtual void Heal(int amount)
    {
        EnsureInitialized();

        if (amount <= 0 || _currentHealth >= _maxHealth)
            return;

        int previousHealth = _currentHealth;
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        int restoredHealth = _currentHealth - previousHealth;
        if (restoredHealth <= 0)
            return;

        Healed?.Invoke(restoredHealth);
        HealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public void RestoreFullHealth()
    {
        EnsureInitialized();

        if (_currentHealth >= _maxHealth)
            return;

        int restoredHealth = _maxHealth - _currentHealth;
        _currentHealth = _maxHealth;
        Healed?.Invoke(restoredHealth);
        HealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    protected virtual void HandleDepleted() => Depleted?.Invoke();

    void EnsureInitialized()
    {
        if (!_initialized)
            ResetHealth();
    }

    void ResetHealth()
    {
        _maxHealth = Mathf.Max(1, _maxHealth);
        _currentHealth = _maxHealth;
        _initialized = true;
    }
}
