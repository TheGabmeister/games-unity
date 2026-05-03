using UnityEngine;

public class FacingRenderer : MonoBehaviour
{
    private SpriteRenderer _sr;
    private Sprite[] _sprites;
    private int _dirIndex = 4; // default south-facing

    private static readonly Vector2Int[] DirVectors =
    {
        new(0, 1),   // 0: N
        new(1, 1),   // 1: NE
        new(1, 0),   // 2: E
        new(1, -1),  // 3: SE
        new(0, -1),  // 4: S
        new(-1, -1), // 5: SW
        new(-1, 0),  // 6: W
        new(-1, 1),  // 7: NW
    };

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        var entity = GetComponent<Entity>();
        if (entity != null && entity.UnitData != null && entity.UnitData.DirectionSprites != null
            && entity.UnitData.DirectionSprites.Length >= 8)
        {
            _sprites = entity.UnitData.DirectionSprites;
            ApplySprite();
        }

        var mover = GetComponent<Mover>();
        if (mover != null)
            mover.OnDirectionChanged += OnDirectionChanged;
    }

    void OnDestroy()
    {
        var mover = GetComponent<Mover>();
        if (mover != null)
            mover.OnDirectionChanged -= OnDirectionChanged;
    }

    void OnDirectionChanged(Vector2Int dir)
    {
        if (_sprites == null) return;

        int best = 0;
        float bestDot = -2f;
        Vector2 d = new Vector2(dir.x, dir.y).normalized;

        for (int i = 0; i < DirVectors.Length; i++)
        {
            float dot = Vector2.Dot(d, new Vector2(DirVectors[i].x, DirVectors[i].y).normalized);
            if (dot > bestDot)
            {
                bestDot = dot;
                best = i;
            }
        }

        if (best != _dirIndex)
        {
            _dirIndex = best;
            ApplySprite();
        }
    }

    void ApplySprite()
    {
        if (_sprites == null || _sr == null) return;
        if (_dirIndex >= 0 && _dirIndex < _sprites.Length)
            _sr.sprite = _sprites[_dirIndex];
    }
}
