using UnityEngine;

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    private AudioSource _audioSource;

    void Awake()
    {
        Instance = this;
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
            _audioSource.PlayOneShot(clip);
    }

    public void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null)
            _audioSource.PlayOneShot(clip, volume);
    }
}
