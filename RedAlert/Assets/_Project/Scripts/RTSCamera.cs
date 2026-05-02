using UnityEngine;

public class RTSCamera : MonoBehaviour
{
    [SerializeField] private float _panSpeed = 10f;
    [SerializeField] private float _edgeScrollSpeed = 10f;
    [SerializeField] private float _edgeScrollThreshold = 10f;
    [SerializeField] private float _sidebarWidthFraction = 0.15f;

    private Camera _camera;

    public static RTSCamera Instance { get; private set; }
    public float SidebarWidthFraction => _sidebarWidthFraction;

    void Awake()
    {
        Instance = this;
        _camera = GetComponent<Camera>();
        _camera.rect = new Rect(0f, 0f, 1f - _sidebarWidthFraction, 1f);
    }

    void LateUpdate()
    {
        if (InputManager.Instance == null) return;
        HandlePan();
        HandleEdgeScroll();
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
        float viewportRight = Screen.width * (1f - _sidebarWidthFraction);

        if (mouse.x <= _edgeScrollThreshold) move.x -= 1f;
        else if (mouse.x >= viewportRight - _edgeScrollThreshold) move.x += 1f;

        if (mouse.y <= _edgeScrollThreshold) move.y -= 1f;
        else if (mouse.y >= Screen.height - _edgeScrollThreshold) move.y += 1f;

        if (move.sqrMagnitude > 0f)
            transform.position += move.normalized * (_edgeScrollSpeed * Time.unscaledDeltaTime);
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
