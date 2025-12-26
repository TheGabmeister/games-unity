using ScriptableObjectArchitecture;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] IntReference _remainingTime;

    [Header("Listen to these events...")]
    [SerializeField] IntGameEvent _onInitializeTimer;
    [SerializeField] GameEvent _onPauseTimer;
    [SerializeField] GameEvent _onStartTimer;

    [Header("Call these events...")]
    //[SerializeField] IntGameEvent _onUpdateTimer;
    [SerializeField] GameEvent _onHunderedSecondsLeft;
    [SerializeField] GameEvent _onFinishedTime;


    private void OnEnable()
    {
        _onInitializeTimer.AddListener(InitializeTimer);
        _onStartTimer.AddListener(StartTimer);
        _onPauseTimer.AddListener(PauseTimer);
    }

    private void OnDisable()
    {
        _onInitializeTimer.RemoveListener(InitializeTimer);
        _onStartTimer.RemoveListener(StartTimer);
        _onPauseTimer.RemoveListener(PauseTimer);
    }

    void StartTimer()
    {
        InvokeRepeating("UpdateTime", 0.0f, 1.0f);
    }

    void InitializeTimer(int time)
    {
        _remainingTime.Value = time;
    }

    void PauseTimer()
    {
        CancelInvoke();
    }

    void UpdateTime()
    {
        _remainingTime.Value -= 1;
        if (_remainingTime.Value == 100)
        {
            _onHunderedSecondsLeft?.Raise();
        }

        if (_remainingTime.Value < 0)
        {
            _onFinishedTime?.Raise();
        }
    }
}
