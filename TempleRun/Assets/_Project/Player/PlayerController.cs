using UnityEngine;
using EventBus;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameSettings _gameSettings;
    float _speed = 10.0f;
    [SerializeField] float _jumpForce = 1250f;
    [SerializeField] GameObject _camera;
    [SerializeField] AudioClip _jumpSound;
    [SerializeField] AudioClip _dieSound;
    [SerializeField] Rigidbody _rigidBody;
    PlayerInput _input;
    bool _isMoving = false;


    void OnEnable()
    {
        Bus<EV_PlayerPossess>.Add(Possess);
        Bus<EV_GameStart>.Add(StartMoving);
    }

    void OnDisable()
    {
        Bus<EV_PlayerPossess>.Remove(Possess);
        Bus<EV_GameStart>.Remove(StartMoving);
    }

    void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _speed = _gameSettings.initialSpeed;
    }

    void OnJump()
    {
        _rigidBody.AddForce(new Vector3(0, _jumpForce, 0));
        Bus<EV_SfxPlay>.Raise(new EV_SfxPlay { clip = _jumpSound});
    }

    void StartMoving()
    {
        _isMoving = true;
    }

    void Update()
    {
        if (_isMoving)
        {
            transform.Translate(Vector3.forward * _speed * Time.deltaTime);
        }
    }

    public void Possess(EV_PlayerPossess e)
    {
        _camera.SetActive(e.value);
        _input.enabled = e.value;
    }
}
