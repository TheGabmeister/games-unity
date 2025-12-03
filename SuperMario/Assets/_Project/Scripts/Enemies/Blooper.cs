using UnityEngine;
using PrimeTween;

public class Blooper : EnemyBase
{
    Rigidbody2D _rb;
    bool _isMoving = false;
    bool _chaseMode = false;
    Sequence _sequence;

    [Header("Variables")]
    //[SerializeField] float _thrustAngle = 45.0f;
    [SerializeField] float _moveDuration = 0.5f;
    [SerializeField] float _movePauseDuration = 0.8f;
    [SerializeField] float _moveSpeed = 10f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {

        if (_isMoving && _player)
        {
            if(_player.transform.position.x - gameObject.transform.position.x > 0)
            {
                _rb.linearVelocity = new Vector3(1 * _moveSpeed, 5, 0) ;
            }
            else
            {
                _rb.linearVelocity = new Vector3(-1 * _moveSpeed, 5, 0) ;
            }
        }
        else
        {
            _rb.linearVelocity = new Vector3(0, -2f, 0);
        }
    }

    public void ToggleChaseMode(bool value)
    {
        
        if (value)
        {
            _sequence = Sequence.Create(cycles: -1, CycleMode.Restart)
                .Chain(Tween.Delay(duration: _movePauseDuration, () => _isMoving = true))
                .Chain(Tween.Delay(duration: _moveDuration, () => _isMoving = false));
        }
        else
        {
            _sequence.Stop();
            _isMoving = false; 
        }
        _chaseMode = value;
    }
}
