using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] InputActionAsset inputActions;

    InputActionMap gameplayMap;
    InputActionMap uiMap;

    public InputAction MoveAction { get; private set; }
    public InputAction ConfirmAction { get; private set; }
    public InputAction CancelAction { get; private set; }
    public InputAction MenuAction { get; private set; }
    public InputAction RunAction { get; private set; }
    public InputAction DebugOverlayAction { get; private set; }
    public InputAction DebugConsoleAction { get; private set; }

    void Awake()
    {
        if (inputActions == null) return;

        gameplayMap = inputActions.FindActionMap("Gameplay");
        uiMap = inputActions.FindActionMap("UI");

        // Gameplay actions
        MoveAction = gameplayMap?.FindAction("Move");
        ConfirmAction = gameplayMap?.FindAction("Confirm");
        CancelAction = gameplayMap?.FindAction("Cancel");
        MenuAction = gameplayMap?.FindAction("Menu");
        RunAction = gameplayMap?.FindAction("Run");

        // Debug actions (always available, from Gameplay map)
        DebugOverlayAction = gameplayMap?.FindAction("DebugOverlay");
        DebugConsoleAction = gameplayMap?.FindAction("DebugConsole");
    }

    public void EnableGameplay()
    {
        uiMap?.Disable();
        gameplayMap?.Enable();
    }

    public void EnableUI()
    {
        gameplayMap?.Disable();
        uiMap?.Enable();
        // Keep debug actions active
        DebugOverlayAction?.Enable();
        DebugConsoleAction?.Enable();
    }

    public void DisableAll()
    {
        gameplayMap?.Disable();
        uiMap?.Disable();
    }

    void OnEnable() => inputActions?.Enable();
    void OnDisable() => inputActions?.Disable();
}
