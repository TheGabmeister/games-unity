using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
public sealed class GroundPlatform : MonoBehaviour
{
    [SerializeField, Min(1)] private int length = 4;

    public int Length => length;

    private void OnValidate()
    {
        ApplyLength();
    }

    private void Awake()
    {
        ApplyLength();
    }

    private void ApplyLength()
    {
        if (length < 1) length = 1;
        var scale = transform.localScale;
        scale.x = length;
        if (scale.y <= 0f) scale.y = 1f;
        if (scale.z <= 0f) scale.z = 1f;
        transform.localScale = scale;

        if (TryGetComponent<BoxCollider2D>(out var box))
        {
            box.size = new Vector2(1f, 1f);
            box.offset = new Vector2(0.5f, 0.5f);
        }
    }
}
