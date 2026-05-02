using UnityEngine;
using System.Collections.Generic;

public class SelectionManager : MonoBehaviour
{
    private readonly List<Selectable> _selected = new();
    private readonly List<Selectable>[] _controlGroups = new List<Selectable>[9];

    private Camera _cam;
    private bool _isDragging;
    private Vector2 _dragStart;
    private float _lastClickTime;
    private Selectable _inspectedEnemy;
    private const float DoubleClickTime = 0.3f;
    private const float DragThreshold = 5f;

    public static SelectionManager Instance { get; private set; }
    public IReadOnlyList<Selectable> Selected => _selected;
    public bool IsDragging => _isDragging;
    public Vector2 DragStart => _dragStart;

    void Awake()
    {
        Instance = this;
        _cam = Camera.main;
        for (int i = 0; i < 9; i++)
            _controlGroups[i] = new List<Selectable>();
    }

    void Update()
    {
        if (InputManager.Instance == null) return;

        var input = InputManager.Instance;

        if (input.Select.WasPressedThisFrame())
        {
            _dragStart = input.MousePosition;
            _isDragging = false;
        }

        if (input.Select.IsPressed())
        {
            if (Vector2.Distance(input.MousePosition, _dragStart) > DragThreshold)
                _isDragging = true;
        }

        if (input.Select.WasReleasedThisFrame())
        {
            Vector2 mouse = input.MousePosition;

            if (_isDragging)
            {
                BoxSelect(_dragStart, mouse);
            }
            else
            {
                float now = Time.unscaledTime;
                if (now - _lastClickTime < DoubleClickTime)
                    DoubleClickSelect(mouse);
                else
                    ClickSelect(mouse);
                _lastClickTime = now;
            }
            _isDragging = false;
        }

        if (input.SelectAll.WasPressedThisFrame())
            SelectAllOnScreen();

        HandleControlGroups(input);
    }

    void ClickSelect(Vector2 screenPos)
    {
        ClearSelection();

        Vector3 world = _cam.ScreenToWorldPoint(screenPos);
        Vector2Int cell = MapManager.Instance.WorldToCell(world);
        Entity entity = MapManager.Instance.GetEntityAt(cell);

        if (entity == null) return;

        var selectable = entity.GetComponent<Selectable>();
        if (selectable == null) return;

        if (entity.OwnerPlayerIndex == PlayerManager.Instance.LocalPlayer.PlayerIndex)
        {
            selectable.Select();
            _selected.Add(selectable);
        }
        else
        {
            selectable.Select();
            _inspectedEnemy = selectable;
        }
    }

    void DoubleClickSelect(Vector2 screenPos)
    {
        Vector3 world = _cam.ScreenToWorldPoint(screenPos);
        Vector2Int cell = MapManager.Instance.WorldToCell(world);
        Entity clicked = MapManager.Instance.GetEntityAt(cell);
        if (clicked == null) return;

        ClearSelection();
        string targetName = clicked.EntityName;
        int localPlayer = PlayerManager.Instance.LocalPlayer.PlayerIndex;

        foreach (var entity in FindObjectsByType<Entity>(FindObjectsSortMode.None))
        {
            if (entity.EntityName != targetName) continue;
            if (entity.OwnerPlayerIndex != localPlayer) continue;

            Vector3 vp = _cam.WorldToViewportPoint(entity.transform.position);
            if (vp.x < 0 || vp.x > 1 || vp.y < 0 || vp.y > 1) continue;

            var selectable = entity.GetComponent<Selectable>();
            if (selectable != null)
            {
                selectable.Select();
                _selected.Add(selectable);
            }
        }
    }

    void BoxSelect(Vector2 start, Vector2 end)
    {
        ClearSelection();

        Rect rect = new Rect(
            Mathf.Min(start.x, end.x),
            Mathf.Min(start.y, end.y),
            Mathf.Abs(end.x - start.x),
            Mathf.Abs(end.y - start.y)
        );

        int localPlayer = PlayerManager.Instance.LocalPlayer.PlayerIndex;
        foreach (var entity in FindObjectsByType<Entity>(FindObjectsSortMode.None))
        {
            if (entity.OwnerPlayerIndex != localPlayer) continue;

            Vector3 screen = _cam.WorldToScreenPoint(entity.transform.position);
            if (!rect.Contains(new Vector2(screen.x, screen.y))) continue;

            var selectable = entity.GetComponent<Selectable>();
            if (selectable != null)
            {
                selectable.Select();
                _selected.Add(selectable);
            }
        }
    }

    void SelectAllOnScreen()
    {
        ClearSelection();
        int localPlayer = PlayerManager.Instance.LocalPlayer.PlayerIndex;

        foreach (var entity in FindObjectsByType<Entity>(FindObjectsSortMode.None))
        {
            if (entity.OwnerPlayerIndex != localPlayer) continue;

            Vector3 vp = _cam.WorldToViewportPoint(entity.transform.position);
            if (vp.x < 0 || vp.x > 1 || vp.y < 0 || vp.y > 1) continue;

            var selectable = entity.GetComponent<Selectable>();
            if (selectable != null)
            {
                selectable.Select();
                _selected.Add(selectable);
            }
        }
    }

    void HandleControlGroups(InputManager input)
    {
        for (int i = 0; i < 9; i++)
        {
            if (!input.Groups[i].WasPressedThisFrame()) continue;

            if (input.IsCtrlHeld)
                _controlGroups[i] = new List<Selectable>(_selected);
            else if (input.IsAltHeld)
                JumpToGroup(i);
            else
                RecallGroup(i);
        }
    }

    void RecallGroup(int group)
    {
        ClearSelection();
        foreach (var s in _controlGroups[group])
        {
            if (s == null) continue;
            s.Select();
            _selected.Add(s);
        }
    }

    void JumpToGroup(int group)
    {
        RecallGroup(group);
        if (_selected.Count == 0) return;

        Vector3 center = Vector3.zero;
        foreach (var s in _selected)
            center += s.transform.position;
        center /= _selected.Count;

        RTSCamera.Instance?.JumpTo(center);
    }

    public void ClearSelection()
    {
        foreach (var s in _selected)
            if (s != null) s.Deselect();
        _selected.Clear();

        if (_inspectedEnemy != null)
        {
            _inspectedEnemy.Deselect();
            _inspectedEnemy = null;
        }
    }

    void OnGUI()
    {
        if (!_isDragging || InputManager.Instance == null) return;

        Vector2 mouse = InputManager.Instance.MousePosition;
        float startY = Screen.height - _dragStart.y;
        float endY = Screen.height - mouse.y;

        Rect rect = new Rect(
            Mathf.Min(_dragStart.x, mouse.x),
            Mathf.Min(startY, endY),
            Mathf.Abs(mouse.x - _dragStart.x),
            Mathf.Abs(endY - startY)
        );

        GUI.color = new Color(0.2f, 1f, 0.2f, 0.25f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = new Color(0.2f, 1f, 0.2f, 1f);
        DrawRectBorder(rect, 1);
    }

    void DrawRectBorder(Rect rect, float thickness)
    {
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), Texture2D.whiteTexture);
    }
}
