using System.Collections.Generic;
using UnityEngine;

public class MegaNest : MonoBehaviour
{
    [SerializeField] GameObject _spawnPrefab;
    [SerializeField] float _spawnInterval = 3f;
    [SerializeField] int _maxActive = 3;
    [SerializeField] float _initialDelay = 1.5f;

    readonly List<GameObject> _active = new();
    float _cooldownTimer;

    void Awake() => _cooldownTimer = _initialDelay;

    void Update()
    {
        _active.RemoveAll(go => !go);

        _cooldownTimer -= Time.deltaTime;
        if (_cooldownTimer > 0f) return;

        if (_active.Count < _maxActive) Spawn();

        _cooldownTimer = _spawnInterval;
    }

    void Spawn()
    {
        if (!_spawnPrefab) return;
        Quaternion rotation = Random.value > 0.5f ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
        var child = Instantiate(_spawnPrefab, transform.position, rotation);
        _active.Add(child);
    }
}
