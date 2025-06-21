using UnityEngine;
using UnityEditor;
using EventBus;
using UnityEngine.SceneManagement;
using Eflatun.SceneReference;

public class GameDirector : MonoBehaviour
{
    [SerializeField] GameObject _playerPrefab;

    GameObject _player;
    [SerializeField] GameObject[] _prefabs;

    PlayerData _playerData;
    [SerializeField] SceneData _sceneData;

    private void OnEnable()
    {
        Bus<EV_GameRestart>.Add(RestartGame);
        Bus<EV_GameSave>.Add(SaveGame);
        Bus<EV_GameLoad>.Add(LoadGame);
        Bus<EV_GameNew>.Add(NewGame);
        Bus<EV_GamePause>.Add(PauseGame);
        Bus<EV_SceneSetCurrent>.Add(SetCurrentScene);
    }
    private void OnDisable()
    {
        Bus<EV_GameRestart>.Remove(RestartGame);
        Bus<EV_GameSave>.Remove(SaveGame);
        Bus<EV_GameLoad>.Remove(LoadGame);
        Bus<EV_GameNew>.Remove(NewGame);
        Bus<EV_GamePause>.Remove(PauseGame);
        Bus<EV_SceneSetCurrent>.Remove(SetCurrentScene);
    }

    void Start()
    {
        var testScene = new SceneReference(SceneManager.GetActiveScene().path);
        
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName == "MainMenu" || activeSceneName == "SplashScreen") return;
        Debug.Log(SceneView.lastActiveSceneView.camera.transform.position);

        
        //SpawnGamePrefabs(default, default);
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
