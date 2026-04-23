using Eflatun.SceneReference;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private SceneReference _splashscreenScene;
    [SerializeField] private SceneReference _introScene;
    [SerializeField] private SceneReference _mainMenuScene;
    [SerializeField] private SceneReference _nameScene;
    [SerializeField] private SceneReference _gameplayBootstrapScene;
    [SerializeField] private SceneReference _gameplayScene;
    [SerializeField] private ScreenFader _screenFader;

    public async void LoadSplashscreenScene()
    {
        await LoadSceneWithFade(_splashscreenScene);
    }

    public async void LoadIntroScene()
    {
        await LoadSceneWithFade(_introScene);
    }

    public async void LoadMainMenuScene()
    {
        await LoadSceneWithFade(_mainMenuScene);
    }

    public async void LoadNameScene()
    {
        await LoadSceneWithFade(_nameScene);
    }

    public async void LoadGameplayScene()
    {
        await _screenFader.FadeOut();
        await SceneLoader.Instance.LoadScenes(_gameplayBootstrapScene, _gameplayScene);
        await _screenFader.FadeIn();
    }

    private async Awaitable LoadSceneWithFade(SceneReference scene)
    {
        await _screenFader.FadeOut();
        await SceneLoader.Instance.LoadScene(scene);
        await _screenFader.FadeIn();
    }
}
