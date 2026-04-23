using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class GenerateHUDPrefab
{
    private const string PrefabPath = PrefabGeneratorUtils.PrefabDir + "/HUD.prefab";

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate HUD")]
    public static void Generate()
    {
        PrefabGeneratorUtils.EnsureFolder(PrefabGeneratorUtils.PrefabDir);

        GameObject root = new GameObject("HUD");
        try
        {
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            GameObject timeGo = new GameObject("TimeText", typeof(RectTransform));
            timeGo.transform.SetParent(root.transform, false);
            TextMeshProUGUI timeText = timeGo.AddComponent<TextMeshProUGUI>();
            timeText.text = "00:00";
            timeText.fontSize = 32;
            timeText.fontStyle = FontStyles.Normal;
            timeText.alignment = TextAlignmentOptions.TopRight;
            timeText.color = Color.white;
            RectTransform timeRt = timeGo.GetComponent<RectTransform>();
            timeRt.anchorMin = new Vector2(1f, 1f);
            timeRt.anchorMax = new Vector2(1f, 1f);
            timeRt.pivot = new Vector2(1f, 1f);
            timeRt.anchoredPosition = new Vector2(-20f, -10f);
            timeRt.sizeDelta = new Vector2(200f, 50f);

            HUD hud = root.AddComponent<HUD>();

            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("_timeText").objectReferenceValue = timeText;
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
