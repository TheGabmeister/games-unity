using UnityEngine;
using UnityEngine.Audio;

public class AudioSystem : Singleton<AudioSystem>
{
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private int _sfxPoolSize = 8;

    private AudioSource _musicSource;
    private AudioSource[] _sfxPool;
    private int _sfxIndex;

    protected override void Awake()
    {
        base.Awake();

        GameObject musicGo = new GameObject("MusicSource");
        musicGo.transform.SetParent(transform);
        _musicSource = musicGo.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop = true;
        AssignToMixerGroup(_musicSource, "Music");

        _sfxPool = new AudioSource[_sfxPoolSize];
        for (int i = 0; i < _sfxPoolSize; i++)
        {
            GameObject sfxGo = new GameObject($"SFXSource_{i}");
            sfxGo.transform.SetParent(transform);
            _sfxPool[i] = sfxGo.AddComponent<AudioSource>();
            _sfxPool[i].playOnAwake = false;
            AssignToMixerGroup(_sfxPool[i], "SFX");
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        AudioSource source = _sfxPool[_sfxIndex];
        source.clip = clip;
        source.volume = volume;
        source.Play();
        _sfxIndex = (_sfxIndex + 1) % _sfxPool.Length;
    }

    public void PlayMusic(AudioClip clip, float volume = 1f, bool loop = true)
    {
        if (clip == null) return;

        _musicSource.clip = clip;
        _musicSource.volume = volume;
        _musicSource.loop = loop;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        _musicSource.Stop();
        _musicSource.clip = null;
    }

    public void SetBusVolume(string busName, float linearVolume)
    {
        if (_mixer == null) return;
        float db = 20f * Mathf.Log10(Mathf.Max(linearVolume, 0.0001f));
        _mixer.SetFloat(busName, db);
    }

    private void AssignToMixerGroup(AudioSource source, string groupName)
    {
        if (_mixer == null) return;
        AudioMixerGroup[] groups = _mixer.FindMatchingGroups(groupName);
        if (groups.Length > 0)
            source.outputAudioMixerGroup = groups[0];
    }
}
