using UnityEngine;
using UnityEngine.SceneManagement;
using EventBus;
using PrimeTween;

public class LevelStreamer : MonoBehaviour
{
    string _currentScene;
    string _nextScene;
    bool _isLoading = false;

    private void OnEnable()
    {
        Bus<E_Scene_Switch>.Add(SwitchScene);
        Bus<E_Scene_Load>.Add(LoadScene);
    }
    private void OnDisable()
    {
        Bus<E_Scene_Switch>.Remove(SwitchScene);
        Bus<E_Scene_Load>.Remove(LoadScene);
    }

    private void Start()
    {
        _currentScene = SceneManager.GetActiveScene().name;
    }

    // Loads the new scene. Afterwards, unloads the previous scene.
    void SwitchScene(E_Scene_Switch message)
    {
        if (_isLoading) 
        {
            Debug.Log("Not yet finished loading a scene!");
            return;
        }

        _isLoading = true;
        _nextScene = message.value;
        SceneManager.sceneLoaded += BeginUnloadingScene;
        SceneManager.LoadScene(_nextScene, LoadSceneMode.Additive);
    }

    void BeginUnloadingScene(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= BeginUnloadingScene;
        SceneManager.UnloadSceneAsync(_currentScene);
        _currentScene = _nextScene;
        _nextScene = null;
        _isLoading = false;
    }

    void LoadScene(E_Scene_Load message)
    {
        SceneManager.LoadScene(message.value);
        _currentScene = message.value;
    }
}
