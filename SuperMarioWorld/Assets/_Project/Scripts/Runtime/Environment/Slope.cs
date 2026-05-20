using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(PolygonCollider2D))]
public sealed class Slope : MonoBehaviour
{
    [SerializeField] private SlopeKind kind = SlopeKind.SteepR;
    [SerializeField, Min(1)] private int length = 2;

    public SlopeKind Kind => kind;
    public int Length => length;

    // Programmatic wiring (editor generators, tests). Runtime content sets these
    // via the inspector; OnValidate / Awake already re-apply the shape.
    public void Configure(SlopeKind newKind, int newLength)
    {
        kind = newKind;
        length = Mathf.Max(1, newLength);
        ApplyShape();
    }

    // Walkable-surface angle in degrees (always positive — direction is encoded in kind).
    // Steep = 45°, Shallow = atan(0.5) ≈ 26.565°.
    public float AngleDegrees => kind is SlopeKind.SteepL or SlopeKind.SteepR
        ? 45f
        : 26.56505f;

    private void OnValidate()
    {
        ApplyShape();
    }

    private void Awake()
    {
        ApplyShape();
    }

    // Canonical shape is authored at the sprite's native footprint: 1x1 for steep,
    // 2x1 for shallow. A uniform scale factor `s` resizes both the collider polygon
    // and the SpriteRenderer together, preserving the angle.
    //   steep length N     → s = N     → triangle (0,0), (N,0), (N,N)
    //   shallow length N   → s = N/2   → triangle (0,0), (N,0), (N,N/2)
    private void ApplyShape()
    {
        if (length < 1) length = 1;
        if (!TryGetComponent<PolygonCollider2D>(out var poly)) return;

        bool steep = kind is SlopeKind.SteepL or SlopeKind.SteepR;
        bool risesLeft = kind is SlopeKind.SteepL or SlopeKind.ShallowL;

        // Canonical dimensions in local units (pre-scale).
        float canonRun = steep ? 1f : 2f;
        float canonRise = 1f;

        Vector2[] points = risesLeft
            ? new[]
            {
                new Vector2(0f, canonRise),
                new Vector2(0f, 0f),
                new Vector2(canonRun, 0f),
            }
            : new[]
            {
                new Vector2(0f, 0f),
                new Vector2(canonRun, 0f),
                new Vector2(canonRun, canonRise),
            };

        poly.pathCount = 1;
        poly.SetPath(0, points);

        float s = steep ? length : length * 0.5f;
        var scale = transform.localScale;
        scale.x = s;
        scale.y = s;
        if (scale.z <= 0f) scale.z = 1f;
        transform.localScale = scale;
    }
}
