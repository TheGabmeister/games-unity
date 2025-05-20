using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

public class Player : MonoBehaviour
{
    private Vector2 moveInput;

    // This method will be called automatically by PlayerInput (Send Messages)
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        float speed = 5f;
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y) * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }
}