using UnityEngine;

// Samples the world around the player each FixedUpdate to detect ground, ceiling,
// and walls. Uses OverlapBox against a configured LayerMask (the "Solid" layer per
// SPEC §4.19) so it's cheap, deterministic, and robust to the player's own collider
// overlapping the ground (BoxCast has ambiguous start-inside-collider behavior).
//
// Callers: PlayerController reads IsGrounded / GroundNormal / GroundAngle /
// CeilingContact / WallLeft / WallRight each physics tick to drive gravity, jump,
// slope, ceiling-cancel, and wall-block logic.
public sealed class GroundProbe : MonoBehaviour
{
    [SerializeField] private BoxCollider2D body;
    [SerializeField] private LayerMask solidMask = ~0;
    [SerializeField] private float probeDistance = 0.1f;
    [SerializeField] private float probeShrink = 0.06f;
    // Tolerance for what counts as "ground" (vs wall) from a ground normal.
    [SerializeField] private float maxSlopeDegrees = 60f;

    public bool IsGrounded { get; private set; }
    public Vector2 GroundNormal { get; private set; } = Vector2.up;
    public float GroundAngleDegrees { get; private set; }
    public bool CeilingContact { get; private set; }
    public bool WallLeft { get; private set; }
    public bool WallRight { get; private set; }

    private void Reset()
    {
        body = GetComponent<BoxCollider2D>();
    }

    private void Awake()
    {
        if (body == null) body = GetComponent<BoxCollider2D>();
        // Never probe into the player's own layer — prevents self-hits on the edges
        // of the probe box overlapping the player collider at sub-pixel tolerances.
        solidMask &= ~(1 << gameObject.layer);
    }

    public void Sample()
    {
        if (body == null)
        {
            IsGrounded = false;
            GroundNormal = Vector2.up;
            GroundAngleDegrees = 0f;
            CeilingContact = false;
            WallLeft = WallRight = false;
            return;
        }

        Vector2 center = (Vector2)body.transform.position + body.offset;
        Vector2 halfSize = body.size * 0.5f;

        // Ground probe: a thin horizontal box just below the player's bottom edge.
        Vector2 groundProbeCenter = center + new Vector2(0f, -halfSize.y - probeDistance * 0.5f);
        Vector2 groundProbeSize = new Vector2(body.size.x - probeShrink, probeDistance);
        var groundHit = Physics2D.OverlapBox(groundProbeCenter, groundProbeSize, 0f, solidMask);
        if (groundHit != null)
        {
            // Infer a normal by casting a short ray from the probe box toward the hit.
            var rayHit = Physics2D.Raycast(
                center + new Vector2(0f, -halfSize.y + 0.01f),
                Vector2.down,
                probeDistance + 0.2f,
                solidMask);
            Vector2 normal = rayHit.collider != null ? rayHit.normal : Vector2.up;
            float angle = Vector2.Angle(normal, Vector2.up);
            IsGrounded = angle <= maxSlopeDegrees;
            GroundNormal = IsGrounded ? normal : Vector2.up;
            GroundAngleDegrees = IsGrounded ? angle : 0f;
        }
        else
        {
            IsGrounded = false;
            GroundNormal = Vector2.up;
            GroundAngleDegrees = 0f;
        }

        // Ceiling probe: thin box just above the player's top edge.
        Vector2 ceilingProbeCenter = center + new Vector2(0f, halfSize.y + probeDistance * 0.5f);
        Vector2 ceilingProbeSize = new Vector2(body.size.x - probeShrink, probeDistance);
        CeilingContact = Physics2D.OverlapBox(ceilingProbeCenter, ceilingProbeSize, 0f, solidMask) != null;

        // Wall probes: thin vertical boxes on each side, covering the middle of the body.
        Vector2 wallProbeSize = new Vector2(probeDistance, body.size.y - probeShrink * 2f);
        Vector2 leftProbeCenter = center + new Vector2(-halfSize.x - probeDistance * 0.5f, 0f);
        Vector2 rightProbeCenter = center + new Vector2(halfSize.x + probeDistance * 0.5f, 0f);
        WallLeft = Physics2D.OverlapBox(leftProbeCenter, wallProbeSize, 0f, solidMask) != null;
        WallRight = Physics2D.OverlapBox(rightProbeCenter, wallProbeSize, 0f, solidMask) != null;
    }
}
