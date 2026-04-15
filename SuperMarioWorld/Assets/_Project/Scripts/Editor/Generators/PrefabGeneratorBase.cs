using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SMW.Editor.Generators
{
    public abstract class PrefabGeneratorBase
    {
        protected abstract string FamilyName { get; }
        protected abstract string OutputFolder { get; }

        public sealed class GenerationEntry
        {
            public string Name;
            public System.Func<GameObject> Build;
        }

        protected abstract IEnumerable<GenerationEntry> Entries();

        public void CreateMissingOnly() => Run(overwrite: false, confirm: false);

        public void RegenerateAll()
        {
            if (!EditorUtility.DisplayDialog(
                    $"Regenerate All {FamilyName}",
                    $"This will OVERWRITE every prefab under {OutputFolder}. Continue?",
                    "Overwrite", "Cancel")) return;
            Run(overwrite: true, confirm: true);
        }

        private void Run(bool overwrite, bool confirm)
        {
            EnsureFolder(OutputFolder);
            int created = 0, overwritten = 0, skipped = 0;
            foreach (var entry in Entries())
            {
                var path = $"{OutputFolder}/{entry.Name}.prefab";
                var exists = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
                if (exists && !overwrite) { skipped++; continue; }

                var built = entry.Build?.Invoke();
                if (built == null) { skipped++; continue; }
                PrefabUtility.SaveAsPrefabAsset(built, path);
                Object.DestroyImmediate(built);
                if (exists) overwritten++; else created++;
            }
            Debug.Log($"[{FamilyName} Generator] Created {created}, overwrote {overwritten}, skipped {skipped}.");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
