using UnityEditor;
using UnityEngine;
using System.IO;

public static class GenerateUnitPrefab
{
    public static void GenerateSprites()
    {
        string unitDir = "Assets/_Project/Sprites/Units";
        string uiDir = "Assets/_Project/Sprites/UI";
        PrefabGeneratorUtils.EnsureFolder(uiDir);

        ImportAsSprite($"{unitDir}/Placeholder.png", 64, new Vector2(0.5f, 0.5f));

        CreateCirclePNG($"{uiDir}/SelectionCircle.png", 64, new Color(0.2f, 1f, 0.2f), 2);
        ImportAsSprite($"{uiDir}/SelectionCircle.png", 64, new Vector2(0.5f, 0.5f));

        CreateSolidPNG($"{uiDir}/HealthBarBG.png", 48, 6, new Color(0.1f, 0.1f, 0.1f, 0.8f));
        ImportAsSprite($"{uiDir}/HealthBarBG.png", 64, new Vector2(0.5f, 0.5f));

        CreateSolidPNG($"{uiDir}/HealthBarFill.png", 48, 6, Color.white);
        ImportAsSprite($"{uiDir}/HealthBarFill.png", 64, new Vector2(0f, 0.5f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated unit + UI sprites");
    }

    public static void GeneratePrefab()
    {
        string dir = "Assets/_Project/Prefabs/Units";
        PrefabGeneratorUtils.EnsureFolder(dir);

        var unitSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/Units/Placeholder.png");
        var circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/SelectionCircle.png");
        var barBGSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/HealthBarBG.png");
        var barFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/HealthBarFill.png");

        PrefabGeneratorUtils.SavePrefab("PlaceholderUnit", $"{dir}/PlaceholderUnit.prefab", root =>
        {
            var sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = unitSprite;
            sr.sortingOrder = 10;

            root.AddComponent<Entity>();
            var selectable = root.AddComponent<Selectable>();

            var circleGO = new GameObject("SelectionCircle");
            circleGO.transform.SetParent(root.transform, false);
            circleGO.transform.localPosition = new Vector3(0f, -0.3f, 0f);
            var circleSR = circleGO.AddComponent<SpriteRenderer>();
            circleSR.sprite = circleSprite;
            circleSR.sortingOrder = 9;
            circleSR.enabled = false;

            var healthBarGO = new GameObject("HealthBar");
            healthBarGO.transform.SetParent(root.transform, false);
            healthBarGO.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            var healthBar = healthBarGO.AddComponent<HealthBar>();

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(healthBarGO.transform, false);
            var bgSR = bgGO.AddComponent<SpriteRenderer>();
            bgSR.sprite = barBGSprite;
            bgSR.sortingOrder = 20;

            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(healthBarGO.transform, false);
            fillGO.transform.localPosition = new Vector3(-0.375f, 0f, 0f);
            var fillSR = fillGO.AddComponent<SpriteRenderer>();
            fillSR.sprite = barFillSprite;
            fillSR.color = Color.green;
            fillSR.sortingOrder = 21;

            var so = new SerializedObject(selectable);
            so.FindProperty("_selectionCircle").objectReferenceValue = circleSR;
            so.FindProperty("_healthBar").objectReferenceValue = healthBar;
            so.ApplyModifiedPropertiesWithoutUndo();

            var hbSO = new SerializedObject(healthBar);
            hbSO.FindProperty("_barFill").objectReferenceValue = fillGO.transform;
            hbSO.FindProperty("_fillRenderer").objectReferenceValue = fillSR;
            hbSO.ApplyModifiedPropertiesWithoutUndo();

            healthBarGO.SetActive(false);
        });
    }

    static void CreateSolidPNG(string path, int width, int height, Color color)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();

        string fullPath = Path.Combine(Application.dataPath, "..", path).Replace("\\", "/");
        string dir = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);
    }

    static void CreateCirclePNG(string path, int size, Color color, int thickness)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        float center = size / 2f;
        float outerRadius = size / 2f - 1;
        float innerRadius = outerRadius - thickness;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
                if (dist <= outerRadius && dist >= innerRadius)
                    pixels[y * size + x] = color;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        string fullPath = Path.Combine(Application.dataPath, "..", path).Replace("\\", "/");
        string dir = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);
    }

    static void ImportAsSprite(string path, int ppu, Vector2 pivot)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = ppu;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot = pivot;
        importer.SetTextureSettings(settings);

        importer.SaveAndReimport();
    }
}
