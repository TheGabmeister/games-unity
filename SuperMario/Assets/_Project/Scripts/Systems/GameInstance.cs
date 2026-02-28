using System.Collections;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Eflatun.SceneReference;
using EventSystem;

public class GameInstance : Singleton<GameInstance>
{
    [SerializeField] LevelData[] _levels;
    //[SerializeField] SceneDataReference _currentLevel;
    //SceneDataReference _nextLevel;

    [SerializeField] private SceneReference _mainMenuLevel;
    [SerializeField] private SceneReference _firstLevel;

    
    int _score = 0;
    int _coins = 0;
    int _lives = 3;

    [SerializeField] AudioClip _starModeMusic;

    public static event Action<int> StatsScoreUpdated;
    public static event Action<int> StatsCoinsUpdated;
    public static event Action<int> StatsLivesUpdated;

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    public void StartGame()
    {
        SceneManager.LoadScene(_firstLevel.Name);
    }

    public void UpdateScore(int value)
    {
        _score += value;
        StatsScoreUpdated?.Invoke(_score);
    }

    public void UpdateCoins(int value)
    {
        _coins += value;
        StatsCoinsUpdated?.Invoke(_coins);
    }

    public void UpdateLives(int value)
    {
        _lives += value;
        StatsLivesUpdated?.Invoke(_lives);
        if (_lives <= 0)
        {
            //_onZeroLivesLeft?.Raise();
        }
    }

    void HandlePlayerDeath()
    {
        StartCoroutine(HandlePlayerDeathCoroutine());
    }

    IEnumerator HandlePlayerDeathCoroutine()
    {
        yield return new WaitForSeconds(2);

        //_updateLives.Raise(-1);
        if (_lives <= 0)
        {
            StartGameOverSequence();
        }
        else
        {
            StartLevel();
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(_mainMenuLevel.Name);
    }

    void StartLevel()
    {

    }

    void StartNextLevel()
    {

    }


    void StartGameOverSequence()
    {
        // Show Game Over UI
    }

    void StartGameFinishSequence()
    {
        Debug.Log("Game Finished!");
    }

    private void ToggleStarMode(bool value)
    {
        if (value)
        {
            //_changeMusic.Raise(_starModeMusic);
        }
        else
        {
            //_changeMusic.Raise(_levels[_currentLevelIndex].music);
        }
    }
}
