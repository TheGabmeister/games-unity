using UnityEngine;

public class InputManager : MonoBehaviour
{
    private InputSystem_Actions _input;
    private bool _playerInputEnabled = true;

    public InputSystem_Actions Actions => _input;
    public bool PlayerInputEnabled => _playerInputEnabled;

    private void Awake()
    {
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
