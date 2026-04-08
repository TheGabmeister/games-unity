using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] InputActionAsset inputActions;

    InputActionMap gameplayMap;
    InputActionMap uiMap;

    // Gameplay-map actions
    InputAction gameplayMove;
    InputAction gameplayConfirm;
    InputAction gameplayCancel;
    InputAction gameplayMenu;
    InputAction gameplayRun;

    // UI-map actions
    InputAction uiNavigate;
    InputAction uiSubmit;
    InputAction uiCancel;

    // Debug actions (always from gameplay map)
    public InputAction DebugOverlayAction { get; private set; }
    public InputAction DebugConsoleAction { get; private set; }

    // Tracks which mode we're in (gameplay map's .enabled is unreliable
    // because individually re-enabling debug actions marks the map as enabled)
    bool uiMode;

    // Run and Menu are gameplay-only
    public InputAction RunAction => gameplayRun;
    public InputAction MenuAction => gameplayMenu;

    // These resolve to whichever map is currently active
    public InputAction MoveAction => uiMode ? uiNavigate : gameplayMove;
    public InputAction ConfirmAction => uiMode ? uiSubmit : gameplayConfirm;
    public InputAction CancelAction => uiMode ? uiCancel : gameplayCancel;

    void Awake()
    {
        if (inputActions == null) return;

        gameplayMap = inputActions.FindActionMap("Gameplay");
        uiMap = inputActions.FindActionMap("UI");

        // Gameplay actions
        gameplayMove = gameplayMap?.FindAction("Move");
        gameplayConfirm = gameplayMap?.FindAction("Confirm");
        gameplayCancel = gameplayMap?.FindAction("Cancel");
        gameplayMenu = gameplayMap?.FindAction("Menu");
        gameplayRun = gameplayMap?.FindAction("Run");

        // UI actions
        uiNavigate = uiMap?.FindAction("Navigate");
        uiSubmit = uiMap?.FindAction("Submit");
        uiCancel = uiMap?.FindAction("Cancel");

        // Debug actions (always available)
        DebugOverlayAction = gameplayMap?.FindAction("DebugOverlay");
        DebugConsoleAction = gameplayMap?.FindAction("DebugConsole");
    }

    public void EnableGameplay()
    {
        uiMode = false;
        uiMap?.Disable();
        gameplayMap?.Enable();
    }

    public void EnableUI()
    {
        uiMode = true;
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
