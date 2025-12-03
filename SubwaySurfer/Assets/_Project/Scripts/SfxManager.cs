using UnityEngine;
using Obvious.Soap;

[RequireComponent(typeof(AudioSource))]
public class SfxManager : MonoBehaviour
{
    AudioSource _audioSource;

    [Header("Listen to these events...")]
    [SerializeField] ScriptableEventAudioClip _onSfxPlay;

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