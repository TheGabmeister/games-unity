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
        StartCoroutine(LoadSceneAsyncByName(name));
    }
    
    IEnumerator LoadSceneAsyncByIndex(int index)
    {
s
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
}