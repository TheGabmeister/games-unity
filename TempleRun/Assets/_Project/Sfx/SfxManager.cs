using UnityEngine;
//using SimpleEventSystem;

[RequireComponent(typeof(AudioSource))]
public class SfxManager : MonoBehaviour
{

    AudioSource _audioSource;

    private void OnEnable()
    {
        //Events.SfxPlay.Sub(PlaySound);
    }

    private void OnDisable()
    {
        //Events.SfxPlay.Unsub(PlaySound);
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
