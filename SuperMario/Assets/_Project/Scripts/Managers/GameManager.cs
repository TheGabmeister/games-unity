using ScriptableObjectArchitecture;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] SceneData[] _levels;
    [SerializeField] SceneDataReference _currentLevel;
    SceneDataReference _nextLevel;
    int _currentLevelIndex = 0;
    [SerializeField] IntReference _lives;
    [SerializeField] AudioClip _starModeMusic;

    [Header("Listen to these events...")]
    [SerializeField] GameEvent _onStartGame;
    [SerializeField] GameEvent _onRestartStartGame;
    [SerializeField] GameEvent _onPlayerDied;
    [SerializeField] GameEvent _onZeroLives;
    [SerializeField] GameEvent _onReachedFinishLine;
    [SerializeField] BoolGameEvent _onToggleStarMode;

    [Header("Call these events...")]
    [SerializeField] StringGameEvent _loadLevel;
    [SerializeField] GameEvent _restartLevel;
    [SerializeField] AudioClipGameEvent _changeMusic;
    [SerializeField] BoolGameEvent _toggleMusicPlay;
    [SerializeField] BoolGameEvent _toggleLoadingScreen;
    [SerializeField] IntGameEvent _updateLives;
    [SerializeField] IntGameEvent _initializeTimer;
    [SerializeField] GameEvent _startTimer;
    [SerializeField] GameEvent _pauseTimer;

    private void OnEnable()
    {
        _onStartGame.AddListener(StartLevel);
        _onRestartStartGame.AddListener(RestartGame);
        _onPlayerDied.AddListener(HandlePlayerDeath);
        _onZeroLives.AddListener(StartGameOverSequence);
        _onReachedFinishLine.AddListener(StartLevelEndSequence);
        _onToggleStarMode.AddListener(ToggleStarMode);
    }

    private void OnDisable()
    {
        _onStartGame.RemoveListener(StartLevel);
        _onRestartStartGame.RemoveListener(RestartGame);
        _onPlayerDied.RemoveListener(HandlePlayerDeath);
        _onZeroLives.RemoveListener(StartGameOverSequence);
        _onReachedFinishLine.RemoveListener(StartLevelEndSequence);
        _onToggleStarMode.RemoveListener(ToggleStarMode);
    }


    void HandlePlayerDeath()
    {
        StartCoroutine(HandlePlayerDeathCoroutine());
    }

    IEnumerator HandlePlayerDeathCoroutine()
    {
        _toggleMusicPlay.Raise(false);  // Or play death music
        yield return new WaitForSeconds(2);

        _updateLives.Raise(-1);
        if (_lives.Value <= 0)
        {
            StartGameOverSequence();
        }
        else
        {
            StartLevel();
        }
    }

    void RestartGame()
    {
        _currentLevelIndex = 0;
        _loadLevel.Raise("MainMenu");
        _toggleLoadingScreen.Raise(false);
    }

    void StartLevel()
    {
        StartCoroutine(StartLevelCoroutine());
    }

    IEnumerator StartLevelCoroutine()
    {
        _toggleLoadingScreen.Raise(true);
        _loadLevel.Raise(_levels[_currentLevelIndex].sceneName);
        _initializeTimer.Raise(_levels[_currentLevelIndex].time);
        _currentLevel.Value = _levels[_currentLevelIndex];
        yield return new WaitForSeconds(2);
        _toggleLoadingScreen.Raise(false);
        _changeMusic.Raise(_levels[_currentLevelIndex].musicName);
        _startTimer.Raise();
    }


    void StartNextLevel()
    {

    }

    void StartLevelEndSequence()
    {
        StartCoroutine(StartLevelEndSequenceCoroutine());
    }

    IEnumerator StartLevelEndSequenceCoroutine()
    {
        _toggleMusicPlay.Raise(false);
        _pauseTimer.Raise();
        yield return new WaitForSeconds(2);
        // Play success music
        _currentLevelIndex++;

        if (_levels[_currentLevelIndex] != null)
        {
            StartLevel();
        }
        else
        {
            StartGameFinishSequence();
        }
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
            _changeMusic.Raise(_starModeMusic);
        }
        else
        {
            _changeMusic.Raise(_levels[_currentLevelIndex].musicName);
        }
    }
}
