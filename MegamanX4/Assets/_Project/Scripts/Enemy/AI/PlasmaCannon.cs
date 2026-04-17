using UnityEngine;

public class PlasmaCannon : MonoBehaviour
{
    enum State { Idle, Charging, Firing, Cooldown }

    [SerializeField] GameObject _beam;
    [SerializeField] SpriteRenderer _chargeTelegraph;
    [SerializeField] float _idleDuration = 0.5f;
    [SerializeField] float _chargeDuration = 1f;
    [SerializeField] float _fireDuration = 1f;
    [SerializeField] float _cooldownDuration = 1f;

    State _state;
    float _stateTimer;

    void Awake() => EnterState(State.Idle);

    void Update()
    {
        _stateTimer -= Time.deltaTime;
        if (_stateTimer > 0f) return;
        EnterState(NextState(_state));
    }

    static State NextState(State s)
    {
        if (s == State.Idle) return State.Charging;
        if (s == State.Charging) return State.Firing;
        if (s == State.Firing) return State.Cooldown;
        return State.Charging;
    }

    void EnterState(State s)
    {
        _state = s;
        _stateTimer = DurationFor(s);
        if (_beam) _beam.SetActive(s == State.Firing);
        if (_chargeTelegraph) _chargeTelegraph.enabled = s == State.Charging;
    }

    float DurationFor(State s)
    {
        if (s == State.Idle) return _idleDuration;
        if (s == State.Charging) return _chargeDuration;
        if (s == State.Firing) return _fireDuration;
        if (s == State.Cooldown) return _cooldownDuration;
        return 0f;
    }
}
