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

    public void LoadStage(string stageSceneName)
    {
        _currentState = GameState.Gameplay;
        _ = _sceneLoader.LoadSceneByName(stageSceneName);
    }
}
