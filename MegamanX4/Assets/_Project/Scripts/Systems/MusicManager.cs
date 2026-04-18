using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    AudioSource _audioSource;

    private void OnEnable()
    {
        MusicEvents.Play.Sub(PlayMusic);
    }
    private void OnDisable()
    {
        MusicEvents.Play.Unsub(PlayMusic);
    }

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayMusic(AudioClip clip)
    {

    }
}
