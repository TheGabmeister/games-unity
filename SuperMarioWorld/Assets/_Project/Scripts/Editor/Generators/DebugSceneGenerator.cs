using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SMW.Editor.Generators
{
    /// Phase 0 scaffold. Each Phase adds its own generator method and menu item.
    public static class DebugSceneGenerator
    {
        public const string DebugSceneFolder = "Assets/_Project/Scenes/Debug";

        [MenuItem("Tools/SMW/Generate/Debug Scenes/Regenerate All")]
        public static void RegenerateAll()
        {
            EnsureFolder(DebugSceneFolder);
            Debug.Log("[SMW Debug Scenes] Phase 0 scaffold — no scenes defined yet. Phase 1 adds Movement Test, Phase 2 adds All Blocks, etc.");
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
