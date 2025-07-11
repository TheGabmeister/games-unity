using System;
using UnityEngine;

public class UIGameplayController : MonoBehaviour
{
    private void Start()
    {
        Events.TestFunction.Invoke();
    }

    void OnEnable()
    {
        GameManager.TimerUpdated += OnTimerUpdated;
    }
    
    void OnDisable()
    {
        GameManager.TimerUpdated -= OnTimerUpdated;
    }

    void OnTimerUpdated(int timeRemaining)
    {
        // Update UI with the remaining time
        Debug.Log($"Time Remaining: {timeRemaining}");
    }
}
