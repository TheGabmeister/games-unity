using EventBus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using PrimeTween;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] GameObject[] _menuButtons;
    [SerializeField] GameObject[] _menus;
    [SerializeField] GameObject _arrow;
    [SerializeField] TMP_Text _inputName;
    [SerializeField] SaveSlot[] _saveSlots;
    int _currentSaveIndex = 0;
    private bool _isAwaitingStartInput = true;

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
        RefreshSaveSlots();
    }

    public void SetCurrentSaveIndex(int i)
    {
        _currentSaveIndex = i;
    }
    
    public void RefreshSaveSlots()
    {
        for (int i = 0; i < _saveSlots.Length; i++)
        {
            if (SaveManager.DoesSaveExist(i))
            {
                var data = SaveManager.LoadData(i);
                _saveSlots[i].PopulateSlot(data);
            }
        }
    }

    public void OnControlsChanged()
    {
        // Make sure this script's Awake is called first before other script attempt to call this.
        if (!_isInitialized) return;

        //ResetSelectedButton();
    }

    public void StartGame(PlayerData d, int i)
    {
        // Play sound
        // Fade to black
        Bus<EV_GameStart>.Raise(new EV_GameStart { data = d, saveSlot = i});
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
        SaveManager.CreateSave(_currentSaveIndex, _inputName.text);
        _inputName.text = "";
        RefreshSaveSlots();
        ToggleActiveMenu(1);
    }
    
    public void OnSubmit()
    {
        if (_isAwaitingStartInput)
        {
            Sequence.Create()
                .ChainCallback(() => Bus<EV_ScreenFadeToBlack>.Raise(new EV_ScreenFadeToBlack { duration = 0.5f }))
                .ChainDelay(0.5f)
                .ChainCallback(() => {
                    ToggleActiveMenu(1);
                })
                .ChainCallback(() => Bus<EV_ScreenFadeToClear>.Raise(new EV_ScreenFadeToClear { duration = 0.5f }));
            _isAwaitingStartInput = false;
        }
    }
}
