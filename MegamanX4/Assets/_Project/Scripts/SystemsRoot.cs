using UnityEngine;

[DisallowMultipleComponent]
public class SystemsRoot : MonoBehaviour
{
    public static SystemsRoot Instance { get; private set; }

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Debug.LogWarning("Duplicate Systems root detected. Destroying the newer instance.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
