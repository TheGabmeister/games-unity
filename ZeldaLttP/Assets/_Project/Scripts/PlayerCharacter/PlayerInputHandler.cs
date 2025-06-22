using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    InputAction _moveAction;
    InputAction _jumpAction;
    InputAction _lookAction;
    InputAction _isSprintPressed;
    InputAction _attackAction;
    InputAction _menuAction;
    InputAction _interactAction;
    InputAction _inventoryAction;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool IsSprintPressed { get; private set; }

    public Action JumpAction = () => { };
    public Action AttackAction = () => { };
    public Action MenuAction = () => { };
    public Action InteractAction = () => { };
    public Action InventoryAction = () => { };

    private void Start()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
        _jumpAction = InputSystem.actions.FindAction("Jump");
        _lookAction = InputSystem.actions.FindAction("Look");
        _isSprintPressed = InputSystem.actions.FindAction("Sprint");
        _attackAction = InputSystem.actions.FindAction("Attack");
        _menuAction = InputSystem.actions.FindAction("Menu");
        _interactAction = InputSystem.actions.FindAction("Interact");
        _inventoryAction = InputSystem.actions.FindAction("Inventory");

        _moveAction.performed += context => MoveInput = context.ReadValue<Vector2>();
        _moveAction.canceled += context => MoveInput = Vector2.zero;

        _lookAction.performed += context => LookInput = context.ReadValue<Vector2>();
        _lookAction.canceled += context => LookInput = Vector2.zero;

        _isSprintPressed.performed += context => IsSprintPressed = true;
        _isSprintPressed.canceled += context => IsSprintPressed = false;

        _jumpAction.started += context => Jump(context);
        _attackAction.started += context => Attack(context);
        _menuAction.started += context => Menu(context);
        _interactAction.started += context => Interact(context);
        _inventoryAction.started += context => Inventory(context);
    }

    void Attack(InputAction.CallbackContext context)
    {
        AttackAction?.Invoke();
    }

    void Jump(InputAction.CallbackContext context)
    {
        JumpAction?.Invoke();
    }

    void Menu(InputAction.CallbackContext context)
    {
        MenuAction?.Invoke();
    }

    void Interact(InputAction.CallbackContext context)
    {
        InteractAction?.Invoke();
    }

    void Inventory(InputAction.CallbackContext context)
    {
        InventoryAction?.Invoke();
    }
}
