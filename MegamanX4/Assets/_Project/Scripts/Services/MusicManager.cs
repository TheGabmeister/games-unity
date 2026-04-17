using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour, IMusicService
{
    AudioSource _audioSource;

    private void Awake()
    {
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
