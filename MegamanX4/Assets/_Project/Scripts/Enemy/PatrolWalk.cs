using UnityEngine;

public class PatrolWalk : MonoBehaviour
{
    [SerializeField] float _speed = 2f;
    [SerializeField] float _wallCheckDistance = 0.35f;
    [SerializeField] float _edgeCheckDistance = 0.6f;
    [SerializeField] Vector2 _bodyHalfExtents = new(0.3f, 0.5f);
    [SerializeField] int _initialFacing = 1;

    int _facing = 1;
    bool _paused;

    public bool IsPatrolling => !_paused;
    public int Facing => _facing;

    void Awake()
    {
        _facing = _initialFacing >= 0 ? 1 : -1;
        ApplyFacingToScale();
    }

    void Update()
    {
        if (_paused) return;

        if (IsWallAhead() || IsEdgeAhead())
        {
            _facing = -_facing;
            ApplyFacingToScale();
            return;
        }

        transform.position += Vector3.right * (_facing * _speed * Time.deltaTime);
    }

    public void Pause() => _paused = true;
    public void Resume() => _paused = false;

    bool IsWallAhead()
    {
        Vector2 origin = transform.position;
        Vector2 direction = new(_facing, 0);
        var hit = Physics2D.Raycast(origin, direction, _bodyHalfExtents.x + _wallCheckDistance, 1 << Layers.Environment);
        return hit.collider;
    }

    bool IsEdgeAhead()
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(_facing * _bodyHalfExtents.x, -_bodyHalfExtents.y + 0.02f);
        var hit = Physics2D.Raycast(origin, Vector2.down, _edgeCheckDistance, 1 << Layers.Environment);
        return !hit.collider;
    }

    void ApplyFacingToScale()
    {
        var scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * _facing;
        transform.localScale = scale;
    }
}
