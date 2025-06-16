using UnityEngine;
using PrimeTween;
using EventBus;

public class GameManager : MonoBehaviour
{
    int _score = 0;
    int _distance = 0;
    int _coins = 0;
    int _coinScore = 10;
    int _highScore = 0;
  
    [SerializeField] GameObject _playerPrefab;

    void OnEnable()
    {
        Bus<EV_GameStart>.Add(StartGame);
        Bus<EV_GameRestart>.Add(RestartGame);
        Bus<EV_PlayerDied>.Add(HandlePlayerDeath);
        Bus<EV_CoinCollected>.Add(AddCoin);
    }

    void OnDisable()
    {
        Bus<EV_GameStart>.Remove(StartGame);
        Bus<EV_GameRestart>.Remove(RestartGame);
        Bus<EV_PlayerDied>.Remove(HandlePlayerDeath);
        Bus<EV_CoinCollected>.Remove(AddCoin);
    }

    void Start()
    {
        Bus<EV_PlayerPossess>.Raise(new EV_PlayerPossess { value = false });
    }

    void StartGame()
    {
        Bus<EV_UiShowGameplay>.Raise();
        Bus<EV_MusicToggle>.Raise(new EV_MusicToggle { value = true });
        Tween.Delay(3f).OnComplete(() =>
            Bus<EV_PlayerPossess>.Raise(new EV_PlayerPossess { value = true }));
    }

    void AddDistance()
    {

    }

    void AddCoin()
    {
        _score += _coinScore;
        _coins += 1;
        UpdateUiStats();
    }

    void UpdateUiStats()
    {
        Bus<EV_UiStatsUpdate>.Raise(new EV_UiStatsUpdate
        {
            score = _score,
            distance = _distance,
            coins = _coins
        });
    }

    void SaveHiScore(int value)
    {

    }

    void HandlePlayerDeath()
    {
        Bus<EV_UiShowGameOver>.Raise();
    }

    void RestartGame()
    {
        
    }
}
