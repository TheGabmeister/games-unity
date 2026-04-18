using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class LevelSelectPrefabGenerator
{
    const string PrefabDir = "Assets/_Project/UI";
    const string PrefabPath = "Assets/_Project/UI/LevelSelectCanvas.prefab";
    const string InputActionsPath = "Assets/_Project/Input/InputSystem_Actions.inputactions";

    static readonly (string sceneName, string display)[] Stages =
    {
        ("AirForce",      "AIR FORCE"),
        ("BioLaboratory", "BIO LABORATORY"),
        ("CyberSpace",    "CYBER SPACE"),
        ("Jungle",        "JUNGLE"),
        ("MarineBase",    "MARINE BASE"),
        ("MilitaryTrain", "MILITARY TRAIN"),
        ("SnowBase",      "SNOW BASE"),
        ("Volcano",       "VOLCANO"),
    };

    [MenuItem("Tools/MegamanX4/Generate LevelSelect Prefab")]
    public static void Generate()
    {
        var canvasGO = new GameObject(
            "LevelSelectCanvas",
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
        var controller = canvasGO.AddComponent<LevelSelectController>();

        var listGO = new GameObject("StageList", typeof(RectTransform), typeof(VerticalLayoutGroup));
        listGO.transform.SetParent(canvasGO.transform, false);
        var listRT = listGO.GetComponent<RectTransform>();
        listRT.anchorMin = Vector2.zero;
        listRT.anchorMax = Vector2.one;
        listRT.offsetMin = Vector2.zero;
        listRT.offsetMax = Vector2.zero;

        var vlg = listGO.GetComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 12f;

        var labels = new TextMeshProUGUI[Stages.Length];
        for (int i = 0; i < Stages.Length; i++)
        {
            var (sceneName, display) = Stages[i];
            var labelGO = new GameObject(sceneName, typeof(RectTransform));
            labelGO.transform.SetParent(listGO.transform, false);

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = display;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 48f;
            tmp.color = Color.white;

            var le = labelGO.AddComponent<LayoutElement>();
            le.preferredHeight = 64f;

            labels[i] = tmp;
        }

        var navSo = new SerializedObject(menuNav);
        var labelsProp = navSo.FindProperty("_labels");
        labelsProp.arraySize = labels.Length;
        for (int i = 0; i < labels.Length; i++)
            labelsProp.GetArrayElementAtIndex(i).objectReferenceValue = labels[i];
        navSo.ApplyModifiedPropertiesWithoutUndo();

        var ctrlSo = new SerializedObject(controller);
        ctrlSo.FindProperty("_menu").objectReferenceValue = menuNav;
        var scenesProp = ctrlSo.FindProperty("_sceneNames");
        scenesProp.arraySize = Stages.Length;
        for (int i = 0; i < Stages.Length; i++)
            scenesProp.GetArrayElementAtIndex(i).stringValue = Stages[i].sceneName;
        ctrlSo.ApplyModifiedPropertiesWithoutUndo();

        if (!Directory.Exists(PrefabDir))
            Directory.CreateDirectory(PrefabDir);

        PrefabUtility.SaveAsPrefabAsset(canvasGO, PrefabPath);
        Object.DestroyImmediate(canvasGO);

        AssetDatabase.Refresh();
        Debug.Log($"Generated: {PrefabPath}");
    }
}
