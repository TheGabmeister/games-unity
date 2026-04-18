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

        var so = new SerializedObject(controller);
        var stagesProp = so.FindProperty("_stages");
        stagesProp.arraySize = Stages.Length;
        for (int i = 0; i < Stages.Length; i++)
        {
            var elem = stagesProp.GetArrayElementAtIndex(i);
            elem.FindPropertyRelative("label").objectReferenceValue = labels[i];
            elem.FindPropertyRelative("sceneName").stringValue = Stages[i].sceneName;
        }
        so.ApplyModifiedPropertiesWithoutUndo();

        if (!Directory.Exists(PrefabDir))
            Directory.CreateDirectory(PrefabDir);

        PrefabUtility.SaveAsPrefabAsset(canvasGO, PrefabPath);
        Object.DestroyImmediate(canvasGO);

        AssetDatabase.Refresh();
        Debug.Log($"Generated: {PrefabPath}");
    }
}
