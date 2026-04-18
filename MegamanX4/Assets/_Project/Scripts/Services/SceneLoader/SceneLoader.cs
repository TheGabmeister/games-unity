using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public Task LoadSceneByName(string sceneName)
    {
        if (!IsSceneInBuild(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' is not included in the build settings.");
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource<bool>();
        var op = SceneManager.LoadSceneAsync(sceneName);
        op.completed += _ => tcs.SetResult(true);
        return tcs.Task;
    }

    public Task LoadSceneByIndex(int index)
    {
        if (index < 0 || index >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"Invalid scene build index: {index}");
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource<bool>();
        var op = SceneManager.LoadSceneAsync(index);
        op.completed += _ => tcs.SetResult(true);
        return tcs.Task;
    }

    static bool IsSceneInBuild(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            var nameFromPath = System.IO.Path.GetFileNameWithoutExtension(path);
            if (string.Equals(nameFromPath, sceneName, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
