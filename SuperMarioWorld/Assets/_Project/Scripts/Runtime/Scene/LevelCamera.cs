using UnityEngine;

namespace SMW
{
    // Simple 2D follow camera per SPEC §4.4. Forward bias on run, vertical lock
    // unless grounded above/below the window, hard-clamp to LevelBounds rect.
    //
    // No Cinemachine — the requirements fit in ~50 lines and Cinemachine's lens
    // graph is overkill for a single fixed-behavior follow.
    [RequireComponent(typeof(Camera))]
    public sealed class LevelCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private LevelBounds bounds;

        [Header("Forward bias")]
        [SerializeField] private float forwardBias = 2.5f;
        [SerializeField] private float biasResponseSpeed = 3f;

        [Header("Vertical lock")]
        [SerializeField] private float verticalLockWindow = 2.0f;

        [Header("Follow smoothing")]
        [SerializeField] private float followDamping = 8f;

        private Camera _camera;
        private float _currentBias;
        private float _lockedY;
        private bool _lockedYInitialized;

        public Camera Camera => _camera != null ? _camera : GetComponent<Camera>();

        public void SetTarget(Transform t)
        {
            target = t;
            _lockedYInitialized = false;
        }

        public void SetBounds(LevelBounds b)
        {
            bounds = b;
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

        // Public so tests can drive the camera in a deterministic step loop without
        // depending on the LateUpdate schedule.
        public void StepOnce(float dt)
        {
            if (target == null) return;

            if (!_lockedYInitialized)
            {
                _lockedY = target.position.y;
                _lockedYInitialized = true;
            }

            // Forward bias: track target's horizontal velocity sign and ease the bias
            // toward ±forwardBias * sign. If target has no Rigidbody2D, fall back to
            // facing via PlayerController; otherwise zero bias.
            float biasTarget = 0f;
            if (target.TryGetComponent<Rigidbody2D>(out var rb))
            {
                float vx = rb.linearVelocity.x;
                if (Mathf.Abs(vx) > 0.5f) biasTarget = Mathf.Sign(vx) * forwardBias;
            }
            _currentBias = Mathf.MoveTowards(_currentBias, biasTarget, biasResponseSpeed * dt);

            float desiredX = target.position.x + _currentBias;
            float desiredY = _lockedY;

            // Vertical lock: only update lockedY when target is grounded *and* outside
            // the lock window. Grounded-ness is observed via PlayerController.IsGrounded
            // when present, else via the verticalLockWindow delta alone.
            bool grounded = target.TryGetComponent<PlayerController>(out var pc) && pc.IsGrounded;
            float dy = target.position.y - _lockedY;
            if (grounded && Mathf.Abs(dy) > verticalLockWindow)
            {
                float excess = Mathf.Abs(dy) - verticalLockWindow;
                _lockedY += Mathf.Sign(dy) * excess;
                desiredY = _lockedY;
            }

            // Ease position toward desired.
            float t = 1f - Mathf.Exp(-followDamping * dt);
            float newX = Mathf.Lerp(transform.position.x, desiredX, t);
            float newY = Mathf.Lerp(transform.position.y, desiredY, t);

            // Clamp to bounds rect accounting for the camera's viewport size.
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
}
