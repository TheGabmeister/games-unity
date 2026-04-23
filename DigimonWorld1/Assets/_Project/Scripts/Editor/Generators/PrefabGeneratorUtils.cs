using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class PrefabGeneratorUtils
{
    public const string PrefabDir = "Assets/_Project/Prefabs";

    public static void SavePrefab(string name, string path, System.Action<GameObject> configure)
    {
        EnsureFolder(PrefabDir);

        GameObject source = new GameObject(name);
        try
        {
            configure(source);
            PrefabUtility.SaveAsPrefabAsset(source, path, out bool success);
            if (!success)
            {
                Debug.LogError($"Failed to save prefab at {path}");
                return;
            }
        }
        finally
        {
            Object.DestroyImmediate(source);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Prefab generated at {path}");
    }

    public static GameObject CreateCanvasRoot(string name)
    {
        EnsureFolder(PrefabDir);

        GameObject root = new GameObject(name);
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        root.AddComponent<GraphicRaycaster>();

        GameObject eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.transform.SetParent(root.transform, false);
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<InputSystemUIInputModule>();

        return root;
    }

    public static void SaveAndCleanup(GameObject root, string path)
    {
        try
        {
            PrefabUtility.SaveAsPrefabAsset(root, path, out bool success);
            if (!success)
            {
                Debug.LogError($"Failed to save prefab at {path}");
                return;
            }
        }
        finally
        {
            Object.DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Prefab generated at {path}");
    }

    public static GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return panel;
    }

    public static TMP_Text CreateText(string name, Transform parent, string text,
        int fontSize, FontStyles fontStyle, TextAlignmentOptions alignment,
        Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = fontStyle;
        tmp.alignment = alignment;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        return tmp;
    }

    public static TMP_InputField CreateInputField(string name, Transform parent,
        Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        Image bg = go.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        TMP_InputField inputField = go.AddComponent<TMP_InputField>();

        GameObject textArea = new GameObject("Text Area", typeof(RectTransform));
        textArea.transform.SetParent(go.transform, false);
        RectTransform textAreaRt = textArea.GetComponent<RectTransform>();
        textAreaRt.anchorMin = Vector2.zero;
        textAreaRt.anchorMax = Vector2.one;
        textAreaRt.offsetMin = new Vector2(10f, 0f);
        textAreaRt.offsetMax = new Vector2(-10f, 0f);
        textArea.AddComponent<RectMask2D>();

        GameObject placeholderGo = new GameObject("Placeholder", typeof(RectTransform));
        placeholderGo.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI placeholder = placeholderGo.AddComponent<TextMeshProUGUI>();
        placeholder.text = "Enter name...";
        placeholder.fontSize = 24;
        placeholder.fontStyle = FontStyles.Italic;
        placeholder.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
        RectTransform placeholderRt = placeholderGo.GetComponent<RectTransform>();
        placeholderRt.anchorMin = Vector2.zero;
        placeholderRt.anchorMax = Vector2.one;
        placeholderRt.offsetMin = Vector2.zero;
        placeholderRt.offsetMax = Vector2.zero;

        GameObject textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI inputText = textGo.AddComponent<TextMeshProUGUI>();
        inputText.fontSize = 24;
        inputText.color = Color.white;
        inputText.alignment = TextAlignmentOptions.MidlineLeft;
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        inputField.textViewport = textAreaRt;
        inputField.textComponent = inputText;
        inputField.placeholder = placeholder;
        inputField.characterLimit = 8;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        return inputField;
    }

    public static void SetSceneReference(SerializedObject so, string fieldName, string scenePath)
    {
        string guid = AssetDatabase.AssetPathToGUID(scenePath);
        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogWarning($"Scene not found at {scenePath} — {fieldName} will be empty. Generate scenes first.");
            return;
        }

        SerializedProperty prop = so.FindProperty(fieldName);
        SerializedProperty guidProp = prop.FindPropertyRelative("guid");
        SerializedProperty assetProp = prop.FindPropertyRelative("asset");

        guidProp.stringValue = guid;
        assetProp.objectReferenceValue = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
    }

    public static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
