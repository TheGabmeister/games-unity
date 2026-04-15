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

    [Header("Dash")]
    [SerializeField] float dashSpeed = 14f;
    [SerializeField] float dashDuration = 0.35f;
    [SerializeField] float dashCooldown = 0.15f;

    [Header("Wall")]
    [SerializeField] float wallSlideSpeed = 2f;
    [SerializeField] Vector2 wallJumpVelocity = new(8f, 11f);
    [SerializeField] float wallJumpLockTime = 0.18f;

    [Header("Ground / Wall Checks")]
    [SerializeField] Transform groundCheck;
    [SerializeField] Transform wallCheck;
    [SerializeField] float groundCheckRadius = 0.15f;
    [SerializeField] float wallCheckRadius = 0.12f;
    [SerializeField] LayerMask environmentLayers = ~0;

    Rigidbody2D rb;
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction sprintAction;

    Vector2 moveInput;
    bool jumpHeld;
    int facing = 1;

    bool isGrounded;
    bool isTouchingWall;
    bool WallSliding => isTouchingWall && !isGrounded && rb.linearVelocity.y < 0f;
    bool IsDashing => dashTimer > 0f;

    float coyoteTimer;
    float jumpBufferTimer;
    float dashTimer;
    float dashCooldownTimer;
    int dashDirection;
    float wallJumpLockTimer;

    float baseGravityScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];
        baseGravityScale = rb.gravityScale;
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
        UpdateContacts();

        if (isGrounded) coyoteTimer = coyoteTime;

        if (jumpBufferTimer > 0f && TryJump())
            jumpBufferTimer = 0f;

        if (!jumpHeld && rb.linearVelocity.y > 0f && !IsDashing)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        if (IsDashing)
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
        else if (wallJumpLockTimer <= 0f)
            ApplyHorizontalMovement();

        if (WallSliding)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));

        rb.gravityScale = IsDashing ? 0f
            : rb.linearVelocity.y < 0f ? baseGravityScale * fallGravityMultiplier
            : baseGravityScale;
    }

    void ApplyHorizontalMovement()
    {
        float target = moveInput.x * moveSpeed;
        float accel = isGrounded ? groundAcceleration : airAcceleration;
        float x = Mathf.MoveTowards(rb.linearVelocity.x, target, accel * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
    }

    void UpdateContacts()
    {
        isGrounded = groundCheck &&
            Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, environmentLayers);

        isTouchingWall = wallCheck &&
            Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, environmentLayers) &&
            moveInput.x * facing > 0.1f;
    }

    bool TryJump()
    {
        if (isTouchingWall && !isGrounded)
        {
            rb.linearVelocity = new Vector2(-facing * wallJumpVelocity.x, wallJumpVelocity.y);
            wallJumpLockTimer = wallJumpLockTime;
            facing = -facing;
            coyoteTimer = 0f;
            return true;
        }

        if (coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpSpeed);
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (groundCheck) Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        if (wallCheck) Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
    }
}
