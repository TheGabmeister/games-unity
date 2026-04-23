using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private float _walkSpeed = 3f;
    [SerializeField] private float _sprintSpeed = 6f;
    [SerializeField] private float _gravity = -15f;
    [SerializeField] private float _interactDistance = 3f;

    private CharacterController _controller;
    private InputSystem_Actions _input;
    private float _verticalVelocity;
    private IInteractable _currentInteractable;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
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

    private void Update()
    {
        Vector2 moveInput = _input.Player.Move.ReadValue<Vector2>();
        bool sprinting = _input.Player.Sprint.IsPressed();

        Vector3 camForward = _cameraTransform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = _cameraTransform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDir = camForward * moveInput.y + camRight * moveInput.x;

        if (_controller.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        _verticalVelocity += _gravity * Time.deltaTime;

        float speed = sprinting ? _sprintSpeed : _walkSpeed;
        Vector3 velocity = moveDir * speed;
        velocity.y = _verticalVelocity;

        _controller.Move(velocity * Time.deltaTime);

        if (moveDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(moveDir, Vector3.up);

        IInteractable newInteractable = null;

        Vector3 rayOrigin = transform.position + Vector3.up;
        Vector3 rayDir = transform.forward;
        Debug.DrawRay(rayOrigin, rayDir * _interactDistance, Color.yellow);

        if (Physics.SphereCast(rayOrigin, 0.3f, rayDir, out RaycastHit hit, _interactDistance))
            newInteractable = hit.collider.GetComponent<IInteractable>();

        if (newInteractable != _currentInteractable)
        {
            _currentInteractable?.HidePrompt();
            newInteractable?.ShowPrompt();
            _currentInteractable = newInteractable;
        }

        if (_currentInteractable != null && _input.Player.Interact.WasPressedThisFrame())
            _currentInteractable.Interact();
    }
}
