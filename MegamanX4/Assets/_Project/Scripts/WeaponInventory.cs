using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerInput))]
public class WeaponInventory : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] List<WeaponData> _weapons = new();

    [Header("Charge")]
    [SerializeField] float _semiChargeTime = 0.4f;
    [SerializeField] float _fullChargeTime = 1.2f;

    [Header("Buster lemon cap")]
    [SerializeField] int _maxSmallShots = 3;

    [Header("Charge flash")]
    [SerializeField] Color _semiFlashColor = Color.white;
    [SerializeField] Color _fullFlashColor = new(0.4f, 1f, 1f);
    [SerializeField] float _flashPeriod = 0.08f;

    PlayerController _controller;
    PlayerInput _playerInput;
    SpriteRenderer _spriteRenderer;

    InputAction _weaponNextAction;
    InputAction _weaponPrevAction;

    int _activeIndex;
    bool _isCharging;
    float _chargeTimer;
    readonly List<Projectile> _activeSmallShots = new();

    public WeaponData ActiveWeapon =>
        _weapons.Count > 0 && _activeIndex >= 0 && _activeIndex < _weapons.Count
            ? _weapons[_activeIndex]
            : null;

    public bool IsCharging => _isCharging;
    public float ChargeTimer => _chargeTimer;
    public int ActiveSmallShotCount => _activeSmallShots.Count;

    void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _playerInput = GetComponent<PlayerInput>();
        _weaponNextAction = _playerInput.actions["WeaponNext"];
        _weaponPrevAction = _playerInput.actions["WeaponPrev"];
    }

    void OnEnable()
    {
        if (_weaponNextAction != null) _weaponNextAction.started += OnWeaponNext;
        if (_weaponPrevAction != null) _weaponPrevAction.started += OnWeaponPrev;
    }

    void OnDisable()
    {
        if (_weaponNextAction != null) _weaponNextAction.started -= OnWeaponNext;
        if (_weaponPrevAction != null) _weaponPrevAction.started -= OnWeaponPrev;
    }

    public void Initialize(SpriteRenderer sr)
    {
        _spriteRenderer = sr;
        ApplyWeaponTint();
    }

    public void StartCharge()
    {
        if (_controller.IsKnockedBack) return;
        _isCharging = true;
        _chargeTimer = 0f;
    }

    public bool ReleaseCharge()
    {
        if (!_isCharging) return false;
        _isCharging = false;

        var weapon = ActiveWeapon;
        if (weapon)
        {
            if (_chargeTimer >= _fullChargeTime)
                Spawn(weapon.fullPrefab, isSmall: false);
            else if (_chargeTimer >= _semiChargeTime)
                Spawn(weapon.semiPrefab, isSmall: false);
            else if (_activeIndex != 0 || _activeSmallShots.Count < _maxSmallShots)
                Spawn(weapon.smallPrefab, isSmall: true);
        }

        _chargeTimer = 0f;
        RestoreColor();
        return true;
    }

    public void CancelCharge()
    {
        if (!_isCharging) return;
        _isCharging = false;
        _chargeTimer = 0f;
        RestoreColor();
    }

    void Update()
    {
        if (_isCharging) _chargeTimer += Time.deltaTime;
        UpdateChargeFlash();
    }

    void OnWeaponNext(InputAction.CallbackContext _) => CycleWeapon(+1);
    void OnWeaponPrev(InputAction.CallbackContext _) => CycleWeapon(-1);

    void CycleWeapon(int direction)
    {
        if (_weapons.Count <= 1) return;
        CancelCharge();
        _activeIndex = (_activeIndex + direction + _weapons.Count) % _weapons.Count;
        ApplyWeaponTint();
    }

    void Spawn(GameObject prefab, bool isSmall)
    {
        if (!prefab) return;
        var muzzle = _controller.MuzzleAnchor;
        var go = Instantiate(prefab, muzzle.position, muzzle.rotation);
        if (!isSmall || _activeIndex != 0) return;
        if (!go.TryGetComponent<Projectile>(out var shot)) return;
        _activeSmallShots.Add(shot);
        shot.Destroyed += () => _activeSmallShots.Remove(shot);
    }

    void UpdateChargeFlash()
    {
        if (!_spriteRenderer) return;
        if (!_isCharging)
        {
            _spriteRenderer.color = IdleColor;
            return;
        }

        if (_chargeTimer >= _fullChargeTime)
        {
            bool phase = Mathf.FloorToInt(_chargeTimer / _flashPeriod) % 2 == 0;
            _spriteRenderer.color = phase ? _fullFlashColor : Color.white;
        }
        else if (_chargeTimer >= _semiChargeTime)
        {
            bool phase = Mathf.FloorToInt(_chargeTimer / _flashPeriod) % 2 == 0;
            _spriteRenderer.color = phase ? _semiFlashColor : IdleColor;
        }
        else
        {
            _spriteRenderer.color = IdleColor;
        }
    }

    void RestoreColor()
    {
        if (_spriteRenderer) _spriteRenderer.color = IdleColor;
    }

    void ApplyWeaponTint()
    {
        if (_spriteRenderer) _spriteRenderer.color = IdleColor;
    }

    Color IdleColor => ActiveWeapon ? ActiveWeapon.tint : Color.white;
}
