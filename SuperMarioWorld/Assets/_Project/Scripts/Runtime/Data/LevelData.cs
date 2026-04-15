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
        [SerializeField] private Palette themePalette;
        [SerializeField] private List<EntryPoint> entryPoints = new();
        [SerializeField] private List<SubArea> subAreas = new();
        [SerializeField] private List<LevelData> unlocksOnNormalExit = new();
        [SerializeField] private List<LevelData> unlocksOnSecretExit = new();

        public string LevelId => levelId;
        public string DisplayName => displayName;
        public SceneReference SceneRef => sceneRef;
        public int TimeLimitSeconds => timeLimitSeconds;
        public MusicId MusicId => musicId;
        public Palette ThemePalette => themePalette;
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
        private void OnValidate()
        {
            if (sceneRef == null) return;
            string sceneName;
            try { sceneName = sceneRef.Name; }
            catch { return; }
            if (string.IsNullOrEmpty(sceneName)) return;

            var editorScenes = UnityEditor.EditorBuildSettings.scenes;
            bool registered = false;
            foreach (var s in editorScenes)
            {
                if (s.enabled && s.path.EndsWith($"{sceneName}.unity"))
                {
                    registered = true;
                    break;
                }
            }
            if (!registered)
                Debug.LogWarning($"[LevelData] {levelId}: sceneRef '{sceneName}' is not registered in Build Settings. Add it or Play-from-level will fail silently.", this);
        }
#endif
    }
}
