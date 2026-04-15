using System;
using System.Collections.Generic;
using Eflatun.SceneReference;
using UnityEngine;

namespace SMW
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "SMW/Level/Level Data")]
    public sealed class LevelData : ScriptableObject
    {
        [Serializable]
        public struct EntryPoint
        {
            public string name;
            public Vector2 position;
        }

        [Serializable]
        public struct SubArea
        {
            public string name;
            public Vector2 regionOffset;
            public Vector2 boundsMin;
            public Vector2 boundsMax;
        }

        [SerializeField] private string levelId;
        [SerializeField] private string displayName;
        [SerializeField] private SceneReference sceneRef;
        [SerializeField] private int timeLimitSeconds = 300;
        [SerializeField] private MusicId musicId = MusicId.Overworld;
        [SerializeField] private List<EntryPoint> entryPoints = new();
        [SerializeField] private List<SubArea> subAreas = new();
        [SerializeField] private List<LevelData> unlocksOnNormalExit = new();
        [SerializeField] private List<LevelData> unlocksOnSecretExit = new();

        public string LevelId => levelId;
        public string DisplayName => displayName;
        public SceneReference SceneRef => sceneRef;
        public int TimeLimitSeconds => timeLimitSeconds;
        public MusicId MusicId => musicId;
        public IReadOnlyList<EntryPoint> EntryPoints => entryPoints;
        public IReadOnlyList<SubArea> SubAreas => subAreas;
        public IReadOnlyList<LevelData> UnlocksOnNormalExit => unlocksOnNormalExit;
        public IReadOnlyList<LevelData> UnlocksOnSecretExit => unlocksOnSecretExit;

        public bool TryGetEntryPoint(string name, out Vector2 pos)
        {
            foreach (var e in entryPoints) if (e.name == name) { pos = e.position; return true; }
            pos = Vector2.zero;
            return false;
        }

#if UNITY_EDITOR
        // Returns a warning message if sceneRef points at a scene not in Build Settings.
        // Returns null when sceneRef is empty or already registered. Does not depend on Eflatun's
        // runtime guid-to-path map — resolves via UnityEditor.AssetDatabase so tests are deterministic.
        public string GetSceneRefValidationWarning()
        {
            if (sceneRef == null) return null;
            string guid;
            try { guid = sceneRef.Guid; }
            catch { return null; }
            if (string.IsNullOrEmpty(guid)) return null;

            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return null;

            foreach (var s in UnityEditor.EditorBuildSettings.scenes)
                if (s.enabled && s.path == path) return null;

            var sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
            return $"[LevelData] {levelId}: sceneRef '{sceneName}' is not registered in Build Settings. Add it or Play-from-level will fail silently.";
        }

        private void OnValidate()
        {
            var warning = GetSceneRefValidationWarning();
            if (warning != null) Debug.LogWarning(warning, this);
        }
#endif
    }
}
