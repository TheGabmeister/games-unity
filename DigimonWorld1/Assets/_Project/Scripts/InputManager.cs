using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    private InputSystem_Actions _input;
    private bool _playerInputEnabled = true;

    public InputSystem_Actions Actions => _input;
    public bool PlayerInputEnabled => _playerInputEnabled;

    protected override void Awake()
    {
        base.Awake();
        _input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _input.Player.Enable();
    }

    private void OnDisable()
    {
        _input.Player.Disable();
    }

    public void SetPlayerInputEnabled(bool enabled)
    {
        _playerInputEnabled = enabled;
    }
}
