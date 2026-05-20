using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameStateMachine : MonoBehaviour
{
    public static GameStateMachine Instance { get; private set; }

    private readonly Stack<IGameState> _stack = new();

    public IGameState Current => _stack.Count > 0 ? _stack.Peek() : null;
    public event Action<IGameState, IGameState> StateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Transition(IGameState next)
    {
        var prev = Current;
        if (prev != null)
        {
            prev.OnExit();
            _stack.Pop();
        }
        if (next != null)
        {
            _stack.Push(next);
            next.OnEnter();
        }
        StateChanged?.Invoke(prev, next);
    }

    public void Push(IGameState modal)
    {
        _stack.Push(modal);
        modal.OnEnter();
        StateChanged?.Invoke(null, modal);
    }

    public void Pop()
    {
        if (_stack.Count == 0) return;
        var top = _stack.Pop();
        top.OnExit();
        StateChanged?.Invoke(top, Current);
    }

    public void TransitionToTitle() => Transition(new TitleState());
    public void TransitionToOverworld() => Transition(new OverworldState());
    public void TransitionToLevel(LevelData data, string entryPoint) =>
        Transition(new LevelState(data, entryPoint));

#if UNITY_EDITOR
    public void EnterDirectLevel(LevelData data, string entryPoint)
    {
        if (Current is LevelState) return;
        Transition(new LevelState(data, entryPoint));
    }

    public void EnterDirectTitle()
    {
        if (Current is TitleState) return;
        Transition(new TitleState());
    }

    public void EnterDirectOverworld()
    {
        if (Current is OverworldState) return;
        Transition(new OverworldState());
    }
#endif
}
