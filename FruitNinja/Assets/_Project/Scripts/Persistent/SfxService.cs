using UnityEngine;
using UnityServiceLocator;

[RequireComponent(typeof(AudioSource))]
public class SfxService : MonoBehaviour, ISfxService
{
    AudioSource _audioSource;

    void Awake()
    {
        ServiceLocator.Global.Register<ISfxService>(this);
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