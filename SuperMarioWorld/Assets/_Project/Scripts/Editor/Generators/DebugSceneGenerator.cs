using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SMW
{
    // Debug scene generator. Each phase adds its own scene method + menu item
    // alongside "Regenerate All". Content levels are hand-authored; debug scenes
    // are generator-owned (SPEC §4.26) — hand-edits are lost on next regeneration
    // by design.
    public static class DebugSceneGenerator
    {
        public const string DebugSceneFolder = "Assets/_Project/Scenes/Debug";
        public const string DebugLevelDataFolder = "Assets/_Project/Data/Levels";
        public const string DebugLevelDataPath = DebugLevelDataFolder + "/LevelData_Debug.asset";

        [MenuItem("Tools/SMW/Generate/Debug Scenes/Phase 1 Movement Test")]
        public static void GenerateMovementTest()
        {
            EnsureFolder(DebugSceneFolder);
            // Ensure the asset exists + persists before we touch the scene.
            GetOrCreateDebugLevelData();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var scenePath = $"{DebugSceneFolder}/MovementTest.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Re-load by path AFTER NewScene(Single) so we hold a fresh persistent
            // reference. ScriptableObjects created moments ago can lose their stable
            // instance-ID across a scene reset, producing a null fileID on scene save.
            var data = AssetDatabase.LoadAssetAtPath<LevelData>(DebugLevelDataPath);
            if (data == null)
            {
                Debug.LogError($"[DebugSceneGenerator] LevelData_Debug not found at {DebugLevelDataPath} after create+save. Scene generation aborted.");
                return;
            }

            // Camera.
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(LevelCamera));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.backgroundColor = new Color(0.45f, 0.7f, 0.95f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(12f, 5f, -10f);

            // LevelBounds.
            var boundsGo = new GameObject("LevelBounds", typeof(BoxCollider2D), typeof(LevelBounds));
            boundsGo.layer = 18;
            boundsGo.transform.position = new Vector3(25f, 6f, 0f);
            var boundsBox = boundsGo.GetComponent<BoxCollider2D>();
            boundsBox.isTrigger = true;
            boundsBox.size = new Vector2(56f, 16f);

            // LevelRoot + LevelContext + LevelRunState.
            var rootGo = new GameObject("LevelRoot", typeof(LevelRoot), typeof(LevelContext), typeof(LevelRunState));
            var lcam = camGo.GetComponent<LevelCamera>();
            var lbounds = boundsGo.GetComponent<LevelBounds>();
            var lrun = rootGo.GetComponent<LevelRunState>();

            var ctxSo = new SerializedObject(rootGo.GetComponent<LevelContext>());
            SetObjRef(ctxSo, "levelCamera", lcam);
            SetObjRef(ctxSo, "levelBounds", lbounds);
            SetObjRef(ctxSo, "runState", lrun);
            ctxSo.ApplyModifiedPropertiesWithoutUndo();

            var rootSo = new SerializedObject(rootGo.GetComponent<LevelRoot>());
            SetObjRef(rootSo, "levelData", data);
            rootSo.FindProperty("defaultEntryPoint").stringValue = "default";
            SetObjRef(rootSo, "levelContext", rootGo.GetComponent<LevelContext>());
            rootSo.ApplyModifiedPropertiesWithoutUndo();

            // Spawn marker.
            var spawn = new GameObject("Spawn_Default", typeof(SpawnMarker));
            spawn.transform.position = new Vector3(2f, 2f, 0f);

            // Environment group.
            var env = new GameObject("Environment");

            // Long flat ground.
            SpawnGround(env, new Vector2(0f, 0f), 20);

            // Stepped platforms (for variable-height jump practice).
            SpawnGround(env, new Vector2(6f, 3f), 3);
            SpawnGround(env, new Vector2(11f, 5f), 3);
            SpawnGround(env, new Vector2(16f, 7f), 3);

            // Steep slope section: up 2, plateau, down 2.
            SpawnSlope(env, new Vector2(20f, 0f), SlopeKind.SteepR, 2);
            SpawnGround(env, new Vector2(22f, 1f), 2);
            SpawnSlope(env, new Vector2(24f, 0f), SlopeKind.SteepL, 2);

            // Shallow slope section: up 2 over 4, plateau, down.
            SpawnSlope(env, new Vector2(28f, 0f), SlopeKind.ShallowR, 4);
            SpawnGround(env, new Vector2(32f, 1f), 2);
            SpawnSlope(env, new Vector2(34f, 0f), SlopeKind.ShallowL, 4);

            // Resume flat ground.
            SpawnGround(env, new Vector2(40f, 0f), 10);

            EditorSceneManager.SaveScene(scene, scenePath);
            RegisterInBuildSettings(scenePath);
            Debug.Log($"[SMW Debug Scenes] Movement Test written to {scenePath}.");
        }

        [MenuItem("Tools/SMW/Generate/Debug Scenes/Regenerate All")]
        public static void RegenerateAll()
        {
            EnsureFolder(DebugSceneFolder);
            GenerateMovementTest();
            Debug.Log("[SMW Debug Scenes] Phase 1: Movement Test regenerated. Later phases will add more.");
        }

        private static void SpawnGround(GameObject parent, Vector2 pos, int length)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{EnvironmentPrefabGenerator.PrefabFolder}/Ground_Platform.prefab");
            if (prefab == null)
            {
                Debug.LogError("[DebugSceneGenerator] Ground_Platform prefab missing — run Tools → SMW → Generate → Prefabs → Environment first.");
                return;
            }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
            go.transform.position = pos;
            var gp = go.GetComponent<GroundPlatform>();
            var so = new SerializedObject(gp);
            so.FindProperty("length").intValue = length;
            so.ApplyModifiedPropertiesWithoutUndo();
            // Nudge the prefab to re-apply via OnValidate.
            EditorUtility.SetDirty(gp);
        }

        private static void SpawnSlope(GameObject parent, Vector2 pos, SlopeKind kind, int length)
        {
            var assetName = kind.AssetName();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{EnvironmentPrefabGenerator.PrefabFolder}/{assetName}.prefab");
            if (prefab == null)
            {
                Debug.LogError($"[DebugSceneGenerator] {assetName} prefab missing — run Tools → SMW → Generate → Prefabs → Environment first.");
                return;
            }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
            go.transform.position = pos;
            var slope = go.GetComponent<Slope>();
            var so = new SerializedObject(slope);
            so.FindProperty("kind").enumValueIndex = (int)kind;
            so.FindProperty("length").intValue = length;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(slope);
        }

        private static LevelData GetOrCreateDebugLevelData()
        {
            EnsureFolder(DebugLevelDataFolder);
            var data = AssetDatabase.LoadAssetAtPath<LevelData>(DebugLevelDataPath);
            if (data != null) return data;

            data = ScriptableObject.CreateInstance<LevelData>();
            var so = new SerializedObject(data);
            so.FindProperty("levelId").stringValue = "debug";
            so.FindProperty("displayName").stringValue = "Debug";
            so.FindProperty("timeLimitSeconds").intValue = 999;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(data, DebugLevelDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        private static void RegisterInBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == scenePath)) return;
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void SetObjRef(SerializedObject so, string propName, Object value)
        {
            var prop = so.FindProperty(propName);
            if (prop == null)
            {
                Debug.LogError($"[DebugSceneGenerator] Property '{propName}' not found on {so.targetObject.GetType().Name}.");
                return;
            }
            if (value == null)
            {
                Debug.LogError($"[DebugSceneGenerator] Assigning NULL to {so.targetObject.GetType().Name}.{propName} — upstream lookup failed.");
            }
            prop.objectReferenceValue = value;
        }

        public static UnityEngine.SceneManagement.Scene CreateEmptyDebugScene(string name)
        {
            EnsureFolder(DebugSceneFolder);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var path = $"{DebugSceneFolder}/{name}.unity";
            EditorSceneManager.SaveScene(scene, path);
            return scene;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
