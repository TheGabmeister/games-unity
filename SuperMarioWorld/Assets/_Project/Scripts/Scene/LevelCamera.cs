using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class LevelCamera : MonoBehaviour
{
    public Transform Target;

    [SerializeField] float _zOffset = 10f;
    [SerializeField] float _horizontalDeadZone = 0.1f;
    [SerializeField] float _horizontalRecenterSpeed = 8f;
    [SerializeField] float _nudgeSpeed = 3f;
    [SerializeField] float _nudgeMaxOffset = 1.5f;
    [SerializeField] float _verticalSmoothSpeed = 6f;

    VerticalCameraMode _verticalMode = VerticalCameraMode.NoScrollUnlessTriggered;
    LevelBounds _bounds;

    float _cameraX;
    float _cameraY;
    float _nudgeOffset;
    float _lastTargetGroundY;
    bool _initialized;
    int _travelDir = 1; // 1 = right, -1 = left

    // Horizontal offset from center in the travel direction (fraction of half-screen width)
    const float LeadOffset = 0.35f;
    // How far target must cross center (as fraction of half-screen) before camera reverses
    const float ReversalThreshold = 0.65f;

    public void SetVerticalMode(VerticalCameraMode mode) => _verticalMode = mode;
    public void SetBounds(LevelBounds bounds) => _bounds = bounds;

    void LateUpdate()
    {
        if (Target == null) return;

        if (!_initialized)
        {
            _cameraX = Target.position.x;
            _cameraY = Target.position.y;
            _lastTargetGroundY = Target.position.y;
            _initialized = true;
        }

        float dt = Time.deltaTime;
        float targetX = Target.position.x;
        float targetY = Target.position.y;

        UpdateHorizontal(targetX, dt);
        UpdateNudge(dt);
        UpdateVertical(targetY, dt);
        ClampToBounds();

        transform.position = new Vector3(_cameraX + _nudgeOffset, _cameraY, -_zOffset);
    }

    void UpdateHorizontal(float targetX, float dt)
    {
        float halfWidth = GetHalfScreenWidth();
        float lead = halfWidth * LeadOffset * _travelDir;
        float desiredX = targetX - lead;
        float offsetFromCenter = targetX - _cameraX;

        // Check if target has moved far enough past center to trigger a reversal
        if (_travelDir == 1 && offsetFromCenter < -halfWidth * ReversalThreshold)
            _travelDir = -1;
        else if (_travelDir == -1 && offsetFromCenter > halfWidth * ReversalThreshold)
            _travelDir = 1;

        // Dead zone: don't move if target is within the zone
        float deadZoneHalf = halfWidth * _horizontalDeadZone;
        float diff = desiredX - _cameraX;

        if (Mathf.Abs(diff) > deadZoneHalf)
            _cameraX = Mathf.Lerp(_cameraX, desiredX, _horizontalRecenterSpeed * dt);
    }

    void UpdateNudge(float dt)
    {
        var input = PlayerInputBinding.Instance;
        if (input == null) return;

        float nudgeInput = 0f;
        if (input.CameraNudgeLeftHeld) nudgeInput = -1f;
        if (input.CameraNudgeRightHeld) nudgeInput = 1f;

        float nudgeTarget = nudgeInput * _nudgeMaxOffset;
        _nudgeOffset = Mathf.MoveTowards(_nudgeOffset, nudgeTarget, _nudgeSpeed * dt);
    }

    void UpdateVertical(float targetY, float dt)
    {
        switch (_verticalMode)
        {
            case VerticalCameraMode.ScrollAtWill:
                _cameraY = Mathf.Lerp(_cameraY, targetY, _verticalSmoothSpeed * dt);
                break;

            case VerticalCameraMode.NoScrollUnlessTriggered:
                var pc = Target.GetComponent<PlayerController>();
                bool onGround = pc != null && !IsAirMode(pc);

                if (onGround)
                {
                    // Track ground position — only scroll when Mario lands at a different elevation
                    if (Mathf.Abs(targetY - _lastTargetGroundY) > 0.05f)
                    {
                        _lastTargetGroundY = targetY;
                        _cameraY = Mathf.Lerp(_cameraY, targetY, _verticalSmoothSpeed * dt);
                    }
                }
                break;
        }
    }

    static bool IsAirMode(PlayerController pc)
    {
        // PlayerController doesn't expose Mode directly, but we can infer from GroundProbe
        var probe = pc.GetComponent<GroundProbe>();
        return probe != null && !probe.IsGrounded;
    }

    void ClampToBounds()
    {
        if (_bounds == null)
            _bounds = Object.FindAnyObjectByType<LevelBounds>();
        if (_bounds == null) return;

        float halfW = GetHalfScreenWidth();
        float halfH = GetHalfScreenHeight();
        var rect = _bounds.Rect;

        _cameraX = Mathf.Clamp(_cameraX, rect.xMin + halfW, rect.xMax - halfW);
        _cameraY = Mathf.Clamp(_cameraY, rect.yMin + halfH, rect.yMax - halfH);
    }

    float GetHalfScreenWidth()
    {
        var cam = GetComponent<Camera>();
        return cam.orthographicSize * cam.aspect;
    }

    float GetHalfScreenHeight()
    {
        return GetComponent<Camera>().orthographicSize;
    }
}
