using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : PersistentSingleton<MusicManager>
{
    AudioSource _audioSource;

    [SerializeField] AudioClip _starMusic;

    protected override void Awake()
    {
        base.Awake();
        _audioSource = GetComponent<AudioSource>();
    }

    public void Play(AudioClip clip)
    {
        _audioSource.resource = clip;
        _audioSource.Play();
    }

    public void Stop()
    {
        _audioSource.Stop();
    }

    public void Pause(bool value)
    {
        if(value)
        {
            _audioSource.Pause();
        }
        else
        {
            _audioSource.UnPause();
        }
    }
}
