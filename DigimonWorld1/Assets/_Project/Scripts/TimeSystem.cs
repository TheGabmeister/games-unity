using System;
using UnityEngine;

public class TimeSystem : Singleton<TimeSystem>
{
    [SerializeField] private int _startHour = 6;
    [SerializeField] private int _startMinute = 0;

    private float _timeAccumulator;
    private int _hour;
    private int _minute;
    private int _day;
    private bool _paused;

    public int Hour => _hour;
    public int Minute => _minute;
    public int Day => _day;
    public string TimeString => $"{_hour:D2}:{_minute:D2}";
    public bool IsPaused => _paused;

    public event Action OnHourChanged;

    protected override void Awake()
    {
        base.Awake();
        _hour = _startHour;
        _minute = _startMinute;
        _day = 1;
    }

    private void Update()
    {
        if (_paused) return;

        _timeAccumulator += Time.deltaTime;

        while (_timeAccumulator >= 1f)
        {
            _timeAccumulator -= 1f;
            _minute++;

            if (_minute >= 60)
            {
                _minute = 0;
                _hour++;
                OnHourChanged?.Invoke();

                if (_hour >= 24)
                {
                    _hour = 0;
                    _day++;
                }
            }
        }
    }

    public void SetPaused(bool paused)
    {
        _paused = paused;
    }
}
