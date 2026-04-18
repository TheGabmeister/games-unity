using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxManager : MonoBehaviour
{
    AudioSource _audioSource;

    private void OnEnable()
    {
        SfxEvents.Play.Sub(PlaySound);
    }
    private void OnDisable()
    {
        SfxEvents.Play.Unsub(PlaySound);
    }

    void Awake()
    {
         _audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
    }
}
