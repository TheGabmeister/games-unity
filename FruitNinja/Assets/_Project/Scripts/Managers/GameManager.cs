using System;
using UnityEngine;
using PrimeTween;

public class GameManager : MonoBehaviour
{
    int _score = 0;
    int _highScore = 0;
    int _lives = 3;
    ScreenFader _screenFader;
    
    void Awake()
    {
        _screenFader = Services.GetScreenFader();
    }
    
    public void UpdateScore(int amount)
    {
        _score += amount;
        if (_score > _highScore)
        {
            _highScore = _score;
        }
        Events.GameScoreUpdated?.Invoke(_score);
    }
    
    public void BombStrucked()
    {
        _lives--;
        Events.GameLivesUpdated?.Invoke(_lives);
        if (_lives <= 0)
        {
            StartGameOverSequence();
        }
    }

    void StartGameOverSequence()
    {
        Sequence.Create()
            .ChainCallback(() => Debug.Log("Shake Camera"))
            .ChainCallback(() => _screenFader.FadeToColor(new Color(1,1,1,1), 1f))
            .ChainDelay(1f)
            .ChainCallback(() => _screenFader.FadeToColor(new Color(1,1,1,0), 1f))
            .ChainCallback(() => SaveHighScore())
            .ChainCallback(() => Events.GameEnded?.Invoke());
    }

    void SaveHighScore()
    {
            PlayerPrefs.SetInt("HighScore", _highScore);
            PlayerPrefs.Save();
    }
}