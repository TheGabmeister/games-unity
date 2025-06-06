using UnityEngine;
using EventBus;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    AudioSource _audioSource;

    private void OnEnable()
    {
        Bus.MusicPlay.Sub(PlayMusic);
        Bus.MusicPause.Sub(PauseMusic);
        Bus.MusicStop.Sub(StopMusic);
    }

    private void OnDisable()
    {
        Bus.MusicPlay.Unsub(PlayMusic);
        Bus.MusicPause.Unsub(PauseMusic);
        Bus.MusicStop.Unsub(StopMusic);
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void PlayMusic(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
    }

    void PauseMusic()
    {
        _audioSource.Pause();
    }

    void StopMusic()
    {
        _audioSource.Stop();
    }
}
