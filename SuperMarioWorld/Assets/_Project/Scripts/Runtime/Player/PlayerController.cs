using UnityEngine;

// Dynamic Rigidbody2D with gravityScale = 0 — Physics2D handles penetration
// resolution against solid colliders, but all acceleration is applied manually.
// See SPEC §4.2.
//
// Tuning constants are SMW-inspired defaults; the Phase 1 manual-verification
// gate is "run + jump feels responsive" — expect iteration after playtesting.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(GroundProbe))]
[RequireComponent(typeof(PlayerInputBinding))]
public sealed class PlayerController : MonoBehaviour
{
    [Header("Horizontal")]
    [SerializeField] private float walkMaxSpeed = 6f;
    [SerializeField] private float runMaxSpeed = 10f;
    [SerializeField] private float groundAccel = 80f;
    [SerializeField] private float airAccel = 50f;
    [SerializeField] private float groundFriction = 40f;
    [SerializeField] private float skidDecel = 100f;
    [SerializeField] private float skidSpeedThreshold = 4f;

    [Header("Vertical")]
    [SerializeField] private float gravity = 60f;
    [SerializeField] private float jumpHoldGravity = 20f;
    [SerializeField] private float jumpVelocity = 15f;
    [SerializeField] private float spinJumpVelocity = 12f;
    [SerializeField] private float maxFallSpeed = 22f;

    [Header("Timings (ticks @ 60Hz)")]
    [SerializeField] private int jumpHoldMaxTicks = 18;
    // Budget for "jump ticks after leaving ground" and "jump ticks before landing".
    // 7 gives the "6 frames post-ledge succeeds, 7 fails" cadence tested in Phase 1.
    [SerializeField] private int coyoteTicks = 7;
    [SerializeField] private int bufferTicks = 7;

    [Header("Slope")]
    [SerializeField] private float downhillSpeedBoost = 1.3f;

    private Rigidbody2D _rb;
    private GroundProbe _probe;
    private PlayerInputBinding _input;
    private PlayerCarry _carry;

    // Runtime state — exposed as read-only properties for tests + HUD.
    public bool IsGrounded => _probe != null && _probe.IsGrounded;
    public bool IsSpinJumping { get; private set; }
    public bool IsSkidding { get; private set; }
    public bool IsCrouching { get; private set; }
    public int Facing { get; private set; } = 1;
    public Vector2 Velocity => _rb != null ? _rb.linearVelocity : Vector2.zero;

    private int _coyoteCounter;
    private int _bufferCounter;
    private int _jumpHoldCounter;
    private bool _jumpingAscending;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _probe = GetComponent<GroundProbe>();
        _input = GetComponent<PlayerInputBinding>();
        _carry = GetComponent<PlayerCarry>();

        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void FixedUpdate()
    {
        _probe.Sample();

        // Coyote: decremented after we've had a chance to read the "just left ground" state.
        bool groundedNow = _probe.IsGrounded;
        if (groundedNow)
        {
            _coyoteCounter = coyoteTicks;
        }

        // Jump buffer: set on press in Update path — check and decrement here.
        if (_input.JumpPressedThisFrame || _input.SpinJumpPressedThisFrame)
        {
            _bufferCounter = bufferTicks;
        }

        bool wantSpin = _input.SpinJumpPressedThisFrame || (IsSpinJumping && _bufferCounter > 0);
        bool canStartJump = (_coyoteCounter > 0 || groundedNow) && _bufferCounter > 0;
        bool startJumpThisTick = false;

        if (canStartJump)
        {
            float launchVel = wantSpin ? spinJumpVelocity : jumpVelocity;
            var v = _rb.linearVelocity;
            v.y = launchVel;
            _rb.linearVelocity = v;
            _jumpingAscending = true;
            _jumpHoldCounter = jumpHoldMaxTicks;
            IsSpinJumping = wantSpin;
            _coyoteCounter = 0;
            _bufferCounter = 0;
            startJumpThisTick = true;
        }

        // Horizontal integration.
        IntegrateHorizontal(groundedNow);

        // Vertical integration — skip the gravity pass on the frame we just launched,
        // otherwise the upward velocity loses a tick of height to gravity before the
        // jump-hold reduction kicks in.
        if (!startJumpThisTick)
        {
            IntegrateVertical();
        }

        // Ceiling cancel — zero upward velocity on head contact.
        if (_probe.CeilingContact && _rb.linearVelocity.y > 0f)
        {
            var v = _rb.linearVelocity;
            v.y = 0f;
            _rb.linearVelocity = v;
            _jumpingAscending = false;
            _jumpHoldCounter = 0;
        }

        // Land check — reset spin state, clear ascending flag.
        if (groundedNow && _rb.linearVelocity.y <= 0f)
        {
            IsSpinJumping = false;
            _jumpingAscending = false;
            _jumpHoldCounter = 0;
        }

        // Decrement counters.
        if (!groundedNow && _coyoteCounter > 0) _coyoteCounter--;
        if (_bufferCounter > 0) _bufferCounter--;

        IsCrouching = _input.Crouch && groundedNow;
        UpdateFacing();
    }

