using UnityEngine;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(WeaponInventory))]
public class PlayerController : MonoBehaviour
{
    public event Action Died;

    [Header("Visuals")]
    [SerializeField] Transform _visual;
    [SerializeField] Sprite _idleSprite;
    [SerializeField] Sprite _jumpSprite;
    [SerializeField] Sprite _fallSprite;
    [SerializeField] Sprite _dashSprite;

    SpriteRenderer _spriteRenderer;

    [Header("Movement")]
    [SerializeField] float _moveSpeed = 6f;
    [SerializeField] float _groundAcceleration = 60f;
    [SerializeField] float _airAcceleration = 30f;

    [Header("Jump")]
    [SerializeField] float _jumpSpeed = 12f;
    [SerializeField] float _jumpCutMultiplier = 0.5f;
    [SerializeField] float _coyoteTime = 0.1f;
    [SerializeField] float _jumpBufferTime = 0.1f;
    [SerializeField] float _fallGravityMultiplier = 1.8f;

    [Header("Gravity")]
    [SerializeField] float _gravity = 40f;
    [SerializeField] float _maxFallSpeed = 20f;

    [Header("Dash")]
    [SerializeField] float _dashSpeed = 14f;
    [SerializeField] float _dashDuration = 0.35f;
    [SerializeField] float _dashCooldown = 0.15f;

    [Header("Wall")]
    [SerializeField] float _wallSlideSpeed = 2f;
    [SerializeField] Vector2 _wallJumpVelocity = new(8f, 11f);
    [SerializeField] float _wallJumpLockTime = 0.18f;

    [Header("Knockback")]
    [SerializeField] float _knockbackSpeedX = 5f;
    [SerializeField] float _knockbackSpeedY = 6f;
    [SerializeField] float _knockbackDuration = 0.35f;

    [Header("Ladder")]
    [SerializeField] LayerMask _ladderLayer;
    [SerializeField] float _climbSpeed = 5f;
    [SerializeField] float _ladderShootLockTime = 0.2f;
    [SerializeField] Sprite _climbSprite;
    [SerializeField] Sprite _climbShootSprite;

    [Header("Collision")]
    [SerializeField] LayerMask _environmentLayers = ~0;
    [SerializeField] float _skinWidth = 0.02f;
    [SerializeField] float _probeDistance = 0.05f;

    [Header("Shooting")]
    [SerializeField] Transform _muzzleAnchor;

    Rigidbody2D _rb;
    PlayerInput _playerInput;
    WeaponInventory _inventory;
    InputAction _moveAction;
    InputAction _jumpAction;
    InputAction _sprintAction;
    InputAction _attackAction;

    ContactFilter2D _contactFilter;
    readonly RaycastHit2D[] _castHits = new RaycastHit2D[8];

    Vector2 _velocity;
    Vector2 _moveInput;
    bool _jumpHeld;
    int _facing = 1;

    bool _isGrounded;
    bool _isTouchingWall;
    bool WallSliding => _isTouchingWall && !_isGrounded && _velocity.y < 0f;
    public bool IsDashing => _dashTimer > 0f;

    float _coyoteTimer;
    float _jumpBufferTimer;
    float _dashTimer;
    float _dashCooldownTimer;
    int _dashDirection;
    float _wallJumpLockTimer;
    float _knockbackTimer;
    public bool IsKnockedBack => _knockbackTimer > 0f;
    public int Facing => _facing;
    public bool OnLadder => _onLadder;
    public Transform MuzzleAnchor => _muzzleAnchor;

    bool _dashJumpLock;

    Collider2D _currentLadder;
    bool _onLadder;
    bool _climbingShootLock;
    float _ladderShootLockUntil;

    Collider2D _playerCollider;
    Health _health;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;

        _playerInput = GetComponent<PlayerInput>();
        _moveAction = _playerInput.actions["Move"];
        _jumpAction = _playerInput.actions["Jump"];
        _sprintAction = _playerInput.actions["Sprint"];
        _attackAction = _playerInput.actions["Attack"];

