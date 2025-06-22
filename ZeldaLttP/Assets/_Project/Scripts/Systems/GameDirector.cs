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
        Bus<EV_GamePause>.Add(PauseGame);
        Bus<EV_GameStateChange>.Add(ChangeState);
    }
    private void OnDisable()
    {
        Bus<EV_GameRestart>.Remove(RestartGame);
        Bus<EV_GamePause>.Remove(PauseGame);
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
            case GameState.MainMenu:
                _state = GameState.MainMenu;
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

    void PauseGame(EV_GamePause e)
    {
        if (e.value)
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
    MainMenu,
    Gameplay,
    GameOver
}