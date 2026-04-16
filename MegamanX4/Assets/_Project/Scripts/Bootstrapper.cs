using UnityEngine;

public static class Bootstrapper {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
    {
        var prefab = Resources.Load<GameObject>("Systems");
        if (!prefab)
        {
            Debug.LogError("Missing Resources/Systems prefab.");
            return;
        }
        Object.DontDestroyOnLoad(Object.Instantiate(prefab));
    } 
}
/*
// The addressables version
public static class Bootstrapper {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute() => Object.DontDestroyOnLoad(Addressables.InstantiateAsync("Systems").WaitForCompletion());
}

*/