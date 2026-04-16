using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Health))]
public class PlayerController : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] Transform visual;
    [SerializeField] Sprite idleSprite;
    [SerializeField] Sprite jumpSprite;
    [SerializeField] Sprite fallSprite;
    [SerializeField] Sprite dashSprite;

    SpriteRenderer spriteRenderer;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float groundAcceleration = 60f;
    [SerializeField] float airAcceleration = 30f;

    [Header("Jump")]
    [SerializeField] float jumpSpeed = 12f;
    [SerializeField] float jumpCutMultiplier = 0.5f;
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float jumpBufferTime = 0.1f;
    [SerializeField] float fallGravityMultiplier = 1.8f;

    [Header("Gravity")]
    [SerializeField] float gravity = 40f;
    [SerializeField] float maxFallSpeed = 20f;

    [Header("Dash")]
    [SerializeField] float dashSpeed = 14f;
    [SerializeField] float dashDuration = 0.35f;
    [SerializeField] float dashCooldown = 0.15f;

    [Header("Wall")]
    [SerializeField] float wallSlideSpeed = 2f;
    [SerializeField] Vector2 wallJumpVelocity = new(8f, 11f);
    [SerializeField] float wallJumpLockTime = 0.18f;

    [Header("Knockback")]
    [SerializeField] float knockbackSpeedX = 5f;
    [SerializeField] float knockbackSpeedY = 6f;
    [SerializeField] float knockbackDuration = 0.35f;

    [Header("Ladder")]
    [SerializeField] LayerMask ladderLayer;
    [SerializeField] float climbSpeed = 5f;
    [SerializeField] float ladderShootLockTime = 0.2f;
    [SerializeField] Sprite climbSprite;
    [SerializeField] Sprite climbShootSprite;

    [Header("Collision")]
    [SerializeField] LayerMask environmentLayers = ~0;
    [SerializeField] float skinWidth = 0.02f;
    [SerializeField] float probeDistance = 0.05f;

    [Header("Shooting")]
    [SerializeField] GameObject smallShotPrefab;
    [SerializeField] GameObject semiShotPrefab;
    [SerializeField] GameObject fullShotPrefab;
    [SerializeField] Transform muzzleAnchor;
    [SerializeField] float semiChargeTime = 0.4f;
    [SerializeField] float fullChargeTime = 1.2f;
    [SerializeField] int maxSmallShots = 3;
    [SerializeField] Color semiFlashColor = Color.white;
    [SerializeField] Color fullFlashColor = new(0.4f, 1f, 1f);
    [SerializeField] float flashPeriod = 0.08f;

    Rigidbody2D rb;
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction sprintAction;
    InputAction attackAction;

    bool isCharging;
    float chargeTimer;
    Color baseSpriteColor = Color.white;
    readonly List<BusterShot> activeSmallShots = new();

    ContactFilter2D contactFilter;
    readonly RaycastHit2D[] castHits = new RaycastHit2D[8];

    Vector2 velocity;
    Vector2 moveInput;
    bool jumpHeld;
    int facing = 1;

    bool isGrounded;
    bool isTouchingWall;
    bool WallSliding => isTouchingWall && !isGrounded && velocity.y < 0f;
    public bool IsDashing => dashTimer > 0f;

    float coyoteTimer;
    float jumpBufferTimer;
    float dashTimer;
    float dashCooldownTimer;
    int dashDirection;
    float wallJumpLockTimer;
    float knockbackTimer;
    bool IsKnockedBack => knockbackTimer > 0f;

    bool dashJumpLock;

    Collider2D currentLadder;
    bool onLadder;
    bool climbingShootLock;
    float ladderShootLockUntil;

    Collider2D playerCollider;
    Health health;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];
        attackAction = playerInput.actions["Attack"];

        health = GetComponent<Health>();
        playerCollider = GetComponent<Collider2D>();

        contactFilter = new ContactFilter2D { useLayerMask = true, useTriggers = false };
        contactFilter.SetLayerMask(environmentLayers);

        if (visual) spriteRenderer = visual.GetComponent<SpriteRenderer>();
        if (spriteRenderer) baseSpriteColor = spriteRenderer.color;
    }

    void UpdateSprite()
    {
        if (!spriteRenderer) return;
        Sprite s = onLadder ? (climbingShootLock ? climbShootSprite : climbSprite)
                 : IsDashing ? dashSprite
                 : !isGrounded && velocity.y > 0.01f ? jumpSprite
                 : !isGrounded ? fallSprite
                 : idleSprite;
        if (s) spriteRenderer.sprite = s;
    }

    void OnEnable()
    {
        jumpAction.started += OnJumpStarted;
        jumpAction.canceled += OnJumpCanceled;
        sprintAction.started += OnSprintStarted;
        attackAction.started += OnAttackStarted;
        attackAction.canceled += OnAttackCanceled;
        health.Damaged += OnHealthDamaged;
    }

    void OnDisable()
    {
        jumpAction.started -= OnJumpStarted;
        jumpAction.canceled -= OnJumpCanceled;
        sprintAction.started -= OnSprintStarted;
        attackAction.started -= OnAttackStarted;
        attackAction.canceled -= OnAttackCanceled;
        health.Damaged -= OnHealthDamaged;
    }

    void OnHealthDamaged(int amount, Vector2 sourcePosition)
    {
        if (onLadder) { ExitLadder(fall: true); return; }
        ApplyKnockback(sourcePosition);
    }

    Collider2D QueryLadder()
    {
        var b = playerCollider.bounds;
        return Physics2D.OverlapBox(b.center, b.size, 0f, ladderLayer);
    }

    bool AtLadderTop(Collider2D ladder)
    {
        float feet = rb.position.y - playerCollider.bounds.extents.y;
        return Mathf.Abs(feet - ladder.bounds.max.y) < 0.1f;
    }

    void TryGrabLadder(Collider2D ladder)
    {
        if (ladder == null) return;
        bool pressingUp   = moveInput.y >  0.5f;
        bool pressingDown = moveInput.y < -0.5f;
        bool grab = pressingUp
                 || (pressingDown && !isGrounded)
                 || (pressingDown && isGrounded && AtLadderTop(ladder));
        if (grab) EnterLadder(ladder);
    }

    void EnterLadder(Collider2D ladder)
    {
        currentLadder = ladder;
        onLadder = true;
        rb.position = new Vector2(ladder.bounds.center.x, rb.position.y);
        velocity = Vector2.zero;
        dashJumpLock = false;
        wallJumpLockTimer = 0f;
        coyoteTimer = 0f;
        dashTimer = 0f;
        climbingShootLock = false;
        ladderShootLockUntil = 0f;
    }

    void ExitLadder(bool fall)
    {
        onLadder = false;
        currentLadder = null;
        climbingShootLock = false;
        if (fall) velocity = Vector2.zero;
    }

    void TickLadder()
    {
        if (climbingShootLock && Time.time >= ladderShootLockUntil)
            climbingShootLock = false;

        // Facing updates from input even while locked so next shot aims correctly;
        // no horizontal motion is applied.
        if (moveInput.x >  0.1f) facing =  1;
        else if (moveInput.x < -0.1f) facing = -1;

        // Jump off ladder: exit + normal jump; inherit walk speed only if a direction is held.
        if (jumpBufferTimer > 0f)
        {
            jumpBufferTimer = 0f;
            velocity = new Vector2(moveInput.x * moveSpeed, jumpSpeed);
            ExitLadder(fall: false);
            return;
        }

        // Auto-dismount at top rung.
        if (rb.position.y >= currentLadder.bounds.max.y)
        {
            rb.position = new Vector2(rb.position.x, currentLadder.bounds.max.y + playerCollider.bounds.extents.y);
            velocity = Vector2.zero;
            ExitLadder(fall: false);
            return;
        }

        // Drop off at bottom rung when pressing Down.
        if (moveInput.y < -0.5f && rb.position.y <= currentLadder.bounds.min.y)
        {
            ExitLadder(fall: true);
            return;
        }

        velocity = climbingShootLock
            ? Vector2.zero
            : new Vector2(0f, moveInput.y * climbSpeed);

        Move(velocity * Time.fixedDeltaTime);
    }

    public void ApplyKnockback(Vector2 sourcePosition)
    {
        int dir;
        if (WallSliding)
            dir = -facing;
        else
            dir = rb.position.x >= sourcePosition.x ? 1 : -1;

        velocity = new Vector2(dir * knockbackSpeedX, knockbackSpeedY);
        knockbackTimer = knockbackDuration;

        dashTimer = 0f;
        dashJumpLock = false;
        wallJumpLockTimer = 0f;
        jumpBufferTimer = 0f;

        if (isCharging)
        {
            isCharging = false;
            chargeTimer = 0f;
            if (spriteRenderer) spriteRenderer.color = baseSpriteColor;
        }

        facing = -dir;
    }

    void OnJumpStarted(InputAction.CallbackContext _)
    {
        jumpHeld = true;
        jumpBufferTimer = jumpBufferTime;
    }

    void OnJumpCanceled(InputAction.CallbackContext _) => jumpHeld = false;

    void OnSprintStarted(InputAction.CallbackContext _) => TryStartDash();

    void OnAttackStarted(InputAction.CallbackContext _)
    {
        if (IsKnockedBack) return;
        isCharging = true;
        chargeTimer = 0f;
    }

    void OnAttackCanceled(InputAction.CallbackContext _)
    {
        if (!isCharging) return;
        isCharging = false;

        if (chargeTimer >= fullChargeTime)
            Spawn(fullShotPrefab, isSmall: false);
        else if (chargeTimer >= semiChargeTime)
            Spawn(semiShotPrefab, isSmall: false);
        else if (activeSmallShots.Count < maxSmallShots)
            Spawn(smallShotPrefab, isSmall: true);

        chargeTimer = 0f;
        if (spriteRenderer) spriteRenderer.color = baseSpriteColor;

        if (onLadder)
        {
            climbingShootLock = true;
            ladderShootLockUntil = Time.time + ladderShootLockTime;
        }
    }

    void Spawn(GameObject prefab, bool isSmall)
    {
        if (!prefab) return;
        Vector2 pos = muzzleAnchor ? (Vector2)muzzleAnchor.position : (Vector2)transform.position;
        var go = Instantiate(prefab, pos, Quaternion.identity);
        if (!go.TryGetComponent<BusterShot>(out var shot)) return;
        shot.Fire(facing);
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

        if (chargeTimer >= fullChargeTime)
        {
            bool phase = Mathf.FloorToInt(chargeTimer / flashPeriod) % 2 == 0;
            spriteRenderer.color = phase ? fullFlashColor : Color.white;
        }
        else if (chargeTimer >= semiChargeTime)
        {
            bool phase = Mathf.FloorToInt(chargeTimer / flashPeriod) % 2 == 0;
            spriteRenderer.color = phase ? semiFlashColor : baseSpriteColor;
        }
        else
        {
            spriteRenderer.color = baseSpriteColor;
        }
    }

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        coyoteTimer -= Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;
        dashTimer -= Time.deltaTime;
        dashCooldownTimer -= Time.deltaTime;
        wallJumpLockTimer -= Time.deltaTime;
        knockbackTimer -= Time.deltaTime;

        if (!IsKnockedBack)
        {
            if (moveInput.x > 0.1f) facing = 1;
            else if (moveInput.x < -0.1f) facing = -1;
        }

        if (visual)
        {
            var s = visual.localScale;
            s.x = Mathf.Abs(s.x) * facing;
            visual.localScale = s;
        }

        if (isCharging) chargeTimer += Time.deltaTime;
        UpdateChargeFlash();
        UpdateSprite();
    }

    void FixedUpdate()
    {
        Probe();

        var ladder = QueryLadder();
        if (!onLadder) TryGrabLadder(ladder);

        if (onLadder)
        {
            TickLadder();
            return;
        }

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            dashJumpLock = false;
        }

        if (jumpBufferTimer > 0f && TryJump())
            jumpBufferTimer = 0f;

        ApplyGravity();

        if (!jumpHeld && velocity.y > 0f && !IsDashing)
            velocity.y *= jumpCutMultiplier;

        if (IsKnockedBack)
        {
            // velocity set by ApplyKnockback; let it ride. Gravity still applies.
        }
        else if (IsDashing)
            velocity = new Vector2(dashDirection * dashSpeed, 0f);
        else if (dashJumpLock)
            velocity.x = dashDirection * dashSpeed;
        else if (wallJumpLockTimer <= 0f)
            ApplyHorizontalInput();

        if (WallSliding && !IsKnockedBack && !onLadder)
            velocity.y = Mathf.Max(velocity.y, -wallSlideSpeed);

        Move(velocity * Time.fixedDeltaTime);
    }

    void ApplyHorizontalInput()
    {
        float target = moveInput.x * moveSpeed;
        float accel = isGrounded ? groundAcceleration : airAcceleration;
        velocity.x = Mathf.MoveTowards(velocity.x, target, accel * Time.fixedDeltaTime);
    }

    void ApplyGravity()
    {
        if (IsDashing) return;
        float g = velocity.y < 0f ? gravity * fallGravityMultiplier : gravity;
        velocity.y = Mathf.Max(velocity.y - g * Time.fixedDeltaTime, -maxFallSpeed);
    }

    void Probe()
    {
        int downCount = rb.Cast(Vector2.down, contactFilter, castHits, probeDistance);
        isGrounded = downCount > 0 && velocity.y <= 0.0001f;

        int sideCount = rb.Cast(new Vector2(facing, 0f), contactFilter, castHits, probeDistance);
        isTouchingWall = sideCount > 0 && moveInput.x * facing > 0.1f;
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

        int count = rb.Cast(dir, contactFilter, castHits, magnitude + skinWidth);
        float travel = magnitude;
        for (int i = 0; i < count; i++)
        {
            float d = castHits[i].distance - skinWidth;
            if (d < travel) travel = Mathf.Max(0f, d);
        }

        rb.position += dir * travel;

        if (travel < magnitude - 0.0001f)
        {
            if (axisX) velocity.x = 0f;
            else velocity.y = 0f;
        }
    }

    bool TryJump()
    {
        if (IsKnockedBack || onLadder) return false;

        if (IsDashing)
        {
            velocity = new Vector2(dashDirection * dashSpeed, jumpSpeed);
            dashTimer = 0f;
            dashJumpLock = true;
            coyoteTimer = 0f;
            return true;
        }

        if (isTouchingWall && !isGrounded)
        {
            velocity = new Vector2(-facing * wallJumpVelocity.x, wallJumpVelocity.y);
            wallJumpLockTimer = wallJumpLockTime;
            facing = -facing;
            coyoteTimer = 0f;
            dashJumpLock = false;
            return true;
        }

        if (coyoteTimer > 0f)
        {
            velocity.y = jumpSpeed;
            coyoteTimer = 0f;
            return true;
        }

        return false;
    }

    void TryStartDash()
    {
        if (dashCooldownTimer > 0f || IsDashing || IsKnockedBack || onLadder) return;
        dashTimer = dashDuration;
        dashCooldownTimer = dashDuration + dashCooldown;
        dashDirection = Mathf.Abs(moveInput.x) > 0.1f ? (int)Mathf.Sign(moveInput.x) : facing;
        coyoteTimer = 0f;   // dashing off a ledge must not preserve coyote (SPEC.md §1.2)
    }
}
