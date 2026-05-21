using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DependencyGraphWindow : EditorWindow
{
    DependencyGraph _graph;
    Vector2 _panOffset;
    bool _isPanning;
    Vector2 _panStartMouse;
    Vector2 _panStartOffset;
    bool _scanRequested;
    string _scanFolder = "Assets/_Project/Scripts";
    float _zoom = 1f;
    float _lastStyledZoom = -1f;
    Dictionary<string, int> _refCounts;
    int _edgeCount;

    const float ZoomMin = 0.25f;
    const float ZoomMax = 2f;
    const float ToolbarHeight = 21f;
    const float ArrowSize = 12f;
    const float ArrowAngle = 20f;
    const float EdgeWidth = 2.5f;
    static readonly Color EdgeColor = new(0.8f, 0.8f, 0.8f, 0.6f);

    GUIStyle _nodeStyle;
    GUIStyle _titleStyle;
    GUIStyle _subtitleStyle;

    [MenuItem("Tools/Dependency Graph")]
    static void Open()
    {
        var window = GetWindow<DependencyGraphWindow>("Dependency Graph");
        window.minSize = new Vector2(600, 400);
    }

    void OnGUI()
    {
        if (_scanRequested && Event.current.type == EventType.Layout)
        {
            _scanRequested = false;
            _graph = DependencyScanner.Scan(_scanFolder);
            if (_graph != null && _graph.Nodes.Count > 0)
            {
                DependencyGraphLayout.ApplyLayout(_graph, 50f, 50f);
                CacheGraphStats();
                CenterGraph();
            }
            Repaint();
        }

        InitStyles();
        DrawToolbar();

        if (_graph == null || _graph.Nodes.Count == 0)
        {
            DrawEmptyState();
            return;
        }

        HandleZoom();
        HandlePanning();
        DrawEdges();
        DrawNodes();

        if (Event.current.type == EventType.MouseDrag)
            Repaint();
    }

    void CacheGraphStats()
    {
        _refCounts = new Dictionary<string, int>();
        _edgeCount = 0;
        foreach (var node in _graph.Nodes)
        {
            foreach (var dep in node.DependsOn)
            {
                _refCounts.TryGetValue(dep, out int c);
                _refCounts[dep] = c + 1;
                _edgeCount++;
            }
        }
    }

    void InitStyles()
    {
        _nodeStyle ??= new GUIStyle("flow node 0");
        _titleStyle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.UpperCenter,
            normal = { textColor = Color.white }
        };
        _subtitleStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.UpperCenter,
            normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
        };

        if (!Mathf.Approximately(_lastStyledZoom, _zoom))
        {
            _lastStyledZoom = _zoom;
            int pad = Mathf.Max(1, Mathf.RoundToInt(8 * _zoom));
            _nodeStyle.padding = new RectOffset(pad, pad, pad, pad);
            _titleStyle.fontSize = Mathf.Max(1, Mathf.RoundToInt(11 * _zoom));
            _subtitleStyle.fontSize = Mathf.Max(1, Mathf.RoundToInt(9 * _zoom));
        }
    }

    void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("Scan", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            _scanRequested = true;
            Repaint();
        }

        GUILayout.Label(_scanFolder, EditorStyles.toolbarButton, GUILayout.MinWidth(100));

        if (GUILayout.Button("Browse", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            var picked = EditorUtility.OpenFolderPanel("Select Scripts Folder", _scanFolder, "");
            if (!string.IsNullOrEmpty(picked))
            {
                var dataPath = Application.dataPath.Replace('\\', '/');
                picked = picked.Replace('\\', '/');
                if (picked.StartsWith(dataPath))
                    _scanFolder = "Assets" + picked.Substring(dataPath.Length);
            }
        }

        GUILayout.Space(10);

        if (_graph != null && _graph.Nodes.Count > 0)
            GUILayout.Label($"{_graph.Nodes.Count} scripts, {_edgeCount} dependencies");

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Center", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            CenterGraph();
            Repaint();
        }

        if (GUILayout.Button($"{Mathf.RoundToInt(_zoom * 100)}%", EditorStyles.toolbarButton, GUILayout.Width(45)))
        {
            _zoom = 1f;
            if (_graph != null && _graph.Nodes.Count > 0)
                CenterGraph();
            Repaint();
        }

        GUILayout.EndHorizontal();
    }

    void DrawEmptyState()
    {
        GUILayout.Space(40);
        EditorGUILayout.HelpBox(
            $"No scripts found under {_scanFolder} (excluding Editor/).\n" +
            "Add scripts and click Scan, or use Browse to pick a different folder.",
            MessageType.Info);
    }

    void DrawNodes()
    {
        BeginWindows();
        for (int i = 0; i < _graph.Nodes.Count; i++)
        {
            var node = _graph.Nodes[i];
            var screenRect = GetScreenRect(node);

            var newRect = GUI.Window(i, screenRect, DrawNodeContent, GUIContent.none, _nodeStyle);

            node.Rect.x = (newRect.x - _panOffset.x) / _zoom;
            node.Rect.y = (newRect.y - _panOffset.y - ToolbarHeight) / _zoom;
        }
        EndWindows();
    }

    void DrawNodeContent(int id)
    {
        var node = _graph.Nodes[id];

        GUILayout.Space(2);
        GUILayout.Label(node.TypeName, _titleStyle);

        int depCount = node.DependsOn.Count;
        _refCounts.TryGetValue(node.TypeName, out int refCount);
        GUILayout.Label($"Deps: {depCount}  Refs: {refCount}", _subtitleStyle);

        var e = Event.current;
        if (e.type == EventType.MouseDown && e.clickCount == 2)
        {
            var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(node.FilePath);
            if (asset != null)
                AssetDatabase.OpenAsset(asset);
            e.Use();
        }

        GUI.DragWindow();
    }

    void DrawEdges()
    {
        if (_graph == null) return;

        Handles.BeginGUI();
        Handles.color = EdgeColor;

        foreach (var node in _graph.Nodes)
        {
            var sourceRect = GetScreenRect(node);

            foreach (var depName in node.DependsOn)
            {
                if (!_graph.NodesByType.TryGetValue(depName, out var target))
                    continue;

                var targetRect = GetScreenRect(target);

                var startPoint = GetNearestEdgePoint(sourceRect, targetRect.center);
                var endRaw = GetNearestEdgePoint(targetRect, sourceRect.center);

                var dir = (endRaw - startPoint).normalized;
                var endPoint = endRaw - dir * ArrowSize;

                Handles.DrawAAPolyLine(EdgeWidth, startPoint, endPoint);
                DrawArrowhead(endRaw, dir);
            }
        }

        Handles.EndGUI();
    }

    void HandleZoom()
    {
        var e = Event.current;
        if (e.type != EventType.ScrollWheel) return;

        float oldZoom = _zoom;
        float zoomDelta = -e.delta.y * 0.05f;
        _zoom = Mathf.Clamp(_zoom + zoomDelta, ZoomMin, ZoomMax);

        if (Mathf.Approximately(oldZoom, _zoom)) return;

        Vector2 mouseScreen = e.mousePosition;
        Vector2 mouseGraph = (mouseScreen - _panOffset - new Vector2(0, ToolbarHeight)) / oldZoom;
        _panOffset = mouseScreen - new Vector2(0, ToolbarHeight) - mouseGraph * _zoom;

        e.Use();
        Repaint();
    }

    void HandlePanning()
    {
        var e = Event.current;

        if (e.type == EventType.MouseDown && (e.button == 2 || (e.button == 0 && e.alt)))
        {
            _isPanning = true;
            _panStartMouse = e.mousePosition;
            _panStartOffset = _panOffset;
            e.Use();
        }

        if (e.type == EventType.MouseDrag && _isPanning)
        {
            _panOffset = _panStartOffset + (e.mousePosition - _panStartMouse);
            e.Use();
            Repaint();
        }

        if (e.type == EventType.MouseUp && _isPanning)
        {
            _isPanning = false;
            e.Use();
        }
    }

    void CenterGraph()
    {
        if (_graph == null || _graph.Nodes.Count == 0) return;

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        foreach (var node in _graph.Nodes)
        {
            if (node.Rect.x < minX) minX = node.Rect.x;
            if (node.Rect.y < minY) minY = node.Rect.y;
            if (node.Rect.xMax > maxX) maxX = node.Rect.xMax;
            if (node.Rect.yMax > maxY) maxY = node.Rect.yMax;
        }

        float graphWidth = (maxX - minX) * _zoom;
        float graphHeight = (maxY - minY) * _zoom;
        float viewWidth = position.width;
        float viewHeight = position.height - ToolbarHeight;

        _panOffset = new Vector2(
            (viewWidth - graphWidth) / 2f - minX * _zoom,
            (viewHeight - graphHeight) / 2f - minY * _zoom);
    }

    Rect GetScreenRect(ScriptNode node)
    {
        return new Rect(
            node.Rect.x * _zoom + _panOffset.x,
            node.Rect.y * _zoom + _panOffset.y + ToolbarHeight,
            node.Rect.width * _zoom,
            node.Rect.height * _zoom);
    }

    static Vector2 GetNearestEdgePoint(Rect rect, Vector2 externalPoint)
    {
        var center = rect.center;
        var dir = externalPoint - center;

        if (dir.sqrMagnitude < 0.001f)
            return center;

        float halfW = rect.width / 2f;
        float halfH = rect.height / 2f;

        float scaleX = dir.x != 0 ? halfW / Mathf.Abs(dir.x) : float.MaxValue;
        float scaleY = dir.y != 0 ? halfH / Mathf.Abs(dir.y) : float.MaxValue;
        float scale = Mathf.Min(scaleX, scaleY);

        return center + dir * scale;
    }

    static void DrawArrowhead(Vector2 tip, Vector2 direction)
    {
        direction.Normalize();
        float rad = ArrowAngle * Mathf.Deg2Rad;

        var right = new Vector2(
            direction.x * Mathf.Cos(rad) - direction.y * Mathf.Sin(rad),
            direction.x * Mathf.Sin(rad) + direction.y * Mathf.Cos(rad));

        var left = new Vector2(
            direction.x * Mathf.Cos(-rad) - direction.y * Mathf.Sin(-rad),
            direction.x * Mathf.Sin(-rad) + direction.y * Mathf.Cos(-rad));

        Handles.DrawAAConvexPolygon(
            (Vector3)tip,
            (Vector3)(tip - right * ArrowSize),
            (Vector3)(tip - left * ArrowSize));
    }
}
