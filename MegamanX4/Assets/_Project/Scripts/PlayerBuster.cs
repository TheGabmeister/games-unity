using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerBuster : MonoBehaviour
{
    [Header("Projectiles")]
    [SerializeField] GameObject _smallShotPrefab;
    [SerializeField] GameObject _semiShotPrefab;
    [SerializeField] GameObject _fullShotPrefab;

    [Header("Charge")]
    [SerializeField] float _semiChargeTime = 0.4f;
    [SerializeField] float _fullChargeTime = 1.2f;

    [Header("On-screen cap")]
    [SerializeField] int _maxSmallShots = 3;

    [Header("Charge flash")]
    [SerializeField] Color _semiFlashColor = Color.white;
    [SerializeField] Color _fullFlashColor = new(0.4f, 1f, 1f);
    [SerializeField] float _flashPeriod = 0.08f;

    PlayerController _controller;
    SpriteRenderer _spriteRenderer;

    bool _isCharging;
    float _chargeTimer;
    Color _baseSpriteColor = Color.white;
    readonly List<BusterShot> _activeSmallShots = new();

    public bool IsCharging => _isCharging;
    public float ChargeTimer => _chargeTimer;
    public int ActiveSmallShotCount => _activeSmallShots.Count;

    void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    public void Initialize(SpriteRenderer sr)
    {
        _spriteRenderer = sr;
        if (_spriteRenderer) _baseSpriteColor = _spriteRenderer.color;
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

        if (_chargeTimer >= _fullChargeTime)
            Spawn(_fullShotPrefab, isSmall: false);
        else if (_chargeTimer >= _semiChargeTime)
            Spawn(_semiShotPrefab, isSmall: false);
        else if (_activeSmallShots.Count < _maxSmallShots)
            Spawn(_smallShotPrefab, isSmall: true);

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

    void Spawn(GameObject prefab, bool isSmall)
    {
        if (!prefab) return;
        var muzzle = _controller.MuzzleAnchor;
        var go = Instantiate(prefab, muzzle.transform.position, muzzle.transform.rotation);
        if (!go.TryGetComponent<BusterShot>(out var shot)) return;
        shot.Fire();
        if (isSmall)
        {
            _activeSmallShots.Add(shot);
            shot.Destroyed += () => _activeSmallShots.Remove(shot);
        }
    }

    void UpdateChargeFlash()
    {
        if (!_spriteRenderer) return;
        if (!_isCharging)
        {
            _spriteRenderer.color = _baseSpriteColor;
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
            _spriteRenderer.color = phase ? _semiFlashColor : _baseSpriteColor;
        }
        else
        {
            _spriteRenderer.color = _baseSpriteColor;
        }
    }

    void RestoreColor()
    {
        if (_spriteRenderer)
            _spriteRenderer.color = _baseSpriteColor;
    }
}
