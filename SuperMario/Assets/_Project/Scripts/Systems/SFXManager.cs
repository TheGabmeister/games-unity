using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXManager : PersistentSingleton<SFXManager>
{
    private AudioSource _audioSource;

    // Awake is called when the script instance is being loaded
    protected override void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void Play(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
    }
}
