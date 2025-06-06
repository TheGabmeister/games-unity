using UnityEngine;
using PrimeTween;
using EventBus;

public class GameManager : MonoBehaviour
{
    int _score = 0;
    int _highScore = 0;

    void OnEnable()
    {
        Bus.EnemyKilled.Sub(AddScore);
        Bus.GameStart.Sub(StartGame);
    }

    void OnDisable()
    {
        Bus.EnemyKilled.Unsub(AddScore);
        Bus.GameStart.Unsub(StartGame);
    }

    void Awake()
    {
        // load high score
    }

    void StartGame()
    {
        Bus.CameraSetAlpha.Publish(1.0f);
        Bus.UiToggleGameplay.Publish();
    }

    void AddScore(int value)
    {
        _score += value;
        Bus.UiUpdateScore.Publish(_score);
    }

    void SaveHiScore(int value)
    {

    }


}
