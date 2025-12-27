using ScriptableObjectArchitecture;
using System.Collections;
using UnityEngine;
using PrimeTween;

public class GameInstance : Singleton<GameInstance>
{
    [SerializeField] LevelData[] _levels;
    [SerializeField] SceneDataReference _currentLevel;
    SceneDataReference _nextLevel;
    [SerializeField] int _lives;
    public int Lives => _lives;
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
        yield return new WaitForSeconds(2);

        _updateLives.Raise(-1);
        if (Lives <= 0)
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
        Sequence.Create()
            .ChainCallback(() => _toggleLoadingScreen.Raise(true))
            .ChainCallback(() => _loadLevel.Raise(_levels[_currentLevelIndex].sceneName))
            .ChainCallback(() => _initializeTimer.Raise(_levels[_currentLevelIndex].time))
            .ChainCallback(() => _currentLevel.Value = _levels[_currentLevelIndex])
            .ChainDelay(2)
            .ChainCallback(() => _toggleLoadingScreen.Raise(false))
            .ChainCallback(() => _startTimer.Raise())
            ;
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
            //_changeMusic.Raise(_starModeMusic);
        }
        else
        {
            //_changeMusic.Raise(_levels[_currentLevelIndex].music);
        }
    }
}
