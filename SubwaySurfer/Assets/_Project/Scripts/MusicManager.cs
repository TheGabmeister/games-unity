using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioClip _gameplayMusic;
    [SerializeField] AudioClip _gameOverMusic;
    AudioSource _audioSource;

    void OnEnable()
    {
        
    }

    void OnDisable()
    {
        
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    
    void PlayGameplayMusic()
    {
        _audioSource.clip = _gameplayMusic;
        _audioSource.Play();
    }
    
    void PlayGameOverMusic()
    {
        _audioSource.clip = _gameOverMusic;
        _audioSource.Play();
    }
}