    private void IntegrateHorizontal(bool grounded)
    {
        float moveX = _input.MoveX;
        bool running = _input.ActionHeld;
        float maxSpeed = running ? runMaxSpeed : walkMaxSpeed;

        // Downhill speed boost: on a slope and moving downhill.
        if (grounded && _probe.GroundAngleDegrees > 1f)
        {
            bool downhill = Mathf.Sign(moveX) == -Mathf.Sign(_probe.GroundNormal.x)
                             && Mathf.Abs(moveX) > 0.1f;
            if (downhill) maxSpeed *= downhillSpeedBoost;
        }

        Vector2 vel = _rb.linearVelocity;
        float targetVX = moveX * maxSpeed;
        float accel = grounded ? groundAccel : airAccel;

        bool inputActive = Mathf.Abs(moveX) > 0.1f;
        bool inputOpposesVelocity = inputActive && Mathf.Sign(moveX) != Mathf.Sign(vel.x) && Mathf.Abs(vel.x) > 0.05f;

        IsSkidding = grounded && inputOpposesVelocity && Mathf.Abs(vel.x) >= skidSpeedThreshold;

        float effectiveAccel = IsSkidding ? skidDecel : accel;

        if (!inputActive && grounded)
        {
            // Friction — bleed velocity toward zero.
            float sign = Mathf.Sign(vel.x);
            float mag = Mathf.Max(0f, Mathf.Abs(vel.x) - groundFriction * Time.fixedDeltaTime);
            vel.x = mag * sign;
        }
        else
        {
            vel.x = Mathf.MoveTowards(vel.x, targetVX, effectiveAccel * Time.fixedDeltaTime);
        }

        _rb.linearVelocity = vel;
    }

    private void IntegrateVertical()
    {
        Vector2 vel = _rb.linearVelocity;

        // Variable jump: reduced gravity while jump button held and we're still ascending.
        bool holdActive = _jumpingAscending
                          && _jumpHoldCounter > 0
                          && vel.y > 0f
                          && (_input.JumpHeld || _input.SpinJumpHeld);

        float g = holdActive ? jumpHoldGravity : gravity;
        vel.y -= g * Time.fixedDeltaTime;

        if (holdActive) _jumpHoldCounter--;
        if (vel.y <= 0f) _jumpingAscending = false;

        // Released jump early → drop the hold window.
        if (_input.JumpReleasedThisFrame)
        {
            _jumpHoldCounter = 0;
            _jumpingAscending = false;
        }

        vel.y = Mathf.Max(vel.y, -maxFallSpeed);
        _rb.linearVelocity = vel;
    }

    private void UpdateFacing()
    {
        float vx = _rb.linearVelocity.x;
        if (Mathf.Abs(vx) > 0.5f) Facing = vx > 0f ? 1 : -1;
        else if (Mathf.Abs(_input.MoveX) > 0.1f) Facing = _input.MoveX > 0f ? 1 : -1;
    }

    // Test hook — place player at a known position without a full scene setup.
    public void Teleport(Vector2 position)
    {
        transform.position = position;
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
        _coyoteCounter = 0;
        _bufferCounter = 0;
        _jumpHoldCounter = 0;
        _jumpingAscending = false;
        IsSpinJumping = false;
    }
}
