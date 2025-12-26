using UnityEngine;


/// <summary>
/// A generic implementation of the Singleton design pattern. 
/// </summary>
/// <typeparam name="T">The type of the MonoBehaviour that should be a Singleton.</typeparam>
public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            // If the singleton instance does not exist, try to find it in the scene
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();

                // Create a new GameObject with the Type T if it does not exist
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    _instance = singletonObject.AddComponent<T>();
                        
                    // Name the singleton instance for the Type
                    singletonObject.name = typeof(T).ToString();
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}
