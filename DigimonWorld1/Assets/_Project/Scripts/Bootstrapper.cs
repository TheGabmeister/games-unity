using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class Bootstrapper
{
    private const string BootstrapSceneName = "_Bootstrap";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadBootstrapScene()
    {
        if (SceneManager.GetSceneByName(BootstrapSceneName).isLoaded) return;
        SceneManager.LoadScene(BootstrapSceneName, LoadSceneMode.Additive);
    }
}
