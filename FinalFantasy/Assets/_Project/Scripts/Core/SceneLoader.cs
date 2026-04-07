using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string CurrentSceneName { get; private set; }

    public async Awaitable LoadScene(string sceneName)
    {
        if (FadeOverlay.Instance != null)
            await FadeOverlay.Instance.FadeOut(0.3f);

        await SceneManager.LoadSceneAsync(sceneName);
        CurrentSceneName = sceneName;

        if (FadeOverlay.Instance != null)
            await FadeOverlay.Instance.FadeIn(0.3f);
    }

    public async Awaitable LoadSceneAdditive(string sceneName)
    {
        await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    public async Awaitable UnloadScene(string sceneName)
    {
        await SceneManager.UnloadSceneAsync(sceneName);
    }
}
