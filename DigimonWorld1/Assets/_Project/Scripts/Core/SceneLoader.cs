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
        AsyncOperation op = SceneManager.LoadSceneAsync(scene.Path, LoadSceneMode.Additive);
        while (!op.isDone)
            await Awaitable.NextFrameAsync();
        OnSceneLoadCompleted?.Invoke();
    }

    public async Awaitable UnloadScene(SceneReference scene)
    {
        Scene loaded = SceneManager.GetSceneByPath(scene.Path);
        if (loaded.isLoaded)
        {
            AsyncOperation op = SceneManager.UnloadSceneAsync(loaded);
            while (!op.isDone)
                await Awaitable.NextFrameAsync();
        }
    }
}
