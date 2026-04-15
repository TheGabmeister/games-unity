using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using SMW.Audio;
using SMW.Core;
using SMW.Feedback;
using SMW.Save;
using SMW.Scene;
using SMW.Score;
using SMW.Session;
using SMW.State;
using SMW.UI;
using SMW.InputRouting;

namespace SMW.Editor.Setup
{
    public static class SceneBootstrapGenerator
    {
        private const string ScenesFolder = "Assets/_Project/Scenes";
        private const string SettingsFolder = "Assets/_Project/Settings";
        private const string DataFolder = "Assets/_Project/Data";
        private const string InputAssetPath = "Assets/_Project/Input/InputSystem_Actions.inputactions";

        [MenuItem("Tools/SMW/Setup/Bootstrap Phase 0 Scenes")]
        public static void Bootstrap()
        {
            EnsureFolders();

            var editorTestSettings = EnsureAsset<SMW.Data.EditorTestSettings>($"{SettingsFolder}/EditorTestSettings.asset");
            var palette = EnsureAsset<SMW.Palette.Palette>($"{DataFolder}/Palette.asset");
            var audioCatalog = EnsureAsset<AudioCatalog>($"{DataFolder}/AudioCatalog.asset");
            _ = editorTestSettings; _ = palette; _ = audioCatalog;

            var bootPath = CreateBootScene();
            var systemsPath = CreateSystemsScene(audioCatalog);
            var titlePath = CreateTitleScene();
            var overworldPath = CreateOverworldScene();

            RegisterBuildSettings(new List<string> { bootPath, systemsPath, titlePath, overworldPath });

            Debug.Log("[SMW Setup] Boot/Systems/Title/Overworld scenes generated and registered.");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/_Project");
            EnsureFolder(ScenesFolder);
            EnsureFolder(SettingsFolder);
            EnsureFolder(DataFolder);
            EnsureFolder("Assets/_Project/Input");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static T EnsureAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static string CreateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("Bootstrapper");
            go.AddComponent<Bootstrapper>();
            var path = $"{ScenesFolder}/Boot.unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        private static string CreateSystemsScene(AudioCatalog catalog)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // GameServices root + services.
            var servicesGo = new GameObject("GameServices");
            var services = servicesGo.AddComponent<GameServices>();
            var saveManager = servicesGo.AddComponent<SaveManager>();
            var sceneLoader = servicesGo.AddComponent<SceneLoader>();
            var gameState = servicesGo.AddComponent<GameStateMachine>();
            var scoreSvc = servicesGo.AddComponent<ScoreService>();
            var feedbackSvc = servicesGo.AddComponent<FeedbackService>();
            var session = servicesGo.AddComponent<GameSession>();
            var inputRouter = servicesGo.AddComponent<InputRouter>();
            _ = inputRouter;

            // Input asset reference.
            var inputAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(InputAssetPath);
            if (inputAsset != null)
            {
                var so = new SerializedObject(inputRouter);
                var prop = so.FindProperty("actions");
                if (prop != null) { prop.objectReferenceValue = inputAsset; so.ApplyModifiedPropertiesWithoutUndo(); }
            }

            // HUDRoot canvas.
            var hudRootGo = new GameObject("HUDRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasScalerPresetApplier));
            var hudCanvas = hudRootGo.GetComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 10;
            MakeChild(hudRootGo, "HudPanel");
            MakeChild(hudRootGo, "PauseMenuPanel").SetActive(false);
            MakeChild(hudRootGo, "GameOverPanel").SetActive(false);

            // TransitionCanvas (fader).
            var transitionGo = new GameObject("TransitionCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var tCanvas = transitionGo.GetComponent<Canvas>();
            tCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            tCanvas.sortingOrder = 1000;
            var fadeImageGo = new GameObject("FadeImage", typeof(RectTransform), typeof(Image));
            fadeImageGo.transform.SetParent(transitionGo.transform, false);
            var fadeRect = fadeImageGo.GetComponent<RectTransform>();
            fadeRect.anchorMin = Vector2.zero; fadeRect.anchorMax = Vector2.one;
            fadeRect.offsetMin = Vector2.zero; fadeRect.offsetMax = Vector2.zero;
            var fadeImage = fadeImageGo.GetComponent<Image>();
            fadeImage.color = new Color(0, 0, 0, 0);
            var fader = transitionGo.AddComponent<ScreenFader>();
            var faderSo = new SerializedObject(fader);
            var fImageProp = faderSo.FindProperty("fadeImage");
            fImageProp.objectReferenceValue = fadeImage;
            faderSo.ApplyModifiedPropertiesWithoutUndo();

            // AudioBus GameObject with channels.
            var audioGo = new GameObject("AudioBus");
            var bus = audioGo.AddComponent<AudioBus>();
            var music = MakeChild(audioGo, "MusicChannel").AddComponent<AudioSource>();
            var sfx = MakeChild(audioGo, "SfxChannel").AddComponent<AudioSource>();
            var jingle = MakeChild(audioGo, "JingleChannel").AddComponent<AudioSource>();
            var ambient = MakeChild(audioGo, "AmbientChannel").AddComponent<AudioSource>();
            var uiSfx = MakeChild(audioGo, "UiSfxChannel").AddComponent<AudioSource>();
            uiSfx.ignoreListenerPause = true;
            foreach (var s in new[] { music, sfx, jingle, ambient, uiSfx })
            {
                s.playOnAwake = false;
                s.spatialBlend = 0f;
            }

            var busSo = new SerializedObject(bus);
            SetObjRef(busSo, "catalog", catalog);
            SetObjRef(busSo, "musicChannel", music);
            SetObjRef(busSo, "sfxChannel", sfx);
            SetObjRef(busSo, "jingleChannel", jingle);
            SetObjRef(busSo, "ambientChannel", ambient);
            SetObjRef(busSo, "uiSfxChannel", uiSfx);
            busSo.ApplyModifiedPropertiesWithoutUndo();

            // EventSystem with Input System UI module.
            var esGo = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            // Wire GameServices references.
            var svcSo = new SerializedObject(services);
            SetObjRef(svcSo, "saveManager", saveManager);
            SetObjRef(svcSo, "sceneLoader", sceneLoader);
            SetObjRef(svcSo, "screenFader", fader);
            SetObjRef(svcSo, "gameState", gameState);
            SetObjRef(svcSo, "scoreService", scoreSvc);
            SetObjRef(svcSo, "feedbackService", feedbackSvc);
            SetObjRef(svcSo, "gameSession", session);
            SetObjRef(svcSo, "audioBus", bus);
            svcSo.ApplyModifiedPropertiesWithoutUndo();

            // Reorder roots for cleanliness.
            servicesGo.transform.SetSiblingIndex(0);
            hudRootGo.transform.SetSiblingIndex(1);
            transitionGo.transform.SetSiblingIndex(2);
            audioGo.transform.SetSiblingIndex(3);
            esGo.transform.SetSiblingIndex(4);

            var path = $"{ScenesFolder}/Systems.unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        private static string CreateTitleScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rootGo = new GameObject("TitleRoot");
            rootGo.AddComponent<TitleRoot>();

            var camGo = new GameObject("Main Camera", typeof(Camera));
            var cam = camGo.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.backgroundColor = Color.black;
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.tag = "MainCamera";

            var canvasGo = new GameObject("TitleCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasScalerPresetApplier));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            MakeChild(canvasGo, "TitlePanel");
            MakeChild(canvasGo, "FileSelectPanel").SetActive(false);

            var path = $"{ScenesFolder}/Title.unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        private static string CreateOverworldScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rootGo = new GameObject("OverworldRoot");
            rootGo.AddComponent<OverworldRoot>();

            var camGo = new GameObject("Main Camera", typeof(Camera));
            var cam = camGo.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.backgroundColor = Color.black;
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.tag = "MainCamera";

            var canvasGo = new GameObject("OverworldCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasScalerPresetApplier));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            MakeChild(canvasGo, "OverworldHudPanel");
            MakeChild(canvasGo, "LevelEntryPopup").SetActive(false);

            var path = $"{ScenesFolder}/Overworld.unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        private static void RegisterBuildSettings(List<string> orderedPaths)
        {
            var existing = new List<EditorBuildSettingsScene>();
            // Add the Phase 0 scenes first so Boot is index 0, preserving any others below.
            foreach (var p in orderedPaths)
                existing.Add(new EditorBuildSettingsScene(p, true));

            // Append any pre-existing scenes not in our ordered list.
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (orderedPaths.Contains(s.path)) continue;
                existing.Add(s);
            }
            EditorBuildSettings.scenes = existing.ToArray();
        }

        private static GameObject MakeChild(GameObject parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            var rt = go.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            return go;
        }

        private static void SetObjRef(SerializedObject so, string propName, Object value)
        {
            var p = so.FindProperty(propName);
            if (p != null) p.objectReferenceValue = value;
        }
    }
}
