using UnityEngine;
using UnityServiceLocator;

[RequireComponent(typeof(AudioSource))]
public class SfxManager : MonoBehaviour, ISfxService
{

    AudioSource _audioSource;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
    }
}

public interface ISfxService
{
    void PlaySound(AudioClip clip);
}