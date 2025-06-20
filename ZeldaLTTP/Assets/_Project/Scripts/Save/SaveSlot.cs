using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EventBus;

public class SaveSlot : MonoBehaviour
{
    public static int slot = 0;
    [SerializeField] string _filename;
    [SerializeField] TMP_Text _text;
    PlayerData _playerData;
    [SerializeField] MainMenuUI _mainMenu;

    private void OnEnable()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        GetComponent<Button>().onClick.RemoveListener(OnClick);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (ES3.FileExists( _filename ) && ES3.KeyExists("playerData", _filename))
        {
            _playerData = ES3.Load<PlayerData>("playerData", _filename);
        }
    }

    private void Start()
    {
        if(_playerData != null)
        {
            _text.text = _playerData.username;
        }
    }

    void OnClick()
    {
        if (_playerData != null)
        {
            Bus<EV_GameStart>.Raise(new EV_GameStart { });
        }
        else
        {
            
            slot = transform.GetSiblingIndex();
            _mainMenu.ToggleActiveMenu(1);
        }
    }

    public void Refresh()
    {
        _playerData = ES3.Load<PlayerData>("playerData", _filename);
        _text.text = _playerData.username;
    }
}
