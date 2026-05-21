using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class GroundProbe : MonoBehaviour
{
    [SerializeField] LayerMask _solidLayers;
    [SerializeField] LayerMask _oneWayLayers;
    [SerializeField] float _skinWidth = 0.02f;

    BoxCollider2D _box;

    public bool IsGrounded { get; private set; }
    public bool HitCeiling { get; private set; }
    public bool HitWallLeft { get; private set; }
    public bool HitWallRight { get; private set; }
    public float GroundAngle { get; private set; }
    public Vector2 GroundNormal { get; private set; }

    LayerMask AllSolid => _solidLayers | _oneWayLayers;

    void Awake()
    {
        _box = GetComponent<BoxCollider2D>();
    }

    public void Sample()
    {
        var bounds = _box.bounds;
        var center = (Vector2)bounds.center;
        var extents = (Vector2)bounds.extents;

        SampleGround(center, extents);
        SampleCeiling(center, extents);
        SampleWalls(center, extents);
    }

    void SampleGround(Vector2 center, Vector2 extents)
    {
        var boxCenter = center + Vector2.down * (extents.y + _skinWidth * 0.5f);
        var boxSize = new Vector2(extents.x * 2f - _skinWidth * 2f, _skinWidth);

        IsGrounded = Physics2D.OverlapBox(boxCenter, boxSize, 0f, AllSolid) != null;

        if (IsGrounded)
        {
            var hit = Physics2D.Raycast(center, Vector2.down, extents.y + _skinWidth * 4f, AllSolid);
            if (hit.collider != null)
            {
                GroundNormal = hit.normal;
                GroundAngle = Vector2.Angle(Vector2.up, hit.normal);
            }
            else
            {
                GroundNormal = Vector2.up;
                GroundAngle = 0f;
            }
        }
        else
        {
            GroundNormal = Vector2.up;
            GroundAngle = 0f;
        }
    }

    void SampleCeiling(Vector2 center, Vector2 extents)
    {
        var boxCenter = center + Vector2.up * (extents.y + _skinWidth * 0.5f);
        var boxSize = new Vector2(extents.x * 2f - _skinWidth * 2f, _skinWidth);

        HitCeiling = Physics2D.OverlapBox(boxCenter, boxSize, 0f, _solidLayers) != null;
    }

    void SampleWalls(Vector2 center, Vector2 extents)
    {
        var verticalShrink = _skinWidth * 2f;
        var boxSize = new Vector2(_skinWidth, extents.y * 2f - verticalShrink);

        var leftCenter = center + Vector2.left * (extents.x + _skinWidth * 0.5f);
        HitWallLeft = Physics2D.OverlapBox(leftCenter, boxSize, 0f, _solidLayers) != null;

        var rightCenter = center + Vector2.right * (extents.x + _skinWidth * 0.5f);
        HitWallRight = Physics2D.OverlapBox(rightCenter, boxSize, 0f, _solidLayers) != null;
    }
}
