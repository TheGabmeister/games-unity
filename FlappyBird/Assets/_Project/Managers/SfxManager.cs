using UnityEngine;
using EventBus;

[RequireComponent(typeof(AudioSource))]
public class SfxManager : MonoBehaviour
{

    AudioSource _audioSource;

    private void OnEnable()
    {
        Bus.SfxPlay.Sub(PlaySound);
    }

    private void OnDisable()
    {
        Bus.SfxPlay.Unsub(PlaySound);
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void PlaySound(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
    }
}
