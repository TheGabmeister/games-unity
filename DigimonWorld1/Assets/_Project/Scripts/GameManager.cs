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
    [SerializeField] private ScreenFader _screenFader;

    public void LoadSplashscreenScene()
    {
        LoadScene(_splashscreenScene);
    }

    public void LoadIntroScene()
    {
        LoadScene(_introScene);
    }

    public void LoadMainMenuScene()
    {
        LoadScene(_mainMenuScene);
    }

    public void LoadNameScene()
    {
        LoadScene(_nameScene);
    }

    public void LoadGameplayScene()
    {
        LoadScene(_gameplayScene);
    }

    private void LoadScene(SceneReference scene)
    {
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene loaded = SceneManager.GetSceneAt(i);
            if (loaded.buildIndex != 0)
                SceneManager.UnloadSceneAsync(loaded);
        }

        SceneManager.LoadScene(scene.Path, LoadSceneMode.Additive);
    }
}
