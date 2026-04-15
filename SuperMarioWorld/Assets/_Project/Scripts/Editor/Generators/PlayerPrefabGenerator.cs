using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SMW
{
    public sealed class PlayerPrefabGenerator : PrefabGeneratorBase
    {
        public const string PrefabFolder = "Assets/_Project/Prefabs/Player";
        public const string PlayerPrefabPath = PrefabFolder + "/Player.prefab";
        public const string SpritePath = "Assets/_Project/Art/Sprites/Player/Player.svg";
        public const string InputActionsPath = "Assets/_Project/Input/InputSystem_Actions.inputactions";
        private const int PlayerLayer = 8;

        protected override string FamilyName => "Player Prefab";
        protected override string OutputFolder => PrefabFolder;

        [MenuItem("Tools/SMW/Generate/Prefabs/Player — Create Missing Only")]
        public static void CreateMissing() => new PlayerPrefabGenerator().CreateMissingOnly();

        [MenuItem("Tools/SMW/Generate/Prefabs/Player — Regenerate All (Overwrite)")]
        public static void Regenerate() => new PlayerPrefabGenerator().RegenerateAll();

        protected override IEnumerable<GenerationEntry> Entries()
        {
            yield return new GenerationEntry { Name = "Player", Build = BuildPlayer };
        }

        private static GameObject BuildPlayer()
        {
            var go = new GameObject("Player");
            go.layer = PlayerLayer;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.9f, 1.9f);
            box.offset = new Vector2(0f, 0.95f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            sr.sortingOrder = 5;

            go.AddComponent<GroundProbe>();
            go.AddComponent<PlayerCarry>();

            var playerInput = go.AddComponent<PlayerInput>();
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (actions != null)
            {
                var piSo = new SerializedObject(playerInput);
                piSo.FindProperty("m_Actions").objectReferenceValue = actions;
                piSo.FindProperty("m_DefaultActionMap").stringValue = InputMapNames.Player;
                piSo.FindProperty("m_NotificationBehavior").enumValueIndex = (int)PlayerNotifications.InvokeCSharpEvents;
                piSo.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning($"[PlayerPrefabGenerator] InputActionAsset missing at {InputActionsPath} — PlayerInput will be unwired.");
            }

            go.AddComponent<PlayerInputBinding>();
            go.AddComponent<PlayerController>();
            return go;
        }

        // Utility: wire the produced prefab into PlayerInputManager.playerPrefab in
        // Systems.unity. Called by SceneBootstrapGenerator during Phase 1+.
        public static GameObject LoadPrefab()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        }

        public static bool PrefabExists()
        {
            return File.Exists(PlayerPrefabPath);
        }
    }
}
