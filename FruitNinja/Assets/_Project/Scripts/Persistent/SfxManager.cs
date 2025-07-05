using UnityEngine;
using UnityServiceLocator;

[RequireComponent(typeof(AudioSource))]
public class SfxManager : MonoBehaviour, ISfxManager
{
    AudioSource _audioSource;

    void Awake()
    {
        ServiceLocator.Global.Register<ISfxManager>(this);
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
    }
}

public interface ISfxManager
{
    void PlaySound(AudioClip clip);
}