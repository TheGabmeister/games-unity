using UnityEngine;

public class Gravity : MonoBehaviour
{
    [SerializeField] float _gravity = 30f;
    [SerializeField] float _maxFallSpeed = 20f;
    [SerializeField] float _halfHeight = 0.5f;
    [SerializeField] float _groundProbeDistance = 0.05f;

    float _verticalSpeed;

    void FixedUpdate()
    {
        if (IsGrounded() && _verticalSpeed <= 0f)
        {
            _verticalSpeed = 0f;
            return;
        }

        _verticalSpeed = Mathf.Max(_verticalSpeed - _gravity * Time.fixedDeltaTime, -_maxFallSpeed);
        transform.position += Vector3.up * (_verticalSpeed * Time.fixedDeltaTime);
    }

    bool IsGrounded()
    {
        Vector2 origin = (Vector2)transform.position + Vector2.down * (_halfHeight - 0.02f);
        var hit = Physics2D.Raycast(origin, Vector2.down, _groundProbeDistance + 0.02f, 1 << Layers.Environment);
        return hit.collider;
    }
}
