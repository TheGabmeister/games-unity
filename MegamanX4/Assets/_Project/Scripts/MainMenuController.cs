using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class MainMenuController : MonoBehaviour
{
    PlayerInput _playerInput;
    InputAction _submitAction;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _submitAction = _playerInput.actions["Submit"];
    }

    void OnEnable()
    {
        _submitAction.started += OnSubmit;
    }

    void OnDisable()
    {
        _submitAction.started -= OnSubmit;
    }

    void OnSubmit(InputAction.CallbackContext ctx)
    {
        GoToLevelSelect();
    }

    public void GoToLevelSelect()
    {
        if (Services.TryGet<GameStateController>(out var gameState))
            gameState.SetState(GameState.LevelSelect);
    }
}
