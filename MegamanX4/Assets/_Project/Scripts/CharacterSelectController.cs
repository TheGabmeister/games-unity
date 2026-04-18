using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class CharacterSelectController : MonoBehaviour
{
    [SerializeField] MenuNavigator _menu;

    PlayerInput _playerInput;
    InputAction _submitAction;
    InputAction _navigateAction;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _submitAction = _playerInput.actions["Submit"];
        _navigateAction = _playerInput.actions["Navigate"];
    }

    void OnEnable()
    {
        _submitAction.started += OnSubmit;
        _navigateAction.performed += OnNavigate;
        if (_menu)
            _menu.Confirmed += OnConfirm;
    }

    void OnDisable()
    {
        _submitAction.started -= OnSubmit;
        _navigateAction.performed -= OnNavigate;
        if (_menu)
            _menu.Confirmed -= OnConfirm;
    }

    void OnSubmit(InputAction.CallbackContext ctx)
    {
        _menu.Submit();
    }

    void OnNavigate(InputAction.CallbackContext ctx)
    {
        _menu.Navigate(ctx.ReadValue<Vector2>());
    }

    void OnConfirm(int index)
    {
        if (index == 0)
            SelectX();
        else if (index == 1)
            SelectZero();
    }

    void SelectX()
    {
        GameStateEvents.SetState.Raise(GameState.Gameplay);
    }

    void SelectZero()
    {
    }
}
