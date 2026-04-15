using UnityEngine;
using UnityEngine.UI;

namespace SMW.Palette
{
    [ExecuteAlways]
    public sealed class PaletteBinding : MonoBehaviour
    {
        [SerializeField] private Palette palette;
        [SerializeField] private PaletteRole role = PaletteRole.None;

        private SpriteRenderer _sr;
        private Graphic _graphic;

        private void Awake() { Resolve(); Apply(); }
        private void OnEnable()  { if (palette != null) palette.Changed += Apply; Apply(); }
        private void OnDisable() { if (palette != null) palette.Changed -= Apply; }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Resolve();
            Apply();
        }
#endif

        private void Resolve()
        {
            _sr = GetComponent<SpriteRenderer>();
            _graphic = GetComponent<Graphic>();
        }

        private void Apply()
        {
            if (palette == null) return;
            var c = palette.Get(role);
            if (_sr != null) _sr.color = c;
            if (_graphic != null) _graphic.color = c;
        }

        public void SetPalette(Palette p) { palette = p; Apply(); }
        public void SetRole(PaletteRole r) { role = r; Apply(); }
    }
}
