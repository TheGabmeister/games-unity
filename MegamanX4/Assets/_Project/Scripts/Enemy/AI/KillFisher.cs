using UnityEngine;

public class KillFisher : MonoBehaviour
{
    enum Phase { Retracted, Dropping, Dangling, Retracting }

    [SerializeField] Transform _hook;
    [SerializeField] float _dropDistance = 2.5f;
    [SerializeField] float _dropSpeed = 6f;
    [SerializeField] float _retractSpeed = 4f;
    [SerializeField] float _dangleDuration = 0.8f;
    [SerializeField] float _retractedPause = 1.2f;

    Vector3 _hookRestPosition;
    Vector3 _hookExtendedPosition;
    Phase _phase;
    float _phaseTimer;

    void Awake()
    {
        if (_hook)
        {
            _hookRestPosition = _hook.localPosition;
            _hookExtendedPosition = _hookRestPosition + Vector3.down * _dropDistance;
        }
        EnterPhase(Phase.Retracted);
    }

    void Update()
    {
        if (!_hook) return;

        _phaseTimer -= Time.deltaTime;

        if (_phase == Phase.Retracted)
        {
            if (_phaseTimer <= 0f) EnterPhase(Phase.Dropping);
            return;
        }

        if (_phase == Phase.Dangling)
        {
            if (_phaseTimer <= 0f) EnterPhase(Phase.Retracting);
            return;
        }

        Vector3 target = _phase == Phase.Dropping ? _hookExtendedPosition : _hookRestPosition;
        float speed = _phase == Phase.Dropping ? _dropSpeed : _retractSpeed;
        _hook.localPosition = Vector3.MoveTowards(_hook.localPosition, target, speed * Time.deltaTime);

        if (_hook.localPosition == target)
            EnterPhase(_phase == Phase.Dropping ? Phase.Dangling : Phase.Retracted);
    }

    void EnterPhase(Phase p)
    {
        _phase = p;
        if (p == Phase.Retracted) _phaseTimer = _retractedPause;
        else if (p == Phase.Dangling) _phaseTimer = _dangleDuration;
    }
}
