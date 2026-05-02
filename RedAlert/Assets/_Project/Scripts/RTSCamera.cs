using UnityEngine;

public class RTSCamera : MonoBehaviour
{
    [SerializeField] private float _panSpeed = 10f;
    [SerializeField] private float _edgeScrollSpeed = 10f;
    [SerializeField] private float _edgeScrollThreshold = 10f;
    [SerializeField] private float _zoomStep = 1f;
    [SerializeField] private float _minZoom = 3f;
    [SerializeField] private float _maxZoom = 15f;

    private Camera _camera;

    public static RTSCamera Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        _camera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (InputManager.Instance == null) return;
        HandlePan();
        HandleEdgeScroll();
        HandleZoom();
        ClampToMap();
    }

    void HandlePan()
    {
        Vector2 pan = InputManager.Instance.CameraPan.ReadValue<Vector2>();
        if (pan.sqrMagnitude > 0.01f)
        {
            Vector3 move = new Vector3(pan.x, pan.y, 0f) * (_panSpeed * Time.unscaledDeltaTime);
            transform.position += move;
        }
    }

    void HandleEdgeScroll()
    {
        Vector2 mouse = InputManager.Instance.MousePosition;
        Vector3 move = Vector3.zero;

        if (mouse.x <= _edgeScrollThreshold) move.x -= 1f;
        else if (mouse.x >= Screen.width - _edgeScrollThreshold) move.x += 1f;

        if (mouse.y <= _edgeScrollThreshold) move.y -= 1f;
        else if (mouse.y >= Screen.height - _edgeScrollThreshold) move.y += 1f;

        if (move.sqrMagnitude > 0f)
            transform.position += move.normalized * (_edgeScrollSpeed * Time.unscaledDeltaTime);
    }

    void HandleZoom()
    {
        float scroll = InputManager.Instance.CameraZoom.ReadValue<float>();
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float delta = Mathf.Sign(scroll) * _zoomStep;
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - delta, _minZoom, _maxZoom);
        }
    }

    void ClampToMap()
    {
        if (MapManager.Instance == null || MapManager.Instance.Width == 0) return;

        float halfHeight = _camera.orthographicSize;
        float halfWidth = halfHeight * _camera.aspect;

        float minX = halfWidth;
        float maxX = MapManager.Instance.Width - halfWidth;
        float minY = halfHeight;
        float maxY = MapManager.Instance.Height - halfHeight;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }

    public void JumpTo(Vector3 position)
    {
        Vector3 pos = transform.position;
        pos.x = position.x;
        pos.y = position.y;
        transform.position = pos;
    }
}
