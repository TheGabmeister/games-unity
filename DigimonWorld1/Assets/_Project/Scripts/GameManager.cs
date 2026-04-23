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
    [SerializeField] private ZoneData _startingZone;
    [SerializeField] private ScreenFader _screenFader;

    private ZoneData _currentZone;

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
        UnloadNonBootstrapScenes();
        await SceneLoader.Instance.LoadScene(_gameplayScene);
        await SceneLoader.Instance.LoadScene(_startingZone.Scene);
        _currentZone = _startingZone;
        ApplyCameraPosition(_currentZone);
        await _screenFader.FadeIn();
    }

    public async void LoadZone(ZoneData zone)
    {
        if (_currentZone == zone) return;

        await _screenFader.FadeOut();
        if (_currentZone != null)
            await SceneLoader.Instance.UnloadScene(_currentZone.Scene);
        await SceneLoader.Instance.LoadScene(zone.Scene);
        _currentZone = zone;
        ApplyCameraPosition(_currentZone);
        await _screenFader.FadeIn();
    }

    private async Awaitable LoadSceneWithFade(SceneReference scene)
    {
        await _screenFader.FadeOut();
        UnloadNonBootstrapScenes();
        await SceneLoader.Instance.LoadScene(scene);
        _currentZone = null;
        await _screenFader.FadeIn();
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

    private void ApplyCameraPosition(ZoneData zone)
    {
        GameplayCamera cam = FindFirstObjectByType<GameplayCamera>();
        if (cam != null)
            cam.transform.position = zone.CameraPosition;
    }
}
