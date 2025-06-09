using UnityEngine;

namespace EventBus
{ 
    public struct EV_PlayerDied : IEvent { }


    // GameManager Events 
    public struct EV_GameStart : IEvent { }
    public struct EV_GameRestart : IEvent { }

    // MusicManager Events
    public struct EV_MusicChange : IEvent { public AudioClip value; }
    public struct EV_MusicTogglePause : IEvent { public bool value; }
    public struct EV_MusicTogglePlay : IEvent { public bool value; }

    // Sound Effects Manager
    public struct EV_SfxPlay : IEvent { public AudioClip value; }


}
