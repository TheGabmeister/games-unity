using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private AudioClip _swingSound;
    private bool isDragging = false;
    private Vector2 lastPointerPosition;

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isDragging = true;
            lastPointerPosition = Mouse.current.position.ReadValue();
            SfxManager.Instance.PlaySound(_swingSound);
            //Debug.Log("Drag Start: " + lastPointerPosition);
        }
        else if (context.canceled)
        {
            isDragging = false;
            //Debug.Log("Drag End: " + Mouse.current.position.ReadValue());
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
            //Debug.Log("Dragging: " + delta);
        }
    }
}
