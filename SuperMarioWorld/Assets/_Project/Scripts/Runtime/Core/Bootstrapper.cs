using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class Bootstrapper : MonoBehaviour
{
    public const string SystemsSceneName = "Systems";
    public const string BootSceneName = "Boot";
    public const string TitleSceneName = "Title";
    public const string OverworldSceneName = "Overworld";

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

    private void Start()
    {
        if (GameStateMachine.Instance != null)
        {
            GameStateMachine.Instance.TransitionToTitle();
        }
    }
}
