using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadBootstrapScene()
    {
        BootstrapConfig config = Resources.Load<BootstrapConfig>("BootstrapConfig");

        if (SceneManager.GetSceneByPath(config.BootstrapScenePath).isLoaded) return;
        SceneManager.LoadScene(config.BootstrapScenePath, LoadSceneMode.Additive);

        if (SceneManager.GetActiveScene().path == config.GameplayScenePath)
            SceneManager.LoadScene(config.GameplayBootstrapScenePath, LoadSceneMode.Additive);
    }
}
