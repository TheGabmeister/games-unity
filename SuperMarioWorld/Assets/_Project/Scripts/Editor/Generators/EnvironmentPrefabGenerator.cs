using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SMW
{
    public sealed class EnvironmentPrefabGenerator : PrefabGeneratorBase
    {
        public const string PrefabFolder = "Assets/_Project/Prefabs/Environment";
        public const string SpriteFolder = "Assets/_Project/Art/Sprites/Environment";
        private const int SolidLayer = 13;

        protected override string FamilyName => "Environment Prefabs";
        protected override string OutputFolder => PrefabFolder;

        [MenuItem("Tools/SMW/Generate/Prefabs/Environment — Create Missing Only")]
        public static void CreateMissing() => new EnvironmentPrefabGenerator().CreateMissingOnly();

        [MenuItem("Tools/SMW/Generate/Prefabs/Environment — Regenerate All (Overwrite)")]
        public static void Regenerate() => new EnvironmentPrefabGenerator().RegenerateAll();

        protected override IEnumerable<GenerationEntry> Entries()
        {
            yield return new GenerationEntry { Name = "Ground_Platform", Build = BuildGround };
            yield return new GenerationEntry { Name = SlopeKind.SteepL.AssetName(), Build = () => BuildSlope(SlopeKind.SteepL) };
            yield return new GenerationEntry { Name = SlopeKind.SteepR.AssetName(), Build = () => BuildSlope(SlopeKind.SteepR) };
            yield return new GenerationEntry { Name = SlopeKind.ShallowL.AssetName(), Build = () => BuildSlope(SlopeKind.ShallowL) };
            yield return new GenerationEntry { Name = SlopeKind.ShallowR.AssetName(), Build = () => BuildSlope(SlopeKind.ShallowR) };
        }

        private static GameObject BuildGround()
        {
            var go = new GameObject("Ground_Platform");
            go.layer = SolidLayer;
            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1f, 1f);
            box.offset = new Vector2(0.5f, 0.5f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite($"{SpriteFolder}/Ground_Platform.svg");
            sr.drawMode = SpriteDrawMode.Simple;

            go.AddComponent<GroundPlatform>();
            return go;
        }

        private static GameObject BuildSlope(SlopeKind kind)
        {
            var go = new GameObject(kind.AssetName());
            go.layer = SolidLayer;
            var poly = go.AddComponent<PolygonCollider2D>();
            poly.pathCount = 1; // Slope.ApplyShape sets the actual points on Awake / OnValidate.

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite($"{SpriteFolder}/{kind.AssetName()}.svg");

            var slope = go.AddComponent<Slope>();
            var slopeSo = new SerializedObject(slope);
            slopeSo.FindProperty("kind").enumValueIndex = (int)kind;
            slopeSo.FindProperty("length").intValue = 2;
            slopeSo.ApplyModifiedPropertiesWithoutUndo();
            return go;
        }

        private static Sprite LoadSprite(string path)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null) Debug.LogWarning($"[EnvironmentPrefabGenerator] Sprite not found at {path} — prefab will have no visual until the SVG imports.");
            return s;
        }
    }
}
