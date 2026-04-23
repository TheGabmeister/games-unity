using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class GeneratePauseScreenPrefab
{
    private const string PrefabPath = PrefabGeneratorUtils.PrefabDir + "/PauseScreen.prefab";

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate PauseScreen")]
    public static void Generate()
    {
        PrefabGeneratorUtils.EnsureFolder(PrefabGeneratorUtils.PrefabDir);

        GameObject root = new GameObject("PauseScreen");
        try
        {
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Semi-transparent fullscreen overlay
            GameObject panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(root.transform, false);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.7f);
            RectTransform panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            panel.SetActive(false);

            // "PAUSED" title
            GameObject titleGo = new GameObject("PausedText", typeof(RectTransform));
            titleGo.transform.SetParent(panel.transform, false);
            TextMeshProUGUI titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "PAUSED";
            titleText.fontSize = 72;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            RectTransform titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.5f);
            titleRt.anchorMax = new Vector2(0.5f, 0.5f);
            titleRt.pivot = new Vector2(0.5f, 0.5f);
            titleRt.anchoredPosition = new Vector2(0f, 40f);
            titleRt.sizeDelta = new Vector2(600f, 100f);

            // "Press ESC to resume" hint
            GameObject hintGo = new GameObject("HintText", typeof(RectTransform));
            hintGo.transform.SetParent(panel.transform, false);
            TextMeshProUGUI hintText = hintGo.AddComponent<TextMeshProUGUI>();
            hintText.text = "Press ESC to resume";
            hintText.fontSize = 28;
            hintText.alignment = TextAlignmentOptions.Center;
            hintText.color = new Color(0.7f, 0.7f, 0.7f);
            RectTransform hintRt = hintGo.GetComponent<RectTransform>();
            hintRt.anchorMin = new Vector2(0.5f, 0.5f);
            hintRt.anchorMax = new Vector2(0.5f, 0.5f);
            hintRt.pivot = new Vector2(0.5f, 0.5f);
            hintRt.anchoredPosition = new Vector2(0f, -40f);
            hintRt.sizeDelta = new Vector2(600f, 50f);

            PauseScreen screen = root.AddComponent<PauseScreen>();

            SerializedObject so = new SerializedObject(screen);
            so.FindProperty("_panel").objectReferenceValue = panel;
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
