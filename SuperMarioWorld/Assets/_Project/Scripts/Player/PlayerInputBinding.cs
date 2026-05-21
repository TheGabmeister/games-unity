using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInputBinding : MonoBehaviour
{
    [SerializeField] private InputActionAsset actions;

    private InputAction _move;
    private InputAction _jump;
    private InputAction _spinJump;
    private InputAction _action;
    private InputAction _pause;
    private InputAction _cameraNudgeLeft;
    private InputAction _cameraNudgeRight;

    private bool _debugMode;

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

    public bool CameraNudgeLeftHeld { get; private set; }
    public bool CameraNudgeRightHeld { get; private set; }

    public static PlayerInputBinding Instance { get; private set; }

    public void SwitchActionMap(string mapName)
    {
        if (actions == null) return;
        foreach (var map in actions.actionMaps)
            map.Disable();
        var target = actions.FindActionMap(mapName);
        target?.Enable();
    }

    private void Awake()
    {
        Instance = this;

        if (actions == null) return;
        _move = actions.FindAction("Player/Move");
        _jump = actions.FindAction("Player/Jump");
        _spinJump = actions.FindAction("Player/SpinJump");
        _action = actions.FindAction("Player/Action");
        _pause = actions.FindAction("Player/Pause");
        _cameraNudgeLeft = actions.FindAction("Player/CameraNudgeLeft");
        _cameraNudgeRight = actions.FindAction("Player/CameraNudgeRight");
    }

    private void OnEnable()
    {
        if (actions != null) actions.Enable();
    }

    private void OnDisable()
    {
        if (actions != null) actions.Disable();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (_debugMode || actions == null) return;

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

        CameraNudgeLeftHeld = _cameraNudgeLeft != null && _cameraNudgeLeft.IsPressed();
        CameraNudgeRightHeld = _cameraNudgeRight != null && _cameraNudgeRight.IsPressed();
    }
}
