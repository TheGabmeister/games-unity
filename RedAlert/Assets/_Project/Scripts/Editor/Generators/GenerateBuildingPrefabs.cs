using UnityEditor;
using UnityEngine;

public static class GenerateBuildingPrefabs
{
    const string UnitDir = "Assets/_Project/Data/Units";
    const string PrefabDir = "Assets/_Project/Prefabs/Buildings";
    const string SpriteDir = "Assets/_Project/Sprites";

    static readonly string[] AllBuildings =
    {
        "ConstructionYard", "PowerPlant", "AdvancedPower", "Barracks",
        "WarFactory", "RadarDome", "AlliedTech", "SovietTech",
        "Helipad", "NavalYard", "SubPen", "Airfield",
        "ServiceDepot", "Kennel", "Pillbox", "CamoPillbox",
        "GunTurret", "AAGun", "FlameTurret", "TeslaCoil",
        "SAMSite", "GapGenerator", "Chronosphere", "IronCurtain",
        "MissileSilo", "Wall", "OreRefinery", "OreSilo"
    };

    public static void Generate()
    {
        PrefabGeneratorUtils.EnsureFolder(PrefabDir);

        var circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/UI/SelectionCircle.png");
        var barBGSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/UI/HealthBarBG.png");
        var barFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/UI/HealthBarFill.png");

        int count = 0;
        foreach (var name in AllBuildings)
        {
            var unitData = AssetDatabase.LoadAssetAtPath<UnitData>($"{UnitDir}/{name}.asset");
            if (unitData == null)
            {
                Debug.LogWarning($"UnitData not found for {name}, skipping prefab.");
                continue;
            }

            string path = $"{PrefabDir}/{name}.prefab";
            bool isRefinery = name == "OreRefinery";
            bool isSilo = name == "OreSilo";

            PrefabGeneratorUtils.SavePrefab(name, path, root =>
            {
                var sr = root.AddComponent<SpriteRenderer>();
                sr.sprite = unitData.Sprite;
                sr.sortingOrder = 5;

                var entity = root.AddComponent<Entity>();

                if (isRefinery) root.AddComponent<Refinery>();
                else if (isSilo) root.AddComponent<Silo>();

                if (unitData.ProducesCategory != UnitCategory.Infantry || name == "Barracks" || name == "Kennel")
                {
                    if (unitData.ProducesCategory != 0)
                        root.AddComponent<PrimaryBuilding>();
                }

                root.AddComponent<BuildingDamage>();
                var selectable = root.AddComponent<Selectable>();

                var entitySO = new SerializedObject(entity);
                entitySO.FindProperty("_unitData").objectReferenceValue = unitData;
                entitySO.ApplyModifiedPropertiesWithoutUndo();

                ConfigureSelectable(root, selectable, circleSprite, barBGSprite, barFillSprite);
            });

            unitData.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            EditorUtility.SetDirty(unitData);
            count++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Generated {count} building prefabs");
    }

    static void ConfigureSelectable(GameObject root, Selectable selectable,
        Sprite circleSprite, Sprite barBGSprite, Sprite barFillSprite)
    {
        var circleGO = new GameObject("SelectionCircle");
        circleGO.transform.SetParent(root.transform, false);
        circleGO.transform.localPosition = new Vector3(0f, -0.3f, 0f);
        var circleSR = circleGO.AddComponent<SpriteRenderer>();
        circleSR.sprite = circleSprite;
        circleSR.sortingOrder = 4;
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

        var fireGO = new GameObject("FireOverlay");
        fireGO.transform.SetParent(root.transform, false);
        var fireSR = fireGO.AddComponent<SpriteRenderer>();
        fireSR.color = new Color(1f, 0.4f, 0f, 0.6f);
        fireSR.sortingOrder = 6;
        fireGO.SetActive(false);

        var bdSO = new SerializedObject(root.GetComponent<BuildingDamage>());
        bdSO.FindProperty("_fireOverlay").objectReferenceValue = fireGO;
        bdSO.ApplyModifiedPropertiesWithoutUndo();

        healthBarGO.SetActive(false);
    }
}
