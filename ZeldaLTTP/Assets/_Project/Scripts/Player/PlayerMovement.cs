using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    PlayerInputHandler _input;
    Rigidbody2D _rb;
    SpriteRenderer _spriteRenderer;

    Vector2 _moveVector;

    [SerializeField] float _moveSpeed = 8f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _input = GetComponent<PlayerInputHandler>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        _moveVector = _input.MoveInput;
    }

    void FixedUpdate()
    {
        HandleMovement();
        RotateToMovement();
    }

    void HandleMovement()
    {
        _rb.linearVelocity = _moveVector * _moveSpeed;
    }

    void RotateToMovement()
    {
        if (_moveVector == Vector2.zero) return;

        // Convert vector to an angle between 0 and 360
        float angle = Vector2.SignedAngle(Vector2.right, _moveVector);
        if (angle < 0)
        {
            angle += 360;
        }

        // Snap rotation to only up,down,left,right
        if (angle < 45 || angle >= 315) _rb.MoveRotation(Quaternion.Euler(0f, 0f, 0f));
        else if (angle >= 45 && angle < 135) _rb.MoveRotation(Quaternion.Euler(0f, 0f, 90f));
        else if (angle >= 135 && angle < 225) _rb.MoveRotation(Quaternion.Euler(0f, 0f, 180f));
        else if (angle >= 225 || angle < 315) _rb.MoveRotation(Quaternion.Euler(0f, 0f, 270f));
    }
}
