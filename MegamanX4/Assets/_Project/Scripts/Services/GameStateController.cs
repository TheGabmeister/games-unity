using UnityEngine;

public enum GameState
{
    Title,
    LevelSelect,
    Gameplay
}

[DisallowMultipleComponent]
public class GameStateController : MonoBehaviour
{
    const string TitleSceneName = "Title";
    const string LevelSelectSceneName = "LevelSelect";

    [SerializeField] GameState _currentState = GameState.Title;

    public GameState CurrentState => _currentState;

    public void SetState(GameState state)
    {
        if (_currentState == state)
            return;

        _currentState = state;

        if (_currentState == GameState.Title)
            LoadTitleScene();
        else if (_currentState == GameState.LevelSelect)
            LoadLevelSelectScene();
    }

    public void LoadStage(string stageSceneName)
    {
        _currentState = GameState.Gameplay;
        var sceneLoader = Services.Instance.Get<ISceneLoader>();
        sceneLoader.LoadSceneByName(stageSceneName);
    }

    void LoadTitleScene()
    {
        var sceneLoader = Services.Instance.Get<ISceneLoader>();
        sceneLoader.LoadSceneByName(TitleSceneName);
    }

    void LoadLevelSelectScene()
    {
        var sceneLoader = Services.Instance.Get<ISceneLoader>();
        sceneLoader.LoadSceneByName(LevelSelectSceneName);
    }
}
