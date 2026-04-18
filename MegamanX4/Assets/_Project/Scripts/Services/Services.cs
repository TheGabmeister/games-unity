using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Services : MonoBehaviour
{
    readonly Dictionary<Type, object> _services = new();

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
        RegisterKnownServices();
    }

    public bool Has<T>() where T : class => _services.ContainsKey(typeof(T));

    public T Get<T>() where T : class
    {
        if (TryResolve(out T service))
            return service;

        Debug.LogError($"Required service '{typeof(T).FullName}' is not registered.", this);
        return null;
    }

    public static bool TryGet<T>(out T service) where T : class
    {
        var instance = Instance;
        if (!instance)
        {
            service = null;
            return false;
        }

        return instance.TryResolve(out service);
    }

    public void Register<T>(T service) where T : class
    {
        if (service == null)
        {
            Debug.LogError($"Cannot register null for service contract '{typeof(T).FullName}'.", this);
            return;
        }

        var contract = typeof(T);
        if (_services.TryGetValue(contract, out var existing))
        {
            if (ReferenceEquals(existing, service))
                return;

            Debug.LogError($"Duplicate registration for service contract '{contract.FullName}'. Keeping the original instance.", this);
            return;
        }

        _services.Add(contract, service);
    }

    public void Unregister<T>(T service) where T : class
    {
        if (service == null)
            return;

        var contract = typeof(T);
        if (!_services.TryGetValue(contract, out var existing))
            return;

        if (!ReferenceEquals(existing, service))
        {
            Debug.LogWarning($"Ignoring unregister for service contract '{contract.FullName}' because it does not match the active instance.", this);
            return;
        }

        _services.Remove(contract);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        _services.Clear();
    }

    bool TryResolve<T>(out T service) where T : class
    {
        if (_services.TryGetValue(typeof(T), out var existing) && existing is T typedService)
        {
            service = typedService;
            return true;
        }

        service = null;
        return false;
    }

    void RegisterKnownServices()
    {
        RegisterService<CheckpointService, ICheckpointService>(GetComponentInChildren<CheckpointService>());
        RegisterService<MusicManager, IMusicService>(GetComponentInChildren<MusicManager>(true));
        RegisterService<SfxManager, ISfxService>(GetComponentInChildren<SfxManager>(true));
        RegisterService<GameStateController, GameStateController>(GetComponentInChildren<GameStateController>(true));
    }

    void RegisterService<TConcrete, TContract>(TConcrete service)
        where TConcrete : class, TContract
        where TContract : class
    {
        if (service == null)
            return;

        Register<TConcrete>(service);

        if (typeof(TConcrete) != typeof(TContract))
            Register<TContract>(service);
    }
}
