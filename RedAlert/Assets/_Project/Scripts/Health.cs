using UnityEngine;

public class Health : MonoBehaviour
{
    private int _maxHP = 100;
    private int _currentHP = 100;

    public int CurrentHP => _currentHP;
    public int MaxHP => _maxHP;
    public bool IsDead => _currentHP <= 0;
    public float Ratio => _maxHP > 0 ? (float)_currentHP / _maxHP : 0f;

    public System.Action<float> OnHealthChanged;
    public System.Action OnDeath;

    public void Initialize(int maxHP)
    {
        _maxHP = maxHP;
        _currentHP = maxHP;
        OnHealthChanged?.Invoke(Ratio);
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        _currentHP = Mathf.Max(0, _currentHP - damage);
        OnHealthChanged?.Invoke(Ratio);
        if (_currentHP <= 0)
            OnDeath?.Invoke();
    }
}
