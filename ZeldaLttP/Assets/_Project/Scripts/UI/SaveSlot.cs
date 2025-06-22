using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EventBus;

public class SaveSlot : MonoBehaviour
{
    public static int slot = 0;
    [SerializeField] TMP_Text _username;
    private PlayerData _playerData;
    [SerializeField] MainMenuUI _mainMenu;
    Button _button;

    void Awake()
    {
        _button = GetComponent<Button>();
    }

    void OnEnable()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    
    
    public void PopulateSlot(PlayerData data)
    {
        _playerData = data;
        _username.text = data.username;
    }

    private void Start()
    {
        // if(_playerData != null)
        // {
        //     _text.text = _playerData.username;
        // }
    }

    void OnClick()
    {
        if (_playerData != null)
        {
            //Bus<EV_GameStart>.Raise(new EV_GameStart { });
        }
        else
        {
            
            slot = transform.GetSiblingIndex();
            _mainMenu.ToggleActiveMenu(2);
        }
    }
}
