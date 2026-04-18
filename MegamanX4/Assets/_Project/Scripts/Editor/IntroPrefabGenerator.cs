using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public static class IntroPrefabGenerator
{
    const string AssetDir = "Assets/_Project/UI";
    const string PrefabPath = "Assets/_Project/UI/IntroCanvas.prefab";
    const string RenderTexturePath = "Assets/_Project/UI/IntroRenderTexture.renderTexture";
    const string InputActionsPath = "Assets/_Project/Input/InputSystem_Actions.inputactions";
    const string VideoClipPath = "Assets/_Project/Videos/IntroVideo.mp4";

    [MenuItem("Tools/MegamanX4/Generate Intro Prefab")]
    public static void Generate()
    {
        if (!Directory.Exists(AssetDir))
            Directory.CreateDirectory(AssetDir);

        var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(RenderTexturePath);
        if (!rt)
        {
            rt = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32)
            {
                name = "IntroRenderTexture"
            };
            AssetDatabase.CreateAsset(rt, RenderTexturePath);
            AssetDatabase.SaveAssets();
        }

        var videoClip = AssetDatabase.LoadAssetAtPath<VideoClip>(VideoClipPath);

        var canvasGO = new GameObject(
            "IntroCanvas",
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

        var videoGO = new GameObject("VideoPlayer");
        videoGO.transform.SetParent(canvasGO.transform, false);
        var videoPlayer = videoGO.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = true;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = rt;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        if (videoClip)
            videoPlayer.clip = videoClip;

        var rawImageGO = new GameObject("VideoRawImage", typeof(RectTransform));
        rawImageGO.transform.SetParent(canvasGO.transform, false);
        var rawRT = rawImageGO.GetComponent<RectTransform>();
        rawRT.anchorMin = Vector2.zero;
        rawRT.anchorMax = Vector2.one;
        rawRT.offsetMin = Vector2.zero;
        rawRT.offsetMax = Vector2.zero;
        var rawImage = rawImageGO.AddComponent<RawImage>();
        rawImage.texture = rt;

        var controller = canvasGO.AddComponent<IntroController>();
        var so = new SerializedObject(controller);
        so.FindProperty("_videoPlayer").objectReferenceValue = videoPlayer;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(canvasGO, PrefabPath);
        Object.DestroyImmediate(canvasGO);

        AssetDatabase.Refresh();
        Debug.Log($"Generated: {PrefabPath} (RenderTexture at {RenderTexturePath})");
    }
}
