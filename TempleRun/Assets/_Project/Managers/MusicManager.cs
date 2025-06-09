using UnityEngine;
using EventBus;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    AudioSource _audioSource;

    void OnEnable()
    {
        Bus<EV_MusicToggle>.Add(ToggleMusic);
    }

    void OnDisable()
    {
        Bus<EV_MusicToggle>.Remove(ToggleMusic);
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void ToggleMusic(EV_MusicToggle e)
    {
        if (e.value)
            _audioSource.Play();
        else
            _audioSource.Stop();
    }
}
