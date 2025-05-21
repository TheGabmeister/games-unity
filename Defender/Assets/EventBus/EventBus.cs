using System;
using UnityEngine;

// A simple event system based on https://playable.design/a-simple-event-system-for-unity
// This approach, while simple, allocates memory on the heap. A better (but more verbose) approach
// is to allocate on the stack. Example: https://github.com/adammyhre/Unity-Event-Bus

namespace EventBus
{
    public static class Bus
    {
        // When there are too many events, you need to start categorizing each event to a separate class as shown in the example at the end.
        // An alternative approach is to use robust naming scheme (Ex. Player_Jumped) and use partial classes.

        public static readonly GameEvent OnJump = new();
        public static readonly GameEvent<float> OnDamage = new();
        public static readonly GameEvent<GameObject> OnPickup = new();

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