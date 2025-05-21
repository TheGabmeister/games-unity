using UnityEngine;
using EventBus;

public class GameManager : MonoBehaviour
{
    int _score = 0;

    void OnEnable()
    {
        Bus.EnemyKilled.Sub(AddScore);
    }

    void OnDisable()
    {
        Bus.EnemyKilled.Unsub(AddScore);
    }

    void AddScore(int value)
    {
        _score += value;
        Bus.UiUpdateScore.Publish(_score);
    }
}
