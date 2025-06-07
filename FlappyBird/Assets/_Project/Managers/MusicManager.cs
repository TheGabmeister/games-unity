using UnityEngine;
using SimpleEventSystem;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    AudioSource _audioSource;

    private void OnEnable()
    {
        Events.MusicPlay.Sub(PlayMusic);
        Events.MusicPause.Sub(PauseMusic);
        Events.MusicStop.Sub(StopMusic);
    }

    private void OnDisable()
    {
        Events.MusicPlay.Unsub(PlayMusic);
        Events.MusicPause.Unsub(PauseMusic);
        Events.MusicStop.Unsub(StopMusic);
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
