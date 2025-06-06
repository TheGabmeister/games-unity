using UnityEngine;
using UnityEngine.InputSystem;
using EventBus;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 5f;
    [SerializeField] float _thrustSpeed = 7f;
    [SerializeField] GameObject _bulletPrefab;
    [SerializeField] Transform _firePoint;
    [SerializeField] AudioClip _bulletSound;
    [SerializeField] GameObject _deathParticleFx;

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
        Vector3 velocity = new Vector3(0, moveInput.y, 0) * _moveSpeed * Time.deltaTime;
        if (isThrusting)
        {
           velocity.x += _thrustSpeed * Time.deltaTime;
        }
        transform.position += velocity;
    }

    void Fire()
    {
        if (_bulletPrefab && _firePoint)
        {
            Instantiate(_bulletPrefab, _firePoint.position, _firePoint.rotation);
            Bus.SfxPlay.Publish(_bulletSound);
        }
    }

    void UseSmartBomb()
    {

            // Fire smart bomb event here
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

    public void OnHit()
    {
        Die();
    }



    void OnTriggerEnter2D()
    {
        Die();
    }

    void Die()
    {
        Bus.PlayerKilled.Publish();
        Instantiate(_deathParticleFx, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}