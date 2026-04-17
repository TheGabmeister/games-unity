using UnityEngine;

[RequireComponent(typeof(PlayerDetector))]
public class SwoopAttack : MonoBehaviour
{
    enum State { Idle, Diving, Returning, Cooldown }

    [SerializeField] float _swoopSpeed = 8f;
    [SerializeField] float _returnSpeed = 6f;
    [SerializeField] float _cooldown = 3f;
    [SerializeField] float _arrivalDistance = 0.5f;
    [SerializeField] Transform _returnAnchor;

    PlayerDetector _detector;
    HoverSine _hover;
    State _state = State.Idle;
    Vector2 _diveTarget;
    Vector2 _returnTarget;
    float _cooldownUntil;

    void Awake()
    {
        _detector = GetComponent<PlayerDetector>();
        _hover = GetComponent<HoverSine>();
    }

    void Update()
    {
        switch (_state)
        {
            case State.Idle: TryStartDive(); break;
            case State.Diving: Step(_diveTarget, _swoopSpeed, State.Returning); break;
            case State.Returning: Step(_returnTarget, _returnSpeed, State.Cooldown); break;
            case State.Cooldown:
                if (Time.time >= _cooldownUntil) _state = State.Idle;
                break;
        }
    }

    void TryStartDive()
    {
        if (!_detector.CanSeePlayer) return;
        _returnTarget = _returnAnchor ? (Vector2)_returnAnchor.position : (Vector2)transform.position;
        _diveTarget = _detector.PlayerPosition;
        if (_hover) _hover.Pause();
        _state = State.Diving;
    }

    void Step(Vector2 target, float speed, State next)
    {
        Vector2 pos = transform.position;
        Vector2 delta = target - pos;
        float step = speed * Time.deltaTime;

        if (delta.magnitude <= Mathf.Max(step, _arrivalDistance))
        {
            transform.position = new Vector3(target.x, target.y, transform.position.z);
            EnterState(next);
            return;
        }

        transform.position = pos + delta.normalized * step;
    }

    void EnterState(State next)
    {
        _state = next;
        if (next == State.Cooldown)
        {
            _cooldownUntil = Time.time + _cooldown;
            if (_hover)
            {
                _hover.SetCenter(transform.position.y);
                _hover.Resume();
            }
        }
    }
}
