using System;
using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    [SerializeField] float _range = 8f;
    [SerializeField] bool _requireLineOfSight = true;

    bool _canSeePlayer;
    Transform _playerTransform;

    public bool CanSeePlayer => _canSeePlayer;

    public Vector2 PlayerPosition
    {
        get
        {
            if (!_playerTransform) return Vector2.zero;
            return _playerTransform.position;
        }
    }

    public event Action PlayerDetected;
    public event Action PlayerLost;

    void FixedUpdate()
    {
        bool wasVisible = _canSeePlayer;
        _canSeePlayer = CheckForPlayer();

        if (_canSeePlayer && !wasVisible)
            PlayerDetected?.Invoke();
        else if (!_canSeePlayer && wasVisible)
            PlayerLost?.Invoke();
    }

    bool CheckForPlayer()
    {
        var collider = Physics2D.OverlapCircle(transform.position, _range, 1 << Layers.Player);
        if (!collider) return false;

        if (collider.attachedRigidbody)
            _playerTransform = collider.attachedRigidbody.transform;
        else
            _playerTransform = collider.transform;

        if (!_requireLineOfSight) return true;

        Vector2 origin = transform.position;
        Vector2 target = _playerTransform.position;
        Vector2 direction = target - origin;
        float distance = direction.magnitude;

        var hit = Physics2D.Raycast(origin, direction, distance, 1 << Layers.Environment);
        return !hit.collider;
    }
}
