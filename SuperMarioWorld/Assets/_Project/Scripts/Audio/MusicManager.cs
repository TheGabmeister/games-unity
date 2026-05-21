using UnityEngine;

public sealed class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioSource _source;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Play(AudioClip clip)
    {
        if (_source == null) return;
        if (clip == null) { _source.Stop(); return; }
        _source.clip = clip;
        _source.loop = true;
        _source.Play();
    }

    public void Stop()
    {
        if (_source != null) _source.Stop();
    }
}
