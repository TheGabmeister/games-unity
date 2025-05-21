using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float thrustSpeed = 7f;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject smartBombPrefab;

    Vector2 moveInput;
    bool isThrusting = false;
    bool facingRight = true;

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnThrust(InputValue value)
    {
        isThrusting = value.isPressed;
    }

    void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            Fire();
        }
    }

    void OnSmartBomb(InputValue value)
    {
        if (value.isPressed)
        {
            UseSmartBomb();
        }
    }

    void OnHyperspace(InputValue value)
    {
        if (value.isPressed)
        {
            UseHyperspace();
        }
    }

    void OnReverse(InputValue value)
    {
        if (value.isPressed)
        {
            ReverseDirection();
        }
    }

    void Update()
    {
        Vector3 velocity = new Vector3(moveInput.x, 0, 0) * moveSpeed * Time.deltaTime;
        if (isThrusting)
        {
            velocity.y += thrustSpeed * Time.deltaTime;
        }
        transform.position += velocity;
    }

    void Fire()
    {
        if (bulletPrefab && firePoint)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }

    void UseSmartBomb()
    {
        // Implement smart bomb logic (e.g., destroy all enemies on screen)
        if (smartBombPrefab)
        {
            Instantiate(smartBombPrefab, transform.position, Quaternion.identity);
        }
    }

    void UseHyperspace()
    {
        // Teleport to a random position within game bounds
        Vector3 randomPos = new Vector3(
            Random.Range(-8f, 8f),
            Random.Range(-4f, 4f),
            transform.position.z
        );
        transform.position = randomPos;
    }

    void ReverseDirection()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        moveInput.x *= -1;
    }
}