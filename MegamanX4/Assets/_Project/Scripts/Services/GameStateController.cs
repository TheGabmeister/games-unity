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

    [SerializeField] GameState _currentState = GameState.Title;

    public GameState CurrentState => _currentState;

    public void SetState(GameState state)
    {
        if (_currentState == state)
            return;

        _currentState = state;

        if (_currentState == GameState.Title)
            LoadTitleScene();
    }

    void LoadTitleScene()
    {
        if (!Services.TryGet<ISceneLoader>(out var sceneLoader) || sceneLoader == null)
        {
            Debug.LogWarning("GameStateController could not find a SceneLoader when switching to Title.", this);
            return;
        }

        sceneLoader.LoadSceneByName(TitleSceneName);
    }
}
