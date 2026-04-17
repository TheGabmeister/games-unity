using System;
using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    [SerializeField] float _range = 8f;
    [SerializeField, Range(0f, 360f)] float _coneAngle = 360f;
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

        if (_coneAngle < 360f)
        {
            Vector2 origin = transform.position;
            Vector2 toPlayer = ((Vector2)_playerTransform.position - origin);
            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                Vector2 forward = (Vector2)transform.right * Mathf.Sign(transform.lossyScale.x);
                float angle = Vector2.Angle(forward, toPlayer);
                if (angle > _coneAngle * 0.5f) return false;
            }
        }

        if (!_requireLineOfSight) return true;

        Vector2 losOrigin = transform.position;
        Vector2 target = _playerTransform.position;
        Vector2 direction = target - losOrigin;
        float distance = direction.magnitude;

        var hit = Physics2D.Raycast(losOrigin, direction, distance, 1 << Layers.Environment);
        return !hit.collider;
    }
}
