using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxManager : Singleton<SfxManager>
{
    AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
    }
}