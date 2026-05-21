using UnityEngine;

public class LevelManager : MonoBehaviour
{

    [SerializeField] LevelData _levelData;
    
    [SerializeField] GameObject _cameraPrefab;
    [SerializeField] GameObject _playerPrefab;


    void Start()
    {
        var playerStart = GameObject.FindGameObjectWithTag("PlayerStart").transform.position;

        Instantiate(_playerPrefab, playerStart, Quaternion.identity);

        Instantiate(_cameraPrefab, playerStart - new Vector3(0,-10,0), Quaternion.identity);
        if (_cameraPrefab.TryGetComponent<LevelCamera>(out var levelCamera))
        {
            levelCamera.SetTarget(_playerPrefab.transform);
        }
    }
}
