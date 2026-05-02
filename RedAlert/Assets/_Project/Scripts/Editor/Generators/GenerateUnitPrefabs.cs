using UnityEditor;
using UnityEngine;

public static class GenerateUnitPrefabs
{
    public static void Generate()
    {
        string dir = "Assets/_Project/Prefabs/Units";
        string dataDir = "Assets/_Project/Data/Units";
        PrefabGeneratorUtils.EnsureFolder(dir);

        var circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/SelectionCircle.png");
        var barBGSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/HealthBarBG.png");
        var barFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/HealthBarFill.png");

        string[] unitNames = { "RifleInfantry", "RocketSoldier", "LightTank", "Ranger", "HeavyTank", "Artillery" };

        foreach (var unitName in unitNames)
        {
            var unitData = AssetDatabase.LoadAssetAtPath<UnitData>($"{dataDir}/{unitName}.asset");
            if (unitData == null)
            {
                Debug.LogError($"UnitData not found: {unitName}. Run Generate Combat Data first.");
                continue;
            }

            string prefabPath = $"{dir}/{unitName}.prefab";
            CreateUnitPrefab(unitName, prefabPath, unitData,
                circleSprite, barBGSprite, barFillSprite);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            unitData.Prefab = prefab;
            EditorUtility.SetDirty(unitData);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Generated {unitNames.Length} unit prefabs");
    }

    static void CreateUnitPrefab(string name, string path, UnitData unitData,
        Sprite circleSprite, Sprite barBGSprite, Sprite barFillSprite)
    {
        PrefabGeneratorUtils.SavePrefab(name, path, root =>
        {
            var sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = unitData.Sprite;
            sr.sortingOrder = 10;

            var entity = root.AddComponent<Entity>();
            root.AddComponent<Mover>();
            root.AddComponent<Attacker>();
            var selectable = root.AddComponent<Selectable>();

            var entitySO = new SerializedObject(entity);
            entitySO.FindProperty("_unitData").objectReferenceValue = unitData;
            entitySO.ApplyModifiedPropertiesWithoutUndo();

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
            fillGO.transform.localPosition = Vector3.zero;
            var fillSR = fillGO.AddComponent<SpriteRenderer>();
            fillSR.sprite = barFillSprite;
            fillSR.color = Color.green;
            fillSR.sortingOrder = 21;

            var selectSO = new SerializedObject(selectable);
            selectSO.FindProperty("_selectionCircle").objectReferenceValue = circleSR;
            selectSO.FindProperty("_healthBar").objectReferenceValue = healthBar;
            selectSO.ApplyModifiedPropertiesWithoutUndo();

            var hbSO = new SerializedObject(healthBar);
            hbSO.FindProperty("_barFill").objectReferenceValue = fillGO.transform;
            hbSO.FindProperty("_fillRenderer").objectReferenceValue = fillSR;
            hbSO.ApplyModifiedPropertiesWithoutUndo();

            healthBarGO.SetActive(false);
        });
    }
}
