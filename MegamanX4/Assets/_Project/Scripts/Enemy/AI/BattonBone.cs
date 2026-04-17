using UnityEngine;

[RequireComponent(typeof(PlayerDetector))]
public class BattonBone : MonoBehaviour
{
    enum State { Sleeping, Dropping, Flying }

    [SerializeField] float _dropSpeed = 4f;
    [SerializeField] float _dropDuration = 0.35f;
    [SerializeField] float _flySpeed = 4f;
    [SerializeField] float _sineAmplitude = 0.8f;
    [SerializeField] float _sineFrequency = 2.5f;
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] Sprite _sleepingSprite;
    [SerializeField] Sprite _flyingSprite;

    PlayerDetector _detector;
    State _state = State.Sleeping;
    float _stateTimer;
    Vector2 _flyDirection;
    Vector2 _flyOrigin;
    float _flyTime;

    void Awake() => _detector = GetComponent<PlayerDetector>();

    void OnEnable() => _detector.PlayerDetected += OnPlayerDetected;
    void OnDisable() => _detector.PlayerDetected -= OnPlayerDetected;

    void Start()
    {
        ApplySprite(_sleepingSprite);
    }

    void OnPlayerDetected()
    {
        if (_state != State.Sleeping) return;
        _state = State.Dropping;
        _stateTimer = _dropDuration;
    }

    void Update()
    {
        if (_state == State.Sleeping) return;

        if (_state == State.Dropping)
        {
            transform.position += Vector3.down * (_dropSpeed * Time.deltaTime);
            _stateTimer -= Time.deltaTime;
            if (_stateTimer <= 0f) EnterFlying();
            return;
        }

        if (_state == State.Flying)
        {
            _flyTime += Time.deltaTime;
            Vector2 forward = _flyDirection * (_flySpeed * _flyTime);
            Vector2 perp = new Vector2(-_flyDirection.y, _flyDirection.x);
            float sine = Mathf.Sin(_flyTime * _sineFrequency * Mathf.PI * 2f) * _sineAmplitude;
            transform.position = _flyOrigin + forward + perp * sine;
        }
    }

    void EnterFlying()
    {
        _state = State.Flying;
        Vector2 here = transform.position;
        Vector2 toward = _detector.PlayerPosition - here;
        _flyDirection = toward.sqrMagnitude > 0.0001f ? toward.normalized : Vector2.left;
        _flyOrigin = here;
        _flyTime = 0f;
        ApplySprite(_flyingSprite);
    }

    void ApplySprite(Sprite sprite)
    {
        if (_spriteRenderer && sprite) _spriteRenderer.sprite = sprite;
    }
}
