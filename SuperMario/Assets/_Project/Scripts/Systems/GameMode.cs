using UnityEngine;
using PrimeTween;

public class GameMode : MonoBehaviour
{
    [SerializeField] GameObject _playerPrefab;
    [SerializeField] LevelData _levelData;
    int _remainingTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Sequence.Create()
            .ChainDelay(2)
            .ChainCallback(() => SpawnPlayerPrefab())
            .ChainCallback(() => MusicManager.Instance.Play(_levelData.music))
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
        if (_remainingTime == 100)
        {
            //_onHunderedSecondsLeft?.Raise();
        }

        if (_remainingTime <= 0)
        {
            //_onFinishedTime?.Raise();
        }
    }
}
