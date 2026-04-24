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

        string activeScenePath = SceneManager.GetActiveScene().path;

        if (activeScenePath == config.GameplayScenePath || activeScenePath.Contains("/Zones/"))
            SceneManager.LoadScene(config.GameplayScenePath, LoadSceneMode.Additive);
    }
}
