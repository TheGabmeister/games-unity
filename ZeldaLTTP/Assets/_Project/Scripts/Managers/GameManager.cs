using UnityEngine;
using EventBus;
using UnityEngine.SceneManagement;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject _playerPrefab;

    GameObject _player;
    [SerializeField] GameObject[] _prefabs;

    PlayerData _playerData;

    private void OnEnable()
    {
        Bus<E_Game_Restart>.Add(RestartGame);
        Bus<E_Game_Save>.Add(SaveGame);
        Bus<E_Game_Load>.Add(LoadGame);
        Bus<E_Game_New>.Add(NewGame);
        Bus<E_Game_Pause>.Add(PauseGame);
        Bus<E_Scene_SetCurrentScene>.Add(SetCurrentScene);
    }
    private void OnDisable()
    {
        Bus<E_Game_Restart>.Remove(RestartGame);
        Bus<E_Game_Save>.Remove(SaveGame);
        Bus<E_Game_Load>.Remove(LoadGame);
        Bus<E_Game_New>.Remove(NewGame);
        Bus<E_Game_Pause>.Remove(PauseGame);
        Bus<E_Scene_SetCurrentScene>.Remove(SetCurrentScene);
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu") return;
        
        SpawnGamePrefabs(default, default);
    }

    void RestartGame()
    {
        // play sound
        // fade to black
        if (_player) Destroy(_player);

        Bus<E_Scene_Load>.Raise(new E_Scene_Load { value = "MainMenu"});
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
        Bus<E_Scene_Load>.Raise(new E_Scene_Load { value = _playerData.currentScene});
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

        if (_prefabs.Length != 0)
        {
            foreach (GameObject obj in _prefabs)
            {
                
                Instantiate(obj, transform.position, Quaternion.identity);
            }
        }
        else
        {
            Debug.Log("The array is empty or not assigned. Please assign GameObjects to the array.");
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

    void SetCurrentScene(E_Scene_SetCurrentScene message)
    {
        _playerData.currentScene = message.value;
    }

    void PauseGame(E_Game_Pause message)
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
