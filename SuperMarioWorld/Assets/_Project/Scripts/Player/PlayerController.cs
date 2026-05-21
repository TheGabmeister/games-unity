using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(GroundProbe))]
public sealed class PlayerController : MonoBehaviour
{
    // Unit conversion: 1 SNES subpx/frame ≈ 6/21 Unity velocity (units/sec).
    // Walk 21 subpx/f → 6, Run 37 → 10.57, Sprint 49 → 14.
    private const float SubpxToVel = 6f / 21f;

    [Header("Horizontal")]
    [SerializeField] private float walkMaxSpeed = 6f;
    [SerializeField] private float runMaxSpeed = 10.57f;
    [SerializeField] private float sprintMaxSpeed = 14f;
    [SerializeField] private float groundAccel = 26f;
    [SerializeField] private float airAccel = 13f;
    [SerializeField] private float groundFriction = 18f;
    [SerializeField] private float skidDecel = 40f;
    [SerializeField] private float skidSpeedThreshold = 4f;

    [Header("P-Meter")]
    [SerializeField] private int pMeterMax = 112;
    [SerializeField] private int pMeterFillRate = 2;
    [SerializeField] private int pMeterDrainRate = 1;
    [SerializeField] private float pMeterMinSpeed = 8f;

    [Header("Vertical")]
    [SerializeField] private float gravity = 60f;
    [SerializeField] private float jumpHoldGravity = 20f;
    [SerializeField] private float jumpVelocityMin = 15f;
    [SerializeField] private float jumpVelocityMax = 18f;
    [SerializeField] private float spinJumpVelocityMin = 12f;
    [SerializeField] private float spinJumpVelocityMax = 14.4f;
    [SerializeField] private float maxFallSpeed = 20f;

    [Header("Timings (ticks @ 60Hz)")]
    [SerializeField] private int jumpHoldMaxTicks = 18;
    [SerializeField] private int bufferTicks = 7;

    [Header("Slope")]
    [SerializeField] private float downhillSpeedBoost = 1.3f;
    [SerializeField] private float slideAccel = 20f;
    [SerializeField] private float slideMaxSpeed = 12f;
    [SerializeField] private float slideMinAngle = 5f;

    private Rigidbody2D _rb;
    private GroundProbe _probe;
    private PlayerInputBinding _input;
    private PlayerCarry _carry;

    // P-Meter state.
    private int _pMeter;
    private int _oscillationIndex;

    // Run/sprint speed oscillation patterns (subpx/f converted to velocity).
    private static readonly float[] RunOscillation =
    {
        36f * SubpxToVel, 35f * SubpxToVel, 36f * SubpxToVel,
        35f * SubpxToVel, 37f * SubpxToVel
    };
    private static readonly float[] SprintOscillation =
    {
        48f * SubpxToVel, 47f * SubpxToVel, 48f * SubpxToVel,
        47f * SubpxToVel, 49f * SubpxToVel
    };

