using UnityEngine;
using EventBus;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
    [SerializeField] GameObject _playerPrefab;

    GameObject _player;
    GameState _state;
    [SerializeField] SceneLoader _sceneLoader;

    PlayerData _playerData;
    [SerializeField] SceneDictionarySO _sceneDict;
    
    private void OnEnable()
    {
        Bus<EV_GameRestart>.Add(RestartGame);
        Bus<EV_GameSave>.Add(SaveGame);
        Bus<EV_GameLoad>.Add(LoadGame);
        Bus<EV_GameNew>.Add(NewGame);
        Bus<EV_GamePause>.Add(PauseGame);
        Bus<EV_SceneSetCurrent>.Add(SetCurrentScene);
        Bus<EV_GameStateChange>.Add(ChangeState);
    }
    private void OnDisable()
    {
        Bus<EV_GameRestart>.Remove(RestartGame);
        Bus<EV_GameSave>.Remove(SaveGame);
        Bus<EV_GameLoad>.Remove(LoadGame);
        Bus<EV_GameNew>.Remove(NewGame);
        Bus<EV_GamePause>.Remove(PauseGame);
        Bus<EV_SceneSetCurrent>.Remove(SetCurrentScene);
        Bus<EV_GameStateChange>.Add(ChangeState);
    }

    void Start()
    {
#if UNITY_EDITOR
        if (IsGameplayScene())
        {
            SpawnPlayer(Utils.GetPlayerStart());
        }
#else
        
#endif

    }

    void ChangeState(EV_GameStateChange e)
    {
        switch (e.state)
        {
            case GameState.Bootup: 
                break;
            case GameState.StartMenu:
                _state = GameState.StartMenu;
                _sceneLoader.LoadSceneByIndex(1);
                break;
            case GameState.Gameplay:
                // Add your gameplay state handling logic here
                break;
            // Add other cases as needed


            // case GameState.GameOver:
            //     break;
            default:
                Debug.Log("Unhandled game state: " + e.state);
                break;
        }


    }
    
    bool IsGameplayScene()
    {
        // Check if starting scene is SceneType.Gameplay
        // Because the type SceneReference cannot be checked for equality, we instead
        // iterate over the names of each SceneReference and compare that.
        string startScene = SceneManager.GetActiveScene().name;
        foreach (var entry in _sceneDict.scenes)
        {
            if (entry.Key.Name == startScene)
            {
                return entry.Value == SceneType.Gameplay;
            }
        }
        return false;
    }

    void SpawnPlayer(Vector3 pos)
    {
        Instantiate(_playerPrefab, pos, Quaternion.identity);
    }
    
    void RestartGame()
    {
        // play sound
        // fade to black
        if (_player) Destroy(_player);

        Bus<EV_SceneLoad>.Raise(new EV_SceneLoad { value = "MainMenu"});
    }

    void NewGame()
    {
        _playerData.currentScene = "Scene01";
        _playerData.position = new Vector2(0, 0);
        StartGame();
    }

    void StartGame()
    {
        SceneManager.sceneLoaded += SpawnGamePrefabs;
        Bus<EV_SceneLoad>.Raise(new EV_SceneLoad { value = _playerData.currentScene});
    }

    void SpawnGamePrefabs(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= SpawnGamePrefabs;
        if (_playerPrefab)
        {
            var playerStart = GameObject.FindGameObjectWithTag("PlayerStart");
            if(playerStart) 
                _player = Instantiate(_playerPrefab, playerStart.transform.position, Quaternion.identity);
            else
            {
                if(_playerData != null)
                {
                    _player = Instantiate(_playerPrefab, _playerData.position, Quaternion.identity);
                }
                else
                {
                    Debug.LogError("No save data available!");
                }
            }
                
        }
    }

    void SaveGame()
    {
        foreach (Transform transform in _player.transform)
        {
            if (transform.CompareTag("Player"))
            {
                _playerData.position = new Vector2 (transform.position.x, transform.position.y);
                break;
            }
        }

        _playerData.currentScene = SceneManager.GetActiveScene().name;
        ES3.Save("playerData", _playerData);
    }

    void LoadGame()
    {
        _playerData = ES3.Load<PlayerData>("playerData");
        StartGame();
    }

    void SetCurrentScene(EV_SceneSetCurrent message)
    {
        _playerData.currentScene = message.value;
    }

    void PauseGame(EV_GamePause message)
    {
        if (message.value)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }
}

public enum GameState
{
    Bootup,
    StartMenu,
    Gameplay,
    GameOver
}