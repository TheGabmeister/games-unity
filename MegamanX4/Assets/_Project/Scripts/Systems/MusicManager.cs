using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : PersistentSingleton<MusicManager>
{
    AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();
        _audioSource = GetComponent<AudioSource>();
    }

    public void ToggleMusic(bool value)
    {
        if (value)
            _audioSource.Play();
        else
            _audioSource.Stop();
    }
}
