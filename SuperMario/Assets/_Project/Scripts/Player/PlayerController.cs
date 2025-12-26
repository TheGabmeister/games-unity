using ScriptableObjectArchitecture;
using System.Collections;
using UnityEngine;
using PrimeTween;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour
{
    PlayerInputHandler _input;
    Vector2 _moveVector = Vector2.zero;

    Animator _animator;
    Rigidbody2D _rb;
    SpriteRenderer _spriteRenderer;

    bool _isGrounded = true;
    bool _isRunning = false;
    bool _isCrouching = false;
    bool _isSwimming = false;

    [SerializeField] float _invulnerabilityTime = 3.0f;
    [SerializeField] float _starModeTime = 8.0f;
    bool _isInvulnerable = false;
    bool _isBig = false;
    bool _isFireModeActive = false;

    [SerializeField] float _moveSpeed = 5f;
    [SerializeField] float _swimSpeed = 3f;
    [SerializeField] float _jumpForce = 10f;
    [SerializeField] float _runSpeed = 8f;
    [SerializeField] AudioClip _jumpSound;
    [SerializeField] AudioClip _deathSound;
    [SerializeField] AudioClip _fireSound;
    [SerializeField] GameObject _fireBall;
    [SerializeField] Vector2Reference _playerPosition;

    [Header("Listen to these events...")]
    [SerializeField] GameEvent _onFinishedTimer;
    [SerializeField] GameEvent _onPlayerFell;
    [SerializeField] GameEvent _onReachedFinishLine;
    [SerializeField] GameEvent _onPlayerDamaged;

    [Header("Call these events...")]
    [SerializeField] GameEvent _onPlayerDied;
    [SerializeField] GameEvent _onPauseGameToggle;
    [SerializeField] BoolGameEvent _toggleStarModeMusic;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _input = GetComponent<PlayerInputHandler>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _input.FireAction += Fire;
        _input.JumpAction += Jump;
    }

    private void OnDestroy()
    {
        _input.FireAction -= Fire;
        _input.JumpAction -= Jump;
    }

    void OnEnable()
    {
        _onFinishedTimer.AddListener(StartDeathSequence);
        _onPlayerFell.AddListener(StartDeathSequence);
        _onPlayerDamaged.AddListener(TakeDamage);
    }

    void OnDisable()
    {
        _onFinishedTimer.RemoveListener(StartDeathSequence);
        _onPlayerFell.RemoveListener(StartDeathSequence);
        _onPlayerDamaged.RemoveListener(TakeDamage);
    }

    private void Update()
    {
        HandleAnimation();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        _moveVector = _input.MoveInput;
        if (_moveVector.y < -0.5)
        {
            _isCrouching = true;
            return;
        }
        else
        {
            _isCrouching = false;
        }

        

        if (_isSwimming)
        {
            SwimMovement();
        }
        else
        {
            GroundMovement();
        }

        // Save position to scriptable object for camera to reference
        _playerPosition.Value = new Vector2(transform.position.x, transform.position.y);
    }

    void GroundMovement()
    {
        _rb.gravityScale = 3;
        _rb.linearVelocity = new Vector2(_moveVector.x * (_input.IsSprintPressed ? _runSpeed : _moveSpeed), _rb.linearVelocity.y);
        RotateToMovement();
    }

    void SwimMovement()
    {
        _rb.gravityScale = 0;
        _rb.linearVelocity = new Vector2(_moveVector.x * _swimSpeed, _moveVector.y * _swimSpeed);
        RotateToMovement();
    }

    private void Fire()
    {
        Instantiate(_fireBall, transform.position, transform.rotation);
        _playerPosition.Value = new Vector2(transform.position.x, transform.position.y);
        SFXManager.Instance.Play(_fireSound);
    }

    void Jump()
    {
        if (_isGrounded)
        {
            _rb.AddForce(new Vector2(0f, _jumpForce), ForceMode2D.Impulse);
            _isGrounded = false;
            //_onPlaySound.Raise(_jumpSound);
            SFXManager.Instance.Play(_jumpSound);
        }
    }

    // Rotate the character based on the direction of movement
    void RotateToMovement()
    {
        if (_moveVector.x > 0)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (_moveVector.x < 0)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }

    void HandleAnimation()
    {
        
        _animator.SetFloat("Speed", Mathf.Abs(_rb.linearVelocityX));
        _animator.SetBool("IsGrounded", _isGrounded);
        _animator.SetFloat("VerticalVelocity", _rb.linearVelocityY);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if player is grounded
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            _isGrounded = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 4)
        {
            _isSwimming = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 4)
        {
            _isSwimming = false;
        }
    }

    void StartDeathSequence()
    {
        // Death Animation
        //DisableInput();
        SFXManager.Instance.Play(_deathSound);
        _onPlayerDied.Raise();
        Destroy(gameObject);
    }

    void TakeDamage()
    {
        if (_isInvulnerable) return;
        
        if (_isBig)
        {
            Shrink();
            ActivateInvulnerability();
        }
        else
        {
            StartDeathSequence();
        }
    }

    void ActivateInvulnerability()
    {
        _isInvulnerable = true;

        Sequence sequence = Sequence.Create(cycles: -1, Sequence.SequenceCycleMode.Yoyo)
            .Chain(Tween.Color(_spriteRenderer, Color.clear, duration: 0.05f, ease: Ease.Linear))
            .Chain(Tween.Color(_spriteRenderer, Color.white, duration: 0.05f, ease: Ease.Linear));

        Tween.Delay(duration: _invulnerabilityTime, () =>
            sequence.Stop(),
            _isInvulnerable = false
            );
    }

    public void ActivateStarMode()
    {
        Sequence sequence = Sequence.Create(cycles: -1, Sequence.SequenceCycleMode.Yoyo)
            .Chain(Tween.Color(_spriteRenderer, Color.red, duration: 0.05f, ease: Ease.Linear))
            .Chain(Tween.Color(_spriteRenderer, Color.yellow, duration: 0.05f, ease: Ease.Linear))
            .Chain(Tween.Color(_spriteRenderer, Color.green, duration: 0.05f, ease: Ease.Linear))
            .Chain(Tween.Color(_spriteRenderer, Color.cyan, duration: 0.05f, ease: Ease.Linear))
            .Chain(Tween.Color(_spriteRenderer, Color.blue, duration: 0.05f, ease: Ease.Linear))
            .Chain(Tween.Color(_spriteRenderer, Color.magenta, duration: 0.05f, ease: Ease.Linear));

        Tween.Delay(duration: _starModeTime, () => sequence.Stop());
    }



    public void ActivateFireMode()
    {
        if(!_isBig)
        {
            Grow();
        }
        _isFireModeActive = true;
    }

    public void Grow()
    {
        _isBig = true;
        // Stop gameplay
        // Play animation
        // 
    }

    public void Shrink()
    {
        _isBig = false;
        _isFireModeActive = false;
    }
}