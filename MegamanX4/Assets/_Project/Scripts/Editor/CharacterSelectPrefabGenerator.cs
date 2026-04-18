using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class CharacterSelectPrefabGenerator
{
    const string PrefabDir = "Assets/_Project/UI";
    const string PrefabPath = "Assets/_Project/UI/CharacterSelectCanvas.prefab";
    const string InputActionsPath = "Assets/_Project/Input/InputSystem_Actions.inputactions";

    static readonly string[] Characters = { "X", "ZERO" };

    [MenuItem("Tools/MegamanX4/Generate CharacterSelect Prefab")]
    public static void Generate()
    {
        var canvasGO = new GameObject(
            "CharacterSelectCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvasGO.layer = LayerMask.NameToLayer("UI");

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var playerInput = canvasGO.AddComponent<PlayerInput>();
        var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
        if (actions)
            playerInput.actions = actions;
        playerInput.defaultActionMap = "UI";
        playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

        var menuNav = canvasGO.AddComponent<MenuNavigator>();
        var controller = canvasGO.AddComponent<CharacterSelectController>();

        var listGO = new GameObject("CharacterList", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        listGO.transform.SetParent(canvasGO.transform, false);
        var listRT = listGO.GetComponent<RectTransform>();
        listRT.anchorMin = Vector2.zero;
        listRT.anchorMax = Vector2.one;
        listRT.offsetMin = Vector2.zero;
        listRT.offsetMax = Vector2.zero;

        var hlg = listGO.GetComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.spacing = 200f;

        var labels = new TextMeshProUGUI[Characters.Length];
        for (int i = 0; i < Characters.Length; i++)
        {
            var labelGO = new GameObject(Characters[i], typeof(RectTransform));
            labelGO.transform.SetParent(listGO.transform, false);

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = Characters[i];
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 120f;
            tmp.color = Color.white;

            var le = labelGO.AddComponent<LayoutElement>();
            le.preferredWidth = 400f;
            le.preferredHeight = 200f;

            labels[i] = tmp;
        }

        var navSo = new SerializedObject(menuNav);
        navSo.FindProperty("_mode").enumValueIndex = (int)MenuNavigator.NavMode.Horizontal;
        var labelsProp = navSo.FindProperty("_labels");
        labelsProp.arraySize = labels.Length;
        for (int i = 0; i < labels.Length; i++)
            labelsProp.GetArrayElementAtIndex(i).objectReferenceValue = labels[i];
        navSo.ApplyModifiedPropertiesWithoutUndo();

        var ctrlSo = new SerializedObject(controller);
        ctrlSo.FindProperty("_menu").objectReferenceValue = menuNav;
        ctrlSo.FindProperty("_xStageSceneName").stringValue = "SkyLagoon";
        ctrlSo.ApplyModifiedPropertiesWithoutUndo();

        if (!Directory.Exists(PrefabDir))
            Directory.CreateDirectory(PrefabDir);

        PrefabUtility.SaveAsPrefabAsset(canvasGO, PrefabPath);
        Object.DestroyImmediate(canvasGO);

        AssetDatabase.Refresh();
        Debug.Log($"Generated: {PrefabPath}");
    }
}
