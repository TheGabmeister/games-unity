using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityServiceLocator;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject _playerPrefab;
    [SerializeField] LevelData[] _levelData;
    private int _timeRemaining = 10;

    public event Action<int> TimerUpdated;
    
    private void OnEnable()
    {

    }
    private void OnDisable()
    {

    }

    void Awake()
    {
        ServiceLocator.Global.Register<GameManager>(this);
    }
    
    void Start()
    {
#if UNITY_EDITOR
        // Check if starting scene is SceneType.Gameplay
        // Because the type SceneReference cannot be checked for equality, we instead
        // iterate over the names of each SceneReference and compare that.
        string startScene = SceneManager.GetActiveScene().name;
        foreach (var data in _levelData)
        {
            if (data.levelName.Name == startScene && data.levelType == LevelType.Gameplay)
            {
                Instantiate(_playerPrefab, GetPlayerStartPosition(), Quaternion.identity);
            }
        }
#endif

        
    }

    void StartTimer()
    {
        InvokeRepeating(nameof(DeductTimer), 0f, 1f);
    }
    
    Vector3 GetPlayerStartPosition()
    {
        var playerStart = GameObject.FindGameObjectWithTag("PlayerStart");
        if (!playerStart)
        {
            // Use editor camera view instead
            Vector3 cameraPos = SceneView.lastActiveSceneView.camera.transform.position;
            return new Vector3(cameraPos.x, cameraPos.y, 0);
        }
        return playerStart.transform.position;
    }

    void DeductTimer()
    {
        _timeRemaining--;
        TimerUpdated?.Invoke(_timeRemaining);
        if (_timeRemaining <= 0)
        {
            StartGameOverSequence();
        }
    }

    void StartGameOverSequence()
    {
        
    }
}