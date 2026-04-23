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
    [SerializeField] private ZoneData[] _allZones;

    private ZoneData _currentZone;
    private bool _isTransitioning;

    private void Start()
    {
        foreach (var zone in _allZones)
        {
            if (SceneManager.GetSceneByPath(zone.Scene.Path).isLoaded)
            {
                _currentZone = zone;
                ApplyCameraPosition(_currentZone);
                return;
            }
        }
    }

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
        await SceneLoader.Instance.LoadScene(_startingZone.Scene);
        _currentZone = _startingZone;
        ApplyCameraPosition(_currentZone);
        await ScreenFader.Instance.FadeIn();
    }

    public async void LoadZone(ZoneData zone)
    {
        if (_currentZone == zone || _isTransitioning) return;
        _isTransitioning = true;

        await ScreenFader.Instance.FadeOut();
        if (_currentZone != null)
            await SceneLoader.Instance.UnloadScene(_currentZone.Scene);
        await SceneLoader.Instance.LoadScene(zone.Scene);
        _currentZone = zone;
        ApplyCameraPosition(_currentZone);
        await ScreenFader.Instance.FadeIn();

        _isTransitioning = false;
    }

    private async Awaitable LoadSceneWithFade(SceneReference scene)
    {
        await ScreenFader.Instance.FadeOut();
        UnloadNonBootstrapScenes();
        await SceneLoader.Instance.LoadScene(scene);
        _currentZone = null;
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

    private void ApplyCameraPosition(ZoneData zone)
    {
        GameplayCamera cam = FindFirstObjectByType<GameplayCamera>();
        if (cam != null)
            cam.transform.position = zone.CameraPosition;
    }
}
