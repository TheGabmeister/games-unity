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
    [SerializeField] GameState _currentState = GameState.Title;

    public GameState CurrentState => _currentState;
}
