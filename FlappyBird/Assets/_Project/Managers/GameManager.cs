using UnityEngine;
using PrimeTween;
using EventBus;

public class GameManager : MonoBehaviour
{
    int _score = 0;
    int _highScore = 0;

    void Update()
    {
        
    }

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
        Sequence.Create()
            .ChainCallback(() => Bus.CameraFadeToBlack.Publish(0.5f))
            .ChainDelay(0.5f)
            .ChainCallback(() => Bus.UiToggleGameplay.Publish())
            .ChainCallback(() => Bus.CameraFadeToClear.Publish(0.5f))
        ;
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
