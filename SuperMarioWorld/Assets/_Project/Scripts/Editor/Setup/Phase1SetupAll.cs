using System.IO;
using UnityEditor;
using UnityEngine;

namespace SMW
{
    // Composite entry point for Phase 1. Order matters:
    //   1. Environment prefabs land first (MovementTest depends on them).
    //   2. Player prefab lands next (Systems regen wires PlayerInputManager.playerPrefab).
    //   3. Systems.unity regenerates so playerPrefab is populated.
    //   4. MovementTest debug scene regenerates with all refs live.
    public static class Phase1SetupAll
    {
        [MenuItem("Tools/SMW/Setup/Run Full Phase 1 Setup")]
        public static void RunAll()
        {
            Debug.Log("[Phase1SetupAll] Starting full Phase 1 setup.");

            // Force-reimport art SVGs so the SvgImportDefaults postprocessor applies
            // PPU = 16. Without this, existing imports keep the Unity default (100).
            ReimportArtSvgs();

            new EnvironmentPrefabGenerator().CreateMissingOnly();
            new PlayerPrefabGenerator().CreateMissingOnly();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SceneBootstrapGenerator.Bootstrap();

            DebugSceneGenerator.GenerateMovementTest();

            AssetDatabase.SaveAssets();

            // Sanity-check that the Player prefab made it into Systems.unity.
            var pp = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabGenerator.PlayerPrefabPath);
            if (pp == null) Debug.LogWarning("[Phase1SetupAll] Player prefab not found on disk after generation — re-run.");
            else Debug.Log($"[Phase1SetupAll] Player prefab resolves to: {pp.name}. Verify the Input GameObject in Systems.unity shows this as its Player Prefab field.");

            Debug.Log("[Phase1SetupAll] Done. Press Play from Scenes/Debug/MovementTest.unity.");
        }

        private static void ReimportArtSvgs()
        {
            const string artFolder = "Assets/_Project/Art/Sprites";
            if (!Directory.Exists(artFolder)) return;
            int count = 0;
            foreach (var fullPath in Directory.EnumerateFiles(artFolder, "*.svg", SearchOption.AllDirectories))
            {
                var assetPath = fullPath.Replace('\\', '/');
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                count++;
            }
            Debug.Log($"[Phase1SetupAll] Reimported {count} SVG(s) to apply PPU=16.");
        }
    }
}
