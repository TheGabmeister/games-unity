using UnityEngine;

[DisallowMultipleComponent]
public class Services : MonoBehaviour
{
    public static Services Instance { get; private set; }

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
