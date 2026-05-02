using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class GenerateBattleUIPrefab
{
    private const string PrefabPath = PrefabGeneratorUtils.UIPrefabDir + "/BattleUI.prefab";

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate BattleUI")]
    public static void Generate()
    {
        PrefabGeneratorUtils.EnsureFolder(PrefabGeneratorUtils.UIPrefabDir);

        GameObject root = new GameObject("BattleUI");
        try
        {
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 85;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Battle panel — fullscreen semi-transparent
            GameObject panel = new GameObject("BattlePanel", typeof(RectTransform));
            panel.transform.SetParent(root.transform, false);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.8f);
            StretchFull(panel);
            panel.SetActive(false);

            // --- Partner info (top-left) ---
            TextMeshProUGUI partnerNameText = CreateText(panel, "PartnerName", "Partner",
                new Vector2(0f, 0.85f), new Vector2(0.25f, 0.95f), 28, FontStyles.Bold, Color.yellow, TextAlignmentOptions.TopLeft);
            TextMeshProUGUI partnerHPText = CreateText(panel, "PartnerHP", "HP 1000 / 1000",
                new Vector2(0f, 0.78f), new Vector2(0.25f, 0.85f), 24, FontStyles.Normal, Color.white, TextAlignmentOptions.TopLeft);
            TextMeshProUGUI partnerMPText = CreateText(panel, "PartnerMP", "MP 500 / 500",
                new Vector2(0f, 0.71f), new Vector2(0.25f, 0.78f), 24, FontStyles.Normal, new Color(0.5f, 0.8f, 1f), TextAlignmentOptions.TopLeft);
            TextMeshProUGUI partnerStatusText = CreateText(panel, "PartnerStatus", "",
                new Vector2(0f, 0.64f), new Vector2(0.25f, 0.71f), 22, FontStyles.Italic, new Color(1f, 0.5f, 0.5f), TextAlignmentOptions.TopLeft);

            // --- Enemy info (top-right) ---
            TextMeshProUGUI enemyNameText = CreateText(panel, "EnemyName", "Enemy",
                new Vector2(0.75f, 0.85f), new Vector2(1f, 0.95f), 28, FontStyles.Bold, new Color(1f, 0.4f, 0.4f), TextAlignmentOptions.TopRight);
            TextMeshProUGUI enemyHPText = CreateText(panel, "EnemyHP", "HP 800 / 800",
                new Vector2(0.75f, 0.78f), new Vector2(1f, 0.85f), 24, FontStyles.Normal, Color.white, TextAlignmentOptions.TopRight);
            TextMeshProUGUI enemyMPText = CreateText(panel, "EnemyMP", "MP 400 / 400",
                new Vector2(0.75f, 0.71f), new Vector2(1f, 0.78f), 24, FontStyles.Normal, new Color(0.5f, 0.8f, 1f), TextAlignmentOptions.TopRight);
            TextMeshProUGUI enemyStatusText = CreateText(panel, "EnemyStatus", "",
                new Vector2(0.75f, 0.64f), new Vector2(1f, 0.71f), 22, FontStyles.Italic, new Color(1f, 0.5f, 0.5f), TextAlignmentOptions.TopRight);

            // --- Command panel (center-bottom) ---
            GameObject commandPanel = CreateSubPanel(panel, "CommandPanel",
                new Vector2(0.35f, 0.15f), new Vector2(0.65f, 0.55f), new Color(0.1f, 0.1f, 0.2f, 0.9f));
            TextMeshProUGUI commandListText = CreateText(commandPanel, "CommandList", "> Attack\n  Technique\n  Item\n  Flee\n  Auto",
                new Vector2(0f, 0f), new Vector2(1f, 1f), 26, FontStyles.Normal, Color.white, TextAlignmentOptions.TopLeft, 20f);

            // --- Technique panel (left) ---
            GameObject techniquePanel = CreateSubPanel(panel, "TechniquePanel",
                new Vector2(0.05f, 0.15f), new Vector2(0.45f, 0.55f), new Color(0.05f, 0.1f, 0.2f, 0.9f));
            TextMeshProUGUI techniqueListText = CreateText(techniquePanel, "TechniqueList", "",
                new Vector2(0f, 0f), new Vector2(1f, 1f), 24, FontStyles.Normal, Color.white, TextAlignmentOptions.TopLeft, 15f);
            techniquePanel.SetActive(false);

            // --- Item panel (right) ---
            GameObject itemPanel = CreateSubPanel(panel, "ItemPanel",
                new Vector2(0.55f, 0.15f), new Vector2(0.95f, 0.55f), new Color(0.1f, 0.05f, 0.15f, 0.9f));
            TextMeshProUGUI itemListText = CreateText(itemPanel, "ItemList", "",
                new Vector2(0f, 0f), new Vector2(1f, 1f), 24, FontStyles.Normal, Color.white, TextAlignmentOptions.TopLeft, 15f);
            itemPanel.SetActive(false);

            // --- Battle log (bottom strip) ---
            TextMeshProUGUI battleLogText = CreateText(panel, "BattleLog", "",
                new Vector2(0.02f, 0.0f), new Vector2(0.7f, 0.14f), 20, FontStyles.Normal, new Color(0.8f, 0.8f, 0.8f), TextAlignmentOptions.BottomLeft, 10f);

            // --- Instructions (bottom-right) ---
            TextMeshProUGUI instructionsText = CreateText(panel, "Instructions", "W/S: Navigate  E: Confirm",
                new Vector2(0.7f, 0.0f), new Vector2(1f, 0.08f), 18, FontStyles.Normal, new Color(0.6f, 0.6f, 0.6f), TextAlignmentOptions.BottomRight);

            // --- Wire component ---
            BattleUI ui = root.AddComponent<BattleUI>();

            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("_battlePanel").objectReferenceValue = panel;
            so.FindProperty("_partnerNameText").objectReferenceValue = partnerNameText;
            so.FindProperty("_partnerHPText").objectReferenceValue = partnerHPText;
            so.FindProperty("_partnerMPText").objectReferenceValue = partnerMPText;
            so.FindProperty("_partnerStatusText").objectReferenceValue = partnerStatusText;
            so.FindProperty("_enemyNameText").objectReferenceValue = enemyNameText;
            so.FindProperty("_enemyHPText").objectReferenceValue = enemyHPText;
            so.FindProperty("_enemyMPText").objectReferenceValue = enemyMPText;
            so.FindProperty("_enemyStatusText").objectReferenceValue = enemyStatusText;
            so.FindProperty("_commandPanel").objectReferenceValue = commandPanel;
            so.FindProperty("_commandListText").objectReferenceValue = commandListText;
            so.FindProperty("_techniquePanel").objectReferenceValue = techniquePanel;
            so.FindProperty("_techniqueListText").objectReferenceValue = techniqueListText;
            so.FindProperty("_itemPanel").objectReferenceValue = itemPanel;
            so.FindProperty("_itemListText").objectReferenceValue = itemListText;
            so.FindProperty("_battleLogText").objectReferenceValue = battleLogText;
            so.FindProperty("_instructionsText").objectReferenceValue = instructionsText;
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

    private static void StretchFull(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static GameObject CreateSubPanel(GameObject parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Color bgColor)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent.transform, false);
        Image img = panel.AddComponent<Image>();
        img.color = bgColor;
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return panel;
    }

    private static TextMeshProUGUI CreateText(GameObject parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, int fontSize, FontStyles fontStyle,
        Color color, TextAlignmentOptions alignment, float padding = 0f)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = fontStyle;
        tmp.color = color;
        tmp.alignment = alignment;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(padding, padding);
        rt.offsetMax = new Vector2(-padding, -padding);
        return tmp;
    }
}
