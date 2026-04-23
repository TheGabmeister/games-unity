using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class GenerateHUDPrefab
{
    private const string PrefabPath = PrefabGeneratorUtils.UIPrefabDir + "/HUD.prefab";

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate HUD")]
    public static void Generate()
    {
        PrefabGeneratorUtils.EnsureFolder(PrefabGeneratorUtils.UIPrefabDir);

        GameObject root = new GameObject("HUD");
        try
        {
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Time — top-right
            GameObject timeGo = new GameObject("TimeText", typeof(RectTransform));
            timeGo.transform.SetParent(root.transform, false);
            TextMeshProUGUI timeText = timeGo.AddComponent<TextMeshProUGUI>();
            timeText.text = "00:00";
            timeText.fontSize = 32;
            timeText.alignment = TextAlignmentOptions.TopRight;
            timeText.color = Color.white;
            RectTransform timeRt = timeGo.GetComponent<RectTransform>();
            timeRt.anchorMin = new Vector2(1f, 1f);
            timeRt.anchorMax = new Vector2(1f, 1f);
            timeRt.pivot = new Vector2(1f, 1f);
            timeRt.anchoredPosition = new Vector2(-20f, -10f);
            timeRt.sizeDelta = new Vector2(200f, 40f);

            // Day — below time
            GameObject dayGo = new GameObject("DayText", typeof(RectTransform));
            dayGo.transform.SetParent(root.transform, false);
            TextMeshProUGUI dayText = dayGo.AddComponent<TextMeshProUGUI>();
            dayText.text = "Day 1";
            dayText.fontSize = 24;
            dayText.alignment = TextAlignmentOptions.TopRight;
            dayText.color = new Color(0.8f, 0.8f, 0.8f);
            RectTransform dayRt = dayGo.GetComponent<RectTransform>();
            dayRt.anchorMin = new Vector2(1f, 1f);
            dayRt.anchorMax = new Vector2(1f, 1f);
            dayRt.pivot = new Vector2(1f, 1f);
            dayRt.anchoredPosition = new Vector2(-20f, -50f);
            dayRt.sizeDelta = new Vector2(200f, 30f);

            // Partner stats — top-left
            GameObject statsGo = new GameObject("StatsText", typeof(RectTransform));
            statsGo.transform.SetParent(root.transform, false);
            TextMeshProUGUI statsText = statsGo.AddComponent<TextMeshProUGUI>();
            statsText.text = "";
            statsText.fontSize = 22;
            statsText.alignment = TextAlignmentOptions.TopLeft;
            statsText.color = Color.white;
            RectTransform statsRt = statsGo.GetComponent<RectTransform>();
            statsRt.anchorMin = new Vector2(0f, 1f);
            statsRt.anchorMax = new Vector2(0f, 1f);
            statsRt.pivot = new Vector2(0f, 1f);
            statsRt.anchoredPosition = new Vector2(20f, -10f);
            statsRt.sizeDelta = new Vector2(400f, 120f);

            HUD hud = root.AddComponent<HUD>();

            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("_timeText").objectReferenceValue = timeText;
            so.FindProperty("_dayText").objectReferenceValue = dayText;
            so.FindProperty("_statsText").objectReferenceValue = statsText;
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
