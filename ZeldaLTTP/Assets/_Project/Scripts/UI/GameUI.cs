using EventBus;
using UnityEngine;


public class GameUI : MonoBehaviour
{
    [SerializeField] GameObject _saveQuitMenu;
    [SerializeField] GameObject _inventory;

    private void OnEnable()
    {
        Bus<E_GameUI_ToggleMenu>.Add(ToggleSaveQuitMenu);
        Bus<E_GameUI_ToggleInventory>.Add(ToggleInventory);
    }
    private void OnDisable()
    {
        Bus<E_GameUI_ToggleMenu>.Remove(ToggleSaveQuitMenu);
        Bus<E_GameUI_ToggleInventory>.Remove(ToggleInventory);
    }

    private void ToggleSaveQuitMenu()
    {
        _saveQuitMenu.SetActive(_saveQuitMenu.activeSelf ? false : true);    
    }

    private void ToggleInventory()
    {
        if (_inventory.activeSelf)
        {
            Bus<E_Game_Pause>.Raise(new E_Game_Pause { value = false});
            _inventory.SetActive(false);
        }
        else
        {
            Bus<E_Game_Pause>.Raise(new E_Game_Pause { value = true});
            _inventory.SetActive(true);
        }
        
    }

    public void Save()
    {
        Bus<E_Game_Save>.Raise(new E_Game_Save { });
    }

    public void SaveAndQuit()
    {
        Bus<E_Game_Save>.Raise(new E_Game_Save { });
        Bus<E_Game_Restart>.Raise(new E_Game_Restart { });
    }

    public void Quit()
    {
        Bus<E_Game_Restart>.Raise(new E_Game_Restart { });
    }
}
