using ScriptableObjectArchitecture;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    AudioSource _audioSource;
    AudioClip _currentClip;

    [SerializeField] AudioClip _starMusic;

    [Header("Listen to these events...")]
    [SerializeField] AudioClipGameEvent _onChangeMusic;
    [SerializeField] BoolGameEvent _onPlayMusic;
    [SerializeField] BoolGameEvent _onPauseMusic;


    private void OnEnable()
    {
        _onChangeMusic.AddListener(ChangeMusic);
        _onPlayMusic.AddListener(TogglePlay);
        _onPauseMusic.AddListener(TogglePause);
    }

    private void OnDisable()
    {
        _onChangeMusic.RemoveListener(ChangeMusic);
        _onPlayMusic.RemoveListener(TogglePlay);
        _onPauseMusic.RemoveListener(TogglePause);
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void ChangeMusic(AudioClip clip)
    {
        _audioSource.resource = clip;
        _audioSource.Play();
    }

    private void TogglePlay(bool value)
    {
        if (value)
        {
            _audioSource.Play();
        }
        else
        {
            _audioSource.Stop();
        }
    }

    private void TogglePause(bool value)
    {
        if(value)
        {
            _audioSource.Pause();
        }
        else
        {
            _audioSource.UnPause();
        }
    }
}
