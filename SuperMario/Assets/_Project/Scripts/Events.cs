using System;

namespace EventSystem
{
    public static class Events
    {
        public static readonly GameEvent<int> TimerUpdated = new();
        public static readonly GameEvent TimerHundredSecondsLeft = new();
        public static readonly GameEvent TimerFinished = new();


        public static readonly GameEvent<LevelData> LevelDataInitialized = new();
        public static readonly GameEvent PauseToggled = new();

        public static readonly GameEvent<int> StatsScoreUpdated = new();
        public static readonly GameEvent<int> StatsLivesUpdated = new();
        public static readonly GameEvent<int> StatsCoinsUpdated = new();

        public static readonly GameEvent PickedupCoin = new();
        public static readonly GameEvent PickedupMushroom = new();
        public static readonly GameEvent PickedupFlower = new();
        public static readonly GameEvent PickedupOneUp = new();
        public static readonly GameEvent PickedupStar = new();

        public static readonly GameEvent PlayerDied = new();
        public static readonly GameEvent CheckpointPassed = new();
    }

    public class GameEvent
    {
        private event Action action = delegate { };
        public void Raise() => action?.Invoke();
        public void Sub(Action subscriber) => action += subscriber;
        public void Unsub(Action subscriber) => action -= subscriber;
    }

    public class GameEvent<T>
    {
        private event Action<T> action;
        public void Raise(T param) => action?.Invoke(param);
        public void Sub(Action<T> subscriber) => action += subscriber;
        public void Unsub(Action<T> subscriber) => action -= subscriber;
    }
}