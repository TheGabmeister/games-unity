using UnityEngine;

public class BuildingDamage : MonoBehaviour
{
    [SerializeField] private GameObject _fireOverlay;

    private Health _health;

    void Awake()
    {
        _health = GetComponent<Health>();
        if (_fireOverlay != null)
            _fireOverlay.SetActive(false);
    }

    void OnEnable()
    {
        if (_health != null)
            _health.OnHealthChanged += UpdateDamageState;
    }

    void OnDisable()
    {
        if (_health != null)
            _health.OnHealthChanged -= UpdateDamageState;
    }

    void UpdateDamageState(float ratio)
    {
        if (_fireOverlay != null)
            _fireOverlay.SetActive(ratio <= 0.5f);
    }
}
