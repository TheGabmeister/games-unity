using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;
using Obvious.Soap;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float _initalSpeed = 10.0f;
    float _currentSpeed = 10.0f;
    [SerializeField] float _jumpForce = 1250f;
    [SerializeField] GameObject _camera;
    [SerializeField] AudioClip _jumpSound;
    [SerializeField] AudioClip _dieSound;
    Rigidbody _rb;
    PlayerInput _input;
    bool _isMoving = false;
    bool _isAlive = true;
    bool _canCollide = true;

    [Header("Listen to these events...")] 
    [SerializeField] ScriptableEventNoParam _onPlayerStartMoving;

    void OnEnable()
    {
        _onPlayerStartMoving.OnRaised += StartMoving;
    }
    
    void OnDisable()
    {
        _onPlayerStartMoving.OnRaised -= StartMoving;
    }

    void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _rb = GetComponent<Rigidbody>();
    }

    void OnJump()
    {
        _rb.AddForce(new Vector3(0, _jumpForce, 0));
        //Bus<EV_SfxPlay>.Raise(new EV_SfxPlay { clip = _jumpSound });
    }

    void StartMoving()
    {
        _isMoving = true;
    }

    void Update()
    {
        if (_isMoving)
        {
            transform.Translate(Vector3.forward * _currentSpeed * Time.deltaTime);

#if UNITY_EDITOR || UNITY_STANDALONE
            // Simulate tilt with horizontal keys (A/D or Left/Right)
            Vector3 tilt = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
#else
            Vector3 tilt = Input.acceleration;
#endif
            transform.Translate(Vector3.right * tilt.x * 3 * Time.deltaTime);
        }
    }

    // void Possess(EV_PlayerPossess e)
    // {
    //     _camera.SetActive(e.value);
    //     _input.enabled = e.value;
    // }

    void OnTriggerEnter(Collider col)
    {
        if (!_isAlive) return;
        if (!_canCollide) return;

        if (col.gameObject.layer == LayerMask.NameToLayer("Killzone"))
        {
            _isMoving = false;
            _input.enabled = false;
            _isAlive = false;
            //Bus<EV_SfxPlay>.Raise(new EV_SfxPlay { clip = _dieSound });
            //Bus<EV_PlayerDied>.Raise(new EV_PlayerDied());
        }

        if (col.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Decelerate();
        }

    }

    void OnCollisionEnter(Collision col)
    {
        if (!_isAlive) return;
        if (!_canCollide) return;

        if (col.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Decelerate();
        }
    }

    void Decelerate()
    {
        // Prevent multiple collisions by accident
        _canCollide = false;

        float slowdownSpeed = 0.5f * _currentSpeed;

        // Make sure player does not decelerate to 0
        if (slowdownSpeed < 0.5f * _initalSpeed)
            slowdownSpeed = 0.5f * _initalSpeed;

        Tween.Custom(slowdownSpeed, _currentSpeed, duration: 1f, onValueChange: newVal => _currentSpeed = newVal);
        Tween.Delay(1.0f).OnComplete(() => _canCollide = true);
    }
}