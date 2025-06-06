using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] AudioClip mainMenuMusic;
    [SerializeField] AudioClip gameplayMusic;

    private AudioSource audioSource;
    

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "MainMenu":
                audioSource.clip = mainMenuMusic;
                break;
            case "Gameplay":
                audioSource.clip = gameplayMusic;
                break;
                // add more cases for other scenes as needed
        }
        audioSource.Play();
    }
}