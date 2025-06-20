using EventBus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] GameObject[] _menuButtons;
    [SerializeField] GameObject[] _menus;
    [SerializeField] GameObject _arrow;
    [SerializeField] TMP_Text _inputName;
    [SerializeField] SaveSlot[] _saveSlots;
    internal int _currentSaveSlot { private get; set; } = 0;

    private PlayerInput _input;
    bool _isInitialized = false;
    GameObject _currentSelectedButton;

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _isInitialized = true;
    }

    private void Start()
    {
        ResetSelectedButton();
        GetSaveData();
    }

    public void OnControlsChanged()
    {
        // Make sure this script's Awake is called first before other script attempt to call this.
        if (!_isInitialized) return;

        ResetSelectedButton();
    }

    private void LateUpdate()
    {
        if (_input.currentControlScheme == "Gamepad")
        {
            // Proceed only if the select button changes
            if (EventSystem.current.currentSelectedGameObject == _currentSelectedButton) return;

            // Place cursor to the left of the UI element
            var transform = EventSystem.current.currentSelectedGameObject.transform;
            var width = EventSystem.current.currentSelectedGameObject.GetComponent<RectTransform>().rect.width;
            _arrow.transform.position = new Vector2(transform.position.x - width / 2 - 100, transform.position.y);

            _currentSelectedButton = EventSystem.current.currentSelectedGameObject;
        }
    }

    void ResetSelectedButton()
    {
        if (_input.currentControlScheme == "Gamepad")
        {
            _arrow.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_menuButtons[0]);
            _currentSelectedButton = _menuButtons[0];
            _arrow.transform.position = _menuButtons[0].transform.position;
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            _arrow.SetActive(false);
            _currentSelectedButton = null;
        }
    }

    public void LoadGame()
    {
        Bus<E_Game_Load>.Raise(new E_Game_Load { });
    }
    public void NewGame()
    {
        Bus<E_Game_New>.Raise(new E_Game_New { });
    }

    public void SelectPlayerSlot(int num)
    {
        if (num == 0)
        {
            ToggleActiveMenu(1);
        }
        else
        {
            // start game with active save file
        }
    }

    public void ToggleActiveMenu(int num)
    {
        foreach (var obj in _menus)
        {
            obj.SetActive(false);
        }
        _menus[num].SetActive(true);
    }

    void GetSaveData()
    {

    }

    public void AppendLabel(string c)
    {
        
        _inputName.text = string.Concat(_inputName.text, c);
    }

    public void BackspaceLabel()
    {
        if (_inputName.text.Length > 0)
            _inputName.text = _inputName.text.Remove(_inputName.text.Length - 1);
    }

    public void CreateNewSave()
    {
        string fileName = "SaveFile" + (SaveSlot.slot + 1) + ".es3";
        var playerData = new PlayerData();
        playerData.username = _inputName.text;
        ES3.Save("playerData", playerData, fileName);
        ToggleActiveMenu(0);
        _saveSlots[SaveSlot.slot].Refresh();
        _inputName.text = "";
    }
}
