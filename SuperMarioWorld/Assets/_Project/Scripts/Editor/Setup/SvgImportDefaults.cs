using Unity.VectorGraphics.Editor;
using UnityEditor;
using UnityEngine;

namespace SMW
{
    // Enforces placeholder-SVG import defaults so 1 SVG unit maps to 1 game pixel
    // and a 16-unit viewBox therefore maps to exactly one 1-world-unit tile.
    //
    // Applies to anything under Assets/_Project/Art/Sprites/. Project SVGs that live
    // elsewhere (package samples, shader-graph icons, etc.) are left alone.
    public sealed class SvgImportDefaults : AssetPostprocessor
    {
        private const string ProjectArtRoot = "Assets/_Project/Art/Sprites";
        public const float PixelsPerUnit = 16f;

        private void OnPreprocessAsset()
        {
            if (assetImporter is not SVGImporter svg) return;
            if (!assetPath.StartsWith(ProjectArtRoot)) return;
            if (Mathf.Approximately(svg.SvgPixelsPerUnit, PixelsPerUnit)) return;

            svg.SvgPixelsPerUnit = PixelsPerUnit;
        }
    }
}
