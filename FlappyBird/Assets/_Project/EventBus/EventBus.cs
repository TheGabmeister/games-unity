using System;
using UnityEngine;

// A simple event system based on https://playable.design/a-simple-event-system-for-unity
// This approach, while simple, allocates memory on the heap. A better (but more verbose) approach
// is to allocate on the stack. Example: https://github.com/adammyhre/Unity-Event-Bus

namespace EventBus
{
    public static class Bus
    {
        
        public static readonly GameEvent GameStart = new();
        public static readonly GameEvent GameRestart = new();

        public static readonly GameEvent PlayerKilled = new();
        public static readonly GameEvent<int> EnemyKilled = new();

        public static readonly GameEvent<int> UiUpdateScore = new();
        public static readonly GameEvent UiToggleGameplay = new();
        public static readonly GameEvent UiToggleMenu = new();

        public static readonly GameEvent<AudioClip> MusicPlay = new(); 
        public static readonly GameEvent MusicPause = new(); 
        public static readonly GameEvent MusicStop = new(); 
 
        public static readonly GameEvent<AudioClip> SfxPlay = new();
    }

    public class GameEvent
    {
        private event Action action = delegate { };
        public void Publish() => action?.Invoke();
        public void Sub(Action subscriber) => action += subscriber;
        public void Unsub(Action subscriber) => action -= subscriber;
    }

    public class GameEvent<T>
    {
        private event Action<T> action;
        public void Publish(T param) => action?.Invoke(param);
        public void Sub(Action<T> subscriber) => action += subscriber;
        public void Unsub(Action<T> subscriber) => action -= subscriber;
    }
}