        _health = GetComponent<Health>();
        _playerCollider = GetComponent<Collider2D>();
        _inventory = GetComponent<WeaponInventory>();

        _contactFilter = new ContactFilter2D { useLayerMask = true, useTriggers = false };
        _contactFilter.SetLayerMask(_environmentLayers);

        if (_visual) _spriteRenderer = _visual.GetComponent<SpriteRenderer>();
        _inventory.Initialize(_spriteRenderer);
    }

    void UpdateSprite()
    {
        if (!_spriteRenderer) return;
        Sprite s = _onLadder ? (_climbingShootLock ? _climbShootSprite : _climbSprite)
                 : IsDashing ? _dashSprite
                 : !_isGrounded && _velocity.y > 0.01f ? _jumpSprite
                 : !_isGrounded ? _fallSprite
                 : _idleSprite;
        if (s) _spriteRenderer.sprite = s;
    }

    void OnEnable()
    {
        _jumpAction.started += OnJumpStarted;
        _jumpAction.canceled += OnJumpCanceled;
        _sprintAction.started += OnSprintStarted;
        _attackAction.started += OnAttackStarted;
        _attackAction.canceled += OnAttackCanceled;
        _health.Damaged += OnHealthDamaged;
        _health.Depleted += OnHealthDepleted;
    }

    void OnDisable()
    {
        _jumpAction.started -= OnJumpStarted;
        _jumpAction.canceled -= OnJumpCanceled;
        _sprintAction.started -= OnSprintStarted;
        _attackAction.started -= OnAttackStarted;
        _attackAction.canceled -= OnAttackCanceled;
        _health.Damaged -= OnHealthDamaged;
        _health.Depleted -= OnHealthDepleted;
    }

    void OnHealthDamaged(int amount, Vector2 sourcePosition)
    {
        if (_onLadder) { ExitLadder(fall: true); return; }
        ApplyKnockback(sourcePosition);
    }

    void OnHealthDepleted()
    {
        Died?.Invoke();
        Destroy(gameObject);
    }

    Collider2D QueryLadder()
    {
        var b = _playerCollider.bounds;
        return Physics2D.OverlapBox(b.center, b.size, 0f, _ladderLayer);
    }

    bool AtLadderTop(Collider2D ladder)
    {
        float feet = _rb.position.y - _playerCollider.bounds.extents.y;
        return Mathf.Abs(feet - ladder.bounds.max.y) < 0.1f;
    }

    void TryGrabLadder(Collider2D ladder)
    {
        if (ladder == null) return;
        bool pressingUp   = _moveInput.y >  0.5f;
        bool pressingDown = _moveInput.y < -0.5f;
        bool grab = pressingUp
                 || (pressingDown && !_isGrounded)
                 || (pressingDown && _isGrounded && AtLadderTop(ladder));
        if (grab) EnterLadder(ladder);
    }

    void EnterLadder(Collider2D ladder)
    {
        _currentLadder = ladder;
        _onLadder = true;
        _rb.position = new Vector2(ladder.bounds.center.x, _rb.position.y);
        _velocity = Vector2.zero;
        _dashJumpLock = false;
        _wallJumpLockTimer = 0f;
        _coyoteTimer = 0f;
        _dashTimer = 0f;
        _climbingShootLock = false;
        _ladderShootLockUntil = 0f;
    }

    void ExitLadder(bool fall)
    {
        _onLadder = false;
        _currentLadder = null;
        _climbingShootLock = false;
        if (fall) _velocity = Vector2.zero;
    }

    void TickLadder()
    {
        if (_climbingShootLock && Time.time >= _ladderShootLockUntil)
            _climbingShootLock = false;

        // Facing updates from input even while locked so next shot aims correctly;
        // no horizontal motion is applied.
        if (_moveInput.x >  0.1f) _facing =  1;
        else if (_moveInput.x < -0.1f) _facing = -1;

        // Jump off ladder: exit + normal jump; inherit walk speed only if a direction is held.
        if (_jumpBufferTimer > 0f)
        {
            _jumpBufferTimer = 0f;
            _velocity = new Vector2(_moveInput.x * _moveSpeed, _jumpSpeed);
            ExitLadder(fall: false);
            return;
        }

        // Auto-dismount at top rung.
        if (_rb.position.y >= _currentLadder.bounds.max.y)
        {
            _rb.position = new Vector2(_rb.position.x, _currentLadder.bounds.max.y + _playerCollider.bounds.extents.y);
            _velocity = Vector2.zero;
            ExitLadder(fall: false);
            return;
        }

        // Drop off at bottom rung when pressing Down.
        if (_moveInput.y < -0.5f && _rb.position.y <= _currentLadder.bounds.min.y)
        {
            ExitLadder(fall: true);
            return;
        }

        _velocity = _climbingShootLock
            ? Vector2.zero
            : new Vector2(0f, _moveInput.y * _climbSpeed);

        Move(_velocity * Time.fixedDeltaTime);
    }

    public void ApplyKnockback(Vector2 sourcePosition)
    {
        int dir;
        if (WallSliding)
            dir = -_facing;
        else
            dir = _rb.position.x >= sourcePosition.x ? 1 : -1;

        _velocity = new Vector2(dir * _knockbackSpeedX, _knockbackSpeedY);
        _knockbackTimer = _knockbackDuration;

        _dashTimer = 0f;
        _dashJumpLock = false;
        _wallJumpLockTimer = 0f;
        _jumpBufferTimer = 0f;

        _inventory.CancelCharge();

        _facing = -dir;
    }

    void OnJumpStarted(InputAction.CallbackContext _)
    {
        _jumpHeld = true;
        _jumpBufferTimer = _jumpBufferTime;
    }

    void OnJumpCanceled(InputAction.CallbackContext _) => _jumpHeld = false;

    void OnSprintStarted(InputAction.CallbackContext _) => TryStartDash();

    void OnAttackStarted(InputAction.CallbackContext _) => _inventory.StartCharge();

    void OnAttackCanceled(InputAction.CallbackContext _)
    {
        if (!_inventory.ReleaseCharge()) return;

        if (_onLadder)
        {
            _climbingShootLock = true;
            _ladderShootLockUntil = Time.time + _ladderShootLockTime;
        }
    }

    void Update()
    {
        _moveInput = _moveAction.ReadValue<Vector2>();

        _coyoteTimer -= Time.deltaTime;
        _jumpBufferTimer -= Time.deltaTime;
        _dashTimer -= Time.deltaTime;
        _dashCooldownTimer -= Time.deltaTime;
        _wallJumpLockTimer -= Time.deltaTime;
        _knockbackTimer -= Time.deltaTime;

        if (!IsKnockedBack)
        {
            if (_moveInput.x > 0.1f) _facing = 1;
            else if (_moveInput.x < -0.1f) _facing = -1;
        }

        if (_visual)
            _visual.localRotation = _facing >= 0 ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);

        UpdateSprite();
    }

    void FixedUpdate()
    {
        Probe();

        var ladder = QueryLadder();
        if (!_onLadder) TryGrabLadder(ladder);

        if (_onLadder)
        {
            TickLadder();
            return;
        }

        if (_isGrounded)
        {
            _coyoteTimer = _coyoteTime;
            _dashJumpLock = false;
        }

        if (_jumpBufferTimer > 0f && TryJump())
            _jumpBufferTimer = 0f;

        ApplyGravity();

        if (!_jumpHeld && _velocity.y > 0f && !IsDashing)
            _velocity.y *= _jumpCutMultiplier;

        if (IsKnockedBack)
        {
            // velocity set by ApplyKnockback; let it ride. Gravity still applies.
        }
        else if (IsDashing)
            _velocity = new Vector2(_dashDirection * _dashSpeed, 0f);
        else if (_dashJumpLock)
            _velocity.x = _dashDirection * _dashSpeed;
        else if (_wallJumpLockTimer <= 0f)
            ApplyHorizontalInput();

        if (WallSliding && !IsKnockedBack && !_onLadder)
            _velocity.y = Mathf.Max(_velocity.y, -_wallSlideSpeed);

        Move(_velocity * Time.fixedDeltaTime);
    }

    void ApplyHorizontalInput()
    {
        float target = _moveInput.x * _moveSpeed;
        float accel = _isGrounded ? _groundAcceleration : _airAcceleration;
        _velocity.x = Mathf.MoveTowards(_velocity.x, target, accel * Time.fixedDeltaTime);
    }

    void ApplyGravity()
    {
        if (IsDashing) return;
        float g = _velocity.y < 0f ? _gravity * _fallGravityMultiplier : _gravity;
        _velocity.y = Mathf.Max(_velocity.y - g * Time.fixedDeltaTime, -_maxFallSpeed);
    }

    void Probe()
    {
        int downCount = _rb.Cast(Vector2.down, _contactFilter, _castHits, _probeDistance);
        _isGrounded = downCount > 0 && _velocity.y <= 0.0001f;

        int sideCount = _rb.Cast(new Vector2(_facing, 0f), _contactFilter, _castHits, _probeDistance);
        _isTouchingWall = sideCount > 0 && _moveInput.x * _facing > 0.1f;
    }

    void Move(Vector2 delta)
    {
        MoveAxis(new Vector2(delta.x, 0f), axisX: true);
        MoveAxis(new Vector2(0f, delta.y), axisX: false);
    }

    void MoveAxis(Vector2 delta, bool axisX)
    {
        float magnitude = delta.magnitude;
        if (magnitude < 0.0001f) return;
        Vector2 dir = delta / magnitude;

        int count = _rb.Cast(dir, _contactFilter, _castHits, magnitude + _skinWidth);
        float travel = magnitude;
        for (int i = 0; i < count; i++)
        {
            float d = _castHits[i].distance - _skinWidth;
            if (d < travel) travel = Mathf.Max(0f, d);
        }

        _rb.position += dir * travel;

        if (travel < magnitude - 0.0001f)
        {
            if (axisX) _velocity.x = 0f;
            else _velocity.y = 0f;
        }
    }

    bool TryJump()
    {
        if (IsKnockedBack || _onLadder) return false;

        if (IsDashing)
        {
            _velocity = new Vector2(_dashDirection * _dashSpeed, _jumpSpeed);
            _dashTimer = 0f;
            _dashJumpLock = true;
            _coyoteTimer = 0f;
            return true;
        }

        if (_isTouchingWall && !_isGrounded)
        {
            _velocity = new Vector2(-_facing * _wallJumpVelocity.x, _wallJumpVelocity.y);
            _wallJumpLockTimer = _wallJumpLockTime;
            _facing = -_facing;
            _coyoteTimer = 0f;
            _dashJumpLock = false;
            return true;
        }

        if (_coyoteTimer > 0f)
        {
            _velocity.y = _jumpSpeed;
            _coyoteTimer = 0f;
            return true;
        }

        return false;
    }

    void TryStartDash()
    {
        if (_dashCooldownTimer > 0f || IsDashing || IsKnockedBack || _onLadder) return;
        _dashTimer = _dashDuration;
        _dashCooldownTimer = _dashDuration + _dashCooldown;
        _dashDirection = Mathf.Abs(_moveInput.x) > 0.1f ? (int)Mathf.Sign(_moveInput.x) : _facing;
        _coyoteTimer = 0f;   // dashing off a ledge must not preserve coyote (SPEC.md §1.2)
    }
}
