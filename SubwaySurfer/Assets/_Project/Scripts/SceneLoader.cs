using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByIndex(int index)
    {
        if (index >= 0 && index < SceneManager.sceneCountInBuildSettings)
        {
            StartCoroutine(LoadSceneAsyncByIndex(index));
        }
        else
        {
            Debug.LogError($"Invalid scene build index: {index}");
        }
    }
    
    public void LoadSceneByName(string name)
    {
        if (IsSceneInBuild(name))
        {
            StartCoroutine(LoadSceneAsyncByName(name));
        }
        else
        {
            Debug.LogError($"Scene '{name}' is not included in the build settings.");
        }

    }
    
    IEnumerator LoadSceneAsyncByIndex(int index)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(index);
            
        // You can monitor the loading progress
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            Debug.Log($"Loading progress: {progress * 100}%");
                
            // Yield control back to Unity until next frame
            yield return null;
        }
    }

    IEnumerator LoadSceneAsyncByName(string name)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name);
        yield return null;
    }
    
    private bool IsSceneInBuild(string sceneName)
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        
            if (string.Equals(sceneNameFromPath, sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
    
        return false;
    }
}