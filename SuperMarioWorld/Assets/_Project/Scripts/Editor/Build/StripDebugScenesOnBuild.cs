using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SMW
{
    public sealed class StripDebugScenesOnBuild : IPreprocessBuildWithReport
    {
        public const string DebugFolderToken = "/Scenes/Debug/";
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var src = EditorBuildSettings.scenes;
            var kept = new List<EditorBuildSettingsScene>(src.Length);
            int stripped = 0;
            foreach (var s in src)
            {
                if (s.path != null && s.path.Contains(DebugFolderToken)) { stripped++; continue; }
                kept.Add(s);
            }
            if (stripped > 0)
            {
                EditorBuildSettings.scenes = kept.ToArray();
                Debug.Log($"[SMW Build] Stripped {stripped} debug scene(s) from build list.");
            }
        }
    }
}
