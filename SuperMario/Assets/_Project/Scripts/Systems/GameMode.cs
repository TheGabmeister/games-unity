using EventSystem;
using PrimeTween;
using System;
using UnityEngine;

public class GameMode : MonoBehaviour
{
    [SerializeField] GameObject _playerPrefab;
    [SerializeField] AudioClip _pauseAudioClip;
    [SerializeField] LevelData _levelData;
    int _remainingTime;
    bool _isGamePaused = false;


    private void OnEnable()
    {
        Events.PauseToggled.Sub(OnPauseToggled);
    }

    private void OnDisable()
    {
        Events.PauseToggled.Unsub(OnPauseToggled);
    }

    void Start()
    {
        Sequence.Create()
            .ChainDelay(2)
            .ChainCallback(() => SpawnPlayerPrefab())
            .ChainCallback(() => Events.LevelDataInitialized.Raise(_levelData))
            //.ChainCallback(() => MusicManager.Instance.Play(_levelData.music))
            .ChainCallback(() => _remainingTime = _levelData.time)
            .ChainCallback(() => InvokeRepeating("UpdateTime", 0.0f, 1.0f))
        ;
    }

    void SpawnPlayerPrefab()
    {
        GameObject playerStart = GameObject.FindWithTag("PlayerStart");

        if (playerStart != null)
        {
            Vector2 pos2D = new Vector2(playerStart.transform.position.x, playerStart.transform.position.y);
            Instantiate(_playerPrefab, pos2D, Quaternion.identity);
        }
        else
        {
            Debug.Log($"No GameObject with tag 'PlayerStart' found in the scene. Spawning in (0,0)...");
            Instantiate(_playerPrefab, Vector2.zero, Quaternion.identity);
        }
    }

    void PauseTimer()
    {
        CancelInvoke();
    }

    void UpdateTime()
    {
        _remainingTime -= 1;
        Events.TimerUpdated.Raise(_remainingTime);

        if (_remainingTime == 100)
        {
            Events.TimerHundredSecondsLeft.Raise();
        }

        if (_remainingTime <= 0)
        {
            Events.TimerFinished.Raise();
        }
    }

    void OnPauseToggled()
    {
        if (_isGamePaused)
        {
            _isGamePaused = false;
            //_onUnpauseGame.Raise();
            //_onPlaySound.Raise(_pauseAudioClip);
            Time.timeScale = 1;
        }
        else
        {
            _isGamePaused = true;
            //_onPauseGame.Raise();
            //_onPlaySound.Raise(_pauseAudioClip);
            Time.timeScale = 0;
        }
    }
}
