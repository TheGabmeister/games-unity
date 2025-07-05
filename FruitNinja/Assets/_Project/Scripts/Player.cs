using UnityEngine;
using UnityEngine.InputSystem;
using UnityServiceLocator;

public class Player : MonoBehaviour
{
    ISfxManager _sfxManager;
    [SerializeField] private AudioClip _swingSound;
    
    void Awake()
    {
        ServiceLocator.Global.Get(out _sfxManager);
    }

    void Start()
    {
        _sfxManager.PlaySound(_swingSound);
    }
    
    private bool isDragging = false;
    private Vector2 lastPointerPosition;

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isDragging = true;
            lastPointerPosition = Mouse.current.position.ReadValue();
            Debug.Log("Drag Start: " + lastPointerPosition);
        }
        else if (context.canceled)
        {
            isDragging = false;
            Debug.Log("Drag End: " + Mouse.current.position.ReadValue());
        }
    }
    
    void Update()
    {
        if (isDragging)
        {
            Vector2 currentPointerPosition = Mouse.current.position.ReadValue();
            Vector2 delta = currentPointerPosition - lastPointerPosition;
            lastPointerPosition = currentPointerPosition;

            // Handle the dragging logic here
            Debug.Log("Dragging: " + delta);
        }
    }
}
