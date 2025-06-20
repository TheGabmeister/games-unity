using UnityEngine;
using Obvious.Soap;

[RequireComponent(typeof(AudioSource))]
public class SfxManager : MonoBehaviour
{
    [SerializeField] ScriptableEventAudioClip _onSfxPlay;
    AudioSource _audioSource;

    void OnEnable()
    {
        _onSfxPlay.OnRaised += PlaySound;
    }
    
    void OnDisable()
    {
        _onSfxPlay.OnRaised -= PlaySound;
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