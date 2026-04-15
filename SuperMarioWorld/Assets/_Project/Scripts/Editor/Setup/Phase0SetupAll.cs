using UnityEditor;
using UnityEngine;

namespace SMW
{
    // Composite entry point: runs every Phase 0 setup step the tests depend on.
    // Invoked from Unity CLI: -executeMethod SMW.Phase0SetupAll.RunAll
    public static class Phase0SetupAll
    {
        [MenuItem("Tools/SMW/Setup/Run Full Phase 0 Setup")]
        public static void RunAll()
        {
            Debug.Log("[Phase0SetupAll] Starting full Phase 0 setup.");
            SceneBootstrapGenerator.Bootstrap();
            PhysicsLayerMatrixSetup.Apply();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Phase0SetupAll] Done.");
        }
    }
}
