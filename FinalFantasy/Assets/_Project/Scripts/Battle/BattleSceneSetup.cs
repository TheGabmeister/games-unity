using System.Collections.Generic;
using UnityEngine;

/// Sets up the battle scene: camera, procedural background, enemy sprite rendering,
/// and party/enemy positioning. Placed in the Battle scene.
public class BattleSceneSetup : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] float enemyYCenter = 1.5f;
    [SerializeField] float partyYCenter = -0.5f;
    [SerializeField] float enemySpacing = 1.2f;
    [SerializeField] float partySpacing = 1.4f;
    [SerializeField] float partyXOffset = -1.5f; // shift party left to avoid UI overlap

    Camera battleCamera;
    GameObject backgroundObj;
    List<GameObject> enemyVisuals = new();
    List<GameObject> partyVisuals = new();

    void Awake()
    {
        SetupCamera();
        SetupBackground();
    }

    public void SetupBattle(List<BattleActor> partyActors, List<BattleActor> enemyActors)
    {
        PositionEnemies(enemyActors);
        PositionParty(partyActors);
    }

    void SetupCamera()
    {
        var camObj = new GameObject("BattleCamera");
        camObj.transform.SetParent(transform);
        battleCamera = camObj.AddComponent<Camera>();
        battleCamera.orthographic = true;
        battleCamera.orthographicSize = 4f;
        battleCamera.clearFlags = CameraClearFlags.SolidColor;
        battleCamera.backgroundColor = new Color(0.05f, 0.02f, 0.1f); // dark purple
        battleCamera.depth = 10;
        camObj.transform.position = new Vector3(0, 0, -10);
    }

    void SetupBackground()
    {
        // Procedural background: gradient quad
        backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(transform);
        backgroundObj.transform.position = new Vector3(0, 0, 5);

        var sr = backgroundObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateBackgroundSprite();
        sr.sortingOrder = -100;
        backgroundObj.transform.localScale = new Vector3(20, 12, 1);
    }

    Sprite CreateBackgroundSprite()
    {
        int w = 64, h = 64;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // Gradient from dark blue (bottom) to dark purple (top)
        var bottomColor = new Color(0.02f, 0.02f, 0.08f);
        var topColor = new Color(0.08f, 0.02f, 0.12f);

        for (int y = 0; y < h; y++)
        {
            float t = (float)y / h;
            Color c = Color.Lerp(bottomColor, topColor, t);

            // Add some noise for texture
            for (int x = 0; x < w; x++)
            {
                float noise = ((x * 17 + y * 31) % 7) / 100f;
                tex.SetPixel(x, y, new Color(c.r + noise, c.g + noise * 0.5f, c.b + noise, 1));
            }
        }

        // Draw some stars
        int[] starX = { 5, 15, 28, 42, 55, 10, 38, 50, 22, 60 };
        int[] starY = { 55, 48, 58, 44, 52, 60, 62, 45, 50, 56 };
        for (int i = 0; i < starX.Length; i++)
        {
            if (starX[i] < w && starY[i] < h)
                tex.SetPixel(starX[i], starY[i], new Color(0.8f, 0.8f, 0.9f));
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16);
    }

    void PositionEnemies(List<BattleActor> enemies)
    {
        // Clean up old visuals
        foreach (var v in enemyVisuals) if (v != null) Destroy(v);
        enemyVisuals.Clear();

        int count = enemies.Count;
        if (count == 0) return;

        // Arrange enemies in rows (max ~5 per row)
        int maxPerRow = 5;
        int rows = Mathf.CeilToInt((float)count / maxPerRow);
        int idx = 0;

        for (int row = 0; row < rows; row++)
        {
            int inThisRow = Mathf.Min(maxPerRow, count - idx);
            float totalWidth = (inThisRow - 1) * enemySpacing;
            float startX = -totalWidth / 2f;
            float y = enemyYCenter + (rows > 1 ? (row - (rows - 1) / 2f) * 1.2f : 0);

            for (int col = 0; col < inThisRow; col++)
            {
                if (idx >= count) break;
                var enemy = enemies[idx];
                float x = startX + col * enemySpacing;
                var pos = new Vector3(x, y, 0);

                var visual = CreateEnemyVisual(enemy, pos);
                enemy.Visual = visual;
                enemy.HomePosition = pos;
                enemyVisuals.Add(visual);
                idx++;
            }
        }
    }

    void PositionParty(List<BattleActor> party)
    {
        foreach (var v in partyVisuals) if (v != null) Destroy(v);
        partyVisuals.Clear();

        float totalWidth = (party.Count - 1) * partySpacing;
        float startX = -totalWidth / 2f + partyXOffset;

        for (int i = 0; i < party.Count; i++)
        {
            var actor = party[i];
            float x = startX + i * partySpacing;
            var pos = new Vector3(x, partyYCenter, 0);

            var visual = CreatePartyVisual(actor, pos);
            actor.Visual = visual;
            actor.HomePosition = pos;
            partyVisuals.Add(visual);
        }
    }

    GameObject CreateEnemyVisual(BattleActor enemy, Vector3 position)
    {
        var go = new GameObject($"Enemy_{enemy.Name}");
        go.transform.SetParent(transform);
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;

        var data = enemy.EnemyData;
        float scale = data?.SizeScale ?? 1f;
        go.transform.localScale = Vector3.one * scale;

        // Create procedural enemy sprite based on shape and color
        var shape = data?.Shape ?? EnemyShape.Circle;
        var primary = data?.PrimaryColor ?? Color.red;
        var secondary = data?.SecondaryColor ?? Color.white;
        sr.sprite = CreateEnemySprite(shape, primary, secondary);

        // Add name label above
        AddWorldLabel(go, enemy.Name, new Vector3(0, 0.7f * scale, 0));

        return go;
    }

    GameObject CreatePartyVisual(BattleActor actor, Vector3 position)
    {
        var go = new GameObject($"Party_{actor.Name}");
        go.transform.SetParent(transform);
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;

        // Party members: colored circle based on class color
        Color classColor = Color.white;
        if (actor.PartyMember?.ClassDef != null)
            classColor = actor.PartyMember.ClassDef.ClassColor;

        sr.sprite = CreateCircleSprite(classColor);

        // Name label below
        AddWorldLabel(go, actor.Name, new Vector3(0, -0.7f, 0));

        return go;
    }

    Sprite CreateEnemySprite(EnemyShape shape, Color primary, Color secondary)
    {
        int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // Clear
        var clear = new Color(0, 0, 0, 0);
        var pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        int cx = size / 2, cy = size / 2;

        switch (shape)
        {
            case EnemyShape.Circle:
                DrawFilledCircle(pixels, size, cx, cy, 12, primary);
                DrawFilledCircle(pixels, size, cx, cy - 3, 4, secondary); // face detail
                break;

            case EnemyShape.Square:
                DrawFilledRect(pixels, size, 4, 4, 28, 28, primary);
                DrawFilledRect(pixels, size, 10, 10, 22, 22, secondary); // inner detail
                break;

            case EnemyShape.Triangle:
                DrawFilledTriangle(pixels, size, cx, 28, 4, 4, 28, 4, primary);
                break;

            case EnemyShape.Diamond:
                DrawFilledDiamond(pixels, size, cx, cy, 13, primary);
                DrawFilledDiamond(pixels, size, cx, cy, 5, secondary);
                break;

            case EnemyShape.Hexagon:
                DrawFilledCircle(pixels, size, cx, cy, 14, primary);
                // Add spikes
                for (int angle = 0; angle < 6; angle++)
                {
                    float rad = angle * Mathf.PI / 3f;
                    int sx = cx + Mathf.RoundToInt(Mathf.Cos(rad) * 12);
                    int sy = cy + Mathf.RoundToInt(Mathf.Sin(rad) * 12);
                    if (sx >= 0 && sx < size && sy >= 0 && sy < size)
                        pixels[sy * size + sx] = secondary;
                }
                break;
        }

        // Eyes (two bright pixels)
        SetPixelSafe(pixels, size, cx - 3, cy + 2, Color.white);
        SetPixelSafe(pixels, size, cx + 3, cy + 2, Color.white);

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
    }

    Sprite CreateCircleSprite(Color color)
    {
        int size = 16;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        var pixels = new Color[size * size];
        var clear = new Color(0, 0, 0, 0);
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        DrawFilledCircle(pixels, size, size / 2, size / 2, 6, color);

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
    }

    void AddWorldLabel(GameObject parent, string text, Vector3 offset)
    {
        // World-space canvas for enemy name
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(parent.transform);
        labelObj.transform.localPosition = offset;

        var labelCanvas = labelObj.AddComponent<Canvas>();
        labelCanvas.renderMode = RenderMode.WorldSpace;
        labelCanvas.sortingOrder = 20;

        var rect = labelCanvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 30);
        rect.localScale = Vector3.one * 0.01f;

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(labelObj.transform, false);
        var tmp = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 18;
        tmp.color = Color.white;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        var tRect = tmp.rectTransform;
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.sizeDelta = Vector2.zero;
    }


    // --- Drawing helpers ---

    static void DrawFilledCircle(Color[] pixels, int texSize, int cx, int cy, int radius, Color color)
    {
        for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
                if (x * x + y * y <= radius * radius)
                    SetPixelSafe(pixels, texSize, cx + x, cy + y, color);
    }

    static void DrawFilledRect(Color[] pixels, int texSize, int x1, int y1, int x2, int y2, Color color)
    {
        for (int y = y1; y < y2; y++)
            for (int x = x1; x < x2; x++)
                SetPixelSafe(pixels, texSize, x, y, color);
    }

    static void DrawFilledTriangle(Color[] pixels, int texSize, int x0, int y0, int x1, int y1, int x2, int y2, Color color)
    {
        int minX = Mathf.Min(x0, Mathf.Min(x1, x2));
        int maxX = Mathf.Max(x0, Mathf.Max(x1, x2));
        int minY = Mathf.Min(y0, Mathf.Min(y1, y2));
        int maxY = Mathf.Max(y0, Mathf.Max(y1, y2));

        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                if (PointInTriangle(x, y, x0, y0, x1, y1, x2, y2))
                    SetPixelSafe(pixels, texSize, x, y, color);
    }

    static void DrawFilledDiamond(Color[] pixels, int texSize, int cx, int cy, int radius, Color color)
    {
        for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
                if (Mathf.Abs(x) + Mathf.Abs(y) <= radius)
                    SetPixelSafe(pixels, texSize, cx + x, cy + y, color);
    }

    static void SetPixelSafe(Color[] pixels, int texSize, int x, int y, Color color)
    {
        if (x >= 0 && x < texSize && y >= 0 && y < texSize)
            pixels[y * texSize + x] = color;
    }

    static bool PointInTriangle(int px, int py, int x0, int y0, int x1, int y1, int x2, int y2)
    {
        float d1 = Sign(px, py, x0, y0, x1, y1);
        float d2 = Sign(px, py, x1, y1, x2, y2);
        float d3 = Sign(px, py, x2, y2, x0, y0);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }

    static float Sign(int px, int py, int x1, int y1, int x2, int y2)
    {
        return (px - x2) * (y1 - y2) - (x1 - x2) * (py - y2);
    }
}
