using UnityEngine;

public class SelfHeal : MonoBehaviour
{
    [SerializeField] private float _healInterval = 2f;
    [SerializeField] private int _healAmount = 1;
    [SerializeField] private float _maxHealRatio = 0.5f;

    private Health _health;
    private float _timer;

    void Awake()
    {
        _health = GetComponent<Health>();
    }

    void Update()
    {
        if (_health == null || _health.IsDead) return;
        if (_health.Ratio >= _maxHealRatio) return;

        _timer += Time.deltaTime;
        if (_timer < _healInterval) return;

        _timer = 0f;
        int targetHP = Mathf.FloorToInt(_health.MaxHP * _maxHealRatio);
        int healable = targetHP - _health.CurrentHP;
        if (healable <= 0) return;

        _health.Heal(Mathf.Min(_healAmount, healable));
    }
}
