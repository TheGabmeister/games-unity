using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class LevelCamera : MonoBehaviour
{
    public enum VerticalScrollMode { ScrollAtWill, LockUnlessTriggered }

    [SerializeField] private Transform target;
    [SerializeField] private LevelBounds bounds;

    [Header("Forward bias")]
    [SerializeField] private float forwardBias = 2.5f;
    [SerializeField] private float biasResponseSpeed = 3f;

    [Header("Direction reversal")]
    [SerializeField] private float reversalThreshold = 0.65f;
    [SerializeField] private float reversalTransitionSpeed = 12f;

    [Header("Vertical")]
    [SerializeField] private VerticalScrollMode verticalMode = VerticalScrollMode.LockUnlessTriggered;
    [SerializeField] private float verticalLockWindow = 2.0f;
    [SerializeField] private float scrollAtWillDamping = 8f;

    [Header("Follow smoothing")]
    [SerializeField] private float followDamping = 8f;

    [Header("L/R Nudge")]
    [SerializeField] private float nudgeDistance = 3f;
    [SerializeField] private float nudgeSpeed = 6f;
    [SerializeField] private bool nudgeEnabled = true;

    private Camera _camera;
    private float _currentBias;
    private float _lockedY;
    private bool _lockedYInitialized;

    private int _biasSign;
    private bool _inReversal;
    private float _nudgeOffset;

    public Camera Camera => _camera != null ? _camera : GetComponent<Camera>();

    public void SetTarget(Transform t)
    {
        target = t;
        _lockedYInitialized = false;
        _inReversal = false;
    }

    public void SetBounds(LevelBounds b)
    {
        bounds = b;
    }

    public void SetNudgeEnabled(bool enabled)
    {
        nudgeEnabled = enabled;
        if (!enabled) _nudgeOffset = 0f;
    }

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _camera.orthographic = true;
        if (_camera.orthographicSize <= 0.01f) _camera.orthographicSize = 7f;
    }

    private void LateUpdate()
    {
        StepOnce(Time.deltaTime);
    }

    public void StepOnce(float dt)
    {
        if (target == null) return;

        if (!_lockedYInitialized)
        {
            _lockedY = target.position.y;
            _lockedYInitialized = true;
            _biasSign = 0;
        }

        // --- Forward bias with direction reversal hold ---
        float targetVx = 0f;
        if (target.TryGetComponent<Rigidbody2D>(out var rb))
            targetVx = rb.linearVelocity.x;

        int newSign = 0;
        if (Mathf.Abs(targetVx) > 0.5f)
            newSign = targetVx > 0f ? 1 : -1;

        if (newSign != 0 && newSign != _biasSign && _biasSign != 0)
            _inReversal = true;

        if (newSign != 0)
            _biasSign = newSign;

        if (_inReversal)
        {
            float halfW = _camera.orthographicSize * _camera.aspect;
            float screenX = target.position.x - transform.position.x;
            float normalizedX = (screenX + halfW) / (2f * halfW);
            bool pastThreshold = (_biasSign > 0 && normalizedX > reversalThreshold)
                              || (_biasSign < 0 && normalizedX < (1f - reversalThreshold));

            if (pastThreshold)
            {
                _currentBias = Mathf.MoveTowards(
                    _currentBias, _biasSign * forwardBias,
                    reversalTransitionSpeed * dt);

                if (Mathf.Abs(_currentBias - _biasSign * forwardBias) < 0.01f)
                    _inReversal = false;
            }
        }
        else
        {
            float biasTarget = _biasSign * forwardBias;
            _currentBias = Mathf.MoveTowards(_currentBias, biasTarget, biasResponseSpeed * dt);
        }

        // --- L/R camera nudge ---
        float nudgeTarget = 0f;
        if (nudgeEnabled && target.TryGetComponent<PlayerInputBinding>(out var input))
        {
            if (input.CameraNudgeLeftHeld) nudgeTarget = -nudgeDistance;
            else if (input.CameraNudgeRightHeld) nudgeTarget = nudgeDistance;
        }
        _nudgeOffset = Mathf.MoveTowards(_nudgeOffset, nudgeTarget, nudgeSpeed * dt);

        float desiredX = target.position.x + _currentBias + _nudgeOffset;

        // --- Vertical ---
        bool grounded = target.TryGetComponent<PlayerController>(out var pc) && pc.IsGrounded;
        float desiredY;

        if (verticalMode == VerticalScrollMode.ScrollAtWill)
        {
            float tY = 1f - Mathf.Exp(-scrollAtWillDamping * dt);
            desiredY = Mathf.Lerp(_lockedY, target.position.y, tY);
            _lockedY = desiredY;
        }
        else
        {
            float dy = target.position.y - _lockedY;
            if (grounded && Mathf.Abs(dy) > verticalLockWindow)
            {
                float excess = Mathf.Abs(dy) - verticalLockWindow;
                _lockedY += Mathf.Sign(dy) * excess;
            }
            desiredY = _lockedY;
        }

        // --- Smoothing ---
        float t = 1f - Mathf.Exp(-followDamping * dt);
        float newX = Mathf.Lerp(transform.position.x, desiredX, t);
        float newY = Mathf.Lerp(transform.position.y, desiredY, t);

        // --- Bounds clamp ---
        if (bounds != null && _camera != null)
        {
            var rect = bounds.Rect;
            float halfH = _camera.orthographicSize;
            float halfW = halfH * _camera.aspect;
            float minX = rect.xMin + halfW;
            float maxX = rect.xMax - halfW;
            float minY = rect.yMin + halfH;
            float maxY = rect.yMax - halfH;
            if (minX > maxX) { float m = (rect.xMin + rect.xMax) * 0.5f; minX = maxX = m; }
            if (minY > maxY) { float m = (rect.yMin + rect.yMax) * 0.5f; minY = maxY = m; }
            newX = Mathf.Clamp(newX, minX, maxX);
            newY = Mathf.Clamp(newY, minY, maxY);
        }

        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}
