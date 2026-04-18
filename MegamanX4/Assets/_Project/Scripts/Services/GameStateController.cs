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

    public GameState CurrentState => _currentState;

    public void SetState(GameState state)
    {
        if (_currentState == state)
            return;

        _currentState = state;

        if (_currentState == GameState.Intro)
            LoadSimpleScene(IntroSceneName);
        else if (_currentState == GameState.Title)
            LoadSimpleScene(TitleSceneName);
        else if (_currentState == GameState.CharacterSelect)
            LoadSimpleScene(CharacterSelectSceneName);
        else if (_currentState == GameState.LevelSelect)
            LoadSimpleScene(LevelSelectSceneName);
    }

    public void LoadStage(string stageSceneName)
    {
        _currentState = GameState.Gameplay;
        var sceneLoader = Services.Instance.Get<ISceneLoader>();
        sceneLoader.LoadSceneByName(stageSceneName);
    }

    void LoadSimpleScene(string sceneName)
    {
        var sceneLoader = Services.Instance.Get<ISceneLoader>();
        sceneLoader.LoadSceneByName(sceneName);
    }
}
