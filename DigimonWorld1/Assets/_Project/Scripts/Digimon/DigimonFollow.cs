using UnityEngine;

public class DigimonFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _moveSpeed = 2.5f;
    [SerializeField] private float _followDistance = 2f;
    [SerializeField] private float _stopDistance = 1.5f;

    private CharacterController _controller;
    private float _verticalVelocity;
    private const float Gravity = -15f;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (_target == null)
            return;

        Vector3 toTarget = _target.position - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        if (_controller.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        _verticalVelocity += Gravity * Time.deltaTime;

        if (distance > _followDistance)
        {
            Vector3 moveDir = toTarget.normalized;
            Vector3 velocity = moveDir * _moveSpeed;
            velocity.y = _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(moveDir, Vector3.up);
        }
        else if (distance > _stopDistance)
        {
            Vector3 moveDir = toTarget.normalized;
            float speed = _moveSpeed * ((distance - _stopDistance) / (_followDistance - _stopDistance));
            Vector3 velocity = moveDir * speed;
            velocity.y = _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(moveDir, Vector3.up);
        }
        else
        {
            Vector3 velocity = Vector3.zero;
            velocity.y = _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }
    }
}
