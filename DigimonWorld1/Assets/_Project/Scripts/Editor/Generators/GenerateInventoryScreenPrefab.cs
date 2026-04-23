using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class GenerateInventoryScreenPrefab
{
    private const string PrefabPath = PrefabGeneratorUtils.PrefabDir + "/InventoryScreen.prefab";

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate InventoryScreen")]
    public static void Generate()
    {
        PrefabGeneratorUtils.EnsureFolder(PrefabGeneratorUtils.PrefabDir);

        GameObject root = new GameObject("InventoryScreen");
        try
        {
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Panel — center screen
            GameObject panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(root.transform, false);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.85f);
            RectTransform panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.2f, 0.1f);
            panelRt.anchorMax = new Vector2(0.8f, 0.9f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            panel.SetActive(false);

            // Title
            GameObject titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(panel.transform, false);
            TextMeshProUGUI titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "INVENTORY";
            titleText.fontSize = 36;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Top;
            titleText.color = Color.yellow;
            RectTransform titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -10f);
            titleRt.sizeDelta = new Vector2(0f, 50f);

            // Bits text — below title, right-aligned
            GameObject bitsGo = new GameObject("BitsText", typeof(RectTransform));
            bitsGo.transform.SetParent(panel.transform, false);
            TextMeshProUGUI bitsText = bitsGo.AddComponent<TextMeshProUGUI>();
            bitsText.text = "Bits: 0";
            bitsText.fontSize = 26;
            bitsText.alignment = TextAlignmentOptions.TopRight;
            bitsText.color = new Color(1f, 0.85f, 0.2f);
            RectTransform bitsRt = bitsGo.GetComponent<RectTransform>();
            bitsRt.anchorMin = new Vector2(0f, 1f);
            bitsRt.anchorMax = new Vector2(1f, 1f);
            bitsRt.pivot = new Vector2(0.5f, 1f);
            bitsRt.anchoredPosition = new Vector2(-20f, -60f);
            bitsRt.sizeDelta = new Vector2(-40f, 35f);

            // Item list — main area
            GameObject listGo = new GameObject("ItemListText", typeof(RectTransform));
            listGo.transform.SetParent(panel.transform, false);
            TextMeshProUGUI listText = listGo.AddComponent<TextMeshProUGUI>();
            listText.text = "(empty)";
            listText.fontSize = 24;
            listText.alignment = TextAlignmentOptions.TopLeft;
            listText.color = Color.white;
            RectTransform listRt = listGo.GetComponent<RectTransform>();
            listRt.anchorMin = new Vector2(0f, 0.15f);
            listRt.anchorMax = new Vector2(1f, 0.88f);
            listRt.offsetMin = new Vector2(30f, 0f);
            listRt.offsetMax = new Vector2(-30f, 0f);

            // Instructions — bottom of panel
            GameObject instrGo = new GameObject("InstructionsText", typeof(RectTransform));
            instrGo.transform.SetParent(panel.transform, false);
            TextMeshProUGUI instrText = instrGo.AddComponent<TextMeshProUGUI>();
            instrText.text = "Tab/I: Close";
            instrText.fontSize = 20;
            instrText.alignment = TextAlignmentOptions.Bottom;
            instrText.color = new Color(0.7f, 0.7f, 0.7f);
            RectTransform instrRt = instrGo.GetComponent<RectTransform>();
            instrRt.anchorMin = new Vector2(0f, 0f);
            instrRt.anchorMax = new Vector2(1f, 0.15f);
            instrRt.offsetMin = new Vector2(20f, 10f);
            instrRt.offsetMax = new Vector2(-20f, 0f);

            InventoryScreen screen = root.AddComponent<InventoryScreen>();

            SerializedObject so = new SerializedObject(screen);
            so.FindProperty("_panel").objectReferenceValue = panel;
            so.FindProperty("_itemListText").objectReferenceValue = listText;
            so.FindProperty("_bitsText").objectReferenceValue = bitsText;
            so.FindProperty("_instructionsText").objectReferenceValue = instrText;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out bool success);
            if (!success)
            {
                Debug.LogError($"Failed to save prefab at {PrefabPath}");
                return;
            }
        }
        finally
        {
            Object.DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Prefab generated at {PrefabPath}");
    }
}
