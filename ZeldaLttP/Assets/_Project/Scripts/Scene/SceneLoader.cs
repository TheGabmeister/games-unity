using UnityEngine;
using UnityEngine.SceneManagement;
using PrimeTween;

public class SceneLoader : PersistentSingleton<SceneLoader>
{
    string _currentScene;
    string _nextScene;
    bool _isLoading = false;

    private void Start()
    {
        _currentScene = SceneManager.GetActiveScene().name;
    }

    // Loads the new scene. Afterwards, unloads the previous scene.
    void SwitchScene(string sceneName)
    {
        if (_isLoading) 
        {
            Debug.Log("Not yet finished loading a scene!");
            return;
        }

        _isLoading = true;
        _nextScene = sceneName;
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

    void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        _currentScene = sceneName;
    }
}
