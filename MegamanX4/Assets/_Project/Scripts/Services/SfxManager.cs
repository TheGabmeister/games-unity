using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxManager : MonoBehaviour, ISfxService
{
    AudioSource _audioSource;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        var services = ResolveServices();
        if (!services)
            return;

        services.Register<SfxManager>(this);
        services.Register<ISfxService>(this);
    }

    void OnDisable()
    {
        var services = Services.Instance;
        if (!services)
            return;

        services.Unregister<ISfxService>(this);
        services.Unregister<SfxManager>(this);
    }

    public void PlaySound(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
    }

    static Services ResolveServices()
    {
        return Services.Instance ? Services.Instance : Object.FindFirstObjectByType<Services>();
    }
}
