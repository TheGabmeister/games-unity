using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset _inputActions;

    private InputActionMap _playerMap;

    public InputAction Point { get; private set; }
    public InputAction Select { get; private set; }
    public InputAction Command { get; private set; }
    public InputAction CameraPan { get; private set; }
    public InputAction ModifierCtrl { get; private set; }
    public InputAction ModifierAlt { get; private set; }
    public InputAction Stop { get; private set; }
    public InputAction Guard { get; private set; }
    public InputAction Scatter { get; private set; }
    public InputAction ModifierAttackMove { get; private set; }
    public InputAction SelectAll { get; private set; }
    public InputAction[] Groups { get; private set; } = new InputAction[9];

    public static InputManager Instance { get; private set; }

    public Vector2 MousePosition => Point.ReadValue<Vector2>();
    public bool IsCtrlHeld => ModifierCtrl.IsPressed();
    public bool IsAltHeld => ModifierAlt.IsPressed();
    public bool IsAttackMoveHeld => ModifierAttackMove.IsPressed();

    void Awake()
    {
        Instance = this;
        _playerMap = _inputActions.FindActionMap("Player");

        Point = _playerMap.FindAction("Point");
        Select = _playerMap.FindAction("Select");
        Command = _playerMap.FindAction("Command");
        CameraPan = _playerMap.FindAction("CameraPan");
        ModifierCtrl = _playerMap.FindAction("ModifierCtrl");
        ModifierAlt = _playerMap.FindAction("ModifierAlt");
        Stop = _playerMap.FindAction("Stop");
        Guard = _playerMap.FindAction("Guard");
        Scatter = _playerMap.FindAction("Scatter");
        ModifierAttackMove = _playerMap.FindAction("ModifierAttackMove");
        SelectAll = _playerMap.FindAction("SelectAll");

        for (int i = 0; i < 9; i++)
            Groups[i] = _playerMap.FindAction($"Group{i + 1}");
    }

    void OnEnable() => _playerMap?.Enable();
    void OnDisable() => _playerMap?.Disable();
}
