using UnityEngine;

// A simple service locator pattern. This singleton is instantiated
// by Bootstrapper.cs during RuntimeInitializeLoadType.BeforeSceneLoad.
// Should only be one instance for the entire game!

public class Services : MonoBehaviour
{
    public static Services Instance { get; private set; }

    [SerializeField] GameManager _gameManager;
    public GameManager GameManager => _gameManager;
    
    [SerializeField] MusicManager _musicManager;
    public MusicManager MusicManager => _musicManager;
    
    [SerializeField] SfxManager _sfxManager;
    public SfxManager SfxManager => _sfxManager;
    
    [SerializeField] ScreenFader _screenFader;
    public ScreenFader ScreenFader => _screenFader;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
    
    public static GameManager GetGameManager() => Instance.GameManager;
    public static MusicManager GetMusicManager() => Instance.MusicManager;
    public static SfxManager GetSfxManager() => Instance.SfxManager;
    public static ScreenFader GetScreenFader() => Instance.ScreenFader;
}