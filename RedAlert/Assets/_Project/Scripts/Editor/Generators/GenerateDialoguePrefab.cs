using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class GenerateDialoguePrefab
{
    private const string PrefabPath = PrefabGeneratorUtils.UIPrefabDir + "/DialogueManager.prefab";

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate DialogueManager")]
    public static void Generate()
    {
        PrefabGeneratorUtils.EnsureFolder(PrefabGeneratorUtils.UIPrefabDir);

        GameObject root = new GameObject("DialogueManager");
        try
        {
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            root.AddComponent<GraphicRaycaster>();

            // Dialogue panel — bottom 25% of screen
            GameObject panel = new GameObject("DialoguePanel", typeof(RectTransform));
            panel.transform.SetParent(root.transform, false);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.8f);
            RectTransform panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0f, 0f);
            panelRt.anchorMax = new Vector2(1f, 0.25f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            panel.SetActive(false);

            // Speaker name — top-left of panel
            GameObject speakerGo = new GameObject("SpeakerText", typeof(RectTransform));
            speakerGo.transform.SetParent(panel.transform, false);
            TextMeshProUGUI speakerText = speakerGo.AddComponent<TextMeshProUGUI>();
            speakerText.text = "";
            speakerText.fontSize = 32;
            speakerText.fontStyle = FontStyles.Bold;
            speakerText.alignment = TextAlignmentOptions.TopLeft;
            speakerText.color = Color.yellow;
            RectTransform speakerRt = speakerGo.GetComponent<RectTransform>();
            speakerRt.anchorMin = new Vector2(0f, 1f);
            speakerRt.anchorMax = new Vector2(0f, 1f);
            speakerRt.pivot = new Vector2(0f, 1f);
            speakerRt.anchoredPosition = new Vector2(40f, -10f);
            speakerRt.sizeDelta = new Vector2(400f, 50f);

            // Body text — fills panel with padding
            GameObject bodyGo = new GameObject("BodyText", typeof(RectTransform));
            bodyGo.transform.SetParent(panel.transform, false);
            TextMeshProUGUI bodyText = bodyGo.AddComponent<TextMeshProUGUI>();
            bodyText.text = "";
            bodyText.fontSize = 28;
            bodyText.fontStyle = FontStyles.Normal;
            bodyText.alignment = TextAlignmentOptions.TopLeft;
            bodyText.color = Color.white;
            RectTransform bodyRt = bodyGo.GetComponent<RectTransform>();
            bodyRt.anchorMin = Vector2.zero;
            bodyRt.anchorMax = Vector2.one;
            bodyRt.offsetMin = new Vector2(40f, 20f);
            bodyRt.offsetMax = new Vector2(-40f, -60f);

            DialogueManager manager = root.AddComponent<DialogueManager>();

            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("_dialoguePanel").objectReferenceValue = panel;
            so.FindProperty("_speakerText").objectReferenceValue = speakerText;
            so.FindProperty("_bodyText").objectReferenceValue = bodyText;
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
