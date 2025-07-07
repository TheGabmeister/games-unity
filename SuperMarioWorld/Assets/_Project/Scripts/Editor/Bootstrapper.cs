using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif



public static class Bootstrapper
{
    private const string EditorPrefKey = "BootstrapperEnabled";
    
    public static bool Enabled
    {
        get
        {
#if UNITY_EDITOR
            return EditorPrefs.GetBool(EditorPrefKey, true);
#else
            return true;
#endif
        }
        set
        {
#if UNITY_EDITOR
            EditorPrefs.SetBool(EditorPrefKey, value);
#endif
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
    {
        if (Enabled)
        {
            Object.DontDestroyOnLoad(Object.Instantiate(Resources.Load("Systems")));
        }
    }
}