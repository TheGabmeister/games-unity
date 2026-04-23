using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class Bootstrapper
{
    private const string BootstrapSceneName = "_Bootstrap";
    private const string GameplayBootstrapSceneName = "_GameplayBootstrap";
    private const string GameplaySceneName = "_Gameplay";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadBootstrapScene()
    {
        if (SceneManager.GetSceneByName(BootstrapSceneName).isLoaded) return;
        SceneManager.LoadScene(BootstrapSceneName, LoadSceneMode.Additive);

        if (SceneManager.GetActiveScene().name == GameplaySceneName)
            SceneManager.LoadScene(GameplayBootstrapSceneName, LoadSceneMode.Additive);
    }
}
