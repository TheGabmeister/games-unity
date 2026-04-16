using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] Image _healthFill;
    [SerializeField] Image _energyFill;
    [SerializeField] GameObject _energyRoot;

    Health _health;
    WeaponInventory _weapons;

    public void Bind(Health health, WeaponInventory weapons)
    {
        Unbind();

        _health = health;
        _weapons = weapons;

        if (_health != null)
            _health.HealthChanged += OnHealthChanged;
        else
            Debug.LogWarning("HUD.Bind: no Health component on player.", this);

        if (_weapons != null)
        {
            _weapons.EnergyChanged += OnEnergyChanged;
            _weapons.ActiveWeaponChanged += OnActiveWeaponChanged;
        }
        else
            Debug.LogWarning("HUD.Bind: no WeaponInventory component on player.", this);

        RefreshHealth();
        RefreshActiveWeapon();
    }

    void OnDestroy() => Unbind();

    void Unbind()
    {
        if (_health != null) _health.HealthChanged -= OnHealthChanged;
        if (_weapons != null)
        {
            _weapons.EnergyChanged -= OnEnergyChanged;
            _weapons.ActiveWeaponChanged -= OnActiveWeaponChanged;
        }
        _health = null;
        _weapons = null;
    }

    void OnHealthChanged(int current, int max) => UpdateHealthFill(current, max);
    void OnEnergyChanged() => RefreshEnergyFill();
    void OnActiveWeaponChanged() => RefreshActiveWeapon();

    void RefreshHealth()
    {
        if (_health == null) return;
        UpdateHealthFill(_health.CurrentHealth, _health.MaxHealth);
    }

    void UpdateHealthFill(int current, int max)
    {
        if (!_healthFill) return;
        if (max <= 0)
        {
            _healthFill.fillAmount = 0f;
            return;
        }
        _healthFill.fillAmount = (float)current / max;
    }

    void RefreshActiveWeapon()
    {
        if (_weapons == null) return;

        var weapon = _weapons.ActiveWeapon;
        bool visible = weapon && weapon.maxEnergy > 0;

        if (_energyRoot) _energyRoot.SetActive(visible);

        if (!visible) return;
        if (_energyFill) _energyFill.color = weapon.tint;
        RefreshEnergyFill();
    }

    void RefreshEnergyFill()
    {
        if (_weapons == null) return;
        if (!_energyFill) return;

        int idx = _weapons.ActiveIndex;
        int cur = _weapons.GetEnergy(idx);
        int max = _weapons.GetMaxEnergy(idx);
        if (max <= 0)
        {
            _energyFill.fillAmount = 0f;
            return;
        }
        _energyFill.fillAmount = (float)cur / max;
    }
}
