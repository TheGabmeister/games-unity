using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    int _score = 0;
    
    public void AddScore(int amount)
    {
        _score += amount;
        Events.GameScoreUpdated?.Invoke(_score);
    }
}
