using UnityEngine;

public enum GameState
{
    Boot,
    Title,
    Exploration,
    Battle,
    Menu,
    Dialogue,
    Shop,
    Transition,
    GameOver
}

public enum ExplorationMode
{
    Walking,
    Canoe,
    Ship,
    Airship
}

public class GameStateManager : MonoBehaviour
{
    public GameState CurrentState { get; private set; } = GameState.Boot;
    public GameState PreviousState { get; private set; }
    public ExplorationMode CurrentExplorationMode { get; private set; } = ExplorationMode.Walking;

    public event System.Action<GameState, GameState> OnStateChanged;

    public void ChangeState(GameState newState)
    {
        if (newState == CurrentState) return;
        PreviousState = CurrentState;
        CurrentState = newState;
        OnStateChanged?.Invoke(PreviousState, CurrentState);
    }

    public void SetExplorationMode(ExplorationMode mode)
    {
        CurrentExplorationMode = mode;
    }
}
