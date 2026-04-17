using UnityEngine;

public class MiruToraeru : MonoBehaviour
{
    enum State { Hidden, Appearing, Attacking, Disappearing }

    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] HurtBox _hurtBox;
    [SerializeField] Transform _muzzle;
    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] float _hiddenDuration = 1.2f;
    [SerializeField] float _appearDuration = 0.3f;
    [SerializeField] float _attackDuration = 0.4f;
    [SerializeField] float _disappearDuration = 0.3f;
    [SerializeField] float _teleportOffset = 3.5f;
    [SerializeField] float _teleportHeight = 1f;
    [SerializeField] float _playerSearchRange = 14f;

    State _state;
    float _stateTimer;
    bool _firedThisAttack;

    void Awake()
    {
        if (!_spriteRenderer) _spriteRenderer = GetComponent<SpriteRenderer>();
        if (!_hurtBox) _hurtBox = GetComponent<HurtBox>();
        EnterState(State.Hidden);
    }

    void Update()
    {
        _stateTimer -= Time.deltaTime;

        UpdateFade();

        if (_state == State.Attacking && !_firedThisAttack)
        {
            FireAtPlayer();
            _firedThisAttack = true;
        }

        if (_stateTimer > 0f) return;

        EnterState(NextState(_state));
    }

    static State NextState(State s)
    {
        if (s == State.Hidden) return State.Appearing;
        if (s == State.Appearing) return State.Attacking;
        if (s == State.Attacking) return State.Disappearing;
        return State.Hidden;
    }

    void EnterState(State s)
    {
        _state = s;
        _stateTimer = DurationFor(s);
        _firedThisAttack = false;

        if (s == State.Hidden)
            TeleportNearPlayer();

        if (_hurtBox)
            _hurtBox.enabled = s == State.Attacking || s == State.Disappearing;

        UpdateFade();
    }

    float DurationFor(State s)
    {
        if (s == State.Hidden) return _hiddenDuration;
        if (s == State.Appearing) return _appearDuration;
        if (s == State.Attacking) return _attackDuration;
        return _disappearDuration;
    }

    void UpdateFade()
    {
        if (!_spriteRenderer) return;
        float alpha = CurrentAlpha();
        var c = _spriteRenderer.color;
        c.a = alpha;
        _spriteRenderer.color = c;
    }

    float CurrentAlpha()
    {
        if (_state == State.Hidden) return 0f;
        if (_state == State.Attacking) return 1f;
        if (_state == State.Appearing)
        {
            float t = 1f - Mathf.Clamp01(_stateTimer / _appearDuration);
            return t;
        }
        // Disappearing
        return Mathf.Clamp01(_stateTimer / _disappearDuration);
    }

    void TeleportNearPlayer()
    {
        var player = FindPlayer();
        if (!player) return;

        int side = Random.value < 0.5f ? -1 : 1;
        Vector2 target = (Vector2)player.position + new Vector2(side * _teleportOffset, _teleportHeight);
        transform.position = target;

        int facing = player.position.x < target.x ? -1 : 1;
        var scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * facing;
        transform.localScale = scale;
    }

    void FireAtPlayer()
    {
        if (!_projectilePrefab) return;
        var player = FindPlayer();
        if (!player) return;

        Vector3 spawnPos = _muzzle ? _muzzle.position : transform.position;
        Vector2 direction = ((Vector2)player.position - (Vector2)spawnPos).normalized;
        if (direction.sqrMagnitude < 0.0001f) direction = Vector2.right;

        float z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Instantiate(_projectilePrefab, spawnPos, Quaternion.Euler(0f, 0f, z));
    }

    Transform FindPlayer()
    {
        var collider = Physics2D.OverlapCircle(transform.position, _playerSearchRange, 1 << Layers.Player);
        if (!collider) return null;
        return collider.attachedRigidbody ? collider.attachedRigidbody.transform : collider.transform;
    }
}
