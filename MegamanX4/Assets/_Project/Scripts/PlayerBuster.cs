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
    [SerializeField] Color semiFlashColor = Color.white;
    [SerializeField] Color fullFlashColor = new(0.4f, 1f, 1f);
    [SerializeField] float flashPeriod = 0.08f;

    PlayerController _controller;
    SpriteRenderer spriteRenderer;

    bool isCharging;
    float chargeTimer;
    Color baseSpriteColor = Color.white;
    readonly List<BusterShot> activeSmallShots = new();

    public bool IsCharging => isCharging;
    public float ChargeTimer => chargeTimer;
    public int ActiveSmallShotCount => activeSmallShots.Count;

    void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    public void Initialize(SpriteRenderer sr)
    {
        spriteRenderer = sr;
        if (spriteRenderer) baseSpriteColor = spriteRenderer.color;
    }

    public void StartCharge()
    {
        if (_controller.IsKnockedBack) return;
        isCharging = true;
        chargeTimer = 0f;
    }

    public bool ReleaseCharge()
    {
        if (!isCharging) return false;
        isCharging = false;

        if (chargeTimer >= _fullChargeTime)
            Spawn(_fullShotPrefab, isSmall: false);
        else if (chargeTimer >= _semiChargeTime)
            Spawn(_semiShotPrefab, isSmall: false);
        else if (activeSmallShots.Count < _maxSmallShots)
            Spawn(_smallShotPrefab, isSmall: true);

        chargeTimer = 0f;
        RestoreColor();
        return true;
    }

    public void CancelCharge()
    {
        if (!isCharging) return;
        isCharging = false;
        chargeTimer = 0f;
        RestoreColor();
    }

    void Update()
    {
        if (isCharging) chargeTimer += Time.deltaTime;
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
            activeSmallShots.Add(shot);
            shot.Destroyed += () => activeSmallShots.Remove(shot);
        }
    }

    void UpdateChargeFlash()
    {
        if (!spriteRenderer) return;
        if (!isCharging)
        {
            spriteRenderer.color = baseSpriteColor;
            return;
        }

        if (chargeTimer >= _fullChargeTime)
        {
            bool phase = Mathf.FloorToInt(chargeTimer / flashPeriod) % 2 == 0;
            spriteRenderer.color = phase ? fullFlashColor : Color.white;
        }
        else if (chargeTimer >= _semiChargeTime)
        {
            bool phase = Mathf.FloorToInt(chargeTimer / flashPeriod) % 2 == 0;
            spriteRenderer.color = phase ? semiFlashColor : baseSpriteColor;
        }
        else
        {
            spriteRenderer.color = baseSpriteColor;
        }
    }

    void RestoreColor()
    {
        if (spriteRenderer) 
            spriteRenderer.color = baseSpriteColor;
    }
}
