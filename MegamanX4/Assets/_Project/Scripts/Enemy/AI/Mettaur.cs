using UnityEngine;

public class Mettaur : MonoBehaviour
{
    enum State { Hiding, Peeking }

    [SerializeField] Transform _muzzle;
    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] float _hideDuration = 1.2f;
    [SerializeField] float _peekDuration = 0.8f;
    [SerializeField] float _spreadStep = 30f;
    [SerializeField] int _shotCount = 3;

    HurtBox _hurtBox;
    State _state;
    float _stateTimer;
    bool _firedThisPeek;

    void Awake()
    {
        _hurtBox = GetComponent<HurtBox>();
        EnterState(State.Hiding);
    }

    void Update()
    {
        _stateTimer -= Time.deltaTime;

        if (_state == State.Peeking && !_firedThisPeek)
        {
            FireSpread(_shotCount);
            _firedThisPeek = true;
        }

        if (_stateTimer > 0f) return;

        EnterState(_state == State.Hiding ? State.Peeking : State.Hiding);
    }

    void EnterState(State s)
    {
        _state = s;
        _stateTimer = s == State.Hiding ? _hideDuration : _peekDuration;
        _firedThisPeek = false;
        if (_hurtBox) _hurtBox.enabled = s == State.Peeking;
    }

    void FireSpread(int n)
    {
        if (!_muzzle || !_projectilePrefab || n <= 0) return;

        float start = -_spreadStep * (n - 1) * 0.5f;
        for (int i = 0; i < n; i++)
        {
            float angle = start + _spreadStep * i;
            Quaternion rot = _muzzle.rotation * Quaternion.Euler(0f, 0f, angle);
            Instantiate(_projectilePrefab, _muzzle.position, rot);
        }
    }
}
