using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace SMW
{
    public sealed class LevelDataValidatorTest
    {
        private const string TempScenePath = "Assets/_LevelValidatorTemp.unity";

        [TearDown]
        public void Cleanup()
        {
            if (!string.IsNullOrEmpty(TempScenePath) && File.Exists(TempScenePath))
                AssetDatabase.DeleteAsset(TempScenePath);
        }

        [Test]
        public void Unregistered_SceneRef_Produces_Validation_Warning()
        {
            // 1. Create a temp scene asset on disk. Do NOT add it to Build Settings.
            // Save the current active scene's state first if dirty, then create a single-mode
            // empty scene, save it, and restore. Single-mode avoids the "untitled unsaved" error
            // that blocks Additive-mode NewScene in batchmode.
            var prevScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var prevPath = prevScene.path;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, TempScenePath);
            AssetDatabase.ImportAsset(TempScenePath);

            // Restore any prior scene so the test doesn't leave the editor in a surprising state.
            if (!string.IsNullOrEmpty(prevPath))
                EditorSceneManager.OpenScene(prevPath, OpenSceneMode.Single);

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(TempScenePath);
            Assert.IsNotNull(sceneAsset, $"Setup failure: couldn't load temp scene at {TempScenePath}");
            var guid = AssetDatabase.AssetPathToGUID(TempScenePath);
            Assert.IsFalse(string.IsNullOrEmpty(guid), "Setup failure: temp scene has no GUID");

            foreach (var s in EditorBuildSettings.scenes)
                Assert.AreNotEqual(TempScenePath, s.path,
                    "Setup failure: temp scene must NOT be in Build Settings for this test.");

            // 2. Build a LevelData pointing at the unregistered scene.
            var data = ScriptableObject.CreateInstance<LevelData>();
            try
            {
                var so = new SerializedObject(data);
                so.FindProperty("levelId").stringValue = "TempValidator";

                var sceneRefProp = so.FindProperty("sceneRef");
                Assert.IsNotNull(sceneRefProp, "Couldn't find sceneRef property on LevelData");
                sceneRefProp.FindPropertyRelative("asset").objectReferenceValue = sceneAsset;
                sceneRefProp.FindPropertyRelative("guid").stringValue = guid;
                so.ApplyModifiedPropertiesWithoutUndo();

                // 3. Helper returns the warning string directly — this is what OnValidate calls.
                var warning = data.GetSceneRefValidationWarning();
                Assert.IsNotNull(warning, "Expected a validation warning when sceneRef targets an unregistered scene.");
                StringAssert.Contains("not registered in Build Settings", warning);
                StringAssert.Contains("_LevelValidatorTemp", warning);

                // 4. And the OnValidate path itself logs it. Use LogAssert to capture the actual log call.
                LogAssert.Expect(LogType.Warning, new Regex(".*not registered in Build Settings.*"));
                var onValidate = typeof(LevelData).GetMethod("OnValidate",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(onValidate, "LevelData.OnValidate not found via reflection");
                onValidate.Invoke(data, null);
            }
            finally
            {
                Object.DestroyImmediate(data);
            }
        }

        [Test]
        public void Null_SceneRef_Produces_No_Warning()
        {
            var data = ScriptableObject.CreateInstance<LevelData>();
            try
            {
                // Fresh LevelData has sceneRef with zero-GUID — helper should return null, no noise.
                Assert.IsNull(data.GetSceneRefValidationWarning(),
                    "Unassigned sceneRef should not produce a warning.");
            }
            finally
            {
                Object.DestroyImmediate(data);
            }
        }
    }
}
