using System;
using UnityEngine;

public class GameManager : MonoBehaviour, IGameManager
{
    int _score = 0;
    int _highScore = 0;
    int _lives = 3;
    
    public void UpdateScore(int amount)
    {
        _score += amount;
        if (_score > _highScore)
        {
            _highScore = _score;
        }
        Events.GameScoreUpdated?.Invoke(_score);
    }
    
    void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", _highScore);
        PlayerPrefs.Save();
    }
}

public interface IGameManager
{
    public void UpdateScore(int amount);
}