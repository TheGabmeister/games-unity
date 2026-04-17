using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour, IMusicService
{
    AudioSource _audioSource;

    void OnEnable()
    {
        var services = ResolveServices();
        if (!services)
            return;

        services.Register<MusicManager>(this);
        services.Register<IMusicService>(this);
    }

    void OnDisable()
    {
        var services = Services.Instance;
        if (!services)
            return;

        services.Unregister<IMusicService>(this);
        services.Unregister<MusicManager>(this);
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void ToggleMusic(bool value)
    {
        if (value)
            _audioSource.Play();
        else
            _audioSource.Stop();
    }

    static Services ResolveServices()
    {
        return Services.Instance ? Services.Instance : Object.FindFirstObjectByType<Services>();
    }
}