    // Runtime state — exposed as read-only properties for tests + HUD.
    public bool IsGrounded => _probe != null && _probe.IsGrounded;
    public bool IsSpinJumping { get; private set; }
    public bool IsSkidding { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsSprinting => _pMeter >= pMeterMax;
    public int PMeterValue => _pMeter;
    public int Facing { get; private set; } = 1;
    public Vector2 Velocity => _rb != null ? _rb.linearVelocity : Vector2.zero;

    private int _bufferCounter;
    private int _jumpHoldCounter;
    private bool _jumpingAscending;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _probe = GetComponent<GroundProbe>();
        _input = PlayerInputBinding.Instance;
        _carry = GetComponent<PlayerCarry>();

        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void FixedUpdate()
    {
        _probe.Sample();

        bool groundedNow = _probe.IsGrounded;

        // Jump buffer.
        if (_input.JumpPressedThisFrame || _input.SpinJumpPressedThisFrame)
            _bufferCounter = bufferTicks;

        // No coyote time per SNES original — must be grounded this tick to jump.
        bool wantSpin = _input.SpinJumpPressedThisFrame || (IsSpinJumping && _bufferCounter > 0);
        bool canStartJump = groundedNow && _bufferCounter > 0 && !IsSliding;
        bool startJumpThisTick = false;

        if (canStartJump)
        {
            float xSpeed01 = Mathf.InverseLerp(0f, sprintMaxSpeed, Mathf.Abs(_rb.linearVelocity.x));
            float launchVel;
            if (wantSpin)
                launchVel = Mathf.Lerp(spinJumpVelocityMin, spinJumpVelocityMax, xSpeed01);
            else
                launchVel = Mathf.Lerp(jumpVelocityMin, jumpVelocityMax, xSpeed01);

            var v = _rb.linearVelocity;
            v.y = launchVel;
            _rb.linearVelocity = v;
            _jumpingAscending = true;
            _jumpHoldCounter = jumpHoldMaxTicks;
            IsSpinJumping = wantSpin;
            _bufferCounter = 0;
            startJumpThisTick = true;
        }

        // Slope sliding.
        UpdateSliding(groundedNow);

        if (IsSliding)
            IntegrateSlide();
        else
            IntegrateHorizontal(groundedNow);

        // Skip gravity on launch frame to preserve full first-tick height.
        if (!startJumpThisTick)
            IntegrateVertical();

        // Ceiling cancel.
        if (_probe.CeilingContact && _rb.linearVelocity.y > 0f)
        {
            var v = _rb.linearVelocity;
            v.y = 0f;
            _rb.linearVelocity = v;
            _jumpingAscending = false;
            _jumpHoldCounter = 0;
        }

        // Land check.
        if (groundedNow && _rb.linearVelocity.y <= 0f)
        {
            IsSpinJumping = false;
            _jumpingAscending = false;
            _jumpHoldCounter = 0;
        }

        // P-Meter.
        UpdatePMeter(groundedNow);

        if (_bufferCounter > 0) _bufferCounter--;

        IsCrouching = _input.Crouch && groundedNow && !IsSliding;
        UpdateFacing();
    }

    private void UpdatePMeter(bool grounded)
    {
        bool filling = grounded
                       && _input.ActionHeld
                       && Mathf.Abs(_input.MoveX) > 0.1f
                       && Mathf.Abs(_rb.linearVelocity.x) >= pMeterMinSpeed;

        if (filling)
            _pMeter = Mathf.Min(_pMeter + pMeterFillRate, pMeterMax);
        else
            _pMeter = Mathf.Max(_pMeter - pMeterDrainRate, 0);
    }

    private void UpdateSliding(bool grounded)
    {
        if (!IsSliding)
        {
            bool onSlope = grounded && _probe.GroundAngleDegrees >= slideMinAngle;
            if (onSlope && _input.Crouch)
                IsSliding = true;
        }
        else
        {
            bool flat = !grounded || _probe.GroundAngleDegrees < slideMinAngle;
            bool hitWall = (Facing > 0 && _probe.WallRight) || (Facing < 0 && _probe.WallLeft);
            if (flat || hitWall || _input.JumpPressedThisFrame || _input.SpinJumpPressedThisFrame)
                IsSliding = false;
        }
    }

    private void IntegrateSlide()
    {
        Vector2 vel = _rb.linearVelocity;
        float slopeDir = -Mathf.Sign(_probe.GroundNormal.x);
        float target = slopeDir * slideMaxSpeed;
        vel.x = Mathf.MoveTowards(vel.x, target, slideAccel * Time.fixedDeltaTime);
        _rb.linearVelocity = vel;
    }

    private void IntegrateHorizontal(bool grounded)
    {
        float moveX = _input.MoveX;
        bool running = _input.ActionHeld;

        float maxSpeed;
        if (IsSprinting && running)
        {
            maxSpeed = SprintOscillation[_oscillationIndex % SprintOscillation.Length];
            _oscillationIndex++;
        }
        else if (running)
        {
            float absVel = Mathf.Abs(_rb.linearVelocity.x);
            if (absVel >= runMaxSpeed - 0.5f)
            {
                maxSpeed = RunOscillation[_oscillationIndex % RunOscillation.Length];
                _oscillationIndex++;
            }
            else
            {
                maxSpeed = runMaxSpeed;
                _oscillationIndex = 0;
            }
        }
        else
        {
            maxSpeed = walkMaxSpeed;
            _oscillationIndex = 0;
        }

        // Downhill speed boost on slopes.
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
        bool inputOpposesVelocity = inputActive && Mathf.Sign(moveX) != Mathf.Sign(vel.x)
                                    && Mathf.Abs(vel.x) > 0.05f;

        IsSkidding = grounded && inputOpposesVelocity && Mathf.Abs(vel.x) >= skidSpeedThreshold;

        float effectiveAccel = IsSkidding ? skidDecel : accel;

        if (!inputActive && grounded)
        {
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

        bool holdActive = _jumpingAscending
                          && _jumpHoldCounter > 0
                          && vel.y > 0f
                          && (_input.JumpHeld || _input.SpinJumpHeld);

        float g = holdActive ? jumpHoldGravity : gravity;
        vel.y -= g * Time.fixedDeltaTime;

        if (holdActive) _jumpHoldCounter--;
        if (vel.y <= 0f) _jumpingAscending = false;

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
        if (IsSliding) return;
        float vx = _rb.linearVelocity.x;
        if (Mathf.Abs(vx) > 0.5f) Facing = vx > 0f ? 1 : -1;
        else if (Mathf.Abs(_input.MoveX) > 0.1f) Facing = _input.MoveX > 0f ? 1 : -1;
    }

    public void Teleport(Vector2 position)
    {
        transform.position = position;
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
        _bufferCounter = 0;
        _jumpHoldCounter = 0;
        _jumpingAscending = false;
        IsSpinJumping = false;
        IsSliding = false;
        _pMeter = 0;
        _oscillationIndex = 0;
    }
}
