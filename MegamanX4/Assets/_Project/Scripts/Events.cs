using System;
using UnityEngine;


    public static class GameStateEvents
    {
        public static readonly GameEvent<GameState> SetState = new();

    }

    public static class PlayerEvents
    {
        public static readonly GameEvent Died = new();
        public static readonly GameEvent CheckpointPassed = new();
    }

    public static class MusicEvents
    {
        public static readonly GameEvent<AudioClip> Play = new();
        public static readonly GameEvent Pause = new();
        public static readonly GameEvent Stop = new();
    }

    public static class SfxEvents
    {
        public static readonly GameEvent<AudioClip> Play = new();
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


/* Example usage

using SimpleEventSystem;

private void OnEnable()
{
    Events.PauseToggled.Sub(OnPauseToggled);
}
private void OnDisable()
{
    Events.PauseToggled.Unsub(OnPauseToggled);
}

private void Start()
{
    MusicEvents.Play.Raise(audioClip);
}
*/