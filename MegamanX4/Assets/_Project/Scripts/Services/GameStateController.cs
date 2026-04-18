using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public enum GameState
{
    Intro,
    Title,
    CharacterSelect,
    LevelSelect,
    Gameplay
}

[DisallowMultipleComponent]
public class GameStateController : MonoBehaviour
{
    const string IntroSceneName = "Init";
    const string TitleSceneName = "Title";
    const string CharacterSelectSceneName = "CharacterSelect";
    const string LevelSelectSceneName = "LevelSelect";

    [SerializeField] GameState _currentState = GameState.Title;
    [SerializeField] SceneLoader _sceneLoader;
    [SerializeField] ScreenFader _fader;
    [SerializeField] LoadingScreen _loading;
    [SerializeField] float _fadeDuration = 0.3f;
    [SerializeField] float _minLoadingSeconds = 1f;

    public GameState CurrentState => _currentState;
    public ScreenFader Fader => _fader;

    void Awake()
    {
        if (!_sceneLoader)
            _sceneLoader = transform.root.GetComponentInChildren<SceneLoader>(true);
        if (!_fader)
            _fader = transform.root.GetComponentInChildren<ScreenFader>(true);
        if (!_loading)
            _loading = transform.root.GetComponentInChildren<LoadingScreen>(true);
    }

    public void SetState(GameState state)
    {
        if (_currentState == state)
            return;

        _currentState = state;

        if (_currentState == GameState.Intro)
            _ = _sceneLoader.LoadSceneByName(IntroSceneName);
        else if (_currentState == GameState.Title)
            _ = _sceneLoader.LoadSceneByName(TitleSceneName);
        else if (_currentState == GameState.CharacterSelect)
            _ = _sceneLoader.LoadSceneByName(CharacterSelectSceneName);
        else if (_currentState == GameState.LevelSelect)
            _ = _sceneLoader.LoadSceneByName(LevelSelectSceneName);
    }

    public async void GoToCharacterSelect()
    {
        _currentState = GameState.CharacterSelect;
        await FadeToLoadingThenLoad(CharacterSelectSceneName);
    }

    public async void LoadStage(string stageSceneName)
    {
        _currentState = GameState.Gameplay;
        await FadeToLoadingThenLoad(stageSceneName);
    }

    async Task FadeToLoadingThenLoad(string sceneName)
    {
        await Fade(Color.black);
        _loading.Show();
        await Fade(Color.clear);
        await Tween.Delay(_minLoadingSeconds);
        await Fade(Color.black);
        await _sceneLoader.LoadSceneByName(sceneName);
        _loading.Hide();
        await Fade(Color.clear);
    }

    Tween Fade(Color color) => _fader.FadeToColor(color, _fadeDuration);
}
