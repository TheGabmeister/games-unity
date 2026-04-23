using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class GenerateStatusScreenPrefab
{
    private const string PrefabPath = PrefabGeneratorUtils.UIPrefabDir + "/StatusScreen.prefab";

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate StatusScreen")]
    public static void Generate()
    {
        PrefabGeneratorUtils.EnsureFolder(PrefabGeneratorUtils.UIPrefabDir);

        GameObject root = new GameObject("StatusScreen");
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

            // Identity — name, stage, attribute at top
            GameObject identityGo = new GameObject("IdentityText", typeof(RectTransform));
            identityGo.transform.SetParent(panel.transform, false);
            TextMeshProUGUI identityText = identityGo.AddComponent<TextMeshProUGUI>();
            identityText.text = "";
            identityText.fontSize = 32;
            identityText.fontStyle = FontStyles.Bold;
            identityText.alignment = TextAlignmentOptions.Top;
            identityText.color = Color.yellow;
            RectTransform identityRt = identityGo.GetComponent<RectTransform>();
            identityRt.anchorMin = new Vector2(0f, 0.85f);
            identityRt.anchorMax = new Vector2(1f, 1f);
            identityRt.offsetMin = new Vector2(30f, 0f);
            identityRt.offsetMax = new Vector2(-30f, -10f);

            // Stats — left column (HP, MP, OFF, DEF, SPD, BRN)
            GameObject statsGo = new GameObject("StatsText", typeof(RectTransform));
            statsGo.transform.SetParent(panel.transform, false);
            TextMeshProUGUI statsText = statsGo.AddComponent<TextMeshProUGUI>();
            statsText.text = "";
            statsText.fontSize = 26;
            statsText.alignment = TextAlignmentOptions.TopLeft;
            statsText.color = Color.white;
            RectTransform statsRt = statsGo.GetComponent<RectTransform>();
            statsRt.anchorMin = new Vector2(0f, 0.3f);
            statsRt.anchorMax = new Vector2(0.5f, 0.85f);
            statsRt.offsetMin = new Vector2(30f, 0f);
            statsRt.offsetMax = new Vector2(0f, 0f);

            // Condition — right column (age, weight, hunger, tiredness, etc.)
            GameObject conditionGo = new GameObject("ConditionText", typeof(RectTransform));
            conditionGo.transform.SetParent(panel.transform, false);
            TextMeshProUGUI conditionText = conditionGo.AddComponent<TextMeshProUGUI>();
            conditionText.text = "";
            conditionText.fontSize = 24;
            conditionText.alignment = TextAlignmentOptions.TopLeft;
            conditionText.color = new Color(0.85f, 0.85f, 0.85f);
            RectTransform conditionRt = conditionGo.GetComponent<RectTransform>();
            conditionRt.anchorMin = new Vector2(0.5f, 0.15f);
            conditionRt.anchorMax = new Vector2(1f, 0.85f);
            conditionRt.offsetMin = new Vector2(10f, 0f);
            conditionRt.offsetMax = new Vector2(-30f, 0f);

            // Instructions — bottom
            GameObject instrGo = new GameObject("InstructionsText", typeof(RectTransform));
            instrGo.transform.SetParent(panel.transform, false);
            TextMeshProUGUI instrText = instrGo.AddComponent<TextMeshProUGUI>();
            instrText.text = "C / ESC: Close";
            instrText.fontSize = 20;
            instrText.alignment = TextAlignmentOptions.Bottom;
            instrText.color = new Color(0.7f, 0.7f, 0.7f);
            RectTransform instrRt = instrGo.GetComponent<RectTransform>();
            instrRt.anchorMin = new Vector2(0f, 0f);
            instrRt.anchorMax = new Vector2(1f, 0.15f);
            instrRt.offsetMin = new Vector2(20f, 10f);
            instrRt.offsetMax = new Vector2(-20f, 0f);

            StatusScreen screen = root.AddComponent<StatusScreen>();

            SerializedObject so = new SerializedObject(screen);
            so.FindProperty("_panel").objectReferenceValue = panel;
            so.FindProperty("_identityText").objectReferenceValue = identityText;
            so.FindProperty("_statsText").objectReferenceValue = statsText;
            so.FindProperty("_conditionText").objectReferenceValue = conditionText;
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
