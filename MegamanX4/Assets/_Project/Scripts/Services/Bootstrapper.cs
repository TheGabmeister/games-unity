using UnityEngine;

public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
    {
        if (Services.Instance || Object.FindFirstObjectByType<Services>())
            return;

        var prefab = Resources.Load<GameObject>("GameServices");
        if (!prefab)
        {
            Debug.LogError("Missing Resources/GameServices prefab.");
            return;
        }

        Object.Instantiate(prefab);
    }
}
/*
// The addressables version
public static class Bootstrapper {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute() => Object.DontDestroyOnLoad(Addressables.InstantiateAsync("Systems").WaitForCompletion());
}

*/
