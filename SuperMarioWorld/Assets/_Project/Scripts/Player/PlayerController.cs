using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(GroundProbe))]
public class PlayerController : MonoBehaviour
{
    const int SubpixelsPerUnit = 1024; // 64 pixels/unit * 16 subpixels/pixel
    const float InvSubpixels = 1f / SubpixelsPerUnit;
    const int MaxWalkSpeed = 21;
    const int MaxRunSpeed = 37;
    const int MaxSprintSpeed = 49;
    const int TerminalVelocity = 64;
    const int MaxSlopeAngle = 45;
    const int PMeterMax = 112;
    const int GravityHeld = 3;
    const int GravityReleased = 6;
    const float GroundSnapDistance = 0.15f;

    static readonly int[] SprintOscillation = { 48, 47, 48, 47, 49 };
    static readonly int[] RunOscillation = { 36, 35, 36, 35, 37 };

    // Normal jump: indexed by abs(xSpeed) / 8
    static readonly int[] JumpTableNormal = { -80, -82, -85, -87, -90, -92, -95 };
    // Spin jump: indexed by abs(xSpeed) / 8
    static readonly int[] JumpTableSpin = { -74, -76, -78, -80, -82, -85, -87 };

    enum Mode { Ground, Air, Crouch, Slide }

    Rigidbody2D _rb;
    GroundProbe _probe;

    Mode _mode = Mode.Air;
    int _facingDir = 1; // 1 = right, -1 = left
    bool _isSpinning;

    // Subpixel position accumulator
    long _subX;
    long _subY;

    // Velocity in subpixels/frame
    int _velX;
    int _velY;

    // P-Meter
    int _pMeter;
    int _oscillationIndex;

    // Jump state
    bool _jumpHeld;

