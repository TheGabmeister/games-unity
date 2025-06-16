using UnityEngine;
using PrimeTween;

public class GameManager : MonoBehaviour
{
    int _score = 0;
    int _distance = 0;
    int _coins = 0;
    int _coinScore = 10;
    int _highScore = 0;
  
    [SerializeField] GameObject _playerPrefab;

    void Start()
    {

    }

    void StartGame()
    {

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

    }

    void SaveHiScore(int value)
    {

    }

    void HandlePlayerDeath()
    {
        
    }

    void RestartGame()
    {
        
    }
}
