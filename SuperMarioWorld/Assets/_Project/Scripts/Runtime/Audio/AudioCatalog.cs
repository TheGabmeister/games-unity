using System;
using System.Collections.Generic;
using UnityEngine;

namespace SMW.Audio
{
    [CreateAssetMenu(fileName = "AudioCatalog", menuName = "SMW/Audio/Audio Catalog")]
    public sealed class AudioCatalog : ScriptableObject
    {
        [Serializable] public struct SfxEntry   { public SfxId id;   public AudioClip clip; }
        [Serializable] public struct MusicEntry { public MusicId id; public AudioClip clip; }
        [Serializable] public struct JingleEntry{ public JingleId id;public AudioClip clip; }
        [Serializable] public struct UiSfxEntry { public UiSfxId id; public AudioClip clip; }

        [SerializeField] private List<SfxEntry> sfx = new();
        [SerializeField] private List<MusicEntry> music = new();
        [SerializeField] private List<JingleEntry> jingles = new();
        [SerializeField] private List<UiSfxEntry> uiSfx = new();

        public AudioClip Get(SfxId id)    { foreach (var e in sfx)     if (e.id == id) return e.clip; return null; }
        public AudioClip Get(MusicId id)  { foreach (var e in music)   if (e.id == id) return e.clip; return null; }
        public AudioClip Get(JingleId id) { foreach (var e in jingles) if (e.id == id) return e.clip; return null; }
        public AudioClip Get(UiSfxId id)  { foreach (var e in uiSfx)   if (e.id == id) return e.clip; return null; }
    }
}
