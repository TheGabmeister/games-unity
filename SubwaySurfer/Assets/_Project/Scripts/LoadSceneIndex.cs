using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneIndex : MonoBehaviour
{
    [SerializeField] private int _index = 1;
    
    void Start()
    {
        StartCoroutine(LoadSceneAsynchronously());
    }
    
    private IEnumerator LoadSceneAsynchronously()
    {
        if (_index >= 0 && _index < SceneManager.sceneCountInBuildSettings)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_index);
            
            // You can monitor the loading progress
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                Debug.Log($"Loading progress: {progress * 100}%");
                
                // Yield control back to Unity until next frame
                yield return null;
            }
        }
        else
        {
            Debug.LogError($"Invalid scene build index: {_index}");
        }
    }

}