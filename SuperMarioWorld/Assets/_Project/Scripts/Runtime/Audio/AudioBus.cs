using System.Collections.Generic;
using UnityEngine;

public sealed class AudioBus : MonoBehaviour
{
    public static AudioBus Instance { get; private set; }

    [SerializeField] private AudioCatalog catalog;
    [SerializeField] private AudioSource musicChannel;
    [SerializeField] private AudioSource sfxChannel;
    [SerializeField] private AudioSource jingleChannel;
    [SerializeField] private AudioSource ambientChannel;
    [SerializeField] private AudioSource uiSfxChannel;
    [SerializeField] private bool verbose = false;

    private readonly Stack<MusicStackEntry> _musicStack = new();

    private struct MusicStackEntry
    {
        public MusicId id;
        public float time;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        if (uiSfxChannel != null) uiSfxChannel.ignoreListenerPause = true;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void PlaySfx(SfxId id)
    {
        var clip = catalog != null ? catalog.Get(id) : null;
        if (clip == null) { if (verbose) Debug.Log($"[AudioBus] SFX {id} (no clip)"); return; }
        if (sfxChannel != null) sfxChannel.PlayOneShot(clip);
    }

    public void PlayUiSfx(UiSfxId id)
    {
        var clip = catalog != null ? catalog.Get(id) : null;
        if (clip == null) { if (verbose) Debug.Log($"[AudioBus] UI {id} (no clip)"); return; }
        if (uiSfxChannel != null) uiSfxChannel.PlayOneShot(clip);
    }

    public void PushMusic(MusicId id)
    {
        if (musicChannel != null && musicChannel.isPlaying)
            _musicStack.Push(new MusicStackEntry { id = CurrentMusicId(), time = musicChannel.time });
        PlayMusicInternal(id);
    }

    public void PopMusic()
    {
        if (_musicStack.Count == 0) { StopMusic(); return; }
        var prev = _musicStack.Pop();
        PlayMusicInternal(prev.id, prev.time);
    }

    public void ReplaceMusic(MusicId id)
    {
        _musicStack.Clear();
        PlayMusicInternal(id);
    }

    public void StopMusic()
    {
        if (musicChannel != null) musicChannel.Stop();
    }

    public void PlayJingle(JingleId id)
    {
        var clip = catalog != null ? catalog.Get(id) : null;
        if (clip == null) { if (verbose) Debug.Log($"[AudioBus] Jingle {id} (no clip)"); return; }
        if (jingleChannel != null) jingleChannel.PlayOneShot(clip);
    }

    private MusicId CurrentMusicId()
    {
        return MusicId.None; // tracked on stack push; placeholder for Phase 0 stub
    }

    private void PlayMusicInternal(MusicId id, float startTime = 0f)
    {
        if (musicChannel == null) return;
        var clip = catalog != null ? catalog.Get(id) : null;
        if (clip == null) { if (verbose) Debug.Log($"[AudioBus] Music {id} (no clip)"); musicChannel.Stop(); return; }
        musicChannel.clip = clip;
        musicChannel.time = Mathf.Min(startTime, clip.length - 0.01f);
        musicChannel.loop = true;
        musicChannel.Play();
    }
}
