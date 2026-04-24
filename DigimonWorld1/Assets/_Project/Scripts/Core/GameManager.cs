using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private SceneReference _splashscreenScene;
    [SerializeField] private SceneReference _introScene;
    [SerializeField] private SceneReference _mainMenuScene;
    [SerializeField] private SceneReference _nameScene;
    [SerializeField] private SceneReference _gameplayScene;

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
        await ScreenFader.Instance.FadeOut();
        UnloadNonBootstrapScenes();
        await SceneLoader.Instance.LoadScene(_gameplayScene);
        ZoneData startingZone = GameplayManager.Instance.StartingZone;
        await SceneLoader.Instance.LoadScene(startingZone.Scene);
        GameplayManager.Instance.SetInitialZone(startingZone);
        await ScreenFader.Instance.FadeIn();
    }

    private async Awaitable LoadSceneWithFade(SceneReference scene)
    {
        await ScreenFader.Instance.FadeOut();
        UnloadNonBootstrapScenes();
        await SceneLoader.Instance.LoadScene(scene);
        await ScreenFader.Instance.FadeIn();
    }

    private void UnloadNonBootstrapScenes()
    {
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene loaded = SceneManager.GetSceneAt(i);
            if (loaded.buildIndex != 0)
                SceneManager.UnloadSceneAsync(loaded);
        }
    }
}
