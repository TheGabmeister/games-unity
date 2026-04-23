using System;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    public event Action OnSceneLoadStarted;
    public event Action OnSceneLoadCompleted;

    public async Awaitable LoadScene(SceneReference scene)
    {
        OnSceneLoadStarted?.Invoke();
        UnloadNonBootstrapScenes();
        await LoadSceneAsync(scene);
        OnSceneLoadCompleted?.Invoke();
    }

    public async Awaitable LoadScenes(params SceneReference[] scenes)
    {
        OnSceneLoadStarted?.Invoke();
        UnloadNonBootstrapScenes();
        foreach (var scene in scenes)
            await LoadSceneAsync(scene);
        OnSceneLoadCompleted?.Invoke();
    }

    private void UnloadNonBootstrapScenes()
    {
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene loaded = SceneManager.GetSceneAt(i);
            if (loaded.buildIndex != 0)
                SceneManager.UnloadSceneAsync(loaded);
        }
    }

    private async Awaitable LoadSceneAsync(SceneReference scene)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(scene.Path, LoadSceneMode.Additive);
        while (!op.isDone)
            await Awaitable.NextFrameAsync();
    }
}
