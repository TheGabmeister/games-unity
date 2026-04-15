using System.IO;
using UnityEditor;
using UnityEngine;

namespace SMW
{
    // Composite entry point for Phase 1. Order matters:
    //   1. SVG reimport so PPU=16 applies to any already-imported art.
    //   2. Environment prefabs land first (Player generator doesn't depend on them,
    //      but debug/content scenes that drag env prefabs in expect them present).
    //   3. Player prefab lands next.
    //   4. Systems.unity regenerates so PlayerInputManager.playerPrefab is wired.
    //
    // Debug scenes (e.g. MovementTest) are hand-authored — see SPEC §4.26.
    public static class Phase1SetupAll
    {
        [MenuItem("Tools/SMW/Setup/Run Full Phase 1 Setup")]
        public static void RunAll()
        {
            Debug.Log("[Phase1SetupAll] Starting full Phase 1 setup.");

            ReimportArtSvgs();

            new EnvironmentPrefabGenerator().CreateMissingOnly();
            new PlayerPrefabGenerator().CreateMissingOnly();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SceneBootstrapGenerator.Bootstrap();

            AssetDatabase.SaveAssets();

            var pp = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabGenerator.PlayerPrefabPath);
            if (pp == null) Debug.LogWarning("[Phase1SetupAll] Player prefab not found on disk after generation — re-run.");
            else Debug.Log($"[Phase1SetupAll] Player prefab resolves to: {pp.name}. Verify the Input GameObject in Systems.unity shows this as its Player Prefab field.");

            Debug.Log("[Phase1SetupAll] Done. Open your hand-authored MovementTest.unity to playtest.");
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
