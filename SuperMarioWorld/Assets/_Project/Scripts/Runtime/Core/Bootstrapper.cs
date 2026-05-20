using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class Bootstrapper : MonoBehaviour
{
    const string SystemsSceneName = "Systems";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureSystemsLoaded()
    {
        var active = SceneManager.GetActiveScene();
        if (active.name == SystemsSceneName)
        {
            Debug.LogError(
                "[SMW] Don't Play from Systems.unity — it is a persistent layer, not an entry scene. Use Boot, Title, Overworld, or a Level scene.");
            return;
        }

        if (!SceneManager.GetSceneByName(SystemsSceneName).isLoaded)
        {
            SceneManager.LoadScene(SystemsSceneName, LoadSceneMode.Additive);
        }
    }
}
