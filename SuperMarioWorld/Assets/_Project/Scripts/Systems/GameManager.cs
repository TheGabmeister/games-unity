using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject _playerPrefab;
    [SerializeField] LevelData[] _levelData;
    private int _timeRemaining = 10;

    public static event Action<int> TimerUpdated;
    
    private void OnEnable()
    {
        Events.TestFunction += TestFunction;
    }
    private void OnDisable()
    {
        Events.TestFunction -= TestFunction;
    }
    
    void Start()
    {
#if UNITY_EDITOR
        if (IsGameplayScene())
        {
            Instantiate(_playerPrefab, GetPlayerStartPosition(), Quaternion.identity);
        }
#endif
    }

    void StartTimer()
    {
        InvokeRepeating(nameof(DeductTimer), 0f, 1f);
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

    bool IsGameplayScene()
    {
        // Check if starting scene is SceneType.Gameplay
        // Because the type SceneReference cannot be checked for equality, we instead
        // iterate over the names of each SceneReference and compare that.
        string startScene = SceneManager.GetActiveScene().name;
        foreach (var data in _levelData)
        {
            if (data.levelName.Name == startScene)
            {
                return data.levelType == LevelType.Gameplay;
            }
        }
        return false;
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
    
    public void TestFunction()
    {
        Debug.Log("Test function called from GameManager");
    }
}