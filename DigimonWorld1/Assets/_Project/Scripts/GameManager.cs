using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : PersistentSingleton<GameManager>
{
    [SerializeField] private SceneReference _splashscreenScene;
    [SerializeField] private SceneReference _introScene;
    [SerializeField] private SceneReference _mainMenuScene;
    [SerializeField] private SceneReference _nameScene;
    [SerializeField] private SceneReference _gameplayScene;

    public void LoadSplashscreenScene()
    {
        SceneManager.LoadScene(_splashscreenScene.Path, LoadSceneMode.Additive);
    }

    public void LoadIntroScene()
    {
        SceneManager.LoadScene(_introScene.Path, LoadSceneMode.Additive);
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene(_mainMenuScene.Path, LoadSceneMode.Additive);
    }

    public void LoadNameScene()
    {
        SceneManager.LoadScene(_nameScene.Path, LoadSceneMode.Additive);
    }

    public void LoadGameplayScene()
    {
        SceneManager.LoadScene(_gameplayScene.Path, LoadSceneMode.Additive);
    }
}
