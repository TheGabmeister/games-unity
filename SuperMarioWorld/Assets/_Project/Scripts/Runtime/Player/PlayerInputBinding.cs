using UnityEngine;
using UnityEngine.InputSystem;

// Translates the New Input System's PlayerInput callbacks into plain intent fields
// the controller can read each FixedUpdate. Tests bypass this layer and write the
// fields directly via DebugOverride — the component tolerates a missing PlayerInput
// so tests can build a stripped player without the Input System dependency.
//
// Crouch is derived, not bound — Move.y < -0.5 per SPEC §4.1.
public sealed class PlayerInputBinding : MonoBehaviour
{
    private PlayerInput _input;
    private InputAction _move;
    private InputAction _jump;
    private InputAction _spinJump;
    private InputAction _action;
    private InputAction _pause;

    public Vector2 Move { get; private set; }
    public float MoveX => Move.x;
    public bool Crouch => Move.y < -0.5f;

    public bool JumpHeld { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }
    public bool JumpReleasedThisFrame { get; private set; }

    public bool SpinJumpHeld { get; private set; }
    public bool SpinJumpPressedThisFrame { get; private set; }

    public bool ActionHeld { get; private set; }
    public bool ActionPressedThisFrame { get; private set; }
    public bool ActionReleasedThisFrame { get; private set; }

    public bool PausePressedThisFrame { get; private set; }

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
        if (_input == null || _input.actions == null) return;
        var asset = _input.actions;
        _move = asset.FindAction("Player/Move", throwIfNotFound: false);
        _jump = asset.FindAction("Player/Jump", throwIfNotFound: false);
        _spinJump = asset.FindAction("Player/SpinJump", throwIfNotFound: false);
        _action = asset.FindAction("Player/Action", throwIfNotFound: false);
        _pause = asset.FindAction("Player/Pause", throwIfNotFound: false);
    }

    private void Update()
    {
        // No-op when driven by DebugOverride (tests build a stripped player with
        // no PlayerInput; real gameplay always has one).
        if (_input == null) return;

        if (_move != null) Move = _move.ReadValue<Vector2>();

        JumpPressedThisFrame = _jump != null && _jump.WasPressedThisFrame();
        JumpReleasedThisFrame = _jump != null && _jump.WasReleasedThisFrame();
        JumpHeld = _jump != null && _jump.IsPressed();

        SpinJumpPressedThisFrame = _spinJump != null && _spinJump.WasPressedThisFrame();
        SpinJumpHeld = _spinJump != null && _spinJump.IsPressed();

        ActionPressedThisFrame = _action != null && _action.WasPressedThisFrame();
        ActionReleasedThisFrame = _action != null && _action.WasReleasedThisFrame();
        ActionHeld = _action != null && _action.IsPressed();

        PausePressedThisFrame = _pause != null && _pause.WasPressedThisFrame();
    }

    // Test / scripted-input hook: let tests inject synthetic state without routing
    // through the Input System. Writes bypass the Input-System read in Update().
    public void DebugOverride(
        Vector2 move,
        bool jumpHeld, bool jumpPressed, bool jumpReleased,
        bool spinJumpHeld, bool spinJumpPressed,
        bool actionHeld, bool actionPressed, bool actionReleased)
    {
        Move = move;
        JumpHeld = jumpHeld;
        JumpPressedThisFrame = jumpPressed;
        JumpReleasedThisFrame = jumpReleased;
        SpinJumpHeld = spinJumpHeld;
        SpinJumpPressedThisFrame = spinJumpPressed;
        ActionHeld = actionHeld;
        ActionPressedThisFrame = actionPressed;
        ActionReleasedThisFrame = actionReleased;
    }
}
