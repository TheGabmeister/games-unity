using UnityEngine;

namespace EventBus
{ 
    // Player Events
    public struct EV_PlayerSpawned : IEvent { }
    public struct EV_PlayerDied : IEvent { }
    public struct EV_PlayerDamaged : IEvent { public int value; }

    // Enemy Events
    public struct EV_EnemySpawned : IEvent { public EnemySO value; }
    public struct EV_EnemyDied : IEvent { public EnemySO value; }

    // GameManager Events 
    public struct EV_GameStart : IEvent { }
    public struct EV_GameRestart : IEvent { }
    public struct EV_GameSave : IEvent { }
    public struct EV_GameLoad : IEvent { }
    public struct EV_GameNew : IEvent { }
    public struct EV_GamePause : IEvent { public bool value; }

    // SceneManager Events 
    public struct EV_SceneSwitch : IEvent { public string value; }
    public struct EV_SceneLoad : IEvent { public string value; }
    public struct EV_SceneSetCurrent : IEvent { public string value; }

    // InventoryManager Events
    public struct EV_InventoryAdd : IEvent { public ItemDataSO value; public int amount; }

    // GameUI Events
    public struct EV_UIToggleMenu : IEvent { }
    public struct EV_UIToggleInventory : IEvent { }

    // MusicManager Events
    public struct EV_MusicChange : IEvent { public AudioClip value; }
    public struct EV_MusicTogglePause : IEvent { public bool value; }
    public struct EV_MusicPlay : IEvent { public bool value; }

    // Sound Effects Manager
    public struct EV_SfxPlay : IEvent { public AudioClip value; }

    // Item Events
    public struct EV_ItemObtained : IEvent { public ItemSO value; }
    public struct EV_ItemUsed : IEvent { public ItemSO value; }

}
