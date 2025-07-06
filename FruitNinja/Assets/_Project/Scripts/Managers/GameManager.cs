using System;
using UnityEngine;
using System.Collections;
using PrimeTween;

public class GameManager : MonoBehaviour
{
    int _score = 0;
    int _highScore = 0;
    int _lives = 3;
    int _timeRemaining = 90;
    ScreenFader _screenFader;
    Camera _camera;
    
    void Awake()
    {
        _screenFader = Services.GetScreenFader();
    }

    void Start()
    {
        StartZenMode();
    }

    void StartGameplay()
    {
        
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
            StartClassicGameOverSequence();
        }
    }

    void StartClassicGameOverSequence()
    {
        Sequence.Create()
            .ChainCallback(() => Debug.Log("Shake Camera"))
            .ChainCallback(() => _screenFader.FadeToColor(new Color(1,1,1,1), 1f))
            .ChainDelay(1f)
            .ChainCallback(() => _screenFader.FadeToColor(new Color(1,1,1,0), 1f))
            .ChainCallback(() => SaveHighScore())
            .ChainCallback(() => Events.GameClassicModeEnded?.Invoke());
    }

    void SaveHighScore()
    {
            PlayerPrefs.SetInt("HighScore", _highScore);
            PlayerPrefs.Save();
    }
    
    void StartZenMode()
    {
        Events.GameZenModeStarted?.Invoke();
        _timeRemaining = 90;
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        Events.GameTimerUpdated?.Invoke(_timeRemaining);
        while(_timeRemaining > 0) {
            yield return new WaitForSeconds(1);
            _timeRemaining -= 1;
            Events.GameTimerUpdated?.Invoke(_timeRemaining);
            Debug.Log("Game Timer: " + _timeRemaining);
        }
        Debug.Log("Game Over! Time's up!");
    }
}