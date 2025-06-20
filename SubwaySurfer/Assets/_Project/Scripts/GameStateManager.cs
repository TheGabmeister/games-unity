using System;
using UnityEngine;
using Obvious.Soap;

public class GameStateManager : MonoBehaviour
{
    [Header("Listen to these events...")] 
    [SerializeField] ScriptableEventNoParam _onGameStateStart;
    
    void OnEnable()
    {
        _onGameStateStart.OnRaised += OnTest;
    }
    
    void OnDisable()
    {
        _onGameStateStart.OnRaised -= OnTest;
    }

    void Start()
    {
        GameState.InitGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnTest()
    {
        Debug.Log("Testing");
    }
}
