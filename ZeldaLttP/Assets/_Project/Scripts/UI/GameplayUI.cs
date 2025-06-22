using EventBus;
using UnityEngine;


public class GameplayUI : MonoBehaviour
{
    [SerializeField] GameObject _saveQuitMenu;
    [SerializeField] GameObject _inventory;

    private void OnEnable()
    {
        Bus<EV_UIToggleMenu>.Add(ToggleSaveQuitMenu);
        Bus<EV_UIToggleInventory>.Add(ToggleInventory);
    }
    private void OnDisable()
    {
        Bus<EV_UIToggleMenu>.Remove(ToggleSaveQuitMenu);
        Bus<EV_UIToggleInventory>.Remove(ToggleInventory);
    }

    private void ToggleSaveQuitMenu()
    {
        _saveQuitMenu.SetActive(_saveQuitMenu.activeSelf ? false : true);    
    }

    public void ToggleInventory()
    {
        if (_inventory.activeSelf)
        {
            Bus<EV_GamePause>.Raise(new EV_GamePause { value = false});
            _inventory.SetActive(false);
        }
        else
        {
            Bus<EV_GamePause>.Raise(new EV_GamePause { value = true});
            _inventory.SetActive(true);
        }
        
    }

    public void Save()
    {
        Bus<EV_GameSave>.Raise(new EV_GameSave { });
    }

    public void SaveAndQuit()
    {
        Bus<EV_GameSave>.Raise(new EV_GameSave { });
        Bus<EV_GameRestart>.Raise(new EV_GameRestart { });
    }

    public void Quit()
    {
        Bus<EV_GameRestart>.Raise(new EV_GameRestart { });
    }
}
