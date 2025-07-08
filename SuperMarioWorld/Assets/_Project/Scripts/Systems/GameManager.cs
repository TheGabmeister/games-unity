using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject _playerPrefab;
    [SerializeField] SceneData[] _sceneData;
    
    private void OnEnable()
    {

    }
    private void OnDisable()
    {

    }
    
    void Start()
    {
#if UNITY_EDITOR
        // Check if starting scene is SceneType.Gameplay
        // Because the type SceneReference cannot be checked for equality, we instead
        // iterate over the names of each SceneReference and compare that.
        string startScene = SceneManager.GetActiveScene().name;
        foreach (var s in _sceneData)
        {
            if (s.sceneName.Name == startScene && s.sceneType == SceneType.Gameplay)
            {
                Instantiate(_playerPrefab, GetPlayerStartPosition(), Quaternion.identity);
            }
        }
#endif
    }
    
    public Vector3 GetPlayerStartPosition()
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
}