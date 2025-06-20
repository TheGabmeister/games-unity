using System;
using UnityEngine;
using Obvious.Soap;

public class GameStateManager : MonoBehaviour
{
    [Header("Listen to these events...")] 
    [SerializeField] ScriptableEventNoParam _onGameStateStart;
    
    [Header("Call these events...")] 
    [SerializeField] ScriptableEventUIState _onUIStateChange;
    [SerializeField] ScriptableEventNoParam _onPlayerStartMoving;
    
    void OnEnable()
    {
        _onGameStateStart.OnRaised += StartGameplay;
    }
    
    void OnDisable()
    {
        _onGameStateStart.OnRaised -= StartGameplay;
    }

    
    void StartGameplay()
    {
        _onUIStateChange.Raise(UIState.GameOver);
        _onPlayerStartMoving.Raise();
    }
}