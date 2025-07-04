using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextScene : MonoBehaviour
{
    [SerializeField] string _nextScene;
    
    void Start()
    {
        Invoke(nameof(LoadMainScene), 1f);
    }

    void LoadMainScene()
    {
        SceneManager.LoadScene(_nextScene);
    }
}
