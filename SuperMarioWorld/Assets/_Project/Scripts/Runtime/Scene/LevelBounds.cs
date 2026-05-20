using UnityEngine;

// Marker component defining a rectangular clamp region for the LevelCamera and
// for kill-floor logic. Uses a BoxCollider2D as the underlying shape (trigger),
// on the "LevelBounds" layer per SPEC §4.19. The collider is only a convenient
// inspector handle — no physics interaction is expected with it (the collision
// matrix disables LevelBounds ↔ everything).
[RequireComponent(typeof(BoxCollider2D))]
public sealed class LevelBounds : MonoBehaviour
{
    [SerializeField] private BoxCollider2D box;

    public Rect Rect
    {
        get
        {
            if (box == null) box = GetComponent<BoxCollider2D>();
            Vector2 center = (Vector2)box.transform.position + box.offset;
            Vector2 size = box.size * box.transform.lossyScale;
            return new Rect(center - size * 0.5f, size);
        }
    }

    public Vector2 Min => Rect.min;
    public Vector2 Max => Rect.max;

    private void Reset()
    {
        box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
    }
}