    public bool IsSprinting => _pMeter >= PMeterMax;
    public int FacingDir => _facingDir;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _probe = GetComponent<GroundProbe>();
    }

    void Start()
    {
        var pos = transform.position;
        _subX = (long)(pos.x * SubpixelsPerUnit);
        _subY = (long)(pos.y * SubpixelsPerUnit);
    }

    void FixedUpdate()
    {
        var input = PlayerInputBinding.Instance;
        if (input == null) return;

        _probe.Sample();

        switch (_mode)
        {
            case Mode.Ground: TickGround(input); break;
            case Mode.Air:    TickAir(input);    break;
            case Mode.Crouch: TickCrouch(input); break;
            case Mode.Slide:  TickSlide(input);  break;
        }

        ApplyVelocity();
    }

    // --- Ground ---

    void TickGround(PlayerInputBinding input)
    {
        if (!_probe.IsGrounded)
        {
            EnterAir(false);
            TickAir(input);
            return;
        }

        _isSpinning = false;
        float moveX = input.MoveX;
        bool runHeld = input.ActionHeld;

        if (input.Crouch && Mathf.Abs(_velX) < 2)
        {
            _mode = Mode.Crouch;
            _velX = 0;
            TickCrouch(input);
            return;
        }

        if (input.Crouch && _probe.GroundAngle > 5f)
        {
            _mode = Mode.Slide;
            TickSlide(input);
            return;
        }

        UpdateFacing(moveX);
        ApplyGroundAcceleration(moveX, runHeld);
        UpdatePMeter(moveX, runHeld);
        ClampGroundSpeed(runHeld);

        if (input.JumpPressedThisFrame)
        {
            StartJump(false);
            return;
        }

        if (input.SpinJumpPressedThisFrame)
        {
            StartJump(true);
            return;
        }

        _velY = 0;
        SnapToGround();
    }

    void ApplyGroundAcceleration(float moveX, bool runHeld)
    {
        int inputDir = moveX > 0.1f ? 1 : (moveX < -0.1f ? -1 : 0);

        if (inputDir == 0)
        {
            ApplyFriction();
            return;
        }

        bool skidding = (_velX > 0 && inputDir < 0) || (_velX < 0 && inputDir > 0);
        if (skidding)
        {
            ApplySkidDecel(inputDir);
            return;
        }

        int accel = runHeld ? 2 : 1;
        _velX += inputDir * accel;
    }

    void ApplyFriction()
    {
        if (_velX > 0) _velX = Mathf.Max(0, _velX - 1);
        else if (_velX < 0) _velX = Mathf.Min(0, _velX + 1);
    }

    void ApplySkidDecel(int inputDir)
    {
        int decel = 2;
        if (_velX > 0) _velX = Mathf.Max(0, _velX - decel);
        else if (_velX < 0) _velX = Mathf.Min(0, _velX + decel);

        if (_velX == 0)
            _velX += inputDir;
    }

    void ClampGroundSpeed(bool runHeld)
    {
        int maxSpeed;
        if (IsSprinting)
            maxSpeed = GetOscillatedSpeed(SprintOscillation);
        else if (runHeld)
            maxSpeed = GetOscillatedSpeed(RunOscillation);
        else
            maxSpeed = MaxWalkSpeed;

        _velX = Mathf.Clamp(_velX, -maxSpeed, maxSpeed);
    }

    int GetOscillatedSpeed(int[] pattern)
    {
        int speed = pattern[_oscillationIndex % pattern.Length];
        _oscillationIndex++;
        return speed;
    }

    void UpdatePMeter(float moveX, bool runHeld)
    {
        int inputDir = moveX > 0.1f ? 1 : (moveX < -0.1f ? -1 : 0);
        bool filling = runHeld && inputDir != 0 && Mathf.Abs(_velX) >= MaxRunSpeed - 4;

        if (filling)
            _pMeter = Mathf.Min(_pMeter + 2, PMeterMax);
        else
            _pMeter = Mathf.Max(_pMeter - 1, 0);
    }

    void SnapToGround()
    {
        var hit = Physics2D.Raycast(transform.position, Vector2.down, GroundSnapDistance + 0.5f,
            _probe.GetInstanceID() != 0 ? Physics2D.AllLayers : 0);

        if (_probe.IsGrounded)
        {
            var origin = (Vector2)transform.position;
            var bounds = GetComponent<BoxCollider2D>().bounds;
            var rayStart = new Vector2(origin.x, bounds.min.y);
            var rayHit = Physics2D.Raycast(rayStart, Vector2.down, GroundSnapDistance, Physics2D.AllLayers);
            if (rayHit.collider != null)
            {
                float targetY = rayHit.point.y + bounds.extents.y;
                _subY = (long)(targetY * SubpixelsPerUnit);
            }
        }
    }

    // --- Air ---

    void TickAir(PlayerInputBinding input)
    {
        if (_probe.IsGrounded && _velY >= 0)
        {
            Land();
            return;
        }

        float moveX = input.MoveX;
        UpdateFacing(moveX);
        ApplyAirDrift(moveX, input.ActionHeld);

        _jumpHeld = _isSpinning ? input.SpinJumpHeld : input.JumpHeld;

        if (_probe.HitCeiling && _velY < 0)
            _velY = 0;

        int gravity = _jumpHeld && _velY < 0 ? GravityHeld : GravityReleased;
        _velY += gravity;

        if (_velY > TerminalVelocity)
            _velY = TerminalVelocity;

        if (_probe.HitWallLeft && _velX < 0) _velX = 0;
        if (_probe.HitWallRight && _velX > 0) _velX = 0;
    }

    void ApplyAirDrift(float moveX, bool runHeld)
    {
        int inputDir = moveX > 0.1f ? 1 : (moveX < -0.1f ? -1 : 0);
        if (inputDir == 0) return;

        int maxSpeed = runHeld ? MaxRunSpeed : MaxWalkSpeed;
        int newVelX = _velX + inputDir;
        if (Mathf.Abs(newVelX) <= maxSpeed || Mathf.Abs(newVelX) < Mathf.Abs(_velX))
            _velX = newVelX;
    }

    void StartJump(bool spin)
    {
        _isSpinning = spin;
        int absX = Mathf.Abs(_velX);
        int index = Mathf.Min(absX / 8, 6);
        int[] table = spin ? JumpTableSpin : JumpTableNormal;
        _velY = table[index];
        _jumpHeld = true;
        EnterAir(true);
    }

    void EnterAir(bool fromJump)
    {
        _mode = Mode.Air;
        if (!fromJump)
            _jumpHeld = false;
    }

    void Land()
    {
        _mode = Mode.Ground;
        _velY = 0;
        _isSpinning = false;
        _oscillationIndex = 0;
    }

    // --- Crouch ---

    void TickCrouch(PlayerInputBinding input)
    {
        if (!_probe.IsGrounded)
        {
            EnterAir(false);
            TickAir(input);
            return;
        }

        ApplyFriction();

        if (!input.Crouch)
        {
            _mode = Mode.Ground;
            return;
        }

        if (input.JumpPressedThisFrame)
        {
            StartJump(false);
            return;
        }

        if (input.SpinJumpPressedThisFrame)
        {
            StartJump(true);
            return;
        }

        _velY = 0;
    }

    // --- Slide ---

    void TickSlide(PlayerInputBinding input)
    {
        if (!_probe.IsGrounded)
        {
            EnterAir(false);
            TickAir(input);
            return;
        }

        if (_probe.GroundAngle < 5f)
        {
            _mode = Mode.Ground;
            return;
        }

        if (!input.Crouch)
        {
            _mode = Mode.Ground;
            return;
        }

        int slopeDir = _probe.GroundNormal.x > 0f ? -1 : 1;
        _velX += slopeDir * 1;
        _facingDir = slopeDir;

        int slideMax = MaxRunSpeed;
        _velX = Mathf.Clamp(_velX, -slideMax, slideMax);

        _velY = 0;
        SnapToGround();
    }

    // --- Shared ---

    void UpdateFacing(float moveX)
    {
        if (moveX > 0.1f) _facingDir = 1;
        else if (moveX < -0.1f) _facingDir = -1;
    }

    void ApplyVelocity()
    {
        if (_probe.HitWallLeft && _velX < 0) _velX = 0;
        if (_probe.HitWallRight && _velX > 0) _velX = 0;

        _subX += _velX;
        _subY += _velY;

        float newX = _subX * InvSubpixels;
        float newY = _subY * InvSubpixels;

        _rb.MovePosition(new Vector2(newX, newY));
    }

    public void ApplyBounce(int ySpeedSubpixels)
    {
        _velY = ySpeedSubpixels;
        _mode = Mode.Air;
    }
}
