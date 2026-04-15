using System;
using System.Collections.Generic;
using UnityEngine;

namespace SMW.Palette
{
    [CreateAssetMenu(fileName = "Palette", menuName = "SMW/Art/Palette")]
    public sealed class Palette : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public PaletteRole role;
            public Color color;
        }

        [SerializeField] private List<Entry> entries = new();

        public event Action Changed;

        public Color Get(PaletteRole role)
        {
            foreach (var e in entries) if (e.role == role) return e.color;
            return Color.white;
        }

        public bool TryGet(PaletteRole role, out Color color)
        {
            foreach (var e in entries) if (e.role == role) { color = e.color; return true; }
            color = Color.white;
            return false;
        }

        public void NotifyChanged() => Changed?.Invoke();

#if UNITY_EDITOR
        private void OnValidate() => Changed?.Invoke();
#endif
    }
}
