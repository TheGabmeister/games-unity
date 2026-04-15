using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
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

    [Header("Collision")]
    [SerializeField] LayerMask environmentLayers = ~0;
    [SerializeField] float skinWidth = 0.02f;
    [SerializeField] float probeDistance = 0.05f;

    Rigidbody2D rb;
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction sprintAction;

    ContactFilter2D contactFilter;
    readonly RaycastHit2D[] castHits = new RaycastHit2D[8];

    Vector2 velocity;
    Vector2 moveInput;
    bool jumpHeld;
    int facing = 1;

    bool isGrounded;
    bool isTouchingWall;
    bool WallSliding => isTouchingWall && !isGrounded && velocity.y < 0f;
    bool IsDashing => dashTimer > 0f;

    float coyoteTimer;
    float jumpBufferTimer;
    float dashTimer;
    float dashCooldownTimer;
    int dashDirection;
    float wallJumpLockTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];

        contactFilter = new ContactFilter2D { useLayerMask = true, useTriggers = false };
        contactFilter.SetLayerMask(environmentLayers);
    }

    void OnEnable()
    {
        jumpAction.started += OnJumpStarted;
        jumpAction.canceled += OnJumpCanceled;
        sprintAction.started += OnSprintStarted;
    }

    void OnDisable()
    {
        jumpAction.started -= OnJumpStarted;
        jumpAction.canceled -= OnJumpCanceled;
        sprintAction.started -= OnSprintStarted;
    }

    void OnJumpStarted(InputAction.CallbackContext _)
    {
        jumpHeld = true;
        jumpBufferTimer = jumpBufferTime;
    }

    void OnJumpCanceled(InputAction.CallbackContext _) => jumpHeld = false;

    void OnSprintStarted(InputAction.CallbackContext _) => TryStartDash();

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        coyoteTimer -= Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;
        dashTimer -= Time.deltaTime;
        dashCooldownTimer -= Time.deltaTime;
        wallJumpLockTimer -= Time.deltaTime;

        if (moveInput.x > 0.1f) facing = 1;
        else if (moveInput.x < -0.1f) facing = -1;

        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * facing;
        transform.localScale = s;
    }

    void FixedUpdate()
    {
        Probe();

        if (isGrounded) coyoteTimer = coyoteTime;

        if (jumpBufferTimer > 0f && TryJump())
            jumpBufferTimer = 0f;

        ApplyGravity();

        if (!jumpHeld && velocity.y > 0f && !IsDashing)
            velocity.y *= jumpCutMultiplier;

        if (IsDashing)
            velocity = new Vector2(dashDirection * dashSpeed, 0f);
        else if (wallJumpLockTimer <= 0f)
            ApplyHorizontalInput();

        if (WallSliding)
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
        if (isTouchingWall && !isGrounded)
        {
            velocity = new Vector2(-facing * wallJumpVelocity.x, wallJumpVelocity.y);
            wallJumpLockTimer = wallJumpLockTime;
            facing = -facing;
            coyoteTimer = 0f;
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
        if (dashCooldownTimer > 0f || IsDashing) return;
        dashTimer = dashDuration;
        dashCooldownTimer = dashDuration + dashCooldown;
        dashDirection = Mathf.Abs(moveInput.x) > 0.1f ? (int)Mathf.Sign(moveInput.x) : facing;
    }
}
