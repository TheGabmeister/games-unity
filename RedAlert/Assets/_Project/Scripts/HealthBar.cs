using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Transform _barFill;
    [SerializeField] private SpriteRenderer _fillRenderer;

    private float _maxHP = 100f;
    private float _currentHP = 100f;

    public float CurrentHP => _currentHP;
    public float MaxHP => _maxHP;
    public bool IsDead => _currentHP <= 0f;

    public System.Action OnDeath;

    public void Initialize(float maxHP)
    {
        _maxHP = maxHP;
        _currentHP = maxHP;
        UpdateBar();
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        _currentHP = Mathf.Max(0, _currentHP - damage);
        UpdateBar();
        if (_currentHP <= 0f)
            OnDeath?.Invoke();
    }

    void UpdateBar()
    {
        float ratio = _currentHP / _maxHP;
        if (_barFill != null)
            _barFill.localScale = new Vector3(ratio, 1f, 1f);

        if (_fillRenderer != null)
        {
            if (ratio > 0.5f)
                _fillRenderer.color = Color.green;
            else if (ratio > 0.25f)
                _fillRenderer.color = Color.yellow;
            else
                _fillRenderer.color = Color.red;
        }
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
