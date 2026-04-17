using UnityEngine;

public class TriScan : MonoBehaviour
{
    enum State { Idle, Firing }

    [SerializeField] Transform _emitterRoot;
    [SerializeField] GameObject[] _beams;
    [SerializeField] float _idleDuration = 1.5f;
    [SerializeField] float _fireDuration = 1.5f;
    [SerializeField] float _rotationSpeed = 120f;

    State _state;
    float _stateTimer;

    void Awake() => EnterState(State.Idle);

    void Update()
    {
        if (_state == State.Firing && _emitterRoot)
            _emitterRoot.Rotate(0f, 0f, _rotationSpeed * Time.deltaTime);

        _stateTimer -= Time.deltaTime;
        if (_stateTimer > 0f) return;

        EnterState(_state == State.Idle ? State.Firing : State.Idle);
    }

    void EnterState(State s)
    {
        _state = s;
        _stateTimer = s == State.Idle ? _idleDuration : _fireDuration;
        SetBeamsActive(s == State.Firing);
    }

    void SetBeamsActive(bool active)
    {
        if (_beams == null) return;
        for (int i = 0; i < _beams.Length; i++)
            if (_beams[i]) _beams[i].SetActive(active);
    }
}
