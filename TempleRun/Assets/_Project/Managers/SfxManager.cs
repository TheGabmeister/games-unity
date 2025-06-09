using UnityEngine;
using EventBus;

[RequireComponent(typeof(AudioSource))]
public class SfxManager : MonoBehaviour
{

    AudioSource _audioSource;

    void OnEnable()
    {
        Bus<EV_SfxPlay>.Add(PlaySound);
    }

    void OnDisable()
    {
        Bus<EV_SfxPlay>.Remove(PlaySound);
    }

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void PlaySound(EV_SfxPlay e)
    {
        _audioSource.PlayOneShot(e.clip);
    }
}
