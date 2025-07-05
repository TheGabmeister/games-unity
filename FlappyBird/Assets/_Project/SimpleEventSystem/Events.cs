using System;
using UnityEngine;

namespace SimpleEventSystem
{
    public static class Events
    {
        public static readonly GameEvent GameStart = new();
        public static readonly GameEvent GameRestart = new();


        public static readonly GameEvent PlayerDied = new();
        public static readonly GameEvent CheckpointPassed = new();


        public static readonly GameEvent<AudioClip> MusicPlay = new(); 
        public static readonly GameEvent MusicPause = new(); 
        public static readonly GameEvent MusicStop = new(); 
 
        public static readonly GameEvent<AudioClip> SfxPlay = new();
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