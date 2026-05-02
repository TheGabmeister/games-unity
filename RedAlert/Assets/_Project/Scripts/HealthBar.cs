using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Transform _barFill;
    [SerializeField] private SpriteRenderer _fillRenderer;

    private Health _health;
    private float _ratio = 1f;

    void Awake()
    {
        _health = GetComponentInParent<Health>(true);
    }

    void OnEnable()
    {
        if (_health != null)
            _health.OnHealthChanged += UpdateBar;
        UpdateBar(_ratio);
    }

    void OnDisable()
    {
        if (_health != null)
            _health.OnHealthChanged -= UpdateBar;
    }

    void UpdateBar(float ratio)
    {
        _ratio = ratio;
        if (_barFill != null)
        {
            _barFill.localScale = new Vector3(ratio, 1f, 1f);
            const float HalfWidth = 0.375f;
            _barFill.localPosition = new Vector3(HalfWidth * (ratio - 1f), 0f, 0f);
        }

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
