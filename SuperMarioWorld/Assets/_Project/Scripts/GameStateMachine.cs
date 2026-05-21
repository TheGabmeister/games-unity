using System;
using Eflatun.SceneReference;
using UnityEngine;

public enum GameState
{
    None,
    Title,
    Overworld,
    Level,
}

public sealed class GameStateMachine : MonoBehaviour
{
    public static GameStateMachine Instance { get; private set; }

    [SerializeField] private SceneReference titleScene;
    [SerializeField] private SceneReference overworldScene;

    public GameState Current { get; private set; }
    public LevelData CurrentLevelData { get; private set; }
    public string CurrentEntryPoint { get; private set; }

    public event Action<GameState, GameState> StateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public async void TransitionToTitle()
    {
        var prev = Current;
        Current = GameState.Title;
        CurrentLevelData = null;
        CurrentEntryPoint = null;
        PlayerInputBinding.Instance?.SwitchActionMap(InputMapNames.UI);
        StateChanged?.Invoke(prev, Current);
        await SceneLoader.Instance.LoadAsync(titleScene);
    }

    public async void TransitionToOverworld()
    {
        var prev = Current;
        Current = GameState.Overworld;
        CurrentLevelData = null;
        CurrentEntryPoint = null;
        PlayerInputBinding.Instance?.SwitchActionMap(InputMapNames.Overworld);
        StateChanged?.Invoke(prev, Current);
        await SceneLoader.Instance.LoadAsync(overworldScene);
    }

    public async void TransitionToLevel(LevelData data, string entryPoint)
    {
        var prev = Current;
        Current = GameState.Level;
        CurrentLevelData = data;
        CurrentEntryPoint = entryPoint;
        PlayerInputBinding.Instance?.SwitchActionMap(InputMapNames.Player);
        StateChanged?.Invoke(prev, Current);
        await SceneLoader.Instance.LoadAsync(data.SceneRef);
    }

#if UNITY_EDITOR
    public void EnterDirectLevel(LevelData data, string entryPoint)
    {
        if (Current == GameState.Level) return;
        Current = GameState.Level;
        CurrentLevelData = data;
        CurrentEntryPoint = entryPoint;
        PlayerInputBinding.Instance?.SwitchActionMap(InputMapNames.Player);
    }

    public void EnterDirectTitle()
    {
        if (Current == GameState.Title) return;
        Current = GameState.Title;
        PlayerInputBinding.Instance?.SwitchActionMap(InputMapNames.UI);
    }

    public void EnterDirectOverworld()
    {
        if (Current == GameState.Overworld) return;
        Current = GameState.Overworld;
        PlayerInputBinding.Instance?.SwitchActionMap(InputMapNames.Overworld);
    }
#endif
}
