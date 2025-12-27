using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxManager : Singleton<SfxManager>
{
    private AudioSource _audioSource;

    // Awake is called when the script instance is being loaded
    protected override void Awake()
    {
        base.Awake();
        _audioSource = GetComponent<AudioSource>();
    }

    public void Play(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
    }
}
