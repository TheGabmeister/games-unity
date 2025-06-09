using UnityEngine;

namespace EventBus
{ 
    public struct EV_PlayerEnableCam : IEvent { }
    public struct EV_PlayerDied : IEvent { }


    public struct EV_GameStart : IEvent { }
    public struct EV_GameRestart : IEvent { }
    public struct EV_CoinCollected : IEvent { }

    public struct EV_UiStatsUpdate : IEvent { public int score, distance, coins; }

    public struct EV_UiShowStart : IEvent { }
    public struct EV_UiShowGameplay : IEvent { }
    public struct EV_UiShowGameOver : IEvent { }

    public struct EV_MusicToggle : IEvent { public bool value; }

    public struct EV_SfxPlay : IEvent { public AudioClip clip; }


}
